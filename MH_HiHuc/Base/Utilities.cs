using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MH_HiHuc.Base
{
    public static class Utilities
    {
        public static double RadiansToDegrees(double radians)
        {
            return (180 / Math.PI) * radians;
        }

        public static double NormaliseBearing(double ang)
        {
            if (ang > Math.PI)
                ang -= 2 * Math.PI;
            if (ang < -Math.PI)
                ang += 2 * Math.PI;
            return ang;
        }
        public static double NormaliseHeading(double ang)
        {
            if (ang > 2 * Math.PI)
                ang -= 2 * Math.PI;
            if (ang < 0)
                ang += 2 * Math.PI;
            return ang;
        }

        public static Color GetTeamColor()
        {
            return ColorTranslator.FromHtml("#816ea5");
        }
    }
}
