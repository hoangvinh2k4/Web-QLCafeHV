using QLCafeHV.Models;

namespace QLCafeHV.Services
{
    public interface IMergeService
    {
        Task<bool> MergeTables(int targetTableId, int sourceTableId);
    }
}