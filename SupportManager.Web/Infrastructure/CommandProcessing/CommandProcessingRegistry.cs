﻿using FluentValidation;
using MediatR;
using StructureMap.Configuration.DSL;

namespace SupportManager.Web.Infrastructure.CommandProcessing
{
    public class CommandProcessingRegistry : Registry
    {
        public CommandProcessingRegistry()
        {
            Scan(scan =>
            {
                scan.AssemblyContainingType<IMediator>();
                scan.AssemblyContainingType<CommandProcessingRegistry>();

                scan.AddAllTypesOf(typeof(IRequestHandler<>));
                scan.AddAllTypesOf(typeof(IRequestHandler<,>));
                scan.AddAllTypesOf(typeof(IAsyncRequestHandler<>));
                scan.AddAllTypesOf(typeof(IAsyncRequestHandler<,>));
                scan.AddAllTypesOf(typeof(IValidator<>));
                scan.WithDefaultConventions();
            });

            For<SingleInstanceFactory>().Use<SingleInstanceFactory>(ctx => t => ctx.GetInstance(t));
            For<MultiInstanceFactory>().Use<MultiInstanceFactory>(ctx => t => ctx.GetAllInstances(t));
        }
    }

    public abstract class RequestHandler<TRequest> : IRequestHandler<TRequest> where TRequest : IRequest
    {
        protected abstract void HandleCore(TRequest request);

        public void Handle(TRequest message)
        {
            HandleCore(message);
        }
    }
}