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
    public class NghiemHD_WallHunter : AdvancedRobot
    {
        private bool peek; 
        private double moveAmount; 

        public override void Run()
        {
            moveAmount = Math.Max(BattleFieldWidth, BattleFieldHeight);
            peek = false;

            TurnLeft(Heading % 90);
            Ahead(moveAmount);
            peek = true;
            TurnGunLeft(90);
            TurnLeft(90);

            while (true)
            {
                if(Math.Round(Heading) == Math.Round(GunHeading))
                {
                    TurnGunLeft(90);
                }

                // Look before we turn when ahead() completes.
                peek = true;
                // Move up the wall
                Ahead(moveAmount);
                // Don't look now
                peek = false;
                // Turn to the next wall
                TurnLeft(90);
            }
        }

        /// <summary>
        ///   onHitRobot:  Move away a bit.
        /// </summary>
        public override void OnHitRobot(HitRobotEvent e)
        {
            // If he's in front of us, set back up a bit.
            if (e.Bearing > -90 && e.Bearing < 90)
            {
                //Back(100);
                TurnGunRight(Heading - GunHeading + e.Bearing);
            } // else he's in back of us, so set ahead a bit.
            else
            {
                Ahead(100);
                TurnGunRight(Heading - GunHeading + e.Bearing);
            }
        }

        /// <summary>
        ///   onScannedRobot:  Fire!
        /// </summary>
        public override void OnScannedRobot(ScannedRobotEvent e)
        {
            Fire(3);
            // Note that scan is called automatically when the robot is moving.
            // By calling it manually here, we make sure we generate another scan event if there's a robot on the next
            // wall, so that we do not start moving up it until it's gone.
            if (peek)
            {
                Scan();                
            }
        }

               
    }
}
