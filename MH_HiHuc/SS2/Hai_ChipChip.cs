using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robocode;
using Robocode.Util;
using System.Drawing;

namespace RoboCodeTournament
{
    public class TTTH_ChipChip : AdvancedRobot
    {
        // tracking enermy robot
        int moveDirection = 1, run = 1;
        private EnemyRobot enemy = new EnemyRobot();
        private double walMargin = 150;
        double farY, farX;
        private const int DISTANCE = 200;

        public override void Run()
        {
            SetColors(Color.FromArgb(255, 195, 51), Color.Red, Color.OrangeRed, Color.RoyalBlue, Color.Purple);
            IsAdjustRadarForRobotTurn = true;
            IsAdjustGunForRobotTurn = true;
            enemy.Reset();
            TurnRadarRightRadians(double.PositiveInfinity);
        }

        public override void OnScannedRobot(ScannedRobotEvent evnt)
        {
            if (enemy.None()
                || evnt.Distance < (enemy.Distance - DISTANCE / 2)
                || enemy.Name.Equals(enemy.Name))
            {
                enemy.Update(evnt);
            }

            double absBearing = enemy.BearingRadians + HeadingRadians;
            double laterVel = enemy.Velocity * Math.Sin(enemy.HeadingRadians - absBearing);
            double gunTurnAmount = 1;
            SetTurnRadarLeftRadians(RadarTurnRemainingRadians);

            if (enemy.Distance > DISTANCE )
            {
                BulletColor = Color.Red; // set bullet color

                gunTurnAmount = Utils.NormalRelativeAngle(absBearing - GunHeadingRadians + laterVel / 20);
                SetTurnGunRightRadians(gunTurnAmount);
                SetTurnRightRadians(Utils.NormalRelativeAngle(absBearing - HeadingRadians + laterVel / Velocity));
                SetAhead((enemy.Distance - DISTANCE) * moveDirection);
                SetFire(3);
            }
            else
            {
                BulletColor = Color.Pink; // set bullet color

                gunTurnAmount = Utils.NormalRelativeAngle(absBearing - GunHeadingRadians + laterVel / 10);
                SetTurnGunRightRadians(gunTurnAmount);
                SetTurnRightRadians(Utils.NormalRelativeAngle(absBearing - HeadingRadians + laterVel / Velocity));
                SetTurnLeft(-90 - enemy.Bearing);
                SetAhead((DISTANCE * 1.5 + enemy.Distance) * moveDirection);
                SetFire(3);
            }

        }
        
        public override void OnHitWall(HitWallEvent evnt)
        {
            moveDirection = -moveDirection;
            SetTurnRight(evnt.BearingRadians * moveDirection);
            SetAhead((DISTANCE * 2) * moveDirection);
        }

        public override void OnHitRobot(HitRobotEvent evnt)
        {
            moveDirection = -moveDirection;
            enemy.Reset();
            SetTurnRight(evnt.BearingRadians);
            SetAhead(DISTANCE * 2 * moveDirection);
        }

        public override void OnHitByBullet(HitByBulletEvent evnt)
        {
            moveDirection = -moveDirection;
            enemy.Reset();
            SetTurnRight(evnt.BearingRadians);
            SetAhead((DISTANCE * 2) * moveDirection);
        }
    }  

    public class EnemyRobot
    {
        public double Bearing { get; set; }
        public double BearingRadians { get; set; }
        public double Distance { get; set; }
        public double Energy { get; set; }
        public double Heading { get; set; }
        public double HeadingRadians { get; set; }
        public double Velocity { get; set; }
        public string Name { get; set; }

        /// <summary>
        /// Update enemy parameters based on scanned event
        /// </summary>
        /// <param name="e"></param>
        public void Update(ScannedRobotEvent e)
        {
            Bearing = e.Bearing;
            BearingRadians = e.BearingRadians;
            Distance = e.Distance;
            Energy = e.Energy;
            Heading = e.Heading;
            HeadingRadians = e.HeadingRadians;
            Velocity = e.Velocity;
            Name = e.Name;
        }

        public void Reset()
        {
            Bearing = 0.0;
            BearingRadians = 0.0;
            Distance = 0.0;
            Energy = 0.0;
            Heading = 0.0;
            HeadingRadians = 0.0;
            Velocity = 0.0;
            Name = "";
        }

        public bool None()
        {
            return Name.Equals("");
        }

        public EnemyRobot()
        {
            Reset();
        }
    }
}
