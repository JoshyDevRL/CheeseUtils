using System;
using rlbot.flat;
using CheeseUtils.Math;
using CheeseUtils;

namespace CheeseUtils
{
    public class Car
    {
        public const float JumpMaxDuration = 0.2f;
        public const float JumpAccel = 1458.333f;
        public const float JumpVel = 291.667f;
        public const float AirThrottleAccel = 66.667f;
        public const float BrakeAccel = 3500.0f;
        public const float BoostAccel = 991.667f;
        public const float BoostConsumption = 33.3f;
        public const float MaxSpeed = 2300.0f;
        public const float StickyAccel = 325.0f;
        public const float PitchAngularAccel = 12.46f;
        public const float YawAngularAccel = 9.11f;
        public const float RollAngularAccel = 38.34f;
        public const float MaxAngularVel = 5.5f;

        public Vec3 Location;
        public Vec3 Velocity;
        public Vec3 AngularVelocity;
        public Vec3 LocalAngularVelocity;
        public Vec3 Rotation;
        public Mat3x3 Orientation;

        public Vec3 Forward { get { return Orientation.Forward; } set { Orientation[0] = value; } }
        public Vec3 Right { get { return Orientation.Right; } set { Orientation[1] = value; } }
        public Vec3 Up { get { return Orientation.Up; } set { Orientation[2] = value; } }

        public string Name;
        public int Index;
        public int Team;
        public int Boost;
        public bool IsGrounded;
        public bool HasJumped;
        public bool HasDoubleJumped;
        public bool IsDemolished;
        public bool IsSupersonic;

        public Car()
        {
            Location = Vec3.Zero;
            Velocity = Vec3.Zero;
            AngularVelocity = Vec3.Zero;
            LocalAngularVelocity = Vec3.Zero;
            Rotation = Vec3.Zero;
            Orientation = new Mat3x3(Rotation);

            Name = "";
            Index = 0;
            Team = 0;
            Boost = 0;
            IsGrounded = false;
            HasJumped = false;
            HasDoubleJumped = false;
            IsDemolished = false;
            IsSupersonic = false;
        }

        public Car(Car originalCar)
        {
            Location = originalCar.Location;
            Velocity = originalCar.Velocity;
            AngularVelocity = originalCar.AngularVelocity;
            LocalAngularVelocity = originalCar.LocalAngularVelocity;
            Rotation = originalCar.Rotation;
            Orientation = new Mat3x3(Rotation);

            Name = originalCar.Name;
            Index = originalCar.Index;
            Team = originalCar.Team;
            Boost = originalCar.Boost;
            IsGrounded = originalCar.IsGrounded;
            HasJumped = originalCar.HasJumped;
            HasDoubleJumped = originalCar.HasDoubleJumped;
            IsDemolished = originalCar.IsDemolished;
            IsSupersonic = originalCar.IsSupersonic;
        }

        public Car(int index, PlayerInfo playerInfo)
        {
            Location = playerInfo.Physics.Value.Location.HasValue ? new Vec3(playerInfo.Physics.Value.Location.Value) : Vec3.Zero;
            Velocity = playerInfo.Physics.Value.Velocity.HasValue ? new Vec3(playerInfo.Physics.Value.Velocity.Value) : Vec3.Zero;
            AngularVelocity = playerInfo.Physics.Value.AngularVelocity.HasValue ? new Vec3(playerInfo.Physics.Value.AngularVelocity.Value) : Vec3.Zero;
            Rotation = playerInfo.Physics.Value.Rotation.HasValue ? new Vec3(playerInfo.Physics.Value.Rotation.Value) : Vec3.Zero;
            Orientation = new Mat3x3(Rotation);
            LocalAngularVelocity = Local(AngularVelocity);

            Name = playerInfo.Name;
            Index = index;
            Team = playerInfo.Team;
            Boost = playerInfo.Boost;
            IsGrounded = playerInfo.HasWheelContact;
            HasJumped = playerInfo.Jumped;
            HasDoubleJumped = playerInfo.DoubleJumped;
            IsDemolished = playerInfo.IsDemolished;
            IsSupersonic = playerInfo.IsSupersonic;
        }

        public void Update(PlayerInfo playerInfo)
        {
            Location = playerInfo.Physics.Value.Location.HasValue ? new Vec3(playerInfo.Physics.Value.Location.Value) : Location;
            Velocity = playerInfo.Physics.Value.Velocity.HasValue ? new Vec3(playerInfo.Physics.Value.Velocity.Value) : Velocity;
            AngularVelocity = playerInfo.Physics.Value.AngularVelocity.HasValue ? new Vec3(playerInfo.Physics.Value.AngularVelocity.Value) : AngularVelocity;
            Rotation = playerInfo.Physics.Value.Rotation.HasValue ? new Vec3(playerInfo.Physics.Value.Rotation.Value) : Rotation;
            Orientation = new Mat3x3(Rotation);
            LocalAngularVelocity = Local(AngularVelocity);

            Boost = playerInfo.Boost;
            IsGrounded = playerInfo.HasWheelContact;
            HasJumped = playerInfo.Jumped;
            HasDoubleJumped = playerInfo.DoubleJumped;
            IsDemolished = playerInfo.IsDemolished;
            IsSupersonic = playerInfo.IsSupersonic;
        }

        public Vec3 Local(Vec3 vec)
        {
            return Orientation.Dot(vec);
        }

        public float PredictLandingTime()
        {
            if (IsGrounded)
                return 0;

            float groundLandingTime = Utils.Quadratic(Game.Gravity.z / 2, Velocity.z, Location.z - 15)[1];
            float ceilingLandingTime = Utils.Quadratic(Game.Gravity.z / 2, Velocity.z, Location.z + 15 - Field.Height)[1];
            float landingTime = ceilingLandingTime < 0 ? groundLandingTime : ceilingLandingTime;
            Vec3 landingPos = PredictLocation(landingTime);

            if (!Field.InField(landingPos, 150))
            {
                Surface landingSurface = Field.FindLandingSurface(this);
                landingTime = MathF.Max((Location - landingSurface.Limit(Location)).Dot(landingSurface.Normal) / Velocity.Dot(-landingSurface.Normal), 0);
            }

            return landingTime;
        }

        public Vec3 PredictLandingPosition()
        {
            return PredictLocation(PredictLandingTime());
        }

        public Car Predict(float time)
        {
            return new Car(this) { Location = Location + Velocity * time + Game.Gravity * 0.5f * time * time, Velocity = Velocity + Game.Gravity * time };
        }

        public Vec3 PredictLocation(float time)
        {
            return Location + Velocity * time + Game.Gravity * 0.5f * time * time;
        }

        public Vec3 PredictVelocity(float time)
        {
            return Velocity + Game.Gravity * time;
        }

        public Vec3 LocationAfterDodge()
        {
            return Location + (Velocity + Velocity.FlatNorm() * 500).Cap(0, Car.MaxSpeed) * 1.3f;
        }

        public Vec3 LocationAfterJump(float time, float elapsed)
        {
            float jumpTimeRemaining = Utils.Cap(JumpMaxDuration - elapsed, 0, JumpMaxDuration);
            float stickTimeRemaining = Utils.Cap(0.05f - elapsed, 0, 0.05f);
            return Location + Velocity * time + Game.Gravity * 0.5f * MathF.Pow(time, 2) +
                (IsGrounded ? (Up * JumpVel * time) : Vec3.Zero) +
                Up * JumpAccel * jumpTimeRemaining * (time - 0.5f * jumpTimeRemaining) -
                Up * StickyAccel * stickTimeRemaining * (time - 0.5f * stickTimeRemaining);
        }

        public Vec3 LocationAfterDoubleJump(float time, float elapsed)
        {
            float jumpTimeRemaining = Utils.Cap(JumpMaxDuration - elapsed, 0, JumpMaxDuration);
            float stickTimeRemaining = Utils.Cap(0.05f - elapsed, 0, 0.05f);
            return Location + Velocity * time + Game.Gravity * 0.5f * MathF.Pow(time, 2) +
                (IsGrounded ? (Up * JumpVel * time) : Vec3.Zero) +
                Up * JumpAccel * jumpTimeRemaining * (time - 0.5f * jumpTimeRemaining) -
                Up * StickyAccel * stickTimeRemaining * (time - 0.5f * stickTimeRemaining) +
                (!HasDoubleJumped ? Up * JumpVel * (time - jumpTimeRemaining) : Vec3.Zero);
        }

        public Vec3 VelocityAfterJump(float time, float elapsed)
        {
            float jumpTimeRemaining = Utils.Cap(JumpMaxDuration - elapsed, 0, JumpMaxDuration);
            float stickTimeRemaining = Utils.Cap(0.05f - elapsed, 0, 0.05f);
            return Velocity + Game.Gravity * time + Up * (IsGrounded ? JumpVel : 0) + Up * JumpAccel * jumpTimeRemaining - Up * StickyAccel * stickTimeRemaining;
        }

        public Vec3 VelocityAfterDoubleJump(float time, float elapsed)
        {
            float jumpTimeRemaining = Utils.Cap(JumpMaxDuration - elapsed, 0, JumpMaxDuration);
            float stickTimeRemaining = Utils.Cap(0.05f - elapsed, 0, 0.05f);
            return Velocity + Game.Gravity * time + Up * JumpVel * (IsGrounded ? 2 : 1) + Up * JumpAccel * jumpTimeRemaining - Up * StickyAccel * stickTimeRemaining;
        }
    }
}