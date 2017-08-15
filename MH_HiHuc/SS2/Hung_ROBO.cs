using Robocode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HungNguyen_Robo
{
    public class HungNguyenKhanh_ROBO : AdvancedRobot
    {
        private bool movingForward;
        public override void Run()
        {
            while (true)
            {
                SetAhead(10000);
                movingForward = true;

                for (int i = 0; i < 5;i++ )
                {
                    MakeATurn();
                }
            }
        }

        /// <summary>
        ///   Fire when we see a robot
        /// </summary>
        public override void OnScannedRobot(ScannedRobotEvent e)
        {
            if (e.Distance < 50 && Energy > 50)
            {
                Fire(3);
            } // otherwise, Fire 1.
            else
            {
                Fire(1);
            }
            // Call scan again, before we turn the gun
            Scan();
        }

        /// <summary>
        ///   We were hit!  Turn perpendicular to the bullet,
        ///   so our seesaw might avoid a future shot.
        /// </summary>
        public override void OnHitByBullet(HitByBulletEvent e)
        {
            TurnLeft(90 - e.Bearing);
        }

        public override void OnHitWall(HitWallEvent evnt)
        {
            reverseDirection();
             
        }
        public void reverseDirection()
        {
            if (movingForward)
            {
                SetBack(20000);
                movingForward = false;
            }
            else
            {
                SetAhead(20000);
                movingForward = true;
            }
        }

        private void MakeATurn()
        {
            SetTurnRight(45);
            TurnGunRight(270);
            WaitFor(new TurnCompleteCondition(this));

            SetTurnLeft(90);
            TurnGunRight(270);
            WaitFor(new TurnCompleteCondition(this));

            SetTurnRight(90);
            TurnGunRight(270);
            WaitFor(new TurnCompleteCondition(this));

            SetTurnLeft(45);
            TurnGunRight(270);
            WaitFor(new TurnCompleteCondition(this));
        }
    }
}
