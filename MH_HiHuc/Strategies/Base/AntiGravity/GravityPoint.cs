using System;

namespace MH_HiHuc.Strategies.Base.AntiGravity
{
    public class GravityPoint : PointD
    {
        public double Power { get; set; }

        public GravityPoint(double x, double y, double power) : base(x, y)
        {
            X = x;
            Y = y;
            Power = power;
        }

        public double GetForce(double x, double y, double forceReducer)
        {
            return Power / Math.Pow(this.Distance(x, y), forceReducer);
        }

        public double GetForce(PointD point, double weight)
        {
            return GetForce(point.X, point.Y, weight);
        }
    }
}