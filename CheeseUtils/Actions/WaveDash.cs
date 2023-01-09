using System;
using CheeseUtils.Math;
using CheeseUtils;

namespace CheeseUtils
{
    public class WaveDash : CUAction
    {
        public bool Finished { get; private set; }
        public bool Interruptible { get; private set; }
        public CUAction FollowUpAction { get; private set; }

        public Vec3 Direction;
        public float JumpTime;
        public float Duration { get { return JumpTime * 4 + 0.8f; } }

        private bool _jumping = true;
        private float _startTime = -1;
        private Vec3 _input = Vec3.Zero;

        public WaveDash(Vec3? direction = null, float jumpTime = 0.05f, CUAction followUpAction = null)
        {
            Interruptible = false;
            Finished = false;
            FollowUpAction = followUpAction;

            Direction = direction ?? Vec3.Zero;
            JumpTime = jumpTime;
        }

        public void Run(CUBot bot)
        {
            if (_startTime == -1)
            {
                _startTime = Game.Time;
                _jumping = bot.Me.IsGrounded;
            }
            float elapsed = Game.Time - _startTime;

            if (elapsed < JumpTime && _jumping)
            {
                bot.Controller.Jump = true;
            }

            else if (!bot.Me.IsGrounded && bot.Me.Location.z < 40 && bot.Me.Velocity.z < -100)
            {
                if (_input.Length() == 0)
                {
                    _input = Direction.Length() > 0 ?
                            new Vec3(bot.Me.Local(Direction)[1], -bot.Me.Local(Direction)[0]) :
                            new Vec3(bot.Me.Local(bot.Me.Velocity).Normalize()[1], -bot.Me.Local(bot.Me.Velocity).Normalize()[0]);
                }

                bot.Controller.Yaw = _input[0];
                bot.Controller.Pitch = _input[1];
                bot.Controller.Jump = true;
            }

            else if (!bot.Me.IsGrounded)
            {
                bot.AimAt(bot.Me.Forward);
            }

            else
            {
                Finished = true;
            }
        }
    }
}
