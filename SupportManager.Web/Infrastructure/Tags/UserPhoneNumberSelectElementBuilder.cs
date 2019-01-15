using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using HtmlTags;
using HtmlTags.Conventions;
using HtmlTags.Conventions.Elements;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using SupportManager.DAL;

namespace SupportManager.Web.Infrastructure.Tags
{
    public class UserPhoneNumberSelectElementBuilder : ElementTagBuilder
    {
        public override bool Matches(ElementRequest subject)
        {
            return subject.Accessor.PropertyType == typeof(UserPhoneNumber) &&
                subject.Accessor.OwnerType.Namespace.StartsWith("SupportManager.Web.Areas.Teams");
        }

        public override HtmlTag Build(ElementRequest request)
        {
            var aca = request.Get<IActionContextAccessor>();
            var teamId = int.Parse((string) aca.ActionContext.RouteData.Values["teamId"]);
            var db = request.Get<SupportManagerContext>();

            var results = db.TeamMembers.Where(m => m.TeamId == teamId).SelectMany(m => m.User.PhoneNumbers)
                .Select(upn => new {upn.User.DisplayName, upn.Id, upn.Value}).OrderBy(upn => upn.DisplayName).ToList();

            var selectTag = new SelectTag(t =>
            {
                t.Option(string.Empty, string.Empty); // blank default option
                foreach (var result in results)
                {
                    t.Option($"{result.DisplayName} - {result.Value}", result.Id);
                }
            });

            var entity = request.Value<UserPhoneNumber>();
            if (entity != null)
            {
                selectTag.SelectByValue(entity.Id);
            }

            return selectTag;
        }
    }
}