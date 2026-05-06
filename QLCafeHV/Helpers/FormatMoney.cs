using System.Globalization;

namespace QLCafeHV.Helpers
{
    public static class FormatMoney
    {
        public static string ToVnd(this decimal value)
        {
            return value.ToString("N0", new CultureInfo("vi-VN")) + " đ";
        }
    }
}