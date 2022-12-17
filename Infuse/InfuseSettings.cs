using Auxiliary.Configuration;
using System.Text.Json.Serialization;

namespace Infuse
{
    public class InfuseSettings : ISettings
    {
        [JsonPropertyName("blacklist")]
        public List<int> BlacklistedBuffs { get; set; } = new();
    }
}
