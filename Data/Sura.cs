using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace quranTranslationExtractor.Data
{
    public class Sura
    {
        [JsonPropertyName("id")]
        public int SuraId { get; set; }
        [JsonPropertyName("name_bng")]
        public string Name { get; set; } = string.Empty;
        [JsonPropertyName("verses_count")]
        public int TotalVerses { get; set; }
    }
}
