using System.Text.Json.Serialization;

namespace PermissionSaver.Models
{
    public class PermissionData
    {
        [JsonPropertyName("windows_acl")]
        public string? WindowsAclSddl { get; set; }

        [JsonPropertyName("unix_mode")]
        public int? UnixFileModeInt { get; set; }
    }
}
