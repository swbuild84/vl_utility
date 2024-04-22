using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace vl_utility
{
    public static class MyConvert
    {
        public static double ToDouble(string val)
        {
            try
            {
                if (val == "") return 0;
                string rplc = val.Replace(",", ".");
                return double.Parse(rplc, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                
                throw;
            }
        }
    }
}
