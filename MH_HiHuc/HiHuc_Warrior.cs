using MH_HiHuc.Strategies;
using MH_HiHuc.Base;
using Robocode;

namespace MH_HiHuc
{
    public class HiHucWarrior : HiHucCore
    {
        public override void Run()
        {
            Stragegy = new Solo(this);
            Stragegy.Init();

            while (true)
            {
                Stragegy.Run();
            }
        }
    }
}
