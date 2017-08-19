using MH_HiHuc.Strategies;
using Robocode;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MH_HiHuc.Base
{
    public class HiHucCore : TeamRobot
    {
        internal Dictionary<string, Enemy> Targets = new Dictionary<string, Enemy>();
        internal IStrategy Stragegy { get; set; }
        internal PointD Position
        {
            get
            {
                return new PointD(X, Y);
            }
        }

        public override void OnScannedRobot(ScannedRobotEvent e)
        {
            TrackEnemy(e);

            Stragegy.OnScannedRobot(e);
        }

        private void TrackEnemy(ScannedRobotEvent e)
        {
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
            Targets[e.Name].IsTeamate = IsTeammate(e.Name);

            // Send enemy to droid
            if (!Targets[e.Name].IsTeamate)
            {
                BroadcastMessage(Targets[e.Name]);
            }
        }

        public override void OnRobotDeath(RobotDeathEvent evnt)
        {
            if (Targets.ContainsKey(evnt.Name))
            {
                Targets[evnt.Name].Live = false;
            }

        }

        public override void OnHitRobot(HitRobotEvent e)
        {
            Stragegy.OnHitRobot(e);
        }

        public override void OnHitByBullet(HitByBulletEvent evnt)
        {
            Stragegy.OnHitByBullet(evnt);
        }

        public override void OnPaint(IGraphics graphics)
        {
            Stragegy.OnPaint(graphics);
            base.OnPaint(graphics);
        }

        internal void GotoPoint(PointD point)
        {
            Console.WriteLine("Going to " + point.X + "," + point.Y);
            double dist = 20;
            double angle = Utilities.RadiansToDegrees(Position.GetBearing(point));
            double r = TurnByDegrees(angle);
            SetAhead(dist * r);
        }

        internal int TurnByDegrees(double angle)
        {
            double ang;
            int dir;
            ang = Utilities.NormaliseBearing(Heading - angle);
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
            SetTurnLeft(ang);
            return dir;
        }

        internal int EnemyCount()
        {
            Enemy[] enemies = new Enemy[Targets.Values.Count];
            Targets.Values.CopyTo(enemies, 0);
            return enemies.Count(c => c.Live && !c.IsTeamate);
        }

        internal int TeamCount()
        {
            Enemy[] enemies = new Enemy[Targets.Values.Count];
            Targets.Values.CopyTo(enemies, 0);
            return enemies.Count(c => c.Live && c.IsTeamate);
        }

        public override void OnMessageReceived(MessageEvent evnt)
        {
            if (evnt.Message is Enemy)
            {
                var enemy = (Enemy)evnt.Message;
                Targets[enemy.Name] = enemy;
                Stragegy.OnEnemyMessage(enemy);
            }
        }
    }
}