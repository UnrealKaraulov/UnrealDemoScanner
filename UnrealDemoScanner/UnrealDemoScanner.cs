using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
#if !NET6_0_OR_GREATER
using System.Windows.Forms;
#endif
using ConsoleTables;
using DemoScanner.DemoStuff;
using DemoScanner.DemoStuff.GoldSource;
using static DemoScanner.DG.DemoScanner;

namespace DemoScanner.DG
{
    public static class DemoScanner
    {
        public const string PROGRAMNAME = "Unreal Demo Scanner";
        public const string PROGRAMVERSION = "1.73.6b";

        public static bool DEMOSCANNER_HLTV = false;

        public enum AngleDirection
        {
            AngleDirectionLeft = -1,
            AngleDirectionRight = 1,
            AngleDirectionNO = 0
        }

        public enum TEXTMSG_Type
        {
            TEXT_NONE = 0,
            TEXT_PRINTNOTIFY = 1,
            TEXT_PRINTCONSOLE = 2,
            TEXT_PRINTTALK = 3,
            TEXT_PRINTCENTER = 4,
            TEXT_PRINTRADIO = 5
        }

        public enum WeaponIdType
        {
            WEAPON_BAD = -1,
            WEAPON_NONE,
            WEAPON_P228,
            WEAPON_GLOCK,
            WEAPON_SCOUT,
            WEAPON_HEGRENADE,
            WEAPON_XM1014,
            WEAPON_C4,
            WEAPON_MAC10,
            WEAPON_AUG,
            WEAPON_SMOKEGRENADE,
            WEAPON_ELITE,
            WEAPON_FIVESEVEN,
            WEAPON_UMP45,
            WEAPON_SG550,
            WEAPON_GALIL,
            WEAPON_FAMAS,
            WEAPON_USP,
            WEAPON_GLOCK18,
            WEAPON_AWP,
            WEAPON_MP5N,
            WEAPON_M249,
            WEAPON_M3,
            WEAPON_M4A1,
            WEAPON_TMP,
            WEAPON_G3SG1,
            WEAPON_FLASHBANG,
            WEAPON_DEAGLE,
            WEAPON_SG552,
            WEAPON_AK47,
            WEAPON_KNIFE,
            WEAPON_P90,
            WEAPON_SHIELDGUN = 99,
            WEAPON_BAD2 = 255
        }
        public static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }

        public class MyTreeNode
        {
            public string Text { get; set; }
            public List<MyTreeNode> Nodes { get; set; }

            public MyTreeNode(string text)
            {
                Text = text;
                Nodes = new List<MyTreeNode>();
            }
        }

        public const int RESOURCE_DOWNLOAD_THREADS = 20;
        public const float EPSILON = 0.0000005f;
        public const float EPSILON_2 = 0.00005f;
        public const int SENS_COUNT_FOR_AIM = 15;
        public const int MAX_MONITOR_REFRESHRATE = 365;
        public const float MAX_SPREAD_CONST = 0.00004f;
        public const float MAX_SPREAD_CONST2 = 0.00004f;
        public const float MIN_SENS_DETECTED = 0.0004f; //SENS < 0.02 (0.018)
        public const float MIN_SENS_WARNING = 0.004f; //SENS < 0.2 (0.18)
        public const float MIN_PLAYABLE_SENS = 0.004f;

        public static bool FoundFirstPluginPacket;
        public static int plugin_all_packets;
        public static int plugin_valid_packets;

        public static bool SkipNextErrors;
        public static int[] myThreadStates = new int[RESOURCE_DOWNLOAD_THREADS + 1];
        public static bool INSPECT_BAD_SHOT = false;
        public static bool DEBUG_ENABLED;
        public static bool NO_TELEPORT;
        public static bool DUMP_ALL_FRAMES;
        public static bool PREVIEW_FRAMES;
        public static bool PREVIEW_ENTS;

        public static string OutDumpString = "";

        public static Dictionary<string, string> testData = new Dictionary<string, string>();

        public static List<string> outFrames = new List<string>();
        public static float CurrentFrameTimeBetween;
        public static float CurrentTime;
        public static float NoWeaponAnimTime;
        public static float LastEventDetectTime;
        public static int LastEventId;
        public static float CurrentTimeSvc;
        public static float CurrentTime2;
        public static float CurrentTime3;
        public static float PreviousTime;
        public static float PreviousTime2;
        public static float PreviousTime3;
        public static List<string> whiteListCMDLIST = new List<string>();
        public static List<string> unknownCMDLIST = new List<string>();
        public static bool IsJump;
        public static bool FirstJump;
        public static bool FirstAttack;
        public static bool NewDirectory;
        public static float LastBadMsecTime;
        public static float PluginJmpTime;
        public static uint LastPlayerHull;
        public static bool NoWeaponData = true;

        //public static int DuckHack4Search = 0;
        public static bool IsDuck;
        public static bool IsDuckPressed;
        public static bool FirstDuck;
        public static float LastUnDuckTime;
        public static float LastDuckTime;
        public static int MouseJumps = -1;
        public static int JumpWithAlias = -1;
        public static int LocalPlayerId = -1;
        public static int LocalPlayerUserId = -1;
        public static int LocalPlayerUserId2 = -1;
        public static int TmpPlayerNum = -1;
        public static int TmpPlayerEnt = -1;
        public static bool FirstEventShift;
        public static FPoint oldoriginpos;
        public static FPoint curoriginpos;
        public static float speedhackdetect_time;
        public static int CurrentFrameLerp;
        public static int LerpBeforeAttack;
        public static int LerpBeforeStopAttack;
        public static int LerpAfterAttack;
        public static bool NeedDetectLerpAfterAttack;
        public static int LerpSearchFramesCount;
        public static float CurrentFrameTime;
        public static float PreviousFrameTime;
        public static float Aim8CurrentFrameViewanglesX;
        public static float Aim8CurrentFrameViewanglesY;

        public static int SearchFastZoom = -1;

        public static bool FirstAim11skip = true;
        public static bool FirstAim16skip = true;

        public static int AutoPistolStrikes;
        public static int AutoAttackStrikes;
        public static int AutoAttackStrikesID;
        public static int AutoAttackStrikesLastID;
        public static float ViewanglesXBeforeBeforeAttack;
        public static float ViewanglesYBeforeBeforeAttack;
        public static float ViewanglesXBeforeAttack;
        public static float ViewanglesYBeforeAttack;
        public static float CurrentFramePunchangleZ;
        public static string CurrentTimeString = "";
        public static float PreviousFramePunchangleZ;
        public static int NeedSearchViewAnglesAfterAttack;
        public static float ViewanglesXAfterAttack;
        public static float ViewanglesYAfterAttack;
        public static bool NeedSearchViewAnglesAfterAttackNext = true;
        public static float ViewanglesXAfterAttackNext;
        public static float ViewanglesYAfterAttackNext;
        public static bool CurrentFrameAttacked;
        public static bool CurrentFrameAttacked2;
        public static bool PreviousFrameAttacked;
        public static bool PreviousFrameAttacked2;
        public static bool PreviousFrameAlive;
        public static bool CurrentFrameAlive;
        public static bool RealAlive;
        public static bool CurrentFrameJumped;
        public static bool PreviousFrameJumped;
        public static bool CurrentFrameOnGround;
        public static bool PreviousFrameOnGround;
        public static bool CurrentFrameDuck;
        public static bool PreviousFrameDuck;
        public static int KreedzHacksCount;
        public static int FakeLagAim;
        public static int FakeLag2Aim = -2;
        public static List<int> FakeLagsValus = new List<int>();
        public static int LastJumpFrame;
        public static int JumpHackCount2;
        public static int FrameDuplicates;
        public static float LastJumpTime;
        public static float LastJumpBtnTime;
        public static float LastJumpNoGroundTime;
        public static float LastUnJumpTime;
        public static float IdealJmpTmpTime1;
        public static float IdealJmpTmpTime2;
        public static int BadTimeFound;
        public static float BadFoundTime;
        public static WeaponIdType CurrentWeapon = WeaponIdType.WEAPON_NONE;
        public static WeaponIdType StrikesWeapon = WeaponIdType.WEAPON_NONE;
        public static bool WeaponChanged;
        public static int IsAttack;
        public static bool IsInAttack() { return IsAttack > 0; }
        public static bool IsAttack2;
        public static int AmmoCount;
        public static int AttackErrors;
        public static float LastAttackHack;
        public static int TotalAimBotDetected;
        public static int TriggerAimAttackCount;
        public static bool TriggerAttackFound;
        public static bool KnifeTriggerAttackFound;
        public static float LastTriggerAttack;
        public static float LastSilentAim;
        public static float IsNoAttackLastTime;
        public static float IsNoAttackLastTime2;
        public static float IsAttackLastTime;
        public static int AttackCheck = -1;
        public static bool IsReload;
        public static int SelectSlot;
        public static int FoundForceCenterView;
        public static float ForceCenterViewTime;
        public static bool SearchPunch;
        public static int SkipNextAttack;
        public static int attackscounter;
        public static int attackscounter6;
        public static int attackscounter3;
        public static int attackscounter4;
        public static int attackscounter5;
        public static int JumpCount;
        public static int JumpCount2;
        public static int JumpCount3;
        public static int JumpCount4;
        public static int JumpCount5;
        public static int JumpCount6;
        public static float last_rg_jump_time = 9999.0f;
        public static int DeathsCoount;
        public static int KillsCount;
        public static List<int> averagefps = new List<int>();
        public static List<float> averagefps2 = new List<float>();
        public static int LastTimeOut = -1;
        public static int StuckFrames;
        public static float LastTimeDesync;
        public static bool SecondFound;
        public static string LastSecondString = "";
        public static int CurrentFps;
        public static int CurrentFpsSecond;
        public static int RealFpsMin = int.MaxValue;
        public static int RealFpsMax = int.MinValue;
        public static float LastFpsCheckTime = -1.0f;
        public static bool SecondFound2;
        public static int CurrentFps2;
        public static int CurrentFps2Second;
        public static int RealFpsMin2 = int.MaxValue;
        public static int RealFpsMax2 = int.MinValue;
        public static float LastFpsCheckTime2 = -1.0f;
        public static float LastCmdTime;
        public static string LastCmdTimeString = "00h:00m:00s:000ms";
        public static int LastCmdFrameId;
        public static string LastCmd = "";
        public static int CurrentFrameId;
        public static int CurrentFrameIdWeapon;
        public static int CaminCount;
        public static int FrameCrash;
        public static float CurrentSensitivity = -1.0f;
        public static List<float> PlayerSensitivityHistory = new List<float>();
        public static int CheckedSensCount;

        public static List<PLAYER_USED_SENS> PlayerSensUsageList = new List<PLAYER_USED_SENS>();
        public static float AngleLength = -1.0f;
        public static float AngleLengthStartTime;
        public static List<float> PlayerAngleLenHistory = new List<float>();
        public static List<string> PlayerSensitivityHistoryStrTime = new List<string>();
        public static List<string> PlayerSensitivityHistoryStrWeapon = new List<string>();
        public static List<string> PlayerSensitivityHistoryStrFOV = new List<string>();
        public static string LastSensWeapon = "";
        public static int PlayerSensitivityWarning;
        public static int CurrentFrameIdAll;
        private static float LastClientDataTime;
        public static int NeedSearchID;
        public static FPoint3D CDFRAME_ViewAngles;
        public static FPoint3D PREV_CDFRAME_ViewAngles;
        public static int SkipAimType22 = 2;
        public static int demo_skipped_frames = 0;
        public static bool NeedCheckAttack;
        public static bool NeedCheckAttack2;
        public static float LastAimedTimeDamage;
        public static int FovByFunc;
        public static int FovByFunc2;
        public static float ClientFov2 = 40.0f;
        public static float ClientFov = 90.0f;
        public static float cdframeFov = 90.0f;
        public static float checkFov = 90.0f;
        public static float checkFov2 = 90.0f;
        public static string DemoName = "";
        public static bool DisableJump5AndAim16;

        public static int sv_minrate = -1;
        public static int sv_maxrate = -1;
        public static int sv_minupdaterate = -1;
        public static int sv_maxupdaterate = -1;

        public static bool UserAlive;
        public static bool FirstUserAlive = true;
        public static int NeedWriteAim;
        public static float NeedWriteAimTime;
        public static object sync = new object();
        public static bool SearchAutoReload;
        public static bool NewAttack;
        public static bool NewAttack2;
        public static int NewAttackFrame;
        public static int NewAttack2Frame;

        public static int NewAttackForTrigger;
        public static int LastAttackForTrigger = -1;
        public static int LastAttackForTriggerFrame = -1;
        public static BinaryWriter PreviewFramesWriter;
        public static BinaryWriter ViewDemoHelperComments;
        public static List<string> OutTextDetects = new List<string>();
        public static List<string> OutTextMessages = new List<string>();
        public static int ViewDemoCommentCount;

        public static byte[] xcommentdata =
        {
            0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00,
            0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00
        };

        public static float[] LastSCMD_Angles1 = new float[2];
        public static float[] LastSCMD_Angles2 = new float[2];
        public static float[] LastSCMD_Angles3 = new float[2];

        public static float LastSCMD_Msec1;
        public static float LastSCMD_Msec2;

        public static float DemoStartTime;
        public static float DemoStartTime2;
        public static bool FoundFirstTime;
        public static bool FoundFirstTime2;
        public static bool AimType1FalseDetect;
        public static bool NeedSearchAim2;
        public static bool Aim2AttackDetected;
        public static int ShotFound = -1;
        public static int AttackFloodTimes;
        public static float ChangeWeaponTime;
        public static float ChangeWeaponTime2;
        public static float ReloadKeyPressTime;
        public static float ReloadKeyUnPressTime;
        public static float ReloadHackTime;
        public static WeaponIdType LastWatchWeapon = WeaponIdType.WEAPON_NONE;
        public static int ReallyAim2;
        public static float LastKreedzHackTime;
        public static bool NeedDetectBHOPHack;
        public static float LastDeathTime;
        public static List<Player> playerList = new List<Player>();
        public static List<Player> fullPlayerList = new List<Player>();
        public static int FovHackDetected;
        public static float FovHackTime = -9999.0f;
        public static int ThirdHackDetected;
        public static List<WindowResolution> playerresolution = new List<WindowResolution>();
        public static float LastResolutionX = 90.0f, LastResolutionY = 90.0f;
        public static int CheatKey;
        public static int Reloads;
        public static int Reloads2;
        public static int Reloads3;
        public static bool WeaponAvaiabled;
        public static int WeaponAvaiabledFrameId;
        public static float WeaponAvaiabledFrameTime;
        public static float LastPrimaryAttackTime;
        public static float WaitPrimaryAttackTime;
        public static float PrevWaitPrimaryAttackTime;
        public static float LastPrevPrimaryAttackTime;
        public static int PrimaryCheckTimer;

        public struct PrimHistory
        {
            public float primary_time;
            public float attack_time;
            public PrimHistory(float prim, float att)
            {
                primary_time = prim;
                attack_time = att;
            }
        }

        public static List<PrimHistory> PrimaryAttackHistory = new List<PrimHistory>();
        public static bool UsingAnotherMethodWeaponDetection;
        public static float LastCmdHack;
        public static int AimType7Frames;
        public static float OldAimType7Time;
        public static int OldAimType7Frames;
        public static int AimType7Event;
        public static bool SearchJumpBug;
        public static int MaxIdealJumps = 10;
        public static int CurrentIdealJumpsStrike;
        public static bool SearchNextJumpStrike;
        public static List<string> CommandsDump = new List<string>();
        public static string framecrashcmd = "";
        public static float[] CDFrameYAngleHistory = new float[3] { 0.0f, 0.0f, 0.0f };

        public class CDAngleHistoryAim12item
        {
            public float[] tmp = new float[3];
        }
        public static List<CDAngleHistoryAim12item> CDAngleHistoryAim12List = new List<CDAngleHistoryAim12item>();

        public static float[] CDFrameYAngleHistoryAim12 = new float[3] { 0.0f, 0.0f, 0.0f };
        public static List<WarnStruct> DemoScannerWarnList = new List<WarnStruct>();
        public static string LastWarnStr = "";
        public static float LastAnglePunchSearchTime;
        public static float NewViewAngleSearcherAngle;
        public static string lastnormalanswer = "";
        public static float nospreadtest;
        public static GoldSource.NetMsgFrame CurrentNetMsgFrame;
        public static GoldSource.NetMsgFrame PreviousNetMsgFrame;
        public static string TotalFreewareTool = "[ПОЛНОСТЬЮ БЕСПЛАТНЫЙ] [TOTALLY FREE]";
        public static string SourceCode = "https://github.com/UnrealKaraulov/UnrealDemoScanner";
        public static int usagesrccode;
        public static List<UcmdLerpAndMs> historyUcmdLerpAndMs = new List<UcmdLerpAndMs>();
        public static int cipid;
        public static float LastAim5Detected;
        public static float LastAim5DetectedReal;
        public static float AimType8WarnTime;
        public static bool AimType8False;
        public static float AimType8WarnTime2;
        public static int BypassWarn8_2;
        public static float bAimType8WarnTime;
        public static float bAimType8WarnTime2;
        public static int AimType8Warn;
        public static float Aim7PunchangleY;
        public static float nospreadtest2;
        public static float MaximumTimeBetweenFrames;
        public static bool GameEnd;
        public static int LostStopAttackButton;
        public static float LastLostAttackTime;
        public static float LastLostAttackTime2;
        public static int ModifiedDemoFrames;
        public static int TimeShiftCount = -5;
        public static float LastAliveTime;
        public static float LastTeleportusTime;
        public static int Aim73FalseSkip = 2;
        public static int UserNameAndSteamIDField;
        public static int UserNameAndSteamIDField2;
        public static string LastUsername = "";
        public static string LastSteam = "";
        public static int MessageId;
        public static int SVC_CHOKEMSGID;
        public static int SVC_CLIENTUPDATEMSGID;
        public static int SVC_TIMEMSGID;
        public static uint LossPackets;
        public static uint LossPackets2;
        public static uint ServerLagCount;
        public static int ChokePackets;
        public static float LastLossPacket;
        public static float LastLossTime;
        public static float LastLossTime2;
        public static float LastLossTimeEnd;
        public static float LastChokePacket;
        public static string LastStuffCmdCommand = "";
        public static bool MoveLeft = true;
        public static bool MoveRight = true;
        public static float LastUnMoveLeft;
        public static float LastMoveLeft;
        public static bool StrafeOptimizerFalse;
        public static int DetectStrafeOptimizerStrikes;
        public static float LastUnMoveRight;
        public static float LastMoveRight;
        public static int FramesOnGround;
        public static int FramesOnFly;
        public static int FlyDirection;
        public static float PreviousSimvelZ;
        public static float PreviousSimvelZ_forstuck;
        public static float PreviousSimorgZ_forstuck;
        public static int AirStuckWarnTimes;
        public static bool SearchFakeJump;
        public static int TotalFramesOnFly;
        public static int TotalFramesOnGround;
        public static int TotalAttackFramesOnFly;
        public static string ServerName = "";
        public static string MapName = "";
        public static string GameDir = "";
        public static byte StartPlayerID = 255;
        public static bool DealthMatch;
        public static float GameEndTime;
        public static int ViewEntity = -1;
        public static int ViewModel = -1;
        public static int CL_Intermission = -1;
        public static uint LastPlayerEntity;
        public static uint LastEntity;
        public static int PlayerTeleportus;
        public static int NeedSkipDemoRescan;
        public static bool DemoRescanned;
        public static bool FirstBypassKill = true;
        public static int BypassCount;
        public static float LastMovementHackTime;
        public static int AirShots;
        public static bool InForward = true;
        public static float LastUnMoveForward;
        public static float LastMoveForward;
        public static int DuckStrikes;
        public static bool NeedDetectThirdPersonHack;
        public static int ThirdPersonHackDetectionTimeout = -1;
        public static float NoSpreadDetectionTime;
        public static int FrameUnattackStrike;
        public static int FrameAttackStrike;
        public static float LastUseTime;
        public static int MoveLeftStrike;
        public static int MoveRightStrike;
        public static bool InStrafe;
        public static float LastStrafeDisabled;
        public static float LastStrafeEnabled;
        public static int BHOPcount;
        public static int BHOP_GroundWarn;
        public static int BHOP_JumpWarn;
        public static int BHOP_GroundSearchDirection;
        public static float LastBhopTime;
        public static int CurrentFrameDuplicated;
        public static int UnknownMessages;
        public static string LastAltTabStart = "00h:00m:00s:000ms";
        public static float LastAltTabStartTime;
        public static bool AltTabEndSearch;
        public static int AltTabCount2;
        public static float LastAngleManipulation;
        public static bool NeedFirstNickname = true;
        public static int AngleStrikeDirection;
        public static float AngleDirectionChangeTime;
        public static AngleDirection LastAngleDirection = AngleDirection.AngleDirectionNO;
        public static bool NeedCheckAngleDiffForStrafeOptimizer;
        public static float LastStrafeOptimizerWarnTime;
        public static bool CurrentFrameForward;
        public static bool PreviousFrameForward;
        public static GoldSource.UCMD_BUTTONS CurrentFrameButtons;
        public static GoldSource.UCMD_BUTTONS PreviousFrameButtons;
        public static bool SearchOneFrameDuck;
        public static float NeedSearchUserAliveTime;
        public static float LastJumpHackFalseDetectionTime;
        public static int AngleDirectionChanges;
        public static int StrafeAngleDirectionChanges;
        public static float LastCmdDuckTime;
        public static float LastCmdUnduckTime;
        public static int CurrentNetMsgFrameId;
        public static int StopAttackBtnFrameId;
        public static bool NeedSearchAim3;
        public static bool LossFalseDetection;
        public static float[] TimeShift4Times = new float[3] { 0.0f, 0.0f, 0.0f };
        public static int LastFrameDiff;
        public static bool AimType6FalseDetect;
        public static float PlayersVoiceTimer;
        public static float Aim8DetectionTimeY = 0.0f;
        public static float Aim8DetectionTimeX;
        public static float NewAttackTime;
        public static float NewAttackTimeAim9;
        public static float LastGameMaximizeTime;
        public static int WeaponAnimWarn;
        public static WeaponIdType LastWeaponAnim = WeaponIdType.WEAPON_NONE;
        public static float ReloadTime;
        public static WeaponIdType EndReloadWeapon = WeaponIdType.WEAPON_NONE;
        public static WeaponIdType StartReloadWeapon = WeaponIdType.WEAPON_NONE;
        public static int ReloadWarns;
        public static float RoundEndTime;
        public static float LastSideMoveTime;
        public static float LastForwardMoveTime;
        public static float LastBackMoveTime;
        public static float LastDamageTime;
        public static float StartGameSecond = float.MaxValue;
        public static float EndGameSecond = float.MinValue;
        public static int CurrentGameSecond;
        public static int CurrentGameSecond2;
        public static float LastRealJumpTime;
        public static uint LastWeaponReloadStatus;
        public static float LastScreenshotTime;
        public static float GameEndTime2;
        public static float GameStartTime;
        public static bool PlayerFrozen;
        public static float PlayerFrozenTime;
        public static float PlayerUnFrozenTime;
        public static int ReturnToGameDetects;
        public static bool IsScreenFade;
        public static float LastViewChange;
        public static bool HideWeapon;
        public static float HideWeaponTime;
        public static bool MINIMIZED = true;
        public static int NeedIgnoreAttackFlag;
        public static int NeedIgnoreAttackFlagCount;
        public static float LastSoundTime;
        public static float FoundBigVelocityTime;
        public static float FoundVelocityTime;
        public static float LastBeamFound;
        public static float LastForceCenterView;
        public static int LastIncomingSequence;
        public static int FrameErrors;
        public static int SkipFirstCMD4 = 3;
        public static int LastIncomingAcknowledged;
        public static int LastOutgoingSequence;
        public static int maxLastIncomingSequence;
        public static int maxLastIncomingAcknowledged;
        public static int maxLastOutgoingSequence;
        public static int AlternativeTimeCounter;
        public static bool InBack = true;
        public static float LastMoveBack;
        public static bool InLook;
        public static float LastLookDisabled;
        public static float LastLookEnabled;
        public static int FlyJumps;
        public static bool SearchMoveHack1;
        public static string KnownSkyName = string.Empty;
        public static DateTime StartScanTime;
        public static float HorAngleTime;
        public static int WarnsAfterGameEnd;
        public static int WarnsDuringLevel;

        public static bool SKIP_RESULTS;
        public static bool LOG_MODE;
        public static List<string> LOG_MODE_OUTPUT = new List<string>();

        public static int EmptyFrames;
        public static List<float> LastPunchAngleX = new List<float>();
        public static List<float> LastPunchAngleY = new List<float>();
        public static int PunchWarnings;
        public static int LostAngleWarnings;
        public static int ClientDataCountMessages;
        public static int ClientDataCountDemos;
        public static int SVC_SETANGLEMSGID;
        public static float LastFakeLagTime;
        public static int SVC_ADDANGLEMSGID;
        public static float LastAttackStartCmdTime;
        public static float LastAttackStartStopCmdTime;
        public static float LastAttackBtnTime;
        public static float LastFloodAttackTime;
        public static bool LASTFRAMEISCLIENTDATA;
        public static int BadAnglesFoundCount;
        public static int MapAndCrc32_Top;
        public static int MapAndCrc32_Left;
        public static int FPS_OVERFLOW;
        public static bool FoundFpsHack1;
        public static float FpsOverflowTime;
        public static string DownloadLocation = "";
        public static int downloadlocationchanged;
        public static List<string> FileDirectories = new List<string>();
        public static string SteamID = "";
        public static string RecordDate = "";
        /*
          var type = BitBuffer.ReadUnsignedBits(4);
                var downloadname = BitBuffer.ReadString();
                var index = BitBuffer.ReadUnsignedBits(12);
                ulong downloadsize = BitBuffer.ReadUnsignedBits(24);
                var flags = BitBuffer.ReadUnsignedBits(3);

         
         */

        public struct RESOURCE_STRUCT
        {
            public uint res_type;
            public string res_path;
            public uint res_index;
            public uint res_flags;

            public RESOURCE_STRUCT(uint type, string path, uint index, uint flags)
            {
                res_type = type;
                res_path = path;
                res_index = index;
                res_flags = flags;
            }

            public override string ToString()
            {
                return "Type:" + res_type + ". Index:" + res_index + ".Flags:" + res_flags + ". Path:\"" +
                    res_path + "\"";
            }
        }

        public static List<RESOURCE_STRUCT> DownloadedResources = new List<RESOURCE_STRUCT>();
        public static ulong DownloadResourcesSize;


        public static List<float> LastSearchViewAngleY = new List<float>();
        public static int PluginEvents = -1;
        public static int BadEvents;
        public static uint CurrentEvents;
        public static uint Event7Hack;
        public static bool IsRussia;
        public static bool FirstMap = true;
        public static float LastAttackPressed;
        public static float LastScoreTime;
        public static int PluginFrameNum = -1;
        public static string PluginVersion = string.Empty;
        public static float flPluginVersion;
        public static int InitAimMissingSearch;
        public static uint LastLossPacketCount;
        public static int CurrentMsgBytes;
        public static int MaxBytesPerSecond;
        public static List<int> BytesPerSecond = new List<int>();
        public static int MsgOverflowSecondsCount;
        public static int CurrentMsgHudCount;
        public static int CurrentMsgDHudCount;
        public static int CurrentMsgPrintCount;
        public static int CurrentMsgStuffCmdCount;
        public static int MaxHudMsgPerSecond;
        public static int MaxDHudMsgPerSecond;
        public static int MaxStuffCmdMsgPerSecond;
        public static int MaxPrintCmdMsgPerSecond;
        public static int SkipChangeWeapon;

        public static string VoiceCodec = "";
        public static byte VoiceQuality = 5;

        public static int SearchJumpHack5;
        //public static int SearchJumpHack51;
        public static bool BadPunchAngle;
        public static float FrametimeMin = 9999.0f, MsecMin = 9999.0f, FrametimeMax, MsecMax;
        public static bool DetectCmdHackType10;
        public static float CmdHack10_origX;
        public static float CmdHack10_origY;
        public static float CmdHack10_origZ;
        public static float CmdHack10_detecttime;
        public static bool FoundCustomClientPattern;
        public static bool FoundNextClient;
        public static List<float> Punch0_Search = new List<float>();
        public static List<float> Punch0_Search_Time = new List<float>();
        public static float Punch0_Valid_Time;

        public static
            byte[] EMPTY_WAV =
            new byte[] {
                0x52, 0x49, 0x46, 0x46, // "RIFF"
                0x24, 0x00, 0x00, 0x00, // File size in bytes excluding first 8 bytes
                0x57, 0x41, 0x56, 0x45, // "WAVE"
                0x66, 0x6D, 0x74, 0x20, // "fmt " (with space)
                0x10, 0x00, 0x00, 0x00, // Length of format part (16 for PCM format)
                0x01, 0x00,             // Format type (1 for PCM)
                0x01, 0x00,             // Number of channels (1 for mono)
                0x44, 0xAC, 0x00, 0x00, // Sample rate (44100 Hz)
                0x88, 0x58, 0x01, 0x00, // Byte rate (44100 * 2 (16 bits = 2 bytes) * 1 (mono) = 88200)
                0x02, 0x00,             // Block align (2 = 16 bits * 1 channel / 8)
                0x10, 0x00,             // Bits per sample (16 bits)
                0x64, 0x61, 0x74, 0x61, // "data"
                0x00, 0x00, 0x00, 0x00  // Data size in bytes (0 for empty file)
            };
        public static
            byte[] EMPTY_MP3 =
            new byte[] {
  0x52, 0x49, 0x46, 0x46, 0x25, 0x00, 0x00, 0x00, 0x57, 0x41, 0x56, 0x45, 0x66, 0x6d, 0x74, 0x20,
  0x10, 0x00, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x44, 0xac, 0x00, 0x00, 0x88, 0x58, 0x01, 0x00,
  0x02, 0x00, 0x10, 0x00, 0x64, 0x61, 0x74, 0x61, 0x74, 0x00, 0x00, 0x00, 0x00
};
        public static byte[] EMPTY_SPR = {
  0x49, 0x44, 0x53, 0x50, 0x02, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00,
  0xf3, 0x04, 0xb5, 0x40, 0x08, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x01, 0x17, 0xcf, 0x17, 0x44, 0xd8, 0x44,
  0x56, 0xdc, 0x56, 0x82, 0xe5, 0x82, 0x95, 0xe9, 0x95, 0xe2, 0xf9, 0xe2, 0xf0, 0xfc, 0xf0, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x00, 0x00, 0xff, 0x00, 0x00, 0x00, 0x00, 0xfc, 0xff,
  0xff, 0xff, 0x04, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0xff, 0xff,
  0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x02, 0x03, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0x00, 0x02, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x01, 0x03, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0x02, 0x04, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
  0xff, 0x00, 0x02, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff
};
        public static byte[] EMPTY_MDL =
                {
  0x49, 0x44, 0x53, 0x54, 0x0a, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x88, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0xf4, 0x00, 0x00, 0x00, 0x48, 0x01, 0x00, 0x00,
  0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x44, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x62, 0x6c, 0x61, 0x63, 0x6b, 0x2e, 0x62, 0x6d, 0x70, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x18, 0x00, 0x00, 0x00, 0x18, 0x00, 0x00, 0x00,
  0x48, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
  0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
};

        public static bool IsNewAttack()
        {
            return NewAttack ||
                   (NewAttackFrame <= CurrentFrameIdAll && Math.Abs(CurrentFrameIdAll - NewAttackFrame) < 5);
        }

        public static bool IsNewSecondKnifeAttack()
        {
            return NewAttack2 || (NewAttack2Frame <= CurrentFrameIdAll &&
                                  Math.Abs(CurrentFrameIdAll - NewAttack2Frame) < 5);
        }

        public static WeaponIdType GetWeaponByStr(string str)
        {
            if (str.ToLower().IndexOf("weapon_") == -1)
            {
                str = "weapon_" + str.ToLower();
            }

            str = str.ToUpper();
            foreach (var weaponName in Enum.GetNames(typeof(WeaponIdType)))
            {
                if (weaponName == str)
                {
                    return (WeaponIdType)Enum.Parse(typeof(WeaponIdType), weaponName);
                }
            }

            return WeaponIdType.WEAPON_NONE;
        }

        public static bool IsCmdChangeWeapon()
        {
            var retvar = abs(CurrentTime - ChangeWeaponTime);
            return retvar < 0.3f && retvar >= 0;
        }

        public static bool IsRealChangeWeapon()
        {
            var retvar = abs(CurrentTime - ChangeWeaponTime2);
            return retvar < 0.3f && retvar >= 0;
        }

        public static bool IsForceCenterView()
        {
            var retvar = abs(CurrentTime - ForceCenterViewTime);
            return retvar < 2.0f && retvar >= 0;
        }


        public static string Rusifikator(string str)
        {
            str = str.Replace(" at ", " на ");
            return str;
        }

        public static void DemoScanner_AddTextMessage(string msg, string type, float time, string timestring)
        {
            if (msg.Length == 0)
            {
                return;
            }

            msg = msg.Replace("\n", "^n").Replace("\r", "^n").Replace("\x01", "^1").Replace("\x02", "^2")
                .Replace("\x03", "^3").Replace("\x04", "^4");
            if (msg.Length == 0)
            {
                return;
            }

            OutTextMessages.Add("[" + type + "] : [" + msg + "]" + " at (" + time + ") " + timestring);
        }

        public static void DemoScanner_AddInfo(string info, bool is_plugin = false, bool no_prefix = false)
        {
            var tmpcol = Console.ForegroundColor;

            if (IsRussia)
            {
                info = Rusifikator(info);
            }

            string prefix = string.Empty;

            if (is_plugin)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                if (!no_prefix)
                {
                    if (IsRussia)
                    {
                        prefix = "[Модуль] ";
                    }
                    else
                    {
                        prefix = "[PLUGIN] ";
                    }
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                if (!no_prefix)
                {
                    if (IsRussia)
                    {
                        prefix = "[ИНФОРМАЦИЯ] ";
                    }
                    else
                    {
                        prefix = "[INFO] ";
                    }
                }
            }

            if (LOG_MODE)
            {
                LOG_MODE_OUTPUT.Add(prefix + info);
                return;
            }

            Console.Write(prefix);

            if (is_plugin)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
            }

            OutTextDetects.Add(prefix + info);
            AddViewDemoHelperComment(prefix + info);

            Console.WriteLine(info);
            Console.ForegroundColor = tmpcol;
        }

        public static bool DemoScanner_AddWarn(string warn, bool detected = true, bool unused1 = true,
            bool skipallchecks = false, bool uds_plugin = false, bool hide_prefix = false)
        {
            if (IsRussia)
            {
                warn = Rusifikator(warn);
            }

            if (GameEnd && !uds_plugin)
            {
                if (detected)
                {
                    WarnsAfterGameEnd++;
                }

                return false;
            }

            if (CL_Intermission != 0)
            {
                if (detected)
                {
                    WarnsDuringLevel++;
                }

                return false;
            }

            if (LastWarnStr == warn)
            {
                return true;
            }

            LastWarnStr = warn;
            var warnStruct = new WarnStruct
            {
                Warn = warn,
                WarnTime = CurrentTime,
                Detected = detected,
                SkipAllChecks = skipallchecks,
                Visited = false,
                Plugin = uds_plugin,
                HidePrefix = hide_prefix
            };

            DemoScannerWarnList.Add(warnStruct);
            return true;
        }

        public static void UpdateWarnList(bool force = false)
        {
            for (var i = 0; i < DemoScannerWarnList.Count; i++)
            {
                var curwarn = DemoScannerWarnList[i];
                if (!curwarn.Visited && (abs(CurrentTime - curwarn.WarnTime) > 0.2f || force || DEBUG_ENABLED))
                {
                    curwarn.Visited = true;
                    var tmpcol = Console.ForegroundColor;
                    string prefix = string.Empty;
                    string postfix = string.Empty;

                    if ((curwarn.Detected && !IsPlayerLossConnection(curwarn.WarnTime) && RealAlive) ||
                        curwarn.SkipAllChecks)
                    {
                        if (curwarn.Plugin)
                        {
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            if (IsRussia)
                            {
                                prefix = "[Модуль] ";
                            }
                            else
                            {
                                prefix = "[PLUGIN] ";
                            }

                            Console.Write(prefix);
                        }

                        if (!curwarn.Plugin || !curwarn.HidePrefix)
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            if (IsRussia)
                            {
                                prefix = "[ОБНАРУЖЕНО] ";
                            }
                            else
                            {
                                prefix = "[DETECTED] ";
                            }

                            LastLossPacket = 0.0f;
                            Console.Write(prefix);
                        }

                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine(curwarn.Warn);
                    }
                    else
                    {
                        if (curwarn.Plugin)
                        {
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            if (IsRussia)
                            {
                                prefix = "[Модуль] ";
                            }
                            else
                            {
                                prefix = "[PLUGIN] ";
                            }

                            Console.Write(prefix);
                        }
                        if (!curwarn.Plugin || !curwarn.HidePrefix)
                        {
                            Console.ForegroundColor = ConsoleColor.Gray;
                            if (IsRussia)
                            {
                                prefix = "[ПРЕДУПРЕЖДЕНИЕ] ";
                            }
                            else
                            {
                                prefix = "[WARNING] ";
                            }

                            Console.Write(prefix);
                        }

                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.Write(curwarn.Warn);
                        Console.ForegroundColor = ConsoleColor.Red;
                        if (IsPlayerLossConnection(curwarn.WarnTime))
                        {
                            if (IsRussia)
                            {
                                postfix = " (ЛАГ)";
                            }
                            else
                            {
                                postfix = " (LAG)";
                            }

                            Console.Write(postfix);
                        }
                        else if (!RealAlive)
                        {
                            if (IsRussia)
                            {
                                postfix = " (УМЕР)";
                            }
                            else
                            {
                                postfix = " (DEAD)";
                            }

                            Console.Write(postfix);
                        }

                        Console.WriteLine();
                    }

                    if (curwarn.Detected)
                    {
                        LossFalseDetection = false;
                    }

                    if (LOG_MODE)
                    {
                        LOG_MODE_OUTPUT.Add(prefix + curwarn.Warn + postfix);
                    }

                    OutTextDetects.Add(prefix + curwarn.Warn + postfix);
                    AddViewDemoHelperComment(prefix + curwarn.Warn + postfix);

                    DemoScannerWarnList[i] = curwarn;
                    Console.ForegroundColor = tmpcol;
                }
            }
        }

        public static void CheckConsoleCheat(string s)
        {
            var s2 = s;
            if (s[0] == '+' || s[0] == '-')
            {
                s2 = s.Remove(0, 1);
            }

            if (!whiteListCMDLIST.Contains(s2) && !unknownCMDLIST.Contains(s2))
            {
                if (IsRussia)
                {
                    unknownCMDLIST.Add("[ОБНАРУЖЕНА] [НЕИЗВЕСТНАЯ КОМАНДА] : \"" + s + "\" at (" + CurrentTime + ") " +
                                       CurrentTimeString);
                }
                else
                {
                    unknownCMDLIST.Add("[DETECTED] [UNKNOWN CMD] : \"" + s + "\" at (" + CurrentTime + ") " +
                                       CurrentTimeString);
                }
            }
        }

        public static void CheckConsoleCommand(string s2, bool isstuff = false)
        {
            var s = s2.Trim().TrimBad();
            var sLower = s.ToLower();
            if (!isstuff)
            {
                CheckConsoleCheat(s);
            }

            var wait = CurrentFrameId - LastCmdFrameId;

            if (isstuff)
            {
                if (sLower.IndexOf("snapshot") > -1 || sLower.IndexOf("screenshot") > -1)
                {
                    LastScreenshotTime = CurrentTime;

                    if (IsRussia)
                    {
                        DemoScanner_AddInfo("Администратор сделал скриншот игроку, время " + CurrentTimeString);
                    }
                    else
                    {
                        DemoScanner_AddInfo("Server request player screenshot at " + CurrentTimeString);
                    }
                }
            }

            if (DUMP_ALL_FRAMES)
            {
                OutDumpString += "{CMD:\"" + s2 + " " + (isstuff ? "STUFFCMD\"" : "\"") + "}\n";
            }

            if (FrameCrash > 4)
            {
                FirstDuck = false;
                FirstJump = false;
                FirstAttack = false;
                SearchJumpBug = false;
                if (JumpWithAlias >= -1 && FrameCrash <= 7)
                {
                    JumpWithAlias--;
                }
            }

            if (LastStuffCmdCommand != "" && s == LastStuffCmdCommand.Trim().TrimBad())
            {
                LastStuffCmdCommand = "";
                CommandsDump.Add("wait" + wait + ";");
                if (IsRussia)
                {
                    CommandsDump.Add(CurrentTimeString + " [НОМЕР КАДРА: " + CurrentFrameId + "] : " + s + "(" +
                                     CurrentTime + ") --> ВЫПОЛНЕНО СЕРВЕРОМ");
                }
                else
                {
                    CommandsDump.Add(CurrentTimeString + " [FRAME NUMBER: " + CurrentFrameId + "] : " + s + "(" +
                                     CurrentTime + ") --> EXECUTED BY SERVER");
                }

                return;
            }

            LastStuffCmdCommand = "";
            if (isstuff)
            {
                CommandsDump.Add("wait" + wait + ";");
                if (IsRussia)
                {
                    CommandsDump.Add(CurrentTimeString + " [НОМЕР КАДРА: " + CurrentFrameId + "] : " + s + "(" +
                                     CurrentTime + ") --> ВЫПОЛНЕНО ЧЕРЕЗ STUFFTEXT");
                }
                else
                {
                    CommandsDump.Add(CurrentTimeString + " [FRAME NUMBER: " + CurrentFrameId + "] : " + s + "(" +
                                     CurrentTime + ") --> EXECUTED BY STUFFTEXT");
                }
            }
            else
            {
                CommandsDump.Add("wait" + wait + ";");
                if (IsRussia)
                {
                    CommandsDump.Add(CurrentTimeString + " [НОМЕР КАДРА: " + CurrentFrameId + "] : " + s + "(" +
                                     CurrentTime + ")");
                }
                else
                {
                    CommandsDump.Add(CurrentTimeString + " [FRAME NUMBER: " + CurrentFrameId + "] : " + s + "(" +
                                     CurrentTime + ")");
                }
            }

            if (sLower.IndexOf("-showscores") > -1)
            {
                LastAltTabStart = LastCmdTimeString;
                LastAltTabStartTime = CurrentTime;
                AltTabEndSearch = true;
            }

            if (s.IndexOf("-") > -1 && framecrashcmd != sLower)
            {
                framecrashcmd = sLower;
                FrameCrash++;
                if (FrameCrash > 7 && AltTabEndSearch)
                {
                    AltTabCount2++;
                    AltTabEndSearch = false;
                    MINIMIZED = true;

                    if (IsRussia)
                    {
                        DemoScanner_AddInfo("Игрок свернул игру с " + LastAltTabStart + " по " + CurrentTimeString);
                    }
                    else
                    {
                        DemoScanner_AddInfo(
                            "Player minimized game from " + LastAltTabStart + " to " + CurrentTimeString);
                    }

                    if (abs(LastScreenshotTime) > EPSILON && LastScreenshotTime > LastAltTabStartTime && LastScreenshotTime < CurrentTime)
                    {
                        LastScreenshotTime = 0.0f;
                        if (IsRussia)
                        {
                            DemoScanner_AddInfo("Игрок был свернут, скриншот может быть черным.");
                        }
                        else
                        {
                            DemoScanner_AddInfo("Game is minimized and screenshot can be black.");
                        }
                    }

                    LastGameMaximizeTime = CurrentTime;
                }
            }
            else
            {
                AltTabEndSearch = false;
                FrameCrash = 0;
            }

            if (s.IndexOf("+") > -1)
            {
                AltTabEndSearch = false;
                FrameCrash = 0;
            }

            if (sLower.IndexOf("+reload") > -1)
            {
                FrameCrash = 0;
                ReloadKeyPressTime = CurrentTime;
                ReloadHackTime = 0.0f;
                //AutoPistolStrikes = 0;
                //LastPrimaryAttackTime = 0.0f;
                //LastPrevPrimaryAttackTime = 0.0f;
            }
            else if (sLower.IndexOf("-reload") > -1)
            {
                ReloadKeyUnPressTime = CurrentTime;
                ReloadHackTime = 0.0f;
                //AutoPistolStrikes = 0;
                //LastPrimaryAttackTime = 0.0f;
                //LastPrevPrimaryAttackTime = 0.0f;
            }

            if (sLower.IndexOf("+klook") > -1)
            {
                CheatKey++;
            }

            if (sLower.IndexOf("+camin") > -1 && FirstJump)
            {
                CaminCount++;
                if (CaminCount == 3)
                {
                    if (abs(CurrentTime - LastJumpTime) < 1.0)
                    {
                        if (abs(CurrentTime - LastKreedzHackTime) > 2.5f)
                        {
                            if (DemoScanner_AddWarn("[JUMPHACK XTREME] at (" + CurrentTime + ") " + CurrentTimeString))
                            {
                                LastKreedzHackTime = CurrentTime;
                                KreedzHacksCount++;
                            }
                        }
                    }

                    CaminCount = 0;
                }
            }
            else if (sLower.IndexOf("-camin") > -1)
            {
                CaminCount = 0;
            }

            if (sLower.IndexOf("force_centerview") > -1)
            {
                ForceCenterViewTime = CurrentTime;
                FoundForceCenterView++;
            }

            if (sLower.IndexOf("slot") > -1 || sLower.IndexOf("invprev") > -1 || sLower.IndexOf("invnext") > -1)
            {
                InitAimMissingSearch = -1;
                SkipNextAttack = 2;
                SelectSlot = 2;
                NeedSearchAim2 = false;
                Aim2AttackDetected = false;
                ShotFound = -1;
                ChangeWeaponTime = CurrentTime;
                //AutoPistolStrikes = 0;
                //LastPrimaryAttackTime = 0.0f;
                // LastPrevPrimaryAttackTime = 0.0f;
            }

            if (sLower.IndexOf("+attack2") > -1)
            {
                //AutoPistolStrikes = 0;
                //LastPrimaryAttackTime = 0.0f;
                //LastPrevPrimaryAttackTime = 0.0f;
                LastAttackStartStopCmdTime = CurrentTime;
                IsAttack2 = true;
                if (!IsUserAlive())
                {
                    IsAttack2 = false;
                }

                SearchFastZoom = 1;
            }
            else if (sLower.IndexOf("-attack2") > -1)
            {
                //AutoPistolStrikes = 0;
                //LastPrimaryAttackTime = 0.0f;
                //LastPrevPrimaryAttackTime = 0.0f;
                InitAimMissingSearch = -1;
                LastAttackStartStopCmdTime = CurrentTime;
                IsAttack2 = false;

                if (SearchFastZoom == 1 && wait == 1)
                {
                    SearchFastZoom = 2;
                }
                else
                {
                    SearchFastZoom = 0;
                }
            }
            else if (sLower.IndexOf("attack2") == -1 && sLower.IndexOf("attack3") == -1)
            {
                if (sLower.IndexOf("+attack") > -1)
                {
                    if (SearchFastZoom == 2 && wait == 1)
                    {
                        SearchFastZoom = 3;
                    }
                    else
                    {
                        SearchFastZoom = 0;
                    }
                }
                else if (sLower.IndexOf("-attack") > -1)
                {
                    if (SearchFastZoom == 3 && wait == 1)
                    {
                        SearchFastZoom = 4;
                    }
                    else
                    {
                        SearchFastZoom = 0;
                    }
                }
                else
                {
                    SearchFastZoom = 0;
                }
            }
            else
            {
                SearchFastZoom = 0;
            }

            if (sLower.IndexOf("attack2") == -1 && sLower.IndexOf("attack3") == -1)
            {
                if (sLower.IndexOf("+attack") > -1)
                {
                    if (abs(CurrentTime) < 0.01 || CurrentTime < LastAttackStartStopCmdTime)
                    {
                        BadTimeFound += 50;
                    }

                    SearchAutoReload = false;
                    AttackFloodTimes++;
                    if (AttackFloodTimes >= 4)
                    {
                        if (abs(CurrentTime - LastFloodAttackTime) > 20.0)
                        {
                            if (DemoScanner_AddWarn("[ATTACK FLOOD TYPE 1] at (" + CurrentTime + ") " + CurrentTimeString))
                            {
                                TotalAimBotDetected++;
                                LastFloodAttackTime = CurrentTime;
                            }
                        }

                        AttackFloodTimes = 0;
                    }

                    FirstAttack = true;
                    FrameCrash = 0;
                    //Console.WriteLine("+ATTACK!");
                    CDAngleHistoryAim12item tmpCdAngles = new CDAngleHistoryAim12item();
                    tmpCdAngles.tmp[0] = fullnormalizeangle(PREV_CDFRAME_ViewAngles.Y);
                    tmpCdAngles.tmp[1] = fullnormalizeangle(CDFRAME_ViewAngles.Y);
                    tmpCdAngles.tmp[2] = -99999.0f;
                    CDAngleHistoryAim12List.Insert(0, tmpCdAngles);

                    if (CDAngleHistoryAim12List.Count > 5)
                        CDAngleHistoryAim12List.RemoveAt(CDAngleHistoryAim12List.Count - 1);

                    NeedIgnoreAttackFlag = 0;

                    if (IsUserAlive())
                    {
                        attackscounter++;
                    }

                    NeedSearchAim3 = false;
                    /* if (DEBUG_ENABLED)
                     {
                         Console.WriteLine("User alive func:" + IsUserAlive() + ". User real alive ? : " + RealAlive + ". Weapon:" + CurrentWeapon + ".Frame:" + wait
                             + ".Frame2: " + (CurrentFrameIdWeapon - WeaponAvaiabledFrameId) + ".Frame3: " + wait);
                     }*/
                    SearchPunch = abs(LastAttackStartStopCmdTime - CurrentTime) > 0.5f;
                    LastAttackStartStopCmdTime = CurrentTime;
                    LastAttackStartCmdTime = CurrentTime;


                    if (IsUserAlive())
                    {
                        if (InitAimMissingSearch < 0)
                        {
                            InitAimMissingSearch++;
                        }
                        else
                        {
                            if (abs(CurrentTime - IsAttackLastTime) > 2.0)
                            {
                                if (IsRealWeapon())
                                {
                                    if (InitAimMissingSearch <= 0)
                                    {
                                        InitAimMissingSearch = 3;
                                    }
                                }
                                else
                                {
                                    InitAimMissingSearch = 0;
                                }
                            }
                        }

                        if (AutoAttackStrikes >= 4)
                        {
                            if (AimType6FalseDetect && AutoAttackStrikes == 4)
                            {
                                // DISABLED FOR REWRITE
                                //DemoScanner_AddWarn("[AIM TYPE 6] at (" + CurrentTime + "):" + CurrentTimeString, false);
                                AutoAttackStrikes = 0;
                                AimType6FalseDetect = false;
                            }
                            else if (!AimType6FalseDetect)
                            {
                                //DemoScanner_AddWarn("[AIM TYPE 6] at (" + CurrentTime + "):" + CurrentTimeString, false);
                                AimType6FalseDetect = false;
                                //SilentAimDetected++;
                                //Console.ForegroundColor = tmpcol; 
                                AutoAttackStrikes = 0;
                            }
                        }
                        else if (CurrentFrameId - LastCmdFrameId <= 7 && CurrentFrameId - LastCmdFrameId > 1)
                        {
                            if (CurrentFrameIdWeapon - WeaponAvaiabledFrameId == 1)
                            {
                                AimType6FalseDetect = true;
                            }

                            if (!AimType6FalseDetect && WeaponAvaiabled &&
                                CurrentFrameIdWeapon - WeaponAvaiabledFrameId <= 7 &&
                                CurrentFrameIdWeapon - WeaponAvaiabledFrameId > 1)
                            {
                                AimType6FalseDetect = false;
                            }

                            if (CurrentWeapon == WeaponIdType.WEAPON_DEAGLE ||
                                CurrentWeapon == WeaponIdType.WEAPON_USP || CurrentWeapon == WeaponIdType.WEAPON_P228 ||
                                CurrentWeapon == WeaponIdType.WEAPON_ELITE ||
                                CurrentWeapon == WeaponIdType.WEAPON_FIVESEVEN ||
                                CurrentWeapon == WeaponIdType.WEAPON_GLOCK18 ||
                                CurrentWeapon == WeaponIdType.WEAPON_GLOCK)
                            {
                                if (StrikesWeapon == WeaponIdType.WEAPON_NONE)
                                {
                                    StrikesWeapon = CurrentWeapon;
                                }
                                else
                                {
                                    if (StrikesWeapon != CurrentWeapon)
                                    {
                                        AutoAttackStrikes = 0;
                                        StrikesWeapon = CurrentWeapon;
                                    }
                                }

                                AutoAttackStrikes++;
                                if (AutoAttackStrikesLastID == AutoAttackStrikesID)
                                {
                                    AutoAttackStrikes--;
                                }

                                AutoAttackStrikesLastID = AutoAttackStrikesID;
                            }
                            else
                            {
                                AutoAttackStrikes = 0;
                            }
                        }
                        else
                        {
                            AutoAttackStrikes = 0;
                        }

                        LastFrameDiff = CurrentFrameId - LastCmdFrameId;
                        ViewanglesXBeforeBeforeAttack = PREV_CDFRAME_ViewAngles.X;
                        ViewanglesYBeforeBeforeAttack = PREV_CDFRAME_ViewAngles.Y;
                        NeedSearchViewAnglesAfterAttack++;
                        LerpBeforeAttack = CurrentFrameLerp;
                        NeedDetectLerpAfterAttack = true;

                        if (IsInAttack())
                        {
                            AttackErrors++;
                            LastAttackHack = CurrentTime;
                        }

                        if (AttackCheck < 0 && CurrentFrameDuplicated == 0)
                        {
                            /* if (DEBUG_ENABLED)
                                 Console.WriteLine("Attack. Start search aim type 1!");*/
                            if (SkipNextAttack == 0)
                            {
                                SkipNextAttack = 1;
                            }

                            AttackCheck = 1;
                        }

                        Aim2AttackDetected = false;
                        NeedSearchAim2 = true;
                        IsAttack = 5;
                        IsAttackLastTime = CurrentTime;
                    }
                    else
                    {
                        if (InitAimMissingSearch > 0)
                        {
                            InitAimMissingSearch = 0;
                        }

                        AutoAttackStrikes = 0;
                        NeedSearchViewAnglesAfterAttack = 0;
                        Aim2AttackDetected = false;
                        NeedSearchAim2 = false;
                        IsAttack = 5;
                        IsAttackLastTime = 0.0f;
                    }

                    ShotFound = -1;
                    WeaponChanged = false;
                    LastLostAttackTime = 0.0f;
                }
                else if (sLower.IndexOf("-attack") > -1)
                {
                    //LastPrimaryAttackTime = 0.0f;
                    //LastPrevPrimaryAttackTime = 0.0f;
                    SearchAutoReload = true;
                    InitAimMissingSearch = -1;
                    if (IsUserAlive())
                    {
                        LerpBeforeStopAttack = CurrentFrameLerp;
                        LerpSearchFramesCount = 9;
                    }

                    if (abs(LastLostAttackTime) > EPSILON)
                    {
                        LastLostAttackTime = 0.0f;
                        if (LostStopAttackButton > 0)
                        {
                            LostStopAttackButton--;
                        }
                    }

                    LastAttackStartStopCmdTime = CurrentTime;


                    NeedSearchViewAnglesAfterAttack = 0;
                    if (IsUserAlive() && abs(CurrentTime) > EPSILON && abs(CurrentTime - IsAttackLastTime) < 0.05 &&
                        abs(IsAttackLastTime - CurrentTime) > EPSILON)
                    {
                        AttackErrors++;
                    }

                    // Stop attack delay > 2 frames
                    if (IsUserAlive() && !IsCmdChangeWeapon() && !IsRealChangeWeapon() && abs(CurrentTime) > EPSILON &&
                        abs(CurrentTime - IsAttackLastTime) > 0.15 && abs(CurrentTime - IsNoAttackLastTime) > 0.15 &&
                        abs(IsAttackLastTime - CurrentTime) > EPSILON && IsInAttack() &&
                        CurrentNetMsgFrameId - StopAttackBtnFrameId > 2 && StopAttackBtnFrameId != 0 && NeedSearchAim3)
                    {
                        NeedSearchAim3 = false;
                        DemoScanner_AddWarn(
                            "[AIM TYPE 3 " + CurrentWeapon + "] at (FRAME:" + StopAttackBtnFrameId + "):" +
                            CurrentTimeString, false);
                    }

                    NeedIgnoreAttackFlag = 1;
                    NeedSearchAim2 = false;
                    NeedWriteAim = 0;
                    IsNoAttackLastTime = CurrentTime;
                    IsNoAttackLastTime2 = CurrentTime2;
                    IsAttack = 0;
                    AttackCheck = -1;
                    Aim2AttackDetected = false;
                    SelectSlot--;
                    if (ShotFound > 2)
                    {
                        ReallyAim2 = 1;
                    }
                    else if (ShotFound > 1)
                    {
                        ReallyAim2 = 2;
                    }

                    ShotFound = -1;
                    AttackFloodTimes = 0;
                }
            }

            if (sLower.IndexOf("+duck") > -1)
            {
                if (!IsDuckPressed)
                {
                    if (CurrentIdealJumpsStrike > 6)
                    {
                        CurrentIdealJumpsStrike--;
                    }
                }

                //DuckHack4Search = 0;
                FrameCrash = 0;
                IsDuckPressed = true;
                DuckStrikes++;
                FirstDuck = true;
                LastDuckTime = CurrentTime;
                if (DuckStrikes == 8)
                {
                    if (abs(CurrentTime - LastKreedzHackTime) > 20.0)
                    {
                        if (DemoScanner_AddWarn("[DUCK FLOOD TYPE 1] at (" + CurrentTime + ") " + CurrentTimeString))
                        {
                            KreedzHacksCount++;
                            LastKreedzHackTime = CurrentTime;
                        }
                    }
                }
            }
            else if (sLower.IndexOf("-duck") > -1)
            {
                //DuckHack4Search = 0;
                IsDuckPressed = false;
                FirstDuck = true;
                LastUnDuckTime = CurrentTime;
                DuckStrikes = 0;
            }

            if (DetectStrafeOptimizerStrikes > 5 && !IsPlayerTeleport())
            {
                if (!StrafeOptimizerFalse && StrafeAngleDirectionChanges > 4)
                {
                    if (DemoScanner_AddWarn(
                        "[STRAFE OPTIMIZER" + /*(DetectStrafeOptimizerStrikes <= 5 ? " ( WARN ) " : "") +*/ "] at (" +
                        CurrentTime + ") : " + CurrentTimeString))
                    {
                        KreedzHacksCount++;
                    }
                }

                /* if (DEBUG_ENABLED)
                     Console.WriteLine(" --- DIR CHANGE : " + StrafeAngleDirectionChanges + " --- ");
                */
                if (DetectStrafeOptimizerStrikes > 5)
                {
                    StrafeOptimizerFalse = false;
                    StrafeAngleDirectionChanges = 0;
                    DetectStrafeOptimizerStrikes = 0;
                }
            }

            if (sLower.IndexOf("+forward") > -1)
            {
                InForward = true;
                LastMoveForward = CurrentTime;
                StrafeOptimizerFalse = false;
            }
            else if (sLower.IndexOf("-forward") > -1)
            {
                StrafeOptimizerFalse = true;
                if (InForward && DetectStrafeOptimizerStrikes >= 3)
                {
                    DetectStrafeOptimizerStrikes--;
                    DetectStrafeOptimizerStrikes--;
                }

                InForward = false;
                LastUnMoveForward = CurrentTime;
            }

            if (sLower.IndexOf("+back") > -1)
            {
                InBack = true;
                LastMoveBack = CurrentTime;
            }
            else if (sLower.IndexOf("-back") > -1)
            {
                InBack = false;
                LastMoveBack = CurrentTime;
            }

            if (sLower.IndexOf("+strafe") > -1)
            {
                InStrafe = true;
                LastStrafeEnabled = CurrentTime;
            }
            else if (sLower.IndexOf("-strafe") > -1)
            {
                InStrafe = false;
                LastStrafeDisabled = CurrentTime;
            }

            if (sLower.IndexOf("+mlook") > -1)
            {
                InLook = true;
                LastLookEnabled = CurrentTime;
            }
            else if (sLower.IndexOf("-mlook") > -1)
            {
                InLook = false;
                LastLookDisabled = CurrentTime;
            }

            if (sLower.IndexOf("+use") > -1)
            {
                LastUseTime = CurrentTime;
            }

            if (sLower.IndexOf("+moveleft") > -1)
            {
                if (RealAlive && (!CurrentFrameOnGround || CurrentFrameJumped))
                {
                    if (abs(CurrentTime - LastUnMoveRight) < EPSILON && abs(CurrentTime - LastMoveRight) < 1.00f)
                    {
                        /*if (DEBUG_ENABLED)
                            Console.WriteLine("11111:" + AngleStrikeDirection);
                        */
                        if (!(AngleStrikeDirection < 2 && AngleStrikeDirection > -90))
                        {
                            StrafeOptimizerFalse = true;
                        }

                        if (abs(CurrentTime - LastMoveRight) > 0.50f || abs(CurrentTime - LastMoveRight) < 0.01f)
                        {
                            StrafeOptimizerFalse = true;
                        }

                        NeedCheckAngleDiffForStrafeOptimizer = true;
                        LastStrafeOptimizerWarnTime = CurrentTime;
                        DetectStrafeOptimizerStrikes++;
                    }
                    else
                    {
                        StrafeOptimizerFalse = false;
                        DetectStrafeOptimizerStrikes = 0;
                        StrafeAngleDirectionChanges = 0;
                    }
                }
                else
                {
                    StrafeOptimizerFalse = false;
                    DetectStrafeOptimizerStrikes = 0;
                    StrafeAngleDirectionChanges = 0;
                }

                MoveLeft = true;
                LastMoveLeft = CurrentTime;
            }
            else if (sLower.IndexOf("-moveleft") > -1)
            {
                if (RealAlive && (!CurrentFrameOnGround || CurrentFrameJumped))
                {
                    if (LastCmd.ToLower().IndexOf("-moveright") > -1)
                    {
                        if (DetectStrafeOptimizerStrikes > 1)
                        {
                            StrafeOptimizerFalse = true;
                        }
                    }
                }
                else
                {
                    StrafeOptimizerFalse = false;
                    DetectStrafeOptimizerStrikes = 0;
                    StrafeAngleDirectionChanges = 0;
                }

                MoveLeft = false;
                LastUnMoveLeft = CurrentTime;
            }
            else if (sLower.IndexOf("+moveright") > -1)
            {
                if (RealAlive && (!CurrentFrameOnGround || CurrentFrameJumped) &&
                    abs(CurrentTime - LastUnMoveLeft) < EPSILON && abs(CurrentTime - LastMoveLeft) < 1.00f)
                {
                    if (!(AngleStrikeDirection > -2 && AngleStrikeDirection < 90))
                    {
                        StrafeOptimizerFalse = true;
                    }

                    if (abs(CurrentTime - LastMoveLeft) > 0.50f || abs(CurrentTime - LastMoveLeft) < 0.01)
                    {
                        StrafeOptimizerFalse = true;
                    }

                    NeedCheckAngleDiffForStrafeOptimizer = true;
                    LastStrafeOptimizerWarnTime = CurrentTime;
                    DetectStrafeOptimizerStrikes++;
                }
                else
                {
                    StrafeOptimizerFalse = false;
                    DetectStrafeOptimizerStrikes = 0;
                    StrafeAngleDirectionChanges = 0;
                }

                MoveRight = true;
                LastMoveRight = CurrentTime;
            }
            else if (sLower.IndexOf("-moveright") > -1)
            {
                if (RealAlive && (!CurrentFrameOnGround || CurrentFrameJumped))
                {
                    if (LastCmd.ToLower().IndexOf("-moveleft") > -1)
                    {
                        if (DetectStrafeOptimizerStrikes > 1)
                        {
                            StrafeOptimizerFalse = true;
                        }
                    }
                }
                else
                {
                    StrafeOptimizerFalse = false;
                    DetectStrafeOptimizerStrikes = 0;
                    StrafeAngleDirectionChanges = 0;
                }

                MoveRight = false;
                LastUnMoveRight = CurrentTime;
            }

            if (sLower.IndexOf("+jump") > -1)
            {
                SearchJumpBug = true;
                FrameCrash = 0;
                FirstJump = true;

                if (RealAlive)
                {
                    FlyJumps++;
                    NeedDetectBHOPHack = true;
                }
                else
                {
                    NeedDetectBHOPHack = false;
                }

                if (IsUserAlive())
                {
                    if (BHOP_JumpWarn > 2)
                    {
                        if (abs(CurrentTime - LastBhopTime) > 1.0f)
                        {
                            if (DemoScanner_AddWarn("[BHOP TYPE 1.2] at (" + CurrentTime + ") " + CurrentTimeString + " [" +
                                                (BHOP_JumpWarn - 1) + "]" + " times."))
                            {
                                BHOPcount += BHOP_JumpWarn - 1;
                                LastBhopTime = CurrentTime;
                                BHOP_JumpWarn = 0;
                            }
                        }
                    }

                    if (BHOP_GroundWarn > 2)
                    {
                        if (abs(CurrentTime - LastBhopTime) > 0.75f && abs(CurrentTime - LastBhopTime) < 2.5f)
                        {
                            if (DemoScanner_AddWarn(
                                "[BHOP TYPE 2.2] at (" + CurrentTime + ") " + CurrentTimeString + " [" +
                                (BHOP_GroundWarn - 1) + "]" + " times.", BHOP_GroundWarn > 3, BHOP_GroundWarn > 3))
                            {
                                BHOPcount += BHOP_GroundWarn - 1;
                                LastBhopTime = CurrentTime;
                                BHOP_GroundWarn = 0;
                            }
                        }
                    }

                    JumpCount++;
                    if (abs(LastJumpTime - CurrentTime) < EPSILON && abs(LastJumpTime) > EPSILON)
                    {
                        MouseJumps++;
                    }

                    if (IsJump)
                    {
                        MouseJumps++;
                    }
                }

                if (abs(CurrentTime) < 0.01 || CurrentTime < LastJumpTime)
                {
                    BadTimeFound += 10;
                }

                LastJumpTime = CurrentTime;
                LastJumpFrame = CurrentFrameId;
                JumpHackCount2 = -1;
                IsJump = true;
                BHOP_GroundWarn = 0;
                BHOP_JumpWarn = 0;
                BHOP_GroundSearchDirection = 0;
            }

            if (sLower.IndexOf("-jump") > -1)
            {
                FirstJump = true;
                if (IsUserAlive())
                {
                    JumpCount4++;
                    if (abs(LastJumpTime - CurrentTime) < EPSILON && abs(LastJumpTime) > EPSILON)
                    {
                        MouseJumps++;
                        LastJumpHackFalseDetectionTime = CurrentTime;
                    }

                    if (!IsJump && SearchJumpBug)
                    {
                        JumpWithAlias++;
                    }

                    if (BHOP_JumpWarn > 2)
                    {
                        if (abs(CurrentTime - LastBhopTime) > 1.0f)
                        {
                            if (DemoScanner_AddWarn("[BHOP TYPE 1.1] at (" + CurrentTime + ") " + CurrentTimeString + " [" +
                                                (BHOP_JumpWarn - 1) + "]" + " times."))
                            {
                                BHOPcount += BHOP_JumpWarn - 1;
                                LastBhopTime = CurrentTime;
                                BHOP_JumpWarn = 0;
                            }
                        }
                    }

                    if (BHOP_GroundWarn > 2)
                    {
                        if (abs(CurrentTime - LastBhopTime) > 0.75f && abs(CurrentTime - LastBhopTime) < 2.5f)
                        {
                            if (DemoScanner_AddWarn(
                                "[BHOP TYPE 2.1] at (" + CurrentTime + ") " + CurrentTimeString + " [" +
                                (BHOP_GroundWarn - 1) + "]" + " times.", BHOP_GroundWarn > 3, BHOP_GroundWarn > 3))
                            {
                                BHOPcount += BHOP_GroundWarn - 1;
                                LastBhopTime = CurrentTime;
                                BHOP_GroundWarn = 0;
                            }
                        }
                    }

                    LastJumpTime = CurrentTime;
                }

                LastUnJumpTime = CurrentTime;
                JumpHackCount2 = 0;
                IsJump = false;
                BHOP_GroundWarn = 0;
                BHOP_JumpWarn = 0;
                BHOP_GroundSearchDirection = 0;
                NeedDetectBHOPHack = false;
            }

            if (!isstuff)
            {
                LastCmd = s;
                LastCmdTime = CurrentTime;
                LastCmdTimeString = CurrentTimeString;
                LastCmdFrameId = CurrentFrameId;
            }
        }

        public static float GetDistance(FPoint p1, FPoint p2)
        {
            var xDelta = p2.X - p1.X;
            var yDelta = p2.Y - p1.Y;
            return abs((float)Math.Sqrt(Math.Pow(xDelta, 2) + (float)Math.Pow(yDelta, 2)));
        }

        public static float GetDistanceAngle(FPoint p1, FPoint p2)
        {
            var xDelta = AngleBetween(p2.X, p1.X);
            var yDelta = AngleBetween(p2.Y, p1.Y);
            return abs((float)Math.Sqrt(Math.Pow(xDelta, 2) + (float)Math.Pow(yDelta, 2)));
        }

        public static void PrintNodesRecursive(MyTreeNode oParentNode)
        {
            outFrames.Add(oParentNode.Text);
            foreach (MyTreeNode oSubNode in oParentNode.Nodes)
            {
                PrintNodesRecursive(oSubNode);
            }
        }

        public static bool IsRealWeapon()
        {
            var retval = CurrentWeapon != WeaponIdType.WEAPON_NONE && CurrentWeapon != WeaponIdType.WEAPON_BAD &&
                         CurrentWeapon != WeaponIdType.WEAPON_BAD2;
            //Console.WriteLine("Weapon:" + retval);
            return retval;
        }

        public static bool IsUserAlive()
        {
            return RealAlive && IsRealWeapon();
        }

        public static string Truncate(string value, int maxLength)
        {
            return string.IsNullOrEmpty(value) ? value :
                value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        public static byte[] GetNullBytes(int len)
        {
            var retval = new byte[len];
            Array.Clear(retval, 0, retval.Length);
            return retval;
        }

        public static void AddViewDemoHelperComment(string comment, float speed = 0.5f)
        {
            ViewDemoCommentCount++;
            var utf8 = Encoding.UTF8.GetBytes(comment);
            ViewDemoHelperComments.Write(CurrentTime);
            ViewDemoHelperComments.Write(speed);
            ViewDemoHelperComments.Write(xcommentdata);
            ViewDemoHelperComments.Write(utf8.Length + 1);
            ViewDemoHelperComments.Write(utf8);
            ViewDemoHelperComments.Write(GetNullBytes(1));
        }

        public static float CalcFov(float fov_x, float width, float height)
        {
            if (abs(width * 3 - 4 * height) < EPSILON || abs(width * 4 - height * 5) < EPSILON)
            {
                double v3 = fov_x, v4 = 0.0;
                if (fov_x < 1.0)
                {
                    v4 = 0.9999999999999999;
                }
                else if (v3 <= 179.0)
                {
                    v4 = Math.Tan(v3 / 360.0 * Math.PI);
                }
                else
                {
                    v4 = 0.9999999999999999;
                }

                return Convert.ToSingle(Math.Atan(height / (width / v4)) * 360.0 / Math.PI);
            }

            var new_y = Convert.ToSingle(2.0f *
                Math.Atan(Math.Tan(fov_x * Math.PI / 180.0f / 2.0f) * 0.75f /*768.0/1024.0*/) * 180.0f / Math.PI);
            return Convert.ToSingle(2.0f * Math.Atan(Math.Tan(new_y * Math.PI / 180.0f / 2.0f) * width / height) *
                180.0f / Math.PI);
        }

        public static void AddResolution(int x, int y)
        {
            if (x != 0 && y != 0)
            {
                foreach (var s in playerresolution)
                {
                    if (s.x == x && s.y == y)
                    {
                        return;
                    }
                }

                var tmpres = new WindowResolution { x = x, y = y };
                playerresolution.Add(tmpres);
            }

            LastResolutionX = x;
            LastResolutionY = y;
        }

        public static string TrimBad(this string value)
        {
            return value.Replace("\n", "").Replace("\r", "");
        }

        public static string GetAim7String(ref int val1, ref int val2, ref int val3, int type, float angle,
            ref bool detect)
        {
            if (val1 > 11)
            {
                val1 = 11;
            }

            if (val2 > 11)
            {
                val2 = 11;
            }

            val1 -= 1;
            val2 -= 1;
            val1 *= 10;
            val2 *= 10;
            if (angle < 0.0001f)
            {
                val3 = 10;
            }
            else if (angle < 0.001f)
            {
                val3 = 20;
            }
            else if (angle < 0.01f)
            {
                val3 = 30;
            }
            else if (angle < 0.1f)
            {
                val3 = 40;
            }
            else if (angle < 1.0f)
            {
                val3 = 50;
            }
            else if (angle < 2.0f)
            {
                val3 = 60;
            }
            else if (angle < 3.0f)
            {
                val3 = 70;
            }
            else if (angle < 4.0f)
            {
                val3 = 80;
            }
            else if (angle < 10.0f)
            {
                val3 = 90;
            }
            else
            {
                val3 = angle < 50.0f ? 100 : 1000;
            }

            if (type - 1 == 4)
            {
                detect = false;
                if (val3 > 50)
                {
                    detect = true;
                }
            }

            if (abs(Aim7PunchangleY) > EPSILON)
            {
                detect = false;
                val1 = 5;
                val2 = 5;
            }

            if (val1 <= 10)
            {
                detect = false;
                val1 = 5;
            }

            if (val2 <= 10)
            {
                detect = false;
                val2 = 5;
            }

            if (IsForceCenterView())
            {
                val1 = 5;
                val2 = 5;
                val3 = 5;
            }

            return "[AIM TYPE 7." + (type - 1) + " " + CurrentWeapon + " P1:" + val1 + "% P2:" + val2 + "% P3:" + val3 + "%]";
        }

        public static string GetTimeString(float time)
        {
            try
            {
                var t = TimeSpan.FromSeconds(CurrentTime);
                return string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms", t.Hours, t.Minutes, t.Seconds, t.Milliseconds);
            }
            catch
            {
            }

            return "00h:00m:00s";
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            try
            {
                Process.GetCurrentProcess().Kill();
            }
            catch
            {

            }
        }

        [STAThread]
        private static void Main(string[] args)
        {
            try
            {
                Console.CancelKeyPress += Console_CancelKeyPress;
                RunDemoScanner(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.DarkRed;
                if (IsRussia)
                {
                    Console.WriteLine("Сканер не может продолжить работу, обнаружена критическая ошибка!");
                }
                else
                {
                    Console.WriteLine("Critical error. Analyzing can not be continued.");
                }
                Console.WriteLine(ex.Message);
            }

        }

        public static void RunDemoScanner(string[] args)
        {
            try
            {
#if !NET6_0_OR_GREATER
                NativeConsoleMethods.CenterConsole();
#endif
                Console.SetWindowSize(114, 32);
                Console.SetBufferSize(114, 5555);
                Console.SetWindowSize(115, 32);
#if !NET6_0_OR_GREATER
                NativeConsoleMethods.CenterConsole();
#endif
            }
            catch
            {
                Console.WriteLine("Error in console settings!");
            }

            //new EntitiesPreviewWindow("qwerqwer").ShowDialog();
            //return;
            try
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Title = "[ANTICHEAT/ANTIHACK] " + PROGRAMNAME + " " + PROGRAMVERSION + ". Demo:" + DemoName +
                                "." + TotalFreewareTool;
            }
            catch
            {
                Console.WriteLine("Error Fatal");
                return;
            }

            var originalOut = Console.Out;

            var CurrentDemoFilePath = "";
            var filefound = false;
        DEMO_FULLRESET:
            foreach (var arg in args)
            {
                if (!filefound)
                {
                    CurrentDemoFilePath = arg.Replace("\"", "");
                    if (File.Exists(CurrentDemoFilePath))
                    {
                        filefound = true;
                    }
                }

                if (arg.IndexOf("-debug") > -1)
                {
                    DEBUG_ENABLED = true;
                    Console.WriteLine("Debug mode activated.");
                }
                else if (arg.IndexOf("-noteleport") > -1)
                {
                    NO_TELEPORT = true;
                    Console.WriteLine("Ignore teleport mode activated.");
                }
                else if (arg.IndexOf("-dump") > -1)
                {
                    DUMP_ALL_FRAMES = true;
                    Console.WriteLine("Dump mode activated.");
                }
                else if (arg.IndexOf("-log") > -1)
                {
                    SKIP_RESULTS = true;
                    LOG_MODE = true;
                    Console.WriteLine("Log mode activated.");
                    Console.SetOut(new IgnoreConsoleWriter());
                }
                else if (arg.IndexOf("-skip") > -1)
                {
                    SKIP_RESULTS = true;
                }
                else if (arg.IndexOf("-pfrm") > -1)
                {

#if NET6_0_OR_GREATER
                    Console.WriteLine("ONLY WINDOWS IS SUPPORTED!");
#else
                    PREVIEW_FRAMES = true;
                    Console.WriteLine("PREVIEW FRAME MODE");
#endif
                }
                else if (arg.IndexOf("-pent") > -1)
                {
                    PREVIEW_ENTS = true;
                    Console.WriteLine("PREVIEW ENT MODE");
                }
                else if (arg.IndexOf("-alive") > -1)
                {
                    CurrentWeapon = WeaponIdType.WEAPON_AK47;
                    CurrentFrameAlive = true;
                    PreviousFrameAlive = true;
                    LastAliveTime = 1.0f;
                    RealAlive = true;
                    UserAlive = true;
                    FirstUserAlive = false;
                    if (IsRussia)
                    {
                        Console.WriteLine("ИГРОК ОТМЕЧЕН ЖИВЫМ С НАЧАЛА ДЕМО!");
                    }
                    else
                    {
                        Console.WriteLine("SCAN WITH FORCE USER ALIVE AT START DEMO!");
                    }
                }
            }

            if (!SKIP_RESULTS && !DemoRescanned)
            {
                try
                {
                    Console.WriteLine("Search for updates...(hint: to change language, need remove lang file!)");
                    Task.Run(async () =>
                    {
                        HttpClient client = new HttpClient();
                        client.Timeout = TimeSpan.FromMilliseconds(3000);
                        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://raw.githubusercontent.com/UnrealKaraulov/UnrealDemoScanner/main/UnrealDemoScanner/UnrealDemoScanner.cs");
                        request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(0, 2048);

                        using (HttpResponseMessage response = await client.SendAsync(request))
                        {
                            string str_from_github = await response.Content.ReadAsStringAsync();
                            Console.Clear();
                            if (str_from_github.IndexOf("PROGRAMVERSION") > 0)
                            {
                                var regex = new Regex(@"PROGRAMVERSION\s+=\s+""(.*?)"";");
                                var match = regex.Match(str_from_github);
                                if (match.Success)
                                {
                                    if (match.Groups[1].Value != PROGRAMVERSION)
                                    {
                                        Console.WriteLine($"Found new version \"{match.Groups[1].Value}\"! Current version:\"{PROGRAMVERSION}\".");
                                    }
                                }
                            }
                        }
                    }).Wait();
                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }

            var CurrentDir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName).Replace("\\", "/");
            if (CurrentDir.Length > 0 && (CurrentDir.EndsWith("\\") || CurrentDir.EndsWith("/")))
            {
                CurrentDir.Remove(CurrentDir.Length - 1);
            }

            if (!SKIP_RESULTS)
            {
                if (!File.Exists(CurrentDir + "/lang.ru") && !File.Exists(CurrentDir + "/lang.en"))
                {
                    Console.Write("Enter language EN - Engish / RU - Russian:");
                    var lang = Console.ReadLine();
                    if (lang.ToLower() == "en")
                    {
                        File.Create(CurrentDir + "/lang.en").Close();
                    }
                    else
                    {
                        File.Create(CurrentDir + "/lang.ru").Close();
                    }
                }

                IsRussia = !File.Exists(CurrentDir + "/lang.en");
            }
            else
            {
                if (LOG_MODE)
                {
                    IsRussia = !File.Exists(CurrentDir + "/lang.en");
                }
                else
                {
                    IsRussia = false;
                }
            }

            try
            {
                string codestring = GetSourceCodeString();
                outFrames = new List<string>();
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(PROGRAMNAME + " " + PROGRAMVERSION + " " + TotalFreewareTool);
                Console.ForegroundColor = ConsoleColor.Yellow;
                if (IsRussia)
                {
                    Console.WriteLine("Скачивайте последнюю версию сканера и модуля по ссылке :");
                }
                else
                {
                    Console.WriteLine("Download latest version demo scanner and server plugin :");
                }

                Console.WriteLine(codestring);
                Console.ForegroundColor = ConsoleColor.Red;
                if (IsRussia)
                {
                    Console.WriteLine("БАЗА ДАННЫХ СОДЕРЖИТ СЛЕДУЮЩИЕ ВИДЫ ЧИТОВ И ХАКОВ:");
                }
                else
                {
                    Console.WriteLine("THIS BASE CONTAIN NEXT CHEAT/HACK:");
                }
            }
            catch
            {
                Console.WriteLine("Error fatal");
                return;
            }

            Console.WriteLine("[TRIGGER BOT]        [AIMBOT]        [BHOP]");
            Console.WriteLine("[JUMPHACK]           [STRAFEHACK]    [MOVEMENTHACK]");
            Console.WriteLine("[WHEELJUMP]          [AUTORELOAD]    [NORELOAD]");
            Console.WriteLine("[FASTRUN]            [KNIFEBOT]      [AND MORE OTHER]");
            Console.ForegroundColor = ConsoleColor.Gray;
            /* WHITELIST CMDS */
            whiteListCMDLIST.Add("forward");
            whiteListCMDLIST.Add("autobuy");
            whiteListCMDLIST.Add("attack");
            whiteListCMDLIST.Add("moveright");
            whiteListCMDLIST.Add("back");
            whiteListCMDLIST.Add("moveleft");
            whiteListCMDLIST.Add("duck");
            whiteListCMDLIST.Add("reload");
            whiteListCMDLIST.Add("invnext");
            whiteListCMDLIST.Add("invprev");
            whiteListCMDLIST.Add("slot1");
            whiteListCMDLIST.Add("speed");
            whiteListCMDLIST.Add("showscores");
            whiteListCMDLIST.Add("buy");
            whiteListCMDLIST.Add("attack2");
            whiteListCMDLIST.Add("slot2");
            whiteListCMDLIST.Add("slot3");
            whiteListCMDLIST.Add("slot4");
            whiteListCMDLIST.Add("slot5");
            whiteListCMDLIST.Add("slot6");
            whiteListCMDLIST.Add("slot7");
            whiteListCMDLIST.Add("slot8");
            whiteListCMDLIST.Add("slot9");
            whiteListCMDLIST.Add("slot10");
            whiteListCMDLIST.Add("strafe");
            whiteListCMDLIST.Add("jump");
            whiteListCMDLIST.Add("moveup");
            whiteListCMDLIST.Add("movedown");
            whiteListCMDLIST.Add("mlook");
            whiteListCMDLIST.Add("use");
            whiteListCMDLIST.Add("commandmenu");
            whiteListCMDLIST.Add("left");
            whiteListCMDLIST.Add("right");
            whiteListCMDLIST.Add("klook");
            whiteListCMDLIST.Add("lookdown");
            whiteListCMDLIST.Add("lookup");
            whiteListCMDLIST.Add("impulse");
            whiteListCMDLIST.Add("rebuy");
            whiteListCMDLIST.Add("drawradar");
            whiteListCMDLIST.Add("hideradar");
            whiteListCMDLIST.Add("jlook");
            whiteListCMDLIST.Add("spec_menu");
            whiteListCMDLIST.Add("force_centerview");
            whiteListCMDLIST.Add("adjust_crosshair");
            whiteListCMDLIST.Add("joyadvancedupdate");
            whiteListCMDLIST.Add("cancelselect");
            whiteListCMDLIST.Add("trackplayer");
            whiteListCMDLIST.Add("clearplayers");
            whiteListCMDLIST.Add("gunsmoke");
            whiteListCMDLIST.Add("credits");
            whiteListCMDLIST.Add("spec_mode");
            whiteListCMDLIST.Add("spec_toggleinset");
            whiteListCMDLIST.Add("spec_decal");
            whiteListCMDLIST.Add("spec_help");
            whiteListCMDLIST.Add("togglescores");
            whiteListCMDLIST.Add("spec_drawnames");
            whiteListCMDLIST.Add("spec_drawcone");
            whiteListCMDLIST.Add("spec_autodirector");
            whiteListCMDLIST.Add("spec_drawstatus");
            whiteListCMDLIST.Add("campitchup");
            whiteListCMDLIST.Add("campitchdown");
            whiteListCMDLIST.Add("camyawleft");
            whiteListCMDLIST.Add("camyawright");
            whiteListCMDLIST.Add("camin");
            whiteListCMDLIST.Add("camout");
            whiteListCMDLIST.Add("thirdperson");
            whiteListCMDLIST.Add("firstperson");
            whiteListCMDLIST.Add("cammousemove");
            whiteListCMDLIST.Add("camdistance");
            whiteListCMDLIST.Add("snapto");
            whiteListCMDLIST.Add("alt1");
            whiteListCMDLIST.Add("score");
            whiteListCMDLIST.Add("graph");
            whiteListCMDLIST.Add("break");
            whiteListCMDLIST.Add("nvgadjust");
            whiteListCMDLIST.Add("hud_saytext");
            whiteListCMDLIST.Add("buy_preset_edit");
            whiteListCMDLIST.Add("voice_showbanned");
            whiteListCMDLIST.Add("vgui_runscript");
            // csldr
            whiteListCMDLIST.Add("lookat");

            if (IsRussia)
            {
                Console.WriteLine("Перетащите демо файл в это окно или введите путь вручную:");
            }
            else
            {
                Console.WriteLine("Drag & drop .dem/.pfrm/.efrm file. Or enter path manually:");
            }

            if (!filefound && !SKIP_RESULTS)
            {
                while (!File.Exists(CurrentDemoFilePath))
                {
                    CurrentDemoFilePath = Console.ReadLine().Replace("\"", "");
                    if (CurrentDemoFilePath.IndexOf("-debug") == 0)
                    {
                        DEBUG_ENABLED = true;
                        Console.WriteLine("Debug mode activated.");
                    }
                    else if (CurrentDemoFilePath.IndexOf("-noteleport") == 0)
                    {
                        NO_TELEPORT = true;
                        Console.WriteLine("Ignore teleport mode activated.");
                    }
                    else if (CurrentDemoFilePath.IndexOf("-dump") == 0)
                    {
                        DUMP_ALL_FRAMES = true;
                        Console.WriteLine("Dump mode activated.");
                    }
                    else if (CurrentDemoFilePath.IndexOf("-pfrm") == 0)
                    {
#if NET6_0_OR_GREATER
                        Console.WriteLine("ONLY WINDOWS IS SUPPORTED!");
#else
                        PREVIEW_FRAMES = true;
                        Console.WriteLine("PREVIEW FRAME MODE");
#endif
                    }
                    else if (CurrentDemoFilePath.IndexOf("-pent") == 0)
                    {
                        PREVIEW_ENTS = true;
                        Console.WriteLine("PREVIEW ENT MODE");
                    }
                    else if (CurrentDemoFilePath.IndexOf("-alive") == 0)
                    {
                        CurrentWeapon = WeaponIdType.WEAPON_AK47;
                        CurrentFrameAlive = true;
                        PreviousFrameAlive = true;
                        LastAliveTime = 1.0f;
                        RealAlive = true;
                        UserAlive = true;
                        FirstUserAlive = false;
                        Console.WriteLine("SCAN WITH FORCE USER ALIVE AT START DEMO!");
                    }
                }
            }
            else if (!filefound)
            {
                Console.WriteLine("[ERROR] NO FILE FOUND! ERROR!");
                return;
            }
#if !NET6_0_OR_GREATER
            if (CurrentDemoFilePath.ToLower().EndsWith(".pfrm"))
            {
                var tmpPreview = new Preview(CurrentDemoFilePath);
                tmpPreview.ShowDialog();
                return;
            }

            if (CurrentDemoFilePath.ToLower().EndsWith(".pent"))
            {
                var tmpPreview = new EntitiesPreviewWindow(CurrentDemoFilePath);
                tmpPreview.ShowDialog();
                return;
            }
#endif
            if (!CurrentDemoFilePath.ToLower().EndsWith(".dem"))
            {
                Console.WriteLine("[ERROR] INVALID FILE EXTENSION!");
                return;
            }

            StartScanTime = DateTime.Now;
            try
            {
                if (DUMP_ALL_FRAMES)
                {
                    File.Delete(CurrentDemoFilePath.Remove(CurrentDemoFilePath.Length - 3) + "_Frames.txt");
                    File.Create(CurrentDemoFilePath.Remove(CurrentDemoFilePath.Length - 3) + "_Frames.txt").Close();
                    if (File.Exists(CurrentDemoFilePath.Remove(CurrentDemoFilePath.Length - 3) + "_Frames.txt"))
                    {
                        File.AppendAllText(CurrentDemoFilePath.Remove(CurrentDemoFilePath.Length - 3) + "_Frames.txt",
                            "Полный дамп демо в текстовом формате\n");
                    }
                }
            }
            catch
            {
                Console.WriteLine("Error fatal");
                return;
            }

            if (TotalFreewareTool.Length != 37)
            {
                return;
            }

            if (PREVIEW_FRAMES)
            {
                try
                {
                    if (File.Exists(CurrentDemoFilePath + ".pfrm"))
                    {
                        File.Delete(CurrentDemoFilePath + ".pfrm");
                    }

                    PreviewFramesWriter = new BinaryWriter(File.OpenWrite(CurrentDemoFilePath + ".pfrm"));
                    PreviewFramesWriter.BaseStream.Seek(0, SeekOrigin.Begin);
                }
                catch
                {
                    PREVIEW_FRAMES = false;
                    Console.WriteLine("Cant generate pfrm file");
                }
            }

            var CurrentDemoFile = CrossDemoParser.Parse(CurrentDemoFilePath);
            if (File.Exists(CurrentDemoFilePath.Remove(CurrentDemoFilePath.Length - 3) + "log"))
            {
                if (File.Exists(CurrentDemoFilePath.Remove(CurrentDemoFilePath.Length - 3) + "log.bak"))
                {
                    try
                    {
                        File.Delete(CurrentDemoFilePath.Remove(CurrentDemoFilePath.Length - 3) + "log.bak");
                    }
                    catch
                    {
                        Console.WriteLine("Error: No access to log file.");
                    }
                }

                try
                {
                    File.Delete(CurrentDemoFilePath.Remove(CurrentDemoFilePath.Length - 3) + "log");
                }
                catch
                {
                    Console.WriteLine("File " + CurrentDemoFilePath.Remove(CurrentDemoFilePath.Length - 3) + "log" +
                                      " open error : No access to remove!");
                    Console.Write("[ERROR] No access to file... Try again!");
                    if (!SKIP_RESULTS)
                    {
                        Console.ReadKey();
                    }

                    return;
                }
            }

            if (SourceCode.Length != 51)
            {
                return;
            }

            ViewDemoHelperComments = new BinaryWriter(new MemoryStream());
            ViewDemoHelperComments.Write(1751611137);
            ViewDemoHelperComments.Write(0);
            Console.WriteLine("DEMO " + Path.GetFileName(CurrentDemoFilePath));
            DemoName = Truncate(Path.GetFileName(CurrentDemoFilePath), 25);

            Console.ForegroundColor = ConsoleColor.Green;
            if (IsRussia)
            {
                Console.WriteLine("Начинается анализ демо файла. Пожалуйста подождите...");
            }
            else
            {
                Console.WriteLine("Start demo analyze.....");
            }

            var halfLifeDemoParser = new HalfLifeDemoParser(CurrentDemoFile);
            if (usagesrccode != 1)
            {
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            if (IsRussia)
            {
                Console.Write("Протокол демо файла и игры : ");
            }
            else
            {
                Console.Write("Demo / Game protocol : ");
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(CurrentDemoFile.GsDemoInfo.Header.DemoProtocol);
            Console.Write(" / ");
            Console.WriteLine(CurrentDemoFile.GsDemoInfo.Header.NetProtocol);
            Console.ForegroundColor = ConsoleColor.Green;
            if (IsRussia)
            {
                Console.Write("Игра : ");
            }
            else
            {
                Console.Write("Game : ");
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(CurrentDemoFile.GsDemoInfo.Header.GameDir);
            Console.ForegroundColor = ConsoleColor.Green;
            if (IsRussia)
            {
                Console.Write("  Карта : ");
            }
            else
            {
                Console.Write("  Map : ");
            }

            if (!SKIP_RESULTS)
            {
                MapAndCrc32_Top = Console.CursorTop;
                MapAndCrc32_Left = Console.CursorLeft;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("\"maps/" + CurrentDemoFile.GsDemoInfo.Header.MapName + ".bsp\" ");
            MapName = "maps/" + CurrentDemoFile.GsDemoInfo.Header.MapName + ".bsp";
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            if (IsRussia)
            {
                Console.Write("Никнейм и стим : ");
            }
            else
            {
                Console.Write("Username and steamid : ");
            }

            if (!SKIP_RESULTS)
            {
                UserNameAndSteamIDField = Console.CursorTop;
                UserNameAndSteamIDField2 = Console.CursorLeft;
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            //if (CurrentDemoFile.GsDemoInfo.DirectoryEntries.Count > 1)
            //{
            //    if (CurrentDemoFile.GsDemoInfo.DirectoryEntries[1].TrackTime > CurrentDemoFile.GsDemoInfo.DirectoryEntries[0].TrackTime)
            //    {
            //        CurrentDemoFile.GsDemoInfo.DirectoryEntries.Reverse();
            //    }
            //}
            for (var index = 0; index < CurrentDemoFile.GsDemoInfo.DirectoryEntries.Count; index++)
            {
                for (var frameindex = 0;
                     frameindex < CurrentDemoFile.GsDemoInfo.DirectoryEntries[index].Frames.Count;
                     frameindex++)
                {
                    if (SkipNextErrors)
                    {
                        SkipNextErrors = false;
                        if (IsRussia)
                        {
                            Console.WriteLine(
                                "Критическая ошибка сканирования[1]. \nРезультаты сканирования могут быть не однозначными...");
                        }
                        else
                        {
                            Console.WriteLine(
                                "Critical error in message parser.[1] \nThe scan result may not be unambiguous...");
                        }
                    }

                    if (NeedSkipDemoRescan == 1)
                    {
                        Console.WriteLine();
                        if (IsRussia)
                        {
                            Console.WriteLine("Извините возникла критическая ошибка при сканировании демо. Повтор..." +
                                              CurrentTimeString);
                        }
                        else
                        {
                            Console.WriteLine("Sorry but need rescan demo! This action is automatically!" +
                                              CurrentTimeString);
                        }

                        Console.WriteLine();
                        /* START: SOME BLACK MAGIC OUTSIDE HOGWARTS */
                        FieldInfo[] fields = typeof(DemoScanner).GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

                        foreach (var field in fields)
                        {
                            try
                            {
                                Type fieldType = field.FieldType;
                                object defaultValue = fieldType.IsValueType ? Activator.CreateInstance(fieldType) : null;
                                field.SetValue(null, defaultValue);
                            }
                            catch
                            {
                            }
                        }
                        typeof(DemoScanner)
                            .GetConstructor(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, null,
                                new Type[0], null).Invoke(null, null);
                        /* END: VERY DARK BLACK MAGIC!!!!!! */
                        DemoRescanned = true;
                        FirstBypassKill = false;
                        NeedSkipDemoRescan = 2;
                        goto DEMO_FULLRESET;
                    }

                    var frame = CurrentDemoFile.GsDemoInfo.DirectoryEntries[index].Frames[frameindex];
                    try
                    {
                        switch (frame.Key.Type)
                        {
                            case GoldSource.DemoFrameType.NetMsg:
                                if (abs(((GoldSource.NetMsgFrame)frame.Value).RParms.Time) > EPSILON)
                                {
                                    if (abs(((GoldSource.NetMsgFrame)frame.Value).RParms.Time) > EPSILON &&
                                        abs(GameStartTime) < EPSILON)
                                    {
                                        GameStartTime = ((GoldSource.NetMsgFrame)frame.Value).RParms.Time;
                                    }

                                    if (((GoldSource.NetMsgFrame)frame.Value).RParms.Time > GameEndTime2 + EPSILON &&
                                        abs(((GoldSource.NetMsgFrame)frame.Value).RParms.Time - GameStartTime) > EPSILON)
                                    {
                                        GameEndTime2 = ((GoldSource.NetMsgFrame)frame.Value).RParms.Time;
                                    }
                                }

                                break;
                            case GoldSource.DemoFrameType.DemoStart:
                                break;
                            case GoldSource.DemoFrameType.ConsoleCommand:
                                break;
                            case GoldSource.DemoFrameType.ClientData:
                                break;
                            case GoldSource.DemoFrameType.NextSection:
                                break;
                            case GoldSource.DemoFrameType.Event:
                                break;
                            case GoldSource.DemoFrameType.WeaponAnim:
                                break;
                            case GoldSource.DemoFrameType.Sound:
                                break;
                            case GoldSource.DemoFrameType.DemoBuffer:
                                break;
                            default:
                                if (abs(((GoldSource.NetMsgFrame)frame.Value).RParms.Time) > EPSILON &&
                                    abs(GameStartTime) < EPSILON)
                                {
                                    GameStartTime = ((GoldSource.NetMsgFrame)frame.Value).RParms.Time;
                                }

                                if (((GoldSource.NetMsgFrame)frame.Value).RParms.Time > GameEndTime2 + EPSILON &&
                                    abs(((GoldSource.NetMsgFrame)frame.Value).RParms.Time - GameStartTime) > EPSILON)
                                {
                                    GameEndTime2 = ((GoldSource.NetMsgFrame)frame.Value).RParms.Time;
                                }

                                break;
                        }
                    }
                    catch
                    {
                        Console.WriteLine("Error parsing game start time.");
                    }
                }
            }

            for (var index = 0; index < CurrentDemoFile.GsDemoInfo.DirectoryEntries.Count; index++)
            {
                NewDirectory = true;
                FrameErrors = LastOutgoingSequence = LastIncomingAcknowledged = LastIncomingSequence = 0;
                var entrynode = new MyTreeNode("Directory entry [" + (index + 1) + "] - " +
                                             CurrentDemoFile.GsDemoInfo.DirectoryEntries[index].Frames.Count);
                TimeShift4Times = new float[3] { 0.0f, 0.0f, 0.0f };
                //int frame_num = 0;
                //int skip_frames_check = 3;
                for (var frameindex = 0;
                     frameindex < CurrentDemoFile.GsDemoInfo.DirectoryEntries[index].Frames.Count;
                     frameindex++)
                {
                    UpdateWarnList();
                    var frame = CurrentDemoFile.GsDemoInfo.DirectoryEntries[index].Frames[frameindex];
                    PreviousTime2 = CurrentTime2;
                    CurrentTime2 = frame.Key.Time;
                    CurrentFrameTimeBetween += abs(CurrentTime2 - PreviousTime2);
                    CurrentFrameIdWeapon++;
                    CurrentFrameIdAll++;
                    var row = index + "/" + frame.Key.FrameIndex + " " + "[" + frame.Key.Time + "s]: " +
                              frame.Key.Type.ToString().ToUpper();

                    var node = new MyTreeNode("");
                    var subnode = new MyTreeNode("");
                    //int tmp_frame_skip = Math.Abs(frame.Key.FrameIndex - frame_num);
                    //if (tmp_frame_skip > 1)
                    //{
                    //    skip_frames_check--;
                    //    if (skip_frames_check <= 0)
                    //    {
                    //        demo_skipped_frames++;
                    //        Console.WriteLine("frame.Key.FrameIndex = " + frame.Key.FrameIndex + " frame_num = " + frame_num + " dir count = " + CurrentDemoFile.GsDemoInfo.DirectoryEntries.Count);
                    //    }
                    //}
                    //frame_num = frame.Key.FrameIndex;
                    switch (frame.Key.Type)
                    {
                        case GoldSource.DemoFrameType.DemoStart:
                            LASTFRAMEISCLIENTDATA = false;
                            break;
                        case GoldSource.DemoFrameType.ConsoleCommand:
                            {
                                var cmdframe = (GoldSource.ConsoleCommandFrame)frame.Value;
                                LASTFRAMEISCLIENTDATA = false;
                                if (DUMP_ALL_FRAMES)
                                {
                                    subnode.Text += "{\n";
                                    subnode.Text += cmdframe.Command;
                                    //subnode.Text += "(" + cmdframe.BxtData.Length + ")";
                                    subnode.Text += "}\n";
                                }

                                CheckConsoleCommand(cmdframe.Command);
                                break;
                            }

                        case GoldSource.DemoFrameType.ClientData:
                            {
                                if (CurrentFrameDuplicated > 0)
                                {
                                    CurrentFrameDuplicated -= 1;
                                }

                                ClientDataCountDemos++;
                                LastClientDataTime = CurrentTime;
                                NeedSearchID = CurrentFrameIdAll;
                                CurrentFrameId++;
                                if (DUMP_ALL_FRAMES)
                                {
                                    subnode.Text += "{\n";
                                }

                                var cdframe = (GoldSource.ClientDataFrame)frame.Value;

                                PREV_CDFRAME_ViewAngles = CDFRAME_ViewAngles;

                                CDFRAME_ViewAngles = cdframe.Viewangles;

                                if (CDAngleHistoryAim12List.Count > 0 &&
                                    abs(CDAngleHistoryAim12List[0].tmp[2] - (-99999.0f)) < 0.1f)
                                {
                                    CDAngleHistoryAim12List[0].tmp[2] =
                                       fullnormalizeangle(CDFRAME_ViewAngles.Y);
                                }

                                oldoriginpos.X = curoriginpos.X;
                                oldoriginpos.Y = curoriginpos.Y;
                                curoriginpos.X = cdframe.Origin.X;
                                curoriginpos.Y = cdframe.Origin.Y;

                                if (DUMP_ALL_FRAMES)
                                {
                                    subnode.Text += "Origin.X = " + cdframe.Origin.X + "\n";
                                    subnode.Text += "Origin.Y = " + cdframe.Origin.Y + "\n";
                                    subnode.Text += "Origin.Z = " + cdframe.Origin.Z + "\n";
                                    subnode.Text += "Viewangles.X = " + cdframe.Viewangles.X + "\n";
                                    subnode.Text += "Viewangles.Y = " + cdframe.Viewangles.Y + "\n";
                                    subnode.Text += "Viewangles.Z = " + cdframe.Viewangles.Z + "\n";
                                    subnode.Text += "WeaponBits = " + cdframe.WeaponBits + "\n";
                                    subnode.Text += "Fov = " + cdframe.Fov + "\n";
                                    subnode.Text += "}\n";
                                }

                                if (DetectCmdHackType10)
                                {
                                    DetectCmdHackType10 = false;
                                    if (RealAlive && abs(CurrentTime - LastDeathTime) > 5.0f &&
                                        abs(CurrentTime - LastAliveTime) > 2.0f)
                                    {
                                        if (abs(CurrentTime - CmdHack10_detecttime) > 30.0f)
                                        {
                                            if (abs(CmdHack10_detecttime) > 0.0001f)
                                            {
                                                if (ThirdHackDetected <= 0)
                                                {
                                                    if (abs(CmdHack10_origX) > 0.001f &&
                                                        abs(CmdHack10_origX - cdframe.Origin.X) > 0.001f)
                                                    {
                                                        DemoScanner_AddWarn(
                                                            "[THIRD PERSON TYPE 2] at (" + CurrentTime + "):" +
                                                            CurrentTimeString,
                                                            false /*!IsAngleEditByEngine() && !IsPlayerLossConnection()*/);
                                                    }

                                                    if (abs(CmdHack10_origY) > 0.001f &&
                                                        abs(CmdHack10_origY - cdframe.Origin.Y) > 0.001f)
                                                    {
                                                        DemoScanner_AddWarn(
                                                            "[THIRD PERSON TYPE 2] at (" + CurrentTime + "):" +
                                                            CurrentTimeString,
                                                            false /*!IsAngleEditByEngine() && !IsPlayerLossConnection()*/);
                                                    }

                                                    if (abs(CmdHack10_origZ) > 0.001f &&
                                                        abs(CmdHack10_origZ - cdframe.Origin.Z) > 0.001f)
                                                    {
                                                        DemoScanner_AddWarn(
                                                            "[THIRD PERSON TYPE 2] at (" + CurrentTime + "):" +
                                                            CurrentTimeString,
                                                            false /*!IsAngleEditByEngine() && !IsPlayerLossConnection()*/);
                                                    }
                                                }

                                                CmdHack10_detecttime = CurrentTime;
                                            }
                                        }
                                    }

                                    CmdHack10_origX = CmdHack10_origY = CmdHack10_origZ = 0.0f;
                                }

                                cdframeFov = cdframe.Fov;
                                if (RealAlive && (CurrentFrameAttacked || CurrentFrameJumped) &&
                                    abs(CurrentTime - LastDeathTime) > 5.0f && abs(CurrentTime - LastAliveTime) > 2.0f)
                                {
                                    if (abs(CurrentTime - FovHackTime) > 30.0f)
                                    {
                                        if (FovHackDetected <= 10 && !FoundNextClient)
                                        {
                                            /* AWP, SCOUT, ETC ... for next update weapon check */
                                            var fov2_clean1 = CalcFov(10.0f, LastResolutionX, LastResolutionY);
                                            var fov2_clean2 = CalcFov(15.0f, LastResolutionX, LastResolutionY);
                                            var fov2_clean3 = CalcFov(40.0f, LastResolutionX, LastResolutionY);
                                            var fov2_clean4 = CalcFov(55.0f, LastResolutionX, LastResolutionY);
                                            var fov_clean1 = 10.0f;
                                            var fov_clean2 = 15.0f;
                                            var fov_clean3 = 40.0f;
                                            var fov_clean4 = 55.0f;
                                            // NORMAL FOV FOR CUSTOM GAME CLIENT
                                            var fov2_clean5 = CalcFov(90.0f, LastResolutionX, LastResolutionY);
                                            var fov_clean5 = 90.0f;
                                            // IF fov found, and fov not clean:
                                            if (checkFov2 > 0.01 && abs(checkFov - fov_clean1) > 0.01 &&
                                                abs(checkFov - fov_clean2) > 0.01 && abs(checkFov - fov_clean3) > 0.01 &&
                                                abs(checkFov - fov_clean4) > 0.01 && abs(checkFov - fov_clean5) > 0.01 &&
                                                abs(checkFov - fov2_clean1) > 0.01 && abs(checkFov - fov2_clean2) > 0.01 &&
                                                abs(checkFov - fov2_clean3) > 0.01 && abs(checkFov - fov2_clean4) > 0.01 &&
                                                abs(checkFov - fov2_clean5) > 0.01)
                                            {
                                                var fov1 = CalcFov(ClientFov, LastResolutionX, LastResolutionY);
                                                var fov2 = CalcFov(ClientFov2, LastResolutionX, LastResolutionY);
                                                var fov3 = CalcFov(FovByFunc, LastResolutionX, LastResolutionY);
                                                var fov4 = CalcFov(FovByFunc2, LastResolutionX, LastResolutionY);
                                                // If fov not by server
                                                if (abs(checkFov - fov1) > 0.01 && abs(checkFov - fov2) > 0.01 &&
                                                    abs(checkFov - fov3) > 0.01 && abs(checkFov - fov4) > 0.01)
                                                {
                                                    DemoScanner_AddWarn(
                                                        "[FOV HACK TYPE 2] [" + checkFov + " FOV] at (" + CurrentTime +
                                                        "):" + CurrentTimeString,
                                                        abs(checkFov - ClientFov) > 0.01 &&
                                                        abs(checkFov - ClientFov2) > 0.01 &&
                                                        abs(checkFov - FovByFunc) > 0.01 &&
                                                        abs(checkFov - FovByFunc2) > 0.01 && !IsAngleEditByEngine() &&
                                                        !IsPlayerLossConnection());
                                                    FovHackTime = CurrentTime;
                                                    FovHackDetected += 1;
                                                }
                                            }
                                        }
                                    }
                                }

                                checkFov = cdframeFov;
                                CDFrameYAngleHistory[0] = CDFrameYAngleHistory[1];
                                CDFrameYAngleHistory[1] = CDFrameYAngleHistory[2];
                                if (CDFrameYAngleHistory[2] > 360.0f && cdframe.Viewangles.Y < 10.0f)
                                {
                                    if (LASTFRAMEISCLIENTDATA)
                                    {
                                        BadAnglesFoundCount++;
                                    }
                                }

                                CDFrameYAngleHistory[2] = cdframe.Viewangles.Y;
                                LASTFRAMEISCLIENTDATA = true;
                                if (GetDistance(curoriginpos, oldoriginpos) > 100)
                                {
                                    PlayerTeleportus++;
                                    ReloadWarns = 0;
                                    if (DEBUG_ENABLED)
                                    {
                                        Console.WriteLine("Teleportus " + CurrentTime + ":" + CurrentTimeString);
                                    }

                                    if (abs(LastGameMaximizeTime) > EPSILON && abs(LastTeleportusTime) > EPSILON &&
                                        abs(LastGameMaximizeTime - CurrentTime) < EPSILON && abs(GameEndTime) <= EPSILON)
                                    {
                                        if (ReturnToGameDetects > 1)
                                        {
                                            DemoScanner_AddWarn("['RETURN TO GAME' FEATURE] ", true, true, true);
                                        }

                                        ReturnToGameDetects++;
                                    }

                                    LastTeleportusTime = CurrentTime;
                                }
                                else
                                {
                                    for (var n = frameindex + 2;
                                         n < CurrentDemoFile.GsDemoInfo.DirectoryEntries[index].Frames.Count;
                                         n++)
                                    {
                                        var tmpframe = CurrentDemoFile.GsDemoInfo.DirectoryEntries[index].Frames[n];
                                        if (tmpframe.Key.Type == GoldSource.DemoFrameType.ClientData)
                                        {
                                            var cdframe2 = (GoldSource.ClientDataFrame)tmpframe.Value;
                                            if (GetDistance(new FPoint(cdframe2.Origin.X, cdframe2.Origin.Y),
                                                    curoriginpos) > 250)
                                            {
                                                LastTeleportusTime = CurrentTime;
                                            }

                                            break;
                                        }
                                    }
                                }

                                if (abs(CurrentTime) > EPSILON)
                                {
                                    if (LastFpsCheckTime < 0.0)
                                    {
                                        LastFpsCheckTime = CurrentTime;
                                    }

                                    if (abs(CurrentTime - LastFpsCheckTime) >= 1.0f)
                                    {
                                        LastFpsCheckTime = CurrentTime;
                                        if (DUMP_ALL_FRAMES)
                                        {
                                            subnode.Text += "CurrentFps:" + CurrentFps + "\n";
                                        }

                                        if (CurrentFps > RealFpsMax)
                                        {
                                            RealFpsMax = CurrentFps;
                                        }

                                        if (CurrentFps < RealFpsMin && CurrentFps > 0)
                                        {
                                            RealFpsMin = CurrentFps;
                                        }

                                        if (abs(CurrentTime) <= EPSILON || abs(CurrentTime2) <= EPSILON)
                                        {
                                            CurrentMsgBytes = CurrentMsgHudCount =
                                                CurrentMsgStuffCmdCount = CurrentMsgPrintCount = CurrentMsgDHudCount = 0;
                                        }
                                        else
                                        {

                                            if (MaxBytesPerSecond < CurrentMsgBytes)
                                            {
                                                MaxBytesPerSecond = CurrentMsgBytes;
                                            }

                                            BytesPerSecond.Add(CurrentMsgBytes);

                                            if (CurrentMsgBytes >= 970000)
                                            {
                                                MsgOverflowSecondsCount++;
                                            }

                                            if (MaxHudMsgPerSecond < CurrentMsgHudCount)
                                            {
                                                MaxHudMsgPerSecond = CurrentMsgHudCount;
                                            }

                                            if (MaxDHudMsgPerSecond < CurrentMsgDHudCount)
                                            {
                                                MaxDHudMsgPerSecond = CurrentMsgDHudCount;
                                            }


                                            if (MaxStuffCmdMsgPerSecond < CurrentMsgStuffCmdCount)
                                            {
                                                MaxStuffCmdMsgPerSecond = CurrentMsgStuffCmdCount;
                                            }

                                            if (MaxPrintCmdMsgPerSecond < CurrentMsgPrintCount)
                                            {
                                                MaxPrintCmdMsgPerSecond = CurrentMsgPrintCount;
                                            }
                                        }

                                        CurrentMsgBytes = CurrentMsgHudCount = CurrentMsgStuffCmdCount = CurrentMsgPrintCount = CurrentMsgDHudCount = 0;
                                        SecondFound = true;
                                        averagefps.Add(CurrentFps);
                                        CurrentFpsSecond = CurrentFps;
                                        CurrentFps = 0;
                                        CurrentGameSecond++;
                                    }
                                    else
                                    {
                                        CurrentFps++;
                                    }
                                }

                                var tmpAngleDirY = GetAngleDirection(fullnormalizeangle(PREV_CDFRAME_ViewAngles.Y),
                                    fullnormalizeangle(CDFRAME_ViewAngles.Y));
                                if (tmpAngleDirY == AngleDirection.AngleDirectionLeft)
                                {
                                    if (AngleStrikeDirection > 0)
                                    {
                                        AngleStrikeDirection = -1;
                                    }
                                    else
                                    {
                                        AngleStrikeDirection--;
                                    }

                                    if (LastAngleDirection != tmpAngleDirY)
                                    {
                                        AngleDirectionChanges++;
                                        StrafeAngleDirectionChanges++;
                                        AngleDirectionChangeTime = CurrentTime;
                                        if (DetectStrafeOptimizerStrikes > 1 && NeedCheckAngleDiffForStrafeOptimizer &&
                                            abs(CurrentTime - LastStrafeOptimizerWarnTime) > 0.02f)
                                        {
                                            StrafeOptimizerFalse = true;
                                        }

                                        NeedCheckAngleDiffForStrafeOptimizer = false;
                                    }

                                    LastAngleDirection = tmpAngleDirY;
                                }
                                else if (tmpAngleDirY == AngleDirection.AngleDirectionRight)
                                {
                                    if (AngleStrikeDirection < 0)
                                    {
                                        AngleStrikeDirection = 1;
                                    }
                                    else
                                    {
                                        AngleStrikeDirection++;
                                    }

                                    if (LastAngleDirection != tmpAngleDirY)
                                    {
                                        AngleDirectionChanges++;
                                        StrafeAngleDirectionChanges++;
                                        AngleDirectionChangeTime = CurrentTime;
                                        if (DetectStrafeOptimizerStrikes > 1 && NeedCheckAngleDiffForStrafeOptimizer &&
                                            abs(CurrentTime - LastStrafeOptimizerWarnTime) > 0.02f)
                                        {
                                            StrafeOptimizerFalse = true;
                                        }

                                        NeedCheckAngleDiffForStrafeOptimizer = false;
                                    }

                                    LastAngleDirection = tmpAngleDirY;
                                }

                                if (!PreviousFrameAlive || !CurrentFrameAlive)
                                {
                                    PREV_CDFRAME_ViewAngles.X = CDFRAME_ViewAngles.X;
                                    PREV_CDFRAME_ViewAngles.Y = CDFRAME_ViewAngles.Y;
                                }

                                var skip_sens_check = false;
                                if ((normalizeangle(abs(PREV_CDFRAME_ViewAngles.X)) > 88.95 &&
                                     normalizeangle(abs(PREV_CDFRAME_ViewAngles.X)) < 89.1) ||
                                    (normalizeangle(abs(CDFRAME_ViewAngles.X)) > 88.95 &&
                                     normalizeangle(abs(CDFRAME_ViewAngles.X)) < 89.1))
                                {
                                    skip_sens_check = true;
                                    HorAngleTime = CurrentTime;
                                }

                                if (RealAlive && !skip_sens_check)
                                {
                                    var tmpXangle = AngleBetween(PREV_CDFRAME_ViewAngles.X, CDFRAME_ViewAngles.X);
                                    var tmpYangle = AngleBetween(PREV_CDFRAME_ViewAngles.Y, CDFRAME_ViewAngles.Y);
                                    if (AngleLength < 0.0)
                                    {
                                        AngleLengthStartTime = CurrentTime;
                                        AngleLength = 0.0f;
                                    }

                                    AngleLength += tmpXangle;
                                    AngleLength += tmpYangle;
                                    var flAngleRealDetect = MIN_SENS_DETECTED;
                                    var flAngleWarnDetect = MIN_SENS_WARNING;
                                    if (PlayerSensitivityHistory.Count > SENS_COUNT_FOR_AIM)
                                    {
                                        var flMinSensAngle = 9999.0f;
                                        var maxcheckangels = SENS_COUNT_FOR_AIM;
                                        for (var i = PlayerSensitivityHistory.Count - maxcheckangels;
                                             i < PlayerSensitivityHistory.Count;
                                             i++)
                                        {
                                            var cursens = PlayerSensitivityHistory[i];
                                            if (cursens < flMinSensAngle)
                                            {
                                                flMinSensAngle = cursens;
                                            }
                                        }

                                        if (flMinSensAngle < MIN_PLAYABLE_SENS)
                                        {
                                            flMinSensAngle = MIN_PLAYABLE_SENS;
                                        }

                                        flAngleRealDetect = flMinSensAngle / 2.1f;
                                        flAngleWarnDetect = flMinSensAngle / 1.1f;
                                        CheckedSensCount++;
                                        if (CheckedSensCount >= SENS_COUNT_FOR_AIM)
                                        {
                                            var foundUsageSens = false;
                                            for (var i = 0; i < PlayerSensUsageList.Count; i++)
                                            {
                                                var tmpSensUsageStruct = PlayerSensUsageList[i];
                                                if (abs(tmpSensUsageStruct.sens - flMinSensAngle) < 0.001)
                                                {
                                                    foundUsageSens = true;
                                                    tmpSensUsageStruct.usagecount++;
                                                    PlayerSensUsageList[i] = tmpSensUsageStruct;
                                                }
                                            }

                                            if (!foundUsageSens)
                                            {
                                                var tmpSensUsageStruct = new PLAYER_USED_SENS();
                                                tmpSensUsageStruct.sens = flMinSensAngle;
                                                tmpSensUsageStruct.usagecount = 1;
                                                PlayerSensUsageList.Add(tmpSensUsageStruct);
                                            }
                                        }
                                    }

                                    if (CurrentWeapon == WeaponIdType.WEAPON_AWP ||
                                        CurrentWeapon == WeaponIdType.WEAPON_SCOUT ||
                                        CurrentWeapon == WeaponIdType.WEAPON_G3SG1 ||
                                        CurrentWeapon == WeaponIdType.WEAPON_SG550)
                                    {
                                        flAngleRealDetect /= 60.0f;
                                        flAngleWarnDetect /= 30.0f;
                                    }
                                    else if (CurrentWeapon == WeaponIdType.WEAPON_SG552 ||
                                             CurrentWeapon == WeaponIdType.WEAPON_AUG)
                                    {
                                        flAngleRealDetect /= 30.0f;
                                        flAngleWarnDetect /= 15.0f;
                                    }

                                    if (IsNewAttack() || IsNewSecondKnifeAttack())
                                    {
                                        if ((NewAttack || NewAttack2) && PlayerSensitivityWarning == 0 &&
                                            abs(LastAim5DetectedReal) > EPSILON &&
                                            abs(CurrentTime - LastAim5DetectedReal) < 0.5f)
                                        {
                                            if (DemoScanner_AddWarn(
                                                "[AIM TYPE 5.1 " + CurrentWeapon + "] at (" + LastAim5DetectedReal + "):" +
                                                GetTimeString(LastAim5DetectedReal),
                                                !IsTakeDamage() && !IsPlayerLossConnection() && !IsAngleEditByEngine() &&
                                                !IsCmdChangeWeapon() && !IsPlayerInDuck() && !IsPlayerUnDuck()))
                                            {
                                                if (!IsTakeDamage() && !IsPlayerLossConnection() && !IsAngleEditByEngine() &&
                                                    !IsCmdChangeWeapon() && !IsPlayerInDuck() && !IsPlayerUnDuck())
                                                {
                                                    TotalAimBotDetected++;
                                                }
                                                LastAim5DetectedReal = 0.0f;
                                                LastAim5Detected = 0.0f;
                                            }
                                        }
                                        else if (PlayerSensitivityWarning == 0 && abs(LastAim5DetectedReal) > EPSILON &&
                                                 abs(CurrentTime - LastAim5DetectedReal) < 0.5f)
                                        {
                                            if (DemoScanner_AddWarn(
                                                "[AIM TYPE 5.9 " + CurrentWeapon + "] at (" + LastAim5DetectedReal + "):" +
                                                GetTimeString(LastAim5DetectedReal),
                                                !IsTakeDamage() && !IsPlayerLossConnection() && !IsAngleEditByEngine() &&
                                                !IsCmdChangeWeapon() && !IsPlayerInDuck() && !IsPlayerUnDuck()))
                                            {
                                                if (!IsTakeDamage() && !IsPlayerLossConnection() && !IsAngleEditByEngine() &&
                                                    !IsCmdChangeWeapon() && !IsPlayerInDuck() && !IsPlayerUnDuck())
                                                {
                                                    TotalAimBotDetected++;
                                                }
                                            }
                                            LastAim5DetectedReal = 0.0f;
                                            LastAim5Detected = 0.0f;
                                        }
                                        else if (PlayerSensitivityWarning == 0 && abs(LastAim5Detected) > EPSILON &&
                                                 abs(CurrentTime - LastAim5Detected) < 0.5f)
                                        {
                                            if (!IsAngleEditByEngine() && !IsTakeDamage() && !IsCmdChangeWeapon() && !IsPlayerInDuck() && !IsPlayerUnDuck())
                                            {
                                                if (DemoScanner_AddWarn(
                                                    "[AIM TYPE 5.2 " + CurrentWeapon + "] at (" + LastAim5Detected + "):" +
                                                    GetTimeString(LastAim5Detected), !IsPlayerLossConnection()))
                                                {
                                                    TotalAimBotDetected++;
                                                }
                                            }
                                            else
                                            {
                                                DemoScanner_AddWarn(
                                                    "[AIM TYPE 5.3 " + CurrentWeapon + "] at (" + LastAim5Detected + "):" +
                                                    GetTimeString(LastAim5Detected), false);
                                            }

                                            LastAim5DetectedReal = 0.0f;
                                            LastAim5Detected = 0.0f;
                                        }
                                        else if (PlayerSensitivityWarning == 1)
                                        {
                                            if (abs(LastAim5DetectedReal) > EPSILON &&
                                                abs(CurrentTime - LastAim5DetectedReal) < 0.75f)
                                            {
                                                if (!IsAngleEditByEngine() && !IsPlayerInDuck() && !IsPlayerUnDuck())
                                                {
                                                    DemoScanner_AddWarn(
                                                        "[AIM TYPE 5.4 " + CurrentWeapon + "] at (" + LastAim5DetectedReal +
                                                        "):" + GetTimeString(LastAim5DetectedReal), false);
                                                    LastAim5DetectedReal = 0.0f;
                                                }
                                                else
                                                {
                                                    DemoScanner_AddWarn(
                                                        "[AIM TYPE 5.6 " + CurrentWeapon + "] at (" + LastAim5DetectedReal +
                                                        "):" + GetTimeString(LastAim5DetectedReal), false);
                                                    LastAim5DetectedReal = 0.0f;
                                                }
                                            }

                                            if (abs(LastAim5Detected) > EPSILON &&
                                                abs(CurrentTime - LastAim5Detected) < 0.75f)
                                            {
                                                if (!IsAngleEditByEngine() && !IsPlayerInDuck() && !IsPlayerUnDuck())
                                                {
                                                    DemoScanner_AddWarn(
                                                        "[AIM TYPE 5.4 " + CurrentWeapon + "] at (" + LastAim5Detected +
                                                        "):" + GetTimeString(LastAim5Detected), false);
                                                    LastAim5Detected = 0.0f;
                                                }
                                                else
                                                {
                                                    DemoScanner_AddWarn(
                                                        "[AIM TYPE 5.7 " + CurrentWeapon + "] at (" + LastAim5Detected +
                                                        "):" + GetTimeString(LastAim5Detected), false);
                                                    LastAim5Detected = 0.0f;
                                                }
                                            }

                                            PlayerSensitivityWarning = 0;
                                        }
                                        else if (PlayerSensitivityWarning == 2)
                                        {
                                            if (abs(LastAim5Detected) > EPSILON &&
                                                abs(CurrentTime - LastAim5Detected) < 0.75f)
                                            {
                                                DemoScanner_AddWarn(
                                                    "[AIM TYPE 5.10 " + CurrentWeapon + "] at (" + LastAim5Detected + "):" +
                                                    GetTimeString(LastAim5Detected), false);
                                                LastAim5Detected = 0.0f;
                                            }

                                            PlayerSensitivityWarning = 0;
                                        }
                                        else
                                        {
                                            if (abs(LastAim5DetectedReal) > EPSILON &&
                                                abs(CurrentTime - LastAim5DetectedReal) > 0.75f)
                                            {
                                                if (!IsAngleEditByEngine() && !IsPlayerInDuck() && !IsPlayerUnDuck())
                                                {
                                                    DemoScanner_AddWarn(
                                                        "[AIM TYPE 5.5 " + CurrentWeapon + "] at (" + LastAim5DetectedReal +
                                                        "):" + GetTimeString(LastAim5DetectedReal), false);
                                                    LastAim5DetectedReal = 0.0f;
                                                    PlayerSensitivityWarning = 0;
                                                }
                                                else
                                                {
                                                    DemoScanner_AddWarn(
                                                        "[AIM TYPE 5.8 " + CurrentWeapon + "] at (" + LastAim5DetectedReal +
                                                        "):" + GetTimeString(LastAim5DetectedReal), false);
                                                    LastAim5DetectedReal = 0.0f;
                                                    PlayerSensitivityWarning = 0;
                                                }
                                            }

                                            if (abs(LastAim5Detected) > EPSILON &&
                                                abs(CurrentTime - LastAim5Detected) > 0.75f)
                                            {
                                                if (!IsAngleEditByEngine() && !IsPlayerInDuck() && !IsPlayerUnDuck())
                                                {
                                                    DemoScanner_AddWarn(
                                                        "[AIM TYPE 5.5 " + CurrentWeapon + "] at (" + LastAim5Detected +
                                                        "):" + GetTimeString(LastAim5Detected), false);
                                                    LastAim5Detected = 0.0f;
                                                    PlayerSensitivityWarning = 0;
                                                }
                                            }
                                        }
                                    }

                                    if (abs(tmpXangle) > EPSILON && tmpXangle < flAngleRealDetect)
                                    {
                                        if (CurrentWeapon == WeaponIdType.WEAPON_C4 ||
                                            CurrentWeapon == WeaponIdType.WEAPON_HEGRENADE ||
                                            CurrentWeapon == WeaponIdType.WEAPON_SMOKEGRENADE ||
                                            CurrentWeapon == WeaponIdType.WEAPON_FLASHBANG ||
                                            CurrentWeapon == WeaponIdType.WEAPON_NONE ||
                                            CurrentWeapon == WeaponIdType.WEAPON_BAD ||
                                            CurrentWeapon == WeaponIdType.WEAPON_BAD2)
                                        {
                                        }
                                        else
                                        {
                                            if (CurrentFrameAttacked || PreviousFrameAttacked || BadPunchAngle ||
                                                IsTakeDamage() || IsPlayerLossConnection() || IsAngleEditByEngine() ||
                                                IsCmdChangeWeapon() || IsPlayerInDuck() || IsPlayerUnDuck())
                                            {
                                                LastAim5Detected = CurrentTime;
                                                PlayerSensitivityWarning = 1;
                                            }
                                            else
                                            {
                                                PlayerSensitivityWarning = 0;
                                                LastAim5DetectedReal = CurrentTime;
                                            }
                                        }
                                    }
                                    else if (abs(tmpXangle) > EPSILON && tmpXangle < flAngleWarnDetect &&
                                             (LastAim5DetectedReal < EPSILON ||
                                              abs(CurrentTime - LastAim5DetectedReal) > 0.5f))
                                    {
                                        if (CurrentWeapon == WeaponIdType.WEAPON_C4 ||
                                            CurrentWeapon == WeaponIdType.WEAPON_HEGRENADE ||
                                            CurrentWeapon == WeaponIdType.WEAPON_SMOKEGRENADE ||
                                            CurrentWeapon == WeaponIdType.WEAPON_FLASHBANG ||
                                            CurrentWeapon == WeaponIdType.WEAPON_NONE ||
                                            CurrentWeapon == WeaponIdType.WEAPON_BAD ||
                                            CurrentWeapon == WeaponIdType.WEAPON_BAD2)
                                        {
                                        }
                                        else
                                        {
                                            PlayerSensitivityWarning = 1;
                                            LastAim5Detected = CurrentTime;
                                        }
                                    }
                                    else if (abs(tmpYangle) > EPSILON && tmpYangle < flAngleRealDetect)
                                    {
                                        if (CurrentWeapon == WeaponIdType.WEAPON_C4 ||
                                            CurrentWeapon == WeaponIdType.WEAPON_HEGRENADE ||
                                            CurrentWeapon == WeaponIdType.WEAPON_SMOKEGRENADE ||
                                            CurrentWeapon == WeaponIdType.WEAPON_FLASHBANG ||
                                            CurrentWeapon == WeaponIdType.WEAPON_NONE ||
                                            CurrentWeapon == WeaponIdType.WEAPON_BAD ||
                                            CurrentWeapon == WeaponIdType.WEAPON_BAD2)
                                        {
                                        }
                                        //if (!CurrentFrameAttacked
                                        //        && !PreviousFrameAttacked && CurrentFrameOnGround && PreviousFrameOnGround && !BadPunchAngle && !IsTakeDamage() && !IsAngleEditByEngine())
                                        //{
                                        //    Console.WriteLine("PREV_CDFRAME_ViewAngles.Y=" + PREV_CDFRAME_ViewAngles.Y + " CDFRAME_ViewAngles.Y=",)
                                        //    LastAim5Detected = CurrentTime;
                                        //    PlayerSensitivityWarning = 2;
                                        //}
                                    }

                                    if (CurrentSensitivity < 0.0 ||
                                        (abs(tmpXangle) > EPSILON && tmpXangle < CurrentSensitivity))
                                    {
                                        CurrentSensitivity = tmpXangle;
                                    }

                                    if (CurrentWeapon == WeaponIdType.WEAPON_NONE ||
                                        CurrentWeapon == WeaponIdType.WEAPON_BAD ||
                                        CurrentWeapon == WeaponIdType.WEAPON_BAD2)
                                    {
                                        //fixyou
                                    }
                                    else
                                    {
                                        LastSensWeapon = CurrentWeapon.ToString();
                                    }

                                    if (SecondFound && LastSecondString != CurrentTimeString)
                                    {
                                        if (abs(CurrentSensitivity) > EPSILON)
                                        {
                                            LastSecondString = CurrentTimeString;
                                            if (AngleLength > EPSILON && abs(CurrentTime - AngleLengthStartTime) > 0.5)
                                            {
                                                PlayerAngleLenHistory.Add(AngleLength /
                                                                          abs(CurrentTime - AngleLengthStartTime));
                                                AngleLength = -1.0f;
                                            }
                                            else
                                            {
                                                PlayerAngleLenHistory.Add(0.0f);
                                            }

                                            PlayerSensitivityHistory.Add(CurrentSensitivity);
                                            PlayerSensitivityHistoryStrTime.Add("(" + LastFpsCheckTime + "): " +
                                                                                CurrentTimeString);
                                            PlayerSensitivityHistoryStrWeapon.Add(LastSensWeapon);
                                            PlayerSensitivityHistoryStrFOV.Add((int)checkFov + "/" + (int)checkFov2);
                                        }

                                        CurrentSensitivity = -1.0f;
                                    }


                                    if (IsAngleEditByEngine() || !CurrentFrameOnGround || IsRealChangeWeapon() || IsCmdChangeWeapon())
                                    {
                                        AimType7Frames = -2;
                                        AimType7Event = 0;
                                        OldAimType7Time = 0.0f;
                                    }

                                    if (AimType7Event == 2 || AimType7Event == 3 || AimType7Event == 4 ||
                                        AimType7Event == 5)
                                    {
                                        if (abs(tmpXangle) < EPSILON && abs(tmpYangle) < EPSILON && AimType7Frames < 20)
                                        {
                                            AimType7Frames++;
                                        }
                                        else
                                        {
                                            // Console.WriteLine("11");
                                            if (AimType7Frames > 2)
                                            {
                                                if (AimType7Event == 4)
                                                {
                                                    Aim73FalseSkip--;
                                                }

                                                if (AngleBetween(Aim8CurrentFrameViewanglesY, CDFRAME_ViewAngles.Y) >
                                                    EPSILON &&
                                                    AngleBetween(Aim8CurrentFrameViewanglesX, CDFRAME_ViewAngles.X) >
                                                    EPSILON && !IsAngleEditByEngine() &&
                                                    CurrentFrameOnGround)
                                                {
                                                    if (AimType7Event == 4 && Aim73FalseSkip < 0)
                                                    {
                                                        var tmpangle2 = AngleBetween(Aim8CurrentFrameViewanglesX,
                                                            CDFRAME_ViewAngles.X);
                                                        tmpangle2 += AngleBetween(Aim8CurrentFrameViewanglesY,
                                                            CDFRAME_ViewAngles.Y);
                                                        var Aim7var1 = OldAimType7Frames;
                                                        var Aim7var2 = AimType7Frames;
                                                        var Aim7var3 = 0;
                                                        var Aim7detected = true;
                                                        var Aim7str = GetAim7String(ref Aim7var1, ref Aim7var2,
                                                            ref Aim7var3, AimType7Event, tmpangle2, ref Aim7detected);
                                                        if (Aim7detected && abs(OldAimType7Time) > EPSILON)
                                                        {
                                                            if (DemoScanner_AddWarn(
                                                                Aim7str + " at (" + OldAimType7Time + "):" +
                                                                GetTimeString(OldAimType7Time),
                                                                Aim7detected && Aim7var3 > 50 && Aim7var1 >= 20 &&
                                                                Aim7var2 >= 20 && !IsCmdChangeWeapon() && !IsPlayerInDuck() && !IsPlayerUnDuck() && !IsPlayerLossConnection()))
                                                            {
                                                                if (Aim7var3 > 50)
                                                                {
                                                                    TotalAimBotDetected++;
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else if (AimType7Event != 4)
                                                    {
                                                        var tmpangle2 = AngleBetween(Aim8CurrentFrameViewanglesX,
                                                            CDFRAME_ViewAngles.X);
                                                        tmpangle2 += AngleBetween(Aim8CurrentFrameViewanglesY,
                                                            CDFRAME_ViewAngles.Y);
                                                        var Aim7var1 = OldAimType7Frames;
                                                        var Aim7var2 = AimType7Frames;
                                                        var Aim7var3 = 0;
                                                        var Aim7detected = true;
                                                        var Aim7str = GetAim7String(ref Aim7var1, ref Aim7var2,
                                                            ref Aim7var3, AimType7Event, tmpangle2, ref Aim7detected);
                                                        if (Aim7detected && abs(OldAimType7Time) > EPSILON)
                                                        {
                                                            DemoScanner_AddWarn(
                                                                Aim7str + " at (" + OldAimType7Time + "):" +
                                                                GetTimeString(OldAimType7Time),
                                                                Aim7detected && Aim7var3 > 50 && Aim7var1 >= 30 &&
                                                                Aim7var2 >= 30 && !IsCmdChangeWeapon() && !IsPlayerLossConnection());
                                                        }
                                                    }
                                                }
                                            }

                                            if (AngleBetween(Aim8CurrentFrameViewanglesY, CDFRAME_ViewAngles.Y) < EPSILON &&
                                                !IsAngleEditByEngine() && CurrentFrameOnGround /* &&
                                                    Aim8CurrentFrameViewanglesX !=
                                                    CurrentFrameViewanglesX*/)
                                            {
                                                AimType7Event += 10;
                                                var tmpangle2 = AngleBetween(Aim8CurrentFrameViewanglesX,
                                                    CDFRAME_ViewAngles.X);
                                                tmpangle2 += AngleBetween(Aim8CurrentFrameViewanglesY,
                                                    CDFRAME_ViewAngles.Y);
                                                var Aim7var1 = OldAimType7Frames;
                                                var Aim7var2 = AimType7Frames;
                                                var Aim7var3 = 0;
                                                var Aim7detected = true;
                                                var Aim7str = GetAim7String(ref Aim7var1, ref Aim7var2, ref Aim7var3,
                                                    AimType7Event, tmpangle2, ref Aim7detected);
                                                if (Aim7detected && abs(OldAimType7Time) > EPSILON)
                                                {
                                                    DemoScanner_AddWarn(
                                                        Aim7str + " at (" + OldAimType7Time + "):" +
                                                        GetTimeString(OldAimType7Time),
                                                        Aim7detected && !IsPlayerLossConnection() && Aim7var3 > 50 &&
                                                        Aim7var1 >= 20 && Aim7var2 >= 20 && !IsCmdChangeWeapon() && !IsPlayerInDuck() && !IsPlayerUnDuck());
                                                }
                                            }

                                            AimType7Frames = 0;
                                            AimType7Event = 0;
                                        }
                                    }

                                    if (AimType7Event == 1)
                                    {
                                        AimType7Event = abs(tmpXangle) > EPSILON && abs(tmpYangle) > EPSILON ? 52 : 53;
                                    }
                                    else if (AimType7Event == 11)
                                    {
                                        AimType7Event = abs(tmpXangle) > EPSILON && abs(tmpYangle) > EPSILON ? 4 : 0;
                                    }

                                    // Если AimType7Event равен 0 то искать паттерн
                                    /*
                                              5 кадров нет движения
                                              в момент выстрела прицел изменяет свое положение
                                              следующий кадр изменение и 5 кадров нет движения
                                        */
                                    if (AimType7Event == 0)
                                    {
                                        // Если угол не изменился
                                        if (abs(tmpXangle) < EPSILON && abs(tmpYangle) < EPSILON /* &&
                                        !CurrentFrameAttacked*/)
                                        {
                                            // Console.WriteLine("3");
                                            Aim8CurrentFrameViewanglesY = CDFRAME_ViewAngles.Y;
                                            Aim8CurrentFrameViewanglesX = CDFRAME_ViewAngles.X;
                                            // if (!CurrentFrameDuplicated)
                                            AimType7Frames++;
                                        }
                                        // Иначе Если угол изменился и набралось больше 1 таких кадров то вкл поиск аим
                                        else
                                        {
                                            if (AimType7Frames > 1 && abs(tmpXangle) > EPSILON && abs(tmpYangle) > EPSILON)
                                            {
                                                OldAimType7Time = CurrentTime;
                                                //Console.WriteLine("4");
                                                OldAimType7Frames = AimType7Frames;
                                                AimType7Event = 1;
                                                if (CurrentFrameAttacked)
                                                {
                                                    AimType7Event = 11;
                                                }
                                            }

                                            AimType7Frames = 0;
                                        }
                                    }
                                }
                                else
                                {
                                    LastAim5DetectedReal = 0.0f;
                                    LastAim5Detected = 0.0f;
                                    AimType7Event = 0;
                                    AimType7Frames = 0;
                                    OldAimType7Time = 0.0f;
                                }

                                //Console.Write(RealAlive);
                                //Console.Write(" ");
                                //Console.Write(CurrentSensitivity);
                                //Console.Write(" ");
                                //Console.Write(tmpXangle);
                                //Console.Write(" ");
                                //Console.Write(SecondFound);
                                //Console.Write("\n");
                                break;
                            }

                        case GoldSource.DemoFrameType.NextSection:
                            {
                                NewDirectory = true;
                                break;
                            }

                        case GoldSource.DemoFrameType.Event:
                            {
                                CurrentEvents++;
                                LASTFRAMEISCLIENTDATA = false;

                                LastEventDetectTime = CurrentTime;

                                if (DUMP_ALL_FRAMES)
                                {
                                    subnode.Text += "{\n";
                                }

                                var eframe = (GoldSource.EventFrame)frame.Value;

                                //bool found = false;

                                //foreach (var res in DownloadedResources)
                                //{
                                //    if (res.res_index == eframe.Index && res.res_type == 5)
                                //    {
                                //        found = true;
                                //        Console.WriteLine("Event:" + res.res_path + " at " + CurrentTimeString);
                                //    }
                                //}

                                //if (!found)
                                //    Console.WriteLine("Event:" + "[UNKNOWN]" + " at " + CurrentTimeString);

                                LastEventId = eframe.Index;

                                if (abs(LastEventDetectTime) > EPSILON && abs(LastAttackPressed - LastEventDetectTime) > 0.25 && !IsInAttack())
                                {

                                }
                                else
                                {
                                    LastEventDetectTime = 0.0f;
                                }

                                if (DUMP_ALL_FRAMES)
                                {
                                    subnode.Text += "Flags = " + eframe.Flags + "\n";
                                    subnode.Text += "Index = " + eframe.Index + "\n";
                                    subnode.Text += "Delay = " + eframe.Delay + "\n";
                                    subnode.Text += "EventArgumentsFlags = " + eframe.EventArguments.Flags + "\n";
                                    subnode.Text += "EntityIndex = " + eframe.EventArguments.EntityIndex + "\n";
                                    subnode.Text += "Origin.X = " + eframe.EventArguments.Origin.X + "\n";
                                    subnode.Text += "Origin.Y = " + eframe.EventArguments.Origin.Y + "\n";
                                    subnode.Text += "Origin.Z = " + eframe.EventArguments.Origin.Z + "\n";
                                    subnode.Text += "Angles.X = " + eframe.EventArguments.Angles.X + "\n";
                                    subnode.Text += "Angles.Y = " + eframe.EventArguments.Angles.Y + "\n";
                                    subnode.Text += "Angles.Z = " + eframe.EventArguments.Angles.Z + "\n";
                                    subnode.Text += "Velocity.X = " + eframe.EventArguments.Velocity.X + "\n";
                                    subnode.Text += "Velocity.Y = " + eframe.EventArguments.Velocity.Y + "\n";
                                    subnode.Text += "Velocity.Z = " + eframe.EventArguments.Velocity.Z + "\n";
                                    subnode.Text += "Ducking = " + eframe.EventArguments.Ducking + "\n";
                                    subnode.Text += "Fparam1 = " + eframe.EventArguments.Fparam1 + "\n";
                                    subnode.Text += "Fparam2 = " + eframe.EventArguments.Fparam2 + "\n";
                                    subnode.Text += "Iparam1 = " + eframe.EventArguments.Iparam1 + "\n";
                                    subnode.Text += "Iparam2 = " + eframe.EventArguments.Iparam2 + "\n";
                                    subnode.Text += "Bparam1 = " + eframe.EventArguments.Bparam1 + "\n";
                                    subnode.Text += "Bparam2 = " + eframe.EventArguments.Bparam2 + "\n";
                                    subnode.Text += "}\n";
                                }

                                if (abs(CurrentTime - LastClientDataTime) < EPSILON &&
                                    abs(CurrentTime - LastAttackStartStopCmdTime) < EPSILON)
                                {
                                    if (eframe.EventArguments.Iparam1 == 0 && eframe.EventArguments.Iparam2 == 0 &&
                                        abs(CurrentNetMsgFrame.RParms.Punchangle.Y) < EPSILON)
                                    {
                                        if (abs(eframe.EventArguments.Origin.X) > EPSILON ||
                                            abs(eframe.EventArguments.Origin.Y) > EPSILON ||
                                            abs(eframe.EventArguments.Origin.Z) > EPSILON)
                                        {
                                            if (AngleBetween(CDFRAME_ViewAngles.Y, eframe.EventArguments.Angles.Y) >
                                                EPSILON && AngleBetween(PREV_CDFRAME_ViewAngles.Y,
                                                    eframe.EventArguments.Angles.Y) > EPSILON)
                                            {
                                                DemoScanner_AddWarn(
                                                    "[BETA] [AIM TYPE 10 " + CurrentWeapon + "] at (" + IsAttackLastTime +
                                                    "):" + CurrentTimeString, false);
                                            }
                                        }
                                    }
                                }

                                break;
                            }

                        case GoldSource.DemoFrameType.WeaponAnim:
                            {
                                LASTFRAMEISCLIENTDATA = false;
                                if (DUMP_ALL_FRAMES)
                                {
                                    subnode.Text += "{\n";
                                }

                                var waframe = (GoldSource.WeaponAnimFrame)frame.Value;
                                if (DUMP_ALL_FRAMES)
                                {
                                    subnode.Text += "Anim = " + waframe.Anim + "\n";
                                    subnode.Text += "Body = " + waframe.Body + "\n";
                                    subnode.Text += "}\n";
                                }

                                if (RealAlive && !IsRoundEnd() && IsRealWeapon() &&
                                    CurrentWeapon != WeaponIdType.WEAPON_KNIFE &&
                                    CurrentWeapon != WeaponIdType.WEAPON_XM1014 &&
                                    CurrentWeapon != WeaponIdType.WEAPON_M3 &&
                                    CurrentWeapon != WeaponIdType.WEAPON_HEGRENADE &&
                                    CurrentWeapon != WeaponIdType.WEAPON_FLASHBANG &&
                                    CurrentWeapon != WeaponIdType.WEAPON_SMOKEGRENADE &&
                                    CurrentWeapon != WeaponIdType.WEAPON_C4)
                                {
                                    if (!IsAngleEditByEngine())
                                    {
                                        if (LastWeaponAnim != WeaponIdType.WEAPON_NONE && LastWeaponAnim != CurrentWeapon)
                                        {
                                            WeaponAnimWarn = 0;
                                        }

                                        if (waframe.Anim == 0 && waframe.Body == 0)
                                        {
                                            WeaponAnimWarn++;
                                            LastWeaponAnim = CurrentWeapon;
                                            if (WeaponAnimWarn > 50 && abs(CurrentTime - NoWeaponAnimTime) > 1.0f)
                                            {
                                                DemoScanner_AddWarn(
                                                    "[NO WEAPON ANIM " + CurrentWeapon + "] at (" + CurrentTime + ") " +
                                                    CurrentTimeString, !IsPlayerLossConnection());
                                                WeaponAnimWarn = 0;
                                                NoWeaponAnimTime = CurrentTime;
                                            }
                                        }
                                        else
                                        {
                                            if (WeaponAnimWarn > 0)
                                            {
                                                WeaponAnimWarn--;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (WeaponAnimWarn > 0)
                                        {
                                            WeaponAnimWarn--;
                                        }
                                    }
                                }
                                else
                                {
                                    WeaponAnimWarn = 0;
                                }

                                break;
                            }

                        case GoldSource.DemoFrameType.Sound:
                            {
                                LASTFRAMEISCLIENTDATA = false;
                                if (DUMP_ALL_FRAMES)
                                {
                                    subnode.Text += "{\n";
                                }

                                var sframe = (GoldSource.SoundFrame)frame.Value;
                                if (DUMP_ALL_FRAMES)
                                {
                                    subnode.Text += "Channel = " + sframe.Channel + "\n";
                                    subnode.Text += "Sample = " + sframe.Sample + "\n";
                                    subnode.Text += "Attenuation = " + sframe.Attenuation + "\n";
                                    subnode.Text += "Volume = " + sframe.Volume + "\n";
                                    subnode.Text += "Flags = " + sframe.Flags + "\n";
                                    subnode.Text += "Pitch = " + sframe.Pitch + "\n";
                                    subnode.Text += "}\n";
                                }

                                if (sframe.Sample.ToLower().IndexOf("ladder") > -1)
                                {
                                    FlyDirection = 0;
                                }
                                //Console.WriteLine("FlyDirection 0:" + FlyDirection);
                                //Console.WriteLine("Sound:" + CurrentTimeString);
                                break;
                            }

                        case GoldSource.DemoFrameType.DemoBuffer:
                            {
                                var bframe = (GoldSource.DemoBufferFrame)frame.Value;
                                if (bframe.Buffer.Count > 0)
                                {
                                    if (DUMP_ALL_FRAMES)
                                    {
                                        subnode.Text += "{ DEMOBUFFER HAS DATA\n";
                                        subnode.Text += BitConverter.ToString(bframe.Buffer.ToArray()).Replace("-", "");
                                        subnode.Text += "}\n";
                                    }

                                    if (bframe.Buffer.Count >= 8)
                                    {
                                        if (bframe.Buffer[0] == 1)
                                        {
                                            var floatfov = BitConverter.ToSingle(bframe.Buffer.ToArray(), 4);
                                            if (RealAlive && (CurrentFrameAttacked || CurrentFrameJumped) &&
                                                abs(CurrentTime - LastDeathTime) > 5.0f &&
                                                abs(CurrentTime - LastAliveTime) > 2.0f)
                                            {
                                                if (abs(CurrentTime - FovHackTime) > 30.0f)
                                                {
                                                    if (FovHackDetected <= 10 && !FoundNextClient)
                                                    {
                                                        /* AWP, SCOUT, ETC ... for next update weapon check */
                                                        var fov2_clean1 = CalcFov(10.0f, LastResolutionX, LastResolutionY);
                                                        var fov2_clean2 = CalcFov(15.0f, LastResolutionX, LastResolutionY);
                                                        var fov2_clean3 = CalcFov(40.0f, LastResolutionX, LastResolutionY);
                                                        var fov2_clean4 = CalcFov(55.0f, LastResolutionX, LastResolutionY);
                                                        var fov_clean1 = 10.0f;
                                                        var fov_clean2 = 15.0f;
                                                        var fov_clean3 = 40.0f;
                                                        var fov_clean4 = 55.0f;
                                                        // NORMAL FOV FOR CUSTOM GAME CLIENT
                                                        var fov2_clean5 = CalcFov(90.0f, LastResolutionX, LastResolutionY);
                                                        var fov_clean5 = 90.0f;
                                                        // IF fov found, and fov not clean:
                                                        if (checkFov2 > 0.01 && abs(checkFov2 - fov_clean1) > 0.01 &&
                                                            abs(checkFov2 - fov_clean2) > 0.01 &&
                                                            abs(checkFov2 - fov_clean3) > 0.01 &&
                                                            abs(checkFov2 - fov_clean4) > 0.01 &&
                                                            abs(checkFov2 - fov_clean5) > 0.01 &&
                                                            abs(checkFov2 - fov2_clean1) > 0.01 &&
                                                            abs(checkFov2 - fov2_clean2) > 0.01 &&
                                                            abs(checkFov2 - fov2_clean3) > 0.01 &&
                                                            abs(checkFov2 - fov2_clean4) > 0.01 &&
                                                            abs(checkFov2 - fov2_clean5) > 0.01)
                                                        {
                                                            var fov1 = CalcFov(ClientFov, LastResolutionX, LastResolutionY);
                                                            var fov2 = CalcFov(ClientFov2, LastResolutionX,
                                                                LastResolutionY);
                                                            var fov3 = CalcFov(FovByFunc, LastResolutionX, LastResolutionY);
                                                            var fov4 = CalcFov(FovByFunc2, LastResolutionX,
                                                                LastResolutionY);
                                                            // If fov not by server
                                                            if (abs(checkFov2 - fov1) > 0.01 &&
                                                                abs(checkFov2 - fov2) > 0.01 &&
                                                                abs(checkFov2 - fov3) > 0.01 &&
                                                                abs(checkFov2 - fov4) > 0.01)
                                                            {
                                                                DemoScanner_AddWarn(
                                                                    "[FOV HACK TYPE 2] [" + checkFov2 + " FOV] at (" +
                                                                    CurrentTime + "):" + CurrentTimeString,
                                                                    abs(checkFov2 - ClientFov) > 0.01 &&
                                                                    abs(checkFov2 - ClientFov2) > 0.01 &&
                                                                    abs(checkFov2 - FovByFunc) > 0.01 &&
                                                                    abs(checkFov2 - FovByFunc2) > 0.01 &&
                                                                    !IsAngleEditByEngine() && !IsPlayerLossConnection());
                                                                FovHackTime = CurrentTime;
                                                                FovHackDetected += 1;
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            checkFov2 = floatfov;
                                            // Console.WriteLine(CurrentTimeString + "[DEMOBUFFER: " + CurrentWeapon + "] = " + fov + " framefov = " + cdframeFov);
                                        }
                                    }
                                }

                                // LASTFRAMEISCLIENTDATA = false;
                                /**/
                                break;
                            }

                        case GoldSource.DemoFrameType.NetMsg:
                        default:
                            {
                                if (NewDirectory)
                                {
                                    FrameErrors = LastOutgoingSequence = LastIncomingAcknowledged = LastIncomingSequence = 0;
                                    LASTFRAMEISCLIENTDATA = false;
                                    if (DUMP_ALL_FRAMES)
                                    {
                                        subnode.Text += "End of the DirectoryEntry!";
                                    }

                                    if (UserAlive)
                                    {
                                        FirstUserAlive = false;
                                    }

                                    UserAlive = false;
                                    RealAlive = false;
                                    //FirstBypassKill = true;
                                    BypassCount = 0;
                                    FirstDuck = false;
                                    FirstJump = false;
                                    FirstAttack = false;
                                    SearchJumpBug = false;
                                    TmpPlayerEnt = -1;
                                    TmpPlayerNum = -1;
                                    LocalPlayerId = -1;
                                    fullPlayerList.AddRange(playerList);
                                    playerList.Clear();
                                }

                                LASTFRAMEISCLIENTDATA = false;
                                MessageId = 0;
                                NewViewAngleSearcherAngle = 0.0f;

                                //if (frame.Key.Type != GoldSource.DemoFrameType.NetMsg)
                                //{
                                //    Console.WriteLine("Invalid");
                                //}

                                CurrentNetMsgFrameId++;
                                NeedCheckAttack = true;
                                NeedCheckAttack2 = true;

                                //IsAttackSkipTimes--;

                                CurrentFrameIdWeapon += 10;
                                row = index + "/" + frame.Key.FrameIndex + " " + "[" + frame.Key.Time + "s]: " +
                                      "NETMESSAGE";
                                var nf = (GoldSource.NetMsgFrame)frame.Value;
                                CurrentNetMsgFrame = nf;

                                //var AngleYBigChanged = false;
                                //if (AngleBetween(CurrentNetMsgFrame.RParms.Viewangles.Y,
                                //        PreviousNetMsgFrame.RParms.Viewangles.Y) > 2.0) AngleYBigChanged = true;

                                PreviousTime3 = CurrentTime3;
                                CurrentTime3 = frame.Key.Time;
                                PreviousTime = CurrentTime;

                                if (AlternativeTimeCounter == 1)
                                {
                                    if (abs(CurrentTime) > 1.0 && abs(CurrentTimeSvc) < 0.01f)
                                    {
                                        BadTimeFound += 25;
                                    }
                                    else
                                    {
                                        if (CurrentTime > 1.0 && CurrentTimeSvc < 0.01f)
                                        {
                                            BadTimeFound += 25;
                                        }
                                    }

                                    CurrentTime = CurrentTimeSvc;
                                }
                                else if (AlternativeTimeCounter == 0)
                                {
                                    if (abs(CurrentTime) > 1.0 && abs(nf.RParms.Time) < 0.01f)
                                    {
                                        BadTimeFound += 25;
                                    }
                                    else
                                    {
                                        if (CurrentTime > 1.0 && nf.RParms.Time < 0.01f)
                                        {
                                            BadTimeFound += 25;
                                        }
                                    }

                                    CurrentTime = nf.RParms.Time;
                                }
                                else
                                {
                                    var newtime = abs(CurrentTime3 - PreviousTime3);
                                    if (newtime > 1.0f)
                                    {
                                        newtime = 1.0f;
                                    }

                                    CurrentTime += newtime;
                                }

                                if (BadTimeFound > 500 && AlternativeTimeCounter <= 2)
                                {
                                    BadTimeFound = 0;
                                    AlternativeTimeCounter++;
                                    var newAltTimer = AlternativeTimeCounter;
                                    var isDump = DUMP_ALL_FRAMES;


                                    Console.Clear();

                                    if (IsRussia)
                                    {
                                        DemoScanner_AddWarn(
                                            "[ОШИБКА ВРЕМЕНИ. ДЕМО ВЗЛОМАНО?!:" + AlternativeTimeCounter + " ] на (" +
                                            CurrentTime + "):" + CurrentTimeString, true, true, true);
                                    }
                                    else
                                    {
                                        DemoScanner_AddWarn(
                                            "[TIME ERROR! DEMO IS CRACKED?!:" + AlternativeTimeCounter + " ] at (" +
                                            CurrentTime + "):" + CurrentTimeString, true, true, true);
                                    }

                                    DemoScanner.ForceFlushScanResults();

                                    Thread.Sleep(5000);

                                    Console.Clear();

                                    if (IsRussia)
                                    {
                                        DemoScanner_AddInfo("ПОПЫТКА СМЕНЫ РЕЖИМА ПОДСЧЕТА ВРЕМЕНИ. РЕЖИМ №:" + (AlternativeTimeCounter + 1));
                                    }
                                    else
                                    {
                                        DemoScanner_AddInfo("TRY TO CHANGE TIME METHOD. METHOD №:" + (AlternativeTimeCounter + 1));
                                    }

                                    if (AlternativeTimeCounter >= 2)
                                    {
                                        if (IsRussia)
                                        {
                                            DemoScanner_AddInfo("В этом режиме время не будет совпадать с игровым!!");
                                        }
                                        else
                                        {
                                            DemoScanner_AddInfo("In this method demo time can not be synced with game time!");
                                        }
                                    }

                                    Console.WriteLine();

                                    /* START: SOME BLACK MAGIC OUTSIDE HOGWARTS */
                                    FieldInfo[] fields = typeof(DemoScanner).GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

                                    foreach (var field in fields)
                                    {
                                        try
                                        {
                                            Type fieldType = field.FieldType;
                                            object defaultValue = fieldType.IsValueType ? Activator.CreateInstance(fieldType) : null;
                                            field.SetValue(null, defaultValue);
                                        }
                                        catch
                                        {
                                        }
                                    }
                                    typeof(DemoScanner)
                                        .GetConstructor(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, null,
                                            new Type[0], null).Invoke(null, null);
                                    /* END: VERY DARK BLACK MAGIC!!!!!! */

                                    DUMP_ALL_FRAMES = isDump;
                                    AlternativeTimeCounter = newAltTimer;
                                    DemoRescanned = true;
                                    goto DEMO_FULLRESET;
                                }

                                if (BadTimeFound > 500 && AlternativeTimeCounter == 3)
                                {
                                    BadTimeFound = 0;
                                    AlternativeTimeCounter++;
                                }
                                else
                                {
                                    if (CurrentTime < PreviousTime)
                                    {
                                        BadFoundTime = PreviousTime;
                                        BadTimeFound++;
                                    }
                                }

                                if (PREVIEW_FRAMES && IsUserAlive())
                                {
                                    PreviewFramesWriter.Write(CurrentTime);
                                    PreviewFramesWriter.Write(Convert.ToUInt16(PreviousNetMsgFrame.UCmd.Buttons));
                                    PreviewFramesWriter.Write(PreviousNetMsgFrame.RParms.Viewangles.X);
                                    PreviewFramesWriter.Write(PreviousNetMsgFrame.RParms.Viewangles.Y);
                                    PreviewFramesWriter.Write(PreviousNetMsgFrame.RParms.Viewangles.Z);
                                    PreviewFramesWriter.Write(PreviousNetMsgFrame.RParms.ClViewangles.X);
                                    PreviewFramesWriter.Write(PreviousNetMsgFrame.RParms.ClViewangles.Y);
                                    PreviewFramesWriter.Write(PreviousNetMsgFrame.RParms.ClViewangles.Z);
                                    PreviewFramesWriter.Write(PreviousNetMsgFrame.UCmd.Viewangles.X);
                                    PreviewFramesWriter.Write(PreviousNetMsgFrame.UCmd.Viewangles.Y);
                                    PreviewFramesWriter.Write(PreviousNetMsgFrame.UCmd.Viewangles.Z);
                                    PreviewFramesWriter.Write(CDFRAME_ViewAngles.X);
                                    PreviewFramesWriter.Write(CDFRAME_ViewAngles.Y);
                                    PreviewFramesWriter.Write(CDFRAME_ViewAngles.Z);
                                }


                                if (abs(nf.RParms.Punchangle.Y) > EPSILON)
                                {
                                    addAngleInViewListYDelta(give_bad_float(-15000.0f));
                                }
                                else
                                {
                                    addAngleInViewListYDelta(give_bad_float(nf.RParms.ClViewangles.Y));
                                }

                                if (abs(CurrentTime - PreviousTime) > 0.15f)
                                {
                                    LastLossTime = PreviousTime;
                                    LastLossTimeEnd = CurrentTime;
                                    LossPackets2++;
                                }

                                /*
                                   if (abs(CurrentTime) > EPSILON && abs(CurrentTime - PreviousTime) > 0.01 && CurrentFrameTimeBetween > abs(CurrentTime - PreviousTime) + 0.2)
                                    {
                                        ServerLagCount++;
                                    }
                                */

                                CurrentFrameTimeBetween = 0.0f;
                                try
                                {
                                    CurrentTimeString = "MODIFIED";
                                    var t = TimeSpan.FromSeconds(CurrentTime);
                                    CurrentTimeString = string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms", t.Hours,
                                        t.Minutes, t.Seconds, t.Milliseconds);
                                    lastnormalanswer = CurrentTimeString;
                                    Console.Title = "[ANTICHEAT/ANTIHACK] " + PROGRAMNAME + " " + PROGRAMVERSION +
                                                    ". Demo:" + DemoName + ". DEMO TIME: " + CurrentTimeString;
                                }
                                catch
                                {
                                    ModifiedDemoFrames += 1;
                                    try
                                    {
                                        Console.Title = "[ANTICHEAT/ANTIHACK] " + PROGRAMNAME + " " + PROGRAMVERSION +
                                                        ". Demo:" + DemoName + ". DEMO TIME: " + lastnormalanswer;
                                    }
                                    catch
                                    {
                                        try
                                        {
                                            Console.Title = "[ANTICHEAT/ANTIHACK] " + PROGRAMNAME + " " + PROGRAMVERSION +
                                                            ". Demo:" + "BAD NAME" + ". DEMO TIME: " + lastnormalanswer;
                                        }
                                        catch
                                        {
                                            Console.WriteLine("Error access to frametime.");
                                        }
                                    }
                                }

                                if (abs(CurrentTime) > EPSILON && StartGameSecond > CurrentTime)
                                {
                                    StartGameSecond = CurrentTime;
                                }

                                if (abs(CurrentTime) > EPSILON && EndGameSecond < CurrentTime)
                                {
                                    EndGameSecond = CurrentTime;
                                }

                                if (abs(CurrentTime) > EPSILON)
                                {
                                    CurrentMsgBytes += nf.MsgBytes.Length;
                                }

                                if (SkipNextErrors)
                                {
                                    SkipNextErrors = false;
                                    if (IsRussia)
                                    {
                                        Console.WriteLine(
                                            "Критическая ошибка сканирования[2]. \nРезультаты сканирования могут быть не однозначными...");
                                    }
                                    else
                                    {
                                        Console.WriteLine(
                                            "Critical error in message parser.[2] \nThe scan result may not be unambiguous...");
                                    }
                                }

                                ParseGameData(halfLifeDemoParser, nf.MsgBytes);

                                if (SkipNextErrors)
                                {
                                    SkipNextErrors = false;
                                    if (IsRussia)
                                    {
                                        Console.WriteLine(
                                            "Критическая ошибка сканирования[3]. \nРезультаты сканирования могут быть не однозначными...");
                                    }
                                    else
                                    {
                                        Console.WriteLine(
                                            "Critical error in message parser.[3] \nThe scan result may not be unambiguous...");
                                    }
                                }

                                if (nf.MsgBytes.Length < 8 && abs(CurrentTime) > EPSILON)
                                {
                                    EmptyFrames++;
                                    if (EmptyFrames > 5 && EmptyFrames < 7)
                                    {
                                        // DemoScanner_AddWarn("MORE THAN ONE EMPTY FRAMES:" + CurrentTimeString, false);
                                    }
                                }

                                if (abs(CurrentTime) > EPSILON && !FoundFirstTime)
                                {
                                    DemoStartTime = CurrentTime;
                                    FoundFirstTime = true;
                                }

                                if (abs(CurrentTime2) > EPSILON && !FoundFirstTime2)
                                {
                                    DemoStartTime2 = CurrentTime2;
                                    FoundFirstTime2 = true;
                                }

                                if (CurrentFrameDuplicated > 0)
                                {
                                    CurrentFrameDuplicated -= 1;
                                }

                                PreviousFrameTime = CurrentFrameTime;
                                CurrentFrameTime = nf.RParms.Frametime;
                                CurrentFrameLerp = nf.UCmd.LerpMsec;
                                CurrentFramePunchangleZ = nf.RParms.Punchangle.Z;

                                if (abs(CurrentTime) > EPSILON)
                                {
                                    if (FrametimeMin > CurrentFrameTime && CurrentFrameTime > 0.0f)
                                    {
                                        FrametimeMin = CurrentFrameTime;
                                    }

                                    if (FrametimeMax < CurrentFrameTime)
                                    {
                                        FrametimeMax = CurrentFrameTime;
                                    }

                                    if (MsecMin > nf.UCmd.Msec && nf.UCmd.Msec > 0.0f)
                                    {
                                        MsecMin = nf.UCmd.Msec;
                                    }

                                    if (MsecMax < nf.UCmd.Msec)
                                    {
                                        MsecMax = nf.UCmd.Msec;
                                    }
                                }

                                if (abs(CurrentTime) > EPSILON &&
                                    abs(PreviousNetMsgFrame.RParms.Time - nf.RParms.Time) < EPSILON)
                                {
                                    if (DUMP_ALL_FRAMES)
                                    {
                                        subnode.Text += "[D U P L I C A D E R]";
                                    }

                                    CurrentFrameDuplicated = 2;
                                    FrameDuplicates++;
                                }

                                TmpPlayerNum = nf.RParms.Playernum;
                                TmpPlayerEnt = nf.RParms.Viewentity;

                                if (abs(CurrentTime) > EPSILON && CurrentFrameDuplicated <= 1)
                                {
                                    if (LocalPlayerId < 0)
                                    {
                                        LocalPlayerId = TmpPlayerNum;
                                        //if (DUMP_ALL_FRAMES) subnode.Text += "[HERE PLAYER FOUND]";
                                        //Console.WriteLine(LocalPlayerId.ToString());
                                    }
                                }

                                var voice_time = abs(CurrentTime - PreviousTime);
                                if (voice_time > 1.0f)
                                {
                                    voice_time = 1.0f;
                                }

                                PlayersVoiceTimer += voice_time;

                                CurrentFrameAlive = UserAlive;
                                RealAlive = CurrentFrameAlive && PreviousFrameAlive;

                                //Console.WriteLine((RealAlive ? "ALIVE!" : "DEAD!") + "at " + CurrentTimeString);

                                if (SkipNextAttack == 2)
                                {
                                    SkipNextAttack = 1;
                                }

                                CurrentFrameButtons = nf.UCmd.Buttons;
                                if (CurrentFrameButtons.HasFlag(GoldSource.UCMD_BUTTONS.IN_ATTACK2))
                                {
                                    CurrentFrameAttacked2 = true;
                                    LastAttackPressed = CurrentTime;
                                }
                                else
                                {
                                    CurrentFrameAttacked2 = false;
                                }

                                if (CurrentFrameButtons.HasFlag(GoldSource.UCMD_BUTTONS.IN_SCORE))
                                {
                                    LastScoreTime = CurrentTime;
                                }

                                if (CurrentFrameButtons.HasFlag(GoldSource.UCMD_BUTTONS.IN_ATTACK))
                                {
                                    NewAttackForTrigger += 1;
                                    FrameAttackStrike++;
                                    FrameUnattackStrike = 0;
                                    CurrentFrameAttacked = true;
                                    LastAttackPressed = CurrentTime;
                                    if (!IsInAttack() && NeedIgnoreAttackFlag > 0)
                                    {
                                        if (CurrentFrameDuplicated <= 0)
                                        {
                                            NeedIgnoreAttackFlag += 2;
                                        }
                                        else
                                        {
                                            NeedIgnoreAttackFlag += 1;
                                        }
                                    }
                                    if (IsCmdChangeWeapon() || IsRealChangeWeapon() || !IsUserAlive())
                                    {
                                        LastEventDetectTime = 0.0f;
                                    }
                                    if (abs(LastEventDetectTime) > EPSILON && abs(LastAttackPressed - LastEventDetectTime) > 0.25)
                                    {
                                        if (DemoScanner_AddWarn("[BETA] [TRIGGER TYPE 3." + (LastEventId < 0 ? 2 : 1) + " " + CurrentWeapon + "] at (" + CurrentTime + ") " +
                                                CurrentTimeString, false))
                                        {
                                            TriggerAimAttackCount++;
                                            LastTriggerAttack = CurrentTime;
                                        }

                                        /*foreach (var res in DownloadedResources)
                                        {
                                            if (res.res_index == Math.Abs(LastEventId) && res.res_type == 5)
                                            {
                                                Console.WriteLine("[Debug] event:" + res.res_path + " at " + CurrentTimeString);
                                            }
                                        }*/
                                        LastEventDetectTime = 0.0f;
                                    }
                                    else
                                    {
                                        LastEventDetectTime = 0.0f;
                                    }
                                }
                                else
                                {
                                    IsAttack--;
                                    FrameUnattackStrike++;
                                    FrameAttackStrike = 0;
                                    if (!NeedSearchAim3 && PreviousFrameAttacked && RealAlive &&
                                        abs(CurrentTime) > EPSILON && abs(CurrentTime - IsAttackLastTime) > 0.075f &&
                                        abs(CurrentTime - IsNoAttackLastTime) > 0.075f)
                                    {
                                        if (StopAttackBtnFrameId != CurrentNetMsgFrameId)
                                        {
                                            NeedSearchAim3 = true;
                                            StopAttackBtnFrameId = CurrentNetMsgFrameId;
                                        }
                                    }
                                    else if (!RealAlive)
                                    {
                                        NeedSearchAim3 = false;
                                    }

                                    if (!IsInAttack())
                                    {
                                        NeedIgnoreAttackFlag = 0;
                                    }

                                    CurrentFrameAttacked = false;
                                }

                                if (NeedIgnoreAttackFlag == 6)
                                {
                                    NeedIgnoreAttackFlagCount++;
                                }

                                CurrentFrameForward = CurrentFrameButtons.HasFlag(GoldSource.UCMD_BUTTONS.IN_FORWARD);
                                if (RealAlive && !InStrafe && !InForward && !InBack && !PreviousFrameForward &&
                                    CurrentFrameForward && abs(CurrentTime - LastMoveForward) > 1.0f &&
                                    abs(CurrentTime - LastMoveBack) > 1.0f && abs(CurrentTime - LastUnMoveForward) > 1.5f)
                                {
                                    if (abs(CurrentTime - LastMovementHackTime) > 1.5)
                                    {
                                        DemoScanner_AddWarn("[FORWARD HACK TYPE 1] at (" + CurrentTime + ") " +
                                                            CurrentTimeString);
                                        LastMovementHackTime = CurrentTime;
                                    }
                                }

                                NewAttack = false;
                                NewAttack2 = false;

                                if (CurrentFrameButtons.HasFlag(GoldSource.UCMD_BUTTONS.IN_RELOAD) &&
                                    !PreviousFrameButtons.HasFlag(GoldSource.UCMD_BUTTONS.IN_RELOAD))
                                {
                                    Reloads3++;
                                    CheckConsoleCommand("Reload key set", true);
                                    if (IsUserAlive())
                                    {
                                        if (abs(CurrentTime - ReloadKeyPressTime) > 0.5)
                                        {
                                            ReloadHackTime = CurrentTime;
                                        }
                                        else if (PreviousFrameButtons.HasFlag(GoldSource.UCMD_BUTTONS.IN_ATTACK) &&
                                                 !CurrentFrameButtons.HasFlag(GoldSource.UCMD_BUTTONS.IN_ATTACK))
                                        {
                                            if ((abs(IsNoAttackLastTime - CurrentTime) > 0.1 &&
                                                abs(IsAttackLastTime - CurrentTime) > 0.1) ||
                                                abs(ReloadKeyPressTime - CurrentTime) > 0.1)
                                            {
                                                DemoScanner_AddWarn("[BETA] [SILENT RELOAD " + CurrentWeapon + "] at (" +
                                                                    CurrentTime + ") " + CurrentTimeString);
                                            }
                                        }
                                    }
                                }

                                if (CurrentFrameButtons.HasFlag(GoldSource.UCMD_BUTTONS.IN_ATTACK) &&
                                    !PreviousFrameButtons.HasFlag(GoldSource.UCMD_BUTTONS.IN_ATTACK))
                                {
                                    // Console.WriteLine("+IN_ATTACK + " + CurrentTimeString + "[" + PREV_CDFRAME_ViewAngles.Y +"/" + CDFRAME_ViewAngles.Y + "]");
                                    LastAttackBtnTime = CurrentTime;
                                    //Console.WriteLine("ATTACK");

                                    //PrimaryCheckTimer--;
                                    //if (abs(PrimaryCheckTimer) > 2)
                                    //{
                                    //    //Console.WriteLine("ClearFuckPrimary2");
                                    //    PrimaryCheckTimer = 0;
                                    //    PrimaryAttackHistory.Clear();
                                    //}

                                }
                                //    if (IsUserAlive() && !IsCmdChangeWeapon() && abs(LastPrimaryAttackTime) > EPSILON &&
                                //        abs(LastPrevPrimaryAttackTime) > EPSILON &&
                                //        abs(CurrentTime - LastPrevPrimaryAttackTime) <= abs(PrimaryAttackHistory[3]))
                                //    {
                                //        if (abs(IsAttackLastTime - LastAttackCmdTime) < EPSILON &&
                                //            abs(IsAttackLastTime - IsNoAttackLastTime) > EPSILON &&
                                //            PrimaryAttackHistory[3] > PrimaryAttackHistory[2] &&
                                //            PrimaryAttackHistory[1] > PrimaryAttackHistory[2] &&
                                //            abs(abs(CurrentTime - LastPrevPrimaryAttackTime) -
                                //                abs(PrimaryAttackHistory[2])) < 0.06f)
                                //        {
                                //            //Console.WriteLine(CurrentTime + "/" + LastPrevPrimaryAttackTime + "/" + LastAttackCmdTime
                                //            //    + "/" + IsAttackLastTime + "/" + IsNoAttackLastTime + "/" + (PrimaryAttackHistory[3] > PrimaryAttackHistory[2] && PrimaryAttackHistory[1] > PrimaryAttackHistory[2]).ToString()
                                //            //    + "/" + ((abs(abs(CurrentTime - LastPrevPrimaryAttackTime) - abs(PrimaryAttackHistory[2]))) < 0.06f).ToString());
                                //            if (abs(IsAttackLastTime - LastPrevPrimaryAttackTime) < EPSILON)
                                //                AutoPistolStrikes++;
                                //            if (AutoPistolStrikes == 3)
                                //            {
                                //                DemoScanner_AddWarn(
                                //                    "[AIM TYPE 2.2 " + CurrentWeapon + "] at (" + CurrentTime + ") " +
                                //                    CurrentTimeString,
                                //                    SkipAimType22-- <= 0 && !IsPlayerLossConnection() &&
                                //                    !IsCmdChangeWeapon() && !IsAngleEditByEngine());
                                //                AutoPistolStrikes = 0;
                                //            }
                                //        }
                                //        else
                                //        {
                                //            AutoPistolStrikes = 0;
                                //        }

                                //        LastPrimaryAttackTime = 0.0f;
                                //        LastPrevPrimaryAttackTime = 0.0f;
                                //    }
                                //    else
                                //    {
                                //        AutoPistolStrikes = 0;
                                //    }
                                //}

                                //if (!IsUserAlive())
                                //{
                                //    ReloadHackTime = 0.0f;
                                //    AutoPistolStrikes = 0;
                                //    LastPrimaryAttackTime = 0.0f;
                                //    LastPrevPrimaryAttackTime = 0.0f;
                                //}

                                if (!IsUserAlive())
                                {
                                    ReloadHackTime = 0.0f;
                                }

                                if (abs(ReloadHackTime) > EPSILON && abs(CurrentTime - ReloadHackTime) > 1.0)
                                {
                                    DemoScanner_AddWarn(
                                        "[AUTORELOAD TYPE 1 " + CurrentWeapon + "] at (" + ReloadHackTime + ") " +
                                        GetTimeString(ReloadHackTime), !IsCmdChangeWeapon() && !IsAngleEditByEngine());
                                    ReloadHackTime = 0.0f;
                                }

                                if (IsUserAlive())
                                {
                                    if (CurrentFrameDuplicated == 0 && InitAimMissingSearch > 1)
                                    {
                                        InitAimMissingSearch--;
                                    }

                                    //if (InitAimMissingSearch == 1)
                                    //{
                                    //    if (CurrentTime - IsAttackLastTime > 0.10)
                                    //    {
                                    //        DemoScanner_AddWarn(
                                    //                   "[AIM TYPE 1.5 " + CurrentWeapon.ToString() + "] at (" + IsAttackLastTime +
                                    //                   "):" + CurrentTimeString, false);
                                    //        SilentAimDetected++;
                                    //        InitAimMissingSearch = 0;
                                    //    }
                                    //}

                                    if (CurrentFrameAttacked)
                                    {
                                        if (InitAimMissingSearch > 0)
                                        {
                                            InitAimMissingSearch = 0;
                                        }
                                    }
                                    else
                                    {
                                        if (FirstAttack && IsRealWeapon())
                                        {
                                            if (InitAimMissingSearch == 1)
                                            {
                                                if (DemoScanner_AddWarn(
                                                    "[AIM TYPE 4.2 " + CurrentWeapon + "] at (" + IsAttackLastTime + "):" +
                                                    GetTimeString(IsAttackLastTime),
                                                    !IsCmdChangeWeapon() && !IsAngleEditByEngine() && !IsReload &&
                                                    SelectSlot <= 0 && !IsPlayerLossConnection() && !IsForceCenterView() && !IsPlayerInDuck() && !IsPlayerUnDuck()))
                                                {
                                                    TotalAimBotDetected++;
                                                }
                                                InitAimMissingSearch = 0;
                                            }
                                        }
                                        else
                                        {
                                            if (InitAimMissingSearch > 0)
                                            {
                                                InitAimMissingSearch = 0;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (InitAimMissingSearch > 0)
                                    {
                                        InitAimMissingSearch = 0;
                                    }
                                }

                                if (!PreviousFrameAttacked2 && CurrentFrameAttacked2)
                                {
                                    if (CurrentWeapon == WeaponIdType.WEAPON_KNIFE)
                                    {
                                        NewAttack2 = true;
                                        NewAttack2Frame = CurrentFrameIdAll;
                                    }
                                }

                                if (!PreviousFrameAttacked && CurrentFrameAttacked)
                                {
                                    if (IsUserAlive())
                                    {
                                        attackscounter3++;
                                    }

                                    //if (abs(CurrentTime - IsNoAttackLastTime) > 240.0)
                                    //{
                                    //    UsingAnotherMethodWeaponDetection = !UsingAnotherMethodWeaponDetection;
                                    //}

                                    if (DEBUG_ENABLED)
                                    {
                                        Console.WriteLine("ATTACK:" + CurrentWeapon);
                                    }

                                    FirstAttack = true;
                                    NewAttack = true;
                                    NewAttackFrame = CurrentFrameIdAll;

                                    if (IsForceCenterView())
                                    {
                                        if (abs(CurrentTime - LastForceCenterView) > 10.0f)
                                        {
                                            DemoScanner_AddInfo("[ILLEGAL FORCE_CENTERVIEW] at (" + CurrentTime + "):" +
                                                                CurrentTimeString);
                                            LastForceCenterView = CurrentTime;
                                        }

                                        LastAim5DetectedReal = 0.0f;
                                        LastAim5Detected = 0.0f;
                                    }
                                }

                                if (CurrentWeapon == WeaponIdType.WEAPON_KNIFE || CurrentWeapon == WeaponIdType.WEAPON_C4 ||
                                    CurrentWeapon == WeaponIdType.WEAPON_HEGRENADE ||
                                    CurrentWeapon == WeaponIdType.WEAPON_SMOKEGRENADE ||
                                    CurrentWeapon == WeaponIdType.WEAPON_FLASHBANG ||
                                    CurrentWeapon == WeaponIdType.WEAPON_NONE || CurrentWeapon == WeaponIdType.WEAPON_BAD ||
                                    CurrentWeapon == WeaponIdType.WEAPON_BAD2 //|| IsAngleEditByEngine()
                                    || !RealAlive)
                                {
                                    ReloadWarns = 0;
                                }

                                if (CurrentWeapon == WeaponIdType.WEAPON_XM1014 || CurrentWeapon == WeaponIdType.WEAPON_M3)
                                {
                                    ReloadWarns = 0;
                                }

                                if (RealAlive)
                                {
                                    if (NewAttack && !IsAngleEditByEngine())
                                    {
                                        NewAttackTime = CurrentTime;
                                        NewAttackTimeAim9 = CurrentTime;
                                        if (abs(CurrentTime - Aim8DetectionTimeX) < 0.25)
                                        {
                                            //Console.WriteLine("Aim8DetectionTimeX warn!");
                                            Aim8DetectionTimeX = 0.0f;
                                        }
                                        else if (abs(CurrentTime - Aim8DetectionTimeY) < 0.25)
                                        {
                                            //Console.WriteLine("Aim8DetectionTimeY warn!");
                                            Aim8DetectionTimeY = 0.0f;
                                        }
                                    }

                                    if (MoveLeft && !MoveRight)
                                    {
                                        MoveRightStrike = 0;
                                        MoveLeftStrike++;
                                    }
                                    else if (MoveRight && !MoveLeft)
                                    {
                                        MoveLeftStrike = 0;
                                        MoveRightStrike++;
                                    }
                                    else
                                    {
                                        MoveRightStrike = 0;
                                        MoveLeftStrike = 0;
                                    }
                                }
                                else
                                {
                                    MoveRightStrike = 0;
                                    MoveLeftStrike = 0;
                                }

                                if (abs(nf.UCmd.Sidemove) > EPSILON)
                                {
                                    LastSideMoveTime = CurrentTime;
                                }

                                if (abs(nf.UCmd.Forwardmove) < EPSILON)
                                {

                                }
                                else if (nf.UCmd.Forwardmove > 0.1f)
                                {
                                    LastForwardMoveTime = CurrentTime;
                                }
                                else if (nf.UCmd.Forwardmove < 0.1f)
                                {
                                    LastBackMoveTime = CurrentTime;
                                }

                                // if (RealAlive)
                                //if (CurrentFrameAttacked)
                                //{
                                //    Console.WriteLine("AngleDirX111:" + GetAngleDirection(nf.RParms.ClViewangles.X, PreviousNetMsgFrame.RParms.ClViewangles.X) +
                                //        ". AngleDirY111:" + GetAngleDirection(nf.RParms.ClViewangles.Y, PreviousNetMsgFrame.RParms.ClViewangles.Y));
                                //    Console.WriteLine("AngleDirX222:" + GetAngleDirection(nf.RParms.Viewangles.X, PreviousNetMsgFrame.RParms.Viewangles.X) +
                                //        ". AngleDirY222:" + GetAngleDirection(nf.RParms.Viewangles.Y, PreviousNetMsgFrame.RParms.Viewangles.Y));
                                //    Console.WriteLine("AngleDirX333:" + GetAngleDirection(nf.UCmd.Viewangles.Y, PreviousNetMsgFrame.UCmd.Viewangles.Y) +
                                //        ". AngleDirY333:" + GetAngleDirection(nf.UCmd.Viewangles.Y, PreviousNetMsgFrame.UCmd.Viewangles.Y) + "\n");
                                //}
                                /* if (IsRoundEnd() || /*LastBackMoveTime == 0.0f ||*/
                                /*LastForwardMoveTime == 0.0f || LastMoveForward == 0.0f)
                                         /*{
                                             DesyncHackWarns = 0;
                                         }

                                         if (CurrentTime - LastJumpTime < 1.5 || !CurrentFrameOnGround)
                                         {
                                             DesyncHackWarns = 0;
                                         }

                                         if (CurrentTime - LastMoveBack < 1.5)
                                         {
                                             DesyncHackWarns = 0;
                                         }

                                         if (IsBigVelocity() || IsHookDetected())
                                         {
                                             DesyncHackWarns = 0;
                                         }
                                         else if (IsPlayerLossConnection() && DesyncHackWarns > 0)
                                         {
                                             DesyncHackWarns--;
                                         }
                                         else if (IsAngleEditByEngine() && DesyncHackWarns > 0)
                                         {
                                             DesyncHackWarns = 0;
                                         }
                                         if (abs(CurrentTime - LastScreenshotTime) < 30.0f)
                                         {
                                             DesyncHackWarns = 0;
                                         }

                                         if (InBack || CurrentTime - LastMoveBack < 2.0)
                                         {
                                             DesyncHackWarns = 0;
                                         }

                                         if (InForward || CurrentTime - LastMoveForward < 2.0)
                                         {
                                             DesyncHackWarns = 0;
                                         }

                                         if (abs(CurrentTime - GameEndTime2) < 30.0f)
                                         {
                                             DesyncHackWarns = 0;
                                         }

                                         if (abs(CurrentTime - GameStartTime) < 60.0f)
                                         {
                                             DesyncHackWarns = 0;
                                         }

                                         if (IsPlayerTeleport())
                                         {
                                             DesyncHackWarns = 0;
                                         }

                                         if (abs(nf.RParms.Simvel.X) > 100.0f ||
                                                 abs(nf.RParms.Simvel.Y) > 100.0f || abs(nf.RParms.Simvel.X) < -100.0f ||
                                                 abs(nf.RParms.Simvel.Y) < -100.0f || nf.RParms.Simvel.Z != 0.0f)
                                         {
                                             DesyncHackWarns = 0;
                                         }

                                         if (CurrentTime - LastSideMoveTime < 2.5 ||
                                          CurrentTime - LastForwardMoveTime < 2.5 ||
                                          CurrentTime - LastBackMoveTime < 2.5)
                                         {
                                             if (DesyncHackWarns > 0)
                                             {
                                                 DesyncHackWarns--;
                                             }
                                         }

                                         if (abs(nf.RParms.Simvel.X) > 0.1f ||
                                             abs(nf.RParms.Simvel.Y) > 0.1f)
                                         {
                                             DesyncHackWarns++;

                                             if (DesyncHackWarns > 4 && CurrentTime - LastDesyncDetectTime > 0.1)
                                             {
                                                 LastDesyncDetectTime = CurrentTime;
                                                 if (!IsAngleEditByEngine() && !IsPlayerLossConnection() && CurrentTime - LastSoundTime > 2.0)
                                                 {
                                                     DesyncDetects++;
                                                     DemoScanner_AddWarn(
                                                        "[DESYNC HACK] at (" +
                                                        CurrentTime + ") " + CurrentTimeString);
                                                 }
                                                 DesyncHackWarns = 0;
                                             }
                                         }
                                         else
                                         {
                                             if (DesyncHackWarns > 0)
                                             {
                                                 DesyncHackWarns--;
                                             }
                                         }*/
                                if ((nf.UCmd.Sidemove < -40.0f || nf.UCmd.Sidemove > 40.0f) &&
                                    !InStrafe && !MoveLeft && !MoveRight &&
                                    abs(CurrentTime - LastMoveLeft) > 0.5f &&
                                    abs(CurrentTime - LastMoveRight) > 0.5f &&
                                    abs(CurrentTime - LastUnMoveLeft) > 0.5f &&
                                    abs(CurrentTime - LastUnMoveRight) > 0.5f &&
                                    abs(CurrentTime - LastStrafeDisabled) > 0.5f &&
                                    abs(CurrentTime - LastStrafeEnabled) > 0.5f &&
                                    abs(CurrentTime - LastMovementHackTime) > 2.5f)
                                {
                                    if (DemoScanner_AddWarn(
                                        "[MOVEMENT HACK TYPE 2] at (" + CurrentTime + ") " + CurrentTimeString,
                                        IsValidMovement() && !IsPlayerLossConnection() &&
                                        (nf.UCmd.Sidemove < -100 || nf.UCmd.Sidemove > 100)))
                                    {
                                        KreedzHacksCount++;
                                        LastMovementHackTime = CurrentTime;
                                    }
                                }

                                if (SearchMoveHack1)
                                {
                                    if (CurrentFrameButtons.HasFlag(GoldSource.UCMD_BUTTONS.IN_MOVELEFT) ||
                                        CurrentFrameButtons.HasFlag(GoldSource.UCMD_BUTTONS.IN_MOVERIGHT))
                                    {
                                        //Console.WriteLine("MovementPressed " +
                                        //    (MoveLeft ? "MOVELEFT PRESSED |" : "NO MOVELEFT |") + " " +
                                        //    (MoveRight ? "MOVERIGHT PRESSED |" : "NO MOVERIGHT |") + " " +
                                        //    +CurrentTime + " " +
                                        //    LastMoveLeft + " " + LastMoveRight + " " + LastUnMoveLeft + " " + LastUnMoveRight + 
                                        //    " -> " + nf.UCmd.Sidemove);
                                        if (!MoveLeft && !MoveRight && abs(CurrentTime - LastMoveLeft) > 0.5f &&
                                            abs(CurrentTime - LastMoveRight) > 0.5f &&
                                            abs(CurrentTime - LastUnMoveLeft) > 0.5f &&
                                            abs(CurrentTime - LastUnMoveRight) > 0.5f)
                                        {
                                            DemoScanner_AddWarn(
                                                "[MOVEMENT HACK TYPE 1] at (" + CurrentTime + ") " + CurrentTimeString,
                                                !IsAngleEditByEngine() && !IsPlayerLossConnection());
                                        }
                                        SearchMoveHack1 = false;
                                    }

                                    if (SearchMoveHack1)
                                    {
                                        if (DemoScanner_AddWarn(
                                            "[MOVEMENT HACK TYPE 1] at (" + CurrentTime + ") " + CurrentTimeString, false))
                                        {
                                            KreedzHacksCount++;
                                        }
                                        SearchMoveHack1 = false;
                                    }
                                }

                                if (RealAlive)
                                {
                                    if (CurrentFrameButtons.HasFlag(GoldSource.UCMD_BUTTONS.IN_MOVELEFT) ||
                                        CurrentFrameButtons.HasFlag(GoldSource.UCMD_BUTTONS.IN_MOVERIGHT))
                                    {
                                        //Console.WriteLine("MovementPressed " +
                                        //    (MoveLeft ? "MOVELEFT PRESSED |" : "NO MOVELEFT |") + " " +
                                        //    (MoveRight ? "MOVERIGHT PRESSED |" : "NO MOVERIGHT |") + " " +
                                        //    +CurrentTime + " " +
                                        //    LastMoveLeft + " " + LastMoveRight + " " + LastUnMoveLeft + " " + LastUnMoveRight + 
                                        //    " -> " + nf.UCmd.Sidemove);
                                        if (!MoveLeft && !MoveRight &&
                                            abs(CurrentTime - LastMoveLeft) > 0.5f &&
                                            abs(CurrentTime - LastMoveRight) > 0.5f &&
                                            abs(CurrentTime - LastUnMoveLeft) > 0.5f &&
                                            abs(CurrentTime - LastUnMoveRight) > 0.5f)
                                        {
                                            if (abs(CurrentTime - LastMovementHackTime) > 2.5f)
                                            {
                                                SearchMoveHack1 = true;
                                                LastMovementHackTime = CurrentTime;
                                            }
                                        }
                                    }
                                }

                                if (IsInAttack())
                                {
                                    if (PreviousFrameAttacked && !CurrentFrameAttacked)
                                    {
                                        var tmpframeattacked = 0;
                                        for (var n = frameindex + 1;
                                             n < CurrentDemoFile.GsDemoInfo.DirectoryEntries[index].Frames.Count;
                                             n++)
                                        {
                                            if (tmpframeattacked == -1 || tmpframeattacked > 2)
                                            {
                                                if (tmpframeattacked > 2)
                                                {
                                                    CheckConsoleCommand("-attack(PROGRAM)", true);
                                                    LastLostAttackTime = CurrentTime;
                                                    LastLostAttackTime2 = CurrentTime;
                                                    LostStopAttackButton += 1;
                                                }

                                                break;
                                            }

                                            var tmpframe = CurrentDemoFile.GsDemoInfo.DirectoryEntries[index].Frames[n];
                                            switch (tmpframe.Key.Type)
                                            {
                                                case GoldSource.DemoFrameType.DemoStart:
                                                    break;
                                                case GoldSource.DemoFrameType.ConsoleCommand:
                                                    break;
                                                case GoldSource.DemoFrameType.ClientData:
                                                    break;
                                                case GoldSource.DemoFrameType.NextSection:
                                                    break;
                                                case GoldSource.DemoFrameType.Event:
                                                    break;
                                                case GoldSource.DemoFrameType.WeaponAnim:
                                                    break;
                                                case GoldSource.DemoFrameType.Sound:
                                                    break;
                                                case GoldSource.DemoFrameType.DemoBuffer:
                                                    break;
                                                case GoldSource.DemoFrameType.NetMsg:
                                                default:
                                                    var tmpnetmsgframe1 = (GoldSource.NetMsgFrame)tmpframe.Value;
                                                    if (tmpnetmsgframe1 != nf)
                                                    {
                                                        if (tmpnetmsgframe1.UCmd.Buttons.HasFlag(GoldSource.UCMD_BUTTONS
                                                                .IN_ATTACK) || IsCmdChangeWeapon())
                                                        {
                                                            tmpframeattacked = -1;
                                                            if (IsCmdChangeWeapon())
                                                            {
                                                                NeedSearchAim3 = false;
                                                            }

                                                            break;
                                                        }

                                                        tmpframeattacked++;
                                                    }

                                                    break;
                                            }
                                        }
                                    }
                                }

                                if (NewAttack)
                                {
                                    if (AimType7Event == 53)
                                    {
                                        AimType7Event = 3;
                                    }

                                    if (AimType7Event == 52)
                                    {
                                        AimType7Event = 2;
                                    }
                                }
                                else
                                {
                                    if (AimType7Event == 53)
                                    {
                                        AimType7Event = 0;
                                    }

                                    if (AimType7Event == 52)
                                    {
                                        AimType7Event = 0;
                                    }
                                }

                                CurrentFrameJumped = CurrentFrameButtons.HasFlag(GoldSource.UCMD_BUTTONS.IN_JUMP);

                                if (CurrentFrameJumped)
                                {
                                    LastJumpBtnTime = CurrentTime;
                                }

                                if (SearchJumpHack5 > 1)
                                {
                                    if (RealAlive)
                                    {
                                        SearchJumpHack5--;
                                        if (IsPlayerAnyJumpPressed() || !IsUserAlive())
                                        {
                                            SearchJumpHack5 = 0;
                                        }
                                    }
                                    else
                                    {
                                        SearchJumpHack5 = 0;
                                    }
                                }
                                else if (SearchJumpHack5 == 1)
                                {

                                    SearchJumpHack5--;
                                    if (!IsPlayerAnyJumpPressed() && IsUserAlive() && !DisableJump5AndAim16)
                                    {
                                        DemoScanner_AddWarn(
                                            "[JUMPHACK TYPE 5] at (" + CurrentTime + "):" +
                                            CurrentTimeString, false, true, false, true);
                                    }
                                }

                                //if (SearchJumpHack51 > 1)
                                //{
                                //    SearchJumpHack51--;
                                //    if (IsPlayerBtnJumpPressed() || !IsUserAlive()) SearchJumpHack51 = 0;
                                //}
                                //else if (SearchJumpHack51 == 1)
                                //{
                                //    SearchJumpHack51--;
                                //    if (!IsPlayerBtnJumpPressed() && IsUserAlive() && !DisableJump5AndAim16)
                                //    {
                                //        DemoScanner_AddWarn(
                                //            "[JUMPHACK TYPE 5.1] at (" + CurrentTime + "):" +
                                //            CurrentTimeString, true, true, false, true);
                                //    }
                                //}

                                if (PreviousFrameJumped && !CurrentFrameJumped)
                                {
                                    if (NeedDetectBHOPHack && RealAlive)
                                    {
                                        BHOP_JumpWarn++;
                                        if (BHOP_JumpWarn > 10)
                                        {
                                            if (abs(CurrentTime - LastBhopTime) > 1.0f)
                                            {
                                                if (DemoScanner_AddWarn("[BHOP TYPE 1.3] at (" + CurrentTime + ") " + CurrentTimeString + " [" +
                                                                    (BHOP_JumpWarn - 1) + "]" + " times."))
                                                {
                                                    BHOPcount += BHOP_JumpWarn - 1;
                                                    LastBhopTime = CurrentTime;
                                                    BHOP_JumpWarn = 0;
                                                }
                                            }
                                        }
                                    }
                                }

                                if (!PreviousFrameJumped && CurrentFrameJumped)
                                {
                                    if (IsUserAlive())
                                    {
                                        JumpCount2++;
                                        NeedDetectBHOPHack = true;
                                    }
                                }

                                //Console.WriteLine("JMP BUTTON at (" + CurrentTime + ") : " + CurrentTimeString);
                                //if (FirstAttack && CurrentTime == IsAttackLastTime)
                                //{
                                //    SearchAim6 = true;
                                //}
                                //else
                                //{
                                //    if (SearchAim6 )
                                //    {
                                //    }
                                //    SearchAim6 = false;
                                //}
                                CurrentFrameDuck = CurrentFrameButtons.HasFlag(GoldSource.UCMD_BUTTONS.IN_DUCK);
                                if (!FirstDuck && CurrentFrameDuck && PreviousFrameDuck)
                                {
                                    FirstDuck = true;
                                    IsDuck = true;
                                }
                                else if (!FirstDuck && !CurrentFrameDuck && !PreviousFrameDuck)
                                {
                                    FirstDuck = true;
                                    IsDuck = false;
                                }

                                if (!PreviousFrameDuck && CurrentFrameDuck)
                                {
                                    LastCmdDuckTime = CurrentTime;
                                }
                                else if (PreviousFrameDuck && !CurrentFrameDuck)
                                {
                                    LastCmdUnduckTime = CurrentTime;
                                }

                                if (!CurrentFrameDuck && IsDuckPressed)
                                {
                                    IsDuckPressed = false;
                                    LastUnDuckTime = CurrentTime;
                                    //Console.WriteLine("Reset already induck!");
                                }

                                if (RealAlive && !IsDuck && !IsDuckPressed && FirstDuck &&
                                    abs(CurrentTime - LastUnDuckTime) > 2.5f && abs(CurrentTime - LastDuckTime) > 2.5f &&
                                    abs(CurrentTime - LastAliveTime) > 1.2f && !IsPlayerTeleport())
                                {
                                    if (!PreviousFrameDuck && CurrentFrameDuck)
                                    {
                                        SearchOneFrameDuck = true;
                                    }
                                    else if (SearchOneFrameDuck && PreviousFrameDuck && !CurrentFrameDuck)
                                    {
                                        if (abs(CurrentTime - LastKreedzHackTime) > 2.5f)
                                        {
                                            if (DemoScanner_AddWarn(
                                                "[DUCK HACK TYPE 3] at (" + CurrentTime + ") " + CurrentTimeString,
                                                !IsPlayerLossConnection()))
                                            {
                                                LastKreedzHackTime = CurrentTime;
                                                KreedzHacksCount++;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        SearchOneFrameDuck = false;
                                    }

                                    if (CurrentFrameDuck && PreviousFrameDuck && LastDuckTime > LastUnDuckTime &&
                                        abs(CurrentTime - LastUnDuckTime) > 1.5f && abs(CurrentTime - LastDuckTime) > 5.0f)
                                    {
                                        if (abs(CurrentTime - LastKreedzHackTime) > 2.5f)
                                        {
                                            if (DemoScanner_AddWarn(
                                                "[DUCK HACK TYPE 2] at (" + CurrentTime + ") " + CurrentTimeString, false))
                                            {
                                                LastKreedzHackTime = CurrentTime;
                                                KreedzHacksCount++;
                                            }
                                        }
                                    }

                                    if (CurrentFrameDuck && !PreviousFrameDuck && LastUnDuckTime > LastDuckTime &&
                                        abs(CurrentTime - LastDuckTime) > 2.5f && abs(CurrentTime - LastUnDuckTime) > 0.2f)
                                    {
                                        if (DuckStrikes < 2)
                                        {
                                            if (abs(CurrentTime - LastKreedzHackTime) > 2.5f)
                                            {
                                                if (DemoScanner_AddWarn(
                                                    "[DUCK HACK TYPE 1] at (" + CurrentTime + ") " + CurrentTimeString,
                                                    !IsPlayerLossConnection()))
                                                {
                                                    LastKreedzHackTime = CurrentTime;
                                                    KreedzHacksCount++;
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    SearchOneFrameDuck = false;
                                }

                                CurrentFrameOnGround = nf.RParms.Onground != 0;
                                if (CurrentFrameOnGround && PreviousFrameOnGround)
                                {
                                    FramesOnGround++;
                                    FramesOnFly = 0;
                                }

                                if (CurrentFrameOnGround)
                                {
                                    TotalFramesOnGround++;
                                }
                                else
                                {
                                    TotalFramesOnFly++;
                                }

                                if (CurrentFrameAttacked)
                                {
                                    if (!CurrentFrameOnGround)
                                    {
                                        TotalAttackFramesOnFly++;
                                    }
                                }

                                if (RealAlive && CurrentFrameOnGround)
                                {
                                    StrafeAngleDirectionChanges = 0;
                                }

                                if (NewAttack)
                                {
                                    if (!CurrentFrameOnGround)
                                    {
                                        AirShots++;
                                    }
                                }

                                //if (RealAlive)
                                //{
                                //    if (FramesOnGround > 10)
                                //    {
                                //        if (nf.UCmd.Forwardmove > MaxSpeed)
                                //        {
                                //            MaxSpeed = nf.UCmd.Forwardmove;
                                //            Console.WriteLine("Max speed : " + MaxSpeed);
                                //        }
                                //    }
                                //}
                                if (!PreviousFrameOnGround && !CurrentFrameOnGround && RealAlive &&
                                    nf.RParms.Waterlevel == 0)
                                {
                                    if (CurrentFrameDuplicated == 0)
                                    {
                                        FramesOnFly++;
                                        FramesOnGround = 0;
                                        if (nf.RParms.Simvel.Z > 100.0 && nf.RParms.Simvel.Z > PreviousSimvelZ)
                                        {
                                            if (SearchFakeJump && FlyDirection < -5 && PreviousSimvelZ < -500.0f)
                                            {
                                                if (abs(CurrentTime - LastJumpBtnTime) > 0.2)
                                                {
                                                    if (DemoScanner_AddWarn("[BETA] [JUMPHACK HPP] at (" + CurrentTime +
                                                                        ") : " + CurrentTimeString))
                                                    {
                                                        KreedzHacksCount++;
                                                    }
                                                }
                                            }

                                            if (FlyDirection < 0)
                                            {
                                                FlyDirection = 0;
                                            }

                                            FlyDirection++;
                                        }
                                        else if (nf.RParms.Simvel.Z < -100.0 && nf.RParms.Simvel.Z < PreviousSimvelZ)
                                        {
                                            if (FlyDirection > 0)
                                            {
                                                FlyDirection = 0;
                                            }

                                            FlyDirection--;
                                        }
                                        else
                                        {
                                            FlyDirection /= 2;
                                        }

                                        var abssimvelz = abs(nf.RParms.Simvel.Z);
                                        var abssimorgz = abs(nf.RParms.Simorg.Z);
                                        if (abssimvelz > 50.0)
                                        {
                                            if (abs(PreviousSimvelZ_forstuck) < 0.01)
                                            {
                                                PreviousSimvelZ_forstuck = abssimvelz;
                                                PreviousSimorgZ_forstuck = abssimorgz;
                                            }
                                            else if (abs(abssimvelz - PreviousSimvelZ_forstuck) > 3.0 ||
                                                     abs(abssimorgz - PreviousSimorgZ_forstuck) > 3.0)
                                            {
                                                PreviousSimvelZ_forstuck = 0.0f;
                                                PreviousSimorgZ_forstuck = 0.0f;
                                                AirStuckWarnTimes = 0;
                                            }
                                            else
                                            {
                                                AirStuckWarnTimes++;
                                                if (AirStuckWarnTimes > 50)
                                                {
                                                    if (DemoScanner_AddWarn("[BETA] [AIRSTUCK HACK] at (" + CurrentTime +
                                                                        ") : " + CurrentTimeString))
                                                    {
                                                        KreedzHacksCount++;
                                                        AirStuckWarnTimes = 0;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            PreviousSimvelZ_forstuck = 0.0f;
                                            PreviousSimorgZ_forstuck = 0.0f;
                                            AirStuckWarnTimes = 0;
                                        }

                                        PreviousSimvelZ = nf.RParms.Simvel.Z;
                                        SearchFakeJump = true;
                                    }
                                }
                                else
                                {
                                    PreviousSimvelZ_forstuck = 0.0f;
                                    PreviousSimorgZ_forstuck = 0.0f;
                                    FlyDirection = 0;
                                    AirStuckWarnTimes = 0;
                                }

                                //if (CurrentFrameDuplicated == 1)
                                //{
                                //    if (FlyDirection < 0)
                                //        FlyDirection++;
                                //    else if (FlyDirection > 0)
                                //        FlyDirection--;
                                //}
                                if (PreviousFrameOnGround || CurrentFrameOnGround)
                                {
                                    FlyDirection /= 2;
                                    SearchFakeJump = false;
                                }

                                if (!PreviousFrameOnGround && CurrentFrameOnGround)
                                {
                                    if (NeedDetectBHOPHack && RealAlive)
                                    {
                                        BHOP_GroundSearchDirection = 1;
                                    }
                                }
                                else if (PreviousFrameOnGround && CurrentFrameOnGround)
                                {
                                    if (NeedDetectBHOPHack && RealAlive && BHOP_GroundSearchDirection > 0)
                                    {
                                        BHOP_GroundSearchDirection += 1;
                                    }
                                    else
                                    {
                                        BHOP_GroundSearchDirection = 0;
                                    }
                                }
                                else if (PreviousFrameOnGround && !CurrentFrameOnGround)
                                {
                                    FirstJump = true;
                                    if (IsUserAlive())
                                    {
                                        if (nf.RParms.Simvel.Z > 100.0
                                            && abs(LastJumpFrame - CurrentFrameId) < 5)
                                        {
                                            JumpCount3++;
                                        }

                                        /*
                                        if (abs(CurrentTime - PluginJmpTime) > 0.35)
                                        {
                                            if (flPluginVersion > 1.62 && abs(CurrentTime - LastKreedzHackTime) > 2.5f &&
                                                        abs(LastJumpHackFalseDetectionTime) < EPSILON)
                                            {
                                                if (DemoScanner_AddWarn(
                                                    "[JUMPHACK TYPE 6] at (" + CurrentTime + ") " +
                                                    CurrentTimeString, !IsAngleEditByEngine()))
                                                {
                                                    LastKreedzHackTime = CurrentTime;
                                                    KreedzHacksCount++;
                                                }
                                            }
                                        }*/

                                        //Console.WriteLine("LastJumpNoGroundTime = " + LastJumpNoGroundTime);
                                        LastJumpNoGroundTime = CurrentTime;
                                    }

                                    if (DEBUG_ENABLED)
                                    {
                                        CheckConsoleCommand("over ground", true);
                                    }

                                    if (NeedDetectBHOPHack && RealAlive)
                                    {
                                        if (BHOP_GroundSearchDirection < 5)
                                        {
                                            BHOP_GroundWarn++;
                                        }

                                        BHOP_GroundSearchDirection = 0;
                                    }

                                    LastRealJumpTime = CurrentTime;
                                }

                                //if (NeedDetectBHOPHack)
                                //{
                                //    NeedDetectBHOPHack = false;
                                //    if (!CurrentFrameOnGround && CurrentFramePunchangleZ > 2.0f)
                                //    {
                                //        Console.WriteLine("BHOP");
                                //    }
                                //}
                                //if (!PreviousFrameOnGround && CurrentFrameOnGround &&
                                //    RealAlive
                                //    && PreviousFramePunchangleZ == 0.0f &&
                                //    CurrentFramePunchangleZ > 2.0f)
                                //    NeedDetectBHOPHack = true;


                                AddLerpAndMs(nf.UCmd.LerpMsec, nf.UCmd.Msec);
                                BadPunchAngle = abs(CurrentFramePunchangleZ) > EPSILON /* ||
                                 abs(nf.RParms.Punchangle.Y) > EPSILON ||
                                 abs(nf.RParms.Punchangle.X) > EPSILON*/;
                                addAngleInPunchListX(nf.RParms.Punchangle.X);
                                addAngleInPunchListY(nf.RParms.Punchangle.Y);

                                if (WeaponChanged || IsTakeDamage(1.5f) || CurrentWeapon == WeaponIdType.WEAPON_NONE ||
                                    CurrentWeapon == WeaponIdType.WEAPON_BAD || CurrentWeapon == WeaponIdType.WEAPON_BAD2 ||
                                    CurrentWeapon == WeaponIdType.WEAPON_C4 ||
                                    CurrentWeapon == WeaponIdType.WEAPON_HEGRENADE ||
                                    CurrentWeapon == WeaponIdType.WEAPON_SMOKEGRENADE ||
                                    CurrentWeapon == WeaponIdType.WEAPON_FLASHBANG ||
                                    !IsUserAlive() || IsAngleEditByEngine() || IsPlayerInDuck() || IsPlayerUnDuck())
                                {
                                    PunchWarnings = 0;
                                    LostAngleWarnings = 0;
                                }
                                else if (PunchWarnings > 2)
                                {
                                    PunchWarnings = 0;

                                    if (IsPlayerLossConnection())
                                    {
                                        DemoScanner_AddWarn(
                                            "[BETA] [AIM TYPE 9.2 " + CurrentWeapon + "] at (" + LastAnglePunchSearchTime +
                                            "):" + GetTimeString(LastAnglePunchSearchTime), false);
                                    }
                                    else
                                    {
                                        DemoScanner_AddWarn(
                                          "[BETA] [AIM TYPE 9.1 " + CurrentWeapon + "] at (" + LastAnglePunchSearchTime +
                                          "):" + GetTimeString(LastAnglePunchSearchTime), false);
                                    }
                                }
                                else if (LostAngleWarnings > 2)
                                {
                                    LostAngleWarnings = 0;
                                    // TODO
                                    DemoScanner_AddWarn("[BETA] [AIM TYPE 9.2 " + CurrentWeapon.ToString() + "] at (" + LastAnglePunchSearchTime +
                                     "):" + CurrentTimeString, false);
                                }

                                if (AimType7Event != 0)
                                {
                                    if (abs(Aim7PunchangleY) < EPSILON && abs(nf.RParms.Punchangle.Y) > EPSILON)
                                    {
                                        Aim7PunchangleY = nf.RParms.Punchangle.Y;
                                    }
                                }
                                else
                                {
                                    Aim7PunchangleY = 0.0f;
                                }

                                if (IsUserAlive() && abs(CurrentTime) > EPSILON)
                                {
                                    if (CurrentFrameLerp < 8)
                                    {
                                        if (abs(CurrentTime - LastCmdHack) > 5.0)
                                        {
                                            DemoScanner_AddWarn(
                                                "[CMD HACK TYPE 6] at (" + CurrentTime + ") " + CurrentTimeString,
                                                !IsAngleEditByEngine());
                                            if (DEBUG_ENABLED)
                                            {
                                                Console.WriteLine("BAD BAD LERP:" + CurrentFrameLerp);
                                            }

                                            LastCmdHack = CurrentTime;
                                        }
                                    }
                                }

                                if (!FakeLagsValus.Contains(CurrentFrameLerp))
                                {
                                    FakeLagsValus.Add(CurrentFrameLerp);
                                }

                                // 01 
                                if (RealAlive)
                                {
                                    if (SearchNextJumpStrike)
                                    {
                                        SearchNextJumpStrike = false;
                                        if (abs(IdealJmpTmpTime1 - LastUnJumpTime) > EPSILON &&
                                            abs(IdealJmpTmpTime2 - LastJumpTime) > EPSILON && PreviousFrameOnGround &&
                                            !CurrentFrameOnGround && (abs(CurrentTime - LastUnJumpTime) < 0.15f ||
                                                                      abs(CurrentTime - LastJumpTime) < 0.15f))
                                        {
                                            IdealJmpTmpTime1 = LastUnJumpTime;
                                            IdealJmpTmpTime2 = LastJumpTime;
                                            CurrentIdealJumpsStrike++;
                                            if (CurrentIdealJumpsStrike > MaxIdealJumps)
                                            {
                                                DemoScanner_AddWarn("[IDEALJUMP x" + CurrentIdealJumpsStrike + "] at (" +
                                                                    CurrentTime + ") : " + CurrentTimeString);
                                                CurrentIdealJumpsStrike = 0;
                                            }

                                            if (DEBUG_ENABLED)
                                            {
                                                Console.WriteLine("IDEALJUMP WARN [" + (CurrentFrameId - LastCmdFrameId) +
                                                                  "] (" + CurrentTime + ") : " + CurrentTimeString);
                                            }
                                        }
                                        else
                                        {
                                            CurrentIdealJumpsStrike = 0;
                                        }
                                    }
                                    else
                                    {
                                        if (!PreviousFrameOnGround && CurrentFrameOnGround)
                                        {
                                            if (FramesOnFly > 10)
                                            {
                                                SearchNextJumpStrike = true;
                                            }

                                            FlyJumps = 0;
                                        }
                                    }
                                }
                                else
                                {
                                    FlyJumps = 0;
                                    SearchNextJumpStrike = false;
                                    CurrentIdealJumpsStrike = 0;
                                }

                                if (NeedDetectThirdPersonHack)
                                {
                                    if (ThirdPersonHackDetectionTimeout > 0)
                                    {
                                        ThirdPersonHackDetectionTimeout--;
                                    }
                                    else if (ThirdPersonHackDetectionTimeout == 0)
                                    {
                                        DemoScanner_AddWarn("[THIRD PERSON TYPE 1] at (" + CurrentTime + "):" +
                                                            CurrentTimeString, !IsPlayerLossConnection());
                                        ThirdHackDetected += 1;
                                        NeedDetectThirdPersonHack = false;
                                        ThirdPersonHackDetectionTimeout = -1;
                                        if (ThirdHackDetected == 1)
                                        {
                                            if (IsRussia)
                                            {
                                                DemoScanner_AddInfo("Внимание. Обнаружен вид от третьего лица.");
                                                DemoScanner_AddInfo(
                                                    "Отключаются ложные обнаружение : NO SPREAD TYPE X и THIRD PERSON TYPE 2.");
                                            }
                                            else
                                            {
                                                DemoScanner_AddInfo("WARNING! Deteceted 'THIRD PERSON'");
                                                DemoScanner_AddInfo(
                                                    "False NO SPREAD TYPE X and THIRD PERSON TYPE 2 detection is disabled.");
                                            }
                                        }
                                    }
                                }

                                if (RealAlive && !IsAngleEditByEngine() &&
                                    abs(CurrentTime - LastJumpTime) > 0.5f)
                                {
                                    if (!NeedDetectThirdPersonHack && CurrentFrameAttacked &&
                                        GetDistance(new FPoint(nf.View.X, nf.View.Y),
                                            new FPoint(nf.RParms.Vieworg.X, nf.RParms.Vieworg.Y)) > 50 &&
                                        abs(CurrentTime - LastAliveTime) > 2.0f && !IsTakeDamage() &&
                                        abs(CurrentTime - LastDeathTime) > 5.0f && ThirdHackDetected < 5 &&
                                        CurrentWeapon != WeaponIdType.WEAPON_NONE &&
                                        CurrentWeapon != WeaponIdType.WEAPON_BAD &&
                                        CurrentWeapon != WeaponIdType.WEAPON_BAD2)
                                    {
                                        NeedDetectThirdPersonHack = true;
                                        ThirdPersonHackDetectionTimeout = 10;
                                    }
                                }
                                else
                                {
                                    NeedDetectThirdPersonHack = false;
                                    ThirdPersonHackDetectionTimeout = -1;
                                }

                                if (abs(CurrentTime2) > EPSILON)
                                {
                                    if (LastFpsCheckTime2 < 0.0)
                                    {
                                        LastFpsCheckTime2 = CurrentTime2;
                                    }

                                    if (abs(CurrentTime2 - LastFpsCheckTime2) >= 1.0f)
                                    {
                                        LastFpsCheckTime2 = CurrentTime2;
                                        if (DUMP_ALL_FRAMES)
                                        {
                                            subnode.Text += "CurrentFps2:" + CurrentFps2 + "\n";
                                        }

                                        if (CurrentFps2 > RealFpsMax2)
                                        {
                                            RealFpsMax2 = CurrentFps2;
                                        }

                                        if (CurrentFps2 < RealFpsMin2 && CurrentFps2 > 0)
                                        {
                                            RealFpsMin2 = CurrentFps2;
                                        }

                                        SecondFound2 = true;
                                        CurrentGameSecond2++;
                                        MaxIdealJumps = 10;
                                        if (averagefps2.Count > 1)
                                        {
                                            var tmpafps = averagefps2.Average();
                                            if (tmpafps < 50.0f)
                                            {
                                                MaxIdealJumps = 10;
                                            }
                                            else if (tmpafps < 60.0f)
                                            {
                                                MaxIdealJumps = 9;
                                            }
                                            else if (tmpafps < 75.0f)
                                            {
                                                MaxIdealJumps = 8;
                                            }
                                            else if (tmpafps < 100.0f)
                                            {
                                                MaxIdealJumps = 5;
                                            }
                                            else if (tmpafps < 150.0f)
                                            {
                                                MaxIdealJumps = 4;
                                            }
                                        }

                                        if (nf.RParms.Frametime > EPSILON)
                                        {
                                            averagefps2.Add(1000.0f / (1000.0f * nf.RParms.Frametime));
                                        }

                                        CurrentFps2Second = CurrentFps2;
                                        CurrentFps2 = 0;
                                    }
                                    else
                                    {
                                        CurrentFps2++;
                                    }
                                }

                                if (!RealAlive || HideWeapon || CurrentFrameDuplicated > 1 ||
                                    CurrentWeapon == WeaponIdType.WEAPON_NONE || CurrentWeapon == WeaponIdType.WEAPON_BAD ||
                                    CurrentWeapon == WeaponIdType.WEAPON_BAD2 || CurrentWeapon == WeaponIdType.WEAPON_C4 ||
                                    CurrentWeapon == WeaponIdType.WEAPON_HEGRENADE ||
                                    CurrentWeapon == WeaponIdType.WEAPON_SMOKEGRENADE ||
                                    CurrentWeapon == WeaponIdType.WEAPON_FLASHBANG || IsPlayerTeleport())
                                {
                                    AimType8Warn = 0;
                                    AimType8WarnTime = 0.0f;
                                    AimType8WarnTime2 = 0.0f;
                                }
                                else
                                {
                                    if (CurrentFrameAttacked || PreviousFrameAttacked ||
                                        (CurrentWeapon == WeaponIdType.WEAPON_KNIFE &&
                                         (CurrentFrameAttacked2 || PreviousFrameAttacked2)))
                                    {
                                        if (IsForceCenterView())
                                        {
                                            if (abs(CurrentTime - LastForceCenterView) > 10.0f)
                                            {
                                                DemoScanner_AddInfo("[FORCE_CENTERVIEW 2] at (" + CurrentTime + "):" +
                                                                    CurrentTimeString);
                                                LastForceCenterView = CurrentTime;
                                            }

                                            AimType8WarnTime = 0.0f;
                                            AimType8WarnTime2 = 0.0f;
                                            AimType8False = false;
                                        }

                                        if (abs(AimType8WarnTime) > EPSILON && abs(CurrentTime - AimType8WarnTime) < 0.350f)
                                        {
                                            if (DemoScanner_AddWarn(
                                                "[AIM TYPE 8.1 " + CurrentWeapon + "] at (" + AimType8WarnTime + "):" +
                                                GetTimeString(AimType8WarnTime), !AimType8False && !IsCmdChangeWeapon() && !IsPlayerInDuck() && !IsPlayerUnDuck()))
                                            {
                                                if (!AimType8False && !IsCmdChangeWeapon())
                                                {
                                                    TotalAimBotDetected++;
                                                }
                                            }
                                            AimType8WarnTime = 0.0f;
                                            AimType8False = false;
                                        }
                                        else if (abs(AimType8WarnTime2) > EPSILON &&
                                                 abs(CurrentTime - AimType8WarnTime2) < 0.350f)
                                        {
                                            if (DemoScanner_AddWarn(
                                                "[AIM TYPE 8.2 " + CurrentWeapon + "] at (" + AimType8WarnTime2 + "):" +
                                                GetTimeString(AimType8WarnTime2), /*CurrentWeapon != WeaponIdType.WEAPON_AWP
                                    && CurrentWeapon != WeaponIdType.WEAPON_SCOUT &&*/
                                                !AimType8False && !IsCmdChangeWeapon()))
                                            {
                                                if (!AimType8False && !IsCmdChangeWeapon())
                                                {
                                                    TotalAimBotDetected++;
                                                }
                                            }
                                            AimType8WarnTime2 = 0.0f;
                                            AimType8False = false;
                                        }
                                    }

                                    if (RealAlive && (CurrentFrameAttacked || PreviousFrameAttacked) &&
                                        CurrentFrameOnGround && abs(CurrentTime - LastDeathTime) > 2.0f &&
                                        abs(CurrentTime - LastAliveTime) > 2.0f && !IsForceCenterView() &&
                                        !IsAngleEditByEngine() && !IsPlayerInDuck() && !IsPlayerUnDuck())
                                    {
                                        if (AngleBetween(CDFRAME_ViewAngles.X, nf.RParms.Viewangles.X) > EPSILON &&
                                            AngleBetween(PREV_CDFRAME_ViewAngles.X, nf.RParms.Viewangles.X) > EPSILON)
                                        {
                                            var spreadtest = AngleBetween(CDFRAME_ViewAngles.X,
                                                nf.RParms.Viewangles.X - nf.RParms.Punchangle.X);
                                            var spreadtest2 = AngleBetween(PREV_CDFRAME_ViewAngles.X,
                                                nf.RParms.Viewangles.X - nf.RParms.Punchangle.X);
                                            if (spreadtest > nospreadtest && spreadtest2 > nospreadtest)
                                            {
                                                nospreadtest = spreadtest > spreadtest2 ? spreadtest : spreadtest2;
                                            }
                                            // Console.WriteLine(nospreadtest.ToString("F8"));
                                            if (abs(NoSpreadDetectionTime - CurrentTime) > 0.01f &&
                                                spreadtest > MAX_SPREAD_CONST && spreadtest2 > MAX_SPREAD_CONST)
                                            {
                                                if (ThirdHackDetected <= 0)
                                                {
                                                    NoSpreadDetectionTime = CurrentTime;
                                                    DemoScanner_AddWarn(
                                                        "[NO SPREAD TYPE 1 " + CurrentWeapon + "] at (" + CurrentTime +
                                                        "):" + CurrentTimeString, false);
                                                }
                                            }
                                        }

                                        if (AngleBetween(CDFRAME_ViewAngles.Y, nf.RParms.Viewangles.Y) > EPSILON &&
                                            AngleBetween(PREV_CDFRAME_ViewAngles.Y, nf.RParms.Viewangles.Y) > EPSILON)
                                        {
                                            var spreadtest = AngleBetween(CDFRAME_ViewAngles.Y,
                                                nf.RParms.Viewangles.Y - nf.RParms.Punchangle.Y);
                                            var spreadtest2 = AngleBetween(PREV_CDFRAME_ViewAngles.Y,
                                                nf.RParms.Viewangles.Y - nf.RParms.Punchangle.Y);
                                            if (spreadtest > nospreadtest2 && spreadtest2 > nospreadtest2)
                                            {
                                                nospreadtest2 = spreadtest > spreadtest2 ? spreadtest : spreadtest2;
                                            }
                                            // Console.WriteLine(nospreadtest.ToString("F8"));
                                            if (abs(NoSpreadDetectionTime - CurrentTime) > 0.1f &&
                                                spreadtest > MAX_SPREAD_CONST2 && spreadtest2 > MAX_SPREAD_CONST2)
                                            {
                                                if (ThirdHackDetected <= 0)
                                                {
                                                    NoSpreadDetectionTime = CurrentTime;
                                                    DemoScanner_AddWarn(
                                                        "[NO SPREAD TYPE 2 " + CurrentWeapon + "] at (" + CurrentTime +
                                                        "):" + CurrentTimeString, false);
                                                }
                                            }
                                        }

                                        FPoint3D tmpClAngles = new FPoint3D();
                                        VectorsToAngles(nf.RParms.Forward, nf.RParms.Right, nf.RParms.Up, ref tmpClAngles);

                                        var normCD_Angles1 = CDFRAME_ViewAngles;
                                        if (normCD_Angles1.Y > 180.0f)
                                        {
                                            normCD_Angles1.Y -= 360.0f;
                                        }

                                        if (normCD_Angles1.Y < -180.0f)
                                        {
                                            normCD_Angles1.Y += 360.0f;
                                        }

                                        var normCD_Angles2 = PREV_CDFRAME_ViewAngles;
                                        if (normCD_Angles2.Y > 180.0f)
                                        {
                                            normCD_Angles2.Y -= 360.0f;
                                        }

                                        if (normCD_Angles2.Y < -180.0f)
                                        {
                                            normCD_Angles2.Y += 360.0f;
                                        }

                                        var spreadtest_1 = AngleBetween(normCD_Angles1.X, tmpClAngles.X);
                                        var spreadtest_2 = AngleBetween(normCD_Angles1.Y, tmpClAngles.Y);
                                        var spreadtest_1_2 = AngleBetween(normCD_Angles2.X, tmpClAngles.X);
                                        var spreadtest_2_2 = AngleBetween(normCD_Angles2.Y, tmpClAngles.Y);
                                        var spreadtest_3 = AngleBetween(normCD_Angles1.Z, tmpClAngles.Z);
                                        var spreadtest_3_2 = AngleBetween(normCD_Angles2.Z, tmpClAngles.Z);

                                        if ((spreadtest_3 > MAX_SPREAD_CONST && spreadtest_3_2 > MAX_SPREAD_CONST) ||
                                            (spreadtest_1 > MAX_SPREAD_CONST && spreadtest_1_2 > MAX_SPREAD_CONST) ||
                                                (spreadtest_2 > MAX_SPREAD_CONST && spreadtest_2_2 > MAX_SPREAD_CONST))
                                        {
                                            if (spreadtest_3 < 0.001f && spreadtest_3 < 0.001f)
                                            {
                                                if (abs(NoSpreadDetectionTime - CurrentTime) > 0.01f)
                                                {
                                                    if (ThirdHackDetected <= 0)
                                                    {
                                                        NoSpreadDetectionTime = CurrentTime;
                                                        DemoScanner_AddWarn(
                                                            "[NO SPREAD TYPE 3 " + CurrentWeapon + "] at (" + CurrentTime +
                                                            "):" + CurrentTimeString/* + "[" +
                                                            spreadtest_1 + " " +
                                                            spreadtest_1_2 + " " +
                                                            spreadtest_2 + " " +
                                                            spreadtest_2_2 + " " +
                                                            spreadtest_3 + " " +
                                                            spreadtest_3_2 + " " + "]"*/, false);
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    //{ 
                                    //    //if (CurrentFrameAttacked)
                                    //    //    ;// Console.WriteLine("ATTACKED");
                                    //    //else if (CurrentFrameDuck)
                                    //    //    ;// Console.WriteLine("DUCKED");
                                    //    //else if (CurrentFrameJumped)
                                    //    //    ;// Console.WriteLine("JUMPED");
                                    //    //else if (PreviousFrameAttacked)
                                    //    //    ;// Console.WriteLine("PREVATT");
                                    //    //else if (!CurrentFrameOnGround)
                                    //    //    ;// Console.WriteLine("NOGROUND");
                                    //    //else if (CurrentTime - IsAttackLastTime < 3.0f || IsInAttack())
                                    //    //    ;// Console.WriteLine("LASTATTACK");
                                    //    //else
                                    //    //{
                                    //    //    Console.WriteLine("NONE(" + CurrentFrameButtons + ")");
                                    //    //}
                                    //}
                                    if (abs(nf.RParms.ClViewangles.Y - PREV_CDFRAME_ViewAngles.Y) > EPSILON ||
                                        abs(nf.UCmd.Viewangles.Y - PREV_CDFRAME_ViewAngles.Y) > EPSILON)
                                    {
                                        if (CDFRAME_ViewAngles != nf.RParms.ClViewangles)
                                        {
                                            if (AimType8Warn > 5)
                                            {
                                                AimType8Warn = 0;
                                            }

                                            AimType8Warn++;
                                            if (AimType8Warn == 1 || AimType8Warn == 2)
                                            {
                                                if (abs(bAimType8WarnTime - CurrentTime) > EPSILON)
                                                {
                                                    AimType8WarnTime = CurrentTime;
                                                }

                                                bAimType8WarnTime = CurrentTime;
                                                //Console.WriteLine("LastClientDataTime:" + LastClientDataTime + ". AimType8WarnTime:" + CurrentTime
                                                //     + ". LastOverflow:" + FpsOverflowTime);
                                                if (!AimType8False)
                                                {
                                                    AimType8False = CurrentWeapon == WeaponIdType.WEAPON_C4 ||
                                                                    CurrentWeapon == WeaponIdType.WEAPON_HEGRENADE ||
                                                                    CurrentWeapon == WeaponIdType.WEAPON_SMOKEGRENADE ||
                                                                    CurrentWeapon == WeaponIdType.WEAPON_FLASHBANG ||
                                                                    !CurrentFrameOnGround || IsAngleEditByEngine() ||
                                                                    IsPlayerLossConnection() || IsCmdChangeWeapon();
                                                }
                                            }
                                        }
                                        else
                                        {
                                            AimType8Warn = 0;
                                        }

                                        if (CDFRAME_ViewAngles != nf.UCmd.Viewangles)
                                        {
                                            if (AimType8Warn > 5)
                                            {
                                                AimType8Warn = 0;
                                            }

                                            AimType8Warn++;
                                            if (AimType8Warn == 1 || AimType8Warn == 2)
                                            {
                                                if (abs(bAimType8WarnTime2 - CurrentTime) > EPSILON)
                                                {
                                                    AimType8WarnTime2 = CurrentTime;
                                                }

                                                bAimType8WarnTime2 = CurrentTime;
                                                if (!AimType8False)
                                                {
                                                    AimType8False = CurrentWeapon == WeaponIdType.WEAPON_C4 ||
                                                                    CurrentWeapon == WeaponIdType.WEAPON_HEGRENADE ||
                                                                    CurrentWeapon == WeaponIdType.WEAPON_SMOKEGRENADE ||
                                                                    CurrentWeapon == WeaponIdType.WEAPON_FLASHBANG ||
                                                                    !CurrentFrameOnGround || IsAngleEditByEngine() ||
                                                                    IsPlayerLossConnection() || IsCmdChangeWeapon();
                                                }
                                                //AimType8Warn = -1;
                                            }
                                        }
                                        else
                                        {
                                            AimType8Warn = 0;
                                        }
                                    }
                                    else
                                    {
                                        if (CDFRAME_ViewAngles != nf.UCmd.Viewangles &&
                                            CDFRAME_ViewAngles != nf.RParms.ClViewangles)
                                        {
                                            if (abs(CurrentTime - FpsOverflowTime) < 1.0f)
                                            {
                                                FPS_OVERFLOW++;
                                                if (FPS_OVERFLOW > 5)
                                                {
                                                    FoundFpsHack1 = true;
                                                }

                                                if (FPS_OVERFLOW == 20)
                                                {
                                                    DemoScanner_AddWarn("[FPS HACK TYPE 1] at (" + CurrentTime + "):" +
                                                                        CurrentTimeString);
                                                }
                                            }
                                            else
                                            {
                                                FPS_OVERFLOW = 0;
                                            }

                                            FpsOverflowTime = CurrentTime;
                                        }
                                    }
                                }

                                if (abs(CurrentTime) > EPSILON && abs(PreviousTime) > EPSILON &&
                                    abs(CurrentTime2) > EPSILON && abs(PreviousTime2) > EPSILON)
                                {
                                    if (abs(MaximumTimeBetweenFrames) < EPSILON)
                                    {
                                        MaximumTimeBetweenFrames = 0.01f;
                                    }
                                    else
                                    {
                                        if (CurrentFrameDuplicated == 0 && abs(CurrentTime - PreviousTime) > 0.5f &&
                                            CurrentFrameId > 10)
                                        {
                                            TimeShiftCount += 1;
                                            if (LastTimeOut != 1 && TimeShiftCount - LossPackets > 4 + ChokePackets &&
                                                abs(CurrentTime - LastChokePacket) > 60)
                                            {
                                                DemoScanner_AddWarn(
                                                    "[TIMESHIFT] at (" + DemoStartTime + "):" + "(" + CurrentTime + "):" +
                                                    "(" + DemoStartTime2 + "):" + "(" + CurrentTime2 + "):" +
                                                    CurrentTimeString, false);
                                                LastTimeOut = 1;
                                            }
                                        }

                                        if (CurrentTime - PreviousTime > MaximumTimeBetweenFrames)
                                        {
                                            MaximumTimeBetweenFrames = abs(CurrentTime - PreviousTime);
                                        }
                                    }
                                }

                                if (abs(LastTimeDesync) < EPSILON)
                                {
                                    LastTimeDesync = abs(CurrentTime - CurrentTime2);
                                }
                                else
                                {
                                    if (abs(LastTimeDesync - abs(CurrentTime - CurrentTime2)) > 0.08f ||
                                        PreviousTime - CurrentTime > EPSILON || PreviousTime2 - CurrentTime2 > EPSILON)
                                    {
                                        if (RealAlive)
                                        {
                                            //Console.WriteLine("LastTimeDesync1:" + abs(LastTimeDesync - abs(CurrentTime - CurrentTime2)).ToString());
                                            if (SecondFound && abs(CurrentTime) > EPSILON && abs(CurrentTime2) > EPSILON)
                                            {
                                                if (CurrentFrameDuplicated == 0)
                                                {
                                                    TimeShiftCount += 1;
                                                    if (LastTimeOut != 2 &&
                                                        TimeShiftCount - LossPackets > 4 + ChokePackets &&
                                                        abs(CurrentTime - LastChokePacket) > 60)
                                                    {
                                                        DemoScanner_AddWarn(
                                                            "[TIMESHIFT 2] at (" + DemoStartTime + "):" + "(" +
                                                            CurrentTime + "):" + "(" + DemoStartTime2 + "):" + "(" +
                                                            CurrentTime2 + "):" + CurrentTimeString, false);
                                                        LastTimeOut = 2;
                                                    }
                                                }
                                            }

                                            //Console.WriteLine("Second.");
                                            if (SecondFound2 && abs(CurrentTime) > EPSILON && abs(CurrentTime2) > EPSILON)
                                            {
                                                if (CurrentFrameDuplicated == 0)
                                                {
                                                    TimeShiftCount += 1;
                                                    if (LastTimeOut != 3 &&
                                                        TimeShiftCount - LossPackets > 4 + ChokePackets &&
                                                        abs(CurrentTime - LastChokePacket) > 60)
                                                    {
                                                        DemoScanner_AddWarn(
                                                            "[TIMESHIFT 3] at (" + DemoStartTime + "):" + "(" +
                                                            CurrentTime + "):" + "(" + DemoStartTime2 + "):" + "(" +
                                                            CurrentTime2 + "):" + CurrentTimeString, false);
                                                        LastTimeOut = 3;
                                                    }
                                                }
                                            }
                                            //Console.WriteLine("Second2.");
                                        }

                                        LastTimeDesync = abs(CurrentTime - CurrentTime2);
                                    }
                                    else
                                    {
                                        LastTimeDesync = abs(CurrentTime - CurrentTime2);
                                    }
                                }

                                //if (TimeShift4Times[0] == 0.0f ||
                                //    TimeShift4Times[1] == 0.0f ||
                                //    TimeShift4Times[2] == 0.0f )
                                //{
                                //    TimeShift4Times[0] = CurrentTime;
                                //    TimeShift4Times[1] = CurrentTime2;
                                //    TimeShift4Times[2] = CurrentTime3;
                                //}
                                //else
                                //{
                                //    if (TimeShift4Times[0] > CurrentTime)
                                //    {
                                //        Console.WriteLine("timeshift 111");
                                //    }
                                //    else if (TimeShift4Times[1] > CurrentTime2)
                                //    {
                                //        Console.WriteLine("timeshift 222");
                                //    }
                                //    else if (TimeShift4Times[2] > CurrentTime3)
                                //    {
                                //        Console.WriteLine("timeshift 333");
                                //    }
                                //    TimeShift4Times[0] = CurrentTime;
                                //    TimeShift4Times[1] = CurrentTime2;
                                //    TimeShift4Times[2] = CurrentTime3;
                                //}
                                if (NeedDetectLerpAfterAttack)
                                {
                                    NeedDetectLerpAfterAttack = false;
                                    LerpAfterAttack = CurrentFrameLerp;
                                }

                                if (LerpSearchFramesCount > 0)
                                {
                                    LerpSearchFramesCount--;
                                    if (LerpSearchFramesCount == 1)
                                    {
                                        if (LerpBeforeStopAttack > LerpBeforeAttack)
                                        {
                                            if (LerpAfterAttack > LerpBeforeAttack)
                                            {
                                                if (CurrentFrameLerp == LerpBeforeAttack && RealAlive && FirstAttack)
                                                {
                                                    if (DemoScanner_AddWarn(
                                                        "[AIM TYPE 4.1 " + CurrentWeapon + "] at (" + CurrentTime + "):" +
                                                        CurrentTimeString,
                                                        !IsPlayerLossConnection() && !IsCmdChangeWeapon()))
                                                    {
                                                        TotalAimBotDetected++;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                if (NeedSearchViewAnglesAfterAttack == 1)
                                {
                                    ViewanglesXBeforeAttack = CDFRAME_ViewAngles.X;
                                    ViewanglesYBeforeAttack = CDFRAME_ViewAngles.Y;
                                    //Console.WriteLine("Aim3 0:" + ViewanglesXBeforeBeforeAttack + ":" + ViewanglesYBeforeBeforeAttack);
                                    //Console.WriteLine("Aim3 1:" + ViewanglesXBeforeAttack + ":" + ViewanglesYBeforeAttack);
                                    NeedSearchViewAnglesAfterAttack++;
                                }
                                else if (NeedSearchViewAnglesAfterAttack == 2 && IsInAttack() && RealAlive)
                                {
                                    if (CurrentWeapon == WeaponIdType.WEAPON_C4 ||
                                        CurrentWeapon == WeaponIdType.WEAPON_HEGRENADE ||
                                        CurrentWeapon == WeaponIdType.WEAPON_SMOKEGRENADE ||
                                        CurrentWeapon == WeaponIdType.WEAPON_FLASHBANG ||
                                        CurrentWeapon == WeaponIdType.WEAPON_NONE ||
                                        CurrentWeapon == WeaponIdType.WEAPON_BAD ||
                                        CurrentWeapon == WeaponIdType.WEAPON_BAD2)
                                    {
                                    }
                                    else if (CurrentFrameAttacked)
                                    {
                                        NeedSearchViewAnglesAfterAttack = 0;
                                        ViewanglesXAfterAttack = CDFRAME_ViewAngles.X;
                                        ViewanglesYAfterAttack = CDFRAME_ViewAngles.Y;
                                        NeedSearchViewAnglesAfterAttackNext = true;
                                    }
                                }
                                else
                                {
                                    NeedSearchViewAnglesAfterAttack = 0;
                                    if (NeedSearchViewAnglesAfterAttackNext && RealAlive && IsInAttack())
                                    {
                                        NeedSearchViewAnglesAfterAttackNext = false;
                                        ViewanglesXAfterAttackNext = CDFRAME_ViewAngles.X;
                                        ViewanglesYAfterAttackNext = CDFRAME_ViewAngles.Y;
                                        if (abs(ViewanglesXBeforeBeforeAttack - ViewanglesXAfterAttack) < EPSILON &&
                                            abs(ViewanglesYBeforeBeforeAttack - ViewanglesYAfterAttack) < EPSILON &&
                                            abs(ViewanglesXBeforeBeforeAttack - ViewanglesXAfterAttackNext) < EPSILON &&
                                            abs(ViewanglesYBeforeBeforeAttack - ViewanglesYAfterAttackNext) < EPSILON &&
                                            abs(ViewanglesXBeforeBeforeAttack - ViewanglesXBeforeAttack) > EPSILON &&
                                            abs(ViewanglesYBeforeBeforeAttack - ViewanglesYBeforeAttack) > EPSILON)
                                        {
                                            //DemoScanner_AddWarn(
                                            //      "Warn [AIM TYPE 3] at (" +
                                            //      CurrentTime + "):" + CurrentTimeString + " (???)", false);
                                            //if (maxfalsepositiveaim3 > 0 &&
                                            //    SilentAimDetected <= 1 &&
                                            //    KreedzHacksCount <= 1)
                                            //{
                                            //    DemoScanner_AddWarn(
                                            //        "Detected [AIM TYPE 3] at (" +
                                            //        CurrentTime + "):" + CurrentTimeString + " (???)", false);
                                            //    maxfalsepositiveaim3--;
                                            //}
                                            //else if (!IsTeleportus())
                                            //{
                                            //    DemoScanner_AddWarn(
                                            //        "Detected [AIM TYPE 3] at (" +
                                            //        CurrentTime + "):" + CurrentTimeString);
                                            //    SilentAimDetected++;
                                            //}
                                            //else
                                            //{
                                            //    DemoScanner_AddWarn(
                                            //        "Detected [AIM TYPE 3] at (" +
                                            //        CurrentTime + "):" + CurrentTimeString + " (???)", false);
                                            //}
                                        }
                                        else if (abs(ViewanglesXBeforeAttack - ViewanglesXAfterAttack) > EPSILON &&
                                                 abs(ViewanglesYBeforeAttack - ViewanglesYAfterAttack) >
                                                 EPSILON && // ViewanglesXBeforeAttack == ViewanglesXAfterAttackNext &&
                                                 abs(ViewanglesYBeforeAttack - ViewanglesYAfterAttackNext) < EPSILON)
                                        {
                                            //var tmpcol = Console.ForegroundColor;
                                            //Console.ForegroundColor = ConsoleColor.Gray;
                                            //TextComments.Add("Detected [AIM TYPE 3] at (" + CurrentTime + "):" + CurrentTimeString + " (???)");
                                            //AddViewDemoHelperComment("Detected [AIM TYPE 3]. Weapon:" + CurrentWeapon.ToString() + " (???)", 0.5f);
                                            //Console.WriteLine("Detected [AIM TYPE 3] at (" + CurrentTime + "):" + CurrentTimeString + " (???)");
                                            //Console.ForegroundColor = tmpcol;
                                        }
                                    }
                                    else
                                    {
                                        NeedSearchViewAnglesAfterAttackNext = false;
                                    }
                                }

                                if (abs(CurrentTime) > EPSILON && RealAlive)
                                {
                                    if ((IsJump && FirstJump) || CurrentTime - LastUnJumpTime < 0.5f)
                                    {
                                        JumpHackCount2 = 0;
                                    }

                                    if (!CurrentFrameJumped && !FirstJump)
                                    {
                                        FirstJump = true;
                                        IsJump = false;
                                    }
                                    else
                                    {
                                        if (FirstJump && !IsJump)
                                        {
                                            if (JumpHackCount2 > 0)
                                            {
                                                JumpHackCount2 = 0;
                                                if (abs(LastJumpHackFalseDetectionTime) > EPSILON &&
                                                    abs(CurrentTime - LastJumpHackFalseDetectionTime) > 5.0f)
                                                {
                                                    LastJumpHackFalseDetectionTime = 0.0f;
                                                }

                                                if (abs(CurrentTime - LastUnJumpTime) > 1.5f &&
                                                    abs(CurrentTime - LastJumpTime) > 1.5f)
                                                {
                                                    if (abs(CurrentTime - LastKreedzHackTime) > 2.5f &&
                                                        abs(LastJumpHackFalseDetectionTime) < EPSILON)
                                                    {
                                                        if (DemoScanner_AddWarn(
                                                            "[JUMPHACK TYPE 1] at (" + CurrentTime + ") " +
                                                            CurrentTimeString, !IsAngleEditByEngine()))
                                                        {
                                                            LastKreedzHackTime = CurrentTime;
                                                            KreedzHacksCount++;
                                                        }
                                                    }
                                                }
                                                else if (abs(CurrentTime - LastUnJumpTime) > 0.5f &&
                                                         abs(CurrentTime - LastJumpTime) > 0.5f)
                                                {
                                                    if (abs(CurrentTime - LastKreedzHackTime) > 2.5f &&
                                                        !IsAngleEditByEngine() &&
                                                        abs(LastJumpHackFalseDetectionTime) < EPSILON)
                                                    {
                                                        if (DemoScanner_AddWarn(
                                                            "[JUMPHACK TYPE 3] at (" + CurrentTime + ") " +
                                                            CurrentTimeString, false))
                                                        {
                                                            LastKreedzHackTime = CurrentTime;
                                                            KreedzHacksCount++;
                                                        }
                                                    }
                                                }
                                                else if (abs(CurrentTime - LastUnJumpTime) > 0.3f &&
                                                         abs(CurrentTime - LastJumpTime) > 0.3f)
                                                {
                                                    if (abs(CurrentTime - LastKreedzHackTime) > 2.5f &&
                                                        !IsAngleEditByEngine() &&
                                                        abs(LastJumpHackFalseDetectionTime) < EPSILON)
                                                    {
                                                        if (DemoScanner_AddWarn(
                                                            "[JUMPHACK TYPE 4] at (" + CurrentTime + ") " +
                                                            CurrentTimeString, false, false))
                                                        {
                                                            LastKreedzHackTime = CurrentTime;
                                                            KreedzHacksCount++;
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        if (FirstJump && !IsJump && CurrentFrameJumped)
                                        {
                                            if (abs(CurrentTime - LastUnJumpTime) > 0.5f &&
                                                abs(CurrentTime - LastJumpTime) > 0.5f)
                                            {
                                                JumpHackCount2++;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    JumpHackCount2 = 0;
                                }

                                if (KnownSkyName == string.Empty)
                                {
                                    KnownSkyName = nf.MVars.SkyName;
                                }
                                else if (KnownSkyName != nf.MVars.SkyName)
                                {
                                    if (IsRussia)
                                    {
                                        DemoScanner_AddInfo("Сменилось небо с \"" + KnownSkyName + "\" на \"" +
                                                            nf.MVars.SkyName + "\" (" + CurrentTime + "):" +
                                                            CurrentTimeString);
                                    }
                                    else
                                    {
                                        DemoScanner_AddInfo("Player changed sky name from \"" + KnownSkyName + "\" to \"" +
                                                            nf.MVars.SkyName + "\" at (" + CurrentTime + "):" +
                                                            CurrentTimeString);
                                    }

                                    KnownSkyName = nf.MVars.SkyName;
                                }

                                var toRemovePunch0 = new List<int>();
                                for (var i = 0; i < Punch0_Search_Time.Count; i++)
                                {
                                    if (toRemovePunch0.Contains(i))
                                    {
                                        continue;
                                    }

                                    if (abs(Punch0_Search_Time[i]) > EPSILON)
                                    {
                                        if (abs(Punch0_Search_Time[i] - CurrentTime) > 0.5f)
                                        {
                                            if (abs(NoSpreadDetectionTime - CurrentTime) > 1.00f)
                                            {
                                                NoSpreadDetectionTime = CurrentTime;

                                                if (!IsTakeDamage())
                                                {
                                                    if (!IsPlayerLossConnection())
                                                    {
                                                        DemoScanner_AddWarn(
                                                            "[BETA] [NO SPREAD TYPE 4.1 " + CurrentWeapon + "] at (" +
                                                            Punch0_Search_Time[i] /*+ " + debug=" + Punch0_Search[i] */ + "):" +
                                                            CurrentTimeString, false);
                                                    }
                                                    else
                                                    {
                                                        DemoScanner_AddWarn(
                                                            "[BETA] [NO SPREAD TYPE 4.2 " + CurrentWeapon + "] at (" +
                                                            Punch0_Search_Time[i] /*+ " + debug=" + Punch0_Search[i] */ + "):" +
                                                            CurrentTimeString, false);
                                                    }
                                                }
                                            }

                                            Punch0_Search_Time[i] = 0.0f;
                                        }
                                        else if (abs(nf.RParms.Punchangle.X - Punch0_Search[i]) <= MAX_SPREAD_CONST)
                                        {
                                            Punch0_Valid_Time = CurrentTime;
                                            Punch0_Search_Time[i] = 0.0f;
                                        }
                                        else if (CurrentFrameDuplicated > 0 &&
                                                 abs(Punch0_Search_Time[i] - CurrentTime) < EPSILON_2)
                                        {
                                            Punch0_Search_Time[i] = 0.0f;
                                        }
                                    }
                                    else
                                    {
                                        toRemovePunch0.Add(i);
                                    }
                                }

                                toRemovePunch0.Reverse();
                                foreach (var del in toRemovePunch0)
                                {
                                    Punch0_Search_Time.RemoveAt(del);
                                    Punch0_Search.RemoveAt(del);
                                }

                                if (DUMP_ALL_FRAMES)
                                {
                                    subnode.Text += "{\n";
                                    subnode.Text += "RParms.Time  = " + nf.RParms.Time + "(" + CurrentTimeString + ")\n";
                                    subnode.Text += "RParms.Vieworg.X  = " + nf.RParms.Vieworg.X + "\n";
                                    subnode.Text += "RParms.Vieworg.Y  = " + nf.RParms.Vieworg.Y + "\n";
                                    subnode.Text += "RParms.Vieworg.Z  = " + nf.RParms.Vieworg.Z + "\n";
                                    subnode.Text += "RParms.Viewangles.X  = " + nf.RParms.Viewangles.X + "\n";
                                    subnode.Text += "RParms.Viewangles.Y  = " + nf.RParms.Viewangles.Y + "\n";
                                    //subnode.Text += "(delta[1]:" + give_bad_float(nf.RParms.Viewangles.Y) + ")" + "\n";
                                    subnode.Text += "RParms.Viewangles.Z  = " + nf.RParms.Viewangles.Z + "\n";
                                    subnode.Text += "RParms.Forward.X  = " + nf.RParms.Forward.X + "\n";
                                    subnode.Text += "RParms.Forward.Y  = " + nf.RParms.Forward.Y + "\n";
                                    subnode.Text += "RParms.Forward.Z  = " + nf.RParms.Forward.Z + "\n";
                                    subnode.Text += "RParms.Right.X  = " + nf.RParms.Right.X + "\n";
                                    subnode.Text += "RParms.Right.Y  = " + nf.RParms.Right.Y + "\n";
                                    subnode.Text += "RParms.Right.Z  = " + nf.RParms.Right.Z + "\n";
                                    subnode.Text += "RParms.Up.X  = " + nf.RParms.Up.X + "\n";
                                    subnode.Text += "RParms.Up.Y  = " + nf.RParms.Up.Y + "\n";
                                    subnode.Text += "RParms.Up.Z  = " + nf.RParms.Up.Z + "\n";
                                    subnode.Text += "RParms.Frametime  = " + nf.RParms.Frametime + "\n";
                                    subnode.Text += "RParms.Intermission  = " + nf.RParms.Intermission + "\n";
                                    subnode.Text += "RParms.Paused  = " + nf.RParms.Paused + "\n";
                                    subnode.Text += "RParms.Spectator  = " + nf.RParms.Spectator + "\n";
                                    subnode.Text += "RParms.Onground  = " + nf.RParms.Onground + "\n";
                                }

                                if (nf.RParms.Intermission != 0)
                                {
                                    if (CL_Intermission == 0)
                                    {
                                        if (!NewDirectory)
                                        {
                                            if (IsRussia)
                                            {
                                                Console.WriteLine("---------- Подготовка к смене уровня [" + CurrentTimeString +
                                                             "] ----------");
                                            }
                                            else
                                            {
                                                Console.WriteLine("---------- Preparing to changelevel [" + CurrentTimeString +
                                                             "] ----------");
                                            }

                                        }
                                    }
                                    CL_Intermission = 1;
                                }
                                else
                                {
                                    CL_Intermission = 0;
                                }

                                if (DUMP_ALL_FRAMES)
                                {
                                    subnode.Text += "RParms.Waterlevel  = " + nf.RParms.Waterlevel + "\n";
                                    subnode.Text += "RParms.Simvel.X  = " + nf.RParms.Simvel.X + "\n";
                                    subnode.Text += "RParms.Simvel.Y  = " + nf.RParms.Simvel.Y + "\n";
                                    subnode.Text += "RParms.Simvel.Z  = " + nf.RParms.Simvel.Z + "\n";
                                    subnode.Text += "RParms.Simorg.X  = " + nf.RParms.Simorg.X + "\n";
                                    subnode.Text += "RParms.Simorg.Y  = " + nf.RParms.Simorg.Y + "\n";
                                    subnode.Text += "RParms.Simorg.Z  = " + nf.RParms.Simorg.Z + "\n";
                                    subnode.Text += "RParms.Viewheight.X  = " + nf.RParms.Viewheight.X + "\n";
                                    subnode.Text += "RParms.Viewheight.Y  = " + nf.RParms.Viewheight.Y + "\n";
                                    subnode.Text += "RParms.Viewheight.Z  = " + nf.RParms.Viewheight.Z + "\n";
                                    subnode.Text += "RParms.Idealpitch  = " + nf.RParms.Idealpitch + "\n";
                                    subnode.Text += "RParms.ClViewangles.X  = " + nf.RParms.ClViewangles.X + "\n";
                                    subnode.Text += "RParms.ClViewangles.Y  = " + nf.RParms.ClViewangles.Y + "\n";
                                    subnode.Text += "RParms.ClViewangles.Z  = " + nf.RParms.ClViewangles.Z + "\n";
                                    subnode.Text += "RParms.Health  = " + nf.RParms.Health + "\n";
                                    subnode.Text += "RParms.Crosshairangle.X  = " + nf.RParms.Crosshairangle.X + "\n";
                                    subnode.Text += "RParms.Crosshairangle.Y  = " + nf.RParms.Crosshairangle.Y + "\n";
                                    subnode.Text += "RParms.Crosshairangle.Z  = " + nf.RParms.Crosshairangle.Z + "\n";
                                    subnode.Text += "RParms.Viewsize  = " + nf.RParms.Viewsize + "\n";
                                    subnode.Text += "RParms.Punchangle.X  = " + nf.RParms.Punchangle.X + "\n";
                                    subnode.Text += "RParms.Punchangle.Y  = " + nf.RParms.Punchangle.Y + "\n";
                                    subnode.Text += "RParms.Punchangle.Z  = " + nf.RParms.Punchangle.Z + "\n";
                                    subnode.Text += "RParms.Maxclients  = " + nf.RParms.Maxclients + "\n";
                                    subnode.Text += "RParms.Viewentity  = " + nf.RParms.Viewentity + "\n";
                                    subnode.Text += "RParms.Playernum  = " + nf.RParms.Playernum + "\n";
                                }

                                ViewModel = nf.Viewmodel;

                                if (!UserAlive && ViewEntity == nf.RParms.Viewentity)
                                {
                                    if (CL_Intermission == 0 && FirstUserAlive && nf.Viewmodel != 0)
                                    {
                                        NeedSearchUserAliveTime = CurrentTime;
                                    }

                                    CL_Intermission = 0;
                                    ViewEntity = -1;
                                }

                                AddResolution(nf.RParms.Viewport.Z, nf.RParms.Viewport.W);
                                if (DUMP_ALL_FRAMES)
                                {
                                    subnode.Text += "RParms.MaxEntities  = " + nf.RParms.MaxEntities + "\n";
                                    subnode.Text += "RParms.Demoplayback  = " + nf.RParms.Demoplayback + "\n";
                                    subnode.Text += "RParms.Hardware  = " + nf.RParms.Hardware + "\n";
                                    subnode.Text += "RParms.Smoothing  = " + nf.RParms.Smoothing + "\n";
                                    subnode.Text += "RParms.PtrCmd  = " + nf.RParms.PtrCmd + "\n";
                                    subnode.Text += "RParms.PtrMovevars  = " + nf.RParms.PtrMovevars + "\n";
                                    subnode.Text += "RParms.Viewport.X  = " + nf.RParms.Viewport.X + "\n";
                                    subnode.Text += "RParms.Viewport.Y  = " + nf.RParms.Viewport.Y + "\n";
                                    subnode.Text += "RParms.Viewport.Z  = " + nf.RParms.Viewport.Z + "\n";
                                    subnode.Text += "RParms.Viewport.W  = " + nf.RParms.Viewport.W + "\n";
                                    subnode.Text += "RParms.NextView  = " + nf.RParms.NextView + "\n";
                                    subnode.Text += "RParms.OnlyClientDraw  = " + nf.RParms.OnlyClientDraw + "\n";
                                    subnode.Text += "UCmd.LerpMsec  = " + nf.UCmd.LerpMsec + "\n";
                                    subnode.Text += "UCmd.Msec  = " + nf.UCmd.Msec + "\n";
                                    subnode.Text += "UCmd.Align1  = " + nf.UCmd.Align1 + "\n";
                                    subnode.Text += "UCmd.Forwardmove  = " + nf.UCmd.Forwardmove + "\n";
                                    subnode.Text += "UCmd.Sidemove  = " + nf.UCmd.Sidemove + "\n";
                                    subnode.Text += "UCmd.Upmove  = " + nf.UCmd.Upmove + "\n";
                                    subnode.Text += "UCmd.Lightlevel  = " + nf.UCmd.Lightlevel + "\n";
                                    subnode.Text += "UCmd.Align2  = " + nf.UCmd.Align2 + "\n";
                                    subnode.Text += "UCmd.Buttons  = " + nf.UCmd.Buttons + "\n";
                                }

                                if (nf.UCmd.Msec > 50)
                                {
                                    LastBadMsecTime = CurrentTime;
                                }

                                if (IsUserAlive() && FirstJump && abs(CurrentTime) > EPSILON)
                                {
                                    if (nf.UCmd.Msec == 0 && nf.RParms.Frametime > EPSILON_2 && CurrentFrameDuplicated == 0)
                                    {
                                        if (abs(CurrentTime - LastCmdHack) > 5.0)
                                        {
                                            if (RealFpsMax > 2000)
                                            {
                                                DemoScanner_AddWarn(
                                                    "[FPS HACK TYPE 3. FPS = " + RealFpsMax + " ] at (" + CurrentTime +
                                                    ") " + CurrentTimeString, !IsAngleEditByEngine());
                                            }

                                            if (RealFpsMax < 600)
                                            {
                                                DemoScanner_AddWarn(
                                                    "[CMD HACK TYPE 1] at (" + CurrentTime + ") " + CurrentTimeString,
                                                    !IsAngleEditByEngine());
                                            }

                                            LastCmdHack = CurrentTime;
                                        }

                                        StuckFrames++;
                                    }
                                    else if (nf.UCmd.Msec / nf.RParms.Frametime < 500.0f)
                                    {
                                        if (abs(CurrentTime - LastCmdHack) > 4.0)
                                        {
                                            if (FoundFpsHack1)
                                            {
                                                DemoScanner_AddWarn("[FPS HACK TYPE 2] at (" + CurrentTime + "):" +
                                                                    CurrentTimeString);
                                            }
                                            else
                                            {
                                                DemoScanner_AddWarn(
                                                    "[CMD HACK TYPE 2] at (" + CurrentTime + ") " + CurrentTimeString,
                                                    !IsAngleEditByEngine());
                                            }

                                            LastCmdHack = CurrentTime;
                                        }
                                        //Console.WriteLine("BAD BAD " + nf.UCmd.Msec + " / " + nf.RParms.Frametime + " = " + ((float)nf.UCmd.Msec / nf.RParms.Frametime).ToString());
                                    }
                                }

                                if (ShotFound > 0 && !IsReload && IsInAttack() && RealAlive && SelectSlot <= 0)
                                {
                                }
                                else
                                {
                                    ShotFound = 0;
                                }

                                if (ReallyAim2 > 0)
                                {
                                    if (FirstAttack)
                                    {
                                        if (CurrentWeapon == WeaponIdType.WEAPON_KNIFE ||
                                            CurrentWeapon == WeaponIdType.WEAPON_C4 ||
                                            CurrentWeapon == WeaponIdType.WEAPON_HEGRENADE ||
                                            CurrentWeapon == WeaponIdType.WEAPON_SMOKEGRENADE ||
                                            CurrentWeapon == WeaponIdType.WEAPON_FLASHBANG)
                                        {
                                        }
                                        else
                                        {
                                            if (ReallyAim2 == 2 && TotalAimBotDetected < 2 && TriggerAimAttackCount < 2)
                                            {
                                                DemoScanner_AddWarn(
                                                    "[AIM TYPE 2.1 " + CurrentWeapon + "] at (" + CurrentTime + "):" +
                                                    CurrentTimeString, false);
                                            }
                                            else
                                            {
                                                if (DemoScanner_AddWarn(
                                                    "[AIM TYPE 2.1 " + CurrentWeapon + "] at (" + CurrentTime + "):" +
                                                    CurrentTimeString,
                                                    !IsPlayerLossConnection() &&
                                                    !IsCmdChangeWeapon() /* && !IsForceCenterView() */ &&
                                                    !IsAngleEditByEngine()))
                                                {
                                                    if (!IsPlayerLossConnection() && !IsCmdChangeWeapon() &&
                                                        !IsAngleEditByEngine())
                                                    {
                                                        TotalAimBotDetected++;
                                                    }
                                                }
                                            }
                                            //FirstAttack = false;
                                        }
                                    }

                                    ReallyAim2 = 0;
                                }

                                if (!IsPlayerBtnAttackedPressed() && !CurrentFrameAttacked && !IsAttack2 &&
                                    CurrentWeapon == WeaponIdType.WEAPON_KNIFE && NeedIgnoreAttackFlag == 0)
                                {
                                    if (!KnifeTriggerAttackFound && CurrentFrameAttacked2 && FirstAttack)
                                    {
                                        if (SelectSlot > 0)
                                        {
                                            //  Console.WriteLine("Select weapon(" + CurrentTime + ") " + CurrentTimeString);
                                        }
                                        else if (attackscounter3 < 2)
                                        {
                                            //  Console.WriteLine("Select weapon(" + CurrentTime + ") " + CurrentTimeString);
                                        }
                                        else if (abs(CurrentTime2 - IsNoAttackLastTime2) < 0.1f)
                                        {
                                            //   Console.WriteLine("No attack in current frame(" + CurrentTime + ") " + CurrentTimeString);
                                        }
                                        else if (IsReload)
                                        {
                                            //  Console.WriteLine("Reload weapon(" + CurrentTime + ") " + CurrentTimeString);
                                        }
                                        else if (!RealAlive)
                                        {
                                            //  Console.WriteLine("Dead (" + CurrentTime + ") " + CurrentTimeString);
                                        }
                                        else
                                        {
                                            LastTriggerAttack = CurrentTime;
                                            //FirstAttack = false;
                                            KnifeTriggerAttackFound = true;
                                        }
                                    }
                                }

                                if (!IsPlayerBtnAttackedPressed() && !IsInAttack() && NeedIgnoreAttackFlag == 0)
                                {
                                    if (!TriggerAttackFound && CurrentFrameAttacked && FirstAttack)
                                    {
                                        if (CurrentWeapon == WeaponIdType.WEAPON_C4 ||
                                            CurrentWeapon == WeaponIdType.WEAPON_KNIFE ||
                                            CurrentWeapon == WeaponIdType.WEAPON_HEGRENADE ||
                                            CurrentWeapon == WeaponIdType.WEAPON_SMOKEGRENADE ||
                                            CurrentWeapon == WeaponIdType.WEAPON_FLASHBANG)
                                        {
                                            //  Console.WriteLine("Invalid weapon(" + CurrentTime + ") " + CurrentTimeString);
                                        }
                                        else if (SelectSlot > 0)
                                        {
                                            //  Console.WriteLine("Select weapon(" + CurrentTime + ") " + CurrentTimeString);
                                        }
                                        else if (attackscounter3 < 2)
                                        {
                                            //  Console.WriteLine("Select weapon(" + CurrentTime + ") " + CurrentTimeString);
                                        }
                                        else if (abs(CurrentTime2 - IsNoAttackLastTime2) < 0.1f)
                                        {
                                            //   Console.WriteLine("No attack in current frame(" + CurrentTime + ") " + CurrentTimeString);
                                        }
                                        else if (IsReload)
                                        {
                                            //  Console.WriteLine("Reload weapon(" + CurrentTime + ") " + CurrentTimeString);
                                        }
                                        else if (!RealAlive)
                                        {
                                            //  Console.WriteLine("Dead (" + CurrentTime + ") " + CurrentTimeString);
                                        }
                                        else
                                        {
                                            //FirstAttack = false;
                                            LastTriggerAttack = CurrentTime;
                                            TriggerAttackFound = true;
                                        }
                                    }
                                }

                                if (!CurrentFrameAttacked && !PreviousFrameAttacked && CurrentFrameDuplicated == 0)
                                {
                                    if (TriggerAttackFound && !IsPlayerBtnAttackedPressed())
                                    {
                                        if (DemoScanner_AddWarn(
                                            "[TRIGGER TYPE 1 " + CurrentWeapon + "] at (" + LastTriggerAttack + ") " +
                                            GetTimeString(LastTriggerAttack),
                                            !IsCmdChangeWeapon() && !IsAngleEditByEngine()))
                                        {
                                            TriggerAimAttackCount++;
                                        }
                                    }

                                    TriggerAttackFound = false;
                                }

                                if (!CurrentFrameAttacked2 && !PreviousFrameAttacked2 && CurrentFrameDuplicated == 0)
                                {
                                    if (KnifeTriggerAttackFound && !IsPlayerBtnAttackedPressed())
                                    {
                                        if (DemoScanner_AddWarn(
                                            "[KNIFEBOT TYPE 1 " + CurrentWeapon + "] at (" + LastTriggerAttack + ") " +
                                            GetTimeString(LastTriggerAttack),
                                            !IsCmdChangeWeapon() && !IsAngleEditByEngine()))
                                        {
                                            TriggerAimAttackCount++;
                                        }
                                    }

                                    KnifeTriggerAttackFound = false;
                                }

                                if (NeedSearchAim2 && !IsReload && RealAlive && SelectSlot <= 0)
                                {
                                    if (!Aim2AttackDetected)
                                    {
                                        if (CurrentFrameAttacked)
                                        {
                                            ShotFound++;
                                            Aim2AttackDetected = true;
                                        }
                                    }
                                    else
                                    {
                                        if (!CurrentFrameAttacked)
                                        {
                                            ShotFound++;
                                            Aim2AttackDetected = false;
                                        }
                                    }
                                }
                                else
                                {
                                    NeedSearchAim2 = false;
                                    Aim2AttackDetected = false;
                                }

                                if (NeedWriteAim > 0 && CurrentFrameAttacked)
                                {
                                    if (AimType1FalseDetect)
                                    {
                                        AimType1FalseDetect = false;
                                        if (FirstAttack)
                                        {
                                            if ((TotalAimBotDetected > 0 || KreedzHacksCount > 0) && !IsAngleEditByEngine())
                                            {
                                                if (DemoScanner_AddWarn(
                                                    "[AIM TYPE 1.2 " + CurrentWeapon + "] at (" + NeedWriteAimTime + "):" +
                                                    GetTimeString(NeedWriteAimTime),
                                                    NeedWriteAim == 2 && !IsCmdChangeWeapon() &&
                                                    !IsPlayerLossConnection() && !IsForceCenterView()))
                                                {
                                                    TotalAimBotDetected++;
                                                }
                                            }
                                            else
                                            {
                                                DemoScanner_AddWarn(
                                                    "[AIM TYPE 1.3 " + CurrentWeapon + "] at (" + NeedWriteAimTime + "):" +
                                                    GetTimeString(NeedWriteAimTime), false, false);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (FirstAttack)
                                        {
                                            if (DemoScanner_AddWarn(
                                                "[AIM TYPE 1.1 " + CurrentWeapon + "] at (" + NeedWriteAimTime + "):" +
                                                GetTimeString(NeedWriteAim),
                                                NeedWriteAim == 2 && !IsCmdChangeWeapon() && !IsPlayerLossConnection() &&
                                                !IsForceCenterView() && !IsAngleEditByEngine()))
                                            {
                                                if (!IsCmdChangeWeapon() && !IsPlayerLossConnection() && !IsForceCenterView() &&
                                                    !IsAngleEditByEngine())
                                                {
                                                    TotalAimBotDetected++;
                                                }
                                            }
                                        }
                                    }

                                    NeedWriteAim = 0;
                                    LastSilentAim = CurrentTime;
                                }

                                if (AttackCheck > -1)
                                {
                                    if (!CurrentFrameAttacked && !IsReload && SelectSlot <= 0 && IsInAttack() && RealAlive &&
                                        IsRealWeapon() && CurrentWeapon != WeaponIdType.WEAPON_KNIFE &&
                                        CurrentWeapon != WeaponIdType.WEAPON_C4 &&
                                        CurrentWeapon != WeaponIdType.WEAPON_HEGRENADE &&
                                        CurrentWeapon != WeaponIdType.WEAPON_SMOKEGRENADE &&
                                        CurrentWeapon != WeaponIdType.WEAPON_FLASHBANG)
                                    {
                                        if (DEBUG_ENABLED)
                                        {
                                            Console.WriteLine("Check attack:" + AttackCheck + " " + SkipNextAttack + " " +
                                                              CurrentFrameButtons + " " + CurrentTime);
                                        }

                                        if (AttackCheck > 0)
                                        {
                                            AttackCheck--;
                                        }
                                        else
                                        {
                                            if (SkipNextAttack <= 0)
                                            {
                                                //File.AppendAllText("bug.txt", "NeedWriteAim" + CurrentTime + " " + IsAttackLastTime + "\n");
                                                if (DEBUG_ENABLED)
                                                {
                                                    Console.WriteLine("Aim detected?... Teleport:" + IsAngleEditByEngine() +
                                                                      ". Alive:" + IsUserAlive());
                                                }

                                                if (CurrentFrameDuplicated == 0)
                                                {
                                                    NeedWriteAim = 2;
                                                    if (IsAngleEditByEngine())
                                                    {
                                                        NeedWriteAim = 1;
                                                    }

                                                    AttackCheck = -1;
                                                    NeedWriteAimTime = CurrentTime;
                                                }

                                                AttackCheck--;
                                            }

                                            if (CurrentFrameDuplicated == 0)
                                            {
                                                SkipNextAttack -= 1;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        NeedWriteAim = 0;
                                        AttackCheck = -1;
                                        if (DEBUG_ENABLED)
                                        {
                                            Console.WriteLine("Stop search aim type 1.");
                                        }
                                        //if (AttackCheck != -2)
                                        //{
                                        //    //if (SelectSlot > 0)
                                        //    //    Console.WriteLine("select slot...");
                                        //    //if (!IsInAttack())
                                        //    //    Console.WriteLine("not in attack...");
                                        //    //if (!UserAlive)
                                        //    //    Console.WriteLine("user invalid blya...");
                                        //    //if (IsReload)
                                        //    //    Console.WriteLine("in reload...");
                                        //    //if ((CurrentFrameButtons & 1) > 0)
                                        //    //    Console.WriteLine("not attack...");
                                        //    //Console.WriteLine("Weapon:" + CurrentWeapon.ToString());
                                        //    //Console.WriteLine("Don't check attack!");
                                        //    AttackCheck = -2;
                                        //}
                                    }
                                }

                                if (DUMP_ALL_FRAMES)
                                {
                                    subnode.Text += "UCmd.Impulse  = " + nf.UCmd.Impulse + "\n";
                                    subnode.Text += "UCmd.Weaponselect  = " + nf.UCmd.Weaponselect + "\n";
                                    subnode.Text += "UCmd.Align3  = " + nf.UCmd.Align3 + "\n";
                                    subnode.Text += "UCmd.Align4  = " + nf.UCmd.Align4 + "\n";
                                    subnode.Text += "UCmd.ImpactIndex  = " + nf.UCmd.ImpactIndex + "\n";
                                    subnode.Text += "UCmd.ImpactPosition.X  = " + nf.UCmd.ImpactPosition.X + "\n";
                                    subnode.Text += "UCmd.ImpactPosition.Y  = " + nf.UCmd.ImpactPosition.Y + "\n";
                                    subnode.Text += "UCmd.ImpactPosition.Z  = " + nf.UCmd.ImpactPosition.Z + "\n";
                                    subnode.Text += "MVars.Gravity  = " + nf.MVars.Gravity + "\n";
                                    subnode.Text += "MVars.Stopspeed  = " + nf.MVars.Stopspeed + "\n";
                                    subnode.Text += "MVars.Maxspeed  = " + nf.MVars.Maxspeed + "\n";
                                    subnode.Text += "MVars.Spectatormaxspeed  = " + nf.MVars.Spectatormaxspeed + "\n";
                                    subnode.Text += "MVars.Accelerate  = " + nf.MVars.Accelerate + "\n";
                                    subnode.Text += "MVars.Airaccelerate  = " + nf.MVars.Airaccelerate + "\n";
                                    subnode.Text += "MVars.Wateraccelerate  = " + nf.MVars.Wateraccelerate + "\n";
                                    subnode.Text += "MVars.Friction  = " + nf.MVars.Friction + "\n";
                                    subnode.Text += "MVars.Edgefriction  = " + nf.MVars.Edgefriction + "\n";
                                    subnode.Text += "MVars.Waterfriction  = " + nf.MVars.Waterfriction + "\n";
                                    subnode.Text += "MVars.Entgravity  = " + nf.MVars.Entgravity + "\n";
                                    subnode.Text += "MVars.Bounce  = " + nf.MVars.Bounce + "\n";
                                    subnode.Text += "MVars.Stepsize  = " + nf.MVars.Stepsize + "\n";
                                    subnode.Text += "MVars.Maxvelocity  = " + nf.MVars.Maxvelocity + "\n";
                                    subnode.Text += "MVars.Zmax  = " + nf.MVars.Zmax + "\n";
                                    subnode.Text += "MVars.WaveHeight  = " + nf.MVars.WaveHeight + "\n";
                                    subnode.Text += "MVars.Footsteps  = " + nf.MVars.Footsteps + "\n";
                                    subnode.Text += "MVars.SkyName  = " + nf.MVars.SkyName + "\n";
                                    subnode.Text += "MVars.Rollangle  = " + nf.MVars.Rollangle + "\n";
                                    subnode.Text += "MVars.Rollspeed  = " + nf.MVars.Rollspeed + "\n";
                                    subnode.Text += "MVars.SkycolorR  = " + nf.MVars.SkycolorR + "\n";
                                    subnode.Text += "MVars.SkycolorG  = " + nf.MVars.SkycolorG + "\n";
                                    subnode.Text += "MVars.SkycolorB  = " + nf.MVars.SkycolorB + "\n";
                                    subnode.Text += "MVars.SkyvecX  = " + nf.MVars.SkyvecX + "\n";
                                    subnode.Text += "MVars.SkyvecY  = " + nf.MVars.SkyvecY + "\n";
                                    subnode.Text += "MVars.SkyvecZ  = " + nf.MVars.SkyvecZ + "\n";
                                    subnode.Text += "View.X  = " + nf.View.X + "\n";
                                    subnode.Text += "View.Y  = " + nf.View.Y + "\n";
                                    subnode.Text += "View.Z  = " + nf.View.Z + "\n";
                                    subnode.Text += "Viewmodel  = " + nf.Viewmodel + "\n";
                                    subnode.Text += "IncomingSequence  = " + nf.IncomingSequence + "(" +
                                                    nf.IncomingSequence.ToString("X2") + ")\n";
                                    subnode.Text += "IncomingAcknowledged  = " + nf.IncomingAcknowledged + "(" +
                                                    nf.IncomingAcknowledged.ToString("X2") + ")\n";
                                    subnode.Text += "IncomingReliableAcknowledged  = " + nf.IncomingReliableAcknowledged +
                                                    "(" + nf.IncomingReliableAcknowledged.ToString("X2") + ")\n";
                                    subnode.Text += "IncomingReliableSequence  = " + nf.IncomingReliableSequence + "(" +
                                                    nf.IncomingReliableSequence.ToString("X2") + ")\n";
                                    subnode.Text += "OutgoingSequence  = " + nf.OutgoingSequence + "(" +
                                                    nf.OutgoingSequence.ToString("X2") + ")\n";
                                    subnode.Text += "ReliableSequence  = " + nf.ReliableSequence + "(" +
                                                    nf.ReliableSequence.ToString("X2") + ")\n";
                                    subnode.Text += "LastReliableSequence = " + nf.LastReliableSequence + "(" +
                                                    nf.LastReliableSequence.ToString("X2") + ")\n";
                                }

                                //subnode.Text += "msg = " + nf.Msg + "\n";
                                if (abs(CurrentTime) > 0 && (FirstAttack || FirstJump) &&
                                    !NewDirectory)
                                {
                                    if (LastIncomingSequence > 0 &&
                                        Math.Abs(nf.IncomingSequence - LastIncomingSequence) > LastLossPacketCount + 3 &&
                                        Math.Abs(nf.IncomingSequence - LastIncomingSequence) > 8 &&
                                        Math.Abs(nf.OutgoingSequence - LastOutgoingSequence) > 6 &&
                                        CurrentFrameDuplicated == 0)
                                    {
                                        if (FrameErrors > 0 && IsUserAlive())
                                        {
                                            if (abs(CurrentTime - LastCmdHack) > 3.0)
                                            {
                                                SkipFirstCMD4--;
                                                if (SkipFirstCMD4 <= 0)
                                                {
                                                    DemoScanner_AddWarn(
                                                        "[CMD HACK TYPE 4] at (" + CurrentTime + ") " + CurrentTimeString,
                                                        false);
                                                    LastCmdHack = CurrentTime;
                                                }
                                                FrameErrors = 0;
                                            }
                                        }

                                        FrameErrors++;
                                    }
                                }

                                if (IsUserAlive())
                                {
                                    if (LastIncomingSequence > 0 && nf.IncomingSequence < LastIncomingSequence)
                                    {
                                        if (abs(CurrentTime - LastCmdHack) > 3.0)
                                        {
                                            DemoScanner_AddWarn(
                                                "[CMD HACK TYPE 9] at (" + CurrentTime + ") " + CurrentTimeString, false);
                                            LastCmdHack = CurrentTime;
                                        }
                                    }
                                }

                                // Console.WriteLine("BAD BAD " + nf.UCmd.Msec + " / " + nf.RParms.Frametime + " = " + ((float)nf.UCmd.Msec / nf.RParms.Frametime).ToString() + " / " + (nf.IncomingSequence - LastIncomingSequence) + " / " + (nf.OutgoingSequence - LastOutgoingSequence));
                                //DuckHack4Search = 0;
                                //if (LastPlayerHull == 1 && (!CurrentFrameOnGround || !PreviousFrameOnGround))
                                //{
                                //    DuckHack4Search = 0;
                                //}
                                //if (CurrentFrameDuplicated <= 0 && LastPlayerHull < 1)
                                //{
                                //    DuckHack4Search--;
                                //    if (DuckHack4Search == 1 && IsUserAlive() && abs(CurrentTime - LastUnDuckTime) > 1.2
                                //        /*&& abs(CurrentTime - LastDuckTime) > 1.2*/ && abs(CurrentTime - LastKreedzHackTime) > 1.0
                                //        && !IsPlayerTeleport())
                                //    {
                                //        DemoScanner_AddWarn(
                                //                     "[DUCK HACK TYPE 4." + (LastPlayerHull + 1) + "] at (" + CurrentTime +
                                //                     ") " + CurrentTimeString, LastPlayerHull == 1);
                                //        KreedzHacksCount++;
                                //        LastKreedzHackTime = CurrentTime;
                                //    }
                                //}
                                //else
                                //{
                                //    DuckHack4Search = 0;
                                //}
                                if (LastIncomingSequence > 0 && Math.Abs(nf.IncomingSequence - LastIncomingSequence) >
                                    maxLastIncomingSequence)
                                {
                                    maxLastIncomingSequence = Math.Abs(nf.IncomingSequence - LastIncomingSequence);
                                }

                                LastIncomingSequence = nf.IncomingSequence;
                                if (LastIncomingAcknowledged > 0 &&
                                    Math.Abs(nf.IncomingAcknowledged - LastIncomingAcknowledged) > maxLastIncomingAcknowledged)
                                {
                                    maxLastIncomingAcknowledged =
                                        Math.Abs(nf.IncomingAcknowledged - LastIncomingAcknowledged);
                                }

                                LastIncomingAcknowledged = nf.IncomingAcknowledged;
                                if (abs(LastChokePacket - CurrentTime) > 0.5 && abs(CurrentTime) > EPSILON &&
                                    nf.OutgoingSequence > 0 && LastOutgoingSequence > 0 &&
                                    nf.OutgoingSequence - LastOutgoingSequence > 3)
                                {
                                    ServerLagCount++;
                                }

                                if (LastOutgoingSequence > 0 && Math.Abs(nf.OutgoingSequence - LastOutgoingSequence) >
                                    maxLastOutgoingSequence)
                                {
                                    maxLastOutgoingSequence = Math.Abs(nf.OutgoingSequence - LastOutgoingSequence);
                                }

                                LastOutgoingSequence = nf.OutgoingSequence;
                                if (DUMP_ALL_FRAMES)
                                {
                                    subnode.Text += OutDumpString;
                                    OutDumpString = "";
                                    subnode.Text += 1 / nf.RParms.Frametime + " FPS";
                                    subnode.Text += "}\n";
                                }

                                PreviousFrameButtons = CurrentFrameButtons;
                                PreviousFrameAlive = CurrentFrameAlive;
                                PreviousFrameAttacked = CurrentFrameAttacked;
                                PreviousFrameAttacked2 = CurrentFrameAttacked2;
                                PreviousFrameJumped = CurrentFrameJumped;
                                PreviousFrameDuck = CurrentFrameDuck;
                                PreviousFrameOnGround = CurrentFrameOnGround;
                                PreviousFramePunchangleZ = CurrentFramePunchangleZ;
                                PreviousFrameForward = CurrentFrameForward;


                                NewDirectory = false;
                                SecondFound = false;
                                SecondFound2 = false;
                                PreviousNetMsgFrame = nf;
                                // FrameCrash = 0;
                                break;
                            }
                    }

                    node.Text = row;
                    if (subnode.Text.Length > 0)
                    {
                        node.Nodes.Add(subnode);
                    }

                    entrynode.Nodes.Add(node);
                }

                PrintNodesRecursive(entrynode);
            }

            try
            {
                if (DUMP_ALL_FRAMES &&
                    File.Exists(CurrentDemoFilePath.Remove(CurrentDemoFilePath.Length - 3) + "_Frames.txt"))
                {
                    File.AppendAllLines(CurrentDemoFilePath.Remove(CurrentDemoFilePath.Length - 3) + "_Frames.txt",
                        outFrames.ToArray());
                }
            }
            catch
            {
                Console.WriteLine("Error access write frame log!");
            }

            if (LastCmd == "-strafe")
            {
                MINIMIZED = true;
                if (IsRussia)
                {
                    DemoScanner_AddInfo("Игрок свернул игру с " + LastAltTabStart + " и до конца игры.");
                }
                else
                {
                    DemoScanner_AddInfo("Player minimized game from " + LastAltTabStart + " to \"FINAL\"");
                }

                if (abs(LastScreenshotTime) > EPSILON && LastScreenshotTime > LastAltTabStartTime)
                {
                    if (IsRussia)
                    {
                        DemoScanner_AddInfo("Игрок был свернут, скриншот может быть черным.");
                    }
                    else
                    {
                        DemoScanner_AddInfo("Game is minimized and screenshot can be black.");
                    }
                }
            }

            //else
            //{
            //    Console.WriteLine(CurrentTime - LastCmdTime > 5.0);
            //}
            if (PluginVersion.Length == 0)
            {
                if (IsRussia)
                {
                    DemoScanner_AddInfo("Не найден бесплатный серверный модуль. Часть данных недоступна.", true);
                    DemoScanner_AddInfo(
                        "https://github.com/UnrealKaraulov/UnrealDemoScanner/blob/main/PLUGIN/plugin.sma", true);
                    DemoScanner_AddInfo("Возможны ложные срабатывания на [AIM TYPE 1.6].", true);
                }
                else
                {
                    DemoScanner_AddInfo("AMXX Plugin not found. Some detects is skipped.", true);
                    DemoScanner_AddInfo(
                        "https://github.com/UnrealKaraulov/UnrealDemoScanner/blob/main/PLUGIN/plugin.sma", true);
                    DemoScanner_AddInfo("Possible false detect one of aimbots: [AIM TYPE 1.6].", true);
                }
            }
            else
            {
                if (!DisableJump5AndAim16)
                {
                    if (JumpCount3 > 20 && (JumpCount6 == 0 || (JumpCount3 > JumpCount6 && JumpCount3 / JumpCount6 > 1.5)))
                    {
                        DemoScanner_AddWarn("[UNKNOWN BHOP/JUMPHACK TYPE 1.1] in demo file!", true, true, !DisableJump5AndAim16,
                            true);
                    }

                    if (attackscounter3 > 20 && attackscounter6 <= 5)
                    {
                        DemoScanner_AddWarn("[UNKNOWN AIM/TRIGGER HACK TYPE 1.1] in demo file!", true, true, !DisableJump5AndAim16,
                            true);
                    }

                    if (((!NoWeaponData && attackscounter4 > 100) || (NoWeaponData && attackscounter > 100)) && attackscounter6 <= 2)
                    {
                        DemoScanner_AddWarn("[UNKNOWN AIM/TRIGGER HACK TYPE 1.2] in demo file!", true, true, !DisableJump5AndAim16,
                           true);
                    }
                }
            }
#if !NET6_0_OR_GREATER
            if (PREVIEW_FRAMES)
            {
                PreviewFramesWriter.Close();
                var tmpPreview = new Preview(CurrentDemoFilePath + ".pfrm");
                tmpPreview.ShowDialog();
                return;
            }
#endif
            ForceFlushScanResults();
            /*Console.ForegroundColor = ConsoleColor.DarkGreen;
            if (IsRussia)
            {
                Console.WriteLine(PROGRAMNAME + " [ " + PROGRAMVERSION + " ] результаты анализа:");
            }
            else
            {
                Console.WriteLine(PROGRAMNAME + " [ " + PROGRAMVERSION + " ] scan result:");
            }*/

            GameEnd = false;
            CL_Intermission = 0;

            if (NeedIgnoreAttackFlagCount > 0)
            {
                if (IsRussia)
                {
                    DemoScanner_AddInfo("Незавршенных атак: " + NeedIgnoreAttackFlagCount + ".");
                }
                else
                {
                    DemoScanner_AddInfo("Lost attack flag: " + NeedIgnoreAttackFlagCount + ".");
                }
            }

            if (attackscounter == 0 || attackscounter3 == 0 || (attackscounter4 == 0 && !NoWeaponData) || (PluginVersion.Length > 0 && flPluginVersion > 1.59 && attackscounter6 == 0))
            {
                if (IsRussia)
                {
                    DemoScanner_AddInfo("Игрок не совершал выстрелы в данном демо файле!");
                }
                else
                {
                    DemoScanner_AddInfo("The player did not fire shots in this demo file!");
                }
            }

            if (JumpCount == 0 || JumpCount2 == 0 || JumpCount3 == 0
                 || JumpCount4 == 0 || JumpCount5 == 0 || (PluginVersion.Length > 0 && flPluginVersion >= 1.54 && JumpCount6 == 0))
            {
                if (IsRussia)
                {
                    DemoScanner_AddInfo("Игрок не совершал прыжков в данном демо файле!");
                }
                else
                {
                    DemoScanner_AddInfo("The player did not jump in this demo file!");
                }
            }

            if (BadEvents > 8)
            {
                if (IsRussia)
                {
                    DemoScanner_AddInfo("Нехватает кадров: " + BadEvents + ".");
                }
                else
                {
                    DemoScanner_AddInfo("Lost frames: " + BadEvents + ".");
                }
            }

            /*
            if (BadAnglesFoundCount > 0)
            {
                DemoScanner_AddInfo("Found strange angles issue: " + BadAnglesFoundCount + ".");
            }*/
            Console.ForegroundColor = ConsoleColor.DarkRed;
            if (FoundForceCenterView > 0)
            {
                if (IsRussia)
                {
                    DemoScanner_AddInfo("Использование запрещенной команды force_centerview: " + FoundForceCenterView +
                                        " " + (FoundForceCenterView > 50
                                            ? "\n. (Подозрительно. Проверьте демку вручную.)"
                                            : ""));
                }
                else
                {
                    DemoScanner_AddInfo("Used illegal force_centerview commands: " + FoundForceCenterView + " " +
                                        (FoundForceCenterView > 50
                                            ? "\n. (Check demo manually for possible demoscanner bypass)"
                                            : ""));
                }
            }

            if (ModifiedDemoFrames > 0)
            {
                if (IsRussia)
                {
                    DemoScanner_AddInfo("Обнаружены проблемы с кадрами, возможно они были изменены:" +
                                        ModifiedDemoFrames);
                }
                else
                {
                    DemoScanner_AddInfo("Possible demoscanner bypass. Tried to kill frames:" + ModifiedDemoFrames);
                }
            }

            if (WarnsAfterGameEnd > 8)
            {
                if (IsRussia)
                {
                    DemoScanner_AddInfo("Есть предупреждения после конца игры: " + WarnsAfterGameEnd);
                }
                else
                {
                    DemoScanner_AddInfo("Possible demoscanner bypass. Detects after game end: " + WarnsAfterGameEnd);
                }
            }

            if (WarnsDuringLevel > 8)
            {
                if (IsRussia)
                {
                    DemoScanner_AddInfo("Есть предупреждения во время смены уровня: " + WarnsDuringLevel);
                }
                else
                {
                    DemoScanner_AddInfo("Detects during changelevel: " + WarnsDuringLevel);
                }
            }

            //if (AttackErrors > 0)
            //{
            //    TextComments.Add("Detected [UNKNOWN CONFIG] для атаки. Detect count:" + AttackErrors);
            //    Console.WriteLine("Detected [UNKNOWN CONFIG] для атаки. Detect count:" + AttackErrors);
            //    Console.WriteLine("Last using at " + LastAttackHack + " second game time.");
            //}
            if (MouseJumps > 10)
            {
                if (IsRussia)
                {
                    OutTextDetects.Add("Обнаружен бинд прыжка на колесо мыши. Количество прыжков:" + MouseJumps);
                    Console.WriteLine("Обнаружен бинд прыжка на колесо мыши. Количество прыжков:" + MouseJumps);
                }
                else
                {
                    OutTextDetects.Add("Detected [MOUSE JUMP] bind. Detect count:" + MouseJumps);
                    Console.WriteLine("Detected [MOUSE JUMP] bind. Detect count:" + MouseJumps);
                }
            }

            if (JumpWithAlias > 15 && MouseJumps > 15)
            {
                if (IsRussia)
                {
                    OutTextDetects.Add("Обнаружен алиас \"+jump;wait;-jump; like alias\". Количество:" + JumpWithAlias);
                    Console.WriteLine("Обнаружен алиас \"+jump;wait;-jump; like alias\". Количество:" + JumpWithAlias);
                    if (MouseJumps > JumpWithAlias)
                    {
                        Console.WriteLine("Вероятность использования: " +
                                          Math.Round(
                                              Convert.ToSingle(JumpWithAlias) / Convert.ToSingle(MouseJumps) * 100.0f,
                                              1) + "%");
                    }
                    else
                    {
                        Console.WriteLine("Высокая вероятность использования.");
                    }
                }
                else
                {
                    OutTextDetects.Add("Detected \"+jump;wait;-jump; like alias\". Detect count:" + JumpWithAlias);
                    Console.WriteLine("Detected \"+jump;wait;-jump; like alias\". Detect count:" + JumpWithAlias);
                    if (MouseJumps > JumpWithAlias)
                    {
                        Console.WriteLine("Mouse jump / alias ratio: " +
                                          Math.Round(
                                              Convert.ToSingle(JumpWithAlias) / Convert.ToSingle(MouseJumps) * 100.0f,
                                              1) + "%");
                    }
                    else
                    {
                        Console.WriteLine("Hight alias ratio detected.");
                    }
                }
            }

            //if (BHOPcount / 2 > 1)
            //{
            //    if (IsRussia)
            //    {
            //        OutTextDetects.Add("[BHOP] Количество предупреждений:" + BHOPcount / 2);
            //        Console.WriteLine("[BHOP] Количество предупреждений:" + BHOPcount / 2);
            //    }
            //    else
            //    {
            //        OutTextDetects.Add("[BHOP] Warn count:" + BHOPcount / 2);
            //        Console.WriteLine("[BHOP] Warn count:" + BHOPcount / 2);
            //    }
            //}

            //if (TriggerAimAttackCount > 0)
            //{
            //    if (IsRussia)
            //    {
            //        OutTextDetects.Add("[Триппер бот] Количество предупреждений:" + TriggerAimAttackCount);
            //        Console.WriteLine("[Триппер бот] Количество предупреждений:" + TriggerAimAttackCount);
            //    }
            //    else
            //    {
            //        OutTextDetects.Add("[TRIGGERBOT] Warn count:" + TriggerAimAttackCount);
            //        Console.WriteLine("[TRIGGERBOT] Warn count:" + TriggerAimAttackCount);
            //    }
            //}

            //if (TotalAimBotDetected > 0)
            //{
            //    if (IsRussia)
            //    {
            //        OutTextDetects.Add("[AIM] Количество предупреждений:" + TotalAimBotDetected);
            //        Console.WriteLine("[AIM] Количество предупреждений:" + TotalAimBotDetected);
            //    }
            //    else
            //    {
            //        OutTextDetects.Add("[AIM] Warn count:" + TotalAimBotDetected);
            //        Console.WriteLine("[AIM] Warn count:" + TotalAimBotDetected);
            //    }
            //}

            if (FakeLagAim > 0)
            {
                if (IsRussia)
                {
                    OutTextDetects.Add("[ФЕЙК ЛАГ] Количество предупреждений:" + FakeLagAim);
                    Console.WriteLine("[ФЕЙК ЛАГ] Количество предупреждений:" + FakeLagAim);
                }
                else
                {
                    OutTextDetects.Add("[FAKELAG] Warn count:" + FakeLagAim);
                    Console.WriteLine("[FAKELAG] Warn count:" + FakeLagAim);
                }
            }

            //if (BypassWarn8_2 > 25)
            //{
            //    OutTextDetects.Add("[AIM8 BYPASS WITH DOOR]  Warn count:" + FakeLagAim);
            //    Console.WriteLine("[AIM8 BYPASS WITH DOOR] Warn count:" + BypassWarn8_2);
            //}
            //if (KreedzHacksCount > 0)
            //{
            //    if (IsRussia)
            //    {
            //        OutTextDetects.Add(
            //            "[STRAFE/GROUND/FASTRUN ХАК] Количество предупреждений:" +
            //            KreedzHacksCount /*+ ". Found " + JumpCount + " +jump commands"*/);
            //        Console.WriteLine(
            //            "[STRAFE/GROUND/FASTRUN ХАК] Количество предупреждений:" +
            //            KreedzHacksCount /*+ ". Found " + JumpCount + " +jump commands*/);
            //    }
            //    else
            //    {
            //        OutTextDetects.Add(
            //            "[STRAFE/GROUND/FASTRUN HACK] Warn count:" +
            //            KreedzHacksCount /*+ ". Found " + JumpCount + " +jump commands"*/);
            //        Console.WriteLine(
            //            "[STRAFE/GROUND/FASTRUN HACK] Warn count:" +
            //            KreedzHacksCount /*+ ". Found " + JumpCount + " +jump commands*/);
            //    }
            //}

            if (UnknownMessages > 0)
            {
                if (IsRussia)
                {
                    OutTextDetects.Add("Внимание. Обнаружено несколько неопознанных пакетов данных:" + UnknownMessages);
                    Console.WriteLine("Внимание. Обнаружено несколько неопознанных пакетов данных:" + UnknownMessages);
                }
                else
                {
                    OutTextDetects.Add("Warning. Unknown messages detected. Detect count:" + UnknownMessages);
                    Console.WriteLine("Warning. Unknown messages detected. Detect count:" + UnknownMessages);
                }
            }

            if (unknownCMDLIST.Count > 0)
            {
                if (IsRussia)
                {
                    Console.WriteLine("Обнаружены неопознанные чат команды:");
                    OutTextDetects.Add("Обнаружены неопознанные чат команды:");
                }
                else
                {
                    Console.WriteLine("Bind/alias from blacklist detected:");
                    OutTextDetects.Add("Bind/alias from blacklist detected:");
                }

                foreach (var chet in unknownCMDLIST)
                {
                    OutTextDetects.Add(chet);
                    Console.WriteLine(chet);
                }
            }

            //if (BadTimeFound > 100)
            //{
            //    if (IsRussia)
            //    {
            //        TextComments.Add("[TIME HACK] Обнаружен обход сканера. Метод: остановка времени.");
            //        Console.WriteLine("[TIME HACK] Обнаружен обход сканера. Метод: остановка времени.");
            //    }
            //    else
            //    {
            //        TextComments.Add("[TIME HACK] Scanner bypass with method TIME detected!");
            //        Console.WriteLine("[TIME HACK] Scanner bypass with method TIME detected!");
            //    }
            //}
            //if (Math.Round((1000.0f / MsecMin), 5) /
            //    Math.Round(1000.0 / (1000.0f * FrametimeMin), 5) > 1.75f)
            //{
            //    Console.WriteLine(" [aim/jump] Обнаружены манипуляции с задержкой. (???)");
            //    Console.WriteLine("Unknown delay/timer hack detected. (???)");
            //}
            //if (Convert.ToSingle(RealFpsMax) / Math.Round(1000.0 / (1000.0f * FrametimeMin), 5) > 1.5f)
            //{
            //    Console.WriteLine(" [speedhack] Обнаружены манипуляции с задержкой. (???)");
            //    Console.WriteLine("Unknown delay/timer hack detected. (???)");
            //}
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            ViewDemoHelperComments.Seek(4, SeekOrigin.Begin);
            ViewDemoHelperComments.Write(ViewDemoCommentCount);
            ViewDemoHelperComments.Seek(0, SeekOrigin.Begin);
            ForceFlushScanResults();

            try
            {
                var EndScanTime = Trim(new DateTime((DateTime.Now - StartScanTime).Ticks), 10);
                var t1 = TimeSpan.FromSeconds(StartGameSecond);
                var t1str = string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms", t1.Hours, t1.Minutes, t1.Seconds,
                    t1.Milliseconds);
                var t2 = TimeSpan.FromSeconds(EndGameSecond);
                var t2str = string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms", t2.Hours, t2.Minutes, t2.Seconds,
                    t2.Milliseconds);
                if (IsRussia)
                {
                    Console.WriteLine("Анализ завершен, потрачено " + EndScanTime.ToString("T") + " времени");
                    Console.WriteLine("Игровое время демо начинается с " + t1str + " по " + t2str + " секунд.");
                }
                else
                {
                    Console.WriteLine("Scan completed. Scan time: " + EndScanTime.ToString("T"));
                    Console.WriteLine("Demo playing time: " + t1str + " ~ " + t2str + " seconds.");
                }
            }
            catch
            {
                if (IsRussia)
                {
                    Console.WriteLine("Анализ завершен");
                }
                else
                {
                    Console.WriteLine("Scan completed.");
                }
            }

            //foreach (var s in testData)
            //{
            //    File.AppendAllText("Delta.txt", s.Key + "=\n");
            //    File.AppendAllText("Delta.txt", s.Value);
            //}

            if (CurrentDemoFile.GsDemoInfo.ParsingErrors.Count > 0)
            {
                if (IsRussia)
                {
                    Console.WriteLine("Во время сканирования были обнаружены следующие ошибки:");
                }
                else
                {
                    Console.WriteLine("Parse errors:");
                }
                Console.ForegroundColor = ConsoleColor.DarkRed;
                foreach (var s in CurrentDemoFile.GsDemoInfo.ParsingErrors)
                {
                    var s2 = Regex.Replace(s, @"[^\u0000-\u007F]+", "_");
                    Console.WriteLine("Error:" + s2.Trim());
                }
            }

            if (PlayerSensUsageList.Count > 1)
            {
                PlayerSensUsageList =
                    new List<PLAYER_USED_SENS>(PlayerSensUsageList.OrderByDescending(x => x.usagecount));
            }

            if (LOG_MODE)
            {
                try
                {
                    File.WriteAllLines(CurrentDemoFilePath + ".log", LOG_MODE_OUTPUT.ToArray());
                }
                catch
                {
                }


                Console.SetOut(originalOut);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine();
            }

            if (SKIP_RESULTS)
            {
                return;
            }

            while (true)
            {
                ConsoleTable table = null;
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Gray;

                if (IsRussia)
                {
                    Console.WriteLine("------------Выберите команду:------------");
                }
                else
                {
                    Console.WriteLine("------------Select command:------------");
                }

                Console.ForegroundColor = ConsoleColor.Red;
                if (IsRussia)
                {
                    Console.WriteLine("ВАЖНО!");
                    Console.WriteLine("Введите команду '8' что бы получить помощь.");
                    Console.WriteLine("Введите команду '11' для получения информации о детектах!");
                    Console.WriteLine("ВАЖНО!");
                }
                else
                {
                    Console.WriteLine("IMPORTANT!");
                    Console.WriteLine("Enter command '8' for get help!");
                    Console.WriteLine("Enter command '11' for get info about detections and warnings!");
                    Console.WriteLine("IMPORTANT!");
                }

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Gray;
                if (IsRussia)
                {
                    table = new ConsoleTable("в CDB", "в TXT", "Демо инфо", "Игроки", "Голоса", "История мыши",
                        "Команды");
                    table.AddRow("1", "2", "3", "4", "5", "6", "7");
                    table.Write(Format.Alternative);
                    table = new ConsoleTable("Помощь", "Скачать", "Все сообщения", "Читы", "Перескан", "Выход");
                    table.AddRow("8", "9", "10", "11", "12", "0/Enter");
                    table.Write(Format.Alternative);
                }
                else
                {
                    table = new ConsoleTable("Save CDB", "Save TXT", "Demo info", "Player info", "Wav Player",
                        "Sens History", "Commands");
                    table.AddRow("1", "2", "3", "4", "5", "6", "7");
                    table.Write(Format.Alternative);
                    table = new ConsoleTable("Help", "Download", "All msg", "Hacks", "Rescan", "Exit");
                    table.AddRow("8", "9", "10", "11", "12", "0/Enter");
                    table.Write(Format.Alternative);
                }

                var command = Console.ReadLine();
                if (command.Length == 0 || command == "0")
                {
                    return;
                }

                if (command == "12")
                {
                    /* START: SOME BLACK MAGIC OUTSIDE HOGWARTS */
                    FieldInfo[] fields = typeof(DemoScanner).GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

                    foreach (var field in fields)
                    {
                        try
                        {
                            Type fieldType = field.FieldType;
                            object defaultValue = fieldType.IsValueType ? Activator.CreateInstance(fieldType) : null;
                            field.SetValue(null, defaultValue);
                        }
                        catch
                        {
                        }
                    }
                    typeof(DemoScanner)
                        .GetConstructor(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, null,
                            new Type[0], null).Invoke(null, null);
                    /* END: VERY DARK BLACK MAGIC!!!!!! */
                    DemoRescanned = true;
                    FirstBypassKill = false;
                    goto DEMO_FULLRESET;
                }

                if (command == "11")
                {
                    if (IsRussia)
                    {
                        DemoScanner_AddInfo("[AIM TYPE 1.X] - Чит аимбот");
                        DemoScanner_AddInfo("[AIM TYPE 2.X] - Чит аимбот, автоатака");
                        DemoScanner_AddInfo("[AIM TYPE 3] - Чит аимбот, прерывание атаки");
                        DemoScanner_AddInfo("[AIM TYPE 4.X] - Чит аимбот, лаги при выстрелах");
                        DemoScanner_AddInfo("[AIM TYPE 5.X] - Чит аимбот, манипуляция мышью");
                        DemoScanner_AddInfo("[AIM TYPE 7.X] - Чит HPP, триггер бот, аим бот");
                        DemoScanner_AddInfo("[AIM TYPE 8.X] - Чит аимбот, используется во многих функциях");
                        DemoScanner_AddInfo("[AIM TYPE 9.X] - Чит аимбот, антиразброс");
                        DemoScanner_AddInfo("[AIM TYPE 10] - Чит аимбот, подмена угла");
                        DemoScanner_AddInfo("[AIM TYPE 11] - Чит аимбот, скрытие выстрела");
                        DemoScanner_AddInfo("[AIRSTUCK HACK] - Функия читов, зависание в воздухе");
                        DemoScanner_AddInfo("[AUTORELOAD TYPE 1] - Чит аимбот, автоперезарядка");
                        DemoScanner_AddInfo("[ATTACK FLOOD TYPE X] - Обход сканера");
                        DemoScanner_AddInfo("[BHOP TYPE 1] - Распрыжка без соотвествующей команды, читы");
                        DemoScanner_AddInfo("[BHOP TYPE 2] - Распрыжка вообще без команд, читы");
                        DemoScanner_AddInfo("[CMD HACK TYPE 1]");
                        DemoScanner_AddInfo("[CMD HACK TYPE 2] - Зависание в воздухе, флуд командами, читы");
                        DemoScanner_AddInfo("[CMD HACK TYPE 4] - Генератор фейк лагов, часть аимбота");
                        DemoScanner_AddInfo("[CMD HACK TYPE 6] - Фейк лаг, часть аим бота, подделка данных");
                        DemoScanner_AddInfo("[CMD HACK TYPE 9] - Часть аим бота, подделка данных");
                        DemoScanner_AddInfo("[DUCK FLOOD TYPE X] - Обход сканера, читы");
                        DemoScanner_AddInfo("[DUCK HACK TYPE 1] - Приседание без команды, читы");
                        DemoScanner_AddInfo("[DUCK HACK TYPE 2] - Приседание без кнопок, читы");
                        DemoScanner_AddInfo("[DUCK HACK TYPE 3] - Так называемый ddrun/gstrafe, читы");
                        DemoScanner_AddInfo("[DUCK HACK TYPE 4] - Приседание без соответсвующих команд, читы");
                        DemoScanner_AddInfo("[JUMPHACK TYPE 5.X] - Распрыжка без команд, попытка обхода");
                        DemoScanner_AddInfo("[FAKELAG TYPE X] - Фейклаги, подделка данных");
                        DemoScanner_AddInfo("[FAKESPEED TYPE X] - Спидхак или подделка данных");
                        DemoScanner_AddInfo("[FORWARD HACK TYPE 1] - Чит на ускорение, распрыжку");
                        DemoScanner_AddInfo("[FOV HACK TYPE X] - Смена FOV, может быть читом");
                        DemoScanner_AddInfo("[FPS HACK TYPE 1] ");
                        DemoScanner_AddInfo("[FPS HACK TYPE 2] - ФПС бустер, может быть читом");
                        DemoScanner_AddInfo("[FPS HACK TYPE 3] - Очень высокий фпс, может быть читом");
                        DemoScanner_AddInfo("[IDEALJUMP] - Идеальный прыжок, может быть читом(или сервер плагин)");
                        DemoScanner_AddInfo("[JUMPHACK XTREME] - Чит XTREME");
                        DemoScanner_AddInfo("[JUMPHACK HPP] - Чит HPP (Или из-за экстремальных настроек сервера)");
                        DemoScanner_AddInfo("[JUMPHACK TYPE X] - Остальные читы на прыжки");
                        DemoScanner_AddInfo("[KNIFEBOT TYPE X] - Кнайф бот");
                        DemoScanner_AddInfo(
                            "[MOVEMENT HACK TYPE 1] - Чит на ускорение, стрейфы, часть аимбота/KZ хака");
                        DemoScanner_AddInfo("[MOVEMENT HACK TYPE 2] - Чит на ускорение, часть аимбота/KZ хака");
                        DemoScanner_AddInfo("[NO WEAPON ANIM] - Скрытие анимации оружия, часть аим бота");
                        DemoScanner_AddInfo("[NO SPREAD TYPE X] - Чит аимбот, антиразброс");
                        DemoScanner_AddInfo(
                            "['RETURN TO GAME' FEATURE] - Автовозврат к игре (чит или функция кастом клиентов)");
                        DemoScanner_AddInfo("[STRAFE OPTIMIZER] - Чит [strafe_optimizer.exe] и подобные");
                        DemoScanner_AddInfo("[TIMESHIFT] - Чит спидхак, если много срабатываний");
                        DemoScanner_AddInfo(
                            "[THIRD PERSON TYPE X] - Вид от третьего лица, может быть читом, или сервер-плагином");
                        DemoScanner_AddInfo("[TRIGGER TYPE X] - Триггер бот");
                        DemoScanner_AddInfo("[BETA] - Находится на бета тестировании, проверять в игре!");
                    }
                    else
                    {
                        DemoScanner_AddInfo("[AIM TYPE 1.X] - Aimbot");
                        DemoScanner_AddInfo("[AIM TYPE 2.X] - Aimbot, autoattack");
                        DemoScanner_AddInfo("[AIM TYPE 3] - Aimbot, attack interruption");
                        DemoScanner_AddInfo("[AIM TYPE 4.X] - Aimbot, lagshot");
                        DemoScanner_AddInfo("[AIM TYPE 5.X] - Aimbot, fake sensitivity");
                        DemoScanner_AddInfo("[AIM TYPE 7.X] - HPP, trigger/aimbot");
                        DemoScanner_AddInfo("[AIM TYPE 8.X] - Aimbot, fake angles");
                        DemoScanner_AddInfo("[AIM TYPE 9.X] - Aimbot, no-spread");
                        DemoScanner_AddInfo("[AIM TYPE 10] - Aimbot, angle hack");
                        DemoScanner_AddInfo("[AIM TYPE 11] - Aimbot, hidden attack");
                        DemoScanner_AddInfo("[AIRSTUCK HACK] - 'Airstuck' feature");
                        DemoScanner_AddInfo("[AUTORELOAD TYPE 1] - Aimbot, autoreload");
                        DemoScanner_AddInfo("[ATTACK FLOOD TYPE X] - Scanner bypass");
                        DemoScanner_AddInfo("[BHOP TYPE 1] - Auto bhop without jump");
                        DemoScanner_AddInfo("[BHOP TYPE 2] - Auto bhop without any commands");
                        DemoScanner_AddInfo("[CMD HACK TYPE 1]");
                        DemoScanner_AddInfo("[CMD HACK TYPE 2] - Airstuck, fake cmd, part of aimbot");
                        DemoScanner_AddInfo("[CMD HACK TYPE 4] - Fakelag generation, part of aimbot");
                        DemoScanner_AddInfo("[CMD HACK TYPE 6] - Fakelag, fake cmd, part of aimbot");
                        DemoScanner_AddInfo("[CMD HACK TYPE 9] - Fake data, part of aimbot");
                        DemoScanner_AddInfo("[DUCK FLOOD TYPE X] - Scanner bypass, part of hack");
                        DemoScanner_AddInfo("[DUCK HACK TYPE 1] - No duck, part of hack");
                        DemoScanner_AddInfo("[DUCK HACK TYPE 2] - Duck no button, part of hack");
                        DemoScanner_AddInfo("[DUCK HACK TYPE 3] - ddrun/gstrafe, part of hack");
                        DemoScanner_AddInfo("[DUCK HACK TYPE 4] - Duck no any commands, part of hack");
                        DemoScanner_AddInfo("[JUMPHACK TYPE 5.X] - Jump no any command, bypass tries");
                        DemoScanner_AddInfo("[FAKELAG TYPE X] - Fakelag, fake data");
                        DemoScanner_AddInfo("[FAKESPEED TYPE X] - Speedhack, fake data");
                        DemoScanner_AddInfo("[FORWARD HACK TYPE 1] - Run forward hack");
                        DemoScanner_AddInfo("[FOV HACK TYPE X] - FOV changed, can be part of hack");
                        DemoScanner_AddInfo("[FPS HACK TYPE 1]");
                        DemoScanner_AddInfo("[FPS HACK TYPE 2] - FPS-booster, can be part of hack");
                        DemoScanner_AddInfo("[FPS HACK TYPE 3] - Really big FPS, can be part of hack");
                        DemoScanner_AddInfo(
                            "[IDEALJUMP] - One-Frame ideal jump, can be part of hack (or server plugin)");
                        DemoScanner_AddInfo("[JUMPHACK XTREME] - XTREME Jump");
                        DemoScanner_AddInfo(
                            "[JUMPHACK HPP] - HPP JumpHack (Or extreme server settings like accelerate)");
                        DemoScanner_AddInfo("[JUMPHACK TYPE X] - Other jump hacks");
                        DemoScanner_AddInfo("[KNIFEBOT TYPE X] - Knife bot");
                        DemoScanner_AddInfo("[MOVEMENT HACK TYPE 1] - Run/strafe hack, part of aimbot/KZ hack");
                        DemoScanner_AddInfo("[MOVEMENT HACK TYPE 2] - Run forward hack, part of aimbot/KZ hack");
                        DemoScanner_AddInfo("[NO WEAPON ANIM] - No weapon animation, part of aimbot");
                        DemoScanner_AddInfo("[NO SPREAD TYPE X] - Aimbot, no spread");
                        DemoScanner_AddInfo(
                            "['RETURN TO GAME' FEATURE] - Part of aimbot features, or another CS-1.6 client feature");
                        DemoScanner_AddInfo("[STRAFE OPTIMIZER] - strafe_optimizer.exe (and same hacks)");
                        DemoScanner_AddInfo("[TIMESHIFT] - Speedhack (if multiple times)");
                        DemoScanner_AddInfo(
                            "[THIRD PERSON TYPE X] - Third person hack, can be part of hack, or server plugin");
                        DemoScanner_AddInfo("[TRIGGER TYPE X] - Trigger bot");
                        DemoScanner_AddInfo("[BETA] - This warn in beta test mode. Please check it ingame!");
                    }
                }

                if (command == "9")
                {
                    Console.Write("Enter path to cstrike dir[D:/HalfLife1/cstrike/]:");
                    var strikedir = Console.ReadLine().Replace("\"", "");
                    if (strikedir.EndsWith("/") || strikedir.EndsWith("\\"))
                    {
                        strikedir = strikedir.Remove(strikedir.Length - 1);
                    }

                    if (File.Exists(strikedir + "/../" + "hw.dll"))
                    {
                        if (!Directory.Exists(strikedir))
                        {
                            try
                            {
                                Directory.CreateDirectory(strikedir);
                            }
                            catch
                            {
                            }
                        }

                        if (Directory.Exists(strikedir))
                        {
                            Console.WriteLine("Download " + DownloadedResources.Count + " resources with total size: " +
                                              DownloadResourcesSize + " bytes");
                            Console.WriteLine("Download start time:" + DateTime.Now.ToString("HH:mm:ss"));
                            if (File.Exists(CurrentDir + "/DownloadError.txt"))
                            {
                                try
                                {
                                    File.Delete(CurrentDir + "/DownloadError.txt");
                                }
                                catch
                                {
                                }
                            }

                            var sum = 0;
                            var threads = 0;
                            var threadid = 0;

                            List<string> invalidPaths = new List<string>();

                            Parallel.ForEach(DownloadedResources,
                                new ParallelOptions { MaxDegreeOfParallelism = RESOURCE_DOWNLOAD_THREADS }, res =>
                                {
                                    string url = res.res_path.Replace("/", "\\");
                                    var current_thread_id = 0;
                                    lock (sync)
                                    {
                                        if (threadid > RESOURCE_DOWNLOAD_THREADS)
                                        {
                                            threadid = 0;
                                        }

                                        current_thread_id = threadid;
                                        threadid++;
                                        Interlocked.Increment(ref sum);
                                        Interlocked.Increment(ref threads);
                                    }

                                    Thread.SetData(Thread.GetNamedDataSlot("int"), current_thread_id);
                                    if (char.IsLetterOrDigit(res.res_path[0]) && !File.Exists(strikedir + "/" + res.res_path) &&
                                        !File.Exists(strikedir + "/../cstrike/" + res.res_path) &&
                                        !File.Exists(strikedir + "/../valve/" + res.res_path))
                                    {
                                        lock (sync)
                                        {
                                            ClearCurrentConsoleLine();
                                            Console.Write("\rDownload \"" + res.res_path + "\" " + sum + " of " +
                                                          DownloadedResources.Count + ". In " +
                                                          GetActiveDownloadThreads() + " of " +
                                                          (GetActiveDownloadThreads() > threads
                                                              ? GetActiveDownloadThreads()
                                                              : threads) + " threads.");
                                        }

                                        try
                                        {
                                            Task.Run(async () =>
                                            {
                                                HttpClient client = new HttpClient();
                                                client.Timeout = TimeSpan.FromMilliseconds(3000);

                                                HttpResponseMessage response = await client.GetAsync(DownloadLocation + url, HttpCompletionOption.ResponseHeadersRead);

                                                using (var stream = await response.Content.ReadAsStreamAsync())
                                                {
                                                    using (MemoryStream memoryStream = new MemoryStream())
                                                    {
                                                        byte[] buffer = new byte[8192];
                                                        int bytesRead;
                                                        long totalBytes = response.Content.Headers.ContentLength ?? -1;

                                                        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                                                        {
                                                            memoryStream.Write(buffer, 0, bytesRead);

                                                            var curthreadid = Thread.GetData(Thread.GetNamedDataSlot("int")) != null
                                                                ? (int)Thread.GetData(Thread.GetNamedDataSlot("int"))
                                                                : 0;
                                                            UpdateThreadState(curthreadid, Environment.TickCount);
                                                        }

                                                        if (response.IsSuccessStatusCode)
                                                        {
                                                            try
                                                            {
                                                                Directory.CreateDirectory(Path.GetDirectoryName(strikedir + "/" + res.res_path));
                                                            }
                                                            catch
                                                            {

                                                            }
                                                            File.WriteAllBytes(strikedir + "//" + res.res_path, memoryStream.ToArray());
                                                        }
                                                    }
                                                }
                                            }).Wait();
                                        }
                                        catch
                                        {
                                            lock (sync)
                                            {
                                                try
                                                {
                                                    ClearCurrentConsoleLine();
                                                    invalidPaths.Add(res.res_path);
                                                    var dwnerrorstr = "\rFailed to download \"" + res.res_path + "\" file.";
                                                    Console.Write(dwnerrorstr);
                                                    Thread.Sleep(50);
                                                    File.AppendAllText(CurrentDir + "/DownloadError.txt",
                                                        dwnerrorstr + "\r\n");
                                                }
                                                catch
                                                {
                                                    Console.WriteLine("Error download...");
                                                }
                                            }
                                        }
                                    }

                                    lock (sync)
                                    {
                                        threads--;
                                    }

                                    Thread.Sleep(10);
                                });
                            Console.WriteLine();
                            Console.WriteLine("Download end time:" + DateTime.Now.ToString("HH:mm:ss"));
                            if (File.Exists(CurrentDir + "/DownloadError.txt"))
                            {
                                Console.WriteLine("Files downloaded with errors... Opening log...");
                                Process.Start(new ProcessStartInfo
                                {
                                    FileName = CurrentDir + "/DownloadError.txt",
                                    UseShellExecute = true
                                });

                                if (invalidPaths.Count > 0)
                                {
                                    Console.Write("You want to replace missing files with empty? (Y/N):");

                                    if (Console.ReadLine().ToLower() == "y")
                                    {
                                        foreach (var path in invalidPaths)
                                        {
                                            try
                                            {
                                                string lowerpath = path.ToLower().TrimEnd('.');
                                                if (lowerpath.EndsWith(".spr"))
                                                {
                                                    try
                                                    {
                                                        Directory.CreateDirectory(Path.GetDirectoryName(strikedir + "/" + path));
                                                    }
                                                    catch
                                                    {
                                                    }
                                                    File.WriteAllBytes(strikedir + "//" + path, EMPTY_SPR);
                                                }
                                                else if (lowerpath.EndsWith(".mdl"))
                                                {
                                                    try
                                                    {
                                                        Directory.CreateDirectory(Path.GetDirectoryName(strikedir + "/" + path));
                                                    }
                                                    catch
                                                    {
                                                    }
                                                    File.WriteAllBytes(strikedir + "//" + path, EMPTY_MDL);
                                                }
                                                else if (lowerpath.EndsWith(".mp3"))
                                                {
                                                    try
                                                    {
                                                        Directory.CreateDirectory(Path.GetDirectoryName(strikedir + "/" + path));
                                                    }
                                                    catch
                                                    {
                                                    }
                                                    File.WriteAllBytes(strikedir + "//" + path, EMPTY_MP3);
                                                }
                                                else if (lowerpath.EndsWith(".wav"))
                                                {
                                                    try
                                                    {
                                                        Directory.CreateDirectory(Path.GetDirectoryName(strikedir + "/" + path));
                                                    }
                                                    catch
                                                    {
                                                    }
                                                    File.WriteAllBytes(strikedir + "//" + path, EMPTY_WAV);
                                                }
                                                else if (lowerpath.EndsWith(".bsp"))
                                                {
                                                    Console.WriteLine("Error .bsp file can not be empty: " + path);
                                                }
                                                else if (lowerpath.EndsWith(".ini") || lowerpath.EndsWith(".cfg"))
                                                {
                                                    Console.WriteLine("Can't write configuration file: " + path);
                                                }
                                                else if (
                                                    lowerpath.EndsWith(".dll") ||
                                                    lowerpath.EndsWith(".exe") ||
                                                    lowerpath.EndsWith(".drv") ||
                                                    lowerpath.EndsWith(".bat") ||
                                                    lowerpath.EndsWith(".cmd") ||
                                                    lowerpath.EndsWith(".asi") ||
                                                    lowerpath.EndsWith(".mix") ||
                                                    lowerpath.EndsWith(".m3d") ||
                                                    lowerpath.EndsWith(".flt")
                                                    )
                                                {
                                                    Console.WriteLine("Can't write executable file: " + path);
                                                }
                                                else
                                                {
                                                    try
                                                    {
                                                        Directory.CreateDirectory(Path.GetDirectoryName(strikedir + "/" + path));
                                                    }
                                                    catch
                                                    {
                                                    }
                                                    File.WriteAllBytes(strikedir + "//" + path, new byte[0]);
                                                }
                                            }
                                            catch
                                            {
                                                Console.WriteLine("Error write " + path + " file!");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Bad directory[1]! Please enter path to Half Life directory!");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Bad directory[1]! Please enter path to Half Life directory!");
                    }
                }

                if (command == "8")
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.WriteLine(" ");
                    Console.WriteLine(" ");
                    Console.WriteLine(" ");
                    Console.WriteLine(
                        " * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * ");
                    Console.WriteLine(" * Developer: Karaulov");
                    Console.WriteLine(" * Unreal Demo Scanner tool for search hack patterns in demo file.");
                    Console.WriteLine(" * Unreal Demo Scanner программа помогающая обнаружить читеров.");
                    Console.WriteLine(" * Report false positive: (Сообщить о ложном срабатывании):");
                    Console.WriteLine(" * https://dev-cs.ru/threads/10684/ , https://goldsrc.ru/threads/4627/");
                    Console.WriteLine(
                        " * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * ");
                    Console.WriteLine(" ");
                    Console.WriteLine(" ");
                    Console.WriteLine(" ");
                    Console.WriteLine(
                        " * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * ");
                    Console.WriteLine(" * (DETECTED)        ———  Cheat detected with very big possibility! ");
                    Console.WriteLine(" * (WARNING)         ———  Scanner think that this moment very strange ");
                    Console.WriteLine(" *                   ———  need check demo manually in game! ");
                    Console.WriteLine(" * Warn types: ");
                    Console.WriteLine(" * (LAG)             ———  Player with unstable connection");
                    Console.WriteLine(" * (DEAD)            ———  Player is dead");
                    Console.WriteLine(
                        " * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * ");
                    Console.WriteLine(
                        " * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * ");
                    Console.WriteLine(" * (ОБНАРУЖЕНО)        ———  С большой вероятность игрок использует чит ");
                    Console.WriteLine(" * (ПРЕДУПРЕЖДЕНИЕ)    ———  Сканер считает этот момент странным ");
                    Console.WriteLine(" *                     ———  требуется ручная проверка в игре! ");
                    Console.WriteLine(" * Warn types: ");
                    Console.WriteLine(" * (LAG)             ———  У игрока были лаги во время этого варна");
                    Console.WriteLine(" * (DEAD)            ———  Игрок умер");
                    Console.WriteLine(
                        " * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * ");
                    Console.WriteLine(
                        " * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * ");
                    Console.WriteLine(" * [ОБНАРУЖЕНО] - требуется всего несколько за демо что бы с точностью сказать");
                    Console.WriteLine(" *               что игрок пользовался запрещенными программами.  ");
                    Console.WriteLine(
                        " * [ПРЕДУПРЕЖДЕНИЕ] - даже несколько срабатвыаний за демо не говорит об наличии читов");
                    Console.WriteLine(" *        но указывает на необходимость проверить игрока вручную.");
                    Console.WriteLine(" * ");
                    Console.WriteLine(" *                НО ВНИМАНИЕ, АДМИН!!");
                    Console.WriteLine(" * ");
                    Console.WriteLine(" * Учитывайте наличие на сервере нестандартных надстроек таких как");
                    Console.WriteLine(" * bhop, увеличенный/уменьшенный разброс, и так далее. Вы должны убе-");
                    Console.WriteLine(" * дится что на ваших чистых демо нет ложных срабатываний и если они");
                    Console.WriteLine(" * связаны с наличием определенных плагинов, учитывать это.");
                    Console.WriteLine(" * ");
                    Console.WriteLine(" *                Благодарности:");
                    Console.WriteLine(" *   s1lent (https://github.com/s1lentq), garey(https://github.com/Garey27)");
                    Console.WriteLine(" *   - за помощь в некоторых моментах связанных с движком");
                    Console.WriteLine(" *   А так же:");
                    Console.WriteLine(" *   Всем кто предоставлял сведения о проблемах и ложных срабатываниях.");
                    Console.WriteLine(
                        " * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * ");
                }

                if (command == "7")
                {
                    try
                    {
                        CommandsDump.Insert(0, "This file created by Unreal Demo Scanner\n\n");
                        File.WriteAllLines(CurrentDir + "/Commands.txt", CommandsDump.ToArray());
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = CurrentDir + "/Commands.txt",
                            UseShellExecute = true
                        });
                    }
                    catch
                    {
                        Console.WriteLine("Error access to command file.");
                    }
                }

                if (command == "6")
                {
                    table = new ConsoleTable("Second", "Sensitivity", "Length", "Time", "Weapon", "FOV");
                    for (var i = 0; i < PlayerSensitivityHistory.Count; i++)
                    {
                        table.AddRow(i + 1, (PlayerSensitivityHistory[i] / 0.022f).ToString("F6"),
                            PlayerAngleLenHistory[i], PlayerSensitivityHistoryStrTime[i],
                            PlayerSensitivityHistoryStrWeapon[i], PlayerSensitivityHistoryStrFOV[i]);
                    }

                    table.Write(Format.Alternative);
                    try
                    {
                        File.WriteAllText(CurrentDir + "/SensHistory.txt", table.ToStringAlternative());
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = CurrentDir + "/SensHistory.txt",
                            UseShellExecute = true
                        });
                    }
                    catch
                    {
                        Console.WriteLine("Error access write sens history log!");
                    }
                }

                if (command == "5")
                {
                    if (!File.Exists(CurrentDir + "/revoicedecoder.exe"))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        if (IsRussia)
                        {
                            Console.WriteLine("Установите декодер в " + CurrentDir + "/revoicedecoder.exe");
                        }
                        else
                        {
                            Console.WriteLine("Please install decoder at " + CurrentDir + "/revoicedecoder.exe");
                        }
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        continue;
                    }

                    bool one_file = false;

                    if (IsRussia)
                    {
                        Console.Write("Декодировать в один звуковой файл? (Y/N):");
                    }
                    else
                    {
                        Console.Write("Decode in one voice file? (Y/N):");
                    }

                    if (Console.ReadLine().ToLower() == "y")
                    {
                        one_file = true;
                    }

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    try
                    {
                        try
                        {
                            if (Directory.Exists(CurrentDir + "/output"))
                            {
                                Directory.Delete(CurrentDir + "/output", true);
                            }
                        }
                        catch
                        {
                            Console.WriteLine("Error deleting out folder.");
                        }

                        try
                        {
                            if (Directory.Exists(CurrentDir + "/input"))
                            {
                                Directory.Delete(CurrentDir + "/input", true);
                            }
                        }
                        catch
                        {
                            Console.WriteLine("Error deleting input folder.");
                        }

                        if (!Directory.Exists(CurrentDir + "/output"))
                        {
                            Directory.CreateDirectory(CurrentDir + "/output");
                        }

                        if (!Directory.Exists(CurrentDir + "/input"))
                        {
                            Directory.CreateDirectory(CurrentDir + "/input");
                        }

                        if (playerList.Count > 0)
                        {
                            fullPlayerList.AddRange(playerList);
                        }

                        playerList.Clear();

                        Dictionary<string, string> voice_paths = new Dictionary<string, string>();

                        foreach (var player in fullPlayerList)
                        {
                            if (player.UserName.Length <= 0)
                            {
                                player.UserName = "USER[" + player.iStructCounter + "]";
                            }

                            if (player.voicedata.Count > 0)
                            {
                                var filename = player.UserName + "_[" + player.UserSteamId + "][id_" + player.iSlot + "][user_" + player.ServerUserIdLong + "].wav";

                                filename = Regex.Replace(filename, @"[^\u0000-\u007F]+", "_");

                                foreach (var c in Path.GetInvalidFileNameChars())
                                {
                                    filename = filename.Replace(c, '_');
                                }

                                var inputfile = CurrentDir + "/input/" + filename;
                                var outputfile = CurrentDir + "/output/" + filename;
                                if (one_file)
                                {
                                    outputfile = CurrentDir + "/output/voice.wav";
                                }

                                voice_paths[player.UserName + "[" + player.UserSteamId + "]"] = outputfile;

                                var voice_stream = new FileStream(inputfile, FileMode.Create);
                                var binaryWriter = new BinaryWriter(voice_stream);
                                binaryWriter.BaseStream.Seek(0, SeekOrigin.Begin);

                                binaryWriter.Write((byte)(one_file ? 1 : 0));
                                binaryWriter.Write((byte)VoiceQuality);

                                binaryWriter.Write((float)PlayersVoiceTimer);

                                foreach (var v in player.voicedata)
                                {
                                    binaryWriter.Write((float)v.time);
                                    binaryWriter.Write((int)v.len);
                                    binaryWriter.Write(v.data);
                                }

                                binaryWriter.Flush();
                                voice_stream.Flush();
                                binaryWriter.Close();
                                voice_stream.Close();
                            }
                        }

                        var process = new Process();
                        process.StartInfo.FileName = CurrentDir + "/revoicedecoder.exe";
                        process.StartInfo.WorkingDirectory = CurrentDir;
                        process.Start();
                        process.WaitForExit();

                        bool found_one = false;

                        foreach (var user in voice_paths)
                        {
                            bool found = File.Exists(user.Value);

                            if (found)
                            {
                                found_one = true;
                            }

                            var tmpconsolecolor = Console.ForegroundColor;
                            Console.Write("User \"" + user.Key + "\" :");
                            Console.ForegroundColor = found ? ConsoleColor.Green : ConsoleColor.Red;
                            Console.WriteLine(found ? "Success!" : "Failed!");
                            Console.ForegroundColor = tmpconsolecolor;
                        }

                        if (found_one)
                        {
                            Console.WriteLine("Success players voice decode!");

                            Process.Start(new ProcessStartInfo
                            {
                                FileName = CurrentDir + "/output/",
                                UseShellExecute = true
                            });
                        }
                        else
                        {
                            Console.WriteLine("No voice detected!");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Player voice decode error:" + ex.Message);
                    }
                }

                if (command == "4")
                {
                    try
                    {
                        File.Delete(CurrentDir + "/players.txt");
                        File.AppendAllText(CurrentDir + "/players.txt", "Current players:\n");
                        File.AppendAllText(CurrentDir + "/players.txt", "Local player:" + LocalPlayerId);
                        foreach (var player in playerList)
                        {
                            if (player.UserName.Length > 0)
                            {
                                table = new ConsoleTable("Player:" + player.UserName + "(" + player.iSlot + ")");
                                foreach (var keys in player.InfoKeys)
                                {
                                    table.AddRow(keys.Key + " = " + keys.Value);
                                }

                                table.AddRow("SLOTID = " + player.iSlot);
                                File.AppendAllText(CurrentDir + "/players.txt", table.ToStringAlternative());
                            }
                        }

                        File.AppendAllText(CurrentDir + "/players.txt", "Old players:\n");
                        foreach (var player in fullPlayerList)
                        {
                            if (player.UserName.Length > 0)
                            {
                                table = new ConsoleTable("Player:" + player.UserName + "(" + player.iSlot + ")");
                                foreach (var keys in player.InfoKeys)
                                {
                                    table.AddRow(keys.Key + " = " + keys.Value);
                                }

                                table.AddRow("SLOTID = " + player.iSlot);
                                File.AppendAllText(CurrentDir + "/players.txt", table.ToStringAlternative());
                            }
                        }

                        Console.WriteLine("All players saved to players.txt");
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = CurrentDir + "/players.txt",
                            UseShellExecute = true
                        });
                    }
                    catch
                    {
                        Console.WriteLine("Error while saving players to txt!");
                    }
                }

                if (command == "1")
                {
                    if (ViewDemoCommentCount > 0)
                    {
                        if (File.Exists(CurrentDemoFilePath.Remove(CurrentDemoFilePath.Length - 3) + "cdb"))
                        {
                            if (File.Exists(CurrentDemoFilePath.Remove(CurrentDemoFilePath.Length - 3) + "cdb.bak"))
                            {
                                try
                                {
                                    File.Delete(CurrentDemoFilePath.Remove(CurrentDemoFilePath.Length - 3) + "cdb.bak");
                                }
                                catch
                                {
                                    Console.WriteLine("Error: No access to VDH log path.");
                                }
                            }

                            try
                            {
                                File.Delete(CurrentDemoFilePath.Remove(CurrentDemoFilePath.Length - 3) + "cdb");
                            }
                            catch
                            {
                                Console.WriteLine("File " + CurrentDemoFilePath.Remove(CurrentDemoFilePath.Length - 3) +
                                                  "cdb" + " open error : No access to remove!");
                                Console.Write("No access to file... Try again!");
                                Console.ReadKey();
                                return;
                            }
                        }

                        try
                        {
                            var binaryReader = new BinaryReader(ViewDemoHelperComments.BaseStream);
                            ViewDemoHelperComments.BaseStream.Seek(0, SeekOrigin.Begin);
                            var comments = binaryReader.ReadBytes((int)binaryReader.BaseStream.Length);
                            File.WriteAllBytes(CurrentDemoFilePath.Remove(CurrentDemoFilePath.Length - 3) + "cdb",
                                comments);
                            Console.WriteLine("View Demo Helper comments saved");
                        }
                        catch
                        {
                            Console.WriteLine("Can't write comments!");
                        }
                    }
                    else
                    {
                        Console.WriteLine("No View Demo Helper comments found.");
                    }
                }

                if (command == "10")
                {
                    if (OutTextMessages.Count > 0)
                    {
                        try
                        {
                            var textdatapath = CurrentDemoFilePath.Remove(CurrentDemoFilePath.Length - 4) +
                                               "_msglist.txt";
                            if (File.Exists(textdatapath))
                            {
                                File.Delete(textdatapath);
                            }

                            File.WriteAllLines(textdatapath, OutTextMessages.ToArray());
                            Console.WriteLine("Text comments saved");
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = textdatapath,
                                UseShellExecute = true
                            });
                        }
                        catch
                        {
                            Console.WriteLine("Can't write messages!");
                        }
                    }
                    else
                    {
                        Console.WriteLine("No text messages found.");
                    }
                }

                if (command == "2")
                {
                    if (OutTextDetects.Count > 0)
                    {
                        try
                        {
                            var textdatapath = CurrentDemoFilePath.Remove(CurrentDemoFilePath.Length - 3) + "txt";
                            if (File.Exists(textdatapath))
                            {
                                File.Delete(textdatapath);
                            }

                            File.WriteAllLines(textdatapath, OutTextDetects.ToArray());
                            Console.WriteLine("Text comments saved");

                            Process.Start(new ProcessStartInfo
                            {
                                FileName = textdatapath,
                                UseShellExecute = true
                            });
                        }
                        catch
                        {
                            Console.WriteLine("Can't write text detects!");
                        }
                    }
                    else
                    {
                        Console.WriteLine("No text detects found.");
                    }
                }

                if (command == "3")
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("Demo information / Дополнительная информация о демке");
                    //  Console.WriteLine("Min angles:" + MinFrameViewanglesX + " " + MinFrameViewanglesY);
                    table = new ConsoleTable("ТИП/TYPE", "ПРЫЖКИ/JUMPS", "АТАКА/ATTACKS");
                    table
                        .AddRow(1, JumpCount, attackscounter)
                        .AddRow(2, JumpCount2, attackscounter3)
                        .AddRow(3, JumpCount3, attackscounter4)
                        .AddRow(4, JumpCount4, attackscounter5)
                        .AddRow(5, JumpCount5, attackscounter6)
                        .AddRow(6, JumpCount6, "SKIP");
                    table.Write(Format.Alternative);
                    if (IsRussia)
                    {
                        Console.WriteLine("Reload type 1:" + Reloads);
                        Console.WriteLine("Reload type 2:" + Reloads2);
                        Console.WriteLine("Shots fired:" + attackscounter4);
                        Console.WriteLine("Bad Shots fired:" + attackscounter5);
                        Console.WriteLine("Attack in air:" + AirShots);
                        Console.WriteLine("Teleport count:" + PlayerTeleportus);
                        Console.WriteLine("Fly time: " +
                                          Convert.ToInt32(100.0 / (TotalFramesOnFly + TotalFramesOnGround) *
                                                          TotalFramesOnFly) + "%");
                        Console.WriteLine("Attack in fly: " +
                                          Convert.ToInt32(100.0 / (TotalFramesOnFly + TotalFramesOnGround) *
                                                          TotalAttackFramesOnFly) + "%");
                    }
                    else
                    {
                        Console.WriteLine("Перезарядка (тип 1 / тип 2):" + Reloads + "/" + Reloads2);
                        Console.WriteLine("Выстрелов (количество/не обработанных):" + attackscounter4 + "/" +
                                          attackscounter5);
                        Console.WriteLine("Выстрелов в воздухе:" + AirShots);
                        Console.WriteLine("Количество респавнов(или телепортов):" + PlayerTeleportus);
                        Console.WriteLine("Время в полете: " +
                                          Convert.ToInt32(100.0 / (TotalFramesOnFly + TotalFramesOnGround) *
                                                          TotalFramesOnFly) + "%");
                        Console.WriteLine("Процент атаки в полете: " +
                                          Convert.ToInt32(100.0 / (TotalFramesOnFly + TotalFramesOnGround) *
                                                          TotalAttackFramesOnFly) + "%");
                    }

                    table = new ConsoleTable("УБИЙСТВ /KILLS", "СМЕРТЕЙ/DEATHS");
                    table.AddRow(KillsCount, DeathsCoount);
                    table.Write(Format.Alternative);
                    Console.WriteLine("Calculated FPS / Подсчитанный FPS");
                    table = new ConsoleTable("Максимальный FPS / FPS MAX", "Минимальная зареджка/Min Delay",
                        "Средний FPS/Average FPS");
                    table.AddRow(
                        Math.Round(FrametimeMin, 5) + "(" + Math.Round(1000.0f / (1000.0f * FrametimeMin), 5) + " FPS)",
                        MsecMin + "(" + Math.Round(1000.0f / MsecMin, 5) + " FPS)",
                        averagefps2.Count > 2 ? averagefps2.Average().ToString() : "UNKNOWN");
                    table.Write(Format.Alternative);
                    table = new ConsoleTable("Минимальный FPS / FPS MIN", "Максимальная зареджка/Max Delay",
                        "Средний FPS/Average FPS");
                    table.AddRow(
                        Math.Round(FrametimeMax, 5) + "(" + Math.Round(1000.0f / (1000.0f * FrametimeMax), 5) + " FPS)",
                        MsecMax + "(" + Math.Round(1000.0f / MsecMax, 5) + " FPS)",
                        averagefps.Count > 2 ? averagefps.Average().ToString() : "UNKNOWN");
                    table.Write(Format.Alternative);
                    try
                    {
                        //if (new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).Length < 1000000)
                        //{
                        Console.WriteLine("Real FPS / Реальный FPS");
                        table = new ConsoleTable("Минимальный FPS / FPS MIN", "Максимальный FPS / FPS MAX");
                        table.AddRow(RealFpsMin == int.MaxValue ? "UNKNOWN" : RealFpsMin.ToString(),
                            RealFpsMax == int.MinValue ? "UNKNOWN" : RealFpsMax.ToString());
                        table.AddRow(RealFpsMin2 == int.MaxValue ? "UNKNOWN" : RealFpsMin2.ToString(),
                            RealFpsMax2 == int.MinValue ? "UNKNOWN" : RealFpsMax2.ToString());
                        table.Write(Format.Alternative);
                        //}
                    }
                    catch
                    {
                        Console.WriteLine("Error access write FPS info!");
                    }

                    if (playerresolution.Count > 0)
                    {
                        table = new ConsoleTable("Display (Разрешение экрана)");
                        foreach (var s in playerresolution)
                        {
                            table.AddRow(s.x + "x" + s.y);
                        }

                        table.Write(Format.Alternative);
                    }

                    if (CurrentFrameIdAll > 0)
                    {
                        Console.WriteLine("Frames(всего кадров): " + CurrentFrameIdAll);
                    }

                    if (StuckFrames > 10)
                    {
                        if (IsRussia)
                        {
                            DemoScanner_AddInfo("Залипаний из-за высокого фпс [msec == 0]: " + StuckFrames + ".");
                        }
                        else
                        {
                            DemoScanner_AddInfo("Stuck frames [msec == 0]: " + StuckFrames + ".");
                        }
                    }

                    if (LostStopAttackButton > 4)
                    {
                        DemoScanner_AddInfo("Lost -attack commands: " + LostStopAttackButton + ".");
                    }

                    if (NeedIgnoreAttackFlagCount > 0)
                    {
                        DemoScanner_AddInfo("Lost +attack commands: " + NeedIgnoreAttackFlagCount + ".");
                    }

                    if (ModifiedDemoFrames > 0)
                    {
                        DemoScanner_AddInfo("Possible demoscanner bypass. Tried to kill frames:" + ModifiedDemoFrames);
                    }

                    if (demo_skipped_frames > 0)
                    {
                        DemoScanner_AddInfo("Skipped frames:" + demo_skipped_frames);
                    }

                    Console.WriteLine("Maximum time between frames:" + MaximumTimeBetweenFrames.ToString("F6"));
                    Console.WriteLine("ServerName:" + ServerName);
                    Console.WriteLine("MapName:" + MapName);
                    Console.WriteLine("GameDir:" + GameDir);

                    if (sv_minrate != -1)
                    {
                        Console.WriteLine("[PLUGIN] sv_minrate = " + sv_minrate);
                        Console.WriteLine("[PLUGIN] sv_maxrate = " + sv_maxrate);
                        Console.WriteLine("[PLUGIN] sv_minupdaterate = " + sv_minupdaterate);
                        Console.WriteLine("[PLUGIN] sv_maxupdaterate = " + sv_maxupdaterate);
                    }

                    Console.WriteLine("Download Location:");
                    Console.WriteLine(DownloadLocation);
                    Console.WriteLine("DealthMatch:" + DealthMatch);
                    if (playerList.Count > 0)
                    {
                        Console.WriteLine("Players: " + playerList.Count);
                    }

                    Console.WriteLine("Codecname:" + VoiceCodec);

                    if (CheatKey > 0)
                    {
                        Console.WriteLine("Possible press cheat key " + CheatKey + " times. (???)");
                    }

                    if (FrameDuplicates > 0)
                    {
                        Console.WriteLine("Duplicate frames(Same frame): " + FrameDuplicates);
                    }

                    if (attackscounter4 > 50 && attackscounter5 > attackscounter4 / 2)
                    {
                        if (IsRussia)
                        {
                            DemoScanner_AddInfo("Странные данные: " + attackscounter5 + " неправильных выстрелов из " +
                                                attackscounter4);
                        }
                        else
                        {
                            DemoScanner_AddInfo("Just strange info: " + attackscounter5 + " strange shoots. Total " +
                                                attackscounter4);
                        }
                    }

                    if ((averagefps.Count > 0 && averagefps.Average() > MAX_MONITOR_REFRESHRATE) ||
                        (averagefps2.Count > 0 && averagefps2.Average() > MAX_MONITOR_REFRESHRATE))
                    {
                        if (IsRussia)
                        {
                            DemoScanner_AddInfo("Игрок играет с высоким фпс: " + RealFpsMax);
                        }
                        else
                        {
                            DemoScanner_AddInfo("Player playing with non default fps. Max fps: " + RealFpsMax);
                        }
                    }

                    if (IsRussia)
                    {
                        if (PlayerSensUsageList.Count > 1)
                        {
                            Console.WriteLine("Player 'sensitivity' cvar: " +
                                              (PlayerSensUsageList[0].sens / 0.022).ToString("F2") + "(or " +
                                              (PlayerSensUsageList[1].sens / 0.022).ToString("F2") + ")");
                        }
                        else if (PlayerSensUsageList.Count == 1)
                        {
                            Console.WriteLine("Player 'sensitivity' cvar: " +
                                              (PlayerSensUsageList[0].sens / 0.022).ToString("F2"));
                        }
                        else
                        {
                            Console.WriteLine("Can't detect player 'sensitivity' cvar!");
                        }
                    }
                    else
                    {
                        if (PlayerSensUsageList.Count > 1)
                        {
                            Console.WriteLine("Квар 'sensitivity'(сенс) игрока: " +
                                              (PlayerSensUsageList[0].sens / 0.022).ToString("F2") + "(or " +
                                              (PlayerSensUsageList[1].sens / 0.022).ToString("F2") + ")");
                        }
                        else if (PlayerSensUsageList.Count == 1)
                        {
                            Console.WriteLine("Квар 'sensitivity'(сенс) игрока: " +
                                              (PlayerSensUsageList[0].sens / 0.022).ToString("F2"));
                        }
                        else
                        {
                            Console.WriteLine("Не удалось определить 'sensitivity' игрока.");
                        }
                    }


                    if (!IsRussia)
                    {
                        Console.WriteLine("[SERVER DIAGNOSTIC DATA]");
                        Console.WriteLine("Max input bytes per second: " + MaxBytesPerSecond);
                        if (BytesPerSecond.Count > 1)
                        {
                            Console.WriteLine("Average input bytes per second: " + (int)BytesPerSecond.Average());
                        }
                        Console.WriteLine("Max channel overflows (100kbytes rate): " + MsgOverflowSecondsCount);
                        Console.WriteLine("Max HUD messages per seconds: " + MaxHudMsgPerSecond);
                        Console.WriteLine("Max DHUD messages per seconds: " + MaxDHudMsgPerSecond);
                        Console.WriteLine("Max PRINT messages per seconds: " + MaxPrintCmdMsgPerSecond);
                        Console.WriteLine("Max STUFFCMD messages per seconds: " + MaxStuffCmdMsgPerSecond);
                        Console.WriteLine("Server lags found(DROP FPS): " + ServerLagCount);
                        Console.WriteLine("Loss count: " + LossPackets);
                        Console.WriteLine("Frameskip count: " + LossPackets2);
                        Console.WriteLine("Choke count(small sv_minrate): " + ChokePackets);
                        Console.WriteLine("[END SERVER DIAGNOSTIC DATA]");
                    }
                    else
                    {
                        Console.WriteLine("[БЛОК ДАННЫХ ДЛЯ ДИАГНОСТИКИ СЕРВЕРА]");
                        Console.WriteLine("Максимальное количество байт в секунду: " + MaxBytesPerSecond);
                        if (BytesPerSecond.Count > 1)
                        {
                            Console.WriteLine("Среднее количество байт в секунду: " + (int)BytesPerSecond.Average());
                        }
                        Console.WriteLine("Количество перегрузок канала (100кбайт): " + MsgOverflowSecondsCount);
                        Console.WriteLine("Количество HUD сообщений в секунду: " + MaxHudMsgPerSecond);
                        Console.WriteLine("Количество DHUD сообщений в секунду: " + MaxDHudMsgPerSecond);
                        Console.WriteLine("Количество PRINT сообщений в секунду: " + MaxPrintCmdMsgPerSecond);
                        Console.WriteLine("Количество STUFFCMD сообщений в секунду: " + MaxStuffCmdMsgPerSecond);
                        Console.WriteLine("Количество подвисаний сервера (низкий фпс): " + ServerLagCount);
                        Console.WriteLine("Игрок отправил потерянных пакетов: " + LossPackets);
                        Console.WriteLine("Игрок сделал пропуск кадров: " + LossPackets2);
                        Console.WriteLine("Количество CHOKE(слишком низкий sv_minrate): " + ChokePackets);
                        Console.WriteLine("[КОНЕЦ БЛОКА ДАННЫХ ДИАГНОСТИКИ СЕРВЕРА]");
                    }

                    ForceFlushScanResults();
                }
            }
        }

        public static void UpdateThreadState(int thredid, int state)
        {
            lock (sync)
            {
                myThreadStates[thredid] = state;
            }
        }

        public static int GetActiveDownloadThreads()
        {
            var cnt = 0;
            lock (sync)
            {
                for (var i = 0; i < myThreadStates.Length; i++)
                {
                    if (abs(Environment.TickCount - myThreadStates[i]) < 5000)
                    {
                        cnt++;
                    }
                }
            }

            return cnt;
        }
        public static string RemoveCtrlChars(string str)
        {
            return new string(str.Where(c => !char.IsControl(c)).ToArray());
        }

        private static void AddLerpAndMs(int lerpMsec, byte msec)
        {
            while (historyUcmdLerpAndMs.Count > 15)
            {
                historyUcmdLerpAndMs.RemoveAt(0);
            }

            var tmpUcmdLerpAndMs = new UcmdLerpAndMs { lerp = lerpMsec, msec = msec };
            //Console.WriteLine("add.lerpms:" + lerpMsec + ", add.ms:" + msec);
            historyUcmdLerpAndMs.Add(tmpUcmdLerpAndMs);
        }

        public static bool FindLerpAndMs(int lerpMsec, byte msec, bool old_variant = true)
        {
            if (old_variant)
            {
                if (lerpMsec > 0 || msec > 0)
                {
                    return true;
                }
            }

            foreach (var v in historyUcmdLerpAndMs)
            {
                if (v.lerp < 0 || v.lerp == lerpMsec /* && (v.msec == msec || v.msec == Convert.ToByte(msec * 2))*/)
                {
                    return true;
                }
            }

            return false;
        }

        public static DateTime Trim(this DateTime date, long ticks)
        {
            return new DateTime(date.Ticks - date.Ticks % ticks, date.Kind);
        }

        public static void ForceFlushScanResults()
        {
            UpdateWarnList(true);
        }

        public static bool IsRoundEnd()
        {
            return abs(CurrentTime - RoundEndTime) < 3.0f;
        }

        private static double truncate(double x)
        {
            return x > 0.0 ? Math.Floor(x) : -Math.Floor(-x);
        }

        private static double MyFmod(double x, double y)
        {
            return x - truncate(x / y) * y;
        }

        private static double normalizeangle(double angle)
        {
            return MyFmod(angle, 360.0);
        }

        public static float give_bad_float(float val)
        {
            uint shift = (1 << 16);
            uint mask = shift - 1;

            int d = (int)(shift * (val % 360.0f)) / 360;
            d &= (int)mask;
            return (float)(d * (360.0f / (1 << 16)));
        }

        private static float fullnormalizeangle(float angle)
        {
            var retval = Convert.ToSingle(MyFmod(angle, 360.0));
            return retval < 0.0 ? retval + 360.0f : retval;
        }

        public static float AngleBetween(double angle1, double angle2)
        {
            if (Math.Abs(angle1 - angle2) <= EPSILON)
            {
                return 0.0f;
            }

            var newangle1 = normalizeangle(angle1);
            var newangle2 = normalizeangle(angle2);
            var anglediff = normalizeangle(newangle1 - newangle2);
            if (360.0 - anglediff < anglediff)
            {
                anglediff = 360.0 - anglediff;
            }

            var retval = abs(Convert.ToSingle(anglediff));
            if (retval <= EPSILON)
            {
                return 0.0f;
            }

            return retval;
        }

        public static float AngleBetweenSigned(double angle1, double angle2)
        {
            if (Math.Abs(angle1 - angle2) <= EPSILON)
            {
                return 0.0f;
            }

            var newangle1 = normalizeangle(angle1);
            var newangle2 = normalizeangle(angle2);
            var anglediff = normalizeangle(newangle1 - newangle2);
            if (360.0 - anglediff < anglediff)
            {
                anglediff = 360.0 - anglediff;
            }

            var retval = Convert.ToSingle(anglediff);
            if (retval <= EPSILON)
            {
                return 0.0f;
            }

            return retval;
        }

        public static string GetSourceCodeString()
        {
            usagesrccode++;
            return SourceCode.Length != 51 ? throw new Exception("ANAL ERROR") : SourceCode;
        }

        private static void ParseGameData(HalfLifeDemoParser halfLifeDemoParser, byte[] msgBytes)
        {
            halfLifeDemoParser.ParseGameDataMessages(msgBytes);
        }

        public static float abs(float val)
        {
            return Math.Abs(val);
        }

        public static bool isAngleInPunchListY(float angle)
        {
            // Console.WriteLine("SearchAngle:" + angle);
            if (LastPunchAngleY.Count < 6)
            {
                return true;
            }

            if (!(CurrentWeapon == WeaponIdType.WEAPON_ELITE || CurrentWeapon == WeaponIdType.WEAPON_USP ||
                  CurrentWeapon == WeaponIdType.WEAPON_AWP || CurrentWeapon == WeaponIdType.WEAPON_SCOUT ||
                  CurrentWeapon == WeaponIdType.WEAPON_DEAGLE || CurrentWeapon == WeaponIdType.WEAPON_P228 ||
                  CurrentWeapon == WeaponIdType.WEAPON_FIVESEVEN || CurrentWeapon == WeaponIdType.WEAPON_XM1014 ||
                  CurrentWeapon == WeaponIdType.WEAPON_M3 || CurrentWeapon == WeaponIdType.WEAPON_NONE ||
                  CurrentWeapon == WeaponIdType.WEAPON_KNIFE || CurrentWeapon == WeaponIdType.WEAPON_HEGRENADE ||
                  CurrentWeapon == WeaponIdType.WEAPON_FLASHBANG || CurrentWeapon == WeaponIdType.WEAPON_SMOKEGRENADE ||
                  CurrentWeapon == WeaponIdType.WEAPON_BAD2 || CurrentWeapon == WeaponIdType.WEAPON_BAD))
            {
                return true;
            }

            foreach (var f in LastPunchAngleY)
            {
                if (AngleBetween(angle, f) < 0.015f)
                {
                    return true;
                }
            }

            return false;
        }

        public static void addAngleInPunchListY(float angle)
        {
            // Console.WriteLine("addAngleInPunchListY:" + angle);
            if (LastPunchAngleY.Count > 8)
            {
                LastPunchAngleY.RemoveAt(0);
            }

            LastPunchAngleY.Add(angle);
        }

        public static void addAngleInPunchListX(float angle)
        {
            //Console.WriteLine("addAngleInPunchListX:" + angle);
            if (LastPunchAngleX.Count > 5)
            {
                LastPunchAngleX.RemoveAt(0);
            }

            LastPunchAngleX.Add(angle);
        }

        public static void addAngleInViewListYDelta(float angle)
        {
            // Console.WriteLine("addAngleInViewListY:" + angle);
            if (LastSearchViewAngleY.Count > 25)
            {
                LastSearchViewAngleY.RemoveAt(0);
            }

            LastSearchViewAngleY.Add(angle);
        }

        public static bool isAngleInViewListYDelta(float angle)
        {
            foreach (var f in LastSearchViewAngleY)
            {
                if (f < -1000.0f || AngleBetween(angle, f) < 0.0001f)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsPlayerLossConnection()
        {
            var retcheck = abs(CurrentTime - LastLossPacket);
            var retcheck2 = abs(CurrentTime - LastLossTimeEnd);
            var retval = retcheck < 1.0f || retcheck2 < 1.5f;
            // if (retval)
            //      Console.WriteLine("CurrentTime:" + CurrentTime + ". LastLossPacket:" + LastLossPacket + ". LastLossTimeEnd:" + LastLossTimeEnd);
            return retval;
        }

        public static bool IsPlayerLossConnection(float CurrTime)
        {
            var retcheck = abs(CurrTime - LastLossPacket);
            var retcheck2 = abs(CurrTime - LastLossTimeEnd);
            var retval = retcheck < 1.0f || retcheck2 < 1.5f;
            //if (retval)
            //    Console.WriteLine("CurrentTime:" + CurrentTime + ". LastLossPacket:" + LastLossPacket + ". LastLossTimeEnd:" + LastLossTimeEnd);
            return retval;
        }

        public static bool IsHookDetected()
        {
            var retcheck = abs(CurrentTime - LastBeamFound);
            var retval = retcheck < 5.0f;
            /*if (retval)
                Console.WriteLine("CurrentTime:" + CurrentTime + ". LastBeamFound:" + LastBeamFound);*/
            return retval;
        }

        public static bool IsBigVelocity()
        {
            var retcheck = abs(CurrentTime - FoundBigVelocityTime);
            var retval = retcheck < 2.0f;
            /* if (retval)
                 Console.WriteLine("CurrentTime:" + CurrentTime + ". FoundBigVelocityTime:" + FoundBigVelocityTime);*/
            return retval;
        }

        public static bool IsVelocity()
        {
            var retcheck = abs(CurrentTime - FoundVelocityTime);
            var retval = retcheck < 2.0f;
            /* if (retval)
                 Console.WriteLine("CurrentTime:" + CurrentTime + ". FoundBigVelocityTime:" + FoundBigVelocityTime);*/
            return retval;
        }

        public static bool IsViewChanged()
        {
            var retcheck = abs(CurrentTime - LastViewChange);
            var retval = retcheck < 1.5f;
            /*if (retval)
                Console.WriteLine("CurrentTime:" + CurrentTime + ". LastViewChange:" + LastViewChange);*/
            return retval;
        }

        public static bool IsTakeDamage(float val = 0.50f)
        {
            var retcheck = abs(CurrentTime - LastDamageTime);
            var retval = retcheck < val && retcheck >= 0.0f;
            /* if (retval)
                 Console.WriteLine("CurrentTime:" + CurrentTime + ". LastDamageTime:" + LastDamageTime);*/
            return retval;
        }

        public static bool IsPlayerFrozen()
        {
            var retcheck = abs(CurrentTime - PlayerUnFrozenTime);
            var retval = PlayerFrozen || retcheck < 4.5f;
            /* if (retval)
                 Console.WriteLine("CurrentTime:" + CurrentTime + ". PlayerUnFrozenTime:" + PlayerUnFrozenTime);*/
            return retval;
        }

        public static bool IsPlayerTeleport()
        {
            var retcheck = abs(CurrentTime - LastTeleportusTime);
            var retval = retcheck < 0.50f;
            /*if (retval)
                Console.WriteLine("CurrentTime:" + CurrentTime + ". LastTeleportusTime:" + LastTeleportusTime);*/
            return retval;
        }

        public static bool IsPlayerAttackedPressed()
        {
            var retcheck = abs(CurrentTime - LastAttackPressed);
            var retval = retcheck < 2.30f;
            retcheck = abs(CurrentTime - LastAttackStartStopCmdTime);
            retval = retval || retcheck < 2.30f;
            /*if (retval)
                Console.WriteLine("CurrentTime:" + CurrentTime + ". LastAttackPressed:" + LastAttackPressed);
            */
            return retval;
        }

        public static bool IsPlayerBtnAttackedPressed()
        {
            var retcheck = abs(CurrentTime - LastAttackStartStopCmdTime);
            var retval = retcheck < 0.75f;
            /*if (retval)
                Console.WriteLine("CurrentTime:" + CurrentTime + ". LastAttackCmdTime:" + LastAttackCmdTime);
            */
            return retval;
        }

        public static bool IsPlayerBtnJumpPressed()
        {
            var retcheck = abs(CurrentTime - LastJumpBtnTime);
            var retval = retcheck < 2.00f;
            /*if (retval)
                Console.WriteLine("CurrentTime:" + CurrentTime + ". LastJumpBtnTime:" + LastJumpBtnTime);
            */
            return retval;
        }

        public static bool IsPlayerAnyJumpPressed()
        {
            var retcheck = abs(CurrentTime - LastJumpBtnTime);
            var retval = retcheck < 2.0f;
            retcheck = abs(CurrentTime - LastJumpTime);
            retval = retval || retcheck < 2.00f;
            /*if (retval)
                Console.WriteLine("CurrentTime:" + CurrentTime + ". LastJumpTime:" + LastJumpBtnTime);
            */
            return retval;
        }

        public static bool IsPlayerInDuck()
        {
            var retcheck = abs(CurrentTime - LastCmdDuckTime);
            var retval = retcheck < 1.02f;
            /*
             Console.WriteLine("CurrentTime:" + CurrentTime + ". LastDuckUnduckTime:" + LastDuckUnduckTime);
             */
            return retval;
        }

        public static bool IsPlayerUnDuck()
        {
            var retcheck = abs(CurrentTime - LastCmdUnduckTime);
            var retval = retcheck < 0.21f;
            /*
             Console.WriteLine("CurrentTime:" + CurrentTime + ". LastDuckUnduckTime:" + LastDuckUnduckTime);
             */
            return retval;
        }

        public static bool IsAngleEditByEngine()
        {
            return (!NO_TELEPORT && (IsPlayerTeleport() || abs(CurrentTime - LastAngleManipulation) < 0.50f ||
                                     IsTakeDamage() || IsPlayerFrozen() ||
                                     IsViewChanged() || HideWeapon || abs(CurrentTime - LastLookDisabled) < 0.75f)) ||
                   abs(CurrentTime - HorAngleTime) < 0.15;
        }

        public static bool IsValidMovement()
        {
            return !((!NO_TELEPORT && (IsPlayerTeleport() || abs(CurrentTime - LastAngleManipulation) < 0.50f ||
                                       IsTakeDamage() || IsPlayerFrozen() || IsViewChanged() || HideWeapon ||
                                       abs(CurrentTime - LastLookDisabled) < 0.75f)) ||
                     abs(CurrentTime - HorAngleTime) < 0.15);
        }

        public static void VectorsToAngles(FPoint3D fwd, FPoint3D right, FPoint3D up, ref FPoint3D angles)
        {
            var sp = -fwd.Z;
            angles.X = (float)(Math.Asin(sp) * (180.0 / Math.PI));
            angles.Y = (float)(Math.Atan2(fwd.Y, fwd.X) * (180.0 / Math.PI));
            var cp = (float)Math.Cos(angles.X * (Math.PI / 180.0));
            var cr = up.Z / cp;
            angles.Z = (float)(Math.Acos(cr) * (180.0 / Math.PI));
        }

        public static AngleDirection GetAngleDirection(float val1, float val2)
        {
            var retval = AngleDirection.AngleDirectionNO;
            if (abs(val1 - val2) > EPSILON)
            {
                retval = AngleBetweenSigned(val1, val2) > AngleBetweenSigned(val2, val1)
                    ? AngleDirection.AngleDirectionLeft
                    : AngleDirection.AngleDirectionRight;
            }

            return retval;
        }

        public static void ProcessPluginMessage(string cmd)
        {
            if (DUMP_ALL_FRAMES)
            {
                OutDumpString += "\n{ UCMD PLUGIN MESSAGE:" + (cmd.Length > 0 ? cmd : "INVALID MSG!") + "}\n";
            }

            if (cmd.Length <= 0)
            {
                if (abs(CurrentTime - LastCmdHack) > 10.0f)
                {
                    DemoScanner_AddWarn("[PLUGINHACK TYPE 1.1] at (" + CurrentTime + ") " + CurrentTimeString, true, true, true, true);
                    LastCmdHack = CurrentTime;
                }
                return;
            }

            var cmdList = cmd.Split('/');
            if (cmdList.Length > 0)
            {
                if (cmdList[0] == "UDS")
                {
                    if (cmdList.Length > 2)
                    {
                        if (cmdList[1] == "AUTH")
                        {
                            if (SteamID.Length == 0)
                            {
                                plugin_valid_packets++;
                                SteamID = cmdList[2];
                                DemoScanner_AddInfo("[INFO] SteamID игрока: " + SteamID, true);
                            }
                        }
                        else if (cmdList[1] == "DATE")
                        {
                            if (RecordDate.Length == 0)
                            {
                                plugin_valid_packets++;
                                RecordDate = cmdList[2];
                                if (IsRussia)
                                {
                                    DemoScanner_AddInfo("[INFO] Дата записи: " + RecordDate, true);
                                }
                                else
                                {
                                    DemoScanner_AddInfo("[INFO] Record date: " + RecordDate, true);
                                }
                            }
                        }
                        else if (cmdList[1] == "VER" || cmdList[1] == "UCMD")
                        {
                            if (PluginVersion.Length == 0)
                            {
                                plugin_valid_packets++;
                                PluginVersion = cmdList[1] == "UCMD" ? "1.5" : cmdList[2];
                                flPluginVersion = 0.0f;
                                float.TryParse(PluginVersion, NumberStyles.Any, CultureInfo.InvariantCulture, out flPluginVersion);

                                if (IsRussia)
                                {
                                    DemoScanner_AddInfo("[INFO] Найден демо-плагин версии " + PluginVersion, true);
                                }
                                else
                                {
                                    DemoScanner_AddInfo("[INFO] Found module version " + PluginVersion, true);
                                }

                                if (flPluginVersion < 1.59)
                                {
                                    if (IsRussia)
                                    {
                                        DemoScanner_AddWarn("[INFO] На сервере установлена старая версия плагина [" + PluginVersion + "]", true, false,
                                            true, true);
                                        DemoScanner_AddWarn(
                                            "[ERROR] Отключается обнаружение [JUMPHACK TYPE 5][AIM TYPE 1.6][AIM TYPE 12.X]", true,
                                            false, true, true);
                                    }
                                    else
                                    {
                                        DemoScanner_AddWarn("[INFO] Old plugin version installed at server [" + PluginVersion + "]", true, false, true,
                                            true);
                                        DemoScanner_AddWarn(
                                            "[ERROR] Disabled detection of [JUMPHACK TYPE 5][AIM TYPE 1.6][AIM TYPE 12.X]", true,
                                            false, true, true);
                                    }

                                    DisableJump5AndAim16 = true;
                                }
                            }
                        }
                        else if (cmdList[1] == "JMP")
                        {
                            plugin_valid_packets++;
                            if (PluginVersion.Length != 0)
                            {
                                var id = int.Parse(cmdList[2]);
                                if (IsUserAlive())
                                {
                                    JumpCount6++;
                                }

                                //Console.WriteLine("PluginJmpTime = " + LastJumpNoGroundTime);
                                //Console.WriteLine(JumpCount6.ToString());
                                if (JumpCount6 > 1)
                                {
                                    SearchJumpHack5 = 4;
                                }
                                PluginJmpTime = CurrentTime;
                            }
                        }
                        else if (cmdList[1] == "SCMD")
                        {
                            plugin_valid_packets++;
                            if (PluginVersion.Length != 0 && !DisableJump5AndAim16)
                            {
                                if (IsUserAlive())
                                {
                                    attackscounter6++;
                                }

                                var lerpms = int.Parse(cmdList[2]);
                                var ms = Convert.ToByte(int.Parse(cmdList[3]));
                                var incomingframenum = int.Parse(cmdList[4]);

                                LastSCMD_Angles1 = new float[] { float.Parse(cmdList[5], NumberStyles.Any,
                      CultureInfo.InvariantCulture), float.Parse(cmdList[6], NumberStyles.Any,
                      CultureInfo.InvariantCulture) };
                                LastSCMD_Angles1[0] = fullnormalizeangle(LastSCMD_Angles1[0]);
                                LastSCMD_Angles1[1] = fullnormalizeangle(LastSCMD_Angles1[1]);

                                LastSCMD_Angles2 = new float[] { float.Parse(cmdList[7], NumberStyles.Any,
                      CultureInfo.InvariantCulture), float.Parse(cmdList[8], NumberStyles.Any,
                      CultureInfo.InvariantCulture) };

                                LastSCMD_Angles2[0] = fullnormalizeangle(LastSCMD_Angles2[0]);
                                LastSCMD_Angles2[1] = fullnormalizeangle(LastSCMD_Angles2[1]);

                                LastSCMD_Angles3 = new float[] { float.Parse(cmdList[9], NumberStyles.Any,
                      CultureInfo.InvariantCulture), float.Parse(cmdList[10], NumberStyles.Any,
                      CultureInfo.InvariantCulture) };

                                LastSCMD_Angles3[0] = fullnormalizeangle(LastSCMD_Angles3[0]);
                                LastSCMD_Angles3[1] = fullnormalizeangle(LastSCMD_Angles3[1]);

                                LastSCMD_Msec1 = float.Parse(cmdList[11], NumberStyles.Any,
                      CultureInfo.InvariantCulture);
                                LastSCMD_Msec2 = float.Parse(cmdList[12], NumberStyles.Any,
                      CultureInfo.InvariantCulture);

                                //Console.WriteLine("SCMD");
                                //Console.WriteLine(LastSCMD_Angles1[0].ToString() + "/" + LastSCMD_Angles1[1].ToString() + " " +
                                //   LastSCMD_Angles2[0].ToString() + "/" + LastSCMD_Angles2[1].ToString() + " " +
                                //    LastSCMD_Angles3[0].ToString() + "/" + LastSCMD_Angles3[1].ToString() + "[" + incomingframenum + "] = " + LastSCMD_Msec1 + "/" + LastSCMD_Msec2);

                                if (abs(CurrentTime - LastAttackStartCmdTime) > 1.0f &&
                                    FirstAttack && IsUserAlive() && !DisableJump5AndAim16)
                                {
                                    // first detection can be false if demo started in +attack
                                    // just skip it
                                    if (!FirstAim16skip)
                                    {
                                        if (DemoScanner_AddWarn(
                                            "[AIM TYPE 1.6 " + CurrentWeapon + "] at (" + CurrentTime + "):" +
                                            CurrentTimeString,
                                            !IsCmdChangeWeapon() && !IsAngleEditByEngine() && !IsPlayerLossConnection() &&
                                            !IsForceCenterView() && IsPlayerBtnAttackedPressed(), true, false, true))
                                        {
                                            TotalAimBotDetected++;
                                        }
                                    }
                                    FirstAim16skip = false;
                                }
                                FirstAttack = true;

                                if (IsUserAlive() && FirstJump && abs(CurrentTime) > EPSILON)
                                {
                                    if (ms <= 1)
                                    {
                                        if (abs(CurrentTime - LastCmdHack) > 5.0 && CurrentFpsSecond < 450 && CurrentFps2Second < 450)
                                        {
                                            DemoScanner_AddWarn(
                                                "[CMD HACK TYPE 3.1] at (" + CurrentTime + ") " + CurrentTimeString,
                                                ms == 0 && !IsAngleEditByEngine());

                                            LastCmdHack = CurrentTime;
                                        }
                                    }
                                }

                                if (PluginFrameNum < 0 || incomingframenum < PluginFrameNum)
                                {
                                    PluginFrameNum = incomingframenum;
                                }
                                else
                                {
                                    if (abs(incomingframenum - PluginFrameNum) > 2)
                                    {
                                        if (abs(CurrentTime - LastCmdHack) > 5.0)
                                        {
                                            DemoScanner_AddWarn(
                                                "[CMD HACK TYPE 5.1] at (" + CurrentTime + ") " + CurrentTimeString,
                                                !IsAngleEditByEngine());
                                            LastCmdHack = CurrentTime;
                                        }
                                    }

                                    PluginFrameNum = incomingframenum;
                                }

                                if (DUMP_ALL_FRAMES)
                                {
                                    OutDumpString += "\n{ ACMD PLUGIN. Lerp " + lerpms + ". ms " + ms + "}\n";
                                }

                                if (!FindLerpAndMs(lerpms, ms))
                                {
                                    DemoScanner_AddWarn("[FAKELAG TYPE 1.1] at (" + CurrentTime + "):" + CurrentTimeString,
                                        true, true, false, true);
                                    FakeLagAim++;
                                }
                                else
                                {
                                    if (!FindLerpAndMs(lerpms, ms, false))
                                    {
                                        FakeLag2Aim++;
                                        if (FakeLag2Aim > 0)
                                        {
                                            DemoScanner_AddWarn(
                                                "[FAKELAG TYPE 1.2] at (" + CurrentTime + "):" + CurrentTimeString, false,
                                                true, false, true);
                                        }
                                    }
                                }
                            }
                            else if (plugin_all_packets > 10)
                            {
                                if (IsRussia)
                                {
                                    DemoScanner_AddWarn("[Ошибка] Не получено приветствие от плагина!", true, false, true,
                                        true, true);
                                }
                                else
                                {
                                    DemoScanner_AddWarn("[Error] No found 'auth' message from module!", true, false, true,
                                        true, true);
                                }

                                if (IsRussia)
                                {
                                    DemoScanner_AddWarn("[WARN] Автоматический сброс версии плагина в 1.60", true, false, true,
                                        true, true);
                                }
                                else
                                {
                                    DemoScanner_AddWarn("[WARN] Auto detect module version set to 1.60", true, false, true,
                                        true, true);
                                }

                                PluginVersion = "1.60";
                                flPluginVersion = 1.60f;
                            }
                        }
                        else if (cmdList[1] == "ACMD")
                        {
                            plugin_valid_packets++;
                            if (PluginVersion.Length != 0 && !DisableJump5AndAim16)
                            {
                                if (IsUserAlive())
                                {
                                    attackscounter6++;
                                }

                                var lerpms = int.Parse(cmdList[2]);
                                var ms = Convert.ToByte(int.Parse(cmdList[3]));
                                var incomingframenum = int.Parse(cmdList[4]);

                                var tmp_ACMD_Angles1 = new float[] { float.Parse(cmdList[5], NumberStyles.Any,
                      CultureInfo.InvariantCulture), float.Parse(cmdList[6], NumberStyles.Any,
                      CultureInfo.InvariantCulture) };

                                tmp_ACMD_Angles1[0] = fullnormalizeangle(tmp_ACMD_Angles1[0]);
                                tmp_ACMD_Angles1[1] = fullnormalizeangle(tmp_ACMD_Angles1[1]);

                                var tmp_ACMD_Angles2 = new float[] { float.Parse(cmdList[7], NumberStyles.Any,
                      CultureInfo.InvariantCulture), float.Parse(cmdList[8], NumberStyles.Any,
                      CultureInfo.InvariantCulture) };

                                tmp_ACMD_Angles2[0] = fullnormalizeangle(tmp_ACMD_Angles2[0]);
                                tmp_ACMD_Angles2[1] = fullnormalizeangle(tmp_ACMD_Angles2[1]);

                                var tmp_ACMD_Angles3 = new float[] { float.Parse(cmdList[9], NumberStyles.Any,
                      CultureInfo.InvariantCulture), float.Parse(cmdList[10], NumberStyles.Any,
                      CultureInfo.InvariantCulture) };

                                tmp_ACMD_Angles3[0] = fullnormalizeangle(tmp_ACMD_Angles3[0]);
                                tmp_ACMD_Angles3[1] = fullnormalizeangle(tmp_ACMD_Angles3[1]);

                                float time_msec = float.Parse(cmdList[11], NumberStyles.Any,
                      CultureInfo.InvariantCulture);

                                //Console.WriteLine("ACMD");
                                //Console.WriteLine(tmp_ACMD_Angles1[0].ToString() + "/" + tmp_ACMD_Angles1[1].ToString() + " " +
                                //   tmp_ACMD_Angles2[0].ToString() + "/" + tmp_ACMD_Angles2[1].ToString() + " " +
                                //    tmp_ACMD_Angles3[0].ToString() + "/" + tmp_ACMD_Angles3[1].ToString() + "[" + incomingframenum + "] = " + time_msec);

                                //Console.WriteLine("CD History: ");
                                //foreach (var v in CDAngleHistoryAim12List)
                                //{
                                //    Console.WriteLine(v.tmp[0] + "/" + v.tmp[1] + "/" + v.tmp[2]);
                                //}

                                bool mir_found = false;

                                foreach (var v in CDAngleHistoryAim12List)
                                {
                                    mir_found = v.tmp[0] == v.tmp[1]
                                    && v.tmp[1] == v.tmp[2] &&
                                    v.tmp[0] == LastSCMD_Angles1[1];
                                    if (mir_found)
                                        break;
                                }

                                if (IsUserAlive() && FirstJump && abs(CurrentTime) > EPSILON)
                                {
                                    if (ms <= 1)
                                    {
                                        if (abs(CurrentTime - LastCmdHack) > 5.0 && CurrentFpsSecond < 450 && CurrentFps2Second < 450)
                                        {
                                            DemoScanner_AddWarn(
                                                "[CMD HACK TYPE 3.2] at (" + CurrentTime + ") " + CurrentTimeString,
                                                ms == 0 && !IsAngleEditByEngine());

                                            LastCmdHack = CurrentTime;
                                        }
                                    }
                                }

                                if (IsUserAlive() && (LastSCMD_Msec1 < EPSILON_2 || LastSCMD_Msec2 < EPSILON_2 || time_msec < EPSILON_2))
                                {
                                    if (/*abs(tmp_SCMD_Angles1[0] - tmp_SCMD_Angles2[0]) > EPSILON_2 &&*/
                                        abs(tmp_ACMD_Angles1[1] - tmp_ACMD_Angles2[1]) > EPSILON_2 &&
                                        /*abs(tmp_SCMD_Angles2[0] - tmp_SCMD_Angles3[0]) > EPSILON_2 &&*/
                                        abs(tmp_ACMD_Angles2[1] - tmp_ACMD_Angles3[1]) > EPSILON_2 &&
                                        /*abs(LastACMD_Angles1[0] - LastACMD_Angles2[0]) > EPSILON_2 &&*/
                                        abs(LastSCMD_Angles1[1] - LastSCMD_Angles2[1]) > EPSILON_2 &&
                                        /*abs(LastACMD_Angles2[0] - LastACMD_Angles3[0]) > EPSILON_2 &&*/
                                        abs(LastSCMD_Angles2[1] - LastSCMD_Angles3[1]) > EPSILON_2)
                                    {
                                        // Console.WriteLine("True Angles 1!");
                                        if (abs(LastSCMD_Angles1[0] - tmp_ACMD_Angles3[0]) < EPSILON_2 &&
                                        abs(LastSCMD_Angles1[1] - tmp_ACMD_Angles3[1]) < EPSILON_2 &&
                                        abs(LastSCMD_Angles2[0] - tmp_ACMD_Angles1[0]) < EPSILON_2 &&
                                        abs(LastSCMD_Angles2[1] - tmp_ACMD_Angles1[1]) < EPSILON_2 &&
                                        abs(LastSCMD_Angles3[0] - tmp_ACMD_Angles2[0]) < EPSILON_2 &&
                                        abs(LastSCMD_Angles3[1] - tmp_ACMD_Angles2[1]) < EPSILON_2)
                                        {
                                            DemoScanner_AddWarn(
                                                "[BETA] [AIM TYPE 12.1 " + CurrentWeapon + "] at (" + CurrentTime +
                                                "):" + CurrentTimeString, abs(CurrentTime - LastBadMsecTime) > 1.0f, mir_found, false, true);
                                        }
                                        else if (abs(LastSCMD_Angles1[0] - tmp_ACMD_Angles3[0]) < EPSILON_2 &&
                                        abs(LastSCMD_Angles1[1] - tmp_ACMD_Angles3[1]) < EPSILON_2 &&
                                        //abs(LastSCMD_Angles2[0] - tmp_ACMD_Angles1[0]) < EPSILON_2 &&
                                        abs(LastSCMD_Angles2[1] - tmp_ACMD_Angles1[1]) < EPSILON_2 &&
                                        abs(LastSCMD_Angles3[0] - tmp_ACMD_Angles2[0]) < EPSILON_2 &&
                                        abs(LastSCMD_Angles3[1] - tmp_ACMD_Angles2[1]) < EPSILON_2)
                                        {
                                            DemoScanner_AddWarn(
                                                "[BETA] [AIM TYPE 12.2 " + CurrentWeapon + "] at (" + CurrentTime +
                                                "):" + CurrentTimeString, abs(CurrentTime - LastBadMsecTime) > 1.0f, mir_found, false, true);
                                        }
                                        else if (abs(LastSCMD_Angles1[0] - tmp_ACMD_Angles3[0]) < EPSILON_2 &&
                                        abs(LastSCMD_Angles1[1] - tmp_ACMD_Angles3[1]) < EPSILON_2 &&
                                        abs(LastSCMD_Angles2[0] - tmp_ACMD_Angles1[0]) < EPSILON_2 &&
                                        abs(LastSCMD_Angles2[1] - tmp_ACMD_Angles1[1]) < EPSILON_2 &&
                                        abs(LastSCMD_Angles3[0] - tmp_ACMD_Angles2[0]) > EPSILON_2 &&
                                        abs(LastSCMD_Angles3[1] - tmp_ACMD_Angles2[1]) > EPSILON_2)
                                        {
                                            DemoScanner_AddWarn(
                                               "[BETA] [AIM TYPE 12.3 " + CurrentWeapon + "] at (" + CurrentTime +
                                               "):" + CurrentTimeString, mir_found && !IsAngleEditByEngine() && !IsPlayerUnDuck() && !IsPlayerUnDuck() && !IsPlayerAnyJumpPressed(), true, false, true);
                                        }
                                    }
                                    if (
                                        abs(LastSCMD_Angles1[0] - LastSCMD_Angles2[0]) > EPSILON_2 &&
                                        abs(LastSCMD_Angles1[1] - LastSCMD_Angles2[1]) > EPSILON_2 &&
                                        abs(LastSCMD_Angles2[0] - LastSCMD_Angles3[0]) > EPSILON_2 &&
                                        abs(LastSCMD_Angles2[1] - LastSCMD_Angles3[1]) > EPSILON_2 &&
                                        abs(tmp_ACMD_Angles1[0] - tmp_ACMD_Angles2[0]) > EPSILON_2 &&
                                        abs(tmp_ACMD_Angles1[1] - tmp_ACMD_Angles2[1]) > EPSILON_2 &&
                                        abs(tmp_ACMD_Angles2[0] - tmp_ACMD_Angles3[0]) < EPSILON_2 &&
                                        abs(tmp_ACMD_Angles2[1] - tmp_ACMD_Angles3[1]) < EPSILON_2
                                        )
                                    {
                                        // Console.WriteLine("True Angles 2!");
                                        if (abs(LastSCMD_Angles1[0] - tmp_ACMD_Angles3[0]) < EPSILON_2 &&
                                        abs(LastSCMD_Angles1[1] - tmp_ACMD_Angles3[1]) < EPSILON_2 &&
                                        /*abs(LastSCMD_Angles1[0] - tmp_ACMD_Angles2[0]) < EPSILON_2 &&
                                        abs(LastSCMD_Angles1[1] - tmp_ACMD_Angles2[1]) < EPSILON_2 &&*/
                                        abs(LastSCMD_Angles2[0] - tmp_ACMD_Angles1[0]) < EPSILON_2 &&
                                        abs(LastSCMD_Angles2[1] - tmp_ACMD_Angles1[1]) < EPSILON_2 &&
                                        abs(LastSCMD_Angles3[0] - tmp_ACMD_Angles2[0]) > EPSILON_2 &&
                                        abs(LastSCMD_Angles3[1] - tmp_ACMD_Angles2[1]) > EPSILON_2 &&
                                        abs(LastSCMD_Angles3[0] - tmp_ACMD_Angles1[0]) > EPSILON_2 &&
                                        abs(LastSCMD_Angles3[1] - tmp_ACMD_Angles1[1]) > EPSILON_2 &&
                                        abs(LastSCMD_Angles3[0] - tmp_ACMD_Angles3[0]) > EPSILON_2 &&
                                        abs(LastSCMD_Angles3[1] - tmp_ACMD_Angles3[1]) > EPSILON_2 &&
                                        abs(tmp_ACMD_Angles2[0] - tmp_ACMD_Angles3[0]) < EPSILON_2 &&
                                        abs(tmp_ACMD_Angles2[1] - tmp_ACMD_Angles3[1]) < EPSILON_2)
                                        {
                                            /*
                                             * DemoScanner_AddWarn(
                                            "[BETA] [AIM TYPE 12.4 " + CurrentWeapon + "] at (" + CurrentTime +
                                            "):" + CurrentTimeString, mir_found && !IsAngleEditByEngine() && !IsPlayerUnDuck() && !IsPlayerUnDuck() && !IsPlayerAnyJumpPressed(), true, false, true);
                                            */
                                        }
                                        else if (abs(LastSCMD_Angles1[0] - tmp_ACMD_Angles2[0]) < EPSILON_2 &&
                                        abs(LastSCMD_Angles1[1] - tmp_ACMD_Angles2[1]) < EPSILON_2 &&
                                        abs(LastSCMD_Angles2[0] - tmp_ACMD_Angles1[0]) < EPSILON_2 &&
                                        abs(LastSCMD_Angles2[1] - tmp_ACMD_Angles1[1]) < EPSILON_2 &&
                                        abs(LastSCMD_Angles3[0] - tmp_ACMD_Angles3[0]) < EPSILON_2 &&
                                        abs(LastSCMD_Angles3[1] - tmp_ACMD_Angles3[1]) < EPSILON_2 &&
                                        abs(LastSCMD_Angles3[0] - tmp_ACMD_Angles2[0]) < EPSILON_2 &&
                                        abs(LastSCMD_Angles3[1] - tmp_ACMD_Angles2[1]) < EPSILON_2 &&
                                        abs(LastSCMD_Angles3[0] - tmp_ACMD_Angles1[0]) > EPSILON_2 &&
                                        abs(LastSCMD_Angles3[1] - tmp_ACMD_Angles1[1]) > EPSILON_2)
                                        {
                                            DemoScanner_AddWarn(
                                                "[BETA] [AIM TYPE 12.5 " + CurrentWeapon + "] at (" + CurrentTime +
                                                "):" + CurrentTimeString, mir_found && abs(CurrentTime - LastBadMsecTime) > 1.0f, true, false, true);
                                        }
                                    }
                                }

                                if (PluginFrameNum < 0 || incomingframenum < PluginFrameNum)
                                {
                                    PluginFrameNum = incomingframenum;
                                }
                                else
                                {
                                    if (abs(incomingframenum - PluginFrameNum) > 2)
                                    {
                                        if (abs(CurrentTime - LastCmdHack) > 5.0)
                                        {
                                            DemoScanner_AddWarn(
                                                "[CMD HACK TYPE 5.2] at (" + CurrentTime + ") " + CurrentTimeString,
                                                !IsAngleEditByEngine());

                                            //Console.WriteLine("BAD BAD " + nf.UCmd.Msec + " / " + nf.RParms.Frametime + " = " + ((float)nf.UCmd.Msec / nf.RParms.Frametime).ToString());
                                            LastCmdHack = CurrentTime;
                                        }
                                    }

                                    PluginFrameNum = incomingframenum;
                                }

                                if (DUMP_ALL_FRAMES)
                                {
                                    OutDumpString += "\n{ ACMD PLUGIN. Lerp " + lerpms + ". ms " + ms + "}\n";
                                }

                                if (!FindLerpAndMs(lerpms, ms))
                                {
                                    DemoScanner_AddWarn("[FAKELAG TYPE 1.3] at (" + CurrentTime + "):" + CurrentTimeString,
                                        true, true, false, true);
                                    FakeLagAim++;
                                }
                                else
                                {
                                    if (!FindLerpAndMs(lerpms, ms, false))
                                    {
                                        FakeLag2Aim++;
                                        if (FakeLag2Aim > 0)
                                        {
                                            DemoScanner_AddWarn(
                                                "[FAKELAG TYPE 1.4] at (" + CurrentTime + "):" + CurrentTimeString, false,
                                                true, false, true);
                                        }
                                    }
                                }
                            }
                        }
                        else if (cmdList[1] == "EVENTS" && (PluginVersion.Length == 0 || flPluginVersion < 1.56))
                        {
                            plugin_valid_packets++;
                            if (PluginVersion.Length != 0 && !DisableJump5AndAim16)
                            {
                                var events = int.Parse(cmdList[2]);
                                if (DUMP_ALL_FRAMES)
                                {
                                    OutDumpString += "\n{ EVENT PLUGIN }\n";
                                }

                                if (PluginEvents == -1)
                                {
                                    CurrentEvents = 0;
                                    PluginEvents = 0;
                                }
                                else if (CurrentEvents > 0)
                                {
                                    if (CurrentEvents - PluginEvents > 4 && CurrentEvents - (PluginEvents + events) > 4)
                                    {
                                        if (PluginEvents != 0)
                                        {
                                            Event7Hack++;
                                            if (Event7Hack > 1)
                                            {
                                                DemoScanner_AddWarn(
                                                    "[CMD HACK TYPE 7] at (" + CurrentTime + "):" +
                                                    CurrentTimeString, false, true, false, true);
                                            }
                                        }
                                        else
                                        {
                                            BadEvents += 8;
                                        }

                                        CurrentEvents = 0;
                                        PluginEvents = 0;
                                        FirstEventShift = true;
                                    }

                                    PluginEvents += events;
                                }
                            }
                        }
                        else if (cmdList[1] == "XEVENT")
                        {
                            plugin_valid_packets++;
                            if (PluginVersion.Length != 0 && !DisableJump5AndAim16)
                            {
                                if (DUMP_ALL_FRAMES)
                                {
                                    OutDumpString += "\n{ EVENT PLUGIN }\n";
                                }

                                var tmpEvent = 0;
                                if (cmdList.Length > 1)
                                {
                                    int.TryParse(cmdList[2], out tmpEvent);
                                }

                                LastEventId = -tmpEvent;

                                if (abs(LastEventDetectTime) > EPSILON && abs(LastAttackPressed - LastEventDetectTime) > 0.5 && !IsInAttack())
                                {

                                }
                                else
                                {
                                    LastEventDetectTime = 0.0f;
                                }

                                //if (PluginEvents == -1)
                                //{
                                //    CurrentEvents = 0;
                                //    PluginEvents = 0;
                                //}
                                //else if (CurrentEvents > 0)
                                //{
                                //    if (CurrentEvents - PluginEvents > 4)
                                //    {
                                //        if (PluginEvents != 0)
                                //            DemoScanner_AddWarn(
                                //                "[CMD HACK TYPE 8] at (" + CurrentTime + "):" +
                                //                CurrentTimeString, false, true, false, true);
                                //        else
                                //            BadEvents += 8;

                                //        CurrentEvents = 0;
                                //        PluginEvents = 0;
                                //        FirstEventShift = true;
                                //    }

                                //    PluginEvents += events;
                                //}
                            }
                        }
                        else if (cmdList[1] == "MINR" || cmdList[1] == "MINUR"
                            || cmdList[1] == "MAXR" || cmdList[1] == "MAXUR")
                        {
                            plugin_valid_packets++;
                            if (PluginVersion.Length != 0)
                            {
                                var rate = int.Parse(cmdList[2]);
                                if (cmdList[1] == "MINR")
                                {
                                    sv_minrate = rate;
                                    if (rate > 0 && rate < 25000)
                                    {
                                        if (IsRussia)
                                        {
                                            DemoScanner_AddWarn("[INFO] На сервере слишком низкий sv_minrate. Необходим >= 25000", true,
                                                false, true, true);
                                            DemoScanner_AddWarn(
                                                "[ERROR] Отключается обнаружение [JUMPHACK TYPE 5] и [AIM TYPE 1.6]", true,
                                                false, true, true);
                                        }
                                        else
                                        {
                                            DemoScanner_AddWarn("[INFO] Small sv_minrate at server. Recomended >= 25000",
                                                true, false, true, true);
                                            DemoScanner_AddWarn(
                                                "[ERROR] Disabled detection of [JUMPHACK TYPE 5] and [AIM TYPE 1.6]", true,
                                                false, true, true);
                                        }
                                        DisableJump5AndAim16 = true;
                                    }
                                }
                                else if (cmdList[1] == "MAXR")
                                {
                                    sv_maxrate = rate;
                                    if (rate > 0 && rate < 25000)
                                    {
                                        if (IsRussia)
                                        {
                                            DemoScanner_AddWarn("[INFO] На сервере слишком низкий sv_maxrate. Необходим >= 25000", true,
                                                false, true, true);
                                            DemoScanner_AddWarn(
                                                "[ERROR] Отключается обнаружение [JUMPHACK TYPE 5] и [AIM TYPE 1.6]", true,
                                                false, true, true);
                                        }
                                        else
                                        {
                                            DemoScanner_AddWarn("[INFO] Small sv_maxrate at server. Recomended >= 25000",
                                                true, false, true, true);
                                            DemoScanner_AddWarn(
                                                "[ERROR] Disabled detection of [JUMPHACK TYPE 5] and [AIM TYPE 1.6]", true,
                                                false, true, true);
                                        }
                                        DisableJump5AndAim16 = true;
                                    }
                                }
                                else if (cmdList[1] == "MINUR")
                                {
                                    sv_minupdaterate = rate;
                                    if (rate < 30)
                                    {
                                        if (IsRussia)
                                        {
                                            DemoScanner_AddWarn("[INFO] На сервере слишком низкий sv_minupdaterate. Необходим >= 30", true,
                                                false, true, true);
                                            DemoScanner_AddWarn(
                                                "[ERROR] Отключается обнаружение [JUMPHACK TYPE 5] и [AIM TYPE 1.6]", true,
                                                false, true, true);
                                        }
                                        else
                                        {
                                            DemoScanner_AddWarn("[INFO] Small sv_minupdaterate at server. Recomended >= 30",
                                                true, false, true, true);
                                            DemoScanner_AddWarn(
                                                "[ERROR] Disabled detection of [JUMPHACK TYPE 5] and [AIM TYPE 1.6]", true,
                                                false, true, true);
                                        }
                                        DisableJump5AndAim16 = true;
                                    }
                                }
                                else if (cmdList[1] == "MAXUR")
                                {
                                    sv_maxupdaterate = rate;
                                    if (rate < 30)
                                    {
                                        if (IsRussia)
                                        {
                                            DemoScanner_AddWarn("[INFO] На сервере слишком низкий sv_maxupdaterate. Необходим >= 30", true,
                                                false, true, true);
                                            DemoScanner_AddWarn(
                                                "[ERROR] Отключается обнаружение [JUMPHACK TYPE 5] и [AIM TYPE 1.6]", true,
                                                false, true, true);
                                        }
                                        else
                                        {
                                            DemoScanner_AddWarn("[INFO] Small sv_maxupdaterate at server. Recomended >= 30",
                                                true, false, true, true);
                                            DemoScanner_AddWarn(
                                                "[ERROR] Disabled detection of [JUMPHACK TYPE 5] and [AIM TYPE 1.6]", true,
                                                false, true, true);
                                        }
                                        DisableJump5AndAim16 = true;
                                    }
                                }
                            }
                        }
                        else if (cmdList[1] == "BAD")
                        {
                            if (cmdList[2] == "1")
                            {
                                if (IsRussia)
                                {
                                    DemoScanner_AddWarn("[INFO] На сервере слишком низкий sv_minrate или sv_maxrate",
                                        true, false, true, true);
                                    DemoScanner_AddWarn(
                                        "[ERROR] Отключается обнаружение [JUMPHACK TYPE 5] и [AIM TYPE 1.6]", true,
                                        false, true, true);
                                }
                                else
                                {
                                    DemoScanner_AddWarn("[INFO] Small sv_minrate or sv_maxrate value at server", true,
                                        false, true, true);
                                    DemoScanner_AddWarn(
                                        "[ERROR] Disabled detection of [JUMPHACK TYPE 5] and [AIM TYPE 1.6]", true,
                                        false, true, true);
                                }
                            }
                            else
                            {
                                if (IsRussia)
                                {
                                    DemoScanner_AddWarn("[INFO] На сервере слишком низкий sv_minupdaterate", true,
                                        false, true, true);
                                    DemoScanner_AddWarn(
                                        "[ERROR] Отключается обнаружение [JUMPHACK TYPE 5] и [AIM TYPE 1.6]", true,
                                        false, true, true);
                                }
                                else
                                {
                                    DemoScanner_AddWarn("[INFO] Small sv_maxupdaterate or sv_minupdaterate at server",
                                        true, false, true, true);
                                    DemoScanner_AddWarn(
                                        "[ERROR] Disabled detection of [JUMPHACK TYPE 5] and [AIM TYPE 1.6]", true,
                                        false, true, true);
                                }
                            }

                            DisableJump5AndAim16 = true;
                        }
                        else if (cmdList[1] == "CONSOLE")
                        {
                            if (cmdList.Length > 2)
                            {
                                var tmpcolor = Console.ForegroundColor;
                                Console.ForegroundColor = ConsoleColor.DarkYellow;
                                Console.Write("[SERVER PRINT] ");
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine(RemoveCtrlChars(cmdList[2]));
                                Console.ForegroundColor = tmpcolor;
                            }
                        }
                        /* else if (cmdList[1] == "ANGLE")
                         {
                             float angle = 0.0f;
                             try
                             {
                                 angle = BitConverter.ToSingle(BitConverter.GetBytes(int.Parse(cmdList[2])), 0);
                             }
                             catch
                             {

                             }
                             Console.WriteLine(angle);
                             if (DUMP_ALL_FRAMES)
                             {
                                 OutDumpString += "\n{ATTACK ANGLE: " + angle + " " + CurrentTimeString + " " + CurrentTime + " }\n";
                             }
                         }*/
                        else if (flPluginVersion >= 1.59)
                        {
                            if (abs(CurrentTime - LastCmdHack) > 10.0f)
                            {
                                if (cmdList[1][0] == 'X')
                                {
                                    DemoScanner_AddWarn("[ALTERNATIVE HACK 2024 version:\"CMD\"] at (" + Math.Round(CurrentTime) +
                                                                           ")", true, true, true, true);
                                }
                                else
                                {
                                    DemoScanner_AddWarn("[PLUGINHACK TYPE 1.2] at (" + CurrentTime + ") " + CurrentTimeString, true, true, true, true);
                                }
                                LastCmdHack = CurrentTime;
                            }
                        }
                    }
                    else
                    {
                        if (abs(CurrentTime - LastCmdHack) > 10.0f)
                        {
                            DemoScanner_AddWarn("[PLUGINHACK TYPE 1.3] at (" + CurrentTime + ") " + CurrentTimeString, true, true, true, true);
                            LastCmdHack = CurrentTime;
                        }
                    }
                }
                else
                {
                    if (abs(CurrentTime - LastCmdHack) > 10.0)
                    {
                        string smallcmd = (cmdList[0].Length > 3 ? cmdList[0].Remove(3) : cmdList[0]).Trim();
                        if (smallcmd.Length > 0 && smallcmd[0] == 'v')
                        {
                            DemoScanner_AddWarn("[ALTERNATIVE HACK 2023 version:\"" + smallcmd + "\"] at (" + CurrentTime +
                                                                            ") : " + CurrentTimeString, true, true, true, true);
                        }
                        else if (smallcmd.Length > 0 && smallcmd[0] == 'X')
                        {
                            DemoScanner_AddWarn("[INTERIUM HACK 2023 version:\"" + smallcmd + "\"] at (" + CurrentTime +
                                                                            ") : " + CurrentTimeString, true, true, true, true);
                        }
                        else
                        {
                            DemoScanner_AddWarn("[PLUGINHACK TYPE 1.4] at (" + CurrentTime + ") " + CurrentTimeString, true, true, true, true);
                            LastCmdHack = CurrentTime;
                        }
                        LastCmdHack = CurrentTime;
                    }
                }
            }
            else if (abs(CurrentTime - LastCmdHack) > 10.0f)
            {
                DemoScanner_AddWarn("[PLUGINHACK TYPE 1.6] at (" + CurrentTime + ") " + CurrentTimeString, true, true, true, true);
                LastCmdHack = CurrentTime;
            }
        }

        public struct PLAYER_USED_SENS
        {
            public float sens;
            public int usagecount;
        }

        public struct FPoint
        {
            public float X, Y;

            public FPoint(float x, float y)
            {
                X = x;
                Y = y;
            }
        }

        public struct WarnStruct
        {
            public string Warn;
            public bool Detected;
            public bool Visited;
            public bool SkipAllChecks;
            public float WarnTime;
            public bool Plugin;
            public bool HidePrefix;
        }

        public struct UcmdLerpAndMs
        {
            public int lerp;
            public byte msec;
        }

        public struct WindowResolution
        {
            public int x, y;
        }



        public struct VOICEDATA
        {
            public int len;
            public float time;
            public byte[] data;
            public VOICEDATA(int _len, byte[] _data, float PlayersVoiceTimer)
            {
                len = _len;
                data = _data;
                time = PlayersVoiceTimer;
            }
        }

        public class Player
        {
            public Dictionary<string, string> InfoKeys;
            public string UserName;
            public int iStructCounter;
            public int iSlot;
            public int ServerUserIdLong;
            public string UserSteamId;
            public List<VOICEDATA> voicedata;

            public Player(int slot, int serveruserid)
            {
                iStructCounter = cipid++;
                UserName = string.Empty;
                iSlot = slot;
                ServerUserIdLong = serveruserid;
                InfoKeys = new Dictionary<string, string>();
                voicedata = new List<VOICEDATA>();
                UserSteamId = string.Empty;
            }

            public void WriteVoice(int len, byte[] data)
            {
                if (len > 0)
                {
                    voicedata.Add(new VOICEDATA(len, data, PlayersVoiceTimer));
                }
            }

            public override string ToString()
            {
                return "Username:" + UserName + "|Steam:" + UserSteamId + "|Slot:" + iSlot + "|UserID:" + ServerUserIdLong + "|PID" + iStructCounter;
            }
        }
    }

    public abstract class DemoParser
    {
        // Func
        public delegate TResult Function<TResult>();

        public delegate TResult Function<T, TResult>(T arg);

        public delegate TResult Function<T1, T2, TResult>(T1 arg1, T2 arg2);

        public delegate TResult
            Function<T1, T2, T3, TResult>(T1 arg1, T2 arg2, T3 arg3);

        public delegate TResult Function<T1, T2, T3, T4, TResult>(T1 arg1, T2 arg2,
            T3 arg3, T4 arg4);

        // Action
        public delegate void Procedure();

        public delegate void Procedure<T1>(T1 arg);

        public delegate void Procedure<T1, T2>(T1 arg1, T2 arg2);

        public delegate void Procedure<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3);

        public delegate void Procedure<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);

        public Hashtable messageHandlerTable = new Hashtable(); // id -> length and callback
        public Hashtable messageStringTable = new Hashtable(); // just for logging purposes, id -> name

        protected CrossParseResult demo;
        protected BinaryReader fileReader;
        protected FileStream fileStream;

        /// <summary>
        ///     Calculates a Steam ID from a "*sid" Half-Life infokey value.
        /// </summary>
        /// <param name="sidInfoKeyValue">The "*sid" infokey value.</param>
        /// <returns>A Steam ID in the format "STEAM_0:x:y".</returns>
        public static string CalculateSteamId(string sidInfoKeyValue)
        {
            if (!long.TryParse(sidInfoKeyValue, out var id) || id == 0)
            {
                // HLTV proxy or LAN dedicated server.
                return null;
            }

            var universe = id >> 56;
            var accountid = id & 0xFFFFFFFF;

            if (universe == 1)
            {
                universe = 0;
            }

            return string.Format("STEAM_{0}:{1}:{2}", universe, accountid & 1, accountid >> 1);
        }

        public void AddMessageHandler(byte id, int length)
        {
            //Debug.Assert(length != -1);
            AddMessageHandler(id, length, null);
        }

        public void AddMessageHandler(byte id, Procedure callback)
        {
            Debug.Assert(callback != null);
            AddMessageHandler(id, -1, callback);
        }

        public void AddMessageHandler(byte id, int length, Procedure callback)
        {
            var newHandler = new MessageHandler { Id = id, Length = length, Callback = callback };
            // replace message handler if it already exists
            if (messageHandlerTable.Contains(id))
            {
                messageHandlerTable.Remove(id);
            }

            messageHandlerTable.Add(newHandler.Id, newHandler);
        }

        protected MessageHandler FindMessageHandler(byte messageId)
        {
            return (MessageHandler)messageHandlerTable[messageId];
        }

        protected void AddMessageIdString(byte id, string s)
        {
            if (messageStringTable[id] != null)
            {
                messageStringTable.Remove(id);
            }

            messageStringTable.Add(id, s);
        }

        public string FindMessageIdString(byte id)
        {
            var s = (string)messageStringTable[id];
            return s == null ? "UNKNOWN MESSAGE" : s;
        }

        protected class MessageHandler
        {
            public Procedure Callback;
            public byte Id;
            public int Length;
        }
    }

    public class HalfLifeDemoParser : DemoParser
    {
        public enum MessageId : byte
        {
            svc_bad = 0,
            svc_nop = 1,
            svc_disconnect = 2,
            svc_event = 3,
            svc_version = 4,
            svc_setview = 5,
            svc_sound = 6,
            svc_time = 7,
            svc_print = 8,
            svc_stufftext = 9,
            svc_setangle = 10,
            svc_serverinfo = 11,
            svc_lightstyle = 12,
            svc_updateuserinfo = 13,
            svc_deltadescription = 14,
            svc_clientdata = 15,
            svc_stopsound = 16,
            svc_pings = 17,
            svc_particle = 18,
            svc_damage = 19,
            svc_spawnstatic = 20,
            svc_event_reliable = 21,
            svc_spawnbaseline = 22,
            svc_tempentity = 23,
            svc_setpause = 24,
            svc_signonnum = 25,
            svc_centerprint = 26,
            svc_killedmonster = 27,
            svc_foundsecret = 28,
            svc_spawnstaticsound = 29,
            svc_intermission = 30,
            svc_finale = 31,
            svc_cdtrack = 32,
            svc_restore = 33,
            svc_cutscene = 34,
            svc_weaponanim = 35,
            svc_decalname = 36,
            svc_roomtype = 37,
            svc_addangle = 38,
            svc_newusermsg = 39,
            svc_packetentities = 40,
            svc_deltapacketentities = 41,
            svc_choke = 42,
            svc_resourcelist = 43,
            svc_newmovevars = 44,
            svc_resourcerequest = 45,
            svc_customization = 46,
            svc_crosshairangle = 47,
            svc_soundfade = 48,
            svc_filetxferfailed = 49,
            svc_hltv = 50,
            svc_director = 51,
            svc_voiceinit = 52,
            svc_voicedata = 53,
            svc_sendextrainfo = 54,
            svc_timescale = 55,
            svc_resourcelocation = 56,
            svc_sendcvarvalue = 57,
            svc_sendcvarvalue2 = 58,
            svc_exec = 59
        }


        public static int ErrorCount;
        private readonly Hashtable deltaDecoderTable;
        private readonly Hashtable userMessageCallbackTable; // name -> Common.NoArgsDelegate callback
        private readonly Hashtable userMessageTable; // name -> id, length
        private bool readingGameData;

        public HalfLifeDemoParser(CrossParseResult demo)
        {
            this.demo = demo;
            // user messages
            userMessageTable = new Hashtable();
            userMessageCallbackTable = new Hashtable();
            // delta descriptions
            deltaDecoderTable = new Hashtable();
            var deltaDescription = new HalfLifeDeltaStructure("delta_description_t");
            AddDeltaStructure(deltaDescription);
            deltaDescription.AddEntry("flags", 32, 1.0f, 1.0f, HalfLifeDeltaStructure.ENTRY_TYPES.Integer);
            deltaDescription.AddEntry("name", 8, 1.0f, 1.0f, HalfLifeDeltaStructure.ENTRY_TYPES.String);
            deltaDescription.AddEntry("offset", 16, 1.0f, 1.0f, HalfLifeDeltaStructure.ENTRY_TYPES.Integer);
            deltaDescription.AddEntry("size", 8, 1.0f, 1.0f, HalfLifeDeltaStructure.ENTRY_TYPES.Integer);
            deltaDescription.AddEntry("nBits", 8, 1.0f, 1.0f, HalfLifeDeltaStructure.ENTRY_TYPES.Integer);
            deltaDescription.AddEntry("divisor", 32, 4000.0f, 1.0f, HalfLifeDeltaStructure.ENTRY_TYPES.Float);
            deltaDescription.AddEntry("preMultiplier", 32, 4000.0f, 1.0f, HalfLifeDeltaStructure.ENTRY_TYPES.Float);
            // message handlers
            AddMessageHandler((byte)MessageId.svc_nop, MessageNop);
            AddMessageHandler((byte)MessageId.svc_disconnect, MessageDisconnect);
            AddMessageHandler((byte)MessageId.svc_event, MessageEvent);
            AddMessageHandler((byte)MessageId.svc_version, MessageVersion);
            AddMessageHandler((byte)MessageId.svc_setview, MessageView);
            AddMessageHandler((byte)MessageId.svc_sound, MessageSound);
            AddMessageHandler((byte)MessageId.svc_time, MessageTime);
            AddMessageHandler((byte)MessageId.svc_print, MessagePrint);
            AddMessageHandler((byte)MessageId.svc_stufftext, MessageStuffText);
            AddMessageHandler((byte)MessageId.svc_setangle, MessageSetAngle);
            AddMessageHandler((byte)MessageId.svc_serverinfo, MessageServerInfo);
            AddMessageHandler((byte)MessageId.svc_lightstyle, MessageLightStyle);
            AddMessageHandler((byte)MessageId.svc_updateuserinfo, MessageUpdateUserInfo);
            AddMessageHandler((byte)MessageId.svc_deltadescription, MessageDeltaDescription);
            AddMessageHandler((byte)MessageId.svc_clientdata, MessageClientData);
            AddMessageHandler((byte)MessageId.svc_stopsound, MessageStopSound);
            AddMessageHandler((byte)MessageId.svc_pings, MessagePings);
            AddMessageHandler((byte)MessageId.svc_particle, 11);
            AddMessageHandler((byte)MessageId.svc_spawnstatic, MessageSpawnStatic);
            AddMessageHandler((byte)MessageId.svc_event_reliable, MessageEventReliable);
            AddMessageHandler((byte)MessageId.svc_spawnbaseline, MessageSpawnBaseline);
            AddMessageHandler((byte)MessageId.svc_tempentity, MessageTempEntity);
            AddMessageHandler((byte)MessageId.svc_setpause, MessageSetPause);
            AddMessageHandler((byte)MessageId.svc_signonnum, MessageSign);
            AddMessageHandler((byte)MessageId.svc_centerprint, MessageCenterPrint);
            AddMessageHandler((byte)MessageId.svc_spawnstaticsound, 14);
            AddMessageHandler((byte)MessageId.svc_intermission, MessageInterMission);
            AddMessageHandler((byte)MessageId.svc_finale, 1);
            AddMessageHandler((byte)MessageId.svc_cdtrack, 2);
            AddMessageHandler((byte)MessageId.svc_weaponanim, 2);
            AddMessageHandler((byte)MessageId.svc_roomtype, MessageRoomType);
            AddMessageHandler((byte)MessageId.svc_addangle, MessageAddAngle);
            AddMessageHandler((byte)MessageId.svc_newusermsg, MessageNewUserMsg);
            AddMessageHandler((byte)MessageId.svc_packetentities, MessagePacketEntities);
            AddMessageHandler((byte)MessageId.svc_deltapacketentities, MessageDeltaPacketEntities);
            AddMessageHandler((byte)MessageId.svc_choke, MessageChoke);
            AddMessageHandler((byte)MessageId.svc_resourcelist, MessageResourceList);
            AddMessageHandler((byte)MessageId.svc_newmovevars, MessageNewMoveVars);
            AddMessageHandler((byte)MessageId.svc_resourcerequest, 8);
            AddMessageHandler((byte)MessageId.svc_customization, MessageCustomization);
            AddMessageHandler((byte)MessageId.svc_crosshairangle, 2);
            AddMessageHandler((byte)MessageId.svc_soundfade, 4);
            AddMessageHandler((byte)MessageId.svc_filetxferfailed, MessageFileTransferFailed);
            AddMessageHandler((byte)MessageId.svc_hltv, MessageHltv);
            AddMessageHandler((byte)MessageId.svc_director, MessageDirector);
            AddMessageHandler((byte)MessageId.svc_voiceinit, MessageVoiceInit);
            AddMessageHandler((byte)MessageId.svc_voicedata, MessageVoiceData);
            AddMessageHandler((byte)MessageId.svc_sendextrainfo, MessageSendExtraInfo);
            AddMessageHandler((byte)MessageId.svc_timescale, 4);
            AddMessageHandler((byte)MessageId.svc_resourcelocation, MessageResourceLocation);
            AddMessageHandler((byte)MessageId.svc_sendcvarvalue, MessageSendCvarValue);
            AddMessageHandler((byte)MessageId.svc_sendcvarvalue2, MessageSendCvarValue2);
            AddMessageHandler((byte)MessageId.svc_foundsecret, 0);
            AddMessageHandler((byte)MessageId.svc_damage, 0);
            AddMessageHandler((byte)MessageId.svc_killedmonster, 0);
            AddMessageHandler((byte)MessageId.svc_restore, MessageRestore);
            AddMessageHandler((byte)MessageId.svc_cutscene, MessageCutSceneCenterPrint);
            AddMessageHandler((byte)MessageId.svc_decalname, MessageReplaceDecal);
            AddMessageHandler((byte)MessageId.svc_exec, MessageTFC_Exec);
            AddUserMessageHandler("DeathMsg", MessageDeath);
            AddUserMessageHandler("CurWeapon", MessageCurWeapon);
            AddUserMessageHandler("ResetHUD", MessageResetHud);
            AddUserMessageHandler("ScreenFade", MessageScreenFade);
            AddUserMessageHandler("Health", MessageHealth);
            AddUserMessageHandler("Crosshair", MessageCrosshair);
            AddUserMessageHandler("Spectator", MessageSpectator);
            AddUserMessageHandler("ScoreAttrib", MessageScoreAttrib);
            AddUserMessageHandler("TextMsg", TextMsg);
            AddUserMessageHandler("Damage", Damage);
            AddUserMessageHandler("SetFOV", SetFOV);
            AddUserMessageHandler("HideWeapon", HideWeapon);
            AddUserMessageHandler("WeapPickup", WeapPickup);
            AddUserMessageHandler("WeaponList", WeaponList);
            AddUserMessageHandler("HealthInfo", HealthInfo);
            AddUserMessageHandler("HLTV", HLTVMSG);
            //AddUserMessageHandler("StatusText", StatusText);
        }

        // public so svc_deltadescription can be parsed elsewhere
        public void AddDeltaStructure(HalfLifeDeltaStructure structure)
        {
            // remove decoder if it already exists (duplicate svc_deltadescription message)
            // e.g. GotFrag Demo 6 (rs vs TSO).zip
            if (deltaDecoderTable[structure.Name] != null)
            {
                deltaDecoderTable.Remove(structure.Name);
            }

            deltaDecoderTable.Add(structure.Name, structure);
        }

        public HalfLifeDeltaStructure GetDeltaStructure(string name)
        {
            var structure = (HalfLifeDeltaStructure)deltaDecoderTable[name];
            if (structure == null)
            {
                structure = new HalfLifeDeltaStructure(name);
            }

            return structure;
        }

        // public so that svc_newusermsg can be parsed elsewhere
        public void AddUserMessage(byte id, sbyte length, string name)
        {
            // some demos contain duplicate user message definitions
            // e.g. GotFrag Demo 13 (mTw.nine vs CosaNostra).zip
            // others, due to what seems like completely fucking retarded servers, have many copies of the 
            // same user message registered with different id's
            if (userMessageTable.Contains(name))
            {
                userMessageTable.Remove(name);
            }

            var userMessage = new UserMessage { Id = id, Length = length };
            userMessageTable.Add(name, userMessage);
            AddMessageIdString(id, name);
            // see if there's a handler waiting to be attached to this message
            var callback = (Procedure)userMessageCallbackTable[name];
            AddMessageHandler(id, length, callback);
        }

        public void AddUserMessageHandler(string name, Procedure callback)
        {
            if (userMessageCallbackTable.Contains(name))
            {
                userMessageCallbackTable.Remove(name);
            }

            userMessageCallbackTable.Add(name, callback);
        }

        public void ParseGameDataMessages(byte[] frameData)
        {
            if (ErrorCount > 100)
            {
                return;
            }

            if (DUMP_ALL_FRAMES)
            {
                OutDumpString += "{\n";
            }

            try
            {
                ParseGameDataMessages(frameData, null);
            }
            catch (Exception ex)
            {
                ErrorCount++;
                var tmpcolor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                if (DEBUG_ENABLED)
                {
                    Console.WriteLine("(E=" + ex.Message + ") (" + ex.Source + ") \n(" + ex.StackTrace + ") at " +
                                      CurrentTime);
                }
                else
                {
                    Console.WriteLine("(E=" + ex.Message + ") at " + CurrentTime);
                }

                Console.ForegroundColor = tmpcolor;
                if (DUMP_ALL_FRAMES)
                {
                    OutDumpString += "(E=" + ex.Message + ") (" + ex.Source + ") \n(" + ex.StackTrace + ")";
                    OutDumpString += "FATAL ERROR. STOP MESSAGE PARSING.\n}\n";
                }

                SkipNextErrors = true;
            }

            if (DUMP_ALL_FRAMES)
            {
                OutDumpString += "}\n";
            }
        }

        public void ParseGameDataMessages(byte[] frameData, Function<byte, byte> userMessageCallback)
        {
            // read game data frame into memory
            BitBuffer = new BitBuffer(frameData);
            readingGameData = true;
            var messageFrameOffsetOld = 0;
            if (frameData.Length == 0 && BitBuffer.CurrentByte == 0)
            {
                //DemoScanner.EmptyFrames++;
                return;
            }

            // start parsing messages
            while (true)
            {
                DemoScanner.MessageId += 1;
                if (DUMP_ALL_FRAMES)
                {
                    OutDumpString += "\n{MSGLEN-" + frameData.Length + ".MSGBYTE:" + BitBuffer.CurrentByte;
                }

                var messageId = BitBuffer.ReadByte();
                if (DUMP_ALL_FRAMES)
                {
                    OutDumpString += "MSGID:" + messageId + ".MSGBYTE:" + BitBuffer.CurrentByte;
                }

                // File.AppendAllText("messages.bin", messageId + "\n");
                var messageName = Enum.GetName(typeof(MessageId), messageId);
                if (messageName == null) // a user message, presumably
                {
                    messageName = FindMessageIdString(messageId);
                }

                if (DUMP_ALL_FRAMES)
                {
                    OutDumpString += ".\nMSGNAME:[" + messageName + "]";
                }

                var messageHandler = FindMessageHandler(messageId);
                if (messageName == "svc_intermission")
                {
                    if (!GameEnd)
                    {
                        GameEnd = true;
                        GameEndTime = CurrentTime;
                        Console.WriteLine("---------- [Конец игры / Game Over (" + CurrentTimeString + ")] ----------");
                    }
                    else
                    {
                        Console.WriteLine("---------- [Конец игры 2 / Game Over 2 (" + CurrentTimeString +
                                          ")] ----------");
                    }
                }

                // Handle the conversion of user message id's.
                // Used by demo writing to convert to the current network protocol.
                if (messageId > 64 && userMessageCallback != null)
                {
                    var newMessageId = userMessageCallback(messageId);
                    if (newMessageId != messageId)
                    {
                        // write the new id to the bitbuffer
                        BitBuffer.SeekBytes(-1);
                        BitBuffer.RemoveBytes(1);
                        BitBuffer.InsertBytes(new[] { newMessageId });
                    }
                }

                // unknown message
                if (messageHandler == null)
                {
                    if (messageId != 0)
                    {
                        UnknownMessages++;
                        CheckConsoleCommand("Unknown message id:" + messageId, true);
                    }

                    break;
                }

                // callback takes priority over length
                if (messageHandler.Callback != null)
                {
                    if (messageHandler.Length != -1)
                    {
                        var curbits = BitBuffer.CurrentBit;
                        messageHandler.Callback();
                        BitBuffer.SeekBits(curbits, SeekOrigin.Begin);
                        Seek(messageHandler.Length);
                    }
                    else
                    {
                        if (messageId >= 64)
                        {
                            var curbits = BitBuffer.CurrentBit;
                            messageHandler.Callback();
                            BitBuffer.SeekBits(curbits, SeekOrigin.Begin);
                            var length = BitBuffer.ReadByte();
                            Seek(length);
                        }
                        else
                        {
                            messageHandler.Callback();
                        }
                    }
                }
                else if (messageHandler.Length != -1)
                {
                    Seek(messageHandler.Length);
                }
                else if (messageId >= 64)
                {
                    // All non-engine user messages start with a byte that is the number of bytes in the message remaining.
                    var length = BitBuffer.ReadByte();
                    Seek(length);
                }
                else
                {
                    throw new ApplicationException(string.Format("Unknown message id \"{0}\"", messageId));
                }

                // Check if we've reached the end of the frame, or if any of the messages have called SkipGameDataFrame (readingGameData will be false).
                if (BitBuffer.CurrentByte < 0 || BitBuffer.CurrentByte >= BitBuffer.Length || !readingGameData)
                {
                    break;
                }

                if (BitBuffer.CurrentByte >= messageFrameOffsetOld)
                {
                    messageFrameOffsetOld = BitBuffer.CurrentByte;
                }
                else
                {
                    //MessageBox.Show("error");
                    throw new ApplicationException(string.Format("Offset error message {0} -> {1}", BitBuffer.CurrentByte, messageFrameOffsetOld));
                }
            }

            readingGameData = false;
        }

        public void Seek(long deltaOffset)
        {
            Seek(deltaOffset, SeekOrigin.Current);
        }

        public void Seek(long offset, SeekOrigin origin)
        {
            if (readingGameData)
            {
                //Debug.Assert(offset <= int.MaxValue);
                BitBuffer.SeekBytes((int)offset, origin);
            }
            else
            {
                fileStream.Seek(offset, origin);
            }
        }

        private class UserMessage
        {
            public byte Id;
            public sbyte Length;
        }

        public BitBuffer BitBuffer { get; private set; }

        #region Engine Messages

        private void MessageNop()
        {
            if (BitBuffer.Data.Length - BitBuffer.CurrentByte > 5)
            {
                if (BitBuffer.Data[BitBuffer.CurrentByte] == (byte)MessageId.svc_foundsecret &&
                    BitBuffer.Data[BitBuffer.CurrentByte + 1] == (byte)MessageId.svc_foundsecret &&
                    BitBuffer.Data[BitBuffer.CurrentByte + 2] == (byte)MessageId.svc_killedmonster &&
                    BitBuffer.Data[BitBuffer.CurrentByte + 3] == (byte)MessageId.svc_nop &&
                    BitBuffer.Data[BitBuffer.CurrentByte + 4] == (byte)MessageId.svc_print)
                {
                    FoundCustomClientPattern = true;
                    return;
                }
            }

            if (BitBuffer.Data.Length >= 16)
            {
                DemoScanner_AddWarn("[BETA] [PLUGINHACK TYPE 1.5] at (" + CurrentTime + ") " + CurrentTimeString, false, true, true, true);
            }
        }

        private void MessageDisconnect()
        {
            CurrentMsgPrintCount++;
            var msg = BitBuffer.ReadString();
            DemoScanner_AddTextMessage(msg, "DISCONNECT", CurrentTime, CurrentTimeString);
            if (DUMP_ALL_FRAMES)
            {
                OutDumpString += "MessageDisconnect:" + msg;
            }
        }

        public void MessageEvent()
        {
            if (demo.GsDemoInfo.Header.NetProtocol <= 43)
            {
                BitBuffer.Endian = BitBuffer.EndianType.Big;
            }

            var nEvents = BitBuffer.ReadUnsignedBits(5);
            if (DUMP_ALL_FRAMES)
            {
                OutDumpString += "MessageEvents:(" + nEvents + "){\n";
            }

            for (var i = 0; i < nEvents; i++)
            {
                var nIndex = BitBuffer.ReadUnsignedBits(10);
                if (DUMP_ALL_FRAMES)
                {
                    OutDumpString += "index:" + nIndex; // event index
                }

                foreach (var res in DownloadedResources)
                {
                    if (res.res_index == nIndex && res.res_type == 5)
                    {
                        if (DUMP_ALL_FRAMES)
                        {
                            OutDumpString += " = (" + res.res_path + ")"; // event index
                        }
                        //Console.WriteLine("Event2:" + res.res_path + " at " + CurrentTimeString);
                    }
                }
                if (DUMP_ALL_FRAMES)
                {
                    OutDumpString += "{\n";
                }

                //Console.WriteLine("EVENT 2:" + nIndex);
                var packetIndexBit = BitBuffer.ReadBoolean();
                if (packetIndexBit)
                {
                    var packindex = BitBuffer.ReadBits(11); // packet index
                    if (DUMP_ALL_FRAMES)
                    {
                        OutDumpString += "\nPACKET ID[" + packindex + "]\n";
                    }

                    var deltaBit = BitBuffer.ReadBoolean();
                    if (deltaBit)
                    {
                        GetDeltaStructure("event_t").ReadDelta(BitBuffer, null);
                    }
                }

                var fireTimeBit = BitBuffer.ReadBoolean();
                if (fireTimeBit)
                {
                    if (DUMP_ALL_FRAMES)
                    {
                        OutDumpString += "FIRETIME:" + BitBuffer.ReadUnsignedBits(16);
                    }
                    else
                    {
                        BitBuffer.ReadUnsignedBits(16);
                    }
                    //BitBuffer.SeekBits(16); // fire time
                }

                if (DUMP_ALL_FRAMES)
                {
                    OutDumpString += "}\n";
                }
            }

            if (DUMP_ALL_FRAMES)
            {
                OutDumpString += "}\n";
            }

            BitBuffer.SkipRemainingBits();
            BitBuffer.Endian = BitBuffer.EndianType.Little;
        }

        private void MessageVersion()
        {
            var version = BitBuffer.ReadUInt32(); // uint: server network protocol number.
            if (DUMP_ALL_FRAMES)
            {
                OutDumpString += "PROTOCOL VERSION[" + version + "]\n";
            }
        }

        private void MessageView()
        {
            int entityview = BitBuffer.ReadInt16();
            if (CL_Intermission == 0)
            {
                if (ViewModel > 0)
                {
                    ViewEntity = entityview;
                }
            }

            LastViewChange = CurrentTime;
            if (DUMP_ALL_FRAMES)
            {
                OutDumpString += "switch view to " + entityview + " player\n";
            }
        }

        public void MessageTime()
        {
            var time = BitBuffer.ReadSingle();
            if (DUMP_ALL_FRAMES)
            {
                OutDumpString += "[" + time + "]";
                try
                {
                    var t = TimeSpan.FromSeconds(time);
                    var CurrentTimeString = string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms", t.Hours, t.Minutes,
                        t.Seconds, t.Milliseconds);
                    OutDumpString += "[" + CurrentTimeString + "]";
                }
                catch
                {
                }
            }

            CurrentTimeSvc = time;
            SVC_TIMEMSGID = DemoScanner.MessageId;
        }

        public void MessageSound()
        {
            if (demo.GsDemoInfo.Header.NetProtocol <= 43)
            {
                BitBuffer.Endian = BitBuffer.EndianType.Big;
            }

            var flags = BitBuffer.ReadUnsignedBits(9);
            if ((flags & (1 << 0)) != 0) // volume
            {
                BitBuffer.ReadBits(8);
            }

            if ((flags & (1 << 1)) != 0) // attenuation * 64
            {
                BitBuffer.ReadBits(8);
            }

            var channel = BitBuffer.ReadBits(3); // channel
            var edictnum = BitBuffer.ReadBits(11); // edict number
            uint sound_id = 0;
            if ((flags & (1 << 2)) != 0) // sound index (short)
            {
                sound_id = BitBuffer.ReadUnsignedBits(16);
            }
            else // sound index (byte)
            {
                sound_id = BitBuffer.ReadUnsignedBits(8);
            }

            var coord = BitBuffer.ReadVectorCoord(); // position

            string soundname = sound_id.ToString();
            foreach (var res in DownloadedResources)
            {
                if (res.res_type == 0 && res.res_index == sound_id)
                {
                    soundname = res.res_path;
                }
            }

            if (DUMP_ALL_FRAMES)
            {
                OutDumpString += "\nSound -> " + soundname + " Edict -> " + edictnum + " Coord -> " + coord + " \n";
            }

            if ((flags & (1 << 3)) != 0) // pitch
            {
                BitBuffer.ReadBits(8);
            }

            BitBuffer.SkipRemainingBits();
            BitBuffer.Endian = BitBuffer.EndianType.Little;
            if (channel == 2 && ViewEntity == edictnum)
            {
                LastSoundTime = CurrentTime;
            }
        }

        private void MessagePrint()
        {
            CurrentMsgPrintCount++;
            var message = BitBuffer.ReadString();
            if (FoundCustomClientPattern)
            {
                FoundCustomClientPattern = false;
                if (!IsRussia)
                {
                    DemoScanner_AddInfo("Found custom client version: [" + message + "]");
                    if (message.ToLower().IndexOf("nextclient") >= 0)
                    {
                        FoundNextClient = true;
                        DemoScanner_AddInfo("Skip detect custom FOV");
                    }
                }
                else
                {
                    DemoScanner_AddInfo("Обнаружен кастомный клиент: [" + message + "]");
                    if (message.ToLower().IndexOf("nextclient") >= 0)
                    {
                        FoundNextClient = true;
                        DemoScanner_AddInfo("Пропускается обнаружение FOV");
                    }
                }
            }

            if (DEBUG_ENABLED)
            {
                Console.WriteLine("PRINT:" + message);
            }

            if (DUMP_ALL_FRAMES)
            {
                OutDumpString += "MessagePrint:" + message;
            }

            DemoScanner_AddTextMessage(message, "PRINT", CurrentTime, CurrentTimeString);
        }

        private void MessageStuffText()
        {
            CurrentMsgStuffCmdCount++;
            var stuffstr = BitBuffer.ReadString();
            if (DUMP_ALL_FRAMES)
            {
                OutDumpString += "MessageStuffText:" + stuffstr;
            }

            CheckConsoleCommand(stuffstr, true);
            LastStuffCmdCommand = stuffstr;
            DemoScanner_AddTextMessage(stuffstr, "CLIENT_CMD", CurrentTime, CurrentTimeString);
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct UInt32Union
        {
            [FieldOffset(0)] public int Value;
            [FieldOffset(0)] public byte Byte1;
            [FieldOffset(1)] public byte Byte2;
            [FieldOffset(2)] public byte Byte3;
            [FieldOffset(3)] public byte Byte4;
        }

        private static int Swap(int value)
        {
            var src = new UInt32Union { Value = value };
            var dest = new UInt32Union { Byte1 = src.Byte4, Byte2 = src.Byte3, Byte3 = src.Byte2, Byte4 = src.Byte1 };
            return dest.Value;
        }

        private int LongSwap(int l)
        {
            return Swap(l);
        }

        private unsafe void COM_UnMunge3(byte* data, int len, int seq)
        {
            byte[] mungify_table3 =
                { 0x20, 0x07, 0x13, 0x61, 0x03, 0x45, 0x17, 0x72, 0x0A, 0x2D, 0x48, 0x0C, 0x4A, 0x12, 0xA9, 0xB5 };
            int i;
            int mungelen;
            int c;
            int* pc;
            byte* p;
            int j;
            mungelen = len & ~3;
            mungelen /= 4;
            for (i = 0; i < mungelen; i++)
            {
                pc = (int*)&data[i * 4];
                c = *pc;
                c ^= seq;
                p = (byte*)&c;
                for (j = 0; j < 4; j++)
                {
                    *p++ ^= (byte)((0xa5 | (j << j) | j | mungify_table3[(i + j) & 0x0f]) & 0xFF);
                }

                c = LongSwap(c);
                c ^= ~seq;
                *pc = c;
            }
        }

        private unsafe void MessageServerInfo()
        {
            NewDirectory = true;

            BitBuffer.ReadInt32();
            BitBuffer.ReadInt32();
            var mapcrc32 = BitBuffer.ReadUInt32();
            COM_UnMunge3((byte*)&mapcrc32, 4, (-1 - StartPlayerID) & 0xFF);
            BitBuffer.ReadBytes(16);
            demo.MaxClients = BitBuffer.ReadByte();
            StartPlayerID = BitBuffer.ReadByte();
            DealthMatch = BitBuffer.ReadByte() > 0;
            GameDir = BitBuffer.ReadString(); // game dir
            if (demo.GsDemoInfo.Header.NetProtocol > 43)
            {
                ServerName = BitBuffer.ReadString(); // server name                
            }

            var tmpMapName = BitBuffer.ReadString();

            GameEnd = false;
            CL_Intermission = 0;
            Console.WriteLine("---------- [Начало новой игры / Start new game ( END: " + CurrentTimeString +
                              ")] ----------");

            if (tmpMapName != MapName)
            {
                if (IsRussia)
                {
                    DemoScanner_AddInfo("Смена уровня на \"" + tmpMapName + "\"( CRC " + mapcrc32 + " )");
                }
                else
                {
                    DemoScanner_AddInfo("Changelevel to \"" + tmpMapName + "\"( CRC " + mapcrc32 + " )");
                }

                MapName = tmpMapName;
                CL_Intermission = 0;
            }

            if (demo.GsDemoInfo.Header.NetProtocol == 45)
            {
                var extraInfo = BitBuffer.ReadByte();
                Seek(-1);
                if (extraInfo != (byte)MessageId.svc_sendextrainfo)
                {
                    BitBuffer.ReadString(); // skip mapcycle
                    if (BitBuffer.ReadByte() > 0)
                    {
                        Seek(36);
                    }
                }
            }
            else
            {
                BitBuffer.ReadString(); // skip mapcycle
                if (demo.GsDemoInfo.Header.NetProtocol > 43)
                {
                    if (BitBuffer.ReadByte() > 0)
                    {
                        Seek(21);
                    }
                }
            }

            if (FirstMap)
            {
                if (!SKIP_RESULTS)
                {
                    var tmpcursortop = Console.CursorTop;
                    var tmpcursorleft = Console.CursorLeft;
                    Console.CursorTop = MapAndCrc32_Top;
                    Console.CursorLeft = MapAndCrc32_Left;
                    var tmpconsolecolor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("\"" + MapName + "\" ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("(CRC \"" + mapcrc32 + "\")              ");
                    Console.ForegroundColor = tmpconsolecolor;
                    Console.CursorTop = tmpcursortop;
                    Console.CursorLeft = tmpcursorleft;
                    FirstMap = false;
                }
            }
        }

        private void MessageLightStyle()
        {
            Seek(1);
            var light = BitBuffer.ReadString();
            DemoScanner_AddTextMessage(light, "LIGHT", CurrentTime, CurrentTimeString);
        }

        public void MessageUpdateUserInfo()
        {
            var slot = BitBuffer.ReadByte();

            var userid = BitBuffer.ReadInt32();
            var userinfo_string = BitBuffer.ReadString();
            //Console.WriteLine(s);
            if (demo.GsDemoInfo.Header.NetProtocol > 43)
            {
                Seek(16); // string hash
            }

            if (userinfo_string.Length < 5)
            {
                return;
            }

            /*
             * Если s пустая значит
             * ищем игрока с id и присваиваем ему новый слот
             * игрок со старым slot удаляем и перемещаем в другое место
             * */
            Player player = null;
            var playerfound = false;
            var player_in_struct_id = 0;

            // поиск игрока с нужным UserID если он существует
            for (player_in_struct_id = 0; player_in_struct_id < playerList.Count; player_in_struct_id++)
            {
                if (playerList[player_in_struct_id].ServerUserIdLong == userid
                    || (playerList[player_in_struct_id].iSlot == slot && playerList[player_in_struct_id].ServerUserIdLong == -1))
                {
                    playerfound = true;
                    playerList[player_in_struct_id].ServerUserIdLong = userid;
                    player = playerList[player_in_struct_id];
                    break;
                }
            }

            // Если нет создаем нового
            // create player if it doesn't exist
            if (!playerfound)
            {
                player = new Player(slot, userid);
                playerList.Insert(0, player);
                player_in_struct_id = 0;
            }

            var userinfo_string_bak = userinfo_string;
            try
            {
                // parse infokey string
                userinfo_string = userinfo_string.Remove(0, 1); // trim leading slash
                var infoKeyTokens = userinfo_string.Split('\\');
                for (var n = 0; n + 1 < infoKeyTokens.Length; n += 2)
                {
                    if (n + 1 >= infoKeyTokens.Length)
                    {
                        // Must be an odd number of strings - a key without a value - ignore it.
                        break;
                    }

                    var key = infoKeyTokens[n];

                    if (key == "STEAMID")
                    {
                        key = "pid";
                    }

                    if (key.ToLower() == "name")
                    {
                        string newname = infoKeyTokens[n + 1];
                        string oldname = player.UserName;
                        if (newname.Length > 0)
                        {
                            player.UserName = newname;

                            if (LocalPlayerId != -1 && slot == LocalPlayerId)
                            {
                                bool samenames = false;

                                if (LastUsername.Length > 2 && player.UserName.Length > 2)
                                {
                                    samenames = LastUsername.Substring(0, 3).Equals(player.UserName.Substring(0, 3)) &&
                                        player.UserName.IndexOf(" + ") > 0;
                                }

                                if (player.UserName != LastUsername && !samenames)
                                {
                                    if (LastUsername.Length != 0 && LastUsername.IndexOf(player.UserName) != 0 &&
                                       player.UserName.IndexOf(LastUsername) != 0 &&
                                       LocalPlayerUserId == player.ServerUserIdLong)
                                    {
                                        if (IsRussia)
                                        {
                                            DemoScanner_AddInfo("Игрок сменил никнейм с [" + LastUsername + "] на [" + player.UserName + "] на " + CurrentTimeString);
                                        }
                                        else
                                        {
                                            DemoScanner_AddInfo("Player changes nickname from [" + LastUsername + "] to [" + player.UserName + "] at " + CurrentTimeString);
                                        }
                                    }

                                    if (!SKIP_RESULTS)
                                    {
                                        if (LastUsername.Length == 0 ||
                                            (LastUsername.IndexOf(player.UserName) != 0
                                            && player.UserName.IndexOf(LastUsername) != 0)
                                            || player.UserName.Length < LastUsername.Length)
                                        {
                                            var tmpcursortop = Console.CursorTop;
                                            var tmpcursorleft = Console.CursorLeft;
                                            Console.CursorTop = UserNameAndSteamIDField;
                                            Console.CursorLeft = UserNameAndSteamIDField2;
                                            var tmpconsolecolor = Console.ForegroundColor;
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            for (var i = 0; i < 64; i++)
                                            {
                                                Console.Write(" ");
                                            }

                                            Console.CursorLeft = UserNameAndSteamIDField2;
                                            Console.Write(player.UserName.TrimBad().Trim());
                                            Console.ForegroundColor = ConsoleColor.Cyan;
                                            Console.WriteLine(" [" + LastSteam + "]");
                                            Console.ForegroundColor = tmpconsolecolor;
                                            Console.CursorTop = tmpcursortop;
                                            Console.CursorLeft = tmpcursorleft;
                                        }
                                    }

                                    LastUsername = player.UserName;
                                }

                                LocalPlayerUserId = player.ServerUserIdLong;
                            }
                            if (oldname != null && oldname != newname && newname.Length > 0 && oldname.Length > 0)
                            {
                                if (IsRussia)
                                {
                                    DemoScanner_AddTextMessage("Игрок " + userid + " сменил никнейм с [" + oldname + "] на [" + newname + "] на " + CurrentTimeString, "PLAYERINFO", CurrentTime, CurrentTimeString);
                                }
                                else
                                {
                                    DemoScanner_AddTextMessage("Player " + userid + " changes nickname from [" + oldname + "] to [" + newname + "] at " + CurrentTimeString, "PLAYERINFO", CurrentTime, CurrentTimeString);
                                }
                            }
                        }
                    }

                    if (key == "*sid")
                    {
                        key = "STEAMID";

                        string tmpSteamKey = infoKeyTokens[n + 1];
                        if (tmpSteamKey.Length > 0)
                        {
                            infoKeyTokens[n + 1] = CalculateSteamId(infoKeyTokens[n + 1]);
                            string oldsteam = player.UserSteamId;
                            player.UserSteamId = infoKeyTokens[n + 1];

                            if (LocalPlayerId != -1 && slot == LocalPlayerId)
                            {
                                if (LastSteam != null && LastSteam.Length != 0 && LastSteam != player.UserSteamId
                                    && LocalPlayerUserId2 == player.ServerUserIdLong)
                                {
                                    DemoScanner_AddWarn("[STEAMID HACK] FROM [" + LastSteam + "] TO [" + player.UserSteamId +
                                        "] at (" + CurrentTime + ") " + CurrentTimeString, true, true, true);
                                }
                                if (player.UserSteamId != null && player.UserSteamId != LastSteam)
                                {
                                    if (!SKIP_RESULTS)
                                    {
                                        var tmpcursortop = Console.CursorTop;
                                        var tmpcursorleft = Console.CursorLeft;
                                        Console.CursorTop = UserNameAndSteamIDField;
                                        Console.CursorLeft = UserNameAndSteamIDField2;
                                        var tmpconsolecolor = Console.ForegroundColor;
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        for (var i = 0; i < 64; i++)
                                        {
                                            Console.Write(" ");
                                        }

                                        Console.CursorLeft = UserNameAndSteamIDField2;
                                        Console.Write(player.UserName.TrimBad().Trim());
                                        Console.ForegroundColor = ConsoleColor.Cyan;
                                        Console.WriteLine(" [" + player.UserSteamId + "]");
                                        Console.ForegroundColor = tmpconsolecolor;
                                        Console.CursorTop = tmpcursortop;
                                        Console.CursorLeft = tmpcursorleft;
                                    }
                                    LastSteam = player.UserSteamId;
                                }
                                LocalPlayerUserId2 = player.ServerUserIdLong;
                            }
                            if (oldsteam != null && player.UserSteamId != null && oldsteam != player.UserSteamId && oldsteam.Length > 0 && player.UserSteamId.Length > 0)
                            {
                                DemoScanner_AddTextMessage("[STEAMID HACK] USER " + userid + " NAME " + (player.UserName != null ? player.UserName : "ERROR NO NAME") + " FROM [" + LastSteam + "] TO [" + player.UserSteamId +
                                        "] at (" + CurrentTime + ") " + CurrentTimeString, "PLAYERINFO", CurrentTime, CurrentTimeString);
                            }
                        }
                    }

                    // If the key already exists, overwrite it.
                    if (player.InfoKeys.ContainsKey(key))
                    {
                        player.InfoKeys.Remove(key);
                    }

                    player.InfoKeys.Add(key, infoKeyTokens[n + 1]);
                }

                playerList[player_in_struct_id] = player;
            }
            catch
            {
                Console.WriteLine("Error in parsing:" + userinfo_string_bak);
            }
        }

        public void MessageDeltaDescription()
        {
            var structureName = BitBuffer.ReadString();
            if (demo.GsDemoInfo.Header.NetProtocol == 43)
            {
                BitBuffer.Endian = BitBuffer.EndianType.Big;
            }

            var newDeltaStructure = new HalfLifeDeltaStructure(structureName);
            AddDeltaStructure(newDeltaStructure);
            var deltaDescription = GetDeltaStructure("delta_description_t");
            var nEntries = BitBuffer.ReadUnsignedBits(16);
            for (ushort i = 0; i < nEntries; i++)
            {
                var newDelta = deltaDescription.CreateDelta();
                deltaDescription.ReadDelta(BitBuffer, newDelta);
                newDeltaStructure.AddEntry(newDelta);
            }

            BitBuffer.SkipRemainingBits();
            BitBuffer.Endian = BitBuffer.EndianType.Little;
        }

        public void MessageClientData()
        {
            if (DEMOSCANNER_HLTV)
            {
                return;
            }

            ClientDataCountMessages++;
            SVC_CLIENTUPDATEMSGID = DemoScanner.MessageId;
            if (demo.GsDemoInfo.Header.NetProtocol <= 43)
            {
                BitBuffer.Endian = BitBuffer.EndianType.Big;
            }

            var deltaSequence = BitBuffer.ReadBoolean();
            int seqnum = 0;

            if (deltaSequence)
            {
                seqnum = BitBuffer.ReadBits(8); // delta sequence number
            }

            if (DUMP_ALL_FRAMES)
            {
                OutDumpString += "\n[seqnum = " + seqnum + "]\n";
            }

            GetDeltaStructure("clientdata_t").ReadDelta(BitBuffer, null);

            while (BitBuffer.ReadBoolean())
            {
                int idx = 0;

                if (demo.GsDemoInfo.Header.NetProtocol < 47)
                {
                    idx = BitBuffer.ReadBits(5); // weapon index
                }
                else
                {
                    idx = BitBuffer.ReadBits(6); // weapon index
                }

                GetDeltaStructure("weapon_data_t").ReadDelta(BitBuffer, null);
            }

            BitBuffer.SkipRemainingBits();
            BitBuffer.Endian = BitBuffer.EndianType.Little;
            if (SVC_CHOKEMSGID - 1 == SVC_TIMEMSGID || SVC_CLIENTUPDATEMSGID - 1 == SVC_TIMEMSGID ||
                SVC_CLIENTUPDATEMSGID - 1 == SVC_ADDANGLEMSGID || SVC_CLIENTUPDATEMSGID - 1 == SVC_SETANGLEMSGID)
            {
                ;
            }
            else
            {
                if (abs(CurrentTime - speedhackdetect_time) > 5.0)
                {
                    DemoScanner_AddWarn("[FAKESPEED TYPE 2] at (" + CurrentTime + "):" + CurrentTimeString, false);

                    speedhackdetect_time = CurrentTime;
                }
            }
        }

        public void MessageStopSound()
        {
            int ent = BitBuffer.ReadUInt16();
            if (DUMP_ALL_FRAMES)
            {
                OutDumpString += "[STOP SOUND ENT " + ent + " ]\n";
            }
        }

        public void MessagePings()
        {
            if (demo.GsDemoInfo.Header.NetProtocol <= 43)
            {
                BitBuffer.Endian = BitBuffer.EndianType.Big;
            }

            int players = 0;
            while (BitBuffer.ReadBoolean())
            {
                players++;
                var slotid = BitBuffer.ReadUnsignedBits(5);
                var pings = BitBuffer.ReadUnsignedBits(12);
                var loss = BitBuffer.ReadUnsignedBits(7);
                if (slotid == LocalPlayerId && loss > 0)
                {
                    LastLossPacketCount = loss;

                    if (DUMP_ALL_FRAMES)
                    {
                        OutDumpString += "[SKIP " + loss + " frames(loss packets)]\n";
                    }

                    CheckConsoleCommand("Lost packets: " + loss + ". Ping: " + pings, true);
                    LossPackets++;
                    FrameErrors = LastOutgoingSequence = LastIncomingAcknowledged = LastIncomingSequence = 0;
                    LastLossTime2 = CurrentTime;
                    PluginFrameNum = -1;

                    //if (!LossFalseDetection &&
                    //    (LastLossPacket <= EPSILON || Math.Abs(CurrentTime - LastLossPacket) > 60.0f))
                    //{
                    //    LossFalseDetection = true;

                    //    var col = Console.ForegroundColor;
                    //    Console.ForegroundColor = ConsoleColor.Red;
                    //    if (TriggerAimAttackCount > 0) TriggerAimAttackCount--;

                    //    if (TotalAimBotDetected > 0) TotalAimBotDetected--;

                    //    if (KreedzHacksCount > 0) KreedzHacksCount--;

                    //    if (JumpHackCount2 > 0) JumpHackCount2--;

                    //    if (IsRussia)
                    //        Console.WriteLine("[ЛАГ] Предупреждение! Игрок завис и один детект может быть ложным!");
                    //    else
                    //        Console.WriteLine("[LAG] Warning! Player has lag and previous detection can be false!");

                    //    Console.ForegroundColor = col;
                    //}

                    if (abs(CurrentTime - LastScoreTime) > 0.75f &&
                        abs(LastLossPacket) > EPSILON && abs(CurrentTime - LastLossPacket) < 0.75f)
                    {
                        if (abs(CurrentTime - LastFakeLagTime) > 20.0f)
                        {
                            DemoScanner_AddWarn("[FAKELAG TYPE 3.1] at (" + CurrentTime + "):" + CurrentTimeString, false);
                            LastFakeLagTime = CurrentTime;
                        }
                    }

                    LastLossPacket = CurrentTime;
                }
            }

            if (players > 32)
            {
                if (abs(CurrentTime - LastFakeLagTime) > 20.0f)
                {
                    DemoScanner_AddWarn("[FAKELAG TYPE 3.2] at (" + CurrentTime + "):" + CurrentTimeString, false);
                    LastFakeLagTime = CurrentTime;
                }
            }

            BitBuffer.SkipRemainingBits();
            BitBuffer.Endian = BitBuffer.EndianType.Little;
        }

        private void MessageSpawnStatic()
        {
            int modelindex = BitBuffer.ReadUInt16();
            if (DUMP_ALL_FRAMES)
            {
                OutDumpString += "MessageSpawnStatic:( id " + modelindex + " ){\n";
            }

            BitBuffer.SeekBytes(16);
            var renderMode = BitBuffer.ReadByte();
            if (renderMode != 0)
            {
                BitBuffer.SeekBytes(5);
            }
        }

        private void MessageEventReliable()
        {
            if (demo.GsDemoInfo.Header.NetProtocol <= 43)
            {
                BitBuffer.Endian = BitBuffer.EndianType.Big;
            }

            var nIndex = BitBuffer.ReadBits(10); // event index
            if (DUMP_ALL_FRAMES)
            {
                OutDumpString += "MessageEventReliable:( id " + nIndex + " ){\n";
            }


            //foreach (var res in DownloadedResources)
            //{
            //    if (res.res_index == nIndex && res.res_type == 5)
            //    {
            //        Console.WriteLine("Event3:" + res.res_path + " at " + CurrentTimeString);
            //    }
            //}

            GetDeltaStructure("event_t").ReadDelta(BitBuffer, null);

            var delayBit = BitBuffer.ReadBoolean();
            if (delayBit)
            {
                var delay_reliable = BitBuffer.ReadBits(16); // delay / 100.0f
                if (DUMP_ALL_FRAMES)
                {
                    OutDumpString += "EventDelay:( " + delay_reliable / 100.0f + " ){\n";
                }
            }

            BitBuffer.SkipRemainingBits();
            BitBuffer.Endian = BitBuffer.EndianType.Little;
        }

        public void MessageSpawnBaseline()
        {
            if (demo.GsDemoInfo.Header.NetProtocol <= 43)
            {
                BitBuffer.Endian = BitBuffer.EndianType.Big;
            }

            while (true)
            {
                var entityIndex = BitBuffer.ReadUnsignedBits(11);
                if (entityIndex == (1 << 11) - 1) // all 1's
                {
                    break;
                }

                var entityType = BitBuffer.ReadUnsignedBits(2);
                string entityTypeString;
                if ((entityType & 1) != 0) // is bit 1 set?
                {
                    if (entityIndex > 0 && entityIndex <= demo.MaxClients)
                    {
                        if (DUMP_ALL_FRAMES)
                        {
                            OutDumpString += "\nPL_ENT:" + entityIndex;
                            if (entityIndex == LocalPlayerId + 1)
                            {
                                OutDumpString += "(LOCALPLAYER)";
                            }
                        }


                        LastPlayerEntity = entityIndex;
                        entityTypeString = "entity_state_player_t";
                    }
                    else
                    {
                        if (DUMP_ALL_FRAMES)
                        {
                            OutDumpString += "\nENT:" + entityIndex;
                        }

                        LastEntity = entityIndex;
                        entityTypeString = "entity_state_t";
                    }
                }
                else
                {
                    if (DUMP_ALL_FRAMES)
                    {
                        OutDumpString += "\nENT:" + entityIndex;
                    }

                    entityTypeString = "custom_entity_state_t";
                }

                GetDeltaStructure(entityTypeString).ReadDelta(BitBuffer, null);
            }

            var footer = BitBuffer.ReadUnsignedBits(5); // should be all 1's
            if (footer != (1 << 5) - 1)
            {
                throw new ApplicationException("Bad svc_spawnbaseline footer.");
            }

            var nExtraData = BitBuffer.ReadUnsignedBits(6);
            for (var i = 0; i < nExtraData; i++)
            {
                GetDeltaStructure("entity_state_t").ReadDelta(BitBuffer, null);
            }

            BitBuffer.Endian = BitBuffer.EndianType.Little;
            BitBuffer.SkipRemainingBits();
        }

        private void MessageTempEntity()
        {
            var type = BitBuffer.ReadByte();
            // if (DemoScanner.CurrentTime > 1471 && DemoScanner.CurrentTime < 1472)
            // Console.WriteLine("Type:" + type);
            switch (type)
            {
                // obsolete
                case 16: // TE_BEAM
                case 26: // TE_BEAMHOSE
                    break;
                // simple coord format messages
                case 2: // TE_GUNSHOT
                case 4: // TE_TAREXPLOSION 
                case 9: // TE_SPARKS
                case 10: // TE_LAVASPLASH
                case 11: // TE_TELEPORT
                    Seek(6);
                    break;
                case 0: // TE_BEAMPOINTS
                    Seek(24);
                    break;
                case 1: // TE_BEAMENTPOINT
                    LastBeamFound = CurrentTime;
                    Seek(20);
                    break;
                case 3: // TE_EXPLOSION
                    Seek(11);
                    break;
                case 5: // TE_SMOKE
                    Seek(10);
                    break;
                case 6: // TE_TRACER
                    Seek(12);
                    break;
                case 7: // TE_LIGHTNING 
                    Seek(17);
                    break;
                case 8: // TE_BEAMENTS
                    Seek(16);
                    break;
                case 12: // TE_EXPLOSION2
                    Seek(8);
                    break;
                case 13: // TE_BSPDECAL
                    Seek(8);
                    var entityIndex = BitBuffer.ReadUInt16();
                    if (entityIndex != 0)
                    {
                        Seek(2);
                    }

                    break;
                case 14: // TE_IMPLOSION
                    Seek(9);
                    break;
                case 15: // TE_SPRITETRAIL
                    Seek(19);
                    break;
                case 17: // TE_SPRITE
                    Seek(10);
                    break;
                case 18: // TE_BEAMSPRITE
                    Seek(16);
                    break;
                case 19: // TE_BEAMTORUS
                case 20: // TE_BEAMDISK
                case 21: // TE_BEAMCYLINDER
                    Seek(24);
                    break;
                case 22: // TE_BEAMFOLLOW
                    Seek(10);
                    break;
                case 23: // TE_GLOWSPRITE
                    // SDK is wrong
                    /*
                        write_coord()	 position
                        write_coord()
                        write_coord()
                        write_short()	 model index
                        write_byte()	 life in 0.1's
                        write_byte()	scale in 0.1's
                        write_byte()	brightness
                    */
                    Seek(11);
                    break;
                case 24: // TE_BEAMRING
                    Seek(16);
                    break;
                case 25: // TE_STREAK_SPLASH
                    Seek(19);
                    break;
                case 27: // TE_DLIGHT
                    Seek(12);
                    break;
                case 28: // TE_ELIGHT
                    Seek(16);
                    break;
                case 29: // TE_TEXTMESSAGE
                    CurrentMsgHudCount++;
                    Seek(5);
                    var textParmsEffect = BitBuffer.ReadByte();
                    Seek(14);
                    if (textParmsEffect == 2)
                    {
                        Seek(2);
                    }

                    var hudmsg = BitBuffer.ReadString();
                    DemoScanner_AddTextMessage(hudmsg, "TE_HUD", CurrentTime, CurrentTimeString);
                    break;
                case 30: // TE_LINE
                case 31: // TE_BOX
                    Seek(17);
                    break;
                case 99: // TE_KILLBEAM
                    LastBeamFound = CurrentTime;
                    Seek(2);
                    break;
                case 100: // TE_LARGEFUNNEL
                    Seek(10);
                    break;
                case 101: // TE_BLOODSTREAM
                    Seek(14);
                    break;
                case 102: // TE_SHOWLINE
                    Seek(12);
                    break;
                case 103: // TE_BLOOD
                    Seek(14);
                    break;
                case 104: // TE_DECAL
                    Seek(9);
                    break;
                case 105: // TE_FIZZ
                    Seek(5);
                    break;
                case 106: // TE_MODEL
                    // WRITE_ANGLE could be a short..
                    Seek(17);
                    break;
                case 107: // TE_EXPLODEMODEL
                    Seek(13);
                    break;
                case 108: // TE_BREAKMODEL
                    Seek(24);
                    break;
                case 109: // TE_GUNSHOTDECAL
                    Seek(9);
                    break;
                case 110: // TE_SPRITE_SPRAY
                    Seek(17);
                    break;
                case 111: // TE_ARMOR_RICOCHET
                    Seek(7);
                    break;
                case 112: // TE_PLAYERDECAL (could be a trailing short after this, apparently...)
                    Seek(10);
                    break;
                case 113: // TE_BUBBLES
                case 114: // TE_BUBBLETRAIL
                    Seek(19);
                    break;
                case 115: // TE_BLOODSPRITE
                    Seek(12);
                    break;
                case 116: // TE_WORLDDECAL
                case 117: // TE_WORLDDECALHIGH
                    Seek(7);
                    break;
                case 118: // TE_DECALHIGH
                    Seek(9);
                    break;
                case 119: // TE_PROJECTILE
                    Seek(16);
                    break;
                case 120: // TE_SPRAY
                    Seek(18);
                    break;
                case 121: // TE_PLAYERSPRITES
                    Seek(5);
                    break;
                case 122: // TE_PARTICLEBURST
                    Seek(10);
                    break;
                case 123: // TE_FIREFIELD
                    BitBuffer.ReadInt16();
                    BitBuffer.ReadInt16();
                    BitBuffer.ReadInt16();
                    BitBuffer.ReadInt16();
                    BitBuffer.ReadInt16();
                    BitBuffer.ReadByte();
                    BitBuffer.ReadByte();
                    BitBuffer.ReadByte();
                    break;
                case 124: // TE_PLAYERATTACHMENT
                    Seek(7);
                    break;
                case 125: // TE_KILLPLAYERATTACHMENTS
                    Seek(1);
                    break;
                case 126: // TE_MULTIGUNSHOT
                    Seek(18);
                    break;
                case 127: // TE_USERTRACER
                    Seek(15);
                    break;
                default:
                    throw new ApplicationException(string.Format("Unknown tempentity type \"{0}\".", type));
            }
            // BitBuffer.SkipRemainingBits();
        }

        private void MessageSign()
        {
            int signid = BitBuffer.ReadByte();
            if (DEBUG_ENABLED)
            {
                Console.Write("sign num:" + signid + "\n");
            }
        }

        private void MessageSetPause()
        {
            var pause = BitBuffer.ReadByte() > 0;
            var tmpcolor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Gray;
            if (IsRussia)
            {
                Console.WriteLine("[ПАУЗА] Сервер был поставлен на паузу. Срабатывание может быть ложным! Время " +
                                  CurrentTimeString + ". ");
            }
            else
            {
                Console.WriteLine("[POSSIBLE FALSE DETECTION BELLOW!] Warning!!! Server " +
                                  (pause ? "paused" : "unpaused") + " at " + CurrentTimeString + ". ");
            }

            Console.ForegroundColor = tmpcolor;
        }

        private void MessageCenterPrint()
        {
            CurrentMsgPrintCount++;
            var msgprint = BitBuffer.ReadString();
            if (DEBUG_ENABLED)
            {
                Console.Write("msgcenterprint:" + msgprint);
            }

            if (DUMP_ALL_FRAMES)
            {
                OutDumpString += "MessageCenterPrint:" + msgprint;
            }

            if (msgprint.IndexOf("%s") == 0)
            {
                var msgprint2 = BitBuffer.ReadString();
                if (DUMP_ALL_FRAMES)
                {
                    OutDumpString += "->" + msgprint2;
                }

                if (DEBUG_ENABLED)
                {
                    Console.Write("..bad msgcenterprint?.." + msgprint + ">>>>>" + msgprint2);
                }

                msgprint += "|" + msgprint2;
            }

            DemoScanner_AddTextMessage(msgprint, "PRINT_CENTER", CurrentTime, CurrentTimeString);
        }

        private void MessageInterMission()
        {
            if (CL_Intermission == 0)
            {
                if (IsRussia)
                {
                    Console.WriteLine("---------- Подготовка к смене уровня [" + CurrentTimeString +
                                 "] ----------");
                }
                else
                {
                    Console.WriteLine("---------- Preparing to changelevel [" + CurrentTimeString +
                                 "] ----------");
                }
            }
            CL_Intermission = 1;
        }

        public void MessageNewUserMsg()
        {
            var id = BitBuffer.ReadByte();
            var length = BitBuffer.ReadSByte();
            var name = BitBuffer.ReadString(16);
            if (DUMP_ALL_FRAMES)
            {
                OutDumpString += "user msg:" + name + " len:" + length + " id " + id + "\n";
            }

            AddUserMessage(id, length, name);
        }

        public void MessagePacketEntities()
        {
            var num_ents = BitBuffer.ReadBits(16); // num entities (not reliable at all, loop until footer - see below)
            if (DUMP_ALL_FRAMES)
            {
                OutDumpString += "MessagePacketEntities->total_num_ents:" + num_ents + "\n";
            }

            if (demo.GsDemoInfo.Header.NetProtocol <= 43)
            {
                BitBuffer.Endian = BitBuffer.EndianType.Big;
            }

            uint entityNumber = 0;
            // begin entity parsing
            while (true)
            {
                var footer = BitBuffer.ReadUInt16();
                if (footer == 0)
                {
                    break;
                }

                BitBuffer.SeekBits(-16);
                var entityNumberIncrement = BitBuffer.ReadBoolean();
                if (!entityNumberIncrement) // entity number isn't last entity number + 1, need to read it in
                {
                    // is the following entity number absolute, or relative from the last one?
                    var absoluteEntityNumber = BitBuffer.ReadBoolean();
                    if (absoluteEntityNumber)
                    {
                        entityNumber = BitBuffer.ReadUnsignedBits(11);
                    }
                    else
                    {
                        entityNumber += BitBuffer.ReadUnsignedBits(6);
                    }
                }
                else
                {
                    entityNumber++;
                }

                if (demo.GsDemoInfo.Header.GameDir == "tfc")
                {
                    BitBuffer.ReadBoolean(); // unknown
                }

                var custom = BitBuffer.ReadBoolean();
                var useBaseline = BitBuffer.ReadBoolean();
                if (useBaseline)
                {
                    BitBuffer.SeekBits(6); // baseline index
                }

                var entityType = "entity_state_t";
                if (entityNumber > 0 && entityNumber <= demo.MaxClients)
                {
                    if (DUMP_ALL_FRAMES)
                    {
                        OutDumpString += "\nPID1:" + entityNumber;
                    }

                    LastPlayerEntity = entityNumber;
                    entityType = "entity_state_player_t";
                }
                else if (custom)
                {
                    if (DUMP_ALL_FRAMES)
                    {
                        OutDumpString += "\nENT:" + entityNumber;
                    }

                    LastEntity = entityNumber;
                    entityType = "custom_entity_state_t";
                }

                GetDeltaStructure(entityType).ReadDelta(BitBuffer, null);
            }

            BitBuffer.SkipRemainingBits();
            BitBuffer.Endian = BitBuffer.EndianType.Little;
        }

        public void MessageDeltaPacketEntities()
        {
            var num_ents = BitBuffer.ReadBits(16); // num entities (not reliable at all, loop until footer - see below)
            var seq_number = BitBuffer.ReadByte(); // delta sequence number
            if (DUMP_ALL_FRAMES)
            {
                OutDumpString += "MessageDeltaPacketEntities->total_num_ents:" + num_ents + ", seq_num:" + seq_number +
                                 " (" + seq_number.ToString("X2") + ")\n";
            }

            if (demo.GsDemoInfo.Header.NetProtocol <= 43)
            {
                BitBuffer.Endian = BitBuffer.EndianType.Big;
            }

            uint entityNumber = 0;
            while (true)
            {
                var footer = BitBuffer.ReadUInt16();
                if (footer == 0)
                {
                    break;
                }

                BitBuffer.SeekBits(-16);
                var removeEntity = BitBuffer.ReadBoolean();
                // is the following entity number absolute, or relative from the last one?
                var absoluteEntityNumber = BitBuffer.ReadBoolean();
                if (absoluteEntityNumber)
                {
                    entityNumber = BitBuffer.ReadUnsignedBits(11);
                }
                else
                {
                    entityNumber += BitBuffer.ReadUnsignedBits(6);
                }

                if (entityNumber > 0 && entityNumber <= demo.MaxClients)
                {
                    LastPlayerEntity = entityNumber;
                    if (DUMP_ALL_FRAMES)
                    {
                        OutDumpString += "\nPL_ENT:" + entityNumber;
                        if (entityNumber == LocalPlayerId + 1)
                        {
                            OutDumpString += "(LOCALPLAYER)";
                        }
                    }
                }
                else
                {
                    LastEntity = entityNumber;
                    if (DUMP_ALL_FRAMES)
                    {
                        OutDumpString += "\nENT:" + entityNumber;
                    }
                }

                if (!removeEntity)
                {
                    if (demo.GsDemoInfo.Header.GameDir == "tfc")
                    {
                        BitBuffer.ReadBoolean(); // unknown
                    }

                    var custom = BitBuffer.ReadBoolean();
                    if (demo.GsDemoInfo.Header.NetProtocol <= 43)
                    {
                        BitBuffer.SeekBits(1); // unknown
                    }

                    var entityType = "entity_state_t";
                    if (entityNumber > 0 && entityNumber <= demo.MaxClients)
                    {
                        entityType = "entity_state_player_t";
                    }
                    else if (custom)
                    {
                        entityType = "custom_entity_state_t";
                    }

                    GetDeltaStructure(entityType).ReadDelta(BitBuffer, null);
                }
            }

            BitBuffer.SkipRemainingBits();
            BitBuffer.Endian = BitBuffer.EndianType.Little;
        }

        private void MessageResourceList()
        {
            DownloadedResources.Clear();
            if (demo.GsDemoInfo.Header.NetProtocol <= 43)
            {
                BitBuffer.Endian = BitBuffer.EndianType.Big;
            }

            var nEntries = BitBuffer.ReadUnsignedBits(12);
            for (var i = 0; i < nEntries; i++)
            {
                var type = BitBuffer.ReadUnsignedBits(4);
                var downloadname = BitBuffer.ReadString();
                var index = BitBuffer.ReadUnsignedBits(12);
                ulong downloadsize = BitBuffer.ReadUnsignedBits(24);
                var flags = BitBuffer.ReadUnsignedBits(3);

                if ((flags & 4) != 0) // md5 hash
                {
                    BitBuffer.ReadBytes(16);
                }

                var has_extra = BitBuffer.ReadBoolean();
                if (has_extra)
                {
                    BitBuffer.ReadBytes(32);
                }

                if (type == 0)
                {
                    DownloadedResources.Add(new RESOURCE_STRUCT(type, "sound/" + downloadname, index, flags));
                }
                else
                {
                    DownloadedResources.Add(new RESOURCE_STRUCT(type, downloadname, index, flags));
                }
                DownloadResourcesSize += downloadsize;
                if (downloadname.Length >= 64)
                {
                    Console.WriteLine("[Server issue] Filename \"" + downloadname + "\" length > 63.");
                }
            }

            var CleanDownloadedResources = DownloadedResources.OrderBy(r => r.res_type).ThenBy(r => r.res_index).ToList();
            CleanDownloadedResources = CleanDownloadedResources.Distinct().ToList();

            if (DownloadedResources.Count != CleanDownloadedResources.Count)
            {
                Console.WriteLine("[Server issue] Removed " + (DownloadedResources.Count - CleanDownloadedResources.Count) + " duplicated resources.");
            }

            DownloadedResources = CleanDownloadedResources;

            //foreach (var res in DownloadedResources)
            //{
            //    Console.WriteLine(res.ToString() + " at " + CurrentTimeString);
            //}

            // consistency list
            // indices of resources to force consistency upon?
            if (BitBuffer.ReadBoolean())
            {
                while (BitBuffer.ReadBoolean())
                {
                    var nBits = BitBuffer.ReadBoolean() ? 5 : 10;
                    BitBuffer.SeekBits(nBits); // consistency index
                }
            }

            BitBuffer.SkipRemainingBits();
            BitBuffer.Endian = BitBuffer.EndianType.Little;
        }


        private void MessageChoke()
        {
            if (DUMP_ALL_FRAMES)
            {
                OutDumpString += "CHOKE!\n";
            }

            ChokePackets += 1;
            SVC_CHOKEMSGID = DemoScanner.MessageId;
            if (SVC_CHOKEMSGID - 1 == SVC_SETANGLEMSGID || SVC_CHOKEMSGID - 1 == SVC_TIMEMSGID)
            {
                ;
            }
            else
            {
                if (abs(CurrentTime - LastFakeLagTime) > 5.0f)
                {
                    DemoScanner_AddWarn("[FAKELAG TYPE 2] at (" + CurrentTime + "):" + CurrentTimeString, false);

                    LastFakeLagTime = CurrentTime;
                }
            }

            LastChokePacket = CurrentTime;
        }

        private void MessageNewMoveVars()
        {
            // TODO: see OHLDS, SV_SetMoveVars
            /*
            MSG_WriteFloat(buf, movevars.gravity);
           MSG_WriteFloat(buf, movevars.stopspeed);
           MSG_WriteFloat(buf, movevars.maxspeed);
           MSG_WriteFloat(buf, movevars.spectatormaxspeed);
           MSG_WriteFloat(buf, movevars.accelerate);
           MSG_WriteFloat(buf, movevars.airaccelerate);
           MSG_WriteFloat(buf, movevars.wateraccelerate);
           MSG_WriteFloat(buf, movevars.friction);
           MSG_WriteFloat(buf, movevars.edgefriction);
           MSG_WriteFloat(buf, movevars.waterfriction);
           MSG_WriteFloat(buf, movevars.entgravity);
           MSG_WriteFloat(buf, movevars.bounce);
           MSG_WriteFloat(buf, movevars.stepsize);
           MSG_WriteFloat(buf, movevars.maxvelocity);
           MSG_WriteFloat(buf, movevars.zmax);
           MSG_WriteFloat(buf, movevars.waveHeight);
           MSG_WriteByte(buf, (movevars.footsteps != 0)); //Sets it to 1 if nonzero, just in case someone's abusing the whole 'bool' thing.
           MSG_WriteFloat(buf, movevars.rollangle);
           MSG_WriteFloat(buf, movevars.rollspeed);
           MSG_WriteFloat(buf, movevars.skycolor_r);
           MSG_WriteFloat(buf, movevars.skycolor_g);
           MSG_WriteFloat(buf, movevars.skycolor_b);
           MSG_WriteFloat(buf, movevars.skyvec_x);
           MSG_WriteFloat(buf, movevars.skyvec_y);
           MSG_WriteFloat(buf, movevars.skyvec_z);
           MSG_WriteString(buf, movevars.skyName);
            */
            // same as gamedata header
            // 800, 75, 900, 500, 5, 10, 10, 4, 2, 1, 1, 1, 18, 2000, 6400, 0, 1, 0
            // + 24 bytes of unknown
            // different size in network protocols < 45 like gamedata frame header???
            Seek(97);
            BitBuffer.ReadString();
        }

        private void MessageCustomization()
        {
            BitBuffer.ReadByte();
            BitBuffer.ReadByte();
            BitBuffer.ReadString();
            BitBuffer.ReadUInt16();
            BitBuffer.ReadUInt32();
            var resourceflags = BitBuffer.ReadByte();
            if ((resourceflags & (1 << 2)) > 0)
            {
                BitBuffer.ReadBytes(16);
            }

            BitBuffer.SkipRemainingBits();
        }

        private void MessageFileTransferFailed()
        {
            CurrentMsgPrintCount++;
            // string: filename
            var fail_msg = BitBuffer.ReadString();
            DemoScanner_AddTextMessage(fail_msg, "FILE_FAILED", CurrentTime, CurrentTimeString);
        }

        private void MessageHltv()
        {
            var subCommand = BitBuffer.ReadByte();
            if (subCommand == 0)
            {
                LocalPlayerId = 0;
                LocalPlayerUserId = 0;
                LocalPlayerUserId2 = 0;
                AlternativeTimeCounter = 1;
                DEMOSCANNER_HLTV = true;
                var backcolor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.DarkRed;
                if (IsRussia)
                {
                    DemoScanner_AddInfo("Внимание! Демо файл отмечен как HLTV.");
                    DemoScanner_AddInfo("Сканирование POV демо на наличие читов отключается.");
                }
                else
                {
                    DemoScanner_AddInfo("Warning! Demo file marked as HLTV.");
                    DemoScanner_AddInfo("POV demo HACK analyze is disabled.");
                }
                Console.ForegroundColor = backcolor;
            }
            else if (subCommand == 1) // HLTV_STATUS
            {
                BitBuffer.ReadBytes(10);
            }
            else if (subCommand == 2)
            {
                var hltv_msg = BitBuffer.ReadString();
                DemoScanner_AddTextMessage(hltv_msg, "HLTV", CurrentTime, CurrentTimeString);
            }
            else if (subCommand > 0)
            {

            }
        }

        public static string ByteArrayToString(byte[] ba)
        {
            return BitConverter.ToString(ba).Replace("-", "");
        }

        private void MessageDirector()
        {
            var len = BitBuffer.ReadByte(); // DIRECTOR CMD LEN
            var msgtype = BitBuffer.ReadByte(); // DIRECTOR TYPE
            if (msgtype == 10) // DIRECTOR STUFF CMD
            {
                var stuffstr = BitBuffer.ReadString();
                CurrentMsgStuffCmdCount++;
                CheckConsoleCommand(stuffstr, true);
                LastStuffCmdCommand = stuffstr;
                DemoScanner_AddTextMessage(stuffstr, "DIRECTOR_CMD", CurrentTime, CurrentTimeString);
            }
            else if (msgtype == 6) // DIRECTOR HUD
            {
                BitBuffer.ReadByte(); // effects
                BitBuffer.ReadUInt32(); // color
                BitBuffer.ReadSingle(); // x
                BitBuffer.ReadSingle(); // y
                BitBuffer.ReadSingle(); // fadein
                BitBuffer.ReadSingle(); // fadeout
                BitBuffer.ReadSingle(); // holdtime
                BitBuffer.ReadSingle(); // fxtime
                var dhudstr = BitBuffer.ReadString();
                DemoScanner_AddTextMessage(dhudstr, "DIRECTOR_HUD", CurrentTime, CurrentTimeString);
                CurrentMsgDHudCount++;
            }
            else if (len > 0)
            {
                ByteArrayToString(BitBuffer.ReadBytes(len - 1));
            }
        }

        private void MessageVoiceInit()
        {
            // string: codec name (sv_voicecodec, either voice_miles or voice_speex)
            // byte: quality (sv_voicequality, 1 to 5)
            var tmpcodecname = BitBuffer.ReadString();
            if (tmpcodecname.Length > 0)
            {
                VoiceCodec = tmpcodecname;
            }

            //MessageBox.Show(codecname);
            if (DUMP_ALL_FRAMES)
            {
                OutDumpString += "MessageVoiceInit:" + tmpcodecname + "\n";
            }

            if (demo.GsDemoInfo.Header.NetProtocol >= 47)
            {
                if (tmpcodecname.Length > 0)
                {
                    VoiceQuality = BitBuffer.ReadByte();
                }
            }

            DemoScanner_AddTextMessage(tmpcodecname, "INIT_CODEC", CurrentTime, CurrentTimeString);
        }

        public static List<Player> tempVoicePlayer = new List<Player>();

        private void MessageVoiceData()
        {
            // byte: client id/slot?
            // short: data length
            // length bytes: data
            int playerid = BitBuffer.ReadByte();
            var length = BitBuffer.ReadUInt16();
            var data = BitBuffer.ReadBytes(length);

            if (DUMP_ALL_FRAMES)
            {
                OutDumpString += "MessageVoiceData: SLOT:" + playerid + " LEN:" + length + "\n";
            }

            bool found = false;
            for (var i = 0; i < playerList.Count; i++)
            {
                if (playerList[i].iSlot == playerid)
                {
                    found = true;
                    playerList[i].WriteVoice(length, data);
                    break;
                }
            }

            if (!found)
            {
                playerList.Add(new Player(playerid, -1));
                playerList[playerList.Count - 1].WriteVoice(length, data);
            }
        }

        private void MessageSendExtraInfo()
        {
            // string: "com_clientfallback", always seems to be null
            // byte: sv_cheats
            // NOTE: had this backwards before, shouldn't matter
            var extra = BitBuffer.ReadString();
            DemoScanner_AddTextMessage(extra, "SEARCH_EXTRA", CurrentTime, CurrentTimeString);
            FileDirectories.Add(extra);
            Seek(1);
        }

        private void MessageResourceLocation()
        {
            // string: location?
            var tmpDownLocation = BitBuffer.ReadString();
            if (tmpDownLocation.Length > 0 && tmpDownLocation.ToLower().StartsWith("http"))
            {
                DemoScanner_AddTextMessage(tmpDownLocation, "DOWNLOAD_URL", CurrentTime, CurrentTimeString);
                if (DownloadLocation.Length > 0)
                {
                    downloadlocationchanged++;
                    tmpDownLocation = tmpDownLocation.Trim();
                    if (downloadlocationchanged > 1)
                    {
                        if (!IsRussia)
                        {
                            DemoScanner_AddInfo("[WARN] Download URL changed during game!");
                            DemoScanner_AddInfo("[WARN] New download location: " + tmpDownLocation);
                        }
                        else
                        {
                            DemoScanner_AddInfo("[WARN] Ссылка на загрузку ресурсов изменена во время игры!");
                            DemoScanner_AddInfo("[WARN] Новый адрес: " + tmpDownLocation);
                        }
                    }
                }
                DownloadLocation = tmpDownLocation;
            }
            else
            {
                plugin_all_packets++;
                if (!FoundFirstPluginPacket)
                {
                    FoundFirstPluginPacket = true;
                    if (IsRussia)
                    {
                        DemoScanner_AddInfo("[HELLO] На сервере установлен демо-плагин! Поможет найти больше читов :)", true);
                    }
                    else
                    {
                        DemoScanner_AddInfo("[HELLO] Found unreal demo plugin! It can help to find more cheats :)", true);
                    }
                }

                try
                {
                    ProcessPluginMessage(tmpDownLocation);
                }
                catch
                {
                    Console.WriteLine("Error in demo scanner AMXX plugin. Please update to new version.");
                }
            }
        }

        private void MessageSendCvarValue()
        {
            var cvarname = BitBuffer.ReadString(); // The cvar.
            DemoScanner_AddTextMessage(cvarname, "CVAR_REQUEST", CurrentTime, CurrentTimeString);
        }

        private void MessageRestore()
        {
            var restore = BitBuffer.ReadString(); // CL_Restore(str)
            DemoScanner_AddTextMessage(restore, "CL_RESTORE", CurrentTime, CurrentTimeString);
        }

        private void MessageReplaceDecal()
        {
            int decalid = BitBuffer.ReadByte(); // decal
            var decalname = BitBuffer.ReadString(); // DecalSetName(decal,name)
            DemoScanner_AddTextMessage("ID:" + decalid + " = " + decalname, "SET_DECAL", CurrentTime,
                CurrentTimeString);
        }

        private void MessageTFC_Exec()
        {
            if (BitBuffer.ReadByte() == 1)
            {
                BitBuffer.ReadByte();
            }
        }

        private void MessageCutSceneCenterPrint()
        {
            CurrentMsgPrintCount++;
            var msg = BitBuffer.ReadString(); // CenterPrint(str)
            DemoScanner_AddTextMessage(msg, "CENTER_SCENE", CurrentTime, CurrentTimeString);
        }

        private void MessageSendCvarValue2()
        {
            Seek(4); // unsigned int
            var cvarname = BitBuffer.ReadString(); // The cvar.
            DemoScanner_AddTextMessage(cvarname, "CVAR_REQUEST_V2", CurrentTime, CurrentTimeString);
        }

        private void MessageSetAngle()
        {
            var AnglePitch = BitBuffer.ReadUInt16() * (360.0f / 65536);
            var AngleYaw = BitBuffer.ReadUInt16() * (360.0f / 65536);
            var AngleRoll = BitBuffer.ReadUInt16() * (360.0f / 65536);
            if (DUMP_ALL_FRAMES)
            {
                OutDumpString += "Pitch/Yaw/Roll:" + AnglePitch + "/" + AngleYaw + "/" + AngleRoll + ".\n";
            }

            SVC_SETANGLEMSGID = DemoScanner.MessageId;
        }

        private void MessageAddAngle()
        {
            BitBuffer.ReadUInt16();
            if (abs(AimType8WarnTime2) > EPSILON)
            {
                AimType8WarnTime2 = 0.0f;
                BypassWarn8_2++;
            }

            LastAngleManipulation = CurrentTime;
            SVC_ADDANGLEMSGID = DemoScanner.MessageId;
        }

        private void MessageRoomType()
        {
            BitBuffer.ReadUInt16();
            //if (DemoScanner.DEBUG_ENABLED)
            //{
            //    Console.WriteLine("Alive 10 at " + DemoScanner.CurrentTimeString);
            //}
            //if (!DemoScanner.UserAlive)
            //    DemoScanner.DemoScanner_AddTextMessage("Respawn", "USER_INFO", DemoScanner.CurrentTime, DemoScanner.CurrentTimeString);
            //DemoScanner.UserAlive = true;
            //DemoScanner.LastAliveTime = DemoScanner.CurrentTime;
            //DemoScanner.FirstUserAlive = false;
        }

        private void MessageCurWeapon()
        {
            var cStatus = BitBuffer.ReadByte();
            var weaponid_byte = BitBuffer.ReadByte();
            var clip = BitBuffer.ReadByte();
            var weaponid = (WeaponIdType)Enum.ToObject(typeof(WeaponIdType), weaponid_byte);
            byte bad = 0xFF;
            if (cStatus == 0 && weaponid_byte == bad && clip == bad && (UserAlive || FirstUserAlive))
            {
                LastDeathTime = CurrentTime;
                if (!FirstUserAlive)
                {
                    DeathsCoount++;
                }

                if (UserAlive)
                {
                    DemoScanner_AddTextMessage("Death", "USER_INFO", CurrentTime, CurrentTimeString);
                }

                FirstUserAlive = false;
                UserAlive = false;
                RealAlive = false;
                if (DUMP_ALL_FRAMES)
                {
                    OutDumpString += "LocalPlayer  killed. Method : cur weapon!\n";
                }

                if (DEBUG_ENABLED)
                {
                    Console.WriteLine("LocalPlayer killed. Method : cur weapon! at " + CurrentTimeString);
                }

                return;
            }

            if (UsingAnotherMethodWeaponDetection)
            {
                return;
            }

            if ((SkipChangeWeapon != 1 ||
                 (weaponid != WeaponIdType.WEAPON_C4 && weaponid != WeaponIdType.WEAPON_NONE)) &&
                CurrentWeapon != weaponid && cStatus == 1 && (clip > 0 || weaponid == WeaponIdType.WEAPON_KNIFE ||
                                                              weaponid == WeaponIdType.WEAPON_C4 ||
                                                              weaponid == WeaponIdType.WEAPON_HEGRENADE ||
                                                              weaponid == WeaponIdType.WEAPON_SMOKEGRENADE ||
                                                              weaponid == WeaponIdType.WEAPON_FLASHBANG))
            {
                /*if (DemoScanner.UsingAnotherMethodWeaponDetection || !DemoScanner.IsRealWeapon())*/
                if (DEBUG_ENABLED)
                {
                    Console.WriteLine("Select weapon method 1 (" + UsingAnotherMethodWeaponDetection + "):" + weaponid +
                                      ":" + CurrentTimeString);
                }

                //AutoPistolStrikes = 0;
                //LastPrimaryAttackTime = 0.0f;
                //LastPrevPrimaryAttackTime = 0.0f;
                NeedSearchAim2 = false;
                Aim2AttackDetected = false;
                ShotFound = -1;
                SelectSlot = 0;
                WeaponChanged = true;
                ChangeWeaponTime2 = CurrentTime;
                AmmoCount = 0;
                // DemoScanner.IsAttackSkipTimes = 0;
                if (CurrentWeapon != WeaponIdType.WEAPON_NONE)
                {
                    SkipNextAttack = 2;
                }

                //DemoScanner.InitAimMissingSearch = -1;
                IsNoAttackLastTime = CurrentTime + 1.0f;
                NeedCheckAttack = false;
                NeedCheckAttack2 = false;
                //DemoScanner.UserAlive = true;
                if (DUMP_ALL_FRAMES)
                {
                    OutDumpString += "\nCHANGE WEAPON 22 (" + CurrentTime + ") " + CurrentTimeString;
                    OutDumpString += "\nFrom " + CurrentWeapon + " to " + weaponid;
                    Console.WriteLine("CHANGE WEAPON 22 (" + CurrentTime + ") " + CurrentTimeString);
                    Console.WriteLine("From " + CurrentWeapon + " to " + weaponid);
                }

                NeedSearchAim2 = false;
                Aim2AttackDetected = false;
                ShotFound = -1;
                SelectSlot = 0;
                WeaponChanged = true;
                ChangeWeaponTime2 = CurrentTime;
                AmmoCount = 0;
                // DemoScanner.IsAttackSkipTimes = 0;
                //if (DemoScanner.CurrentWeapon != DemoScanner.WeaponIdType.WEAPON_NONE) DemoScanner.SkipNextAttack = 2;
                //DemoScanner.InitAimMissingSearch = -1;
                IsNoAttackLastTime = CurrentTime + 1.0f;
                NeedCheckAttack = false;
                NeedCheckAttack2 = false;
                CurrentWeapon = weaponid;
            }

            SkipChangeWeapon--;
        }

        private void Damage()
        {
            BitBuffer.ReadByte();
            BitBuffer.ReadByte();
            BitBuffer.ReadInt32();
            BitBuffer.ReadInt16();
            BitBuffer.ReadInt16();
            BitBuffer.ReadInt16();
            // after it found NULL byte //fixme
            LastDamageTime = CurrentTime;
            // Console.WriteLine("Damage at " + DemoScanner.CurrentTimeString);
        }

        private void HideWeapon()
        {
            var flags = BitBuffer.ReadByte();
            if ((flags & 4) > 0 || (flags & 1) > 0)
            {
                DemoScanner.HideWeapon = true;
                //**
                HideWeaponTime = CurrentTime;
            }
            else
            {
                DemoScanner.HideWeapon = false;
            }
        }

        private void HLTVMSG()
        {
            var dest = BitBuffer.ReadByte();
            var health_and_flags = BitBuffer.ReadByte();
        }
        /* private void StatusText()
         {
             BitBuffer.ReadByte();
             var dest = BitBuffer.ReadByte();
             var data = BitBuffer.ReadString();
             Console.WriteLine("Line:" + dest + " = \"" + data + "\"");
         }*/

        private void HealthInfo()
        {
            var entid = BitBuffer.ReadByte();
            var health = BitBuffer.ReadInt32();
            if (DUMP_ALL_FRAMES)
            {
                OutDumpString += "\nEnt:" + entid + " = " + health + "HP\n";
            }
        }

        private void WeaponList()
        {
            BitBuffer.ReadByte(); //message length
            BitBuffer.ReadString();
            BitBuffer.ReadSByte();
            BitBuffer.ReadByte();
            BitBuffer.ReadSByte();
            BitBuffer.ReadByte();
            BitBuffer.ReadSByte();
            BitBuffer.ReadSByte();
            BitBuffer.ReadSByte();
            BitBuffer.ReadByte();
            /*Console.Write("/WeaponName:" + BitBuffer.ReadString().Remove(0, 1) + "\n");
                Console.Write("/PrimaryAmmoID:" + BitBuffer.ReadSByte() + "\n");
                Console.Write("/PrimaryAmmoMaxAmount:" + BitBuffer.ReadByte() + "\n");
                Console.Write("/SecondaryAmmoID:" + BitBuffer.ReadSByte() + "\n");
                Console.Write("/SecondaryAmmoMaxAmount:" + BitBuffer.ReadByte() + "\n");
                Console.Write("/SlotID:" + BitBuffer.ReadSByte() + "\n");
                Console.Write("/NumberInSlot:" + BitBuffer.ReadSByte() + "\n");
                Console.Write("/WeaponID:" + BitBuffer.ReadSByte() + "\n");
                Console.Write("/Flags:" + BitBuffer.ReadByte() + "\n");

                Console.WriteLine();*/
        }

        private void WeapPickup()
        {
            var weaponid_byte = BitBuffer.ReadByte();
            var weaponid = (WeaponIdType)Enum.ToObject(typeof(WeaponIdType), weaponid_byte);
            if (UsingAnotherMethodWeaponDetection)
            {
                if (UserAlive)
                {
                    if ( /*!DemoScanner.UsingAnotherMethodWeaponDetection || */
                        !IsRealWeapon())
                    {
                        CurrentWeapon = weaponid;
                    }

                    if (DEBUG_ENABLED)
                    {
                        Console.WriteLine("Select weapon method 2 (" + UsingAnotherMethodWeaponDetection + "):" +
                                          weaponid + ":" + CurrentTimeString);
                    }

                    if (DUMP_ALL_FRAMES)
                    {
                        OutDumpString += "\nSelect weapon(" + UsingAnotherMethodWeaponDetection + "):" + weaponid +
                                         "\n";
                    }
                }
            }
            // else
            // {
            //    DemoScanner.CurrentWeapon = DemoScanner.WeaponIdType.WEAPON_BAD;
            // }
        }

        private void SetFOV()
        {
            var newfov = BitBuffer.ReadByte();
            if (DUMP_ALL_FRAMES)
            {
                OutDumpString += "newfov " + newfov + "\n";
            }

            if (FovByFunc != newfov)
            {
                if (newfov != 0)
                {
                    SkipChangeWeapon = 2;
                }

                if (DEBUG_ENABLED)
                {
                    Console.WriteLine("Change fov from " + FovByFunc + " to " + newfov + " at " + CurrentTimeString);
                }
            }

            if (newfov != FovByFunc && newfov != FovByFunc2)
            {
                FovByFunc2 = FovByFunc;
                FovByFunc = newfov;
            }
        }

        private void TextMsg()
        {
            CurrentMsgPrintCount++;
            var len = BitBuffer.ReadByte(); // message len for -1 len
            var target = (TEXTMSG_Type)BitBuffer.ReadByte(); // message type
            if (target == TEXTMSG_Type.TEXT_PRINTRADIO)
            {
                BitBuffer.ReadString(); // Client Index ?
            }

            var arg1 = BitBuffer.ReadStringMaxLen(256);
            var arg2 = arg1.IndexOf("%s") == 0 || (arg1.IndexOf("#") == 0 && arg1.IndexOf(" ") == -1 &&
                                                   target != TEXTMSG_Type.TEXT_PRINTCENTER)
                ? BitBuffer.ReadStringMaxLen(256)
                : "";
            /*string arg3 = BitBuffer.ReadStringMaxLen(256).Replace("\n", "^n").Replace("\r", "^n")
                .Replace("\x01", "^1").Replace("\x02", "^2")
                .Replace("\x03", "^3").Replace("\x04", "^4");
            string arg4 = BitBuffer.ReadStringMaxLen(256).Replace("\n", "^n").Replace("\r", "^n")
                .Replace("\x01", "^1").Replace("\x02", "^2")
                .Replace("\x03", "^3").Replace("\x04", "^4");
            string arg5 = BitBuffer.ReadStringMaxLen(256).Replace("\n", "^n").Replace("\r", "^n")
                .Replace("\x01", "^1").Replace("\x02", "^2")
                .Replace("\x03", "^3").Replace("\x04", "^4");*/
            //Console.WriteLine("[" + target.ToString() + "]\"" + arg1 + "|" + arg2 /*+ "|" + arg3 + "|" + arg4 + "|" + arg5 + "|" */+ "\"");
            if ( /*arg1 == "#Game_Commencing"
                || */arg1 == "#Game_will_restart_in" || arg1 == "#CTs_Win" || arg1 == "#Terrorists_Win" ||
                     arg1 == "#Round_Draw" || arg1 == "#Target_Bombed" || arg1 == "#Target_Saved" ||
                     arg1 == "#Terrorists_Escaped" || arg1 == "#Terrorists_Not_Escaped" ||
                     arg1 == "#CTs_PreventEscape" || arg1 == "#Escaping_Terrorists_Neutralized" ||
                     arg1 == "#Hostages_Not_Rescued" || arg1 == "#All_Hostages_Rescued" || arg1 == "#VIP_Escaped" ||
                     arg1 == "#VIP_Not_Escaped" || arg1 == "#VIP_Assassinated" || arg1 == "#Bomb_Defused")
            {
                RoundEndTime = CurrentTime;
            }

            if (arg2.Length > 0)
            {
                arg1 = arg1 + "|" + arg2;
            }

            DemoScanner_AddTextMessage(arg1, target.ToString(), CurrentTime, CurrentTimeString);
        }

        private void MessageDeath()
        {
            BitBuffer.ReadByte(); // length
            var iKiller = BitBuffer.ReadByte();
            var iVictim = BitBuffer.ReadByte();
            BitBuffer.ReadByte(); // headshot
            var weapon = BitBuffer.ReadString();
            if (iVictim > 32 || iKiller > 32)
            {
                return;
            }

            if (iVictim == LocalPlayerId + 1 && (UserAlive || FirstUserAlive))
            {
                LastDeathTime = CurrentTime;
                if (!FirstUserAlive)
                {
                    DeathsCoount++;
                }

                if (UserAlive)
                {
                    DemoScanner_AddTextMessage("Killed", "USER_INFO", CurrentTime, CurrentTimeString);
                }

                FirstUserAlive = false;
                UserAlive = false;
                RealAlive = false;
                if (DUMP_ALL_FRAMES)
                {
                    OutDumpString += "LocalPlayer " + iVictim + " killed!\n";
                }

                if (DEBUG_ENABLED)
                {
                    Console.WriteLine("LocalPlayer " + iVictim + " killed! at " + CurrentTimeString);
                }

                if (GameEnd && abs(CurrentTime - GameEndTime) > 5.0)
                {
                    DemoScanner_AddInfo("False game end message. Resetting game status.");
                    GameEnd = false;
                    CL_Intermission = 0;
                }
            }
            else if (iKiller == LocalPlayerId + 1 && iVictim != iKiller)
            {
                KillsCount++;
                if (LastAttackForTrigger == NewAttackForTrigger)
                {
                    if (LastAttackForTriggerFrame != CurrentFrameIdAll && !IsPlayerAttackedPressed() && IsUserAlive() &&
                        !IsCmdChangeWeapon())
                    {
                        var wpntype = GetWeaponByStr(weapon);
                        if (wpntype == WeaponIdType.WEAPON_NONE || wpntype == WeaponIdType.WEAPON_BAD ||
                            wpntype == WeaponIdType.WEAPON_BAD2 || wpntype == WeaponIdType.WEAPON_C4 ||
                            wpntype == WeaponIdType.WEAPON_HEGRENADE || wpntype == WeaponIdType.WEAPON_SMOKEGRENADE ||
                            wpntype == WeaponIdType.WEAPON_FLASHBANG)
                        {
                        }
                        else
                        {
                            if (DemoScanner_AddWarn("[TRIGGER TYPE 2 " + wpntype + "] at (" + CurrentTime + ") " +
                                                CurrentTimeString, !IsPlayerLossConnection()))
                            {
                                TriggerAimAttackCount++;
                                LastTriggerAttack = CurrentTime;
                            }
                        }
                    }

                    LastAttackForTrigger = -1;
                }
                else
                {
                    LastAttackForTrigger = NewAttackForTrigger;
                }

                LastAttackForTriggerFrame = CurrentFrameIdAll;
                if (GameEnd && abs(CurrentTime - GameEndTime) > 5.0)
                {
                    DemoScanner_AddInfo("False game end message. Resetting game status.");
                    GameEnd = false;
                    CL_Intermission = 0;
                }

                if (!UserAlive && (abs(CurrentTime - LastDeathTime) > 10.0f || FirstUserAlive))
                {
                    DemoScanner_AddTextMessage("Respawn[2]", "USER_INFO", CurrentTime, CurrentTimeString);
                    if (FirstUserAlive)
                    {
                        UserAlive = true;
                        if (DEBUG_ENABLED)
                        {
                            Console.WriteLine("Alive 2 at " + CurrentTimeString);
                        }

                        LastAliveTime = CurrentTime;
                    }
                    else
                    {
                        if (!FirstBypassKill)
                        {
                            if (BypassCount > 1)
                            {
                                var tmpcolor = Console.ForegroundColor;
                                Console.ForegroundColor = ConsoleColor.Gray;
                                Console.WriteLine("Warning (???) Tried to bypass demo scanner (" + CurrentTimeString +
                                                  ")");
                                Console.WriteLine("( dead user kill another player! dead user is alive! )");
                                Console.ForegroundColor = tmpcolor;
                            }

                            BypassCount++;
                        }
                        else
                        {
                            if (NeedSkipDemoRescan == 0)
                            {
                                FirstBypassKill = false;
                                NeedSkipDemoRescan = 1;
                            }
                        }
                    }

                    FirstUserAlive = false;
                }
            }

            if (DUMP_ALL_FRAMES)
            {
                OutDumpString += "User " + iVictim + " killed!\n";
            }
        }

        private void MessageResetHud()
        {
        }

        private void MessageHealth()
        {
            BitBuffer.ReadByte();
        }

        private void MessageCrosshair()
        {
            BitBuffer.ReadByte();
        }

        private void MessageSpectator()
        {
            var clientid = BitBuffer.ReadByte();
            BitBuffer.ReadByte();
            if (LocalPlayerId + 1 == clientid)
            {
                if (UserAlive || FirstUserAlive)
                {
                    if (!FirstUserAlive)
                    {
                        DeathsCoount++;
                    }

                    if (UserAlive)
                    {
                        DemoScanner_AddTextMessage("Death[Spectator]", "USER_INFO", CurrentTime, CurrentTimeString);
                    }

                    LastDeathTime = CurrentTime;
                    FirstUserAlive = false;
                    UserAlive = false;
                    RealAlive = false;
                    Console.WriteLine("Forcing user dead because he join to spectator! (" + CurrentTimeString + ")");
                }
            }
        }

        private void MessageScoreAttrib()
        {
            var pid = BitBuffer.ReadByte();
            var flags = BitBuffer.ReadByte();
            if (pid == LocalPlayerId + 1 && (flags & 1) == 0)
            {
                if (!UserAlive)
                {
                    if (DEBUG_ENABLED)
                    {
                        Console.WriteLine("Alive 3 at " + CurrentTimeString);
                    }

                    DemoScanner_AddTextMessage("Respawn[3]", "USER_INFO", CurrentTime, CurrentTimeString);
                    UserAlive = true;
                    LastAliveTime = CurrentTime;
                    FirstUserAlive = false;
                }
            }
        }

        public float UnFixedUnsigned16(ushort value)
        {
            return value / 4096.0f;
        }

        private void MessageScreenFade()
        {
            var duration_enc = BitBuffer.ReadUInt16();
            var holdTime_enc = BitBuffer.ReadUInt16();
            var duration = UnFixedUnsigned16(duration_enc);
            var holdTime = UnFixedUnsigned16(holdTime_enc);
            var fadeFlags = BitBuffer.ReadUInt16();
            if ((fadeFlags & 4) > 0 && !IsScreenFade)
            {
                IsScreenFade = (fadeFlags & 0x0004) > 0;
                DemoScanner_AddInfo("[Full screen fade START] at " + CurrentTimeString);
            }
            else if (!((fadeFlags & 4) > 0) && IsScreenFade)
            {
                IsScreenFade = (fadeFlags & 0x0004) > 0;
                DemoScanner_AddInfo("[Full screen fade END] at " + CurrentTimeString);
            }

            var r = BitBuffer.ReadByte();
            var g = BitBuffer.ReadByte();
            var b = BitBuffer.ReadByte();
            var a = BitBuffer.ReadByte();
            if (DUMP_ALL_FRAMES)
            {
                OutDumpString += "MessageScreenFade. DUR:" + duration + ". HOLD:" + holdTime + ". RGBA:" + r + " " + g +
                                 " " + b + " " + a + "\n";
            }

            if (DEBUG_ENABLED)
            {
                Console.WriteLine("MessageScreenFade. DUR:" + duration + ". HOLD:" + holdTime + ". RGBA:" + r + " " +
                                  g + " " + b + " " + a);
            }
        }

        public const float EPSILON = DemoScanner.EPSILON;

        public static float abs(float val)
        {
            return Math.Abs(val);
        }

        #endregion
    }

    [Serializable]
    public class BitBufferOutOfRangeException : Exception
    {
        public BitBufferOutOfRangeException()
        {
        }

        public BitBufferOutOfRangeException(string message) : base(message)
        {
        }

        public BitBufferOutOfRangeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected BitBufferOutOfRangeException(SerializationInfo serializationInfo, StreamingContext streamingContext)
        {
        }
    }

    public class BitBuffer
    {
        public enum EndianType
        {
            Little,
            Big
        }

        private readonly List<byte> data;

        public BitBuffer(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data", "Value cannot be null.");
            }

            this.data = new List<byte>(data);
            Endian = EndianType.Little;
        }

        public void SeekBits(int count)
        {
            SeekBits(count, SeekOrigin.Current);
        }

        public void SeekBits(int offset, SeekOrigin origin)
        {
            if (origin == SeekOrigin.Current)
            {
                CurrentBit += offset;
            }
            else if (origin == SeekOrigin.Begin)
            {
                CurrentBit = offset;
            }
            else if (origin == SeekOrigin.End)
            {
                CurrentBit = data.Count * 8 - offset;
            }

            if (CurrentBit < 0 || CurrentBit > data.Count * 8)
            {
                throw new BitBufferOutOfRangeException();
            }
        }

        public void SeekBytes(int count)
        {
            SeekBits(count * 8);
        }

        public void SeekBytes(int offset, SeekOrigin origin)
        {
            SeekBits(offset * 8, origin);
        }

        /// <summary>
        ///     Seeks past the remaining bits in the current byte.
        /// </summary>
        public void SkipRemainingBits()
        {
            var bitOffset = CurrentBit % 8;
            if (bitOffset != 0)
            {
                SeekBits(8 - bitOffset);
            }
        }

        // HL 1.1.0.6 bit reading (big endian byte and bit order)
        private uint ReadUnsignedBitsBigEndian(int nBits)
        {
            if (nBits <= 0 || nBits > 32)
            {
                throw new ArgumentException("Value must be a positive integer between 1 and 32 inclusive.", "nBits");
            }

            // check for overflow
            if (CurrentBit + nBits > data.Count * 8)
            {
                throw new BitBufferOutOfRangeException();
            }

            var currentByte = CurrentBit / 8;
            var bitOffset = CurrentBit - currentByte * 8;
            var nBytesToRead = (bitOffset + nBits) / 8;
            if ((bitOffset + nBits) % 8 != 0)
            {
                nBytesToRead++;
            }

            // get bytes we need
            ulong currentValue = 0;
            for (var i = 0; i < nBytesToRead; i++)
            {
                var b = data[currentByte + (nBytesToRead - 1) - i];
                currentValue += (ulong)b << (i * 8);
            }

            // get bits we need from bytes
            currentValue >>= nBytesToRead * 8 - bitOffset - nBits;
            currentValue &= (uint)(((long)1 << nBits) - 1);
            // increment current bit
            CurrentBit += nBits;
            return (uint)currentValue;
        }

        private uint ReadUnsignedBitsLittleEndian(int nBits)
        {
            if (nBits <= 0 || nBits > 32)
            {
                throw new ArgumentException("Value must be a positive integer between 1 and 32 inclusive.", "nBits");
            }

            // check for overflow
            if (CurrentBit + nBits > data.Count * 8)
            {
                throw new BitBufferOutOfRangeException();
            }

            var currentByte = CurrentBit / 8;
            var bitOffset = CurrentBit - currentByte * 8;
            var nBytesToRead = (bitOffset + nBits) / 8;
            if ((bitOffset + nBits) % 8 != 0)
            {
                nBytesToRead++;
            }

            // get bytes we need
            ulong currentValue = 0;
            for (var i = 0; i < nBytesToRead; i++)
            {
                var b = data[currentByte + i];
                currentValue += (ulong)b << (i * 8);
            }

            // get bits we need from bytes
            currentValue >>= bitOffset;
            currentValue &= (uint)(((long)1 << nBits) - 1);
            // increment current bit
            CurrentBit += nBits;
            return (uint)currentValue;
        }

        public uint ReadUnsignedBits(int nBits)
        {
            return Endian == EndianType.Little ? ReadUnsignedBitsLittleEndian(nBits) : ReadUnsignedBitsBigEndian(nBits);
        }

        public int ReadBits(int nBits)
        {
            var result = (int)ReadUnsignedBits(nBits - 1);
            var sign = ReadBoolean() ? 1 : 0;
            if (sign == 1)
            {
                result = -((1 << (nBits - 1)) - result);
            }

            return result;
        }

        public bool ReadBoolean()
        {
            // check for overflow
            if (CurrentBit + 1 > data.Count * 8)
            {
                throw new BitBufferOutOfRangeException();
            }

            var result = (data[CurrentBit / 8] &
                          (Endian == EndianType.Little ? 1 << (CurrentBit % 8) : 128 >> (CurrentBit % 8))) != 0;
            CurrentBit++;
            return result;
        }

        public byte ReadByte()
        {
            return (byte)ReadUnsignedBits(8);
        }

        public sbyte ReadSByte()
        {
            return (sbyte)ReadBits(8);
        }

        public byte[] ReadBytes(int nBytes)
        {
            var result = new byte[nBytes];
            for (var i = 0; i < nBytes; i++)
            {
                result[i] = ReadByte();
            }

            return result;
        }

        public short ReadInt16()
        {
            return (short)ReadBits(16);
        }

        public ushort ReadUInt16()
        {
            return (ushort)ReadUnsignedBits(16);
        }

        public int ReadInt32()
        {
            return ReadBits(32);
        }

        public uint ReadUInt32()
        {
            return ReadUnsignedBits(32);
        }

        public float ReadSingle()
        {
            return BitConverter.ToSingle(ReadBytes(4), 0);
        }

        /// <summary>
        ///     Read a null-terminated string, then skip any remaining bytes to make up length bytes.
        /// </summary>
        /// <param name="length">The total number of bytes to read.</param>
        /// <returns></returns>
        public string ReadString(int length)
        {
            var startBit = CurrentBit;
            var s = ReadString();
            SeekBits(length * 8 - (CurrentBit - startBit));
            return s;
        }

        public string ReadString()
        {
            var bytes = new List<byte>();
            while (true)
            {
                var b = ReadByte();
                if (b == 0x00)
                {
                    break;
                }

                bytes.Add(b);
            }

            return bytes.Count == 0 ? string.Empty : Encoding.UTF8.GetString(bytes.ToArray());
        }

        public string ReadStringMaxLen(int maxlen)
        {
            var bytes = new List<byte>();
            while (true && maxlen-- > 0)
            {
                var b = ReadByte();
                if (b == 0x00)
                {
                    break;
                }

                bytes.Add(b);
            }

            return bytes.Count == 0 ? string.Empty : Encoding.UTF8.GetString(bytes.ToArray());
        }

        public FPoint3D ReadVectorCoord(bool goldSrc = true)
        {
            var xFlag = ReadBoolean();
            var yFlag = ReadBoolean();
            var zFlag = ReadBoolean();
            var fPoint3D = new FPoint3D();
            if (xFlag)
            {
                fPoint3D.X = ReadCoord(goldSrc);
            }

            if (yFlag)
            {
                fPoint3D.Y = ReadCoord(goldSrc);
            }

            if (zFlag)
            {
                fPoint3D.Z = ReadCoord(goldSrc);
            }

            return fPoint3D;
        }

        public float ReadCoord(bool goldSrc)
        {
            var intFlag = ReadBoolean();
            var fractionFlag = ReadBoolean();
            var value = 0.0f;
            if (!intFlag && !fractionFlag)
            {
                return value;
            }

            var sign = ReadBoolean();
            uint intValue = 0;
            uint fractionValue = 0;
            if (intFlag)
            {
                intValue = goldSrc ? ReadUnsignedBits(12) : ReadUnsignedBits(14) + 1;
            }

            if (fractionFlag)
            {
                fractionValue = goldSrc ? ReadUnsignedBits(3) : ReadUnsignedBits(5);
            }

            value = intValue + fractionValue * 1.0f / 32.0f;
            if (sign)
            {
                value = -value;
            }

            return value;
        }

        public void InsertBytes(byte[] insertData)
        {
            if (insertData.Length == 0)
            {
                return;
            }

            if (CurrentBit % 8 != 0)
            {
                throw new ApplicationException(
                    "InsertBytes can only be called if the current bit is aligned to byte boundaries.");
            }

            data.InsertRange(CurrentByte, insertData);
            CurrentBit += insertData.Length * 8;
        }

        public void RemoveBytes(int count)
        {
            if (count == 0)
            {
                return;
            }

            if (CurrentBit % 8 != 0)
            {
                throw new ApplicationException(
                    "RemoveBytes can only be called if the current bit is aligned to byte boundaries.");
            }

            if (CurrentByte + count > Length)
            {
                throw new BitBufferOutOfRangeException();
            }

            data.RemoveRange(CurrentByte, count);
        }

        #region Properties

        /// <summary>
        ///     Data length in bytes.
        /// </summary>
        public int Length => data.Count;

        public int CurrentBit { get; private set; }

        public int CurrentByte => (CurrentBit - CurrentBit % 8) / 8;
        public byte[] Data => data.ToArray();
        public EndianType Endian { get; set; }

        #endregion
    }

    public class HalfLifeDelta
    {
        private readonly List<Entry> entryList;

        public HalfLifeDelta(int nEntries)
        {
            entryList = new List<Entry>(nEntries);
        }

        public void AddEntry(string name)
        {
            var e = new Entry { Name = name, Value = null };
            entryList.Add(e);
        }

        public object FindEntryValue(string name)
        {
            var e = FindEntry(name);
            return e == null ? null : e.Value;
        }

        public void SetEntryValue(string name, object value)
        {
            var e = FindEntry(name);
            if (e == null)
            {
                throw new ApplicationException(string.Format("Delta entry {0} not found.", name));
            }

            e.Value = value;
        }

        public void SetEntryValue(int index, object value)
        {
            entryList[index].Value = value;
        }

        private Entry FindEntry(string name)
        {
            foreach (var e in entryList)
            {
                if (e.Name == name)
                {
                    return e;
                }
            }

            return null;
        }

        private class Entry
        {
            public string Name;
            public object Value;
        }
    }

    /// <summary>
    ///     Stores delta structure entry parameters, as well as handling the creation and decoding of delta compressed data.
    /// </summary>
    public class HalfLifeDeltaStructure
    {
        [Flags]
        public enum ENTRY_TYPES : uint
        {
            Byte = 1u << 0,
            Short = 1u << 1,
            Float = 1u << 2,
            Integer = 1u << 3,
            Angle = 1u << 4,
            TimeWindow8 = 1u << 5,
            TimeWindowBig = 1u << 6,
            String = 1u << 7,
            Signed = 1u << 31,
        };

        public List<Entry> entryList;

        public HalfLifeDeltaStructure(string name)
        {
            Name = name;
            entryList = new List<Entry>();
        }

        public string Name { get; }

        /// <summary>
        ///     Adds an entry. Delta is assumed to be delta_description_t. Should only need to be called when parsing
        ///     svc_deltadescription.
        /// </summary>
        /// <param name="delta"></param>
        public void AddEntry(HalfLifeDelta delta)
        {
            var name = delta.FindEntryValue("name");
            var nBits = delta.FindEntryValue("nBits");
            var divisor = delta.FindEntryValue("divisor");
            var preMultiplier = delta.FindEntryValue("preMultiplier");
            var flags = delta.FindEntryValue("flags");

            name = name != null ? (string)name : "unnamed";
            nBits = nBits != null ? (uint)nBits : 0;
            divisor = divisor != null ? (float)divisor : 0.0f;
            preMultiplier = preMultiplier != null ? (float)preMultiplier : 1.0f;
            flags = flags != null ? (ENTRY_TYPES)(uint)flags : ENTRY_TYPES.Integer;

            AddEntry(name as string, (uint)nBits, (float)divisor, (float)preMultiplier,
               (ENTRY_TYPES)flags);
        }

        /// <summary>
        ///     Adds an entry manually. Should only need to be called when creating a delta_description_t structure.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="nBits"></param>
        /// <param name="divisor"></param>
        /// <param name="flags"></param>
        public void AddEntry(string name, uint nBits, float divisor, float preMultiplier, ENTRY_TYPES flags)
        {
            var entry = new Entry
            { Name = name, nBits = nBits, Divisor = divisor, Flags = flags, PreMultiplier = preMultiplier };
            entryList.Add(entry);
        }

        public HalfLifeDelta CreateDelta()
        {
            var delta = new HalfLifeDelta(entryList.Count);
            // create delta structure with the same entries as the delta decoder, but no data
            foreach (var e in entryList)
            {
                delta.AddEntry(e.Name);
            }

            return delta;
        }

        public void ReadDelta(BitBuffer bitBuffer, HalfLifeDelta delta)
        {
            byte[] mask = new byte[0];
            ReadDelta(bitBuffer, delta, ref mask);
        }


        public void ReadDelta(BitBuffer bitBuffer, HalfLifeDelta delta, ref byte[] bitmaskBytes)
        {
            // read bitmask
            var nBitmaskBytes = bitBuffer.ReadUnsignedBits(3);
            // TODO: error check nBitmaskBytes against nEntries
            if (nBitmaskBytes == 0)
            {
                bitmaskBytes = null;
                return;
            }

            bitmaskBytes = new byte[nBitmaskBytes];
            for (var i = 0; i < nBitmaskBytes; i++)
            {
                bitmaskBytes[i] = bitBuffer.ReadByte();
            }

            for (var i = 0; i < nBitmaskBytes; i++)
            {
                for (var j = 0; j < 8; j++)
                {
                    var index = j + i * 8;
                    if (index == entryList.Count)
                    {
                        return;
                    }

                    if ((bitmaskBytes[i] & (1 << j)) != 0)
                    {
                        if (DUMP_ALL_FRAMES)
                        {
                            OutDumpString += "\nENTRY(" + Name + ")" + entryList[index].Name + " = ";
                        }

                        var value = ParseEntry(bitBuffer, entryList[index]);
                        if (Name != null && entryList[index].Name != null)
                        {
                            //   Console.WriteLine("Name["+ Name + "]:" + entryList[index].Name);
                            //string addline = "|" + entryList[index].Name + "\n";
                            //if (testData.ContainsKey(Name))
                            //{
                            //    if (testData[Name].IndexOf(addline) < 0)
                            //        testData[Name] += addline;
                            //}
                            //else
                            //    testData[Name] = addline;

                            if (Name == "entity_state_player_t")
                            {
                                if (LastPlayerEntity == LocalPlayerId + 1)
                                {
                                    if (entryList[index].Name == "angles[1]")
                                    {
                                        var tmpAngle = value != null ? (float)value : 1.0f;
                                        //if (abs(CurrentNetMsgFrame.RParms.Punchangle.Y) < EPSILON && 
                                        //    !isAngleInViewListYDelta(tmpAngle)
                                        //    && IsUserAlive())
                                        //{
                                        //    Console.WriteLine("AIM BOT + "+ CurrentNetMsgFrame.RParms.Punchangle.Y + " + " + give_bad_float(CurrentNetMsgFrame.RParms.ClViewangles.Y) + " = " + tmpAngle + " = " + CurrentTimeString);
                                        //}
                                        NewViewAngleSearcherAngle = tmpAngle;
                                    }

                                    if (entryList[index].Name == "movetype")
                                    {
                                        var movetype = value != null ? (uint)value : 0;
                                        // Console.WriteLine("Change movetype to :" + movetype);
                                        FlyDirection = 0;
                                    }

                                    if (entryList[index].Name == "usehull")
                                    {
                                        var usehull = value != null ? (uint)value : 0;
                                        //  Console.WriteLine("Change usehull to :" + usehull + "_" + IsDuckPressed);
                                        LastPlayerHull = usehull;
                                        //if (LastPlayerHull == 0 || LastPlayerHull == 1)
                                        //{
                                        //    DuckHack4Search = 4;
                                        //}
                                        //else
                                        //{
                                        //    DuckHack4Search = 0;
                                        //}
                                    }

                                    if (entryList[index].Name == "gaitsequence")
                                    {
                                        var seqnum = value != null ? (uint)value : 0;
                                        if (seqnum > 0 && !UserAlive && CL_Intermission == 0 && !FirstUserAlive)
                                        {
                                            if (DEBUG_ENABLED)
                                            {
                                                Console.WriteLine("Alive 4 at " + CurrentTimeString);
                                            }

                                            DemoScanner_AddTextMessage("Respawn[4]", "USER_INFO", CurrentTime,
                                                CurrentTimeString);
                                            UserAlive = true;
                                            LastAliveTime = CurrentTime;
                                            FirstUserAlive = false;
                                        }
                                        else if (seqnum == 6)
                                        {
                                            if (abs(LastJumpHackFalseDetectionTime) > EPSILON &&
                                                abs(CurrentTime - LastJumpHackFalseDetectionTime) > 5.0f)
                                            {
                                                LastJumpHackFalseDetectionTime = 0.0f;
                                            }

                                            if (abs(CurrentTime - LastUnJumpTime) > 0.3f &&
                                                abs(CurrentTime - LastJumpTime) > 0.3f)
                                            {
                                                if (!IsPlayerAnyJumpPressed())
                                                {
                                                    if (!IsPlayerLossConnection())
                                                    {
                                                        if (abs(CurrentTime - LastKreedzHackTime) > 2.5f &&
                                                            abs(LastJumpHackFalseDetectionTime) < EPSILON)
                                                        {
                                                            if (DemoScanner_AddWarn(
                                                                "[BHOP TYPE 3] at (" + CurrentTime + ") " +
                                                                CurrentTimeString, false, false))
                                                            {
                                                                LastKreedzHackTime = CurrentTime;
                                                                KreedzHacksCount++;
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (abs(CurrentTime - LastKreedzHackTime) > 2.5f &&
                                                            abs(LastJumpHackFalseDetectionTime) < EPSILON)
                                                        {
                                                            if (DemoScanner_AddWarn(
                                                                "[BETA] [BHOP TYPE 3] at (" + CurrentTime + ") " +
                                                                CurrentTimeString, false, false))
                                                            {
                                                                LastKreedzHackTime = CurrentTime;
                                                                KreedzHacksCount++;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            if (Name == "weapon_data_t")
                            {
                                NoWeaponData = false;
                                if (entryList[index].Name == "m_flNextPrimaryAttack")
                                {
                                    if (!UserAlive && abs(CurrentTime - LastDeathTime) > 5.0)
                                    {
                                        DemoScanner_AddTextMessage("Respawn[5]", "USER_INFO", CurrentTime,
                                            CurrentTimeString);
                                        //FirstUserAlive = false;
                                        UserAlive = true;
                                        LastAliveTime = CurrentTime;
                                        if (DEBUG_ENABLED)
                                        {
                                            Console.WriteLine("Alive 5 at " + CurrentTimeString);
                                        }
                                    }

                                    PrevWaitPrimaryAttackTime = WaitPrimaryAttackTime;
                                    WaitPrimaryAttackTime = value != null ? (float)value : 1.0f;

                                    if (WaitPrimaryAttackTime <= 0.016 || (CurrentWeapon == WeaponIdType.WEAPON_DEAGLE &&
                                                          WaitPrimaryAttackTime <= 0.025))
                                    {
                                        if (PrimaryAttackHistory.Count > 0 &&
                                            abs(PrimaryAttackHistory[0].attack_time - LastAttackStartCmdTime) < EPSILON)
                                        {
                                            PrimaryAttackHistory.RemoveAt(0);
                                        }

                                        PrimaryCheckTimer = 0;

                                        LastPrevPrimaryAttackTime = LastPrimaryAttackTime;
                                        LastPrimaryAttackTime = CurrentTime;

                                        PrimaryAttackHistory.Add(new PrimHistory(LastPrimaryAttackTime, LastAttackStartCmdTime));

                                        //Console.WriteLine("Primary:" + LastPrimaryAttackTime + ". Attack:" +
                                        //                            LastAttackStartCmdTime + ". Diff:" + abs(LastPrimaryAttackTime - LastAttackStartCmdTime));

                                        while (PrimaryAttackHistory.Count > 8)
                                        {
                                            PrimaryAttackHistory.RemoveAt(0);
                                        }


                                        //    float diff = 0.0f;
                                        //    int counter = 0;
                                        //        bool clear = false;
                                        //        for (int n = 0; n < PrimaryAttackHistory.Count; n++)
                                        //        {
                                        //            if (n == 0)
                                        //            {
                                        //                diff = abs(PrimaryAttackHistory[0].attack_time - PrimaryAttackHistory[0].primary_time);
                                        //                //Console.WriteLine("Start diff:" + diff);
                                        //            }
                                        //            else
                                        //            {
                                        //                if (abs(PrimaryAttackHistory[n].attack_time - PrimaryAttackHistory[n - 1].attack_time) > EPSILON
                                        //                    && abs(PrimaryAttackHistory[n].primary_time - PrimaryAttackHistory[n - 1].primary_time) > EPSILON)
                                        //                {
                                        //                    float tmpdiff = abs(PrimaryAttackHistory[n].attack_time - PrimaryAttackHistory[n].primary_time);
                                        //                    //Console.WriteLine("Tmpdiff:" + tmpdiff + ". abs(tmpdiff - diff):" + abs(tmpdiff - diff));

                                        //                    if (abs(tmpdiff - diff) < 0.02f)
                                        //                    {
                                        //                        counter++;
                                        //                        if (counter > 3)
                                        //                        {
                                        //                            clear = true;
                                        //                            DemoScanner_AddWarn("[AIM TYPE 2.2 " + CurrentWeapon + "] at (" + CurrentTime + ") " +
                                        //                                        CurrentTimeString, !IsPlayerLossConnection() &&
                                        //                                        !IsCmdChangeWeapon() && !IsAngleEditByEngine());

                                        //                            //Console.WriteLine("ATTACK:" + tmpdiff + " history :");
                                        //                            //for (int v = 0; v < counter; v++)
                                        //                            //{
                                        //                            //    Console.WriteLine("Primary:" + PrimaryAttackHistory[v].primary_time + ". Attack:" +
                                        //                            //        PrimaryAttackHistory[v].attack_time);
                                        //                            //}

                                        //                            break;
                                        //                        }
                                        //                    }
                                        //                    diff = tmpdiff;
                                        //                    //else
                                        //                    // break;
                                        //                }
                                        //                else
                                        //                {
                                        //                    break;
                                        //                }
                                        //            }
                                        //        }

                                        //        if (clear)
                                        //        {
                                        //            PrimaryAttackHistory.Clear();
                                        //        }
                                    }

                                    if (WaitPrimaryAttackTime <= 0.016 || (CurrentWeapon == WeaponIdType.WEAPON_DEAGLE &&
                                                      WaitPrimaryAttackTime <= 0.025))
                                    {
                                        WeaponAvaiabled = true;
                                        WeaponAvaiabledFrameId = CurrentFrameIdWeapon;
                                        WeaponAvaiabledFrameTime = CurrentTime2;
                                    }
                                    else
                                    {
                                        if (WeaponAvaiabled)
                                        {
                                            AutoAttackStrikesID++;
                                        }

                                        WeaponAvaiabled = false;
                                        WeaponAvaiabledFrameId = 0;
                                        WeaponAvaiabledFrameTime = 0.0f;
                                    }
                                }

                                if (entryList[index].Name == "m_fInReload")
                                {
                                    var reloadstatus = value != null ? (uint)value : 0;
                                    if (LastWeaponReloadStatus == reloadstatus)
                                    {
                                        ReloadWarns = 0;
                                    }

                                    LastWeaponReloadStatus = reloadstatus;
                                    if (reloadstatus > 0)
                                    {
                                        if (DEBUG_ENABLED)
                                        {
                                            Console.WriteLine("User reload weapon");
                                        }

                                        IsReload = true;
                                        AttackCheck = -1;
                                        Reloads++;
                                        ReloadTime = CurrentTime;
                                        StartReloadWeapon = CurrentWeapon;
                                    }
                                    else
                                    {
                                        if (RealAlive && EndReloadWeapon == CurrentWeapon &&
                                            StartReloadWeapon == EndReloadWeapon)
                                        {
                                            if (!IsRoundEnd() && abs(CurrentTime - ReloadTime) < 0.2)
                                            {
                                                ReloadWarns++;
                                                if (ReloadWarns > 1)
                                                {
                                                    ReloadWarns = 0;
                                                    DemoScanner_AddWarn("[NO RELOAD " + CurrentWeapon + "] hack at (" +
                                                                        CurrentTime + "):" + CurrentTimeString);
                                                }
                                            }
                                            else
                                            {
                                                if (ReloadWarns > 0)
                                                {
                                                    ReloadWarns--;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            ReloadWarns = 0;
                                        }

                                        EndReloadWeapon = CurrentWeapon;
                                        Reloads2++;
                                        IsReload = false;
                                    }
                                }
                            }

                            if (Name == "clientdata_t")
                            {
                                if (entryList[index].Name == "weaponanim")
                                {
                                    if (SelectSlot > 0)
                                    {
                                        SkipNextAttack = 2;
                                    }
                                }

                                if (entryList[index].Name == "punchangle[0]")
                                {
                                    var punchangle_x = value != null ? (float)value : 0.0f;

                                    //Console.WriteLine("punchangle[0] = " + punchangle_x);

                                    // Search in current or next frames
                                    var foundOld = false;
                                    foreach (var old_punch_x in LastPunchAngleX)
                                    {
                                        if (abs(old_punch_x - punchangle_x) <= MAX_SPREAD_CONST)
                                        {
                                            foundOld = true;
                                        }
                                    }

                                    if (!foundOld && abs(Punch0_Valid_Time - CurrentFrameTime) > 0.1f &&
                                        CurrentFrameDuplicated <= 0 && IsUserAlive())
                                    {
                                        Punch0_Search.Add(punchangle_x);
                                        Punch0_Search_Time.Add(CurrentTime);
                                    }
                                }

                                if (entryList[index].Name == "origin[0]")
                                {
                                    var origin_x = value != null ? (float)value : 0.0f;
                                    DetectCmdHackType10 = true;
                                    CmdHack10_origX = origin_x;
                                }

                                if (entryList[index].Name == "origin[1]")
                                {
                                    var origin_y = value != null ? (float)value : 0.0f;
                                    DetectCmdHackType10 = true;
                                    CmdHack10_origY = origin_y;
                                }

                                if (entryList[index].Name == "origin[2]")
                                {
                                    var origin_z = value != null ? (float)value : 0.0f;
                                    DetectCmdHackType10 = true;
                                    CmdHack10_origZ = origin_z;
                                }

                                if (entryList[index].Name == "velocity[0]" || entryList[index].Name == "velocity[1]")
                                {
                                    var velocity = value != null ? (float)value : 0.0f;
                                    if (abs(velocity) > EPSILON)
                                    {
                                        FoundVelocityTime = CurrentTime;
                                    }

                                    if (velocity > 100.0f || velocity < -100.0f)
                                    {
                                        FoundBigVelocityTime = CurrentTime;
                                    }
                                }

                                if (entryList[index].Name == "fov")
                                {
                                    var fov = value != null ? (float)value : 0.0f;
                                    if (abs(fov - ClientFov) > EPSILON && abs(fov - ClientFov2) > EPSILON)
                                    {
                                        ClientFov2 = ClientFov;
                                        ClientFov = fov;
                                    }
                                }

                                if (entryList[index].Name == "fuser2")
                                {
                                    var rg_jump_time = value != null ? (float)value : 0.0f;

                                    if (rg_jump_time > last_rg_jump_time + 0.1f)
                                    {
                                        if (IsUserAlive())
                                        {
                                            JumpCount5++;
                                            if (!IsPlayerAnyJumpPressed())
                                            {
                                                if (abs(CurrentTime - LastKreedzHackTime) > 2.5f)
                                                {
                                                    if (DemoScanner_AddWarn(
                                                        "[JUMPHACK TYPE 2] at (" + CurrentTime + ") " +
                                                        CurrentTimeString, !IsAngleEditByEngine() && !IsPlayerLossConnection()))
                                                    {
                                                        LastKreedzHackTime = CurrentTime;
                                                        KreedzHacksCount++;
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    last_rg_jump_time = rg_jump_time;
                                }

                                if (entryList[index].Name == "health")
                                {
                                    var hp = value != null ? (float)value : 0.0f;
                                    if (hp <= 0 && (UserAlive || FirstUserAlive))
                                    {
                                        LastDeathTime = CurrentTime;
                                        if (!FirstUserAlive)
                                        {
                                            DeathsCoount++;
                                        }

                                        if (UserAlive)
                                        {
                                            DemoScanner_AddTextMessage("Killed", "USER_INFO", CurrentTime,
                                                CurrentTimeString);
                                        }

                                        FirstUserAlive = false;
                                        UserAlive = false;
                                        RealAlive = false;
                                        if (DEBUG_ENABLED)
                                        {
                                            Console.WriteLine("LocalPlayer killed. Method : clientdata_t health! at " +
                                                              CurrentTimeString);
                                        }
                                    }
                                }

                                if (entryList[index].Name == "bInDuck")
                                {
                                    var isDuck = value != null ? (uint)value > 0 : false;
                                    if (isDuck && abs(CurrentTime - LastDuckTime) > 0.6f)
                                    {
                                        if (IsUserAlive() && abs(CurrentTime - LastKreedzHackTime) > 1.0 &&
                                            !IsPlayerTeleport())
                                        {
                                            if (DemoScanner_AddWarn(
                                                "[DUCK HACK TYPE 5.1] at (" + CurrentTime + ") " + CurrentTimeString,
                                                !IsPlayerLossConnection()))
                                            {
                                                KreedzHacksCount++;
                                                LastKreedzHackTime = CurrentTime;
                                            }
                                        }
                                    }
                                    //else if (!isDuck && abs(CurrentTime - LastUnDuckTime) > 1.0f)
                                    //{
                                    //    if (IsUserAlive() && abs(CurrentTime - LastKreedzHackTime) > 1.0
                                    //         && !IsPlayerTeleport())
                                    //    {
                                    //        DemoScanner_AddWarn(
                                    //                     "[DUCK HACK TYPE 5.2] at (" + CurrentTime +
                                    //                     ") " + CurrentTimeString, !IsPlayerLossConnection());
                                    //        KreedzHacksCount++;
                                    //        LastKreedzHackTime = CurrentTime;
                                    //    }
                                    //}
                                }

                                if (entryList[index].Name == "flags")
                                {
                                    var flags = value != null ? (uint)value : 0;
                                    if ((flags & 4096) > 0 && !PlayerFrozen)
                                    {
                                        PlayerFrozen = true;
                                        PlayerFrozenTime = CurrentTime;
                                        if (IsRussia)
                                        {
                                            DemoScanner_AddInfo("Игрок был поставлен на паузу (" + CurrentTime + "):" +
                                                                CurrentTimeString);
                                        }
                                        else
                                        {
                                            DemoScanner_AddInfo("Player been froze at (" + CurrentTime + "):" +
                                                                CurrentTimeString);
                                        }
                                    }
                                    else if (!((flags & 4096) > 0) && PlayerFrozen)
                                    {
                                        PlayerUnFrozenTime = CurrentTime;
                                        PlayerFrozen = false;
                                        if (IsRussia)
                                        {
                                            DemoScanner_AddInfo("Игрок снят с паузы (" + CurrentTime + "):" +
                                                                CurrentTimeString);
                                        }
                                        else
                                        {
                                            DemoScanner_AddInfo("Player has been unfrozen at (" + CurrentTime + "):" +
                                                                CurrentTimeString);
                                        }
                                    }
                                }
                            }

                            if ( /*this.name == "weapon_data_t" || */Name == "clientdata_t")
                            {
                                if (entryList[index].Name == "m_iId")
                                {
                                    var weaponid = (WeaponIdType)Enum.ToObject(typeof(WeaponIdType), value);
                                    if (CurrentWeapon != weaponid)
                                    {
                                        if (!UsingAnotherMethodWeaponDetection)
                                        {
                                            UsingAnotherMethodWeaponDetection = true;
                                        }

                                        if (DEBUG_ENABLED)
                                        {
                                            Console.WriteLine("Select weapon method 3 (" +
                                                              UsingAnotherMethodWeaponDetection + "):" + weaponid +
                                                              ":" + CurrentTimeString);
                                        }

                                        if (SearchFastZoom == 4)
                                        {
                                            SearchFastZoom++;
                                        }
                                        else if (SearchFastZoom == 5)
                                        {
                                            SearchFastZoom = 0;
                                            DemoScanner_AddWarn(
                                                    "[BETA] [FASTZOOM ALIAS] at (" + CurrentTime +
                                                    "):" + CurrentTimeString, false);
                                        }

                                        if (DUMP_ALL_FRAMES)
                                        {
                                            OutDumpString += "\nCHANGE WEAPON 2 (" + CurrentTime + ") " +
                                                             CurrentTimeString;
                                            OutDumpString += "\nFrom " + CurrentWeapon + " to " + weaponid;
                                            Console.WriteLine("CHANGE WEAPON 2 (" + CurrentTime + ") " +
                                                              CurrentTimeString);
                                            Console.WriteLine("From " + CurrentWeapon + " to " + weaponid);
                                        }

                                        NeedSearchAim2 = false;
                                        Aim2AttackDetected = false;
                                        ShotFound = -1;
                                        SelectSlot = 0;
                                        WeaponChanged = true;
                                        ChangeWeaponTime2 = CurrentTime;
                                        AmmoCount = 0;
                                        // IsAttackSkipTimes = 0;
                                        if (CurrentWeapon != WeaponIdType.WEAPON_NONE)
                                        {
                                            SkipNextAttack = 2;
                                        }

                                        //InitAimMissingSearch = -1;
                                        IsNoAttackLastTime = CurrentTime + 1.0f;
                                        NeedCheckAttack = false;
                                        NeedCheckAttack2 = false;
                                        CurrentWeapon = weaponid;
                                        //AutoPistolStrikes = 0;
                                        // LastPrimaryAttackTime = 0.0f;
                                        //LastPrevPrimaryAttackTime = 0.0f;
                                        if (!UserAlive)
                                        {
                                            // CurrentWeapon = WeaponIdType.WEAPON_BAD;
                                        }
                                    }
                                }
                            }

                            if (Name == "weapon_data_t")
                            {
                                NoWeaponData = false;
                                if (NeedCheckAttack2 && entryList[index].Name == "m_fAimedDamage")
                                {
                                    var damage = value != null ? (float)value : 0;
                                    NeedCheckAttack2 = false;
                                    if (FirstUserAlive && !IsUserAlive())
                                    {
                                        if (DEBUG_ENABLED)
                                        {
                                            Console.WriteLine("Alive 6 at " + CurrentTimeString);
                                        }

                                        DemoScanner_AddTextMessage("Respawn[6]", "USER_INFO", CurrentTime,
                                            CurrentTimeString);
                                        UserAlive = true;
                                        LastAliveTime = CurrentTime;
                                        FirstUserAlive = false;
                                    }

                                    if (IsUserAlive() && abs(LastAimedTimeDamage - damage) > EPSILON)
                                    {
                                        LastAimedTimeDamage = damage;
                                        if (IsPlayerTeleport())
                                        {
                                        }
                                        else if (abs(CurrentTime - LastAngleManipulation) < 0.50f)
                                        {
                                        }
                                        else if (IsTakeDamage())
                                        {
                                        }
                                        else if (IsPlayerFrozen())
                                        {
                                        }
                                        else if (IsViewChanged())
                                        {
                                        }
                                        else if (abs(CurrentTime - LastLookDisabled) < 0.75f)
                                        {
                                        }
                                        else if (IsRoundEnd())
                                        {
                                        }
                                        else if (HideWeapon)
                                        {
                                        }
                                        else if (CurrentFrameDuplicated > 1)
                                        {
                                        }
                                        else if (!IsRealWeapon())
                                        {
                                        }
                                        else if (IsRealChangeWeapon())
                                        {
                                        }
                                        else if (CurrentWeapon == WeaponIdType.WEAPON_NONE ||
                                                 CurrentWeapon == WeaponIdType.WEAPON_BAD ||
                                                 CurrentWeapon == WeaponIdType.WEAPON_BAD2 ||
                                                 CurrentWeapon == WeaponIdType.WEAPON_C4 ||
                                                 CurrentWeapon == WeaponIdType.WEAPON_HEGRENADE ||
                                                 CurrentWeapon == WeaponIdType.WEAPON_SMOKEGRENADE ||
                                                 CurrentWeapon == WeaponIdType.WEAPON_FLASHBANG)
                                        {
                                        }
                                        else
                                        {
                                            if (!CurrentFrameAttacked && !PreviousFrameAttacked &&
                                                !IsPlayerAttackedPressed())
                                            {
                                                // first detection can be false if demo started in +attack
                                                // just skip it
                                                if (!FirstAim11skip)
                                                {
                                                    DemoScanner_AddWarn(
                                                        "[BETA] [AIM TYPE 11 " + CurrentWeapon + "] at (" + CurrentTime +
                                                        "):" + CurrentTimeString,
                                                        !IsCmdChangeWeapon() && !IsPlayerLossConnection());
                                                }
                                                FirstAim11skip = false;
                                            }
                                        }
                                    }
                                }

                                if (NeedCheckAttack && entryList[index].Name == "m_iClip")
                                {
                                    NeedCheckAttack = false;
                                    var ammocount = value != null ? (int)value : 0;
                                    if (FirstUserAlive && !IsUserAlive())
                                    {
                                        if (DEBUG_ENABLED)
                                        {
                                            Console.WriteLine("Alive 7 at " + CurrentTimeString);
                                        }

                                        DemoScanner_AddTextMessage("Respawn[7]", "USER_INFO", CurrentTime,
                                            CurrentTimeString);
                                        UserAlive = true;
                                        LastAliveTime = CurrentTime;
                                        FirstUserAlive = false;
                                    }

                                    if (ammocount != AmmoCount && IsUserAlive())
                                    {
                                        if (DEBUG_ENABLED)
                                        {
                                            Console.Write("!!Shot->Shot->!!! ");
                                        }

                                        attackscounter4++;
                                        /*if (BadPunchAngle)
                                            {
                                                attackscounter5++;
                                                if (DEBUG_ENABLED)
                                                    Console.WriteLine("BadPunchAngle" + "(" +
                                                                      CurrentTime + ") " + CurrentTimeString);
                                            }*/
                                        if (IsPlayerTeleport())
                                        {
                                            attackscounter5++;
                                            if (DEBUG_ENABLED)
                                            {
                                                Console.WriteLine("TELEPORT" + "(" + CurrentTime + ") " +
                                                                  CurrentTimeString);
                                            }

                                            if (INSPECT_BAD_SHOT)
                                            {
                                                Console.ReadKey();
                                            }
                                        }
                                        else if (abs(CurrentTime - LastAngleManipulation) < 0.50f)
                                        {
                                            attackscounter5++;
                                            if (DEBUG_ENABLED)
                                            {
                                                Console.WriteLine("MANIPUL" + "(" + CurrentTime + ") " +
                                                                  CurrentTimeString);
                                            }

                                            if (INSPECT_BAD_SHOT)
                                            {
                                                Console.ReadKey();
                                            }
                                        }
                                        else if (IsTakeDamage())
                                        {
                                            attackscounter5++;
                                            if (DEBUG_ENABLED)
                                            {
                                                Console.WriteLine("DAMAGE" + "(" + CurrentTime + ") " +
                                                                  CurrentTimeString);
                                            }

                                            if (INSPECT_BAD_SHOT)
                                            {
                                                Console.ReadKey();
                                            }
                                        }
                                        else if (IsPlayerFrozen())
                                        {
                                            attackscounter5++;
                                            if (DEBUG_ENABLED)
                                            {
                                                Console.WriteLine("FROZEN" + "(" + CurrentTime + ") " +
                                                                  CurrentTimeString);
                                            }

                                            if (INSPECT_BAD_SHOT)
                                            {
                                                Console.ReadKey();
                                            }
                                        }
                                        else if (abs(CurrentTime - LastCmdDuckTime) < 1.01f)
                                        {
                                            attackscounter5++;
                                            if (DEBUG_ENABLED)
                                            {
                                                Console.WriteLine("INDUCK" + "(" + CurrentTime + ") " +
                                                                  CurrentTimeString);
                                            }

                                            if (INSPECT_BAD_SHOT)
                                            {
                                                Console.ReadKey();
                                            }
                                        }
                                        else if (abs(CurrentTime - LastCmdUnduckTime) < 0.2f)
                                        {
                                            attackscounter5++;
                                            if (DEBUG_ENABLED)
                                            {
                                                Console.WriteLine("INDUCK2" + "(" + CurrentTime + ") " +
                                                                  CurrentTimeString);
                                            }

                                            if (INSPECT_BAD_SHOT)
                                            {
                                                Console.ReadKey();
                                            }
                                        }
                                        else if (IsViewChanged())
                                        {
                                            attackscounter5++;
                                            if (DEBUG_ENABLED)
                                            {
                                                Console.WriteLine("VIEW" + "(" + CurrentTime + ") " +
                                                                  CurrentTimeString);
                                            }

                                            if (INSPECT_BAD_SHOT)
                                            {
                                                Console.ReadKey();
                                            }
                                        }
                                        else if (abs(CurrentTime - LastLookDisabled) < 0.75f)
                                        {
                                            attackscounter5++;
                                            if (DEBUG_ENABLED)
                                            {
                                                Console.WriteLine(
                                                    "INLUK" + "(" + CurrentTime + ") " + CurrentTimeString);
                                            }

                                            if (INSPECT_BAD_SHOT)
                                            {
                                                Console.ReadKey();
                                            }
                                        }
                                        else if (IsPlayerLossConnection())
                                        {
                                            attackscounter5++;
                                            if (DEBUG_ENABLED)
                                            {
                                                Console.WriteLine("LAG" + "(" + CurrentTime + ") " + CurrentTimeString);
                                            }

                                            if (INSPECT_BAD_SHOT)
                                            {
                                                Console.ReadKey();
                                            }
                                        }
                                        else if (IsRoundEnd())
                                        {
                                            attackscounter5++;
                                            if (DEBUG_ENABLED)
                                            {
                                                Console.WriteLine(
                                                    "ROUND" + "(" + CurrentTime + ") " + CurrentTimeString);
                                            }

                                            if (INSPECT_BAD_SHOT)
                                            {
                                                Console.ReadKey();
                                            }
                                        }
                                        else if (HideWeapon)
                                        {
                                            attackscounter5++;
                                            if (DEBUG_ENABLED)
                                            {
                                                Console.WriteLine("HIDE" + "(" + CurrentTime + ") " +
                                                                  CurrentTimeString);
                                            }

                                            if (INSPECT_BAD_SHOT)
                                            {
                                                Console.ReadKey();
                                            }
                                        }
                                        else if (CurrentFrameDuplicated > 1)
                                        {
                                            attackscounter5++;
                                            if (DEBUG_ENABLED)
                                            {
                                                Console.WriteLine("DUP" + "(" + CurrentTime + ") " + CurrentTimeString);
                                            }

                                            if (INSPECT_BAD_SHOT)
                                            {
                                                Console.ReadKey();
                                            }
                                        }
                                        /*else if (!CurrentFrameOnGround)
                                            {
                                                attackscounter5++;
                                                if (DEBUG_ENABLED)
                                                    Console.WriteLine("FLY" + "(" +
                                                                      CurrentTime + ") " + CurrentTimeString);
                                                if (INSPECT_BAD_SHOT)
                                                    Console.ReadKey();
                                            }*/
                                        else if (!IsRealWeapon())
                                        {
                                            attackscounter5++;
                                            if (DEBUG_ENABLED)
                                            {
                                                Console.WriteLine("NOREAL" + "(" + CurrentTime + ") " +
                                                                  CurrentTimeString);
                                            }

                                            if (INSPECT_BAD_SHOT)
                                            {
                                                Console.ReadKey();
                                            }
                                        }
                                        else if (CurrentWeapon == WeaponIdType.WEAPON_NONE ||
                                                 CurrentWeapon == WeaponIdType.WEAPON_BAD ||
                                                 CurrentWeapon == WeaponIdType.WEAPON_BAD2 ||
                                                 CurrentWeapon == WeaponIdType.WEAPON_C4 ||
                                                 CurrentWeapon == WeaponIdType.WEAPON_HEGRENADE ||
                                                 CurrentWeapon == WeaponIdType.WEAPON_SMOKEGRENADE ||
                                                 CurrentWeapon == WeaponIdType.WEAPON_FLASHBANG)
                                        {
                                            attackscounter5++;
                                            if (DEBUG_ENABLED)
                                            {
                                                Console.WriteLine("NOWEAPON");
                                            }

                                            if (INSPECT_BAD_SHOT)
                                            {
                                                Console.ReadKey();
                                            }
                                        }
                                        else
                                        {
                                            if (DEBUG_ENABLED)
                                            {
                                                Console.WriteLine("CLEAN");
                                            }
                                        }
                                    }

                                    // Console.WriteLine("Attack " + CurrentTime + " : " + CurrentTimeString);
                                    //if (LastWatchWeapon == CurrentWeapon && UserAlive && AmmoCount > 0 && ammocount > 0 && AmmoCount - ammocount > 0 && AmmoCount - ammocount < 4)
                                    //{
                                    //    SkipNextAttack2--;
                                    //    if (IsInAttack() || CurrentTime - IsNoAttackLastTime < 0.1)
                                    //    {
                                    //        if (CurrentWeapon == WeaponIdType.WEAPON_FAMAS)
                                    //            SkipNextAttack2 = 3;
                                    //        // Console.WriteLine("Attack");
                                    //    }
                                    //    else if (!IsInAttack() && !IsReload && SkipNextAttack2 <= 0)
                                    //    {
                                    //        // ShotFound = true;
                                    //        TextComments.Add("Detected [TRIGGER BOT] at (" + CurrentTime + ") " + CurrentTimeString);
                                    //        AddViewDemoHelperComment("Detected [TRIGGER BOT]. Weapon:" + CurrentWeapon.ToString(), 0.5f);
                                    //        Console.WriteLine("Detected [TRIGGER BOT] at (" + CurrentTime + ") " + CurrentTimeString);
                                    //        BadAttackCount++;
                                    //        LastBadAttackCount = CurrentTime;
                                    //        IsNoAttackLastTime = 0.0f;//fixme
                                    //    }
                                    //}
                                    LastWatchWeapon = CurrentWeapon;
                                    WeaponChanged = false;
                                    AmmoCount = ammocount;
                                }
                            }
                        }

                        if (delta != null)
                        {
                            delta.SetEntryValue(index, value);
                        }
                    }
                }
            }
        }

        public static float abs(float val)
        {
            return Math.Abs(val);
        }

        private object ParseEntry(BitBuffer bitBuffer, Entry e)
        {
            var signed = (e.Flags & ENTRY_TYPES.Signed) != 0;
            if ((e.Flags & ENTRY_TYPES.Byte) != 0)
            {
                if (signed)
                {
                    var retval = (sbyte)ParseInt(bitBuffer, e);
                    if (DUMP_ALL_FRAMES)
                    {
                        OutDumpString += retval + "(SByte)";
                    }

                    return retval;
                }
                else
                {
                    var retval = (byte)ParseUnsignedInt(bitBuffer, e);
                    if (DUMP_ALL_FRAMES)
                    {
                        OutDumpString += retval + "(Byte)";
                    }

                    return retval;
                }
            }

            if ((e.Flags & ENTRY_TYPES.Short) != 0)
            {
                if (signed)
                {
                    var retval = (short)ParseInt(bitBuffer, e);
                    if (DUMP_ALL_FRAMES)
                    {
                        OutDumpString += retval + "(Int16)";
                    }

                    return retval;
                }
                else
                {
                    var retval = (ushort)ParseUnsignedInt(bitBuffer, e);
                    if (DUMP_ALL_FRAMES)
                    {
                        OutDumpString += retval + "(UInt16)";
                    }

                    return retval;
                }
            }

            if ((e.Flags & ENTRY_TYPES.Integer) != 0)
            {
                if (signed)
                {
                    try
                    {
                        var retval = ParseInt(bitBuffer, e);
                        if (DUMP_ALL_FRAMES)
                        {
                            OutDumpString += retval + "(Int32)";
                        }

                        return retval;
                    }
                    catch
                    {
                        return 0;
                    }
                }

                try
                {
                    var retval = ParseUnsignedInt(bitBuffer, e);
                    if (DUMP_ALL_FRAMES)
                    {
                        OutDumpString += retval + "(UInt32)";
                    }

                    return retval;
                }
                catch
                {
                    return (uint)0;
                }
            }

            try
            {
                if ((e.Flags & ENTRY_TYPES.Float) != 0 || (e.Flags & ENTRY_TYPES.TimeWindow8) != 0 ||
                    (e.Flags & ENTRY_TYPES.TimeWindowBig) != 0)
                {
                    var negative = false;
                    var bitsToRead = (int)e.nBits;
                    if (signed)
                    {
                        negative = bitBuffer.ReadBoolean();
                        bitsToRead--;
                    }

                    var retval = bitBuffer.ReadUnsignedBits(bitsToRead) / e.Divisor * (negative ? -1.0f : 1.0f);
                    if (DUMP_ALL_FRAMES)
                    {
                        OutDumpString += retval + "(FloatBit)";
                    }

                    return retval;
                }

                if ((e.Flags & ENTRY_TYPES.Angle) != 0)
                {
                    var retval = bitBuffer.ReadUnsignedBits((int)e.nBits) * (360.0f / (1 << (int)e.nBits));
                    if (DUMP_ALL_FRAMES)
                    {
                        OutDumpString += retval + "(Float)";
                    }

                    return retval;
                }
            }
            catch
            {
                return 0.0f;
            }

            if ((e.Flags & ENTRY_TYPES.String) != 0)
            {
                var retval = bitBuffer.ReadString();
                if (DUMP_ALL_FRAMES)
                {
                    OutDumpString += retval + "(String)";
                }

                return retval;
            }

            throw new ApplicationException(string.Format("Unknown delta entry type {0}.", e.Flags));
        }

        private int ParseInt(BitBuffer bitBuffer, Entry e)
        {
            var negative = bitBuffer.ReadBoolean();
            return (int)bitBuffer.ReadUnsignedBits((int)e.nBits - 1) / (int)e.Divisor * (negative ? -1 : 1);
        }

        private uint ParseUnsignedInt(BitBuffer bitBuffer, Entry e)
        {
            return bitBuffer.ReadUnsignedBits((int)e.nBits) / (uint)e.Divisor;
        }

        public class Entry
        {
            public float Divisor;
            public ENTRY_TYPES Flags;
            public string Name;
            public uint nBits;
            public float PreMultiplier;
        }
    }

    public class IgnoreConsoleWriter : TextWriter
    {
        public override void Write(char value)
        {

        }

        public override void Write(string value)
        {

        }

        public override System.Text.Encoding Encoding
        {
            get { return System.Text.Encoding.Default; }
        }
    }
    // removed Math.Sign (throw error 'invalid float value')
}