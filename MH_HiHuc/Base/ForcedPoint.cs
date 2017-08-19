using System;

namespace MH_HiHuc.Base
{
    public class ForcedPoint : PointD
    {
        public double Power { get; set; }
        public int AffectTurn { get; set; }

        public ForcedPoint(double x, double y, double power) : base(x, y)
        {
            X = x;
            Y = y;
            Power = power;
        }

        private double GetForce(double x, double y, double forceReducer)
        {
            return Power / Math.Pow(this.Distance(x, y), forceReducer);
        }

        private double GetForce(PointD point, double weight)
        {
            return GetForce(point.X, point.Y, weight);
        }

        public PointD Force(PointD forceTarget)
        {
            var force = GetForce(forceTarget, 2);

            //Find the bearing from the point to us
            var ang = Utilities.NormaliseBearing(forceTarget.GetBearing(this));

            //Add the components of this force to the total force in their respective directions
            forceTarget.X -= Math.Sin(ang) * force;
            forceTarget.Y -= Math.Cos(ang) * force;

            return forceTarget;
        }
    }
}