using System;
using Robocode;
using MH_HiHuc.Strategies.Base.AntiGravity;
using MH_HiHuc.Strategies.Base;
using System.Collections;
using System.Collections.Generic;

namespace MH_HiHuc.Strategies
{
    public class Meele : IStrategy
    {
        public AdvancedRobot MyBot { get; set; }
        public PointD MyBotPosition { get
            {
                return new PointD(MyBot.X, MyBot.Y);
            }
        }
        

        private Dictionary<string, Enemy> targets { get; set; }
        double PI = Math.PI;
        double midpointstrength = 0;
        int midpointcount = 0;
        Random randomizer = new Random();
        public Meele(AdvancedRobot robot)
        {
            MyBot = robot;
            targets = new Dictionary<string, Enemy>();
        }
        public void Init()
        {
            MyBot.IsAdjustGunForRobotTurn = true;
            MyBot.IsAdjustRadarForGunTurn = true;
            MyBot.TurnRadarRightRadians(2 * Math.PI);
        }

        public void Run()
        {
            AntiGravityMove();
            MyBot.SetTurnRadarLeftRadians(2 * PI);
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
                    gravityPoint = new GravityPoint(tank.X, tank.Y, -1000);
                    force = gravityPoint.GetForce(MyBot.X, MyBot.Y, 2);

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
            force = gravityPoint.GetForce(MyBot.X, MyBot.Y, 1.5);

            ang = NormaliseBearing(gravityPoint.GetBearing(MyBot.X, MyBot.Y));
            xforce += Math.Sin(ang) * force;
            yforce += Math.Cos(ang) * force;

            /**The following four lines add wall avoidance.  They will only affect us if the bot is close 
            to the walls due to the force from the walls decreasing at a power 3.**/
            xforce += (new GravityPoint(MyBot.BattleFieldWidth, MyBot.Y, 5000)).GetForce(MyBot.X, MyBot.Y, 3);
            xforce -= (new GravityPoint(0, MyBot.Y, 5000)).GetForce(MyBot.X, MyBot.Y, 3);
            yforce += (new GravityPoint(MyBot.X, MyBot.BattleFieldHeight, 5000)).GetForce(MyBot.X, MyBot.Y, 3);
            yforce -= (new GravityPoint(MyBot.X, 0, 5000)).GetForce(MyBot.X, MyBot.Y, 3);

            //Move in the direction of our resolved force.
            GotoPoint(new PointD(MyBot.X - xforce, MyBot.Y - yforce));
        }

        /**Move towards an x and y coordinate**/
        void GotoPoint(PointD point)
        {
            Console.WriteLine("Going to " + point.X + "," + point.Y);
            double dist = 20;
            double angle = ToDegrees(point.GetBearing(MyBot.X, MyBot.Y));
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

        double ToDegrees(double radians)
        {
            return (180 / Math.PI) * radians;
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
    }
}
