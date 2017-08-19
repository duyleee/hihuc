using MH_HiHuc.Strategies;
using MH_HiHuc.Base;
using Robocode;

namespace MH_HiHuc
{
    public class HiHuc_TL : HiHucCore
    {
        public override void Run()
        {
            if (Others >= 5)
            {
                Stragegy = new Meele(this);
            }
            else
            {
                Stragegy = new Solo(this);
            }

            Stragegy.Init();

            while (true)
            {
                Stragegy.Run();
            }
        }

        public override void OnRobotDeath(RobotDeathEvent evnt)
        {
            base.OnRobotDeath(evnt);

            // TL go first
            if (EnemyCount() <= 4 && !(Stragegy is Solo))
            {
                Stragegy = new Solo(this);
                Stragegy.Init();
            }
        }
    }
}
