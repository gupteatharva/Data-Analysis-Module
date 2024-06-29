using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data
{
  public class FileContext: DbContext
  {
    public FileContext(DbContextOptions<FileContext> options)
        : base(options)
    {
    }
    public DbSet<FileDetail> FileDetails { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      optionsBuilder.UseSqlServer("Data Source=KAUSHALBHIDE; database=FileDetailsDB; Integrated Security=True; Encrypt=False");
      base.OnConfiguring(optionsBuilder);
    }
  }
}
