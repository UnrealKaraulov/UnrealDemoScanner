using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ConsoleTables;
using DemoScanner.DemoStuff;
using DemoScanner.DemoStuff.GoldSource;
using DemoScanner.Settings;
using static DemoScanner.DG.BitWriter;
using static DemoScanner.DG.Common;

namespace DemoScanner.DG
{
    public static class DemoScanner
    {
        public const string PROGRAMNAME = "Unreal Demo Scanner";
        public const string PROGRAMVERSION = "1.66.4_BETA";

        public enum AngleDirection
        {
            AngleDirectionLeft = -1,
            AngleDirectionRight = 1,
            AngleDirectionNO = 0
        }

        public const int RESOURCE_DOWNLOAD_THREADS = 20;

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

        public struct MyThreadState
        {
            public int threadid;
            public int state;
        }

        public static MyThreadState[] myThreadStates = new MyThreadState[RESOURCE_DOWNLOAD_THREADS + 1];

        public const bool INSPECT_BAD_SHOT = false;

        public const int MAX_MONITOR_REFRESHRATE = 365;
        public const int MAX_DEFAULT_FPS_LIMIT = 105; // default fps limit 100 without vsync

        public const float MAX_SPREAD_CONST = 0.00004f;
        public const float MAX_SPREAD_CONST2 = 0.00004f;
        public const int LEARN_FLOAT_COUNT = 3;

        public static bool DEBUG_ENABLED;
        public static bool NO_TELEPORT;
        public static bool DUMP_ALL_FRAMES;

        public const double MIN_SENS_WARN_DETECTED = 0.0004; //SENS < 0.02 (0.018)
        public const double MIN_SENS_WARN_WARNING = 0.004; //SENS < 0.2 (0.18)


        public static string OutDumpString = "";


        /* public static List<float> LearnAngles = new List<float>();
         public static bool ENABLE_LEARN_CLEAN_DEMO;
         public static bool AUTO_LEARN_HACK_DB;
         public static bool ENABLE_LEARN_HACK_DEMO;
         public static bool ENABLE_LEARN_HACK_DEMO_SAVE_ALL_ANGLES = false;
         public static bool ENABLE_LEARN_HACK_DEMO_FORCE_SAVE;
         public static MachineLearn_CheckAngles MachineLearnAnglesCLEAN = new MachineLearn_CheckAngles("Y_ATTACK_DB_v3.bin", LEARN_FLOAT_COUNT);
         public static MachineLearn_CheckAngles MachineLearnAnglesHACK = new MachineLearn_CheckAngles("Y_ATTACK_DB_HACK_v3.bin", LEARN_FLOAT_COUNT);
        */
        public static List<string> outFrames;

        public static float CurrentTime;
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

        public static bool IsDuck;
        public static bool IsDuckPressed;
        public static bool FirstDuck;
        public static float LastUnDuckTime;
        public static float LastDuckTime;

        public static int MouseJumps = -1;
        public static int JumpWithAlias = -1;

        public static int UserId = -1;
        public static int UserId2 = -1;

        public static bool isgroundchange = false;

        public static bool NotFirstEventShift = true;

        public static FPoint oldoriginpos;
        public static FPoint curoriginpos;

        public static int onground_and_alive_tests = 0;
        public static int speedhackdetects = -1;
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


        public static float MinFrameViewanglesY = 10000.0f;
        public static float MinFrameViewanglesX = 10000.0f;

        public static bool CurrentFrameAttacked;
        public static bool PreviousFrameAttacked;
        public static bool PreviousFrameAlive;
        public static bool CurrentFrameAlive;
        public static bool RealAlive;
        public static bool CurrentFrameJumped;
        public static bool PreviousFrameJumped;
        public static bool CurrentFrameOnGround;
        public static bool PreviousFrameOnGround;

        public static bool CurrentFrameDuck;
        public static bool PreviousFrameDuck;

        public static int JumpHackCount;
        public static int FakeLagAim;

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


        public static WeaponIdType CurrentWeapon = WeaponIdType.WEAPON_NONE;
        public static WeaponIdType StrikesWeapon = WeaponIdType.WEAPON_NONE;
        public static bool WeaponChanged;

        public static bool IsAttack;

        public static int AmmoCount;
        public static int AttackErrors;
        public static float LastAttackHack;


        public static int TotalAimBotDetected;

        public static int BadAttackCount;
        public static int FalsePositives = 0;
        public static bool BadAttackFound;
        public static float LastBadAttackCount;
        public static float LastSilentAim;
        public static float IsNoAttackLastTime;
        public static float IsNoAttackLastTime2;
        public static float IsAttackLastTime;

        public static int AttackCheck = -1;

        public static bool IsReload;

        public static int SelectSlot;

        public static bool ChangedWeaponBool = false;
        public static int FoundForceCenterView;
        public static float ForceCenterViewTime;

        public static int SkipNextAttack;
        public static int SkipNextAttack2 = 0;
        public static int AttackCurrentFrameI = 0;


        public static int attackscounter;
        public static int attackscounter2;
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
        public static int DeathsCoount2 = 0;
        public static int KillsCount;


        public static List<int> averagefps = new List<int>();
        public static List<float> averagefps2 = new List<float>();

        public static int LastTimeOut = -1;


        public static float LastTimeDesync;

        public static bool SecondFound;
        public static float CurrentTimeDesync;
        public static int CurrentFps;
        public static int RealFpsMin = int.MaxValue;
        public static int RealFpsMax = int.MinValue;
        public static float LastFpsCheckTime;

        public static bool SecondFound2;
        public static float CurrentTimeDesync2;
        public static int CurrentFps2;
        public static int RealFpsMin2 = int.MaxValue;
        public static int RealFpsMax2 = int.MinValue;
        public static float LastFpsCheckTime2;
        public static float LastCmdTime;
        public static string LastCmdTimeString = "00h:00m:00s:000ms";
        public static int LastCmdFrameId;
        public static string LastCmd = "";
        public static int CurrentFrameId;
        public static int CurrentFrameIdWeapon;

        public static int CaminCount;
        public static int FrameCrash;

        public static double CurrentSensitivity;
        public static List<float> PlayerSensitivityHistory = new List<float>();
        public static float AngleLenMinX;
        public static float AngleLenMaxX = 0.0f;

        public static float AngleLenMinY;
        public static float AngleLenMaxY = 0.0f;
        public static List<float> PlayerAngleLenHistory = new List<float>();
        public static List<string> PlayerSensitivityHistoryStr = new List<string>();
        public static List<string> PlayerSensitivityHistoryStrTime = new List<string>();
        public static List<string> PlayerSensitivityHistoryStrWeapon = new List<string>();
        public static string LastSensWeapon = "";
        public static int PlayerSensitivityWarning;


        public static int CurrentFrameIdAll;
        private static float LastClientDataTime;
        public static int NeedSearchID;
        public static FPoint3D CDFRAME_ViewAngles;
        public static FPoint3D PREV_CDFRAME_ViewAngles;

        public static bool CurrentMovementLeft = false;
        public static int LeftRightMovements = 0;
        public static bool CurrentMovementTop = false;
        public static int TopBottomMovements = 0;


        private static CrossParseResult CurrentDemoFile;
        private static HalfLifeDemoParser halfLifeDemoParser;

        public static bool NeedCheckAttack;

        public static float ClientFov = 90.0f;
        public static float cdframeFov = 90.0f;
        public static string DemoName = "";

        public static bool DisableJump5AndAim16;

        public static bool UserAlive;
        public static bool FirstUserAlive = true;

        public static bool NeedWriteAim;

        public static object sync = new object();

        public static bool NewAttack;
        public static bool NewAttackForLearn;
        public static int NewAttackForTrigger;
        public static int LastAttackForTrigger = -1;
        public static int LastAttackForTriggerFrame = -1;

        public static BinaryWriter ViewDemoHelperComments;
        public static TextWriter TextComments;
        public static Stream TextCommentsStream = new MemoryStream();
        public static int ViewDemoCommentCount;

        public static byte[] xcommentdata =
        {
            0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };

        public static float DemoStartTime;
        public static float DemoStartTime2;
        public static bool FoundFirstTime;
        public static bool FoundFirstTime2;
        public static bool AimType1FalseDetect;


        public static bool NeedSearchAim2;
        public static bool Aim2AttackDetected;

        public static int ShotFound = -1;
        public static int AttackFloodTimes = 0;
        public static float ChangeWeaponTime;
        public static WeaponIdType LastWatchWeapon = WeaponIdType.WEAPON_NONE;
        public static int ReallyAim2;
        public static float LastJumpHackTime;
        public static bool NeedDetectBHOPHack;
        public static int LastTriggerCursor;
        public static float LastDeathTime;
        public static List<Player> playerList = new List<Player>();
        public static List<Player> fullPlayerList = new List<Player>();
        public static int maxfalsepositiveaim3 = 6;
        public static int FovHackDetected;
        public static int ThirdHackDetected;
        public static bool SearchAim6 = false;
        public static List<WindowResolution> playerresolution = new List<WindowResolution>();
        public static int CheatKey;
        public static int Reloads;
        public static int Reloads2;
        public static bool WeaponAvaiabled;
        public static int WeaponAvaiabledFrameId;
        public static float WeaponAvaiabledFrameTime;
        public static bool UsingAnotherMethodWeaponDetection = false;

        public static float LastCmdHack;

        public static int AimType7Frames;
        public static float OldAimType7Time;
        public static int OldAimType7Frames;
        public static int AimType7Event;
        public static int BadVoicePacket;

        public static bool SearchJumpBug;

        public static int MaxIdealJumps = 7;
        public static int CurrentIdealJumpsStrike;
        public static bool SearchNextJumpStrike;


        public static List<string> CommandsDump = new List<string>();
        public static string framecrashcmd = "";


        public static float[] CDFrameYAngleHistory = new float[3] { 0.0f, 0.0f, 0.0f };

        public static float[] AngleXHistory = new float[5];

        public static List<WarnStruct> DemoScannerWarnList = new List<WarnStruct>();

        public static string LastWarnStr = "";
        public static float LastWarnTime;

        public static float LastAnglePunchSearchTime;

        public static List<AngleSearcher> angleSearchersPunch = new List<AngleSearcher>();


        public static List<AngleSearcher> angleSearchersView = new List<AngleSearcher>();
        public static float NewViewAngleSearcherAngle;

        public static string lastnormalanswer = "";

        public static float nospreadtest;

        public static GoldSource.NetMsgFrame CurrentNetMsgFrame;
        public static GoldSource.NetMsgFrame PreviousNetMsgFrame;
        public static GoldSource.NetMsgFrame NextNetMsgFrame = new GoldSource.NetMsgFrame();

        public static string TotalFreewareTool = "[ПОЛНОСТЬЮ БЕСПЛАТНЫЙ] [TOTALLY FREE]";

        public static string SourceCode = "https://github.com/UnrealKaraulov/UnrealDemoScanner";
        public static int usagesrccode;

        public static List<UcmdLerpAndMs> historyUcmdLerpAndMs = new List<UcmdLerpAndMs>();

        public static int cipid;
        public static float LastAim5Detected;
        public static float LastAim5DetectedReal;
        public static bool voicefound;
        public static float AimType8WarnTime;
        public static bool AimType8False;
        public static float AimType8WarnTime2;
        public static float bAimType8WarnTime;
        public static float bAimType8WarnTime2;
        public static int AimType8Warn;

        public static float Aim7PunchangleY;
        public static float nospreadtest2;
        public static float MaximumTimeBetweenFrames;
        public static bool GameEnd;
        public static int LostStopAttackButton;
        public static int ModifiedDemoFrames;
        public static int TimeShiftCount = -5;
        public static float LastAliveTime;
        public static float LastTeleportusTime;
        public static int Aim73FalseSkip = 2;
        public static int UserNameAndSteamIDField;
        public static int UserNameAndSteamIDField2;
        public static string LastUername = "\tNO NAME";
        public static float LastUsernameCheckTime;
        public static int MessageId;
        public static int SVC_CHOKEMSGID;
        public static int SVC_CLIENTUPDATEMSGID;
        public static int SVC_TIMEMSGID;
        public static uint LossPackets;
        public static uint LossPackets2;
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
        public static bool DemoScannerBypassDetected;
        public static int FramesOnGround;
        public static int FramesOnFly;

        public static int TotalFramesOnFly;
        public static int TotalFramesOnGround;
        public static int TotalAttackFramesOnFly;

        public static string ServerName = "";
        public static string MapName = "";
        public static string GameDir = "";
        public static byte StartPlayerID = 255;
        public static bool DealthMatch;
        public static float GameEndTime;
        public static int DuckHack2Strikes;
        public static int DuckHack1Strikes;
        public static bool Intermission;
        public static int ViewEntity = -1;
        public static int ViewModel = -1;
        public static uint LastPlayerEntity;
        public static int PlayerTeleportus;
        public static bool NeedRescanDemoForce;
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
        public static bool SearchOneFrameDuck;
        public static float NeedSearchUserAliveTime;
        public static float LastJumpHackFalseDetectionTime;
        public static int AngleDirectionChanges;
        public static int StrafeAngleDirectionChanges;
        public static float LastStrafeOptimizerDetectWarnTime = 0.0f;
        public static float LastCmdDuckTime;
        public static float LastCmdUnduckTime;
        public static int CurrentNetMsgFrameId;
        public static int StopAttackBtnFrameId;
        public static bool NeedSearchAim3;
        public static bool LossFalseDetection;
        public static float[] TimeShift4Times = new float[3] { 0.0f, 0.0f, 0.0f };
        public static int LastFrameDiff;
        public static bool AimType6FalseDetect;
        public static int CurJmpWarns;
        public static int CurJumpCount;
        public static int JmpWarn;
        public static int RealJumpEmulatorHackWarns;
        public static float MaxSpeed = 0.0f;
        public static float StartLengthSearchTime = 0.0f;
        public static float SecondFrameTime;
        public static float Aim8DetectionTimeY = 0.0f;
        public static float Aim8DetectionTimeX;
        public static float NewAttackTime;
        public static float NewAttackTimeAim9;
        public static float LastGameMaximizeTime;
        public static List<int> JumpHistory = new List<int>();
        public static int JumpEmulatorWarnPart1;
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
        public static int DesyncHackWarns = 0;
        public static float LastDesyncDetectTime = 0.0f;
        public static float LastDamageTime;
        public static int StartGameSecond = int.MaxValue;
        public static int CurrentGameSecond;
        public static int CurrentGameSecond2;
        public static float LastRealJumpTime;
        public static int BHOPJumpHistoryCount1Warn;
        public static uint LastWeaponReloadStatus;
        public static float LastScreenshotTime;
        public static float GameEndTime2;
        public static float GameStartTime;
        public static bool PlayerFrozen;
        public static float PlayerFrozenTime;
        public static float PlayerUnFrozenTime;
        public static int ReturnToGameDetects;
        public static int FovByFunc;
        public static bool IsScreenFade;
        public static float LastViewChange;
        public static bool HideWeapon;
        public static float HideWeaponTime;
        public static bool MINIMIZED = true;
        public static int NeedIgnoreAttackFlag;
        public static int NeedIgnoreAttackFlagCount;
        public static float LastSoundTime;
        public static int DesyncDetects = 0;
        public static float FoundBigVelocityTime;
        public static float FoundVelocityTime;
        public static float LastBeamFound;
        public static float LastForceCenterView;
        public static int LastIncomingSequence;
        public static int FrameErrors;
        public static int LastIncomingAcknowledged;
        public static int LastOutgoingSequence;


        public static int maxLastIncomingSequence;
        public static int maxLastIncomingAcknowledged;
        public static int maxLastOutgoingSequence;


        public static int BadSequences = 0;
        public static bool InBack = true;
        public static float LastMoveBack;

        public static bool InLook;
        public static float LastLookDisabled;
        public static float LastLookEnabled;
        public static int FlyJumps;
        public static bool SearchMoveHack1;
        public static string KnownSkyName = string.Empty;
        public static DateTime StartScanTime;

        public static string codecname;
        public static int WarnsAfterGameEnd;
        public static bool SKIP_RESULTS;
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

        public static bool ForceUpdateName;
        public static float LastAttackTime;
        public static float LastFloodAttackTime = 0.0f;
        public static float LastFloodDuckTime = 0.0f;
        public static bool LASTFRAMEISCLIENTDATA;
        public static int BadAnglesFoundCount;
        public static int MapAndCrc32_Top;
        public static int MapAndCrc32_Left;
        public static int FPS_OVERFLOW;
        public static float FpsOverflowTime;
        public static string DownloadLocation = "http://";
        public static List<string> FileDirectories = new List<string>();

        public static string SteamID = "";
        public static string RecordDate = "";

        public static bool NeedReportDateAndAuth = true;


        public static List<string> DownloadResources = new List<string>();
        public static ulong DownloadResourcesSize;

        public static List<float> LastSearchViewAngleY = new List<float>();

        public static int PluginEvents = -1;
        public static int BadEvents;
        public static int CurrentEvents;
        public static bool IsRussia;
        public static bool FirstMap = true;
        public static float LastAttackPressed;
        public static int PluginFrameNum = -1;
        public static string PluginVersion = string.Empty;
        public static int InitAimMissingSearch;
        public static uint LastLossPacketCount;

        public static int CurrentMsgBytes;
        public static int MaxBytesPerSecond;

        public static int MsgOverflowSecondsCount;

        public static int CurrentMsgCount;
        public static int MaxMsgPerSecond;
        public static int SkipChangeWeapon;
        public static byte VoiceQuality = 5;
        public static int SearchJumpHack5;

        public static int SearchJumpHack51;

        public static bool NeedSearchCMDHACK4 = false;
        public static bool BadPunchAngle = false;

        public static WeaponIdType GetWeaponByStr(string str)
        {
            if (str.ToLower().IndexOf("weapon_") == -1) str = "weapon_" + str.ToLower();

            str = str.ToUpper();

            foreach (string weaponName in Enum.GetNames(typeof(WeaponIdType)))
                if (weaponName == str) return (WeaponIdType)Enum.Parse(typeof(WeaponIdType), weaponName);

            return WeaponIdType.WEAPON_NONE;
        }

        public static bool IsChangeWeapon()
        {
            float retvar = Math.Abs(CurrentTime - ChangeWeaponTime);
            return retvar < 0.3f && retvar >= 0;
        }

        public static bool IsForceCenterView()
        {
            float retvar = Math.Abs(CurrentTime - ForceCenterViewTime);
            return retvar < 2.0f && retvar >= 0;
        }

        public static void AddPunchAngleSearcher(float angle)
        {
            AngleSearcher angleSearcher = new AngleSearcher();

            for (int i = 0; i < angleSearchersPunch.Count; i++)
                if (AngleBetweenAbsolute(angleSearchersPunch[i].angle, angle) < float.Epsilon)
                {
                    angleSearcher = angleSearchersPunch[i];
                    angleSearcher.searchcount = 0;
                    angleSearcher.searchtime = CurrentTime;
                    angleSearchersPunch[i] = angleSearcher;
                    return;
                }

            angleSearcher.angle = angle;
            angleSearcher.searchcount = 0;
            angleSearcher.searchtime = CurrentTime;
            angleSearchersPunch.Add(angleSearcher);
        }

        public static void UpdatePunchAngleSearchers()
        {
            for (int i = 0; i < angleSearchersPunch.Count; i++)
                if (!isAngleInPunchListY(angleSearchersPunch[i].angle))
                {
                    AngleSearcher angleSearcher = angleSearchersPunch[i];
                    angleSearcher.searchcount++;
                    if (angleSearcher.searchcount > 7)
                    {
                        LastAnglePunchSearchTime = angleSearcher.searchtime;
                        PunchWarnings++;
                        angleSearchersPunch.RemoveAt(i);
                        UpdatePunchAngleSearchers();
                        return;
                    }
                    angleSearchersPunch[i] = angleSearcher;
                }
                else
                {
                    angleSearchersPunch.RemoveAt(i);
                    UpdatePunchAngleSearchers();
                    return;
                }
        }

        public static void AddViewAngleSearcher(float angle)
        {
            AngleSearcher angleSearcher = new AngleSearcher();

            for (int i = 0; i < angleSearchersView.Count; i++)
                if (AngleBetweenAbsolute(angleSearchersView[i].angle, angle) < float.Epsilon)
                {
                    angleSearcher = angleSearchersView[i];
                    angleSearcher.searchcount = 0;
                    angleSearcher.searchtime = CurrentTime;
                    angleSearchersView[i] = angleSearcher;
                    return;
                }

            angleSearcher.angle = angle;
            angleSearcher.searchcount = 0;
            angleSearcher.searchtime = CurrentTime;
            angleSearchersView.Add(angleSearcher);
        }

        public static void UpdateViewAngleSearchers()
        {
            for (int i = 0; i < angleSearchersView.Count; i++)
                if (!isAngleInViewListY(angleSearchersView[i].angle))
                {
                    AngleSearcher angleSearcher = angleSearchersView[i];
                    angleSearcher.searchcount++;
                    if (angleSearcher.searchcount > 6)
                    {
                        LastAnglePunchSearchTime = angleSearcher.searchtime;
                        LostAngleWarnings++;
                        angleSearchersView.RemoveAt(i);
                        UpdateViewAngleSearchers();
                        return;
                    }
                    angleSearchersView[i] = angleSearcher;
                }
                else
                {
                    angleSearchersView.RemoveAt(i);
                    UpdateViewAngleSearchers();
                    return;
                }
        }


        public static string Rusifikator(string str)
        {
            str = str.Replace(" at ", " на ");

            return str;
        }

        public static void DemoScanner_AddInfo(string info, bool is_plugin = false, bool no_prefix = false)
        {
            ConsoleColor tmpcol = Console.ForegroundColor;
            if (IsRussia) info = Rusifikator(info);

            if (is_plugin)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                if (!no_prefix)
                {
                    if (IsRussia)
                        Console.Write("[Модуль] ");
                    else
                        Console.Write("[PLUGIN] ");
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                if (!no_prefix)
                {
                    if (IsRussia)
                        Console.Write("[ИНФОРМАЦИЯ] ");
                    else
                        Console.Write("[INFO] ");
                }
            }
            Console.WriteLine(info);
            Console.ForegroundColor = tmpcol;
        }


        public static void DemoScanner_AddWarn(string warn, bool detected = true, bool log = true, bool skipallchecks = false, bool uds_plugin = false)
        {
            if (IsRussia) warn = Rusifikator(warn);

            if (GameEnd && !uds_plugin)
            {
                if (detected) WarnsAfterGameEnd++;

                return;
            }
            if (Math.Abs(LastWarnTime - CurrentTime) < float.Epsilon && !uds_plugin) return;
            LastWarnTime = CurrentTime;
            WarnStruct warnStruct = new WarnStruct
            {
                Warn = warn,
                WarnTime = LastWarnTime,
                Detected = detected,
                Log = log,
                SkipAllChecks = skipallchecks,
                Visited = false,
                Plugin = uds_plugin
            };
            DemoScannerWarnList.Add(warnStruct);
        }

        public static void UpdateWarnList(bool force = false)
        {
            for (int i = 0; i < DemoScannerWarnList.Count; i++)
            {
                WarnStruct curwarn = DemoScannerWarnList[i];
                if (!curwarn.Visited && (Math.Abs(CurrentTime - curwarn.WarnTime) > 0.2f || force))
                {
                    curwarn.Visited = true;

                    ConsoleColor tmpcol = Console.ForegroundColor;
                    if (curwarn.Detected && !IsPlayerLossConnection(curwarn.WarnTime) && RealAlive || curwarn.SkipAllChecks)
                    {
                        if (curwarn.Plugin)
                        {
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            if (IsRussia)
                                Console.Write("[Модуль] ");
                            else
                                Console.Write("[PLUGIN] ");
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            if (IsRussia)
                                Console.Write("[ОБНАРУЖЕНО] ");
                            else
                                Console.Write("[DETECTED] ");

                            LastLossPacket = 0.0f;
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
                                Console.Write("[Модуль] ");
                            else
                                Console.Write("[PLUGIN] ");
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Gray;
                            if (IsRussia)
                                Console.Write("[ПРЕДУПРЕЖДЕНИЕ] ");
                            else
                                Console.Write("[WARNING] ");
                        }

                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.Write(curwarn.Warn);
                        Console.ForegroundColor = ConsoleColor.Red;
                        if (IsPlayerLossConnection(curwarn.WarnTime))
                        {
                            if (IsRussia)
                                Console.Write(" (ЛАГ)");
                            else
                                Console.Write(" (LAG)");
                        }
                        else if (!RealAlive)
                        {
                            if (IsRussia)
                                Console.Write(" (УМЕР)");
                            else
                                Console.Write(" (DEAD)");
                        }
                        Console.WriteLine();
                        /* else if (Math.Abs(curwarn.WarnTime - FpsOverflowTime) <= 0.22)
                        //     Console.WriteLine(" (BAD FPS)");*/
                        //else
                        //{
                        //    if (IsRussia)
                        //        Console.Write(" (ИНФА)");
                        //    else
                        //        Console.Write(" (FALSE)");
                        //}
                    }


                    if (curwarn.Detected) LossFalseDetection = false;

                    if (curwarn.Log)
                    {
                        TextComments.WriteLine(curwarn.Warn);
                        AddViewDemoHelperComment(curwarn.Warn);
                    }


                    DemoScannerWarnList[i] = curwarn;

                    Console.ForegroundColor = tmpcol;
                }
            }
        }

        public static void CheckConsoleCheat(string s)
        {
            string s2 = s;
            if (s[0] == '+' || s[0] == '-') s2 = s.Remove(0, 1);

            if (!whiteListCMDLIST.Contains(s2) && !unknownCMDLIST.Contains(s2))
            {
                if (IsRussia)
                    unknownCMDLIST.Add("[ОБНАРУЖЕНА] [НЕИЗВЕСТНАЯ КОМАНДА] : \"" + s + "\" at (" + CurrentTime + ")" + CurrentTimeString);
                else
                    unknownCMDLIST.Add("[DETECTED] [UNKNOWN CMD] : \"" + s + "\" at (" + CurrentTime + ")" + CurrentTimeString);
            }
        }

        public static void CheckConsoleCommand(string s2, bool isstuff = false)
        {
            string s = s2.Trim().TrimBad();

            if (!isstuff) CheckConsoleCheat(s);

            if (FrameCrash > 4)
            {
                FirstDuck = false;
                FirstJump = false;
                FirstAttack = false;
                SearchJumpBug = false;
                if (JumpWithAlias >= -1 && FrameCrash <= 7) JumpWithAlias--;
            }

            if (LastStuffCmdCommand != "" && s == LastStuffCmdCommand.Trim().TrimBad())
            {
                LastStuffCmdCommand = "";
                CommandsDump.Add("wait" + (CurrentFrameId - LastCmdFrameId) + ";");
                if (IsRussia)
                    CommandsDump.Add(CurrentTimeString + " [НОМЕР КАДРА: " + CurrentFrameId + "] : " + s + "(" + CurrentTime + ") --> ВЫПОЛНЕНО СЕРВЕРОМ");
                else
                    CommandsDump.Add(CurrentTimeString + " [FRAME NUMBER: " + CurrentFrameId + "] : " + s + "(" + CurrentTime + ") --> EXECUTED BY SERVER");

                return;
            }

            LastStuffCmdCommand = "";
            if (isstuff)
            {
                CommandsDump.Add("wait" + (CurrentFrameId - LastCmdFrameId) + ";");
                if (IsRussia)
                    CommandsDump.Add(CurrentTimeString + " [НОМЕР КАДРА: " + CurrentFrameId + "] : " + s + "(" + CurrentTime + ") --> ВЫПОЛНЕНО ЧЕРЕЗ STUFFTEXT");
                else
                    CommandsDump.Add(CurrentTimeString + " [FRAME NUMBER: " + CurrentFrameId + "] : " + s + "(" + CurrentTime + ") --> EXECUTED BY STUFFTEXT");

            }
            else
            {
                CommandsDump.Add("wait" + (CurrentFrameId - LastCmdFrameId) + ";");
                if (IsRussia)
                    CommandsDump.Add(CurrentTimeString + " [НОМЕР КАДРА: " + CurrentFrameId + "] : " + s + "(" + CurrentTime + ")");
                else
                    CommandsDump.Add(CurrentTimeString + " [FRAME NUMBER: " + CurrentFrameId + "] : " + s + "(" + CurrentTime + ")");

            }

            if (s.ToLower().IndexOf("-showscores") > -1)
            {
                LastAltTabStart = LastCmdTimeString;
                AltTabEndSearch = true;
            }

            if (s.ToLower().IndexOf("-") > -1 && framecrashcmd != s.ToLower())
            {
                framecrashcmd = s.ToLower();
                FrameCrash++;
                if (FrameCrash > 7 && AltTabEndSearch)
                {
                    AltTabCount2++;
                    AltTabEndSearch = false;
                    MINIMIZED = true;

                    if (IsRussia)
                        DemoScanner_AddInfo("Игрок свернул игру с " + LastAltTabStart + " по " + CurrentTimeString);
                    else
                        DemoScanner_AddInfo("Player minimized game from " + LastAltTabStart + " to " + CurrentTimeString);

                    LastGameMaximizeTime = CurrentTime;
                }
            }
            else
            {
                AltTabEndSearch = false;
                FrameCrash = 0;
            }

            if (s.ToLower().IndexOf("+") > -1)
            {
                AltTabEndSearch = false;
                FrameCrash = 0;
            }

            if (s.ToLower().IndexOf("+reload") > -1)
            {
                FrameCrash = 0;
            }
            else if (s.ToLower().IndexOf("-reload") > -1)
            {
            }

            if (s.ToLower().IndexOf("+klook") > -1) CheatKey++;

            if (s.ToLower().IndexOf("+camin") > -1 && FirstJump)
            {
                CaminCount++;
                if (CaminCount == 3)
                {
                    if (Math.Abs(CurrentTime - LastJumpTime) < 1.0)
                    {
                        if (JumpHackCount < 10) DemoScanner_AddWarn("[XTREME JUMPHACK] at (" + CurrentTime + ") " + CurrentTimeString);

                        JumpHackCount++;
                    }

                    CaminCount = 0;
                }
            }
            else if (s.ToLower().IndexOf("-camin") > -1)
            {
                CaminCount = 0;
            }

            if (s.ToLower().IndexOf("force_centerview") > -1)
            {
                ForceCenterViewTime = CurrentTime;
                FoundForceCenterView++;
            }

            if (s.ToLower().IndexOf("slot") > -1 || s.ToLower().IndexOf("invprev") > -1 ||
                s.ToLower().IndexOf("invnext") > -1)
            {
                InitAimMissingSearch = -1;
                SkipNextAttack = 2;
                SelectSlot = 2;
                NeedSearchAim2 = false;
                Aim2AttackDetected = false;
                ShotFound = -1;
                ChangeWeaponTime = CurrentTime;
            }

            if (s.ToLower().IndexOf("attack2") == -1 && s.ToLower().IndexOf("attack3") == -1)
            {
                if (s.ToLower().IndexOf("+attack") > -1)
                {
                    AttackFloodTimes++;

                    if (AttackFloodTimes > 4)
                    {
                        if (Math.Abs(CurrentTime - LastFloodAttackTime) > 20.0)
                        {
                            DemoScanner.DemoScanner_AddWarn(
                                    "[ATTACK FLOOD TYPE 1] at (" + DemoScanner.CurrentTime +
                                    ") " + DemoScanner.CurrentTimeString);
                            TotalAimBotDetected++;
                            LastFloodAttackTime = CurrentTime;
                        }
                        AttackFloodTimes = 0;
                    }

                    FirstAttack = true;
                    FrameCrash = 0;
                    if (IsUserAlive()) attackscounter++;

                    NeedSearchAim3 = false;
                    /* if (DemoScanner.DEBUG_ENABLED)
                     {
                         Console.WriteLine("User alive func:" + IsUserAlive() + ". User real alive ? : " + RealAlive + ". Weapon:" + CurrentWeapon + ".Frame:" + (CurrentFrameId - LastCmdFrameId)
                             + ".Frame2: " + (CurrentFrameIdWeapon - WeaponAvaiabledFrameId) + ".Frame3: " + (CurrentFrameId - LastCmdFrameId));
                     }*/

                    LastAttackTime = CurrentTime;

                    if (IsUserAlive())
                    {
                        if (InitAimMissingSearch < 0)
                        {
                            InitAimMissingSearch++;
                        }
                        else
                        {
                            if (Math.Abs(CurrentTime - IsAttackLastTime) > 2.0)
                            {
                                if (FirstAttack && IsRealWeapon())
                                {
                                    if (InitAimMissingSearch <= 0) InitAimMissingSearch = 3;
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
                                //DemoScanner_AddWarn("[AIM TYPE 6] at (" + CurrentTime + "):" + DemoScanner.CurrentTimeString, false);
                                AutoAttackStrikes = 0;
                                AimType6FalseDetect = false;
                            }
                            else if (!AimType6FalseDetect)
                            {
                                //DemoScanner_AddWarn("[AIM TYPE 6] at (" + CurrentTime + "):" + DemoScanner.CurrentTimeString, false);
                                AimType6FalseDetect = false;
                                //SilentAimDetected++;
                                //Console.ForegroundColor = tmpcol; 
                                AutoAttackStrikes = 0;
                            }
                        }
                        else if (CurrentFrameId - LastCmdFrameId <= 7
                                                 && CurrentFrameId - LastCmdFrameId > 1)
                        {
                            if (CurrentFrameIdWeapon - WeaponAvaiabledFrameId == 1) AimType6FalseDetect = true;

                            if (!AimType6FalseDetect && WeaponAvaiabled && CurrentFrameIdWeapon - WeaponAvaiabledFrameId <= 7
                                && CurrentFrameIdWeapon - WeaponAvaiabledFrameId > 1)
                                AimType6FalseDetect = false;

                            if (CurrentWeapon == WeaponIdType.WEAPON_DEAGLE ||
                                CurrentWeapon == WeaponIdType.WEAPON_USP ||
                                CurrentWeapon == WeaponIdType.WEAPON_P228 ||
                                CurrentWeapon == WeaponIdType.WEAPON_ELITE ||
                                CurrentWeapon == WeaponIdType.WEAPON_FIVESEVEN ||
                                CurrentWeapon == WeaponIdType.WEAPON_GLOCK18 ||
                                CurrentWeapon == WeaponIdType.WEAPON_GLOCK
                               )
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

                                if (AutoAttackStrikesLastID == AutoAttackStrikesID) AutoAttackStrikes--;

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

                        if (IsAttack)
                        {
                            AttackErrors++;
                            LastAttackHack = CurrentTime;
                        }

                        if (AttackCheck < 0 && CurrentFrameDuplicated == 0)
                        {
                            /* if (DemoScanner.DEBUG_ENABLED)
                                 Console.WriteLine("Attack. Start search aim type 1!");*/
                            if (SkipNextAttack == 0) SkipNextAttack = 1;

                            AttackCheck = 1;
                        }
                        Aim2AttackDetected = false;
                        NeedSearchAim2 = true;
                        IsAttack = true;
                        IsAttackLastTime = CurrentTime;
                    }
                    else
                    {
                        if (InitAimMissingSearch > 0) InitAimMissingSearch = 0;

                        AutoAttackStrikes = 0;
                        NeedSearchViewAnglesAfterAttack = 0;
                        Aim2AttackDetected = false;
                        NeedSearchAim2 = false;
                        IsAttack = true;
                        IsAttackLastTime = 0.0f;
                    }

                    ShotFound = -1;
                    WeaponChanged = false;
                }
                else if (s.ToLower().IndexOf("-attack") > -1)
                {
                    if (IsUserAlive())
                    {
                        LerpBeforeStopAttack = CurrentFrameLerp;
                        LerpSearchFramesCount = 9;
                    }

                    NeedSearchViewAnglesAfterAttack = 0;

                    if (IsUserAlive() && Math.Abs(CurrentTime) > float.Epsilon &&
                        Math.Abs(CurrentTime - IsAttackLastTime) < 0.05 && Math.Abs(IsAttackLastTime - CurrentTime) > float.Epsilon)
                        AttackErrors++;

                    // Если игрок жив, атакует, время атаки больше чем 150мс, атака остановлена на 2 кадра раньше чем нажат -attack
                    if (IsUserAlive() && Math.Abs(CurrentTime) > float.Epsilon &&
                      Math.Abs(CurrentTime - IsAttackLastTime) > 0.15 &&
                        Math.Abs(CurrentTime - IsNoAttackLastTime) > 0.15 && Math.Abs(IsAttackLastTime - CurrentTime) > float.Epsilon
                        && IsAttack &&
                        CurrentNetMsgFrameId - StopAttackBtnFrameId > 2 && StopAttackBtnFrameId != 0 && NeedSearchAim3)
                    {
                        NeedSearchAim3 = false;
                        /*if (AUTO_LEARN_HACK_DB)
                        {
                            ENABLE_LEARN_HACK_DEMO = true;
                            ENABLE_LEARN_HACK_DEMO_FORCE_SAVE = true;
                        }*/
                        DemoScanner_AddWarn("[AIM TYPE 3 " + CurrentWeapon + "] at (" + StopAttackBtnFrameId +
                                                    "):" + CurrentTimeString, false);
                    }
                    NeedIgnoreAttackFlag = 1;
                    NeedSearchAim2 = false;
                    NeedWriteAim = false;
                    IsNoAttackLastTime = CurrentTime;
                    IsNoAttackLastTime2 = CurrentTime2;
                    IsAttack = false;
                    AttackCheck = -1;
                    Aim2AttackDetected = false;
                    SelectSlot--;
                    if (ShotFound > 2)
                        ReallyAim2 = 1;
                    else if (ShotFound > 1) ReallyAim2 = 2;

                    ShotFound = -1;
                    AttackFloodTimes = 0;
                }
            }

            if (s.ToLower().IndexOf("+duck") > -1)
            {
                if (!IsDuckPressed)
                    if (CurrentIdealJumpsStrike > 6) CurrentIdealJumpsStrike--;
                FrameCrash = 0;
                IsDuckPressed = true;
                DuckStrikes++;
                FirstDuck = true;
                LastDuckTime = CurrentTime;


                if (DuckStrikes == 8)
                {
                    if (Math.Abs(CurrentTime - LastFloodDuckTime) > 20.0)
                    {
                        DemoScanner.DemoScanner_AddWarn(
                                "[DUCK FLOOD TYPE 1] at (" + DemoScanner.CurrentTime +
                                ") " + DemoScanner.CurrentTimeString);
                        JumpHackCount++;
                        LastFloodDuckTime = CurrentTime;
                    }
                }
            }
            else if (s.ToLower().IndexOf("-duck") > -1)
            {
                IsDuckPressed = false;
                FirstDuck = true;
                LastUnDuckTime = CurrentTime;
                DuckStrikes = 0;
            }

            if (DetectStrafeOptimizerStrikes > 5)
            {
                if (!StrafeOptimizerFalse && StrafeAngleDirectionChanges > 4) DemoScanner_AddWarn("[STRAFE OPTIMIZER" + /*(DemoScanner.DetectStrafeOptimizerStrikes <= 5 ? " ( WARN ) " : "") +*/ "] at (" + CurrentTime + ") : " + CurrentTimeString);

                /* if (DemoScanner.DEBUG_ENABLED)
                     Console.WriteLine(" --- DIR CHANGE : " + DemoScanner.StrafeAngleDirectionChanges + " --- ");
                */
                if (DetectStrafeOptimizerStrikes > 5)
                {
                    StrafeOptimizerFalse = false;
                    StrafeAngleDirectionChanges = 0;
                    DetectStrafeOptimizerStrikes = 0;
                }
            }

            if (s.ToLower().IndexOf("+forward") > -1)
            {
                InForward = true;
                LastMoveForward = CurrentTime;
                StrafeOptimizerFalse = false;
            }
            else if (s.ToLower().IndexOf("-forward") > -1)
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

            if (s.ToLower().IndexOf("+back") > -1)
            {
                InBack = true;
                LastMoveBack = CurrentTime;
            }
            else if (s.ToLower().IndexOf("-back") > -1)
            {
                InBack = false;
                LastMoveBack = CurrentTime;
            }


            if (s.ToLower().IndexOf("+strafe") > -1)
            {
                InStrafe = true;
                LastStrafeEnabled = CurrentTime;
            }
            else if (s.ToLower().IndexOf("-strafe") > -1)
            {
                InStrafe = false;
                LastStrafeDisabled = CurrentTime;
            }




            if (s.ToLower().IndexOf("+mlook") > -1)
            {
                InLook = true;
                LastLookEnabled = CurrentTime;
            }
            else if (s.ToLower().IndexOf("-mlook") > -1)
            {
                InLook = false;
                LastLookDisabled = CurrentTime;
            }


            if (s.ToLower().IndexOf("+use") > -1) LastUseTime = CurrentTime;

            if (s.ToLower().IndexOf("+moveleft") > -1)
            {
                if (RealAlive && (!CurrentFrameOnGround || CurrentFrameJumped))
                {
                    if (Math.Abs(CurrentTime - LastUnMoveRight) < float.Epsilon && Math.Abs(CurrentTime - LastMoveRight) < 1.00f)
                    {
                        /*if (DemoScanner.DEBUG_ENABLED)
                            Console.WriteLine("11111:" + AngleStrikeDirection);
                        */
                        if (!(AngleStrikeDirection < 2 && AngleStrikeDirection > -90)) StrafeOptimizerFalse = true;

                        if (Math.Abs(CurrentTime - LastMoveRight) > 0.50f
                            || Math.Abs(CurrentTime - LastMoveRight) < 0.01f)
                            StrafeOptimizerFalse = true;

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
            else if (s.ToLower().IndexOf("-moveleft") > -1)
            {
                if (RealAlive && (!CurrentFrameOnGround || CurrentFrameJumped))
                {
                    if (LastCmd.ToLower().IndexOf("-moveright") > -1)
                        if (DetectStrafeOptimizerStrikes > 1) StrafeOptimizerFalse = true;
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
            else if (s.ToLower().IndexOf("+moveright") > -1)
            {
                if (RealAlive && (!CurrentFrameOnGround || CurrentFrameJumped))
                {
                    if (Math.Abs(CurrentTime - LastUnMoveLeft) < float.Epsilon && Math.Abs(CurrentTime - LastMoveLeft) < 1.00f)
                    {
                        /* if (DemoScanner.DEBUG_ENABLED)
                             Console.WriteLine("22222:" + AngleStrikeDirection);*/
                        if (!(AngleStrikeDirection > -2 && AngleStrikeDirection < 90)) StrafeOptimizerFalse = true;

                        if (Math.Abs(CurrentTime - LastMoveLeft) > 0.50f
                            || Math.Abs(CurrentTime - LastMoveLeft) < 0.01)
                            StrafeOptimizerFalse = true;

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
                MoveRight = true;
                LastMoveRight = CurrentTime;
            }
            else if (s.ToLower().IndexOf("-moveright") > -1)
            {
                if (RealAlive && (!CurrentFrameOnGround || CurrentFrameJumped))
                {
                    if (LastCmd.ToLower().IndexOf("-moveleft") > -1)
                        if (DetectStrafeOptimizerStrikes > 1) StrafeOptimizerFalse = true;
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

            if (s.ToLower().IndexOf("+jump") > -1)
            {
                SearchJumpBug = true;
                FrameCrash = 0;
                FirstJump = true;
                BHOP_GroundWarn = 0;
                BHOP_JumpWarn = 0;
                BHOP_GroundSearchDirection = 0;

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
                    if (CurrentFrameId - LastCmdFrameId == 2)
                        JmpWarn++;
                    else
                        JmpWarn--;

                    JumpCount++;

                    if (Math.Abs(LastJumpTime - CurrentTime) < float.Epsilon && Math.Abs(LastJumpTime) > float.Epsilon) MouseJumps++;

                    if (IsJump) MouseJumps++;
                }

                LastJumpTime = CurrentTime;

                JumpHistory.Add(CurrentFrameId - LastJumpFrame);
                LastJumpFrame = CurrentFrameId;

                JumpHackCount2 = -1;
                IsJump = true;
            }
            if (s.ToLower().IndexOf("-jump") > -1)
            {
                FirstJump = true;
                if (IsUserAlive())
                {
                    JumpCount4++;
                    if (Math.Abs(LastJumpTime - CurrentTime) < float.Epsilon && Math.Abs(LastJumpTime) > float.Epsilon)
                    {
                        MouseJumps++;

                        LastJumpHackFalseDetectionTime = CurrentTime;
                    }

                    if (!IsJump && SearchJumpBug) JumpWithAlias++;


                    if (BHOP_JumpWarn > 2)
                    {
                        BHOPcount += BHOP_JumpWarn - 1;
                        if (Math.Abs(CurrentTime - LastBhopTime) > 1.0f && Math.Abs(LastBhopTime) > float.Epsilon) DemoScanner_AddWarn("[BHOP TYPE 1] at (" + CurrentTime + ") " + CurrentTimeString + " [" + (BHOP_JumpWarn - 1) + "]" + " times.");

                        LastBhopTime = CurrentTime;
                    }

                    if (BHOP_GroundWarn > 2)
                    {
                        if (Math.Abs(CurrentTime - LastBhopTime) > 0.5f && Math.Abs(CurrentTime - LastBhopTime) < 2.5f && Math.Abs(LastBhopTime) > float.Epsilon)
                        {
                            BHOPcount += BHOP_GroundWarn - 1;
                            DemoScanner_AddWarn("[BHOP TYPE 2] at (" + CurrentTime + ") " + CurrentTimeString + " [" + (BHOP_GroundWarn - 1) + "]" + " times.",
                                BHOP_GroundWarn > 3, BHOP_GroundWarn > 3);
                        }
                        LastBhopTime = CurrentTime;
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
            float xDelta = p2.X - p1.X;
            float yDelta = p2.Y - p1.Y;

            return (float)Math.Abs(Math.Sqrt(Math.Pow(xDelta, 2) + Math.Pow(yDelta, 2)));
        }

        public static float GetDistanceAngle(FPoint p1, FPoint p2)
        {
            float xDelta = AngleBetween(p2.X, p1.X);
            float yDelta = AngleBetween(p2.Y, p1.Y);

            return (float)Math.Abs(Math.Sqrt(Math.Pow(xDelta, 2) + Math.Pow(yDelta, 2)));
        }

        public static void PrintNodesRecursive(TreeNode oParentNode)
        {
            outFrames.Add(oParentNode.Text);
            foreach (TreeNode oSubNode in oParentNode.Nodes) PrintNodesRecursive(oSubNode);
        }

        public static bool IsRealWeapon()
        {
            bool retval = CurrentWeapon != WeaponIdType.WEAPON_NONE &&
                   CurrentWeapon != WeaponIdType.WEAPON_BAD
                   && CurrentWeapon != WeaponIdType.WEAPON_BAD2;
            //Console.WriteLine("Weapon:" + retval);
            return retval;
        }

        public static bool IsUserAlive()
        {
            return RealAlive && IsRealWeapon();
        }

        public static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;

            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        public static byte[] GetNullBytes(int len)
        {
            byte[] retval = new byte[len];
            Array.Clear(retval, 0, retval.Length);
            return retval;
        }

        public static void AddViewDemoHelperComment(string comment, float speed = 0.5f)
        {
            ViewDemoCommentCount++;
            byte[] utf8 = Encoding.UTF8.GetBytes(comment);
            ViewDemoHelperComments.Write(CurrentTime);
            ViewDemoHelperComments.Write(speed);
            ViewDemoHelperComments.Write(xcommentdata);
            ViewDemoHelperComments.Write(utf8.Length + 1);
            ViewDemoHelperComments.Write(utf8);
            ViewDemoHelperComments.Write(GetNullBytes(1));
        }


        public static void AddResolution(int x, int y)
        {
            if (x != 0 && y != 0)
            {
                foreach (WindowResolution s in playerresolution)
                    if (s.x == x && s.y == y) return;

                WindowResolution tmpres = new WindowResolution
                {
                    x = x,
                    y = y
                };
                playerresolution.Add(tmpres);
            }
        }

        public static string TrimBad(this string value)
        {
            return value.Replace("\n", "").Replace("\r", "");
        }

        public static string GetAim7String(ref int val1, ref int val2, ref int val3, int type, float angle, ref bool detect)
        {
            if (val1 > 11) val1 = 11;

            if (val2 > 11) val2 = 11;

            val1 -= 1;
            val2 -= 1;

            val1 *= 10;
            val2 *= 10;

            if (angle < 0.0001f)
                val3 = 10;
            else if (angle < 0.001f)
                val3 = 20;
            else if (angle < 0.01f)
                val3 = 30;
            else if (angle < 0.1f)
                val3 = 40;
            else if (angle < 1.0f)
                val3 = 50;
            else if (angle < 2.0f)
                val3 = 60;
            else if (angle < 3.0f)
                val3 = 70;
            else if (angle < 4.0f)
                val3 = 80;
            else if (angle < 10.0f)
                val3 = 90;
            else if (angle < 50.0f)
                val3 = 100;
            else
                val3 = 1000;

            if (type - 1 == 4)
            {
                detect = false;

                if (val3 > 50) detect = true;
            }

            if (Math.Abs(Aim7PunchangleY) > float.Epsilon)
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
            /*if (AUTO_LEARN_HACK_DB)
            {
                ENABLE_LEARN_HACK_DEMO = true;
                ENABLE_LEARN_HACK_DEMO_FORCE_SAVE = true;
            }*/
            return "[AIM TYPE 7." + (type - 1) + " MATCH1:" + val1 + "% MATCH2:" +
           val2 + "% MATCH3:" + val3 + "%]";
        }

        [STAThread]
        private static void Main(string[] args)
        {
            try
            {
                ConsoleHelper.CenterConsole();
                Console.SetWindowSize(114, 32);
                Console.SetBufferSize(114, 5555);
                Console.SetWindowSize(115, 32);
                ConsoleHelper.CenterConsole();
            }
            catch
            {
                Console.WriteLine("Error in console settings!");
            }

            try
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Title =
                    "[ANTICHEAT/ANTIHACK] Unreal Demo Scanner " + PROGRAMVERSION + ". Demo:" + DemoName +
                    "." + TotalFreewareTool;
            }
            catch
            {
                Console.WriteLine("Error Fatal");
                return;
            }
            string CurrentDemoFilePath = "";
            bool filefound = false;

            foreach (string arg in args)
            {
                CurrentDemoFilePath = arg.Replace("\"", "");
                if (File.Exists(CurrentDemoFilePath))
                {
                    filefound = true;
                }
                else if (arg.IndexOf("-debug") > -1)
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
                else if (arg.IndexOf("-skip") > -1)
                {
                    SKIP_RESULTS = true;
                }
                /*else if (arg.IndexOf("-learn_clearn") > -1)
                {
                   ENABLE_LEARN_CLEAN_DEMO = true;
                    if (IsRussia)
                        Console.WriteLine("Активировано машинное обучение чистых демо файлов!");
                    else
                        Console.WriteLine("ACTIVATED MACHINE LEARN FEATURE FOR CLEAN DEMOS!");
                }
                else if (arg.IndexOf("-learn_hack") > -1)
                {
                    AUTO_LEARN_HACK_DB = true;
                    if (IsRussia)
                        Console.WriteLine("Активировано машинное демо файлов только с читами!");
                    else
                        Console.WriteLine("ACTIVATED MACHINE LEARN FEATURE FOR CHEAT DEMOS!");
                }*/
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
                        Console.WriteLine("ИГРОК ОТМЕЧЕН ЖИВЫМ С НАЧАЛА ДЕМО!");
                    else
                        Console.WriteLine("SCAN WITH FORCE USER ALIVE AT START DEMO!");
                }
            }



            if (!SKIP_RESULTS)
                try
                {
                    Console.WriteLine("Search for updates...(hint: to change language, need remove lang file!)");
                    WebClient myWebClient = new WebClient();
                    string str_from_github = myWebClient.
                        DownloadString("https://raw.githubusercontent.com/UnrealKaraulov/UnrealDemoScanner/main/SourceCode/UnrealDemoScanner.cs");
                    Console.Clear();
                    if (str_from_github.IndexOf("PROGRAMVERSION") > 0)
                    {
                        Regex regex = new Regex(@"PROGRAMVERSION\s+=\s+""(.*?)"";");
                        Match match = regex.Match(str_from_github);
                        if (match.Success)
                            if (match.Groups[1].Value != PROGRAMVERSION) Console.WriteLine("Found new version \"" + match.Groups[1].Value + "\"! Current version:\"" + PROGRAMVERSION + "\".");
                    }
                    Thread.Sleep(1000);
                }
                catch
                {

                }

            string CurrentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (!SKIP_RESULTS)
            {


                if (!File.Exists(CurrentDir + @"\lang.ru") && !File.Exists(CurrentDir + @"\lang.en"))
                {
                    Console.Write("Enter language EN - Engish / RU - Russian:");
                    string lang = Console.ReadLine();
                    if (lang.ToLower() == "en")
                        File.Create(CurrentDir + @"\lang.en").Close();
                    else
                        File.Create(CurrentDir + @"\lang.ru").Close();
                }

                if (File.Exists(CurrentDir + @"\lang.en"))
                    IsRussia = false;
                else
                {
                    IsRussia = true;
                    // System.Console.OutputEncoding = Encoding.GetEncoding(1251);
                }
            }


            try
            {
                outFrames = new List<string>();
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Unreal Demo Scanner " + PROGRAMVERSION + " " + TotalFreewareTool);
                Console.ForegroundColor = ConsoleColor.Yellow;
                if (IsRussia)
                    Console.WriteLine("Скачивайте последнюю версию по ссылке :");
                else
                    Console.WriteLine("Download latest version or source code :");

                Console.WriteLine(GetSourceCodeString());
                Console.ForegroundColor = ConsoleColor.Red;
                if (IsRussia)
                    Console.WriteLine("БАЗА ДАННЫХ СОДЕРЖИТ СЛЕДУЮЩИЕ ВИДЫ ЧИТОВ И ХАКОВ:");
                else
                    Console.WriteLine("THIS BASE CONTAIN NEXT CHEAT/HACK:");
            }
            catch
            {
                Console.WriteLine("Error fatal");
                return;
            }

            Console.WriteLine("[TRIGGER BOT]");
            Console.WriteLine("[AIM]");
            Console.WriteLine("[BHOP]");
            Console.WriteLine("[JUMPHACK]");
            Console.WriteLine("[STRAFEHACK]");
            Console.WriteLine("[MOVEMENTHACK]");
            Console.WriteLine("[WHEELJUMP]");
            Console.ForegroundColor = ConsoleColor.DarkGray;

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

            if (IsRussia)
                Console.WriteLine("Перетащите демо файл в это окно или введите путь вручную:");
            else
                Console.WriteLine("Drag & drop .dem file. Or enter path manually:");

            if (!filefound)
            {
                CurrentDemoFilePath = "NOFILE";
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
                    /*else if (CurrentDemoFilePath.IndexOf("-learn_clearn") == 0)
                    {
                        ENABLE_LEARN_CLEAN_DEMO = true;
                        Console.WriteLine("ACTIVATED MACHINE LEARN FEATURE FOR CLEAN DEMOS!");
                    }
                    else if (CurrentDemoFilePath.IndexOf("-learn_hack") == 0)
                    {
                        AUTO_LEARN_HACK_DB = true;
                        Console.WriteLine("ACTIVATED MACHINE LEARN FEATURE FOR CHEAT DEMOS!");
                    }*/
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


            StartScanTime = DateTime.Now;
            if (!DUMP_ALL_FRAMES)
                if (File.Exists(CurrentDir + @"\Frames.txt"))
                {
                    DUMP_ALL_FRAMES = true;
                    Console.WriteLine("Dump mode activated.");
                }

            try
            {
                if (DUMP_ALL_FRAMES)
                {
                    File.Delete(CurrentDir + @"\Frames.txt");
                    File.Create(CurrentDir + @"\Frames.txt").Close();

                    if (File.Exists(CurrentDir + @"\Frames.txt"))
                        File.AppendAllText(CurrentDir + @"\Frames.txt",
                            "Полный дамп демо в текстовом формате\n");
                }
            }
            catch
            {
                Console.WriteLine("Error fatal");
                return;
            }
            if (TotalFreewareTool.Length != 37) return;

            CurrentDemoFile = CrossDemoParser.Parse(CurrentDemoFilePath);

            if (File.Exists(CurrentDemoFilePath.Remove(CurrentDemoFilePath.Length - 3) +
                            "log"))
            {
                if (File.Exists(CurrentDemoFilePath.Remove(CurrentDemoFilePath.Length - 3) +
                                "log.bak"))
                    try
                    {
                        File.Delete(CurrentDemoFilePath.Remove(CurrentDemoFilePath.Length - 3) +
                                    "log.bak");
                    }
                    catch
                    {
                        Console.WriteLine("Error: No access to log file.");
                    }

                try
                {
                    File.Delete(CurrentDemoFilePath.Remove(CurrentDemoFilePath.Length - 3) +
                                "log");
                }
                catch
                {
                    Console.WriteLine(
                        "File " +
                        CurrentDemoFilePath.Remove(CurrentDemoFilePath.Length - 3) + "log" +
                        " open error : No access to remove!");
                    Console.Write("No access to file... Try again!");
                    Console.ReadKey();
                    return;
                }
            }

            try
            {
                TextComments = new StreamWriter(TextCommentsStream);
            }
            catch
            {
                Console.WriteLine(
                    "File " + CurrentDemoFilePath.Remove(CurrentDemoFilePath.Length - 3) +
                    "log" + " open error : No access to create!");
                Console.Write("No access to file... Try again!");
                Console.ReadKey();
                return;
            }

            if (SourceCode.Length != 51) return;

            ViewDemoHelperComments = new BinaryWriter(new MemoryStream());
            ViewDemoHelperComments.Write(1751611137);
            ViewDemoHelperComments.Write(0);

            Console.WriteLine("DEMO " + Path.GetFileName(CurrentDemoFilePath));
            DemoName = Truncate(Path.GetFileName(CurrentDemoFilePath), 25);

            if (CurrentDemoFile.GsDemoInfo.ParsingErrors.Count > 0)
            {
                Console.WriteLine("Parse errors:");
                Console.ForegroundColor = ConsoleColor.DarkRed;
                foreach (string s in CurrentDemoFile.GsDemoInfo.ParsingErrors) Console.WriteLine(s.Trim());
            }

            Console.ForegroundColor = ConsoleColor.Green;
            if (IsRussia)
                Console.WriteLine("Начинается анализ демо файла. Пожалуйста подождите...");
            else
                Console.WriteLine("Start demo analyze.....");

            halfLifeDemoParser = new HalfLifeDemoParser(CurrentDemoFile);

            if (usagesrccode != 1) return;

            Console.ForegroundColor = ConsoleColor.Green;
            if (IsRussia)
                Console.Write("Протокол демо файла и игры : ");
            else
                Console.Write("Demo / Game protocol : ");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(CurrentDemoFile.GsDemoInfo.Header.DemoProtocol);

            Console.Write(" / ");

            Console.WriteLine(CurrentDemoFile.GsDemoInfo.Header.NetProtocol);

            Console.ForegroundColor = ConsoleColor.Green;
            if (IsRussia)
                Console.Write("Игра : ");
            else
                Console.Write("Game : ");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(CurrentDemoFile.GsDemoInfo.Header.GameDir);

            Console.ForegroundColor = ConsoleColor.Green;

            if (IsRussia)
                Console.Write("  Карта : ");
            else
                Console.Write("  Map : ");

            MapAndCrc32_Top = Console.CursorTop;
            MapAndCrc32_Left = Console.CursorLeft;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("\"maps/" + CurrentDemoFile.GsDemoInfo.Header.MapName + ".bsp\" ");
            MapName = "maps/" + CurrentDemoFile.GsDemoInfo.Header.MapName + ".bsp";
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;

            if (IsRussia)
                Console.Write("Никнейм и стим : ");
            else
                Console.Write("Username and steamid : ");

            UserNameAndSteamIDField = Console.CursorTop;
            UserNameAndSteamIDField2 = Console.CursorLeft;
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Cyan;

            //if (CurrentDemoFile.GsDemoInfo.DirectoryEntries.Count > 1)
            //{
            //    if (CurrentDemoFile.GsDemoInfo.DirectoryEntries[1].TrackTime > CurrentDemoFile.GsDemoInfo.DirectoryEntries[0].TrackTime)
            //    {
            //        CurrentDemoFile.GsDemoInfo.DirectoryEntries.Reverse();
            //    }
            //}

            for (int index = 0;
               index < CurrentDemoFile.GsDemoInfo.DirectoryEntries.Count;
               index++)
                for (int frameindex = 0; frameindex < CurrentDemoFile.GsDemoInfo.DirectoryEntries[index]
                         .Frames.Count; frameindex++)
                {
                    GoldSource.FramesHren frame = CurrentDemoFile.GsDemoInfo.DirectoryEntries[index]
                        .Frames[frameindex];
                    try
                    {
                        switch (frame.Key.Type)
                        {
                            case GoldSource.DemoFrameType.NetMsg:
                                if (Math.Abs(((GoldSource.NetMsgFrame)frame.Value).RParms.Time) > float.Epsilon)
                                {
                                    if (Math.Abs(((GoldSource.NetMsgFrame)frame.Value).RParms.Time) > float.Epsilon && Math.Abs(GameStartTime) < float.Epsilon) GameStartTime = ((GoldSource.NetMsgFrame)frame.Value).RParms.Time;

                                    if (((GoldSource.NetMsgFrame)frame.Value).RParms.Time > GameEndTime2 + float.Epsilon &&
                                        Math.Abs(((GoldSource.NetMsgFrame)frame.Value).RParms.Time - GameStartTime) > float.Epsilon)
                                        GameEndTime2 = ((GoldSource.NetMsgFrame)frame.Value).RParms.Time;
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
                                if (Math.Abs(((GoldSource.NetMsgFrame)frame.Value).RParms.Time) > float.Epsilon && Math.Abs(GameStartTime) < float.Epsilon) GameStartTime = ((GoldSource.NetMsgFrame)frame.Value).RParms.Time;

                                if (((GoldSource.NetMsgFrame)frame.Value).RParms.Time > GameEndTime2 + float.Epsilon && Math.Abs(((GoldSource.NetMsgFrame)frame.Value).RParms.Time
                                        - GameStartTime) > float.Epsilon)
                                    GameEndTime2 = ((GoldSource.NetMsgFrame)frame.Value).RParms.Time;

                                break;
                        }
                    }
                    catch
                    {
                        Console.WriteLine("Error parsing game start time.");
                    }

                }

            for (int index = 0; index < CurrentDemoFile.GsDemoInfo.DirectoryEntries.Count; index++)
            {
                NewDirectory = true;
                FrameErrors = LastOutgoingSequence = LastIncomingAcknowledged = LastIncomingSequence = 0;
                TreeNode entrynode =
                    new TreeNode("Directory entry [" + (index + 1) + "] - " +
                                 CurrentDemoFile.GsDemoInfo.DirectoryEntries[index]
                                     .FrameCount);
                TimeShift4Times = new float[3] { 0.0f, 0.0f, 0.0f };
                for (int frameindex = 0; frameindex < CurrentDemoFile.GsDemoInfo.DirectoryEntries[index]
                    .Frames.Count; frameindex++)
                {
                    UpdateWarnList();
                    if (NeedRescanDemoForce)
                    {
                        NeedRescanDemoForce = false;
                        Console.WriteLine();
                        if (IsRussia)
                            Console.WriteLine("Извините возникла критическая ошибка при сканировании демо. Повтор..." + CurrentTimeString);
                        else
                            Console.WriteLine("Sorry but need rescan demo ^_^! This action is automatically!" + CurrentTimeString);

                        Console.WriteLine();
                        frameindex = 0;
                    }
                    GoldSource.FramesHren frame = CurrentDemoFile.GsDemoInfo.DirectoryEntries[index]
                    .Frames[frameindex];
                    PreviousTime2 = CurrentTime2;
                    CurrentTime2 = frame.Key.Time + 0.1f;
                    CurrentFrameIdWeapon++;
                    CurrentFrameIdAll++;

                    string row = index + "/" + frame.Key.FrameIndex + " " + "[" +
                              frame.Key.Time + "s]: " +
                              frame.Key.Type.ToString().ToUpper();
                    TreeNode node = new TreeNode();
                    TreeNode subnode = new TreeNode();


                    switch (frame.Key.Type)
                    {
                        case GoldSource.DemoFrameType.DemoStart:
                            LASTFRAMEISCLIENTDATA = false;
                            break;
                        case GoldSource.DemoFrameType.ConsoleCommand:
                            {
                                LASTFRAMEISCLIENTDATA = false;
                                if (DUMP_ALL_FRAMES)
                                {
                                    subnode.Text += "{\n";
                                    subnode.Text +=
                                        ((GoldSource.ConsoleCommandFrame)frame.Value).Command;
                                }

                                CheckConsoleCommand(
                                    ((GoldSource.ConsoleCommandFrame)frame.Value).Command);
                                if (DUMP_ALL_FRAMES) subnode.Text += "}\n";

                                break;
                            }
                        case GoldSource.DemoFrameType.ClientData:
                            {
                                if (CurrentFrameDuplicated > 0) CurrentFrameDuplicated -= 1;

                                ClientDataCountDemos++;
                                if (asdf.asdf2 != 1) break;
                                LastClientDataTime = CurrentTime;

                                NeedSearchID = CurrentFrameIdAll;

                                CurrentFrameId++;

                                if (DUMP_ALL_FRAMES) subnode.Text += "{\n";

                                GoldSource.ClientDataFrame cdframe = (GoldSource.ClientDataFrame)frame.Value;

                                FPoint3D tmpPrevViewAngles = CDFRAME_ViewAngles;
                                CDFRAME_ViewAngles = cdframe.Viewangles;


                                oldoriginpos.X = curoriginpos.X;
                                oldoriginpos.Y = curoriginpos.Y;
                                curoriginpos.X = cdframe.Origin.X;
                                curoriginpos.Y = cdframe.Origin.Y;

                                addAngleInViewListY(CDFRAME_ViewAngles.Y);

                                if (Math.Abs(NewViewAngleSearcherAngle) > float.Epsilon) AddViewAngleSearcher(NewViewAngleSearcherAngle);

                                NewViewAngleSearcherAngle = 0.0f;

                                /* if (IsUserAlive() &&
                                     CurrentFrameOnGround && (!IsAngleEditByEngineForLearn() || ENABLE_LEARN_CLEAN_DEMO)
                                     )
                                 {
                                     if (LearnAngles.Count > 0 && NewAttackForLearn) LearnAngles = new List<float>();

                                     if (NewAttackForLearn)
                                     {
                                         NewAttackForLearn = false;
                                         LearnAngles = new List<float>
                                         {
                                             PREV_CDFRAME_ViewAngles.Y,
                                             CDFRAME_ViewAngles.Y
                                         };
                                     }
                                     else if (LearnAngles.Count > 0)
                                     {
                                         LearnAngles.Add(CDFRAME_ViewAngles.Y);
                                     }
                                     if (LearnAngles.Count == LEARN_FLOAT_COUNT)
                                     {
                                         if (ENABLE_LEARN_CLEAN_DEMO || ENABLE_LEARN_HACK_DEMO || ENABLE_LEARN_HACK_DEMO_SAVE_ALL_ANGLES)
                                         {
                                             ENABLE_LEARN_HACK_DEMO = false;
                                             WriteLearnAngles();
                                         }
                                         else if (MachineLearnAnglesCLEAN.is_file_exists
                                             && MachineLearnAnglesHACK.is_file_exists)
                                         {
                                             CheckLearnAngles();
                                         }
                                         LearnAngles = new List<float>();
                                     }
                                 }
                                 else
                                 {
                                     NewAttackForLearn = false;
                                     LearnAngles = new List<float>();
                                 }*/

                                if (DUMP_ALL_FRAMES)
                                {
                                    subnode.Text += @"Origin.X = " + cdframe.Origin.X + "\n";
                                    subnode.Text += @"Origin.Y = " + cdframe.Origin.Y + "\n";
                                    subnode.Text += @"Origin.Z = " + cdframe.Origin.Z + "\n";
                                    subnode.Text +=
                                        @"Viewangles.X = " + cdframe.Viewangles.X + "\n";
                                    subnode.Text +=
                                        @"Viewangles.Y = " + cdframe.Viewangles.Y + "\n";
                                    subnode.Text +=
                                        @"Viewangles.Z = " + cdframe.Viewangles.Z + "\n";
                                    subnode.Text +=
                                        @"WeaponBits = " + cdframe.WeaponBits + "\n";
                                    subnode.Text += @"Fov = " + cdframe.Fov + "\n";
                                    subnode.Text += "}\n";
                                }
                                cdframeFov = cdframe.Fov;

                                CDFrameYAngleHistory[0] = CDFrameYAngleHistory[1];
                                CDFrameYAngleHistory[1] = CDFrameYAngleHistory[2];
                                if (CDFrameYAngleHistory[2] > 360.0f && cdframe.Viewangles.Y < 10.0f)
                                    if (LASTFRAMEISCLIENTDATA) BadAnglesFoundCount++;
                                CDFrameYAngleHistory[2] = cdframe.Viewangles.Y;

                                LASTFRAMEISCLIENTDATA = true;


                                if (GetDistance(curoriginpos,
                                          oldoriginpos) > 100)
                                {
                                    PlayerTeleportus++;
                                    ReloadWarns = 0;
                                    if (DEBUG_ENABLED) Console.WriteLine("Teleportus " + CurrentTime + ":" + CurrentTimeString);

                                    if (Math.Abs(LastGameMaximizeTime) > float.Epsilon && Math.Abs(LastTeleportusTime) > float.Epsilon && Math.Abs(LastGameMaximizeTime - CurrentTime) < float.Epsilon
                                        && Math.Abs(GameEndTime) <= float.Epsilon)
                                    {
                                        ReturnToGameDetects++;
                                        if (!IsRussia)
                                            DemoScanner_AddWarn("['RETURN TO GAME' FEATURE]", ReturnToGameDetects > 2, true);
                                        else
                                            DemoScanner_AddWarn("[Функция возврата в игру]", ReturnToGameDetects > 2, true);
                                    }
                                    LastTeleportusTime = CurrentTime;
                                }
                                else
                                {
                                    for (int n = frameindex + 2;
                                          n < CurrentDemoFile.GsDemoInfo.DirectoryEntries[index].Frames.Count;
                                          n++)
                                    {

                                        GoldSource.FramesHren tmpframe = CurrentDemoFile.GsDemoInfo.DirectoryEntries[index].Frames[n];
                                        if (tmpframe.Key.Type == GoldSource.DemoFrameType.ClientData)
                                        {
                                            GoldSource.ClientDataFrame cdframe2 = (GoldSource.ClientDataFrame)tmpframe.Value;

                                            if (GetDistance(new FPoint(cdframe2.Origin.X, cdframe2.Origin.Y),
                                            curoriginpos) > 250)
                                                LastTeleportusTime = CurrentTime;
                                            break;
                                        }
                                    }
                                }

                                if (Math.Abs(CurrentTime) > float.Epsilon/* && !CurrentFrameDuplicated*/)
                                {
                                    if (Math.Abs(LastFpsCheckTime) < float.Epsilon)
                                    {
                                        LastFpsCheckTime = CurrentTime;
                                    }
                                    else
                                    {
                                        if ((CurrentTimeDesync =
                                            Math.Abs(CurrentTime - LastFpsCheckTime)) > 1.0f)
                                        {
                                            if (DUMP_ALL_FRAMES)
                                                subnode.Text +=
                                                    "CurrentFps:" + CurrentFps + "\n";

                                            if (CurrentFps > RealFpsMax) RealFpsMax = CurrentFps;

                                            if (CurrentFps < RealFpsMin && CurrentFps > 0) RealFpsMin = CurrentFps;

                                            if (Math.Abs(CurrentTime) < float.Epsilon ||
                                                Math.Abs(CurrentTime2) < float.Epsilon)
                                            {
                                                CurrentMsgBytes = CurrentMsgCount = 0;
                                            }
                                            else
                                            {
                                                if (MaxBytesPerSecond < CurrentMsgBytes) MaxBytesPerSecond = CurrentMsgBytes;

                                                if (CurrentMsgBytes > 12500) MsgOverflowSecondsCount++;

                                                if (MaxMsgPerSecond < CurrentMsgCount) MaxMsgPerSecond = CurrentMsgCount;
                                            }
                                            CurrentMsgCount = 0;
                                            CurrentMsgBytes = 0;


                                            SecondFound = true;
                                            LastFpsCheckTime = CurrentTime;


                                            averagefps.Add(CurrentFps);
                                            CurrentFps = 0;
                                            CurrentGameSecond++;
                                        }
                                        else
                                        {
                                            CurrentFps++;
                                        }
                                    }
                                }


                                AngleDirection tmpAngleDirY = GetAngleDirection(PREV_CDFRAME_ViewAngles.Y, CDFRAME_ViewAngles.Y);


                                if (tmpAngleDirY == AngleDirection.AngleDirectionLeft)
                                {
                                    if (AngleStrikeDirection > 0)
                                        AngleStrikeDirection = -1;
                                    else
                                        AngleStrikeDirection--;

                                    if (LastAngleDirection != tmpAngleDirY)
                                    {
                                        AngleDirectionChanges++;
                                        StrafeAngleDirectionChanges++;
                                        AngleDirectionChangeTime = CurrentTime;
                                        if (DetectStrafeOptimizerStrikes > 1 && NeedCheckAngleDiffForStrafeOptimizer && Math.Abs(CurrentTime - LastStrafeOptimizerWarnTime) > 0.02f) StrafeOptimizerFalse = true;
                                        NeedCheckAngleDiffForStrafeOptimizer = false;
                                    }
                                    LastAngleDirection = tmpAngleDirY;
                                }
                                else if (tmpAngleDirY == AngleDirection.AngleDirectionRight)
                                {
                                    if (AngleStrikeDirection < 0)
                                        AngleStrikeDirection = 1;
                                    else
                                        AngleStrikeDirection++;

                                    if (LastAngleDirection != tmpAngleDirY)
                                    {
                                        AngleDirectionChanges++;
                                        StrafeAngleDirectionChanges++;
                                        AngleDirectionChangeTime = CurrentTime;
                                        if (DetectStrafeOptimizerStrikes > 1 && NeedCheckAngleDiffForStrafeOptimizer && Math.Abs(CurrentTime - LastStrafeOptimizerWarnTime) > 0.02f) StrafeOptimizerFalse = true;
                                        NeedCheckAngleDiffForStrafeOptimizer = false;
                                    }
                                    LastAngleDirection = tmpAngleDirY;
                                }


                                if (RealAlive)
                                {
                                    double tmpXangle = AngleBetweenAbsolute(PREV_CDFRAME_ViewAngles.X, CDFRAME_ViewAngles.X);
                                    double tmpYangle = AngleBetweenAbsolute(PREV_CDFRAME_ViewAngles.Y, CDFRAME_ViewAngles.Y);

                                    if (AngleLenMinX > tmpXangle * (1.0f / CurrentFrameTime))
                                        AngleLenMinX = (float)tmpXangle * (1.0f / CurrentFrameTime);
                                    if (AngleLenMinY > tmpYangle * (1.0f / CurrentFrameTime))
                                        AngleLenMinY = (float)tmpYangle * (1.0f / CurrentFrameTime);

                                    double flAngleRealDetect = MIN_SENS_WARN_DETECTED;
                                    double flAngleWarnDetect = MIN_SENS_WARN_WARNING;

                                    if (PlayerSensitivityHistory.Count > 15)
                                    {
                                        flAngleRealDetect = 100.0f;
                                        flAngleWarnDetect = 100.0f;

                                        int maxcheckangels = 15;
                                        for (int i = PlayerSensitivityHistory.Count - maxcheckangels; i < PlayerSensitivityHistory.Count; i++)
                                        {
                                            float cursens = PlayerSensitivityHistory[i] * 0.022f;

                                            if (cursens > MIN_SENS_WARN_DETECTED && flAngleRealDetect > cursens / 2.1f) flAngleRealDetect = (PlayerSensitivityHistory[i] * 0.022f) / 2.1f;

                                            if (cursens > MIN_SENS_WARN_WARNING && flAngleWarnDetect > cursens / 1.1f) flAngleWarnDetect = (PlayerSensitivityHistory[i] * 0.022f) / 1.1f;
                                        }
                                    }


                                    if (CurrentWeapon == WeaponIdType.WEAPON_AWP ||
                                        CurrentWeapon == WeaponIdType.WEAPON_SCOUT ||
                                        CurrentWeapon == WeaponIdType.WEAPON_G3SG1 ||
                                        CurrentWeapon == WeaponIdType.WEAPON_SG550)
                                    {
                                        flAngleRealDetect /= 60.0f;
                                        flAngleWarnDetect /= 20.0f;
                                    }
                                    else if (
                                        CurrentWeapon == WeaponIdType.WEAPON_SG552 ||
                                        CurrentWeapon == WeaponIdType.WEAPON_AUG)
                                    {
                                        flAngleRealDetect /= 30.0f;
                                        flAngleWarnDetect /= 15.0f;
                                    }

                                    if (NewAttack)
                                    {
                                        if (PlayerSensitivityWarning == 0 && Math.Abs(LastAim5DetectedReal) > float.Epsilon &&
                                           Math.Abs(CurrentTime - LastAim5DetectedReal) < 0.5f)
                                        {
                                            /*if (AUTO_LEARN_HACK_DB)
                                            {
                                                ENABLE_LEARN_HACK_DEMO = true;
                                                ENABLE_LEARN_HACK_DEMO_FORCE_SAVE = true;
                                            }*/
                                            DemoScanner_AddWarn(
                                                "[AIM TYPE 5.1 " + CurrentWeapon + "] at (" + LastAim5DetectedReal +
                                                "):" + CurrentTimeString, !IsTakeDamage() && !IsPlayerLossConnection() && !IsAngleEditByEngine() && !IsChangeWeapon());
                                            if (!IsTakeDamage() && !IsPlayerLossConnection() && !IsAngleEditByEngine() && !IsChangeWeapon()) TotalAimBotDetected++;

                                            LastAim5DetectedReal = 0.0f;
                                            LastAim5Detected = 0.0f;
                                        }
                                        else if (PlayerSensitivityWarning == 0 && Math.Abs(LastAim5Detected) > float.Epsilon &&
                                           Math.Abs(CurrentTime - LastAim5Detected) < 0.5f)
                                        {
                                            if (!IsAngleEditByEngine() && !IsTakeDamage() && !IsChangeWeapon())
                                            {
                                                /* if (AUTO_LEARN_HACK_DB)
                                                 {
                                                     ENABLE_LEARN_HACK_DEMO = true;
                                                     ENABLE_LEARN_HACK_DEMO_FORCE_SAVE = true;
                                                 }*/
                                                DemoScanner_AddWarn(
                                                    "[AIM TYPE 5.2 " + CurrentWeapon + "] at (" + LastAim5Detected +
                                                    "):" + CurrentTimeString, !IsPlayerLossConnection());
                                                TotalAimBotDetected++;
                                            }
                                            else
                                            {
                                                /*if (AUTO_LEARN_HACK_DB)
                                                {
                                                    ENABLE_LEARN_HACK_DEMO = true;
                                                    ENABLE_LEARN_HACK_DEMO_FORCE_SAVE = true;
                                                }*/
                                                DemoScanner_AddWarn(
                                                    "[AIM TYPE 5.3 " + CurrentWeapon + "] at (" + LastAim5Detected +
                                                    "):" + CurrentTimeString, false);
                                            }
                                            LastAim5DetectedReal = 0.0f;
                                            LastAim5Detected = 0.0f;
                                        }
                                        else if (PlayerSensitivityWarning == 1)
                                        {
                                            if (Math.Abs(LastAim5DetectedReal) > float.Epsilon &&
                                                Math.Abs(CurrentTime - LastAim5DetectedReal) < 0.75f)
                                            {
                                                if (!IsAngleEditByEngine())
                                                {
                                                    /*if (AUTO_LEARN_HACK_DB)
                                                    {
                                                        ENABLE_LEARN_HACK_DEMO = true;
                                                        ENABLE_LEARN_HACK_DEMO_FORCE_SAVE = true;
                                                    }*/
                                                    DemoScanner_AddWarn(
                                                        "[AIM TYPE 5.4 " + CurrentWeapon + "] at (" + LastAim5DetectedReal +
                                                        "):" + CurrentTimeString, false);
                                                    LastAim5DetectedReal = 0.0f;
                                                }
                                                else
                                                {
                                                    DemoScanner_AddWarn(
                                                        "[AIM TYPE 5.6 " + CurrentWeapon + "] at (" + LastAim5DetectedReal +
                                                        "):" + CurrentTimeString, false);
                                                    LastAim5DetectedReal = 0.0f;
                                                }
                                            }

                                            if (Math.Abs(LastAim5Detected) > float.Epsilon &&
                                          Math.Abs(CurrentTime - LastAim5Detected) < 0.75f)
                                            {
                                                if (!IsAngleEditByEngine())
                                                {
                                                    /*if (AUTO_LEARN_HACK_DB)
                                                    {
                                                        ENABLE_LEARN_HACK_DEMO = true;
                                                        ENABLE_LEARN_HACK_DEMO_FORCE_SAVE = true;
                                                    }*/
                                                    DemoScanner_AddWarn(
                                                        "[AIM TYPE 5.4 " + CurrentWeapon + "] at (" + LastAim5Detected +
                                                        "):" + CurrentTimeString, false);
                                                    LastAim5Detected = 0.0f;
                                                }
                                                else
                                                {
                                                    DemoScanner_AddWarn(
                                                        "[AIM TYPE 5.7 " + CurrentWeapon + "] at (" + LastAim5DetectedReal +
                                                        "):" + CurrentTimeString, false);
                                                    LastAim5Detected = 0.0f;
                                                }
                                            }
                                            PlayerSensitivityWarning = 0;
                                        }
                                        else
                                        {
                                            if (Math.Abs(LastAim5DetectedReal) > float.Epsilon &&
                                                Math.Abs(CurrentTime - LastAim5DetectedReal) > 0.75f)
                                            {
                                                if (!IsAngleEditByEngine())
                                                {
                                                    /* if (AUTO_LEARN_HACK_DB)
                                                     {
                                                         ENABLE_LEARN_HACK_DEMO = true;
                                                         ENABLE_LEARN_HACK_DEMO_FORCE_SAVE = true;
                                                     }*/
                                                    DemoScanner_AddWarn(
                                                        "[AIM TYPE 5.5 " + CurrentWeapon + "] at (" + LastAim5DetectedReal +
                                                        "):" + CurrentTimeString, false);
                                                    LastAim5DetectedReal = 0.0f;
                                                    PlayerSensitivityWarning = 0;
                                                }
                                                else
                                                {
                                                    DemoScanner_AddWarn(
                                                        "[AIM TYPE 5.8 " + CurrentWeapon + "] at (" + LastAim5DetectedReal +
                                                        "):" + CurrentTimeString, false);
                                                    LastAim5DetectedReal = 0.0f;
                                                    PlayerSensitivityWarning = 0;
                                                }
                                            }

                                            if (Math.Abs(LastAim5Detected) > float.Epsilon &&
                                            Math.Abs(CurrentTime - LastAim5Detected) > 0.75f)
                                            {
                                                if (!IsAngleEditByEngine())
                                                {
                                                    /*if (AUTO_LEARN_HACK_DB)
                                                    {
                                                        ENABLE_LEARN_HACK_DEMO = true;
                                                        ENABLE_LEARN_HACK_DEMO_FORCE_SAVE = true;
                                                    }*/
                                                    DemoScanner_AddWarn(
                                                        "[AIM TYPE 5.5 " + CurrentWeapon + "] at (" + LastAim5Detected +
                                                        "):" + CurrentTimeString, false);
                                                    LastAim5Detected = 0.0f;
                                                    PlayerSensitivityWarning = 0;
                                                }
                                            }
                                        }
                                    }

                                    if (Math.Abs(tmpXangle) > float.Epsilon &&
                                    tmpXangle < flAngleRealDetect)
                                    {
                                        // Console.WriteLine("1tmpXangle:" + Math.Round((double)tmpXangle, 8).ToString("F8") + ". flAngleRealDetect:" + Math.Round((double)flAngleRealDetect, 8).ToString("F8"));
                                        if (CurrentWeapon == WeaponIdType.WEAPON_C4
                                            || CurrentWeapon == WeaponIdType.WEAPON_HEGRENADE
                                            || CurrentWeapon == WeaponIdType.WEAPON_SMOKEGRENADE
                                            || CurrentWeapon == WeaponIdType.WEAPON_FLASHBANG
                                            || CurrentWeapon == WeaponIdType.WEAPON_NONE
                                            || CurrentWeapon == WeaponIdType.WEAPON_BAD
                                            || CurrentWeapon == WeaponIdType.WEAPON_BAD2)
                                        {

                                        }
                                        else
                                        {
                                            if (CurrentFrameAttacked
                                                    || PreviousFrameAttacked || BadPunchAngle || IsTakeDamage() || IsPlayerLossConnection() || IsAngleEditByEngine() || IsChangeWeapon())
                                            {
                                                LastAim5Detected = CurrentTime;
                                                PlayerSensitivityWarning = 1;
                                            }
                                            else
                                            {
                                                LastAim5DetectedReal = CurrentTime;
                                            }
                                        }
                                    }
                                    else if (Math.Abs(tmpXangle) > float.Epsilon &&
                                    tmpXangle < flAngleWarnDetect)
                                    {
                                        // Console.WriteLine("2tmpXangle:" + Math.Round((double)tmpXangle, 8).ToString("F8") + ". flAngleRealDetect:" + Math.Round((double)flAngleWarnDetect, 8).ToString("F8"));

                                        if (CurrentWeapon == WeaponIdType.WEAPON_C4
                                            || CurrentWeapon == WeaponIdType.WEAPON_HEGRENADE
                                            || CurrentWeapon == WeaponIdType.WEAPON_SMOKEGRENADE
                                            || CurrentWeapon == WeaponIdType.WEAPON_FLASHBANG
                                            || CurrentWeapon == WeaponIdType.WEAPON_NONE
                                            || CurrentWeapon == WeaponIdType.WEAPON_BAD
                                            || CurrentWeapon == WeaponIdType.WEAPON_BAD2)
                                        {

                                        }
                                        else
                                        {
                                            PlayerSensitivityWarning = 1;
                                            LastAim5Detected = CurrentTime;
                                        }
                                    }

                                    if (Math.Abs(tmpXangle) > float.Epsilon && tmpXangle / 0.022f < CurrentSensitivity)
                                        CurrentSensitivity = tmpXangle / 0.022f;

                                    if (CurrentWeapon == WeaponIdType.WEAPON_NONE
                                        || CurrentWeapon == WeaponIdType.WEAPON_BAD
                                        || CurrentWeapon == WeaponIdType.WEAPON_BAD2)
                                    {
                                        //fixyou
                                    }
                                    else
                                    {
                                        LastSensWeapon = CurrentWeapon.ToString();
                                    }

                                    if (SecondFound)
                                    {
                                        if (CurrentSensitivity < 4999.0 &&
                                            Math.Abs(CurrentSensitivity) > float.Epsilon)
                                        {
                                            PlayerAngleLenHistory.Add(GetDistance(new FPoint(0.0f, 0.0f), new FPoint(
                                                   AngleLenMinX, AngleLenMinY)));
                                            PlayerSensitivityHistory.Add(
                                                (float)CurrentSensitivity);
                                            PlayerSensitivityHistoryStr.Add(Math
                                                .Round(CurrentSensitivity, 8,
                                                    MidpointRounding.AwayFromZero)
                                                .ToString("F5"));
                                            PlayerSensitivityHistoryStrTime.Add(
                                                "(" + CurrentTime + "): " + CurrentTimeString);
                                            PlayerSensitivityHistoryStrWeapon.Add(
                                                LastSensWeapon);
                                        }
                                        CurrentSensitivity = 5000.0;
                                    }

                                    if (AimType7Event == 2 || AimType7Event == 3
                                        || AimType7Event == 4 || AimType7Event == 5)
                                    {
                                        if (Math.Abs(tmpXangle) < float.Epsilon && Math.Abs(tmpYangle) < float.Epsilon && AimType7Frames < 20)
                                        {
                                            //Console.WriteLine("5");
                                            //if (!CurrentFrameDuplicated)
                                            AimType7Frames++;

                                            if (IsAngleEditByEngine() || !CurrentFrameOnGround || IsPlayerLossConnection())
                                            {
                                                AimType7Frames = -2;
                                                AimType7Event = 0;
                                            }
                                        }
                                        else
                                        {
                                            // Console.WriteLine("11");
                                            if (AimType7Frames > 2)
                                            {
                                                if (AimType7Event == 4) Aim73FalseSkip--;
                                                if (AngleBetweenAbsolute(Aim8CurrentFrameViewanglesY,
                                                        CDFRAME_ViewAngles.Y) > float.Epsilon &&
                                                    AngleBetweenAbsolute(Aim8CurrentFrameViewanglesX,
                                                        CDFRAME_ViewAngles.X) > float.Epsilon && !IsAngleEditByEngine() && !IsPlayerLossConnection() && CurrentFrameOnGround)
                                                {
                                                    if (AimType7Event == 4 && Aim73FalseSkip < 0)
                                                    {
                                                        float tmpangle2 =
                                                            AngleBetweenAbsolute(
                                                                    Aim8CurrentFrameViewanglesX, CDFRAME_ViewAngles.X);
                                                        tmpangle2 +=
                                                            AngleBetweenAbsolute(
                                                                    Aim8CurrentFrameViewanglesY, CDFRAME_ViewAngles.Y);

                                                        int Aim7var1 = OldAimType7Frames;
                                                        int Aim7var2 = AimType7Frames;
                                                        int Aim7var3 = 0;
                                                        bool Aim7detected = true;
                                                        string Aim7str = GetAim7String(ref Aim7var1,
                                                                ref Aim7var2, ref Aim7var3, AimType7Event,
                                                                tmpangle2, ref Aim7detected);
                                                        if (Aim7detected)
                                                        {
                                                            if (Aim7var3 > 50) TotalAimBotDetected++;

                                                            DemoScanner_AddWarn(Aim7str
                                                                                + " at (" + OldAimType7Time +
                                                                                "):" + CurrentTimeString, Aim7detected && Aim7var3 > 50 && Aim7var1 >= 20 && Aim7var2 >= 20 && !IsChangeWeapon());
                                                        }
                                                    }
                                                    else if (AimType7Event != 4)
                                                    {
                                                        float tmpangle2 =
                                                            AngleBetweenAbsolute(
                                                                    Aim8CurrentFrameViewanglesX, CDFRAME_ViewAngles.X);
                                                        tmpangle2 +=
                                                            AngleBetweenAbsolute(
                                                                    Aim8CurrentFrameViewanglesY, CDFRAME_ViewAngles.Y);

                                                        int Aim7var1 = OldAimType7Frames;
                                                        int Aim7var2 = AimType7Frames;
                                                        int Aim7var3 = 0;
                                                        bool Aim7detected = true;
                                                        string Aim7str = GetAim7String(ref Aim7var1,
                                                                ref Aim7var2, ref Aim7var3, AimType7Event,
                                                                tmpangle2, ref Aim7detected);
                                                        if (Aim7detected)
                                                            DemoScanner_AddWarn(Aim7str
                                                                + " at (" + OldAimType7Time +
                                                                "):" + CurrentTimeString, Aim7detected && Aim7var3 > 50 && Aim7var1 >= 20 && Aim7var2 >= 20 && !IsChangeWeapon());
                                                    }
                                                }
                                            }

                                            if (AngleBetweenAbsolute(Aim8CurrentFrameViewanglesY,
                                                    CDFRAME_ViewAngles.Y) < float.Epsilon && !IsAngleEditByEngine() && CurrentFrameOnGround/* &&
                                                    Aim8CurrentFrameViewanglesX !=
                                                    CurrentFrameViewanglesX*/)
                                            {
                                                AimType7Event += 10;
                                                float tmpangle2 =
                                                    AngleBetweenAbsolute(
                                                            Aim8CurrentFrameViewanglesX, CDFRAME_ViewAngles.X);
                                                tmpangle2 +=
                                                    AngleBetweenAbsolute(
                                                            Aim8CurrentFrameViewanglesY, CDFRAME_ViewAngles.Y);


                                                int Aim7var1 = OldAimType7Frames;
                                                int Aim7var2 = AimType7Frames;
                                                int Aim7var3 = 0;
                                                bool Aim7detected = true;
                                                string Aim7str = GetAim7String(ref Aim7var1,
                                                        ref Aim7var2, ref Aim7var3, AimType7Event,
                                                        tmpangle2, ref Aim7detected);
                                                if (Aim7detected)
                                                    DemoScanner_AddWarn(Aim7str
                                                                        + " at (" + OldAimType7Time +
                                                                        "):" + CurrentTimeString, Aim7detected && !IsPlayerLossConnection() && Aim7var3 > 50 && Aim7var1 >= 20 && Aim7var2 >= 20 && !IsChangeWeapon());
                                            }

                                            AimType7Frames = 0;
                                            AimType7Event = 0;
                                        }
                                    }

                                    if (AimType7Event == 1)
                                    {
                                        //  Console.WriteLine("1");
                                        //if (NewAttack)
                                        //{
                                        if (Math.Abs(tmpXangle) > float.Epsilon && Math.Abs(tmpYangle) > float.Epsilon)
                                            AimType7Event = 52;
                                        else
                                            AimType7Event = 53;
                                        //}
                                        //else
                                        //    AimType7Event = 0;
                                    }
                                    else if (AimType7Event == 11)
                                    {
                                        //  Console.WriteLine("2");
                                        if (Math.Abs(tmpXangle) > float.Epsilon && Math.Abs(tmpYangle) > float.Epsilon)
                                            AimType7Event = 4;
                                        else
                                            AimType7Event = 0;
                                        /*else
   AimType7Event = 5;*/
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
                                        if (Math.Abs(tmpXangle) < float.Epsilon && Math.Abs(tmpYangle) < float.Epsilon/* &&
                                            !CurrentFrameAttacked*/)
                                        {
                                            // Console.WriteLine("3");
                                            Aim8CurrentFrameViewanglesY =
                                                CDFRAME_ViewAngles.Y;
                                            Aim8CurrentFrameViewanglesX =
                                                CDFRAME_ViewAngles.X;
                                            // if (!CurrentFrameDuplicated)
                                            AimType7Frames++;
                                        }
                                        // Иначе Если угол изменился и набралось больше 1 таких кадров то вкл поиск аим
                                        else
                                        {
                                            if (AimType7Frames > 1
                                                && Math.Abs(tmpXangle) > float.Epsilon && Math.Abs(tmpYangle) > float.Epsilon)
                                            {
                                                OldAimType7Time = CurrentTime;
                                                //Console.WriteLine("4");
                                                OldAimType7Frames = AimType7Frames;
                                                AimType7Event = 1;
                                                if (CurrentFrameAttacked) AimType7Event = 11;
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
                                    SecondFrameTime = 0;

                                    AngleLenMinX = 0.0f;
                                    AngleLenMinY = 0.0f;
                                }

                                //Console.Write(RealAlive);
                                //Console.Write(" ");
                                //Console.Write(CurrentSensitivity);
                                //Console.Write(" ");
                                //Console.Write(tmpXangle);
                                //Console.Write(" ");
                                //Console.Write(SecondFound);
                                //Console.Write("\n");

                                PREV_CDFRAME_ViewAngles = tmpPrevViewAngles;

                                break;
                            }
                        case GoldSource.DemoFrameType.NextSection:
                            {
                                NewDirectory = true;
                                FrameErrors = LastOutgoingSequence = LastIncomingAcknowledged = LastIncomingSequence = 0;
                                LASTFRAMEISCLIENTDATA = false;
                                if (DUMP_ALL_FRAMES) subnode.Text += @"End of the DirectoryEntry!";
                                FirstUserAlive = false;
                                UserAlive = false;
                                RealAlive = false;
                                FirstBypassKill = true;
                                BypassCount = 0;
                                FirstDuck = false;
                                FirstJump = false;
                                FirstAttack = false;
                                SearchJumpBug = false;
                                break;
                            }
                        case GoldSource.DemoFrameType.Event:
                            {
                                CurrentEvents++;
                                LASTFRAMEISCLIENTDATA = false;
                                if (DUMP_ALL_FRAMES) subnode.Text += "{\n";

                                GoldSource.EventFrame eframe = (GoldSource.EventFrame)frame.Value;
                                if (DUMP_ALL_FRAMES)
                                {
                                    subnode.Text += @"Flags = " + eframe.Flags + "\n";
                                    subnode.Text += @"Index = " + eframe.Index + "\n";
                                    subnode.Text += @"Delay = " + eframe.Delay + "\n";
                                    subnode.Text +=
                                        @"EventArgumentsFlags = " +
                                        eframe.EventArguments.Flags + "\n";
                                    subnode.Text +=
                                        @"EntityIndex = " + eframe.EventArguments.EntityIndex +
                                        "\n";
                                    subnode.Text +=
                                        @"Origin.X = " + eframe.EventArguments.Origin.X + "\n";
                                    subnode.Text +=
                                        @"Origin.Y = " + eframe.EventArguments.Origin.Y + "\n";
                                    subnode.Text +=
                                        @"Origin.Z = " + eframe.EventArguments.Origin.Z + "\n";
                                    subnode.Text +=
                                        @"Angles.X = " + eframe.EventArguments.Angles.X + "\n";
                                    subnode.Text +=
                                        @"Angles.Y = " + eframe.EventArguments.Angles.Y + "\n";
                                    subnode.Text +=
                                        @"Angles.Z = " + eframe.EventArguments.Angles.Z + "\n";
                                    subnode.Text +=
                                        @"Velocity.X = " + eframe.EventArguments.Velocity.X +
                                        "\n";
                                    subnode.Text +=
                                        @"Velocity.Y = " + eframe.EventArguments.Velocity.Y +
                                        "\n";
                                    subnode.Text +=
                                        @"Velocity.Z = " + eframe.EventArguments.Velocity.Z +
                                        "\n";
                                    subnode.Text +=
                                        @"Ducking = " + eframe.EventArguments.Ducking + "\n";
                                    subnode.Text +=
                                        @"Fparam1 = " + eframe.EventArguments.Fparam1 + "\n";
                                    subnode.Text +=
                                        @"Fparam2 = " + eframe.EventArguments.Fparam2 + "\n";
                                    subnode.Text +=
                                        @"Iparam1 = " + eframe.EventArguments.Iparam1 + "\n";
                                    subnode.Text +=
                                        @"Iparam2 = " + eframe.EventArguments.Iparam2 + "\n";
                                    subnode.Text +=
                                        @"Bparam1 = " + eframe.EventArguments.Bparam1 + "\n";
                                    subnode.Text +=
                                        @"Bparam2 = " + eframe.EventArguments.Bparam2 + "\n";
                                    subnode.Text += "}\n";
                                }

                                if (Math.Abs(CurrentTime - LastClientDataTime) < float.Epsilon)
                                    if (AngleBetweenAbsolute(CDFRAME_ViewAngles.Y, eframe.EventArguments.Angles.Y) > float.Epsilon)
                                    {
                                        // Console.WriteLine("AIM IM IM:" + viewanglesforsearch.Y + ":" + eframe.EventArguments.Angles.Y);
                                    }

                                break;
                            }
                        case GoldSource.DemoFrameType.WeaponAnim:
                            {
                                LASTFRAMEISCLIENTDATA = false;
                                if (DUMP_ALL_FRAMES) subnode.Text += "{\n";

                                GoldSource.WeaponAnimFrame waframe = (GoldSource.WeaponAnimFrame)frame.Value;
                                if (DUMP_ALL_FRAMES)
                                {
                                    subnode.Text += @"Anim = " + waframe.Anim + "\n";
                                    subnode.Text += @"Body = " + waframe.Body + "\n";
                                    subnode.Text += "}\n";
                                }
                                if (RealAlive && !IsRoundEnd() && IsRealWeapon() && CurrentWeapon != WeaponIdType.WEAPON_KNIFE
                                     && CurrentWeapon != WeaponIdType.WEAPON_XM1014 && CurrentWeapon != WeaponIdType.WEAPON_M3
                                      && CurrentWeapon != WeaponIdType.WEAPON_HEGRENADE && CurrentWeapon != WeaponIdType.WEAPON_FLASHBANG
                                       && CurrentWeapon != WeaponIdType.WEAPON_SMOKEGRENADE && CurrentWeapon != WeaponIdType.WEAPON_C4)
                                {
                                    if (!IsAngleEditByEngine())
                                    {
                                        if (LastWeaponAnim != WeaponIdType.WEAPON_NONE && LastWeaponAnim != CurrentWeapon) WeaponAnimWarn = 0;
                                        if (waframe.Anim == 0 && waframe.Body == 0)
                                        {
                                            WeaponAnimWarn++;
                                            LastWeaponAnim = CurrentWeapon;
                                            if (WeaponAnimWarn > 50)
                                            {
                                                DemoScanner_AddWarn("[NO WEAPON ANIM " + CurrentWeapon + "] at (" + CurrentTime + ") " + CurrentTimeString, !IsPlayerLossConnection());
                                                WeaponAnimWarn = 0;
                                            }
                                        }
                                        else
                                        {
                                            if (WeaponAnimWarn > 0) WeaponAnimWarn--;
                                        }
                                    }
                                    else
                                    {
                                        if (WeaponAnimWarn > 0) WeaponAnimWarn--;
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
                                if (DUMP_ALL_FRAMES) subnode.Text += "{\n";

                                GoldSource.SoundFrame sframe = (GoldSource.SoundFrame)frame.Value;
                                if (DUMP_ALL_FRAMES)
                                {
                                    subnode.Text += @"Channel = " + sframe.Channel + "\n";
                                    subnode.Text += @"Sample = " + sframe.Sample + "\n";
                                    subnode.Text += @"Attenuation = " + sframe.Attenuation + "\n";
                                    subnode.Text += @"Volume = " + sframe.Volume + "\n";
                                    subnode.Text += @"Flags = " + sframe.Flags + "\n";
                                    subnode.Text += @"Pitch = " + sframe.Pitch + "\n";

                                    subnode.Text += "}\n";
                                }
                                //Console.WriteLine("Sound:" + CurrentTimeString);
                                break;
                            }
                        case GoldSource.DemoFrameType.DemoBuffer:
                            {
                                // LASTFRAMEISCLIENTDATA = false;
                                /*var bframe = (GoldSource.DemoBufferFrame)frame.Value;
                                if (bframe.Buffer.Length > 0)
                                {
                                    Console.WriteLine("Demobuf found");
                                    File.WriteAllBytes("demobuffer/demobuf_" + CurrentFrameId + ".bin", bframe.Buffer);
                                }*/
                                break;
                            }
                        case GoldSource.DemoFrameType.NetMsg:
                        default:
                            {
                                LASTFRAMEISCLIENTDATA = false;
                                MessageId = 0;
                                NewViewAngleSearcherAngle = 0.0f;
                                //if (frame.Key.Type != GoldSource.DemoFrameType.NetMsg)
                                //{
                                //    Console.WriteLine("Invalid");
                                //}

                                CurrentNetMsgFrameId++;
                                NeedCheckAttack = true;

                                //DemoScanner.IsAttackSkipTimes--;

                                CurrentFrameIdWeapon += 10;

                                row = index + "/" + frame.Key.FrameIndex + " " + "[" +
                                      frame.Key.Time + "s]: " +
                                      "NETMESSAGE";

                                GoldSource.NetMsgFrame nf = (GoldSource.NetMsgFrame)frame.Value;
                                CurrentNetMsgFrame = nf;

                                bool AngleYBigChanged = false;

                                if (AngleBetweenAbsolute(CurrentNetMsgFrame.RParms.Viewangles.Y,
                                    PreviousNetMsgFrame.RParms.Viewangles.Y) > 0.1)
                                    AngleYBigChanged = true;

                                addAngleInViewListY(nf.RParms.Viewangles.Y);


                                PreviousTime3 = CurrentTime3;
                                CurrentTime3 = frame.Key.Time;

                                PreviousTime = CurrentTime;
                                CurrentTime = nf.RParms.Time;

                                if (Math.Abs(CurrentTime) > float.Epsilon && StartGameSecond > CurrentTime / 60.0f) StartGameSecond = Convert.ToInt32(CurrentTime / 60.0f);

                                if (Math.Abs(CurrentTime - PreviousTime) > 0.20f)
                                {
                                    LastLossTime = PreviousTime;
                                    LastLossTimeEnd = CurrentTime;
                                    LossPackets2++;
                                }

                                try
                                {
                                    CurrentTimeString = "MODIFIED";

                                    TimeSpan t = TimeSpan.FromSeconds(CurrentTime);

                                    CurrentTimeString = string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
                                        t.Hours,
                                        t.Minutes,
                                        t.Seconds,
                                        t.Milliseconds);
                                    lastnormalanswer = CurrentTimeString;

                                    Console.Title =
                                        "[ANTICHEAT/ANTIHACK] Unreal Demo Scanner " + PROGRAMVERSION + ". Demo:" +
                                        DemoName + ". DEMO TIME: " + CurrentTimeString;
                                }
                                catch
                                {
                                    ModifiedDemoFrames += 1;
                                    try
                                    {
                                        Console.Title =
                                            "[ANTICHEAT/ANTIHACK] Unreal Demo Scanner " + PROGRAMVERSION + ". Demo:" +
                                            DemoName + ". DEMO TIME: " + lastnormalanswer;
                                    }
                                    catch
                                    {
                                        try
                                        {
                                            Console.Title =
                                                "[ANTICHEAT/ANTIHACK] Unreal Demo Scanner " + PROGRAMVERSION + ". Demo:" +
                                                "BAD NAME" + ". DEMO TIME: " + lastnormalanswer;
                                        }
                                        catch
                                        {
                                            Console.WriteLine("Error access to frametime.");
                                        }
                                    }
                                }

                                CurrentMsgBytes += nf.MsgBytes.Length;
                                ParseGameData(nf.MsgBytes);
                                if (nf.MsgBytes.Length < 8 && Math.Abs(CurrentTime) > float.Epsilon)
                                {
                                    EmptyFrames++;
                                    if (EmptyFrames > 5 && EmptyFrames < 7)
                                    {
                                        // DemoScanner.DemoScanner_AddWarn("MORE THAN ONE EMPTY FRAMES:" + CurrentTimeString, false);
                                    }
                                }

                                if (Math.Abs(CurrentTime) > float.Epsilon && !FoundFirstTime)
                                {
                                    DemoStartTime = CurrentTime;
                                    FoundFirstTime = true;
                                }
                                if (Math.Abs(CurrentTime2) > float.Epsilon && !FoundFirstTime2)
                                {
                                    DemoStartTime2 = CurrentTime2;
                                    FoundFirstTime2 = true;
                                }

                                /*if (UserAlive && nf.RParms.Health <= 0 && CurrentTime != 0.0f)
                                {
                                    //Console.WriteLine("Dead time 2: " + DemoScanner.CurrentTimeString);
                                    UserAlive = false;
                                    DemoScanner.FirstUserAlive = false;
                                    RealAlive = false;
                                    DeathsCoount2++;
                                    //Console.WriteLine("Using dangerous dead detection method:" + DemoScanner.CurrentTimeString);
                                    if (DUMP_ALL_FRAMES)
                                        subnode.Text +=
                                            "LocalPlayer " + UserId + " killed[METHOD 2]!\n";
                                }*/
                                //else if (CurrentTime - LastDeathTime < 0.05)
                                //{
                                //    if (nf.RParms.Health < 0) DeathsCoount2++;
                                //}



                                // if (nf.RParms.Time != 0.0f && CurrentTime == nf.RParms.Time)
                                if (CurrentFrameDuplicated > 0) CurrentFrameDuplicated -= 1;

                                PreviousFrameTime = CurrentFrameTime;
                                CurrentFrameTime = nf.RParms.Frametime;

                                if (Math.Abs(CurrentTime) > float.Epsilon && PreviousNetMsgFrame.RParms.Time == nf.RParms.Time)
                                {
                                    // DemoScanner.FpsOverflowTime = CurrentTime;
                                    //if (nf.MsgBytes.Length <= 8)
                                    //{
                                    if (DUMP_ALL_FRAMES) subnode.Text += "[D U P L I C A D E R]";

                                    CurrentFrameDuplicated = 2;
                                    FrameDuplicates++;
                                    //}
                                    //else
                                    //{
                                    //}
                                }

                                SecondFrameTime += nf.RParms.Frametime;

                                CurrentFrameAlive = UserAlive;


                                RealAlive = CurrentFrameAlive && PreviousFrameAlive;

                                if (SkipNextAttack == 2) SkipNextAttack = 1;

                                if ((nf.UCmd.Buttons & 1) > 0)
                                {
                                    NewAttackForTrigger += 1;
                                    FrameAttackStrike++;
                                    FrameUnattackStrike = 0;
                                    CurrentFrameAttacked = true;

                                    LastAttackPressed = CurrentTime;

                                    if (!IsAttack && NeedIgnoreAttackFlag > 0) NeedIgnoreAttackFlag++;
                                }
                                else
                                {
                                    FrameUnattackStrike++;
                                    FrameAttackStrike = 0;

                                    if (!NeedSearchAim3 && PreviousFrameAttacked && RealAlive && Math.Abs(CurrentTime) > float.Epsilon &&
                                           Math.Abs(CurrentTime - IsAttackLastTime) > 0.075f &&
                                            Math.Abs(CurrentTime - IsNoAttackLastTime) > 0.075f)
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

                                    if (!IsAttack) NeedIgnoreAttackFlag = 0;

                                    CurrentFrameAttacked = false;
                                }


                                if (NeedIgnoreAttackFlag == 4) NeedIgnoreAttackFlagCount++;


                                if ((nf.UCmd.Buttons & 8) > 0)
                                    CurrentFrameForward = true;
                                else
                                    CurrentFrameForward = false;

                                if (RealAlive && !InForward && !InBack && !PreviousFrameForward &&
                                    CurrentFrameForward && Math.Abs(CurrentTime - LastMoveForward) > 1.0f && Math.Abs(CurrentTime - LastMoveBack) > 1.0f
                                    && Math.Abs(CurrentTime - LastUnMoveForward) > 1.5f)
                                    if (Math.Abs(CurrentTime - LastMovementHackTime) > 1.5)
                                    {
                                        DemoScanner_AddWarn(
                                            "[FORWARD HACK TYPE 1] at (" +
                                            CurrentTime + ") " + CurrentTimeString);
                                        LastMovementHackTime = CurrentTime;
                                    }

                                NewAttack = false;


                                if (IsUserAlive())
                                {
                                    if (CurrentFrameDuplicated == 0 && InitAimMissingSearch > 1) InitAimMissingSearch--;

                                    //if (InitAimMissingSearch == 1)
                                    //{
                                    //    if (CurrentTime - IsAttackLastTime > 0.10)
                                    //    {
                                    //        DemoScanner_AddWarn(
                                    //                   "[AIM TYPE 1.5 " + CurrentWeapon.ToString() + "] at (" + DemoScanner.IsAttackLastTime +
                                    //                   "):" + DemoScanner.CurrentTimeString, false);
                                    //        SilentAimDetected++;
                                    //        InitAimMissingSearch = 0;
                                    //    }
                                    //}


                                    if (CurrentFrameAttacked)
                                    {
                                        if (InitAimMissingSearch > 0) InitAimMissingSearch = 0;
                                    }
                                    else
                                    {
                                        if (FirstAttack && IsRealWeapon())
                                        {
                                            if (InitAimMissingSearch == 1)
                                            {
                                                /* if (AUTO_LEARN_HACK_DB)
                                                 {
                                                     ENABLE_LEARN_HACK_DEMO = true;
                                                     ENABLE_LEARN_HACK_DEMO_FORCE_SAVE = true;
                                                 }*/
                                                DemoScanner_AddWarn(
                                                    "[AIM TYPE 1.4 " + CurrentWeapon + "] at (" + IsAttackLastTime +
                                                    "):" + CurrentTimeString, !IsChangeWeapon() && !IsAngleEditByEngine() && !IsReload && SelectSlot <= 0 && !IsPlayerLossConnection() && !IsForceCenterView());
                                                TotalAimBotDetected++;
                                                InitAimMissingSearch = 0;
                                            }
                                        }
                                        else
                                        {
                                            if (InitAimMissingSearch > 0) InitAimMissingSearch = 0;
                                        }
                                    }

                                }
                                else
                                {
                                    if (InitAimMissingSearch > 0) InitAimMissingSearch = 0;
                                }


                                if (!PreviousFrameAttacked && CurrentFrameAttacked)
                                {
                                    if (IsUserAlive()) attackscounter3++;

                                    if (Math.Abs(CurrentTime - IsNoAttackLastTime) > 240.0)
                                    {
                                        UsingAnotherMethodWeaponDetection = !UsingAnotherMethodWeaponDetection;
                                    }

                                    if (DEBUG_ENABLED) Console.WriteLine("ATTACK:" + CurrentWeapon);

                                    FirstAttack = true;
                                    NewAttack = true;
                                    NewAttackForLearn = true;


                                    if (IsForceCenterView())
                                    {
                                        if (Math.Abs(CurrentTime - LastForceCenterView) > 10.0f)
                                        {
                                            DemoScanner_AddInfo(
                                                        "[ILLEGAL FORCE_CENTERVIEW] at (" + CurrentTime +
                                                        "):" + CurrentTimeString);

                                            LastForceCenterView = CurrentTime;
                                        }
                                        LastAim5DetectedReal = 0.0f;
                                        LastAim5Detected = 0.0f;
                                    }
                                }


                                if (CurrentWeapon == WeaponIdType.WEAPON_KNIFE
                                       || CurrentWeapon == WeaponIdType.WEAPON_C4
                                       || CurrentWeapon == WeaponIdType.WEAPON_HEGRENADE
                                       || CurrentWeapon == WeaponIdType.WEAPON_SMOKEGRENADE
                                       || CurrentWeapon == WeaponIdType.WEAPON_FLASHBANG
                                       || CurrentWeapon == WeaponIdType.WEAPON_NONE
                                       || CurrentWeapon == WeaponIdType.WEAPON_BAD
                                       || CurrentWeapon == WeaponIdType.WEAPON_BAD2
                                       //|| DemoScanner.IsAngleEditByEngine()
                                       || !RealAlive
                                       )
                                    ReloadWarns = 0;

                                if (CurrentWeapon == WeaponIdType.WEAPON_XM1014
                                    || CurrentWeapon == WeaponIdType.WEAPON_M3)
                                    ReloadWarns = 0;

                                if (RealAlive)
                                {
                                    if (NewAttack && !IsAngleEditByEngine())
                                    {
                                        NewAttackTime = CurrentTime;
                                        NewAttackTimeAim9 = CurrentTime;
                                        if (Math.Abs(CurrentTime - Aim8DetectionTimeX) < 0.25)
                                            //Console.WriteLine("Aim8DetectionTimeX warn!");
                                            Aim8DetectionTimeX = 0.0f;
                                        else if (Math.Abs(CurrentTime - Aim8DetectionTimeY) < 0.25)
                                            //Aim8DetectionTimeY = 0.0f;
                                            Console.WriteLine("Aim8DetectionTimeY warn!");
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

                                if (Math.Abs(nf.UCmd.Sidemove) > float.Epsilon) LastSideMoveTime = CurrentTime;
                                if (Math.Abs(nf.UCmd.Forwardmove) < float.Epsilon)
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

                                if (RealAlive)
                                    //if (CurrentFrameAttacked)
                                    //{
                                    //    Console.WriteLine("AngleDirX111:" + GetAngleDirection(nf.RParms.ClViewangles.X, PreviousNetMsgFrame.RParms.ClViewangles.X) +
                                    //        ". AngleDirY111:" + GetAngleDirection(nf.RParms.ClViewangles.Y, PreviousNetMsgFrame.RParms.ClViewangles.Y));
                                    //    Console.WriteLine("AngleDirX222:" + GetAngleDirection(nf.RParms.Viewangles.X, PreviousNetMsgFrame.RParms.Viewangles.X) +
                                    //        ". AngleDirY222:" + GetAngleDirection(nf.RParms.Viewangles.Y, PreviousNetMsgFrame.RParms.Viewangles.Y));
                                    //    Console.WriteLine("AngleDirX333:" + GetAngleDirection(nf.UCmd.Viewangles.Y, PreviousNetMsgFrame.UCmd.Viewangles.Y) +
                                    //        ". AngleDirY333:" + GetAngleDirection(nf.UCmd.Viewangles.Y, PreviousNetMsgFrame.UCmd.Viewangles.Y) + "\n");
                                    //}


                                    /* if (IsRoundEnd() || /*DemoScanner.LastBackMoveTime == 0.0f ||*/ /*DemoScanner.LastForwardMoveTime == 0.0f || DemoScanner.LastMoveForward == 0.0f)
                                         /*{
                                             DemoScanner.DesyncHackWarns = 0;
                                         }

                                         if (CurrentTime - LastJumpTime < 1.5 || !CurrentFrameOnGround)
                                         {
                                             DemoScanner.DesyncHackWarns = 0;
                                         }

                                         if (CurrentTime - LastMoveBack < 1.5)
                                         {
                                             DemoScanner.DesyncHackWarns = 0;
                                         }

                                         if (DemoScanner.IsBigVelocity() || IsHookDetected())
                                         {
                                             DemoScanner.DesyncHackWarns = 0;
                                         }
                                         else if (IsPlayerLossConnection() && DemoScanner.DesyncHackWarns > 0)
                                         {
                                             DemoScanner.DesyncHackWarns--;
                                         }
                                         else if (DemoScanner.IsAngleEditByEngine() && DemoScanner.DesyncHackWarns > 0)
                                         {
                                             DemoScanner.DesyncHackWarns = 0;
                                         }
                                         if (Math.Abs(CurrentTime - LastScreenshotTime) < 30.0f)
                                         {
                                             DemoScanner.DesyncHackWarns = 0;
                                         }

                                         if (InBack || CurrentTime - DemoScanner.LastMoveBack < 2.0)
                                         {
                                             DemoScanner.DesyncHackWarns = 0;
                                         }

                                         if (InForward || CurrentTime - DemoScanner.LastMoveForward < 2.0)
                                         {
                                             DemoScanner.DesyncHackWarns = 0;
                                         }

                                         if (Math.Abs(CurrentTime - GameEndTime2) < 30.0f)
                                         {
                                             DemoScanner.DesyncHackWarns = 0;
                                         }

                                         if (Math.Abs(CurrentTime - GameStartTime) < 60.0f)
                                         {
                                             DemoScanner.DesyncHackWarns = 0;
                                         }

                                         if (DemoScanner.IsPlayerTeleport())
                                         {
                                             DemoScanner.DesyncHackWarns = 0;
                                         }

                                         if (Math.Abs(nf.RParms.Simvel.X) > 100.0f ||
                                                 Math.Abs(nf.RParms.Simvel.Y) > 100.0f || Math.Abs(nf.RParms.Simvel.X) < -100.0f ||
                                                 Math.Abs(nf.RParms.Simvel.Y) < -100.0f || nf.RParms.Simvel.Z != 0.0f)
                                         {
                                             DemoScanner.DesyncHackWarns = 0;
                                         }

                                         if (DemoScanner.CurrentTime - DemoScanner.LastSideMoveTime < 2.5 ||
                                          DemoScanner.CurrentTime - DemoScanner.LastForwardMoveTime < 2.5 ||
                                          DemoScanner.CurrentTime - DemoScanner.LastBackMoveTime < 2.5)
                                         {
                                             if (DemoScanner.DesyncHackWarns > 0)
                                             {
                                                 DemoScanner.DesyncHackWarns--;
                                             }
                                         }

                                         if (Math.Abs(nf.RParms.Simvel.X) > 0.1f ||
                                             Math.Abs(nf.RParms.Simvel.Y) > 0.1f)
                                         {
                                             DemoScanner.DesyncHackWarns++;

                                             if (DemoScanner.DesyncHackWarns > 4 && CurrentTime - DemoScanner.LastDesyncDetectTime > 0.1)
                                             {
                                                 DemoScanner.LastDesyncDetectTime = CurrentTime;
                                                 if (!IsAngleEditByEngine() && !IsPlayerLossConnection() && DemoScanner.CurrentTime - DemoScanner.LastSoundTime > 2.0)
                                                 {
                                                     DemoScanner.DesyncDetects++;
                                                     DemoScanner_AddWarn(
                                                        "[DESYNC HACK] at (" +
                                                        CurrentTime + ") " + CurrentTimeString);
                                                 }
                                                 DemoScanner.DesyncHackWarns = 0;
                                             }
                                         }
                                         else
                                         {
                                             if (DemoScanner.DesyncHackWarns > 0)
                                             {
                                                 DemoScanner.DesyncHackWarns--;
                                             }
                                         }*/

                                    if (nf.UCmd.Sidemove < -40.0f || nf.UCmd.Sidemove > 40.0f)
                                        if (!InStrafe && !MoveLeft && !MoveRight && Math.Abs(CurrentTime - LastMoveLeft) > 0.5f &&
                                            Math.Abs(CurrentTime - LastMoveRight) > 0.5f && Math.Abs(CurrentTime - LastUnMoveLeft) > 0.5f &&
                                           Math.Abs(CurrentTime - LastUnMoveRight) > 0.5f &&
                                           Math.Abs(CurrentTime - LastStrafeDisabled) > 0.5f &&
                                            Math.Abs(CurrentTime - LastStrafeEnabled) > 0.5f)
                                        {
                                            if (Math.Abs(CurrentTime - LastMovementHackTime) > 2.5f)
                                            {
                                                DemoScanner_AddWarn(
                                                    "[MOVEMENT HACK TYPE 2] at (" +
                                                    CurrentTime + ") " + CurrentTimeString, !IsAngleEditByEngine() && !IsPlayerLossConnection() && (nf.UCmd.Sidemove < -100 || nf.UCmd.Sidemove > 100));
                                                LastMovementHackTime = CurrentTime;
                                            }
                                            JumpHackCount++;
                                        }

                                if (SearchMoveHack1)
                                {
                                    if ((nf.UCmd.Buttons & 512) > 0
                                           || (nf.UCmd.Buttons & 1024) > 0)
                                        //Console.WriteLine("MovementPressed " +
                                        //    (DemoScanner.MoveLeft ? "MOVELEFT PRESSED |" : "NO MOVELEFT |") + " " +
                                        //    (DemoScanner.MoveRight ? "MOVERIGHT PRESSED |" : "NO MOVERIGHT |") + " " +
                                        //    +CurrentTime + " " +
                                        //    LastMoveLeft + " " + LastMoveRight + " " + LastUnMoveLeft + " " + LastUnMoveRight + 
                                        //    " -> " + nf.UCmd.Sidemove);

                                        if (!MoveLeft && !MoveRight && Math.Abs(CurrentTime - LastMoveLeft) > 0.5f &&
                                           Math.Abs(CurrentTime - LastMoveRight) > 0.5f && Math.Abs(CurrentTime - LastUnMoveLeft) > 0.5f &&
                                           Math.Abs(CurrentTime - LastUnMoveRight) > 0.5f)
                                        {
                                            DemoScanner_AddWarn(
                                                "[MOVEMENT HACK TYPE 1] at (" +
                                                CurrentTime + ") " + CurrentTimeString, !IsAngleEditByEngine() && !IsPlayerLossConnection());
                                            SearchMoveHack1 = false;
                                        }

                                    if (SearchMoveHack1)
                                    {
                                        DemoScanner_AddWarn(
                                                   "[MOVEMENT HACK TYPE 1] at (" +
                                                   CurrentTime + ") " + CurrentTimeString, false);
                                        SearchMoveHack1 = false;
                                    }
                                }

                                if (RealAlive)
                                    if ((nf.UCmd.Buttons & 512) > 0
                                        || (nf.UCmd.Buttons & 1024) > 0)
                                        //Console.WriteLine("MovementPressed " +
                                        //    (DemoScanner.MoveLeft ? "MOVELEFT PRESSED |" : "NO MOVELEFT |") + " " +
                                        //    (DemoScanner.MoveRight ? "MOVERIGHT PRESSED |" : "NO MOVERIGHT |") + " " +
                                        //    +CurrentTime + " " +
                                        //    LastMoveLeft + " " + LastMoveRight + " " + LastUnMoveLeft + " " + LastUnMoveRight + 
                                        //    " -> " + nf.UCmd.Sidemove);

                                        if (!MoveLeft && !MoveRight && Math.Abs(CurrentTime - LastMoveLeft) > 0.5f &&
                                           Math.Abs(CurrentTime - LastMoveRight) > 0.5f && Math.Abs(CurrentTime - LastUnMoveLeft) > 0.5f &&
                                           Math.Abs(CurrentTime - LastUnMoveRight) > 0.5f)
                                        {
                                            if (Math.Abs(CurrentTime - LastMovementHackTime) > 2.5f)
                                            {
                                                SearchMoveHack1 = true;
                                                LastMovementHackTime = CurrentTime;
                                            }
                                            JumpHackCount++;
                                        }

                                if (IsAttack)
                                    if (PreviousFrameAttacked && !CurrentFrameAttacked)
                                    {
                                        int tmpframeattacked = 0;
                                        for (int n = frameindex + 1;
                                             n < CurrentDemoFile.GsDemoInfo.DirectoryEntries[index].Frames.Count;
                                             n++)
                                        {
                                            if (tmpframeattacked == -1 || tmpframeattacked > 2)
                                            {
                                                if (tmpframeattacked > 2)
                                                {
                                                    LostStopAttackButton += 1;
                                                    CheckConsoleCommand("-attack(PROGRAM)", true);
                                                }
                                                break;
                                            }
                                            GoldSource.FramesHren tmpframe = CurrentDemoFile.GsDemoInfo.DirectoryEntries[index].Frames[n];
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
                                                    GoldSource.NetMsgFrame tmpnetmsgframe1 = (GoldSource.NetMsgFrame)tmpframe.Value;
                                                    if (tmpnetmsgframe1 != nf)
                                                    {
                                                        if ((tmpnetmsgframe1.UCmd.Buttons & 1) > 0)
                                                        {
                                                            tmpframeattacked = -1;
                                                            break;
                                                        }

                                                        tmpframeattacked++;
                                                    }
                                                    break;
                                            }
                                        }
                                    }

                                if (NewAttack)
                                {
                                    if (AimType7Event == 53) AimType7Event = 3;

                                    if (AimType7Event == 52) AimType7Event = 2;
                                }
                                else
                                {
                                    if (AimType7Event == 53) AimType7Event = 0;

                                    if (AimType7Event == 52) AimType7Event = 0;
                                }

                                if ((nf.UCmd.Buttons & 2) > 0)
                                    CurrentFrameJumped = true;
                                else
                                    CurrentFrameJumped = false;

                                if (CurrentFrameJumped) LastJumpBtnTime = CurrentTime;


                                if (DemoScanner.SearchJumpHack5 > 1)
                                {
                                    DemoScanner.SearchJumpHack5--;
                                    if (IsPlayerAnyJumpPressed() || !IsUserAlive())
                                    {
                                        DemoScanner.SearchJumpHack5 = 0;
                                    }
                                }
                                else if (DemoScanner.SearchJumpHack5 == 1)
                                {
                                    DemoScanner.SearchJumpHack5--;
                                    if (!IsPlayerAnyJumpPressed() && IsUserAlive() && !DisableJump5AndAim16)
                                    {
                                        DemoScanner_AddWarn("[EXPERIMENTAL][JUMPHACK TYPE 5] at (" + CurrentTime +
                                                            "):" + CurrentTimeString, false, true, false, true);
                                    }
                                }


                                if (DemoScanner.SearchJumpHack51 > 1)
                                {
                                    DemoScanner.SearchJumpHack51--;
                                    if (IsPlayerBtnJumpPressed() || !IsUserAlive())
                                    {
                                        DemoScanner.SearchJumpHack51 = 0;
                                    }
                                }
                                else if (DemoScanner.SearchJumpHack51 == 1)
                                {
                                    DemoScanner.SearchJumpHack51--;
                                    if (!IsPlayerBtnJumpPressed() && IsUserAlive() && !DisableJump5AndAim16)
                                    {
                                        DemoScanner_AddWarn("[EXPERIMENTAL][JUMPHACK TYPE 5.1] at (" + CurrentTime +
                                                            "):" + CurrentTimeString, true, true, false, true);
                                    }
                                }

                                if (PreviousFrameJumped && !CurrentFrameJumped)
                                    if (NeedDetectBHOPHack && RealAlive) BHOP_JumpWarn++;

                                if (!PreviousFrameJumped && CurrentFrameJumped)
                                {
                                    // Console.WriteLine("Real jump at: " + CurrentTimeString);
                                    if (IsUserAlive()) JumpCount2++;
                                }
                                //Console.WriteLine("JMP BUTTON at (" + CurrentTime + ") : " + CurrentTimeString);

                                //if (FirstAttack && CurrentTime == IsAttackLastTime)
                                //{
                                //    DemoScanner.SearchAim6 = true;
                                //}
                                //else
                                //{
                                //    if (DemoScanner.SearchAim6 )
                                //    {

                                //    }
                                //    DemoScanner.SearchAim6 = false;
                                //}

                                if ((nf.UCmd.Buttons & 4) > 0)
                                    CurrentFrameDuck = true;
                                else
                                    CurrentFrameDuck = false;

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
                                    LastCmdDuckTime = CurrentTime;
                                else if (PreviousFrameDuck && !CurrentFrameDuck) LastCmdUnduckTime = CurrentTime;

                                if (!CurrentFrameDuck && IsDuckPressed)
                                {
                                    IsDuckPressed = false;
                                    LastUnDuckTime = CurrentTime;
                                    //Console.WriteLine("Reset already induck!");
                                }

                                if (RealAlive && !IsDuck && !IsDuckPressed && FirstDuck && Math.Abs(CurrentTime - LastUnDuckTime) > 2.5f &&
                                    Math.Abs(CurrentTime - LastDuckTime) > 2.5f && Math.Abs(CurrentTime - LastAliveTime) > 1.2f)
                                {
                                    if (!PreviousFrameDuck && CurrentFrameDuck)
                                    {
                                        SearchOneFrameDuck = true;
                                    }
                                    else if (SearchOneFrameDuck && PreviousFrameDuck && !CurrentFrameDuck)
                                    {
                                        if (Math.Abs(CurrentTime - LastJumpHackTime) > 2.5f)
                                        {
                                            DemoScanner_AddWarn(
                                                "[DUCK HACK TYPE 3] at (" +
                                                CurrentTime + ") " + CurrentTimeString, !IsPlayerLossConnection());
                                            LastJumpHackTime = CurrentTime;
                                            JumpHackCount++;
                                        }
                                    }
                                    else
                                    {
                                        SearchOneFrameDuck = false;
                                    }


                                    if (CurrentFrameDuck && PreviousFrameDuck && LastDuckTime > LastUnDuckTime && Math.Abs(CurrentTime - LastUnDuckTime) > 1.5f &&
                                   Math.Abs(CurrentTime - LastDuckTime) > 5.0f)
                                    {
                                        DuckHack2Strikes++;
                                        if (/*DemoScanner.DuckHack2Strikes > 5 &&*/ Math.Abs(CurrentTime - LastJumpHackTime) > 2.5f)
                                        {
                                            //Console.WriteLine("1:" + (CurrentTime - LastUnDuckTime));
                                            //Console.WriteLine("2:" + (CurrentTime - LastDuckTime));
                                            DemoScanner_AddWarn(
                                                "[DUCK HACK TYPE 2] at (" +
                                                CurrentTime + ") " + CurrentTimeString, false);
                                            LastJumpHackTime = CurrentTime;
                                            JumpHackCount++;
                                            DuckHack2Strikes = 0;
                                        }
                                    }
                                    else
                                    {
                                        DuckHack2Strikes = 0;
                                    }

                                    if (CurrentFrameDuck && !PreviousFrameDuck && LastUnDuckTime > LastDuckTime &&
                                    Math.Abs(CurrentTime - LastDuckTime) > 2.5f &&
                                   Math.Abs(CurrentTime - LastUnDuckTime) > 0.2f)
                                    {
                                        if (DuckStrikes < 2)
                                        {
                                            //DemoScanner.DuckHack1Strikes++;
                                            if (/*DemoScanner.DuckHack1Strikes > 1 && */Math.Abs(CurrentTime - LastJumpHackTime) > 2.5f)
                                            {
                                                //Console.WriteLine("11:" + (CurrentTime - LastUnDuckTime));
                                                //Console.WriteLine("22:" + (CurrentTime - LastDuckTime));
                                                DemoScanner_AddWarn(
                                                    "[DUCK HACK TYPE 1] at (" +
                                                    CurrentTime + ") " + CurrentTimeString, !IsPlayerLossConnection());
                                                LastJumpHackTime = CurrentTime;
                                                JumpHackCount++;
                                                DuckHack1Strikes = 0;
                                            }
                                        }
                                        else
                                        {
                                            DuckHack1Strikes = 0;
                                        }
                                    }
                                    else
                                    {
                                        DuckHack1Strikes = 0;
                                    }
                                }
                                else
                                {
                                    SearchOneFrameDuck = false;
                                    DuckHack2Strikes = 0;
                                    DuckHack1Strikes = 0;
                                }


                                if (nf.RParms.Onground != 0)
                                    CurrentFrameOnGround = true;
                                else
                                    CurrentFrameOnGround = false;

                                if (CurrentFrameOnGround && PreviousFrameOnGround)
                                {
                                    FramesOnGround++;
                                    FramesOnFly = 0;

                                }


                                if (CurrentFrameOnGround)
                                    TotalFramesOnGround++;
                                else
                                    TotalFramesOnFly++;

                                if (CurrentFrameAttacked)
                                    if (!CurrentFrameOnGround) TotalAttackFramesOnFly++;

                                if (RealAlive && CurrentFrameOnGround) StrafeAngleDirectionChanges = 0;

                                if (NewAttack)
                                    if (!CurrentFrameOnGround) AirShots++;

                                //if (RealAlive)
                                //{
                                //    if (FramesOnGround > 10)
                                //    {
                                //        if (nf.UCmd.Forwardmove > DemoScanner.MaxSpeed)
                                //        {
                                //            DemoScanner.MaxSpeed = nf.UCmd.Forwardmove;
                                //            Console.WriteLine("Max speed : " + MaxSpeed);
                                //        }
                                //    }
                                //}

                                if (!PreviousFrameOnGround && !CurrentFrameOnGround)
                                {
                                    FramesOnFly++;
                                    FramesOnGround = 0;
                                }

                                if (!RealAlive) JumpEmulatorWarnPart1 = 0;

                                if (!PreviousFrameOnGround && CurrentFrameOnGround)
                                {
                                    if (NeedDetectBHOPHack && RealAlive) BHOP_GroundSearchDirection = 1;
                                }
                                else if (PreviousFrameOnGround && CurrentFrameOnGround)
                                {
                                    if (NeedDetectBHOPHack && RealAlive && BHOP_GroundSearchDirection > 0)
                                        BHOP_GroundSearchDirection += 1;
                                    else
                                        BHOP_GroundSearchDirection = 0;
                                }
                                else if (PreviousFrameOnGround && !CurrentFrameOnGround)
                                {
                                    FirstJump = true;
                                    if (IsUserAlive())
                                    {
                                        if (nf.RParms.Simvel.Z > 100.0) JumpCount3++;

                                        LastJumpNoGroundTime = CurrentTime;
                                    }
                                    if (DEBUG_ENABLED) CheckConsoleCommand("over ground", true);
                                    if (CurrentFrameId - LastJumpFrame <= 3)
                                    {
                                        if (Math.Abs(CurrentTime - LastRealJumpTime) > 2.25f)
                                        {
                                            if (JumpEmulatorWarnPart1 > 0) JumpEmulatorWarnPart1--;

                                            if (JumpEmulatorWarnPart1 > 0) JumpEmulatorWarnPart1--;
                                        }
                                        //Console.WriteLine("Jump uuump : " + (CurrentFrameId - LastJumpFrame) + ". Time between: " + (CurrentTime - LastRealJumpTime));
                                        if (JumpHistory.Count > 0)
                                        {
                                            JumpHistory.Reverse();
                                            //Console.Write("Jump history : ");
                                            //foreach (var jmpframeid in JumpHistory)
                                            //{
                                            //    Console.Write(" " + jmpframeid);
                                            //}
                                            //Console.WriteLine();
                                            bool needaddjumpemulatorwarn = true;
                                            foreach (int jmpframeid in JumpHistory)
                                                if (jmpframeid < 8 || jmpframeid > 100)
                                                {
                                                    if (JumpEmulatorWarnPart1 > 0) JumpEmulatorWarnPart1--;

                                                    if (JumpEmulatorWarnPart1 > 0) JumpEmulatorWarnPart1--;

                                                    needaddjumpemulatorwarn = false;
                                                    break;
                                                }

                                            if (JumpHistory.Count == 1)
                                            {
                                                BHOPJumpHistoryCount1Warn++;
                                                if (BHOPJumpHistoryCount1Warn > 3) needaddjumpemulatorwarn = false;
                                            }
                                            else
                                            {
                                                BHOPJumpHistoryCount1Warn = 0;
                                            }

                                            if (needaddjumpemulatorwarn)
                                            {
                                                JumpEmulatorWarnPart1++;
                                                if (JumpEmulatorWarnPart1 > 8)
                                                {
                                                    DemoScanner_AddWarn("[HPP BHOP] at (" + CurrentTime + ") : " + CurrentTimeString, false);
                                                    JumpEmulatorWarnPart1 = 0;
                                                }
                                                //Console.WriteLine("ADD WARN #2");
                                                //Console.WriteLine("Waarn: " + DemoScanner.JumpEmulatorWarnPart1);
                                            }

                                        }
                                        else
                                        {
                                            JumpEmulatorWarnPart1++;
                                            if (JumpEmulatorWarnPart1 > 8)
                                            {
                                                DemoScanner_AddWarn("[HPP BHOP] at (" + CurrentTime + ") : " + CurrentTimeString, false);
                                                JumpEmulatorWarnPart1 = 0;
                                            }
                                            //Console.WriteLine("ADD WARN #1");
                                            //Console.WriteLine("Waarn: " + DemoScanner.JumpEmulatorWarnPart1);
                                        }
                                    }
                                    else
                                    {
                                        JumpEmulatorWarnPart1 = 0;
                                    }

                                    if (NeedDetectBHOPHack && RealAlive)
                                    {
                                        if (BHOP_GroundSearchDirection < 5) BHOP_GroundWarn++;
                                        BHOP_GroundSearchDirection = 0;
                                    }

                                    LastRealJumpTime = CurrentTime;

                                    JumpHistory.Clear();
                                }

                                if (PreviousFrameOnGround && !CurrentFrameOnGround && (IsJump || Math.Abs(CurrentTime - LastJumpTime) < 0.2f))
                                {
                                    if (JmpWarn - CurJmpWarns >= 1 && CurJmpWarns > 0)
                                        RealJumpEmulatorHackWarns++;
                                    else
                                        RealJumpEmulatorHackWarns--;

                                    if (JumpCount - CurJumpCount < 5) RealJumpEmulatorHackWarns--;

                                    if (RealJumpEmulatorHackWarns >= 3)
                                    {
                                        RealJumpEmulatorHackWarns = 0;
                                        DemoScanner_AddWarn("[OLD HPP JUMP EMULATOR] at (" + CurrentTime + ") : " + CurrentTimeString, false);
                                    }
                                    else if (RealJumpEmulatorHackWarns < 0)
                                    {
                                        RealJumpEmulatorHackWarns = 0;
                                    }


                                    CurJmpWarns = JmpWarn;
                                    //Console.WriteLine("REAL UNJUMP ( " + (JumpCount - DemoScanner.CurJumpCount) + " ) at (" + CurrentTime + ") : " + CurrentTimeString);
                                    CurJumpCount = JumpCount;
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

                                BadPunchAngle = Math.Abs(nf.RParms.Punchangle.Z) > float.Epsilon/* ||
                                     Math.Abs(nf.RParms.Punchangle.Y) > float.Epsilon ||
                                     Math.Abs(nf.RParms.Punchangle.X) > float.Epsilon*/;

                                addAngleInPunchListY(nf.RParms.Punchangle.Y);
                                UpdatePunchAngleSearchers();

                                if (!AngleYBigChanged)
                                {
                                    UpdateViewAngleSearchers();
                                }
                                else
                                {
                                    LostAngleWarnings = 0;
                                    angleSearchersView = new List<AngleSearcher>();
                                }

                                if (WeaponChanged || IsTakeDamage(1.5f) || CurrentWeapon == WeaponIdType.WEAPON_NONE
                                              || CurrentWeapon == WeaponIdType.WEAPON_BAD
                                              || CurrentWeapon == WeaponIdType.WEAPON_BAD2
                                              || CurrentWeapon == WeaponIdType.WEAPON_C4
                                              || CurrentWeapon == WeaponIdType.WEAPON_HEGRENADE
                                              || CurrentWeapon == WeaponIdType.WEAPON_SMOKEGRENADE
                                              || CurrentWeapon == WeaponIdType.WEAPON_FLASHBANG
                                              || IsPlayerLossConnection() ||
                                                !IsUserAlive() || IsAngleEditByEngine())
                                {
                                    PunchWarnings = 0;
                                    LostAngleWarnings = 0;
                                    angleSearchersPunch = new List<AngleSearcher>();
                                    angleSearchersView = new List<AngleSearcher>();
                                }
                                else if (PunchWarnings > 2)
                                {
                                    PunchWarnings = 0;
                                    DemoScanner_AddWarn("[BETA AIM TYPE 9.1 " + CurrentWeapon + "] at (" + LastAnglePunchSearchTime +
                                    "):" + CurrentTimeString, false);
                                    angleSearchersPunch = new List<AngleSearcher>(); ;
                                }
                                else if (LostAngleWarnings > 2)
                                {
                                    LostAngleWarnings = 0;
                                    // DemoScanner.DemoScanner_AddWarn("[BETA AIM TYPE 9.2 " + DemoScanner.CurrentWeapon.ToString() + "] at (" + LastAnglePunchSearchTime +
                                    // "):" + DemoScanner.CurrentTimeString, false);
                                    NewViewAngleSearcherAngle = 0.0f;
                                }


                                if (AimType7Event != 0)
                                {
                                    if (Math.Abs(Aim7PunchangleY) < float.Epsilon && Math.Abs(nf.RParms.Punchangle.Y) > float.Epsilon) Aim7PunchangleY = nf.RParms.Punchangle.Y;
                                }
                                else
                                {
                                    Aim7PunchangleY = 0.0f;
                                }

                                AddLerpAndMs(nf.UCmd.LerpMsec, nf.UCmd.Msec);

                                CurrentFrameLerp = nf.UCmd.LerpMsec;

                                if (!FakeLagsValus.Contains(CurrentFrameLerp)) FakeLagsValus.Add(CurrentFrameLerp);

                                CurrentFramePunchangleZ = nf.RParms.Punchangle.Z;



                                /*
                                 * 1. Игрок не на земле
                                 * 2. Игрок на земле 1 кадр
                                 * 3. Игрок не на земле
                                // * */



                                // 01 
                                if (RealAlive)
                                {
                                    if (SearchNextJumpStrike)
                                    {
                                        SearchNextJumpStrike = false;
                                        if (Math.Abs(IdealJmpTmpTime1 - LastUnJumpTime) > float.Epsilon &&
                                            Math.Abs(IdealJmpTmpTime2 - LastJumpTime) > float.Epsilon &&
                                            PreviousFrameOnGround && !CurrentFrameOnGround && (Math.Abs(CurrentTime - LastUnJumpTime) < 0.25f
                                            || Math.Abs(CurrentTime - LastJumpTime) < 0.25f))
                                        {
                                            IdealJmpTmpTime1 = LastUnJumpTime;
                                            IdealJmpTmpTime2 = LastJumpTime;
                                            CurrentIdealJumpsStrike++;
                                            if (CurrentIdealJumpsStrike > MaxIdealJumps)
                                            {
                                                CurrentIdealJumpsStrike = 0;
                                                DemoScanner_AddWarn("[IDEALJUMP] at (" + CurrentTime + ") : " + CurrentTimeString);
                                            }
                                            if (DEBUG_ENABLED) Console.WriteLine("IDEALJUMP WARN [" + (CurrentFrameId - LastCmdFrameId) + "] (" + CurrentTime + ") : " + CurrentTimeString);
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
                                            if (FramesOnFly > 10) SearchNextJumpStrike = true;
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


                                if (RealAlive && CurrentFrameAttacked && Math.Abs(CurrentTime - LastDeathTime) > 5.0f && Math.Abs(CurrentTime - LastAliveTime) > 2.0f)
                                    if (FovHackDetected < 5)
                                        if (cdframeFov > 90)
                                            if (!IsAngleEditByEngine() && !IsPlayerLossConnection())
                                            {
                                                DemoScanner_AddWarn(
                                                    "[FOV HACK] at (" + CurrentTime +
                                                    "):" + CurrentTimeString + (Math.Abs(cdframeFov - ClientFov) < float.Epsilon || Math.Abs(cdframeFov - FovByFunc) < float.Epsilon ? "[BY SERVER ???]" : ""));

                                                FovHackDetected += 1;
                                            }

                                if (NeedDetectThirdPersonHack)
                                {
                                    if (ThirdPersonHackDetectionTimeout > 0)
                                    {
                                        ThirdPersonHackDetectionTimeout--;
                                    }
                                    else if (ThirdPersonHackDetectionTimeout == 0)
                                    {
                                        DemoScanner_AddWarn(
                                            "[THIRD PERSON HACK] at (" +
                                            CurrentTime + "):" + CurrentTimeString);
                                        ThirdHackDetected += 1;
                                        NeedDetectThirdPersonHack = false;
                                        ThirdPersonHackDetectionTimeout = -1;
                                    }
                                }

                                if (RealAlive && !IsAngleEditByEngine() && !IsPlayerLossConnection() && Math.Abs(CurrentTime - LastJumpTime) > 0.5f)
                                {
                                    if (!NeedDetectThirdPersonHack && CurrentFrameAttacked && GetDistance(new FPoint(nf.View.X, nf.View.Y),
                                    new FPoint(nf.RParms.Vieworg.X,
                                        nf.RParms.Vieworg.Y)) > 50 &&
                                     Math.Abs(CurrentTime - LastAliveTime) > 2.0f && !IsTakeDamage() && Math.Abs(CurrentTime - LastDeathTime) > 5.0f && ThirdHackDetected < 5 && CurrentWeapon !=
                                                          WeaponIdType.WEAPON_NONE
                                                          && CurrentWeapon !=
                                                          WeaponIdType.WEAPON_BAD &&
                                                          CurrentWeapon !=
                                                          WeaponIdType.WEAPON_BAD2)
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


                                if (Math.Abs(CurrentTime2) > float.Epsilon)
                                {
                                    if (Math.Abs(LastFpsCheckTime2) < float.Epsilon)
                                    {
                                        LastFpsCheckTime2 = CurrentTime2;
                                    }
                                    else
                                    {
                                        if ((CurrentTimeDesync2 =
                                                Math.Abs(CurrentTime2 - LastFpsCheckTime2)) >
                                            1.0f)
                                        {
                                            if (DUMP_ALL_FRAMES)
                                                subnode.Text +=
                                                    "CurrentFps2:" + CurrentFps2 + "\n";

                                            if (CurrentFps2 > RealFpsMax2) RealFpsMax2 = CurrentFps2;

                                            if (CurrentFps2 < RealFpsMin2 && CurrentFps2 > 0) RealFpsMin2 = CurrentFps2;

                                            LastFpsCheckTime2 = CurrentTime2;

                                            SecondFound2 = true;
                                            CurrentGameSecond2++;
                                            MaxIdealJumps = 6;
                                            if (averagefps2.Count > 1)
                                            {
                                                float tmpafps = averagefps2.Average();
                                                if (tmpafps < 40)
                                                    MaxIdealJumps = 10;
                                                else if (tmpafps < 50)
                                                    MaxIdealJumps = 9;
                                                else if (tmpafps < 60)
                                                    MaxIdealJumps = 8;
                                                else if (tmpafps < 70) MaxIdealJumps = 7;
                                            }
                                            CurrentFps2 = 0;
                                        }
                                        else
                                        {
                                            CurrentFps2++;
                                        }
                                    }
                                }



                                if (!RealAlive || HideWeapon || CurrentFrameDuplicated > 1 || CurrentWeapon == WeaponIdType.WEAPON_NONE
                                           || CurrentWeapon == WeaponIdType.WEAPON_BAD
                                           || CurrentWeapon == WeaponIdType.WEAPON_BAD2
                                           || CurrentWeapon == WeaponIdType.WEAPON_C4
                                           || CurrentWeapon == WeaponIdType.WEAPON_HEGRENADE
                                           || CurrentWeapon == WeaponIdType.WEAPON_KNIFE
                                           || CurrentWeapon == WeaponIdType.WEAPON_SMOKEGRENADE
                                           || CurrentWeapon == WeaponIdType.WEAPON_FLASHBANG)
                                {
                                    AimType8Warn = 0;
                                    AimType8WarnTime = 0.0f;
                                    AimType8WarnTime2 = 0.0f;
                                }
                                else
                                {
                                    if (CurrentFrameAttacked || PreviousFrameAttacked)
                                    {
                                        if (IsForceCenterView())
                                        {
                                            if (Math.Abs(CurrentTime - LastForceCenterView) > 10.0f)
                                            {
                                                DemoScanner_AddInfo(
                                                            "[FORCE_CENTERVIEW 2] at (" + CurrentTime +
                                                            "):" + CurrentTimeString);

                                                LastForceCenterView = CurrentTime;
                                            }
                                            AimType8WarnTime = 0.0f;
                                            AimType8WarnTime2 = 0.0f;
                                            AimType8False = false;
                                        }
                                        if (Math.Abs(AimType8WarnTime) > float.Epsilon && Math.Abs(CurrentTime - AimType8WarnTime) < 0.350f)
                                        {
                                            DemoScanner_AddWarn(
                                                "[AIM TYPE 8.1 " + CurrentWeapon + "] at (" + AimType8WarnTime +
                                                "):" + CurrentTime, !AimType8False && !IsChangeWeapon());
                                            if (!AimType8False && !IsChangeWeapon()) TotalAimBotDetected++;

                                            AimType8WarnTime = 0.0f;
                                            AimType8False = false;
                                        }
                                        else if (Math.Abs(AimType8WarnTime2) > float.Epsilon && Math.Abs(CurrentTime - AimType8WarnTime2) < 0.350f)
                                        {
                                            DemoScanner_AddWarn(
                                                "[AIM TYPE 8.2 " + CurrentWeapon + "] at (" + AimType8WarnTime2 +
                                                "):" + CurrentTimeString, /*DemoScanner.CurrentWeapon != WeaponIdType.WEAPON_AWP
                                    && DemoScanner.CurrentWeapon != WeaponIdType.WEAPON_SCOUT &&*/ !AimType8False && !IsChangeWeapon());
                                            if (!AimType8False && !IsChangeWeapon()) TotalAimBotDetected++;

                                            AimType8WarnTime2 = 0.0f;
                                            AimType8False = false;
                                        }
                                    }
                                    if (nf.RParms.ClViewangles.Y != PREV_CDFRAME_ViewAngles.Y
                                       || nf.UCmd.Viewangles.Y != PREV_CDFRAME_ViewAngles.Y)
                                    {
                                        if (!IsForceCenterView() && !IsAngleEditByEngine()
                                            && AngleBetweenAbsolute(CDFRAME_ViewAngles.X, nf.RParms.Viewangles.X) > float.Epsilon
                                        && AngleBetweenAbsolute(PREV_CDFRAME_ViewAngles.X, nf.RParms.Viewangles.X) > float.Epsilon)
                                            if (CurrentFrameAttacked && CurrentFrameOnGround && Math.Abs(CurrentTime - LastDeathTime) > 2.0f
                                                && Math.Abs(CurrentTime - LastAliveTime) > 2.0f)
                                            {
                                                float spreadtest = AngleBetweenAbsolute(CDFRAME_ViewAngles.X, nf.RParms.Viewangles.X - nf.RParms.Punchangle.X);
                                                float spreadtest2 = AngleBetweenAbsolute(PREV_CDFRAME_ViewAngles.X, nf.RParms.Viewangles.X - nf.RParms.Punchangle.X);
                                                if (spreadtest > nospreadtest &&
                                                    spreadtest2 > nospreadtest)
                                                {
                                                    if (spreadtest > spreadtest2)
                                                        nospreadtest = spreadtest;
                                                    else
                                                        nospreadtest = spreadtest2;
                                                    // Console.WriteLine(nospreadtest.ToString("F8"));
                                                }

                                                if (Math.Abs(NoSpreadDetectionTime - CurrentTime) > float.Epsilon && spreadtest > MAX_SPREAD_CONST && spreadtest2 > MAX_SPREAD_CONST)
                                                {
                                                    NoSpreadDetectionTime = CurrentTime;
                                                    DemoScanner_AddWarn(
                                                        "[NOSPREAD TYPE 1 " + CurrentWeapon + "] at (" +
                                                        CurrentTime + "):" + CurrentTimeString, false);
                                                }
                                            }

                                        //if (CurrentFrameAttacked)
                                        //    ;// Console.WriteLine("ATTACKED");
                                        //else if (CurrentFrameDuck)
                                        //    ;// Console.WriteLine("DUCKED");
                                        //else if (CurrentFrameJumped)
                                        //    ;// Console.WriteLine("JUMPED");
                                        //else if (PreviousFrameAttacked)
                                        //    ;// Console.WriteLine("PREVATT");
                                        //else if (!CurrentFrameOnGround)
                                        //    ;// Console.WriteLine("NOGROUND");
                                        //else if (CurrentTime - IsAttackLastTime < 3.0f || IsAttack)
                                        //    ;// Console.WriteLine("LASTATTACK");
                                        //else
                                        //{
                                        //    Console.WriteLine("NONE(" + nf.UCmd.Buttons + ")");
                                        //}
                                        if (AngleBetweenAbsolute(CDFRAME_ViewAngles.Y, nf.RParms.Viewangles.Y) > float.Epsilon
                                            && AngleBetweenAbsolute(PREV_CDFRAME_ViewAngles.Y, nf.RParms.Viewangles.Y) > float.Epsilon)
                                            if (CurrentFrameAttacked && CurrentFrameOnGround && Math.Abs(CurrentTime - LastDeathTime) > 2.0f
                                                && Math.Abs(CurrentTime - LastAliveTime) > 2.0f)
                                            {
                                                float spreadtest = AngleBetweenAbsolute(CDFRAME_ViewAngles.Y, nf.RParms.Viewangles.Y - nf.RParms.Punchangle.Y);
                                                float spreadtest2 = AngleBetweenAbsolute(PREV_CDFRAME_ViewAngles.Y, nf.RParms.Viewangles.Y - nf.RParms.Punchangle.Y);
                                                if (spreadtest > nospreadtest2 &&
                                                    spreadtest2 > nospreadtest2)
                                                {
                                                    if (spreadtest > spreadtest2)
                                                        nospreadtest2 = spreadtest;
                                                    else
                                                        nospreadtest2 = spreadtest2;
                                                    // Console.WriteLine(nospreadtest.ToString("F8"));
                                                }
                                                if (Math.Abs(NoSpreadDetectionTime - CurrentTime) > float.Epsilon && spreadtest > MAX_SPREAD_CONST2
                                                    && spreadtest2 > MAX_SPREAD_CONST2)
                                                {
                                                    NoSpreadDetectionTime = CurrentTime;
                                                    DemoScanner_AddWarn(
                                                        "[NOSPREAD TYPE 2 " + CurrentWeapon + "] at (" +
                                                        CurrentTime + "):" + CurrentTimeString, false);
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
                                    //    //else if (CurrentTime - IsAttackLastTime < 3.0f || IsAttack)
                                    //    //    ;// Console.WriteLine("LASTATTACK");
                                    //    //else
                                    //    //{
                                    //    //    Console.WriteLine("NONE(" + nf.UCmd.Buttons + ")");
                                    //    //}
                                    //}
                                    if (nf.RParms.ClViewangles.Y != PREV_CDFRAME_ViewAngles.Y
                                        || nf.UCmd.Viewangles.Y != PREV_CDFRAME_ViewAngles.Y)
                                    {
                                        if (CDFRAME_ViewAngles != nf.RParms.ClViewangles)
                                        {
                                            /*if (AUTO_LEARN_HACK_DB)
                                            {
                                                ENABLE_LEARN_HACK_DEMO = true;
                                                ENABLE_LEARN_HACK_DEMO_FORCE_SAVE = true;
                                            }*/
                                            if (AimType8Warn > 5) AimType8Warn = 0;

                                            AimType8Warn++;
                                            if (AimType8Warn == 1 ||
                                                AimType8Warn == 2)
                                            {
                                                if (Math.Abs(bAimType8WarnTime - CurrentTime) > float.Epsilon) AimType8WarnTime = CurrentTime;

                                                bAimType8WarnTime = CurrentTime;

                                                //Console.WriteLine("LastClientDataTime:" + LastClientDataTime + ". AimType8WarnTime:" + CurrentTime
                                                //     + ". LastOverflow:" + FpsOverflowTime);

                                                if (!AimType8False)
                                                    AimType8False = CurrentWeapon == WeaponIdType.WEAPON_C4
                                                                    || CurrentWeapon == WeaponIdType.WEAPON_HEGRENADE
                                                                    || CurrentWeapon == WeaponIdType.WEAPON_KNIFE
                                                                    || CurrentWeapon == WeaponIdType.WEAPON_SMOKEGRENADE
                                                                    || CurrentWeapon == WeaponIdType.WEAPON_FLASHBANG || !CurrentFrameOnGround || IsAngleEditByEngine() || IsPlayerLossConnection() || IsChangeWeapon();
                                            }
                                        }
                                        else
                                        {
                                            AimType8Warn = 0;
                                        }
                                        if (CDFRAME_ViewAngles != nf.UCmd.Viewangles)
                                        {
                                            /* if (AUTO_LEARN_HACK_DB)
                                             {
                                                 ENABLE_LEARN_HACK_DEMO = true;
                                                 ENABLE_LEARN_HACK_DEMO_FORCE_SAVE = true;
                                             }*/
                                            if (AimType8Warn > 5) AimType8Warn = 0;

                                            AimType8Warn++;
                                            if (AimType8Warn == 1 ||
                                                AimType8Warn == 2)
                                            {
                                                if (Math.Abs(bAimType8WarnTime2 - CurrentTime) > float.Epsilon) AimType8WarnTime2 = CurrentTime;

                                                bAimType8WarnTime2 = CurrentTime;
                                                if (!AimType8False)
                                                    AimType8False = CurrentWeapon == WeaponIdType.WEAPON_C4
                                                                    || CurrentWeapon == WeaponIdType.WEAPON_HEGRENADE
                                                                    || CurrentWeapon == WeaponIdType.WEAPON_KNIFE
                                                                    || CurrentWeapon == WeaponIdType.WEAPON_SMOKEGRENADE
                                                                    || CurrentWeapon == WeaponIdType.WEAPON_FLASHBANG || !CurrentFrameOnGround || IsAngleEditByEngine() || IsPlayerLossConnection() || IsChangeWeapon();
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
                                        if (CDFRAME_ViewAngles != nf.UCmd.Viewangles
                                            && CDFRAME_ViewAngles != nf.RParms.ClViewangles)
                                        {
                                            if (Math.Abs(CurrentTime - FpsOverflowTime) < 1.0f)
                                            {
                                                FPS_OVERFLOW++;
                                                if (FPS_OVERFLOW == 20)
                                                    DemoScanner_AddWarn("[FPS HACK TYPE 1] at (" +
                                                                        CurrentTime + "):" + CurrentTimeString);
                                            }
                                            else
                                            {
                                                FPS_OVERFLOW = 0;
                                            }
                                            FpsOverflowTime = CurrentTime;
                                        }
                                    }
                                }

                                if (Math.Abs(CurrentTime) > float.Epsilon && Math.Abs(PreviousTime) > float.Epsilon &&
                                    Math.Abs(CurrentTime2) > float.Epsilon && Math.Abs(PreviousTime2) > float.Epsilon)
                                {
                                    if (Math.Abs(MaximumTimeBetweenFrames) < float.Epsilon)
                                    {
                                        MaximumTimeBetweenFrames = 0.01f;
                                    }
                                    else
                                    {
                                        if (CurrentFrameDuplicated == 0 && Math.Abs(CurrentTime - PreviousTime) > 0.5f && CurrentFrameId > 10)
                                        {
                                            TimeShiftCount += 1;

                                            if (LastTimeOut != 1 && TimeShiftCount - LossPackets > 4 + ChokePackets && Math.Abs(CurrentTime - LastChokePacket) > 60)
                                            {
                                                DemoScanner_AddWarn(
                                                    "[TIMESHIFT] at (" + DemoStartTime + "):"
                                                    + "(" + CurrentTime + "):" + "(" + DemoStartTime2 + "):" + "(" + CurrentTime2 + "):" +
                                                    CurrentTimeString, false);
                                                LastTimeOut = 1;

                                            }
                                        }
                                        if (CurrentTime - PreviousTime > MaximumTimeBetweenFrames) MaximumTimeBetweenFrames = Math.Abs(CurrentTime - PreviousTime);
                                    }
                                }

                                if (Math.Abs(LastTimeDesync) < float.Epsilon)
                                {
                                    LastTimeDesync = Math.Abs(CurrentTime - CurrentTime2);
                                }
                                else
                                {
                                    if (Math.Abs(LastTimeDesync -
                                                 Math.Abs(CurrentTime - CurrentTime2)) > 0.08f
                                                 || PreviousTime - CurrentTime > float.Epsilon
                                                 || PreviousTime2 - CurrentTime2 > float.Epsilon)
                                    {
                                        if (RealAlive)
                                        {
                                            //Console.WriteLine("LastTimeDesync1:" + Math.Abs(LastTimeDesync - Math.Abs(CurrentTime - CurrentTime2)).ToString());
                                            if (SecondFound && Math.Abs(CurrentTime) > float.Epsilon &&
                                                Math.Abs(CurrentTime2) > float.Epsilon)
                                                if (CurrentFrameDuplicated == 0)
                                                {
                                                    TimeShiftCount += 1;
                                                    if (LastTimeOut != 2 && TimeShiftCount - LossPackets > 4 + ChokePackets && Math.Abs(CurrentTime - LastChokePacket) > 60)
                                                    {
                                                        DemoScanner_AddWarn(
                                                            "[TIMESHIFT 2] at (" + DemoStartTime + "):"
                                                            + "(" + CurrentTime + "):" + "(" + DemoStartTime2 + "):" + "(" + CurrentTime2 + "):" +
                                                            CurrentTimeString, false);
                                                        LastTimeOut = 2;
                                                    }
                                                }
                                            //Console.WriteLine("Second.");

                                            if (SecondFound2 && Math.Abs(CurrentTime) > float.Epsilon &&
                                                Math.Abs(CurrentTime2) > float.Epsilon)
                                                if (CurrentFrameDuplicated == 0)
                                                {
                                                    TimeShiftCount += 1;
                                                    if (LastTimeOut != 3 && TimeShiftCount - LossPackets > 4 + ChokePackets && Math.Abs(CurrentTime - LastChokePacket) > 60)
                                                    {
                                                        DemoScanner_AddWarn(
                                                            "[TIMESHIFT 3] at (" + DemoStartTime + "):"
                                                            + "(" + CurrentTime + "):" + "(" + DemoStartTime2 + "):" + "(" + CurrentTime2 + "):" +
                                                            CurrentTimeString, false);
                                                        LastTimeOut = 3;
                                                    }
                                                }

                                            //Console.WriteLine("Second2.");



                                        }

                                        LastTimeDesync = Math.Abs(CurrentTime - CurrentTime2);
                                    }
                                    else
                                    {
                                        LastTimeDesync = Math.Abs(CurrentTime - CurrentTime2);
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
                                        if (LerpBeforeStopAttack > LerpBeforeAttack)
                                            if (LerpAfterAttack > LerpBeforeAttack)
                                                if (CurrentFrameLerp == LerpBeforeAttack &&
                                                    RealAlive && FirstAttack)
                                                {
                                                    DemoScanner_AddWarn(
                                                        "[AIM TYPE 4 " + CurrentWeapon + "] at (" +
                                                        CurrentTime + "):" + CurrentTimeString, !IsPlayerLossConnection() && !IsChangeWeapon());
                                                    TotalAimBotDetected++;
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
                                else if (NeedSearchViewAnglesAfterAttack == 2 && IsAttack &&
                                    RealAlive)
                                {
                                    if (CurrentWeapon == WeaponIdType.WEAPON_KNIFE
                                        || CurrentWeapon == WeaponIdType.WEAPON_C4
                                        || CurrentWeapon == WeaponIdType.WEAPON_HEGRENADE
                                        || CurrentWeapon == WeaponIdType.WEAPON_SMOKEGRENADE
                                        || CurrentWeapon == WeaponIdType.WEAPON_FLASHBANG
                                        || CurrentWeapon == WeaponIdType.WEAPON_NONE
                                        || CurrentWeapon == WeaponIdType.WEAPON_BAD
                                        || CurrentWeapon == WeaponIdType.WEAPON_BAD2)
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
                                    if (NeedSearchViewAnglesAfterAttackNext && RealAlive &&
                                        IsAttack)
                                    {
                                        NeedSearchViewAnglesAfterAttackNext = false;
                                        ViewanglesXAfterAttackNext =
                                            CDFRAME_ViewAngles.X;
                                        ViewanglesYAfterAttackNext =
                                            CDFRAME_ViewAngles.Y;

                                        if (Math.Abs(ViewanglesXBeforeBeforeAttack - ViewanglesXAfterAttack) < float.Epsilon &&
                                            Math.Abs(ViewanglesYBeforeBeforeAttack - ViewanglesYAfterAttack) < float.Epsilon &&

                                            Math.Abs(ViewanglesXBeforeBeforeAttack - ViewanglesXAfterAttackNext) < float.Epsilon &&
                                            Math.Abs(ViewanglesYBeforeBeforeAttack - ViewanglesYAfterAttackNext) < float.Epsilon && Math.Abs(ViewanglesXBeforeBeforeAttack - ViewanglesXBeforeAttack) > float.Epsilon && Math.Abs(ViewanglesYBeforeBeforeAttack - ViewanglesYBeforeAttack) > float.Epsilon

                                        )
                                        {
                                            //DemoScanner_AddWarn(
                                            //      "Warn [AIM TYPE 3] at (" +
                                            //      CurrentTime + "):" + DemoScanner.CurrentTimeString + " (???)", false);
                                            //if (maxfalsepositiveaim3 > 0 &&
                                            //    SilentAimDetected <= 1 &&
                                            //    JumpHackCount <= 1)
                                            //{
                                            //    DemoScanner_AddWarn(
                                            //        "Detected [AIM TYPE 3] at (" +
                                            //        CurrentTime + "):" + DemoScanner.CurrentTimeString + " (???)", false);
                                            //    maxfalsepositiveaim3--;
                                            //}
                                            //else if (!IsTeleportus())
                                            //{
                                            //    DemoScanner_AddWarn(
                                            //        "Detected [AIM TYPE 3] at (" +
                                            //        CurrentTime + "):" + DemoScanner.CurrentTimeString);
                                            //    SilentAimDetected++;
                                            //}
                                            //else
                                            //{
                                            //    DemoScanner_AddWarn(
                                            //        "Detected [AIM TYPE 3] at (" +
                                            //        CurrentTime + "):" + DemoScanner.CurrentTimeString + " (???)", false);
                                            //}
                                        }
                                        else if (Math.Abs(ViewanglesXBeforeAttack -
                                                 ViewanglesXAfterAttack) > float.Epsilon &&
                                                 Math.Abs(ViewanglesYBeforeAttack -
                                                 ViewanglesYAfterAttack) > float.Epsilon &&
                                                 // ViewanglesXBeforeAttack == ViewanglesXAfterAttackNext &&
                                                 Math.Abs(ViewanglesYBeforeAttack -
                                                 ViewanglesYAfterAttackNext) < float.Epsilon
                                        )
                                        {
                                            //var tmpcol = Console.ForegroundColor;
                                            //Console.ForegroundColor = ConsoleColor.Gray;
                                            //DemoScanner.TextComments.WriteLine("Detected [AIM TYPE 3] at (" + CurrentTime + "):" + DemoScanner.CurrentTimeString + " (???)");
                                            //AddViewDemoHelperComment("Detected [AIM TYPE 3]. Weapon:" + CurrentWeapon.ToString() + " (???)", 0.5f);
                                            //Console.WriteLine("Detected [AIM TYPE 3] at (" + CurrentTime + "):" + DemoScanner.CurrentTimeString + " (???)");
                                            //Console.ForegroundColor = tmpcol;
                                        }
                                    }
                                    else
                                    {
                                        NeedSearchViewAnglesAfterAttackNext = false;
                                    }
                                }


                                if (Math.Abs(CurrentTime) > float.Epsilon && RealAlive)
                                {
                                    if (IsJump && FirstJump ||
                                        CurrentTime - LastUnJumpTime < 0.5f)
                                        JumpHackCount2 = 0;

                                    if (!CurrentFrameJumped && !FirstJump)
                                    {
                                        FirstJump = true;
                                        IsJump = false;
                                    }
                                    else
                                    {
                                        if (FirstJump && !IsJump)
                                            if (JumpHackCount2 > 0)
                                            {
                                                JumpHackCount2 = 0;
                                                if (Math.Abs(LastJumpHackFalseDetectionTime) > float.Epsilon && Math.Abs(CurrentTime - LastJumpHackFalseDetectionTime) > 5.0f) LastJumpHackFalseDetectionTime = 0.0f;
                                                if (Math.Abs(CurrentTime - LastUnJumpTime) > 1.5f &&
                                                    Math.Abs(CurrentTime - LastJumpTime) > 1.5f)
                                                {
                                                    if (Math.Abs(CurrentTime - LastJumpHackTime) > 2.5f && Math.Abs(LastJumpHackFalseDetectionTime) < float.Epsilon)
                                                    {
                                                        DemoScanner_AddWarn(
                                                            "[JUMPHACK TYPE 1] at (" +
                                                            CurrentTime + ") " + CurrentTimeString, !IsAngleEditByEngine());

                                                        LastJumpHackTime = CurrentTime;
                                                        JumpHackCount++;
                                                    }

                                                }
                                                else if (Math.Abs(CurrentTime - LastUnJumpTime) > 0.5f &&
                                                         Math.Abs(CurrentTime - LastJumpTime) > 0.5f)
                                                {
                                                    if (Math.Abs(CurrentTime - LastJumpHackTime) > 2.5f
                                                        && !IsAngleEditByEngine() && Math.Abs(LastJumpHackFalseDetectionTime) < float.Epsilon)
                                                    {
                                                        DemoScanner_AddWarn(
                                                            "[JUMPHACK TYPE 3] at (" +
                                                            CurrentTime + ") " + CurrentTimeString, false);

                                                        LastJumpHackTime = CurrentTime;
                                                        JumpHackCount++;
                                                    }
                                                }
                                                else if (Math.Abs(CurrentTime - LastUnJumpTime) > 0.3f &&
                                                         Math.Abs(CurrentTime - LastJumpTime) > 0.3f)
                                                {
                                                    if (Math.Abs(CurrentTime - LastJumpHackTime) > 2.5f
                                                        && !IsAngleEditByEngine() && Math.Abs(LastJumpHackFalseDetectionTime) < float.Epsilon)
                                                    {
                                                        DemoScanner_AddWarn(
                                                            "[JUMPHACK TYPE 4] at (" +
                                                            CurrentTime + ") " + CurrentTimeString, false, false);

                                                        LastJumpHackTime = CurrentTime;
                                                        JumpHackCount++;
                                                    }
                                                }
                                            }

                                        if (FirstJump && !IsJump && CurrentFrameJumped)
                                            if (Math.Abs(CurrentTime - LastUnJumpTime) > 0.5f &&
                                                Math.Abs(CurrentTime - LastJumpTime) > 0.5f)
                                                JumpHackCount2++;
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
                                        DemoScanner_AddInfo("Сменилось небо с \"" + KnownSkyName + "\" на \"" + nf.MVars.SkyName + "\" (" + CurrentTime +
                                                            "):" + CurrentTimeString);
                                    else
                                        DemoScanner_AddInfo("Player changed sky name from \"" + KnownSkyName + "\" to \"" + nf.MVars.SkyName + "\" at (" + CurrentTime +
                                                            "):" + CurrentTimeString);

                                    KnownSkyName = nf.MVars.SkyName;
                                }

                                if (DUMP_ALL_FRAMES)
                                {
                                    subnode.Text += "{\n";
                                    subnode.Text +=
                                        @"RParms.Time  = " + nf.RParms.Time + "(" + CurrentTimeString +
                                        ")\n";
                                    subnode.Text +=
                                        @"RParms.Vieworg.X  = " + nf.RParms.Vieworg.X + "\n";
                                    subnode.Text +=
                                        @"RParms.Vieworg.Y  = " + nf.RParms.Vieworg.Y + "\n";
                                    subnode.Text +=
                                        @"RParms.Vieworg.Z  = " + nf.RParms.Vieworg.Z + "\n";
                                    subnode.Text +=
                                        @"RParms.Viewangles.X  = " + nf.RParms.Viewangles.X +
                                        "\n";
                                    subnode.Text +=
                                        @"RParms.Viewangles.Y  = " + nf.RParms.Viewangles.Y +
                                        "\n";
                                    subnode.Text +=
                                        @"RParms.Viewangles.Z  = " + nf.RParms.Viewangles.Z +
                                        "\n";
                                    subnode.Text +=
                                        @"RParms.Forward.X  = " + nf.RParms.Forward.X + "\n";
                                    subnode.Text +=
                                        @"RParms.Forward.Y  = " + nf.RParms.Forward.Y + "\n";
                                    subnode.Text +=
                                        @"RParms.Forward.Z  = " + nf.RParms.Forward.Z + "\n";
                                    subnode.Text +=
                                        @"RParms.Right.X  = " + nf.RParms.Right.X + "\n";
                                    subnode.Text +=
                                        @"RParms.Right.Y  = " + nf.RParms.Right.Y + "\n";
                                    subnode.Text +=
                                        @"RParms.Right.Z  = " + nf.RParms.Right.Z + "\n";
                                    subnode.Text += @"RParms.Up.X  = " + nf.RParms.Up.X + "\n";
                                    subnode.Text += @"RParms.Up.Y  = " + nf.RParms.Up.Y + "\n";
                                    subnode.Text += @"RParms.Up.Z  = " + nf.RParms.Up.Z + "\n";
                                    subnode.Text +=
                                        @"RParms.Frametime  = " + nf.RParms.Frametime + "\n";
                                    subnode.Text +=
                                        @"RParms.Intermission  = " + nf.RParms.Intermission +
                                        "\n";
                                    subnode.Text +=
                                        @"RParms.Paused  = " + nf.RParms.Paused + "\n";
                                    subnode.Text +=
                                        @"RParms.Spectator  = " + nf.RParms.Spectator + "\n";
                                    subnode.Text +=
                                        @"RParms.Onground  = " + nf.RParms.Onground + "\n";
                                }

                                if (nf.RParms.Intermission != 0)
                                    // Console.WriteLine("Intermiss");
                                    Intermission = true;

                                if (nf.RParms.Frametime > float.Epsilon) averagefps2.Add(1000.0f / (1000.0f * nf.RParms.Frametime));

                                if (DUMP_ALL_FRAMES)
                                {
                                    subnode.Text +=
                                        @"RParms.Waterlevel  = " + nf.RParms.Waterlevel + "\n";
                                    subnode.Text +=
                                        @"RParms.Simvel.X  = " + nf.RParms.Simvel.X + "\n";
                                    subnode.Text +=
                                        @"RParms.Simvel.Y  = " + nf.RParms.Simvel.Y + "\n";
                                    subnode.Text +=
                                        @"RParms.Simvel.Z  = " + nf.RParms.Simvel.Z + "\n";
                                    subnode.Text +=
                                        @"RParms.Simorg.X  = " + nf.RParms.Simorg.X + "\n";
                                    subnode.Text +=
                                        @"RParms.Simorg.Y  = " + nf.RParms.Simorg.Y + "\n";
                                    subnode.Text +=
                                        @"RParms.Simorg.Z  = " + nf.RParms.Simorg.Z + "\n";
                                    subnode.Text +=
                                        @"RParms.Viewheight.X  = " + nf.RParms.Viewheight.X +
                                        "\n";
                                    subnode.Text +=
                                        @"RParms.Viewheight.Y  = " + nf.RParms.Viewheight.Y +
                                        "\n";
                                    subnode.Text +=
                                        @"RParms.Viewheight.Z  = " + nf.RParms.Viewheight.Z +
                                        "\n";
                                    subnode.Text +=
                                        @"RParms.Idealpitch  = " + nf.RParms.Idealpitch + "\n";
                                    subnode.Text +=
                                        @"RParms.ClViewangles.X  = " +
                                        nf.RParms.ClViewangles.X + "\n";
                                    subnode.Text +=
                                        @"RParms.ClViewangles.Y  = " +
                                        nf.RParms.ClViewangles.Y + "\n";
                                    subnode.Text +=
                                        @"RParms.ClViewangles.Z  = " +
                                        nf.RParms.ClViewangles.Z + "\n";
                                    subnode.Text +=
                                        @"RParms.Health  = " + nf.RParms.Health + "\n";

                                    subnode.Text +=
                                        @"RParms.Crosshairangle.X  = " +
                                        nf.RParms.Crosshairangle.X + "\n";
                                    subnode.Text +=
                                        @"RParms.Crosshairangle.Y  = " +
                                        nf.RParms.Crosshairangle.Y + "\n";
                                    subnode.Text +=
                                        @"RParms.Crosshairangle.Z  = " +
                                        nf.RParms.Crosshairangle.Z + "\n";
                                    subnode.Text +=
                                        @"RParms.Viewsize  = " + nf.RParms.Viewsize + "\n";
                                    subnode.Text +=
                                        @"RParms.Punchangle.X  = " + nf.RParms.Punchangle.X +
                                        "\n";
                                    subnode.Text +=
                                        @"RParms.Punchangle.Y  = " + nf.RParms.Punchangle.Y +
                                        "\n";
                                    subnode.Text +=
                                        @"RParms.Punchangle.Z  = " + nf.RParms.Punchangle.Z +
                                        "\n";
                                    subnode.Text +=
                                        @"RParms.Maxclients  = " + nf.RParms.Maxclients + "\n";
                                    subnode.Text +=
                                        @"RParms.Viewentity  = " + nf.RParms.Viewentity + "\n";
                                    subnode.Text +=
                                        @"RParms.Playernum  = " + nf.RParms.Playernum + "\n";
                                }

                                //if (UserAlive)
                                {
                                    UserId = nf.RParms.Playernum;
                                    UserId2 = nf.RParms.Viewentity - 1;
                                }

                                ViewModel = nf.Viewmodel;

                                if (!UserAlive)
                                    if (ViewEntity == nf.RParms.Viewentity)
                                    {
                                        if (!Intermission && FirstUserAlive && nf.Viewmodel != 0)
                                            NeedSearchUserAliveTime = CurrentTime;
                                        // Console.WriteLine("Alive 4");
                                        Intermission = false;
                                        ViewEntity = -1;
                                    }


                                //if (DemoScanner.NeedSearchUserAliveTime != 0.0 && CurrentTime - DemoScanner.NeedSearchUserAliveTime < 5.0)
                                //{
                                //    if ((CurrentFrameForward && InForward) || (CurrentFrameDuck && IsDuck))
                                //    {
                                //        if (DemoScanner.DEBUG_ENABLED)
                                //            Console.WriteLine("Alive 4 at " + CurrentTimeString);
                                //        UserAlive = true;
                                //        FirstUserAlive = false;
                                //        LastAliveTime = CurrentTime;
                                //    }
                                //}
                                //else
                                //{
                                //    DemoScanner.NeedSearchUserAliveTime = 0.0f;
                                //}

                                if (UserAlive && Math.Abs(CurrentTime) > float.Epsilon)
                                    if (UserId != UserId2 && !DemoScannerBypassDetected &&
                                       Math.Abs(CurrentTime - LastUseTime) > 60)
                                    {
                                        DemoScannerBypassDetected = true;
                                        DemoScanner_AddWarn("ERROR [DEMO SCANER BYPASS] ??? VERY STRANGE ISSUE AT " + CurrentTimeString, false, false);
                                    }

                                if (ForceUpdateName || Math.Abs(LastUsernameCheckTime) < float.Epsilon
                                                    || Math.Abs(CurrentTime - LastUsernameCheckTime) > 60.0f || LastUername == "\tNO NAME")
                                {
                                    ForceUpdateName = false;
                                    string plname = "\tNO NAME";
                                    string plsteam = "NOSTEAM";
                                    foreach (Player player in fullPlayerList)
                                    {
                                        if (player.Name.Length == 0) continue;

                                        if (player.Slot == UserId)
                                            if ("NOSTEAM" != player.Steam || NeedFirstNickname)
                                            {
                                                NeedFirstNickname = false;
                                                plname = player.Name;
                                                plsteam = player.Steam;
                                            }
                                    }
                                    if ((LastUername.Length == 0
                                        || plname.IndexOf(LastUername) == -1 && LastUername.IndexOf(plname) == -1
                                        )
                                        &&
                                        plsteam != "NOSTEAM" && plname != "\tNO NAME")
                                    {
                                        int tmpcursortop = Console.CursorTop;
                                        int tmpcursorleft = Console.CursorLeft;
                                        Console.CursorTop = UserNameAndSteamIDField;
                                        Console.CursorLeft = UserNameAndSteamIDField2;
                                        ConsoleColor tmpconsolecolor = Console.ForegroundColor;
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        for (int i = 0; i < 64; i++) Console.Write(" ");
                                        Console.CursorLeft = UserNameAndSteamIDField2;
                                        Console.Write(plname.TrimBad().Trim());
                                        Console.ForegroundColor = ConsoleColor.Cyan;
                                        Console.WriteLine(" [" + plsteam + "]");
                                        Console.ForegroundColor = tmpconsolecolor;
                                        Console.CursorTop = tmpcursortop;
                                        Console.CursorLeft = tmpcursorleft;
                                        LastUername = plname;
                                    }
                                    LastUsernameCheckTime = CurrentTime;
                                }

                                AddResolution(nf.RParms.Viewport.Z, nf.RParms.Viewport.W);

                                if (DUMP_ALL_FRAMES)
                                {
                                    subnode.Text +=
                                        @"RParms.MaxEntities  = " + nf.RParms.MaxEntities +
                                        "\n";
                                    subnode.Text +=
                                        @"RParms.Demoplayback  = " + nf.RParms.Demoplayback +
                                        "\n";
                                    subnode.Text +=
                                        @"RParms.Hardware  = " + nf.RParms.Hardware + "\n";
                                    subnode.Text +=
                                        @"RParms.Smoothing  = " + nf.RParms.Smoothing + "\n";
                                    subnode.Text +=
                                        @"RParms.PtrCmd  = " + nf.RParms.PtrCmd + "\n";
                                    subnode.Text +=
                                        @"RParms.PtrMovevars  = " + nf.RParms.PtrMovevars +
                                        "\n";
                                    subnode.Text +=
                                        @"RParms.Viewport.X  = " + nf.RParms.Viewport.X + "\n";
                                    subnode.Text +=
                                        @"RParms.Viewport.Y  = " + nf.RParms.Viewport.Y + "\n";
                                    subnode.Text +=
                                        @"RParms.Viewport.Z  = " + nf.RParms.Viewport.Z + "\n";
                                    subnode.Text +=
                                        @"RParms.Viewport.W  = " + nf.RParms.Viewport.W + "\n";
                                    subnode.Text +=
                                        @"RParms.NextView  = " + nf.RParms.NextView + "\n";
                                    subnode.Text +=
                                        @"RParms.OnlyClientDraw  = " +
                                        nf.RParms.OnlyClientDraw + "\n";
                                    subnode.Text +=
                                        @"UCmd.LerpMsec  = " + nf.UCmd.LerpMsec + "\n";
                                    subnode.Text += @"UCmd.Msec  = " + nf.UCmd.Msec + "\n";
                                    subnode.Text += @"UCmd.Align1  = " + nf.UCmd.Align1 + "\n";
                                    subnode.Text +=
                                        @"UCmd.Forwardmove  = " + nf.UCmd.Forwardmove + "\n";
                                    subnode.Text +=
                                        @"UCmd.Sidemove  = " + nf.UCmd.Sidemove + "\n";
                                    subnode.Text += @"UCmd.Upmove  = " + nf.UCmd.Upmove + "\n";
                                    subnode.Text +=
                                        @"UCmd.Lightlevel  = " + nf.UCmd.Lightlevel + "\n";
                                    subnode.Text += @"UCmd.Align2  = " + nf.UCmd.Align2 + "\n";
                                    subnode.Text +=
                                        @"UCmd.Buttons  = " + nf.UCmd.Buttons + "\n";
                                }

                                if (IsUserAlive() && FirstJump && Math.Abs(CurrentTime) > float.Epsilon)
                                {
                                    if (nf.UCmd.Msec == 0 && nf.RParms.Frametime > float.Epsilon)
                                    {
                                        if (Math.Abs(CurrentTime - LastCmdHack) > 5.0)
                                            DemoScanner_AddWarn(
                                                "[CMD HACK TYPE 1] at (" +
                                                CurrentTime + ") " + CurrentTimeString, !IsAngleEditByEngine());
                                        LastCmdHack = CurrentTime;
                                    }
                                    else if (nf.UCmd.Msec / nf.RParms.Frametime < 500.0f)
                                    {
                                        if (Math.Abs(CurrentTime - LastCmdHack) > 5.0)
                                            DemoScanner_AddWarn(
                                                "[CMD HACK TYPE 2] at (" +
                                                CurrentTime + ") " + CurrentTimeString, !IsAngleEditByEngine());
                                        //Console.WriteLine("BAD BAD " + nf.UCmd.Msec + " / " + nf.RParms.Frametime + " = " + ((float)nf.UCmd.Msec / nf.RParms.Frametime).ToString());
                                        LastCmdHack = CurrentTime;
                                    }
                                }

                                /*
                                 * 1. Игрок жмет +attack
                                 * 2. Если во втором кадре нет IN_ATTACK значит аим
                                 * 3. Но если IN_ATTACK нет вообще, до -attack, значит выбор слота
                                 */


                                /*
                                 * 1. Игрок жмет +attack
                                 * 2. Поиск IN_ATTACK
                                 * 3. ЕСЛИ IN_ATTACK пропала до -attack аим2
                                 */
                                if (ShotFound > 0 && !IsReload && IsAttack &&
                                    RealAlive && SelectSlot <= 0)
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
                                        if (CurrentWeapon == WeaponIdType.WEAPON_KNIFE
                                            || CurrentWeapon == WeaponIdType.WEAPON_C4
                                            || CurrentWeapon == WeaponIdType.WEAPON_HEGRENADE
                                            || CurrentWeapon == WeaponIdType.WEAPON_SMOKEGRENADE
                                            || CurrentWeapon == WeaponIdType.WEAPON_FLASHBANG)
                                        {
                                        }
                                        else
                                        {
                                            if (ReallyAim2 == 2 && TotalAimBotDetected < 2 &&
                                                BadAttackCount < 2)
                                            {
                                                /*  if (AUTO_LEARN_HACK_DB)
                                                  {
                                                      ENABLE_LEARN_HACK_DEMO = true;
                                                      ENABLE_LEARN_HACK_DEMO_FORCE_SAVE = true;
                                                  }*/
                                                DemoScanner_AddWarn(
                                                    "[AIM TYPE 2 " + CurrentWeapon + "] at (" + CurrentTime +
                                                    "):" + CurrentTimeString, false);
                                            }
                                            else
                                            {
                                                /* if (AUTO_LEARN_HACK_DB)
                                                 {
                                                     ENABLE_LEARN_HACK_DEMO = true;
                                                     ENABLE_LEARN_HACK_DEMO_FORCE_SAVE = true;
                                                 }*/
                                                DemoScanner_AddWarn(
                                                    "[AIM TYPE 2 " + CurrentWeapon + "] at (" + CurrentTime +
                                                    "):" + CurrentTimeString, !IsPlayerLossConnection() && !IsChangeWeapon() && !IsForceCenterView() && !IsAngleEditByEngine());
                                                if (!IsPlayerLossConnection() && !IsChangeWeapon() && !IsAngleEditByEngine()) TotalAimBotDetected++;
                                            }

                                            //FirstAttack = false;
                                        }
                                    }

                                    ReallyAim2 = 0;
                                }

                                if (!IsAttack && NeedIgnoreAttackFlag == 0)
                                {
                                    if (!BadAttackFound && CurrentFrameAttacked && FirstAttack)
                                    {
                                        if (CurrentWeapon == WeaponIdType.WEAPON_KNIFE
                                            || CurrentWeapon == WeaponIdType.WEAPON_C4
                                            || CurrentWeapon == WeaponIdType.WEAPON_HEGRENADE
                                            || CurrentWeapon == WeaponIdType.WEAPON_SMOKEGRENADE
                                            || CurrentWeapon == WeaponIdType.WEAPON_FLASHBANG)
                                        {
                                            //  Console.WriteLine("Invalid weapon(" + DemoScanner.CurrentTime + ") " + DemoScanner.CurrentTimeString);
                                        }
                                        else if (SelectSlot > 0)
                                        {
                                            //  Console.WriteLine("Select weapon(" + DemoScanner.CurrentTime + ") " + DemoScanner.CurrentTimeString);
                                        }
                                        else if (attackscounter3 < 2)
                                        {
                                            //  Console.WriteLine("Select weapon(" + DemoScanner.CurrentTime + ") " + DemoScanner.CurrentTimeString);
                                        }
                                        else if (Math.Abs(CurrentTime2 - IsNoAttackLastTime2) < 0.1f)
                                        {
                                            //   Console.WriteLine("No attack in current frame(" + DemoScanner.CurrentTime + ") " + DemoScanner.CurrentTimeString);
                                        }
                                        else if (IsReload)
                                        {
                                            //  Console.WriteLine("Reload weapon(" + DemoScanner.CurrentTime + ") " + DemoScanner.CurrentTimeString);
                                        }
                                        else if (!RealAlive)
                                        {
                                            //  Console.WriteLine("Dead (" + DemoScanner.CurrentTime + ") " + DemoScanner.CurrentTimeString);
                                        }
                                        else
                                        {
                                            /* if (AUTO_LEARN_HACK_DB)
                                             {
                                                 ENABLE_LEARN_HACK_DEMO = true;
                                                 ENABLE_LEARN_HACK_DEMO_FORCE_SAVE = true;
                                             }*/
                                            DemoScanner_AddWarn(
                                                "[TRIGGER TYPE 1 " + CurrentWeapon + "] at (" + CurrentTime +
                                                ") " + CurrentTimeString, !IsChangeWeapon() && !IsAngleEditByEngine());

                                            LastTriggerCursor = Console.CursorTop;

                                            BadAttackCount++;
                                            LastBadAttackCount = CurrentTime;
                                            //FirstAttack = false;
                                            BadAttackFound = true;
                                        }
                                    }
                                }
                                else
                                {
                                    if (!CurrentFrameAttacked) BadAttackFound = false;
                                }

                                if (NeedSearchAim2 && !IsReload && RealAlive &&
                                    SelectSlot <= 0)
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

                                if (NeedWriteAim && CurrentFrameAttacked)
                                {
                                    NeedWriteAim = false;
                                    if (AimType1FalseDetect)
                                    {
                                        AimType1FalseDetect = false;
                                        if (FirstAttack)
                                        {
                                            if ((TotalAimBotDetected > 0 || JumpHackCount > 0) && !IsAngleEditByEngine())
                                            {
                                                /*  if (AUTO_LEARN_HACK_DB)
                                                  {
                                                      ENABLE_LEARN_HACK_DEMO = true;
                                                      ENABLE_LEARN_HACK_DEMO_FORCE_SAVE = true;
                                                  }*/
                                                DemoScanner_AddWarn(
                                                    "[AIM TYPE 1.2 " + CurrentWeapon + "] at (" + CurrentTime +
                                                    "):" + CurrentTimeString, !IsChangeWeapon() && !IsPlayerLossConnection() && !IsForceCenterView());
                                                TotalAimBotDetected++;
                                            }
                                            else
                                            {
                                                /*if (AUTO_LEARN_HACK_DB)
                                                {
                                                    ENABLE_LEARN_HACK_DEMO = true;
                                                    ENABLE_LEARN_HACK_DEMO_FORCE_SAVE = true;
                                                }*/
                                                DemoScanner_AddWarn(
                                                     "[AIM TYPE 1.3 " + CurrentWeapon + "] at (" + CurrentTime +
                                                     "):" + CurrentTimeString, false, false);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (FirstAttack)
                                        {
                                            /* if (AUTO_LEARN_HACK_DB)
                                             {
                                                 ENABLE_LEARN_HACK_DEMO = true;
                                                 ENABLE_LEARN_HACK_DEMO_FORCE_SAVE = true;
                                             }*/
                                            DemoScanner_AddWarn(
                                                "[AIM TYPE 1 " + CurrentWeapon + "] at (" + CurrentTime +
                                                "):" + CurrentTimeString, !IsChangeWeapon() && !IsPlayerLossConnection() && !IsForceCenterView() && !IsAngleEditByEngine());
                                            if (!IsAngleEditByEngine() && !IsChangeWeapon() && !IsPlayerLossConnection() && !IsForceCenterView() && !IsAngleEditByEngine())
                                                TotalAimBotDetected++;
                                        }
                                    }

                                    LastSilentAim = CurrentTime;
                                }

                                if (AttackCheck > -1)
                                {
                                    if (!CurrentFrameAttacked && !IsReload && SelectSlot <= 0 &&
                                        IsAttack && RealAlive && IsRealWeapon() &&
                                        CurrentWeapon != WeaponIdType.WEAPON_KNIFE
                                        && CurrentWeapon != WeaponIdType.WEAPON_C4 &&
                                        CurrentWeapon != WeaponIdType.WEAPON_HEGRENADE &&
                                        CurrentWeapon != WeaponIdType.WEAPON_SMOKEGRENADE
                                        && CurrentWeapon != WeaponIdType.WEAPON_FLASHBANG)
                                    {

                                        if (DEBUG_ENABLED) Console.WriteLine("Check attack:" + AttackCheck + " " + SkipNextAttack + " " + nf.UCmd.Buttons + " " + CurrentTime);

                                        if (AttackCheck > 0)
                                        {
                                            AttackCheck--;
                                        }
                                        else
                                        {
                                            if (SkipNextAttack <= 0)
                                            {
                                                //File.AppendAllText("bug.txt", "NeedWriteAim" + CurrentTime + " " + IsAttackLastTime + "\n");
                                                if (DEBUG_ENABLED) Console.WriteLine("Aim detected?... Teleport:" + IsAngleEditByEngine() + ". Alive:" + IsUserAlive());
                                                if (!IsAngleEditByEngine() && CurrentFrameDuplicated == 0)
                                                {
                                                    NeedWriteAim = true;
                                                    AttackCheck = -1;
                                                }
                                                AttackCheck--;
                                            }
                                            if (CurrentFrameDuplicated == 0) SkipNextAttack -= 1;
                                        }
                                    }
                                    else
                                    {
                                        NeedWriteAim = false;
                                        AttackCheck = -1;
                                        if (DEBUG_ENABLED) Console.WriteLine("Stop search aim type 1.");
                                        //if (AttackCheck != -2)
                                        //{
                                        //    //if (DemoScanner.SelectSlot > 0)
                                        //    //    Console.WriteLine("select slot...");
                                        //    //if (!DemoScanner.IsAttack)
                                        //    //    Console.WriteLine("not in attack...");
                                        //    //if (!DemoScanner.UserAlive)
                                        //    //    Console.WriteLine("user invalid blya...");
                                        //    //if (IsReload)
                                        //    //    Console.WriteLine("in reload...");
                                        //    //if ((nf.UCmd.Buttons & 1) > 0)
                                        //    //    Console.WriteLine("not attack...");
                                        //    //Console.WriteLine("Weapon:" + CurrentWeapon.ToString());
                                        //    //Console.WriteLine("Don't check attack!");
                                        //    AttackCheck = -2;
                                        //}
                                    }
                                }

                                if (DUMP_ALL_FRAMES)
                                {
                                    subnode.Text +=
                                        @"UCmd.Impulse  = " + nf.UCmd.Impulse + "\n";
                                    subnode.Text +=
                                        @"UCmd.Weaponselect  = " + nf.UCmd.Weaponselect + "\n";
                                    subnode.Text += @"UCmd.Align3  = " + nf.UCmd.Align3 + "\n";
                                    subnode.Text += @"UCmd.Align4  = " + nf.UCmd.Align4 + "\n";
                                    subnode.Text +=
                                        @"UCmd.ImpactIndex  = " + nf.UCmd.ImpactIndex + "\n";
                                    subnode.Text +=
                                        @"UCmd.ImpactPosition.X  = " +
                                        nf.UCmd.ImpactPosition.X + "\n";
                                    subnode.Text +=
                                        @"UCmd.ImpactPosition.Y  = " +
                                        nf.UCmd.ImpactPosition.Y + "\n";
                                    subnode.Text +=
                                        @"UCmd.ImpactPosition.Z  = " +
                                        nf.UCmd.ImpactPosition.Z + "\n";
                                    subnode.Text +=
                                        @"MVars.Gravity  = " + nf.MVars.Gravity + "\n";
                                    subnode.Text +=
                                        @"MVars.Stopspeed  = " + nf.MVars.Stopspeed + "\n";
                                    subnode.Text +=
                                        @"MVars.Maxspeed  = " + nf.MVars.Maxspeed + "\n";
                                    subnode.Text +=
                                        @"MVars.Spectatormaxspeed  = " +
                                        nf.MVars.Spectatormaxspeed + "\n";
                                    subnode.Text +=
                                        @"MVars.Accelerate  = " + nf.MVars.Accelerate + "\n";
                                    subnode.Text +=
                                        @"MVars.Airaccelerate  = " + nf.MVars.Airaccelerate +
                                        "\n";
                                    subnode.Text +=
                                        @"MVars.Wateraccelerate  = " +
                                        nf.MVars.Wateraccelerate + "\n";
                                    subnode.Text +=
                                        @"MVars.Friction  = " + nf.MVars.Friction + "\n";
                                    subnode.Text +=
                                        @"MVars.Edgefriction  = " + nf.MVars.Edgefriction +
                                        "\n";
                                    subnode.Text +=
                                        @"MVars.Waterfriction  = " + nf.MVars.Waterfriction +
                                        "\n";
                                    subnode.Text +=
                                        @"MVars.Entgravity  = " + nf.MVars.Entgravity + "\n";
                                    subnode.Text +=
                                        @"MVars.Bounce  = " + nf.MVars.Bounce + "\n";
                                    subnode.Text +=
                                        @"MVars.Stepsize  = " + nf.MVars.Stepsize + "\n";
                                    subnode.Text +=
                                        @"MVars.Maxvelocity  = " + nf.MVars.Maxvelocity + "\n";
                                    subnode.Text += @"MVars.Zmax  = " + nf.MVars.Zmax + "\n";
                                    subnode.Text +=
                                        @"MVars.WaveHeight  = " + nf.MVars.WaveHeight + "\n";
                                    subnode.Text +=
                                        @"MVars.Footsteps  = " + nf.MVars.Footsteps + "\n";
                                    subnode.Text +=
                                        @"MVars.SkyName  = " + nf.MVars.SkyName + "\n";
                                    subnode.Text +=
                                        @"MVars.Rollangle  = " + nf.MVars.Rollangle + "\n";
                                    subnode.Text +=
                                        @"MVars.Rollspeed  = " + nf.MVars.Rollspeed + "\n";
                                    subnode.Text +=
                                        @"MVars.SkycolorR  = " + nf.MVars.SkycolorR + "\n";
                                    subnode.Text +=
                                        @"MVars.SkycolorG  = " + nf.MVars.SkycolorG + "\n";
                                    subnode.Text +=
                                        @"MVars.SkycolorB  = " + nf.MVars.SkycolorB + "\n";
                                    subnode.Text +=
                                        @"MVars.SkyvecX  = " + nf.MVars.SkyvecX + "\n";
                                    subnode.Text +=
                                        @"MVars.SkyvecY  = " + nf.MVars.SkyvecY + "\n";
                                    subnode.Text +=
                                        @"MVars.SkyvecZ  = " + nf.MVars.SkyvecZ + "\n";
                                    subnode.Text += @"View.X  = " + nf.View.X + "\n";
                                    subnode.Text += @"View.Y  = " + nf.View.Y + "\n";
                                    subnode.Text += @"View.Z  = " + nf.View.Z + "\n";
                                    subnode.Text += @"Viewmodel  = " + nf.Viewmodel + "\n";
                                    subnode.Text +=
                                        @"IncomingSequence  = " + nf.IncomingSequence + "\n";
                                    subnode.Text +=
                                        @"IncomingAcknowledged  = " + nf.IncomingAcknowledged +
                                        "\n";
                                    subnode.Text +=
                                        @"IncomingReliableAcknowledged  = " +
                                        nf.IncomingReliableAcknowledged + "\n";
                                    subnode.Text +=
                                        @"IncomingReliableSequence  = " +
                                        nf.IncomingReliableSequence + "\n";
                                    subnode.Text +=
                                        @"OutgoingSequence  = " + nf.OutgoingSequence + "\n";
                                    subnode.Text +=
                                        @"ReliableSequence  = " + nf.ReliableSequence + "\n";
                                    subnode.Text +=
                                        @"LastReliableSequence = " + nf.LastReliableSequence +
                                        "\n";
                                }
                                //subnode.Text += @"msg = " + nf.Msg + "\n";

                                if (NeedSearchCMDHACK4 && Math.Abs(CurrentTime) > 0 && (FirstAttack || FirstJump) && !NewDirectory)
                                {
                                    if (LastIncomingSequence > 0 && nf.IncomingSequence - LastIncomingSequence > LastLossPacketCount + 1
                                        && nf.IncomingSequence - LastIncomingSequence > 5 && nf.OutgoingSequence - LastOutgoingSequence > 4 && CurrentFrameDuplicated == 0)
                                    {
                                        if (FrameErrors > 0 && IsUserAlive())
                                        {
                                            if (Math.Abs(CurrentTime - LastCmdHack) > 3.0)
                                                DemoScanner_AddWarn(
                                                    "[CMD HACK TYPE 4] at (" +
                                                    CurrentTime + ") " + CurrentTimeString, false);
                                            // Console.WriteLine("BAD BAD " + nf.UCmd.Msec + " / " + nf.RParms.Frametime + " = " + ((float)nf.UCmd.Msec / nf.RParms.Frametime).ToString() + " / " + (nf.IncomingSequence - LastIncomingSequence) + " / " + (nf.OutgoingSequence - LastOutgoingSequence));
                                            LastCmdHack = CurrentTime;
                                            NeedSearchCMDHACK4 = false;
                                        }
                                        FrameErrors++;
                                    }

                                    if (LastIncomingSequence > 0 && Math.Abs(nf.IncomingSequence - LastIncomingSequence) > maxLastIncomingSequence)
                                        maxLastIncomingSequence = Math.Abs(nf.IncomingSequence - LastIncomingSequence);
                                    // Console.WriteLine(nf.IncomingSequence + " / " + DemoScanner.LastIncomingSequence + " = " + DemoScanner.maxLastIncomingSequence + " - > " + CurrentTime);
                                    LastIncomingSequence = nf.IncomingSequence;


                                    if (LastIncomingAcknowledged > 0 && Math.Abs(nf.IncomingAcknowledged - LastIncomingAcknowledged) > LastIncomingAcknowledged) maxLastIncomingAcknowledged = Math.Abs(nf.IncomingAcknowledged - LastIncomingAcknowledged);
                                    LastIncomingAcknowledged = nf.IncomingAcknowledged;


                                    if (LastOutgoingSequence > 0 && Math.Abs(nf.OutgoingSequence - LastOutgoingSequence) > maxLastOutgoingSequence) maxLastOutgoingSequence = Math.Abs(nf.OutgoingSequence - LastOutgoingSequence);
                                    LastOutgoingSequence = nf.OutgoingSequence;
                                }
                                else
                                {
                                    FrameErrors = LastOutgoingSequence = LastIncomingAcknowledged = LastIncomingSequence = 0;
                                }


                                if (DUMP_ALL_FRAMES)
                                {
                                    subnode.Text += OutDumpString;
                                    OutDumpString = "";
                                    subnode.Text += 1 / nf.RParms.Frametime + @" FPS";


                                    subnode.Text += "}\n";
                                }

                                PreviousFrameAlive = CurrentFrameAlive;
                                PreviousFrameAttacked = CurrentFrameAttacked;
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
                    if (subnode.Text.Length > 0) node.Nodes.Add(subnode);

                    entrynode.Nodes.Add(node);
                }

                PrintNodesRecursive(entrynode);
            }

            try
            {
                if (File.Exists(CurrentDir + @"\Frames.txt") || DUMP_ALL_FRAMES) File.WriteAllLines(CurrentDir + @"\Frames.txt", outFrames.ToArray());
            }
            catch
            {
                Console.WriteLine("Error access write frame log!");
            }

            if (LastCmd == "-strafe")
            {
                MINIMIZED = true;
                if (IsRussia)
                    DemoScanner_AddInfo("Игрок свернул игру с " + LastAltTabStart + " и до конца игры.");
                else
                    DemoScanner_AddInfo("Player minimized game from " + LastAltTabStart + " to \"FINAL\"");
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
                    DemoScanner_AddInfo("Возможны ложные срабатывания на [AIM TYPE 1.6].", true);
                }
                else
                {
                    DemoScanner_AddInfo("AMXX Plugin not found. Some detects is skipped.", true);
                    DemoScanner_AddInfo("Possible false detect one of aimbots: [AIM TYPE 1.6].", true);
                }

            }
            else
            {
                if (JumpCount3 > 20 && JumpCount6 <= 1)
                    DemoScanner_AddWarn("[UNKNOWN BHOP/JUMPHACK] in demo file!", true, true, !DisableJump5AndAim16, true);
                if (attackscounter3 > 20 && attackscounter2 <= 1)
                    DemoScanner_AddWarn("[UNKNOWN AIM/TRIGGER HACK] in demo file!", true, true, !DisableJump5AndAim16, true);
            }



            ForceFlushScanResults();

            Console.ForegroundColor = ConsoleColor.DarkGreen;

            if (IsRussia)
                Console.WriteLine("Unreal Demo Scanner [ " + PROGRAMVERSION + " ] результаты анализа:");
            else
                Console.WriteLine("Unreal Demo Scanner [ " + PROGRAMVERSION + " ] scan result:");


            // Console.WriteLine("MAX BETWEENS: " + DemoScanner.maxLastIncomingSequence + "/ " + DemoScanner.maxLastIncomingAcknowledged + "/ " + DemoScanner.maxLastOutgoingSequence);

            //Console.WriteLine(AngleLenMaxX);
            //Console.WriteLine(AngleLenMaxY);
            //Console.WriteLine(nospreadtest.ToString("F8"));
            //Console.WriteLine(nospreadtest2.ToString("F8"));

            if (NeedIgnoreAttackFlagCount > 0)
            {
                if (IsRussia)
                    DemoScanner_AddInfo("Незавршенных атак: " + NeedIgnoreAttackFlagCount + ".");
                else
                    DemoScanner_AddInfo("Lost attack flag: " + NeedIgnoreAttackFlagCount + ".");
            }

            if (BadEvents > 8)
            {
                if (IsRussia)
                    DemoScanner_AddInfo("Нехватает кадров: " + BadEvents + ".");
                else
                    DemoScanner_AddInfo("Lost frames: " + BadEvents + ".");
            }


            /*
            if (BadAnglesFoundCount > 0)
            {
                DemoScanner.DemoScanner_AddInfo("Found strange angles issue: " + BadAnglesFoundCount + ".");
            }*/

            Console.ForegroundColor = ConsoleColor.DarkRed;

            if (FoundForceCenterView > 0)
            {

                if (IsRussia)
                    DemoScanner_AddInfo("Использование запрещенной команды force_centerview: " + FoundForceCenterView + " " + (FoundForceCenterView > 50 ? "\n. (Подозрительно. Проверьте демку вручную.)" : ""));
                else
                    DemoScanner_AddInfo("Used illegal force_centerview commands: " + FoundForceCenterView + " " + (FoundForceCenterView > 50 ? "\n. (Check demo manually for possible demoscanner bypass)" : ""));
            }

            if (ModifiedDemoFrames > 0)
            {
                if (IsRussia)
                    DemoScanner_AddInfo("Обнаружены проблемы с кадрами, возможно они были изменены:" + ModifiedDemoFrames);
                else
                    DemoScanner_AddInfo("Possible demoscanner bypass. Tried to kill frames:" + ModifiedDemoFrames);
            }

            if (WarnsAfterGameEnd > 8)
            {
                if (IsRussia)
                    DemoScanner_AddInfo("Есть предупреждения после конца игры: " + WarnsAfterGameEnd);
                else
                    DemoScanner_AddInfo("Possible demoscanner bypass. Detects after game end: " + WarnsAfterGameEnd);
            }

            //if (AttackErrors > 0)
            //{
            //    TextComments.WriteLine("Detected [UNKNOWN CONFIG] для атаки. Detect count:" + AttackErrors);
            //    Console.WriteLine("Detected [UNKNOWN CONFIG] для атаки. Detect count:" + AttackErrors);
            //    Console.WriteLine("Last using at " + LastAttackHack + " second game time.");
            //}


            if (MouseJumps > 10)
            {
                if (IsRussia)
                {
                    TextComments.WriteLine("Обнаружен бинд прыжка на колесо мыши. Количество прыжков:" + MouseJumps);
                    Console.WriteLine("Обнаружен бинд прыжка на колесо мыши. Количество прыжков:" + MouseJumps);
                }
                else
                {
                    TextComments.WriteLine("Detected [MOUSE JUMP] bind. Detect count:" + MouseJumps);
                    Console.WriteLine("Detected [MOUSE JUMP] bind. Detect count:" + MouseJumps);
                }
            }

            if (JumpWithAlias > 15 && MouseJumps > 15)
            {
                if (IsRussia)
                {
                    TextComments.WriteLine("Обнаружен алиас \"+jump;wait;-jump; like alias\". Количество:" + JumpWithAlias);
                    Console.WriteLine("Обнаружен алиас \"+jump;wait;-jump; like alias\". Количество:" + JumpWithAlias);
                    if (MouseJumps > JumpWithAlias && MouseJumps > 0)
                        Console.WriteLine("Вероятность использования: " + Math.Round(Convert.ToSingle(JumpWithAlias) / Convert.ToSingle(MouseJumps) * 100.0f, 1) + "%");
                    else
                        Console.WriteLine("Высокая вероятность использования.");
                }
                else
                {
                    TextComments.WriteLine("Detected \"+jump;wait;-jump; like alias\". Detect count:" + JumpWithAlias);
                    Console.WriteLine("Detected \"+jump;wait;-jump; like alias\". Detect count:" + JumpWithAlias);

                    if (MouseJumps > JumpWithAlias && MouseJumps > 0)
                        Console.WriteLine("Mouse jump / alias ratio: " + Math.Round(Convert.ToSingle(JumpWithAlias) / Convert.ToSingle(MouseJumps) * 100.0f, 1) + "%");
                    else
                        Console.WriteLine("Hight alias ratio detected.");
                }
            }

            if (BHOPcount / 2 > 1)
            {
                if (IsRussia)
                {
                    TextComments.WriteLine("[BHOP] Количество предупреждений:" + BHOPcount / 2);
                    Console.WriteLine("[BHOP] Количество предупреждений:" + BHOPcount / 2);
                }
                else
                {
                    TextComments.WriteLine("[BHOP] Warn count:" + BHOPcount / 2);
                    Console.WriteLine("[BHOP] Warn count:" + BHOPcount / 2);
                }
            }

            if (BadAttackCount > 0)
            {
                if (IsRussia)
                {
                    TextComments.WriteLine("[Триппер бот] Количество предупреждений:" + BadAttackCount);
                    Console.WriteLine("[Триппер бот] Количество предупреждений:" + BadAttackCount);
                }
                else
                {
                    TextComments.WriteLine(
                        "[TRIGGERBOT] Warn count:" + BadAttackCount);
                    Console.WriteLine("[TRIGGERBOT] Warn count:" + BadAttackCount);
                }
            }

            if (TotalAimBotDetected > 0)
            {
                if (IsRussia)
                {
                    TextComments.WriteLine("[AIM] Количество предупреждений:" + TotalAimBotDetected);
                    Console.WriteLine("[AIM] Количество предупреждений:" + TotalAimBotDetected);
                }
                else
                {
                    TextComments.WriteLine("[AIM] Warn count:" + TotalAimBotDetected);
                    Console.WriteLine("[AIM] Warn count:" + TotalAimBotDetected);
                }
            }



            if (FakeLagAim > 0)
            {
                if (IsRussia)
                {
                    TextComments.WriteLine("[ФЕЙК ЛАГ] Количество предупреждений:" + FakeLagAim);
                    Console.WriteLine("[ФЕЙК ЛАГ] Количество предупреждений:" + FakeLagAim);
                }
                else
                {
                    TextComments.WriteLine("[FAKELAG] Warn count:" + FakeLagAim);
                    Console.WriteLine("[FAKELAG] Warn count:" + FakeLagAim);
                }
            }

            if (JumpHackCount > 0)
            {
                if (IsRussia)
                {
                    TextComments.WriteLine(
                        "[STRAFE/GROUND/FASTRUN ХАК] Количество предупреждений:" +
                        JumpHackCount /*+ ". Found " + JumpCount + " +jump commands"*/);
                    Console.WriteLine(
                        "[STRAFE/GROUND/FASTRUN ХАК] Количество предупреждений:" +
                        JumpHackCount /*+ ". Found " + JumpCount + " +jump commands*/);
                }
                else
                {
                    TextComments.WriteLine(
                        "[STRAFE/GROUND/FASTRUN HACK] Warn count:" +
                        JumpHackCount /*+ ". Found " + JumpCount + " +jump commands"*/);
                    Console.WriteLine(
                        "[STRAFE/GROUND/FASTRUN HACK] Warn count:" +
                        JumpHackCount /*+ ". Found " + JumpCount + " +jump commands*/);
                }
            }

            if (UnknownMessages > 0)
            {
                TextComments.WriteLine("Warning. Unknown messages detected. Detect count:" + UnknownMessages);
                Console.WriteLine("Warning. Unknown messages detected. Detect count:" + UnknownMessages);
            }


            if (unknownCMDLIST.Count > 0)
            {
                Console.WriteLine("Bind/alias from blacklist detected:");
                TextComments.WriteLine("Bind/alias from blacklist detected:");
                foreach (string chet in unknownCMDLIST)
                {
                    TextComments.WriteLine(chet);
                    Console.WriteLine(chet);
                }
            }


            double maxfps1 =
                Math.Round(
                    1000.0f / (1000.0f * CurrentDemoFile.GsDemoInfo.AditionalStats
                        .FrametimeMin), 5);
            double maxfps2 =
                Math.Round(1000.0f / CurrentDemoFile.GsDemoInfo.AditionalStats.MsecMin, 5);

            if (averagefps2.Count > 2 && Math.Abs(maxfps1 - 1000.0f) < float.Epsilon && Math.Abs(maxfps2 - 1000.0f) < float.Epsilon &&
                maxfps1 / averagefps2.Average() > 4.0f)
                DemoScanner_AddWarn("[UNKNOWN BHOP/JUMPHACK] in demo file!", false);
            //if (Math.Round((1000.0f / CurrentDemoFile.GsDemoInfo.AditionalStats.MsecMin), 5) /
            //    Math.Round(1000.0 / (1000.0f * CurrentDemoFile.GsDemoInfo.AditionalStats.FrametimeMin), 5) > 1.75f)
            //{
            //    Console.WriteLine(" [aim/jump] Обнаружены манипуляции с задержкой. (???)");
            //    Console.WriteLine("Unknown delay/timer hack detected. (???)");
            //}

            //if (Convert.ToSingle(RealFpsMax) / Math.Round(1000.0 / (1000.0f * CurrentDemoFile.GsDemoInfo.AditionalStats.FrametimeMin), 5) > 1.5f)
            //{
            //    Console.WriteLine(" [speedhack] Обнаружены манипуляции с задержкой. (???)");
            //    Console.WriteLine("Unknown delay/timer hack detected. (???)");
            //}

            Console.ForegroundColor = ConsoleColor.DarkGreen;

            ViewDemoHelperComments.Seek(4, SeekOrigin.Begin);
            ViewDemoHelperComments.Write(ViewDemoCommentCount);
            ViewDemoHelperComments.Seek(0, SeekOrigin.Begin);


            ForceFlushScanResults();
            DateTime EndScanTime = Trim(new DateTime((DateTime.Now - StartScanTime).Ticks), 10);

            TimeSpan time1 = TimeSpan.FromSeconds(Math.Min(StartGameSecond, CurrentGameSecond));

            TimeSpan time2 = TimeSpan.FromSeconds(Math.Max(StartGameSecond, CurrentGameSecond));

            if (IsRussia)
            {
                Console.WriteLine("Анализ завершен, потрачено " + EndScanTime.ToString("T") + " времени");
                Console.WriteLine("Игровое время демо начинается с " + time1.ToString("T") + " по " + time2.ToString("T") + " секунд.");
            }
            else
            {
                Console.WriteLine("Scan completed. Scan time: " + EndScanTime.ToString("T"));
                Console.WriteLine("Demo playing time: " + time1.ToString("T") + " ~ " + time2.ToString("T") + " seconds.");
            }

            /*if (ENABLE_LEARN_CLEAN_DEMO)
                if (BHOPcount < 4 && BadAttackCount < 4 && TotalAimBotDetected < 4 && FakeLagAim < 4 && JumpHackCount < 4) MachineLearnAnglesCLEAN.WriteAnglesDB();


            if (ENABLE_LEARN_HACK_DEMO_FORCE_SAVE || ENABLE_LEARN_HACK_DEMO_SAVE_ALL_ANGLES) MachineLearnAnglesHACK.WriteAnglesDB();
            */
            if (SKIP_RESULTS) return;

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                ConsoleTable table = null;
                if (IsRussia)
                    Console.WriteLine("Выберите следующее действие:");
                else
                    Console.WriteLine("Choose next actions:");

                if (IsRussia)
                    Console.WriteLine("Нажмите 8 получения для инструкции!");
                else
                    Console.WriteLine("Choose 8 for read help!");

                Console.WriteLine();

                if (IsRussia)
                {
                    table = new ConsoleTable("в CDB", "в TXT", "Демо инфо",
                        "Игроки", "Голоса", "История мыши", "Команды");
                    table.AddRow("1", "2", "3", "4", "5", "6", "7");
                    table.Write(Format.Alternative);
                    table = new ConsoleTable("Помощь", "Скачать", "Выход");
                    table.AddRow("8", "9", "0/Enter");
                    table.Write(Format.Alternative);
                }
                else
                {
                    table = new ConsoleTable("Save CDB", "Save TXT", "Demo info",
                  "Player info", "Wav Player", "Sens History", "Commands");
                    table.AddRow("1", "2", "3", "4", "5", "6", "7");
                    table.Write(Format.Alternative);
                    table = new ConsoleTable("Help", "Download", "Exit");
                    table.AddRow("8", "9", "0/Enter");
                    table.Write(Format.Alternative);
                }

                string command = Console.ReadLine();

                if (command.Length == 0 || command == "0") return;

                if (command == "9")
                {
                    Console.Write("Enter path to cstrike dir:");
                    string strikedir = Console.ReadLine().Replace("\"", "");

                    if (strikedir.EndsWith("/") || strikedir.EndsWith("\\")) strikedir.Remove(strikedir.Length - 1);

                    if (File.Exists(strikedir + "\\..\\" + "hw.dll"))
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
                            DownloadResources = DownloadResources.Distinct().ToList();
                            Console.WriteLine("Download " + DownloadResources.Count + " resources with total size: " + DownloadResourcesSize + " bytes");
                            Console.WriteLine("Download start time:" + DateTime.Now.ToString("HH:mm:ss"));
                            if (File.Exists(CurrentDir + @"\DownloadError.txt"))
                            {
                                try
                                {
                                    File.Delete(CurrentDir + @"\DownloadError.txt");
                                }
                                catch
                                {

                                }
                            }

                            int sum = 0;
                            int threads = 0;
                            int threadid = 0;
                            Parallel.ForEach
                                (DownloadResources, new ParallelOptions { MaxDegreeOfParallelism = RESOURCE_DOWNLOAD_THREADS }, s =>
                             {
                                 s = s.Replace("/", "\\");
                                 int current_thread_id = 0;

                                 lock (sync)
                                 {
                                     if (threadid > RESOURCE_DOWNLOAD_THREADS)
                                         threadid = 0;
                                     current_thread_id = threadid;
                                     threadid++;
                                     sum = sum + 1;
                                     threads++;
                                 }

                                 Thread.SetData(Thread.GetNamedDataSlot("int"), current_thread_id);

                                 if (char.IsLetterOrDigit(s[0]) && !File.Exists(strikedir + "\\" + s) && !File.Exists(strikedir + "\\..\\cstrike\\" + s) && !File.Exists(strikedir + "\\..\\valve\\" + s))
                                 {
                                     lock (sync)
                                     {
                                         ConsoleHelper.ClearCurrentConsoleLine();
                                         Console.Write("\rDownload \"" + s + "\" " + sum + " of " + DownloadResources.Count + ". In " + GetActiveDownloadThreads() + " of " + (GetActiveDownloadThreads()  > threads ? GetActiveDownloadThreads() : threads) + " threads.");
                                     }
                                     bWebClient myWebClient = new bWebClient();
                                     try
                                     {
                                         myWebClient.DownloadProgressChanged += MyWebClient_DownloadProgressChanged;
                                         byte[] tmptaskdata = myWebClient.DownloadDataTaskAsync(DownloadLocation + s).Result;
                                         try
                                         {
                                             Directory.CreateDirectory(Path.GetDirectoryName(strikedir + "\\" + s));
                                         }
                                         catch
                                         {

                                         }
                                         File.WriteAllBytes(strikedir + "\\" + s, tmptaskdata);
                                     }
                                     catch
                                     {
                                         lock (sync)
                                         {
                                             ConsoleHelper.ClearCurrentConsoleLine();
                                             string dwnerrorstr = "\rFailed to download \"" + s + "\" file.";
                                             Console.Write(dwnerrorstr);
                                             Thread.Sleep(50);
                                             File.AppendAllText(CurrentDir + @"\DownloadError.txt", dwnerrorstr + "\r\n");
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
                            if (File.Exists(CurrentDir + @"\DownloadError.txt"))
                            {
                                Process.Start(CurrentDir + @"\DownloadError.txt");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Bad directory! Please enter path to Half Life directory!");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Bad directory! Please enter path to Half Life directory!");
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
                    Console.WriteLine(
                        " * Unreal Demo Scanner tool for search hack patterns in demo file.");
                    Console.WriteLine(
                        " * Unreal Demo Scanner программа помогающая обнаружить читеров.");
                    Console.WriteLine(
                        " * Report false positive: (Сообщить о ложном срабатывании):");
                    Console.WriteLine(
                        " * https://c-s.net.ua/forum/topic90973.html , https://goldsrc.ru/threads/4627/");
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
                    Console.WriteLine(" * DETECTED - требуется всего несколько за демо что бы с точностью сказать");
                    Console.WriteLine(" *            что игрок пользовался запрещенными программами.  ");
                    Console.WriteLine(" * WARN - несколько срабатвыаний за демо не говорит об наличии читов");
                    Console.WriteLine(" *        но указывает на необходимость проверить игрока вручную.");
                    Console.WriteLine(" * ");
                    Console.WriteLine(" *                НО ВНИМАНИЕ!");
                    Console.WriteLine(" * ");
                    Console.WriteLine(" * Учитывайте остальные факторы указанные вам в консоли (Лаги, и т.п)");
                    Console.WriteLine(
                        " * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * ");
                }

                if (command == "7")
                    try
                    {
                        CommandsDump.Insert(0, "This file created by Unreal Demo Scanner\n\n");
                        File.WriteAllLines(CurrentDir + @"\Commands.txt", CommandsDump.ToArray());
                        Process.Start(CurrentDir + @"\Commands.txt");
                    }
                    catch
                    {
                        Console.WriteLine("Error access to command file.");
                    }

                if (command == "6")
                {
                    table = new ConsoleTable("Second", "Sensitivity", "Length", "Time", "Weapon");
                    for (int i = 0; i < PlayerSensitivityHistoryStr.Count; i++)
                        table.AddRow(i + 1, PlayerSensitivityHistoryStr[i], PlayerAngleLenHistory[i],
                            PlayerSensitivityHistoryStrTime[i]
                            , PlayerSensitivityHistoryStrWeapon[i]);

                    table.Write(Format.Alternative);
                    try
                    {
                        File.WriteAllText(CurrentDir + @"\SensHistory.txt", table.ToStringAlternative());
                        Process.Start(CurrentDir + @"\SensHistory.txt");
                    }
                    catch
                    {
                        Console.WriteLine("Error access write sens history log!");
                    }
                }

                if (command == "5")
                {
                    if (!File.Exists(CurrentDir +
                            @"\revoicedecoder.exe"))
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;

                        Console.WriteLine(
                            "No decoder found at path " + CurrentDir +
                            @"\revoicedecoder.exe");

                        continue;
                    }

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    try
                    {
                        try
                        {
                            if (Directory.Exists(CurrentDir + @"\out")) Directory.Delete(CurrentDir + @"\out", true);
                        }
                        catch
                        {
                            Console.WriteLine("Error deleting out folder.");
                        }

                        if (!Directory.Exists(CurrentDir + @"\out")) Directory.CreateDirectory(CurrentDir + @"\out");

                        fullPlayerList.AddRange(playerList);
                        int offset = 0;

                        foreach (Player player in fullPlayerList)
                        {
                            if (player.Name.Length <= 0) continue;

                            if (player.voicedata_stream.Length > 0)
                            {
                                if (File.Exists(CurrentDir + @"\input.wav.enc"))
                                {
                                    try
                                    {
                                        File.Delete(CurrentDir + @"\input.wav.enc");
                                    }
                                    catch
                                    {
                                        Console.WriteLine("Error deleting input.wav.enc");
                                    }
                                }

                                BinaryReader binaryReader =
                                    new BinaryReader(player.voicedata_stream);
                                player.voicedata_stream.Seek(0, SeekOrigin.Begin);
                                List<byte> data2 = new List<byte>(binaryReader.ReadBytes(
                                    (int)player.voicedata_stream.Length));

                                data2.Insert(0, (byte)VoiceQuality);

                                File.WriteAllBytes(CurrentDir + @"\input.wav.enc", data2.ToArray());
                                Process process = new Process();
                                process.StartInfo.FileName = CurrentDir +
                            @"\revoicedecoder.exe";
                                process.StartInfo.WorkingDirectory = CurrentDir;
                                process.Start();
                                process.WaitForExit();

                                string filename = Regex.Replace(player.Name, @"[^\u0000-\u007F]+", "_") + "(" + player.Slot + ").wav";

                                foreach (char c in Path.GetInvalidFileNameChars()) filename = filename.Replace(c, 'x');

                                if (File.Exists(CurrentDir + @"\out\" + filename))
                                {
                                    offset++;
                                    filename = offset + "_" + filename;
                                }
                                if (File.Exists(CurrentDir + @"\output.wav"))
                                    File.Move(CurrentDir + @"\output.wav", CurrentDir + @"\out\" + filename);
                            }
                        }

                        if (File.Exists(CurrentDir + @"\input.wav.enc"))
                        {
                            try
                            {
                                File.Delete(CurrentDir + @"\input.wav.enc");
                            }
                            catch
                            {
                                Console.WriteLine("Error deleting input.wav.enc");
                            }
                        }

                        Console.WriteLine("Success players voice decode!");
                        Process.Start(CurrentDir + @"\out\");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Player voice decode error:" + ex.Message);
                    }
                }

                if (command == "4")
                    try
                    {
                        File.Delete(CurrentDir + @"\players.txt");
                        File.AppendAllText(CurrentDir + @"\players.txt", "Current players:\n");

                        File.AppendAllText(CurrentDir + @"\players.txt",
                            "Local player:" + UserId);

                        foreach (Player player in playerList)
                            if (player.Name.Length > 0)
                            {
                                table = new ConsoleTable(
                                    "Player:" + player.Name + "(" + player.Slot + ")");
                                foreach (KeyValuePair<string, string> keys in player.InfoKeys) table.AddRow(keys.Key + " = " + keys.Value);

                                table.AddRow("SLOTID = " + player.Slot);
                                File.AppendAllText(CurrentDir + @"\players.txt",
                                    table.ToStringAlternative());
                            }

                        File.AppendAllText(CurrentDir + @"\players.txt", "Old players:\n");
                        foreach (Player player in fullPlayerList)
                            if (player.Name.Length > 0)
                            {
                                table = new ConsoleTable(
                                    "Player:" + player.Name + "(" + player.Slot + ")");
                                foreach (KeyValuePair<string, string> keys in player.InfoKeys) table.AddRow(keys.Key + " = " + keys.Value);

                                table.AddRow("SLOTID = " + player.Slot);
                                File.AppendAllText(CurrentDir + @"\players.txt",
                                    table.ToStringAlternative());
                            }

                        Console.WriteLine("All players saved to players.txt");
                        Process.Start(CurrentDir + @"\players.txt");
                    }
                    catch
                    {
                        Console.WriteLine("Error while saving players to txt!");
                    }

                if (command == "1")
                {
                    if (ViewDemoCommentCount > 0)
                    {

                        if (File.Exists(CurrentDemoFilePath.Remove(CurrentDemoFilePath.Length - 3) +
                                        "cdb"))
                        {
                            if (File.Exists(CurrentDemoFilePath.Remove(CurrentDemoFilePath.Length - 3) +
                                            "cdb.bak"))
                                try
                                {
                                    File.Delete(CurrentDemoFilePath.Remove(CurrentDemoFilePath.Length - 3) +
                                                "cdb.bak");
                                }
                                catch
                                {
                                    Console.WriteLine("Error: No access to VDH log path.");
                                }

                            try
                            {
                                File.Delete(CurrentDemoFilePath.Remove(CurrentDemoFilePath.Length - 3) +
                                            "cdb");
                            }
                            catch
                            {
                                Console.WriteLine(
                                    "File " +
                                    CurrentDemoFilePath.Remove(CurrentDemoFilePath.Length - 3) + "cdb" +
                                    " open error : No access to remove!");
                                Console.Write("No access to file... Try again!");
                                Console.ReadKey();
                                return;
                            }
                        }

                        try
                        {
                            BinaryReader binaryReader =
                                new BinaryReader(ViewDemoHelperComments.BaseStream);
                            ViewDemoHelperComments.BaseStream.Seek(0, SeekOrigin.Begin);
                            byte[] comments =
                                binaryReader.ReadBytes((int)binaryReader.BaseStream
                                    .Length);
                            File.WriteAllBytes(
                                CurrentDemoFilePath.Remove(CurrentDemoFilePath.Length - 3) +
                                "cdb", comments);
                            Console.WriteLine("View Demo Helper comments saved");
                        }
                        catch
                        {
                            Console.WriteLine("Can't write comments!");
                        }
                    }
                    else
                        Console.WriteLine("No View Demo Helper comments found.");
                }

                if (command == "2")
                {
                    if (TextCommentsStream.Length > 0)
                        try
                        {
                            BinaryReader binaryReader = new BinaryReader(TextCommentsStream);
                            TextCommentsStream.Seek(0, SeekOrigin.Begin);
                            byte[] comments =
                                binaryReader.ReadBytes((int)binaryReader.BaseStream
                                    .Length);
                            File.WriteAllBytes(
                                CurrentDemoFilePath.Remove(CurrentDemoFilePath.Length - 3) +
                                "txt", comments);
                            Console.WriteLine("Text comments saved");
                            Process.Start(
                                CurrentDemoFilePath.Remove(CurrentDemoFilePath.Length - 3) +
                                "txt");
                        }
                        catch
                        {
                            Console.WriteLine("Can't write comments!");
                        }
                    else
                        Console.WriteLine("No text comments found.");
                }

                if (command == "3")
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;

                    Console.WriteLine(
                        "Demo information / Дополнительная информация о демке");
                    //  Console.WriteLine("Min angles:" + MinFrameViewanglesX + " " + MinFrameViewanglesY);

                    table = new ConsoleTable("ТИП/TYPE", "ПРЫЖКИ/JUMPS", "АТАКА/ATTACKS");

                    table.AddRow(1, JumpCount, attackscounter)
                        .AddRow(2, JumpCount2, attackscounter3)
                        .AddRow(3, JumpCount3, attackscounter4)
                        .AddRow(4, JumpCount4, attackscounter5)
                        .AddRow(5, JumpCount5, attackscounter2)
                        .AddRow(6, JumpCount6, "SKIP");

                    table.Write(Format.Alternative);

                    Console.WriteLine("Reload type 1:" + Reloads);
                    Console.WriteLine("Reload type 2:" + Reloads2);
                    Console.WriteLine("Shots fired:" + attackscounter4);
                    Console.WriteLine("Bad Shots fired:" + attackscounter5);
                    Console.WriteLine("Attack in air:" + AirShots);
                    Console.WriteLine("Teleport count:" + PlayerTeleportus);
                    Console.WriteLine("Fly time: " + Convert.ToInt32(100.0 / (TotalFramesOnFly + TotalFramesOnGround) * TotalFramesOnFly) + "%");
                    Console.WriteLine("Attack in fly: " + Convert.ToInt32(100.0 / (TotalFramesOnFly + TotalFramesOnGround) * TotalAttackFramesOnFly) + "%");

                    table = new ConsoleTable(
                        "УБИЙСТВ /KILLS", "СМЕРТЕЙ/DEATHS");

                    table.AddRow(KillsCount, DeathsCoount);

                    table.Write(Format.Alternative);

                    Console.WriteLine("Calculated FPS / Подсчитанный FPS");
                    table = new ConsoleTable("Максимальный FPS / FPS MAX",
                        "Минимальная зареджка/Min Delay", "Средний FPS/Average FPS");
                    table.AddRow(
                        Math.Round(CurrentDemoFile.GsDemoInfo.AditionalStats.FrametimeMin,
                            5) + "(" + Math.Round(
                            1000.0f / (1000.0f * CurrentDemoFile.GsDemoInfo.AditionalStats
                                .FrametimeMin), 5) + " FPS)",
                        CurrentDemoFile.GsDemoInfo.AditionalStats.MsecMin + "(" +
                        Math.Round(
                            1000.0f / CurrentDemoFile.GsDemoInfo.AditionalStats.MsecMin,
                            5) + " FPS)",
                        averagefps2.Count > 2
                            ? averagefps2.Average().ToString()
                            : "UNKNOWN");
                    table.Write(Format.Alternative);
                    table = new ConsoleTable("Минимальный FPS / FPS MIN",
                        "Максимальная зареджка/Max Delay", "Средний FPS/Average FPS");
                    table.AddRow(
                        Math.Round(CurrentDemoFile.GsDemoInfo.AditionalStats.FrametimeMax,
                            5) + "(" + Math.Round(
                            1000.0f / (1000.0f * CurrentDemoFile.GsDemoInfo.AditionalStats
                                .FrametimeMax), 5) + " FPS)",
                        CurrentDemoFile.GsDemoInfo.AditionalStats.MsecMax + "(" +
                        Math.Round(
                            1000.0f / CurrentDemoFile.GsDemoInfo.AditionalStats.MsecMax,
                            5) + " FPS)",
                        averagefps.Count > 2 ? averagefps.Average().ToString() : "UNKNOWN");
                    table.Write(Format.Alternative);

                    try
                    {
                        //if (new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).Length < 1000000)
                        //{
                        Console.WriteLine("Real FPS / Реальный FPS");
                        table = new ConsoleTable("Минимальный FPS / FPS MIN",
                            "Максимальный FPS / FPS MAX");
                        table.AddRow(
                            RealFpsMin == int.MaxValue ? "UNKNOWN" : RealFpsMin.ToString(),
                            RealFpsMax == int.MinValue ? "UNKNOWN" : RealFpsMax.ToString());
                        table.AddRow(
                            RealFpsMin2 == int.MaxValue
                                ? "UNKNOWN"
                                : RealFpsMin2.ToString(),
                            RealFpsMax2 == int.MinValue
                                ? "UNKNOWN"
                                : RealFpsMax2.ToString());
                        table.Write(Format.Alternative);
                        //}
                    }
                    catch
                    {
                        Console.WriteLine("Error access write FPS info!");
                    }

                    if (playerresolution.Count > 0)
                    {
                        table = new ConsoleTable("Screen resolution");
                        foreach (WindowResolution s in playerresolution) table.AddRow(s.x + "x" + s.y);

                        table.Write(Format.Alternative);
                    }

                    if (CurrentFrameIdAll > 0) Console.WriteLine("Frames: " + CurrentFrameIdAll);

                    if (LostStopAttackButton > 2) DemoScanner_AddInfo("Lost -attack commands: " + LostStopAttackButton + ".");

                    if (NeedIgnoreAttackFlagCount > 0) DemoScanner_AddInfo("Lost attack flag: " + NeedIgnoreAttackFlagCount + ".");

                    if (ModifiedDemoFrames > 0) DemoScanner_AddInfo("Possible demoscanner bypass. Tried to kill frames:" + ModifiedDemoFrames);

                    Console.WriteLine("Maximum time between frames:" + MaximumTimeBetweenFrames.ToString("F6"));


                    Console.WriteLine("ServerName:" + ServerName);
                    Console.WriteLine("MapName:" + MapName);
                    Console.WriteLine("GameDir:" + GameDir);
                    Console.WriteLine("Download Location:");
                    Console.WriteLine(DownloadLocation);
                    Console.WriteLine("DealthMatch:" + DealthMatch);

                    if (playerList.Count > 0) Console.WriteLine("Players: " + playerList.Count);

                    Console.WriteLine("Codecname:" + codecname);

                    if (CheatKey > 0)
                        Console.WriteLine(
                            "Possible press cheat key " + CheatKey + " times. (???)");

                    if (FrameDuplicates > 0) Console.WriteLine("Duplicate frames(Same frame): " + FrameDuplicates);

                    if (attackscounter4 > 50 && attackscounter5 > attackscounter4 / 2)
                    {
                        if (IsRussia)
                            DemoScanner_AddInfo("Странные данные: " + attackscounter5 + " неправильных выстрелов из " + attackscounter4);
                        else
                            DemoScanner_AddInfo("Just strange info: " + attackscounter5 + " strange shoots. Total " + attackscounter4);
                    }

                    if (averagefps.Average() > MAX_MONITOR_REFRESHRATE ||
               averagefps2.Average() > MAX_MONITOR_REFRESHRATE)
                    {
                        if (IsRussia)
                            DemoScanner_AddInfo("Игрок играет с высоким фпс: " + RealFpsMax);
                        else
                            DemoScanner_AddInfo("Player playing with non default fps. Max fps: " + RealFpsMax);
                    }

                    Console.WriteLine("Max input bytes per second : " + MaxBytesPerSecond + ". Count:" + MsgOverflowSecondsCount);

                    Console.WriteLine("Max text messages per seconds : " + MaxMsgPerSecond);

                    Console.WriteLine("Loss count:" + LossPackets);

                    Console.WriteLine("Frameskip count:" + LossPackets2);

                    Console.WriteLine("Choke count : " + ChokePackets);

                    ForceFlushScanResults();
                }
            }
        }

        public static void UpdateThreadState(int thredid, int state)
        {
            lock (sync)
            {
                myThreadStates[thredid].state = state;
            }
        }

        public static int GetActiveDownloadThreads()
        {
            int cnt = 0;
            lock (sync)
            {
                for (int i = 0; i < myThreadStates.Length; i++)
                {
                    if (Math.Abs(Environment.TickCount - myThreadStates[i].state) < 5000)
                    {
                        cnt++;
                    }
                }
            }
            return cnt;
        }

        private static void MyWebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            int threadid = Thread.GetData(Thread.GetNamedDataSlot("int")) != null ? (int)Thread.GetData(Thread.GetNamedDataSlot("int")) : 0;
            UpdateThreadState(threadid, Environment.TickCount);
        }

        private static void AddLerpAndMs(int lerpMsec, byte msec)
        {
            while (historyUcmdLerpAndMs.Count > 10) historyUcmdLerpAndMs.RemoveAt(0);

            UcmdLerpAndMs tmpUcmdLerpAndMs = new UcmdLerpAndMs
            {
                lerp = lerpMsec,
                msec = msec
            };
            historyUcmdLerpAndMs.Add(tmpUcmdLerpAndMs);
        }

        public static bool FindLerpAndMs(int lerpMsec, byte msec)
        {
            if (lerpMsec > 0 || msec > 0) return true;

            foreach (UcmdLerpAndMs v in historyUcmdLerpAndMs)
                if (v.lerp < 0 || v.lerp == lerpMsec && v.msec == msec) return true;
            msec = Convert.ToByte(msec * 2.0);
            if (lerpMsec > 0 || msec > 0) return true;

            foreach (UcmdLerpAndMs v in historyUcmdLerpAndMs)
                if (v.lerp < 0 || v.lerp == lerpMsec && v.msec == msec) return true;
            return false;
        }
        /*
        private static void WriteLearnAngles()
        {
            List<float> newLearnAngles = new List<float>();
            bool anyfromzero = false;
            double math = Math.Round(Math.Abs(AngleBetween(LearnAngles[0], LearnAngles[LearnAngles.Count - 1])), 3, MidpointRounding.AwayFromZero);
            if (Math.Abs(math) > float.Epsilon) anyfromzero = true;
            newLearnAngles.Add(Convert.ToSingle(math));
            for (int i = 0; i < LearnAngles.Count - 1; i++)
            {
                math = Math.Round(Math.Abs(AngleBetween(LearnAngles[i], LearnAngles[i + 1])), 3, MidpointRounding.AwayFromZero);
                if (Math.Abs(math) > float.Epsilon) anyfromzero = true;
                newLearnAngles.Add(Convert.ToSingle(math));
            }
            if (!anyfromzero) return;
            if (ENABLE_LEARN_HACK_DEMO_FORCE_SAVE || ENABLE_LEARN_HACK_DEMO_SAVE_ALL_ANGLES)
                MachineLearnAnglesHACK.AddAnglesToDB(newLearnAngles);
            else
                MachineLearnAnglesCLEAN.AddAnglesToDB(newLearnAngles);
        }*/

        /* private static void CheckLearnAngles()
         {
             List<float> newLearnAngles = new List<float>();
             bool anyfromzero = false;
             double math = Math.Round(Math.Abs(AngleBetween(LearnAngles[0], LearnAngles[LearnAngles.Count - 1])), 3, MidpointRounding.AwayFromZero);
             if (Math.Abs(math) > float.Epsilon) anyfromzero = true;

             newLearnAngles.Add(Convert.ToSingle(math));
             for (int i = 0; i < LearnAngles.Count - 1; i++)
             {
                 math = Math.Round(Math.Abs(AngleBetween(LearnAngles[i], LearnAngles[i + 1])), 3, MidpointRounding.AwayFromZero);
                 if (Math.Abs(math) > float.Epsilon) anyfromzero = true;
                 newLearnAngles.Add(Convert.ToSingle(math));
             }

             if (!anyfromzero) return;

             if (MachineLearnAnglesHACK.IsAnglesInDB(newLearnAngles, 1.0f) && !MachineLearnAnglesCLEAN.IsAnglesInDB(newLearnAngles, 1.0f))
             {
                 if (IsRussia)
                     DemoScanner_AddWarn("[МАШИННОЕ ОБУЧЕНИЕ] ПОДОЗРИТЕЛЬНЫЙ МОМЕНТ 100% СОВПАДЕНИЕ at " + CurrentTimeString, false);
                 else
                     DemoScanner_AddWarn("[BETA] MACHINE LEARN AIM 100% MATCH: at " + CurrentTimeString, false);
             }
             else if (MachineLearnAnglesHACK.IsAnglesInDB(newLearnAngles, 0.1f) && !MachineLearnAnglesCLEAN.IsAnglesInDB(newLearnAngles, 0.1f))
             {
                 if (IsRussia)
                     DemoScanner_AddWarn("[МАШИННОЕ ОБУЧЕНИЕ] ПОДОЗРИТЕЛЬНЫЙ МОМЕНТ 75% СОВПАДЕНИЕ at " + CurrentTimeString, false);
                 else
                     DemoScanner_AddWarn("[BETA] MACHINE LEARN AIM 75% MATCH: at " + CurrentTimeString, false);
             }
             else if (MachineLearnAnglesHACK.IsAnglesInDB(newLearnAngles, 0.01f) && !MachineLearnAnglesCLEAN.IsAnglesInDB(newLearnAngles, 0.01f))
             {
                 if (IsRussia)
                     DemoScanner_AddWarn("[МАШИННОЕ ОБУЧЕНИЕ] ПОДОЗРИТЕЛЬНЫЙ МОМЕНТ 40% СОВПАДЕНИЕ at " + CurrentTimeString, false);
                 else
                     DemoScanner_AddWarn("[BETA] MACHINE LEARN AIM 40% MATCH: at " + CurrentTimeString, false);
             }
         }

         */
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
            return Math.Abs(CurrentTime - RoundEndTime) < 3.0f;
        }

        private static double truncate(float x)
        {
            return x < 0.0f ? -Math.Floor(-x) : Math.Floor(x);
        }

        private static float MyFmod(float x, float y)
        {
            return Convert.ToSingle(x - truncate(x / y) * y);
        }

        private static float normalizeangle(float angle)
        {
            angle = MyFmod(angle, 360.0f);
            while (angle < 0) angle += 360.0f;

            return angle;
        }

        public static float AngleBetween(float angle1, float angle2)
        {
            angle1 = normalizeangle(angle1);
            angle2 = normalizeangle(angle2);
            float anglediff = normalizeangle(angle1 - angle2);
            if (360.0f - anglediff < anglediff) anglediff = 360.0f - anglediff;

            return Math.Abs(anglediff);
        }

        public static float AngleBetweenAbsolute(float angle1, float angle2)
        {
            return Math.Abs(AngleBetween(angle1, angle2));
        }

        public static string GetSourceCodeString()
        {
            usagesrccode++;
            asdf.asdf2++;
            if (SourceCode.Length != 51) throw new Exception("ANAL ERROR");

            return SourceCode;
        }

        private static void ParseGameData(byte[] msgBytes)
        {
            halfLifeDemoParser.ParseGameDataMessages(msgBytes);
        }

        public static bool isAngleInPunchListX(float angle)
        {
            while (LastPunchAngleX.Count < 5) LastPunchAngleX.Add(0.0f);

            if (!(CurrentWeapon == WeaponIdType.WEAPON_ELITE
                  || CurrentWeapon == WeaponIdType.WEAPON_USP
                  || CurrentWeapon == WeaponIdType.WEAPON_AWP
                  || CurrentWeapon == WeaponIdType.WEAPON_SCOUT
                  || CurrentWeapon == WeaponIdType.WEAPON_DEAGLE
                  || CurrentWeapon == WeaponIdType.WEAPON_P228
                  || CurrentWeapon == WeaponIdType.WEAPON_FIVESEVEN
                  || CurrentWeapon == WeaponIdType.WEAPON_XM1014
                  || CurrentWeapon == WeaponIdType.WEAPON_M3
                  || CurrentWeapon == WeaponIdType.WEAPON_SG550
                  || CurrentWeapon == WeaponIdType.WEAPON_G3SG1
                ))
                return true;

            foreach (float f in LastPunchAngleX)
            {
                if (AngleBetweenAbsolute(angle, f) < 0.001f) return true;

                if (CurrentWeapon == WeaponIdType.WEAPON_XM1014
                    || CurrentWeapon == WeaponIdType.WEAPON_M3)
                    if (AngleBetweenAbsolute(angle, f) > 2.9f && AngleBetweenAbsolute(angle, f) < 11.1f) return true;

                if (CurrentWeapon == WeaponIdType.WEAPON_SG550
                    || CurrentWeapon == WeaponIdType.WEAPON_G3SG1)
                {
                    float addangle = angle * 0.25f;
                    if (AngleBetweenAbsolute(angle, f - addangle) > 0.74f && AngleBetweenAbsolute(angle, f - addangle) < 1.26f) return true;
                    if (AngleBetweenAbsolute(angle, f + addangle) > 0.74f && AngleBetweenAbsolute(angle, f + addangle) < 1.26f) return true;
                }

                if (CurrentWeapon == WeaponIdType.WEAPON_ELITE
                   || CurrentWeapon == WeaponIdType.WEAPON_USP
                   || CurrentWeapon == WeaponIdType.WEAPON_AWP
                   || CurrentWeapon == WeaponIdType.WEAPON_SCOUT
                   || CurrentWeapon == WeaponIdType.WEAPON_DEAGLE
                   || CurrentWeapon == WeaponIdType.WEAPON_P228
                   || CurrentWeapon == WeaponIdType.WEAPON_FIVESEVEN)
                {
                    if (AngleBetweenAbsolute(angle, f) > 1.9f && AngleBetweenAbsolute(angle, f) < 2.1f) return true;

                    if (AngleBetweenAbsolute(angle, f) > 3.9f && AngleBetweenAbsolute(angle, f) < 4.1f) return true;
                }

            }

            return false;
        }

        public static bool isAngleInPunchListY(float angle)
        {
            // Console.WriteLine("SearchAngle:" + angle);
            if (LastPunchAngleY.Count < 6) return true;

            if (!(CurrentWeapon == WeaponIdType.WEAPON_ELITE
                  || CurrentWeapon == WeaponIdType.WEAPON_USP
                  || CurrentWeapon == WeaponIdType.WEAPON_AWP
                  || CurrentWeapon == WeaponIdType.WEAPON_SCOUT
                  || CurrentWeapon == WeaponIdType.WEAPON_DEAGLE
                  || CurrentWeapon == WeaponIdType.WEAPON_P228
                  || CurrentWeapon == WeaponIdType.WEAPON_FIVESEVEN
                  || CurrentWeapon == WeaponIdType.WEAPON_XM1014
                  || CurrentWeapon == WeaponIdType.WEAPON_M3
                  || CurrentWeapon == WeaponIdType.WEAPON_NONE
                  || CurrentWeapon == WeaponIdType.WEAPON_KNIFE
                  || CurrentWeapon == WeaponIdType.WEAPON_HEGRENADE
                  || CurrentWeapon == WeaponIdType.WEAPON_FLASHBANG
                  || CurrentWeapon == WeaponIdType.WEAPON_SMOKEGRENADE
                  || CurrentWeapon == WeaponIdType.WEAPON_BAD2
                  || CurrentWeapon == WeaponIdType.WEAPON_BAD
                ))
                return true;

            foreach (float f in LastPunchAngleY)
                if (AngleBetweenAbsolute(angle, f) < 0.015f) return true;

            return false;
        }

        public static void addAngleInPunchListY(float angle)
        {
            // Console.WriteLine("addAngleInPunchListY:" + angle);
            if (LastPunchAngleY.Count > 8) LastPunchAngleY.RemoveAt(0);

            LastPunchAngleY.Add(angle);
        }

        public static void addAngleInPunchListX(float angle)
        {
            //Console.WriteLine("addAngleInPunchListX:" + angle);
            if (LastPunchAngleX.Count > 5) LastPunchAngleX.RemoveAt(0);
            LastPunchAngleX.Add(angle);
        }

        public static void addAngleInViewListY(float angle)
        {
            // Console.WriteLine("addAngleInViewListY:" + angle);
            if (LastSearchViewAngleY.Count > 15) LastSearchViewAngleY.RemoveAt(0);

            LastSearchViewAngleY.Add(angle);
        }

        public static bool isAngleInViewListY(float angle)
        {
            foreach (float f in LastSearchViewAngleY)
                if (AngleBetweenAbsolute(angle, f) < 0.1f) return true;
            return false;
        }

        public static bool IsPlayerLossConnection()
        {
            float retcheck = Math.Abs(CurrentTime - LastLossPacket);
            float retcheck2 = Math.Abs(CurrentTime - LastLossTimeEnd);
            bool retval = retcheck < 1.5f || retcheck2 < 1.5f;

            // if (retval)
            //      Console.WriteLine("CurrentTime:" + CurrentTime + ". LastLossPacket:" + LastLossPacket + ". LastLossTimeEnd:" + LastLossTimeEnd);


            return retval;
        }

        public static bool IsPlayerLossConnection(float CurrTime)
        {
            float retcheck = Math.Abs(CurrTime - LastLossPacket);
            float retcheck2 = Math.Abs(CurrTime - LastLossTimeEnd);
            bool retval = retcheck < 1.5f || retcheck2 < 1.5f;

            //if (retval)
            //    Console.WriteLine("CurrentTime:" + CurrentTime + ". LastLossPacket:" + LastLossPacket + ". LastLossTimeEnd:" + LastLossTimeEnd);


            return retval;
        }

        public static bool IsHookDetected()
        {
            float retcheck = Math.Abs(CurrentTime - LastBeamFound);
            bool retval = retcheck < 5.0f && retcheck >= 0f;

            /*if (retval)
                Console.WriteLine("CurrentTime:" + CurrentTime + ". LastBeamFound:" + LastBeamFound);*/

            return retval;
        }

        public static bool IsBigVelocity()
        {
            float retcheck = Math.Abs(CurrentTime - FoundBigVelocityTime);
            bool retval = retcheck < 2.0f && retcheck >= 0f;

            /* if (retval)
                 Console.WriteLine("CurrentTime:" + CurrentTime + ". FoundBigVelocityTime:" + FoundBigVelocityTime);*/

            return retval;
        }

        public static bool IsVelocity()
        {
            float retcheck = Math.Abs(CurrentTime - FoundVelocityTime);
            bool retval = retcheck < 2.0f && retcheck >= 0f;

            /* if (retval)
                 Console.WriteLine("CurrentTime:" + CurrentTime + ". FoundBigVelocityTime:" + FoundBigVelocityTime);*/

            return retval;
        }

        public static bool IsViewChanged()
        {
            float retcheck = Math.Abs(CurrentTime - LastViewChange);
            bool retval = retcheck < 1.5f && retcheck >= 0f;

            /*if (retval)
                Console.WriteLine("CurrentTime:" + CurrentTime + ". LastViewChange:" + LastViewChange);*/

            return retval;
        }

        public static bool IsGameStartSecond()
        {
            bool retval = CurrentGameSecond > 1.0f || CurrentGameSecond2 > 1.0f || CurrentFrameIdAll > 100.0f;

            return retval;
        }

        public static bool IsTakeDamage(float val = 0.50f)
        {
            float retcheck = Math.Abs(CurrentTime - LastDamageTime);
            bool retval = retcheck < val && retcheck >= 0.0f;

            /* if (retval)
                 Console.WriteLine("CurrentTime:" + CurrentTime + ". LastDamageTime:" + LastDamageTime);*/

            return retval;
        }

        public static bool IsPlayerFrozen()
        {
            float retcheck = Math.Abs(CurrentTime - PlayerUnFrozenTime);
            bool retval = PlayerFrozen || retcheck < 4.5f && retcheck >= 0f;

            /* if (retval)
                 Console.WriteLine("CurrentTime:" + CurrentTime + ". PlayerUnFrozenTime:" + PlayerUnFrozenTime);*/

            return retval;
        }

        public static bool IsPlayerTeleport()
        {
            float retcheck = Math.Abs(CurrentTime - LastTeleportusTime);
            bool retval = retcheck < 0.50f && retcheck >= 0f;

            /*if (retval)
                Console.WriteLine("CurrentTime:" + CurrentTime + ". LastTeleportusTime:" + LastTeleportusTime);*/

            return retval;
        }

        public static bool IsPlayerAttackedPressed()
        {
            float retcheck = Math.Abs(CurrentTime - LastAttackPressed);
            bool retval = retcheck < 3.00f && retcheck >= 0f;
            retcheck = Math.Abs(CurrentTime - LastAttackTime);
            retval = retval || retcheck < 3.00f && retcheck >= 0f;
            /*if (retval)
                Console.WriteLine("CurrentTime:" + CurrentTime + ". LastAttackPressed:" + LastAttackPressed);
            */
            return retval;
        }

        public static bool IsPlayerBtnAttackedPressed()
        {
            float retcheck = Math.Abs(CurrentTime - LastAttackPressed);
            bool retval = retcheck < 1.00f && retcheck >= 0f;
            /*if (retval)
                Console.WriteLine("CurrentTime:" + CurrentTime + ". LastAttackPressed:" + LastAttackPressed);
            */
            return retval;
        }

        public static bool IsPlayerBtnJumpPressed()
        {
            float retcheck = Math.Abs(CurrentTime - LastJumpBtnTime);
            bool retval = retcheck < 2.00f && retcheck >= 0f;
            /*if (retval)
                Console.WriteLine("CurrentTime:" + CurrentTime + ". LastJumpBtnTime:" + LastJumpBtnTime);
            */
            return retval;
        }

        public static bool IsPlayerAnyJumpPressed()
        {
            float retcheck = Math.Abs(CurrentTime - LastJumpBtnTime);
            bool retval = retcheck < 2.00f && retcheck >= 0f;
            retcheck = Math.Abs(CurrentTime - LastJumpTime);
            retval = retval || retcheck < 2.00f && retcheck >= 0f;
            /*if (retval)
                Console.WriteLine("CurrentTime:" + CurrentTime + ". LastJumpTime:" + LastJumpBtnTime);
            */
            return retval;
        }

        public static bool IsPlayerInDuck()
        {
            float retcheck = Math.Abs(CurrentTime - LastCmdDuckTime);
            bool retval = retcheck < 1.02f && retcheck >= 0f;
            /*
             Console.WriteLine("CurrentTime:" + CurrentTime + ". LastDuckUnduckTime:" + LastDuckUnduckTime);
             */
            return retval;
        }

        public static bool IsPlayerUnDuck()
        {
            float retcheck = Math.Abs(CurrentTime - LastCmdUnduckTime);
            bool retval = retcheck < 0.21f && retcheck >= 0f;
            /*
             Console.WriteLine("CurrentTime:" + CurrentTime + ". LastDuckUnduckTime:" + LastDuckUnduckTime);
             */
            return retval;
        }

        public static bool IsAngleEditByEngine()
        {
            if (NO_TELEPORT) return false;

            return /*BadPunchAngle ||*/
                IsPlayerTeleport() ||
                Math.Abs(CurrentTime - LastAngleManipulation) < 0.50f ||
                IsPlayerInDuck() ||
                IsPlayerUnDuck() ||
                IsTakeDamage() ||
                IsPlayerFrozen() ||
                IsViewChanged() ||
                HideWeapon ||
               Math.Abs(CurrentTime - LastLookDisabled) < 0.75f;
        }

        public static bool IsAngleEditByEngineForLearn()
        {
            if (NO_TELEPORT) return false;

            return
                IsPlayerTeleport() ||
                CurrentTime - LastAngleManipulation < 0.20f ||
                IsPlayerInDuck() ||
                IsPlayerUnDuck() ||
                IsTakeDamage() ||
                IsPlayerFrozen() ||
                IsViewChanged() ||
                HideWeapon ||
                Math.Abs(CurrentTime - LastLookDisabled) < 0.4f;
        }


        public static AngleDirection GetAngleDirection(float val1, float val2)
        {
            uint uval1 = (uint)(val1 / 60);
            uint uval2 = (uint)(val2 / 60);

            if (Math.Abs(val1 - val2) < float.Epsilon) return AngleDirection.AngleDirectionNO;

            if (uval1 == uval2)
            {
                if (val2 < val1)
                    return AngleDirection.AngleDirectionLeft;
                return AngleDirection.AngleDirectionRight;
            }

            switch (uval1)
            {
                case 0:
                    if (uval2 == 1 || uval2 == 2 || uval2 == 3)
                        return AngleDirection.AngleDirectionRight;
                    else
                        return AngleDirection.AngleDirectionLeft;

                case 1:
                    if (uval2 == 2 || uval2 == 3 || uval2 == 4)
                        return AngleDirection.AngleDirectionRight;
                    else
                        return AngleDirection.AngleDirectionLeft;

                case 2:
                    if (uval2 == 3 || uval2 == 4 || uval2 == 5)
                        return AngleDirection.AngleDirectionRight;
                    else
                        return AngleDirection.AngleDirectionLeft;

                case 3:
                    if (uval2 == 4 || uval2 == 5 || uval2 == 6)
                        return AngleDirection.AngleDirectionRight;
                    else
                        return AngleDirection.AngleDirectionLeft;

                case 4:
                    if (uval2 == 5 || uval2 == 6 || uval2 == 0)
                        return AngleDirection.AngleDirectionRight;
                    else
                        return AngleDirection.AngleDirectionLeft;

                case 5:
                    if (uval2 == 6 || uval2 == 0 || uval2 == 1)
                        return AngleDirection.AngleDirectionRight;
                    else
                        return AngleDirection.AngleDirectionLeft;

                case 6:
                    if (uval2 == 0 || uval2 == 1 || uval2 == 2)
                        return AngleDirection.AngleDirectionRight;
                    else
                        return AngleDirection.AngleDirectionLeft;
            }
            return 0;
        }

        public static void ProcessPluginMessage(string cmd)
        {
            if (DUMP_ALL_FRAMES) OutDumpString += "\n{ UCMD PLUGIN MESSAGE:" + cmd + "}\n";
            string[] cmdList = cmd.Split('/');
            if (cmdList.Length > 0)
                if (cmdList[0] == "UDS")
                    if (cmdList.Length > 2)
                    {
                        if (cmdList[1] == "AUTH")
                        {
                            SteamID = cmdList[2];
                        }
                        else if (cmdList[1] == "DATE")
                        {
                            RecordDate = cmdList[2];
                            if (NeedReportDateAndAuth)
                            {
                                NeedReportDateAndAuth = false;
                                DemoScanner_AddWarn("[INFO] " + SteamID +
                                                    ". Record date:" + RecordDate, true, false, true, true);
                            }
                        }
                        else if (cmdList[1] == "VER" || cmdList[1] == "UCMD")
                        {
                            if (PluginVersion.Length == 0)
                            {
                                if (cmdList[1] == "UCMD")
                                    PluginVersion = "< 1.5";
                                else
                                    PluginVersion = cmdList[2];

                                DemoScanner_AddWarn("[INFO] Found module version " + PluginVersion, true, false, true, true);


                                if (cmdList[1] == "UCMD" || PluginVersion == "1.3" || PluginVersion == "1.2" || PluginVersion == "1.4")
                                {
                                    if (IsRussia)
                                    {
                                        DemoScanner_AddWarn("[INFO] На сервере старая версия плагина", true, false, true, true);
                                        DemoScanner_AddWarn("[ERROR] Отключается обнаружение [JUMPHACK TYPE 5] и [AIM TYPE 1.6]", true, false, true, true);
                                    }
                                    else
                                    {
                                        DemoScanner_AddWarn("[INFO] Old plugin version at server.", true, false, true, true);
                                        DemoScanner_AddWarn("[ERROR] Disabled detection of [JUMPHACK TYPE 5] and [AIM TYPE 1.6]", true, false, true, true);
                                    }
                                    DisableJump5AndAim16 = true;
                                }
                            }
                        }
                        else if (cmdList[1] == "JMP")
                        {
                            int id = int.Parse(cmdList[2]);
                            if (IsUserAlive()) JumpCount6++;
                            if (id == 1)
                                DemoScanner.SearchJumpHack5 = 5;
                            else
                                DemoScanner.SearchJumpHack51 = 5;
                        }
                        else if (cmdList[1] == "XCMD")
                        {
                            if (IsUserAlive()) attackscounter2++;

                            int lerpms = int.Parse(cmdList[2]);
                            byte ms = Convert.ToByte(int.Parse(cmdList[3]));
                            int incomingframenum = int.Parse(cmdList[4]);

                            if (!IsPlayerBtnAttackedPressed() && FirstAttack && IsUserAlive() && !DisableJump5AndAim16)
                            {
                                /* if (AUTO_LEARN_HACK_DB)
                                 {
                                     ENABLE_LEARN_HACK_DEMO = true;
                                     ENABLE_LEARN_HACK_DEMO_FORCE_SAVE = true;
                                 }*/
                                DemoScanner_AddWarn(
                                    "[AIM TYPE 1.6 " + CurrentWeapon + "] at (" + CurrentTime +
                                    "):" + CurrentTimeString, !IsChangeWeapon() && !IsAngleEditByEngine() && !IsPlayerLossConnection() && !IsForceCenterView());
                                TotalAimBotDetected++;
                            }

                            if (IsUserAlive() && FirstJump && Math.Abs(CurrentTime) > float.Epsilon)
                                if (ms <= 1)
                                {
                                    if (Math.Abs(CurrentTime - LastCmdHack) > 5.0)
                                        DemoScanner_AddWarn(
                                            "[CMD HACK TYPE 3] at (" +
                                            CurrentTime + ") " + CurrentTimeString, ms == 0 && !IsAngleEditByEngine() && CurrentFps < 500 && CurrentFps2 < 500);
                                    //Console.WriteLine("BAD BAD " + nf.UCmd.Msec + " / " + nf.RParms.Frametime + " = " + ((float)nf.UCmd.Msec / nf.RParms.Frametime).ToString());
                                    LastCmdHack = CurrentTime;
                                }

                            if (PluginFrameNum < 0 || incomingframenum < PluginFrameNum)
                            {
                                PluginFrameNum = incomingframenum;
                            }
                            else
                            {
                                if (Math.Abs(incomingframenum - PluginFrameNum) > 1)
                                {
                                    if (Math.Abs(CurrentTime - LastCmdHack) > 5.0)
                                        DemoScanner_AddWarn(
                                            "[CMD HACK TYPE 5] at (" +
                                            CurrentTime + ") " + CurrentTimeString, !IsAngleEditByEngine());
                                    //Console.WriteLine("BAD BAD " + nf.UCmd.Msec + " / " + nf.RParms.Frametime + " = " + ((float)nf.UCmd.Msec / nf.RParms.Frametime).ToString());
                                    LastCmdHack = CurrentTime;
                                }
                                PluginFrameNum = incomingframenum;
                            }

                            if (DUMP_ALL_FRAMES) OutDumpString += "\n{ UCMD PLUGIN. Lerp " + lerpms + ". ms " + ms + "}\n";
                            if (!FindLerpAndMs(lerpms, ms))
                            {
                                DemoScanner_AddWarn("[FAKELAG] at (" + CurrentTime +
                                                    "):" + CurrentTimeString, true, true, false, true);
                                FakeLagAim++;
                            }
                        }
                        else if (cmdList[1] == "EVENT" && !DisableJump5AndAim16)
                        {
                            if (DUMP_ALL_FRAMES) OutDumpString += "\n{ EVENT PLUGIN }\n";
                            if (PluginEvents == -1)
                            {
                                CurrentEvents = 0;
                                PluginEvents = 0;
                            }
                            else
                            {
                                if (Math.Abs(PluginEvents - CurrentEvents) > 3)
                                {
                                    if (CurrentEvents != 0)
                                        DemoScanner_AddWarn("[EXPERIMENTAL][UNKNOWN HACK<" + CurrentEvents + "><" + PluginEvents + ">] at (" + CurrentTime +
                                                            "):" + CurrentTimeString, !NotFirstEventShift, true, false, true);
                                    else
                                        BadEvents += 8;
                                    CurrentEvents = 0;
                                    PluginEvents = 0;
                                    NotFirstEventShift = false;
                                }
                                PluginEvents++;
                            }

                        }
                        else if (cmdList[1] == "BAD")
                        {
                            if (cmdList[2] == "1")
                            {
                                if (IsRussia)
                                {
                                    DemoScanner_AddWarn("[INFO] На сервере слишком низкий sv_minrate или sv_maxrate", true, false, true, true);
                                    DemoScanner_AddWarn("[ERROR] Отключается обнаружение [JUMPHACK TYPE 5] и [AIM TYPE 1.6]", true, false, true, true);
                                }
                                else
                                {
                                    DemoScanner_AddWarn("[INFO] Small sv_minrate or sv_maxrate value at server", true, false, true, true);
                                    DemoScanner_AddWarn("[ERROR] Disabled detection of [JUMPHACK TYPE 5] and [AIM TYPE 1.6]", true, false, true, true);
                                }
                            }
                            else
                            {
                                if (IsRussia)
                                {
                                    DemoScanner_AddWarn("[INFO] На сервере слишком низкий sv_minupdaterate", true, false, true, true);
                                    DemoScanner_AddWarn("[ERROR] Отключается обнаружение [JUMPHACK TYPE 5] и [AIM TYPE 1.6]", true, false, true, true);
                                }
                                else
                                {
                                    DemoScanner_AddWarn("[INFO] Small sv_maxupdaterate or sv_minupdaterate at server", true, false, true, true);
                                    DemoScanner_AddWarn("[ERROR] Disabled detection of [JUMPHACK TYPE 5] and [AIM TYPE 1.6]", true, false, true, true);
                                }
                            }

                            DisableJump5AndAim16 = true;
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
                                 DemoScanner.OutDumpString += "\n{ATTACK ANGLE: " + angle + " " + CurrentTimeString + " " + CurrentTime + " }\n";
                             }
                         }*/
                    }
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
            public bool Log;
            public bool Visited;
            public bool SkipAllChecks;
            public float WarnTime;
            public bool Plugin;
        }

        public struct AngleSearcher
        {
            public float angle;
            public int searchcount;
            public float searchtime;
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

        public class Player : IDisposable
        {
            public Dictionary<string, string> InfoKeys;
            public string Name;
            public int pid;
            public int Slot;
            public string Steam;
            public BinaryWriter voicedata;
            public Stream voicedata_stream;
            private int VoiceExploit;

            public Player(int slot)
            {
                pid = cipid++;
                VoiceExploit = 0;
                Name = string.Empty;
                Slot = slot;
                InfoKeys = new Dictionary<string, string>();
                voicedata_stream = new MemoryStream();
                voicedata = new BinaryWriter(voicedata_stream);
                Steam = "NOSTEAM";
            }

            void IDisposable.Dispose()
            {
                voicedata.Close();
                voicedata_stream.Close();
            }

            public void WriteVoice(int len, byte[] data)
            {
                if (len > 0)
                {
                    voicedata.Write((int)len);
                    voicedata.Write(data);
                }
            }
        }
    }

    public static class Common
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

        public delegate void Procedure<T1, T2, T3, T4>(T1 arg1, T2 arg2, T4 arg4);


        /// <summary>
        ///     Calculates a Steam ID from a "*sid" Half-Life infokey value.
        /// </summary>
        /// <param name="sidInfoKeyValue">The "*sid" infokey value.</param>
        /// <returns>A Steam ID in the format "STEAM_0:x:y".</returns>
        public static string CalculateSteamId(string sidInfoKeyValue)
        {
            if (!long.TryParse(sidInfoKeyValue, out long id) || id == 0)
                // HLTV proxy or LAN dedicated server.
                return null;

            long universe = id >> 56;
            long type = (id >> 52) & 0xF;
            long instance = (id >> 32) & 0xFFFFF;
            long accountid = id & 0xFFFFFFFF;
            if (universe == 1) universe = 0;

            return string.Format("STEAM_{0}:{1}:{2}", universe, accountid & 1, accountid >> 1);
        }
    }

    public abstract class DemoParser
    {
        private readonly Hashtable messageHandlerTable; // id -> length and callback

        private readonly Hashtable
            messageStringTable; // just for logging purposes, id -> name

        protected CrossParseResult demo;
        protected BinaryReader fileReader;
        protected FileStream fileStream;
        private byte[] messageLogFrameData;
        private long messageLogFrameOffset;

        // message logging
        private Queue messageLogQueue;

        public DemoParser()
        {
            messageHandlerTable = new Hashtable();
            messageStringTable = new Hashtable();
        }

        public void Open()
        {
            fileStream = File.OpenRead(demo.GsDemoInfo.FileName);
            fileReader = new BinaryReader(fileStream);
        }

        public void Close()
        {
            if (fileReader != null) fileReader.Close();

            if (fileStream != null) fileStream.Close();
        }

        public void AddMessageHandler(byte id)
        {
            AddMessageHandler(id, -1, null);
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

        private void AddMessageHandler(byte id, int length, Procedure callback)
        {
            MessageHandler newHandler = new MessageHandler
            {
                Id = id,
                Length = length,
                Callback = callback
            };

            // replace message handler if it already exists
            if (messageHandlerTable.Contains(id)) messageHandlerTable.Remove(id);

            messageHandlerTable.Add(newHandler.Id, newHandler);
        }

        protected MessageHandler FindMessageHandler(byte messageId)
        {
            return (MessageHandler)messageHandlerTable[messageId];
        }

        protected void AddMessageIdString(byte id, string s)
        {
            if (messageStringTable[id] != null) messageStringTable.Remove(id);

            messageStringTable.Add(id, s);
        }

        public string FindMessageIdString(byte id)
        {
            string s = (string)messageStringTable[id];

            if (s == null) return "UNKNOWN MESSAGE";

            return s;
        }

        protected void BeginMessageLog(long frameOffset, byte[] frameData)
        {
            messageLogFrameOffset = frameOffset;
            messageLogFrameData = frameData;
            messageLogQueue = new Queue();
        }

        protected void LogMessage(byte id, string name, int offset)
        {
            MessageLog log = new MessageLog
            {
                Id = id,
                Name = name,
                Offset = offset
            };

            messageLogQueue.Enqueue(log);
        }

        private class MessageLog
        {
            public byte Id;
            public string Name;
            public int Offset;
        }

        protected class MessageHandler
        {
            public Procedure Callback;
            public byte Id;
            public int Length;
        }

        #region Properties

        public BinaryReader Reader => fileReader;

        public long Position => fileStream.Position;

        public long FileLength => fileStream.Length;

        #endregion
    }

    public class HalfLifeDemoParser : DemoParser
    {
        public enum MessageId : byte
        {
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
        public static bool SkipNextErrors;
        private readonly Hashtable deltaDecoderTable;

        private readonly Hashtable
            userMessageCallbackTable; // name -> Common.NoArgsDelegate callback

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

            HalfLifeDeltaStructure deltaDescription = new HalfLifeDeltaStructure("delta_description_t");
            AddDeltaStructure(deltaDescription);

            deltaDescription.AddEntry("flags", 32, 1.0f, 1.0f,
                HalfLifeDeltaStructure.EntryFlags.Integer);
            deltaDescription.AddEntry("name", 8, 1.0f, 1.0f,
                HalfLifeDeltaStructure.EntryFlags.String);
            deltaDescription.AddEntry("offset", 16, 1.0f, 1.0f,
                HalfLifeDeltaStructure.EntryFlags.Integer);
            deltaDescription.AddEntry("size", 8, 1.0f, 1.0f,
                HalfLifeDeltaStructure.EntryFlags.Integer);
            deltaDescription.AddEntry("nBits", 8, 1.0f, 1.0f,
                HalfLifeDeltaStructure.EntryFlags.Integer);
            deltaDescription.AddEntry("divisor", 32, 4000.0f, 1.0f,
                HalfLifeDeltaStructure.EntryFlags.Float);
            deltaDescription.AddEntry("preMultiplier", 32, 4000.0f, 1.0f,
                HalfLifeDeltaStructure.EntryFlags.Float);


            // message handlers
            AddMessageHandler((byte)MessageId.svc_nop, 0);
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
            AddMessageHandler((byte)MessageId.svc_updateuserinfo,
                MessageUpdateUserInfo);
            AddMessageHandler((byte)MessageId.svc_deltadescription,
                MessageDeltaDescription);
            AddMessageHandler((byte)MessageId.svc_clientdata, MessageClientData);
            AddMessageHandler((byte)MessageId.svc_stopsound, 2);
            AddMessageHandler((byte)MessageId.svc_pings, MessagePings);
            AddMessageHandler((byte)MessageId.svc_particle, 11);
            AddMessageHandler((byte)MessageId.svc_spawnstatic, MessageSpawnStatic);
            AddMessageHandler((byte)MessageId.svc_event_reliable,
                MessageEventReliable);
            AddMessageHandler((byte)MessageId.svc_spawnbaseline, MessageSpawnBaseline);
            AddMessageHandler((byte)MessageId.svc_tempentity, MessageTempEntity);
            AddMessageHandler((byte)MessageId.svc_setpause, MessageSetPause);
            AddMessageHandler((byte)MessageId.svc_signonnum, 1);
            AddMessageHandler((byte)MessageId.svc_centerprint, MessageCenterPrint);
            AddMessageHandler((byte)MessageId.svc_spawnstaticsound, 14);
            AddMessageHandler((byte)MessageId.svc_intermission, MessageInterMission);
            AddMessageHandler((byte)MessageId.svc_finale, 1);
            AddMessageHandler((byte)MessageId.svc_cdtrack, 2);
            AddMessageHandler((byte)MessageId.svc_weaponanim, 2);
            AddMessageHandler((byte)MessageId.svc_roomtype, MessageRoomType);
            AddMessageHandler((byte)MessageId.svc_addangle, MessageAddAngle);
            AddMessageHandler((byte)MessageId.svc_newusermsg, MessageNewUserMsg);
            AddMessageHandler((byte)MessageId.svc_packetentities,
                MessagePacketEntities);
            AddMessageHandler((byte)MessageId.svc_deltapacketentities,
                MessageDeltaPacketEntities);
            AddMessageHandler((byte)MessageId.svc_choke, MessageChoke);
            AddMessageHandler((byte)MessageId.svc_resourcelist, MessageResourceList);
            AddMessageHandler((byte)MessageId.svc_newmovevars, MessageNewMoveVars);
            AddMessageHandler((byte)MessageId.svc_resourcerequest, 8);
            AddMessageHandler((byte)MessageId.svc_customization, MessageCustomization);
            AddMessageHandler((byte)MessageId.svc_crosshairangle, 2);
            AddMessageHandler((byte)MessageId.svc_soundfade, 4);
            AddMessageHandler((byte)MessageId.svc_filetxferfailed,
                MessageFileTransferFailed);
            AddMessageHandler((byte)MessageId.svc_hltv, MessageHltv);
            AddMessageHandler((byte)MessageId.svc_director, MessageDirector);
            AddMessageHandler((byte)MessageId.svc_voiceinit, MessageVoiceInit);
            AddMessageHandler((byte)MessageId.svc_voicedata, MessageVoiceData);
            AddMessageHandler((byte)MessageId.svc_sendextrainfo, MessageSendExtraInfo);
            AddMessageHandler((byte)MessageId.svc_timescale, 4);
            AddMessageHandler((byte)MessageId.svc_resourcelocation,
                MessageResourceLocation);
            AddMessageHandler((byte)MessageId.svc_sendcvarvalue, MessageSendCvarValue);
            AddMessageHandler((byte)MessageId.svc_sendcvarvalue2,
                MessageSendCvarValue2);

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

        }

        // public so svc_deltadescription can be parsed elsewhere
        public void AddDeltaStructure(HalfLifeDeltaStructure structure)
        {
            // remove decoder if it already exists (duplicate svc_deltadescription message)
            // e.g. GotFrag Demo 6 (rs vs TSO).zip

            if (deltaDecoderTable[structure.Name] != null) deltaDecoderTable.Remove(structure.Name);

            deltaDecoderTable.Add(structure.Name, structure);
        }

        public HalfLifeDeltaStructure GetDeltaStructure(string name)
        {
            HalfLifeDeltaStructure structure = (HalfLifeDeltaStructure)deltaDecoderTable[name];

            if (structure == null) structure = new HalfLifeDeltaStructure(name);

            return structure;
        }

        // public so that svc_newusermsg can be parsed elsewhere
        public void AddUserMessage(byte id, sbyte length, string name)
        {
            // some demos contain duplicate user message definitions
            // e.g. GotFrag Demo 13 (mTw.nine vs CosaNostra).zip
            // others, due to what seems like completely fucking retarded servers, have many copies of the 
            // same user message registered with different id's
            if (userMessageTable.Contains(name)) userMessageTable.Remove(name);

            UserMessage userMessage = new UserMessage
            {
                Id = id,
                Length = length
            };

            userMessageTable.Add(name, userMessage);
            AddMessageIdString(id, name);

            // see if there's a handler waiting to be attached to this message
            Procedure callback = (Procedure)userMessageCallbackTable[name];

            if (callback == null)
                AddMessageHandler(id, length);
            else
                AddMessageHandler(id, callback);
        }

        public void AddUserMessageHandler(string name, Procedure callback)
        {
            // override existing callback
            if (userMessageCallbackTable.Contains(name)) userMessageCallbackTable.Remove(name);

            userMessageCallbackTable.Add(name, callback);

            // TODO: assert that all svc_newusermsg messages have been parsed?

            // find user message id
            /*UserMessage userMessage = (UserMessage)userMessageTable[name];

            // should be ok if message doesn't exist, since different mods may not have certain messages
            if (userMessage != null)
            {
                // add message handler
                AddMessageHandler(userMessage.Id, callback);
            }*/
        }

        public int FindUserMessageLength(string name)
        {
            // shouldn't return null, since this method should be called from a message handler, and that message handler couldn't be called if there wasn't a UserMessage entry
            UserMessage userMessage = (UserMessage)userMessageTable[name];

            if (userMessage.Length == -1)
                // if svc_newusermsg length is -1, first byte is length
                return BitBuffer.ReadByte();

            return userMessage.Length;
        }

        public FrameHeader ReadFrameHeader()
        {
            FrameHeader header = new FrameHeader
            {
                Type = fileReader.ReadByte(),
                Timestamp = fileReader.ReadSingle(),
                Number = fileReader.ReadUInt32()
            };

            InLoadingSegment = header.Type == 0;

            return header;
        }

        public int GetFrameLength(byte frameType)
        {
            int length = 0;

            switch (frameType)
            {
                case 2: // ???
                    break;

                case 3: // client command
                    length = 64;
                    break;

                case 4:
                    length = 32;
                    break;

                case 5: // end of segment
                    break;

                case 6:
                    length = 84;
                    break;

                case 7:
                    length = 8;
                    break;

                case 8:
                    Seek(4);
                    length = fileReader.ReadInt32();
                    Seek(-8);
                    length += 24;
                    break;

                case 9:
                    length = 4 + fileReader.ReadInt32();
                    Seek(-4);
                    break;

                default:
                    throw new ApplicationException("Unknown frame type.");
            }

            return length;
        }

        public void SkipFrame(byte frameType)
        {
            Seek(GetFrameLength(frameType));
        }

        public GameDataFrameHeader ReadGameDataFrameHeader()
        {
            GameDataFrameHeader header = new GameDataFrameHeader();

            if (demo.GsDemoInfo.Header.NetProtocol <= 43)
            {
                // TODO: read resolution
                Seek(560);

                // 264 bytes
                // movevars
            }
            else
            {
                // 464 bytes
                Seek(220);
                header.ResolutionWidth = fileReader.ReadUInt32();
                header.ResolutionHeight = fileReader.ReadUInt32();
                // supposedly bpp here (int)
                // TODO: check if true
                Seek(236);

                // 436 bytes "demo info"
                // -first 64 bytes: view angles (16 floats)
                //      -0 (4 bytes)
                //      -x, y, z (12 bytes)
                //      -pitch, yaw... presumably roll
                // -156 bytes unknown
                // -8 bytes: resolution
                // -60 bytes unknown
                // -98 bytes + string (0?): movevars
                // -81 bytes unknown (assuming string is null)
                //      last 15 bytes control view model animation

                // 28 bytes (7 ints) "sequence info"
            }

            header.Length = fileReader.ReadUInt32();

            return header;
        }

        public void ParseGameDataMessages(byte[] frameData)
        {
            if (ErrorCount > 100) return;

            if (DemoScanner.DUMP_ALL_FRAMES) DemoScanner.OutDumpString += "{\n";

            try
            {
                ParseGameDataMessages(frameData, null);
            }
            catch (Exception ex)
            {
                ErrorCount++;
                ConsoleColor tmpcolor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("(E=" + ex.Message + ")");
                Console.ForegroundColor = tmpcolor;
                if (ErrorCount > 5)
                {
                    if (DemoScanner.DUMP_ALL_FRAMES) DemoScanner.OutDumpString += "FATAL ERROR. STOP MESSAGE PARSING.\n}\n";

                    if (!SkipNextErrors && MessageBox.Show(
                            "Error while demo parsing.\nPossible demo is HLTV or too old\nPlease convert demo using coldemoplayer and try again!\nContinue?? (Skip all errors but got bad result)",
                            "Something wrong!", MessageBoxButtons.YesNo) !=
                        DialogResult.Yes)
                        Environment.Exit(-1);

                    SkipNextErrors = true;
                    return;
                }

                if (DemoScanner.DUMP_ALL_FRAMES) DemoScanner.OutDumpString += "ERROR IN PARSE MESSAGE.";
            }
            //catch
            //{
            //    ErrorCount++;
            //    var tmpcolor = Console.ForegroundColor;
            //    Console.ForegroundColor = ConsoleColor.Red;
            //    Console.Write("(E)");
            //    Console.ForegroundColor = tmpcolor;
            //    if (ErrorCount > 5)
            //    {
            //        if (DemoScanner.DUMP_ALL_FRAMES)
            //            DemoScanner.OutDumpString += "FATAL ERROR. STOP MESSAGE PARSING.\n}\n";
            //        if (!SkipNextErrors && MessageBox.Show(
            //                "Error while demo parsing.\nPossible demo is HLTV or too old\nPlease convert demo using coldemoplayer and try again!\nContinue?? (Skip all errors but got bad result)",
            //                "Something wrong!", MessageBoxButtons.YesNo) !=
            //            DialogResult.Yes) Environment.Exit(-1);
            //        SkipNextErrors = true;
            //        return;
            //    }

            //    if (DemoScanner.DUMP_ALL_FRAMES) DemoScanner.OutDumpString += "ERROR IN PARSE MESSAGE.";
            //}

            if (DemoScanner.DUMP_ALL_FRAMES) DemoScanner.OutDumpString += "}\n";
        }

        public void ParseGameDataMessages(byte[] frameData,
            Function<byte, byte> userMessageCallback)
        {
            // read game data frame into memory
            BitBuffer = new BitBuffer(frameData);
            readingGameData = true;
            int messageFrameOffsetOld = 0;
            if (frameData.Length == 0 && BitBuffer.CurrentByte == 0)
                //DemoScanner.EmptyFrames++;
                return;
            // start parsing messages
            while (true)
            {
                DemoScanner.MessageId += 1;
                if (DemoScanner.DUMP_ALL_FRAMES)
                    DemoScanner.OutDumpString +=
                        "{MSGLEN-" + frameData.Length + ".MSGBYTE:" + BitBuffer.CurrentByte;

                byte messageId = BitBuffer.ReadByte();

                if (DemoScanner.DUMP_ALL_FRAMES) DemoScanner.OutDumpString += "MSGID:" + messageId + ".MSGBYTE:" + BitBuffer.CurrentByte;
                // File.AppendAllText("messages.bin", messageId + "\n");

                string messageName = Enum.GetName(typeof(MessageId), messageId);

                if (messageName == null) // a user message, presumably
                    messageName = FindMessageIdString(messageId);
                if (DemoScanner.DUMP_ALL_FRAMES) DemoScanner.OutDumpString += ".MSGNAME:" + messageName + "}\n";

                MessageHandler messageHandler = FindMessageHandler(messageId);


                if (messageName == "svc_intermission")
                {
                    if (!DemoScanner.GameEnd)
                    {
                        DemoScanner.GameEnd = true;
                        DemoScanner.GameEndTime = DemoScanner.CurrentTime;
                        Console.WriteLine("---------- [Конец игры / Game Over (" + DemoScanner.CurrentTimeString + ")] ----------");
                    }
                    else
                    {
                        Console.WriteLine("---------- [Конец игры 2 / Game Over 2 (" + DemoScanner.CurrentTimeString + ")] ----------");
                    }
                }

                // Handle the conversion of user message id's.
                // Used by demo writing to convert to the current network protocol.
                if (messageId > 64 && userMessageCallback != null)
                {
                    byte newMessageId = userMessageCallback(messageId);

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
                        DemoScanner.UnknownMessages++;
                        DemoScanner.CheckConsoleCommand("Unknown message id:" + messageId, true);
                    }
                    break;
                }

                // callback takes priority over length
                if (messageHandler.Callback != null)
                {
                    messageHandler.Callback();
                }
                else if (messageHandler.Length != -1)
                {
                    Seek(messageHandler.Length);
                }
                else if (messageId >= 64)
                {
                    // All non-engine user messages start with a byte that is the number of bytes in the message remaining.
                    byte length = BitBuffer.ReadByte();

                    Seek(length);
                }
                else
                {
                    throw new ApplicationException(string.Format("Unknown message id \"{0}\"", messageId));
                }

                // Check if we've reached the end of the frame, or if any of the messages have called SkipGameDataFrame (readingGameData will be false).
                if (BitBuffer.CurrentByte < 0 || BitBuffer.CurrentByte >= BitBuffer.Length || !readingGameData) break;

                if (BitBuffer.CurrentByte >= messageFrameOffsetOld)
                    messageFrameOffsetOld = BitBuffer.CurrentByte;
                else
                    //MessageBox.Show("error");
                    throw new ApplicationException(string.Format("Offset error message \"{0}\"", 1234));
            }

            readingGameData = false;
        }

        public void SkipGameDataFrame()
        {
            readingGameData = false;
        }

        public void Seek(long deltaOffset)
        {
            Seek(deltaOffset, SeekOrigin.Current);
        }

        public void Seek(long offset, SeekOrigin origin)
        {
            if (readingGameData)
                //Debug.Assert(offset <= int.MaxValue);
                BitBuffer.SeekBytes((int)offset, origin);
            else
                fileStream.Seek(offset, origin);
        }

        public float ReadCoord() // TODO: move to bitbuffer?
        {
            return BitBuffer.ReadInt16() * (1.0f / 8.0f);
        }

        private class UserMessage
        {
            public byte Id;
            public sbyte Length;
        }

        public class FrameHeader
        {
            public uint Number;
            public float Timestamp;
            public byte Type;
        }

        public class GameDataFrameHeader
        {
            public uint Length;
            public uint ResolutionHeight;
            public uint ResolutionWidth;
        }

        #region Properties

        public bool InLoadingSegment { get; private set; } = true;

        public BitBuffer BitBuffer { get; private set; }

        public int GameDataDemoInfoLength
        {
            get
            {
                if (demo.GsDemoInfo.Header.NetProtocol <= 43) return 532;

                return 436;
            }
        }

        // doesn't need to be a property, but for consistencies sake...
        public int GameDataSequenceInfoLength => 28;

        #endregion

        #region Engine Messages

        private void MessageDisconnect()
        {
            if (DemoScanner.DUMP_ALL_FRAMES)
                DemoScanner.OutDumpString += "MessageDisconnect:" + BitBuffer.ReadString();
            else
                BitBuffer.ReadString();
        }

        public void MessageEvent()
        {
            if (demo.GsDemoInfo.Header.NetProtocol <= 43) BitBuffer.Endian = BitBuffer.EndianType.Big;

            uint nEvents = BitBuffer.ReadUnsignedBits(5);
            if (DemoScanner.DUMP_ALL_FRAMES) DemoScanner.OutDumpString += "MessageEvents:(" + nEvents + "){\n";

            for (int i = 0; i < nEvents; i++)
            {
                if (DemoScanner.DUMP_ALL_FRAMES)
                {
                    DemoScanner.OutDumpString +=
                        "index:" + BitBuffer.ReadUnsignedBits(10); // event index
                    DemoScanner.OutDumpString += "{\n";
                }
                else
                {
                    BitBuffer.ReadUnsignedBits(10);
                }

                bool packetIndexBit = BitBuffer.ReadBoolean();

                if (packetIndexBit)
                {
                    BitBuffer.SeekBits(11); // packet index

                    bool deltaBit = BitBuffer.ReadBoolean();

                    if (deltaBit) GetDeltaStructure("event_t").ReadDelta(BitBuffer, null);
                }

                bool fireTimeBit = BitBuffer.ReadBoolean();

                if (fireTimeBit)
                {
                    if (DemoScanner.DUMP_ALL_FRAMES)
                        DemoScanner.OutDumpString += "TIME:" + BitBuffer.ReadUnsignedBits(16);
                    else
                        BitBuffer.ReadUnsignedBits(16);
                    //BitBuffer.SeekBits(16); // fire time
                }

                if (DemoScanner.DUMP_ALL_FRAMES) DemoScanner.OutDumpString += "}\n";
            }

            if (DemoScanner.DUMP_ALL_FRAMES) DemoScanner.OutDumpString += "}\n";

            BitBuffer.SkipRemainingBits();
            BitBuffer.Endian = BitBuffer.EndianType.Little;
        }

        private void MessageVersion()
        {
            Seek(4); // uint: server network protocol number.
        }

        private void MessageView()
        {
            int entityview = BitBuffer.ReadInt16();

            if (!DemoScanner.Intermission)
                if (DemoScanner.ViewModel > 0) DemoScanner.ViewEntity = entityview;

            DemoScanner.LastViewChange = DemoScanner.CurrentTime;

            if (DemoScanner.DUMP_ALL_FRAMES) DemoScanner.OutDumpString += "switch view to " + entityview + " player\n";
        }

        public void MessageTime()
        {
            float timemsg = BitBuffer.ReadSingle();
            DemoScanner.SVC_TIMEMSGID = DemoScanner.MessageId;
        }

        public void MessageSound()
        {
            if (demo.GsDemoInfo.Header.NetProtocol <= 43) BitBuffer.Endian = BitBuffer.EndianType.Big;

            uint flags = BitBuffer.ReadUnsignedBits(9);

            int vol = 0;
            if ((flags & (1 << 0)) != 0) // volume
                vol = BitBuffer.ReadBits(8);

            int att = 0;
            if ((flags & (1 << 1)) != 0) // attenuation * 64
                att = BitBuffer.ReadBits(8);

            int channel = BitBuffer.ReadBits(3); // channel
            int edictnum = BitBuffer.ReadBits(11); // edict number
            int soundidx = 0;
            if ((flags & (1 << 2)) != 0) // sound index (short)
                soundidx = BitBuffer.ReadBits(16);
            else // sound index (byte)
                soundidx = BitBuffer.ReadBits(8);

            BitBuffer.ReadVectorCoord(true); // position
            int pitch = 0;
            if ((flags & (1 << 3)) != 0) // pitch
                pitch = BitBuffer.ReadBits(8);

            BitBuffer.SkipRemainingBits();
            BitBuffer.Endian = BitBuffer.EndianType.Little;
            if (channel == 2 && DemoScanner.ViewEntity == edictnum) DemoScanner.LastSoundTime = DemoScanner.CurrentTime;
        }

        private void MessagePrint()
        {
            DemoScanner.CurrentMsgCount++;
            string message = BitBuffer.ReadString();
            if (DemoScanner.DEBUG_ENABLED) Console.WriteLine("PRINT:" + message);
            if (DemoScanner.DUMP_ALL_FRAMES) DemoScanner.OutDumpString += "MessagePrint:" + message;
        }

        private void MessageStuffText()
        {
            DemoScanner.CurrentMsgCount++;
            string stuffstr =
                BitBuffer.ReadString();
            if (DemoScanner.DUMP_ALL_FRAMES) DemoScanner.OutDumpString += "MessageStuffText:" + stuffstr;

            DemoScanner.CheckConsoleCommand(stuffstr, true);

            DemoScanner.LastStuffCmdCommand = stuffstr;



            if (DemoScanner.LastStuffCmdCommand.IndexOf("snapshot") > -1 ||
                DemoScanner.LastStuffCmdCommand.IndexOf("screenshot") > -1)
            {
                //if (DemoScanner.CurrentTime - DemoScanner.LastStrafeDisabled < 3.5f)
                //{
                //    DemoScanner.DemoScanner_AddWarn("Player tried to got black screenshot at " + DemoScanner.CurrentTimeString, false, false);
                //}
                DemoScanner.LastScreenshotTime = DemoScanner.CurrentTime;
                if (DemoScanner.IsRussia)
                    DemoScanner.DemoScanner_AddInfo("Администратор сделал скриншот игроку, время " + DemoScanner.CurrentTimeString);
                else
                    DemoScanner.DemoScanner_AddInfo("Server request player screenshot at " + DemoScanner.CurrentTimeString);
            }
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
            UInt32Union src = new UInt32Union
            {
                Value = value
            };

            UInt32Union dest = new UInt32Union
            {
                Byte1 = src.Byte4,
                Byte2 = src.Byte3,
                Byte3 = src.Byte2,
                Byte4 = src.Byte1
            };

            return dest.Value;
        }

        private int LongSwap(int l)
        {
            return Swap(l);
        }

        private unsafe void COM_UnMunge3(byte* data, int len, int seq)
        {
            byte[] mungify_table3 = {
                0x20, 0x07, 0x13, 0x61,
                0x03, 0x45, 0x17, 0x72,
                0x0A, 0x2D, 0x48, 0x0C,
                0x4A, 0x12, 0xA9, 0xB5
            };

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
                for (j = 0; j < 4; j++) *p++ ^= (byte)((0xa5 | (j << j) | j | mungify_table3[(i + j) & 0x0f]) & 0xFF);
                c = LongSwap(c);
                c ^= ~seq;
                *pc = c;
            }
        }

        private unsafe void MessageServerInfo()
        {
            int protocol = BitBuffer.ReadInt32();
            int spawncount = BitBuffer.ReadInt32();
            int mapcrc32 = BitBuffer.ReadInt32();

            COM_UnMunge3((byte*)&mapcrc32, 4, (-1 - DemoScanner.StartPlayerID) & 0xFF);


            byte[] clientdll_md5hash = BitBuffer.ReadBytes(16);

            demo.MaxClients = BitBuffer.ReadByte();

            DemoScanner.StartPlayerID = BitBuffer.ReadByte();
            DemoScanner.DealthMatch = BitBuffer.ReadByte() > 0;

            DemoScanner.GameDir = BitBuffer.ReadString(); // game dir

            if (demo.GsDemoInfo.Header.NetProtocol > 43)
                DemoScanner.ServerName =
                    BitBuffer.ReadString(); // server name                

            string tmpMapName = BitBuffer.ReadString();
            if (tmpMapName != DemoScanner.MapName)
            {
                DemoScanner.GameEnd = false;
                Console.WriteLine("---------- [Начало новой игры / Start new game (" + DemoScanner.CurrentTimeString + ")] ----------");
                if (DemoScanner.IsRussia)
                    DemoScanner.DemoScanner_AddInfo("Смена уровня на \"" + tmpMapName + "\"( CRC " + mapcrc32 + " ) время: " + DemoScanner.CurrentTimeString);
                else
                    DemoScanner.DemoScanner_AddInfo("Changelevel to \"" + tmpMapName + "\"( CRC " + mapcrc32 + " ) at " + DemoScanner.CurrentTimeString);

                DemoScanner.MapName = tmpMapName;
            }

            if (demo.GsDemoInfo.Header.NetProtocol == 45)
            {
                byte extraInfo = BitBuffer.ReadByte();
                Seek(-1);

                if (extraInfo != (byte)MessageId.svc_sendextrainfo)
                {
                    BitBuffer.ReadString(); // skip mapcycle

                    if (BitBuffer.ReadByte() > 0) Seek(36);
                }
            }
            else
            {
                BitBuffer.ReadString(); // skip mapcycle

                if (demo.GsDemoInfo.Header.NetProtocol > 43)
                    if (BitBuffer.ReadByte() > 0) Seek(21);
            }

            if (DemoScanner.FirstMap)
            {
                int tmpcursortop = Console.CursorTop;
                int tmpcursorleft = Console.CursorLeft;
                Console.CursorTop = DemoScanner.MapAndCrc32_Top;
                Console.CursorLeft = DemoScanner.MapAndCrc32_Left;
                ConsoleColor tmpconsolecolor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("\"" + DemoScanner.MapName + "\" ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("(CRC \"" + mapcrc32 + "\")              ");
                Console.ForegroundColor = tmpconsolecolor;
                Console.CursorTop = tmpcursortop;
                Console.CursorLeft = tmpcursorleft;
                DemoScanner.FirstMap = false;
            }
        }

        private void MessageLightStyle()
        {
            Seek(1);
            BitBuffer.ReadString();
        }

        public void MessageUpdateUserInfo()
        {
            bool update_name_if_need = false;
            byte slot = BitBuffer.ReadByte();
            if (slot == DemoScanner.UserId) update_name_if_need = true;
            int id = BitBuffer.ReadInt32();
            string s = BitBuffer.ReadString();
            //Console.WriteLine(s);
            if (demo.GsDemoInfo.Header.NetProtocol > 43) Seek(16); // string hash

            if (s.Length < 5) return;
            /*
* Если s пустая значит
* ищем игрока с id и присваиваем ему новый слот
* игрок со старым slot удаляем и перемещаем в другое место
* */

            DemoScanner.Player player = null;
            bool playerfound = false;

            int player_in_struct_id = 0;

            // поиск игрока с нужным ID если он существует
            for (player_in_struct_id = 0; player_in_struct_id < DemoScanner.playerList.Count; player_in_struct_id++)
                if (DemoScanner.playerList[player_in_struct_id].Slot == slot)
                {
                    playerfound = true;
                    player = DemoScanner.playerList[player_in_struct_id];
                    break;
                }

            // Если нет создаем нового
            // create player if it doesn't exist
            if (!playerfound)
            {
                player = new DemoScanner.Player(slot);
                DemoScanner.playerList.Add(player);
                player_in_struct_id = DemoScanner.playerList.Count - 1;
            }

            if (playerfound)
            {
                DemoScanner.fullPlayerList.Add(player);
                DemoScanner.playerList.RemoveAt(player_in_struct_id);
                player = new DemoScanner.Player(slot);
                DemoScanner.playerList.Add(player);
                player_in_struct_id = DemoScanner.playerList.Count - 1;
            }

            string bakaka = s;
            try
            {
                // parse infokey string
                s = s.Remove(0, 1); // trim leading slash
                string[] infoKeyTokens = s.Split('\\');
                for (int n = 0; n + 1 < infoKeyTokens.Length; n += 2)
                {
                    if (n + 1 >= infoKeyTokens.Length)
                        // Must be an odd number of strings - a key without a value - ignore it.
                        break;

                    string key = infoKeyTokens[n];

                    if (key == "STEAMID") key = "pid";

                    if (key.ToLower() == "name")
                    {
                        player.Name = infoKeyTokens[n + 1];
                        if (update_name_if_need && player.Name != DemoScanner.LastUername) DemoScanner.ForceUpdateName = true;
                    }
                    if (key == "*sid")
                    {
                        key = "STEAMID";
                        infoKeyTokens[n + 1] =
                            CalculateSteamId(infoKeyTokens[n + 1]);
                        player.Steam = infoKeyTokens[n + 1];
                    }

                    // If the key already exists, overwrite it.
                    if (player.InfoKeys.ContainsKey(key)) player.InfoKeys.Remove(key);

                    player.InfoKeys.Add(key, infoKeyTokens[n + 1]);
                }

                DemoScanner.playerList[player_in_struct_id] = player;
            }
            catch
            {
                Console.WriteLine("Error in parsing:" + bakaka);
            }
        }

        public void MessageDeltaDescription()
        {
            string structureName = BitBuffer.ReadString();

            if (demo.GsDemoInfo.Header.NetProtocol == 43) BitBuffer.Endian = BitBuffer.EndianType.Big;

            uint nEntries = BitBuffer.ReadUnsignedBits(16);

            HalfLifeDeltaStructure newDeltaStructure = new HalfLifeDeltaStructure(structureName);
            AddDeltaStructure(newDeltaStructure);

            HalfLifeDeltaStructure deltaDescription = GetDeltaStructure("delta_description_t");

            for (ushort i = 0; i < nEntries; i++)
            {
                HalfLifeDelta newDelta = deltaDescription.CreateDelta();
                deltaDescription.ReadDelta(BitBuffer, newDelta);

                newDeltaStructure.AddEntry(newDelta);
            }

            BitBuffer.SkipRemainingBits();
            BitBuffer.Endian = BitBuffer.EndianType.Little;
        }


        public void MessageClientData()
        {
            DemoScanner.ClientDataCountMessages++;
            DemoScanner.SVC_CLIENTUPDATEMSGID = DemoScanner.MessageId;



            if (demo.GsDemoInfo.Header.NetProtocol <= 43) BitBuffer.Endian = BitBuffer.EndianType.Big;

            bool deltaSequence = BitBuffer.ReadBoolean();

            if (deltaSequence) BitBuffer.SeekBits(8); // delta sequence number

            HalfLifeDeltaStructure tmpdelta = GetDeltaStructure("clientdata_t");
            tmpdelta.ReadDelta(BitBuffer, null);

            while (BitBuffer.ReadBoolean())
            {
                if (demo.GsDemoInfo.Header.NetProtocol < 47)
                    BitBuffer.SeekBits(5); // weapon index
                else
                    BitBuffer.SeekBits(6); // weapon index

                GetDeltaStructure("weapon_data_t").ReadDelta(BitBuffer, null);
            }

            BitBuffer.SkipRemainingBits();
            BitBuffer.Endian = BitBuffer.EndianType.Little;




            if (DemoScanner.SVC_CHOKEMSGID - 1 == DemoScanner.SVC_TIMEMSGID
                || DemoScanner.SVC_CLIENTUPDATEMSGID - 1 == DemoScanner.SVC_TIMEMSGID
                || DemoScanner.SVC_CLIENTUPDATEMSGID - 1 == DemoScanner.SVC_ADDANGLEMSGID
                || DemoScanner.SVC_CLIENTUPDATEMSGID - 1 == DemoScanner.SVC_SETANGLEMSGID)
            {
                ;
            }
            else
            {
                if (Math.Abs(DemoScanner.CurrentTime - DemoScanner.speedhackdetect_time) > 5.0)
                    DemoScanner.DemoScanner_AddWarn(
                        "[FakeSpeed Type 2] at (" + DemoScanner.CurrentTime +
                        "):" + DemoScanner.CurrentTimeString, false);
                DemoScanner.speedhackdetect_time = DemoScanner.CurrentTime;
            }
        }

        public void MessagePings()
        {
            if (demo.GsDemoInfo.Header.NetProtocol <= 43) BitBuffer.Endian = BitBuffer.EndianType.Big;

            while (BitBuffer.ReadBoolean())
            {
                uint slotid = BitBuffer.ReadUnsignedBits(5);
                uint pings = BitBuffer.ReadUnsignedBits(12);
                uint loss = BitBuffer.ReadUnsignedBits(7);
                if (slotid == DemoScanner.UserId && loss > 0)
                {
                    DemoScanner.LastLossPacketCount = loss;

                    if (DemoScanner.DUMP_ALL_FRAMES) DemoScanner.OutDumpString += "[SKIP " + loss + " frames(loss packets)]\n";

                    DemoScanner.CheckConsoleCommand("Lost packets: " + loss + ". Ping: " + pings, true);
                    DemoScanner.LossPackets++;
                    DemoScanner.FrameErrors = DemoScanner.LastOutgoingSequence = DemoScanner.LastIncomingAcknowledged = DemoScanner.LastIncomingSequence = 0;
                    DemoScanner.LastLossTime2 = DemoScanner.CurrentTime;
                    DemoScanner.PluginFrameNum = -1;
                    DemoScanner.NeedSearchCMDHACK4 = true;

                    if (!DemoScanner.LossFalseDetection && (Math.Abs(DemoScanner.LastLossPacket) <= float.Epsilon || Math.Abs(DemoScanner.CurrentTime - DemoScanner.LastLossPacket) > 60.0f))
                    {
                        DemoScanner.LossFalseDetection = true;
                        ConsoleColor col = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Red;

                        if (DemoScanner.BadAttackCount > 0) DemoScanner.BadAttackCount--;

                        if (DemoScanner.TotalAimBotDetected > 0) DemoScanner.TotalAimBotDetected--;

                        if (DemoScanner.JumpHackCount > 0) DemoScanner.JumpHackCount--;

                        if (DemoScanner.JumpHackCount2 > 0) DemoScanner.JumpHackCount2--;

                        if (DemoScanner.IsRussia)
                            Console.WriteLine("[ЛАГ] Предупреждение! Игрок завис и один детект может быть ложным!");
                        else
                            Console.WriteLine("[LAG] Warning! Player has lag and previous detection can be false!");

                        Console.ForegroundColor = col;
                        DemoScanner.LastLossPacket = DemoScanner.CurrentTime;
                    }
                }
            }

            BitBuffer.SkipRemainingBits();
            BitBuffer.Endian = BitBuffer.EndianType.Little;
        }

        private void MessageSpawnStatic()
        {
            BitBuffer.SeekBytes(18);
            byte renderMode = BitBuffer.ReadByte();

            if (renderMode != 0) BitBuffer.SeekBytes(5);
        }

        private void MessageEventReliable()
        {
            if (demo.GsDemoInfo.Header.NetProtocol <= 43) BitBuffer.Endian = BitBuffer.EndianType.Big;

            BitBuffer.SeekBits(10); // event index

            GetDeltaStructure("event_t").ReadDelta(BitBuffer, null);

            bool delayBit = BitBuffer.ReadBoolean();

            if (delayBit) BitBuffer.SeekBits(16); // delay / 100.0f

            BitBuffer.SkipRemainingBits();
            BitBuffer.Endian = BitBuffer.EndianType.Little;
        }

        public void MessageSpawnBaseline()
        {
            if (demo.GsDemoInfo.Header.NetProtocol <= 43) BitBuffer.Endian = BitBuffer.EndianType.Big;

            while (true)
            {
                uint entityIndex = BitBuffer.ReadUnsignedBits(11);

                if (entityIndex == (1 << 11) - 1) // all 1's
                    break;

                uint entityType = BitBuffer.ReadUnsignedBits(2);
                string entityTypeString;

                if ((entityType & 1) != 0) // is bit 1 set?
                {
                    if (entityIndex > 0 && entityIndex <= demo.MaxClients)
                    {
                        if (DemoScanner.DUMP_ALL_FRAMES) DemoScanner.OutDumpString += "PID3:" + entityIndex + "\n";

                        DemoScanner.LastPlayerEntity = entityIndex;
                        entityTypeString = "entity_state_player_t";
                    }
                    else
                    {
                        entityTypeString = "entity_state_t";
                    }
                }
                else
                {
                    entityTypeString = "custom_entity_state_t";
                }

                GetDeltaStructure(entityTypeString).ReadDelta(BitBuffer, null);
            }

            uint footer = BitBuffer.ReadUnsignedBits(5); // should be all 1's

            if (footer != (1 << 5) - 1) throw new ApplicationException("Bad svc_spawnbaseline footer.");

            uint nExtraData = BitBuffer.ReadUnsignedBits(6);

            for (int i = 0; i < nExtraData; i++) GetDeltaStructure("entity_state_t").ReadDelta(BitBuffer, null);

            BitBuffer.Endian = BitBuffer.EndianType.Little;
            BitBuffer.SkipRemainingBits();
        }

        private void MessageTempEntity()
        {
            byte type = BitBuffer.ReadByte();
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
                    DemoScanner.LastBeamFound = DemoScanner.CurrentTime;
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

                    ushort entityIndex = BitBuffer.ReadUInt16();

                    if (entityIndex != 0) Seek(2);

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
                    DemoScanner.CurrentMsgCount++;
                    Seek(5);
                    byte textParmsEffect = BitBuffer.ReadByte();
                    Seek(14);

                    if (textParmsEffect == 2) Seek(2);

                    BitBuffer
                        .ReadString(); // capped to 512 bytes (including null terminator)
                    break;

                case 30: // TE_LINE
                case 31: // TE_BOX
                    Seek(17);
                    break;

                case 99: // TE_KILLBEAM
                    DemoScanner.LastBeamFound = DemoScanner.CurrentTime;
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

                case 112
                    : // TE_PLAYERDECAL (could be a trailing short after this, apparently...)
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
                    throw new ApplicationException(
                        string.Format("Unknown tempentity type \"{0}\".", type));
            }
            // BitBuffer.SkipRemainingBits();
        }

        private void MessageSetPause()
        {
            bool pause = BitBuffer.ReadByte() > 0;
            ConsoleColor tmpcolor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Gray;

            if (DemoScanner.IsRussia)
                Console.WriteLine("[ПАУЗА] Сервер был поставлен на паузу. Срабатывание может быть ложным! Время " + DemoScanner.CurrentTimeString + ". ");
            else
                Console.WriteLine("[POSSIBLE FALSE DETECTION BELLOW!] Warning!!! Server " + (pause ? "paused" : "unpaused") + " at " + DemoScanner.CurrentTimeString + ". ");

            Console.ForegroundColor = tmpcolor;
        }

        private void MessageCenterPrint()
        {
            DemoScanner.CurrentMsgCount++;
            string msgprint = BitBuffer.ReadString();
            if (DemoScanner.DEBUG_ENABLED) Console.Write("msgcenterprint:" + msgprint);

            if (DemoScanner.DUMP_ALL_FRAMES) DemoScanner.OutDumpString += "MessageCenterPrint:" + msgprint;

            if (msgprint == "%s")
            {
                string msgprint2 = BitBuffer.ReadString();
                if (DemoScanner.DUMP_ALL_FRAMES) DemoScanner.OutDumpString += "->" + msgprint2;

                if (DemoScanner.DEBUG_ENABLED) Console.Write("..bad msgcenterprint?.." + msgprint + ">>>>>" + msgprint2);
            }
        }

        private void MessageInterMission()
        {

        }

        public void MessageNewUserMsg()
        {
            byte id = BitBuffer.ReadByte();
            sbyte length = BitBuffer.ReadSByte();
            string name = BitBuffer.ReadString(16);
            if (DemoScanner.DUMP_ALL_FRAMES)
                DemoScanner.OutDumpString +=
                    "user msg:" + name + " len:" + length + " id " + id + "\n";

            AddUserMessage(id, length, name);
        }

        public void MessagePacketEntities()
        {
            BitBuffer.SeekBits(
                16); // num entities (not reliable at all, loop until footer - see below)

            if (demo.GsDemoInfo.Header.NetProtocol <= 43) BitBuffer.Endian = BitBuffer.EndianType.Big;

            uint entityNumber = 0;

            // begin entity parsing
            while (true)
            {
                ushort footer = BitBuffer.ReadUInt16();

                if (footer == 0) break;

                BitBuffer.SeekBits(-16);

                bool entityNumberIncrement = BitBuffer.ReadBoolean();

                if (!entityNumberIncrement
                ) // entity number isn't last entity number + 1, need to read it in
                {
                    // is the following entity number absolute, or relative from the last one?
                    bool absoluteEntityNumber = BitBuffer.ReadBoolean();

                    if (absoluteEntityNumber)
                        entityNumber = BitBuffer.ReadUnsignedBits(11);
                    else
                        entityNumber += BitBuffer.ReadUnsignedBits(6);
                }
                else
                {
                    entityNumber++;
                }

                if (demo.GsDemoInfo.Header.GameDir == "tfc") BitBuffer.ReadBoolean(); // unknown

                bool custom = BitBuffer.ReadBoolean();
                bool useBaseline = BitBuffer.ReadBoolean();

                if (useBaseline) BitBuffer.SeekBits(6); // baseline index

                string entityType = "entity_state_t";

                if (entityNumber > 0 && entityNumber <= demo.MaxClients)
                {
                    if (DemoScanner.DUMP_ALL_FRAMES) DemoScanner.OutDumpString += "PID1:" + entityNumber + "\n";

                    DemoScanner.LastPlayerEntity = entityNumber;
                    entityType = "entity_state_player_t";
                }
                else if (custom)
                {
                    entityType = "custom_entity_state_t";
                }

                GetDeltaStructure(entityType).ReadDelta(BitBuffer, null);
            }

            BitBuffer.SkipRemainingBits();
            BitBuffer.Endian = BitBuffer.EndianType.Little;
        }

        public void MessageDeltaPacketEntities()
        {
            BitBuffer.SeekBits(
                16); // num entities (not reliable at all, loop until footer - see below)
            BitBuffer.SeekBits(8); // delta sequence number

            if (demo.GsDemoInfo.Header.NetProtocol <= 43) BitBuffer.Endian = BitBuffer.EndianType.Big;

            uint entityNumber = 0;

            while (true)
            {
                ushort footer = BitBuffer.ReadUInt16();

                if (footer == 0) break;

                BitBuffer.SeekBits(-16);

                bool removeEntity = BitBuffer.ReadBoolean();

                // is the following entity number absolute, or relative from the last one?
                bool absoluteEntityNumber = BitBuffer.ReadBoolean();

                if (absoluteEntityNumber)
                    entityNumber = BitBuffer.ReadUnsignedBits(11);
                else
                    entityNumber += BitBuffer.ReadUnsignedBits(6);

                if (!removeEntity)
                {
                    if (demo.GsDemoInfo.Header.GameDir == "tfc") BitBuffer.ReadBoolean(); // unknown

                    bool custom = BitBuffer.ReadBoolean();

                    if (demo.GsDemoInfo.Header.NetProtocol <= 43) BitBuffer.SeekBits(1); // unknown

                    string entityType = "entity_state_t";

                    if (entityNumber > 0 && entityNumber <= demo.MaxClients)
                    {
                        if (DemoScanner.DUMP_ALL_FRAMES) DemoScanner.OutDumpString += "PID2:" + entityNumber + "\n";

                        DemoScanner.LastPlayerEntity = entityNumber;
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
            if (demo.GsDemoInfo.Header.NetProtocol <= 43) BitBuffer.Endian = BitBuffer.EndianType.Big;

            uint nEntries = BitBuffer.ReadUnsignedBits(12);

            for (int i = 0; i < nEntries; i++)
            {
                uint type = BitBuffer.ReadUnsignedBits(4); // entry type
                string downloadname = BitBuffer.ReadString(); // entry name
                uint index = BitBuffer.ReadUnsignedBits(12);
                ulong downloadsize = BitBuffer.ReadUnsignedBits(24);
                uint flags = BitBuffer.ReadUnsignedBits(3);
                byte[] hash = new byte[16];
                if ((flags & 4) != 0) // md5 hash
                    hash = BitBuffer.ReadBytes(16);

                bool has_extra = BitBuffer.ReadBoolean();
                byte[] info = new byte[32];

                if (has_extra) info = BitBuffer.ReadBytes(32);

                if (type == 0)
                    DemoScanner.DownloadResources.Add("sound\\" + downloadname);
                else
                    DemoScanner.DownloadResources.Add(downloadname);

                DemoScanner.DownloadResourcesSize += downloadsize;
            }

            // consistency list
            // indices of resources to force consistency upon?
            if (BitBuffer.ReadBoolean())
                while (BitBuffer.ReadBoolean())
                {
                    int nBits = BitBuffer.ReadBoolean() ? 5 : 10;
                    BitBuffer.SeekBits(nBits); // consistency index
                }

            BitBuffer.SkipRemainingBits();
            BitBuffer.Endian = BitBuffer.EndianType.Little;
        }

        private void MessageChoke()
        {
            if (DemoScanner.DUMP_ALL_FRAMES) DemoScanner.OutDumpString += "CHOKE!\n";

            DemoScanner.ChokePackets += 1;
            DemoScanner.SVC_CHOKEMSGID = DemoScanner.MessageId;

            if (DemoScanner.SVC_CHOKEMSGID - 1 == DemoScanner.SVC_SETANGLEMSGID ||
                DemoScanner.SVC_CHOKEMSGID - 1 == DemoScanner.SVC_TIMEMSGID)
            {
                ;
            }
            else
            {
                if (Math.Abs(DemoScanner.CurrentTime - DemoScanner.LastFakeLagTime) > 5.0f)
                    DemoScanner.DemoScanner_AddWarn(
                        "[Fakelag Type 2] at (" + DemoScanner.CurrentTime +
                        "):" + DemoScanner.CurrentTimeString, false);
                DemoScanner.LastFakeLagTime = DemoScanner.CurrentTime;
            }


            DemoScanner.LastChokePacket = DemoScanner.CurrentTime;
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

            Seek(98);
            BitBuffer.ReadString();
        }

        private bool IsBadMessage(string s)
        {
            foreach (char c in s)
                if (char.IsLetterOrDigit(c) || c == '.')
                {

                }
                else
                {
                    return true;
                }

            return false;
        }

        private void MessageCustomization()
        {
            byte playerid = BitBuffer.ReadByte();
            byte resourcetype = BitBuffer.ReadByte();
            string resourcename = BitBuffer.ReadString();
            ushort resourceindex = BitBuffer.ReadUInt16();
            uint downloadsize = BitBuffer.ReadUInt32();
            byte resourceflags = BitBuffer.ReadByte();
            byte[] hash = new byte[16];
            if ((resourceflags & (1 << 2)) > 0) hash = BitBuffer.ReadBytes(16);
            BitBuffer.SkipRemainingBits();
        }

        private void MessageFileTransferFailed()
        {
            DemoScanner.CurrentMsgCount++;
            // string: filename
            BitBuffer.ReadString();
        }

        private void MessageHltv()
        {
            // TODO: CHECK THE SDK - MESSAGE_BEGIN( MSG_SPEC, SVC_HLTV );

            /*

            new:
            #define HLTV_ACTIVE				0	// tells client that he's an spectator and will get director command
            #define HLTV_STATUS				1	// send status infos about proxy 
            #define HLTV_CAMERA				2	// set the actual director camera position
            #define HLTV_EVENT				3	// informs the dircetor about ann important game event

            old:
            #define HLTV_ACTIVE				0	// tells client that he's an spectator and will get director commands
            #define HLTV_STATUS				1	// send status infos about proxy 
            #define HLTV_LISTEN				2	// tell client to listen to a multicast stream
             */

            byte subCommand = BitBuffer.ReadByte();

            if (subCommand == 1) // HLTV_LISTEN/HLTV_CAMERA
            {
                BitBuffer.ReadInt32();
                BitBuffer.ReadInt16();
                BitBuffer.ReadInt16();
                BitBuffer.ReadInt32();
                BitBuffer.ReadInt32();
                BitBuffer.ReadInt16();
            }
            else if (subCommand == 2)
            {
                BitBuffer.ReadString();
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
            byte len = BitBuffer.ReadByte();
            string bytes = ByteArrayToString(BitBuffer.ReadBytes(len));
            DemoScanner.CurrentMsgCount++;
        }

        private void MessageVoiceInit()
        {
            // string: codec name (sv_voicecodec, either voice_miles or voice_speex)
            // byte: quality (sv_voicequality, 1 to 5)

            string tmpcodecname = BitBuffer.ReadString();
            if (tmpcodecname.Length > 0) DemoScanner.codecname = tmpcodecname;

            //MessageBox.Show(codecname);
            if (DemoScanner.DUMP_ALL_FRAMES) DemoScanner.OutDumpString += "MessageVoiceInit:" + tmpcodecname + "\n";

            if (demo.GsDemoInfo.Header.NetProtocol >= 47)
            {
                if (tmpcodecname.Length > 0)
                {
                    DemoScanner.VoiceQuality = BitBuffer.ReadByte();
                }
            }
        }


        private void MessageVoiceData()
        {
            // byte: client id/slot?
            // short: data length
            // length bytes: data

            int playerid = BitBuffer.ReadByte();
            ushort length = BitBuffer.ReadUInt16();
            byte[] data = BitBuffer.ReadBytes(length);

            if (DemoScanner.DUMP_ALL_FRAMES) DemoScanner.OutDumpString += "MessageVoiceData:" + length + "\n";
            //MessageBox.Show(playerid + "(2):" + length);

            if (playerid >= 0 && playerid <= 33)
                for (int i = 0; i < DemoScanner.playerList.Count; i++)
                    if (DemoScanner.playerList[i].Slot == playerid) DemoScanner.playerList[i].WriteVoice(length, data);
        }

        private void MessageSendExtraInfo()
        {
            // string: "com_clientfallback", always seems to be null
            // byte: sv_cheats

            // NOTE: had this backwards before, shouldn't matter
            string extra = BitBuffer.ReadString();
            DemoScanner.FileDirectories.Add(extra);
            Seek(1);
        }

        private void MessageResourceLocation()
        {
            // string: location?
            string tmpDownLocation = BitBuffer.ReadString();
            if (tmpDownLocation.ToLower().StartsWith("http"))
                DemoScanner.DownloadLocation = tmpDownLocation;
            else
                try
                {
                    DemoScanner.ProcessPluginMessage(tmpDownLocation);
                }
                catch
                {
                    Console.WriteLine("Error in demo scanner AMXX plugin. Please update to new version.");
                }
        }

        private void MessageSendCvarValue()
        {
            BitBuffer.ReadString(); // The cvar.
        }

        private void MessageRestore()
        {
            BitBuffer.ReadString(); // CL_Restore(str)
        }

        private void MessageReplaceDecal()
        {
            BitBuffer.ReadByte();   // decal
            BitBuffer.ReadString(); // DecalSetName(decal,name)
        }

        private void MessageTFC_Exec()
        {
            if (BitBuffer.ReadByte() == 1) BitBuffer.ReadByte();
        }

        private void MessageCutSceneCenterPrint()
        {
            BitBuffer.ReadString(); // CenterPrint(str)
        }

        private void MessageSendCvarValue2()
        {
            Seek(4); // unsigned int
            BitBuffer.ReadString(); // The cvar.
        }

        private void MessageSetAngle()
        {
            ushort AnglePitch = BitBuffer.ReadUInt16();
            ushort AngleYaw = BitBuffer.ReadUInt16();
            ushort AngleRoll = BitBuffer.ReadUInt16();
            if (DemoScanner.DUMP_ALL_FRAMES) DemoScanner.OutDumpString += "Pitch/Yaw/Roll:" + AnglePitch + "/" + AngleYaw + "/" + AngleRoll + ".\n";

            DemoScanner.SVC_SETANGLEMSGID = DemoScanner.MessageId;
        }

        private void MessageAddAngle()
        {
            ushort angle = BitBuffer.ReadUInt16();
            DemoScanner.LastAngleManipulation = DemoScanner.CurrentTime;
            DemoScanner.SVC_ADDANGLEMSGID = DemoScanner.MessageId;
        }

        private void MessageRoomType()
        {
            ushort roomtype = BitBuffer.ReadUInt16();
            if (DemoScanner.DEBUG_ENABLED) Console.WriteLine("Alive 10 at " + DemoScanner.CurrentTimeString);

            DemoScanner.UserAlive = true;
            DemoScanner.LastAliveTime = DemoScanner.CurrentTime;
            DemoScanner.FirstUserAlive = false;
        }


        private void MessageCurWeapon()
        {
            byte cStatus = BitBuffer.ReadByte();

            byte weaponid_byte = BitBuffer.ReadByte();

            byte clip = BitBuffer.ReadByte();
            DemoScanner.WeaponIdType weaponid =
                (DemoScanner.WeaponIdType)Enum.ToObject(typeof(DemoScanner.WeaponIdType),
                    weaponid_byte);

            byte bad = 0xFF;

            if (cStatus == 0 && weaponid_byte == bad && clip == bad && (DemoScanner.UserAlive || DemoScanner.FirstUserAlive))
            {
                DemoScanner.LastDeathTime = DemoScanner.CurrentTime;
                if (!DemoScanner.FirstUserAlive)
                    DemoScanner.DeathsCoount++;
                DemoScanner.FirstUserAlive = false;
                DemoScanner.UserAlive = false;
                DemoScanner.RealAlive = false;
                if (DemoScanner.DUMP_ALL_FRAMES) DemoScanner.OutDumpString += "LocalPlayer  killed. Method : cur weapon!\n";

                if (DemoScanner.DEBUG_ENABLED) Console.WriteLine("LocalPlayer killed. Method : cur weapon! at " + DemoScanner.CurrentTimeString);

                return;
            }

            if (DemoScanner.UsingAnotherMethodWeaponDetection)
                return;

            if ((DemoScanner.SkipChangeWeapon != 1 || (weaponid != DemoScanner.WeaponIdType.WEAPON_C4 && weaponid != DemoScanner.WeaponIdType.WEAPON_NONE))
                && DemoScanner.CurrentWeapon != weaponid && cStatus == 1
                && (clip > 0 || weaponid == DemoScanner.WeaponIdType.WEAPON_KNIFE
                                        || weaponid == DemoScanner.WeaponIdType.WEAPON_C4
                                        || weaponid == DemoScanner.WeaponIdType.WEAPON_HEGRENADE
                                        || weaponid == DemoScanner.WeaponIdType.WEAPON_SMOKEGRENADE
                                        || weaponid == DemoScanner.WeaponIdType.WEAPON_FLASHBANG))
            {
                /*if (DemoScanner.UsingAnotherMethodWeaponDetection || !DemoScanner.IsRealWeapon())*/
                if (DemoScanner.DEBUG_ENABLED) Console.WriteLine("Select weapon method 1 (" + DemoScanner.UsingAnotherMethodWeaponDetection + "):" + weaponid + ":" + DemoScanner.CurrentTimeString);

                DemoScanner.NeedSearchAim2 = false;
                DemoScanner.Aim2AttackDetected = false;
                DemoScanner.ShotFound = -1;

                DemoScanner.SelectSlot = 0;
                DemoScanner.WeaponChanged = true;
                DemoScanner.AmmoCount = 0;
                // DemoScanner.IsAttackSkipTimes = 0;
                if (DemoScanner.CurrentWeapon !=
                    DemoScanner.WeaponIdType.WEAPON_NONE)
                    DemoScanner.SkipNextAttack = 2;

                //DemoScanner.InitAimMissingSearch = -1;
                DemoScanner.IsNoAttackLastTime =
                    DemoScanner.CurrentTime + 1.0f;
                DemoScanner.NeedCheckAttack = false;

                //DemoScanner.UserAlive = true;
                if (DemoScanner.DUMP_ALL_FRAMES)
                {
                    DemoScanner.OutDumpString +=
                        "\nCHANGE WEAPON 22 (" + DemoScanner.CurrentTime + ") " + DemoScanner.CurrentTimeString;
                    DemoScanner.OutDumpString += "\nFrom " + DemoScanner.CurrentWeapon + " to " + weaponid;
                    Console.WriteLine(
                        "CHANGE WEAPON 22 (" + DemoScanner.CurrentTime + ") " + DemoScanner.CurrentTimeString);
                    Console.WriteLine(
                        "From " + DemoScanner.CurrentWeapon + " to " + weaponid);
                }

                DemoScanner.NeedSearchAim2 = false;
                DemoScanner.Aim2AttackDetected = false;
                DemoScanner.ShotFound = -1;
                DemoScanner.SelectSlot = 0;
                DemoScanner.WeaponChanged = true;
                DemoScanner.AmmoCount = 0;
                // DemoScanner.IsAttackSkipTimes = 0;
                //if (DemoScanner.CurrentWeapon != DemoScanner.WeaponIdType.WEAPON_NONE) DemoScanner.SkipNextAttack = 2;

                //DemoScanner.InitAimMissingSearch = -1;
                DemoScanner.IsNoAttackLastTime = DemoScanner.CurrentTime + 1.0f;
                DemoScanner.NeedCheckAttack = false;
                DemoScanner.CurrentWeapon = weaponid;
            }
            DemoScanner.SkipChangeWeapon--;

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
            DemoScanner.LastDamageTime = DemoScanner.CurrentTime;
            //Console.WriteLine("Damage at " + DemoScanner.CurrentTimeString);
        }

        private void HideWeapon()
        {
            byte flags = BitBuffer.ReadByte();
            if ((flags & 4) > 0
                || (flags & 1) > 0)
            {
                DemoScanner.HideWeapon = true;
                //**
                DemoScanner.HideWeaponTime = DemoScanner.CurrentTime;
            }
            else
            {
                DemoScanner.HideWeapon = false;
            }
        }

        private void WeaponList()
        {
            string WeaponName = BitBuffer.ReadString();
            byte PrimaryAmmoID = BitBuffer.ReadByte();
            byte PrimaryAmmoMaxAmount = BitBuffer.ReadByte();
            byte SecondaryAmmoID = BitBuffer.ReadByte();
            byte SecondaryAmmoMaxAmount = BitBuffer.ReadByte();
            byte SlotID = BitBuffer.ReadByte();
            byte NumberInSlot = BitBuffer.ReadByte();
            byte WeaponID = BitBuffer.ReadByte();
            byte Flags = BitBuffer.ReadByte();

            /* Console.Write("/WeaponName:" + WeaponName.Remove(0,1) + "\n");
             Console.Write("/PrimaryAmmoID:" + PrimaryAmmoID + "\n");
             Console.Write("/PrimaryAmmoMaxAmount:" + PrimaryAmmoMaxAmount + "\n");
             Console.Write("/SecondaryAmmoID:" + SecondaryAmmoID + "\n");
             Console.Write("/SecondaryAmmoMaxAmount:" + SecondaryAmmoMaxAmount + "\n");
             Console.Write("/SlotID:" + SlotID + "\n");
             Console.Write("/NumberInSlot:" + NumberInSlot + "\n");
             Console.Write("/WeaponID:" + WeaponID + "\n");
             Console.Write("/Flags:" + Flags + "\n");

             Console.WriteLine();*/
        }

        private void WeapPickup()
        {
            byte weaponid_byte = BitBuffer.ReadByte();
            DemoScanner.WeaponIdType weaponid =
                (DemoScanner.WeaponIdType)Enum.ToObject(typeof(DemoScanner.WeaponIdType),
                    weaponid_byte);
            if (DemoScanner.UsingAnotherMethodWeaponDetection)
            {
                if (DemoScanner.UserAlive)
                {
                    if (/*!DemoScanner.UsingAnotherMethodWeaponDetection || */!DemoScanner.IsRealWeapon()) DemoScanner.CurrentWeapon = weaponid;

                    if (DemoScanner.DEBUG_ENABLED) Console.WriteLine("Select weapon method 2 (" + DemoScanner.UsingAnotherMethodWeaponDetection + "):" + weaponid + ":" + DemoScanner.CurrentTimeString);

                    if (DemoScanner.DUMP_ALL_FRAMES) DemoScanner.OutDumpString += "\nSelect weapon(" + DemoScanner.UsingAnotherMethodWeaponDetection + "):" + weaponid + "\n";
                }
            }
            // else
            // {
            //    DemoScanner.CurrentWeapon = DemoScanner.WeaponIdType.WEAPON_BAD;
            // }
        }

        private void SetFOV()
        {
            byte newfov = BitBuffer.ReadByte();

            if (DemoScanner.FovByFunc != newfov && DemoScanner.DEBUG_ENABLED)
            {
                if (newfov != 0)
                    DemoScanner.SkipChangeWeapon = 2;
                Console.WriteLine("Change fov from " + DemoScanner.FovByFunc + " to " + newfov + " at " + DemoScanner.CurrentTimeString);
            }

            DemoScanner.FovByFunc = newfov;
        }

        private void TextMsg()
        {
            DemoScanner.CurrentMsgCount++;
            byte len = BitBuffer.ReadByte();
            int endbyte = BitBuffer.CurrentByte + len;

            byte type = BitBuffer.ReadByte();
            string arg1 = BitBuffer.ReadString().Trim();

            if (/*arg1 == "#Game_Commencing"
                || */arg1 == "#Game_will_restart_in"
                || arg1 == "#CTs_Win"
                || arg1 == "#Terrorists_Win"
                || arg1 == "#Round_Draw"
                || arg1 == "#Target_Bombed"
                || arg1 == "#Target_Saved"
                || arg1 == "#Terrorists_Escaped"
                || arg1 == "#Terrorists_Not_Escaped"
                || arg1 == "#CTs_PreventEscape"
                || arg1 == "#Escaping_Terrorists_Neutralized"
                || arg1 == "#Hostages_Not_Rescued"
                || arg1 == "#All_Hostages_Rescued"
                || arg1 == "#VIP_Escaped"
                || arg1 == "#VIP_Not_Escaped"
                || arg1 == "#VIP_Assassinated"
                || arg1 == "#Bomb_Defused")
                DemoScanner.RoundEndTime = DemoScanner.CurrentTime;
            //var arg2 = BitBuffer.ReadString();
            //var arg3 = BitBuffer.ReadString();
            //var arg4 = BitBuffer.ReadString();
            //var arg5 = BitBuffer.ReadString();
            BitBuffer.SkipRemainingBits();
            BitBuffer.SeekBytes(endbyte, SeekOrigin.Begin);
        }

        private void MessageDeath()
        {
            byte len = BitBuffer.ReadByte();
            byte iKiller = BitBuffer.ReadByte();
            byte iVictim = BitBuffer.ReadByte();
            byte isHeadShot = BitBuffer.ReadByte();
            string weapon = BitBuffer.ReadString();

            if (iVictim > 32 || iKiller > 32)
                return;
            if (iVictim == DemoScanner.UserId + 1 && (DemoScanner.UserAlive || DemoScanner.FirstUserAlive))
            {
                DemoScanner.LastDeathTime = DemoScanner.CurrentTime;
                if (!DemoScanner.FirstUserAlive)
                    DemoScanner.DeathsCoount++;
                DemoScanner.FirstUserAlive = false;
                DemoScanner.UserAlive = false;
                DemoScanner.RealAlive = false;
                if (DemoScanner.DUMP_ALL_FRAMES) DemoScanner.OutDumpString += "LocalPlayer " + iVictim + " killed!\n";

                if (DemoScanner.DEBUG_ENABLED) Console.WriteLine("LocalPlayer " + iVictim + " killed! at " + DemoScanner.CurrentTimeString);

                if (DemoScanner.GameEnd && Math.Abs(DemoScanner.CurrentTime - DemoScanner.GameEndTime) > 5.0)
                {
                    DemoScanner.DemoScanner_AddInfo("False game end message. Resetting game status.");
                    DemoScanner.GameEnd = false;
                }
            }
            else if (iKiller == DemoScanner.UserId + 1 && iVictim != iKiller)
            {
                DemoScanner.KillsCount++;
                if (DemoScanner.LastAttackForTrigger == DemoScanner.NewAttackForTrigger)
                {
                    if (DemoScanner.LastAttackForTriggerFrame != DemoScanner.CurrentFrameIdAll && !DemoScanner.IsPlayerAttackedPressed() && DemoScanner.IsUserAlive() && !DemoScanner.IsChangeWeapon() && !DemoScanner.IsPlayerLossConnection())
                    {
                        DemoScanner.WeaponIdType wpntype = DemoScanner.GetWeaponByStr(weapon);
                        if (wpntype == DemoScanner.WeaponIdType.WEAPON_NONE
                                    || wpntype == DemoScanner.WeaponIdType.WEAPON_BAD
                                    || wpntype == DemoScanner.WeaponIdType.WEAPON_BAD2
                                    || wpntype == DemoScanner.WeaponIdType.WEAPON_C4
                                    || wpntype == DemoScanner.WeaponIdType.WEAPON_HEGRENADE
                                    || wpntype == DemoScanner.WeaponIdType.WEAPON_KNIFE
                                    || wpntype == DemoScanner.WeaponIdType.WEAPON_SMOKEGRENADE
                                    || wpntype == DemoScanner.WeaponIdType.WEAPON_FLASHBANG)
                        {

                        }
                        else
                        {
                            /* if (DemoScanner.AUTO_LEARN_HACK_DB)
                             {
                                 DemoScanner.ENABLE_LEARN_HACK_DEMO = true;
                                 DemoScanner.ENABLE_LEARN_HACK_DEMO_FORCE_SAVE = true;
                             }*/
                            DemoScanner.DemoScanner_AddWarn(
                                "[TRIGGER TYPE 2 " + wpntype + "] at (" + DemoScanner.CurrentTime +
                                ") " + DemoScanner.CurrentTimeString);

                            DemoScanner.BadAttackCount++;
                            DemoScanner.LastBadAttackCount = DemoScanner.CurrentTime;
                        }
                    }
                    DemoScanner.LastAttackForTrigger = -1;
                }
                else
                {
                    DemoScanner.LastAttackForTrigger = DemoScanner.NewAttackForTrigger;
                }

                DemoScanner.LastAttackForTriggerFrame = DemoScanner.CurrentFrameIdAll;

                if (DemoScanner.GameEnd && Math.Abs(DemoScanner.CurrentTime - DemoScanner.GameEndTime) > 5.0)
                {
                    DemoScanner.DemoScanner_AddInfo("False game end message. Resetting game status.");
                    DemoScanner.GameEnd = false;
                }

                if (!DemoScanner.UserAlive && (Math.Abs(DemoScanner.CurrentTime - DemoScanner.LastDeathTime) > 5.0f || DemoScanner.FirstUserAlive))
                {
                    DemoScanner.UserAlive = true;
                    if (DemoScanner.FirstUserAlive)
                    {
                        if (DemoScanner.DEBUG_ENABLED) Console.WriteLine("Alive 3 at " + DemoScanner.CurrentTimeString);

                        DemoScanner.LastAliveTime = DemoScanner.CurrentTime;
                    }
                    else
                    {
                        if (!DemoScanner.FirstBypassKill)
                        {
                            if (DemoScanner.BypassCount > 1)
                            {
                                ConsoleColor tmpcolor = Console.ForegroundColor;
                                Console.ForegroundColor = ConsoleColor.Gray;
                                Console.WriteLine("Warning (???) Tried to bypass demo scanner (" + DemoScanner.CurrentTimeString + ")");
                                Console.WriteLine("( dead user kill another player! dead user is alive! )");
                                Console.ForegroundColor = tmpcolor;
                            }
                            DemoScanner.BypassCount++;
                        }
                        else
                        {
                            DemoScanner.FirstBypassKill = false;
                            DemoScanner.NeedRescanDemoForce = true;
                        }
                    }
                    DemoScanner.FirstUserAlive = false;
                }
            }

            if (DemoScanner.DUMP_ALL_FRAMES) DemoScanner.OutDumpString += "User " + iVictim + " killed!\n";
        }

        private void MessageResetHud()
        {
        }

        private void MessageHealth()
        {
            byte health = BitBuffer.ReadByte();
        }

        private void MessageCrosshair()
        {
            byte cross = BitBuffer.ReadByte();
        }

        private void MessageSpectator()
        {
            byte clientid = BitBuffer.ReadByte();
            byte unknown = BitBuffer.ReadByte();
            if (DemoScanner.UserId2 + 1 == clientid)
                if (DemoScanner.UserAlive || DemoScanner.FirstUserAlive)
                {
                    if (!DemoScanner.FirstUserAlive)
                        DemoScanner.DeathsCoount++;
                    DemoScanner.LastDeathTime = DemoScanner.CurrentTime;
                    DemoScanner.FirstUserAlive = false;
                    DemoScanner.UserAlive = false;
                    DemoScanner.RealAlive = false;
                    Console.WriteLine("Forcing user dead because he join to spectator! (" + DemoScanner.CurrentTimeString + ")");
                }
        }

        private void MessageScoreAttrib()
        {
            byte pid = BitBuffer.ReadByte();
            byte flags = BitBuffer.ReadByte();
            if (pid == DemoScanner.UserId + 1 && (flags & 1) == 0)
                if (!DemoScanner.UserAlive)
                {
                    if (DemoScanner.DEBUG_ENABLED) Console.WriteLine("Alive 2 at " + DemoScanner.CurrentTimeString);

                    DemoScanner.UserAlive = true;
                    DemoScanner.LastAliveTime = DemoScanner.CurrentTime;
                    DemoScanner.FirstUserAlive = false;
                }
        }

        public double UnFixedUnsigned16(ushort value)
        {
            double output = Math.Round(value / Convert.ToDouble(1 << 12), 4, MidpointRounding.ToEven);
            return output;
        }

        private void MessageScreenFade()
        {
            double duration = UnFixedUnsigned16(BitBuffer.ReadUInt16());
            double holdTime = UnFixedUnsigned16(BitBuffer.ReadUInt16());
            ushort fadeFlags = BitBuffer.ReadUInt16();

            if ((fadeFlags & 4) > 0 && !DemoScanner.IsScreenFade)
            {
                DemoScanner.IsScreenFade = (fadeFlags & 0x0004) > 0;
                DemoScanner.DemoScanner_AddInfo("[Full screen fade START] at " + DemoScanner.CurrentTimeString);
            }
            else if (!((fadeFlags & 4) > 0) && DemoScanner.IsScreenFade)
            {
                DemoScanner.IsScreenFade = (fadeFlags & 0x0004) > 0;
                DemoScanner.DemoScanner_AddInfo("[Full screen fade END] at " + DemoScanner.CurrentTimeString);
            }

            byte r = BitBuffer.ReadByte();
            byte g = BitBuffer.ReadByte();
            byte b = BitBuffer.ReadByte();
            byte a = BitBuffer.ReadByte();
            if (DemoScanner.DUMP_ALL_FRAMES)
                DemoScanner.OutDumpString +=
                    "MessageScreenFade. DUR:" + duration + ". HOLD:" + holdTime +
                    ". RGBA:" + r + " " + g + " " + b + " " + a + "\n";

            if (DemoScanner.DEBUG_ENABLED)
                Console.WriteLine("MessageScreenFade. DUR:" + duration + ". HOLD:" + holdTime +
                                  ". RGBA:" + r + " " + g + " " + b + " " + a);
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

        public BitBufferOutOfRangeException(string message, Exception innerException) :
            base(message, innerException)
        {
        }

        protected BitBufferOutOfRangeException(SerializationInfo serializationInfo,
            StreamingContext streamingContext)
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
            if (data == null) throw new ArgumentNullException("data", "Value cannot be null.");

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
                CurrentBit += offset;
            else if (origin == SeekOrigin.Begin)
                CurrentBit = offset;
            else if (origin == SeekOrigin.End) CurrentBit = data.Count * 8 - offset;

            if (CurrentBit < 0 || CurrentBit > data.Count * 8) throw new BitBufferOutOfRangeException();
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
            int bitOffset = CurrentBit % 8;

            if (bitOffset != 0) SeekBits(8 - bitOffset);
        }

        // HL 1.1.0.6 bit reading (big endian byte and bit order)
        private uint ReadUnsignedBitsBigEndian(int nBits)
        {
            if (nBits <= 0 || nBits > 32)
                throw new ArgumentException(
                    "Value must be a positive integer between 1 and 32 inclusive.",
                    "nBits");

            // check for overflow
            if (CurrentBit + nBits > data.Count * 8) throw new BitBufferOutOfRangeException();

            int currentByte = CurrentBit / 8;
            int bitOffset = CurrentBit - currentByte * 8;
            int nBytesToRead = (bitOffset + nBits) / 8;

            if ((bitOffset + nBits) % 8 != 0) nBytesToRead++;

            // get bytes we need
            ulong currentValue = 0;
            for (int i = 0; i < nBytesToRead; i++)
            {
                byte b = data[currentByte + (nBytesToRead - 1) - i];
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
                throw new ArgumentException(
                    "Value must be a positive integer between 1 and 32 inclusive.",
                    "nBits");

            // check for overflow
            if (CurrentBit + nBits > data.Count * 8) throw new BitBufferOutOfRangeException();

            int currentByte = CurrentBit / 8;
            int bitOffset = CurrentBit - currentByte * 8;
            int nBytesToRead = (bitOffset + nBits) / 8;

            if ((bitOffset + nBits) % 8 != 0) nBytesToRead++;

            // get bytes we need
            ulong currentValue = 0;
            for (int i = 0; i < nBytesToRead; i++)
            {
                byte b = data[currentByte + i];
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
            if (Endian == EndianType.Little) return ReadUnsignedBitsLittleEndian(nBits);

            return ReadUnsignedBitsBigEndian(nBits);
        }

        public int ReadBits(int nBits)
        {
            int result = (int)ReadUnsignedBits(nBits - 1);
            int sign = ReadBoolean() ? 1 : 0;

            if (sign == 1) result = -((1 << (nBits - 1)) - result);

            return result;
        }

        public bool ReadBoolean()
        {
            // check for overflow
            if (CurrentBit + 1 > data.Count * 8) throw new BitBufferOutOfRangeException();

            bool result = (data[CurrentBit / 8] & (Endian == EndianType.Little
                    ? 1 << (CurrentBit % 8)
                    : 128 >> (CurrentBit % 8))) != 0;

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
            byte[] result = new byte[nBytes];

            for (int i = 0; i < nBytes; i++) result[i] = ReadByte();

            return result;
        }

        public char[] ReadChars(int nChars)
        {
            char[] result = new char[nChars];

            for (int i = 0; i < nChars; i++) result[i] = (char)ReadByte(); // not unicode

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
            int startBit = CurrentBit;
            string s = ReadString();
            SeekBits(length * 8 - (CurrentBit - startBit));
            return s;
        }

        public string ReadString()
        {
            List<byte> bytes = new List<byte>();

            while (true)
            {
                byte b = ReadByte();

                if (b == 0x00) break;

                bytes.Add(b);
            }

            if (bytes.Count == 0) return "";

            return Encoding.UTF8.GetString(bytes.ToArray());
        }

        public float[] ReadVectorCoord()
        {
            return ReadVectorCoord(false);
        }

        public float[] ReadVectorCoord(bool goldSrc)
        {
            bool xFlag = ReadBoolean();
            bool yFlag = ReadBoolean();
            bool zFlag = ReadBoolean();

            float[] result = new float[3];

            if (xFlag) result[0] = ReadCoord(goldSrc);

            if (yFlag) result[1] = ReadCoord(goldSrc);

            if (zFlag) result[2] = ReadCoord(goldSrc);

            return result;
        }

        public float ReadCoord()
        {
            return ReadCoord(false);
        }

        public float ReadCoord(bool goldSrc)
        {
            bool intFlag = ReadBoolean();
            bool fractionFlag = ReadBoolean();

            float value = 0.0f;

            if (!intFlag && !fractionFlag) return value;

            bool sign = ReadBoolean();
            uint intValue = 0;
            uint fractionValue = 0;

            if (intFlag)
            {
                if (goldSrc)
                    intValue = ReadUnsignedBits(12);
                else
                    intValue = ReadUnsignedBits(14) + 1;
            }

            if (fractionFlag)
            {
                if (goldSrc)
                    fractionValue = ReadUnsignedBits(3);
                else
                    fractionValue = ReadUnsignedBits(5);
            }

            value = intValue + fractionValue * 1.0f / 32.0f;

            if (sign) value = -value;

            return value;
        }

        /// <summary>
        ///     Sets all bits to zero, starting with the current bit and up to nBits.
        ///     Used for Fade to Black removal.
        /// </summary>
        /// <param name="nBits"></param>
        public void ZeroOutBits(int nBits)
        {
            for (int i = 0; i < nBits; i++)
            {
                int currentByte = CurrentBit / 8;
                int bitOffset = CurrentBit - currentByte * 8;

                byte temp = data[currentByte];
                temp -= (byte)(data[currentByte] & (1 << bitOffset));
                data[currentByte] = temp;

                CurrentBit++;
            }
        }

        public void PrintBits(StreamWriter writer, int nBits)
        {
            if (writer == null || nBits == 0) return;

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < nBits; i++) sb.AppendFormat("{0}", ReadBoolean() ? 1 : 0);

            writer.Write(sb + "\n");
        }

        public void InsertBytes(byte[] insertData)
        {
            if (insertData.Length == 0) return;

            if (CurrentBit % 8 != 0)
                throw new ApplicationException(
                    "InsertBytes can only be called if the current bit is aligned to byte boundaries.");

            data.InsertRange(CurrentByte, insertData);
            CurrentBit += insertData.Length * 8;
        }

        public void RemoveBytes(int count)
        {
            if (count == 0) return;

            if (CurrentBit % 8 != 0)
                throw new ApplicationException(
                    "RemoveBytes can only be called if the current bit is aligned to byte boundaries.");

            if (CurrentByte + count > Length) throw new BitBufferOutOfRangeException();

            data.RemoveRange(CurrentByte, count);
        }

        #region Properties

        /// <summary>
        ///     Data length in bytes.
        /// </summary>
        public int Length => data.Count;

        public int CurrentBit { get; private set; }

        public int CurrentByte => (CurrentBit - CurrentBit % 8) / 8;

        public int BitsLeft => data.Count * 8 - CurrentBit;

        public int BytesLeft => data.Count - CurrentByte;

        public byte[] Data => data.ToArray();

        public EndianType Endian { get; set; }

        #endregion
    }

    public class BitWriter
    {
        private readonly List<byte> data;
        private int currentBit;

        public BitWriter()
        {
            data = new List<byte>();
        }

        public byte[] Data => data.ToArray();

        public void WriteUnsignedBits(uint value, int nBits)
        {
            int currentByte = currentBit / 8;
            int bitOffset = currentBit - currentByte * 8;

            // calculate how many bits need to be written to the current byte
            int bitsToWriteToCurrentByte = 8 - bitOffset;
            if (bitsToWriteToCurrentByte > nBits) bitsToWriteToCurrentByte = nBits;

            // calculate how many bytes need to be added to the list
            int bytesToAdd = 0;

            if (nBits > bitsToWriteToCurrentByte)
            {
                int temp = nBits - bitsToWriteToCurrentByte;
                bytesToAdd = temp / 8;

                if (temp % 8 != 0) bytesToAdd++;
            }

            if (bitOffset == 0) bytesToAdd++;

            // add new bytes if needed
            for (int i = 0; i < bytesToAdd; i++) data.Add(new byte());

            int nBitsWritten = 0;

            // write bits to the current byte
            byte b = (byte)(value & ((1 << bitsToWriteToCurrentByte) - 1));
            b <<= bitOffset;
            b += data[currentByte];
            data[currentByte] = b;

            nBitsWritten += bitsToWriteToCurrentByte;
            currentByte++;

            // write bits to all the newly added bytes
            while (nBitsWritten < nBits)
            {
                bitsToWriteToCurrentByte = nBits - nBitsWritten;
                if (bitsToWriteToCurrentByte > 8) bitsToWriteToCurrentByte = 8;

                b = (byte)((value >> nBitsWritten) &
                           ((1 << bitsToWriteToCurrentByte) - 1));
                data[currentByte] = b;

                nBitsWritten += bitsToWriteToCurrentByte;
                currentByte++;
            }

            // set new current bit
            currentBit += nBits;
        }

        public void WriteBits(int value, int nBits)
        {
            WriteUnsignedBits((uint)value, nBits - 1);

            uint sign = value < 0 ? 1u : 0u;
            WriteUnsignedBits(sign, 1);
        }

        public void WriteBoolean(bool value)
        {
            int currentByte = currentBit / 8;

            if (currentByte > data.Count - 1) data.Add(new byte());

            if (value) data[currentByte] += (byte)(1 << (currentBit % 8));

            currentBit++;
        }

        public void WriteByte(byte value)
        {
            WriteUnsignedBits(value, 8);
        }

        public void WriteSByte(sbyte value)
        {
            WriteBits(value, 8);
        }

        public void WriteBytes(byte[] values)
        {
            for (int i = 0; i < values.Length; i++) WriteByte(values[i]);
        }

        public void WriteChars(char[] values)
        {
            for (int i = 0; i < values.Length; i++) WriteByte((byte)values[i]);
        }

        public void WriteInt16(short value)
        {
            WriteBits(value, 16);
        }

        public void WriteUInt16(ushort value)
        {
            WriteUnsignedBits(value, 16);
        }

        public void WriteInt32(int value)
        {
            WriteBits(value, 32);
        }

        public void WriteUInt32(uint value)
        {
            WriteUnsignedBits(value, 32);
        }

        public void WriteString(string value)
        {
            for (int i = 0; i < value.Length; i++) WriteByte((byte)value[i]);

            // null terminator
            WriteByte(0);
        }

        public void WriteString(string value, int length)
        {
            if (length < value.Length + 1)
                throw new ApplicationException(
                    "String length longer than specified length.");

            WriteString(value);

            // write padding 0's
            for (int i = 0; i < length - (value.Length + 1); i++) WriteByte(0);
        }

        public void WriteVectorCoord(bool goldSrc, float[] coord)
        {
            WriteBoolean(true);
            WriteBoolean(true);
            WriteBoolean(true);
            WriteCoord(goldSrc, coord[0]);
            WriteCoord(goldSrc, coord[1]);
            WriteCoord(goldSrc, coord[2]);
        }

        public void WriteCoord(bool goldSrc, float value)
        {
            WriteBoolean(true); // int flag
            WriteBoolean(true); // fraction flag

            // sign
            if (value < 0.0f)
                WriteBoolean(true);
            else
                WriteBoolean(false);

            uint intValue = (uint)value;

            if (goldSrc)
            {
                WriteUnsignedBits(intValue, 12);
                WriteUnsignedBits(0, 3); // FIXME
            }
            else
            {
                WriteUnsignedBits(intValue - 1, 14);
                WriteUnsignedBits(0, 5); // FIXME
            }
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
                Entry e = new Entry
                {
                    Name = name,
                    Value = null
                };

                entryList.Add(e);
            }

            public object FindEntryValue(string name)
            {
                Entry e = FindEntry(name);

                if (e == null) return null;

                return e.Value;
            }

            public void SetEntryValue(string name, object value)
            {
                Entry e = FindEntry(name);

                if (e == null)
                    throw new ApplicationException(
                        string.Format("Delta entry {0} not found.", name));

                e.Value = value;
            }

            public void SetEntryValue(int index, object value)
            {
                entryList[index].Value = value;
            }

            private Entry FindEntry(string name)
            {
                foreach (Entry e in entryList)
                    if (e.Name == name) return e;

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
            public enum EntryFlags
            {
                Byte = 1 << 0,
                Short = 1 << 1,
                Float = 1 << 2,
                Integer = 1 << 3,
                Angle = 1 << 4,
                TimeWindow8 = 1 << 5,
                TimeWindowBig = 1 << 6,
                String = 1 << 7,
                Signed = 1 << 31
            }

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
                object name = delta.FindEntryValue("name");
                object nBits = delta.FindEntryValue("nBits");
                object divisor = delta.FindEntryValue("divisor");
                object flags = delta.FindEntryValue("flags");
                object preMultiplier = delta.FindEntryValue("preMultiplier");

                AddEntry(name != null ? (string)name : "unnamed", nBits != null ? (uint)nBits : 0, divisor != null ? (float)divisor : 0.0f, preMultiplier != null ? (float)preMultiplier : 1.0f, flags != null ? (EntryFlags)(uint)flags : EntryFlags.Integer);
            }

            /// <summary>
            ///     Adds an entry manually. Should only need to be called when creating a delta_description_t structure.
            /// </summary>
            /// <param name="name"></param>
            /// <param name="nBits"></param>
            /// <param name="divisor"></param>
            /// <param name="flags"></param>
            public void AddEntry(string name, uint nBits, float divisor, float preMultiplier,
                EntryFlags flags)
            {
                Entry entry = new Entry
                {
                    Name = name,
                    nBits = nBits,
                    Divisor = divisor,
                    Flags = flags,
                    PreMultiplier = preMultiplier
                };

                entryList.Add(entry);
            }

            public HalfLifeDelta CreateDelta()
            {
                HalfLifeDelta delta = new HalfLifeDelta(entryList.Count);

                // create delta structure with the same entries as the delta decoder, but no data
                foreach (Entry e in entryList) delta.AddEntry(e.Name);

                return delta;
            }

            public void ReadDelta(BitBuffer bitBuffer, HalfLifeDelta delta)
            {
                ReadDelta(bitBuffer, delta, out byte[] bitmaskBytes);

            }

            public void ReadDelta(BitBuffer bitBuffer, HalfLifeDelta delta,
                out byte[] bitmaskBytes)
            {
                // read bitmask
                uint nBitmaskBytes = bitBuffer.ReadUnsignedBits(3);
                // TODO: error check nBitmaskBytes against nEntries

                if (nBitmaskBytes == 0)
                {
                    bitmaskBytes = null;
                    return;
                }

                bitmaskBytes = new byte[nBitmaskBytes];

                for (int i = 0; i < nBitmaskBytes; i++) bitmaskBytes[i] = bitBuffer.ReadByte();

                for (int i = 0; i < nBitmaskBytes; i++)
                    for (int j = 0; j < 8; j++)
                    {
                        int index = j + i * 8;

                        if (index == entryList.Count) return;

                        if ((bitmaskBytes[i] & (1 << j)) != 0)
                        {
                            if (DemoScanner.DUMP_ALL_FRAMES)
                                DemoScanner.OutDumpString +=
                                    "ENTRY(" + Name + ")" + entryList[index].Name + " = ";

                            object value = ParseEntry(bitBuffer, entryList[index]);
                            if (Name != null && entryList[index].Name != null)
                            {
                                if (Name == "entity_state_player_t")
                                    if (DemoScanner.LastPlayerEntity == DemoScanner.UserId2 + 1)
                                    {
                                        if (entryList[index].Name == "angles[1]")
                                        {
                                            float tmpAngle =
                                                value != null ? (float)value : 1.0f;

                                            DemoScanner.NewViewAngleSearcherAngle = tmpAngle;
                                        }

                                        if (entryList[index].Name == "gaitsequence")
                                        {
                                            uint seqnum =
                                                value != null ? (uint)value : 0;
                                            if (seqnum > 0 && !DemoScanner.UserAlive && !DemoScanner.Intermission && !DemoScanner.FirstUserAlive)
                                            {
                                                if (DemoScanner.DEBUG_ENABLED) Console.WriteLine("Alive 1 at " + DemoScanner.CurrentTimeString);

                                                DemoScanner.UserAlive = true;
                                                DemoScanner.LastAliveTime = DemoScanner.CurrentTime;
                                                DemoScanner.FirstUserAlive = false;
                                            }
                                            else if (seqnum == 6)
                                            {
                                                if (Math.Abs(DemoScanner.LastJumpHackFalseDetectionTime) > float.Epsilon && Math.Abs(DemoScanner.CurrentTime - DemoScanner.LastJumpHackFalseDetectionTime) > 5.0f) DemoScanner.LastJumpHackFalseDetectionTime = 0.0f;
                                                if (Math.Abs(DemoScanner.CurrentTime - DemoScanner.LastUnJumpTime) > 0.3f &&
                                                    Math.Abs(DemoScanner.CurrentTime - DemoScanner.LastJumpTime) > 0.3f)
                                                    if (!DemoScanner.IsPlayerAnyJumpPressed() && !DemoScanner.IsPlayerLossConnection())
                                                        if (Math.Abs(DemoScanner.CurrentTime - DemoScanner.LastJumpHackTime) > 2.5f && Math.Abs(DemoScanner.LastJumpHackFalseDetectionTime) < float.Epsilon)
                                                        {
                                                            DemoScanner.DemoScanner_AddWarn(
                                                                "[BHOP TYPE 3] at (" +
                                                                DemoScanner.CurrentTime + ") " + DemoScanner.CurrentTimeString, false, false);

                                                            DemoScanner.LastJumpHackTime = DemoScanner.CurrentTime;
                                                            DemoScanner.JumpHackCount++;
                                                        }
                                            }
                                        }
                                    }

                                if (Name == "weapon_data_t")
                                {
                                    if (entryList[index].Name == "m_flNextPrimaryAttack")
                                    {
                                        if (!DemoScanner.UserAlive && Math.Abs(DemoScanner.CurrentTime - DemoScanner.LastDeathTime) > 5.0)
                                        {
                                            //DemoScanner.FirstUserAlive = false;
                                            DemoScanner.UserAlive = true;
                                            DemoScanner.LastAliveTime = DemoScanner.CurrentTime;
                                            if (DemoScanner.DEBUG_ENABLED) Console.WriteLine("Alive 5 at " + DemoScanner.CurrentTimeString);
                                        }
                                        float reloadstatus =
                                            value != null ? (float)value : 1.0f;
                                        if (reloadstatus <= 0.016 ||
                                            DemoScanner.CurrentWeapon ==
                                            DemoScanner.WeaponIdType.WEAPON_DEAGLE &&
                                            reloadstatus <= 0.025)
                                        {
                                            DemoScanner.WeaponAvaiabled = true;
                                            DemoScanner.WeaponAvaiabledFrameId =
                                                DemoScanner.CurrentFrameIdWeapon;
                                            DemoScanner.WeaponAvaiabledFrameTime =
                                                DemoScanner.CurrentTime2;
                                        }
                                        else
                                        {
                                            if (DemoScanner.WeaponAvaiabled) DemoScanner.AutoAttackStrikesID++;

                                            DemoScanner.WeaponAvaiabled = false;
                                            DemoScanner.WeaponAvaiabledFrameId = 0;
                                            DemoScanner.WeaponAvaiabledFrameTime = 0.0f;
                                        }
                                    }

                                    if (entryList[index].Name == "m_fInReload")
                                    {
                                        uint reloadstatus = value != null ? (uint)value : 0;
                                        if (DemoScanner.LastWeaponReloadStatus == reloadstatus) DemoScanner.ReloadWarns = 0;
                                        DemoScanner.LastWeaponReloadStatus = reloadstatus;
                                        if (reloadstatus > 0)
                                        {
                                            if (DemoScanner.DEBUG_ENABLED) Console.WriteLine("User reload weapon");

                                            DemoScanner.IsReload = true;
                                            DemoScanner.AttackCheck = -1;
                                            DemoScanner.Reloads++;
                                            DemoScanner.ReloadTime = DemoScanner.CurrentTime;
                                            DemoScanner.StartReloadWeapon = DemoScanner.CurrentWeapon;
                                        }
                                        else
                                        {
                                            if (DemoScanner.RealAlive && DemoScanner.EndReloadWeapon == DemoScanner.CurrentWeapon && DemoScanner.StartReloadWeapon == DemoScanner.EndReloadWeapon)
                                            {
                                                if (!DemoScanner.IsRoundEnd() && Math.Abs(DemoScanner.CurrentTime - DemoScanner.ReloadTime) < 0.2)
                                                {
                                                    DemoScanner.ReloadWarns++;
                                                    if (DemoScanner.ReloadWarns > 1)
                                                    {
                                                        DemoScanner.ReloadWarns = 0;
                                                        DemoScanner.DemoScanner_AddWarn("[NO RELOAD " + DemoScanner.CurrentWeapon + "] hack at (" + DemoScanner.CurrentTime +
                                                            "):" + DemoScanner.CurrentTimeString);
                                                    }
                                                }
                                                else
                                                {
                                                    if (DemoScanner.ReloadWarns > 0) DemoScanner.ReloadWarns--;
                                                }
                                            }
                                            else
                                            {
                                                DemoScanner.ReloadWarns = 0;
                                            }
                                            DemoScanner.EndReloadWeapon = DemoScanner.CurrentWeapon;
                                            DemoScanner.Reloads2++;
                                            DemoScanner.IsReload = false;
                                        }
                                    }
                                }

                                if (Name == "clientdata_t")
                                {
                                    if (entryList[index].Name == "weaponanim")
                                        if (DemoScanner.SelectSlot > 0) DemoScanner.SkipNextAttack = 2;

                                    if (entryList[index].Name == "velocity[0]" || entryList[index].Name == "velocity[1]")
                                    {
                                        float velocity = value != null ? (float)value : 0.0f;
                                        if (Math.Abs(velocity) > float.Epsilon) DemoScanner.FoundVelocityTime = DemoScanner.CurrentTime;
                                        if (velocity > 100.0f || velocity < -100.0f) DemoScanner.FoundBigVelocityTime = DemoScanner.CurrentTime;
                                    }

                                    if (entryList[index].Name == "fov")
                                    {
                                        float fov = value != null ? (float)value : 0.0f;
                                        DemoScanner.ClientFov = fov;
                                    }

                                    if (entryList[index].Name == "fuser2")
                                    {
                                        float rg_jump_time = value != null ? (float)value : 0.0f;
                                        if (Math.Abs(rg_jump_time) < float.Epsilon)
                                        {
                                            DemoScanner.last_rg_jump_time = 9999.0f;
                                        }
                                        else
                                        {
                                            if (rg_jump_time > DemoScanner.last_rg_jump_time)
                                                if (DemoScanner.IsUserAlive())
                                                {
                                                    DemoScanner.JumpCount5++;
                                                    if (!DemoScanner.IsPlayerAnyJumpPressed() && !DemoScanner.IsPlayerLossConnection())
                                                        if (Math.Abs(DemoScanner.CurrentTime - DemoScanner.LastJumpHackTime) > 2.5f)
                                                        {
                                                            DemoScanner.DemoScanner_AddWarn(
                                                                "[JUMPHACK TYPE 2] at (" +
                                                                DemoScanner.CurrentTime + ") " + DemoScanner.CurrentTimeString, !DemoScanner.IsAngleEditByEngine());

                                                            DemoScanner.LastJumpHackTime = DemoScanner.CurrentTime;
                                                            DemoScanner.JumpHackCount++;
                                                        }
                                                }

                                            DemoScanner.last_rg_jump_time = rg_jump_time;
                                        }
                                    }

                                    if (entryList[index].Name == "health")
                                    {
                                        float hp = value != null ? (float)value : 0.0f;

                                        if (hp <= 0 && (DemoScanner.UserAlive || DemoScanner.FirstUserAlive))
                                        {
                                            DemoScanner.LastDeathTime = DemoScanner.CurrentTime;
                                            if (!DemoScanner.FirstUserAlive)
                                                DemoScanner.DeathsCoount++;

                                            DemoScanner.FirstUserAlive = false;
                                            DemoScanner.UserAlive = false;
                                            DemoScanner.RealAlive = false;
                                            if (DemoScanner.DEBUG_ENABLED) Console.WriteLine("LocalPlayer killed. Method : clientdata_t health! at " + DemoScanner.CurrentTimeString);
                                        }
                                    }

                                    if (entryList[index].Name == "punchangle[1]")
                                    {
                                        float angle = value != null ? (float)value : 0.0f;
                                    }

                                    if (entryList[index].Name == "flags")
                                    {
                                        uint flags = value != null ? (uint)value : 0;
                                        if ((flags & 4096) > 0 && !DemoScanner.PlayerFrozen)
                                        {
                                            DemoScanner.PlayerFrozen = true;
                                            DemoScanner.PlayerFrozenTime = DemoScanner.CurrentTime;
                                            if (DemoScanner.IsRussia)
                                                DemoScanner.DemoScanner_AddInfo("Игрок был поставлен на паузу (" + DemoScanner.CurrentTime +
                                                                                "):" + DemoScanner.CurrentTimeString);
                                            else
                                                DemoScanner.DemoScanner_AddInfo("Player been froze at (" + DemoScanner.CurrentTime +
                                                                                "):" + DemoScanner.CurrentTimeString);
                                        }
                                        else if (!((flags & 4096) > 0) && DemoScanner.PlayerFrozen)
                                        {
                                            DemoScanner.PlayerUnFrozenTime = DemoScanner.CurrentTime;
                                            DemoScanner.PlayerFrozen = false;
                                            if (DemoScanner.IsRussia)
                                                DemoScanner.DemoScanner_AddInfo("Игрок снят с паузы (" + DemoScanner.CurrentTime +
                                                                                "):" + DemoScanner.CurrentTimeString);
                                            else
                                                DemoScanner.DemoScanner_AddInfo("Player has been unfrozen at (" + DemoScanner.CurrentTime +
                                                                                "):" + DemoScanner.CurrentTimeString);
                                        }
                                    }
                                }

                                if ( /*this.name == "weapon_data_t" || */
                                    Name == "clientdata_t")
                                    if (entryList[index].Name == "m_iId")
                                    {
                                        DemoScanner.WeaponIdType weaponid =
                                            (DemoScanner.WeaponIdType)Enum.ToObject(
                                                typeof(DemoScanner.WeaponIdType), value);

                                        if (DemoScanner.CurrentWeapon != weaponid)
                                        {
                                            if (!DemoScanner.UsingAnotherMethodWeaponDetection)
                                                DemoScanner.UsingAnotherMethodWeaponDetection = true;
                                            if (DemoScanner.DEBUG_ENABLED) Console.WriteLine("Select weapon method 3 (" + DemoScanner.UsingAnotherMethodWeaponDetection + "):" + weaponid + ":" + DemoScanner.CurrentTimeString);

                                            if (DemoScanner.DUMP_ALL_FRAMES)
                                            {
                                                DemoScanner.OutDumpString +=
                                                    "\nCHANGE WEAPON 2 (" +
                                                    DemoScanner.CurrentTime + ") " + DemoScanner.CurrentTimeString;
                                                DemoScanner.OutDumpString +=
                                                    "\nFrom " + DemoScanner.CurrentWeapon +
                                                    " to " + weaponid;
                                                Console.WriteLine(
                                                    "CHANGE WEAPON 2 (" +
                                                    DemoScanner.CurrentTime + ") " + DemoScanner.CurrentTimeString);

                                                Console.WriteLine(
                                                    "From " + DemoScanner.CurrentWeapon +
                                                    " to " + weaponid);
                                            }

                                            DemoScanner.NeedSearchAim2 = false;
                                            DemoScanner.Aim2AttackDetected = false;
                                            DemoScanner.ShotFound = -1;

                                            DemoScanner.SelectSlot = 0;
                                            DemoScanner.WeaponChanged = true;
                                            DemoScanner.AmmoCount = 0;
                                            // DemoScanner.IsAttackSkipTimes = 0;
                                            if (DemoScanner.CurrentWeapon !=
                                                DemoScanner.WeaponIdType.WEAPON_NONE)
                                                DemoScanner.SkipNextAttack = 2;

                                            //DemoScanner.InitAimMissingSearch = -1;
                                            DemoScanner.IsNoAttackLastTime =
                                                DemoScanner.CurrentTime + 1.0f;
                                            DemoScanner.NeedCheckAttack = false;
                                            DemoScanner.CurrentWeapon = weaponid;

                                            if (!DemoScanner.UserAlive)
                                            {
                                                // DemoScanner.CurrentWeapon = DemoScanner.WeaponIdType.WEAPON_BAD;
                                            }
                                        }
                                    }

                                if (Name == "weapon_data_t" && DemoScanner.NeedCheckAttack)
                                    if (entryList[index].Name == "m_iClip")
                                    {
                                        DemoScanner.NeedCheckAttack = false;
                                        int ammocount = value != null ? (int)value : 0;

                                        if (ammocount != DemoScanner.AmmoCount && DemoScanner.IsUserAlive())
                                        {
                                            if (DemoScanner.DEBUG_ENABLED) Console.Write("Shot->");
                                            DemoScanner.attackscounter4++;
                                            /*if (DemoScanner.BadPunchAngle)
                                            {
                                                DemoScanner.attackscounter5++;
                                                if (DemoScanner.DEBUG_ENABLED)
                                                    Console.WriteLine("BadPunchAngle" + "(" +
                                                                      DemoScanner.CurrentTime + ") " + DemoScanner.CurrentTimeString);
                                            }*/
                                            if (DemoScanner.IsPlayerTeleport())
                                            {
                                                DemoScanner.attackscounter5++;
                                                if (DemoScanner.DEBUG_ENABLED)
                                                    Console.WriteLine("TELEPORT" + "(" +
                                                                      DemoScanner.CurrentTime + ") " + DemoScanner.CurrentTimeString);
                                                if (DemoScanner.INSPECT_BAD_SHOT)
                                                    Console.ReadKey();
                                            }
                                            else if (Math.Abs(DemoScanner.CurrentTime - DemoScanner.LastAngleManipulation) < 0.50f)
                                            {
                                                DemoScanner.attackscounter5++;
                                                if (DemoScanner.DEBUG_ENABLED)
                                                    Console.WriteLine("MANIPUL" + "(" +
                                                                      DemoScanner.CurrentTime + ") " + DemoScanner.CurrentTimeString);
                                                if (DemoScanner.INSPECT_BAD_SHOT)
                                                    Console.ReadKey();
                                            }
                                            else if (DemoScanner.IsTakeDamage())
                                            {
                                                DemoScanner.attackscounter5++;
                                                if (DemoScanner.DEBUG_ENABLED)
                                                    Console.WriteLine("DAMAGE" + "(" +
                                                                      DemoScanner.CurrentTime + ") " + DemoScanner.CurrentTimeString);
                                                if (DemoScanner.INSPECT_BAD_SHOT)
                                                    Console.ReadKey();
                                            }
                                            else if (DemoScanner.IsPlayerFrozen())
                                            {
                                                DemoScanner.attackscounter5++;
                                                if (DemoScanner.DEBUG_ENABLED)
                                                    Console.WriteLine("FROZEN" + "(" +
                                                                      DemoScanner.CurrentTime + ") " + DemoScanner.CurrentTimeString);
                                                if (DemoScanner.INSPECT_BAD_SHOT)
                                                    Console.ReadKey();
                                            }
                                            else if (Math.Abs(DemoScanner.CurrentTime - DemoScanner.LastCmdDuckTime) < 1.01f)
                                            {
                                                DemoScanner.attackscounter5++;
                                                if (DemoScanner.DEBUG_ENABLED)
                                                    Console.WriteLine("INDUCK" + "(" +
                                                                      DemoScanner.CurrentTime + ") " + DemoScanner.CurrentTimeString);
                                                if (DemoScanner.INSPECT_BAD_SHOT)
                                                    Console.ReadKey();
                                            }
                                            else if (Math.Abs(DemoScanner.CurrentTime - DemoScanner.LastCmdUnduckTime) < 0.2f)
                                            {
                                                DemoScanner.attackscounter5++;
                                                if (DemoScanner.DEBUG_ENABLED)
                                                    Console.WriteLine("INDUCK2" + "(" +
                                                                      DemoScanner.CurrentTime + ") " + DemoScanner.CurrentTimeString);
                                                if (DemoScanner.INSPECT_BAD_SHOT)
                                                    Console.ReadKey();
                                            }
                                            else if (DemoScanner.IsViewChanged())
                                            {
                                                DemoScanner.attackscounter5++;
                                                if (DemoScanner.DEBUG_ENABLED)
                                                    Console.WriteLine("VIEW" + "(" +
                                                                      DemoScanner.CurrentTime + ") " + DemoScanner.CurrentTimeString);
                                                if (DemoScanner.INSPECT_BAD_SHOT)
                                                    Console.ReadKey();
                                            }
                                            else if (Math.Abs(DemoScanner.CurrentTime - DemoScanner.LastLookDisabled) < 0.75f)
                                            {
                                                DemoScanner.attackscounter5++;
                                                if (DemoScanner.DEBUG_ENABLED)
                                                    Console.WriteLine("INLUK" + "(" +
                                                                      DemoScanner.CurrentTime + ") " + DemoScanner.CurrentTimeString);
                                                if (DemoScanner.INSPECT_BAD_SHOT)
                                                    Console.ReadKey();
                                            }
                                            else if (DemoScanner.IsPlayerFrozen())
                                            {
                                                DemoScanner.attackscounter5++;
                                                if (DemoScanner.DEBUG_ENABLED)
                                                    Console.WriteLine("FROZEN" + "(" +
                                                                      DemoScanner.CurrentTime + ") " + DemoScanner.CurrentTimeString);
                                                if (DemoScanner.INSPECT_BAD_SHOT)
                                                    Console.ReadKey();
                                            }
                                            else if (DemoScanner.IsPlayerLossConnection())
                                            {
                                                DemoScanner.attackscounter5++;
                                                if (DemoScanner.DEBUG_ENABLED)
                                                    Console.WriteLine("LAG" + "(" +
                                                                      DemoScanner.CurrentTime + ") " + DemoScanner.CurrentTimeString);
                                                if (DemoScanner.INSPECT_BAD_SHOT)
                                                    Console.ReadKey();
                                            }
                                            else if (DemoScanner.IsRoundEnd())
                                            {
                                                DemoScanner.attackscounter5++;
                                                if (DemoScanner.DEBUG_ENABLED)
                                                    Console.WriteLine("ROUND" + "(" +
                                                                      DemoScanner.CurrentTime + ") " + DemoScanner.CurrentTimeString);
                                                if (DemoScanner.INSPECT_BAD_SHOT)
                                                    Console.ReadKey();
                                            }
                                            else if (DemoScanner.HideWeapon)
                                            {
                                                DemoScanner.attackscounter5++;
                                                if (DemoScanner.DEBUG_ENABLED)
                                                    Console.WriteLine("HIDE" + "(" +
                                                                      DemoScanner.CurrentTime + ") " + DemoScanner.CurrentTimeString);
                                                if (DemoScanner.INSPECT_BAD_SHOT)
                                                    Console.ReadKey();
                                            }
                                            else if (DemoScanner.CurrentFrameDuplicated > 1)
                                            {
                                                DemoScanner.attackscounter5++;
                                                if (DemoScanner.DEBUG_ENABLED)
                                                    Console.WriteLine("DUP" + "(" +
                                                                      DemoScanner.CurrentTime + ") " + DemoScanner.CurrentTimeString);
                                                if (DemoScanner.INSPECT_BAD_SHOT)
                                                    Console.ReadKey();
                                            }
                                            /*else if (!DemoScanner.CurrentFrameOnGround)
                                            {
                                                DemoScanner.attackscounter5++;
                                                if (DemoScanner.DEBUG_ENABLED)
                                                    Console.WriteLine("FLY" + "(" +
                                                                      DemoScanner.CurrentTime + ") " + DemoScanner.CurrentTimeString);
                                                if (DemoScanner.INSPECT_BAD_SHOT)
                                                    Console.ReadKey();
                                            }*/
                                            else if (!DemoScanner.IsRealWeapon())
                                            {
                                                DemoScanner.attackscounter5++;
                                                if (DemoScanner.DEBUG_ENABLED)
                                                    Console.WriteLine("NOREAL" + "(" +
                                                                    DemoScanner.CurrentTime + ") " + DemoScanner.CurrentTimeString);
                                                if (DemoScanner.INSPECT_BAD_SHOT)
                                                    Console.ReadKey();
                                            }
                                            else if (DemoScanner.CurrentWeapon == DemoScanner.WeaponIdType.WEAPON_NONE
                                                     || DemoScanner.CurrentWeapon == DemoScanner.WeaponIdType.WEAPON_BAD
                                                     || DemoScanner.CurrentWeapon == DemoScanner.WeaponIdType.WEAPON_BAD2
                                                     || DemoScanner.CurrentWeapon == DemoScanner.WeaponIdType.WEAPON_C4
                                                     || DemoScanner.CurrentWeapon == DemoScanner.WeaponIdType.WEAPON_HEGRENADE
                                                     || DemoScanner.CurrentWeapon == DemoScanner.WeaponIdType.WEAPON_KNIFE
                                                     || DemoScanner.CurrentWeapon == DemoScanner.WeaponIdType.WEAPON_SMOKEGRENADE
                                                     || DemoScanner.CurrentWeapon == DemoScanner.WeaponIdType.WEAPON_FLASHBANG)
                                            {
                                                DemoScanner.attackscounter5++;
                                                if (DemoScanner.DEBUG_ENABLED) Console.WriteLine("NOWEAPON");
                                                if (DemoScanner.INSPECT_BAD_SHOT)
                                                    Console.ReadKey();
                                            }
                                            else
                                            {
                                                if (DemoScanner.DEBUG_ENABLED) Console.WriteLine("CLEAN");
                                            }
                                        }
                                        // Console.WriteLine("Attack " + DemoScanner.CurrentTime + " : " + DemoScanner.CurrentTimeString);
                                        //if (DemoScanner.LastWatchWeapon == DemoScanner.CurrentWeapon && DemoScanner.UserAlive && DemoScanner.AmmoCount > 0 && ammocount > 0 && DemoScanner.AmmoCount - ammocount > 0 && DemoScanner.AmmoCount - ammocount < 4)
                                        //{
                                        //    DemoScanner.SkipNextAttack2--;
                                        //    if (DemoScanner.IsAttack || DemoScanner.CurrentTime - DemoScanner.IsNoAttackLastTime < 0.1)
                                        //    {
                                        //        if (DemoScanner.CurrentWeapon == DemoScanner.WeaponIdType.WEAPON_FAMAS)
                                        //            DemoScanner.SkipNextAttack2 = 3;
                                        //        // Console.WriteLine("Attack");
                                        //    }
                                        //    else if (!DemoScanner.IsAttack && !DemoScanner.IsReload && DemoScanner.SkipNextAttack2 <= 0)
                                        //    {
                                        //        // DemoScanner.ShotFound = true;

                                        //        DemoScanner.TextComments.WriteLine("Detected [TRIGGER BOT] at (" + DemoScanner.CurrentTime + ") " + DemoScanner.CurrentTimeString);
                                        //        DemoScanner.AddViewDemoHelperComment("Detected [TRIGGER BOT]. Weapon:" + DemoScanner.CurrentWeapon.ToString(), 0.5f);
                                        //        Console.WriteLine("Detected [TRIGGER BOT] at (" + DemoScanner.CurrentTime + ") " + DemoScanner.CurrentTimeString);
                                        //        DemoScanner.BadAttackCount++;
                                        //        DemoScanner.LastBadAttackCount = DemoScanner.CurrentTime;

                                        //        DemoScanner.IsNoAttackLastTime = 0.0f;//fixme
                                        //    }
                                        //}

                                        DemoScanner.LastWatchWeapon = DemoScanner.CurrentWeapon;
                                        DemoScanner.WeaponChanged = false;
                                        DemoScanner.AmmoCount = ammocount;
                                    }
                            }

                            if (delta != null) delta.SetEntryValue(index, value);

                            if (DemoScanner.DUMP_ALL_FRAMES) DemoScanner.OutDumpString += "\n";
                        }
                    }
            }

            public byte[] CreateDeltaBitmask(HalfLifeDelta delta)
            {
                uint nBitmaskBytes =
                    (uint)(entryList.Count / 8 + (entryList.Count % 8 > 0 ? 1 : 0));
                byte[] bitmaskBytes = new byte[nBitmaskBytes];

                for (int i = 0; i < bitmaskBytes.Length; i++)
                    for (int j = 0; j < 8; j++)
                    {
                        int index = j + i * 8;

                        if (index >= entryList.Count) break;

                        if (delta.FindEntryValue(entryList[index].Name) != null) bitmaskBytes[i] |= (byte)(1 << j);
                    }

                return bitmaskBytes;
            }

            public void WriteDelta(BitWriter bitWriter, HalfLifeDelta delta,
                byte[] bitmaskBytes)
            {
                if (bitmaskBytes == null) // no bitmask bytes
                {
                    bitWriter.WriteUnsignedBits(0, 3);
                    return;
                }

                bitWriter.WriteUnsignedBits((uint)bitmaskBytes.Length, 3);

                for (int i = 0; i < bitmaskBytes.Length; i++) bitWriter.WriteByte(bitmaskBytes[i]);

                for (int i = 0; i < bitmaskBytes.Length; i++)
                    for (int j = 0; j < 8; j++)
                    {
                        int index = j + i * 8;

                        if (index == entryList.Count) return;

                        if ((bitmaskBytes[i] & (1 << j)) != 0) WriteEntry(delta, bitWriter, entryList[index]);
                    }
            }

            public static float ConvertToDelta(float val, float iDivisor, int nBits, bool isAngle)
            {
                bool negative = val < 0.0f;
                if (!isAngle)
                {
                    uint uval = (uint)Math.Abs(val * iDivisor);
                    return uval / iDivisor * (negative ? -1.0f : 1.0f);
                }
                else
                {
                    uint uval = (uint)Math.Abs(val / (360.0f / (1 << nBits)));
                    return uval * (360.0f / (1 << nBits)) * (negative ? -1.0f : 1.0f);
                }
            }

            private object ParseEntry(BitBuffer bitBuffer, Entry e)
            {
                bool signed = (e.Flags & EntryFlags.Signed) != 0;

                if ((e.Flags & EntryFlags.Byte) != 0)
                {
                    if (signed)
                    {
                        sbyte retval = (sbyte)ParseInt(bitBuffer, e);
                        if (DemoScanner.DUMP_ALL_FRAMES) DemoScanner.OutDumpString += retval + "(SByte)";

                        return retval;
                    }
                    else
                    {
                        byte retval = (byte)ParseUnsignedInt(bitBuffer, e);
                        if (DemoScanner.DUMP_ALL_FRAMES) DemoScanner.OutDumpString += retval + "(Byte)";

                        return retval;
                    }
                }

                if ((e.Flags & EntryFlags.Short) != 0)
                {
                    if (signed)
                    {
                        short retval = (short)ParseInt(bitBuffer, e);
                        if (DemoScanner.DUMP_ALL_FRAMES) DemoScanner.OutDumpString += retval + "(Int16)";

                        return retval;
                    }
                    else
                    {
                        ushort retval = (ushort)ParseUnsignedInt(bitBuffer, e);
                        if (DemoScanner.DUMP_ALL_FRAMES) DemoScanner.OutDumpString += retval + "(UInt16)";

                        return retval;
                    }
                }

                if ((e.Flags & EntryFlags.Integer) != 0)
                {
                    if (signed)
                        try
                        {
                            int retval = ParseInt(bitBuffer, e);
                            if (DemoScanner.DUMP_ALL_FRAMES) DemoScanner.OutDumpString += retval + "(Int32)";

                            return retval;
                        }
                        catch
                        {
                            return 0;
                        }

                    try
                    {
                        uint retval = ParseUnsignedInt(bitBuffer, e);
                        if (DemoScanner.DUMP_ALL_FRAMES) DemoScanner.OutDumpString += retval + "(UInt32)";

                        return retval;
                    }
                    catch
                    {
                        return (uint)0;
                    }
                }

                try
                {
                    if ((e.Flags & EntryFlags.Float) != 0 ||
                        (e.Flags & EntryFlags.TimeWindow8) != 0 ||
                        (e.Flags & EntryFlags.TimeWindowBig) != 0)
                    {
                        bool negative = false;
                        int bitsToRead = (int)e.nBits;

                        if (signed)
                        {
                            negative = bitBuffer.ReadBoolean();
                            bitsToRead--;
                        }

                        float retval =
                            bitBuffer.ReadUnsignedBits(bitsToRead) / e.Divisor *
                            (negative ? -1.0f : 1.0f);
                        if (DemoScanner.DUMP_ALL_FRAMES) DemoScanner.OutDumpString += retval + "(FloatBit)";

                        return retval;
                    }


                    if ((e.Flags & EntryFlags.Angle) != 0)
                    {
                        float retval =
                            bitBuffer.ReadUnsignedBits((int)e.nBits) *
                            (360.0f / (1 << (int)e.nBits));
                        if (DemoScanner.DUMP_ALL_FRAMES) DemoScanner.OutDumpString += retval + "(Float)";

                        return retval;
                    }
                }
                catch
                {
                    return 0.0f;
                }

                if ((e.Flags & EntryFlags.String) != 0)
                {
                    string retval = bitBuffer.ReadString();
                    if (DemoScanner.DUMP_ALL_FRAMES) DemoScanner.OutDumpString += retval + "(String)";

                    return retval;
                }

                throw new ApplicationException(
                    string.Format("Unknown delta entry type {0}.", e.Flags));
            }

            private int ParseInt(BitBuffer bitBuffer, Entry e)
            {
                bool negative = bitBuffer.ReadBoolean();
                return (int)bitBuffer.ReadUnsignedBits((int)e.nBits - 1) /
                    (int)e.Divisor * (negative ? -1 : 1);
            }

            private uint ParseUnsignedInt(BitBuffer bitBuffer, Entry e)
            {
                return bitBuffer.ReadUnsignedBits((int)e.nBits) / (uint)e.Divisor;
            }

            private void WriteEntry(HalfLifeDelta delta, BitWriter bitWriter, Entry e)
            {
                bool signed = (e.Flags & EntryFlags.Signed) != 0;
                object value = delta.FindEntryValue(e.Name);
                if (value == null) return;

                if ((e.Flags & EntryFlags.Byte) != 0)
                {
                    if (signed)
                    {
                        sbyte writeValue = (sbyte)value;
                        WriteInt(bitWriter, e, writeValue);
                    }
                    else
                    {
                        byte writeValue = (byte)value;
                        WriteUnsignedInt(bitWriter, e, writeValue);
                    }
                }
                else if ((e.Flags & EntryFlags.Short) != 0)
                {
                    if (signed)
                    {
                        short writeValue = (short)value;
                        WriteInt(bitWriter, e, writeValue);
                    }
                    else
                    {
                        ushort writeValue = (ushort)value;
                        WriteUnsignedInt(bitWriter, e, writeValue);
                    }
                }
                else if ((e.Flags & EntryFlags.Integer) != 0)
                {
                    if (signed)
                        WriteInt(bitWriter, e, (int)value);
                    else
                        WriteUnsignedInt(bitWriter, e, (uint)value);
                }
                else if ((e.Flags & EntryFlags.Angle) != 0)
                {
                    bitWriter.WriteUnsignedBits(
                        (uint)((float)value / (360.0f / (1 << (int)e.nBits))),
                        (int)e.nBits);
                }
                else if ((e.Flags & EntryFlags.String) != 0)
                {
                    bitWriter.WriteString((string)value);
                }
                else if ((e.Flags & EntryFlags.Float) != 0 ||
                         (e.Flags & EntryFlags.TimeWindow8) != 0 ||
                         (e.Flags & EntryFlags.TimeWindowBig) != 0)
                {
                    float writeValue = (float)value;
                    int bitsToWrite = (int)e.nBits;

                    if (signed)
                    {
                        bitWriter.WriteBoolean(writeValue < 0);
                        bitsToWrite--;
                    }

                    bitWriter.WriteUnsignedBits(
                        (uint)(Math.Abs(writeValue) * e.Divisor), bitsToWrite);
                }
                else
                {
                    throw new ApplicationException(
                        string.Format("Unknown delta entry type {0}.", e.Flags));
                }
            }

            private void WriteInt(BitWriter bitWriter, Entry e, int value)
            {
                int writeValue = value * (int)e.Divisor;

                bitWriter.WriteBoolean(writeValue < 0);
                bitWriter.WriteUnsignedBits((uint)Math.Abs(writeValue),
                    (int)e.nBits - 1);
            }

            private void WriteUnsignedInt(BitWriter bitWriter, Entry e, uint value)
            {
                uint writeValue = value * (uint)e.Divisor;
                bitWriter.WriteUnsignedBits((uint)Math.Abs(writeValue), (int)e.nBits);
            }

            public class Entry
            {
                public float Divisor;
                public EntryFlags Flags;
                public string Name;
                public uint nBits;
                public float PreMultiplier;
            }
        }

        public class bWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri uri)
            {
                WebRequest w = base.GetWebRequest(uri);
                w.Timeout = 1000 * 180;
                return w;
            }
        }
    }

    /*
    public class MachineLearn_CheckAngles
    {
        private readonly List<CheckAnglesStruct> ANGLES_DB;

        public int AnglesStructSize = 3;
        private readonly string filename;
        public bool is_file_exists = true;

        public MachineLearn_CheckAngles(string filename, int angles_count)
        {
            this.filename = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\" + filename;
            //Console.WriteLine(this.filename);
            AnglesStructSize = angles_count;
            ANGLES_DB = new List<CheckAnglesStruct>();
            try
            {
                ReadAnglesDB();
                if (ANGLES_DB.Count == 0) is_file_exists = false;
            }
            catch
            {
                is_file_exists = false;
            }
        }

        public void ReadAnglesDB()
        {
            List<float> anglescheck = new List<float>();
            CheckAnglesStruct checkAnglesStruct = new CheckAnglesStruct();
            using (BinaryReader reader = new BinaryReader(File.Open(filename, FileMode.Open)))
            {
                try
                {
                    while (reader.BaseStream.Position != reader.BaseStream.Length)
                    {
                        float val = reader.ReadSingle();
                        //Console.Write("[" + val + "]");
                        anglescheck.Add(val);
                        if (anglescheck.Count == AnglesStructSize)
                        {
                            checkAnglesStruct.anglescheck = new List<float>(anglescheck);
                            ANGLES_DB.Add(checkAnglesStruct);
                            anglescheck = new List<float>();
                            checkAnglesStruct = new CheckAnglesStruct();
                        }
                    }
                }
                catch
                {
                }
            }
        }

        public void WriteAnglesDB()
        {
            try
            {
                using (BinaryWriter writer = new BinaryWriter(File.Open(filename, FileMode.Create)))
                {
                    foreach (CheckAnglesStruct str in ANGLES_DB)
                        foreach (float val in str.anglescheck)
                            //Console.Write("[" + val + "]");
                            writer.Write(val);
                }
            }
            catch
            {
                Console.WriteLine("No access to DB");
            }
        }

        private static bool CmpTwoAngleArray(List<float> a1, List<float> a2, float precision)
        {
            if (a1.Count != a2.Count) return false;

            for (int i = 0; i < a1.Count; i++)
                if (Math.Abs(a1[i] - a2[i]) > precision) return false;
            return true;
        }

        public bool IsAnglesInDB(List<float> angles_in, float precision)
        {
            foreach (CheckAnglesStruct a in ANGLES_DB)
                if (CmpTwoAngleArray(a.anglescheck, angles_in, precision)) return true;
            return false;
        }

        public void AddAnglesToDB(List<float> angles_in, float precision = 0.0005f)
        {
            List<float> angles_out = new List<float>();
            foreach (float angle in angles_in) angles_out.Add((float)Math.Round(angle, 5, MidpointRounding.ToEven));
            if (angles_out.Count != AnglesStructSize)
            {
                Console.WriteLine("ERROR AddAnglesToDB VALUE");
                return;
            }
            if (IsAnglesInDB(angles_out, precision))
            {
                Console.WriteLine("IsAnglesInDB !!");
                return;
            }
            Console.WriteLine("NEW ANGLES");
            CheckAnglesStruct checkAnglesStruct = new CheckAnglesStruct
            {
                anglescheck = angles_out
            };
            ANGLES_DB.Add(checkAnglesStruct);
        }

        public class CheckAnglesStruct
        {
            public List<float> anglescheck;
        }
    }
        */
}