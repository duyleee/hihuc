using System;

namespace MH_HiHuc.Base
{
    [Serializable()]
    public class Enemy
    {
        public string Name { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public bool IsTeamate { get; set; }
        public double Energy { get; set; }
        public double Velocity { get; set; }
        public double HeadingRadians { get; set; }
        public PointD Position
        {
            get
            {
                return new PointD(X, Y);
            }
        }
        public bool Live;
    }
}
