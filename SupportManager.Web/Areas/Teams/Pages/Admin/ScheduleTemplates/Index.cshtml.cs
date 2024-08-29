using System.Data.Entity;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using SupportManager.DAL;
using SupportManager.Web.Areas.Teams.Pages.Shared;
using SupportManager.Web.Infrastructure;
using SupportManager.Web.Infrastructure.CRUD;

namespace SupportManager.Web.Areas.Teams.Pages.Admin.ScheduleTemplates;

public class IndexModel(IMediator mediator) : TeamPageModel
{
    public List<Record<ViewModel>> Data { get; set; }

    public async Task OnGetAsync()
    {
        Data = await mediator.Send(new Index<ViewModel>());
    }

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateProjection<ScheduleTemplate, Record<ViewModel>>()
                .ForMember(dst => dst.Model, opts => opts.MapFrom(src => src))
                .ForCtorParam(nameof(Record<ViewModel>.Model), opts => opts.MapFrom(src => src));
        }
    }

    public class Handler(SupportManagerContext db, TeamId teamId, IMapper mapper) : IRequestHandler<Index<ViewModel>, List<Record<ViewModel>>>
    {
        public async Task<List<Record<ViewModel>>> Handle(Index<ViewModel> request, CancellationToken cancellationToken)
        {
            var templates = await db.Teams.Where(t => t.Id == teamId.Value).SelectMany(t => t.ScheduleTemplates)
                .OrderBy(t => t.Name).ProjectTo<Record<ViewModel>>(mapper.ConfigurationProvider).WithCtorMembers()
                .ToListAsync(cancellationToken);

            return templates;
        }
    }
}