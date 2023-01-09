using System;
using System.Timers;
using System.Collections.Generic;
using CheeseUtils.Math;
using RLBotDotNet;
using rlbot.flat;
using Color = System.Drawing.Color;
using CheeseUtils;

namespace CheeseUtils
{
	public abstract partial class CUBot : Bot
	{
		public new ExtendedRenderer Renderer { get; internal set; }

		public CUAction Action = null;

		public Controller Controller = new Controller();

        public Car Me => Index < Cars.Count ? Cars.AllCars[Index] : new Car();

        public List<Car> Teammates { get { return Cars.AllCars.FindAll(car => car.Team == Team && car.Index != Index); } }
        public List<Car> AliveTeammates { get { return Cars.AllCars.FindAll(car => !car.IsDemolished && car.Team == Team && car.Index != Index); } }
        public List<Car> DeadTeammates { get { return Cars.AllCars.FindAll(car => car.IsDemolished && car.Team == Team && car.Index != Index); } }
        public List<Car> Opponents { get { return Cars.AllCars.FindAll(car => car.Team != Team); } }
        public List<Car> AliveOpponents { get { return Cars.AllCars.FindAll(car => !car.IsDemolished && car.Team != Team); } }
        public List<Car> DeadOpponents { get { return Cars.AllCars.FindAll(car => car.IsDemolished && car.Team != Team); } }

		public Goal OurGoal => Field.Goals[Team];
		public Goal TheirGoal => Field.Goals[1 - Team];

		public bool IsKickoff { get; private set; }
        public float DeltaTime { get; private set; }

        public bool _initialized = false;
        private float _lastTime = 0;

        public CUBot(string botName, int botTeam, int botIndex) : base(botName, botTeam, botIndex)
		{
			Console.WriteLine($"CheeseUtils bot \"{botName}\" has loaded in!");
		}

		private void InitializeBot(GameTickPacket packet)
		{
			Renderer = new ExtendedRenderer(base.Renderer);
			Cars.Initialize(packet);
			Field.Initialize(GetFieldInfo());
			_initialized = true;
		}

		private void UpdateInfo(GameTickPacket packet)
		{
			if (Cars.Count != packet.PlayersLength)
			{
				Cars.Initialize(packet);
			}

			else
			{
				Cars.Update(packet);
			}

			Field.Update(packet);
			Ball.Update(packet.Ball.Value);
			Game.Update(packet);

			if (!IsKickoff && Game.IsRoundActive && Game.IsKickoff)
			{
				Action = null;
			}

			IsKickoff = Game.IsRoundActive && Game.IsRoundActive;
		}

        private void UpdateDeltaTime()
        {
            DeltaTime = Game.Time - _lastTime;
            _lastTime = Game.Time;
        }

        public override Controller GetOutput(GameTickPacket packet)
		{
			Controller = new Controller(); 

			if (!_initialized)
			{
				InitializeBot(packet);
			}

			UpdateInfo(packet);

			Run();

            if (Action != null)
            {
                Action.Run(this);

                if (Action.Finished || Action.Interruptible || Me.IsDemolished)
                {
                    if (Action.FollowUpAction != null && Action.Finished)
					{
						Action = Action.FollowUpAction;
					}

					else
					{
						Action = null;
					}
                }
            }

            UpdateDeltaTime();

            return Controller; 
		}

		public abstract void Run();
	}
}
