using Robocode;
using Robocode.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboCodeTournament
{
    internal class PointD
    {
        public double X { get; set; }
        public double Y { get; set; }
        public PointD(double x, double y)
        {
            X = x;
            Y = y;
        }
        public double Distance(PointD target)
        {
            return Math.Sqrt(Math.Pow(X - target.X, 2) + Math.Pow(X - target.X, 2));
        }
    }
    internal class Target
    {
        public double Energy { get; set; }
        public String Name { get; set; }
        public double Distance { get; set; }
        public PointD Position { get; set; }
    }
    public class NhanNguyen_Hunter : AdvancedRobot
    {
        string trackName = string.Empty;
        Random randomizer = new Random();
        List<Target> enemies = new List<Target>();
        Target enemyUnderTrack = new Target();
        double radarTurn = double.PositiveInfinity;
        double direction = 1;
        double bulletPower = 1;
        PointD myPosition = new PointD(0, 0);

        public override void Run()
        {
            SetColors(Color.Red, Color.Red, Color.Black);
            IsAdjustGunForRobotTurn = (true); // Keep the gun still when we turn
            IsAdjustRadarForGunTurn = (true); // Keep the radar still when we turn
            IsAdjustRadarForRobotTurn = (true);

            SetTurnRadarRightRadians(radarTurn);

            while (true)
            {
                myPosition.X = X;
                myPosition.Y = Y;
                Execute();
            }
        }
        
        //- scan event ------------------------------------------------------------------------------------------------------------------------------
        public override void OnScannedRobot(ScannedRobotEvent e)
        {
            var lastTarget = enemies.FirstOrDefault(i => i.Name == e.Name);
            if (lastTarget == null)
            {
                enemies.Add(new Target
                {
                    Name = e.Name,
                    Energy = e.Energy,
                    Distance = e.Distance,
                    Position = CalcPoint(new PointD(X, Y), e.Distance, HeadingRadians + e.BearingRadians)
                });
            }
            else
            {
                lastTarget.Energy = e.Energy;
                lastTarget.Distance = e.Distance;
                lastTarget.Position = CalcPoint(new PointD(X, Y), e.Distance, HeadingRadians + e.BearingRadians);
                var minDistance = enemies.Min(i => i.Distance);
                var nearestEnemy = enemies.FirstOrDefault(i => i.Distance == minDistance);
                if (e.Name != nearestEnemy.Name) return;
            }

            enemyUnderTrack = enemies.FirstOrDefault(i => i.Name == e.Name);
            // If our target is too far away, turn and move toward it.
            var gunTurn = Utils.NormalRelativeAngleDegrees(e.Bearing + Heading - GunHeading);

            bulletPower = Math.Min(Math.Min(Energy / 6d, 1300d / e.Distance), e.Energy / 3d);
            double absoluteBearing = e.BearingRadians + HeadingRadians;

            double enemyHeading = e.HeadingRadians, oldEnemyHeading = 0;
            double enemyHeadingChange = enemyHeading - oldEnemyHeading;
            double enemyVelocity = e.Velocity;
            oldEnemyHeading = enemyHeading;

            double deltaTime = 0;
            double battleFieldHeight = BattleFieldHeight,
                    battleFieldWidth = BattleFieldWidth;
            double predictedX = enemyUnderTrack.Position.X, predictedY = enemyUnderTrack.Position.Y;
            while ((++deltaTime) * (20.0 - 3.0 * bulletPower) <
                    myPosition.Distance(new PointD(predictedX, predictedY)) && deltaTime < 50)
            {
                predictedX += Math.Sin(enemyHeading) * enemyVelocity;
                predictedY += Math.Cos(enemyHeading) * enemyVelocity;
                enemyHeading += enemyHeadingChange;
                if (predictedX < 18.0
                    || predictedY < 18.0
                    || predictedX > battleFieldWidth - 18.0
                    || predictedY > battleFieldHeight - 18.0)
                {

                    predictedX = Math.Min(Math.Max(18.0, predictedX),
                        battleFieldWidth - 18.0);
                    predictedY = Math.Min(Math.Max(18.0, predictedY),
                        battleFieldHeight - 18.0);
                    break;
                }
            }
            double theta = Utils.NormalAbsoluteAngle(Math.Atan2(predictedX - X, predictedY - Y));
            SetTurnGunRightRadians(Utils.NormalRelativeAngle(theta - GunHeadingRadians));

            SetFire(bulletPower);
            SetTurnRightRadians(Utils.NormalRelativeAngle(e.BearingRadians + HeadingRadians - Math.PI / 2));
            SetAhead(randomizer.Next(150, 200));
            SetBack(randomizer.Next(150, 200));
            SetTurnRadarLeftRadians(RadarTurnRemainingRadians);
        }

        public override void OnDeath(DeathEvent evnt)
        {
            enemies.Clear();
            base.OnDeath(evnt);
        }

        public override void OnRobotDeath(RobotDeathEvent e)
        {
            var enemy = enemies.FirstOrDefault(i => i.Name == e.Name);
            if (enemy != null)
            {
                enemies.Remove(enemy);
            }
            base.OnRobotDeath(e);
        }

        

        private static PointD CalcPoint(PointD p, double dist, double ang) {
            return new PointD(p.X + dist * Math.Sin(ang), p.Y + dist * Math.Cos(ang));
        }
        
        private static double Distance(PointF from, PointF to)
        {
            return Math.Sqrt(Math.Pow(from.X - to.X, 2) + Math.Pow(from.Y - to.Y, 2));
        }

        private static double CalcAngle(PointF p1, PointF p2)
        {
            return Math.Atan2(p2.X - p1.X, p2.Y - p1.Y);
        }
    }
}