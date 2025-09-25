using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuestPDF.Infrastructure;
using quranTranslationExtractor.Application;
using quranTranslationExtractor.Data.Enums;
using quranTranslationExtractor.Infrastructure;
using System.Data;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        using var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlite(context.Configuration.GetConnectionString("DatabaseString")));
                services.AddHttpClient();
                services.AddScoped<ISyncService, SyncService>();
            })
            .Build();

        var syncService = host.Services.GetRequiredService<ISyncService>();

        if (args.Length == 0)
        {
           Console.WriteLine("Enter a command (extract | generatePdf | previewPdf):");
           var input = Console.ReadLine();
           args = new[] { input ?? "" };
        }

        switch (args[0].ToLowerInvariant())
        {
           case PdfCommandType.Extract:
               await syncService.ExtractAndSyncContent();
               break;
           case PdfCommandType.GeneratePdf:
               await syncService.GeneratePDF();
               break;
            case PdfCommandType.PreviewPdf:
                syncService.PreviewPDF();
                break;
            default:
               Console.WriteLine("Unknown command. Use 'extract' or 'generatePdf' or 'previewPdf'.");
               break;
        }
    }
}