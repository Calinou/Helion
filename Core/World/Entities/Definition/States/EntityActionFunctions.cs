using System.Collections.Generic;
using NLog;

namespace Helion.World.Entities.Definition.States
{
    public static class EntityActionFunctions
    {
        public delegate void ActionFunction(Entity entity);
         
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static readonly Dictionary<string, ActionFunction> ActionFunctions = new Dictionary<string, ActionFunction>
        {
            ["ACS_NAMEDEXECUTE"] = ACS_NamedExecute,
            ["ACS_NAMEDEXECUTEALWAYS"] = ACS_NamedExecuteAlways,
            ["ACS_NAMEDEXECUTEWITHRESULT"] = ACS_NamedExecuteWithResult,
            ["ACS_NAMEDLOCKEDEXECUTE"] = ACS_NamedLockedExecute,
            ["ACS_NAMEDLOCKEDEXECUTEDOOR"] = ACS_NamedLockedExecuteDoor,
            ["ACS_NAMEDSUSPEND"] = ACS_NamedSuspend,
            ["ACS_NAMEDTERMINATE"] = ACS_NamedTerminate,
            ["A_ACTIVEANDUNBLOCK"] = A_ActiveAndUnblock,
            ["A_ACTIVESOUND"] = A_ActiveSound,
            ["A_ALERTMONSTERS"] = A_AlertMonsters,
            ["A_BFGSOUND"] = A_BFGSound,
            ["A_BFGSPRAY"] = A_BFGSpray,
            ["A_BABYMETAL"] = A_BabyMetal,
            ["A_BARRELDESTROY"] = A_BarrelDestroy,
            ["A_BASICATTACK"] = A_BasicAttack,
            ["A_BETASKULLATTACK"] = A_BetaSkullAttack,
            ["A_BISHOPMISSILEWEAVE"] = A_BishopMissileWeave,
            ["A_BOSSDEATH"] = A_BossDeath,
            ["A_BRAINAWAKE"] = A_BrainAwake,
            ["A_BRAINDIE"] = A_BrainDie,
            ["A_BRAINEXPLODE"] = A_BrainExplode,
            ["A_BRAINPAIN"] = A_BrainPain,
            ["A_BRAINSCREAM"] = A_BrainScream,
            ["A_BRAINSPIT"] = A_BrainSpit,
            ["A_BRUISATTACK"] = A_BruisAttack,
            ["A_BSPIATTACK"] = A_BspiAttack,
            ["A_BULLETATTACK"] = A_BulletAttack,
            ["A_BURST"] = A_Burst,
            ["A_CPOSATTACK"] = A_CPosAttack,
            ["A_CPOSREFIRE"] = A_CPosRefire,
            ["A_CSTAFFMISSILESLITHER"] = A_CStaffMissileSlither,
            ["A_CENTAURDEFEND"] = A_CentaurDefend,
            ["A_CHANGECOUNTFLAGS"] = A_ChangeCountFlags,
            ["A_CHANGEFLAG"] = A_ChangeFlag,
            ["A_CHANGEVELOCITY"] = A_ChangeVelocity,
            ["A_CHASE"] = A_Chase,
            ["A_CHECKBLOCK"] = A_CheckBlock,
            ["A_CHECKCEILING"] = A_CheckCeiling,
            ["A_CHECKFLAG"] = A_CheckFlag,
            ["A_CHECKFLOOR"] = A_CheckFloor,
            ["A_CHECKFORRELOAD"] = A_CheckForReload,
            ["A_CHECKFORRESURRECTION"] = A_CheckForResurrection,
            ["A_CHECKLOF"] = A_CheckLOF,
            ["A_CHECKPLAYERDONE"] = A_CheckPlayerDone,
            ["A_CHECKPROXIMITY"] = A_CheckProximity,
            ["A_CHECKRANGE"] = A_CheckRange,
            ["A_CHECKRELOAD"] = A_CheckReload,
            ["A_CHECKSIGHT"] = A_CheckSight,
            ["A_CHECKSIGHTORRANGE"] = A_CheckSightOrRange,
            ["A_CHECKSPECIES"] = A_CheckSpecies,
            ["A_CHECKTERRAIN"] = A_CheckTerrain,
            ["A_CLEARLASTHEARD"] = A_ClearLastHeard,
            ["A_CLEAROVERLAYS"] = A_ClearOverlays,
            ["A_CLEARREFIRE"] = A_ClearReFire,
            ["A_CLEARSHADOW"] = A_ClearShadow,
            ["A_CLEARSOUNDTARGET"] = A_ClearSoundTarget,
            ["A_CLEARTARGET"] = A_ClearTarget,
            ["A_CLOSESHOTGUN2"] = A_CloseShotgun2,
            ["A_COMBOATTACK"] = A_ComboAttack,
            ["A_COPYFRIENDLINESS"] = A_CopyFriendliness,
            ["A_COPYSPRITEFRAME"] = A_CopySpriteFrame,
            ["A_COUNTDOWN"] = A_Countdown,
            ["A_COUNTDOWNARG"] = A_CountdownArg,
            ["A_CUSTOMBULLETATTACK"] = A_CustomBulletAttack,
            ["A_CUSTOMCOMBOATTACK"] = A_CustomComboAttack,
            ["A_CUSTOMMELEEATTACK"] = A_CustomMeleeAttack,
            ["A_CUSTOMMISSILE"] = A_CustomMissile,
            ["A_CUSTOMPUNCH"] = A_CustomPunch,
            ["A_CUSTOMRAILGUN"] = A_CustomRailgun,
            ["A_CYBERATTACK"] = A_CyberAttack,
            ["A_DAMAGECHILDREN"] = A_DamageChildren,
            ["A_DAMAGEMASTER"] = A_DamageMaster,
            ["A_DAMAGESELF"] = A_DamageSelf,
            ["A_DAMAGESIBLINGS"] = A_DamageSiblings,
            ["A_DAMAGETARGET"] = A_DamageTarget,
            ["A_DAMAGETRACER"] = A_DamageTracer,
            ["A_DEQUEUECORPSE"] = A_DeQueueCorpse,
            ["A_DETONATE"] = A_Detonate,
            ["A_DIE"] = A_Die,
            ["A_DROPINVENTORY"] = A_DropInventory,
            ["A_DROPITEM"] = A_DropItem,
            ["A_DUALPAINATTACK"] = A_DualPainAttack,
            ["A_EXPLODE"] = A_Explode,
            ["A_EXTCHASE"] = A_ExtChase,
            ["A_FLOOPACTIVESOUND"] = A_FLoopActiveSound,
            ["A_FACEMASTER"] = A_FaceMaster,
            ["A_FACEMOVEMENTDIRECTION"] = A_FaceMovementDirection,
            ["A_FACETARGET"] = A_FaceTarget,
            ["A_FACETRACER"] = A_FaceTracer,
            ["A_FACETRACER"] = A_FaceTracer,
            ["A_FADEIN"] = A_FadeIn,
            ["A_FADEOUT"] = A_FadeOut,
            ["A_FADETO"] = A_FadeTo,
            ["A_FALL"] = A_Fall,
            ["A_FASTCHASE"] = A_FastChase,
            ["A_FATATTACK1"] = A_FatAttack1,
            ["A_FATATTACK2"] = A_FatAttack2,
            ["A_FATATTACK3"] = A_FatAttack3,
            ["A_FATRAISE"] = A_FatRaise,
            ["A_FIRE"] = A_Fire,
            ["A_FIREASSAULTGUN"] = A_FireAssaultGun,
            ["A_FIREBFG"] = A_FireBFG,
            ["A_FIREBULLETS"] = A_FireBullets,
            ["A_FIRECGUN"] = A_FireCGun,
            ["A_FIRECRACKLE"] = A_FireCrackle,
            ["A_FIRECUSTOMMISSILE"] = A_FireCustomMissile,
            ["A_FIREMISSILE"] = A_FireMissile,
            ["A_FIREOLDBFG"] = A_FireOldBFG,
            ["A_FIREPISTOL"] = A_FirePistol,
            ["A_FIREPLASMA"] = A_FirePlasma,
            ["A_FIREPROJECTILE"] = A_FireProjectile,
            ["A_FIRESTGRENADE"] = A_FireSTGrenade,
            ["A_FIRESHOTGUN"] = A_FireShotgun,
            ["A_FIRESHOTGUN2"] = A_FireShotgun2,
            ["A_FREEZEDEATH"] = A_FreezeDeath,
            ["A_FREEZEDEATHCHUNKS"] = A_FreezeDeathChunks,
            ["A_GENERICFREEZEDEATH"] = A_GenericFreezeDeath,
            ["A_GETHURT"] = A_GetHurt,
            ["A_GIVEINVENTORY"] = A_GiveInventory,
            ["A_GIVETOCHILDREN"] = A_GiveToChildren,
            ["A_GIVETOSIBLINGS"] = A_GiveToSiblings,
            ["A_GIVETOTARGET"] = A_GiveToTarget,
            ["A_GRAVITY"] = A_Gravity,
            ["A_GUNFLASH"] = A_GunFlash,
            ["A_HEADATTACK"] = A_HeadAttack,
            ["A_HIDETHING"] = A_HideThing,
            ["A_HOOF"] = A_Hoof,
            ["A_ICEGUYDIE"] = A_IceGuyDie,
            ["A_JUMP"] = A_Jump,
            ["A_JUMPIF"] = A_JumpIf,
            ["A_JUMPIFARMORTYPE"] = A_JumpIfArmorType,
            ["A_JUMPIFCLOSER"] = A_JumpIfCloser,
            ["A_JUMPIFHEALTHLOWER"] = A_JumpIfHealthLower,
            ["A_JUMPIFHIGHERORLOWER"] = A_JumpIfHigherOrLower,
            ["A_JUMPIFINTARGETINVENTORY"] = A_JumpIfInTargetInventory,
            ["A_JUMPIFINTARGETLOS"] = A_JumpIfInTargetLOS,
            ["A_JUMPIFINVENTORY"] = A_JumpIfInventory,
            ["A_JUMPIFMASTERCLOSER"] = A_JumpIfMasterCloser,
            ["A_JUMPIFNOAMMO"] = A_JumpIfNoAmmo,
            ["A_JUMPIFTARGETINLOS"] = A_JumpIfTargetInLOS,
            ["A_JUMPIFTARGETINSIDEMELEERANGE"] = A_JumpIfTargetInsideMeleeRange,
            ["A_JUMPIFTARGETOUTSIDEMELEERANGE"] = A_JumpIfTargetOutsideMeleeRange,
            ["A_JUMPIFTRACERCLOSER"] = A_JumpIfTracerCloser,
            ["A_KEENDIE"] = A_KeenDie,
            ["A_KILLCHILDREN"] = A_KillChildren,
            ["A_KILLMASTER"] = A_KillMaster,
            ["A_KILLSIBLINGS"] = A_KillSiblings,
            ["A_KILLTARGET"] = A_KillTarget,
            ["A_KILLTRACER"] = A_KillTracer,
            ["A_KLAXONBLARE"] = A_KlaxonBlare,
            ["A_LIGHT"] = A_Light,
            ["A_LIGHT0"] = A_Light0,
            ["A_LIGHT1"] = A_Light1,
            ["A_LIGHT2"] = A_Light2,
            ["A_LIGHTINVERSE"] = A_LightInverse,
            ["A_LOADSHOTGUN2"] = A_LoadShotgun2,
            ["A_LOG"] = A_Log,
            ["A_LOGFLOAT"] = A_LogFloat,
            ["A_LOGINT"] = A_LogInt,
            ["A_LOOK"] = A_Look,
            ["A_LOOK2"] = A_Look2,
            ["A_LOOKEX"] = A_LookEx,
            ["A_LOOPACTIVESOUND"] = A_LoopActiveSound,
            ["A_LOWGRAVITY"] = A_LowGravity,
            ["A_LOWER"] = A_Lower,
            ["A_M_SAW"] = A_M_Saw,
            ["A_MELEEATTACK"] = A_MeleeAttack,
            ["A_METAL"] = A_Metal,
            ["A_MISSILEATTACK"] = A_MissileAttack,
            ["A_MONSTERRAIL"] = A_MonsterRail,
            ["A_MONSTERREFIRE"] = A_MonsterRefire,
            ["A_MORPH"] = A_Morph,
            ["A_MUSHROOM"] = A_Mushroom,
            ["A_NOBLOCKING"] = A_NoBlocking,
            ["A_NOGRAVITY"] = A_NoGravity,
            ["A_OPENSHOTGUN2"] = A_OpenShotgun2,
            ["A_OVERLAY"] = A_Overlay,
            ["A_OVERLAYALPHA"] = A_OverlayAlpha,
            ["A_OVERLAYFLAGS"] = A_OverlayFlags,
            ["A_OVERLAYOFFSET"] = A_OverlayOffset,
            ["A_OVERLAYRENDERSTYLE"] = A_OverlayRenderstyle,
            ["A_PAIN"] = A_Pain,
            ["A_PAINATTACK"] = A_PainAttack,
            ["A_PAINDIE"] = A_PainDie,
            ["A_PLAYSOUND"] = A_PlaySound,
            ["A_PLAYSOUNDEX"] = A_PlaySoundEx,
            ["A_PLAYWEAPONSOUND"] = A_PlayWeaponSound,
            ["A_PLAYERSCREAM"] = A_PlayerScream,
            ["A_PLAYERSKINCHECK"] = A_PlayerSkinCheck,
            ["A_POSATTACK"] = A_PosAttack,
            ["A_PRINT"] = A_Print,
            ["A_PRINTBOLD"] = A_PrintBold,
            ["A_PUNCH"] = A_Punch,
            ["A_QUAKE"] = A_Quake,
            ["A_QUAKEEX"] = A_QuakeEx,
            ["A_QUEUECORPSE"] = A_QueueCorpse,
            ["A_RADIUSDAMAGESELF"] = A_RadiusDamageSelf,
            ["A_RADIUSGIVE"] = A_RadiusGive,
            ["A_RADIUSTHRUST"] = A_RadiusThrust,
            ["A_RAILATTACK"] = A_RailAttack,
            ["A_RAISE"] = A_Raise,
            ["A_RAISECHILDREN"] = A_RaiseChildren,
            ["A_RAISEMASTER"] = A_RaiseMaster,
            ["A_RAISESELF"] = A_RaiseSelf,
            ["A_RAISESIBLINGS"] = A_RaiseSiblings,
            ["A_REFIRE"] = A_ReFire,
            ["A_REARRANGEPOINTERS"] = A_RearrangePointers,
            ["A_RECOIL"] = A_Recoil,
            ["A_REMOVE"] = A_Remove,
            ["A_REMOVECHILDREN"] = A_RemoveChildren,
            ["A_REMOVEMASTER"] = A_RemoveMaster,
            ["A_REMOVESIBLINGS"] = A_RemoveSiblings,
            ["A_REMOVETARGET"] = A_RemoveTarget,
            ["A_REMOVETRACER"] = A_RemoveTracer,
            ["A_RESETHEALTH"] = A_ResetHealth,
            ["A_RESETRELOADCOUNTER"] = A_ResetReloadCounter,
            ["A_RESPAWN"] = A_Respawn,
            ["A_SPOSATTACK"] = A_SPosAttack,
            ["A_SARGATTACK"] = A_SargAttack,
            ["A_SAW"] = A_Saw,
            ["A_SCALEVELOCITY"] = A_ScaleVelocity,
            ["A_SCREAM"] = A_Scream,
            ["A_SCREAMANDUNBLOCK"] = A_ScreamAndUnblock,
            ["A_SEEKERMISSILE"] = A_SeekerMissile,
            ["A_SELECTWEAPON"] = A_SelectWeapon,
            ["A_SENTINELBOB"] = A_SentinelBob,
            ["A_SENTINELREFIRE"] = A_SentinelRefire,
            ["A_SETANGLE"] = A_SetAngle,
            ["A_SETARG"] = A_SetArg,
            ["A_SETBLEND"] = A_SetBlend,
            ["A_SETCHASETHRESHOLD"] = A_SetChaseThreshold,
            ["A_SETCROSSHAIR"] = A_SetCrosshair,
            ["A_SETDAMAGETYPE"] = A_SetDamageType,
            ["A_SETFLOAT"] = A_SetFloat,
            ["A_SETFLOATBOBPHASE"] = A_SetFloatBobPhase,
            ["A_SETFLOATSPEED"] = A_SetFloatSpeed,
            ["A_SETFLOORCLIP"] = A_SetFloorClip,
            ["A_SETGRAVITY"] = A_SetGravity,
            ["A_SETHEALTH"] = A_SetHealth,
            ["A_SETINVENTORY"] = A_SetInventory,
            ["A_SETINVULNERABLE"] = A_SetInvulnerable,
            ["A_SETMASS"] = A_SetMass,
            ["A_SETMUGSHOTSTATE"] = A_SetMugshotState,
            ["A_SETPAINTHRESHOLD"] = A_SetPainThreshold,
            ["A_SETPITCH"] = A_SetPitch,
            ["A_SETREFLECTIVE"] = A_SetReflective,
            ["A_SETREFLECTIVEINVULNERABLE"] = A_SetReflectiveInvulnerable,
            ["A_SETRENDERSTYLE"] = A_SetRenderStyle,
            ["A_SETRIPMAX"] = A_SetRipMax,
            ["A_SETRIPMIN"] = A_SetRipMin,
            ["A_SETRIPPERLEVEL"] = A_SetRipperLevel,
            ["A_SETROLL"] = A_SetRoll,
            ["A_SETSCALE"] = A_SetScale,
            ["A_SETSHADOW"] = A_SetShadow,
            ["A_SETSHOOTABLE"] = A_SetShootable,
            ["A_SETSIZE"] = A_SetSize,
            ["A_SETSOLID"] = A_SetSolid,
            ["A_SETSPECIAL"] = A_SetSpecial,
            ["A_SETSPECIES"] = A_SetSpecies,
            ["A_SETSPEED"] = A_SetSpeed,
            ["A_SETSPRITEANGLE"] = A_SetSpriteAngle,
            ["A_SETSPRITEROTATION"] = A_SetSpriteRotation,
            ["A_SETTELEFOG"] = A_SetTeleFog,
            ["A_SETTICS"] = A_SetTics,
            ["A_SETTRANSLATION"] = A_SetTranslation,
            ["A_SETTRANSLUCENT"] = A_SetTranslucent,
            ["A_SETUSERARRAY"] = A_SetUserArray,
            ["A_SETUSERARRAYFLOAT"] = A_SetUserArrayFloat,
            ["A_SETUSERVAR"] = A_SetUserVar,
            ["A_SETUSERVARFLOAT"] = A_SetUserVarFloat,
            ["A_SETVISIBLEROTATION"] = A_SetVisibleRotation,
            ["A_SKELFIST"] = A_SkelFist,
            ["A_SKELMISSILE"] = A_SkelMissile,
            ["A_SKELWHOOSH"] = A_SkelWhoosh,
            ["A_SKULLATTACK"] = A_SkullAttack,
            ["A_SKULLPOP"] = A_SkullPop,
            ["A_SOUNDPITCH"] = A_SoundPitch,
            ["A_SOUNDVOLUME"] = A_SoundVolume,
            ["A_SPAWNDEBRIS"] = A_SpawnDebris,
            ["A_SPAWNFLY"] = A_SpawnFly,
            ["A_SPAWNITEM"] = A_SpawnItem,
            ["A_SPAWNITEMEX"] = A_SpawnItemEx,
            ["A_SPAWNPARTICLE"] = A_SpawnParticle,
            ["A_SPAWNPROJECTILE"] = A_SpawnProjectile,
            ["A_SPAWNSOUND"] = A_SpawnSound,
            ["A_SPIDREFIRE"] = A_SpidRefire,
            ["A_SPOSATTACKUSEATKSOUND"] = A_SPosAttackUseAtkSound,
            ["A_SPRAYDECAL"] = A_SprayDecal,
            ["A_STARTFIRE"] = A_StartFire,
            ["A_STOP"] = A_Stop,
            ["A_STOPSOUND"] = A_StopSound,
            ["A_STOPSOUNDEX"] = A_StopSoundEx,
            ["A_SWAPTELEFOG"] = A_SwapTeleFog,
            ["A_TAKEFROMCHILDREN"] = A_TakeFromChildren,
            ["A_TAKEFROMSIBLINGS"] = A_TakeFromSiblings,
            ["A_TAKEFROMTARGET"] = A_TakeFromTarget,
            ["A_TAKEINVENTORY"] = A_TakeInventory,
            ["A_TELEPORT"] = A_Teleport,
            ["A_THROWGRENADE"] = A_ThrowGrenade,
            ["A_TOSSGIB"] = A_TossGib,
            ["A_TRACER"] = A_Tracer,
            ["A_TRACER2"] = A_Tracer2,
            ["A_TRANSFERPOINTER"] = A_TransferPointer,
            ["A_TROOPATTACK"] = A_TroopAttack,
            ["A_TURRETLOOK"] = A_TurretLook,
            ["A_UNHIDETHING"] = A_UnHideThing,
            ["A_UNSETFLOORCLIP"] = A_UnSetFloorClip,
            ["A_UNSETINVULNERABLE"] = A_UnSetInvulnerable,
            ["A_UNSETREFLECTIVE"] = A_UnSetReflective,
            ["A_UNSETREFLECTIVEINVULNERABLE"] = A_UnSetReflectiveInvulnerable,
            ["A_UNSETSHOOTABLE"] = A_UnSetShootable,
            ["A_UNSETFLOAT"] = A_UnsetFloat,
            ["A_UNSETSOLID"] = A_UnsetSolid,
            ["A_VILEATTACK"] = A_VileAttack,
            ["A_VILECHASE"] = A_VileChase,
            ["A_VILESTART"] = A_VileStart,
            ["A_VILETARGET"] = A_VileTarget,
            ["A_WANDER"] = A_Wander,
            ["A_WARP"] = A_Warp,
            ["A_WEAPONOFFSET"] = A_WeaponOffset,
            ["A_WEAPONREADY"] = A_WeaponReady,
            ["A_WEAVE"] = A_Weave,
            ["A_WOLFATTACK"] = A_WolfAttack,
            ["A_XSCREAM"] = A_XScream,
            ["A_ZOOMFACTOR"] = A_ZoomFactor,
        };
        
        public static ActionFunction? Find(string? actionFuncName)
        {
             if (actionFuncName != null)
             {
                  if (ActionFunctions.TryGetValue(actionFuncName.ToUpper(), out ActionFunction? func)) 
                       return func;
                  Log.Warn("Unable to find action function: {0}", actionFuncName);
             }
                  
             return null;
        }
        
        private static void ACS_NamedExecute(Entity entity)
        {
             // TODO
        }

        private static void ACS_NamedExecuteAlways(Entity entity)
        {
             // TODO
        }

        private static void ACS_NamedExecuteWithResult(Entity entity)
        {
             // TODO
        }

        private static void ACS_NamedLockedExecute(Entity entity)
        {
             // TODO
        }

        private static void ACS_NamedLockedExecuteDoor(Entity entity)
        {
             // TODO
        }

        private static void ACS_NamedSuspend(Entity entity)
        {
             // TODO
        }

        private static void ACS_NamedTerminate(Entity entity)
        {
             // TODO
        }

        private static void A_ActiveAndUnblock(Entity entity)
        {
             // TODO
        }

        private static void A_ActiveSound(Entity entity)
        {
             // TODO
        }

        private static void A_AlertMonsters(Entity entity)
        {
             // TODO
        }

        private static void A_BFGSound(Entity entity)
        {
             // TODO
        }

        private static void A_BFGSpray(Entity entity)
        {
             // TODO
        }

        private static void A_BabyMetal(Entity entity)
        {
             // TODO
        }

        private static void A_BarrelDestroy(Entity entity)
        {
             // TODO
        }

        private static void A_BasicAttack(Entity entity)
        {
             // TODO
        }

        private static void A_BetaSkullAttack(Entity entity)
        {
             // TODO
        }

        private static void A_BishopMissileWeave(Entity entity)
        {
             // TODO
        }

        private static void A_BossDeath(Entity entity)
        {
             // TODO
        }

        private static void A_BrainAwake(Entity entity)
        {
             // TODO
        }

        private static void A_BrainDie(Entity entity)
        {
             // TODO
        }

        private static void A_BrainExplode(Entity entity)
        {
             // TODO
        }

        private static void A_BrainPain(Entity entity)
        {
             // TODO
        }

        private static void A_BrainScream(Entity entity)
        {
             // TODO
        }

        private static void A_BrainSpit(Entity entity)
        {
             // TODO
        }

        private static void A_BruisAttack(Entity entity)
        {
             // TODO
        }

        private static void A_BspiAttack(Entity entity)
        {
             // TODO
        }

        private static void A_BulletAttack(Entity entity)
        {
             // TODO
        }

        private static void A_Burst(Entity entity)
        {
             // TODO
        }

        private static void A_CPosAttack(Entity entity)
        {
             // TODO
        }

        private static void A_CPosRefire(Entity entity)
        {
             // TODO
        }

        private static void A_CStaffMissileSlither(Entity entity)
        {
             // TODO
        }

        private static void A_CentaurDefend(Entity entity)
        {
             // TODO
        }

        private static void A_ChangeCountFlags(Entity entity)
        {
             // TODO
        }

        private static void A_ChangeFlag(Entity entity)
        {
             // TODO
        }

        private static void A_ChangeVelocity(Entity entity)
        {
             // TODO
        }

        private static void A_Chase(Entity entity)
        {
             // TODO
        }

        private static void A_CheckBlock(Entity entity)
        {
             // TODO
        }

        private static void A_CheckCeiling(Entity entity)
        {
             // TODO
        }

        private static void A_CheckFlag(Entity entity)
        {
             // TODO
        }

        private static void A_CheckFloor(Entity entity)
        {
             // TODO
        }

        private static void A_CheckForReload(Entity entity)
        {
             // TODO
        }

        private static void A_CheckForResurrection(Entity entity)
        {
             // TODO
        }

        private static void A_CheckLOF(Entity entity)
        {
             // TODO
        }

        private static void A_CheckPlayerDone(Entity entity)
        {
             // TODO
        }

        private static void A_CheckProximity(Entity entity)
        {
             // TODO
        }

        private static void A_CheckRange(Entity entity)
        {
             // TODO
        }

        private static void A_CheckReload(Entity entity)
        {
             // TODO
        }

        private static void A_CheckSight(Entity entity)
        {
             // TODO
        }

        private static void A_CheckSightOrRange(Entity entity)
        {
             // TODO
        }

        private static void A_CheckSpecies(Entity entity)
        {
             // TODO
        }

        private static void A_CheckTerrain(Entity entity)
        {
             // TODO
        }

        private static void A_ClearLastHeard(Entity entity)
        {
             // TODO
        }

        private static void A_ClearOverlays(Entity entity)
        {
             // TODO
        }

        private static void A_ClearReFire(Entity entity)
        {
             // TODO
        }

        private static void A_ClearShadow(Entity entity)
        {
             // TODO
        }

        private static void A_ClearSoundTarget(Entity entity)
        {
             // TODO
        }

        private static void A_ClearTarget(Entity entity)
        {
             // TODO
        }
        
        private static void A_CloseShotgun2(Entity entity)
        {
             // TODO
        }

        private static void A_ComboAttack(Entity entity)
        {
             // TODO
        }

        private static void A_CopyFriendliness(Entity entity)
        {
             // TODO
        }

        private static void A_CopySpriteFrame(Entity entity)
        {
             // TODO
        }

        private static void A_Countdown(Entity entity)
        {
             // TODO
        }

        private static void A_CountdownArg(Entity entity)
        {
             // TODO
        }

        private static void A_CustomBulletAttack(Entity entity)
        {
             // TODO
        }

        private static void A_CustomComboAttack(Entity entity)
        {
             // TODO
        }

        private static void A_CustomMeleeAttack(Entity entity)
        {
             // TODO
        }

        private static void A_CustomMissile(Entity entity)
        {
             // TODO
        }

        private static void A_CustomPunch(Entity entity)
        {
             // TODO
        }

        private static void A_CustomRailgun(Entity entity)
        {
             // TODO
        }

        private static void A_CyberAttack(Entity entity)
        {
             // TODO
        }

        private static void A_DamageChildren(Entity entity)
        {
             // TODO
        }

        private static void A_DamageMaster(Entity entity)
        {
             // TODO
        }

        private static void A_DamageSelf(Entity entity)
        {
             // TODO
        }

        private static void A_DamageSiblings(Entity entity)
        {
             // TODO
        }

        private static void A_DamageTarget(Entity entity)
        {
             // TODO
        }

        private static void A_DamageTracer(Entity entity)
        {
             // TODO
        }

        private static void A_DeQueueCorpse(Entity entity)
        {
             // TODO
        }

        private static void A_Detonate(Entity entity)
        {
             // TODO
        }

        private static void A_Die(Entity entity)
        {
             // TODO
        }

        private static void A_DropInventory(Entity entity)
        {
             // TODO
        }

        private static void A_DropItem(Entity entity)
        {
             // TODO
        }

        private static void A_DualPainAttack(Entity entity)
        {
             // TODO
        }

        private static void A_Explode(Entity entity)
        {
            entity.EntityManager.World.PhysicsManager.RadiusExplosion(entity, 128);
        }

        private static void A_ExtChase(Entity entity)
        {
             // TODO
        }

        private static void A_FLoopActiveSound(Entity entity)
        {
             // TODO
        }

        private static void A_FaceMaster(Entity entity)
        {
             // TODO
        }

        private static void A_FaceMovementDirection(Entity entity)
        {
             // TODO
        }

        private static void A_FaceTarget(Entity entity)
        {
             // TODO
        }

        private static void A_FaceTracer(Entity entity)
        {
             // TODO
        }

        private static void A_FadeIn(Entity entity)
        {
             // TODO
        }

        private static void A_FadeOut(Entity entity)
        {
             // TODO
        }

        private static void A_FadeTo(Entity entity)
        {
             // TODO
        }

        private static void A_Fall(Entity entity)
        {
             // TODO
        }

        private static void A_FastChase(Entity entity)
        {
             // TODO
        }

        private static void A_FatAttack1(Entity entity)
        {
             // TODO
        }

        private static void A_FatAttack2(Entity entity)
        {
             // TODO
        }

        private static void A_FatAttack3(Entity entity)
        {
             // TODO
        }

        private static void A_FatRaise(Entity entity)
        {
             // TODO
        }

        private static void A_Fire(Entity entity)
        {
             // TODO
        }

        private static void A_FireAssaultGun(Entity entity)
        {
             // TODO
        }

        private static void A_FireBFG(Entity entity)
        {
             // TODO
        }

        private static void A_FireBullets(Entity entity)
        {
             // TODO
        }

        private static void A_FireCGun(Entity entity)
        {
             // TODO
        }

        private static void A_FireCrackle(Entity entity)
        {
             // TODO
        }

        private static void A_FireCustomMissile(Entity entity)
        {
             // TODO
        }

        private static void A_FireMissile(Entity entity)
        {
             // TODO
        }

        private static void A_FireOldBFG(Entity entity)
        {
             // TODO
        }

        private static void A_FirePistol(Entity entity)
        {
             // TODO
        }

        private static void A_FirePlasma(Entity entity)
        {
             // TODO
        }

        private static void A_FireProjectile(Entity entity)
        {
             // TODO
        }

        private static void A_FireSTGrenade(Entity entity)
        {
             // TODO
        }

        private static void A_FireShotgun(Entity entity)
        {
             // TODO
        }

        private static void A_FireShotgun2(Entity entity)
        {
             // TODO
        }

        private static void A_FreezeDeath(Entity entity)
        {
             // TODO
        }

        private static void A_FreezeDeathChunks(Entity entity)
        {
             // TODO
        }

        private static void A_GenericFreezeDeath(Entity entity)
        {
             // TODO
        }

        private static void A_GetHurt(Entity entity)
        {
             // TODO
        }

        private static void A_GiveInventory(Entity entity)
        {
             // TODO
        }

        private static void A_GiveToChildren(Entity entity)
        {
             // TODO
        }

        private static void A_GiveToSiblings(Entity entity)
        {
             // TODO
        }

        private static void A_GiveToTarget(Entity entity)
        {
             // TODO
        }

        private static void A_Gravity(Entity entity)
        {
             // TODO
        }

        private static void A_GunFlash(Entity entity)
        {
             // TODO
        }

        private static void A_HeadAttack(Entity entity)
        {
             // TODO
        }

        private static void A_HideThing(Entity entity)
        {
             // TODO
        }

        private static void A_Hoof(Entity entity)
        {
             // TODO
        }

        private static void A_IceGuyDie(Entity entity)
        {
             // TODO
        }

        private static void A_Jump(Entity entity)
        {
             // TODO
        }

        private static void A_JumpIf(Entity entity)
        {
             // TODO
        }

        private static void A_JumpIfArmorType(Entity entity)
        {
             // TODO
        }

        private static void A_JumpIfCloser(Entity entity)
        {
             // TODO
        }

        private static void A_JumpIfHealthLower(Entity entity)
        {
             // TODO
        }

        private static void A_JumpIfHigherOrLower(Entity entity)
        {
             // TODO
        }

        private static void A_JumpIfInTargetInventory(Entity entity)
        {
             // TODO
        }

        private static void A_JumpIfInTargetLOS(Entity entity)
        {
             // TODO
        }

        private static void A_JumpIfInventory(Entity entity)
        {
             // TODO
        }

        private static void A_JumpIfMasterCloser(Entity entity)
        {
             // TODO
        }

        private static void A_JumpIfNoAmmo(Entity entity)
        {
             // TODO
        }

        private static void A_JumpIfTargetInLOS(Entity entity)
        {
             // TODO
        }

        private static void A_JumpIfTargetInsideMeleeRange(Entity entity)
        {
             // TODO
        }

        private static void A_JumpIfTargetOutsideMeleeRange(Entity entity)
        {
             // TODO
        }

        private static void A_JumpIfTracerCloser(Entity entity)
        {
             // TODO
        }

        private static void A_KeenDie(Entity entity)
        {
             // TODO
        }

        private static void A_KillChildren(Entity entity)
        {
             // TODO
        }

        private static void A_KillMaster(Entity entity)
        {
             // TODO
        }

        private static void A_KillSiblings(Entity entity)
        {
             // TODO
        }

        private static void A_KillTarget(Entity entity)
        {
             // TODO
        }

        private static void A_KillTracer(Entity entity)
        {
             // TODO
        }

        private static void A_KlaxonBlare(Entity entity)
        {
             // TODO
        }

        private static void A_Light(Entity entity)
        {
             // TODO
        }

        private static void A_Light0(Entity entity)
        {
             // TODO
        }

        private static void A_Light1(Entity entity)
        {
             // TODO
        }

        private static void A_Light2(Entity entity)
        {
             // TODO
        }

        private static void A_LightInverse(Entity entity)
        {
             // TODO
        }
        
        private static void A_LoadShotgun2(Entity entity)
        {
             // TODO
        }
        
        private static void A_Log(Entity entity)
        {
             // TODO
        }

        private static void A_LogFloat(Entity entity)
        {
             // TODO
        }

        private static void A_LogInt(Entity entity)
        {
             // TODO
        }

        private static void A_Look(Entity entity)
        {
             // TODO
        }

        private static void A_Look2(Entity entity)
        {
             // TODO
        }

        private static void A_LookEx(Entity entity)
        {
             // TODO
        }

        private static void A_LoopActiveSound(Entity entity)
        {
             // TODO
        }

        private static void A_LowGravity(Entity entity)
        {
             // TODO
        }

        private static void A_Lower(Entity entity)
        {
             // TODO
        }

        private static void A_M_Saw(Entity entity)
        {
             // TODO
        }

        private static void A_MeleeAttack(Entity entity)
        {
             // TODO
        }

        private static void A_Metal(Entity entity)
        {
             // TODO
        }

        private static void A_MissileAttack(Entity entity)
        {
             // TODO
        }

        private static void A_MonsterRail(Entity entity)
        {
             // TODO
        }

        private static void A_MonsterRefire(Entity entity)
        {
             // TODO
        }

        private static void A_Morph(Entity entity)
        {
             // TODO
        }

        private static void A_Mushroom(Entity entity)
        {
             // TODO
        }

        private static void A_NoBlocking(Entity entity)
        {
             // TODO
        }

        private static void A_NoGravity(Entity entity)
        {
             // TODO
        }
        
        private static void A_OpenShotgun2(Entity entity)
        {
             // TODO
        }

        private static void A_Overlay(Entity entity)
        {
             // TODO
        }

        private static void A_OverlayAlpha(Entity entity)
        {
             // TODO
        }

        private static void A_OverlayFlags(Entity entity)
        {
             // TODO
        }

        private static void A_OverlayOffset(Entity entity)
        {
             // TODO
        }

        private static void A_OverlayRenderstyle(Entity entity)
        {
             // TODO
        }

        private static void A_Pain(Entity entity)
        {
             // TODO
        }

        private static void A_PainAttack(Entity entity)
        {
             // TODO
        }

        private static void A_PainDie(Entity entity)
        {
             // TODO
        }

        private static void A_PlaySound(Entity entity)
        {
             // TODO
        }

        private static void A_PlaySoundEx(Entity entity)
        {
             // TODO
        }

        private static void A_PlayWeaponSound(Entity entity)
        {
             // TODO
        }

        private static void A_PlayerScream(Entity entity)
        {
             // TODO
        }

        private static void A_PlayerSkinCheck(Entity entity)
        {
             // TODO
        }

        private static void A_PosAttack(Entity entity)
        {
             // TODO
        }

        private static void A_Print(Entity entity)
        {
             // TODO
        }

        private static void A_PrintBold(Entity entity)
        {
             // TODO
        }

        private static void A_Punch(Entity entity)
        {
             // TODO
        }

        private static void A_Quake(Entity entity)
        {
             // TODO
        }

        private static void A_QuakeEx(Entity entity)
        {
             // TODO
        }

        private static void A_QueueCorpse(Entity entity)
        {
             // TODO
        }

        private static void A_RadiusDamageSelf(Entity entity)
        {
             // TODO
        }

        private static void A_RadiusGive(Entity entity)
        {
             // TODO
        }

        private static void A_RadiusThrust(Entity entity)
        {
             // TODO
        }

        private static void A_RailAttack(Entity entity)
        {
             // TODO
        }

        private static void A_Raise(Entity entity)
        {
             // TODO
        }

        private static void A_RaiseChildren(Entity entity)
        {
             // TODO
        }

        private static void A_RaiseMaster(Entity entity)
        {
             // TODO
        }

        private static void A_RaiseSelf(Entity entity)
        {
             // TODO
        }

        private static void A_RaiseSiblings(Entity entity)
        {
             // TODO
        }

        private static void A_ReFire(Entity entity)
        {
             // TODO
        }

        private static void A_RearrangePointers(Entity entity)
        {
             // TODO
        }

        private static void A_Recoil(Entity entity)
        {
             // TODO
        }

        private static void A_Remove(Entity entity)
        {
             // TODO
        }

        private static void A_RemoveChildren(Entity entity)
        {
             // TODO
        }

        private static void A_RemoveMaster(Entity entity)
        {
             // TODO
        }

        private static void A_RemoveSiblings(Entity entity)
        {
             // TODO
        }

        private static void A_RemoveTarget(Entity entity)
        {
             // TODO
        }

        private static void A_RemoveTracer(Entity entity)
        {
             // TODO
        }

        private static void A_ResetHealth(Entity entity)
        {
             // TODO
        }

        private static void A_ResetReloadCounter(Entity entity)
        {
             // TODO
        }

        private static void A_Respawn(Entity entity)
        {
             // TODO
        }

        private static void A_SPosAttack(Entity entity)
        {
             // TODO
        }

        private static void A_SargAttack(Entity entity)
        {
             // TODO
        }

        private static void A_Saw(Entity entity)
        {
             // TODO
        }

        private static void A_ScaleVelocity(Entity entity)
        {
             // TODO
        }

        private static void A_Scream(Entity entity)
        {
             // TODO
        }

        private static void A_ScreamAndUnblock(Entity entity)
        {
             // TODO
        }

        private static void A_SeekerMissile(Entity entity)
        {
             // TODO
        }

        private static void A_SelectWeapon(Entity entity)
        {
             // TODO
        }

        private static void A_SentinelBob(Entity entity)
        {
             // TODO
        }

        private static void A_SentinelRefire(Entity entity)
        {
             // TODO
        }

        private static void A_SetAngle(Entity entity)
        {
             // TODO
        }

        private static void A_SetArg(Entity entity)
        {
             // TODO
        }

        private static void A_SetBlend(Entity entity)
        {
             // TODO
        }

        private static void A_SetChaseThreshold(Entity entity)
        {
             // TODO
        }

        private static void A_SetCrosshair(Entity entity)
        {
             // TODO
        }

        private static void A_SetDamageType(Entity entity)
        {
             // TODO
        }

        private static void A_SetFloat(Entity entity)
        {
             // TODO
        }

        private static void A_SetFloatBobPhase(Entity entity)
        {
             // TODO
        }

        private static void A_SetFloatSpeed(Entity entity)
        {
             // TODO
        }

        private static void A_SetFloorClip(Entity entity)
        {
             // TODO
        }

        private static void A_SetGravity(Entity entity)
        {
             // TODO
        }

        private static void A_SetHealth(Entity entity)
        {
             // TODO
        }

        private static void A_SetInventory(Entity entity)
        {
             // TODO
        }

        private static void A_SetInvulnerable(Entity entity)
        {
             // TODO
        }

        private static void A_SetMass(Entity entity)
        {
             // TODO
        }

        private static void A_SetMugshotState(Entity entity)
        {
             // TODO
        }

        private static void A_SetPainThreshold(Entity entity)
        {
             // TODO
        }

        private static void A_SetPitch(Entity entity)
        {
             // TODO
        }

        private static void A_SetReflective(Entity entity)
        {
             // TODO
        }

        private static void A_SetReflectiveInvulnerable(Entity entity)
        {
             // TODO
        }

        private static void A_SetRenderStyle(Entity entity)
        {
             // TODO
        }

        private static void A_SetRipMax(Entity entity)
        {
             // TODO
        }

        private static void A_SetRipMin(Entity entity)
        {
             // TODO
        }

        private static void A_SetRipperLevel(Entity entity)
        {
             // TODO
        }

        private static void A_SetRoll(Entity entity)
        {
             // TODO
        }

        private static void A_SetScale(Entity entity)
        {
             // TODO
        }

        private static void A_SetShadow(Entity entity)
        {
             // TODO
        }

        private static void A_SetShootable(Entity entity)
        {
             // TODO
        }

        private static void A_SetSize(Entity entity)
        {
             // TODO
        }

        private static void A_SetSolid(Entity entity)
        {
             // TODO
        }

        private static void A_SetSpecial(Entity entity)
        {
             // TODO
        }

        private static void A_SetSpecies(Entity entity)
        {
             // TODO
        }

        private static void A_SetSpeed(Entity entity)
        {
             // TODO
        }

        private static void A_SetSpriteAngle(Entity entity)
        {
             // TODO
        }

        private static void A_SetSpriteRotation(Entity entity)
        {
             // TODO
        }

        private static void A_SetTeleFog(Entity entity)
        {
             // TODO
        }

        private static void A_SetTics(Entity entity)
        {
             // TODO
        }

        private static void A_SetTranslation(Entity entity)
        {
             // TODO
        }

        private static void A_SetTranslucent(Entity entity)
        {
             // TODO
        }

        private static void A_SetUserArray(Entity entity)
        {
             // TODO
        }

        private static void A_SetUserArrayFloat(Entity entity)
        {
             // TODO
        }

        private static void A_SetUserVar(Entity entity)
        {
             // TODO
        }

        private static void A_SetUserVarFloat(Entity entity)
        {
             // TODO
        }

        private static void A_SetVisibleRotation(Entity entity)
        {
             // TODO
        }

        private static void A_SkelFist(Entity entity)
        {
             // TODO
        }

        private static void A_SkelMissile(Entity entity)
        {
             // TODO
        }

        private static void A_SkelWhoosh(Entity entity)
        {
             // TODO
        }

        private static void A_SkullAttack(Entity entity)
        {
             // TODO
        }

        private static void A_SkullPop(Entity entity)
        {
             // TODO
        }

        private static void A_SoundPitch(Entity entity)
        {
             // TODO
        }

        private static void A_SoundVolume(Entity entity)
        {
             // TODO
        }

        private static void A_SpawnDebris(Entity entity)
        {
             // TODO
        }

        private static void A_SpawnFly(Entity entity)
        {
             // TODO
        }

        private static void A_SpawnItem(Entity entity)
        {
             // TODO
        }

        private static void A_SpawnItemEx(Entity entity)
        {
             // TODO
        }

        private static void A_SpawnParticle(Entity entity)
        {
             // TODO
        }

        private static void A_SpawnProjectile(Entity entity)
        {
             // TODO
        }

        private static void A_SpawnSound(Entity entity)
        {
             // TODO
        }

        private static void A_SpidRefire(Entity entity)
        {
             // TODO
        }
        
        private static void A_SPosAttackUseAtkSound(Entity entity)
        {
             // TODO
        }

        private static void A_SprayDecal(Entity entity)
        {
             // TODO
        }

        private static void A_StartFire(Entity entity)
        {
             // TODO
        }

        private static void A_Stop(Entity entity)
        {
             // TODO
        }

        private static void A_StopSound(Entity entity)
        {
             // TODO
        }

        private static void A_StopSoundEx(Entity entity)
        {
             // TODO
        }

        private static void A_SwapTeleFog(Entity entity)
        {
             // TODO
        }

        private static void A_TakeFromChildren(Entity entity)
        {
             // TODO
        }

        private static void A_TakeFromSiblings(Entity entity)
        {
             // TODO
        }

        private static void A_TakeFromTarget(Entity entity)
        {
             // TODO
        }

        private static void A_TakeInventory(Entity entity)
        {
             // TODO
        }

        private static void A_Teleport(Entity entity)
        {
             // TODO
        }

        private static void A_ThrowGrenade(Entity entity)
        {
             // TODO
        }

        private static void A_TossGib(Entity entity)
        {
             // TODO
        }

        private static void A_Tracer(Entity entity)
        {
             // TODO
        }

        private static void A_Tracer2(Entity entity)
        {
             // TODO
        }

        private static void A_TransferPointer(Entity entity)
        {
             // TODO
        }

        private static void A_TroopAttack(Entity entity)
        {
             // TODO
        }

        private static void A_TurretLook(Entity entity)
        {
             // TODO
        }

        private static void A_UnHideThing(Entity entity)
        {
             // TODO
        }

        private static void A_UnSetFloorClip(Entity entity)
        {
             // TODO
        }

        private static void A_UnSetInvulnerable(Entity entity)
        {
             // TODO
        }

        private static void A_UnSetReflective(Entity entity)
        {
             // TODO
        }

        private static void A_UnSetReflectiveInvulnerable(Entity entity)
        {
             // TODO
        }

        private static void A_UnSetShootable(Entity entity)
        {
             // TODO
        }

        private static void A_UnsetFloat(Entity entity)
        {
             // TODO
        }

        private static void A_UnsetSolid(Entity entity)
        {
             // TODO
        }

        private static void A_VileAttack(Entity entity)
        {
             // TODO
        }

        private static void A_VileChase(Entity entity)
        {
             // TODO
        }

        private static void A_VileStart(Entity entity)
        {
             // TODO
        }

        private static void A_VileTarget(Entity entity)
        {
             // TODO
        }

        private static void A_Wander(Entity entity)
        {
             // TODO
        }

        private static void A_Warp(Entity entity)
        {
             // TODO
        }

        private static void A_WeaponOffset(Entity entity)
        {
             // TODO
        }

        private static void A_WeaponReady(Entity entity)
        {
             // TODO
        }

        private static void A_Weave(Entity entity)
        {
             // TODO
        }

        private static void A_WolfAttack(Entity entity)
        {
             // TODO
        }

        private static void A_XScream(Entity entity)
        {
             // TODO
        }

        private static void A_ZoomFactor(Entity entity)
        {
             // TODO
        }
    }
}