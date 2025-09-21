using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace quranTranslationExtractor.Data
{
    public class Row
    {
        [JsonPropertyName("verses")]
        public Verse[] Verses { get; set; } = [];
        [JsonPropertyName("sura")]
        public Sura Sura { get; set; } = new Sura();
    }
}
