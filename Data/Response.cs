using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace quranTranslationExtractor.Data
{
    public class Response
    {
        [JsonPropertyName("data")]
        public Data Data { get; set; } = new Data();
    }
}
