using System.Drawing;
using Robocode;
using Robocode.Util;
using System;
namespace RoboCodeTournament
{
    public class TQP_LF : Robot
    {
        private int dist = 50; // distance to move when we're hit
        private int wallMargin = 250; 

        // The main method of your robot containing robot logics
        public override void Run()
        {
            // -- Initialization of the robot --
            // Set colors
            BodyColor = (Color.Red);
            GunColor = (Color.Black);
            RadarColor = (Color.Yellow);
            BulletColor = (Color.Red);
            ScanColor = (Color.Green);

            // Here we turn the robot to point upwards, and move the gun 90 degrees
            TurnLeft(Heading - 90);
            TurnGunRight(90);

            // Infinite loop making sure this robot runs till the end of the battle round
            while (true)
            {
                // -- Commands that are repeated forever --
                if(DetectWall())
                {
                    TurnRight(90);
                    Ahead(wallMargin-1);
                }
                else
                {
                    Ahead(wallMargin-1);
                }

            }
        }

        // Robot event handler, when the robot sees another robot
        public override void OnScannedRobot(ScannedRobotEvent e)
        {
            SmartFire(e.Distance);
        }

        /// <summary>
        ///   smartFire:  Custom Fire method that determines firepower based on distance.
        /// </summary>
        /// <param name="robotDistance">
        ///   the distance to the robot to Fire at
        /// </param>
        public void SmartFire(double robotDistance)
        {
            if (robotDistance > 200 )// || Energy < 15)
            {
                Fire(2);
            }
            else if (robotDistance > 50)
            {
                Fire(3);
            }
            else
            {
                Fire(3);
            }
        }

        /// <summary>
        ///   onHitByBullet:  Turn perpendicular to the bullet, and move a bit.
        /// </summary>
        public override void OnHitByBullet(HitByBulletEvent e)
        {
            TurnRight(Utils.NormalRelativeAngleDegrees(90 - (Heading - e.Heading)));

            Ahead(dist);
            dist *= -1;
            Scan();
        }

        /// <summary>
        ///   onHitRobot:  Aim at it.  Fire Hard!
        /// </summary>
        public override void OnHitRobot(HitRobotEvent e)
        {
            double turnGunAmt = Utils.NormalRelativeAngleDegrees(e.Bearing + Heading - GunHeading);

            TurnGunRight(turnGunAmt);
            Fire(3);
        }

        public Boolean DetectWall() 
        {
			return (
				// we're too close to the left wall
				(X <= wallMargin ||
				 // or we're too close to the right wall
				 X >= BattleFieldWidth - wallMargin ||
				 // or we're too close to the bottom wall
				 Y <= wallMargin ||
				 // or we're too close to the top wall
				 Y >= BattleFieldHeight - wallMargin)
				);
		}
    }
}
