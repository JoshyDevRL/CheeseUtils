using CheeseUtils.Math;
using System;

namespace CheeseUtils
{
	public abstract class Shot : CUAction
	{
		public abstract bool Finished { get; internal set; }
		public abstract bool Interruptible { get; internal set; }
		public abstract CUAction FollowUpAction { get; internal set; }

		public abstract BallSlice Slice { get; internal set; }
		public abstract Vec3 ShotTarget { get; internal set; }
		public abstract Vec3 TargetLocation { get; internal set; }
		public abstract Vec3 ShotDirection { get; internal set; }

		public abstract bool IsValid(Car car);

		internal bool ShotValid(float threshold = 60)
		{
			BallSlice[] slices = Ball.Prediction.Slices;
			int soonest = 0;
			int latest = slices.Length - 1;

			while (latest + 1 - soonest > 2)
			{
				int midpoint = (soonest + latest) / 2;
				if (slices[midpoint].Time > Slice.Time)
					latest = midpoint;
				else
					soonest = midpoint;
			}

			float dt = slices[latest].Time - slices[soonest].Time;
			float timeFromSoonest = Slice.Time - slices[soonest].Time;
			Vec3 slopes = (slices[latest].Location - slices[soonest].Location) * (1 / dt);

			Vec3 predictedBallLocation = slices[soonest].Location + (slopes * timeFromSoonest);

			return (Slice.Location - predictedBallLocation).Length() < threshold;
		}

		public abstract void Run(CUBot bot);
	}
}
