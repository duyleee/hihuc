using System;

namespace MH_HiHuc.Strategies.Base
{
    public class Enemy
    {
        public string Name { get; set; }
        public double Bearing { get; set; }
        public double Heading { get; set; }
        public double Speed { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public PointD Position
        {
            get
            {
                return new PointD(X, Y);
            }
        }
        public double Distance { get; set; }
        public double Changehead { get; set; }
        public long Ctime;
        public bool Live;
        public PointD GuessPosition(long when)
        {
            double diff = when - Ctime;
            double newY = Y + Math.Cos(Heading) * Speed * diff;
            double newX = X + Math.Sin(Heading) * Speed * diff;
            return new PointD(newX, newY);
        }

        public double GuessFirePower()
        {
            var firePower = 400/Distance;
            if (firePower > 3)
            {
                firePower = 3;
            }
            return firePower;
        }
    }
}
