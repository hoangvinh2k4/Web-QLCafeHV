using QLCafeHV.Models;
using QLCafeHV.Models.DbConnect;

public class ProductService
{
    private readonly CoffeeContext _context;
    private readonly IWebHostEnvironment _env;

    public ProductService(CoffeeContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    // ================= CREATE =================
    public async Task<bool> Create(ProductModel product)
    {
        var file = product.ImageFile;

        if (file == null || file.Length == 0)
            return false;

        string folder = Path.Combine(_env.WebRootPath, "images");

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
        string path = Path.Combine(folder, fileName);

        using (var stream = new FileStream(path, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        product.ImageUrl = "/images/" + fileName;
        product.CreatedTime = DateTime.Now;

        if (product.Status != 0 && product.Status != 1)
            product.Status = 1;

        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        return true;
    }

    // ================= UPDATE =================
    public async Task<bool> Update(int id, ProductModel product)
    {
        var existing = await _context.Products.FindAsync(id);
        if (existing == null) return false;

        // 👉 update image
        if (product.ImageFile != null && product.ImageFile.Length > 0)
        {
            string folder = Path.Combine(_env.WebRootPath, "images");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string fileName = Guid.NewGuid() + Path.GetExtension(product.ImageFile.FileName);
            string path = Path.Combine(folder, fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await product.ImageFile.CopyToAsync(stream);
            }

            // delete old image
            if (!string.IsNullOrEmpty(existing.ImageUrl))
            {
                string oldPath = Path.Combine(_env.WebRootPath, existing.ImageUrl.TrimStart('/'));

                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
            }

            existing.ImageUrl = "/images/" + fileName;
        }

        existing.ProductName = product.ProductName;
        existing.Price = product.Price;
        existing.Category = product.Category;
        existing.Status = product.Status;
        existing.UpdatedTime = DateTime.Now;

        await _context.SaveChangesAsync();
        return true;
    }

    // ================= DELETE (SOFT) =================
    public async Task<bool> Delete(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return false;

        product.Status = 0;

        await _context.SaveChangesAsync();
        return true;
    }
}