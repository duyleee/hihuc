using System;
using MH_HiHuc.Base;
using Robocode;
using Robocode.Util;
using System.Drawing;

namespace MH_HiHuc.Strategies
{
    internal class SuperRamFire : StrategyBase, IStrategy
    {
        //These are constants. One advantage of these is that the logic in them (such as 20-3*BULLET_POWER)
        //does not use codespace, making them cheaper than putting the logic in the actual code.

        const double BULLET_POWER = 3;//Our bulletpower.
        const double BULLET_DAMAGE = BULLET_POWER * 4;//Formula for bullet damage.
        const double BULLET_SPEED = 20 - 3 * BULLET_POWER;//Formula for bullet speed.

        //Variables
        static double dir = 1;
        static double oldEnemyHeading;
        static double enemyEnergy;

        public SuperRamFire(HiHucCore bot)
        {
            this.MyBot = bot;
        }

        public override void Run()
        {
            if (!string.IsNullOrEmpty(currentTarget) && MyBot.Targets[currentTarget].Live == false)
            {
                MyBot.SetTurnRadarLeft(2 * Math.PI);
            }
        }

        public override void Init()
        {
            MyBot.IsAdjustGunForRobotTurn = true;
            MyBot.IsAdjustRadarForGunTurn = true;
            MyBot.SetTurnRadarRightRadians(2*Math.PI);
            MyBot.SetColors(Utilities.GetTeamColor(), Color.OrangeRed, Color.OrangeRed);
        }
        
        Random randomizer = new Random();
        string currentTarget;
        public override void OnScannedRobot(ScannedRobotEvent e)
        {
            if (MyBot.IsTeammate(e.Name))
            {
                MyBot.SetTurnRadarRightRadians(2 * Math.PI);
                return;
            }
            currentTarget = e.Name;

            var myBotPosition = MyBot.Position;
            double absBearing = e.BearingRadians + MyBot.HeadingRadians;

            //This makes the amount we want to turn be perpendicular to the enemy.
            double turn = absBearing + Math.PI / 2;

            //This formula is used because the 1/e.Distance means that as we get closer to the enemy, we will turn to them more sharply. 
            //We want to do this because it reduces our chances of being defeated before we reach the enemy robot.
            turn -= Math.Max(0.5, (1 / e.Distance) * 100) * dir;

            MyBot.SetTurnRightRadians(Utils.NormalRelativeAngle(turn - MyBot.HeadingRadians));

            //This block of code detects when an opponents energy drops.
            if (enemyEnergy > (enemyEnergy = e.Energy))
            {
                //We use 300/e.Distance to decide if we want to change directions.
                //This means that we will be less likely to reverse right as we are about to ram the enemy robot.
                if (randomizer.NextDouble() > 200 / e.Distance)
                {
                    dir = -dir;
                }
            }

            //This line makes us slow down when we need to turn sharply.
            MyBot.MaxVelocity = (400 / MyBot.TurnRemaining);

            MyBot.SetAhead(100 * dir);

            //Finding the heading and heading change.
            double enemyHeading = e.HeadingRadians;
            double enemyHeadingChange = enemyHeading - oldEnemyHeading;
            oldEnemyHeading = enemyHeading;

            /*This method of targeting is know as circular targeting; you assume your enemy will
             *keep moving with the same speed and turn rate that he is using at fire time.The 
             *base code comes from the wiki.
            */
            double deltaTime = 0;
            double predictedX = myBotPosition.X + e.Distance * Math.Sin(absBearing);
            double predictedY = myBotPosition.Y + e.Distance * Math.Cos(absBearing);
            while ((++deltaTime) * BULLET_SPEED < myBotPosition.Distance(predictedX, predictedY))
            {

                //Add the movement we think our enemy will make to our enemy's current X and Y
                predictedX += Math.Sin(enemyHeading) * e.Velocity;
                predictedY += Math.Cos(enemyHeading) * e.Velocity;


                //Find our enemy's heading changes.
                enemyHeading += enemyHeadingChange;

                //If our predicted coordinates are outside the walls, put them 18 distance units away from the walls as we know 
                //that that is the closest they can get to the wall (Bots are non-rotating 36*36 squares).
                predictedX = Math.Max(Math.Min(predictedX, MyBot.BattleFieldWidth - 18), 18);
                predictedY = Math.Max(Math.Min(predictedY, MyBot.BattleFieldHeight - 18), 18);

            }
            //Find the bearing of our predicted coordinates from us.
            double aim = Utils.NormalAbsoluteAngle(Math.Atan2(predictedX - myBotPosition.X, predictedY - myBotPosition.Y));

            //Aim and fire.
            MyBot.SetTurnGunRightRadians(Utils.NormalRelativeAngle(aim - MyBot.GunHeadingRadians));
            MyBot.SetFire(BULLET_POWER);

            MyBot.SetTurnRadarRightRadians(Utils.NormalRelativeAngle(absBearing - MyBot.RadarHeadingRadians) * 2);
        }

        public override void OnBulletHit(BulletHitEvent e)
        {
            enemyEnergy -= BULLET_DAMAGE;
        }
        public override void OnHitWall(HitWallEvent e)
        {
            dir = -dir;
        }
    }
}
