using QLCafeHV.Models;
using QLCafeHV.Models.DbConnect;
using QLCafeHV.Service.Response;

public class TableService
{
    private readonly CoffeeContext _context;
    private readonly IWebHostEnvironment _env;

    public TableService(CoffeeContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }
    // ================= CREATE =================
    public async Task<ServiceResult<TableModel>> Create(TableModel table)
    {
        if (_context.Tables.Any(t => t.TableName == table.TableName))
        {
            return new ServiceResult<TableModel>
            {
                Success = false,
                Message = "Tên bàn đã tồn tại",
                Data = null
            };
        }

        table.Status = "Đang trống";
        table.CreatedTime = DateTime.Now;

        _context.Tables.Add(table);
        await _context.SaveChangesAsync();

        return new ServiceResult<TableModel>
        {
            Success = true,
            Message = "Thêm bàn thành công",
            Data = table
        };
    }

    // ================= UPDATE =================
    public async Task<ServiceResult<TableModel>> Update(int id, TableModel model)
    {
        var table = await _context.Tables.FindAsync(id);

        if (table == null)
        {
            return new ServiceResult<TableModel>
            {
                Success = false,
                Message = "Bàn không tồn tại",
                Data = null
            };
        }

        bool exists = _context.Tables
            .Any(t => t.TableName == model.TableName && t.TableID != id);

        if (exists)
        {
            return new ServiceResult<TableModel>
            {
                Success = false,
                Message = "Tên bàn đã tồn tại",
                Data = null
            };
        }

        table.TableName = model.TableName;
        table.Status = "Đang trống";
        table.UpdatedTime = DateTime.Now;

        await _context.SaveChangesAsync();

        return new ServiceResult<TableModel>
        {
            Success = true,
            Message = "Cập nhật thành công",
            Data = table
        };
    }

    // ================= DELETE (SOFT) =================
    public async Task<ServiceResult<bool>> Delete(int id)
    {
        var table = await _context.Tables.FindAsync(id);

        if (table == null)
        {
            return new ServiceResult<bool>
            {
                Success = false,
                Message = "Bàn không tồn tại",
                Data = false
            };
        }

        _context.Tables.Remove(table);
        await _context.SaveChangesAsync();

        return new ServiceResult<bool>
        {
            Success = true,
            Message = "Xóa thành công",
            Data = true
        };
    }
}