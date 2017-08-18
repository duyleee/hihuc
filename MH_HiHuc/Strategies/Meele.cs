using System;
using Robocode;
using MH_HiHuc.Strategies.Base.AntiGravity;
using MH_HiHuc.Strategies.Base;
using System.Collections.Generic;
using System.Drawing;

namespace MH_HiHuc.Strategies
{
    public class Meele : IStrategy
    {
        public AdvancedRobot MyBot { get; set; }
        public PointD MyBotPosition
        {
            get
            {
                return new PointD(MyBot.X, MyBot.Y);
            }
        }

        private Dictionary<string, Enemy> targets { get; set; }
        private Enemy target { get; set; }
        double PI = Math.PI;
        double midpointstrength = 0;
        int midpointcount = 0;
        Random randomizer = new Random();
        double firePower;
        public Meele(AdvancedRobot robot)
        {
            MyBot = robot;
            targets = new Dictionary<string, Enemy>();
        }
        public void Init()
        {
            Color col = ColorTranslator.FromHtml("#816ea5");
            MyBot.IsAdjustGunForRobotTurn = true;
            MyBot.IsAdjustRadarForGunTurn = true;
            MyBot.TurnRadarRightRadians(2 * Math.PI);
            MyBot.SetColors(col, col, col);
        }

        public void Run()
        {
            AntiGravityMove();
            MyBot.SetTurnRadarLeftRadians(2 * PI);
            doFirePower();
            //doScanner();
            doGun();
            MyBot.Fire(firePower);
            MyBot.Execute();
        }

        public void OnScannedRobot(ScannedRobotEvent e)
        {
            if (!targets.ContainsKey(e.Name))
            {
                var enemy = new Enemy();
                targets.Add(e.Name, enemy);
            }
            //targets[e.Name]
            //the next line gets the absolute bearing to the point where the bot is
            double absbearing_rad = (MyBot.HeadingRadians + e.BearingRadians) % (2 * PI);
            //this section sets all the information about our target
            target = targets[e.Name];
            targets[e.Name].Name = e.Name;
            double h = NormaliseBearing(e.HeadingRadians - targets[e.Name].Heading);
            h = h / (MyBot.Time - targets[e.Name].Ctime);
            targets[e.Name].Changehead = h;
            targets[e.Name].X = MyBot.X + Math.Sin(absbearing_rad) * e.Distance; //works out the x coordinate of where the target is
            targets[e.Name].Y = MyBot.Y + Math.Cos(absbearing_rad) * e.Distance; //works out the y coordinate of where the target is
            targets[e.Name].Bearing = e.BearingRadians;
            targets[e.Name].Heading = e.HeadingRadians;
            targets[e.Name].Ctime = MyBot.Time;               //game time at which this scan was produced
            targets[e.Name].Speed = e.Velocity;
            targets[e.Name].Distance = e.Distance;
            targets[e.Name].Live = true;
        }

        private void AntiGravityMove()
        {
            double xforce = 0;
            double yforce = 0;
            double force;
            double ang;
            GravityPoint gravityPoint;
            Enemy[] enemies = new Enemy[targets.Values.Count];
            targets.Values.CopyTo(enemies, 0);

            foreach (var tank in enemies)
            {
                if (tank.Live == true)
                {
                    gravityPoint = new GravityPoint(tank.X, tank.Y, 5000);
                    force = gravityPoint.GetForce(MyBotPosition, 2);

                    //Find the bearing from the point to us
                    ang = NormaliseBearing(MyBotPosition.GetBearing(gravityPoint));

                    //Add the components of this force to the total force in their respective directions
                    xforce += Math.Sin(ang) * force;
                    yforce += Math.Cos(ang) * force;
                }
            }

            /**The next section adds a middle point with a random (positive or negative) strength.
		    The strength changes every 5 turns, and goes between -1000 and 1000.  This gives a better
		    overall movement.**/
            midpointcount++;
            if (midpointcount > 5)
            {
                midpointcount = 0;
                midpointstrength = (randomizer.NextDouble() * 2000) - 1000;
            }
            gravityPoint = new GravityPoint(MyBot.BattleFieldWidth / 2, MyBot.BattleFieldHeight / 2, midpointstrength);
            force = gravityPoint.GetForce(MyBotPosition, 1.5);

            ang = NormaliseBearing(gravityPoint.GetBearing(MyBot.X, MyBot.Y));
            xforce += Math.Sin(ang) * force;
            yforce += Math.Cos(ang) * force;

            /**The following four lines add wall avoidance.  They will only affect us if the bot is close 
            to the walls due to the force from the walls decreasing at a power 3.**/
            xforce += (new GravityPoint(MyBot.BattleFieldWidth, MyBot.Y, 5000)).GetForce(MyBotPosition, 2);
            xforce -= (new GravityPoint(0, MyBot.Y, 5000)).GetForce(MyBotPosition, 2);
            yforce += (new GravityPoint(MyBot.X, MyBot.BattleFieldHeight, 5000)).GetForce(MyBotPosition, 2);
            yforce -= (new GravityPoint(MyBot.X, 0, 5000)).GetForce(MyBotPosition, 2);

            //Move in the direction of our resolved force.
            GotoPoint(new PointD(MyBot.X - xforce, MyBot.Y - yforce));
        }

        /**Move towards an x and y coordinate**/
        void GotoPoint(PointD point)
        {
            Console.WriteLine("Going to " + point.X + "," + point.Y);
            double dist = 20;
            double angle = Utilities.RadiansToDegrees(MyBotPosition.GetBearing(point));
            double r = TurnByDegrees(angle);
            MyBot.SetAhead(dist * r);
        }

        /**Turns the shortest angle possible to come to a heading, then returns the direction the
	       the bot needs to move in.**/
        int TurnByDegrees(double angle)
        {
            double ang;
            int dir;
            ang = NormaliseBearing(MyBot.Heading - angle);
            if (ang > 90)
            {
                ang -= 180;
                dir = -1;
            }
            else if (ang < -90)
            {
                ang += 180;
                dir = -1;
            }
            else {
                dir = 1;
            }
            MyBot.SetTurnLeft(ang);
            return dir;
        }



        double NormaliseBearing(double ang)
        {
            if (ang > PI)
                ang -= 2 * PI;
            if (ang < -PI)
                ang += 2 * PI;
            return ang;
        }

        double NormaliseHeading(double ang)
        {
            if (ang > 2 * PI)
                ang -= 2 * PI;
            if (ang < 0)
                ang += 2 * PI;
            return ang;
        }

        void doFirePower()
        {
            firePower = 400 / target.Distance;//selects a bullet power based on our distance away from the target
        }

        void doGun()
        {
            //works out how long it would take a bullet to travel to where the enemy is *now*
            //this is the best estimation we have
            long time = MyBot.Time + (int)(target.Distance / (20 - (3 * firePower)));

            //offsets the gun by the angle to the next shot based on linear targeting provided by the enemy class
            var guessPosition = target.GuessPosition(time);
            double gunOffset = MyBot.GunHeadingRadians - MyBotPosition.GetBearing(guessPosition);
            MyBot.SetTurnGunLeftRadians(NormaliseBearing(gunOffset));
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
                radarOffset = MyBot.RadarHeadingRadians - MyBotPosition.GetBearing(target.X, target.Y);

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
    }
}
