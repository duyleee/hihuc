using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Robocode;

namespace RoboCodeTournament
{
    public class LH_TrieuTuLong : AdvancedRobot
    {
        private double maxDimension;
        private int intQuarter = 1;
        private double firePower = 2;
        
        public override void Run()
        {
            maxDimension = Math.Max(BattleFieldWidth, BattleFieldHeight);

            // Set colors
            BodyColor = (Color.Pink);
            GunColor = (Color.PeachPuff);
            RadarColor = (Color.Yellow);
            BulletColor = (Color.Red);
            ScanColor = (Color.Cyan);
            firePower = 2;

            double posX = this.X;
            double posY = this.Y;
            
            if (posX <= BattleFieldWidth)
            {
                intQuarter = (posY <= BattleFieldHeight) ? 3 : 4;
            } else {
                intQuarter = (posY <= BattleFieldHeight) ? 2 : 1;
            }

            switch (intQuarter)
            {
                case 1:
                case 4:
                    if (Heading <= 180)
                    {
                        TurnLeft(Heading);
                    }
                    else
                    {
                        TurnRight(360 - Heading);
                    }
                    break;
                case 2:
                case 3:
                    if (Heading <= 180)
                    {
                        TurnRight(180 - Heading);
                    }
                    else
                    {
                        TurnLeft(Heading - 180);
                    }
                    break;
            }
            Ahead(BattleFieldHeight);
            switch (intQuarter)
            {
                case 1:
                case 4:
                    TurnRight(90);
                    TurnGunRight(90);
                    break;
                case 2:
                case 3:
                    TurnLeft(90);
                    TurnGunLeft(90);
                    break;
            }
            Ahead(BattleFieldWidth - this.X);
            Back(BattleFieldWidth);

            while (true)
            {
                Ahead(BattleFieldWidth - 5);
                Back(BattleFieldWidth - 10);
            }
        }

        public override void OnScannedRobot(ScannedRobotEvent e)
        {
            Fire(firePower);
        }

        public override void OnHitRobot(HitRobotEvent e)
        {
            // If he's in front of us, set back up a bit.
            if (e.Bearing > -90 && e.Bearing < 90)
            {
                if (intQuarter == 1 || intQuarter == 4)
                {
                    TurnGunLeft(90);
                }
                if (intQuarter == 2 || intQuarter == 3)
                {
                    TurnGunRight(90);
                }
            } // else he's in back of us, so set ahead a bit.
            else
            {
                if (intQuarter == 1 || intQuarter == 4)
                {
                    TurnGunRight(90);
                }
                if (intQuarter == 2 || intQuarter == 3)
                {
                    TurnGunLeft(90);
                }
            }
            firePower = 3;
        }

        public override void OnHitWall(HitWallEvent e)
        {
            if (intQuarter == 1 || intQuarter == 4)
            {
                if (GunHeading < 180)
                    TurnGunRight(180 - GunHeading);
                else if (GunHeading > 180)
                    TurnGunLeft(GunHeading - 180);
            }
            if (intQuarter == 2 || intQuarter == 3)
            {
                if (GunHeading > 0)
                {
                    if (GunHeading < 180)
                        TurnGunLeft(GunHeading);
                    else if (GunHeading > 180)
                        TurnGunRight(360 - GunHeading);
                }
            }
            firePower = 2;
        }

    }
}
