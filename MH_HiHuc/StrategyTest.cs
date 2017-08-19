using MH_HiHuc.Strategies;
using MH_HiHuc.Strategies.Base;
using Robocode;
using System.Drawing;

namespace MH_HiHuc
{
    public class StrategyTest: HiHucCore
    {
        IStrategy Stragegy { get; set; }
        public StrategyTest()
        {
            Stragegy = new Meele(this);
        }

        public override void Run()
        {
            Stragegy.Init();

            while (true)
            {
                Stragegy.Run();
            }
        }

        public override void OnScannedRobot(ScannedRobotEvent evnt)
        {
            base.OnScannedRobot(evnt);
            Stragegy.OnScannedRobot(evnt);
        }

        public override void OnHitRobot(HitRobotEvent e)
        {
            Stragegy.OnHitRobot(e);
        }

        public override void OnHitByBullet(HitByBulletEvent evnt)
        {
            Stragegy.OnHitByBullet(evnt);
        }

        public override void OnPaint(IGraphics graphics)
        {
            Stragegy.OnPaint(graphics);
            base.OnPaint(graphics);
        }

        public override void OnRobotDeath(RobotDeathEvent evnt)
        {
            Stragegy.OnRobotDeath(evnt);
        }
    }
}
