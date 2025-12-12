using Microsoft.EntityFrameworkCore;

namespace QLCafeHV.Models.DbConnect
{
    public class CoffeeContext : DbContext
    {
        public CoffeeContext(DbContextOptions<CoffeeContext> options) : base(options) { }

        public DbSet<TableModel> Tables { get; set; }
        public DbSet<ProductModel> Products { get; set; }
        public DbSet<OrderModel> Orders { get; set; }
        public DbSet<OrderDetailModel> OrderDetails { get; set; }
        public DbSet<EmployeeModel> Employees { get; set; }
        public DbSet<AccountModel> Accounts { get; set; }
        public DbSet<PaymentModel> Payments { get; set; }
    
    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //    {
    //        optionsBuilder
    //            .UseSqlServer("Server=LAPTOP-88BI5LGJ\\SQLEXPRESS;Database=QLCafeHV;Trusted_Connection=True;TrustServerCertificate=True;")
    //            .EnableSensitiveDataLogging(); 
    //    }
    }
}
