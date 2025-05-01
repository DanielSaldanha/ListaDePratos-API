using Microsoft.EntityFrameworkCore;
using ListaDePratos.Model;
namespace ListaDePratos.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<ApiModel> cardapio { get; set; }
    }
}
