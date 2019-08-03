﻿namespace Helion.Maps.Geometry.Lines
{
    public enum LineSpecialType
    {
        None,
        DR_DoorOpenClose,
        W1_DoorOpenStay,
        W1_CloseDoor,
        W1_DoorOpenClose,
        W1_RaiseFloorToLowestAdjacentCeiling,
        W1_FastCrusherCeiling,
        S1_RaiseStairs8,
        W1_RaiseStairs8,
        S1_Donut,
        W1_LowerLiftRaise,
        S_EndLevel,
        W1_LightLevelMatchBrightness,
        W1_LightOnMaxBrightness,
        S1_RaiseFloor32MatchAdjacentChangeTexture,
        S1_RaiseFloor24MatchAdjacentChangeTexture,
        W1_CloseDoor30Seconds,
        W1_BlinkLightStartEveryOneSecond,
        S1_RaiseFloorMatchNextHigherFloor,
        W1_LowerFloorToHighestAdjacentFloor,
        S1_RaiseFloorToMatchNextHigher,
        S1_LowerLiftRaise,
        W1_RaiseFloorToMatchNextHigherChangeTexture,
        S1_LowerFloorToLowerAdjacentFloor,
        G1_RaiseFloorToLowestAdjacentCeiling,
        W1_SlowCrusherCeiling,
        DR_OpenBlueKeyClose,
        DR_OpenYellowKeyClose,
        DR_OpenRedKeyClose,
        S1_OpenDoorClose,
        W1_RaiseFloorByShortestLowerTexture,
        D1_OpenDoorStay,
        D1_OpenBlueKeyStay,
        D1_OpenRedKeyStay,
        D1_OpenYellowKeyStay,
        W1_LightOffMinBrightness,
        W1_LowerFloorEightAboveHighestAdjacentFloor,
        W1_LowerFloorToLowestAdjacentFloorChangeTexture,
        W1_LowerFloorToLowestAdjacentFloor,
        W1_Teleport,
        W1_RaiseCeilingToHighestAdjacentCeiling,
        W1_LowerCeilingToFloor,
        SR_CloseDoor,
        SR_LowerCeilingToFloor,
        W1_LowerCeilingToEightAboveFloor,
        SR_LowerFloorToHighestAdjacentFloor,
        GR_OpenDoorStayOpen,
        G1_RaiseFloorToMatchNextHigherChangeTexture,
        ScrollTextureLeft,
        S1_SlowCrusherCeilingToEightAboveFloor,
        S1_CloseDoor,
        S_EndLevelSecret,
        W_EndLevel,
        W1_StartMovingFloorPerpetual,
        W1_StopMovingFloor,
        S1_CrusherFloorRaiseToEightBelowAdjacentCeiling,
        W1_CrusherFloorRaiseToEightBelowAdjacentCeiling,
        W1_StopCrusherCeiling,
        W1_RaiseFloorTwentyFour,
        W1_RaiseFloorTwentyFourMatchTexture,
        SR_LowerFloorToLowestAdjacentFloor,
        SR_OpenDoorStay,
        SR_LowerLiftRaise,
        SR_OpenDoorClose,
        SR_RaiseFloorToLowestAdjacentCeiling,
        SR_CrusherFloorRaiseToEightBelowAdjacentCeiling,
        SR_RaiseFloorTwentyFourMatchTexture,
        SR_RaiseFloorThirtyTwoMatchTexture,
        SR_RaiseFloorToNextHigherMatchTexture,
        SR_RaiseFloorToNextHigher,
        SR_LowerFloorToEightAboveHighestAdjacentFloor,
        S1_LowerFloorToEightAboveHighestAdjacentFloor,
        WR_LowerCeilingToEightAboveFloor,
        WR_SlowCrusherCeilingFastDamage,
        WR_StopCrusherCeiling,
        WR_CloseDoor,
        WR_CloseDoorThirtySeconds,
        WR_FastCrusherCeilingSlowDamage,
        Unused1,
        WR_LiftOffMinBrightness,
        WR_LightLevelMatchBrightestAdjacent,
        WR_LightOnMaxBrigthness,
        WR_LowerFloorToLowestAdjacentFloor,
        WR_LowerFloorToHighestAdjacentFloor,
        WR_LowerFLoorToLowestAdjacentFloorChangeTexture,
        Unused2,
        WR_OpenDoorStay,
        WR_StartMovingFloorPerpetual,
        WR_LowerLiftRaise,
        WR_StopMovingFloor,
        WR_OpenDoorClose,
        WR_RaiseFloorToLowestAdjacentCeiling,
        WR_RaiseFLoorTwentyFour,
        WR_RaiseFLoorChangeTexture,
        WR_CrusherFLoorRaiseToEightBelowAdjacentCeiling,
        WR_RaiseFloorToMatchNextHigherChangeTexture,
        WR_RaiseByShortestLowerTexture,
        WR_Teleport,
        WR_LowerFloorToEightAboveHighestAdjacentFloor,
        SR_OpenBlueKeyFastStay,
        W1_RaiseStairsFast,
        S1_RaiseFLoorToLowestAdjacentCeiling,
        S1_LowerFLoorToHighestAdjacentFloor,
        S1_OpenDoorStay,
        W1_LightMatchDimmestAdjacent,
        WR_OpenDoorFastClose,
        WR_OpenDoorFastStayOpen,
        WR_CloseDoorFast,
        W1_OpenDoorFastClose,
        W1_OpenDoorFastStay,
        W1_CloseDoorFast,
        S1_OpenDoorFastClose,
        S1_OpenDoorFastSay,
        S1_CloseDoorFast,
        SR_OpenDoorFastClose,
        SR_OpenDoorFastStay,
        SR_CloseDoorFast,
        DR_OpenDoorFastClose,
        D1_OpenDoorFastSay,
        S1_RaiseFloorToNextHigherFLoor,
        WR_LowerLiftFastRaise,
        W1_LowerLiftFastRaise,
        S1_LowerLiftFastRaise,
        SR_LowerLiftFastRaise,
        W_EndLevelSecret,
        W1_MonsterTeleport,
        WR_MonsterTeleport,
        S1_RaiseStairsFast,
        WR_RaiseFloorToNextHigherFloor,
        WR_RaiseFloorFastToNextHigherFloor,
        W1_RaiseFloorFastToNextHigherFloor,
        S1_RaiseFloorToNextHigherFloor,
        SR_RaiseFloorFastToNextHigherFloor,
        S1_OpenBlueKeyFastStay,
        SR_OpenRedKeyFastStay,
        S1_OpenRedKeyFastStay,
        SR_OpenYellowKeyFastStay,
        S1_OpenYellowKeyFastStay,
        SR_LightOnMaxBrightness,
        SR_LightOffMinBrightness,
        S1_RaiseFloor512,
        W1_QuietCrusherCeilingFastDamage,
    }
}
