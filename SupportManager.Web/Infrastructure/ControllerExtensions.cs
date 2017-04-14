using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace SupportManager.Web.Infrastructure
{
    public static class ControllerExtensions
    {
        public static ActionResult RedirectToActionJson<TController>(this Controller controller, Expression<Action<TController>> action)
            where TController : Controller
        {
            return controller.JsonNet(new
            {
                redirect = controller.Url.Action(action)
            });
        }

        public static ActionResult RedirectToActionJson<TController>(this TController controller, Expression<Action<TController>> action)
            where TController : Controller
        {
            return controller.JsonNet(new
            {
                redirect = controller.Url.Action(action)
            }
                );
        }

        public static ActionResult RedirectToActionJson<TController>(this Controller controller, Expression<Func<TController, Task>> func)
            where TController : Controller
        {
            var action = Expression.Lambda<Action<TController>>(func.Body, func.Parameters);

            return controller.JsonNet(new
            {
                redirect = controller.Url.Action(action)
            });
        }

        public static ActionResult RedirectToActionJson<TController>(this TController controller, Expression<Func<TController, Task>> func)
            where TController : Controller
        {
            var action = Expression.Lambda<Action<TController>>(func.Body, func.Parameters);

            return controller.JsonNet(new
                {
                    redirect = controller.Url.Action(action)
                }
            );
        }

        public static ActionResult RedirectToActionJson<TController>(this TController controller, [AspMvcAction] string action)
            where TController : Controller
        {
            return controller.JsonNet(new
                {
                    redirect = controller.Url.Action(action)
                }
            );
        }

        public static ContentResult JsonNet(this Controller controller, object model)
        {
            var serialized = JsonConvert.SerializeObject(model, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            return new ContentResult
            {
                Content = serialized,
                ContentType = "application/json"
            };
        }
    }
}