using System;
using Robocode;
using MH_HiHuc.Base;
using System.Collections.Generic;
using Robocode.Util;
using System.Drawing;

namespace MH_HiHuc.Strategies
{
    public class Zoombie : Meele
    {
        public Zoombie(HiHucCore bot):base(bot)
        {
        }

        public override void Init()
        {
            base.Init();
            MyBot.SetColors(Utilities.GetTeamColor(), Color.White, Color.White);
        }


        double midpointstrength = 0;
        int midpointcount = 0;
        Random randomizer = new Random();

        //Far away from teammate, get close to enemy like zoombie
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
                    var tankForce = new ForcedPoint(tank.X, tank.Y, tank.IsTeamate ? 10000 : -10000); 
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
    }
}
