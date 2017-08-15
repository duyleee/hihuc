using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robocode;

namespace RoboCodeTournament
{
    class DCL_LuckyGuy:Robot
    {
        
        public override void Run()
        {
            
            TurnLeft(Heading - 90);
            TurnGunRight(90);

            while (true)
            {
                Ahead(5000);

                TurnRight(90);
                TurnLeft(45);
            }
        }

        // Robot event handler, when the robot sees another robot
        public override void OnScannedRobot(ScannedRobotEvent e)
        {
            
            if(e.Distance < 100)
                Fire(3);
            else if (e.Distance < 300)
                Fire(2);
            if (e.Distance < 500)
                Fire(1);
        }


        public override void OnHitRobot(HitRobotEvent evnt)
        {
            base.OnHitRobot(evnt);
            Fire(3);
            Fire(3);
            Fire(3);
        }

        public override void OnBulletHit(BulletHitEvent evnt)
        {
            base.OnBulletHit(evnt);
            Fire(2);
        }

        public override void OnHitByBullet(HitByBulletEvent evnt)
        {
            base.OnHitByBullet(evnt);
            double de = evnt.HeadingRadians;
            TurnGunLeft(de);
            Fire(2);
            TurnGunLeft(-de);
            TurnLeft(de);
            Ahead(5000);
        }
    }
}
