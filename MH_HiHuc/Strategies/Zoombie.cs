using System;
using MH_HiHuc.Strategies.Base;
using Robocode;
using System.Collections.Generic;
using System.Drawing;

namespace MH_HiHuc.Strategies
{
    internal class Zoombie : IStrategy
    {
        public HiHucCore MyBot { get; set; }

        public Zoombie(HiHucCore bot)
        {
            MyBot = bot;
        }

        Enemy myEnemy;
        public void OnEnemyMessage(Enemy e)
        {
            if ((myEnemy == null || MyBot.Targets[myEnemy.Name].Live == false))
            {
                myEnemy = e;
            }
        }

        public void Init()
        {
            MyBot.IsAdjustGunForRobotTurn = true;
            Color col = ColorTranslator.FromHtml("#816ea5");
            MyBot.SetColors(col, Color.BlueViolet, Color.DarkCyan);
        }

        public void OnHitByBullet(HitByBulletEvent e)
        {

        }

        public void OnHitRobot(HitRobotEvent evnt)
        {

        }

        public void OnPaint(IGraphics graphics)
        {

        }

        public void OnScannedRobot(ScannedRobotEvent e)
        {

        }

        public void Run()
        {
            
        }

        
    }
}
