using System;
using CheeseUtils.Math;

namespace CheeseUtils
{
	public class Arrive : CUAction
	{
		public bool Finished { get; private set; }
		public bool Interruptible { get; private set; }
		public CUAction FollowUpAction { get; private set; }


        public Vec3 Target;
		public Vec3 Direction;
		public float ArrivalTime;
		public bool AllowFlipping;
		public float RecoveryTime;
		public float TargetSpeed_;
		public Drive Drive;

		public float TimeRemaining { get; private set; }

		public Arrive(Car car, Vec3 target, Vec3? direction = null, float arrivalTime = -1f, bool allowFlipping = true, float recoveryTime = 0f, float targetSpeed_ = -1, CUAction followUpAction = null)
		{
			Interruptible = true;
			Finished = false;
			FollowUpAction = followUpAction;

			TargetSpeed_ = targetSpeed_;
            Target = target;
			Direction = direction ?? Vec3.Zero;
			ArrivalTime = arrivalTime;
			AllowFlipping = allowFlipping;
			Drive = new Drive(car, Target, Car.MaxSpeed, AllowFlipping, arrivalTime >= 0f);
			RecoveryTime = recoveryTime;
		}

		public void Run(CUBot bot)
		{
			float distance = Distance(bot.Me);
			float carSpeed = bot.Me.Velocity.Length();

			TimeRemaining = ArrivalTime < 0 ? distance / Car.MaxSpeed : MathF.Max(ArrivalTime - Game.Time, 0.001f);

			float targetSpeed = TargetSpeed_ == -1 ? distance / TimeRemaining : TargetSpeed_;

			Vec3 predictedLocation = bot.Me.LocationAfterDodge();
			Vec3 shiftedTarget;
			if (Direction.Length() > 0)
			{
				Vec3 surfaceNormal = Field.NearestSurface(Target).Normal;
				Vec3 directionToTarget = bot.Me.Location.FlatDirection(Target, surfaceNormal);

				float additionalShift = RecoveryTime * carSpeed;
				float shift = MathF.Min(Field.DistanceBetweenPoints(Target, bot.Me.Location) * 0.6f, Utils.Cap(carSpeed, 1410, Car.MaxSpeed) * 1.5f);
				float turnRadius = Drive.TurnRadius(Utils.Cap(carSpeed, 500, Car.MaxSpeed)) * 1.2f;

				shift *= targetSpeed < 2200 || ArrivalTime < 0 ? Utils.Cap((shift - additionalShift) / turnRadius, 0f, 1f) : 0;

				Vec3 leftDirection = directionToTarget.Cross(surfaceNormal).Normalize();
				Vec3 rightDirection = directionToTarget.Cross(-surfaceNormal).Normalize();
				shiftedTarget = Field.LimitToNearestSurface(Target - Direction.Clamp(leftDirection, rightDirection, surfaceNormal).Normalize() * shift);
			}
			else
			{
				shiftedTarget = Target;
			}

			float timeLeft = bot.Me.Location.FlatDist(Target) / MathF.Max(carSpeed + 500, 1410);

			Drive.AllowDodges = MathF.Sign(predictedLocation.FlatDirection(Target).Dot(Direction.Cross())) == MathF.Sign(bot.Me.Location.FlatDirection(Target).Dot(Direction.Cross())) && timeLeft > 1.35f + RecoveryTime;
			Drive.Target = shiftedTarget;
			Drive.TargetSpeed = targetSpeed;
			Drive.Run(bot);

			Interruptible = Drive.Interruptible;

			if (Field.LimitToNearestSurface(bot.Me.Location).Dist(Field.LimitToNearestSurface(Target)) < 100 || (ArrivalTime < Game.Time && ArrivalTime > 0))
			{
				Finished = true;
			}
		}

		public float Distance(Car car)
		{
			return Drive.GetDistance(car, Target);
		}

		public float Eta(Car car)
		{
			return Drive.GetEta(car, Target);
		}
	}
}
