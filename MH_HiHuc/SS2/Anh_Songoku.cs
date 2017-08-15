// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AB _Songoku.cs" company="">
//
// </copyright>
// <summary>
//   The a b_ songoku.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RoboCodeTournament
{
    #region

    using System;
    using System.Drawing;

    using net.sf.robocode.io;

    using Robocode;
    using Robocode.Util;

    #endregion

    /// <summary>
    /// The a b_ songoku.
    /// </summary>
    public class AB_Songoku : AdvancedRobot
    {
        #region Public Methods and Operators

        /// <summary>
        /// The on scanned robot.
        /// </summary>
        /// <param name="evnt">
        /// The evnt.
        /// </param>
        public override void OnScannedRobot(ScannedRobotEvent evnt)
        {
            double firePower = Rules.MAX_BULLET_POWER;

            if (this.Others == 1)
            {
                firePower = 2;

                ////firePower = new Random().Next(1, 2);
                ////firePower = evnt.Distance > 300 ? 1 : 2;
            }

            // Tell the game that when we take move,
            // we'll also want to turn right... a lot.
            this.SetTurnRight(10000);

            // Limit our speed to 5
            this.MaxVelocity = 5;

            // Start moving (and turning)
            this.SetAhead(10000);

            double absoluteBearing = this.HeadingRadians + evnt.BearingRadians;

            double radarTurn = absoluteBearing - this.RadarHeadingRadians;
            this.SetTurnRadarRightRadians(Utils.NormalRelativeAngle(radarTurn) * 20);

            double gunTurn = absoluteBearing - this.GunHeadingRadians;
            this.SetTurnGunRightRadians(Utils.NormalRelativeAngle(gunTurn) * 2);
            this.SetFire(firePower);

            double bulletSpeed = Rules.GetBulletSpeed(firePower);
            this.SetTurnGunRightRadians(Utils.NormalRelativeAngle(absoluteBearing - this.GunHeadingRadians + (evnt.Velocity * Math.Sin(evnt.HeadingRadians - absoluteBearing) / bulletSpeed)));

            this.SetFire(firePower);
        }

        /// <summary>
        ///     The run.
        /// </summary>
        public override void Run()
        {
            this.BodyColor = Color.Green;
            this.GunColor = Color.Red;
            this.RadarColor = Color.Green;
            this.BulletColor = Color.Salmon;
            this.ScanColor = Color.Red;

            this.IsAdjustGunForRobotTurn = true;
            ////this.IsAdjustRadarForRobotTurn = true;

            this.MoveToSafePlace();

            while (true)
            {
                this.TurnRadarRight(360);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// * Gets the angle to a point on the battlefield.
        ///     * @param x X coordinate of the point to move to.
        ///     * @param y Y coordinate of the point to move to.
        ///     *
        ///     * @return The angle between the point and the Y axis in degrees.
        /// </summary>
        /// <param name="x">
        /// The x.
        /// </param>
        /// <param name="y">
        /// The y.
        /// </param>
        /// <returns>
        /// The <see cref="double"/>.
        /// </returns>
        private double GetAngleToPoint(double x, double y)
        {
            // Get heading to the center.
            double distX = x - this.X;
            double distY = y - this.Y;

            double angle = Math.Atan(distX / distY) * (180.0 / Math.PI);

            // Check if the robot needs to move south instead of north.
            if (distY < 0)
            {
                angle += 180.0;
            }

            return angle;
        }

        /// <summary>
        /// The move to point.
        /// </summary>
        /// <param name="x">
        /// The x.
        /// </param>
        /// <param name="y">
        /// The y.
        /// </param>
        private void MoveToPoint(double x, double y)
        {
            bool isHeading = this.TurnToPoint(x, y);

            double offsetX = Math.Abs(this.X - x);
            double offsetY = Math.Abs(this.Y - y);
            double moveOffset = Math.Sqrt(offsetX * offsetX + offsetY * offsetY);

            if (isHeading)
            {
                this.Ahead(moveOffset);
            }
            else
            {
                this.Back(moveOffset);
            }
        }

        /// <summary>
        /// The move to safe place.
        /// </summary>
        private void MoveToSafePlace()
        {
            int padding = 150;

            if (this.X < padding)
            ////if (this.X < this.BattleFieldWidth / 2)
            {
                if (this.Y < this.BattleFieldHeight / 2)
                {
                    this.MoveToPoint(padding, padding);
                }
                else
                {
                    this.MoveToPoint(padding, this.BattleFieldHeight - padding);
                }
            }
            else
            {
                if (this.X > this.BattleFieldWidth - padding)
                {
                    if (this.Y < this.BattleFieldHeight / 2)
                    {
                        this.MoveToPoint(this.BattleFieldWidth - padding, padding);
                    }
                    else
                    {
                        this.MoveToPoint(this.BattleFieldWidth - padding, this.BattleFieldHeight - padding);
                    }
                }
            }

            if (this.Y < padding)
            ////if (this.Y < this.BattleFieldHeight / 2)
            {
                if (this.X < this.BattleFieldWidth / 2)
                {
                    this.MoveToPoint(padding, padding);
                }
                else
                {
                    this.MoveToPoint(this.BattleFieldWidth - padding, padding);
                }
            }
            else
            {
                if (this.Y > this.BattleFieldHeight - padding)
                {
                    if (this.X < this.BattleFieldWidth / 2)
                    {
                        this.MoveToPoint(padding, this.BattleFieldHeight - padding);
                    }
                    else
                    {
                        this.MoveToPoint(this.BattleFieldWidth - padding, this.BattleFieldHeight - padding);
                    }
                }
            }
        }

        /// <summary>
        /// * Turns the robot to set its heading to point (x, y).
        ///     * @param x X coordinate of the point.
        ///     * @param y Y coordinate of the point.
        /// </summary>
        /// <param name="x">
        /// The x.
        /// </param>
        /// <param name="y">
        /// The y.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool TurnToPoint(double x, double y)
        {
            double turnAngle = this.GetAngleToPoint(x, y) + (360 - this.Heading);
            bool isHeading = true;

            if (turnAngle > 180)
            {
                turnAngle = turnAngle - 180;
                isHeading = false;
            }

            this.TurnRight(turnAngle);
            return isHeading;
        }

        #endregion
    }
}