using System;
using System.Collections.Generic;
using System.Text;
using Robocode;
using System.Drawing;
using Robocode.Util;

namespace RoboCodeTournament
{
    public class PNC_ARobot : AdvancedRobot
    {
        int moveDirection = 1;
        bool moving = false;

        const int CELL_SIZE = 20;
        List<PNCCoordinate> map;

        public PNC_ARobot()
            : base()
        {
            map = new List<PNCCoordinate>();
            int numberOfCols = (int)800 / CELL_SIZE - 2;
            int numberOfRows = (int)600 / CELL_SIZE - 2;
            for (int i = 2; i < numberOfCols; ++i)
            {
                for (int j = 2; j < numberOfRows; ++j)
                {
                    map.Add(new PNCCoordinate(i * CELL_SIZE, j * CELL_SIZE));
                }
            }

        }

        /// <summary>
        /// Moves forward and turns.
        /// </summary>
        public override void Run()
        {
            moving = false;
            SetAllColors(Color.Yellow);
            int currentMapIndex = -1;
            while (true)
            {
                moving = true;
                int seed = DateTime.Now.Millisecond;
                Random r = new Random(seed);
                int mapIndex = (int)r.Next(0, map.Count);

                if (currentMapIndex == -1) currentMapIndex = mapIndex;
                if (currentMapIndex != mapIndex)
                {
                    int indexToMove = (currentMapIndex + mapIndex) / 2;
                    Goto(map[indexToMove]);
                }

                Goto(map[mapIndex]);
                currentMapIndex = mapIndex;
            }
        }

        /// <summary>
        ///   onHitRobot:  Move away a bit.
        /// </summary>
        public override void OnHitRobot(HitRobotEvent e)
        {
            moveDirection *= -1;
            Ahead(CELL_SIZE * moveDirection);

            // If he's in front of us, set back up a bit then fire.
            if (e.Bearing > -90 && e.Bearing < 90)
            {
                double gunTurnAmt = Utils.NormalRelativeAngleDegrees(e.Bearing + (Heading - RadarHeading));
                TurnGunRight(gunTurnAmt);
                Fire(3);
            }
        }

        /// <summary>
        ///   onScannedRobot:  Fire!
        /// </summary>
        public override void OnScannedRobot(ScannedRobotEvent e)
        {
            if (!(GunHeat == 0 && Math.Abs(GunTurnRemaining) < 10))
            {
                return;
            }

            // If the enemy is closed enough, fire hard.           
            if (e.Distance < 100)
            {
                Fire(3);
            }
            if (e.Distance < 150)
            {
                Fire(2.5);
            }
            else
            {
                Fire(2);
            }

            if (moving)
            {
                Scan();
            }
        }

        /// <summary>
        /// Turns 90 degrees when being hit by enemy's bullet.
        /// </summary>
        public override void OnHitByBullet(HitByBulletEvent e)
        {
            TurnLeft(Utils.NormalRelativeAngleDegrees(90 - e.Bearing));
        }

        public override void OnHitWall(HitWallEvent evnt)
        {
            base.OnHitWall(evnt);
            moveDirection *= -1;
        }

        /// <summary>
        /// Generates a random boolean value.
        /// </summary>
        private bool RandomBool()
        {
            int seed = DateTime.Now.Millisecond;
            Random r = new Random(seed);
            int n = r.Next(2);
            return n > 0;
        }

        private void Goto(PNCCoordinate coor)
        {
            if (coor == null) return;
            Goto(coor.X, coor.Y);
        }

        private void Goto(double x, double y)
        {
            double myX = X, myY = Y;
            double goAngle = Utils.NormalRelativeAngle(Math.Atan2(x - myX, y - myY) - HeadingRadians);
            TurnRightRadians(Math.Tan(goAngle));
            double distance = Math.Cos(goAngle) * Math.Sqrt((myX - x) * (myX - x) + (myY - y) * (myY - y));
            Ahead(distance);
        }

        #region Utils Class
        private class PNCCoordinate
        {
            public double X { get; set; }
            public double Y { get; set; }
            public PNCCoordinate() { }
            public PNCCoordinate(double x, double y)
            {
                X = x;
                Y = y;
            }
        }
        #endregion
    }
}
