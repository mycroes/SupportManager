using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SupportManager.DAL;

namespace SupportManager.Web.Pages.User.ApiKeys
{
    public class DeleteModel : PageModel
    {
        private readonly IMediator mediator;

        public DeleteModel(IMediator mediator) => this.mediator = mediator;

        [BindProperty]
        public Command Data { get; set; }

        public async Task OnGetAsync(int id)
        {
            Data = await mediator.Send(new Query{Id = id});
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await mediator.Send(Data);
            return RedirectToPage("List");
        }

        public class MapperProfile : Profile
        {
            public MapperProfile()
            {
                CreateMap<ApiKey, Command>();
            }
        }

        public class Query : IRequest<Command>
        {
            public int Id { get; set; }
        }

        public class QueryHandler : IRequestHandler<Query, Command>
        {
            private readonly SupportManagerContext db;
            private readonly IMapper mapper;

            public QueryHandler(SupportManagerContext db, IMapper mapper)
            {
                this.db = db;
                this.mapper = mapper;
            }

            public async Task<Command> Handle(Query request, CancellationToken cancellationToken)
            {
                return await db.ApiKeys.Where(apiKey => apiKey.Id == request.Id)
                    .ProjectToSingleAsync<Command>(mapper.ConfigurationProvider);
            }
        }

        public class Command : IRequest
        {
            public int Id { get; set; }
            public string Value { get; set; }
        }

        public class CommandHandler : AsyncRequestHandler<Command>
        {
            private readonly SupportManagerContext db;

            public CommandHandler(SupportManagerContext db)
            {
                this.db = db;
            }

            protected override async Task Handle(Command request, CancellationToken cancellationToken)
            {
                var apiKey = await db.ApiKeys.FindAsync(request.Id);
                if (apiKey == null) return;

                db.ApiKeys.Remove(apiKey);
                await db.SaveChangesAsync();
            }
        }
    }
}