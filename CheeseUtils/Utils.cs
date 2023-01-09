using System;
using System.Collections.Generic;
using System.Linq;
using CheeseUtils.Math;
using CheeseUtils;
using RLBotDotNet;

namespace CheeseUtils
{
    public static class Utils
    {
        public static float Cap(float value, float min, float max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        public static int Cap(int value, int min, int max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        public static float Lerp(float t, float a, float b)
        {
            return (b - a) * t + a;
        }

        public static Vec3 Lerp(float t, Vec3 a, Vec3 b)
        {
            return (b - a) * t + a;
        }

        public static float Invlerp(float v, float a, float b)
        {
            return (v - a) / (b - a);
        }

        public static float Norm(Vec3 v)
        {
            return MathF.Sqrt(v.Dot(v));
        }

        public static float ThrottleAccel(float v)
        {
            const int n = 3;
            float[][] values =
            {
          new float[] {0.0f, 1600.0f},
          new float[] {1400.0f, 160.0f},
          new float[] {1410.0f, 0.0f}
            };

            float input = Clip(MathF.Abs(v), 0.0f, 1410.0f);

            for (int i = 0; i < (n - 1); i++)
            {
                if (values[i][0] <= input && input < values[i + 1][0])
                {
                    float u = (input - values[i][0]) / (values[i + 1][0] - values[i][0]);
                    return Lerp(values[i][1], values[i + 1][1], u);
                }
            }

            return -1.0f;
        }

        public static float MaxTurningCurve(float v)
        {
            const int n = 6;
            float[][] values =
            {
          new float[] {0.0f, 0.00690f},
          new float[] {500.0f, 0.00398f},
          new float[] {1000.0f, 0.00235f},
          new float[] {1500.0f, 0.00138f},
          new float[] {1750.0f, 0.00110f},
          new float[] {2300.0f, 0.00088f}
            };

            float input = Clip(MathF.Abs(v), 0.0f, 2300.0f);

            for (int i = 0; i < (n - 1); i++)
            {
                if (values[i][0] <= input && input < values[i + 1][0])
                {
                    float u = (input - values[i][0]) / (values[i + 1][0] - values[i][0]);
                    return Lerp(values[i][1], values[i + 1][1], u);
                }
            }

            return -1.0f;
        }

        public static float Clip(float x, float minimum, float maximum)
        {
            return MathF.Max(MathF.Min(x, maximum), minimum);
        }

        public static float[] Quadratic(float a, float b, float c)
        {
            float inside = MathF.Sqrt((b * b) - (4 * a * c));
            if (a != 0 && !float.IsNaN(inside))
            {
                return new float[2] { (-b + inside) / (2 * a), (-b - inside) / (2 * a) };
            }
            return new float[2] { -1, -1 };
        }

        public static float TimeToJump(Vec3 up, float height, bool doubleJump = false)
        {
            float gravity = up.Dot(Game.Gravity) != 0 ? up.Dot(Game.Gravity) : -0.001f;
            float heightAfterJump = Car.JumpVel * Car.JumpMaxDuration +
                Car.JumpAccel * Car.JumpMaxDuration * Car.JumpMaxDuration / 2 -
                Car.StickyAccel * 0.05f * (Car.JumpMaxDuration - 0.025f) +
                gravity * Car.JumpMaxDuration * Car.JumpMaxDuration / 2;
            float doubleJumpMultiplier = doubleJump ? 2 : 1;

            float intVelAfterJump = Car.JumpVel * doubleJumpMultiplier - 16.25f + (gravity + Car.JumpAccel) * Car.JumpMaxDuration;
            float finVelAfterJump = MathF.Sqrt(MathF.Max(MathF.Pow(intVelAfterJump, 2) + 2 * gravity * (height - heightAfterJump), 0));

            if (height < heightAfterJump)
            {
                float finVel = MathF.Sqrt(MathF.Max(MathF.Pow(Car.JumpVel - 16.25f, 2) + 2 * (gravity + Car.JumpAccel) * height, 0));
                return (finVel - Car.JumpVel + 16.25f) / (gravity + Car.JumpAccel);
            }
            return Car.JumpMaxDuration + (finVelAfterJump - intVelAfterJump) / gravity;
        }

        public static float PredictRotation(float acceleration, float time)
        {
            if (acceleration * time > Car.MaxAngularVel)
            {
                float timeToReachMax = Car.MaxAngularVel / acceleration;

                return (acceleration / 2) * MathF.Pow(timeToReachMax, 2) + Car.MaxAngularVel * (time - timeToReachMax);
            }

            return (acceleration / 2) * MathF.Pow(time, 2);
        }

        public static float ShotPowerModifier(float value)
        {
            value = Cap(value, 0, 4600);
            if (value <= 500)
            {
                return 0.65f;
            }
            else if (value <= Car.MaxSpeed)
            {
                return Lerp((value - 500) / 1800, 0.65f, 0.55f);
            }
            return Lerp((value - Car.MaxSpeed) / 4600, 0.55f, 0.3f);
        }
    }

    public abstract partial class CUBot : Bot
    {
        public float[] AimAt(Vec3 targetLocation, Vec3 up = new(), bool backwards = false)
        {
            Vec3 localTarget = Me.Local(targetLocation - Me.Location) * (backwards ? -1 : 1);
            Vec3 safeUp = up.Length() != 0 ? up : Vec3.Up;
            Vec3 localUp = Me.Local(safeUp.Normalize());
            float[] targetAngles = new float[3] {
                MathF.Atan2(localTarget.z, localTarget.x),
				MathF.Atan2(localTarget.y, localTarget.x),
				MathF.Atan2(localUp.y, localUp.z)
			};

            Controller.Steer = SteerPD(targetAngles[1], -Me.LocalAngularVelocity[2] * 0.01f) * (backwards ? -1 : 1);
            Controller.Pitch = SteerPD(targetAngles[0], Me.LocalAngularVelocity[1] * 0.2f);
            Controller.Yaw = SteerPD(targetAngles[1], -Me.LocalAngularVelocity[2] * 0.15f);
            Controller.Roll = SteerPD(targetAngles[2], Me.LocalAngularVelocity[0] * 0.25f);

            return targetAngles;
        }

        private static float SteerPD(float angle, float rate)
        {
            return Utils.Cap(MathF.Pow(35 * (angle + rate), 3) / 10, -1f, 1f);
        }

        public float Throttle(float targetSpeed, bool backwards = false)
        {
            float carSpeed = Me.Local(Me.Velocity).x; // The car's speed in the forward direction
            float speedDiff = (targetSpeed * (backwards ? -1 : 1)) - carSpeed;
            Controller.Throttle = Utils.Cap(MathF.Pow(speedDiff, 2) * MathF.Sign(speedDiff) / 1000, -1, 1);
            Controller.Boost = targetSpeed > 1400 && speedDiff > 50 && carSpeed < 2250 && Controller.Throttle == 1 && !backwards;
            return carSpeed;
        }
    }
}
