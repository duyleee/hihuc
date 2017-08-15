using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using net.sf.robocode.serialization;
using Robocode;
using Robocode.RobotInterfaces;
using Robocode.RobotInterfaces.Peer;

namespace RoboCodeTournament
{
    public class Enemy
    {
        public string Name { get; set; }
        public double Energy { get; set; }
        public double Heading { get; set; }
        public double Bearing { get; set; }
        public double Distance { get; set; }
        public double Velocity { get; set; }
        public bool IsSentryRobot { get; set; }

    }

    public class SD_PeaceTank : Robot
    {
        public override void Run()
        {
            Initinalize();

            while (true)
            {
                Fighter();
            }
        }

        private void Initinalize()
        {
            IsAdjustRadarForRobotTurn = true;
            IsAdjustRadarForGunTurn = true;
        }

        private void Fighter()
        {
            Moving();
            TurnRadarRight(360);
        }

        private double _scanDirection = 1;
        private int _missCount = 0;
        List<Enemy> _enemies = new List<Enemy>();

        public override void OnScannedRobot(ScannedRobotEvent ev)
        {
            Log("Enemy Located");
            if (_enemies.All(rb => rb.Name != ev.Name))
            {
                _enemies.Add(new Enemy
                {
                    Name = ev.Name,
                    Bearing = ev.Bearing,
                    Distance = ev.Distance,
                    Energy = ev.Energy,
                    Heading = ev.Heading,
                    IsSentryRobot = ev.IsSentryRobot,
                    Velocity = ev.Velocity
                });
            }
            _scanDirection *= -1;

            //TurnRadarRight(NormalizeBearing(360 * _scanDirection));
            var degree = NormalizeBearing(Heading - GunHeading + ev.Bearing);
            var distance = ev.Distance > 500 ? 180 :
                            ev.Distance > 300 ? 120 :
                            ev.Distance > 150 ? 90 : 70;
            var gun = ev.Distance > 500 ? 1 :
                        ev.Distance > 300 ? 2 :
                        ev.Distance > 150 ? 3 : 5;

            // calculate firepower based on distance
            var firePower = Math.Min(350 / ev.Distance, 3);
            // calculate speed of bullet
            var bulletSpeed = 20 - firePower * 3;
            // distance = rate * time, solved for time
            var time = (long)(ev.Distance / bulletSpeed);

            if (GunHeat == 0)
            {
                TurnGunRight(degree - gun);
                Fire(firePower);
                TurnGunRight(gun);
                Fire(firePower);
                TurnGunRight(gun);
                Fire(firePower);
            }
            TurnRadarRight(distance * _scanDirection);
        }

        public override void OnRobotDeath(RobotDeathEvent evnt)
        {
            var robot = _enemies.First(rb => rb.Name == evnt.Name);
            _enemies.Remove(robot);
        }

        public override void OnBulletHit(BulletHitEvent e)
        {
            Log("Victim: " + e.VictimName + "/ " + e.VictimEnergy);
        }

        public override void OnBulletMissed(BulletMissedEvent e)
        {
            Log("OnBulletMissed");
            if (_missCount >= 5)
            {
                Ahead(30);
                _missCount = 0;
            }
            else
            {
                _missCount++;
            }
        }

        public override void OnHitRobot(HitRobotEvent e)
        {
            Log("I hit a robot " + e.Name + "!  My energy: " + Energy + ", his energy: " + e.Energy);
            Ahead(-100);
            var random = new Random();
            TurnRight(random.Next(0, 180));
            Moving();
        }

        public override void OnHitWall(HitWallEvent evnt)
        {
            Log("OnHitWall");
            Ahead(-100);
            var random = new Random();
            TurnRight(random.Next(0, 180));
            Moving();
        }

        public override void OnHitByBullet(HitByBulletEvent evnt)
        {
            Log("OnHitByBullet");
            var random = new Random();
            TurnRight(random.Next(0, 180));
            Moving();
        }


        private void Log(string message)
        {
            Console.WriteLine(DateTime.Now.ToString("hh:mm:ss") + ": " + message);
        }

        private double NormalizeBearing(double angle)
        {
            while (angle > 180) angle -= 360;
            while (angle < -180) angle += 360;
            return angle;
        }

        private void Moving()
        {
            var distanceX = 800 - X;
            var distanceY = 600 - Y;
            if (distanceX < 10)
            {
                Ahead(1);
                if (800 - X < distanceX)
                {
                    TurnRight(180);
                }

                if (distanceY < 10)
                {
                    
                }

                if (600 - Y > 300)
                {
                    TurnLeft(90);
                }
                else
                {
                    TurnRight(90);
                }
                Ahead(30);
            }
            else
            {
                Ahead(30);
            }
        }
    }
}
