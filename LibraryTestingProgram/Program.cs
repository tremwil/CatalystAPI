using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalyst;
using System.IO;
using System.Threading;
using System.Diagnostics;

using Catalyst.RuntimeInfo;
using Catalyst.Mathf;

namespace LibraryTestingProgram
{
    class Program
    {
        static void Main(string[] args)
        {
            var ginfo = new GameInformation();

            while (true)
            {
                Vec3 previous = ginfo.GetPosition();
                Vec3 current;
                Vec3 velocity;

                float deltaTime;
                long lastTimeMS = 0;
                long currentTimeMS;

                Stopwatch watch = new Stopwatch();
                watch.Start();

                Thread.Sleep(20);

                while (true)
                {
                    currentTimeMS = watch.ElapsedMilliseconds;
                    deltaTime = (currentTimeMS - lastTimeMS) / 1000f;
                    lastTimeMS = currentTimeMS;

                    current = ginfo.GetPosition();
                    velocity = (current - previous) / deltaTime;
                    previous = current;

                    Console.SetCursorPosition(0, 0);
                    Console.WriteLine("delta time : {0}                         \n", deltaTime);
                    Console.WriteLine("Position : {0}                         \n", current);
                    Console.WriteLine("Velocity : {0}                         \n", velocity);
                    Console.WriteLine("Yaw : {0} degrees                         \n", ginfo.GetYaw() * 180 / Math.PI);
                    Console.WriteLine("Is loading? {0}                         \n", ginfo.IsLoading());

                    Thread.Sleep(70);
                }
            }
        }
    }
}
