using rlbot.flat;
using CheeseUtils.Math;

namespace CheeseUtils
{
	public class BallSlice
	{
		public readonly Vec3 Location;
		public readonly Vec3 Velocity;
		public readonly Vec3 AngularVelocity;
		public readonly float Time;

		public BallSlice(PredictionSlice slice)
		{
			Location = new Vec3(slice.Physics.Value.Location.Value);
			Velocity = new Vec3(slice.Physics.Value.Velocity.Value);
			AngularVelocity = new Vec3(slice.Physics.Value.AngularVelocity.Value);
			Time = slice.GameSeconds;
		}

		public Ball ToBall()
		{
			return new Ball(Location, Velocity, AngularVelocity);
		}
	}
}
