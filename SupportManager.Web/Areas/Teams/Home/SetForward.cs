﻿using Hangfire;
using MediatR;
using SupportManager.Contracts;

namespace SupportManager.Web.Areas.Teams.Home
{
    public static class SetForward
    {
        public class Command : IRequest
        {
            public int TeamId { get; set; }
            public int PhoneNumberId { get; set; }
        }

        public class Handler : RequestHandler<Command>
        {
            protected override void Handle(Command request)
            {
                BackgroundJob.Enqueue<IForwarder>(f => f.ApplyForward(request.TeamId, request.PhoneNumberId, null));
            }
        }
    }
}