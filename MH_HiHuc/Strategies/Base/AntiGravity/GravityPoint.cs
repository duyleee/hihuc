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

        public double GetForce(double x, double y, double weight)
        {
            return Power / Math.Pow(this.Distance(x, y), weight);
        }
    }
}