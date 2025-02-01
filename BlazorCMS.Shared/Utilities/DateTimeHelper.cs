using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorCMS.Shared.Utilities
{
    public static class DateTimeHelper
    {
        public static string ToReadableFormat(DateTime dateTime)
        {
            return dateTime.ToString("MMMM dd, yyyy HH:mm");
        }
    }
}
