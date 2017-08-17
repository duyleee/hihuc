using Robocode;

namespace MH_HiHuc.Strategies
{
    public interface IStrategy
    {
        AdvancedRobot MyBot { get; set; }
        void Init();
        void Run();
        void OnScannedRobot(ScannedRobotEvent e);
    }
}
