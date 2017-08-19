using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MH_HiHuc.Strategies.Base;
using Robocode;

namespace MH_HiHuc.Strategies
{
    public class PredictiveTargeting:IStrategy
    {
        Enemy target;                   //our current enemy
        double PI = Math.PI;          //just a constant
        int direction = 1;              //direction we are heading...1 = forward, -1 = backwards
        double firePower;
        public PointD MyBotPosition
        {
            get
            {
                return new PointD(MyBot.X, MyBot.Y);
            }
        }

        public PredictiveTargeting(HiHucCore robot)
        {
            MyBot = robot;
        }
        public HiHucCore MyBot { get; set; }
        public void Init()
        {
            target = new Enemy { Distance = 100000 };
            Color col = ColorTranslator.FromHtml("#816ea5");
            MyBot.SetColors(col, col, col);
            MyBot.IsAdjustGunForRobotTurn = true;
            MyBot.IsAdjustRadarForGunTurn = true;
            MyBot.TurnRadarRightRadians(2 * Math.PI);
        }

        public void Run()
        {
            doFirePower();
            doScanner();
            doGun();
            MyBot.Fire(firePower);
            MyBot.Execute();
        }

        public void OnScannedRobot(ScannedRobotEvent e)
        {
            if ((e.Distance < target.Distance) || (target.Name == e.Name))
            {
                //the next line gets the absolute bearing to the point where the bot is
                double absbearing_rad = (MyBot.HeadingRadians + e.BearingRadians) % (2 * PI);
                //this section sets all the information about our target
                target.Name = e.Name;
                target.X = MyBot.X + Math.Sin(absbearing_rad) * e.Distance; //works out the x coordinate of where the target is
                target.Y = MyBot.Y + Math.Cos(absbearing_rad) * e.Distance; //works out the y coordinate of where the target is
                target.Bearing = e.BearingRadians;
                target.Heading = e.HeadingRadians;
                target.Ctime = MyBot.Time;           //game time at which this scan was produced
                target.Speed = e.Velocity;
                target.Distance = e.Distance;
            }
        }

        void doFirePower()
        {
            firePower = 400 / target.Distance;//selects a bullet power based on our distance away from the target
        }

        void doScanner()
        {
            double radarOffset;
            if (MyBot.Time - target.Ctime > 4)
            {   //if we haven't seen anybody for a bit....
                radarOffset = 360;      //rotate the radar to find a target
            }
            else
            {

                //next is the amount we need to rotate the radar by to scan where the target is now
                radarOffset = MyBot.RadarHeadingRadians - absbearing(MyBot.X, MyBot.Y, target.X, target.Y);

                //this adds or subtracts small amounts from the bearing for the radar to produce the wobbling
                //and make sure we don't lose the target
                if (radarOffset < 0)
                    radarOffset -= PI / 8;
                else
                    radarOffset += PI / 8;
            }
            //turn the radar
            MyBot.SetTurnRadarLeftRadians(NormaliseBearing(radarOffset));
        }

        void doGun()
        {
            //works out how long it would take a bullet to travel to where the enemy is *now*
            //this is the best estimation we have
            long time = MyBot.Time + (int)(target.Distance / (20 - (3 * firePower)));

            //offsets the gun by the angle to the next shot based on linear targeting provided by the enemy class
            var guessPosition = target.GuessPosition(time);
            double gunOffset = MyBot.GunHeadingRadians - MyBotPosition.GetBearing(guessPosition);
            var abc = Utilities.RadiansToDegrees(gunOffset);
            var tub = Utilities.RadiansToDegrees(NormaliseBearing(gunOffset));
            MyBot.SetTurnGunLeftRadians(NormaliseBearing(gunOffset));
        }
        double NormaliseBearing(double ang)
        {
            if (ang > PI)
                ang -= 2 * PI;
            if (ang < -PI)
                ang += 2 * PI;
            return ang;
        }

        public double absbearing(double x1, double y1, double x2, double y2)
        {
            PointD pointD = new PointD(x1, y1);
            double xo = x2 - x1;
            double yo = y2 - y1;
            double h = pointD.Distance(x2, y2);
            if (xo > 0 && yo > 0)
            {
                return Math.Asin(xo / h);
            }
            if (xo > 0 && yo < 0)
            {
                return Math.PI - Math.Asin(xo / h);
            }
            if (xo < 0 && yo < 0)
            {
                return Math.PI + Math.Asin(-xo / h);
            }
            if (xo < 0 && yo > 0)
            {
                return 2.0 * Math.PI - Math.Asin(-xo / h);
            }
            return 0;
        }

        public void OnHitByBullet(HitByBulletEvent e)
        {
            throw new NotImplementedException();
        }

        public void OnPaint(IGraphics graphics)
        {
            throw new NotImplementedException();
        }

        public void OnRobotDeath(RobotDeathEvent evnt)
        {
            throw new NotImplementedException();
        }

        public void OnHitRobot(HitRobotEvent evnt)
        {
            throw new NotImplementedException();
        }
    }
}
