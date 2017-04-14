using System.Collections.Generic;
using System.Data.Entity;
using HtmlTags;
using HtmlTags.Conventions;
using HtmlTags.Conventions.Elements;
using SupportManager.DAL;

namespace SupportManager.Web.Infrastructure.Tags
{
    public abstract class EntitySelectElementBuilder<T> : ElementTagBuilder where T : class
    {
        public override bool Matches(ElementRequest subject)
        {
            return typeof(T).IsAssignableFrom(subject.Accessor.PropertyType);
        }

        public override HtmlTag Build(ElementRequest request)
        {
            var results = Source(request);

            var selectTag = new SelectTag(t =>
            {
                t.Option(string.Empty, string.Empty); // blank default option
                foreach (var result in results)
                {
                    BuildOptionTag(t, result, request);
                }
            });

            var entity = request.Value<T>();

            if (entity != null)
            {
                selectTag.SelectByValue(GetValue(entity));
            }

            return selectTag;
        }

        protected virtual HtmlTag BuildOptionTag(SelectTag select, T model, ElementRequest request)
        {
            return select.Option(GetDisplayValue(model), GetValue(model));
        }

        protected abstract int GetValue(T instance);
        protected abstract string GetDisplayValue(T instance);

        protected virtual IEnumerable<T> Source(ElementRequest request)
        {
            return request.Get<SupportManagerContext>().Set<T>();
        }
    }

    public class UserPhoneNumberSelectElementBuilder : EntitySelectElementBuilder<UserPhoneNumber>
    {
        protected override int GetValue(UserPhoneNumber instance)
        {
            return instance.Id;
        }

        protected override string GetDisplayValue(UserPhoneNumber instance)
        {
            return instance.User.DisplayName + " - " + instance.Value;
        }

        protected override IEnumerable<UserPhoneNumber> Source(ElementRequest request)
        {
            return request.Get<SupportManagerContext>().Set<UserPhoneNumber>().Include(upn => upn.User);
        }
    }

    public class TeamSelectElementBuilder : EntitySelectElementBuilder<SupportTeam>
    {
        protected override int GetValue(SupportTeam instance)
        {
            return instance.Id;
        }

        protected override string GetDisplayValue(SupportTeam instance)
        {
            return instance.Name;
        }
    }
}