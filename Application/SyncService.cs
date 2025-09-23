using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
            var suras = await _context.Suras.ToListAsync();
            var ayats = await _context.Ayats.ToListAsync();
            var tafsirs = await _context.Tafsirs.ToListAsync();

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "quran_bn_text.pdf");
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Size(PageSizes.A4);

                    foreach (var sura in suras)
                    {
                        page.Content().Column(col =>
                        {
                            col.Item().Text(sura.Name)
                            .FontSize(22).Bold().FontColor(Colors.Green.Darken2);

                            col.Spacing(15);

                            var ayatsInSura = ayats.Where(ayat => ayat.SuraIndex == sura.SuraIndex);

                            foreach (var ayat in ayatsInSura)
                            {
                                col.Item().Column(vcol =>
                                {
                                    vcol.Item().Text($"({ayat.AyatIndex}) {ayat.Content}")
                                        .FontSize(14)
                                        .Bold()
                                        .FontColor(Colors.Black);

                                    var tafsirsInAyat = tafsirs.Where(tafsir => tafsir.SuraIndex == sura.SuraIndex
                                    && tafsir.AyatIndex == ayat.AyatIndex);

                                    if (tafsirsInAyat.Any())
                                    {
                                        foreach (var tafsir in tafsirsInAyat)
                                        {
                                            vcol.Item().Text($"{tafsir.TafsirIndexInSura}   • {tafsir.Content}")
                                                .FontSize(12)
                                                .FontColor(Colors.Grey.Darken2);
                                        }
                                    }
                                });
                            }
                        });
                    }
                    page.Footer().AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Page ").FontSize(10);
                        x.CurrentPageNumber().FontSize(10);
                    });
                });
            }).GeneratePdf(filePath);
        }

        private void RenderPdf(string html)
        {
            //PdfDocument pdf = new PdfDocument();
            //pdf = PdfGenerator.GeneratePdf(html, PageSize.A4);
            //pdf.Save("quran_bn_text.pdf");
        }

    }
}