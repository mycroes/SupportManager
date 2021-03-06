﻿using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;
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

        public static ContentResult JsonNet(this Controller controller, object model)
        {
            var serialized = JsonConvert.SerializeObject(model,
                new JsonSerializerSettings {ReferenceLoopHandling = ReferenceLoopHandling.Ignore});

            return new ContentResult {Content = serialized, ContentType = "application/json"};
        }
    }
}
