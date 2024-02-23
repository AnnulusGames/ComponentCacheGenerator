namespace ComponentCacheGenerator.SourceGenerator;

public static class MemberNames
{
    public static string ClassNameToFieldName(string name)
    {
        return char.ToLowerInvariant(name[0]) + name.Substring(1);
    }
}