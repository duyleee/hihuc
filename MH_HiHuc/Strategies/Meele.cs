using System;
using Robocode;
using MH_HiHuc.Base;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Robocode.Util;

namespace MH_HiHuc.Strategies
{
    public class Meele : StrategyBase, IStrategy
    {
        public List<ForcedPoint> BulletForces = new List<ForcedPoint>();
        public Meele(HiHucCore robot)
        {
            MyBot = robot;
        }
        public override void Init()
        {
            MyBot.SetColors(Utilities.GetTeamColor(), Color.Red, Color.DarkCyan);

            MyBot.IsAdjustGunForRobotTurn = true;
            MyBot.IsAdjustRadarForGunTurn = true;
            MyBot.TurnRadarRightRadians(2 * Math.PI);
        }

        public override void Run()
        {
            ForceMoving();
            MyBot.SetTurnRadarLeftRadians(2 * Math.PI);
            MyBot.Execute();
        }

        public override void OnHitByBullet(HitByBulletEvent e)
        {
            BulletForces.Add(new ForcedPoint(this.MyBot.Position.X, this.MyBot.Position.Y, 5000)
            {
                Color = Color.Red,
                AffectTurn = 30
            });
        }

        public override void OnScannedRobot(ScannedRobotEvent e)
        {
            var closestEnemy = MyBot.GetClosestTarget();
            if (closestEnemy != null && closestEnemy.Name == e.Name)
            {
                var bullerPower = 300 / e.Distance;

                //simplest linear targeting algorithm - http://robowiki.net/wiki/Linear_Targeting
                double absoluteBearing = MyBot.HeadingRadians + e.BearingRadians;
                MyBot.SetTurnGunRightRadians(Utils.NormalRelativeAngle(absoluteBearing -
                               MyBot.GunHeadingRadians + (e.Velocity * Math.Sin(e.HeadingRadians -
                               absoluteBearing) / Rules.GetBulletSpeed(bullerPower))));

                MyBot.SetFire(bullerPower);
            }
        }

        #region Moving
        private List<ForcedPoint> recentForces = new List<ForcedPoint>();
        double midpointstrength = 0;
        int midpointcount = 0;
        Random randomizer = new Random();
        internal virtual List<ForcedPoint> GetRecentForced()
        {
            List<ForcedPoint> forces = new List<ForcedPoint>();
            #region Tank Forced
            Enemy[] enemies = new Enemy[MyBot.Targets.Values.Count];
            MyBot.Targets.Values.CopyTo(enemies, 0);
            foreach (var tank in enemies)
            {
                if (tank.Live == true && tank.Name != MyBot.Name) // prevent my name because location send through message from other droid
                {
                    var tankForce = new ForcedPoint(tank.X, tank.Y, tank.IsTeamate ? 3000 : 5000);
                    forces.Add(tankForce);
                }
            }
            #endregion

            #region Middle-Field Forced
            midpointcount++;
            if (midpointcount > 5)
            {
                midpointcount = 0;
                midpointstrength = (randomizer.NextDouble() * 5000);
            }
            var middleFieldForce = new ForcedPoint(MyBot.BattleFieldWidth / 2, MyBot.BattleFieldHeight / 2, midpointstrength);
            forces.Add(middleFieldForce);
            #endregion

            #region Bullets forced
            if (BulletForces.Count > 0)
            {
                foreach (var bulletForce in BulletForces)
                {
                    forces.Add(bulletForce);
                    bulletForce.AffectTurn--;
                }
                BulletForces = BulletForces.FindAll(bf => bf.AffectTurn > 0);
            }
            #endregion

            #region Wall forced
            var wallForcePower = 4500;
            var rightWallForce = (new ForcedPoint(MyBot.BattleFieldWidth, MyBot.Y, wallForcePower)); forces.Add(rightWallForce);
            var leftWallForce = (new ForcedPoint(0, MyBot.Y, wallForcePower)); forces.Add(leftWallForce);
            var topWallForce = (new ForcedPoint(MyBot.X, MyBot.BattleFieldHeight, wallForcePower)); forces.Add(topWallForce);
            var bottomWallForce = (new ForcedPoint(MyBot.X, 0, wallForcePower)); forces.Add(bottomWallForce);
            #endregion

            return forces;
        }

        private void ForceMoving()
        {
            PointD nextPosition = MyBot.Position;

            recentForces.Clear();
            recentForces = GetRecentForced();

            // Adjust position by forces
            for (int i = 0; i < recentForces.Count; i++)
            {
                nextPosition = recentForces[i].Force(nextPosition);
            }

            //Prevent wall
            var wallMargin = 36 / 2 + 1;
            if (nextPosition.X <= wallMargin)
            {
                nextPosition.X = wallMargin;
            }
            if (nextPosition.X >= 1000 - wallMargin)
            {
                nextPosition.X = 1000 - wallMargin;
            }
            if (nextPosition.Y <= wallMargin)
            {
                nextPosition.Y = wallMargin;
            }
            if (nextPosition.Y >= 1000 - wallMargin)
            {
                nextPosition.Y = 1000 - wallMargin;
            }

            //Move in the direction of our resolved force.
            MyBot.GotoPoint(nextPosition);
        }
        #endregion

        #region Debugging
        public override void OnPaint(IGraphics graphics)
        {
            var currentForces = new ForcedPoint[recentForces.Count];
            recentForces.CopyTo(currentForces);

            foreach (var item in currentForces)
            {
                var pen = new Pen(item.Color);
                var teamMatePen = new Pen(Color.Orange);
                var brush = new SolidBrush(item.Color);
                var size = (float)item.Power / 50;
                var drawPoint = new RectangleF
                {
                    X = (float)item.X - size / 2,
                    Y = (float)item.Y - size / 2,
                    Height = size,
                    Width = size
                };
                graphics.DrawEllipse(item.Power == 3000 ? teamMatePen : pen, drawPoint);

                graphics.DrawString(item.Power.ToString(), new Font(FontFamily.GenericSerif, 150, FontStyle.Bold, GraphicsUnit.Millimeter), Brushes.White, new PointF
                {
                    X = (float)item.X,
                    Y = (float)item.Y
                });

            }

            var botsize = 100;
            graphics.DrawEllipse(Pens.DarkCyan, new RectangleF
            {
                X = (float)MyBot.X - botsize / 2,
                Y = (float)MyBot.Y - botsize / 2,
                Height = botsize,
                Width = botsize
            });
        }
        #endregion
    }
}
