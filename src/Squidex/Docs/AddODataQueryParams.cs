using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NJsonSchema;
using NSwag.SwaggerGeneration.Processors;
using NSwag.SwaggerGeneration.Processors.Contexts;
using Squidex.Pipeline.Swagger;

namespace Squidex.Docs
{
    public class AddODataQueryParams : IOperationProcessor
    {
        public async Task<bool> ProcessAsync(OperationProcessorContext context)
        {
            if (context.OperationDescription.Path == "/content")
            {
                context.OperationDescription.Operation.AddQueryParameter("$top", JsonObjectType.Number, "Optional number of contents to take.");
                context.OperationDescription.Operation.AddQueryParameter("$skip", JsonObjectType.Number, "Optional number of contents to skip.");
                context.OperationDescription.Operation.AddQueryParameter("$search", JsonObjectType.String, "Optional OData full text search.");
                context.OperationDescription.Operation.AddQueryParameter("$orderby", JsonObjectType.String, "Optional OData order definition.");
            }

            return true; // return false to exclude the operation from the document
        }
    }
}
