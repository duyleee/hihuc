#region Copyright (c) 2001-2013 Mathew A. Nelson and Robocode contributors

// Copyright (c) 2001-2013 Mathew A. Nelson and Robocode contributors
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://robocode.sourceforge.net/license/epl-v10.html

#endregion

using System;
using System.Drawing;
using Robocode;
using Robocode.Util;

namespace RoboCodeTournament
{
    public class SH_Robot : Robot
    {
        private bool peek; // Don't turn if there's a robot there
        private double moveAmount; // How much to move
        private bool readyToScan = false; // Don't scan if it doesn't ready to scan robots
        /// <summary>
        ///   run: Move around the walls
        /// </summary>
        public override void Run()
        {
            // Set colors
            BodyColor = (Color.Black);
            GunColor = (Color.Black);
            RadarColor = (Color.Orange);
            BulletColor = (Color.Cyan);
            ScanColor = (Color.Cyan);

            // Initialize moveAmount to the maximum possible for this battlefield.
            moveAmount = Math.Max(BattleFieldWidth, BattleFieldHeight);
            // Initialize peek to false
            peek = false;

            // turnLeft to face a wall.
            // getHeading() % 90 means the remainder of
            // getHeading() divided by 90.
            TurnLeft(Heading % 90);
            Ahead(moveAmount);
            // Turn the gun to turn right 90 degrees.
            peek = true;
            TurnGunRight(90);
            TurnRight(90);
            // Robot only scan for the others if it has already move to near by the wall
            readyToScan = true;
            while (true)
            {
                double turnAngel = Math.Round(Heading % 90) == 0 ? 90 : 90 - (Heading % 90);
              //  double turnAngel = Math.Round(Heading % 90) == 0 ? 90 : Heading % 90;
                // Look before we turn when ahead() completes.
                peek = true;
                // Move up the wall
                TurnRadarRight(360);
                Ahead(moveAmount);
             //   this.Execute();
                // Don't look now
                peek = false;
                // Turn to the next wall
                TurnRight(turnAngel);
                readyToScan = true;
            }
        }

        /// <summary>
        ///   onHitRobot:  Move away a bit.
        /// </summary>
        public override void OnHitRobot(HitRobotEvent e)
        {
            //// If he's in front of us, set back up a bit.
            //if (e.Bearing > -90 && e.Bearing < 90)
            //{
            //    Back(100);
            //} // else he's in back of us, so set ahead a bit.
            //else
            //{
            //    Ahead(100);
            //}
        }
        public override void OnHitByBullet(HitByBulletEvent evnt)
        {
            readyToScan = false;
            
        }
        public override void OnBulletMissed(BulletMissedEvent evnt)
        {
            readyToScan = false;
        }
        /// <summary>
        ///   onScannedRobot:  Fire!
        /// </summary>
        public override void OnScannedRobot(ScannedRobotEvent e)
        {
            if (readyToScan)
            {
                
                // Note that scan is called automatically when the robot is moving.
                // By calling it manually here, we make sure we generate another scan event if there's a robot on the next
                // wall, so that we do not start moving up it until it's gone.
                // Calculate exact location of the robot
                double absoluteBearing = Heading + e.Bearing;
                double targetTrack = Utils.NormalRelativeAngleDegrees(absoluteBearing - GunHeading);
                Fire(Math.Min(3 - Math.Abs(targetTrack), Energy - .1));
                // If it's close enough, fire!
                if (Math.Abs(targetTrack) <= 3)
                {
                    TurnGunRight(targetTrack);
                    // We check gun heat here, because calling Fire()
                    // uses a turn, which could cause us to lose track
                    // of the other robot.
                    if (GunHeat == 0)
                    {
                        Fire(Math.Min(3 - Math.Abs(targetTrack), Energy - .1));
                    }
                }
                else
                {
                    // otherwise just set the gun to turn.
                    // Note:  This will have no effect until we call scan()
                    TurnGunRight(targetTrack);
                }
                // Generates another scan event if we see a robot.
                // We only need to call this if the gun (and therefore radar)
                // are not turning.  Otherwise, scan is called automatically.
                if (targetTrack == 0)
                {
                    Scan();
                }

                
            }
            
        }
    }
}