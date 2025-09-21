using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace quranTranslationExtractor.Domain
{
    public class Ayat
    {
        public int Id { get; set; }
        public int AyatIndex { get; set; }
        public int SuraIndex { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
