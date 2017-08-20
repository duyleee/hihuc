using System;
using Robocode;
using MH_HiHuc.Base;
using System.Collections.Generic;
using Robocode.Util;

namespace MH_HiHuc.Strategies
{
    public class Zoombie : Meele
    {
        public Zoombie(HiHucCore bot):base(bot)
        {
        }

        public override void OnEnemyMessage(Enemy e)
        {
        }

        double midpointstrength = 0;
        int midpointcount = 0;
        Random randomizer = new Random();
        internal override List<ForcedPoint> GetRecentForced()
        {
            List<ForcedPoint> forces = new List<ForcedPoint>();
            #region Tank Forced
            Enemy[] enemies = new Enemy[MyBot.Targets.Values.Count];
            MyBot.Targets.Values.CopyTo(enemies, 0);
            foreach (var tank in enemies)
            {
                if (tank.Live == true && tank.Name != MyBot.Name)
                {
                    var tankForce = new ForcedPoint(tank.X, tank.Y, tank.IsTeamate ? 10000 : -10000); //Far away from teammate, stick to enemy like zoombie
                    forces.Add(tankForce);
                }
            }
            #endregion

            #region Middle-Field Forced
            midpointcount++;
            if (midpointcount > 5)
            {
                midpointcount = 0;
                midpointstrength = (randomizer.NextDouble() * 5000);
            }
            var middleFieldForce = new ForcedPoint(MyBot.BattleFieldWidth / 2, MyBot.BattleFieldHeight / 2, midpointstrength);
            forces.Add(middleFieldForce);
            #endregion

            #region Bullets forced
            if (BulletForces.Count > 0)
            {
                foreach (var bulletForce in BulletForces)
                {
                    forces.Add(bulletForce);
                    bulletForce.AffectTurn--;
                }
                BulletForces = BulletForces.FindAll(bf => bf.AffectTurn > 0);
            }
            #endregion

            #region Wall forced
            var wallForcePower = 4500;
            var rightWallForce = (new ForcedPoint(MyBot.BattleFieldWidth, MyBot.Y, wallForcePower)); forces.Add(rightWallForce);
            var leftWallForce = (new ForcedPoint(0, MyBot.Y, wallForcePower)); forces.Add(leftWallForce);
            var topWallForce = (new ForcedPoint(MyBot.X, MyBot.BattleFieldHeight, wallForcePower)); forces.Add(topWallForce);
            var bottomWallForce = (new ForcedPoint(MyBot.X, 0, wallForcePower)); forces.Add(bottomWallForce);
            #endregion

            return forces;
        }

        public override void OnHitRobot(HitRobotEvent e)
        {
            MyBot.TurnRight(e.Bearing);
            double absoluteBearing = MyBot.HeadingRadians + e.BearingRadians;
            MyBot.TurnGunRightRadians(Utils.NormalRelativeAngle(absoluteBearing - MyBot.GunHeadingRadians));

            if (e.Energy > 16)
            {
                MyBot.Fire(3);
            }
            else if (e.Energy > 10)
            {
                MyBot.Fire(2);
            }
            else if (e.Energy > 4)
            {
                MyBot.Fire(1);
            }
            else if (e.Energy > 2)
            {
                MyBot.Fire(.5);
            }
            else if (e.Energy > .4)
            {
                MyBot.Fire(.1);
            }
            MyBot.Ahead(40); // Ram him again!
        }
    }
}
