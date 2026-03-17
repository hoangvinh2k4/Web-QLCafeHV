using System.ComponentModel.DataAnnotations;

namespace QLCafeHV.Models.ViewModels
{
    public class PopupViewModel
    {
        public bool HasOpenShift { get; set; }

        public List<TableModel> Tables { get; set; } = new();
        public List<ShiftItem> ShiftList { get; set; } = new();
        public class ShiftItem
        {
            public int ShiftConfigId { get; set; }
            public string ShiftType { get; set; }
        }
    }
}