// ==========================================================================
//  GlobalSuppressions.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System.Threading.Tasks;
using NJsonSchema;
using NSwag.SwaggerGeneration.Processors;
using NSwag.SwaggerGeneration.Processors.Contexts;
using Squidex.Infrastructure.Tasks;
using Squidex.Pipeline.Swagger;

namespace Squidex.Docs
{
    public class AddODataQueryParams : IOperationProcessor
    {
        public Task<bool> ProcessAsync(OperationProcessorContext context)
        {
            if (context.OperationDescription.Path == "/content/{app}")
            {
                context.OperationDescription.Operation.AddQueryParameter("$top", JsonObjectType.Number, "Optional number of contents to take.");
                context.OperationDescription.Operation.AddQueryParameter("$skip", JsonObjectType.Number, "Optional number of contents to skip.");
                context.OperationDescription.Operation.AddQueryParameter("$search", JsonObjectType.String, "Optional OData full text search.");
                context.OperationDescription.Operation.AddQueryParameter("$orderby", JsonObjectType.String, "Optional OData order definition.");
            }

            return TaskHelper.True; // return false to exclude the operation from the document
        }
    }
}
