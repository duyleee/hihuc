using MH_HiHuc.Strategies;
using MH_HiHuc.Strategies.Base;
using Robocode;

namespace MH_HiHuc
{
    public class HiHuc_Autobot: HiHucCore, IDroid
    {
        public override void Run()
        {
            this.Stragegy = new Zoombie(this);
        }
    }
}
