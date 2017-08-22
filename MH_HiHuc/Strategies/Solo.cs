using System;
using MH_HiHuc.Base;
using Robocode;
using System.Drawing;
using Robocode.Util;

namespace MH_HiHuc.Strategies
{
    internal class Solo : StrategyBase, IStrategy
    {
        public Solo(HiHucCore bot)
        {
            MyBot = bot;
        }

        public override void Init()
        {
            MyBot.SetColors(Utilities.GetTeamColor(), Color.Yellow, Color.DarkCyan);

            MyBot.IsAdjustGunForRobotTurn = true;
        }

        private string currentTarget = string.Empty;
        public override void OnScannedRobot(ScannedRobotEvent e)
        {
            if (MyBot.IsTeammate(e.Name))
            {
                return;
            }
            currentTarget = e.Name;
            Move(e);
            RadarAdjust(e);
            Fire(e);
        }

        private int turnDirection = 1;
        private int moveDirection = 1;
        private double randomDistance = 200;
        private Random randomizer = new Random();
        private void Move(ScannedRobotEvent e)
        {
            //http://mark.random-article.com/weber/java/robocode/lesson5.html - closing in movement
            if (MyBot.DistanceRemaining == 0.0)
            {
                moveDirection *= -1; //revert body direction
                turnDirection *= -1; //revert turn direction
                MyBot.SetAhead(randomDistance * moveDirection);
            }

            var turnAngle = e.BearingRadians + Math.PI / 2;// 90o to enemy
            double randomClosingInAngle = Math.PI / randomizer.Next(5, 7);
            randomClosingInAngle = randomClosingInAngle * moveDirection; // turn close to enemy
            randomClosingInAngle = randomClosingInAngle * (e.Distance > 50 ? 1 : -1); // move out when too close

            MyBot.SetTurnRightRadians(turnAngle - randomClosingInAngle);
        }

        public override void OnHitByBullet(HitByBulletEvent e)
        {
            UpdateMoveFactor();
        }

        public override void OnHitRobot(HitRobotEvent evnt)
        {
            MyBot.Fire(3); // fire enemy when hit him
            UpdateMoveFactor();
        }

        private void UpdateMoveFactor()
        {
            randomDistance += new Random().NextDouble() * 10 * turnDirection;

            if (randomDistance <= 50)
            {
                randomDistance = 50;
            }

            if (randomDistance >= 400)
            {
                randomDistance = 400;
            }
        }

        private void RadarAdjust(ScannedRobotEvent e)
        {
            //radar scan 1 vs 1 algorithm - perfect radar lock - http://robowiki.net/wiki/One_on_One_Radar
            double radarAngleToTurn = MyBot.HeadingRadians - MyBot.RadarHeadingRadians + e.BearingRadians;
            MyBot.SetTurnRadarRightRadians(Utils.NormalRelativeAngle(radarAngleToTurn));
        }

        private void Fire(ScannedRobotEvent e)
        {
            var bullerPower = 500 / e.Distance;

            //simplest linear targeting algorithm - http://robowiki.net/wiki/Linear_Targeting
            double absoluteBearing = MyBot.HeadingRadians + e.BearingRadians;
            MyBot.SetTurnGunRightRadians(Utils.NormalRelativeAngle(absoluteBearing -
                           MyBot.GunHeadingRadians + (e.Velocity * Math.Sin(e.HeadingRadians -
                           absoluteBearing) / Rules.GetBulletSpeed(bullerPower))));

            MyBot.SetFire(bullerPower);
        }

        public override void Run()
        {
            MyBot.TurnRadarRight(360);
        }

        public override void OnPaint(IGraphics graphics)
        {
            if (!string.IsNullOrEmpty(currentTarget) && MyBot.Targets.ContainsKey(currentTarget))
            {
                var enemy = MyBot.Targets[currentTarget];
                var myBotX = MyBot.Position.X;
                var myBotY = MyBot.Position.Y;

                //line to enemy
                graphics.DrawLine(Pens.Red, (float)enemy.X, (float)enemy.Y, (float)myBotX, (float)myBotY);

                //highlight enemy
                graphics.DrawEllipse(Pens.Red, new RectangleF
                {
                    X = (float)enemy.X - 50,
                    Y = (float)enemy.Y - 50,
                    Height = 100,
                    Width = 100
                });

                //90o line with enemy line
                var distance = enemy.Position.Distance(MyBot.Position) + 50;
                var angLeft = Utilities.NormaliseBearing(MyBot.Position.GetBearing(enemy.Position) + Math.PI / 2);
                var angRight = Utilities.NormaliseBearing(MyBot.Position.GetBearing(enemy.Position) - Math.PI / 2);
                var pointSize = 10;

                var startX = myBotX + Math.Sin(angLeft) * distance;
                var startY = myBotY + Math.Cos(angLeft) * distance;
                var endX = myBotX + Math.Sin(angRight) * distance;
                var endY = myBotY + Math.Cos(angRight) * distance;
                graphics.DrawEllipse(Pens.Red, new RectangleF
                {
                    X = (float)startX - pointSize / 2,
                    Y = (float)startY - pointSize / 2,
                    Height = pointSize,
                    Width = pointSize
                });
                graphics.DrawEllipse(Pens.Red, new RectangleF
                {
                    X = (float)endX - pointSize / 2,
                    Y = (float)endY - pointSize / 2,
                    Height = pointSize,
                    Width = pointSize
                });
                graphics.DrawLine(Pens.Red, (float)startX, (float)startY, (float)endX, (float)endY);
            }
        }
    }
}
