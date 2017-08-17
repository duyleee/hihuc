using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MH_HiHuc.Strategies.Base
{
    public static class Utilities
    {
        public static double RadiansToDegrees(double radians)
        {
            return (180 / Math.PI) * radians;
        }
    }
}
