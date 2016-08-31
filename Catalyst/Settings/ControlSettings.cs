using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

using Catalyst.Input;
namespace Catalyst.Settings
{
    /// <summary>
    /// The settings regarding game control.
    /// </summary>
    public class ControlSettings : SettingsCategory
    {
        static ControlSettings()
        {
            @namespace = "GstKeyBinding.default";
        }

        internal ControlSettings(Settings parent) : base(parent) { }

        private static Dictionary<GameAction, string> ActionToFieldName = new Dictionary<GameAction, string>()
        {
            { GameAction.CreateUGC, "ConceptSelectInventoryItem9" },
            { GameAction.Disrupt, "ConceptAltFire" },
            { GameAction.DownActions, "ConceptCrouch" },
            { GameAction.HeavyAttack, "ConceptMouseLeft" },
            { GameAction.Interact, "ConceptInteract" },
            { GameAction.Jump, "ConceptJump" },
            { GameAction.LightAttack, "ConceptFire" },
            { GameAction.MapView, "ConceptToggleScoreboard" },
            { GameAction.MoveBackward, "ConceptMoveBackward" },
            { GameAction.MoveForward, "ConceptMoveForward" },
            { GameAction.MoveRight, "ConceptMoveRight" },
            { GameAction.MoveLeft, "ConceptMoveLeft" },
            { GameAction.MoveSlow, "ConceptCrawl" },
            { GameAction.Quickturn, "ConceptMouseMiddle" },
            { GameAction.RunnersVision, "ConceptButtonRightThumb" },
            { GameAction.Shift, "ConceptMouseRight" }
        };

        /// <summary>
        /// Return an array containing all current input bindings.
        /// </summary>
        /// <returns></returns>
        public InputBinding[] AsArray()
        {
            var arr = new InputBinding[16];
            for (int i = 0; i < GameActions.Amount; i++)
                arr[i] = this[(GameAction)i];

            return arr;
        }

        /// <summary>
        /// Get the keybindings for a specific action.
        /// </summary>
        /// <param name="action">The action to get keybindings for.</param>
        /// <returns></returns>
        public InputBinding this[GameAction action]
        {
            get { return BindingFromFieldName(ActionToFieldName[action]); }
            set { UpdateFieldsFromBinding(value, ActionToFieldName[action]); }
        }

        /// <summary>
        /// Create a binding from raw data.
        /// </summary>
        /// <param name="fieldName">The field name.</param>
        /// <returns></returns>
        private InputBinding BindingFromFieldName(string fieldName)
        {
            var ib = new InputBinding(DIKCode.None, MouseCode.None);

            foreach (var input in settingValues[fieldName])
            {
                if ((int)input["type"] == 0)
                { // Keyboard
                    ib.KeyBinding = DIKCodes.Parse((int)input["button"]);
                }
                if ((int)input["type"] == 1)
                { // Mouse
                    ib.MouseBinding = RawMouseCode.ToMouseCode(
                        (int)input["axis"],
                        (int)input["button"],
                        (int)input["negate"]
                    );
                }
            }

            return ib;
        }

        /// <summary>
        /// Update raw data from bindings.
        /// </summary>
        /// <param name="newBind">The new binding.</param>
        /// <param name="fieldName">The field name.</param>
        private void UpdateFieldsFromBinding(InputBinding newBind, string fieldName)
        {
            var mCtrl = (JArray)settingValues[fieldName];
            JToken tk;
            JToken tk2;
            
            // Keyboard

            tk = mCtrl.Where(x => (int)x["type"] == 0).FirstOrDefault();

            if (tk == null)
            {
                tk2 = JObject.FromObject(new
                {
                    axis = 0,
                    button = (int)newBind.KeyBinding,
                    negate = 0,
                    type = 0
                });

                mCtrl.Add(tk2);
            }
            else
            {
                tk["button"] = (int)newBind.KeyBinding;
                if (newBind.KeyBinding == DIKCode.None) tk.Remove();
            }

            // Mouse

            var info = RawMouseCode.FromMouseCode(newBind.MouseBinding);
            tk = mCtrl.Where(x => (int)x["type"] == 1).FirstOrDefault();

            if (tk == null)
            {
                tk2 = JObject.FromObject(new
                {
                    axis = info.Axis,
                    button = info.Button,
                    negate = info.Negate,
                    type = 1
                });

                mCtrl.Add(tk2);
            }
            else
            {
                tk["axis"] = info.Axis;
                tk["button"] = info.Button;
                tk["negate"] = info.Negate;
                if (newBind.MouseBinding == MouseCode.None) tk.Remove();
            }
        }
    }
}
