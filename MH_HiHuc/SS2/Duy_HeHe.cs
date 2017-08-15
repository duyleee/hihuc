using System;
using System.Drawing;
using Robocode;
using Robocode.Util;

namespace RobocodeTournament
{
    public class DV_HeHe: AdvancedRobot
    {
        private AdvancedEnemyBot enemy = new AdvancedEnemyBot();
        private int radarDirection = 1;
        private int moveDirection = 1;
        private int wallMargin = 80;

        public override void Run()
        {
            Init();

            while (true)
            {
                Scan();
                Move();
                Fire();
                Execute();
            }
        }

        void Init()
        {
            SetColors(Color.Red, Color.Orchid, Color.Yellow);
            BulletColor = Color.White;
            ScanColor = Color.Cyan;
            IsAdjustRadarForGunTurn = true;
            IsAdjustGunForRobotTurn = true;
            enemy.Reset();
        }

        public override void OnScannedRobot(ScannedRobotEvent e)
        {
            if (enemy.None() || e.Distance < enemy.GetDistance() - 100 ||
                    e.Name.Equals(enemy.GetName()))
            {
                enemy.Update(e, this);
            }
        }

        public override void OnRobotDeath(RobotDeathEvent e)
        {
            if (e.Name.Equals(enemy.GetName()))
            {
                enemy.Reset();
            }
        }

        public void Scan()
        {
            if (enemy.None())
            {                
                SetTurnRadarRight(360);
            }
            else
            {
                double turn = Heading - RadarHeading + enemy.GetBearing();
                turn += 30 * radarDirection;
                SetTurnRadarRight(Common.NormalizeBearing(turn));
                radarDirection *= -1;

            }
        }

        public Boolean IsCloseToWall()
        {
            return (X <= wallMargin || X >= (BattleFieldWidth - wallMargin) ||
                Y <= wallMargin || Y >= (BattleFieldHeight - wallMargin)) ? true : false;
        }

        public void Move()
        {
            if (IsCloseToWall())
            {
                SetTurnRight(enemy.GetHeading());
                SetAhead(100);
            }
            else
            {
                SetTurnRight(Common.NormalizeBearing(enemy.GetBearing() + 90 - (15 * moveDirection)));

                if (Time % 20 == 0)
                {
                    moveDirection *= -1;
                    SetAhead(150 * moveDirection);
                }
            }
        }

        public void Fire()
        {
            if (enemy.None())
            {
                TurnRadarRight(360);
                return;
            }
            
            double firePower = Math.Min(500 / enemy.GetDistance(), 3);
            double bulletSpeed = 20 - firePower * 3;
            long time = (long)(enemy.GetDistance() / bulletSpeed);
            double absBearing = Common.AbsoluteBearing(X, Y, enemy.GetFutureX(time), enemy.GetFutureY(time));

            SetTurnGunRight(Utils.NormalRelativeAngleDegrees(absBearing - GunHeading));

            if (GunHeat == 0 && Math.Abs(GunTurnRemaining) < 10)
            {
                SetFire(firePower);
            }
        }

        public override void OnHitRobot(HitRobotEvent e)
        {
            SetBack(50);
        }
    }

    public class Common
    {
        public static double ToDegree(double radian)
        {
            return radian * (180 / Math.PI);
        }

        public static double ToRadians(double degree)
        {
            return (Math.PI / 180) * degree;
        }

        public static double AbsoluteBearing(double x1, double y1, double x2, double y2)
        {
            double dx = x2 - x1;
            double dy = y2 - y1;
            double absBearing = ToDegree(Math.Asin(dx / (Math.Sqrt(Math.Pow(Math.Abs(x1 - x2), 2) + Math.Pow(Math.Abs(y1 - y2), 2)))));

            if (dx > 0 && dy > 0)
            {
                return absBearing;
            }
            else if (dx < 0 && dy > 0)
            {
                return (360 + absBearing);
            }
            else if ((dx > 0 && dy < 0) || (dx < 0 && dy < 0))
            {
                return (180 - absBearing);
            }
            else return 0;
        }

        public static double NormalizeBearing(double angle)
        {
            while (angle > 180) angle -= 360;
            while (angle < -180) angle += 360;
            return angle;
        }
    }

    
    public class AdvancedEnemyBot: EnemyBot {	
        private double x, y;

        public AdvancedEnemyBot() {
            this.Reset();
        }

        public double GetX() {
            return x;
        }

        public double GetY() {
            return y;
        }        

        public void Update(ScannedRobotEvent e, Robot robot) {
		    Update(e);
		
            double absBearingDeg = robot.Heading + e.Bearing;

            if (absBearingDeg < 0)
               absBearingDeg += 360;
       
            x = robot.X + Math.Sin(Common.ToRadians(absBearingDeg)) * e.Distance;

            y = robot.Y + Math.Cos(Common.ToRadians(absBearingDeg)) * e.Distance;
        }

        public double GetFutureX(long time) {
            return x + Math.Sin(Common.ToRadians(GetHeading())) * GetVelocity() * time;
        }

        public double GetFutureY(long time) {
            return y + Math.Cos(Common.ToRadians(GetHeading())) * GetVelocity() * time;
        }

        public void Reset() {
            base.Reset();
            x = 0;
            y = 0;
        }
	
	    public void OnHitByBullet(HitByBulletEvent e){	
	
	    }
    }

    public class EnemyBot
    {
        private double bearing, distance, energy, heading, velocity;
        private String name;

        public EnemyBot()
        {
            Reset();
        }

        public double GetBearing()
        {
            return bearing;
        }

        public double GetDistance()
        {
            return distance;
        }

        public double GetEnergy()
        {
            return energy;
        }

        public double GetHeading()
        {
            return heading;
        }

        public double GetVelocity()
        {
            return velocity;
        }

        public String GetName()
        {
            return name;
        }

        public void Update(ScannedRobotEvent sRE)
        {
            bearing = sRE.Bearing;
            distance = sRE.Distance;
            energy = sRE.Energy;
            heading = sRE.Heading;
            velocity = sRE.Velocity;
            name = sRE.Name;
        }

        public void Reset()
        {
            bearing = 0;
            distance = 0;
            energy = 0;
            heading = 0;
            velocity = 0;
            name = "";
        }

        public Boolean None()
        {
            return name.Length == 0;
        }
    }
}
