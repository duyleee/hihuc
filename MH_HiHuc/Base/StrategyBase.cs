using System;
using MH_HiHuc.Strategies;
using Robocode;

namespace MH_HiHuc.Base
{
    public abstract class StrategyBase : IStrategy
    {
        public virtual HiHucCore MyBot { get; set; }

        public void Clear()
        {
            MyBot.ClearAllEvents();
        }

        public virtual void Init()
        {
        }

        public virtual void OnBulletHit(BulletHitEvent e)
        {
        }

        public virtual void OnEnemyMessage(Enemy e)
        {
        }

        public virtual void OnHitByBullet(HitByBulletEvent e)
        {
        }

        public virtual void OnHitRobot(HitRobotEvent evnt)
        {
        }

        public virtual void OnHitWall(HitWallEvent e)
        {
        }

        public virtual void OnPaint(IGraphics graphics)
        {
        }

        public virtual void OnScannedRobot(ScannedRobotEvent e)
        {
        }

        public virtual void Run()
        {
        }
    }
}
