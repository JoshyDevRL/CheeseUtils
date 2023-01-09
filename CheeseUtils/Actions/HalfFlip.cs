using System;
using CheeseUtils.Math;

namespace CheeseUtils
{
	public class HalfFlip : CUAction
	{
		public const float Duration = 1.25f;
		public bool Finished { get; private set; }
		public bool Interruptible { get; private set; }
		public CUAction FollowUpAction { get; private set; }

		private bool _jumping = true;
		private float _startTime = -1;
		private int _step = 0;

		public HalfFlip(CUAction followUpAction = null)
        {
            Interruptible = false;
            Finished = false;
            FollowUpAction = followUpAction;
        }

        public void Run(CUBot bot)
		{
			if (_startTime == -1)
			{
				_startTime = Game.Time;
				_jumping = bot.Me.IsGrounded;
			}
			float elapsed = Game.Time - _startTime;

			if (elapsed < 0.1f && _jumping)
			{
				bot.Controller.Jump = true;
			}

			else if (_step < 3 && _jumping)
			{
				bot.Controller.Jump = false;
				_step++;
			}

			else if (elapsed < (_jumping ? 0.1f : 0) + 0.2f)
			{
				bot.Controller.Yaw = 0;
				bot.Controller.Pitch = 1;
				bot.Controller.Jump = true;
			}

			else if (elapsed < (_jumping ? 0.1f : 0) + 1f)
			{
				bot.AimAt(bot.Me.Location + bot.Me.Velocity.Flatten());
			}

			else
			{
				Finished = true;
			}
		}
	}
}
