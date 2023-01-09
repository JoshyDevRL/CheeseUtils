using System;
using System.Drawing;
using CheeseUtils.Math;

namespace CheeseUtils
{
	public class Drive : CUAction
	{
		public bool Finished { get; private set; }
		public bool Interruptible { get; private set; }
		public CUAction FollowUpAction { get; private set; }

		public Vec3 Target;
		public float TargetSpeed;
		public bool Backwards;
		public bool AllowDodges;
		public bool WasteBoost;
		public CUAction Action;

		private float timeOnGround = 0;

		public float TimeRemaining { get; private set; }

		public Drive(Car car, Vec3 target, float targetSpeed = Car.MaxSpeed, bool allowDodges = true, bool wasteBoost = false, CUAction followUpAction = null)
		{
			Interruptible = true;
			Finished = false;
			FollowUpAction = followUpAction;

            Target = target;
			TargetSpeed = targetSpeed;

			float forwardsEta = GetEta(car, target, false, false);
			float backwardsEta = GetEta(car, target, true, false);

			Backwards = backwardsEta + 0.5f < forwardsEta && car.Forward.Dot(car.Velocity) < 500 && car.Forward.FlatAngle(car.Location.Direction(target), car.Up) > MathF.PI * 0.6f;
			AllowDodges = allowDodges;
			WasteBoost = wasteBoost;

			Action = null;
		}

		public void Run(CUBot bot)
		{
			TimeRemaining = Distance(bot.Me) / TargetSpeed;
			Surface targetSurface = Field.NearestSurface(Target);

			if (Action == null)
			{
				if (bot.Me.IsGrounded)
					timeOnGround += bot.DeltaTime;

				Surface nextSurface = targetSurface;
				Surface mySurface = Field.NearestSurface(bot.Me.Location);

				float carSpeed = bot.Me.Velocity.Length();
				float forwardSpeed = bot.Me.Velocity.Dot(bot.Me.Forward);

				Vec3 finalTarget = Field.LimitToNearestSurface(Target);
				if (mySurface.Normal.Dot(targetSurface.Normal) < 0.95f)
				{
					nextSurface = FindNextSurface(Field.LimitToNearestSurface(bot.Me.Location), finalTarget);
					finalTarget = nextSurface.Limit(finalTarget);
					Vec3 closestSurfacePoint = mySurface.Limit(finalTarget);
					finalTarget = closestSurfacePoint - nextSurface.Normal.FlatNorm(mySurface.Normal) * MathF.Max(closestSurfacePoint.Dist(finalTarget) - 75, 0);
				}
				if (mySurface.Key != targetSurface.Key)
				{
					finalTarget = FindTargetAroundCorner(bot, finalTarget, nextSurface);
				}

				float turnRadius = TurnRadius(MathF.Abs(forwardSpeed));
				Vec3 nearestTurnCenter = mySurface.Limit(bot.Me.Location) + bot.Me.Right.FlatNorm(mySurface.Normal) * MathF.Sign(bot.Me.Right.Dot(finalTarget - bot.Me.Location)) * turnRadius;
				float landingTime = bot.Me.PredictLandingTime();

				if (Field.DistanceBetweenPoints(nearestTurnCenter, Target) > turnRadius - 40 && bot.Me.IsGrounded)
				{
					bot.Throttle(TargetSpeed, Backwards);
				}
				else if (!bot.Me.IsGrounded)
				{
					TimeRemaining = float.IsNaN(TimeRemaining) ? 0.01f : TimeRemaining;
					bot.Throttle(Distance(bot.Me) / MathF.Max(TimeRemaining - landingTime, 0.01f));
				}
				else
				{
					bot.Throttle(MathF.Max(SpeedFromTurnRadius(TurnRadius(bot.Me, Target)), 400), Backwards);
				}

				float angleToTarget;
				if (bot.Me.IsGrounded || bot.Me.Velocity.FlatLen() < 500)
				{
					angleToTarget = bot.AimAt(finalTarget, backwards: Backwards)[0];
				}
				else
				{
					Vec3 landingNormal = Field.FindLandingSurface(bot.Me).Normal;
					Vec3 targetDirection = Utils.Lerp(Utils.Cap(landingTime * 1.5f - 0.6f, 0, 0.75f), bot.Me.Velocity.FlatNorm(landingNormal), -Vec3.Up);
					bot.AimAt(bot.Me.Location + targetDirection, landingNormal);
					angleToTarget = bot.Me.Forward.Angle(targetDirection);
				}

				bot.Controller.Boost = bot.Controller.Boost && (angleToTarget < 0.3f || (angleToTarget < 0.8f && !bot.Me.IsGrounded)) && !Backwards && WasteBoost;
				bot.Controller.Handbrake = (MathF.Abs(angleToTarget) > 2 || (Field.DistanceBetweenPoints(nearestTurnCenter, Target) < turnRadius - 40 && SpeedFromTurnRadius(TurnRadius(bot.Me, Target)) < 400))
											&& mySurface.Normal.Dot(Vec3.Up) > 0.9f && bot.Me.Velocity.Normalize().Dot(bot.Me.Forward) > 0.9f;

				bot.Renderer.Line3D(finalTarget, finalTarget + Field.NearestSurface(finalTarget).Normal * 200, Color.LimeGreen);

				Vec3 predictedLocation = bot.Me.LocationAfterDodge();
				float timeLeft = bot.Me.Location.FlatDist(finalTarget) / MathF.Max(carSpeed + 500, 1410);
				float speedFlipTimeLeft = bot.Me.Location.FlatDist(finalTarget) / MathF.Max(carSpeed + 500 + MathF.Min(bot.Me.Boost, 40) * Car.BoostAccel / 2, 1410);

				if (AllowDodges && Field.InField(predictedLocation, 50) && carSpeed < 2000 && bot.Me.Location.z < 600 && Game.Gravity.z < -500 && MathF.Abs(bot.Me.Velocity.Dot(bot.Me.Up)) < 100)
				{
					if (forwardSpeed > 0)
					{
						if (TargetSpeed > 100 + forwardSpeed)
						{
							if (bot.Me.Location.z < 200 && bot.Me.IsGrounded && carSpeed > 1000 && bot.Me.Forward.FlatAngle(bot.Me.Location.Direction(finalTarget)) < 0.1f && timeOnGround > 0.2f)
							{
								Dodge dodge = new Dodge(bot.Me.Location.FlatDirection(Target));

								//if (speedFlipTimeLeft > SpeedFlip.Duration && bot.Me.Boost > 0 && Field.InField(predictedLocation, 500) && WasteBoost)
								//{ 
								//	// SPEEDFLIP
								//}
								if (timeLeft > dodge.Duration) // ELSE IF
								{
									Action = dodge;
								}
							}
							else if (bot.Me.Location.z > 100 && !bot.Me.HasDoubleJumped && (!bot.Me.IsGrounded || bot.Me.Velocity.Dot(Vec3.Up) < 200))
							{
								WaveDash wavedash = new WaveDash(bot.Me.Location.FlatDirection(Target));

								if (timeLeft > wavedash.Duration)
								{
									Action = wavedash;
								}
							}
						}
					}
					else if (bot.Me.Location.z < 200 && bot.Me.IsGrounded && carSpeed > 800 && Backwards && (-bot.Me.Forward).FlatAngle(bot.Me.Location.Direction(finalTarget)) < 0.1f && timeOnGround > 0.2f)
					{
						if (timeLeft > HalfFlip.Duration)
						{
							Action = new HalfFlip();
						}
					}
				}
			}
			else if (Action != null && Action.Finished)
			{
				Action = null;
				Backwards = false;
				timeOnGround = 0;
			}
			else if (Action != null)
			{
				Action.Run(bot);
				//if (Action is SpeedFlip)
				//{
				//	bot.Throttle(TargetSpeed + 500, Backwards);
				//}
			}

			bot.Renderer.Line3D(Field.LimitToNearestSurface(Target), Field.LimitToNearestSurface(Target) + targetSurface.Normal * 200, Color.LimeGreen);
			
			Interruptible = Action == null || Action.Interruptible;

			if (Field.LimitToNearestSurface(bot.Me.Location).Dist(Field.LimitToNearestSurface(Target)) < 100)
			{
				Finished = true;
			}
		}

		public float Distance(Car car)
		{
			return GetDistance(car, Target, Backwards);
		}

		public float Eta(Car car)
		{
			return GetEta(car, Target, Backwards, AllowDodges);
		}

		private static Surface FindNextSurface(Vec3 start, Vec3 target)
		{
			Vec3 middle = Field.LimitToNearestSurface((start + target) / 2);

			for (float f = 0; f < 2; f += 0.25f)
			{
				Vec3 nextPos = Field.LimitToNearestSurface(start + (middle - start) * Utils.Cap(f, 0, 1) + (target - middle) * Utils.Cap(f - 1, 0, 1));
				if (Field.NearestSurface(nextPos).Normal.Dot(Field.NearestSurface(start).Normal) < 0.95f)
				{
					return Field.NearestSurface(nextPos);
				}
			}

			return Field.NearestSurface(target);
		}

		private static Vec3 FindTargetAroundCorner(CUBot bot, Vec3 finalTarget, Surface nextSurface)
		{
			Surface mySurface = Field.NearestSurface(bot.Me.Location);

			if (mySurface.Key == "Ground")
			{
				Goal goal = Field.Side(bot.Team) == MathF.Sign(finalTarget.y) ? bot.OurGoal : bot.TheirGoal;

				Vec3 enterLeftDirection = bot.Me.Location.Direction(goal.LeftPost - new Vec3(MathF.Sign(goal.LeftPost.x) * 100, MathF.Sign(goal.LeftPost.y) * 50));
				Vec3 enterRightDirection = bot.Me.Location.Direction(goal.RightPost - new Vec3(MathF.Sign(goal.RightPost.x) * 100, MathF.Sign(goal.RightPost.y) * 50));
				Vec3 exitLeftDirection = bot.Me.Location.Direction(goal.LeftPost + new Vec3(MathF.Sign(goal.LeftPost.x) * 60, -MathF.Sign(goal.LeftPost.y) * 50));
				Vec3 exitRightDirection = bot.Me.Location.Direction(goal.RightPost + new Vec3(MathF.Sign(goal.RightPost.x) * 60, -MathF.Sign(goal.RightPost.y) * 50));

				Vec3 targetDirection = nextSurface.Key.Contains("Goal Ground") ?
									   bot.Me.Location.Direction(finalTarget).Clamp(enterLeftDirection, enterRightDirection, mySurface.Normal) :
									   bot.Me.Location.Direction(finalTarget).Clamp(exitRightDirection, exitLeftDirection, mySurface.Normal);

				return bot.Me.Location + targetDirection.Rescale(bot.Me.Location.Dist(finalTarget));
			}
			else if (mySurface.Key.Contains("Goal Ground"))
			{
				Goal goal = Field.Side(bot.Team) == MathF.Sign(bot.Me.Location.y) ? bot.OurGoal : bot.TheirGoal;

				Vec3 leftDirection = bot.Me.Location.Direction(goal.LeftPost - new Vec3(MathF.Sign(goal.LeftPost.x) * 100, MathF.Sign(goal.LeftPost.y) * 50));
				Vec3 rightDirection = bot.Me.Location.Direction(goal.RightPost - new Vec3(MathF.Sign(goal.RightPost.x) * 100, MathF.Sign(goal.RightPost.y) * 50));

				Vec3 targetDirection = bot.Me.Location.Direction(finalTarget).Clamp(rightDirection, leftDirection, mySurface.Normal);

				return bot.Me.Location + targetDirection.Rescale(bot.Me.Location.Dist(finalTarget));
			}
			else if (mySurface.Key.Contains("Backboard") || mySurface.Key.Contains("Backwall"))
			{
				Goal goal = Field.Side(bot.Team) == MathF.Sign(bot.Me.Location.y) ? bot.OurGoal : bot.TheirGoal;

				Vec3 leftDirection;
				Vec3 rightDirection;
				if (mySurface.Key.Contains("Left Backwall"))
				{
					leftDirection = bot.Me.Location.Direction(goal.TopRight + new Vec3(MathF.Sign(goal.TopRight.x) * 50, 0, 50));
					rightDirection = bot.Me.Location.Direction(goal.BottomRight + new Vec3(MathF.Sign(goal.BottomRight.x) * 50, 0, -50));
				}
				else if (mySurface.Key.Contains("Right Backwall"))
				{
					leftDirection = bot.Me.Location.Direction(goal.BottomLeft + new Vec3(MathF.Sign(goal.BottomLeft.x) * 50, 0, -50));
					rightDirection = bot.Me.Location.Direction(goal.TopLeft + new Vec3(MathF.Sign(goal.TopLeft.x) * 50, 0, 50));
				}
				else
				{
					leftDirection = bot.Me.Location.Direction(goal.TopLeft + new Vec3(MathF.Sign(goal.TopLeft.x) * 50, 0, 50));
					rightDirection = bot.Me.Location.Direction(goal.TopRight + new Vec3(MathF.Sign(goal.TopRight.x) * 50, 0, 50));
				}

				Vec3 targetDirection = bot.Me.Location.Direction(finalTarget).Clamp(leftDirection, rightDirection, mySurface.Normal);

				return bot.Me.Location + targetDirection.Rescale(bot.Me.Location.Dist(finalTarget));
			}

			return finalTarget;
		}

		public static float GetDistance(Car car, Vec3 target)
		{
			float forwardsEta = GetEta(car, target, false, false);
			float backwardsEta = GetEta(car, target, true, false);

			bool backwards = backwardsEta + 0.5f < forwardsEta && car.Forward.Dot(car.Velocity) < 500 && car.Forward.FlatAngle(car.Location.Direction(target), car.Up) > MathF.PI * 0.6f;

			return GetDistance(car, target, backwards);
		}

		public static float GetDistance(Car car, Vec3 target, bool backwards)
		{
			return GetDistance(car, target, backwards, out _, out _);
		}

		public static float GetDistance(Car car, Vec3 target, bool backwards, out float angle, out float radius)
		{
			target = Field.LimitToNearestSurface(target);
			Vec3 carPos = car.PredictLandingPosition();
			Surface carSurface = Field.FindLandingSurface(car);
			Vec3 surfaceNormal = carSurface.Normal;
			Vec3 carForward = car.IsGrounded ? car.Forward : (car.Velocity.FlatLen() > 500 ? car.Velocity.FlatNorm(surfaceNormal) : car.Location.FlatDirection(target, surfaceNormal));
			Vec3 carRight = carForward.Cross(car.IsGrounded ? -car.Up : -surfaceNormal).Normalize();

			float currentSpeed = car.Velocity.Dot(carForward);
			angle = (backwards ? -carForward : carForward).FlatAngle(target - carPos, surfaceNormal);
			float turnSpeed = backwards ? SpeedAfterTurn(-currentSpeed, angle, 0.4f) : SpeedAfterTurn(currentSpeed, angle, 0.5f);
			radius = TurnRadius(turnSpeed);

			Vec3 nearestTurnCenter = carPos + carRight * MathF.Sign(carRight.Dot(target - carPos)) * radius;
			Vec3 limitedTurnCenter = carSurface.Limit(nearestTurnCenter);
			if (nearestTurnCenter.Dist(limitedTurnCenter) > 1)
			{
				Vec3 normal = limitedTurnCenter.Direction(Field.LimitToNearestSurface(nearestTurnCenter + carSurface.Normal * 500));
				nearestTurnCenter = limitedTurnCenter + normal * nearestTurnCenter.Dist(limitedTurnCenter);
			}

			float distance = Field.DistanceBetweenPoints(nearestTurnCenter, target);

			if (distance < radius)
			{
				radius = TurnRadius(car, target);
				nearestTurnCenter = carPos + carRight * MathF.Sign(carRight.Dot(target - carPos)) * radius;

				distance = Field.DistanceBetweenPoints(nearestTurnCenter, target);
			}

			angle = MathF.Abs((carPos - nearestTurnCenter).FlatAngle(target - nearestTurnCenter, surfaceNormal) - ((target - carPos).Dot(backwards ? -carForward : carForward) < 0 ? 2 * MathF.PI : 0));
			angle -= MathF.Acos(Utils.Cap(radius / distance, 0, 1));
			angle = Utils.Cap(angle, 0, 2 * MathF.PI);

			return MathF.Sqrt(MathF.Max(MathF.Pow(distance, 2) - MathF.Pow(radius, 2), 0)) + radius * angle;
		}

		public static float GetEta(Car car, Vec3 target)
		{
			float forwardsEta = GetEta(car, target, false, false);
			float backwardsEta = GetEta(car, target, true, false);

			bool backwards = backwardsEta + 0.5f < forwardsEta && car.Forward.Dot(car.Velocity) < 500 && car.Forward.FlatAngle(car.Location.Direction(target), car.Up) > MathF.PI * 0.6f;

			return GetEta(car, target, backwards, true);
		}

		public static float GetEta(Car car, Vec3 target, bool allowDodges)
		{
			float forwardsEta = GetEta(car, target, false, false);
			float backwardsEta = GetEta(car, target, true, false);

			bool backwards = backwardsEta + 0.5f < forwardsEta && car.Forward.Dot(car.Velocity) < 500 && car.Forward.FlatAngle(car.Location.Direction(target), car.Up) > MathF.PI * 0.6f;

			return GetEta(car, target, backwards, allowDodges);
		}

		public static float GetEta(Car car, Vec3 target, bool backwards, bool allowDodges)
		{
			float distance = GetDistance(car, target, backwards, out float angle, out float radius);
			float turnDistance = angle * radius;
			distance -= turnDistance;

			Vec3 surfaceNormal = car.IsGrounded ? Field.NearestSurface(car.Location).Normal : Field.FindLandingSurface(car).Normal;
			Vec3 carForward = car.IsGrounded ? car.Forward : (car.Velocity.FlatLen() > 500 ? car.Velocity.FlatNorm(surfaceNormal) : car.Location.FlatDirection(target, surfaceNormal));
			float currentSpeed = carForward.Dot(car.Velocity);
			float landingTime = car.PredictLandingTime();

			if (backwards)
			{
				float speed = MathF.Max(SpeedAfterTurn(-currentSpeed, angle, 0.8f), 1400);
				return landingTime + turnDistance / MathF.Max(SpeedFromTurnRadius(radius), 400) + distance / speed;
			}
			else
			{
				float minSpeed = MathF.Max(SpeedAfterTurn(currentSpeed, angle), 1400);
				float finSpeed = Utils.Cap(minSpeed + Car.BoostAccel * car.Boost / Car.BoostConsumption, 1400, Car.MaxSpeed);
				float distanceWhileBoosting = (MathF.Pow(finSpeed, 2) - MathF.Pow(minSpeed, 2)) / (2 * Car.BoostAccel);

				if (distance < distanceWhileBoosting)
				{
					finSpeed = Utils.Cap(MathF.Sqrt(MathF.Max(MathF.Pow(minSpeed, 2) + 2 * Car.BoostAccel * distance, 0)), 1400, Car.MaxSpeed);
					return landingTime + turnDistance / MathF.Max(SpeedFromTurnRadius(radius), 400) + distance / ((minSpeed + finSpeed) / 2);
				}
				if (allowDodges && distance / finSpeed > 1.25f)
				{
					return landingTime + turnDistance / MathF.Max(SpeedFromTurnRadius(radius), 400) + distanceWhileBoosting / ((minSpeed + finSpeed) / 2) + (distance - distanceWhileBoosting) / (finSpeed + 500);
				}

				return landingTime + turnDistance / MathF.Max(SpeedFromTurnRadius(radius), 400) + distanceWhileBoosting / ((minSpeed + finSpeed) / 2) + (distance - distanceWhileBoosting) / finSpeed;
			}
		}

		public static float TurnRadius(Car car, Vec3 target)
		{
			float distance = Field.DistanceBetweenPoints(car.Location, target);
			return (distance / 2) / (car.Right * MathF.Sign(car.Right.Dot(target - car.Location))).Dot(car.Location.FlatDirection(target, car.Up));
		}

		public static float TurnRadius(float speed)
		{
			speed = Utils.Cap(speed, 0.01f, Car.MaxSpeed);
			if (speed <= 500)
				return Utils.Lerp(speed / 500, 145, 251);
			if (speed <= 1000)
				return Utils.Lerp((speed - 500) / 500, 251, 425);
			if (speed <= 1500)
				return Utils.Lerp((speed - 1000) / 500, 425, 727);
			if (speed <= 1750)
				return Utils.Lerp((speed - 1500) / 250, 727, 909);
			return Utils.Lerp((speed - 1750) / 550, 909, 1136);
		}

		public static float SpeedFromTurnRadius(float radius)
		{
			radius = Utils.Cap(radius, 145, 1136);
			if (radius <= 251)
				return Utils.Lerp((radius - 145) / 106, 0, 500);
			if (radius <= 425)
				return Utils.Lerp((radius - 251) / 174, 500, 1000);
			if (radius <= 727)
				return Utils.Lerp((radius - 425) / 302, 1000, 1500);
			if (radius <= 909)
				return Utils.Lerp((radius - 727) / 182, 1500, 1750);
			return Utils.Lerp((radius - 909) / 227, 1750, Car.MaxSpeed);
		}

		public static float SpeedAfterTurn(float currentSpeed, float angle, float modifier = 1)
		{
			return Utils.Cap((1234 * (MathF.Exp(angle * 0.49f * modifier) * angle * 0.49f * modifier * (currentSpeed > 1234 ? 0.2f : 1)) + currentSpeed) / ((MathF.Exp(angle * 0.49f * modifier) * angle * 0.49f * modifier * (currentSpeed > 1234 ? 0.2f : 1)) + 1), 0, Car.MaxSpeed);
		}
	}
}
