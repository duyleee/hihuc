using System;
using Robocode;
using MH_HiHuc.Base;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Robocode.Util;

namespace MH_HiHuc.Strategies
{
    public class Meele : IStrategy
    {
        public HiHucCore MyBot { get; set; }
        List<ForcedPoint> BulletForces = new List<ForcedPoint>();
        public Meele(HiHucCore robot)
        {
            MyBot = robot;
        }
        public void Init()
        {
            MyBot.SetColors(Utilities.GetTeamColor(), Color.Red, Color.DarkCyan);

            MyBot.IsAdjustGunForRobotTurn = true;
            MyBot.IsAdjustRadarForGunTurn = true;
            MyBot.TurnRadarRightRadians(2 * Math.PI);
        }

        public void Run()
        {
            ForceMoving();
            MyBot.SetTurnRadarLeftRadians(2 * Math.PI);
            MyBot.Execute();
        }
        
        public void OnHitByBullet(HitByBulletEvent e)
        {
            BulletForces.Add(new ForcedPoint(this.MyBot.Position.X, this.MyBot.Position.Y, 5000)
            {
                Color = Color.Red,
                AffectTurn = 30
            });
        }

        public void OnScannedRobot(ScannedRobotEvent e)
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

        public void OnHitRobot(HitRobotEvent evnt)
        {

        }
        #region Moving
        private List<ForcedPoint> recentForces = new List<ForcedPoint>();
        double midpointstrength = 0;
        int midpointcount = 0;
        Random randomizer = new Random();
        private void ForceMoving()
        {
            recentForces.Clear();
            PointD nextPosition = MyBot.Position;
            Enemy[] enemies = new Enemy[MyBot.Targets.Values.Count];
            MyBot.Targets.Values.CopyTo(enemies, 0);

            #region Tanks forced
            foreach (var tank in enemies)
            {
                if (tank.Live == true)
                {
                    var tankForce = new ForcedPoint(tank.X, tank.Y, tank.IsTeamate ? 3000 : 5000);
                    recentForces.Add(tankForce);
                    nextPosition = tankForce.Force(nextPosition);
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
            recentForces.Add(middleFieldForce);
            nextPosition = middleFieldForce.Force(nextPosition);
            #endregion

            #region Bullets forced
            if (BulletForces.Count > 0)
            {
                foreach (var bulletForce in BulletForces)
                {
                    recentForces.Add(bulletForce);
                    nextPosition = bulletForce.Force(nextPosition);
                    bulletForce.AffectTurn--;
                }
                BulletForces = BulletForces.FindAll(bf => bf.AffectTurn > 0);
            }
            #endregion

            #region Wall forced
            var wallForcePower = 4500;
            var rightWallForce = (new ForcedPoint(MyBot.BattleFieldWidth, MyBot.Y, wallForcePower)); recentForces.Add(rightWallForce);
            var leftWallForce = (new ForcedPoint(0, MyBot.Y, wallForcePower)); recentForces.Add(leftWallForce);
            var topWallForce = (new ForcedPoint(MyBot.X, MyBot.BattleFieldHeight, wallForcePower)); recentForces.Add(topWallForce);
            var bottomWallForce = (new ForcedPoint(MyBot.X, 0, wallForcePower)); recentForces.Add(bottomWallForce);
            nextPosition = rightWallForce.Force(nextPosition);
            nextPosition = leftWallForce.Force(nextPosition);
            nextPosition = topWallForce.Force(nextPosition);
            nextPosition = bottomWallForce.Force(nextPosition);
            #endregion

            //Move in the direction of our resolved force.
            MyBot.GotoPoint(nextPosition);
        }
        #endregion

        #region Debugging
        public void OnPaint(IGraphics graphics)
        {
            var currentForces = new ForcedPoint[recentForces.Count];
            recentForces.CopyTo(currentForces);

            foreach (var item in currentForces)
            {
                var pen = new Pen(item.Color);
                var brush = new SolidBrush(item.Color);
                var size = (float)item.Power / 50;
                var drawPoint = new RectangleF
                {
                    X = (float)item.X - size / 2,
                    Y = (float)item.Y - size / 2,
                    Height = size,
                    Width = size
                };
                graphics.DrawEllipse(pen, drawPoint);

                graphics.DrawString(item.Power.ToString(), new Font(FontFamily.GenericSerif, 150, FontStyle.Bold, GraphicsUnit.Millimeter), Brushes.White, new PointF
                {
                    X = (float)item.X,
                    Y = (float)item.Y
                });

            }

            var botsize = 100;
            graphics.DrawEllipse(Pens.RoyalBlue, new RectangleF
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
