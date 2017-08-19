using System;
using Robocode;
using MH_HiHuc.Strategies.Base;
using System.Collections.Generic;
using System.Drawing;

namespace MH_HiHuc.Strategies
{
    public class Meele : IStrategy
    {
        public HiHucCore MyBot { get; set; }
        public PointD MyBotPosition
        {
            get
            {
                return new PointD(MyBot.X, MyBot.Y);
            }
        }

        Enemy _target;
        private Enemy target
        {
            get
            {
                if (_target == null || _target.Live == false || MyBot.Targets[_target.Name].Live == false)
                {
                    var currentTargets = new Enemy[MyBot.Targets.Values.Count];
                    MyBot.Targets.Values.CopyTo(currentTargets, 0);
                    double closestDistance = 1000;
                    for (int i = 0; i < currentTargets.Length; i++)
                    {
                        var _distance = currentTargets[i].Position.Distance(MyBotPosition);
                        if (_distance < closestDistance)
                        {
                            closestDistance = _distance;
                            _target = currentTargets[i];
                        }
                    }

                }
                return _target != null ? _target : new Enemy()
                {
                    Live = false
                };
            }
        }
        double PI = Math.PI;
        double midpointstrength = 0;
        int midpointcount = 0;
        Random randomizer = new Random();
        double firePower;
        List<ForcedPoint> BulletForces = new List<ForcedPoint>();
        public Meele(HiHucCore robot)
        {
            MyBot = robot;
        }
        public void Init()
        {
            MyBot.IsAdjustGunForRobotTurn = true;
            MyBot.IsAdjustRadarForGunTurn = true;
            MyBot.TurnRadarRightRadians(2 * Math.PI);
        }

        public void Run()
        {
            ForceMoving();
            MyBot.SetTurnRadarLeftRadians(2 * PI);
            doFirePower();
            doGun();
            MyBot.Fire(firePower);
            MyBot.Execute();
        }

        private List<ForcedPoint> recentForces = new List<ForcedPoint>();
        private void ForceMoving()
        {
            recentForces.Clear();
            PointD nextPosition = MyBotPosition;
            Enemy[] enemies = new Enemy[MyBot.Targets.Values.Count];
            MyBot.Targets.Values.CopyTo(enemies, 0);

            #region Tanks forced
            foreach (var tank in enemies)
            {
                if (tank.Live == true)
                {
                    var tankForce = new ForcedPoint(tank.X, tank.Y, 5000);
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
            GotoPoint(nextPosition);
        }

        /**Move towards an x and y coordinate**/
        private void GotoPoint(PointD point)
        {
            Console.WriteLine("Going to " + point.X + "," + point.Y);
            double dist = 20;
            double angle = Utilities.RadiansToDegrees(MyBotPosition.GetBearing(point));
            double r = TurnByDegrees(angle);
            MyBot.SetAhead(dist * r);
        }

        /**Turns the shortest angle possible to come to a heading, then returns the direction the
	       the bot needs to move in.**/
        private int TurnByDegrees(double angle)
        {
            double ang;
            int dir;
            ang = Utilities.NormaliseBearing(MyBot.Heading - angle);
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

        private void doFirePower()
        {
            firePower = 600 / target.Distance;//selects a bullet power based on our distance away from the target
        }

        private void doGun()
        {
            //works out how long it would take a bullet to travel to where the enemy is *now*
            //this is the best estimation we have
            long time = MyBot.Time + (int)(target.Distance / (20 - (3 * firePower)));

            //offsets the gun by the angle to the next shot based on linear targeting provided by the enemy class
            var guessPosition = target.GuessPosition(time);
            double gunOffset = MyBot.GunHeadingRadians - MyBotPosition.GetBearing(guessPosition);
            MyBot.SetTurnGunLeftRadians(Utilities.NormaliseBearing(gunOffset));
        }

        public void OnHitByBullet(HitByBulletEvent e)
        {
            BulletForces.Add(new ForcedPoint(this.MyBotPosition.X, this.MyBotPosition.Y, 5000)
            {
                Color = Color.Red,
                AffectTurn = 30
            });
        }

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
        }

        public void OnRobotDeath(RobotDeathEvent evnt)
        {
            MyBot.Targets[evnt.Name].Live = false;
        }

        public void OnScannedRobot(ScannedRobotEvent e)
        {
        }
    }
}
