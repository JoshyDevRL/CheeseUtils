using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CheeseUtils;
using CheeseUtils.Math;
using rlbot.flat;

namespace CheeseUtils
{
    public class Car
    {
        public Vec3 Location;
        public Vec3 Velocity;
        public Vec3 AngularVelocity;
        public Vec3 Rotation;
        public Mat3x3 Orientation;

        public Vec3 Forward { get { return Orientation.Forward; } set { Orientation[0] = value; } }
        public Vec3 Right { get { return Orientation.Right; } set { Orientation[1] = value; } }
        public Vec3 Up { get { return Orientation.Up; } set { Orientation[2] = value; } }

        public string Name;
        public int Team;
        public int Index;
        public int Boost;
        public bool IsGrounded;
        public bool IsAirborne;
        public bool HasJumped;
        public bool HasDoubleJumped;
        public bool IsSupersonic;
        public bool IsDemolished;

        public Car()
        {
            Location = Vec3.Zero;
            Velocity = Vec3.Zero;
            AngularVelocity = Vec3.Zero;
            Rotation = Vec3.Zero;
            Orientation = new Mat3x3(Rotation);

            Name = "";
            Team = 0;
            Index = 0;
            Boost = 0;
            IsGrounded = false;
            IsAirborne = false;
            HasJumped = false;
            HasDoubleJumped = false;
            IsSupersonic = false;
            IsDemolished = false;
        }

        public Car(int index, PlayerInfo info)
        {
            Location = info.Physics.Value.Location.HasValue ? new Vec3(info.Physics.Value.Location.Value) : Vec3.Zero;
            Velocity = info.Physics.Value.Velocity.HasValue ? new Vec3(info.Physics.Value.Velocity.Value) : Vec3.Zero;
            AngularVelocity = info.Physics.Value.AngularVelocity.HasValue ? new Vec3(info.Physics.Value.AngularVelocity.Value) : Vec3.Zero;
            Rotation = info.Physics.Value.Rotation.HasValue ? new Vec3(info.Physics.Value.Rotation.Value) : Vec3.Zero;
            Orientation = new Mat3x3(Rotation);

            Name = info.Name;
            Team = info.Team;
            Index = index;
            Boost = info.Boost;
            IsGrounded = info.HasWheelContact;
            IsAirborne = !info.HasWheelContact;
            HasJumped = info.Jumped;
            HasDoubleJumped = info.DoubleJumped;
            IsSupersonic = info.IsSupersonic;
            IsDemolished = info.IsDemolished;
        }

        public void Update(PlayerInfo info)
        {
            Location = info.Physics.Value.Location.HasValue ? new Vec3(info.Physics.Value.Location.Value) : Location;
            Velocity = info.Physics.Value.Velocity.HasValue ? new Vec3(info.Physics.Value.Velocity.Value) : Velocity;
            AngularVelocity = info.Physics.Value.AngularVelocity.HasValue ? new Vec3(info.Physics.Value.AngularVelocity.Value) : AngularVelocity;
            Rotation = info.Physics.Value.Rotation.HasValue ? new Vec3(info.Physics.Value.Rotation.Value) : Rotation;
            Orientation = new Mat3x3(Rotation);

            Boost = info.Boost;
            IsGrounded = info.HasWheelContact;
            IsAirborne = !info.HasWheelContact;
            HasJumped = info.Jumped;
            HasDoubleJumped = info.DoubleJumped;
            IsSupersonic = info.IsSupersonic;
            IsDemolished = info.IsDemolished;
        }
    }
}
    