using System;
using MH_HiHuc.Base;
using Robocode;
using Robocode.Util;
using System.Drawing;

namespace MH_HiHuc.Strategies
{
    internal class RamBot : StrategyBase, IStrategy
    {
        private static double _bulletPower = 3; //Our bulletpower.
        private static double _bulletDamage = _bulletPower * 4; //Formula for bullet damage.
        private static double _bulletSpeed = 20 - 3 * _bulletPower; //Formula for bullet speed.
        
        private double _dir = 1;
        private double _oldEnemyHeading;
        private double _enemyEnergy;

        public RamBot(HiHucCore bot)
        {
            this.MyBot = bot;
        }

        public override void Run()
        {
            if (MyBot.RadarTurnRemaining == 0.0)
            {
                MyBot.SetTurnRadarRightRadians(2 * Math.PI);
            }
        }

        public override void Init()
        {
            MyBot.IsAdjustGunForRobotTurn = true;
            MyBot.IsAdjustRadarForGunTurn = true;
            MyBot.SetColors(Utilities.GetTeamColor(), Color.OrangeRed, Color.OrangeRed);
        }

        public override void OnScannedRobot(ScannedRobotEvent e)
        {
            var closestEnemy = MyBot.GetClosestTarget();
            if (!MyBot.IsTeammate(e.Name) && closestEnemy != null && closestEnemy.Name == e.Name)
            {
                double absBearing = e.BearingRadians + MyBot.HeadingRadians;
                Move(e, absBearing);
                RadarAdjust(e);
                Fire(e, absBearing);

            }
        }

        public override void OnHitWall(HitWallEvent e)
        {
            _dir = -_dir;
        }

        public override void OnBulletHit(BulletHitEvent e)
        {
            _enemyEnergy -= _bulletDamage;
        }

        private void Move(ScannedRobotEvent e, double absoluteBearing)
        {
            // 90o to enemy
            double turn = absoluteBearing + Math.PI / 2;

            //This formula is used because the 1/e.Distance means that as we get closer to the enemy, we will turn to them more sharply. 
            //We want to do this because it reduces our chances of being defeated before we reach the enemy robot.
            turn -= Math.Max(0.5, (1 / e.Distance) * 100) * _dir;

            MyBot.SetTurnRightRadians(Utils.NormalRelativeAngle(turn - MyBot.HeadingRadians));

            //This line makes us slow down when we need to turn sharply.
            MyBot.MaxVelocity = (400 / MyBot.TurnRemaining);
          
            MyBot.SetAhead(100 * _dir);
        }

        private void Fire(ScannedRobotEvent e, double absoluteBearing)
        {
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

        private void RadarAdjust(ScannedRobotEvent e)
        {
            //radar scan 1 vs 1 algorithm - perfect radar lock - http://robowiki.net/wiki/One_on_One_Radar
            double radarAngleToTurn = MyBot.HeadingRadians - MyBot.RadarHeadingRadians + e.BearingRadians;
            MyBot.SetTurnRadarRightRadians(Utils.NormalRelativeAngle(radarAngleToTurn));
        }
    }
}
