﻿using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using SupportManager.Api.Teams;
using SupportManager.DAL;
using SupportManager.Web.Features.User;

namespace SupportManager.Web.Api.User
{
    public static class MyTeams
    {
        public class Query : IRequest<List<SupportManager.Api.Teams.Team>>
        {
            public Query(string userName) => UserName = userName;

            public string UserName { get; }
        }

        public class Handler : IRequestHandler<Query, List<SupportManager.Api.Teams.Team>>
        {
            private readonly SupportManagerContext db;
            private readonly IMapper mapper;

            public Handler(SupportManagerContext db, IMapper mapper)
            {
                this.db = db;
                this.mapper = mapper;
            }

            public async Task<List<SupportManager.Api.Teams.Team>> Handle(Query request, CancellationToken cancellationToken)
            {
                return await db.Users.WhereUserLoginIs(request.UserName).SelectMany(u => u.Memberships)
                    .Select(m => m.Team).ProjectToListAsync<SupportManager.Api.Teams.Team>(mapper.ConfigurationProvider);
            }
        }
    }
}
