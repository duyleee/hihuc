using MH_HiHuc.MH;
using Robocode;
using Robocode.Util;
using System;
using System.Drawing;

namespace MH_HiHuc
{
    //https://www.ibm.com/developerworks/library/j-antigrav/index.html
    public class LastSurvive : AdvancedRobot
    {
        static PointF[] enemyPoints = new PointF[5];
        int count;
        public override void Run()
        {
            while (true)
            {
                TurnRadarRight(360);
            }
        }
        public override void OnScannedRobot(ScannedRobotEvent e)
        {

        }

        void antiGravMove()
        {
            double xforce = 0;
            double yforce = 0;
            double force;
            double ang;
            GravPoint p;

            for (int i = 0; i < gravpoints.size(); i++)
            {
                p = (GravPoint)gravpoints.elementAt(i);
                //Calculate the total force from this point on us
                force = p.power / Math.pow(getRange(getX(), getY(), p.x, p.y), 2);
                //Find the bearing from the point to us
                ang =
            normaliseBearing(Math.PI / 2 - Math.atan2(getY() - p.y, getX() - p.x));
                //Add the components of this force to the total force in their 
                //respective directions
                xforce += Math.sin(ang) * force;
                yforce += Math.cos(ang) * force;
            }

            /**The following four lines add wall avoidance.  They will only 
            affect us if the bot is close to the walls due to the
            force from the walls decreasing at a power 3.**/
            xforce += 5000 / Math.pow(getRange(getX(),
              getY(), getBattleFieldWidth(), getY()), 3);
            xforce -= 5000 / Math.pow(getRange(getX(),
              getY(), 0, getY()), 3);
            yforce += 5000 / Math.pow(getRange(getX(),
              getY(), getX(), getBattleFieldHeight()), 3);
            yforce -= 5000 / Math.pow(getRange(getX(),
              getY(), getX(), 0), 3);

            //Move in the direction of our resolved force.
            goTo(getX() - xforce, getY() - yforce);
        }

        /**Move in the direction of an x and y coordinate**/
        void goTo(double x, double y)
        {
            double dist = 20;
            double angle = RadianToDegree(absoluteBearing(X, Y, x, y));
            double r = turnTo(angle);
            SetAhead(dist * r);
        }

        private double RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
        }
        public double absoluteBearing(double x, double y, double x1, double y1)
        {
            return Math.Atan2(y1 - y, x1 - x);
        }

        /**Turns the shortest angle possible to come to a heading, then returns 
the direction the bot needs to move in.**/
        int turnTo(double angle)
        {
            double ang;
            int dir;
            ang = normalizeBearing(Heading - angle);
            if (ang > 90)
            {
                ang -= 180;
                dir = -1;
            }
            else if (ang < -90)
            {
                ang += 180;
                dir = -1;
            }
            else {
                dir = 1;
            }
            SetTurnLeft(ang);
            return dir;
        }


        // normalizes a bearing to between +180 and -180
        double normalizeBearing(double angle)
        {
            while (angle > 180) angle -= 360;
            while (angle < -180) angle += 360;
            return angle;
        }

        /*Returns the distance between two points**/
        double getRange(double x1, double y1, double x2, double y2)
        {
            double x = x2 - x1;
            double y = y2 - y1;
            double range = Math.Sqrt(x * x + y * y);
            return range;
        }
    }
}
