using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace quranTranslationExtractor.Domain
{
    public class Sura
    {
        public int Id { get; set; }
        public int SuraIndex { get; set; }
        public string Name { get; set; } = string.Empty;
        public int TotalVerses { get; set; }
    }
}
