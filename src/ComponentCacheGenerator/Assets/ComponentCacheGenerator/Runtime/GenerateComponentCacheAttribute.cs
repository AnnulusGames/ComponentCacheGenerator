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

        public Type ComponentType { get; }
        public ComponentSearchScope SearchScope { get; set; } = ComponentSearchScope.Self;
        public bool IsRequired { get; set; } = false;
        public string PropertyName { get; set; } = null;
    }
}