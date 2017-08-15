#region Copyright (c) 2001-2014 Mathew A. Nelson and Robocode contributors

// Copyright (c) 2001-2014 Mathew A. Nelson and Robocode contributors
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://robocode.sourceforge.net/license/epl-v10.html

#endregion

using System.Drawing;
using Robocode;
using System;

namespace RoboCodeTournament
{
    /// <summary>
    ///   SpinBot - a sample robot by Mathew Nelson, and maintained by Flemming N. Larsen
    ///   <p />
    ///   Moves in a circle, firing hard when an enemy is detected
    /// </summary>
    public class Lewis_AAA : AdvancedRobot
    {
        private bool peek;
        private double moveAmount;
        private bool flip;

        public override void Run()
        {
            // Set colors
            BodyColor = (Color.YellowGreen);
            GunColor = (Color.YellowGreen);
            RadarColor = (Color.YellowGreen);
            ScanColor = (Color.Yellow);


            moveAmount = Math.Max(BattleFieldWidth, BattleFieldHeight);
            peek = false;

            TurnLeft(Heading % 90);
            Ahead(moveAmount);
            peek = true;
            TurnGunRight(90);
            TurnRight(90);

            while (true)
            {
                if (flip)
                {
                    peek = true;
                    Back(moveAmount);
                    peek = false;
                    TurnLeft(90);
                }
                else
                {
                    peek = true;
                    Ahead(moveAmount);
                    peek = false;
                    TurnRight(90);
                }
            }
        }

        public override void OnScannedRobot(ScannedRobotEvent e)
        {
            Fire(2);
            if (peek)
            {
                Scan();
            }
        }

        public override void OnHitByBullet(HitByBulletEvent evnt)
        {
            //flip = !flip;
            base.OnHitByBullet(evnt);
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
    }
}