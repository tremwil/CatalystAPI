using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//TODO: add documentation
#pragma warning disable 1591

namespace Catalyst.Input
{
    /// <summary>
    /// The actions that you can do by pressing keys.
    /// </summary>
    public enum GameAction
    {
        MoveForward,
        MoveLeft,
        MoveBackward,
        MoveRight,
        MoveSlow,
        DownActions,
        Jump,
        Shift,
        LightAttack,
        HeavyAttack,
        Disrupt,
        Quickturn,
        Interact,
        RunnersVision,
        MapView,
        CreateUGC
    }

    /// <summary>
    /// A bunch of methods regarding the GameAction enum.
    /// </summary>
    public static class GameActions
    {
        public static int Amount =>
            Enum.GetValues(typeof(GameAction)).Length;
    }
}
