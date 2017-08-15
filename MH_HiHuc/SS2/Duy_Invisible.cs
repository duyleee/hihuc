using System;
using System.Collections.Generic;
using Robocode;
using System.Drawing;
using System.Collections;
using Robocode.Util;

namespace RoboCodeTournament
{
    /*MAIN BOT IMPLEMENTATION*/
    public enum DL_StrategyType
    {
        //Melee_Wall,
        Solo,
        RamFire,
        SuperWall
    }
    public class DL_Invisible : AdvancedRobot
    {
        private DL_Strategy StrategyController { get; set; }

        private Dictionary<DL_StrategyType, DL_Strategy> StrategyList { get; set; }

        public DL_StrategyType CurrentStragegy { get; set; }

        private DL_Strategy GetStrategy(DL_StrategyType type)
        {
            return StrategyList[type];
        }

        private void Decorate()
        {
            this.BodyColor = Color.Black;
            this.GunColor = Color.Cyan;
            this.RadarColor = Color.DarkCyan;
            this.BulletColor = Color.LightCyan;
            this.ScanColor = Color.DarkCyan;
        }

        private void Initial()
        {
            StrategyList = new Dictionary<DL_StrategyType, DL_Strategy>();
            //Add all current strategy to strategy dictionary
            StrategyList.Add(DL_StrategyType.Solo, new DL_Strategy_Solo(this));//This's used for solo 1:1
            StrategyList.Add(DL_StrategyType.RamFire, new DL_Strategy_Ram(this));
            StrategyList.Add(DL_StrategyType.SuperWall, new DL_Strategy_Wall(this));
        }

        public override void Run()
        {
            /*Decorate my robot :D*/
            Decorate();
            /*Prepare all component for the battle :D*/
            Initial();

            /*Select strategy for first time base on current number of others*/
            this.CurrentStragegy = DL_StrategyType.RamFire;

            /*Inital state for the current strategy*/
            this.GetStrategy(CurrentStragegy).Initial();

            /*MAIN ROBOT FLOW*/
            while (true)
            {
                this.GetStrategy(CurrentStragegy).Run();
            }
            /*MAIN ROBOT FLOW*/
        }

        /// <summary>
        /// Track event bot death to update enemy database
        /// </summary>
        /// <param name="evnt"></param>
        public override void OnRobotDeath(RobotDeathEvent evnt)
        {
            this.GetStrategy(CurrentStragegy).OnRobotDeath(evnt);
            //Change the strategy when number of bot on the battle change
            SwitchStrategy(GetStrategy());
        }

        public void SwitchStrategy(DL_StrategyType type)
        {
            if (CurrentStragegy != type)
            {
                this.GetStrategy(CurrentStragegy).Stop();
                CurrentStragegy = type;

                ///*Trick for solo*/
                //if (this.Energy > 50 && CurrentStragegy == StrategyType.Solo)
                //{
                //    timeToBackMainStrategy = this.Time + 50;
                //    CurrentStragegy = StrategyType.RamFire;
                //}

                this.GetStrategy(CurrentStragegy).Initial();
                ResetMyBotState();
            }
        }

        private void ResetMyBotState()
        {
            this.MaxTurnRate = 10;
            this.MaxVelocity = 8;
        }

        private DL_StrategyType GetStrategy()
        {
            if (this.Others >= 3)
            {
                return DL_StrategyType.SuperWall;
            }

            //if (this.Others >= 2)
            //{
            //    return StrategyType.Melee_Wall;
            //}

            return DL_StrategyType.Solo;
        }

        /// <summary>
        /// Update enemy database
        /// </summary>
        /// <param name="evnt"></param>
        public override void OnScannedRobot(ScannedRobotEvent evnt)
        {
            this.GetStrategy(CurrentStragegy).OnScannedRobot(evnt);
        }


        public override void OnHitRobot(HitRobotEvent e)
        {
            this.GetStrategy(CurrentStragegy).OnHitRobot(e);
        }

        public override void OnBulletHit(BulletHitEvent e)
        {
            this.GetStrategy(CurrentStragegy).OnBulletHit(e);
        }

        public override void OnHitByBullet(HitByBulletEvent e)
        {
            if (this.CurrentStragegy == DL_StrategyType.RamFire && this.Energy < 35)
            {
                this.SwitchStrategy(GetStrategy());
            }
            this.GetStrategy(CurrentStragegy).OnHitByBullet(e);
        }

        public long timeToBackMainStrategy = 120;
        public override void OnStatus(StatusEvent e)
        {
            //Go to ram in 120 tick
            if (e.Time > timeToBackMainStrategy)
            {
                if (CurrentStragegy == DL_StrategyType.RamFire)
                {
                    SwitchStrategy(GetStrategy());
                }
            }
        }
        public override void OnPaint(IGraphics graphics)
        {

        }

        public override void OnWin(WinEvent evnt)
        {
            /*Dancing for win*/
            TurnRight(double.MaxValue);
            TurnGunLeft(double.MaxValue);
            TurnRadarLeft(double.MaxValue);
            /*Dancing for win*/
        }
    }
    /*MAIN BOT IMPLEMENTATION*/

    /*SUPPORT CLASS*/
    public class DL_EnemyWave
    {
        public DL_Point2D fireLocation { get; set; }
        public long fireTime { get; set; }
        public double bulletVelocity { get; set; }
        public double directAngle { get; set; }
        public double distanceTraveled { get; set; }
        public int direction;

        public DL_EnemyWave() { }
    }
    public class DL_Point2D
    {
        public double X { get; set; }
        public double Y { get; set; }

        public double Distance(DL_Point2D destinationPoint)
        {
            return DL_Util.Distance(this.X, this.Y, destinationPoint.X, destinationPoint.Y);
        }

        public DL_Point2D Clone()
        {
            return new DL_Point2D
            {
                X = this.X,
                Y = this.Y
            };
        }
    }

    public class DL_Retangle2D
    {
        public double Height { get; set; }
        public double Width { get; set; }
        public double X { get; set; }
        public double Y { get; set; }

        public bool Contains(DL_Point2D point)
        {
            var rectangle = new System.Drawing.Rectangle
            {
                X = (int)this.X,
                Y = (int)this.Y,
                Height = (int)this.Height,
                Width = (int)this.Width
            };
            return rectangle.Contains(new System.Drawing.Point
            {
                X = (int)point.X,
                Y = (int)point.Y
            });
        }

    }
    public abstract class DL_Strategy
    {
        public DL_Strategy(DL_Invisible mybot)
        {
            this.MyBot = mybot;
        }
        /*Reference to current bot */
        internal DL_Invisible MyBot { get; set; }

        /// <summary>
        /// Need to be called in first time switch to other strategy
        /// </summary>
        public abstract void Initial();

        /// <summary>
        /// Handle run method, this method will be run in loop automatic
        /// </summary>
        public abstract void Run();

        /// <summary>
        /// Handle on scanned bot
        /// </summary>
        /// <param name="e"></param>
        public abstract void OnScannedRobot(ScannedRobotEvent e);

        /// <summary>
        /// Handle event that my bullet hit other
        /// </summary>
        /// <param name="e"></param>
        public abstract void OnBulletHit(BulletHitEvent e);

        public abstract void OnHitRobot(HitRobotEvent e);

        /// <summary>
        /// Handle event was hitted by other bullet
        /// </summary>
        /// <param name="e"></param>
        public abstract void OnHitByBullet(HitByBulletEvent e);

        /// <summary>
        /// Handle on my bot die
        /// </summary>
        /// <param name="e"></param>
        public abstract void OnDeadth(DeathEvent e);

        /// <summary>
        /// Handle on other robot death
        /// </summary>
        /// <param name="e"></param>
        public abstract void OnRobotDeath(RobotDeathEvent e);

        public virtual void Stop()
        {
            this.MyBot.Execute();
            this.MyBot.Stop();
        }
    }
    /*SUPPORT CLASS*/

    /*RAM STRATEGY*/
    public class DL_Strategy_Ram : DL_Strategy
    {
        const double BULLET_POWER = 3;//Our bulletpower.
        const double BULLET_DAMAGE = BULLET_POWER * 4;//Formula for bullet damage.
        const double BULLET_SPEED = 20 - 3 * BULLET_POWER;//Formula for bullet speed.

        //Variables
        static double dir = 1;
        static double oldEnemyHeading;
        static double enemyEnergy;

        public DL_Strategy_Ram(DL_Invisible mybot)
            : base(mybot)
        {

        }

        public override void Initial()
        {
            this.MyBot.IsAdjustGunForRobotTurn = true;
            this.MyBot.IsAdjustRadarForGunTurn = true;
            this.MyBot.TurnRadarRightRadians(Double.PositiveInfinity);
            //this.MyBot.Execute();
        }

        public override void Run()
        {
        }

        public override void OnScannedRobot(Robocode.ScannedRobotEvent e)
        {
            double absBearing = e.BearingRadians + this.MyBot.HeadingRadians;

            //This makes the amount we want to turn be perpendicular to the enemy.
            double turn = absBearing + Math.PI / 2;

            //This formula is used because the 1/e.getDistance() means that as we get closer to the enemy, we will turn to them more sharply. 
            //We want to do this because it reduces our chances of being defeated before we reach the enemy robot.
            turn -= Math.Max(0.5, (1 / e.Distance) * 100) * dir;

            this.MyBot.SetTurnRightRadians(Utils.NormalRelativeAngle(turn - this.MyBot.HeadingRadians));

            //This block of code detects when an opponents energy drops.
            if (enemyEnergy > (enemyEnergy = e.Energy))
            {

                //We use 300/e.getDistance() to decide if we want to change directions.
                //This means that we will be less likely to reverse right as we are about to ram the enemy robot.
                if (DL_Util.Random() > 200 / e.Distance)
                {
                    dir = -dir;
                }
            }

            //This line makes us slow down when we need to turn sharply.
            //this.MyBot.MaxVelocity = 400 / this.MyBot.TurnRemaining;

            this.MyBot.SetAhead(100 * dir);

            //Finding the heading and heading change.
            double enemyHeading = e.HeadingRadians;
            double enemyHeadingChange = enemyHeading - oldEnemyHeading;
            oldEnemyHeading = enemyHeading;

            /*This method of targeting is know as circular targeting; you assume your enemy will
             *keep moving with the same speed and turn rate that he is using at fire time.The 
             *base code comes from the wiki.
            */
            double deltaTime = 0;
            double predictedX = this.MyBot.X + e.Distance * Math.Sin(absBearing);
            double predictedY = this.MyBot.Y + e.Distance * Math.Cos(absBearing);
            while ((++deltaTime) * BULLET_SPEED < DL_Util.Distance(this.MyBot.X, this.MyBot.Y, predictedX, predictedY))
            {

                //Add the movement we think our enemy will make to our enemy's current X and Y
                predictedX += Math.Sin(enemyHeading) * e.Velocity;
                predictedY += Math.Cos(enemyHeading) * e.Velocity;


                //Find our enemy's heading changes.
                enemyHeading += enemyHeadingChange;

                //If our predicted coordinates are outside the walls, put them 18 distance units away from the walls as we know 
                //that that is the closest they can get to the wall (Bots are non-rotating 36*36 squares).
                predictedX = Math.Max(Math.Min(predictedX, this.MyBot.BattleFieldWidth - 18), 18);
                predictedY = Math.Max(Math.Min(predictedY, this.MyBot.BattleFieldHeight - 18), 18);

            }
            //Find the bearing of our predicted coordinates from us.
            double aim = Utils.NormalAbsoluteAngle(Math.Atan2(predictedX - this.MyBot.X, predictedY - this.MyBot.Y));

            //Aim and fire.
            this.MyBot.SetTurnGunRightRadians(Utils.NormalRelativeAngle(aim - this.MyBot.GunHeadingRadians));
            this.MyBot.SetFire(BULLET_POWER);

            this.MyBot.SetTurnRadarRightRadians(Utils.NormalRelativeAngle(absBearing - this.MyBot.RadarHeadingRadians) * 2);
        }

        public override void OnBulletHit(Robocode.BulletHitEvent e)
        {
        }

        public override void OnHitRobot(Robocode.HitRobotEvent e)
        {
        }

        public override void OnHitByBullet(Robocode.HitByBulletEvent e)
        {
        }

        public override void OnDeadth(Robocode.DeathEvent e)
        {
        }

        public override void OnRobotDeath(Robocode.RobotDeathEvent e)
        {
        }
    }
    /*RAM STRATEGY*/

    /*SOLO STRATEGY*/
    class DL_Strategy_Solo : DL_Strategy
    {
        public static int BINS = 47;
        public static double[] _surfStats = new double[BINS];
        public DL_Point2D _myLocation;
        public DL_Point2D _enemyLocation;

        public ArrayList _enemyWaves;
        public ArrayList _surfDirections;
        public ArrayList _surfAbsBearings;

        //Used for keep track the enemy's energy => Fire Detecting
        public static double _oppEnergy = 100;

        //Present for the battle field
        public static DL_Retangle2D _fieldRect = new DL_Retangle2D();

        public DL_Strategy_Solo(DL_Invisible mybot)
            : base(mybot)
        {
        }
        /// <summary>
        /// Initial status for solo strategy
        /// </summary>
        public override void Initial()
        {
            _enemyWaves = new ArrayList();
            _surfDirections = new ArrayList();
            _surfAbsBearings = new ArrayList();

            this.MyBot.IsAdjustGunForRobotTurn = true;
            this.MyBot.IsAdjustRadarForGunTurn = true;
            _fieldRect = new DL_Retangle2D
            {
                X = 0 + 18,
                Y = 0 + 18,
                Height = this.MyBot.BattleFieldHeight - 36,
                Width = this.MyBot.BattleFieldWidth - 36
            };
        }


        public override void Run()
        {
            this.MyBot.TurnRadarRightRadians(double.PositiveInfinity);
        }

        public override void OnScannedRobot(Robocode.ScannedRobotEvent e)
        {
            _myLocation = new DL_Point2D
            {
                X = this.MyBot.X,
                Y = this.MyBot.Y
            };

            double lateralVelocity = this.MyBot.Velocity * Math.Sin(e.BearingRadians);
            double absBearing = e.BearingRadians + this.MyBot.HeadingRadians;

            this.MyBot.SetTurnRadarRightRadians(Utils.NormalRelativeAngle(absBearing - this.MyBot.RadarHeadingRadians) * 2);

            _surfDirections.Add((int)((lateralVelocity >= 0) ? 1 : -1));
            _surfAbsBearings.Add((double)(absBearing + Math.PI));


            double bulletPower = _oppEnergy - e.Energy;

            if (DL_Util.IsEnemyFired(bulletPower) && _surfDirections.Count > 2)
            {
                DL_EnemyWave ew = new DL_EnemyWave();
                ew.fireTime = this.MyBot.Time - 1;
                ew.bulletVelocity = DL_Util.BulletVelocity(bulletPower);
                ew.distanceTraveled = DL_Util.BulletVelocity(bulletPower);
                ew.direction = ((int)_surfDirections[2]);
                ew.directAngle = ((Double)_surfAbsBearings[2]);
                ew.fireLocation = _enemyLocation.Clone(); // last tick

                _enemyWaves.Add(ew);
            }

            _oppEnergy = e.Energy;

            // update after EnemyWave detection, because that needs the previous
            // enemy location as the source of the wave
            _enemyLocation = DL_Util.Project(_myLocation, absBearing, e.Distance);
            UpdateWaves();
            DoSurfing();
            Shoot(e);
        }


        public int LowestEnergyToShot = 20;
        private void Shoot(Robocode.ScannedRobotEvent e)
        {
            if (this.MyBot.Energy <= LowestEnergyToShot && e.Energy != 0) return;
            if (e.Distance < 500 || e.Velocity == 0)
            {
                if (e.Velocity == 0)
                {
                    this.MyBot.TurnGunRightRadians(this.MyBot.HeadingRadians - this.MyBot.GunHeadingRadians + e.BearingRadians);
                    this.MyBot.Fire(Math.Min(600 / e.Distance, 3));
                }
                else
                {
                    Noniterative_Linear_Targeting(e);
                }
            }
        }

        public void Noniterative_Linear_Targeting(Robocode.ScannedRobotEvent e)
        {
            double firePower = Math.Min(125 / e.Distance, 3);
            double absoluteBearing = this.MyBot.HeadingRadians + e.BearingRadians;
            this.MyBot.SetTurnGunRightRadians(Utils.NormalRelativeAngle(absoluteBearing -
                this.MyBot.GunHeadingRadians + (e.Velocity * Math.Sin(e.HeadingRadians -
                absoluteBearing) / 13.0)));
            this.MyBot.SetFire(firePower);
        }

        public override void OnBulletHit(Robocode.BulletHitEvent e)
        {
        }

        public override void OnDeadth(Robocode.DeathEvent e)
        {
            ResetData();
        }

        public override void OnRobotDeath(Robocode.RobotDeathEvent e)
        {
            ResetData();
        }

        private void ResetData()
        {
            //Reset risk statitic array for newest target
            for (var index = 0; index < _surfStats.Length; index++)
            {
                _surfStats[index] = 1;
            }
        }


        public override void OnHitByBullet(HitByBulletEvent evnt)
        {
            OnHitByBulletHandler(evnt);
        }

        ArrayList ShottedByEnemy = new ArrayList();
        public void OnHitByBulletHandler(HitByBulletEvent e)
        {
            ShottedByEnemy.Add(e.Bullet.Name.ToLower());
            // If the _enemyWaves collection is empty, we must have missed the
            // detection of this wave somehow.
            if (!(_enemyWaves.Count <= 0))
            {
                DL_Point2D hitBulletLocation = new DL_Point2D
                {
                    X = e.Bullet.X,
                    Y = e.Bullet.Y
                };
                DL_EnemyWave hitWave = null;

                // look through the EnemyWaves, and find one that could've hit us.
                for (int x = 0; x < _enemyWaves.Count; x++)
                {
                    DL_EnemyWave ew = (DL_EnemyWave)_enemyWaves[x];

                    if (Math.Abs(ew.distanceTraveled -
                        _myLocation.Distance(ew.fireLocation)) < 50
                        && Math.Abs(DL_Util.BulletVelocity(e.Bullet.Power)
                            - ew.bulletVelocity) < 0.001)
                    {
                        hitWave = ew;
                        break;
                    }
                }

                if (hitWave != null)
                {
                    logHit(hitWave, hitBulletLocation);
                    // We can remove this wave now, of course.
                    _enemyWaves.Remove(_enemyWaves.LastIndexOf(hitWave));
                }
            }
            if (this.MyBot.Others > 1)
            {
                this.MyBot.TurnRadarLeft(360);
                this.MyBot.Ahead(100);
            }
            /*Check if being hit by sample bot multiple time reset the risk statistic*/
            if (ShottedByEnemy.Count > 0)
            {
                for (var index = ShottedByEnemy.Count - 1; index >= Math.Max(ShottedByEnemy.Count - 5, ShottedByEnemy.Count - 1); index--) ;
                {
                    ResetData();
                }
            }
        }

        //// CREDIT: mini sized predictor from Apollon, by rozu
        //// http://robowiki.net?Apollon
        public DL_Point2D PredictPosition(DL_EnemyWave surfWave, int direction)
        {
            DL_Point2D predictedPosition = _myLocation.Clone();
            double predictedVelocity = this.MyBot.Velocity;
            double predictedHeading = this.MyBot.HeadingRadians;
            double maxTurning, moveAngle, moveDir;

            int counter = 0; // number of ticks in the future
            bool intercepted = false;

            do
            {
                moveAngle =
                    WallSmoothing(predictedPosition, DL_Util.AbsoluteBearing(surfWave.fireLocation,
                    predictedPosition) + (direction * (Math.PI / 2)), direction)
                    - predictedHeading;
                moveDir = 1;

                if (Math.Cos(moveAngle) < 0)
                {
                    moveAngle += Math.PI;
                    moveDir = -1;
                }

                moveAngle = Utils.NormalRelativeAngle(moveAngle);

                // maxTurning is built in like this, you can't turn more then this in one tick
                maxTurning = Math.PI / 720d * (40d - 3d * Math.Abs(predictedVelocity));
                predictedHeading = Utils.NormalRelativeAngle(predictedHeading
                    + DL_Util.Limit(-maxTurning, moveAngle, maxTurning));

                // this one is nice ;). if predictedVelocity and moveDir have
                // different signs you want to breack down
                // otherwise you want to accelerate (look at the factor "2")
                predictedVelocity += (predictedVelocity * moveDir < 0 ? 2 * moveDir : moveDir);
                predictedVelocity = DL_Util.Limit(-8, predictedVelocity, 8);

                // calculate the new predicted position
                predictedPosition = DL_Util.Project(predictedPosition, predictedHeading, predictedVelocity);

                counter++;

                if (predictedPosition.Distance(surfWave.fireLocation) <
                    surfWave.distanceTraveled + (counter * surfWave.bulletVelocity)
                    + surfWave.bulletVelocity)
                {
                    intercepted = true;
                }

            } while (!intercepted && counter < 500);

            return predictedPosition;
        }

        public double checkDanger(DL_EnemyWave surfWave, int direction)
        {
            int index = getFactorIndex(surfWave,
                PredictPosition(surfWave, direction));

            return _surfStats[index];
        }

        public void DoSurfing()
        {
            DL_EnemyWave surfWave = getClosestSurfableWave();

            if (surfWave == null) { return; }

            double dangerLeft = checkDanger(surfWave, -1);
            double dangerRight = checkDanger(surfWave, 1);

            double goAngle = DL_Util.AbsoluteBearing(surfWave.fireLocation, _myLocation);

            if (dangerLeft < dangerRight)
            {
                goAngle = WallSmoothing(_myLocation, goAngle - (Math.PI / 2), -1);
            }
            else
            {
                goAngle = WallSmoothing(_myLocation, goAngle + (Math.PI / 2), 1);
            }

            SetBackAsFront(this.MyBot, goAngle);
        }

        public double WallSmoothing(DL_Point2D botLocation, double angle, int orientation)
        {
            while (!_fieldRect.Contains(DL_Util.Project(botLocation, angle, 160)))
            {
                angle += orientation * 0.05;
            }
            return angle;
        }

        public static void SetBackAsFront(AdvancedRobot robot, double goAngle)
        {
            double angle =
                Utils.NormalRelativeAngle(goAngle - robot.HeadingRadians);
            if (Math.Abs(angle) > (Math.PI / 2))
            {
                if (angle < 0)
                {
                    robot.SetTurnRightRadians(Math.PI + angle);
                }
                else
                {
                    robot.SetTurnLeftRadians(Math.PI - angle);
                }
                robot.SetBack(100);
            }
            else
            {
                if (angle < 0)
                {
                    robot.SetTurnLeftRadians(-1 * angle);
                }
                else
                {
                    robot.SetTurnRightRadians(angle);
                }
                robot.SetAhead(100);
            }
        }

        public void UpdateWaves()
        {
            var _time = this.MyBot.Time;
            ArrayList DeletedWave = new ArrayList();
            for (int x = 0; x < _enemyWaves.Count; x++)
            {
                DL_EnemyWave ew = (DL_EnemyWave)_enemyWaves[x];

                ew.distanceTraveled = (_time - ew.fireTime) * ew.bulletVelocity;
                if (ew.distanceTraveled >
                    _myLocation.Distance(ew.fireLocation) + 50)
                {
                    _enemyWaves.Remove(_enemyWaves[x]);
                    x--;
                }
            }
        }

        public DL_EnemyWave getClosestSurfableWave()
        {
            double closestDistance = 50000; // I juse use some very big number here
            DL_EnemyWave surfWave = null;

            for (int x = 0; x < _enemyWaves.Count; x++)
            {
                DL_EnemyWave ew = (DL_EnemyWave)_enemyWaves[x];
                double distance = _myLocation.Distance(ew.fireLocation)
                    - ew.distanceTraveled;

                if (distance > ew.bulletVelocity && distance < closestDistance)
                {
                    surfWave = ew;
                    closestDistance = distance;
                }
            }

            return surfWave;
        }

        // Given the EnemyWave that the bullet was on, and the point where we
        // were hit, calculate the index into our stat array for that factor.
        public static int getFactorIndex(DL_EnemyWave ew, DL_Point2D targetLocation)
        {
            double offsetAngle = (DL_Util.AbsoluteBearing(ew.fireLocation, targetLocation)
                - ew.directAngle);
            double factor = Utils.NormalRelativeAngle(offsetAngle)
                / DL_Util.MaxEscapeAngle(ew.bulletVelocity) * ew.direction;

            return (int)DL_Util.Limit(0,
                (factor * ((BINS - 1) / 2)) + ((BINS - 1) / 2),
                BINS - 1);
        }

        // Given the EnemyWave that the bullet was on, and the point where we
        // were hit, update our stat array to reflect the danger in that area.
        public void logHit(DL_EnemyWave ew, DL_Point2D targetLocation)
        {
            int index = getFactorIndex(ew, targetLocation);

            for (int x = 0; x < BINS; x++)
            {
                // for the spot bin that we were hit on, add 1;
                // for the bins next to it, add 1 / 2;
                // the next one, add 1 / 5; and so on...
                _surfStats[x] += 1.0 / (Math.Pow(index - x, 2) + 1);
            }
        }

        private string SoloFiring = string.Empty;
        public override void OnHitRobot(HitRobotEvent e)
        {
            if (this.MyBot.Energy > e.Energy)
            {
                this.MyBot.SwitchStrategy(DL_StrategyType.RamFire);
            }
        }
    }
    /*SOLO STRATEGY*/

    /*MELEE WALL STRATEGY*/
    /// <summary>
    /// http://robowiki.net/wiki/SuperWalls
    /// </summary>
    public class DL_Strategy_Wall : DL_Strategy
    {

        public DL_Strategy_Wall(DL_Invisible mybot)
            : base(mybot)
        {

        }

        private bool peek; // Don't turn if there's a robot there
        private double moveAmount; // How much to move

        public override void Initial()
        {
            // Initialize moveAmount to the maximum possible for this battlefield.
            moveAmount = Math.Max(this.MyBot.BattleFieldWidth, this.MyBot.BattleFieldHeight);
            // Initialize peek to false
            peek = false;
            this.MyBot.IsAdjustGunForRobotTurn = false;
            this.MyBot.IsAdjustRadarForGunTurn = false;
            this.MyBot.IsAdjustRadarForRobotTurn = false;
            // turnLeft to face a wall.
            // getHeading() % 90 means the remainder of
            // getHeading() divided by 90.
            this.MyBot.TurnLeft(this.MyBot.Heading % 90);
            this.MyBot.Ahead(moveAmount);
            // Turn the gun to turn right 90 degrees.
            peek = true;
            this.MyBot.TurnGunRight(this.MyBot.Heading - this.MyBot.GunHeading + 90);
            this.MyBot.TurnRight(90);

        }

        public override void Run()
        {
            // Look before we turn when ahead() completes.
            peek = true;
            // Move up the wall
            this.MyBot.Ahead(moveAmount);
            // Don't look now
            peek = false;
            // Turn to the next wall
            this.MyBot.TurnRight(90);
        }

        public override void OnScannedRobot(Robocode.ScannedRobotEvent e)
        {
            if (this.MyBot.Energy > 40 || e.Velocity == 0)
            {
                if (peek)
                {
                    this.MyBot.Fire(2);
                }
                else
                {
                    this.MyBot.Fire(1);
                }
            }
            // Note that scan is called automatically when the robot is moving.
            // By calling it manually here, we make sure we generate another scan event if there's a robot on the next
            // wall, so that we do not start moving up it until it's gone.
            if (peek)
            {
                this.MyBot.Scan();
            }
        }

        public override void OnBulletHit(Robocode.BulletHitEvent e)
        {
        }

        public override void OnHitRobot(Robocode.HitRobotEvent e)
        {
            // If he's in front of us, set back up a bit.
            if (e.Bearing > -90 && e.Bearing < 90)
            {
                this.MyBot.Back(100);
            } // else he's in back of us, so set ahead a bit.
            else
            {
                this.MyBot.Ahead(100);
            }
        }

        int CountBullet = 0;
        long time = 0;
        public override void OnHitByBullet(Robocode.HitByBulletEvent e)
        {
            if (e.Time - time < 10 || CountBullet < 4 || time == 0)
            {
                time = e.Time;
                CountBullet++;
                return;
            }
            CountBullet = 0; time = e.Time;
            if (this.MyBot.X < 40 || this.MyBot.X > this.MyBot.BattleFieldWidth - 40)
            {
                if (this.MyBot.Heading == 90)
                {
                    this.MyBot.TurnLeft(90);

                }
                else
                {
                    this.MyBot.TurnRight(90);
                }
            }
            if (this.MyBot.Y < 40 || this.MyBot.Y > this.MyBot.BattleFieldHeight - 40)
            {
                if (this.MyBot.Heading == 0)
                {
                    this.MyBot.TurnRight(90);
                }
                else
                {
                    this.MyBot.TurnLeft(90);
                }
            }

            // If he's in front of us, set back up a bit.
            if (e.Bearing > -90 && e.Bearing < 90)
            {
                this.MyBot.Back(100);
            } // else he's in back of us, so set ahead a bit.
            else
            {
                this.MyBot.Ahead(100);
            }
            this.Initial();
        }

        public override void OnDeadth(Robocode.DeathEvent e)
        {
        }

        public override void OnRobotDeath(Robocode.RobotDeathEvent e)
        {
        }
    }
    /*MELEE WALL STRATEGY*/

    /*UTIL*/
    public static class DL_Util
    {
        public static double AbsoluteBearing(double x1, double y1, double x2, double y2)
        {
            double xo = x2 - x1;
            double yo = y2 - y1;
            double hyp = Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
            double arcSin = Robocode.Util.Utils.ToDegrees(Math.Asin(xo / hyp));
            double bearing = 0;

            if (xo > 0 && yo > 0)
            { // both pos: lower-Left
                bearing = arcSin;
            }
            else if (xo < 0 && yo > 0)
            { // x neg, y pos: lower-right
                bearing = 360 + arcSin; // arcsin is negative here, actuall 360 - ang
            }
            else if (xo > 0 && yo < 0)
            { // x pos, y neg: upper-left
                bearing = 180 - arcSin;
            }
            else if (xo < 0 && yo < 0)
            { // both neg: upper-right
                bearing = 180 - arcSin; // arcsin is negative here, actually 180 + ang
            }

            return bearing;
        }

        public static double BulletVelocity(double power)
        {
            return (20D - (3D * power));
        }


        public static double AbsoluteBearing(DL_Point2D source, DL_Point2D target)
        {
            return Math.Atan2(target.X - source.X, target.Y - source.Y);
        }

        public static double Limit(double min, double value, double max)
        {
            return Math.Max(min, Math.Min(value, max));
        }

        public static double NormalizeBearing(double angle)
        {
            while (angle > 180) angle -= 360;
            while (angle < -180) angle += 360;
            return angle;
        }

        public static double ToDegree(double degrees)
        {
            return Math.PI * degrees / 180.0;
        }

        public static double Distance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
        }
        public static double MaxEscapeAngle(double velocity)
        {
            return Math.Asin(8.0 / velocity);
        }

        public static DL_Point2D Project(DL_Point2D sourceLocation, double angle, double length)
        {
            return new DL_Point2D
            {
                X = sourceLocation.X + Math.Sin(angle) * length,
                Y = sourceLocation.Y + Math.Cos(angle) * length
            };
        }

        public static bool IsEnemyFired(double energyChange)
        {
            return energyChange < 3.01 && energyChange > 0.09;
        }

        private static Random randomer = new Random();
        public static double Random()
        {
            return randomer.NextDouble();
        }
    }
    /*UTIL*/
}
