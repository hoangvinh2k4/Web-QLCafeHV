using Microsoft.EntityFrameworkCore;
using QLCafeHV.Models;
using QLCafeHV.Models.DbConnect;
using QLCafeHV.Models.ViewModels;

namespace QLCafeHV.Services
{
    public class MergeService : IMergeService
    {
        private readonly CoffeeContext _context;

        public MergeService(CoffeeContext context)
        {
            _context = context;
        }

        public async Task<bool> MergeTables(int targetTableId, int sourceTableId)
        {
            var targetOrder = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o =>
                    o.TableID == targetTableId &&
                    o.Status == "Đang phục vụ");

            var sourceOrder = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o =>
                    o.TableID == sourceTableId &&
                    o.Status == "Đang phục vụ");

            if (targetOrder == null || sourceOrder == null)
                return false;

            foreach (var item in sourceOrder.OrderDetails)
            {
                var existing = targetOrder.OrderDetails
                    .FirstOrDefault(x => x.ProductID == item.ProductID);

                if (existing != null)
                {
                    existing.Quantity += item.Quantity;
                    existing.TotalPrice = existing.Quantity * existing.UnitPrice;
                }
                else
                {
                    targetOrder.OrderDetails.Add(new OrderDetailModel
                    {
                        ProductID = item.ProductID,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.TotalPrice,
                        Note = item.Note
                    });
                }
            }

            targetOrder.TotalAmount = targetOrder.OrderDetails.Sum(x => x.TotalPrice)
                                   + sourceOrder.OrderDetails.Sum(x => x.TotalPrice);

            _context.OrderDetails.RemoveRange(sourceOrder.OrderDetails);
            _context.Orders.Remove(sourceOrder);

            var sourceTable = await _context.Tables.FindAsync(sourceTableId);
            sourceTable.Status = "Đang trống";
            sourceTable.UpdatedTime = DateTime.Now;

            await _context.SaveChangesAsync();

            return true;
        }
    }

}