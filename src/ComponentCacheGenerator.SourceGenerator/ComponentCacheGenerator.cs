using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.DotnetRuntime.Extensions;

namespace ComponentCacheGenerator.SourceGenerator;

[Generator]
public sealed class ComponentCacheGenerator : IIncrementalGenerator
{
    [Flags]
    public enum ComponentSearchScope
    {
        Self = 1,
        Children = 2,
        Parent = 4
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var source = context.SyntaxProvider.ForAttributeWithMetadataName(
            context,
            "ComponentCacheGenerator.GenerateComponentCacheAttribute",
            static (node, token) => true,
            static (context, token) => context);

        context.RegisterSourceOutput(source, Emit);
    }

    static void GetAttributeParameters(AttributeData attr, out INamedTypeSymbol targetType, out string? propertyName, out ComponentSearchScope searchScope, out bool isRequired)
    {
        targetType = (INamedTypeSymbol)attr.ConstructorArguments[0].Value!;
        propertyName = attr.ConstructorArguments.Length == 2
            ? (string)attr.ConstructorArguments[1].Value!
            : null;

        var properties = attr.NamedArguments.ToDictionary(kv => kv.Key, kv => kv.Value);
        searchScope = properties.TryGetValue("SearchScope", out var searchScopeConstant) ? (ComponentSearchScope)searchScopeConstant.Value! : ComponentSearchScope.Self;
        isRequired = properties.TryGetValue("IsRequired", out var isRequiredConstant) ? (bool)isRequiredConstant.Value! : true;
    }

    static bool Verify(SourceProductionContext context, TypeDeclarationSyntax typeSyntax, INamedTypeSymbol targetType)
    {
        var hasError = false;

        static bool IsMonoBehaviour(INamedTypeSymbol? typeSymbol)
        {
            while (typeSymbol != null)
            {
                if (typeSymbol.Name == "MonoBehaviour") return true;
                typeSymbol = typeSymbol!.BaseType;
            }

            return false;
        }

        // require partial
        if (!typeSyntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.MustBePartial, typeSyntax.Identifier.GetLocation(), targetType.Name));
            hasError = true;
        }

        // must be MonoBehaviour
        if (!IsMonoBehaviour(targetType))
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.MustInheritMonoBehaviour, typeSyntax.Identifier.GetLocation(), targetType.Name));
            hasError = true;
        }

        return !hasError;
    }

    static void Emit(SourceProductionContext context, GeneratorAttributeSyntaxContext source)
    {
        var typeSymbol = (INamedTypeSymbol)source.TargetSymbol;
        if (!Verify(context, (TypeDeclarationSyntax)source.TargetNode, typeSymbol)) return;

        var ns = typeSymbol.ContainingNamespace.IsGlobalNamespace
            ? null
            : typeSymbol.ContainingNamespace;

        var hasAwake = typeSymbol.GetMembers("Awake").Any(x => x is IMethodSymbol methodSymbol && methodSymbol.Parameters.Length == 0);
        var awakeCode = hasAwake ? "" :
"""
private void Awake()
{
    this.CacheComponents();
}
""";

        var fullType = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            .Replace("global::", "")
            .Replace("<", "_")
            .Replace(">", "_");

        var requiredComponentCode = new StringBuilder();
        var cachePropertyCode = new StringBuilder();
        var cacheMethodCode = new StringBuilder();

        foreach (var attribute in source.Attributes)
        {
            GetAttributeParameters(attribute, out var targetType, out var propertyName, out var searchScope, out var isRequired);
            var targetTypeFullName = targetType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var cachePropertyName = propertyName ?? MemberNames.ClassNameToFieldName(targetType.Name);
            cachePropertyCode.AppendLine($"private {targetTypeFullName} {cachePropertyName} {{ get; set; }}");

            cacheMethodCode.AppendLine($"{cachePropertyName} = null;");
            
            if ((searchScope & ComponentSearchScope.Self) == ComponentSearchScope.Self)
            {
                cacheMethodCode.AppendLine($"{cachePropertyName} = this.GetComponent<{targetTypeFullName}>();");
            }
            if ((searchScope & ComponentSearchScope.Children) == ComponentSearchScope.Children)
            {
                cacheMethodCode.AppendLine($"if ({cachePropertyName} == null) {cachePropertyName} = this.GetComponentInChildren<{targetTypeFullName}>();");
            }
            if ((searchScope & ComponentSearchScope.Parent) == ComponentSearchScope.Parent)
            {
                cacheMethodCode.AppendLine($"if ({cachePropertyName} == null) {cachePropertyName} = this.GetComponentInParent<{targetTypeFullName}>();");
            }
            if (isRequired)
            {
                if ((searchScope & ComponentSearchScope.Self) == ComponentSearchScope.Self)
                {
                    requiredComponentCode.AppendLine($"[global::UnityEngine.RequireComponent(typeof({targetTypeFullName}))]");
                }

                cacheMethodCode.AppendLine($"if ({cachePropertyName} == null) throw new global::UnityEngine.MissingReferenceException();");
            }
        }

        var code =
$$"""
// <auto-generated/>

{{(ns == null ? "" : $"namespace {ns} {{")}}

{{requiredComponentCode}}
partial class {{typeSymbol.Name}}
{
    {{cachePropertyCode}}

    private void CacheComponents()
    {
        {{cacheMethodCode}}
    }

    {{awakeCode}}
}

{{(ns == null ? "" : "}")}}
""";

        context.AddSource($"{fullType}.ComponentCache.g.cs", code);
    }
}
