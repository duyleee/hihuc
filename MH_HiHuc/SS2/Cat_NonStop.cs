using Robocode;
using Robocode.RobotInterfaces;
using Robocode.Util;
using Robocode.Exception;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FNL
{
    public class NonStop : AdvancedRobot
    {
        private EnemyMap _enemies;

        private int _radarDirection = 1;

        // The main method of your robot containing robot logics
        public override void Run()
        {
            AddCustomEvent(new RadarTurnCompleteCondition(this));
            IsAdjustRadarForGunTurn = true;
            SetTurnRadarRight(360);
            _enemies = new EnemyMap();

            TurnLeft(Heading - 90);
            TurnGunRight(90);

            while (true)
            {
                Ahead(5000);
                TurnRight(90);
            }
        }

        public override void OnCustomEvent(CustomEvent evnt)
        {
            if (evnt.Condition is RadarTurnCompleteCondition)
            {
                Sweep();
            }
        }

        public void Sweep()
        {
            double maxBearingAbs = 0, maxBearing = 0;
            int scannedBots = 0;
            foreach (var enemy in _enemies.Enemies)
            {
                if (enemy != null && enemy.IsUpdated())
                {
                    double bearing = Utils.NormalRelativeAngle(Heading + enemy.GetBearing() - RadarHeading);

                    if (Math.Abs(bearing) > maxBearingAbs)
                    {
                        maxBearingAbs = Math.Abs(bearing);
                        maxBearing = bearing;
                    }
                }
                scannedBots++;
            }

            double radarTurn = 180 * _radarDirection;
            if (scannedBots == Others)
                radarTurn = maxBearing + Math.Sign(maxBearing) * 22.5;

            SetTurnRadarRight(radarTurn);
            _radarDirection = Math.Sign(radarTurn);
        }

        public override void OnScannedRobot(ScannedRobotEvent evnt)
        {
            _enemies.Enemies.Add(new Enemy(evnt));
            Fire(1);
        }
    }

    public class Enemy
    {
        private ScannedRobotEvent _evnt;

        public Enemy(ScannedRobotEvent evnt)
        {
            this._evnt = evnt;
        }

        public bool IsUpdated()
        {
            throw new NotImplementedException();
        }

        public double GetHeading()
        {
            return _evnt.Heading;
        }

        public double GetBearing()
        {
            return _evnt.Bearing;
        }

        public double GetRadarHeading()
        {
            throw new NotImplementedException();
        }
    }

    public class EnemyMap
    {
        public List<Enemy> Enemies { get { return new List<Enemy>(); } }
    }
}
