using System;
using CheeseUtils;
using CheeseUtils.Math;

namespace CheeseUtils
{
    public class Dodge : CUAction
    {
        public bool Finished { get; private set; }
        public bool Interruptible { get; private set; }
        public CUAction FollowUpAction { get; private set; }

        public Vec3 Direction { get; set; }
        public float JumpTime { get; set; }
        public float Duration { get { return JumpTime + 1.15f; } }

        private bool _jumping = true;
        private float _startTime = -1;
        private Vec3 _input = Vec3.Zero;
        private int _step = 0;

        public Dodge(Vec3 direction, float jumpTime = 0.1f, CUAction followUpAction = null)
        {
            Interruptible = false;
            Finished = false;
            FollowUpAction = followUpAction;

            Direction = direction;
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

            if (bot.Me.IsGrounded && elapsed > (_jumping ? JumpTime : 0) + 0.1f)
            {
                Finished = true;
            }

            else if (elapsed < JumpTime && _jumping)
            {
                bot.Controller.Jump = true;
            }

            else if (_step < 3 && _jumping)
            {
                bot.Controller.Jump = false;
                _step++;
            }

            else if (elapsed < (_jumping ? JumpTime : 0) + 0.6f)
            {
                if (_input.Length() == 0)
                {
                    Vec3 localDirection = new Vec3(-bot.Me.Forward.FlatNorm().Cross().Dot(Direction), -bot.Me.Forward.FlatNorm().Dot(Direction));

                    float forwardVel = bot.Me.Forward.Dot(bot.Me.Velocity);
                    float s = MathF.Abs(forwardVel) / Car.MaxSpeed;
                    bool backwardsDodge = MathF.Abs(forwardVel) < 100 ? (localDirection[0] < 0) : (localDirection[0] >= 0) != (forwardVel > 0);

                    localDirection[0] /= backwardsDodge ? (16f / 15f) * (1 + 1.5f * s) : 1;
                    localDirection[1] /= (1 + 0.9f * s);

                    localDirection = localDirection.Normalize();

                    _input = localDirection;
                }

                bot.Controller.Yaw = _input[0];
                bot.Controller.Pitch = _input[1];
                bot.Controller.Jump = true;
            }

            else
            {
                Finished = true;
            }
        }
    }
}
