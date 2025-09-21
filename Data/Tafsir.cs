using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace quranTranslationExtractor.Data
{
    public class Tafsir
    {
        [JsonPropertyName("tafsir_id")]
        public int TafsirId { get; set; }
        [JsonPropertyName("sura_id")]
        public int SuraId { get; set; }
        [JsonPropertyName("verse_id")]
        public int VerseId { get; set; }
        [JsonPropertyName("translation")]
        public string Translation { get; set; } = string.Empty;
    }
}
