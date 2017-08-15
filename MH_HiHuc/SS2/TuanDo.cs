using System;
using System.Drawing;
using Robocode;
using Robocode.Util;

namespace TuanDo
{

    public class TuanDo : Robot
    {
        private int count; 
        private double gunTurnAmt; // How much to turn our gun when searching
        private String trackName; // Name of the robot we're currently tracking

        /// <summary>
        ///   run:  Tracker's main run function
        /// </summary>
        public override void Run()
        {
            // Set colors
            BodyColor = Color.Purple;
            GunColor = Color.Purple;
            RadarColor = Color.Purple;
            ScanColor = (Color.White);
            BulletColor = Color.Purple;

            // Prepare gun
            trackName = null; // Initialize to not tracking anyone
            IsAdjustGunForRobotTurn = (true); // Keep the gun still when we turn
            gunTurnAmt = 10; // Initialize gunTurn to 10

            // Loop forever
            while (true)
            {
                // turn the Gun (looks for enemy)
                TurnGunRight(gunTurnAmt);
                // Keep track of how long we've been looking
                count++;
                // If we've haven't seen our target for 2 turns, look left
                if (count > 2)
                {
                    gunTurnAmt = -10;
                }
                // If we still haven't seen our target for 5 turns, look right
                if (count > 5)
                {
                    gunTurnAmt = 10;
                }
                // If we *still* haven't seen our target after 10 turns, find another target
                if (count > 11)
                {
                    trackName = null;
                }
            }
        }

        /// <summary>
        ///   onScannedRobot:  Here's the good stuff
        /// </summary>
        public override void OnScannedRobot(ScannedRobotEvent e)
        {
            // If we have a target, and this isn't it, return immediately
            // so we can get more ScannedRobotEvents.
            if (trackName != null && e.Name != trackName)
            {
                return;
            }

            // If we don't have a target, well, now we do!
            if (trackName == null)
            {
                trackName = e.Name;
                Out.WriteLine("Tracking " + trackName);
            }
            // This is our target.  Reset count (see the run method)
            count = 0;
            // If our target is too far away, turn and move toward it.
            if (e.Distance > 150)
            {
                gunTurnAmt = Utils.NormalRelativeAngleDegrees(e.Bearing + (Heading - RadarHeading));

                TurnGunRight(gunTurnAmt); // Try changing these to setTurnGunRight,
                TurnRight(e.Bearing); // and see how much Tracker improves...
                // (you'll have to make Tracker an AdvancedRobot)
                Ahead(e.Distance - 140);
                return;
            }

            // Our target is close.
            gunTurnAmt = Utils.NormalRelativeAngleDegrees(e.Bearing + (Heading - RadarHeading));
            TurnGunRight(gunTurnAmt);
            FireIntel();

            // Our target is too close!  Back up.
            if (e.Distance < 100)
            {
                if (e.Bearing > -90 && e.Bearing <= 90)
                {
                    Back(40);
                }
                else
                {
                    Ahead(40);
                }
            }
            Scan();
        }

        /// <summary>
        ///   onHitRobot:  Set him as our new target
        /// </summary>
        public override void OnHitRobot(HitRobotEvent e)
        {
            // Only print if he's not already our target.
            if (trackName != null && trackName != e.Name)
            {
                Out.WriteLine("Tracking " + e.Name + " due to collision");
            }
            // Set the target
            trackName = e.Name;

            gunTurnAmt = Utils.NormalRelativeAngleDegrees(e.Bearing + (Heading - RadarHeading));
            TurnGunRight(gunTurnAmt);
            FireIntel();
            Back(50);
            TurnLeft(90);
        }

       
        private void FireIntel()
        {
            Fire(3);
        }

        public override void OnHitByBullet(HitByBulletEvent e)
        {
            //base.OnHitByBullet(evnt);
            //turnLeft(90 - e.getB()); 
            Back(100);
            TurnLeft(90);
        }
    }
}
