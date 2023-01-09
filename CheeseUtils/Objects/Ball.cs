using System;
using CheeseUtils;
using CheeseUtils.Math;
using rlbot.flat;

namespace CheeseUtils
{
    public class Ball
    {
        public const float MaxSpeed = 6000.0f;
        public const float Radius = 93.15f;

        public static Ball MainBall { get { return new Ball(Location, Velocity, AngularVelocity); } }

        public static Vec3 Location { get; private set; }
        public static Vec3 Velocity { get; private set; }
        public static Vec3 AngularVelocity { get; private set; }

        public Vec3 location;
        public Vec3 velocity;
        public Vec3 angularVelocity;

        public static BallPrediction Prediction { get; private set; }

        static Ball()
        {
            Location = Vec3.Zero;
            Velocity = Vec3.Zero;
            AngularVelocity = Vec3.Zero;
        }

        public Ball(Vec3 location, Vec3 velocity, Vec3? angularVelocity = null)
        {
            this.location = location;
            this.velocity = velocity;
            this.angularVelocity = angularVelocity ?? Vec3.Zero;
        }

        public static void Update(BallInfo info)
        {
            Location = info.Physics.Value.Location.HasValue ? new Vec3(info.Physics.Value.Location.Value) : Location;
            Velocity = info.Physics.Value.Velocity.HasValue ? new Vec3(info.Physics.Value.Velocity.Value) : Velocity;
            AngularVelocity = info.Physics.Value.AngularVelocity.HasValue ? new Vec3(info.Physics.Value.AngularVelocity.Value) : AngularVelocity;
        }
    }
}
    
