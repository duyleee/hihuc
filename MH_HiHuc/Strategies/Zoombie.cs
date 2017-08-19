using System;
using MH_HiHuc.Strategies.Base;
using Robocode;

namespace MH_HiHuc.Strategies
{
    internal class Zoombie : IStrategy
    {
        public HiHucCore MyBot { get; set; }

        public Zoombie(HiHucCore bot)
        {
            this.MyBot = bot;
        }

        public void OnEnemyMessage(Enemy e)
        {
            MyBot.GotoPoint(e.Position);
        }

        public void Init()
        {
            
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
            throw new NotImplementedException();
        }

        
    }
}
