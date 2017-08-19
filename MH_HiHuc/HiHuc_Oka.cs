using MH_HiHuc.Strategies;
using MH_HiHuc.Strategies.Base;
using Robocode;

namespace MH_HiHuc
{
    public class HiHuc_Oka : HiHucCore
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
            if (EnemyCount() <= 3 && !(Stragegy is Solo))
            {
                // Oka go
                Stragegy = new Solo(this);
                Stragegy.Init();
            }
        }
    }
}
