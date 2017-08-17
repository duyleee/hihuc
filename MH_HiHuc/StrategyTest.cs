using MH_HiHuc.Strategies;
using Robocode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MH_HiHuc
{
    public class StrategyTest: TeamRobot
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
            Stragegy.OnScannedRobot(evnt);
        }
    }
}
