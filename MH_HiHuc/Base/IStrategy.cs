using MH_HiHuc.Base;
using Robocode;

namespace MH_HiHuc.Strategies
{
    public interface IStrategy
    {
        HiHucCore MyBot { get; set; }
        void Init();
        void Clear();
        void Run();
        void OnScannedRobot(ScannedRobotEvent e);
        void OnEnemyMessage(Enemy e);
        void OnHitByBullet(HitByBulletEvent e);
        void OnPaint(IGraphics graphics);        
        void OnHitRobot(HitRobotEvent evnt);
        void OnBulletHit(BulletHitEvent e);
        void OnHitWall(HitWallEvent e);
    }
}
