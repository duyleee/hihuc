using MH_HiHuc.Strategies;
using MH_HiHuc.Strategies.Base;
using Robocode;

namespace MH_HiHuc
{
    class HiHuc_MegaTron : HiHucCore, IDroid
    {
        public override void Run()
        {
            this.Stragegy = new Zoombie(this);
        }
    }
}
