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
            Stragegy = new RamBot(this);
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
            if (!IsTeammate(e.Name) && !(Stragegy is RamBot))
            {
                Stragegy = new RamBot(this);
                Stragegy.Init();
            }
        }        
    }
}