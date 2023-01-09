using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CheeseUtils;
using CheeseUtils.Math;
using rlbot.flat;

namespace CheeseUtils
{
    public class Cars
    {
        public static int Count => AllCars.Count;
        public static List<Car> AllCars { get; private set; }
        public static List<Car> BlueCars { get { return AllCars.FindAll(car => car.Team == 0); } }
        public static List<Car> OrangeCars { get { return AllCars.FindAll(car => car.Team == 1); } }
        public static List<Car> AllLivingCars { get { return AllCars.FindAll(car => !car.IsDemolished); } }
        public static List<Car> AliveBlueCars { get { return AllCars.FindAll(car => car.Team == 0 && !car.IsDemolished); } }
        public static List<Car> AliveOrangeCars { get { return AllCars.FindAll(car => car.Team == 1 && !car.IsDemolished); } }
        public static List<Car> AllDeadCars { get { return AllCars.FindAll(car => car.IsDemolished); } }
        public static List<Car> DeadBlueCars { get { return AllCars.FindAll(car => car.Team == 0 && car.IsDemolished); } }
        public static List<Car> DeadOrangeCars { get { return AllCars.FindAll(car => car.Team == 1 && car.IsDemolished); } }

        public static void Initialize(GameTickPacket packet)
        {
            AllCars = new List<Car>();

            for (int c = 0; c < packet.PlayersLength; c++)
            {
                AllCars.Add(new Car(c, packet.Players(c).Value));
            }
        }

        public static void Update(GameTickPacket packet)
        {
            foreach (Car car in AllCars)
            {
                car.Update(packet.Players(car.Index).Value);
            }
        }
    }
}
    