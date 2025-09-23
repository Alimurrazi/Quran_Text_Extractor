using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using quranTranslationExtractor.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using quranTranslationExtractor.Application;
using System.Threading.Tasks;
using QuestPDF.Infrastructure;

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
           Console.WriteLine("Enter a command (extract | generatePdf):");
           var input = Console.ReadLine();
           args = new[] { input ?? "" };
        }

        switch (args[0].ToLowerInvariant())
        {
           case "extract":
               await syncService.ExtractAndSyncContent();
               break;
           case "generatepdf":
               await syncService.GeneratePDF();
               break;
           default:
               Console.WriteLine("Unknown command. Use 'extract' or 'generatePdf'.");
               break;
        }
    }
}