using Robocode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MH_HiHuc
{
    public class TL : TeamRobot
    {
        private int wallMargin = 60;
        double _randomDistance = 120;
        public override void Run()
        {
            var x = 0;var  y =0;
            IsAdjustGunForRobotTurn = true;
            while (true)
            {
                TurnRadarRight(360);
            }
        }
        int moveDirection = 1;
        public override void OnScannedRobot(ScannedRobotEvent e)
        {
            DoMoving(e);
            DoFire(e);

        }
        private void DoFire(ScannedRobotEvent e)
        {
            if (e.Distance < 700)
            {
                //----------------------------------------------------GUN-------------------------------------------
                //choose bullet power base on distance, if distance to enemy larger than 400 => bullet power = 1
                var bullerPower = 400 / e.Distance;

                //simplest linear targeting algorithm - http://robowiki.net/wiki/Linear_Targeting
                double absoluteBearing = Heading + e.Bearing;
                SetTurnGunRight(normalizeBearing(absoluteBearing -
                               GunHeading + (e.Velocity * Math.Sin(e.Heading -
                               absoluteBearing) / Rules.GetBulletSpeed(bullerPower))));

                SetFire(bullerPower);
            }
        }
        private void DoMoving(ScannedRobotEvent e)
        {
            // always square off against our enemy
            SetTurnRight(e.Bearing + 90);
            SetTurnRight(normalizeBearing(e.Bearing + 90 - (15 * moveDirection)));

            // strafe by changing direction every 20 ticks
            if (DistanceRemaining == 0)
            {
                moveDirection *= -1;
                _randomDistance = (new Random().Next(100, 200));
                SetAhead(_randomDistance * moveDirection);
            }
        }

        // normalizes a bearing to between +180 and -180
        double normalizeBearing(double angle)
        {
            while (angle > 180) angle -= 360;
            while (angle < -180) angle += 360;
            return angle;
        }

        public override void OnHitByBullet(HitByBulletEvent evnt)
        {
            
        }

        private void goTo(int x, int y)
        {
            double a;
            SetTurnRightRadians(Math.Tan(
                a = Math.Atan2(x -= (int)X, y -= (int)Y)
                      - HeadingRadians));
            SetAhead(Hypot(x, y) * Math.Cos(a));
        }

        private double Hypot(int x, int y)
        {
            return System.Math.Sqrt(x * x + y * y);
        }
    }
}
