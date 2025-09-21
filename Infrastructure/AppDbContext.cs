using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using quranTranslationExtractor.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace quranTranslationExtractor.Infrastructure
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Ayat> Ayats { get; set; }
        public DbSet<Sura> Suras { get; set; }
        public DbSet<Tafsir> Tafsirs { get; set; }
    }
}
