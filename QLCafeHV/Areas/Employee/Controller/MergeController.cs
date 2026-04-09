using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLCafeHV.Helpers;
using QLCafeHV.Models;
using QLCafeHV.Models.DbConnect;
using QLCafeHV.Models.ViewModels;
using QLCafeHV.Services;
using static QLCafeHV.Models.ViewModels.PopupViewModel;


namespace QLCafeHV.Employee.Controllers
{
    [Area("Employee")]
    public class MergeController : Controller
    {
        private readonly IMergeService _mergeService; 
        private readonly CoffeeContext _context;
        public MergeController(CoffeeContext context, IMergeService mergeService)
        {
            _context = context;
            _mergeService = mergeService;
        }

        [HttpGet]
        public IActionResult GetOccupiedTables(int currentTableId)
        {
            var tables = _context.Tables
                .Where(t => t.Status == "Đang phục vụ" && t.TableID != currentTableId)
                .Select(t => new
                {
                    t.TableID,
                    t.TableName
                })
                .ToList();

            return Json(tables);
        }
        [HttpPost]
        public async Task<IActionResult> MergeTables(int targetTableId, int sourceTableId)
        {
            var result = await _mergeService.MergeTables(targetTableId, sourceTableId);

            return Json(new
            {
                success = result,
                message = result ? "Gộp bàn thành công" : "Không thể gộp bàn"
            });
        }
    }
}
