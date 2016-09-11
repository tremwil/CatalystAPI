using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

using Catalyst;
using Catalyst.Memory;
using Catalyst.Display;
using Catalyst.Input;

namespace LibraryTestingProgram
{
    enum Speeds
    {
        SuperSlow = 1,
        RunSpeed = 8,
        Slow = 25,
        Normal = 50,
        Fast = 100,
        SuperFast = 200,
        UltraFast = 400,
        SanicFast = 1000,
        Over9000 = 9001
    }

    class Program
    {
        static void Main(string[] args)
        {
            NoclipTest();
        }

        static void NoclipTest()
        {
            var memory = new MemoryManager();
            memory.OpenProcess("MirrorsEdgeCatalyst");

            var ginfo = new GameInfo(memory);
            var pinfo = new PlayerInfo(memory);

            Vec3 pos = Vec3.Zero;
            Vec3 dpos;
            Vec3 yawvec = Vec3.Zero;
            int dtms = 5;

            var speedlist = (Speeds[])Enum.GetValues(typeof(Speeds));
            int speed = 2;

            bool noclip = false;
            bool freeze = false;

            var flyup = DIKCode.SPACE;
            var flydown = DIKCode.LSHIFT;
            var toggle = DIKCode.F1;
            var tfreeze = DIKCode.C;

            Overlay.AddAutoField("speed", () => noclip ? "SPEED: " + speedlist[speed].ToString() : "");
            Overlay.AddAutoField("nclip", () => noclip ? "NOCLIP ON" : "");
            Overlay.Enable(true);

            InputController.MakeProcessSpecific("MirrorsEdgeCatalyst");
            InputController.EnableInputHook();

            while (true)
            {
                bool tg = InputController.IsKeyToggled(toggle);
                if (tg && !noclip)
                    pos = pinfo.GetPosition();
                noclip = tg;

                tg = InputController.IsKeyToggled(tfreeze);
                if (tg && !freeze)
                    ginfo.SetTimescale(0);
                if (!tg && freeze)
                    ginfo.SetTimescale(1);
                freeze = tg;

                if (noclip)
                {
                    if (InputController.WasKeyPressed(DIKCode.UP))
                        speed += (speed < speedlist.Length - 1) ? 1 : 0;

                    if (InputController.WasKeyPressed(DIKCode.DOWN))
                        speed -= (speed > 0) ? 1 : 0;

                    yawvec = pinfo.GetCameraYawVector();
                    dpos = Vec3.Zero;

                    if (InputController.IsGameActionPressed(GameAction.MoveForward))
                        dpos += yawvec;

                    if (InputController.IsGameActionPressed(GameAction.MoveBackward))
                        dpos -= yawvec;

                    if (InputController.IsGameActionPressed(GameAction.MoveLeft))
                        dpos += yawvec.Left;

                    if (InputController.IsGameActionPressed(GameAction.MoveRight))
                        dpos += yawvec.Right;

                    if (InputController.IsKeyPressed(flyup))
                        dpos += Vec3.AxisY;

                    if (InputController.IsKeyPressed(flydown))
                        dpos -= Vec3.AxisY;

                    dpos *= (float)speedlist[speed] * dtms / 1000;
                    pos += dpos;

                    pinfo.SetPosition(pos);
                }

                Thread.Sleep(dtms);
            }
        }

        static void MonitorAcceleration()
        {
            var memory = new MemoryManager();
            memory.OpenProcess("MirrorsEdgeCatalyst");

            var ginfo = new GameInfo(memory);
            var pinfo = new PlayerInfo(memory);

            var vpoints = new List<Tuple<float, long>>();

            float cy;
            float ly = pinfo.GetPosition().y;

            float lv = 0;
            float cv = 0;

            float acc = 0;

            Overlay.AddAutoField("vel", () => cv, "V: {0}");
            Overlay.AddAutoField("acc", () => acc, "A: {0}");
            Overlay.Enable(true);

            long ltime = 0;
            long ctime;
            long dtMs;

            Stopwatch watch = new Stopwatch();
            watch.Start();
            Thread.Sleep(10);
          
            while (true)
            {
                ctime = watch.ElapsedMilliseconds;
                dtMs = ctime - ltime;

                cy = pinfo.GetPosition().y;
                cv = (cy - ly) / dtMs * 1000;

                acc = (cv - lv) / dtMs * 1000;

                ltime = ctime;
                ly = cy;
                lv = cv;

                Thread.Sleep(70);
            }
        }

        static void CalculateGamePhysUpdateTime()
        {
            Stopwatch watch = new Stopwatch();
            long freqMS = Stopwatch.Frequency / 1000;

            long ctick;
            long ptick = 0;
            long ticksPerUpdate = 0;

            var memory = new MemoryManager();
            memory.OpenProcess("MirrorsEdgeCatalyst");
            var pinfo = new PlayerInfo(memory);

            Vec3 lp = pinfo.GetPosition();
            Vec3 cp;

            Overlay.AddAutoField("gtick", () => ticksPerUpdate / freqMS);
            Overlay.Enable(true);

            watch.Start();

            while (true)
            {
                cp = pinfo.GetPosition();

                if (lp != cp)
                {
                    ctick = watch.ElapsedTicks;
                    ticksPerUpdate = ctick - ptick;
                    ptick = ctick;
                }

                lp = cp;
            }
        }
    }
}
