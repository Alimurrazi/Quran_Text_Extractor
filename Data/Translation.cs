using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace quranTranslationExtractor.Data
{
    public class Translation
    {
        [JsonPropertyName("translation")]
        public string TranslationStr { get; set; } = string.Empty;
    }
}
