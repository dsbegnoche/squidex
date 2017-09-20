// ==========================================================================
//  AppPatternsController.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Squidex.Controllers.Api.UI.Models;
using Squidex.Domain.Apps.Write.Apps.Commands;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Pipeline;

namespace Squidex.Controllers.Api.Apps
{
    /// <summary>
    /// Manages and configures app patterns.
    /// </summary>
    [MustBeAppOwner]
    [ApiExceptionFilter]
    [AppApi]
    [SwaggerTag(nameof(Apps))]
    public sealed class AppPatternsController : ControllerBase
    {
        public AppPatternsController(ICommandBus commandBus)
            : base(commandBus)
        {
        }

        /// <summary>
        /// Get app patterns.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <returns>
        /// 200 => Patterns returned.
        /// 404 => App not found.
        /// </returns>
        /// <remarks>
        /// Gets all configured regex patterns for the app with the specified name.
        /// </remarks>
        [HttpGet]
        [Route("apps/{app}/patterns/")]
        [ProducesResponseType(typeof(UIRegexSuggestionDto[]), 200)]
        [ApiCosts(1)]
        public IActionResult GetPatterns(string app)
        {
            return Ok(App.Patterns?
                        .Where(x =>
                            !string.IsNullOrWhiteSpace(x.Name) &&
                            !string.IsNullOrWhiteSpace(x.Pattern))
                        .Select(x => new UIRegexSuggestionDto { Name = x.Name, Pattern = x.Pattern, DefaultMessage = x.DefaultMessage })
                        .OrderBy(x => x.Name)
                        .ToList()
                    ?? new List<UIRegexSuggestionDto>());
        }

        /// <summary>
        /// Create a new app client.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="request">Client object that needs to be added to the app.</param>
        /// <returns>
        /// 201 => Client generated.
        /// 404 => App not found.
        /// </returns>
        /// <remarks>
        /// Create a new client for the app with the specified name.
        /// The client secret is auto generated on the server and returned. The client does not exire, the access token is valid for 30 days.
        /// </remarks>
        [HttpPost]
        [Route("apps/{app}/patterns/")]
        [ProducesResponseType(typeof(UIRegexSuggestionDto), 201)]
        [ApiCosts(1)]
        public async Task<IActionResult> PostPattern(string app, [FromBody] UIRegexSuggestionDto request)
        {
            var command = SimpleMapper.Map(request, new AddPattern());

            await CommandBus.PublishAsync(command);

            var response = SimpleMapper.Map(command, new UIRegexSuggestionDto());

            return CreatedAtAction(nameof(GetPatterns), new { app }, response);
        }

        /// <summary>
        /// Revoke an app client
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="name">The name of the pattern to be deleted.</param>
        /// <returns>
        /// 204 => Pattern removed.
        /// 404 => App not found or pattern not found.
        /// </returns>
        /// <remarks>
        /// Schemas using this pattern will still function using the same Regular Expression
        /// </remarks>
        [HttpDelete]
        [Route("apps/{app}/patterns/{name}/")]
        [ApiCosts(1)]
        public async Task<IActionResult> DeletePattern(string app, string name)
        {
            await CommandBus.PublishAsync(new DeletePattern { Name = name });

            return NoContent();
        }
    }
}
