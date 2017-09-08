// ==========================================================================
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Squidex.Domain.Users;
using Squidex.Shared.Users;

namespace Squidex.Controllers.UI.Account
{
    public class IdentityServerWebHookController : Controller
    {
        private readonly UserManager<IUser> userManager;

        public IdentityServerWebHookController(
            UserManager<IUser> userManager)
        {
            this.userManager = userManager;
        }

        /// <summary>Callback for Change Identity Webhook for Identity Server.</summary>
        /// <param name="webHookObj">webhook from CPP</param>
        /// <returns>IActionResult if suceeded or failed</returns>
        [Route("account/external/webhooks/identityserver/")]
        public async Task<IActionResult> ChangeIdentity([FromBody] WebHookResponse<ChangeIdentity> webHookObj)
        {
            if (Request.Method == "GET")
            {
                string echo = Request.Query["echo"];
                if (!string.IsNullOrWhiteSpace(echo))
                {
                    Response.StatusCode = (int)HttpStatusCode.OK;
                    return Content(echo);
                }
            }

            bool isValid = !(webHookObj?.Notifications == null || !webHookObj.Notifications.Any());
            if (isValid)
            {
                // Extra sanity Validation
                foreach (ChangeIdentity notification in webHookObj.Notifications)
                {
                    if (notification == null
                        || notification.Action != "UserIdentityChanged"
                        || string.IsNullOrEmpty(notification.FirstName)
                        || string.IsNullOrEmpty(notification.LastName)
                        || string.IsNullOrEmpty(notification.Email)
                        || notification.Id == default(Guid))
                    {
                        isValid = false;
                        break;
                    }
                }
            }

            if (!isValid)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Content("Invalid Request.");
            }

            // Process notification
            foreach (var notification in webHookObj.Notifications)
            {
                var user = await userManager.FindByIdentityServerId(notification.Id.ToString());
                if (user != null)
                {
                    var displayName = $"{notification.FirstName} {notification.LastName[0]}";

                    user.UpdateEmail(notification.Email);
                    user.SetDisplayName(displayName);
                    user.SetFirstName(notification.FirstName);
                    user.SetLastName(notification.LastName);
                    await userManager.UpdateAsync(user);
                }
            }

            Response.StatusCode = (int)HttpStatusCode.Accepted;
            return Content(string.Empty);
        }
    }
}
