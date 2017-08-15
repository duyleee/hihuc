using Robocode;
using Robocode.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboCodeTournament
{
    public class PhiNg_ThanhGiong : RateControlRobot
    {
        private int turnCounter;

        public override void Run()
        {
            // Set colors
            BodyColor = (Color.Cyan);
            GunColor = (Color.Cyan);
            RadarColor = (Color.White);
            ScanColor = (Color.Cyan);
            BulletColor = (Color.Cyan);

            turnCounter = 0;
            GunRotationRate = (15);

            while (true)
            {
                if (turnCounter % 64 == 0)
                {
                    // Straighten out, if we were hit by a bullet and are turning
                    TurnRate = 0;
                    // Go forward with a velocity of 4
                    VelocityRate = 4;
                }
                if (turnCounter % 64 == 32)
                {
                    // Go backwards, faster
                    VelocityRate = -6;
                }
                turnCounter++;
                Execute();
            }
        }

        public override void OnScannedRobot(ScannedRobotEvent e)
        {
            // Calculate exact location of the robot
            double absoluteBearing = Heading + e.Bearing;
            double bearingFromGun = Utils.NormalRelativeAngleDegrees(absoluteBearing - GunHeading);

            // If it's close enough, fire!
            if (Math.Abs(bearingFromGun) <= 3)
            {
                TurnGunRight(bearingFromGun);
                // We check gun heat here, because calling Fire()
                // uses a turn, which could cause us to lose track
                // of the other robot.
                if (GunHeat == 0)
                {
                    Fire(Math.Min(3 - Math.Abs(bearingFromGun), Energy - .1));
                }
            }
            else
            {
                // otherwise just set the gun to turn.
                // Note:  This will have no effect until we call scan()
                TurnGunRight(bearingFromGun);
            }
        }
        
        private int dist = 50; // distance to move when we're hit
        public override void OnHitByBullet(HitByBulletEvent e)
        {
            TurnRight(Utils.NormalRelativeAngleDegrees(90 - (Heading - e.Heading)));

            Ahead(dist);
            dist *= -1;
            Scan();
        }

        public override void OnHitWall(HitWallEvent e)
        {
            // Move away from the wall
            VelocityRate = (-1 * VelocityRate);
        }
    }
}
