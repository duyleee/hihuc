using System;
using MH_HiHuc.Base;
using Robocode;
using System.Drawing;
using Robocode.Util;

namespace MH_HiHuc.Strategies
{
    internal class Solo : IStrategy
    {
        public HiHucCore MyBot { get; set; }

        public Solo(HiHucCore bot)
        {
            MyBot = bot;
        }

        public void Init()
        {
            Color col = ColorTranslator.FromHtml("#816ea5");
            MyBot.SetColors(col, Color.Yellow, Color.DarkCyan);

            MyBot.IsAdjustGunForRobotTurn = true;
        }

        public void OnHitByBullet(HitByBulletEvent e)
        {
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

        public void OnHitRobot(HitRobotEvent evnt)
        {
            MyBot.Fire(3);
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

        public void OnPaint(IGraphics graphics)
        {
            
        }

        public void OnScannedRobot(ScannedRobotEvent e)
        {
            if (MyBot.IsTeammate(e.Name))
            {
                return;
            }
            //----------------------------------------------------MOVEMENT-------------------------------------------

            if (MyBot.DistanceRemaining == 0) // make robot move every turn
            {
                //reverse move direction, if previous time move ahead => this time move back and vice versa
                _move = _move * -1;
                //reverse turn direction, if previous time turn right => this time turn left and vice versa
                _turn = _turn * -1;
                //move forward or backward
                MyBot.SetAhead(_randomDistance * _move);
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
            MyBot.SetTurnRightRadians((e.BearingRadians + Math.PI / 2) - (angle * _move) * (IsTooCloseWithEnemy));

            //----------------------------------------------------RADAR-------------------------------------------

            //radar scan 1 vs 1 algorithm - perfect radar lock - http://robowiki.net/wiki/One_on_One_Radar
            double radarAngleToTurn = MyBot.HeadingRadians - MyBot.RadarHeadingRadians + e.BearingRadians;
            MyBot.SetTurnRadarRightRadians(Utils.NormalRelativeAngle(radarAngleToTurn));

            //----------------------------------------------------GUN-------------------------------------------
            //choose bullet power base on distance, if distance to enemy larger than 400 => bullet power = 1
            var bullerPower = 400 / e.Distance;

            //simplest linear targeting algorithm - http://robowiki.net/wiki/Linear_Targeting
            double absoluteBearing = MyBot.HeadingRadians + e.BearingRadians;
            MyBot.SetTurnGunRightRadians(Utils.NormalRelativeAngle(absoluteBearing -
                           MyBot.GunHeadingRadians + (e.Velocity * Math.Sin(e.HeadingRadians -
                           absoluteBearing) / Rules.GetBulletSpeed(bullerPower))));

            MyBot.SetFire(bullerPower);
        }

        public void Run()
        {
            MyBot.TurnRadarRight(360);
        }

        public void OnEnemyMessage(Enemy e)
        {
        }

        public void OnDroidMessage(string enemyName)
        {
        }

        int _turn = 1; //1 is left, -1 is right
        int _move = 1; //1 is ahead, -1 is back
        int _number = 5;
        double _randomDistance = 120;
    }
}
