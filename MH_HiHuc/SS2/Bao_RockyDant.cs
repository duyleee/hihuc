using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using Robocode;
using Robocode.Util;

namespace RoboCodeTournament
{
    public class BaoDang_RockyDant : Robot
    {
        private bool isTurnRequired;
        private double stepsOfMove;

        private int getHitCount;
        private string targetName;

        private int targetTrackingCount;

        private double gunTurnValue;
        private String trackName;

        private double oldEnemyHeading;
        public override void Run()
        {
            // Set colors
            BodyColor = (Color.Red);
            GunColor = (Color.Cyan);
            RadarColor = (Color.GreenYellow);
            BulletColor = (Color.Indigo);
            ScanColor = (Color.PaleTurquoise);

            oldEnemyHeading = 0;

            // Initialize stepsOfMove to the maximum possible for this battlefield.
            stepsOfMove = Math.Max(BattleFieldWidth, BattleFieldHeight);
            // Initialize isTurnRequired to false
            isTurnRequired = false;

            // turnLeft to face a wall.
            // getHeading() % 90 means the remainder of
            // getHeading() divided by 90.
            TurnLeft(Heading % 90);
            Ahead(stepsOfMove);
            // Turn the gun to turn right 90 degrees.
            isTurnRequired = true;
            TurnGunRight(90);
            TurnRight(90);

            while (true)
            {
                if (this.Others < 2)
                {
                    this.PlaySolo();
                }
                else
                {
                    this.RunAround();
                }
            }
        }
        public override void OnHitRobot(HitRobotEvent e)
        {
            // If he's in front of us, set back up a bit.
            if (e.Bearing > -90 && e.Bearing < 90)
            {
                Back(100);
            } // else he's in back of us, so set ahead a bit.
            else
            {
                Ahead(100);
            }
        }
        public override void OnHitByBullet(HitByBulletEvent e)
        {
            if (this.Others < 2)
            {
                if (string.IsNullOrEmpty(this.targetName) || this.targetName != e.Name)
                {
                    this.targetName = e.Name;
                    this.getHitCount = 0;
                }

                if (this.targetName == e.Name)
                {
                    this.getHitCount++;

                    if (this.getHitCount > 3 || this.Energy <= 20)
                    {
                        TurnLeft(Heading % 90);
                        Ahead(stepsOfMove);
                    }
                }
                // Calculate exact location of the robot
                double absoluteBearing = Heading + e.Bearing;
                double bearingFromGun = Utils.NormalRelativeAngleDegrees(absoluteBearing - GunHeading);

                // If it's close enough, SwitchFire!
                if (Math.Abs(bearingFromGun) <= 3)
                {
                    TurnGunRight(bearingFromGun);
                    // We check gun heat here, because calling SwitchFire()
                    // uses a turn, which could cause us to lose track
                    // of the other robot.
                    if (GunHeat == 0)
                    {
                        SwitchFire(Math.Min(3 - Math.Abs(bearingFromGun), Energy - .1));
                    }
                }
                else
                {
                    // otherwise just set the gun to turn.
                    // Note:  This will have no effect until we call scan()
                    TurnGunRight(bearingFromGun);
                }
                // Generates another scan event if we see a robot.
                // We only need to call this if the gun (and therefore radar)
                // are not turning.  Otherwise, scan is called automatically.
                if (bearingFromGun == 0)
                {
                    Scan();
                }
            }
        }
        public override void OnScannedRobot(ScannedRobotEvent e)
        {
            if (this.Others < 2)
            {
                //this.WaveSurfing(e);
                this.CornerKiller(e);
            }
            else
            {
                SwitchFire(2);
                // Note that scan is called automatically when the robot is moving.
                // By calling it manually here, we make sure we generate another scan event if there's a robot on the next
                // wall, so that we do not start moving up it until it's gone.
                if (isTurnRequired)
                {
                    Scan();
                }
            }
        }
        public override void OnWin(WinEvent e)
        {
            for (int i = 0; i < 50; i++)
            {
                TurnRight(30);
                TurnLeft(30);
                this.BodyColor = (i % 2 == 0 ? Color.Red : Color.WhiteSmoke);
            }
        }
        private void RunAround()
        {
            // Look before we turn when ahead() completes.
            isTurnRequired = true;
            // Move up the wall
            Ahead(stepsOfMove);
            // Don't look now
            isTurnRequired = false;
            // Turn to the next wall
            TurnRight(90);
        }
        private void SwitchFire(double robotDistance)
        {
            if (robotDistance > 200 || Energy < 15)
            {
                Fire(1);
            }
            else if (robotDistance > 50)
            {
                Fire(2);
            }
            else
            {
                Fire(3);
            }
        }

        private void CornerKiller(ScannedRobotEvent e)
        {
            // Calculate exact location of the robot
            double absoluteBearing = Heading + e.Bearing;
            double bearingFromGun = Utils.NormalRelativeAngleDegrees(absoluteBearing - GunHeading);

            // If it's close enough, SwitchFire!
            if (Math.Abs(bearingFromGun) <= 3)
            {
                TurnGunRight(bearingFromGun);
                // We check gun heat here, because calling SwitchFire()
                // uses a turn, which could cause us to lose track
                // of the other robot.
                if (GunHeat == 0)
                {
                    SwitchFire(Math.Min(3 - Math.Abs(bearingFromGun), Energy - .1));
                }
            }
            else
            {
                // otherwise just set the gun to turn.
                // Note:  This will have no effect until we call scan()
                TurnGunRight(bearingFromGun);
            }
            // Generates another scan event if we see a robot.
            // We only need to call this if the gun (and therefore radar)
            // are not turning.  Otherwise, scan is called automatically.
            if (bearingFromGun == 0)
            {
                Scan();
            }
        }

        private void WaveSurfing(ScannedRobotEvent e)
        {
            double bulletPower = Math.Min(3.0, this.Energy);
            double myX = this.X;
            double myY = this.Y;
            double absoluteBearing = this.Heading + e.BearingRadians;
            double enemyX = this.X + e.Distance * Math.Sin(absoluteBearing);
            double enemyY = this.Y + e.Distance * Math.Cos(absoluteBearing);
            double enemyHeading = e.HeadingRadians;
            double enemyHeadingChange = enemyHeading - oldEnemyHeading;
            double enemyVelocity = e.Velocity;
            oldEnemyHeading = enemyHeading;

            double deltaTime = 0;
            double battleFieldHeight = this.BattleFieldHeight,
                   battleFieldWidth = this.BattleFieldWidth;
            double predictedX = enemyX, predictedY = enemyY;
            
            while ((++deltaTime) * (20.0 - 3.0 * bulletPower) <
                  this.GetPointDistance(myX, myY, predictedX, predictedY))
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
            double theta = Utils.NormalAbsoluteAngle(Math.Atan2(
                predictedX - this.X, predictedY - this.Y));

            this.TurnRadarRight(Utils.NormalRelativeAngle(
                absoluteBearing - this.RadarHeading));
            this.TurnRadarRight(Utils.NormalRelativeAngle(
                theta - this.RadarHeading));
            this.Fire(3);
        }
        private void PlaySolo()
        {
            // turn the Gun (looks for enemy)
            this.TurnGunRight(gunTurnValue);
            // Keep track of how long we've been looking
            targetTrackingCount++;
            // If we've haven't seen our target for 2 turns, look left
            if (targetTrackingCount > 2)
            {
                gunTurnValue = -10;
            }
            // If we still haven't seen our target for 5 turns, look right
            if (targetTrackingCount > 5)
            {
                gunTurnValue = 10;
            }
            // If we *still* haven't seen our target after 10 turns, find another target
            if (targetTrackingCount > 11)
            {
                trackName = null;
            }
        }

        private double GetPointDistance(double pt1X,double pt1Y,double pt2X,double pt2Y)
        {
            return Math.Sqrt(Math.Pow(pt2X - pt1X, 2) + Math.Pow(pt2Y - pt1Y, 2));
        }


    }
}
