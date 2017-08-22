using System;

namespace MH_HiHuc.Base
{
    [Serializable()]
    public class PointD
    {        
        public System.Drawing.Color Color { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public PointD(double x, double y)
        {
            Color = System.Drawing.Color.White;
            X = x;
            Y = y;
        }
        public PointD()
        {
            Color = System.Drawing.Color.White;
            X = 0;
            Y = 0;
        }

        public double Distance(double x, double y)
        {
            double xo = X - x;
            double yo = Y - y;
            return Math.Sqrt(xo * xo + yo * yo);
        }

        public double Distance(PointD point)
        {
            return Distance(point.X, point.Y);
        }

        public double GetBearing(double x, double y)
        {
            var radians =  Math.PI / 2 - Math.Atan2(y- Y, x - X);
            var degrees = Utilities.RadiansToDegrees(radians); //for debugging
            return radians;
        }

        public double GetBearing(PointD point)
        {
            return GetBearing(point.X, point.Y);
        }
    }
}
