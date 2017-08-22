using System;
using System.Drawing;

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
       
        public static Color GetTeamColor()
        {
            return ColorTranslator.FromHtml("#816ea5");
        }
    }
}
