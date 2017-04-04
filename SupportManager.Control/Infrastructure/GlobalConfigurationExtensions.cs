using System;
using Hangfire;
using StructureMap;

namespace SupportManager.Control.Infrastructure
{
    public static class GlobalConfigurationExtensions
    {
        public static IGlobalConfiguration UseNestedContainerActivator(this IGlobalConfiguration This,
            IContainer container)
        {
            if (This == null) throw new ArgumentNullException(nameof(This));
            if (container == null) throw new ArgumentNullException(nameof(container));

            return This.UseActivator(new NestedContainerActivator(container));
        }
    }
}