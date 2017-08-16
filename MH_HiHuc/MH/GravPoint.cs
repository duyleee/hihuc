using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MH_HiHuc.MH
{
    public class GravPoint
    {
        public double x { get; set; }
        public double y { get; set; }
        public double power { get; set; }
        public GravPoint(double pX, double pY, double pPower)
        {
            x = pX;
            y = pY;
            power = pPower;
        }
    }
}
