using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CheeseUtils;
using CheeseUtils.Math;
using rlbot.flat;

namespace CheeseUtils
{
    public static class Game
    {
        public static float Time { get; private set; }
        public static float TimeRemaining { get; private set; }
        
        public static bool IsRoundActive { get; private set; }
        public static bool IsKickoff { get; private set; }
        public static bool MatchEnded { get; private set; }

        static Game()
        {
            Time = 0.0f;
            TimeRemaining = 300.0f;

            IsRoundActive = false;
            IsKickoff = false;
            MatchEnded = false;
        }

        public static void Update(GameTickPacket packet)
        {
            Time = packet.GameInfo.Value.SecondsElapsed;
            TimeRemaining = packet.GameInfo.Value.GameTimeRemaining;

            IsRoundActive = packet.GameInfo.Value.IsRoundActive;
            IsKickoff = packet.GameInfo.Value.IsKickoffPause;
            MatchEnded = packet.GameInfo.Value.IsMatchEnded;
        }
    }
}
    