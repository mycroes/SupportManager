using System.Data.Entity;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SupportManager.DAL;
using SupportManager.Web.Features.User;

namespace SupportManager.Web.Pages.User
{
    public class IndexModel : PageModel
    {
        private readonly IMediator mediator;

        public IndexModel(IMediator mediator) => this.mediator = mediator;

        public Result Data { get; set; }

        public async Task OnGetAsync()
        {
            Data = await mediator.Send(new Query(User.Identity?.Name));
        }

        public record Query(string UserName) : IRequest<Result>;

        public class Result
        {
            public string DisplayName { get; init; }
            public int? PrimaryEmailAddressId { get; init; }
            public string PrimaryEmailAddressValue { get; init; }
            public int? PrimaryPhoneNumberId { get; init; }
            public string PrimaryPhoneNumberLabel { get; init; }
            public string PrimaryPhoneNumberValue { get; init; }
            public List<PhoneNumber> PhoneNumbers { get; init; }
            public List<EmailAddress> EmailAddresses { get; init; }

            public record PhoneNumber
            {
                public int Id { get; init; }
                public string Label { get; init; }
                public string Value { get; init; }
                public bool IsVerified { get; init; }
            }

            public record EmailAddress
            {
                public int Id { get; init; }
                public string Value { get; init; }
                public bool IsVerified { get; init; }
            }
        }

        public class MapperProfile : Profile
        {
            public MapperProfile()
            {
                CreateMap<DAL.User, Result>();
                CreateMap<UserPhoneNumber, Result.PhoneNumber>();
                CreateMap<UserEmailAddress, Result.EmailAddress>();
            }
        }

        public class DetailsQueryHandler : IRequestHandler<Query, Result>
        {
            private readonly SupportManagerContext db;
            private readonly IMapper mapper;

            public DetailsQueryHandler(SupportManagerContext db, IMapper mapper)
            {
                this.db = db;
                this.mapper = mapper;
            }

            public async Task<Result> Handle(Query request, CancellationToken cancellationToken)
            {
                return await db.Users.WhereUserLoginIs(request.UserName)
                    .ProjectTo<Result>(mapper.ConfigurationProvider).SingleAsync();
            }
        }
    }
}
