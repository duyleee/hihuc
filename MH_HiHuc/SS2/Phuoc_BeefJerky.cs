using System;
using Robocode;

namespace RoboCodeTournament
{

    public class THP_BeefJerky : AdvancedRobot
    {
		#region Definition
        const int MODE1 = 7;
        const int MODE2 = 4;
        const int MODE3 = 2;
		const int INFINITIV = 99999;

		public class Rectangle
        {
            double X1, X2, Y1, Y2;
            public Rectangle(double x1, double y1, double x2, double y2)
            {
                X1 = x1;
                Y1 = y1;
                X2 = x2;
                Y2 = y2;
            }

            public bool Contains(double X, double Y)
            {
                if(X > X1 && X < X2 && Y > Y1 && Y < Y2)
                    return true;
                return false;
            }
        }
		
        double turnAngle;
        bool forward;
        double prevEnergy = 100;
        int moveDirection = 1;
        int gunDirection = 1;

        static double wallVariant = 140;
        static double battleFieldWidth = 800;
        static double battleFieldHeight = 600;
        Rectangle _fieldRect = new Rectangle(18, 18, battleFieldWidth - 36, battleFieldHeight - 36);
		
		#endregion
        public override void Run()
        {
            while (true)
            {
                Ahead(100);
                turnAngle = awayFromWall(X, Y, HeadingRadians);
                SetTurnRightRadians(turnAngle);
            }
        }

        public override void OnScannedRobot(ScannedRobotEvent e)
        {
			double energyDrop = prevEnergy - e.Energy;
			prevEnergy = e.Energy;
            SetTurnRight(e.Bearing - (moveDirection * 30) + 90);
            
            if (energyDrop <= 3 && energyDrop > 0)
            {
                moveDirection = -moveDirection;
                SetAhead(moveDirection * (125 + (e.Distance / 4)));
            }

            gunDirection = -gunDirection;
            SetTurnGunRight(INFINITIV * gunDirection);
            Fire(1);
        }

        public override void OnHitRobot(HitRobotEvent e)
        {
			
            if (e.Bearing > -90 && e.Bearing < 90)
            {
                Back(100);
            }
            else
            {
                Ahead(100);
            }
        }

        public override void OnHitWall(HitWallEvent e)
        {			
            if (forward)
            {
                SetBack(100);
            }
            else
            {
                SetAhead(100);
            }
						
            forward = !forward;
        }

        public double awayFromWall(double x, double y, double heading)
        {
            double angle = heading + (4 * Math.PI);

            double testX = x + (Math.Sin(angle) * wallVariant);
            double testY = y + (Math.Cos(angle) * wallVariant);
            double wallDistanceX = Math.Min(x - 18, battleFieldWidth - x - 18);
            double wallDistanceY = Math.Min(y - 18, battleFieldHeight - y - 18);
            double testDistanceX = Math.Min(testX - 18, battleFieldWidth - testX - 18);
            double testDistanceY = Math.Min(testY - 18, battleFieldHeight - testY - 18);

            double adjacent = 0;
            int loopCount = 0;

            while (!_fieldRect.Contains(testX, testY) && loopCount < 20)
            {
                if (testDistanceY < 0 && testDistanceY < testDistanceX)
                {
                    angle = ((int)((angle + (Math.PI / 2)) / Math.PI)) * Math.PI;
                    adjacent = Math.Abs(wallDistanceY);
                }
                else if (testDistanceX < 0 && testDistanceX <= testDistanceY)
                {
                    angle = (((int)(angle / Math.PI)) * Math.PI) + (Math.PI / 2);
                    adjacent = Math.Abs(wallDistanceX);
                }

                angle -= (Math.Abs(Math.Acos(adjacent / wallVariant)) + 0.01);

                testX = x + (Math.Sin(angle) * wallVariant);
                testY = y + (Math.Cos(angle) * wallVariant);
                testDistanceX = Math.Min(testX - 18, battleFieldWidth - testX - 18);
                testDistanceY = Math.Min(testY - 18, battleFieldHeight - testY - 18);
				loopCount++;
            }

            return angle;
        }
       
    }
}
