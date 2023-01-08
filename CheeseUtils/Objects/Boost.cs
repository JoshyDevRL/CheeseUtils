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
    public class Boost
    {
        public int Index;
        public float Radius;
        public float Height;
        public bool IsLarge;
        public Vec3 Location;

        public bool IsActive { get; private set; }
        public float TimeUntilActive { get; private set; }

        public Boost(int index)
        {
            Index = index;
            Radius = 0.0f;
            Height = 0.0f;
            Location = Vec3.Zero;
            IsLarge = false;
            IsActive = true;
            TimeUntilActive = 0.0f;
        }

        public Boost(int index, BoostPad boostPad)
        {
            Index = index;
            Radius = boostPad.IsFullBoost ? 208.0f : 144.0f;
            Height = boostPad.IsFullBoost ? 168.0f : 165.0f;
            Location = new Vec3(boostPad.Location.Value);
            IsLarge = boostPad.IsFullBoost;
            IsActive = true;
            TimeUntilActive = 0.0f;
        }

        public void Update(BoostPadState boost)
        {
            IsActive = boost.IsActive;
            TimeUntilActive = boost.Timer;
        }
    }
}
    