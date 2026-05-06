using Microsoft.EntityFrameworkCore;
using QLCafeHV.Models;
using QLCafeHV.Models.DbConnect;
using QLCafeHV.Service.Response;

public class ReviewService
{
    private readonly CoffeeContext _context;

    public ReviewService(CoffeeContext context)
    {
        _context = context;
    }

    public async Task<ServiceResult<ReviewModel>> Create(ReviewModel model)
    {
        if (model.Rating < 1 || model.Rating > 5)
        {
            return new ServiceResult<ReviewModel>
            {
                Success = false,
                Message = "Số sao không hợp lệ"
            };
        }

        model.CreatedTime = DateTime.Now;

        _context.Reviews.Add(model);
        await _context.SaveChangesAsync();

        return new ServiceResult<ReviewModel>
        {
            Success = true,
            Message = "Đánh giá thành công",
            Data = model
        };
    }

    public async Task<List<ReviewModel>> GetByProduct(int productId)
    {
        return await _context.Reviews
            .Where(x => x.ProductId == productId && x.Status == 1)
            .OrderByDescending(x => x.CreatedTime)
            .ToListAsync();
    }
}