using System;

namespace GTAFramework.Core.Container
{
    /// <summary>
    /// Marca una clase para auto-registro como sistema.
    /// Permite especificar orden de inicialización y si inicia activo.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class AutoRegisterAttribute : Attribute
    {
        /// <summary>Orden de inicialización (menor = antes).</summary>
        public int Priority { get; set; } = 100;

        /// <summary>Si el sistema debe iniciarse activo.</summary>
        public bool StartActive { get; set; } = true;
    }
}
