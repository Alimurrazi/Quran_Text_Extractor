using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using quranTranslationExtractor.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using quranTranslationExtractor.Application;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
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



//// See https://aka.ms/new-console-template for more information

//using quranTranslationExtractor.Data;
//using quranTranslationExtractor.Infrastructure;
//using System.Text.Json;
//using Microsoft.EntityFrameworkCore;

//var builder = WebApplication.CreateBuilder(args);

//var client = new HttpClient();
//var index = 1;

////while(true)
////{
////    var url = $"https://api.muslimbangla.com/sura/103?tables=bn_taqi&page={index}&wordByWord=false&language=bengali";

////    var result = await client.GetStringAsync(url);
////    var value = JsonSerializer.Deserialize<Response>(result);

////    if(value is not null && value.Data.Rows.Verses.Length == 0)
////    {
////        break;
////    }
////    index++;
////}

//builder.Services.AddDbContext<DataContext>(options =>
//    options.UseSqlite(builder.Configuration.GetConnectionString("Database")));