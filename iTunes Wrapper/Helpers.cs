using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iTunes_Wrapper
{
    public class Helpers
    {
        public static double BytesToGB(double bytes)
        {
            return bytes / (1024 * 1024 * 1024);
        }

        public static string BytesToGBString(double bytes) => $"{BytesToGB(bytes):N2} GB";
    }
}
