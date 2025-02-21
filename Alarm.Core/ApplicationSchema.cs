using System.Text.Json.Nodes;

namespace Alarm.Core
{
    public partial class Application
    {
        public static JsonObject GetConfigJsonSchema()
        {
            JsonObject schema = Schema.GetSchema(typeof(MainConfig));

            JsonObject props = schema["properties"]?.AsObject() ?? throw new NullReferenceException();
            JsonObject providerBuilders = props["provider"]?.AsObject() ?? throw new NullReferenceException();

            props["playlist"] = PlaylistInfo.StaticSchema();

            props["volume"] = new JsonObject
            {
                ["type"] = new JsonArray("integer", "boolean"),
                ["minimum"] = -2,
                ["maximum"] = 100
            };

            foreach (var (k, v) in configHandlers)
            {
                props[k] = v.schema;
            }

            JsonArray builders = [];

            foreach (var (k, v) in ProviderBuilder.builders)
            {
                builders.Add(new JsonObject
                {
                    ["properties"] = new JsonObject
                    {
                        ["type"] = new JsonObject
                        {
                            ["type"] = "string",
                            ["const"] = k
                        },
                        ["config"] = v.schema
                    }
                });
            }

            providerBuilders["item"] = new JsonObject
            {
                ["type"] = "object",
                ["anyOf"] = builders
            };

            return schema.SetSchema(Schema.JSON_SCHEMA_DRAFT_07);
        }
    }
}
