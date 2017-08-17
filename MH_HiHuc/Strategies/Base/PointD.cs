using System;

namespace MH_HiHuc.Strategies.Base
{
    public class PointD
    {
        public double X { get; set; }
        public double Y { get; set; }
        public PointD(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double Distance(double x, double y)
        {
            double xo = X - x;
            double yo = Y - y;
            return Math.Sqrt(xo * xo + yo * yo);
        }

        public double GetBearing(double x, double y)
        {
            return Math.PI / 2 - Math.Atan2(y - Y, x - X);
        }

        public double GetBearing(PointD point)
        {
            return GetBearing(point.X, point.Y);
        }
    }
}
