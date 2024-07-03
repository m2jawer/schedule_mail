using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace mail.api.Common
{
    public class TokenHeaderParameter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();

            if (context.MethodInfo.Name != "Account")
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "X-Authorization",
                    In = ParameterLocation.Header,
                    Required = true,
                    Schema = new OpenApiSchema
                    {
                        Type = "string",
                        Description = "登入成功后存在的有效Token信息"
                    }
                });
            }
        }
    }
}
