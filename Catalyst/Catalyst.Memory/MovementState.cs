using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable 1591

namespace Catalyst.Memory
{
    /// <summary>
    /// The different movements in the game.
    /// </summary>
    public enum MovementState
    {
        Dead = -1,
        StateChange = 0,
        Standing = 1,
        Airborne = 2,
        Vaulting = 3,
        Wallclimb = 4,
        HorizontalBar = 5,
        VolountarySlide = 6,
        CoilJump = 7,
        Wallrun = 8,
        // Missing 9
        MagropePullObject = 10,
        ClimbLadderOrPipe = 11,
        // Missing 12
        HorizontalBarSwing = 13,
        // Missing 14
        // Missing 15
        GoodLanding = 16,
        InvolountarySlide = 17,
        LedgeGrab = 18,
        Crouching = 19,
        LightAttackSpecial = 20,
        // Missing 21
        Springboard = 22,
        MagropeClimb = 23,
        BadLanding = 24,
        MagropeSwing = 25,
        HeavyAttack = 26,
        LightAttack = 27,
        // Missing 28
        Zipline = 29,
        Shifting = 30,
        OpenDoor = 31,
    }
}
