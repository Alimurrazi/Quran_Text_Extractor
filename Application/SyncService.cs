using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using QuestPDF.Companion;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Previewer;
using quranTranslationExtractor.Data;
using quranTranslationExtractor.Domain;
using quranTranslationExtractor.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace quranTranslationExtractor.Application
{
    public class SyncService : ISyncService
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;
        public SyncService(AppDbContext context, HttpClient httpClient)
        {
            _context = context;
            _httpClient = httpClient;
        }

        public async Task ExtractAndSyncContent()
        {
            var index = 1;
            var suraIndex = 103;
            //            for(int i = 1;i<=114;i++)
            for (int i = 1; i <= 1; i++)
            {
                bool hasData = true;
                while (hasData)
                {
                    var url = $"https://api.muslimbangla.com/sura/{suraIndex}?tables=bn_taqi&page={index}&wordByWord=false&language=bengali";
                    string? httpResponse = null;
                    Response formattedResponse = new Response();
                    try
                    {
                        httpResponse = await _httpClient.GetStringAsync(url);
                    }
                    catch (HttpRequestException ex)
                    {
                        Console.WriteLine($"HTTP request failed: {ex.Message}");
                    }

                    try
                    {
                        if (httpResponse is not null)
                        {
                            formattedResponse = JsonSerializer.Deserialize<Response>(httpResponse);
                        }
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"Failed to deserialize JSON: {ex.Message}");
                    }

                    if (formattedResponse is not null && formattedResponse.Data.Rows.Verses.Length == 0)
                    {
                        hasData = false;
                        break;
                    }
                    if (index == 1 && formattedResponse.Data is not null)
                    {
                        await InsertSura(formattedResponse.Data.Rows.Sura);
                    }
                    await PopulateContents(formattedResponse);
                    index++;
                }
            }
        }

        private async Task InsertSura(Data.Sura sura)
        {
            var isExists = await _context.Suras.AnyAsync(sura => sura.SuraIndex == sura.Id);

            if (isExists)
            {
                Console.WriteLine($"{sura.Name} already exists");
            }
            else
            {
                var entity = new Domain.Sura()
                {
                    SuraIndex = sura.SuraId,
                    Name = sura.Name,
                    TotalVerses = sura.TotalVerses
                };
                _context.Suras.Add(entity);
                await _context.SaveChangesAsync();
            }
        }

        private async Task InsertAyat(int suraIndex, Verse verse)
        {
            var content = string.Empty;
            if (verse.Translations is null || verse.Translations.Length == 0)
            {
                Console.WriteLine("No translations found");
            }
            else if (verse.Translations.Length > 1)
            {
                Console.WriteLine("Multiple translations found");
            }
            else
            {
                content = verse.Translations[0].TranslationStr;
            }

            var entity = new Ayat()
            {
                AyatIndex = verse.VerseId,
                SuraIndex = suraIndex,
                Content = content,
            };
            _context.Ayats.Add(entity);
            await _context.SaveChangesAsync();
        }

        private async Task InsertTafsir(Data.Tafsir[] tafsirs)
        {
            foreach (var tafsir in tafsirs)
            {
                var entity = new Domain.Tafsir()
                {
                    SuraIndex = tafsir.SuraId,
                    AyatIndex = tafsir.VerseId,
                    TafsirIndexInSura = tafsir.TafsirId,
                    Content = tafsir.Translation,
                };
                _context.Tafsirs.Add(entity);
            }
            await _context.SaveChangesAsync();
        }

        private async Task PopulateContents(Response response)
        {
            foreach (var verse in response.Data.Rows.Verses)
            {
                await InsertAyat(response.Data.Rows.Sura.SuraId, verse);
                await InsertTafsir(verse.Tafsir);
            }
        }

        public async Task GeneratePDF()
        {
            var(suras, ayats, tafsirs) = await FetchDataAsync();

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "quran_bn_text.pdf");
            var renderedPdf = GetRenderedPdf(suras, ayats, tafsirs);
            renderedPdf.GeneratePdf(filePath);
        }

        public void PreviewPDF()
        {
            List<Domain.Sura> suras = new()
{
    new Domain.Sura { Id = 1, SuraIndex = 1, Name = "Al-Fatiha", TotalVerses = 7 },
    new Domain.Sura { Id = 2, SuraIndex = 2, Name = "Al-Baqarah", TotalVerses = 286 }
};

            List<Ayat> ayats = new()
{
    new Ayat { Id = 1, AyatIndex = 1, SuraIndex = 1, Content = "সমস্ত প্রশংসা আল্লাহর, যিনি জগতসমূহের প্রতিপালক" },
    new Ayat { Id = 2, AyatIndex = 2, SuraIndex = 1, Content = "যিনি সকলের প্রতি দয়াবান, পরম দয়ালু" },
    new Ayat { Id = 3, AyatIndex = 255, SuraIndex = 2, Content = "আল্লাহ, তিনি ছাড়া কোনো সত্য উপাস্য নেই, তিনি চিরঞ্জীব, সবকিছুর ধারক ..." }
};

            List<Domain.Tafsir> tafsirs = new()
{
    new Domain.Tafsir
    {
        Id = 1, SuraIndex = 1, AyatIndex = 1, TafsirIndexInSura = 1,
        Content = "আপনি যদি কোনও ইমারতের প্রশংসা করেন, তবে প্রকৃতপক্ষে সে প্রশংসা হয় ইমারতটির নির্মাতার। সুতরাং এই সৃষ্টিজগতের যে-কোনও বস্তুর প্রশংসা করা হলে পরিণামে সে প্রশংসা হয় আল্লাহ তাআলার, যেহেতু সে বস্তু তাঁরই সৃষ্টি। জগতসমূহের প্রতিপালক বলে সে দিকেই ইশারা করা হয়েছে। মানব জগত, জড় জগত ও উদ্ভিদ জগত থেকে শুরু করে নভোমণ্ডল, নক্ষত্রমণ্ডল ও ফিরিশতা জগত পর্যন্ত সব কিছুর সৃজন ও প্রতিপালন আল্লাহ তাআলারই কাজ। এসব জগতের মধ্যে যা কিছু প্রশংসাযোগ্য আছে, আল্লাহ তাআলার সৃজন ও রবূবিয়্যাতের মহিমার কারণেই তা প্রশংসার যোগ্যতা লাভ করেছে।।"
    },
    new Domain.Tafsir
    {
        Id = 2, SuraIndex = 1, AyatIndex = 2, TafsirIndexInSura = 2,
        Content = "আরবী নিয়ম অনুসারে \"رَحْمٰنُ\"-এর অর্থ সেই সত্তা, যার রহমত ও দয়া অত্যন্ত প্রশস্ত (Extensive) অর্থাৎ যার রহমত দ্বারা সকলেই উপকৃত হয়। আর \"رَحِيْمُ\" অর্থ সেই সত্তা, যার রহমত খুব বেশি (Intensive) অর্থাৎ যার প্রতি তা হয়, পরিপূর্ণরূপে হয়। দুনিয়ায় আল্লাহ তাআলার রহমত সকলেই ভোগ করে। মুমিন ও কাফির নির্বিশেষে সকলেই তা দ্বারা উপকৃত হয়। সকলেই রিযক পায় এবং দুনিয়ার নি‘আমতসমূহ দ্বারা সকলেই লাভবান হয়। আখিরাতে যদিও কাফিরদের প্রতি রহমত হবে না, কিন্তু যাদের প্রতি (অর্থাৎ মুমিনদের প্রতি) হবে, পরিপূর্ণরূপে হবে। ফলে সেখানে নি‘আমতের সাথে কোনও রকমের দুঃখ-কষ্ট থাকবে না। رَحْمٰنُ ও رَحِيْمُ -এর মধ্যে এই যে পার্থক্য, এটা প্রকাশ করার জন্যই رَحْمٰنُ -এর তরজমা করা হয়েছে ‘সকলের প্রতি দয়াবান’ আর رَحِيْمُ-এর তরজমা করা হয়েছে ‘পরম দয়ালু’।"
    },
    new Domain.Tafsir
    {
        Id = 3, SuraIndex = 2, AyatIndex = 255, TafsirIndexInSura = 1,
        Content = "আয়াতুল কুরসী: এই আয়াতে আল্লাহর সর্বশক্তিমান হওয়ার ব্যাখ্যা রয়েছে।"
    }
};

            var renderedPdf = GetRenderedPdf(suras, ayats, tafsirs);
            renderedPdf.ShowInCompanion();
        }


        private Document GetRenderedPdf(List<Domain.Sura> suras, List<Ayat> ayats, List<Domain.Tafsir> tafsirs)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(50);
                    page.DefaultTextStyle(x => x.FontColor(Colors.Black));

                    page.Background()
                        .ExtendHorizontal()
                        .ExtendVertical()
                        .Padding(12)
                        .Border(1, Colors.Black);

                    page.Content().Column(col =>
                    {
                        col.Spacing(15);

                        foreach (var sura in suras)
                        {
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().LineHorizontal(1).LineColor(Colors.Black);
                                row.AutoItem().Text("۞").FontSize(12).AlignCenter();
                                row.RelativeItem().LineHorizontal(1).LineColor(Colors.Black);
                            });

                            col.Item().Text(sura.Name)
                                .FontSize(20).Bold().AlignCenter();

                            col.Item().Row(row =>
                            {
                                row.RelativeItem().LineHorizontal(1).LineColor(Colors.Black);
                            });

                            var ayatsInSura = ayats.Where(a => a.SuraIndex == sura.SuraIndex);

                            foreach (var ayat in ayatsInSura)
                            {
                                col.Item().Column(verseCol =>
                                {
                                    verseCol.Item().Row(row =>
                                    {
                                        row.ConstantItem(26).Height(18)
                                           .Border(1, Colors.Black)
                                           .CornerRadius(9)
                                           .AlignCenter().AlignMiddle()
                                           .Text(ayat.AyatIndex.ToString()).Bold().FontSize(10);

                                        row.RelativeItem()
                                           .PaddingLeft(8)
                                           .Text(ayat.Content)
                                           .FontSize(10)
                                           .Bold();
                                    });

                                    var tafsirsInAyat = tafsirs.Where(t =>
                                        t.SuraIndex == sura.SuraIndex &&
                                        t.AyatIndex == ayat.AyatIndex);

                                    if (tafsirsInAyat.Any())
                                    {
                                        verseCol.Item()
                                            .PaddingLeft(24)
                                            .Column(tcol =>
                                            {
                                                tcol.Spacing(6);
                                                foreach (var t in tafsirsInAyat)
                                                {
                                                    tcol.Item()
                                                       .Text($"{t.TafsirIndexInSura}. {t.Content}")
                                                       .FontSize(8)
                                                       .FontColor(Colors.Grey.Darken2);
                                                }
                                            });
                                    }
                                });
                                col.Item().PaddingBottom(4);
                            }
                        }
                    });

                    page.Footer().AlignCenter().Text(txt =>
                    {
                        txt.CurrentPageNumber().FontSize(10);
                    });
                });
            });
            return document;
        }

        public async Task<(List<Domain.Sura> Suras, List<Ayat>Ayats, List<Domain.Tafsir>tafsirs)> FetchDataAsync()
        {
            try
            {

                var suraTask =  _context.Suras.ToListAsync();
                var ayatTask = _context.Ayats.ToListAsync();
                var tafsirTask = _context.Tafsirs.ToListAsync();

                await Task.WhenAll(suraTask, ayatTask, tafsirTask);
                return (await suraTask, await ayatTask, await tafsirTask);

            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new InvalidOperationException("Failed to Fetch Data", ex);
            }
        }
    }
}