using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalyst.Settings;
using SharpDX.DirectInput;
using AutoIt;
using System.Threading;

namespace Catalyst.Input
{
    /// <summary>
    /// A class to handle keyboard and mouse input.
    /// </summary>
    public class InputController
    {
        #region kb/m hook

        private static DirectInput inputHandler;
        private static Keyboard keyboard;
        private static Mouse mouse;

        private static Thread UpdateThread;

        private static object lockVal = new object();

        private static KeyboardState kbstate;
        private static MouseState mstate;

        // 100 times per second
        private static int HOOK_UPDATE_MS = 10;

        #endregion

        /// <summary>
        /// A bool indicating the state of this controller.
        /// </summary>
        public static bool IsListening { get; private set; }

        private static bool[] actionKeysState;
        private static InputBinding[] bindings;

        // Initialize static stuff
        static InputController()
        {
            inputHandler = new DirectInput();
            keyboard = new Keyboard(inputHandler); keyboard.Acquire();
            mouse = new Mouse(inputHandler); mouse.Acquire();
            kbstate = new KeyboardState();
            mstate = new MouseState();
            UpdateThread = new Thread(UpdateKBMState);

            IsListening = false;

            actionKeysState = new bool[GameActions.Amount];
            ResetAKS();

            var sets = new Settings.Settings();
            sets.Load();

            bindings = sets.Controls.AsArray();
        }

        /// <summary>
        /// Start the input hook.
        /// </summary>
        public static void HookStart()
        {
            if (!IsListening)
            {
                IsListening = true;

                // A fast call to HookStop then HookStart will not start the thread twice
                if (!UpdateThread.IsAlive)
                    UpdateThread.Start();
            }
        }

        /// <summary>
        /// Stop the input hook.
        /// </summary>
        public static void HookStop()
        {
            IsListening = false;
        }

        /// <summary>
        /// Set the state of the given key code.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="pressed"></param>
        public static void SetKeyState(DIKCode code, bool pressed)
        {
            var str = "{" + code.ToAutoItString() + (pressed? " down}" : " up}");
            AutoItX.Send(str);
        }

        /// <summary>
        /// Set the state of the given mouse code.
        /// <para></para>NOTE : Mousewheel up/down is only pressed, not held.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="pressed"></param>
        public static void SetButtonState(MouseCode code, bool pressed)
        {
            var raw = code.ToRawCode();

            if (raw.Axis == 24)
            {
                string[] names = new string[3] { "LEFT", "RIGHT", "MIDDLE" };
                if (pressed) AutoItX.MouseDown(names[raw.Button]);
                else AutoItX.MouseUp(names[raw.Button]);
            }
            else
            {
                string dir = (raw.Negate == 0) ? "up" : "down";
                AutoItX.MouseWheel(dir, 10);
            }
        }

        /// <summary>
        /// Set the state of a game action key. 
        /// <para></para> NOTE: Actions bound to the mousewheel will only be pressed and not held.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="pressed"></param>
        public static void SetGameActionState(GameAction action, bool pressed)
        {
            var binding = bindings[(int)action];
            if (binding.KeyBinding != DIKCode.DIK_NONE)
                SetKeyState(binding.KeyBinding, pressed);
            else
                SetButtonState(binding.MouseBinding, pressed);
        }

        /// <summary>
        /// Simulate a keypress on the given key.
        /// </summary>
        /// <param name="code">The key code.</param>
        public static void PressKey(DIKCode code)
        {
            PressKey(code, 20);
        }

        /// <summary>
        /// Simulate a keypress on the given key.
        /// </summary>
        /// <param name="code">The key code.</param>
        /// <param name="pressTimeMS">The time the key will be pressed.</param>
        public static void PressKey(DIKCode code, int pressTimeMS)
        {
            SetKeyState(code, true);
            Thread.Sleep(pressTimeMS);
            SetKeyState(code, false);
        }

        private static void ResetAKS()
        {
            for (int i = 0; i < GameActions.Amount; i++)
            {
                actionKeysState[i] = false;
            }
        }

        /// <summary>
        /// Get the state of a game action key.
        /// </summary>
        /// <param name="action">The action in question.</param>
        /// <returns></returns>
        public static bool GetActionKeyState(GameAction action)
        {
            return actionKeysState[(int)action];
        }

        private static void UpdateKBMState()
        {
            DIKCode key;
            RawMouseCode mcode;

            bool kbcond;
            bool mousecond;

            // allows us to stop the thread when needed
            while (IsListening)
            {
                lock (lockVal)
                {   // Locking allows us to modify static stuff from the thread
                    keyboard.GetCurrentState(ref kbstate);
                    mouse.GetCurrentState(ref mstate);

                    for (int action = 0; action < actionKeysState.Length; action++)
                    {
                        key = bindings[action].KeyBinding;
                        mcode = bindings[action].MouseBinding.ToRawCode();

                        kbcond = (key == DIKCode.DIK_NONE) ? 
                            false : kbstate.IsPressed((Key)key);

                        // Here we use a little math trick.
                        // first the equation 1 - 2x maps 1 to -1 and 0 to 1
                        // then we multiply be the axis knowing that 
                        // for a and b such that sgn(a) = sgn(b) a * b > 0.
                        mousecond = (mcode.Axis == 24) ? 
                            mstate.Buttons[mcode.Button] :
                            mstate.Z * (1 - 2*mcode.Negate) > 0; 

                        actionKeysState[action] = kbcond || mousecond;
                    }
                }
                Thread.Sleep(HOOK_UPDATE_MS);
            }

            // Reset states once the hook stops
            ResetAKS();
        }
    }
}
