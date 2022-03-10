using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace SupportManager.Web.Infrastructure
{
    public static class ControllerExtensions
    {
        public static IActionResult RedirectToActionJson<TController>(this TController controller, [AspMvcAction] string action)
            where TController : Controller
        {
            return controller.JsonNet(new {redirect = controller.Url.Action(action)});
        }

        public static ActionResult RedirectToPageJson<TPage>(this TPage page, string pageName, [CanBeNull] object values = null)
            where TPage : PageModel =>
            page.JsonNet(new { redirect = page.Url.Page(pageName, values) });

        public static ContentResult JsonNet(this Controller controller, object model) => JsonNet(model);

        public static ContentResult JsonNet(this PageModel page, object model) => JsonNet(model);

        private static ContentResult JsonNet(object model)
        {
            var serialized = JsonConvert.SerializeObject(model,
                new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

            return new ContentResult { Content = serialized, ContentType = "application/json" };
        }
    }
}
