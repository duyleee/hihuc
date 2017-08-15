using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Robocode;

namespace RoboCodeTournament
{
    public class TN_DieHard : Robot
    {
        private double maxDimension;
        
        public override void Run()
        {
            maxDimension = Math.Max(BattleFieldWidth, BattleFieldHeight);

            // Set colors
            BodyColor = (Color.Black);
            GunColor = (Color.PeachPuff);
            RadarColor = (Color.Yellow);
            BulletColor = (Color.Red);
            ScanColor = (Color.Cyan);

            TurnLeft(Heading % 90);
            Ahead(maxDimension);

            TurnGunRight(90);
            TurnRight(90);

            while (true)
            {
                Ahead(maxDimension);
                Back(maxDimension);
				if(Time % 5000 == 0)
				{
					TurnRight(90);
				}
            }
        }
		
        public override void OnScannedRobot(ScannedRobotEvent e)
        {
            Fire(2);
        }

        public override void OnHitRobot(HitRobotEvent e)
        {
            // If he's in front of us, set back up a bit.
            if (e.Bearing > -90 && e.Bearing < 90)
            {
                Stop();
                

            } // else he's in back of us, so set ahead a bit.
            else
            {
                Ahead(100);
            }
        }
    }
}
