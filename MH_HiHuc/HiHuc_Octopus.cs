using MH_HiHuc.Strategies;
using MH_HiHuc.Base;
using Robocode;

namespace MH_HiHuc
{
    public class HiHuc_Octopus : HiHucCore
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
            if (IsTeammate(e.Name))
            {
                Stragegy.Clear();
                Stragegy = new Zoombie(this);
                Stragegy.Init();
            }
        }

        public override void OnScannedRobot(ScannedRobotEvent e)
        {
            base.OnScannedRobot(e);
            if (!IsTeammate(e.Name))
            {
                if (e.Distance <= 120 && !(Stragegy is RamBot))
                {
                    Stragegy.Clear();
                    Stragegy = new RamBot(this);
                    Stragegy.Init();
                }
            }
        }
    }
}