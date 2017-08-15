using System;
using System.Text;
using System.Drawing;
using Robocode;
using Robocode.Util;


namespace RoboCodeTournament
{
    public class DungTruong_NightBaron : AdvancedRobot
    {
        double radarTurn = double.PositiveInfinity;
        Random randomizer = new Random();
        double bulletPower = 1;

        public override void Run()
        {
            BodyColor = Color.LightGray;
            GunColor = Color.Gray;
            BulletColor = Color.Red;
            RadarColor = Color.LightGray;
            ScanColor = Color.RosyBrown;

            SetTurnRadarRightRadians(radarTurn);

            while (true)
            {
                Ahead(500); // Move ahead 500
                TurnGunRight(360); // Spin gun around
                Back(500); // Move back 500
                TurnGunRight(360); // Spin gun around
                Execute();
            }
        }

        public override void OnScannedRobot(ScannedRobotEvent e)
        {
            // linear searching
            double absoluteBearing = e.BearingRadians + HeadingRadians;
            double linearBearing = absoluteBearing + Math.Asin(e.Velocity / Rules.GetBulletSpeed(bulletPower) * Math.Sin(e.HeadingRadians - absoluteBearing));
            SetTurnGunRightRadians(Utils.NormalRelativeAngle(linearBearing - GunHeadingRadians));
            SetFire(bulletPower);
            SetTurnRightRadians(Utils.NormalRelativeAngle(e.BearingRadians + HeadingRadians - Math.PI / 2));
            SetAhead(randomizer.Next(150, 200));
            SetBack(randomizer.Next(150, 200));
            SetTurnRadarLeftRadians(RadarTurnRemainingRadians);
        }

        public override void OnHitByBullet(HitByBulletEvent e)
        {
            TurnLeft(90 - e.Bearing);
        }
        public override void OnHitWall(HitWallEvent evnt)
        {
            base.OnHitWall(evnt);
        }
        public override void OnBulletHit(BulletHitEvent evnt)
        {
            base.OnBulletHit(evnt);
        }

        public override void OnHitRobot(HitRobotEvent evnt)
        {
            if (evnt.Bearing > -10 && evnt.Bearing < 10)
            {
                Fire(3);
            }
            if (evnt.IsMyFault)
            {
                TurnRight(10);
            }

            if (evnt.Energy > 16)
            {
                Fire(3);
            }
            if (evnt.Energy > 10)
            {
                Fire(2);
            }
            if (evnt.Energy > 5)
            {
                Fire(1);
            }
            Ahead(40);
        }

        public override void OnRobotDeath(RobotDeathEvent evnt)
        {
            base.OnRobotDeath(evnt);
        }
    }
}
