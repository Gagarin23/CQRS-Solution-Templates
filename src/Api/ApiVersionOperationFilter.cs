using System.Collections.Generic;
using System.Linq;
using Asp.Versioning;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Api;

public class ApiVersionOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var actionMetadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;
        operation.Parameters ??= new List<OpenApiParameter>();

        var apiVersionMetadata = actionMetadata
            .Any(metadataItem => metadataItem is ApiVersionMetadata);
        if (apiVersionMetadata)
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "API-Version",
                In = ParameterLocation.Header,
                Description = "API Version header value",
                Schema = new OpenApiSchema
                {
                    Type = "String",
                    Default = new OpenApiString("1.0")
                }
            });
        }
    }
}