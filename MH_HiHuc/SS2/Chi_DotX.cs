using System.Drawing;
using Robocode;
using Robocode.Util;
using System;

namespace RoboCodeTournament
{
    class CT_DotX : AdvancedRobot
    {
        private int scanDirection = 1;
        private int moveDirection = 1;

        #region Custom Methods
        private void setColor()
        {
            BodyColor = (Color.LightGray);
            GunColor = (Color.Gray);
            RadarColor = (Color.DarkGray);
            ScanColor = (Color.Green);
            BulletColor = (Color.OrangeRed);

        }

        private void smartFocus(double robotBearing)
        {
            double targetAngel = Utils.NormalRelativeAngleDegrees(robotBearing + Heading - GunHeading);
            SetTurnGunRight(targetAngel);
        }

        private void smartFire(double robotDistance)
        {
            if (this.Others == 1)
            {
                if (robotDistance <= 200)
                {
                    Fire(1);
                }
            }
            else
            {
                if (robotDistance < 50 && Energy > 50)
                {
                    Fire(3);
                }
                else if (robotDistance < 100 && Energy > 50)
                {
                    Fire(2);
                }
                else if (robotDistance < BattleFieldHeight)
                {
                    Fire(1);
                }
            }
        }

        public void doMove(double robotBearing)
        {
            //if (this.Velocity == 0)
            //    moveDirection *= -1;

            SetTurnRight(robotBearing + 90);
            SetAhead(1000 * moveDirection);
        }
        #endregion

        public override void Run()
        {
            setColor();
            IsAdjustRadarForRobotTurn = true;
            IsAdjustGunForRobotTurn = true;

            while (true)
            {
                MaxVelocity = 5;
                SetTurnRadarRight(360);

                SetAhead(100 * moveDirection);
                Execute();
            }
        }

        public override void OnScannedRobot(ScannedRobotEvent e)
        {
            scanDirection *= -1;
            SetTurnRadarRight(360 * scanDirection);
            smartFocus(e.Bearing);
            smartFire(e.Distance);
            if (this.Others == 1)
            {
                TurnRight(e.Bearing);
                if (e.Distance >= 200)
                {
                    Ahead(e.Distance + 10);
                }
            }
            doMove(e.Bearing);
        }

        public override void OnBulletMissed(BulletMissedEvent evnt)
        {
            Scan();
        }

        public override void OnHitWall(HitWallEvent e)
        {
            moveDirection *= -1;
        }

        public override void OnHitRobot(HitRobotEvent e)
        {
            smartFocus(e.Bearing);
            smartFire(1);
            if (e.IsMyFault)
            {
                moveDirection *= -1;
            }
        }

        public override void OnHitByBullet(HitByBulletEvent e)
        {
            SetTurnRight(Utils.NormalRelativeAngleDegrees(90 - (Heading - e.Heading)));
        }
    }
}
