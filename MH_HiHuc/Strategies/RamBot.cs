using System;
using MH_HiHuc.Base;
using Robocode;
using Robocode.Util;
using System.Drawing;

namespace MH_HiHuc.Strategies
{
    internal class RamBot : StrategyBase, IStrategy
    {
        public RamBot(HiHucCore bot)
        {
            this.MyBot = bot;
        }

        public override void Run()
        {
            if (string.IsNullOrEmpty(currentTarget) || (MyBot.Targets[currentTarget].Live == false && MyBot.Targets.ContainsKey(currentTarget)))
            {
                MyBot.SetTurnRadarLeft(2 * Math.PI);
            }
        }

        public override void Init()
        {
            MyBot.IsAdjustGunForRobotTurn = true;
            MyBot.IsAdjustRadarForGunTurn = true;
            MyBot.SetTurnRadarRightRadians(2 * Math.PI);
            MyBot.SetColors(Utilities.GetTeamColor(), Color.OrangeRed, Color.OrangeRed);
        }

        string currentTarget;
        double dir = 1;
        public override void OnScannedRobot(ScannedRobotEvent e)
        {
            var closestEnemy = MyBot.GetClosestTarget();
            if (!MyBot.IsTeammate(e.Name) && closestEnemy != null && closestEnemy.Name == e.Name)
            {
                currentTarget = e.Name;
                var myBotPosition = MyBot.Position;
                double absBearing = e.BearingRadians + MyBot.HeadingRadians;

                Move(e, absBearing);
                Fire(e, absBearing);
                MyBot.SetTurnRadarRightRadians(Utils.NormalRelativeAngle(absBearing - MyBot.RadarHeadingRadians) * 2);
            }
        }

        private void Move(ScannedRobotEvent e, double absoluteBearing)
        {
            // 90o to enemy
            double turn = absoluteBearing + Math.PI / 2;

            //This formula is used because the 1/e.Distance means that as we get closer to the enemy, we will turn to them more sharply. 
            //We want to do this because it reduces our chances of being defeated before we reach the enemy robot.
            turn -= Math.Max(0.5, (1 / e.Distance) * 100) * dir;

            MyBot.SetTurnRightRadians(Utils.NormalRelativeAngle(turn - MyBot.HeadingRadians));

            MyBot.SetAhead(100 * dir);
        }

        private void Fire(ScannedRobotEvent e, double absoluteBearing)
        {
            var bullerPower = 400 / e.Distance;

            //simplest linear targeting algorithm - http://robowiki.net/wiki/Linear_Targeting
            MyBot.SetTurnGunRightRadians(Utils.NormalRelativeAngle(absoluteBearing -
                           MyBot.GunHeadingRadians + (e.Velocity * Math.Sin(e.HeadingRadians -
                           absoluteBearing) / Rules.GetBulletSpeed(bullerPower))));

            MyBot.SetFire(bullerPower);
        }

        public override void OnHitWall(HitWallEvent e)
        {
            dir = -dir;
        }
    }
}
