using Robocode;
using Robocode.Util;
using System;
using System.Drawing;

namespace RoboCodeTournament
{
    public class TMD_Honey : AdvancedRobot
    {
        private TMD7_IStrategy _strategy;

        public override void Run()
        {
            // Fashion: How we will appear =))
            BodyColor = Color.FromArgb(0, 250, 30);
            GunColor = Color.Yellow;
            RadarColor = Color.Red;
            BulletColor = Color.FromArgb(255, 255, 5);

            _strategy = new TMD7_CuckooStrategy(this);
            _strategy.Run();
        }

        public override void OnScannedRobot(ScannedRobotEvent e)
        {
            _strategy.HandleOnScannedRobot(e);
        }

        public override void OnWin(WinEvent e)
        {
            _strategy.HandleOnWin(e);
        }
    }

    public interface TMD7_IStrategy
    {
        AdvancedRobot TheRobot { get; }
        void Run();
        void HandleOnScannedRobot(ScannedRobotEvent e);
        void HandleOnWin(WinEvent e);
    }

    public class TMD7_CuckooStrategy : TMD7_IStrategy
    {
        private AdvancedRobot _robot;
        private int _nearestCorner = 2;
        private int _nearestWall = 1;
        private TMD7_PointD[] _corners = new TMD7_PointD[4];

        public TMD7_CuckooStrategy(AdvancedRobot robot)
        {
            _robot = robot;
        }

        #region Implement IStrategy

        public AdvancedRobot TheRobot
        {
            get { return _robot; }
        }

        public void Run()
        {
            #region Set up

            // Function: How flexible we are
            TheRobot.IsAdjustGunForRobotTurn = true;
            TheRobot.IsAdjustRadarForGunTurn = true;

            // Support Info
            if (_corners == null) _corners = new TMD7_PointD[4];
            _corners[0] = new TMD7_PointD(0, 0);
            _corners[1] = new TMD7_PointD(0, TheRobot.BattleFieldHeight);
            _corners[2] = new TMD7_PointD(TheRobot.BattleFieldWidth, TheRobot.BattleFieldHeight);
            _corners[3] = new TMD7_PointD(TheRobot.BattleFieldWidth, 0);

            #endregion

            // Turn radar infinitely
            TheRobot.SetTurnRadarRightRadians(double.PositiveInfinity);

            GoToTheNearestWall();
            DoLoopMovement();
        }

        public void HandleOnScannedRobot(Robocode.ScannedRobotEvent e)
        {
            // Calculate data
            // for turning gun
            // for shoot decision
            // Then turn gun towards the enemy and Fire

            double absoluteBearing = TheRobot.Heading + e.Bearing;
            double angleToTurnGun = Utils.NormalRelativeAngleDegrees(absoluteBearing - TheRobot.GunHeading);
            TheRobot.TurnGunRight(angleToTurnGun);

            if (TheRobot.GunHeat == 0.0 && TheRobot.GunTurnRemaining < 10.0)
            {
                bool decidedToShoot = TheRobot.Energy > 20.0 || (TheRobot.Time % 6 == 0);
                if (decidedToShoot)
                {
                    double distanceToEnemy = e.Distance;
                    double enemyEnergy = e.Energy;
                    double botEnergy = TheRobot.Energy;

                    double power = Math.Min((botEnergy / 5.0), (600.0 / distanceToEnemy));
                    power = Math.Min(power, (enemyEnergy / 3.5));
                    power = Math.Min(power, 3.0);
                    power = Math.Max(power, 0.2);

                    TheRobot.SetFire(power);
                    TheRobot.Execute();
                }
            }
        }

        public void HandleOnWin(Robocode.WinEvent e)
        {
            TheRobot.BodyColor = Color.White;
            TheRobot.GunColor = Color.White;
            TheRobot.RadarColor = Color.FromArgb(243, 214, 3);

            // Victory Dancing
            for (int i = 0; i < 30; i++)
            {
                TheRobot.TurnRight(50.0);
                TheRobot.TurnLeft(50.0);
            }
        }

        #endregion

        #region Private Methods

        private void GoToTheNearestWall()
        {
            // Mark the start of the initial move
            // Determine the nearest corner to go
            // Determine the wall to reach first
            // Turn to that wall
            // Go to that wall
            // When reach the wall, turn howto the back of the bot toward the corner
            // Determine the 1/4 length for the first wall's distance and the 1/4 length for second wall's distance
            // Determine the length need to go first from the point at the wall to the corner
            // Mark completed inital move

            TMD7_PointD botPosition = new TMD7_PointD(TheRobot.X, TheRobot.Y);
            _nearestCorner = DetermineTheNearestCorner(botPosition);
            _nearestWall = DetermineTheNearestWallOfCorner(botPosition, _nearestCorner);

            bool isLeftWall = (_nearestWall == 2);
            double wallAngleToFace = DetermineTheWallAngleToTurnToFace(_nearestCorner, isLeftWall);
            double angleToTurn = Utils.NormalRelativeAngleDegrees(wallAngleToFace - TheRobot.Heading);
            TheRobot.TurnRight(angleToTurn);

            TMD7_PointD wallPointToFace = DetermineTheWallPointToTurnToFace(_nearestCorner, botPosition, isLeftWall);
            double distanceToTheWall = botPosition.DistanceTo(wallPointToFace);
            distanceToTheWall = CalculateDistanceToAvoidHit(distanceToTheWall);
            TheRobot.Ahead(distanceToTheWall);

            double asideAngle = (isLeftWall ? 90.0 : -90.0);
            TheRobot.TurnRight(asideAngle);
        }

        private void DoLoopMovement()
        {
            double distanceOnLeftWall = DeterminceTheDistanceToGoOnLeftWall(_nearestCorner);
            distanceOnLeftWall = CalculateDistanceToAvoidHit(distanceOnLeftWall);
            double distanceOnRightWall = DetermineTheDistanceToGoOnRightWall(_nearestCorner);
            distanceOnRightWall = CalculateDistanceToAvoidHit(distanceOnRightWall);

            TMD7_PointD botPosition = new TMD7_PointD(TheRobot.X, TheRobot.Y);
            TMD7_PointD wallPointToFace = DetermineTheWallPointToTurnToFace(_nearestCorner, botPosition, (_nearestWall == 2));
            double distanceFromTheWallPointToTheCorner = wallPointToFace.DistanceTo(SelectedCorner(_nearestCorner));
            distanceFromTheWallPointToTheCorner = CalculateDistanceToAvoidHit(distanceFromTheWallPointToTheCorner);

            double distance1 = (_nearestWall == 1) ? distanceOnLeftWall : distanceOnRightWall;
            double distance2 = (_nearestWall == 1) ? distanceOnRightWall : distanceOnLeftWall;
            double maxDistanceToTheCorner = Math.Max(distanceFromTheWallPointToTheCorner, distanceOnRightWall);

            double angle1 = (_nearestWall == 1) ? -90.0 : 90.0;
            double angle2 = (_nearestWall == 1) ? 90.0 : -90.0;

            #region Loop action

            while (true)
            {
                // Go back to the corner
                // When reach the corner, turn to the other wall
                // Go for a distance of 1/4 length on that wall
                // Go back to the corner
                // When reach the corner, turn to the first wall
                // Go for a distance of 1/4 length on that wall
                // Reset max distance to the corner, to avoid hit the wall on next times

                TheRobot.Back(maxDistanceToTheCorner);
                TheRobot.TurnRight(angle1);
                TheRobot.Ahead(distance1);
                TheRobot.Back(distance1);
                TheRobot.TurnRight(angle2);
                TheRobot.Ahead(distance2);
                maxDistanceToTheCorner = distance2;
                TheRobot.Execute();
            }

            #endregion
        }

        private int DetermineTheNearestCorner(TMD7_PointD botPosition)
        {
            double distantToCorner1 = botPosition.DistanceTo(_corners[0]);
            double distantToCorner2 = botPosition.DistanceTo(_corners[1]);
            double distantToCorner3 = botPosition.DistanceTo(_corners[2]);
            double distantToCorner4 = botPosition.DistanceTo(_corners[3]);

            int nearestCorner = 1;
            double nearestDistance = distantToCorner1;

            if (distantToCorner2 < nearestDistance)
            {
                nearestCorner = 2;
                nearestDistance = distantToCorner2;
            }
            if (distantToCorner3 < nearestDistance)
            {
                nearestCorner = 3;
                nearestDistance = distantToCorner3;
            }
            if (distantToCorner4 < nearestDistance)
            {
                nearestCorner = 4;
                nearestDistance = distantToCorner4;
            }

            return nearestCorner;
        }

        private int DetermineTheNearestWallOfCorner(TMD7_PointD botPosition, int corner)
        {
            if (corner < 1 || corner > 4) corner = 2;
            double distanceToRightWall, distanceToLeftWall;
            int nearestWall = 1;

            if (corner == 1)
            {
                distanceToRightWall = botPosition.Y;
                distanceToLeftWall = botPosition.X;
            }
            else if (corner == 2)
            {
                distanceToRightWall = botPosition.X;
                distanceToLeftWall = TheRobot.BattleFieldHeight - botPosition.Y;
            }
            else if (corner == 3)
            {
                distanceToRightWall = TheRobot.BattleFieldHeight - botPosition.Y;
                distanceToLeftWall = TheRobot.BattleFieldWidth - botPosition.X;
            }
            else // corner == 4
            {
                distanceToRightWall = TheRobot.BattleFieldWidth - botPosition.X;
                distanceToLeftWall = botPosition.Y;
            }

            if (distanceToLeftWall < distanceToRightWall)
            {
                nearestWall = 2; // left wall
            }

            return nearestWall;
        }

        private double DetermineTheWallAngleToTurnToFace(int nearestCorner, bool isLeftWall)
        {
            if (nearestCorner < 1 && nearestCorner > 4) nearestCorner = 2;
            int cornerIndex = nearestCorner - 1;

            if (isLeftWall)
            {
                double[] leftWallAngles = new double[4] { -90.0, 0.0, 90.0, 180.0 };
                return leftWallAngles[cornerIndex];
            }
            else
            {
                double[] rightWallAngles = new double[4] { 180.0, -90.0, 0.0, 90.0 };
                return rightWallAngles[cornerIndex];
            }
        }

        private TMD7_PointD DetermineTheWallPointToTurnToFace(int nearestCorner, TMD7_PointD botPosition, bool isLeftWall)
        {
            if (nearestCorner < 1 || nearestCorner > 4) nearestCorner = 2;

            if (nearestCorner == 1)
            {
                if (isLeftWall)
                {
                    return new TMD7_PointD(0, botPosition.Y);
                }
                else
                {
                    return new TMD7_PointD(botPosition.X, 0);
                }
            }
            else if (nearestCorner == 2)
            {
                if (isLeftWall)
                {
                    return new TMD7_PointD(botPosition.X, TheRobot.BattleFieldHeight);
                }
                else
                {
                    return new TMD7_PointD(0, botPosition.Y);
                }
            }
            else if (nearestCorner == 3)
            {
                if (isLeftWall)
                {
                    return new TMD7_PointD(TheRobot.BattleFieldWidth, botPosition.Y);
                }
                else
                {
                    return new TMD7_PointD(botPosition.X, TheRobot.BattleFieldHeight);
                }
            }
            else // nearestCorner == 4
            {
                if (isLeftWall)
                {
                    return new TMD7_PointD(botPosition.X, 0);
                }
                else
                {
                    return new TMD7_PointD(TheRobot.BattleFieldWidth, botPosition.Y);
                }
            }
        }

        private double CalculateDistanceToAvoidHit(double rawDistance)
        {
            return rawDistance - TMD7_Constants.RobotHalfSize - TMD7_Constants.GapForTurn;
        }

        private double DeterminceTheDistanceToGoOnLeftWall(int nearestCorner)
        {
            double distanceBasedOnBattleFieldWidth = TheRobot.BattleFieldWidth * 0.25;
            double distanceBasedOnBattleFieldHeight = TheRobot.BattleFieldHeight * 0.25;

            if (nearestCorner < 1 || nearestCorner > 4) nearestCorner = 2;

            if (nearestCorner == 1)
            {
                return distanceBasedOnBattleFieldHeight;
            }
            else if (nearestCorner == 2)
            {
                return distanceBasedOnBattleFieldWidth;
            }
            else if (nearestCorner == 3)
            {
                return distanceBasedOnBattleFieldHeight;
            }
            else // nearestCorner == 4
            {
                return distanceBasedOnBattleFieldWidth;
            }
        }

        private double DetermineTheDistanceToGoOnRightWall(int nearestCorner)
        {
            double distanceBasedOnBattleFieldWidth = TheRobot.BattleFieldWidth * 0.25;
            double distanceBasedOnBattleFieldHeight = TheRobot.BattleFieldHeight * 0.25;

            if (nearestCorner < 1 || nearestCorner > 4) nearestCorner = 2;

            if (nearestCorner == 1)
            {
                return distanceBasedOnBattleFieldWidth;
            }
            else if (nearestCorner == 2)
            {
                return distanceBasedOnBattleFieldHeight;
            }
            else if (nearestCorner == 3)
            {
                return distanceBasedOnBattleFieldWidth;
            }
            else // nearestCorner == 4
            {
                return distanceBasedOnBattleFieldHeight;
            }
        }

        private TMD7_PointD SelectedCorner(int cornerNumber)
        {
            if (cornerNumber < 1 || cornerNumber > 4) cornerNumber = 2;
            int cornerIndex = cornerNumber - 1;
            return _corners[cornerIndex];
        }

        #endregion
    }

    [Serializable]
    public class TMD7_PointD
    {
        public double X { get; set; }

        public double Y { get; set; }

        public TMD7_PointD() { }

        public TMD7_PointD(double x, double y)
        {
            X = x;
            Y = y;
        }

        public TMD7_PointD(TMD7_PointD point)
        {
            if (point != null)
            {
                X = point.X;
                Y = point.Y;
            }
        }

        public static double DistanceBetween(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
        }

        public static double DistanceBetween(TMD7_PointD p1, TMD7_PointD p2)
        {
            double distance = (p1 == null || p2 == null) ? 0.0 : DistanceBetween(p1.X, p1.Y, p2.X, p2.Y);
            return distance;
        }

        public double DistanceTo(double x2, double y2)
        {
            return DistanceBetween(X, Y, x2, y2);
        }

        public double DistanceTo(TMD7_PointD p2)
        {
            return DistanceBetween(X, Y, p2.X, p2.Y);
        }
    }

    public static class TMD7_Constants
    {
        public const double RobotSize = 36.0;
        public const double RobotHalfSize = 18.0;
        public const double GapForTurn = 8.0;
    }
}
