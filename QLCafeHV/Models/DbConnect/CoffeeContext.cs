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
        public DbSet<OrderDeliveryModel> OrderDeliveries { get; set; }
        public DbSet<EmployeeModel> Employees { get; set; }
        public DbSet<AccountModel> Accounts { get; set; }
        public DbSet<PaymentModel> Payments { get; set; }
        public DbSet<SalaryAdjustmentModel> SalaryAdjustments { get; set; }
        public DbSet<SalaryModel> Salaries { get; set; }
        public DbSet<DiscountModel> Discounts { get; set; }
        public DbSet<EWorkShiftModel> EWorkShifts { get; set; }
        public DbSet<AWorkShiftModel> AWorkShifts { get; set; }
        public DbSet<ReviewModel> Reviews { get; set; }
    
    }
}
