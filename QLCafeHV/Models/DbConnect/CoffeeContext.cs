using Microsoft.EntityFrameworkCore;

namespace QLCafeHV.Models.DbConnect
{
    public class CoffeeContext : DbContext
    {
        public CoffeeContext(DbContextOptions<CoffeeContext> options) : base(options) { }

        public DbSet<ProductModel> Products { get; set; }
        public DbSet<OrderModel> Orders { get; set; }
        public DbSet<OrderDetailModel> OrderDetails { get; set; }
        public DbSet<EmployeeModel> Employees { get; set; }
        public DbSet<AccountModel> Accounts { get; set; }
    }
}
