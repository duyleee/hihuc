using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MH_HiHuc.Strategies;
using Robocode;

namespace MH_HiHuc
{
    public class PredictiveTargetingTest:TeamRobot
    {
        IStrategy Stragegy { get; set; }
        public PredictiveTargetingTest()
        {
            Stragegy = new PredictiveTargeting(this);
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
