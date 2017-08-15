using Robocode;
using Robocode.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace RoboCodeTournament
{
    //Strategy: Move random to avoid bullets, track enemies, shooting as much as possible
    public class LuanHua_Quasimodo : AdvancedRobot
    {
        int _turn = 1; //1 is left, -1 is right
        int _move = 1; //1 is ahead, -1 is back
        int _number = 5;
        double _randomDistance = 120;

        public override void Run()
        {
            SetColors(Color.Black, Color.Red, Color.Yellow, Color.White, Color.Blue);
            //whenever robot turn, turn gun also
            IsAdjustGunForRobotTurn = true;

            //sweep radar around
            while (true)
            {
                TurnRadarRight(360);
            }
        }

        public override void OnScannedRobot(ScannedRobotEvent e)
        {
            //----------------------------------------------------MOVEMENT-------------------------------------------

            if (DistanceRemaining == 0) // make robot move every turn
            {
                //reverse move direction, if previous time move ahead => this time move back and vice versa
                _move = _move * -1;
                //reverse turn direction, if previous time turn right => this time turn left and vice versa
                _turn = _turn * -1;
                //move forward or backward
                SetAhead(_randomDistance * _move);
            }

            //e.BearingRadians + Math.PI / 2 => turn tank body always block before enemy
            //then random +- Math.PI/5 -> Math.PI/7 => make random movement, to dodge Linear and circular targeting from enemy                        
            var angle = Math.PI / _number;
            _number++;
            if (_number >= 7)
            {
                _number = 5;
            }
            int IsTooCloseWithEnemy = e.Distance > 50 ? 1 : -1;
            SetTurnRightRadians((e.BearingRadians + Math.PI / 2) - (angle * _move) * (IsTooCloseWithEnemy));

            //----------------------------------------------------RADAR-------------------------------------------

            //radar scan 1 vs 1 algorithm - perfect radar lock - http://robowiki.net/wiki/One_on_One_Radar
            double radarAngleToTurn = HeadingRadians - RadarHeadingRadians + e.BearingRadians;
            SetTurnRadarRightRadians(Utils.NormalRelativeAngle(radarAngleToTurn));

            //----------------------------------------------------GUN-------------------------------------------
            //choose bullet power base on distance, if distance to enemy larger than 400 => bullet power = 1
            var bullerPower = 400 / e.Distance;

            //simplest linear targeting algorithm - http://robowiki.net/wiki/Linear_Targeting
            double absoluteBearing = HeadingRadians + e.BearingRadians;
            SetTurnGunRightRadians(Utils.NormalRelativeAngle(absoluteBearing -
                           GunHeadingRadians + (e.Velocity * Math.Sin(e.HeadingRadians -
                           absoluteBearing) / Rules.GetBulletSpeed(bullerPower))));

            SetFire(bullerPower);
        }

        public override void OnHitByBullet(HitByBulletEvent e)
        {
            //when being hitted, change movement, newMoment = oldMovment +- 10
            _randomDistance += new Random().NextDouble() * 10 * _turn;

            if (_randomDistance <= 50)
            {
                _randomDistance = 50;
            }

            if (_randomDistance >= 400)
            {
                _randomDistance = 400;
            }
        }

        public override void OnHitRobot(HitRobotEvent evnt)
        {
            //when hit another robot, change movement, newMoment = oldMovment +- 10
            Fire(3);
            _randomDistance += new Random().NextDouble() * 10 * _turn;

            if (_randomDistance <= 50)
            {
                _randomDistance = 50;
            }

            if (_randomDistance >= 400)
            {
                _randomDistance = 400;
            }
        }
    }
}
