using System.Reflection;
using CampusStrayCatSystem.Models;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CampusStrayCatSystem.Core {
    public sealed class Utf8ByteLengthSchemaFilter : ISchemaFilter {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context) {
            var attribute = context.MemberInfo?.GetCustomAttribute<Utf8ByteLengthAttribute>();
            if (attribute == null) { return;}

            var byteLimitDescription = $"UTF-8 编码后最多 {attribute.MaximumBytes} 字节。";
            schema.Description = string.IsNullOrWhiteSpace(schema.Description)
                ? byteLimitDescription
                : $"{schema.Description} {byteLimitDescription}";
            schema.Extensions["x-maxUtf8Bytes"] = new OpenApiInteger(attribute.MaximumBytes);}
    }
}
