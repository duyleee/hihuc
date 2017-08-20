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
            Targets[e.Name].X = this.X + Math.Sin(absbearing_rad) * e.Distance;
            Targets[e.Name].Y = this.Y + Math.Cos(absbearing_rad) * e.Distance;
            Targets[e.Name].Live = true;
            Targets[e.Name].Energy = e.Energy;
            Targets[e.Name].Velocity = e.Velocity;
            Targets[e.Name].HeadingRadians = e.HeadingRadians;
            Targets[e.Name].IsTeamate = IsTeammate(e.Name);

            // Update enemy location to teamate
            
                BroadcastMessage(Targets[e.Name]);
            
        }

        public override void OnMessageReceived(MessageEvent evnt)
        {
            if (evnt.Message is Enemy)
            {
                var enemy = (Enemy)evnt.Message;
                if (enemy.Name != Name)
                {
                    Targets[enemy.Name] = enemy;
                    Stragegy.OnEnemyMessage(enemy);
                }
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

        public override void OnBulletHit(BulletHitEvent e)
        {
            Stragegy.OnBulletHit(e);
        }
        public override void OnHitWall(HitWallEvent e)
        {
            Stragegy.OnHitWall(e);
        }

        internal void GotoPoint(PointD point)
        {
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

        internal Enemy GetClosestTarget()
        {
            var _targets = new Enemy[Targets.Values.Count];
            Targets.Values.CopyTo(_targets, 0);
            var _livetargets = _targets.Where(c => c.Live && !c.IsTeamate).ToArray();
            double closestDistance = 1000;
            Enemy closestEnemy = null;
            for (int i = 0; i < _livetargets.Length; i++)
            {
                var distance = _livetargets[i].Position.Distance(this.Position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = _livetargets[i];
                }
            }
            return closestEnemy;
        }
    }
}