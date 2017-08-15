using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Robocode;
using Robocode.Util;
using System.Drawing;
namespace RoboCodeTournament
{
    [Serializable]
    public class PN_EnemyRobot
    {
        public List<PN_Location> Locations { get; set; }
    }

    [Serializable]
    public class PN_PointD
    {
        public PN_PointD(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double X { get; set; }

        public double Y { get; set; }

        public override bool Equals(object obj)
        {
            PN_PointD temp = (obj as PN_PointD);
            return this.X == temp.X && this.Y == temp.Y;
        }

        public override string ToString()
        {
            return string.Format("({0},{1})", this.X, this.Y);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
    [Serializable]
    public class PN_Location
    {
        public PN_PointD Point { get; set; }
        public DateTime DateTime { get; set; }
        public double BearingRadians { get; set; }
        public double HeadingRadians { get; set; }
        public double Distance { get; set; }
        public double Bearing { get; set; }
        public double Velocity { get; set; }
    }

    public class PN_QWERTY : AdvancedRobot
    {
        private int movingForward = 1;
        ///// <summary>
        ///// string : enemyName
        ///// </summary>
        private Dictionary<string, PN_EnemyRobot> _dictEnemyDatas = new Dictionary<string, PN_EnemyRobot>();
        public double POWER
        {
            get
            {
                double _power = (Energy * 3) / 100;
                Console.WriteLine(_power);
                return _power;
            }
        }
        /// <summary>
        ///   run: Crazy's main run function
        /// </summary>
        public override void Run()
        {
            BodyColor = (Color.FromArgb(0, 200, 0));
            GunColor = (Color.FromArgb(0, 150, 50));
            RadarColor = (Color.FromArgb(0, 100, 100));
            BulletColor = (Color.FromArgb(255, 255, 100));
            ScanColor = (Color.FromArgb(255, 200, 200));

            while (true)
            {
                IsAdjustGunForRobotTurn = true;
                IsAdjustRadarForGunTurn = true;
                IsAdjustRadarForRobotTurn = true;
                SetTurnRadarRightRadians(Double.PositiveInfinity);
                MaxVelocity = 5;
                Execute();
            }

        }

        public override void OnHitWall(HitWallEvent e)
        {
            movingForward = -movingForward;
        }

        public override void OnHitRobot(HitRobotEvent e)
        {
            // If we're moving the other robot, reverse!

            if (e.IsMyFault)
            {
                movingForward = -movingForward;
            }

        }
        public override void OnScannedRobot(ScannedRobotEvent e)
        {
            // if have more than 2 enemies, using Meele mode
            if (Others > 2)
            {
                StoreMovementOfEnemy(e);
                MinimumRiskMovement(e);
                FireNearlyEnemy();
            }
            else // using SOLO mode :D
            {
                // SET RADA FOCUS TO ENEMY
                double absoluteBearing = HeadingRadians + e.BearingRadians;
                var value = 3.5 * Utils.NormalRelativeAngle(absoluteBearing - RadarHeadingRadians);
                SetTurnRadarRightRadians(value);
                SetTurnGunRightRadians(value);
                TrackEnemySoloMode(e);
            }

        }
        public override void OnRobotDeath(RobotDeathEvent evnt)
        {
            _dictEnemyDatas.Remove(evnt.Name);
        }
        public override void OnDeath(DeathEvent evnt)
        {
            _dictEnemyDatas = null;
            MaxVelocity = 5;
            MaxTurnRate = 180;
        }
        public override void OnHitByBullet(HitByBulletEvent evnt)
        {
            SetTurnLeft(-90 - evnt.Bearing); //turn perpendicular to the enemy
            SetAhead(140 * movingForward);//move forward
        }

        private void StoreMovementOfEnemy(ScannedRobotEvent e)
        {
            double absBearing = HeadingRadians + e.BearingRadians;
            PN_Location enemyLocation = new PN_Location()
            {
                DateTime = DateTime.Now,
                Point = new PN_PointD(X + Math.Sin(absBearing) * e.Distance, Y + Math.Cos(absBearing) * e.Distance),
                BearingRadians = e.BearingRadians,
                Velocity = e.Velocity,
                HeadingRadians = e.HeadingRadians,
                Bearing = e.Bearing,
                Distance = e.Distance
            };
            if (!_dictEnemyDatas.ContainsKey(e.Name))
            {
                var enemy = new PN_EnemyRobot()
                {
                    Locations = new List<PN_Location>() { enemyLocation }
                };
                _dictEnemyDatas.Add(e.Name, enemy);
            }
            else
            {
                _dictEnemyDatas[e.Name].Locations.Add(enemyLocation);
            }

        }

        private void MinimumRiskMovement(ScannedRobotEvent e)
        {

            // expected ma co khoang 5 nguoi thi dau trong 1 van
            // xac dinh tu giac, tam giac dua vao toa do cua cac enemies
            // if(minh thuoc tu giac, tam giac)
            // tim duong di di chuyen ra ngoai da giac ngan nhat
            // else 
            // tim diet doi phuong
            var lastestLocation = GetLastestLocationOfEnemies();
            if (lastestLocation != null && lastestLocation.Count >= 3)
                BuildPolygon(lastestLocation);

        }

        /// <summary>
        /// code chom tren
        /// http://stackoverflow.com/questions/18808588/going-to-the-center-of-the-map-in-robocode
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        private double Norm(double a)
        {
            // do something smarter with modulus (%) here
            while (a <= -Math.PI) a += 2 * Math.PI;
            while (Math.PI < a) a -= 2 * Math.PI;
            return a;
        }

        /// <summary>
        /// code chom tren
        /// http://stackoverflow.com/questions/18808588/going-to-the-center-of-the-map-in-robocode
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        // get bearing in radians to a target(x,y) from the current position/heading
        private double GetBearing(double x, double y)
        {
            // can rotate the coordinate system to avoid the addition of pi/2 if you like 
            double b = Math.PI / 2 - Math.Atan2(y - Y, x - X);
            var brearing = Norm(b - HeadingRadians);
            if (double.IsNaN(brearing) || double.IsNegativeInfinity(brearing))
                return 0;
            else
                return brearing;

        }
        private double TinhDoDai2Diem(PN_PointD A, PN_PointD B)
        {
            return Math.Sqrt(Math.Pow((A.X - B.X), 2) + Math.Pow((A.Y - B.Y), 2));
        }
        private double TimBanKinh(PN_PointD cucTrai, PN_PointD cucPhai, PN_PointD cucTren, PN_PointD cucDuoi, PN_PointD trongTam)
        {
            if (!cucTrai.Equals(cucTren) && !cucTrai.Equals(cucDuoi) && !cucPhai.Equals(cucTren) && !cucPhai.Equals(cucDuoi))
            {
                return (TinhDoDai2Diem(trongTam, cucTrai) + TinhDoDai2Diem(trongTam, cucPhai) + TinhDoDai2Diem(trongTam, cucDuoi) + TinhDoDai2Diem(trongTam, cucTren)) / 4;

            }
            else
            {
                PN_PointD pointA, pointB, pointC;
                if (cucTrai.Equals(cucTren))
                {
                    pointA = cucTrai;
                    pointB = cucPhai;
                    pointC = cucDuoi;
                }
                else if (cucTrai.Equals(cucDuoi))
                {
                    pointA = cucTrai;
                    pointB = cucPhai;
                    pointC = cucTren;
                }
                else if (cucPhai.Equals(cucTren))
                {
                    pointA = cucTrai;
                    pointB = cucPhai;
                    pointC = cucDuoi;
                }
                else // cuc phai = cuc duoi
                {
                    pointA = cucTrai;
                    pointB = cucPhai;
                    pointC = cucTren;
                }
                return (TinhDoDai2Diem(trongTam, pointA) + TinhDoDai2Diem(trongTam, pointB) + TinhDoDai2Diem(trongTam, pointC)) / 3;
            }
        }

        private void BuildPolygon(List<PN_PointD> lstLocation)
        {
            try
            {
                PN_PointD pointCucTrai, pointCucPhai, pointCucTren, pointCucDuoi;
                pointCucDuoi = pointCucPhai = pointCucTrai = pointCucTren = lstLocation[0];

                // neu co mot diem vu la cuc trai hoac cuc phai....
                for (int i = 1; i < lstLocation.Count; i++)
                {
                    // x nho nhat
                    if (pointCucTrai.X > lstLocation[i].X) // diem cuc trai moi
                    {
                        pointCucTrai = lstLocation[i];
                    }
                    // x lon nhat
                    if (pointCucPhai.X < lstLocation[i].X)
                    {
                        pointCucPhai = lstLocation[i];
                    }
                    // y lon nhat
                    if (pointCucTren.Y < lstLocation[i].Y)
                    {
                        pointCucTren = lstLocation[i];
                    }
                    // y nho nhat
                    if (pointCucDuoi.Y > lstLocation[i].Y)
                    {
                        pointCucDuoi = lstLocation[i];
                    }
                }
                // tim diem cuc trai , toa do x nho nhat
                // tim diem cuc phai, toa do x lon nhat
                // tim diem cuc tren, toa do y lon nhat
                // tim diem cuc duoi, toa do y nho nhat
                Console.WriteLine("-----------------------------------------");
                Console.WriteLine("Cuc trai : {0}", pointCucTrai.ToString());
                Console.WriteLine("Cuc phai : {0}", pointCucPhai.ToString());
                Console.WriteLine("Cuc tren : {0}", pointCucTren.ToString());
                Console.WriteLine("Cuc duoi : {0}", pointCucDuoi.ToString());
                if ((pointCucDuoi.Equals(pointCucTrai) && pointCucDuoi.Equals(pointCucPhai)) ||
                    (pointCucTren.Equals(pointCucTrai) && pointCucTren.Equals(pointCucPhai)) ||
                    pointCucTren.Equals(pointCucDuoi) || pointCucTrai.Equals(pointCucPhai))
                {
                    Console.WriteLine("ko du dieu kien de lap da giac");
                }
                else
                {
                    PN_PointD trongTam = TimTrongTam(pointCucTrai, pointCucPhai, pointCucTren, pointCucDuoi);
                    double banKinh = TimBanKinh(pointCucTrai, pointCucPhai, pointCucTren, pointCucDuoi, trongTam);
                    double dSoVoiTrongTam = TinhDoDai2Diem(new PN_PointD(X, Y), trongTam);
                    double bearing = GetBearing(trongTam.X, trongTam.Y);
                    MaxVelocity = 5;

                    if (IsInsidePolygon(pointCucTrai, pointCucPhai, pointCucTren, pointCucDuoi))
                    {
                        double distance = banKinh - dSoVoiTrongTam + 50;
                        if (distance <= -10)
                        {
                            // random
                            SetTurnRightRadians(bearing);
                            SetTurnLeft(-90); //turn perpendicular to the enemy
                            SetBack(distance);//move forward
                        }
                        else
                        {
                            SetTurnRightRadians(bearing);
                            SetBack(banKinh - dSoVoiTrongTam - 10);
                        }
                        // nam trong da giac
                        Console.WriteLine("Nam trong da giac");
                    }
                    else
                    {
                        double distance = banKinh - dSoVoiTrongTam + 20;
                        if (distance <= -10)
                        {
                            SetTurnRightRadians(bearing);
                            SetTurnLeft(-90); //turn perpendicular to the enemy
                            SetBack(distance);//move forward
                        }
                        else
                        {
                            SetTurnRightRadians(bearing);
                            SetBack(banKinh - dSoVoiTrongTam - 10);
                        }

                        Console.WriteLine("KO Nam trong da giac");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("EXCEPTION : {0}", ex.Message);
            }
        }
        private void FireNearlyEnemy()
        {
            var location = GetNearlyEnemy();
            if (location != null)
            {
                TrackEnemyMeeleMode(location);
            }
        }
        private PN_Location GetNearlyEnemy()
        {
            PN_Location result = null;
            try
            {
                List<PN_Location> lsLocation = new List<PN_Location>();
                double min = double.MaxValue;
                PN_PointD myPosition = new PN_PointD(X, Y);

                foreach (var item in _dictEnemyDatas.Keys)
                {
                    var location = _dictEnemyDatas[item].Locations.LastOrDefault();
                    if (location != null)
                    {
                        double distance = TinhDoDai2Diem(myPosition, location.Point);
                        if (min > distance)
                        {
                            min = distance;
                            result = location;
                        }
                    }

                }
            }
            catch
            { }
            return result;
        }
        private PN_PointD TimTrongTam(PN_PointD cucTrai, PN_PointD cucPhai, PN_PointD cucTren, PN_PointD cucDuoi)
        {
            if (!cucTrai.Equals(cucTren) && !cucTrai.Equals(cucDuoi) && !cucPhai.Equals(cucTren) && !cucPhai.Equals(cucDuoi))
            {
                // la tu giac
                PN_PointD M1 = new PN_PointD((cucTren.X + cucTrai.X) / 2, (cucTren.Y + cucTrai.Y) / 2);
                PN_PointD M2 = new PN_PointD((cucDuoi.X + cucPhai.X) / 2, (cucDuoi.Y + cucPhai.Y) / 2);
                PN_PointD M = new PN_PointD((M1.X + M2.X) / 2, (M1.Y + M2.Y) / 2);
                return M;

            }
            else
            {
                PN_PointD pointA, pointB, pointC;
                if (cucTrai.Equals(cucTren))
                {
                    pointA = cucTrai;
                    pointB = cucPhai;
                    pointC = cucDuoi;
                }
                else if (cucTrai.Equals(cucDuoi))
                {
                    pointA = cucTrai;
                    pointB = cucPhai;
                    pointC = cucTren;
                }
                else if (cucPhai.Equals(cucTren))
                {
                    pointA = cucTrai;
                    pointB = cucPhai;
                    pointC = cucDuoi;
                }
                else // cuc phai = cuc duoi
                {
                    pointA = cucTrai;
                    pointB = cucPhai;
                    pointC = cucTren;
                }
                PN_PointD M = new PN_PointD((pointA.X + pointB.X + pointC.X) / 3, (pointA.Y + pointB.Y + pointC.Y) / 3);
                return M;
            }
        }
        private bool IsValidGiaoDiem(PN_PointD pointMe, PN_PointD pointTarget, PN_PointD pointGiaoDiem)
        {

            if (pointMe.X < pointTarget.X)
            {
                if (pointMe.X < pointGiaoDiem.X)
                {
                    return true;
                }
            }
            else
            {
                if (pointMe.X > pointTarget.X)
                {
                    if (pointMe.X > pointGiaoDiem.X)
                    {
                        return true;
                    }
                }
                else // x= x
                {
                    if (pointMe.Y < pointTarget.Y)
                    {
                        if (pointMe.Y < pointGiaoDiem.Y)
                        {
                            return true;
                        }

                    }
                    else if (pointMe.Y > pointTarget.Y)
                    {
                        if (pointMe.Y > pointGiaoDiem.Y)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;

        }
        private double TimGiaoDiem(double a1, double b1, double a2, double b2)
        {
            double x;
            if (double.IsInfinity(a1))
            {
                x = b1;
                return x;
            }
            if (double.IsInfinity(a2))
            {
                x = b2;
                return x;
            }

            return (b1 - b2) / (a2 - a1);
        }
        /// <summary>
        /// viet phuong trinh duong thang cua tung canh
        /// viet mot phuong trinh duong
        /// là từ điểm đó phóng ra 1 tia. 
        /// Nếu tia này cắt 1 số lẻ cạnh của đa giác thì điểm kia nằm trong đa giác, 
        /// số chẵn cạnh thì nó nằm bên ngoài đa giác. 
        /// Tất nhiên phải xét đến những trường hợp đặc biệt khi mà tia phóng ra cắt các cạnh tại đỉnh.
        /// </summary>
        /// <param name="cucTrai"></param>
        /// <param name="cucPhai"></param>
        /// <param name="cucTren"></param>
        /// <param name="cucDuoi"></param>
        private bool IsInsidePolygon(PN_PointD cucTrai, PN_PointD cucPhai, PN_PointD cucTren, PN_PointD cucDuoi)
        {
            double a1, a2, a3, a4; // he so goc cuc trai - cuc tren, cuc tren - cuc phai, cuc phai - cuc duoi, cuc duoi - cuc trai
            double b1, b2, b3, b4; // he so goc cuc trai - cuc tren, cuc tren - cuc phai, cuc phai - cuc duoi, cuc duoi - cuc trai
            a1 = a2 = a3 = a4 = 0;
            b1 = b2 = b3 = b4 = 0;
            // y = ax+b;
            a1 = BuildHeSoGocA(cucTrai, cucTren);
            b1 = BuildHeSoGocB(cucTrai, a1);

            a2 = BuildHeSoGocA(cucTren, cucPhai);
            b2 = BuildHeSoGocB(cucTren, a2);

            a3 = BuildHeSoGocA(cucPhai, cucDuoi);
            b3 = BuildHeSoGocB(cucPhai, a3);

            a4 = BuildHeSoGocA(cucDuoi, cucTrai);
            b4 = BuildHeSoGocB(cucDuoi, a4);

            // xac dinh diem can chon de xay dung duong thang

            double xTarget, yTarget;
            double aTarget, bTarget;
            xTarget = yTarget = 0;
            aTarget = bTarget = 0;
            PN_PointD pointMe = new PN_PointD(X, Y);
            PN_PointD pointTarget;
            //x = (b2 – b1)/(a1 – a2)
            if (!cucTrai.Equals(cucTren))
            {
                xTarget = (cucTrai.X + cucTren.X) / 2;
                yTarget = a1 * xTarget + b1;
            }
            else
            {
                if (!cucTrai.Equals(cucDuoi))
                {
                    xTarget = (cucTrai.X + cucDuoi.X) / 2;
                    yTarget = a4 * xTarget + b4;

                }
                else
                {
                    Console.WriteLine("KO THE CHON DIEM DE VE DUONG THANG");
                    return false;
                }
            }
            pointTarget = new PN_PointD(xTarget, yTarget);

            aTarget = BuildHeSoGocA(pointMe, pointTarget);
            bTarget = BuildHeSoGocB(pointMe, aTarget);

            double x1, x2, x3, x4;
            x1 = TimGiaoDiem(a1, b1, aTarget, bTarget); // (b1 - bTarget) / (aTarget - a1);
            x2 = TimGiaoDiem(a2, b2, aTarget, bTarget);//(b2 - bTarget) / (aTarget - a2);
            x3 = TimGiaoDiem(a3, b3, aTarget, bTarget); //(b3 - bTarget) / (aTarget - a3);
            x4 = TimGiaoDiem(a4, b4, aTarget, bTarget); //(b4 - bTarget) / (aTarget - a4);
            PN_PointD giaoDiem1, giaoDiem2, giaoDiem3, giaoDiem4;
            giaoDiem1 = new PN_PointD(x1, aTarget * x1 + bTarget);
            giaoDiem2 = new PN_PointD(x2, aTarget * x2 + bTarget);
            giaoDiem3 = new PN_PointD(x3, aTarget * x3 + bTarget);
            giaoDiem4 = new PN_PointD(x4, aTarget * x4 + bTarget);

            int countGiaoDiem = 0;
            if (!double.IsNaN(x1) && IsValidGiaoDiem(pointMe, pointTarget, giaoDiem1)
                && ((cucTrai.X <= x1 && x1 <= cucTren.X) || (cucTrai.X >= x1 && x1 >= cucTren.X)))
                countGiaoDiem++;

            if (!double.IsNaN(x2) && IsValidGiaoDiem(pointMe, pointTarget, giaoDiem2)
                && ((cucTren.X <= x2 && x2 <= cucPhai.X) || (cucTren.X >= x2 && x2 >= cucPhai.X)))
                countGiaoDiem++;

            if (!double.IsNaN(x3) && IsValidGiaoDiem(pointMe, pointTarget, giaoDiem3)
                && ((cucPhai.X <= x3 && x3 <= cucDuoi.X) || (cucPhai.X >= x3 && x3 >= cucDuoi.X)))
                countGiaoDiem++;

            if (!double.IsNaN(x4) && IsValidGiaoDiem(pointMe, pointTarget, giaoDiem4)
                && ((cucDuoi.X <= x4 && x4 <= cucTrai.X) || (cucDuoi.X >= x4 && x4 >= cucTrai.X)))
                countGiaoDiem++;

            if (countGiaoDiem % 2 != 0)
            {
                return true;
            }

            return false;
        }

        private double BuildHeSoGocA(PN_PointD point1, PN_PointD point2)
        {
            return (point2.Y - point1.Y) / (point2.X - point1.X);
            //b1 = cucTrai.Y - a1 * cucTrai.X;
        }
        private double BuildHeSoGocB(PN_PointD point1, double a)
        {
            if (!double.IsInfinity(a))
                return point1.Y - a * point1.X;
            else return point1.X;
        }
        private List<PN_PointD> GetLastestLocationOfEnemies()
        {

            List<PN_PointD> lsLocation = new List<PN_PointD>();
            foreach (var enemyKey in _dictEnemyDatas.Keys)
            {
                var lastLocation = _dictEnemyDatas[enemyKey].Locations.LastOrDefault();
                if (lastLocation != null)
                    lsLocation.Add(lastLocation.Point);
            }
            return lsLocation;
        }

        void CustomerSetFire(double distance, double energyOfEnemy)
        {
            if (distance > 300)
            {
                Fire(0.5);
                Fire(0.5);
                Fire(0.5);
            }
            else
            {
                Fire(3);
            }
         

        }
        /// <summary>
        /// Dung khi tren chien truong con 2 enemy tro xuong
        /// </summary>
        /// <param name="e"></param>
        private void TrackEnemySoloMode(ScannedRobotEvent e)
        {
            double absBearing = e.BearingRadians + HeadingRadians;
            double latVel = e.Velocity * Math.Sin(e.HeadingRadians - absBearing);
            double gunTurnAmt; 
            var randomizer = new Random();
            if (randomizer.NextDouble() > 0.9)
            {
                MaxVelocity = (12 * randomizer.NextDouble()) + 12; // random change the speed 
            }

            if (e.Distance > 150) // neu khoang cach kha xa, lai gan enemy de de ban
            {
                gunTurnAmt = Utils.NormalRelativeAngle(absBearing - GunHeadingRadians + latVel / 22);

                SetTurnGunRightRadians(gunTurnAmt);
                SetTurnRightRadians(Utils.NormalRelativeAngle(absBearing - HeadingRadians + latVel / Velocity));
                SetTurnLeft(-e.Bearing - 40); //quay 1 goc vua du de vua co the chay vong` vua co the tien lai gan enemy
                SetAhead((e.Distance - 140) * movingForward);// tien lai
                CustomerSetFire(e.Distance, e.Energy);
            }
            else // khoang cach gan, xoay xung quanh, neu gap tuong thi chay nguoc lai
            {
                gunTurnAmt = Utils.NormalRelativeAngle(absBearing - GunHeadingRadians + latVel / 15);
                SetTurnGunRightRadians(gunTurnAmt);
                SetTurnLeft(-90 - e.Bearing); //quay nguoi vuong goc voi enemy de quay xung quanh
                SetAhead(100 * movingForward);
                CustomerSetFire(e.Distance, e.Energy);
            }

        }
        /// <summary>
        /// Chien thuat dung khi  dong nguoi, chi chay xung quanh enemy gan nhat
        /// </summary>
        /// <param name="e"></param>
        private void TrackEnemyMeeleMode(PN_Location e)
        {
            double absBearing = e.BearingRadians + HeadingRadians;
            double latVel = e.Velocity * Math.Sin(e.HeadingRadians - absBearing);
            double gunTurnAmt;
            var randomizer = new Random();
            if (randomizer.NextDouble() > 0.9)
            {
                MaxVelocity = (12 * randomizer.NextDouble()) + 12; 
            }

            if (e.Distance > 150)
            {
                gunTurnAmt = Utils.NormalRelativeAngle(absBearing - GunHeadingRadians + latVel / 22);

                SetTurnGunRightRadians(gunTurnAmt);
                SetTurnRightRadians(Utils.NormalRelativeAngle(absBearing - HeadingRadians + latVel / Velocity));
                SetTurnLeft(-90 - e.Bearing); 
                SetAhead(100 * movingForward);
                Fire(POWER);
            }
            else
            {
                gunTurnAmt = Utils.NormalRelativeAngle(absBearing - GunHeadingRadians + latVel / 15);
                SetTurnGunRightRadians(gunTurnAmt);
                SetTurnLeft(-90 - e.Bearing); 
                SetAhead((e.Distance - 140) * movingForward);
                Fire(POWER);
            }
        }

      
    }



}
