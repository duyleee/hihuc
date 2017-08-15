using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Robocode;
using System.Drawing;
using Robocode.Util;

namespace RoboCodeTournament
{
    class HB_PhongTom : RateControlRobot
    {
        private int DiChuyenDienKhung;
        bool isRamFire;
        private int turnDirection = 1;
        int missedcounter;
        public override void Run()
        {
            // Set colors
            BodyColor = (Color.Orange);
            GunColor = (Color.Red);
            RadarColor = (Color.Aqua);
            BulletColor = (Color.GreenYellow);
            ScanColor = (Color.Aqua);

            DiChuyenDienKhung = 0;
            GunRotationRate = (15);
            isRamFire = false;
            missedcounter = 0;
            while (true)
            {
                if (isRamFire)
                {
                    TurnRight(5 * turnDirection);
                }
                else
                {
                    if (DiChuyenDienKhung % 64 == 0)
                    {
                        // set cho xe tang di thang
                        TurnRate = 0;
                        // di chuyen xe tang di ve phia truoc 4 px/turn
                        VelocityRate = 4;
                    }
                    if (DiChuyenDienKhung % 64 == 32)
                    {
                        // di chuyen xe tang di ve phia sau 6 px/turn
                        VelocityRate = -6;
                    }
                    DiChuyenDienKhung++;
                    //thuc thi lenh di chuyen xe
                    Execute();
                }
            }
        }

        public override void OnScannedRobot(ScannedRobotEvent e)
        {
            //kiem tra neu quet doi thu 3 lan lien tuc co nghia la tren san kha nang con 1 den 2 thang la rat cao
            //doi chien thuat qua tim va diet
            if (isRamFire)
            {
                if (e.Bearing >= 0)
                {
                    turnDirection = 1;
                }
                else
                {
                    turnDirection = -1;
                }

                TurnRight(e.Bearing);
                Ahead(e.Distance + 5);
                Fire(1);
                Scan(); // Might want to move ahead again!

            }
            else
            {
                // tinh toan vi tri doi phuong
                //xac dinh vi tri co dinh cua xe doi phuong
                //e.Bearing la phuong huong cua doi phuong
                double absoluteBearing = Heading + e.Bearing;
                double bearingFromGun = Utils.NormalRelativeAngleDegrees(absoluteBearing - GunHeading);
               
                    if (Math.Abs(bearingFromGun) <= 3)
                    {
                        TurnGunRight(bearingFromGun);
                        if (GunHeat == 0)
                        {
                            if (e.Distance < 100)
                            {
                                Fire(Rules.MAX_BULLET_POWER);
                            }
                            else if (e.Distance < 300)
                            {
                                Fire(Math.Min(3 - Math.Abs(bearingFromGun), Energy - .1));
                            }
                            else if (e.Distance < 500)
                            {
                                Fire(2);
                            }
                            else
                            {
                                Fire(1);
                            }
                        }
                    }
                    else
                    {
                        TurnGunRight(bearingFromGun);
                    }
                
            }
        }

        public override void OnHitByBullet(HitByBulletEvent e)
        {
            if (!isRamFire)
            {
                // Neu bi ban thi di chuyen dien khung hon cho no khoi doan
                //quay sang phai theo huong kim dong ho 10 px
                TurnRate = 10;
                VelocityRate = 6;
                Execute();
            }
        }
        public override void OnBulletHit(BulletHitEvent evnt)
        {
            missedcounter = 0;
            if (isRamFire)
            {
                isRamFire = false;
            }
        }
        public override void OnBulletMissed(BulletMissedEvent evnt)
        {
            if (!isRamFire)
            {
                missedcounter++;
                if (missedcounter==10)
                {
                    isRamFire = true;
                }
            }
        }
        public override void OnHitWall(HitWallEvent e)
        {
            if (!isRamFire)
            {
                // ra khoi tuong
                VelocityRate = (-1 * VelocityRate);
            }
        }

        public override void OnWin(WinEvent evnt)
        {
            TurnRight(3600);
        }
        public override void OnHitRobot(HitRobotEvent e)
        {
            // tinh toan vi tri doi phuong
            //xac dinh vi tri co dinh cua xe doi phuong
            //e.Bearing la phuong huong cua doi phuong
            double absoluteBearing = Heading + e.Bearing;
            double bearingFromGun = Utils.NormalRelativeAngleDegrees(absoluteBearing - GunHeading);

            if (Math.Abs(bearingFromGun) <= 3)
            {
                TurnGunRight(bearingFromGun);
                if (GunHeat == 0)
                {
                    Fire(Math.Min(3 - Math.Abs(bearingFromGun), Energy - .1));
                }
            }
            else
            {
                TurnGunRight(bearingFromGun);
            }
            
        }
    }
}
