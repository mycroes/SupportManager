﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;
using SupportManager.Api.Teams;
using SupportManager.Api.Users;

namespace SupportManager.Telegram.Infrastructure
{
    internal interface ISupportManagerApi
    {
        [Get("/user")]
        Task<UserDetails> MyDetails();

        [Get("/user/teams")]
        Task<List<Team>> MyTeams();

        [Post("/user/subscribe")]
        Task Subscribe(string callbackUrl);

        [Get("/team/status/{id}")]
        Task<TeamStatus> GetTeamStatus(int id);

        [Get("/team/schedule/{id}")]
        Task<List<ForwardRegistration>> GetTeamSchedule(int id);

        [Delete("/team/forward/{id}")]
        Task DeleteForward(int id);

        [Post("/team/schedule")]
        Task ScheduleForward([Body] ScheduleForward forward);

        [Post("/team/forward")]
        Task SetForward([Body] SetForward forward);

        [Get("/team/members/{id}")]
        Task<List<UserDetails>> GetMembers(int id);
    }
}