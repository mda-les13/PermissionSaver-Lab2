using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PermissionSaver.Models
{
    public class SerializedNode
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("is_directory")]
        public bool IsDirectory { get; set; }

        [JsonPropertyName("content_base64")]
        public string? ContentBase64 { get; set; }

        [JsonPropertyName("permissions")]
        public PermissionData Permissions { get; set; } = new PermissionData();

        [JsonPropertyName("children")]
        public List<SerializedNode>? Children { get; set; }
    }
}
