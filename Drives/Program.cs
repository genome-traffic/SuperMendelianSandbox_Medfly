using System;

namespace DrivesMedfly
{
    class Program
    {
        static void Main(string[] args)
        {
            Simulation Sim1 = new Simulation();
            Console.WriteLine("Initializing...");
            Sim1.Simulate();
            Console.WriteLine("Simulation Ends.");

        }
    }
}
