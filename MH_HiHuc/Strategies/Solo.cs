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
            Fire(e, 0);
        }

        private int turnDirection = 1;
        private int moveDirection = 1;
        private int turnFactor = 5;
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
            double randomClosingInAngle = Math.PI / turnFactor;
            turnFactor++;
            if (turnFactor >= 7)
            {
                turnFactor = 5;
            }
            randomClosingInAngle = randomClosingInAngle * moveDirection; // turn close to enemy
            randomClosingInAngle = randomClosingInAngle * (e.Distance > 50 ? 1 : -1); // move out when too close

            MyBot.SetTurnRightRadians(turnAngle - randomClosingInAngle);
        }

        public override void OnHitByBullet(HitByBulletEvent e)
        {
            UpdateMoveFactor();
        }

        public override void OnHitRobot(HitRobotEvent e)
        {
            UpdateMoveFactor();

            if (!MyBot.IsTeammate(e.Name))
            {
                double absoluteBearing = MyBot.HeadingRadians + e.BearingRadians;
                MyBot.TurnGunRightRadians(Utils.NormalRelativeAngle(absoluteBearing -
                               MyBot.GunHeadingRadians));
                MyBot.Fire(3); // fire enemy when hit him
            }
            
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
            //http://robowiki.net/wiki/One_on_One_Radar
            double radarAngleToTurn = MyBot.HeadingRadians - MyBot.RadarHeadingRadians + e.BearingRadians;
            MyBot.SetTurnRadarRightRadians(Utils.NormalRelativeAngle(radarAngleToTurn));
        }

        private void Fire(ScannedRobotEvent e)
        {
            var bullerPower = 400 / e.Distance;

            //http://robowiki.net/wiki/Linear_Targeting
            double absoluteBearing = MyBot.HeadingRadians + e.BearingRadians;
            MyBot.SetTurnGunRightRadians(Utils.NormalRelativeAngle(absoluteBearing -
                           MyBot.GunHeadingRadians + (e.Velocity * Math.Sin(e.HeadingRadians -
                           absoluteBearing) / Rules.GetBulletSpeed(bullerPower))));

            MyBot.SetFire(bullerPower);
        }

        private static double _bulletPower = 3; //Our bulletpower.
        private static double _bulletDamage = _bulletPower * 4; //Formula for bullet damage.
        private static double _bulletSpeed = 20 - 3 * _bulletPower; //Formula for bullet speed.

        private double _dir = 1;
        private double _oldEnemyHeading;
        private double _enemyEnergy;
        private void Fire(ScannedRobotEvent e, double absoluteBearing)
        {
            absoluteBearing = MyBot.HeadingRadians + e.BearingRadians;
            //Finding the heading and heading change.
            double enemyHeading = e.HeadingRadians;
            double enemyHeadingChange = enemyHeading - _oldEnemyHeading;
            _oldEnemyHeading = enemyHeading;

            // Circular Targeting http://robowiki.net/wiki/Circular_Targeting
            /*This method of targeting is know as circular targeting; you assume your enemy will
             *keep moving with the same speed and turn rate that he is using at fire time.The 
             *base code comes from the wiki.
            */
            double deltaTime = 0;
            double predictedX = MyBot.X + e.Distance * Math.Sin(absoluteBearing);
            double predictedY = MyBot.Y + e.Distance * Math.Cos(absoluteBearing);
            PointD point = new PointD(MyBot.X, MyBot.Y);
            while ((++deltaTime) * _bulletSpeed < point.Distance(predictedX, predictedY))
            {

                //Add the movement we think our enemy will make to our enemy's current X and Y
                predictedX += Math.Sin(enemyHeading) * e.Velocity;
                predictedY += Math.Cos(enemyHeading) * e.Velocity;


                //Find our enemy's heading changes.
                enemyHeading += enemyHeadingChange;

                //If our predicted coordinates are outside the walls, put them 18 distance units away from the walls as we know 
                //that that is the closest they can get to the wall (Bots are non-rotating 36*36 squares).
                predictedX = Math.Max(Math.Min(predictedX, MyBot.BattleFieldWidth - 18), 18);
                predictedY = Math.Max(Math.Min(predictedY, MyBot.BattleFieldHeight - 18), 18);

            }

            //Find the bearing of our predicted coordinates from us.
            double aim = Utils.NormalAbsoluteAngle(Math.Atan2(predictedX - MyBot.X, predictedY - MyBot.Y));

            var bullerPower = 400 / e.Distance;

            //Aim and fire.
            MyBot.SetTurnGunRightRadians(Utils.NormalRelativeAngle(aim - MyBot.GunHeadingRadians));

            MyBot.SetFire(bullerPower);

            MyBot.SetTurnRadarRightRadians(Utils.NormalRelativeAngle(absoluteBearing - MyBot.RadarHeadingRadians) * 2);
        }

        public override void Run()
        {
            MyBot.TurnRadarRightRadians(2 * Math.PI);
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
