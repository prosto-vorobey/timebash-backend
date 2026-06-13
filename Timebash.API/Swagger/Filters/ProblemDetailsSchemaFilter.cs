using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Timebash.API.Swagger.Filters;

public sealed class ProblemDetailsSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is not OpenApiSchema openApiSchema)
            return;

        if (context.Type == typeof(ProblemDetails))
        {
            ApplyProblemDetailsSchema(openApiSchema);
            return;
        }

        if (context.Type == typeof(ValidationProblemDetails))
        {
            ApplyValidationProblemDetailsSchema(openApiSchema);
            return;
        }
    }

    private static void ApplyProblemDetailsSchema(OpenApiSchema schema)
    {
        schema.Type = JsonSchemaType.Object;
        schema.Properties = new Dictionary<string, IOpenApiSchema>
        {
            ["type"] = new OpenApiSchema { Type = JsonSchemaType.String },
            ["title"] = new OpenApiSchema { Type = JsonSchemaType.String },
            ["status"] = new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int32" },
            ["detail"] = new OpenApiSchema { Type = JsonSchemaType.String },
            ["instance"] = new OpenApiSchema { Type = JsonSchemaType.String },
            ["traceId"] = new OpenApiSchema { Type = JsonSchemaType.String }
        };

        schema.AdditionalProperties = null;
        schema.AdditionalPropertiesAllowed = false;
    }

    private static void ApplyValidationProblemDetailsSchema(OpenApiSchema schema)
    {
        schema.Type = JsonSchemaType.Object;
        schema.Properties = new Dictionary<string, IOpenApiSchema>
        {
            ["type"] = new OpenApiSchema { Type = JsonSchemaType.String },
            ["title"] = new OpenApiSchema { Type = JsonSchemaType.String },
            ["status"] = new OpenApiSchema { Type = JsonSchemaType.Integer, Format = "int32" },
            ["traceId"] = new OpenApiSchema { Type = JsonSchemaType.String },

            ["errors"] = new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
                AdditionalProperties = new OpenApiSchema
                {
                    Type = JsonSchemaType.Array,
                    Items = new OpenApiSchema { Type = JsonSchemaType.String }
                },
                AdditionalPropertiesAllowed = true
            }
        };

        schema.AdditionalProperties = null;
        schema.AdditionalPropertiesAllowed = false;
    }
}
