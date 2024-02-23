using System;

namespace ComponentCacheGenerator
{
    [Flags]
    public enum ComponentSearchScope
    {
        Self = 1,
        Children = 2,
        Parent = 4
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class GenerateComponentCacheAttribute : Attribute
    {
        public GenerateComponentCacheAttribute(Type componentType)
        {
            ComponentType = componentType;
        }

        public GenerateComponentCacheAttribute(Type componentType, string propertyName)
        {
            ComponentType = componentType;
            PropertyName = propertyName;
        }

        public Type ComponentType { get; } = null;
        public string PropertyName { get; } = null;

        public ComponentSearchScope SearchScope { get; set; } = ComponentSearchScope.Self;
        public bool IsRequired { get; set; } = true;
    }
}