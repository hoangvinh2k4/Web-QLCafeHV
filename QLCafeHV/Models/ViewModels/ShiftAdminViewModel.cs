namespace QLCafeHV.Models.ViewModels
{
    public class ShiftAdminViewModel
    {
        // Cấu hình ca 
        public int ShiftId { get; set; }
        public string ShiftType { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? Note { get; set; }
        public ICollection<EWorkShiftModel> EWorkShifts { get; set; }
        public TimeSpan? WorkedDuration
        {
            get
            {
                var s = EWorkShifts.FirstOrDefault();
                return (s?.OpenTime != null && s?.CloseTime != null)
                    ? s.CloseTime - s.OpenTime
                    : null;
            }
        }
    }
}
