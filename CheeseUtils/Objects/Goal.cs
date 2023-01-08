using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using CheeseUtils;
using CheeseUtils.Math;
using rlbot.flat;

namespace CheeseUtils
{
    public class Goal
    {
        public const float Height = 640;
        public const float Width = 1780;
        public const float Depth = 880;

        public int Team;
        public Vec3 Location;
        public Vec3 LeftPost;
        public Vec3 RightPost;
        public Vec3 TopLeft;
        public Vec3 TopRight;
        public Vec3 BottomLeft;
        public Vec3 BottomRight;
        public Vec3 Crossbar;
           
        public Goal(int team)
        {
            Team = team;
            Location = new Vec3(0, Field.Length / 2 * Field.Side(Team), 0);
            LeftPost = new Vec3(Width / 2 * Field.Side(Team), Field.Length / 2 * Field.Side(Team), Height / 2);
            RightPost = new Vec3(Width / 2 * -Field.Side(Team), Field.Length / 2 * Field.Side(Team), Height / 2);
            TopLeft = new Vec3(Width / 2 * Field.Side(Team), Field.Length / 2 * Field.Side(Team), Height);
            TopRight = new Vec3(Width / 2 * -Field.Side(Team), Field.Length / 2 * Field.Side(Team), Height);
            BottomLeft = new Vec3(Width / 2 * Field.Side(Team), Field.Length / 2 * Field.Side(Team), 0);
            BottomRight = new Vec3(Width / 2 * -Field.Side(Team), Field.Length / 2 * Field.Side(Team), 0);
            Crossbar = new Vec3(0, Field.Length / 2 * Field.Side(Team), Height);
        }
    }
}
    