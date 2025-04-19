using System.Text.Json;
using System.Text.Json.Nodes;

namespace Azure.DataApiBuilder.Service.Helpers
{
    public class EntityHelper
    {
        public static void AddEntity(JsonObject config, string entityName, bool enableGraphQL = true, bool enableRest = true, bool allowAnonymous = true)
        {
            if (config == null || string.IsNullOrEmpty(entityName))
            {
                throw new ArgumentException("Config and entity name cannot be null or empty.");
            }

            // Ensure entities object exists
            if (!config.ContainsKey("entities"))
            {
                config["entities"] = new JsonObject();
            }

            var entities = config["entities"] as JsonObject;

            // Create entity configuration
            var entityConfig = new JsonObject
            {
                ["source"] = new JsonObject
                {
                    ["object"] = entityName,
                    ["type"] = "table"
                },
                ["graphql"] = new JsonObject
                {
                    ["enabled"] = enableGraphQL,
                    ["type"] = new JsonObject
                    {
                        ["singular"] = entityName,
                        ["plural"] = entityName
                    }
                },
                ["rest"] = new JsonObject
                {
                    ["enabled"] = enableRest
                }
            };

            // Add permissions if anonymous access is allowed
            if (allowAnonymous)
            {
                entityConfig["permissions"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["role"] = "anonymous",
                        ["actions"] = new JsonArray
                        {
                            new JsonObject
                            {
                                ["action"] = "*"
                            }
                        }
                    }
                };
            }

            // Add entity to entities collection
            entities![entityName] = entityConfig;
        }

        // Helper method to load JSON config from string
        public static JsonObject LoadConfig(string jsonString)
        {
            return JsonNode.Parse(jsonString) as JsonObject
                ?? throw new JsonException("Invalid JSON configuration");
        }

        // Helper method to serialize config back to string
        public static string SaveConfig(JsonObject config)
        {
            return JsonSerializer.Serialize(config, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
    }
}
