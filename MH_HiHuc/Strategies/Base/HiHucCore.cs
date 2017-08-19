using Robocode;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace MH_HiHuc.Strategies.Base
{
    public class HiHucCore : TeamRobot
    {
        internal Dictionary<string, Enemy> Targets = new Dictionary<string, Enemy>(); 

        //public HiHucCore()
        //{
        //    Color bodyColor = ColorTranslator.FromHtml("#816ea5");
        //    SetColors(bodyColor, Color.BlueViolet, Color.Cyan);
        //}

        public override void OnScannedRobot(ScannedRobotEvent e)
        {
            if (Targets == null) { Targets = new Dictionary<string, Base.Enemy>(); }

            if (!Targets.ContainsKey(e.Name))
            {
                var enemy = new Enemy();
                Targets.Add(e.Name, enemy);
            }
            //the next line gets the absolute bearing to the point where the bot is
            double absbearing_rad = (this.HeadingRadians + e.BearingRadians) % (2 * Math.PI);
            //this section sets all the information about our target
            Targets[e.Name].Name = e.Name;
            double h = Utilities.NormaliseBearing(e.HeadingRadians - Targets[e.Name].Heading);
            h = h / (this.Time - Targets[e.Name].Ctime);
            Targets[e.Name].Changehead = h;
            Targets[e.Name].X = this.X + Math.Sin(absbearing_rad) * e.Distance; //works out the x coordinate of where the target is
            Targets[e.Name].Y = this.Y + Math.Cos(absbearing_rad) * e.Distance; //works out the y coordinate of where the target is
            Targets[e.Name].Bearing = e.BearingRadians;
            Targets[e.Name].Heading = e.HeadingRadians;
            Targets[e.Name].Ctime = this.Time;               //game time at which this scan was produced
            Targets[e.Name].Speed = e.Velocity;
            Targets[e.Name].Distance = e.Distance;
            Targets[e.Name].Live = true;
        }
    }
}
