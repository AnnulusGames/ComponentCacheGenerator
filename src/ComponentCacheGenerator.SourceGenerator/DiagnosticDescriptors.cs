#pragma warning disable RS2008

using Microsoft.CodeAnalysis;

namespace ComponentCacheGenerator.SourceGenerator;

public static class DiagnosticDescriptors
{
    const string Category = "ComponentCacheGenerator";

    public static readonly DiagnosticDescriptor MustBePartial = new(
        id: "CCG001",
        title: "GenerateComponentCache type must be partial",
        messageFormat: "The ComponentCacheGenerator type '{0}' must be partial",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MustInheritMonoBehaviour = new(
        id: "CCG002",
        title: "GenerateComponentCache type must inherit MonoBehaviour",
        messageFormat: "The ComponentCacheGenerator type '{0}' must inherit MonoBehaviour",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}

#pragma warning restore RS2008