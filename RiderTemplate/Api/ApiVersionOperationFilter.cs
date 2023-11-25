using System.Collections.Generic;
using System.Linq;
using Application.Common.Constants;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Api;

public class ApiVersionOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();

        var versionOperation = operation.Parameters.FirstOrDefault(x => x.Name == "version");

        if (versionOperation != null)
        {
            versionOperation.Schema.Default = new OpenApiString(Versions.CurrentVersion);
        }
    }
}