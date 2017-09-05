// ==========================================================================
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System.Collections.Generic;
namespace Squidex.Controllers.UI
{
    public class WebHookResponse<T>
    {
        /// <summary>Id of the response.</summary>
        public string Id { get; set; }

        /// <summary>Number of attempt.</summary>
        public int Attempt { get; set; }

        /// <summary>List of notifications.</summary>
        public List<T> Notifications { get; set; }
    }
}
