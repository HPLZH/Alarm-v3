using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;

namespace Alarm.Core
{
    public static class Schema
    {
        public static readonly JsonSerializerOptions JsonSerializerOptions = Configuration.JsonSerializerOptions;

        public static readonly JsonSchemaExporterOptions JsonSchemaExportOptions = new()
        {
            TransformSchemaNode = (JsonSchemaExporterContext context, JsonNode node) =>
            {
                if (context.TypeInfo.Type.IsAssignableTo(typeof(ISchemaDefined)))
                {
                    ISchemaDefined? inst = (ISchemaDefined?)context.TypeInfo.Type.InvokeMember(string.Empty, System.Reflection.BindingFlags.CreateInstance, null, null, null);
                    return inst?.GetSchema() ?? node;
                }

                return node;
            }
        };

        public static JsonObject GetSchema(Type type) => JsonSerializerOptions.GetJsonSchemaAsNode(type, JsonSchemaExportOptions).AsObject();
        public static JsonObject GetSchemaNoTransform(Type type) => JsonSerializerOptions.GetJsonSchemaAsNode(type).AsObject();

        public const string JSON_SCHEMA_DRAFT_07 = "http://json-schema.org/draft-07/schema#";
        public static JsonObject SetSchema(this JsonObject node, string url)
        {
            if (!node.ContainsKey("$schema"))
            {
                node.Insert(0, "$schema", url);
            }
            else
            {
                node["$schema"] = url;
            }
            return node;
        }

        public static JsonNode String() => GetSchemaNoTransform(typeof(string));
    }

    public interface ISchemaDefined
    {
        public JsonNode GetSchema();
    }
}
