using System;
using System.Drawing;
using Robocode;
using System.Collections;
using Robocode.Util;

namespace RoboCodeTournament
{
    public class TranPham_Strongmek007 : AdvancedRobot
    {
        private int _angelDirection = 1;
        private int _backforthDirection = 1;
        private int _angelFactor = 5;
        private double _travelDistance = 200.0;

        // Constants

        static int SEARCH_DEPTH = 30;	// Increasing this slows down game execution - beware!
       
        static int BULLET_SPEED = 11;	// 3 power bullets travel at this speed.
        static int MAX_RANGE = 800;  	// Range where we're guarenteed to get a look-ahead lock
        // 1200 would be another good value as this is the max radar distance.
       
        static int SEARCH_END_BUFFER = SEARCH_DEPTH + MAX_RANGE / BULLET_SPEED;	

        // Globals
        static double[] arcLength = new double[100000];
        static String patternMatcher = "\0\b" + "This space filler for end buffer." +
                                 "The numbers up top assure a 1 length match every time.  This string must be " +
                                 "longer than SEARCH_END_BUFFER";
        static Hashtable htPatternMatcher = new Hashtable();
        public override void Run()
        {

            // Nice (cheap) Green/Blue color
            SetColors(Color.Green, Color.Blue, Color.Red);
                       
            TurnRadarRight(360);
        }

        public override void OnScannedRobot(ScannedRobotEvent e)
        {
            //Future improvement can be applied separate pattern matching base on target, but too slow now, low survival !!!
            patternMatchingFire(0, e, SEARCH_DEPTH, e.BearingRadians + HeadingRadians);
        }

        private void patternMatchingFire(int matchIndex, ScannedRobotEvent e, int searchDepth, double targetBearing)
        {
            int historyIndex = patternMatcher.Length;

            //Currently stand still:
            // - hit virtual wall
            // - end of current movement
            if (DistanceRemaining == 0.0)
            {
                _backforthDirection *= -1; //moving backward
                _angelDirection *= -1; //moving in opposite direction
                SetAhead(_travelDistance * (double)_backforthDirection);
            }

            int distantCoefficient = (e.Distance > 150.0) ? 1 : -1; //too near, move backward
            double coefficientAngel = Math.PI / (double)_angelFactor;
            _angelFactor++;
            if (_angelFactor >= 7)
            {
                _angelFactor = 5;
            }
            SetTurnRightRadians(e.BearingRadians + Math.PI/2 - coefficientAngel * (double)_backforthDirection * (double)distantCoefficient);          

            double firePower = Math.Min(400 / e.Distance, 3);            
            if (GunHeat == 0 && Math.Abs(GunTurnRemaining) < 10)
                SetFire(firePower);
            double bulletSpeed = Rules.GetBulletSpeed(firePower);

            double arcMovement = e.Velocity * Math.Sin(e.HeadingRadians - targetBearing);

            arcLength[historyIndex + 1] = arcLength[historyIndex] + arcMovement;

            // Add ArcMovement to lookup buffer.  Typecast to char so it takes 1 entry.
            patternMatcher += ((char)(arcMovement));

            // Do adjustable buffer pattern match
            do
            {
                matchIndex = patternMatcher.LastIndexOf(
                                patternMatcher.Substring(historyIndex - --searchDepth),
                                historyIndex - SEARCH_END_BUFFER);
            }
            while (matchIndex < 0);

            // Update index to end of search		
            matchIndex += searchDepth;

            // Aim at target (asin() in front of sin would be better, but no room at 3 byte cost.)
            
            SetTurnGunRightRadians(Math.Sin(
                (arcLength[matchIndex + ((int)(e.Distance / bulletSpeed))] - arcLength[matchIndex]) / e.Distance +
                targetBearing - GunHeadingRadians));

            // Lock Radar Infinite 
            SetTurnRadarLeftRadians(RadarTurnRemaining);
                        
            ClearAllEvents();
          
        }

        public override void OnHitByBullet(HitByBulletEvent e)
        {
            _travelDistance += new Random().NextDouble() * 50.0 * (double)_angelDirection;
            if (_travelDistance <= 100.0)
            {
                _travelDistance = 100.0;
            }
            if (_travelDistance >= 400.0)
            {
                _travelDistance = 400.0;
            }
        }
        public override void OnHitRobot(HitRobotEvent evnt)
        {
            _travelDistance += new Random().NextDouble() * 50.0 * (double)_angelDirection;
            if (_travelDistance <= 100.0)
            {
                _travelDistance = 100.0;
            }
            if (_travelDistance >= 400.0)
            {
                _travelDistance = 400.0;
            }
        }
    }
}
