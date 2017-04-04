using System;
using System.Diagnostics;
using Hangfire;
using StructureMap;

namespace SupportManager.Control.Infrastructure
{
    public class NestedContainerActivator : JobActivator
    {
        private readonly IContainer container;

        public NestedContainerActivator(IContainer container)
        {
            this.container = container;
        }

        public override object ActivateJob(Type jobType)
        {
            Debugger.Break();
            return container.GetInstance(jobType);
        }

        public override JobActivatorScope BeginScope(JobActivatorContext context)
        {
            return new NestedContainerJobActivatorScope(container);
        }

        private class NestedContainerJobActivatorScope : JobActivatorScope
        {
            private readonly IContainer container;

            public NestedContainerJobActivatorScope(IContainer container)
            {
                this.container = container.GetNestedContainer();
            }

            public override void DisposeScope()
            {
                container.Dispose();
            }

            public override object Resolve(Type type)
            {
                return container.GetInstance(type);
            }
        }
    }
}