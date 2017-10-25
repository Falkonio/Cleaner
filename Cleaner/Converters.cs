using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cleaner
{
    internal static class Converters
    {
        internal static string LongToString(long lng)
        {
            string str = "";
            double dbl = Convert.ToDouble(lng);
            double kb = Convert.ToDouble(1024);
            double mb = Convert.ToDouble(1024 * 1024);
            double gb = Convert.ToDouble(1024 * 1024 * 1024);
            //double tb = Convert.ToDouble(1024 * 1024 * 1024 * 1024);

            if (dbl == 0)
            {
                str = "None";
            }
            else if (dbl / kb >= 1 && dbl / mb < 1)
            {
                str = $"{ (dbl / kb).ToString("###.##").Trim()} KB";
            }
            else if (dbl / mb >= 1 && dbl / gb < 1)
            {
                str = $"{ (dbl / mb).ToString("###.##").Trim()} MB";
            }
            else if (lng / gb >= 1 && dbl / gb / kb < 1)
            {
                str = $"{ (dbl / gb).ToString("###.##").Trim()} GB";
            }
            else if (dbl / gb / kb >= 1)
            {
                str = $"{ (dbl / gb / kb).ToString("### ###.##").Trim()} TB";
            }
            else
            {
                str = $"{dbl.ToString("### ### ### ###").Trim()} Bytes";
            }

            return str;
        }
    }
}
