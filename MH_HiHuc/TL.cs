using Robocode;
using System;

namespace MH_HiHuc
{
    public class TL : TeamRobot
    {
        
        double _randomDistance = 120;
        private Random randomizer = new Random();
        public override void Run()
        {
            
            var x = 0; var y = 0;
            IsAdjustGunForRobotTurn = true;
            while (true)
            {
                TurnRadarRight(360);
            }
        }

        private int wallMargin = 200;
        private int tooCloseToWall = 0;
        private void InitializeEvents()
        {
            AddCustomEvent("wall_reached", 1, new ConditionTest((condition) =>
            {
                return
                // we're too close to the left wall
                (X <= wallMargin ||
                // or we're too close to the right wall
                X >= BattleFieldWidth - wallMargin ||
                // or we're too close to the bottom wall
                Y <= wallMargin ||
                // or we're too close to the top wall
                Y >= BattleFieldHeight - wallMargin);
            }));
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
            

            int IsTooCloseWithEnemy = e.Distance > 70 ? 1 : -1;
            //SetTurnRight(normalizeBearing(e.Bearing + 90  - (15 * moveDirection * IsTooCloseWithEnemy)));
            SetTurnRight(normalizeBearing(e.Bearing + 90-(15 * moveDirection)));

            if (tooCloseToWall > 0) tooCloseToWall--;

            // normal movement: switch directions if we've stopped
            if (Velocity == 0)
            {
                moveDirection *= -1;
                SetAhead(100 * moveDirection);
            }
            else

            if (DistanceRemaining == 0)
            {
                moveDirection *= -1;
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
            //_randomDistance = (randomizer.Next(100, 200));
        }

        public override void OnCustomEvent(CustomEvent evnt)
        {
            if (evnt.Condition.name == "wall_reached")
            {
                if (tooCloseToWall <= 0)
                {
                    // if we weren't already dealing with the walls, we are now
                    tooCloseToWall += wallMargin;
                    MaxVelocity = 0; // stop!!!
                }
            }
        }
    }
}
