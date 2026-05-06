using QLCafeHV.Models;
using QLCafeHV.Models.DbConnect;
using QLCafeHV.Service.Response;

public class DiscountService
{
    private readonly CoffeeContext _context;

    public DiscountService(CoffeeContext context)
    {
        _context = context;
    }

    // ================= CREATE =================
    public async Task<ServiceResult<DiscountModel>> Create(DiscountModel model)
    {
        if (string.IsNullOrEmpty(model.Code))
        {
            return new ServiceResult<DiscountModel>
            {
                Success = false,
                Message = "Mã giảm giá không được để trống"
            };
        }

        if (_context.Discounts.Any(x => x.Code == model.Code && x.Status == 1))
        {
            return new ServiceResult<DiscountModel>
            {
                Success = false,
                Message = "Mã giảm giá đã tồn tại"
            };
        }

        if (model.StartDate >= model.EndDate)
        {
            return new ServiceResult<DiscountModel>
            {
                Success = false,
                Message = "Ngày bắt đầu phải nhỏ hơn ngày kết thúc"
            };
        }

        model.Status = 1;

        _context.Discounts.Add(model);
        await _context.SaveChangesAsync();

        return new ServiceResult<DiscountModel>
        {
            Success = true,
            Message = "Thêm mã giảm giá thành công",
            Data = model
        };
    }

    // ================= UPDATE =================
    public async Task<ServiceResult<DiscountModel>> Update(int id, DiscountModel model)
    {
        var discount = await _context.Discounts.FindAsync(id);

        if (discount == null)
        {
            return new ServiceResult<DiscountModel>
            {
                Success = false,
                Message = "Mã giảm giá không tồn tại"
            };
        }

        bool exists = _context.Discounts
            .Any(x => x.Code == model.Code && x.DiscountId != id && x.Status == 1);

        if (exists)
        {
            return new ServiceResult<DiscountModel>
            {
                Success = false,
                Message = "Mã đã tồn tại"
            };
        }

        if (model.StartDate >= model.EndDate)
        {
            return new ServiceResult<DiscountModel>
            {
                Success = false,
                Message = "Ngày không hợp lệ"
            };
        }

        discount.Code = model.Code;
        discount.PercentValue = model.PercentValue;
        discount.Quantity = model.Quantity;
        discount.StartDate = model.StartDate;
        discount.EndDate = model.EndDate;
        discount.Status = model.Status;

        await _context.SaveChangesAsync();

        return new ServiceResult<DiscountModel>
        {
            Success = true,
            Message = "Cập nhật thành công",
            Data = discount
        };
    }

    // ================= DELETE (SOFT) =================
    public async Task<ServiceResult<bool>> Delete(int id)
    {
        var discount = await _context.Discounts.FindAsync(id);

        if (discount == null || discount.Status == 0)
        {
            return new ServiceResult<bool>
            {
                Success = false,
                Message = "Không tồn tại",
                Data = false
            };
        }

        // 👉 soft delete
        discount.Status = 0;

        await _context.SaveChangesAsync();

        return new ServiceResult<bool>
        {
            Success = true,
            Message = "Xóa thành công",
            Data = true
        };
    }
}