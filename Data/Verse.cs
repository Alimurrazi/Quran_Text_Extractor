using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace quranTranslationExtractor.Data
{
    public class Verse
    {
        [JsonPropertyName("translations")]
        public Translation[] Translations { get; set; } = [];
        [JsonPropertyName("tafsir")]
        public Tafsir[] Tafsir { get; set; } = [];
        [JsonPropertyName("verse_id")]
        public int VerseId { get; set; }
    }
}
