using Auxiliary.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Infuse
{
    public class InfuseSettings : ISettings
    {
        [JsonPropertyName("blacklist")]
        public List<int> BlacklistedBuffs { get; set; } = new();
    }
}
