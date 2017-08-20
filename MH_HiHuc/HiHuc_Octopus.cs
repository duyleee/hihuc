using MH_HiHuc.Strategies;
using MH_HiHuc.Base;
using Robocode;
using System;

namespace MH_HiHuc
{
    public class HiHuc_Octopus : HiHucCore, IDroid
    {
        public override void Run()
        {
            Stragegy = new MeeleDroid(this);
            Stragegy.Init();
            while (true)
            {
                Stragegy.Run();
            }
        }
    }
}