using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace quranTranslationExtractor.Application
{
    public interface ISyncService
    {
        public Task ExtractAndSyncContent();
        public Task GeneratePDF();
        public void PreviewPDF();
    }
}
