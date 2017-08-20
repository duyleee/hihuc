using MH_HiHuc.Strategies;
using MH_HiHuc.Base;
using Robocode;
using System;

namespace MH_HiHuc
{
    public class HiHuc_Autobot: HiHucCore
    {
        public override void Run()
        {
            //Start as zoombie to find way to enemy, prevent attach teamate
            Stragegy = new Zoombie(this);
            Stragegy.Init();
            while (true)
            {
                Stragegy.Run();
                Execute();
            }
        }

        public override void OnHitRobot(HitRobotEvent e)
        {
            base.OnHitRobot(e);
            if (!IsTeammate(e.Name))
            {
                Stragegy = new SuperRamFire(this);
                Stragegy.Init();
            }
        }
    }
}
