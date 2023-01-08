using System;
using System.Linq;
using System.Collections.Generic;
using rlbot.flat;
using CheeseUtils.Math;
using CheeseUtils;

namespace CheeseUtils
{
	public static class Field
	{
		public const float Length = 10240;
		public const float Width = 8192;
		public const float Height = 1950;

		public const float CornerIntersection = 8064;
		public const float CornerLength = 1629;
		public const float CornerWidth = 1152;

        public static Goal[] Goals { get; private set; }
        public static Goal BlueGoal => Goals[0];
        public static Goal OrangeGoal => Goals[1];

        public static List<Boost> Boosts { get; private set; }

		static Field()
		{
			Goals = new Goal[] { new Goal(0), new Goal(1) };
			Boosts = new List<Boost>();

		}

		public static void Initialize(FieldInfo info)
		{
			for (int c = 0; c < info.BoostPadsLength; c++)
            {
				if (info.BoostPads(c).HasValue)
                {
                    Boosts.Add(new Boost(c, info.BoostPads(c).Value));
				}

                else
                {
                    Boosts.Add(new Boost(c));
                }	
            }
        }

		public static void Update(GameTickPacket packet)
		{
			for (int c = 0; c < packet.BoostPadStatesLength; c++)
            {
				if (packet.BoostPadStates(c).HasValue)
                {
                    Boosts[c].Update(packet.BoostPadStates(c).Value);
				}
            }
        }

        public static int Side(int team)
		{
			return 2 * team - 1;
		}
	}
}
