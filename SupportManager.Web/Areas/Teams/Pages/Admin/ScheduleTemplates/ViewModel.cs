using System.Data.Entity;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentValidation;
using MediatR;
using SupportManager.DAL;
using SupportManager.Web.Infrastructure.CRUD;

namespace SupportManager.Web.Areas.Teams.Pages.Admin.ScheduleTemplates;

public record ViewModel(int TeamId, string Name, DayOfWeek StartDay, TimeSpan StartTime, List<ViewModel.Entry> Entries)
{
    public record Entry(DayOfWeek? DayOfWeek, TimeSpan? Time, int? UserSlot)
    {
        public class Validator : AbstractValidator<Entry>
        {
            public Validator()
            {
                RuleFor(m => m.DayOfWeek).NotNull();
                RuleFor(m => m.Time).NotNull();
                RuleFor(m => m.UserSlot).NotNull();
            }
        }
    }

    public class Validator : AbstractValidator<ViewModel>
    {
        public Validator()
        {
            RuleFor(m => m.Name).NotNull().NotEmpty();
            RuleFor(m => m.Entries).NotNull().NotEmpty();
        }
    }

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateProjection<ScheduleTemplate, ViewModel>();
            CreateProjection<ScheduleTemplateEntry, Entry>();
        }
    }

    public class QueryHandler(SupportManagerContext db, TeamId teamId, IMapper mapper)
        : IRequestHandler<Query<ViewModel>, ViewModel>
    {
        public Task<ViewModel> Handle(Query<ViewModel> request, CancellationToken cancellationToken)
        {
            return db.ScheduleTemplates.Where(t => t.TeamId == teamId.Value).Where(t => t.Id == request.Id)
                .ProjectTo<ViewModel>(mapper.ConfigurationProvider).WithCtorMembers()
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}