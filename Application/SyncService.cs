using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using QuestPDF.Companion;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
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
            List<Domain.Sura> suras = new();
            List<Ayat> ayats = new();
            List<Domain.Tafsir> tafsirs = new();

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
                                                       .Italic()
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