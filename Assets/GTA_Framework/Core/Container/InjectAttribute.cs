using System;

namespace GTAFramework.Core.Container
{
    /// <summary>
    /// Marca un campo o propiedad para inyección automática.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class InjectAttribute : Attribute
    {
    }
}
