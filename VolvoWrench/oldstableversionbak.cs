using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Xml.Serialization;
using ConsoleTables;
using VolvoWrench.Demo_stuff;
using VolvoWrench.Demo_stuff.GoldSource;
using static VolvoWrench.DG.BitWriter;
using static VolvoWrench.DG.Common;
using MessageBox = System.Windows.Forms.MessageBox;

namespace VolvoWrench.DG
{
    /*#define IN_ATTACK 1
#define IN_JUMP	2
#define IN_DUCK	3
#define IN_FORWARD 4
#define IN_BACK	5
#define IN_USE	6
#define IN_CANCEL 7
#define IN_LEFT	 8
#define IN_RIGHT	9
#define IN_MOVELEFT	10
#define IN_MOVERIGHT 11
#define IN_ATTACK2	12
#define IN_RUN      13
#define IN_RELOAD	14
#define IN_ALT1		15
#define IN_SCORE	16 */
    public static class Program
    {
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

        public static List<string> outFrames = null;
        public static bool needsaveframes = false;

        public static float CurrentTime = 0.0f;
        public static float CurrentTime2 = 0.0f;


        public static float CurrentTime3 = 0.0f;
        public static List<string> hackList = new List<string>();
        public static List<bool> BHOPJumps = new List<bool>();
        public static List<float> BHOPJumpsTimes = new List<float>();
        public static bool IsJump = false;
        public static bool FirstJump = false;
        public static bool FirstAttack = false;


        public static bool IsDuck = false;
        public static bool FirstDuck = false;
        public static float LastUnDuckTime = 0.0f;
        public static float LastDuckTime = 0.0f;

        public static int JumpErrors = -1;
        public static int JumpErrors2 = -1;

        public static int UserId = -1;
        public static int UserId2 = -1;

        public static int jumpscount = 0;
        public static bool isgroundchange = false;

        public static Point oldoriginpos = new Point();
        public static int onground_and_alive_tests = 0;
        public static int speedhackdetects = -1;
        public static float speedhackdetect_time = 0.0f;

        public static int CurrentFrameLerp = 0;

        public static int LerpBeforeAttack = 0;
        public static int LerpBeforeStopAttack = 0;
        public static int LerpAfterAttack = 0;
        public static bool NeedDetectLerpAfterAttack = false;
        public static int LerpSearchFramesCount = 0;


        public static double Aim8CurrentFrameViewanglesX = 0.0;
        public static double Aim8CurrentFrameViewanglesY = 0.0;

        public static double CurrentFrameViewanglesX = 0.0;
        public static double CurrentFrameViewanglesY = 0.0;
        public static double PreviewFrameViewanglesX = 0.0;
        public static double PreviewFrameViewanglesY = 0.0;
        public static int AutoAttackStrikes = 0;
        public static int AutoAttackStrikesID = 0;
        public static int AutoAttackStrikesLastID = 0;
        public static double ViewanglesXBeforeBeforeAttack = 0.0;
        public static double ViewanglesYBeforeBeforeAttack = 0.0;

        public static double ViewanglesXBeforeAttack = 0.0;
        public static double ViewanglesYBeforeAttack = 0.0;

        public static double CurrentFramePunchangleZ = 0.0;
        public static string CurrentTimeString = "";
        public static double PreviewFramePunchangleZ = 0.0;

        public static bool NeedSearchViewAnglesAfterAttack = false;

        public static double ViewanglesXAfterAttack = 0.0;
        public static double ViewanglesYAfterAttack = 0.0;

        public static bool NeedSearchViewAnglesAfterAttackNext = true;

        public static double ViewanglesXAfterAttackNext = 0.0;
        public static double ViewanglesYAfterAttackNext = 0.0;


        public static double MinFrameViewanglesY = 10000.0;
        public static double MinFrameViewanglesX = 10000.0;

        // Сохранить lerp перед атакой
        // Сохранить lerp перед остановкой атаки
        // Сохранить lerp после атаки
        // Сохранить lerp через 5 фреймов после атаки


        public static bool CurrentFrameAttacked = false;
        public static bool PreviewFrameAttacked = false;
        public static bool PreviewFrameAlive = false;
        public static bool CurrentFrameAlive = false;
        public static bool RealAlive = false;
        public static bool CurrentFrameJumped = false;
        public static bool PreviewFrameJumped = false;
        public static bool CurrentFrameOnGround = false;
        public static bool PreviewFrameOnGround = false;

        public static bool CurrentFrameDuck = false;
        public static bool PreviewFrameDuck = false;

        public static int JumpHackCount = 0;
        public static int FakeLagAim = 0;

        public static List<int> FakeLagsValus = new List<int>();

        public static int JumpHackCount2 = 0;

        public static int FrameDuplicates = 0;

        public static float LastJumpHack = 0.0f;
        public static float LastJumpTime = 0.0f;
        public static float LastUnJumpTime = 0.0f;


        public static WeaponIdType CurrentWeapon = WeaponIdType.WEAPON_NONE;
        public static WeaponIdType StrikesWeapon = WeaponIdType.WEAPON_NONE;
        public static bool WeaponChanged = false;

        public static bool IsAttack = false;

        //public static int IsAttackSkipTimes = 0;
        public static int AmmoCount = 0;
        public static int AttackErrors = 0;
        public static float LastAttackHack = 0.0f;


        public static int SilentAimDetected = 0;

        public static int BadAttackCount = 0;
        public static int FalsePositives = 0;
        public static bool BadAttackFound = false;
        public static float LastBadAttackCount = 0.0f;
        public static float LastSilentAim = 0.0f;
        public static float IsNoAttackLastTime = 0.0f;
        public static float IsNoAttackLastTime2 = 0.0f;
        public static float IsAttackLastTime = 0.0f;

        public static int AttackCheck = -1;

        public static bool IsReload = false;

        public static int SelectSlot = 0;

        public static int SkipNextAttack = 0;
        public static int SkipNextAttack2 = 0;
        public static int AttackCurrentFrameI = 0;


        public static int attackscounter = 0;
        public static int attackscounter2 = 0;
        public static int attackscounter3 = 0;
        public static int attackscounter4 = 0;

        public static int JumpCount = 0;
        public static int JumpCount2 = 0;
        public static int JumpCount3 = 0;
        public static int JumpCount4 = 0;

        public static int DeathsCoount = 0;
        public static int DeathsCoount2 = 0;
        public static int KillsCount = 0;


        public static List<int> averagefps = new List<int>();
        public static List<double> averagefps2 = new List<double>();

        public static int LastTimeOut = -1;
        public static int LastTimeOutCount = 0;


        public static float LastTimeDesync = 0.0f;

        public static bool SecondFound = false;
        public static float CurrentTimeDesync = 0.0f;
        public static int CurrentFps = 0;
        public static int RealFpsMin = int.MaxValue;
        public static int RealFpsMax = int.MinValue;
        public static float LastFpsCheckTime = 0.0f;

        public static bool SecondFound2 = false;
        public static float CurrentTimeDesync2 = 0.0f;
        public static int CurrentFps2 = 0;
        public static int RealFpsMin2 = int.MaxValue;
        public static int RealFpsMax2 = int.MinValue;
        public static float LastFpsCheckTime2 = 0.0f;


        public static int LastCmdFrameId = 0;
        public static int CurrentFrameId = 0;
        public static int CurrentFrameIdWeapon = 0;

        public static int CaminCount = 0;
        public static int FrameCrash = 0;

        public static double CurrentSensitivity = 0.0f;
        public static List<double> PlayerSensitivityHistory = new List<double>();
        public static List<string> PlayerSensitivityHistoryStr = new List<string>();
        public static List<string> PlayerSensitivityHistoryStrTime = new List<string>();
        public static List<string> PlayerSensitivityHistoryStrWeapon = new List<string>();
        public static string LastSensWeapon = "";
        public static bool PlayerSensitivityWarning = false;


        public static int CurrentFrameIdAll = 0;
        public static int NeedSearchID = 0;
        public static Demo_stuff.L4D2Branch.PortalStuff.Result.DPoint3D viewanglesforsearch = new Demo_stuff.L4D2Branch.PortalStuff.Result.DPoint3D();

        /*
            
            Движение прицелом 4 кадра подряд: ВВЕРХ, ВНИЗ, ВВЕРХ, ВНИЗ
            Движение прицелом 4 кадра подряд: ВЛЕВО, ВПРАВО, ВЛЕВО, ВПРАВО
            
            > < > <
            ^_^_
            
         */

        public static bool CurrentMovementLeft = false;
        public static int LeftRightMovements = 0;
        public static bool CurrentMovementTop = false;
        public static int TopBottomMovements = 0;

        private static readonly string[] CheatMenu =
        {
            "togglepanic",
            "+radar",
            "-radar",
            "XYMenu",
            "#menu",
            "Menukey",
            "_menu"
        };

        private static readonly string[] KNIFEBOTS =
        {
            "+knifefirepovorot",
            "+headleftfire",
            "+interpfire",
            "+clchangerfire",
            "+fonarfire",
            "+un.head1",
            "+shot1back",
            "+back-knife-fire",
            "+gorightfire",
            "+backleftfire",
            "+ddfire",
            "+180knife-fire",
            "+180back-fire",
            "strafe-fire",
            "+leftkniferight",
            "+rightknifeleft",
            "+320knife",
            "lrfire",
            "+longattack",
            "+shotattack",
            "+knifehead",
            "+qslash",
            "+q_stab",
            "+q_slash",
            "fast_turn",
            "knivespeed",
            "+knife",
            "knife-numbers",
            "knifeaim"
        };

        private static readonly string[] ANTIFLASH =
        {
            "+aj",
            "ajf+",
            "sp1",
            "sp2",
            "sp0",
            "af",
            "+ssay",
            "tog1",
            "tog2",
            "noflash",
            "goggle_flash",
            "newell_noflash",
            "transparant",
            "wall",
            "RemoveFlash",
            "antiflash",
            "vis_transflash"
        };

        private static readonly string[] VOICE =
        {
            "hlss",
            "+PlayWAV",
            "-PlayWAV",
            "StartWAV",
            "extoggle",
            "trulalaraz",
            "exon",
            "+gg",
            "-gg",
            "ToggleWAV",
            "extoggle2"
        };

        private static readonly string[] NORECOIL =
        {
            "-necro2",
            "+FMS",
            "W.crs.1",
            "W.crs.2",
            "W.crs.3",
            "W.crs.6",
            "W.crs.7",
            "recoil",
            "+skill",
            "+F.nr.1",
            "+ssgatck",
            "ssgrest",
            "+[rand]_attack",
            "+[rand]_beginattack",
            "+elx.nlock",
            "+antirclbind",
            "antircl",
            "antirclon",
            "+necro2",
            "necro2",
            "t_rcak_47",
            "+rctc",
            "+lkdn",
            "+r",
            "r12",
            "recoil_usp",
            "r35",
            "r09",
            "r2",
            "r22",
            "pisr",
            "ak",
            "ps2",
            "hsducktogoff",
            "mp5r",
            "asnip",
            "recoilSw_usp",
            "para",
            "+[rand].mlt.nr",
            "antircl",
            "+reak",
            "-reak",
            "antirclon",
            "antircloff",
            "Rhox.HS+AT.v1",
            "+hson",
            "+recoilfire6",
            "rec2",
            "+recoilshoot",
            "+hS1_antirecoil",
            "+[[rand]]_attack",
            "+recoilfire1",
            "-recoilfire1",
            "+norec",
            "derecoil",
            "unr_switch",
            "+m4_recoil_middle",
            "+m4_recoil",
            "+head1",
            "+rshoot",
            "-rshoot",
            "+n0r3co1L",
            "normalrecoil",
            "nrmsgon",
            "unr_on",
            "+recoilshoot",
            "+reak2",
            "-reak2",
            "-No.Reko",
            "-No.Reko.1",
            "No.Recoil.Cyc",
            "+4burst.[[rand]]",
            "monster.shot",
            "fastweapup",
            "+fastattack",
            "-fastattack",
            "@fastattack",
            "recoilSw",
            "+PRO100",
            "+GarfIIeld|Moderator[Shot]",
            "vx_recoilamount",
            "r_recoil",
            "lzlrecoil_switch",
            "582recoil_switch",
            "recoil_time",
            "recoil_amount",
            "norec",
            "0ccrecoil",
            "+bindstfst",
            "recoilSw_scout",
            "newell_norecoil",
            "norec",
            "+ss9",
            "+ss9w",
            "+re1",
            "+I0wnYoUUnBeAtAbLE",
            "+DSFGFDGDFG",
            "+api",
            "+nrshoot8",
            "+sk0r_shoot",
            "+[n100J]arec",
            "+n89_+lookdown_arec",
            "+afire",
            "+antir",
            "+cVm.apist",
            "+bum.vecs.1",
            "#_arectog.$",
            "GoD.Recoil.ON",
            "recoilx",
            "recoily",
            "spread",
            "xyrecoil",
            "yrecoil",
            "Norecoilvalue",
            "autorecoil",
            "fum1nrecoil",
            "misc_norecoil"
        };

        private static readonly string[] АIМ =
        {
            "+rrr",
            "+hlhaim",
            "+onlyaim",
            "+aimshoot",
            "+doaim",
            "+thru",
            "+doshoot",
            "+ltfxv4",
            "+christcs",
            "+ltfxv4",
            "+christcs",
            "+christcs",
            "+christcs",
            "+christcs",
            "+christcs",
            "+christcs",
            "+ssa8353",
            "+@koraim",
            "+shot",
            "+ddd",
            "echo_on.[rand]",
            "+[rand].vlock",
            "1Toggle.ajxr",
            "+hhh",
            "+hed",
            "+acc",
            "defav",
            "alak1",
            "NewAimHead",
            "aura1",
            "anowon1",
            "MoRpHeUsha",
            "silahyok",
            "af_fire",
            "+$nonup",
            "Sc0rpi0nShead",
            "htfpure",
            "Headsoulassassin",
            "zeroheadSD",
            "imm",
            "shot111",
            "hspitch1",
            "+thru",
            "+nxhead2",
            "+koraim",
            "deagle][АIМ",
            "1SHOT",
            "a1",
            "SIR1",
            "+.333.shoot",
            "+.333.aim",
            "zbadassdone",
            "att",
            "key_aim",
            "aim_bot",
            "aim_key",
            "aimthru",
            "+ab_a1m",
            "+doaim",
            "aimspot",
            "nospread",
            "+aimkey",
            "-aimkey",
            "+aim",
            "-aim",
            "+nakeshooter",
            "+[[rand]]_attack",
            "aim_Silent",
            "bnup",
            "dshot",
            "+akra",
            "-akra",
            "+snp",
            "snp",
            "-snp",
            "humaim",
            "humaim0",
            "humaim1",
            "-doaim",
            "-thru",
            "+doshoot",
            "-doshoot",
            "+csgaim",
            "-csgaim",
            "vg_aimbot",
            "vg_botfov",
            "1337head1",
            "vwall",
            "vesp",
            "1exmsg1",
            "esno_bot",
            "bb_zglow",
            "resource_esp",
            "ns_health",
            "aimbot",
            "#aim",
            "#aimbot",
            "autoaim",
            "+uW5.vlock",
            "+lEl.doshoot",
            "Aim_active",
            "wpnAWP",
            "Prediction",
            "aim_Prediction",
            "vec4",
            "avadd",
            "pred",
            "predahead",
            "predback",
            "type",
            "bone",
            "silent",
            "hit",
            "lock",
            "vec_deagle",
            "ugdeagle",
            "Aimteam",
            "Aimmode",
            "Aimwall",
            "Aimrange",
            "Aimkey",
            "Aimbot",
            "AimXpos",
            "AimYpos",
            "Aimdraw",
            "Aimmethod",
            "adjust_forward",
            "autopistol",
            "aimbox"
        };

        private static readonly string[] CDHACK =
        {
            "cd_version",
            "cdon",
            "zl_version",
            "autospec"
        };

        private static readonly string[] HZKZHACK =
        {
            "xHack_chat",
            "-strafe2",
            "-superstrefy2",
            "-superstrefy",
            "-jumpbug3",
            "-jumpbug2",
            "-jumpbug",
            "-spowolnienie666",
            "xHack_BHOP",
            "xKz_yawspeed",
            "kyk_BHOP",
            "m4c_BHOP",
            "zhy_BHOP",
            "zhe_hope",
            "manhetten_project",
            "+christcs",
            "ltfxv4",
            "+pi",
            "do_strafe",
            "dzamp",
            "1ststrafe",
            "ststrafe",
            "strafescript",
            "kzh_autoduck",
            "gvd_on",
            "rightstrafe",
            "+sw23.str",
            "+slowmo",
            "slowspeed",
            "+fastgs4",
            "+123",
            "+fuh_loopx",
            "fastspeed",
            "kzh_speed",
            "kzh_fixedpitch",
            "kzh_autojump_units",
            "kzh_BHOP",
            "str_BHOP"
        };

        private static readonly string[] AWPHACK =
        {
            "awpon",
            "awpoff",
            "sniperon",
            "sniper",
            "+awpshot",
            "+dozoom",
            "+[G]_AWPskr1pT",
            "sphyawm",
            "+awpzoom",
            "+awpcon",
            "-awpcon",
            "awpcon",
            "awp_on",
            "+awpwh0re",
            "-awpwh0re",
            "+hY7.qsatt1",
            "+awpp",
            "mlt-qs.on",
            "+TUI.NKL"
        };

        private static readonly string[] BHOPHACK =
        {
            "+BHOP",
            "+IICUXx",
            "BHOPjump",
            "BHOP",
            "hop",
            "joop",
            "-BHOP",
            "@BHOP",
            "+bhp",
            "bhp2",
            "+_BHOP",
            "bhpon",
            "BHOP.on",
            "+buhop",
            "hkhop",
            "+jj",
            "-+",
            "+bb",
            "+hop",
            "jump",
            "+newbh",
            "@BHOP",
            "+jump_2",
            "bunny1",
            "bunny2",
            "bxam",
            "BHOP5x",
            "+cBHOP",
            "smp2",
            "+BH",
            "-gaz",
            "+b",
            "BHOP.tog",
            "my_bunnyjump",
            "+reinbo.atk",
            "BHOP+",
            "+cjump",
            "-zw_jumpbug",
            "+zw_jumpbug",
            "+funjump",
            "singleStrafe1",
            "djson",
            "+rjump",
            "+auto_jump",
            "+djump",
            "j",
            "+pulo",
            "my_jumpstop1",
            "+my_jump",
            "duckjump",
            "+bh",
            "+bh0p",
            "+@BHOP",
            "+GeTBYYmYJumPDoWn",
            "+sbh",
            "CX5yawn1",
            "+cmE_apist",
            "jumptoggle",
            "+BHOPon1",
            "BHOP#"
        };

        private static readonly string[] CheatScript =
        {
            "akvec",
            "lj",
            "+reloadCover",
            "strafe_toggle_on",
            "countjumped8",
            "countjumped7",
            "countjumped6",
            "longjumped",
            "longjumped8",
            "longjumped6",
            "longjumped7",
            "hang",
            "cam_tog",
            "+ramp",
            "7-8strafes",
            "strafe",
            "cjs",
            "kzh_speed",
            "kzh_speed",
            "+fuck",
            "+reloadCover",
            "count",
            "+air",
            "+II",
            "+Cux",
            "+YYY",
            "+speedatk",
            "+dd",
            "+silentdefuse",
            "sigblocked",
            "sigswitch",
            "siground",
            "sigalive",
            "sigdead",
            "fb_stop",
            "+speed50",
            "+attack3",
            "shot1",
            "dfire",
            "coloratk",
            "+quickshot",
            "secondary",
            "ppp",
            "180.on",
            "+www",
            "+me123",
            "+321",
            "+a",
            "+fire",
            "caglar",
            "+my_attack2",
            "on",
            "off",
            "+attak",
            "motd1",
            "fov",
            "drawvmode",
            "cellsmode",
            "fullbmode",
            "whitemode",
            "nightmode",
            "nosky",
            "wireframeWH",
            "Hs",
            "Headshot",
            "Noscope",
            "Sniper",
            "Norain",
            "3rdperson",
            "clearscreen",
            "Speedhack-",
            "Speedhack+",
            "Speedhack++",
            "Hotkey",
            "key",
            "hotkeys",
            "panic",
            "PanicKey",
            "Asus",
            "r_radarhack",
            "db_c4timer",
            "db_botfov",
            "blaon",
            "blaoff",
            "nobobon",
            "noboboff",
            "kuzaim",
            "kuzesp",
            "aimit",
            "aimheight",
            "osd1_alias0",
            "osd1_alias1",
            "vx_aimbot",
            "csxbot_aim",
            "csxbot_shoot",
            "ESno_bot_shoot",
            "ESno_bot_aim",
            "|1|fov",
            "|1|aim",
            "r_esp",
            "goggle_cheat",
            "GlowHack",
            "bot_fov",
            "bot_aim",
            "pubbot_fov",
            "pubbot_aim",
            "lzlwall_switch",
            "MaddinX_aimbot",
            "MaddinX_glowhack",
            "xfov",
            "xaim",
            "+vh1aim",
            "+hheadaim",
            "582wall_switch",
            "$trans",
            "$fov",
            "lambeat",
            "gspike",
            "cheat_walls",
            "cheat_whitewalls",
            "aaaaim",
            "aaaesp",
            "showmenx",
            "toggle_lambert",
            "toggle_cheat",
            "whfix_trace",
            "whfix_enable",
            "h4x0r_on",
            "h4x0r_off",
            "ulambert",
            "edge_togglebot",
            "edge_esp",
            "resaim_switch",
            "resshoot_switch",
            "grenade_drop_T",
            "grenade_drop_CT",
            "db_botfov",
            "SniperHideout",
            "autoreloadon",
            "autoreloadoff",
            "sjumpon",
            "sjumpoff",
            "rf",
            "2y",
            "6y",
            "MaddinX-esp",
            "MaddinX-radar",
            "0ccwall",
            "smokehack",
            "smokehack2",
            "enablehk",
            "swalk_on",
            "nr_switch",
            "p_function",
            "glhalhac0wnz",
            "r_halhac0wnz",
            "suomionparas",
            "glsuomionpop",
            "r_suomionpop",
            "twcon",
            "twcoff",
            "turnon",
            "wireon",
            "enable_wireframe",
            "enable_wallhack",
            "newzitneg",
            "newzitpos",
            "zoomcfgtog",
            "ci_wallhack",
            "ci_spiked",
            "ci_aim",
            "camoff",
            "glhack",
            "spamz0r",
            "wavins_on",
            "zoomitneg",
            "counterspec",
            "terrorspec",
            "counterspawn",
            "+h4x0r",
            "resaim_on",
            "resjump_on",
            "@smoke",
            "@esp",
            "csha",
            "cshb",
            "zzzavSheadDoc",
            "zzzavSchest",
            "zzzavScrotch",
            "cv_wire",
            "cv_wire0",
            "cv_wire1",
            "zwo",
            "drei",
            "vier",
            "dork_bot_aim",
            "dork_esp",
            "dodshoot+",
            "bdshooton",
            "headhitbox",
            "doaimon",
            "jumpshooton",
            "HeadBunnySD",
            "inv_tactical",
            "nobob1",
            "fczadv1",
            "an7hrax#2.10",
            "-fuh_left",
            "+mspeed",
            "+onlyaim",
            "+pistol",
            "dt-asniper",
            "+mysettings",
            "cvcam0",
            "speed_a2x",
            "alloff1",
            "aimbot_button",
            "aimbot_inverse",
            "radar_mode",
            "nadedodge",
            "radarfov",
            "betteraim",
            "AIMBOT_on",
            "fxswitch-on",
            "1DOSmp5",
            "fh-1",
            "toker1shota",
            "Satan1",
            "spycam",
            "wallsensitivity",
            "quakeguns",
            "alloff",
            "alloff2",
            "alloff6",
            "healthbar",
            "antiwallblock",
            "dnowon1",
            "cnowon1",
            "carrier",
            "vec_ak47",
            "MVnums",
            "vec20",
            "nosmoke",
            "prone",
            "nosmash",
            "wallhack",
            "RemoveSmoke",
            "dgl0",
            "antibrk",
            "Cl@ssic1",
            "dak47",
            "nvec",
            "r_r_attare",
            "sig_eak47",
            "+ss",
            "+ss12",
            "autoshoot",
            "nowall",
            "no_wall",
            "5w",
            "sc",
            "+epshot",
            "bC1.s1slot",
            "lag",
            "Lag_On.uI1",
            "lagme",
            "lagshot",
            "lag_on",
            "zoom",
            "zoom1",
            "de.black",
            "v0",
            "sA9.DuckLockOn",
            "+dQ8.rattack",
            "cycle",
            "c",
            "@say",
            "toggle",
            "+hahaha",
            "+cs",
            "min.TSCT",
            "1Toggle.jyvx",
            "+kk",
            "+sk",
            "spam",
            "flood",
            "+cr",
            "+hslock",
            "lagi-suka",
            "+s",
            "+inwiz#",
            "ddd",
            "+w",
            "sluts2",
            "last_pist",
            "qY6.lags",
            "duck_jump",
            "freeshot",
            "@megasay",
            "+fakereload",
            "+reload_fake",
            "spin_spin_1",
            "+shieldbug2",
            "+attgl",
            "shieldbugglock",
            "@re",
            "+fastturn",
            "fastturn",
            "+hspitch1",
            "+s1l3nT_d3fu$3",
            "+reinbo.ch",
            "+B.180",
            "+OSR.apist",
            "+sketched",
            "+pYA.gopeek",
            "+nake89.ba",
            "+ScopeShot",
            "+j58.pump.shot",
            "chase.on",
            "smooth",
            "decals",
            "wsky",
            "qdefuse",
            "+auto",
            "+KZW.OUI",
            "moo.evade1",
            "+lol",
            "balarina",
            "cheat",
            "minmdls",
            "xqzspeakon",
            "crash1",
            "xhairon",
            "[180turn]",
            "+[[rand]]peek",
            "Christ_hax",
            "phg_mdn",
            "#if",
            "True",
            "$path",
            "#echo",
            "VOIPLAYPath",
            "hack",
            "cheat",
            "#console",
            "#alias",
            "#path",
            "pinger",
            "haxx",
            "SCSHv4",
            "SCSHv4_cam",
            "sv_cheat",
            "sv_pure",
            "sv_consistency",
            "cvar_bypass",
            "Radarhack",
            "RadarTrace",
            "ChromeMap",
            "GetIt1.6",
            "PlayerESP",
            "NoColour",
            "screen",
            "Showimpacts",
            "bColor",
            "bgColor",
            "Lory",
            ".ini",
            "set",
            "navigation",
            "aventer",
            "rad_Size",
            "wh_DrawPlayer",
            "hook",
            "fullconf",
            "sdelay",
            "debug",
            "conback",
            "aimfirstpoint",
            "alloff",
            "autoconfig",
            "trans",
            "#+attack2",
            "+att2",
            "blood1",
            "saypub",
            "sequence",
            "sglow",
            "shoot",
            "showtarget",
            "smooth",
            "spinhack",
            "spycam",
            "stats_y",
            "taction",
            "text_background",
            "tglow",
            "time_x",
            "time_y",
            "trans",
            "unvacced",
            "visesp",
            "wa_active",
            "wa_h",
            "wa_title",
            "wa_w",
            "wa_x",
            "walltrans",
            "wbox",
            "weapon",
            "welcomeffx",
            "windowy",
            "wiremodels",
            "xbox",
            "xspy",
            "ybox",
            "yspy",
            "zh4aim",
            "zh4aimsens",
            "zoomcam",
            "zspy",
            "vac_settings",
            "aimshoot_on",
            "+cm1",
            "gsaim_on",
            "+un.head2",
            "time",
            "panic_mode",
            "panic_key",
            "bAutooffsets",
            "fmem_AllocVar",
            "buy0",
            "IngameCommunity",
            "speed",
            "soundmax",
            "confont",
            "saystats",
            "riflerates",
            "Weaponglow_one_ct",
            "Translamb",
            "Loadkey",
            "Savekey",
            "Speedkey",
            "Speedvalue",
            "Infobox",
            "mKeyUp",
            "Showcons",
            "HackName",
            "Time24",
            "FullBright",
            "WinampSong",
            "PWall",
            "HeadBunny",
            "aswitch1",
            "brightmodels",
            "bs_blue",
            "evilmodels",
            "+doshooton",
            "+dodshoot",
            "+bdshooton",
            "ltfxaspeed",
            "ltfxspeed",
            "hadj_f",
            "autospeed",
            "axasus",
            "nigger",
            "namespam",
            "radar",
            "Advertisement",
            "+cf1",
            "esp_fartimeout",
            "vis_wallhack",
            "misc_stats",
            "winamp",
            "addvec",
            "pistol_vec",
            "boxsize",
            "xqz",
            "mode",
            "aim_avdraw",
            "weapon_r",
            "a2",
            "add_light"
        };

        private static readonly string[] OGCHACK =
        {
            "oaim",
            "ofov",
            "esp",
            "aim",
            "xxxfov",
            "xxxaim",
            "600fov",
            "600esp",
            "159fov",
            "159esp",
            "$esp",
            "$aim",
            "crosshair_image",
            "crosshair_mask",
            "skint",
            "sigmenuopen",
            "pistolswitch",
            "knifeswitch",
            "$r_ndmax",
            "$r_coil",
            "d_bot",
            "d_esp",
            "snfglow",
            "xaim0",
            "skinct",
            "+aimshoot",
            "+spam",
            "+rspd",
            "ogco_nighthawk",
            "ogco_ak47",
            "autoedge",
            "+speeder",
            "sigmessage",
            "EntEsp"
        };

        private static readonly string[] ProjectVDCHACK =
        {
            "+ltfxv4",
            "togglehlss",
            "powerup_esp",
            "powerup_glow",
            "wc_human",
            "ns_cloak",
            "blinkglow",
            "vd"
        };

        private static readonly string[] BaDBoYHACK =
        {
            "aswitch0",
            "+gear2",
            "+panic",
            "+fusion"
        };

        private static readonly string[] OGXRebornHACK =
        {
            "lightchange",
            "vis_avdraw",
            "glowon"
        };

        private static readonly string[] FuRiousSP =
        {
            "hackhooked",
            "DrawPlayer",
            "config_deagle",
            "buybots_deagle",
            "enemyconfig",
            "+thrushoot",
            "menu_0",
            "menu_toggle",
            "shoot_on",
            "soundesp_s"
        };

        private static readonly string[] MPHAIMBOTv13 =
        {
            "MinDistance"
        };

        private static readonly string[] BiosbaseHack =
        {
            "HitPointDraw",
            "CleanScreen"
        };

        private static readonly string[] ESPHACK =
        {
            "BoxEsp",
            "Box",
            "weaponesp",
            "distanceesp",
            "nameesp",
            "entityesp",
            "faresp",
            "esp_size",
            "esp_name",
            "transparent",
            "esp_Visible",
            "DrawAimspot",
            "Aimspot",
            "esp_box",
            "espadd",
            "radar_range",
            "m_flAdj_h",
            "m_flAdj_l",
            "m_flBoxESP",
            "Mindistance",
            "draw",
            "HitESP",
            "LineESP",
            "CrosshairESP",
            "CrosshairESPFov",
            "HiddenESP",
            "GlowLambert",
            "TeamESP",
            "weaponglow",
            "far"
        };

        private static readonly string[] hack370hook =
        {
            "coltsetup",
            "avclear"
        };

        private static readonly string[] Absohack =
        {
            "cross",
            "debugESP",
            "wallmode",
            "wire",
            "absohack"
        };

        private static readonly string[] Unrealstealth =
        {
            "wall_hack",
            "wall_cool",
            "rem_flash",
            "#AK47"
        };

        private static readonly string[] ECCHack =
        {
            "misc_panickey",
            "esp_lambert",
            "glowshells"
        };

        private static readonly string[] RunScript =
        {
            "awalk",
            "wr",
            "speed_mod",
            "speed_walk",
            "swon",
            "silenton",
            "FDD"
        };

        private static CrossParseResult CurrentDemoFile;
        private static HalfLifeDemoParser halfLifeDemoParser;

        public static bool NeedCheckAttack;

        public static float ClientFov = 90.0f;
        public static float cdframeFov = 90.0f;
        public static string DemoName = "";

        public static bool UserAlive = false;

        public static bool NeedWriteAim = false;

        public static bool NewAttack = false;

        public static BinaryWriter ViewDemoHelperComments;
        public static TextWriter TextComments;
        private static readonly Stream TextCommentsStream = new MemoryStream();
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

        public static bool FoundFirstTime;
        public static bool AimType1FalseDetect;


        public static bool NeedSearchAim2;
        public static bool Aim2AttackDetected;

        public static int ShotFound = -1;
        public static WeaponIdType LastWatchWeapon = WeaponIdType.WEAPON_NONE;
        public static int ReallyAim2;
        public static int DisableAttackFromNextFrame;
        public static float LastJumpHackTime;
        public static bool NeedDetectBHOPHack;
        public static int LastTriggerCursor;
        public static float LastDeathTime;
        public static string ServerName = "";
        public static string MapName = "";
        public static string GameDir = "";
        public static List<Player> playerList = new List<Player>();
        public static List<Player> fullPlayerList = new List<Player>();
        public static int maxfalsepositiveaim3 = 6;
        public static bool FovHackDetected;
        public static bool ThirdHackDetected;
        public static bool SearchAim6 = false;
        public static List<WindowResolution> playerresolution = new List<WindowResolution>();
        public static int CheatKey;
        public static int Reloads;
        public static int Reloads2;
        public static bool WeaponAvaiabled;
        public static int WeaponAvaiabledFrameId;
        public static float WeaponAvaiabledFrameTime;
        public static bool UsingAnotherMethodWeaponDetection;


        public static int AimType7Frames;
        public static int OldAimType7Frames;
        public static int AimType7Event;
        public static int BadVoicePacket;


        public static void AddJumpToBHOPScan(bool isGround)
        {
            if (IsJump && IsUserAlive())
            {
                if (!isgroundchange)
                    if (isGround)
                    {
                        isgroundchange = true;
                        jumpscount++;
                    }

                if (isgroundchange)
                    if (!isGround)
                    {
                        isgroundchange = false;
                        jumpscount++;
                    }

                if (jumpscount > 4)
                {
                    BHOPJumps.Add(isGround);
                    BHOPJumpsTimes.Add(CurrentTime);
                }
            }
            else
            {
                isgroundchange = false;
                jumpscount = 0;
            }
        }

        public static void BHOPANALYZERMUTE()
        {
            var BHOPcount = 0;
            var i = 5;
            for (i = 5; i < BHOPJumps.Count - 5; i++)
                if (BHOPJumps[i] && IsUserAlive())
                    if (BHOPJumps[i - 2] == false)
                        if (BHOPJumps[i - 3] == false)
                            if (BHOPJumps[i - 4] == false)
                                if (BHOPJumps[i + 2] == false)
                                    if (BHOPJumps[i + 3] == false)
                                        if (BHOPJumps[i + 4] == false)
                                        {
                                            AddViewDemoHelperComment("Detected [BHOP] jump.", 1.00f);
                                            TextComments.WriteLine(
                                                "Detected [BHOP] jump on (" + BHOPJumpsTimes[i] + ") " + Program.CurrentTimeString);
                                            Console.WriteLine(
                                                "Detected [BHOP] jump on (" + BHOPJumpsTimes[i] + ") " + Program.CurrentTimeString);
                                            BHOPcount++;
                                            i += 5;
                                        }
        }

        public static void BHOPANALYZER()
        {
            var BHOPcount = 0;
            var i = 5;
            var firstdetectid = 0;
            for (i = 5; i < BHOPJumps.Count - 5; i++)
                if (BHOPJumps[i] && IsUserAlive())
                    if (BHOPJumps[i - 2] == false)
                        if (BHOPJumps[i - 3] == false)
                            if (BHOPJumps[i - 4] == false)
                                if (BHOPJumps[i + 2] == false)
                                    if (BHOPJumps[i + 3] == false)
                                        if (BHOPJumps[i + 4] == false)
                                        {
                                            if (firstdetectid == 0) firstdetectid = i;


                                            BHOPcount++;
                                            i += 5;
                                        }

            if (BHOPcount > 0)
            {
                TextComments.WriteLine("Detected hack with [BHOP]. Detect count:" + BHOPcount);
                Console.WriteLine("Detected hack with [BHOP]. Detect count:" + BHOPcount);
                Console.WriteLine("First usage at " + BHOPJumpsTimes[firstdetectid] + " second game time.");
            }

            if (JumpErrors > 0)
            {
                TextComments.WriteLine("Detected \"MOUSE JUMP\". Detect count:" + JumpErrors);
                Console.WriteLine("Detected \"MOUSE JUMP\". Detect count:" + JumpErrors);
                // Console.WriteLine("Last using at " + LastJumpHack + " second game time.");
            }

            if (JumpErrors2 > 0)
            {
                TextComments.WriteLine("Detected \"+jump;wait;-jump; like alias\". Detect count:" + JumpErrors2);
                Console.WriteLine("Detected \"+jump;wait;-jump; like alias\". Detect count:" + JumpErrors2);
                // Console.WriteLine("Last using at " + LastJumpHack + " second game time.");
            }
        }

        public static void CheckConsoleCommand(string s2)
        {
            var s = s2.Trim();

            if (FrameCrash > 10)
            {
                FirstDuck = false;
                FirstJump = false;
                FirstAttack = false;
            }

            if (File.Exists("commands.txt"))
            {
                File.AppendAllText("commands.txt", Program.CurrentTimeString + " : " + s + "\n");
                File.AppendAllText("commands.txt", "wait" + (CurrentFrameId - LastCmdFrameId) + ";\n");
            }


            if (s.ToLower().IndexOf("-") > -1) FrameCrash++;

            if (s.ToLower().IndexOf("+reload") > -1)
            {
                FrameCrash = 0;
                // IsReload = true;
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
                    if (CurrentTime - LastJumpTime < 1.0)
                    {
                        if (JumpHackCount < 10)
                        {
                            AddViewDemoHelperComment("Detected [XTREMEHACK] jump.", 1.00f);
                            TextComments.WriteLine("Detected [XTREMEHACK] jump on (" + CurrentTime + ") " + Program.CurrentTimeString);
                            Console.WriteLine("Detected [XTREMEHACK] jump on (" + CurrentTime + ") " + Program.CurrentTimeString);
                        }

                        JumpHackCount++;
                    }

                    CaminCount = 0;
                }
            }
            else if (s.ToLower().IndexOf("-camin") > -1)
            {
                CaminCount = 0;
            }

            if (s.ToLower().IndexOf("slot") > -1 || s.ToLower().IndexOf("invprev") > -1 ||
                s.ToLower().IndexOf("invnext") > -1)
            {
                SkipNextAttack = 1;
                SelectSlot = 2;
                NeedSearchAim2 = false;
                Aim2AttackDetected = false;
                ShotFound = -1;
            }

            if (s.ToLower().IndexOf("attack2") == -1)
            {
                if (s.ToLower().IndexOf("+attack") > -1)
                {
                    FirstAttack = true;
                    FrameCrash = 0;
                    attackscounter++;
                    if (IsUserAlive())
                    {
                        if (AutoAttackStrikes >= 4)
                        {
                            //var tmpcol = Console.ForegroundColor;
                            //Console.ForegroundColor = ConsoleColor.Gray;
                            TextComments.WriteLine("Detected [AIM TYPE 6] on (" + CurrentTime + "):" + Program.CurrentTimeString);
                            AddViewDemoHelperComment("Detected [AIM TYPE 6]. Weapon:" + CurrentWeapon);
                            Console.WriteLine("Detected [AIM TYPE 6] on (" + CurrentTime + "):" + Program.CurrentTimeString);
                            //SilentAimDetected++;
                            //Console.ForegroundColor = tmpcol;
                            /*if (Program.AutoAttackStrikes > 0)
                                Console.WriteLine("Reset combo 3 ( " + Program.AutoAttackStrikes + " )");*/
                            AutoAttackStrikes = 0;
                        }
                        else if (WeaponAvaiabled && CurrentFrameIdWeapon - WeaponAvaiabledFrameId <= 7
                                                 && CurrentFrameIdWeapon - WeaponAvaiabledFrameId > 1)
                        {
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
                                        /* if (Program.AutoAttackStrikes > 0)
                                             Console.WriteLine("Reset combo 5 ( " + Program.AutoAttackStrikes + " )");*/
                                        AutoAttackStrikes = 0;
                                        StrikesWeapon = CurrentWeapon;
                                    }
                                }

                                AutoAttackStrikes++;
                                /*Console.Write("Attack combo ( " + Program.AutoAttackStrikes + "; ID:" + Program.AutoAttackStrikesID + " ):");
                                Console.Write("[TIME:" + (CurrentTime2 - WeaponAvaiabledFrameTime).ToString() + "]");
                                Console.Write("[FRAMES:" + (CurrentFrameIdWeapon - WeaponAvaiabledFrameId).ToString() + "](" + CurrentTime + ") " + Program.CurrentTimeString + "\n");
                                */

                                if (AutoAttackStrikesLastID == AutoAttackStrikesID)
                                {
                                    /* if (Program.AutoAttackStrikes > 0)
                                         Console.WriteLine("Reset combo 6 ( " + Program.AutoAttackStrikes + " )");*/
                                    AutoAttackStrikes = 0;
                                }

                                AutoAttackStrikesLastID = AutoAttackStrikesID;
                            }
                            else
                            {
                                /* if (Program.AutoAttackStrikes > 0)
                                     Console.WriteLine("Reset combo 2 ( " + Program.AutoAttackStrikes + " )");*/
                                AutoAttackStrikes = 0;
                            }
                        }
                        else
                        {
                            /*if (Program.AutoAttackStrikes > 0)
                                Console.WriteLine("Reset combo ( " + Program.AutoAttackStrikes + " )");*/
                            AutoAttackStrikes = 0;
                        }

                        ViewanglesXBeforeBeforeAttack = PreviewFrameViewanglesX;
                        ViewanglesYBeforeBeforeAttack = PreviewFrameViewanglesY;
                        ViewanglesXBeforeAttack = CurrentFrameViewanglesX;
                        ViewanglesYBeforeAttack = CurrentFrameViewanglesY;
                        //Console.WriteLine("ViewanglesXBeforeAttack:" + ViewanglesXBeforeAttack);
                        //Console.WriteLine("ViewanglesYBeforeAttack:" + ViewanglesYBeforeAttack);
                        NeedSearchViewAnglesAfterAttack = true;
                        LerpBeforeAttack = CurrentFrameLerp;
                        NeedDetectLerpAfterAttack = true;
                        if (IsAttack)
                        {
                            AttackErrors++;
                            LastAttackHack = CurrentTime;
                        }

                        if (AttackCheck < 0)
                            //Console.WriteLine("Check attack start!");
                            AttackCheck = 1;
                        Aim2AttackDetected = false;
                        NeedSearchAim2 = true;
                        IsAttack = true;
                        IsAttackLastTime = CurrentTime;
                    }
                    else
                    {
                        AutoAttackStrikes = 0;
                        NeedSearchViewAnglesAfterAttack = false;
                        Aim2AttackDetected = false;
                        NeedSearchAim2 = false;
                        IsAttack = true;
                        IsAttackLastTime = 0.0f;
                    }

                    //Console.WriteLine("+attack1");
                    //Console.WriteLine("IsAttack " + CurrentTime);
                    // IsAttackSkipTimes = 2;
                    //AmmoCount = 0;
                    ShotFound = -1;
                    WeaponChanged = false;
                }
                else if (s.ToLower().IndexOf("-attack") > -1)
                {
                    attackscounter2++;

                    if (LastTriggerCursor == Console.CursorTop && !IsAttack)
                    {
                        if (FalsePositives < 3 && SilentAimDetected <= 0 && BadAttackCount <= 2)
                        {
                            Thread.Sleep(1000);
                            Console.SetCursorPosition(0, LastTriggerCursor - 1);
                            var tmpcol = Console.ForegroundColor;
                            Console.ForegroundColor = ConsoleColor.Gray;
                            Console.Write(
                                "\r( FALSE POSITIVE / ЛОЖНОЕ СРАБАТЫВАНИЕ ) TRIGGERBOT (" +
                                LastBadAttackCount + " second) ");
                            Console.ForegroundColor = tmpcol;
                            Console.SetCursorPosition(0, LastTriggerCursor);
                            BadAttackCount--;
                        }

                        FalsePositives++;
                        LastTriggerCursor--;
                    }

                    if (IsUserAlive())
                    {
                        LerpBeforeStopAttack = CurrentFrameLerp;
                        LerpSearchFramesCount = 9;
                    }
                    else
                    {
                        NeedSearchViewAnglesAfterAttack = false;
                    }

                    if (IsUserAlive() && CurrentTime != 0.0 &&
                        (CurrentTime - IsAttackLastTime < 0.05) & (IsAttackLastTime != CurrentTime))
                    {
                        var tmpcolor = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Gray;
                        AttackErrors++;
                        Console.ForegroundColor = tmpcolor;
                    }

                    NeedSearchAim2 = false;
                    NeedWriteAim = false;
                    IsNoAttackLastTime = CurrentTime;
                    IsNoAttackLastTime2 = CurrentTime2;
                    IsAttack = false;
                    AttackCheck = -1;
                    Aim2AttackDetected = false;
                    //Console.WriteLine("-attack3");
                    SelectSlot--;
                    if (ShotFound > 2)
                        //Console.WriteLine("Shots:" + ShotFound);
                        ReallyAim2 = 1;
                    else if (ShotFound > 1)
                        //Console.WriteLine("Shots2:" + ShotFound);
                        ReallyAim2 = 2;
                    ShotFound = -1;
                }
            }

            if (s.ToLower().IndexOf("+duck") > -1)
            {
                FrameCrash = 0;
                IsDuck = true;
                FirstDuck = true;
                LastDuckTime = CurrentTime;
            }
            else if (s.ToLower().IndexOf("-duck") > -1)
            {
                IsDuck = false;
                FirstDuck = true;
                LastUnDuckTime = CurrentTime;
            }

            if (s.ToLower().IndexOf("+jump") > -1)
            {
                FrameCrash = 0;
                FirstJump = true;
                if (IsUserAlive())
                {
                    JumpCount++;
                    if (LastJumpTime == CurrentTime && LastJumpTime > 0.0f)
                    {
                        JumpErrors++;
                        // Console.Write("11");
                        LastJumpHack = CurrentTime;
                        //if (JumpErrors < 50)
                        //{
                        //    Console.WriteLine("Detected бинд прыжка on колесо мыши on (" + Program.CurrentTime + ") " + Program.CurrentTimeString);
                        //    Program.TextComments.WriteLine("Detected бинд прыжка on колесо мыши on (" + Program.CurrentTime + ") " + Program.CurrentTimeString);
                        //}
                        //else if (JumpErrors == 50)
                        //{
                        //    Program.TextComments.WriteLine("Detected бинд прыжка on колесо мыши on (" + Program.CurrentTime + ") " + Program.CurrentTimeString);
                        //    Program.TextComments.WriteLine("Это будет последнее сообщение об этом скрипте");
                        //    Console.WriteLine("Detected бинд прыжка on колесо мыши on (" + Program.CurrentTime + ") " + Program.CurrentTimeString);
                        //    Console.WriteLine("Это будет последнее сообщение об этом скрипте");
                        //}
                        BHOPJumps.Clear();
                        BHOPJumpsTimes.Clear();
                    }

                    if (IsJump)
                    {
                        JumpErrors++;
                        // Console.Write("22");
                        LastJumpHack = CurrentTime;
                        //if (JumpErrors < 50)
                        //{
                        //    Console.WriteLine("Вероятно Detected скрипт распрыжки on (" + Program.CurrentTime + ") " + Program.CurrentTimeString);
                        //}
                        //else if (JumpErrors == 50)
                        //{
                        //    Console.WriteLine("Вероятно Detected скрипт распрыжки on (" + Program.CurrentTime + ") " + Program.CurrentTimeString);
                        //    Console.WriteLine("Это будет последнее сообщение об этом скрипте");
                        //}
                        BHOPJumps.Clear();
                        BHOPJumpsTimes.Clear();
                    }
                }

                LastJumpTime = CurrentTime;
                JumpHackCount2 = -1;
                IsJump = true;
            }
            else if (s.ToLower().IndexOf("-jump") > -1)
            {
                FirstJump = true;
                if (IsUserAlive())
                {
                    JumpCount4++;
                    if (LastJumpTime == CurrentTime && LastJumpTime > 0.0f)
                    {
                        //Console.Write("33");
                        JumpErrors++;
                        LastJumpHack = CurrentTime;
                        //if (JumpErrors < 50)
                        //{
                        //    Console.WriteLine("Detected бинд прыжка on колесо мыши on (" + Program.CurrentTime + ") " + Program.CurrentTimeString);
                        //    Program.TextComments.WriteLine("Detected бинд прыжка on колесо мыши on (" + Program.CurrentTime + ") " + Program.CurrentTimeString);
                        //}
                        //else if (JumpErrors == 50)
                        //{
                        //    Program.TextComments.WriteLine("Detected бинд прыжка on колесо мыши on (" + Program.CurrentTime + ") " + Program.CurrentTimeString);
                        //    Program.TextComments.WriteLine("Это будет последнее сообщение об этом скрипте");
                        //    Console.WriteLine("Detected бинд прыжка on колесо мыши on (" + Program.CurrentTime + ") " + Program.CurrentTimeString);
                        //    Console.WriteLine("Это будет последнее сообщение об этом скрипте");
                        //}
                        BHOPJumps.Clear();
                        BHOPJumpsTimes.Clear();
                    }

                    if (!IsJump)
                    {
                        if (FrameCrash < 3)
                            JumpErrors2++;
                        //Console.WriteLine(FrameCrash);
                        //Console.WriteLine("Jump unknown script at time: (" + Program.CurrentTime + ") " + Program.CurrentTimeString);
                        //Console.WriteLine("time: (" + (Program.CurrentTime - LastUnJumpTime) + ") ");
                        //Console.WriteLine("time: (" + (Program.CurrentTime - LastJumpTime) + ") ");
                        LastJumpHack = CurrentTime;
                        BHOPJumps.Clear();
                        BHOPJumpsTimes.Clear();
                    }

                    LastJumpTime = CurrentTime;
                }

                LastUnJumpTime = CurrentTime;
                JumpHackCount2 = 0;
                IsJump = false;
            }

            foreach (var cheat in BHOPHACK)
                if (s.ToLower() == cheat.ToLower())
                    hackList.Add("BHOPHACK:" + cheat);

            foreach (var cheat in CheatMenu)
                if (s.ToLower() == cheat.ToLower())
                    hackList.Add("CheatMenu:" + cheat);
            foreach (var cheat in KNIFEBOTS)
                if (s.ToLower() == cheat.ToLower())
                    hackList.Add("KNIFEBOTS:" + cheat);
            foreach (var cheat in ANTIFLASH)
                if (s.ToLower() == cheat.ToLower())
                    hackList.Add("ANTIFLASH:" + cheat);
            foreach (var cheat in VOICE)
                if (s.ToLower() == cheat.ToLower())
                    hackList.Add("VOICE:" + cheat);
            foreach (var cheat in NORECOIL)
                if (s.ToLower() == cheat.ToLower())
                    hackList.Add("NORECOIL:" + cheat);
            foreach (var cheat in АIМ)
                if (s.ToLower() == cheat.ToLower())
                    hackList.Add("АIМ:" + cheat);
            foreach (var cheat in CDHACK)
                if (s.ToLower() == cheat.ToLower())
                    hackList.Add("CDHACK:" + cheat);
            foreach (var cheat in HZKZHACK)
                if (s.ToLower() == cheat.ToLower())
                    hackList.Add("HZKZHACK(распрыжка, спидхак, и прочее):" + cheat);
            foreach (var cheat in AWPHACK)
                if (s.ToLower() == cheat.ToLower())
                    hackList.Add("AWPHACK:" + cheat);
            foreach (var cheat in CheatScript)
                if (s.ToLower() == cheat.ToLower())
                    hackList.Add("CheatScript:" + cheat);
            foreach (var cheat in OGCHACK)
                if (s.ToLower() == cheat.ToLower())
                    hackList.Add("OGCHACK:" + cheat);
            foreach (var cheat in ProjectVDCHACK)
                if (s.ToLower() == cheat.ToLower())
                    hackList.Add("ProjectVDCHACK:" + cheat);
            foreach (var cheat in BaDBoYHACK)
                if (s.ToLower() == cheat.ToLower())
                    hackList.Add("BaDBoYHACK:" + cheat);
            foreach (var cheat in OGXRebornHACK)
                if (s.ToLower() == cheat.ToLower())
                    hackList.Add("OGXRebornHACK:" + cheat);
            foreach (var cheat in FuRiousSP)
                if (s.ToLower() == cheat.ToLower())
                    hackList.Add("FuRiousSP:" + cheat);
            foreach (var cheat in MPHAIMBOTv13)
                if (s.ToLower() == cheat.ToLower())
                    hackList.Add("MPHAIMBOTv13:" + cheat);
            foreach (var cheat in BiosbaseHack)
                if (s.ToLower() == cheat.ToLower())
                    hackList.Add("BiosbaseHack:" + cheat);
            foreach (var cheat in hack370hook)
                if (s.ToLower() == cheat.ToLower())
                    hackList.Add("hack370hook:" + cheat);
            foreach (var cheat in Absohack)
                if (s.ToLower() == cheat.ToLower())
                    hackList.Add("Absohack:" + cheat);
            foreach (var cheat in Unrealstealth)
                if (s.ToLower() == cheat.ToLower())
                    hackList.Add("Unrealstealth:" + cheat);
            foreach (var cheat in ECCHack)
                if (s.ToLower() == cheat.ToLower())
                    hackList.Add("ECCHack:" + cheat);
            foreach (var cheat in RunScript)
                if (s.ToLower() == cheat.ToLower())
                    hackList.Add("RunScript:" + cheat);
            foreach (var cheat in ESPHACK)
                if (s.ToLower() == cheat.ToLower())
                    hackList.Add("ESPHACK:" + cheat);

            LastCmdFrameId = CurrentFrameId;
        }

        public static double GetDistance(Point p1, Point p2)
        {
            var xDelta = p1.X - p2.X;
            var yDelta = p1.Y - p2.Y;

            return Math.Sqrt(Math.Pow(xDelta, 2) + Math.Pow(yDelta, 2));
        }

        public static void PrintNodesRecursive(TreeNode oParentNode)
        {
            outFrames.Add(oParentNode.Text);
            // Start recursion on all subnodes.
            foreach (TreeNode oSubNode in oParentNode.Nodes) PrintNodesRecursive(oSubNode);
        }

        public static bool IsUserAlive()
        {
            return UserAlive && CurrentWeapon != WeaponIdType.WEAPON_NONE &&
                   CurrentWeapon != WeaponIdType.WEAPON_BAD
                   && CurrentWeapon != WeaponIdType.WEAPON_BAD2;
        }

        public static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;

            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
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
            ViewDemoHelperComments.Write((double)CurrentTime);
            ViewDemoHelperComments.Write(speed);
            ViewDemoHelperComments.Write(xcommentdata);
            ViewDemoHelperComments.Write(utf8.Length + 1);
            ViewDemoHelperComments.Write(utf8);
            ViewDemoHelperComments.Write(GetNullBytes(1));
        }


        public static void addresolution(int x, int y)
        {
            if (x != 0 && y != 0)
            {
                foreach (var s in playerresolution)
                    if (s.x == x && s.y == y)
                        return;
                var tmpres = new WindowResolution
                {
                    x = x,
                    y = y
                };
                playerresolution.Add(tmpres);
            }
        }

        public static string GetAim7String(int val1, int val2, int type, double angle)
        {
            if (val1 > 11) val1 = 11;

            if (val2 > 11) val2 = 11;

            val1 -= 1;
            val2 -= 1;

            val1 *= 10;
            val2 *= 10;

            var val3 = 100;
            if (angle < 0.0001)
                val3 = 10;
            else if (angle < 0.001)
                val3 = 20;
            else if (angle < 0.01)
                val3 = 30;
            else if (angle < 0.1)
                val3 = 40;
            else if (angle < 1.0)
                val3 = 50;
            else if (angle < 2.0)
                val3 = 60;
            else if (angle < 3.0)
                val3 = 70;
            else if (angle < 4.0)
                val3 = 80;
            else if (angle < 10.0)
                val3 = 90;
            else if (angle < 50.0)
            {
                val3 = 100;
            }
            else
            {
                val3 = 1000;
            }
            string detect = "Detected";
            string detect2 = "";

            if (type - 1 == 4)
            {
                detect = "WARN";
                detect2 = "(???)";

                if (val3 > 50 && (val2 > 50 || val1 > 50))
                {
                    detect = "Detected";
                }
            }

            if (Aim7PunchangleY != 0.0f)
            {
                detect = "WARN";
                detect2 = "(????)";
                val1 = 5;
                val2 = 5;
            }

            return detect + " [AIM TYPE 7." + (type - 1) + " MATCH1:" + val1 + "% MATCH2:" +
                   val2 + "% MATCH3:" + val3 + "%]" + detect2;
        }

        public static int invalidframes = 0;

        public static string lastnormalanswer = "";

        public static double nospreadtest = 0.0f;

        [STAThread]
        private static void Main(string[] args)
        {
            try
            {
                Console.SetBufferSize(Console.BufferWidth, 4096);
            }
            catch
            {
                Console.WriteLine("Output to external file...");
            }

            try
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Title =
                    "[ANTICHEAT/ANTIHACK] Unreal Demo Scanner v1.33b2. Demo:" + DemoName +
                    ". DEMO TIME: 00:00:00";
            }
            catch
            {

            }

            try
            {
                if (File.Exists("Frames.log"))
                {
                    needsaveframes = true;
                    File.Delete("Frames.log");
                    File.Create("Frames.log").Close();
                }
            }
            catch
            {

            }

            try
            {
                if (File.Exists("Commands.txt"))
                {
                    File.Delete("Commands.txt");
                    File.Create("Commands.txt").Close();
                }
            }
            catch
            {
            }

            try
            {
                if (File.Exists("Frames.log"))
                    File.AppendAllText("Frames.log",
                        "Полный дамп демо в текстовом формате\n");

                if (File.Exists("commands.txt"))
                    File.AppendAllText("commands.txt",
                        "Список некоторых команд отправленных игроком\n");

                outFrames = new List<string>();
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Unreal Demo Scanner v1.33b2");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("THIS BASE CONTAIN NEXT CHEAT/HACK:");
            }
            catch
            {
            }

            Console.WriteLine("[TRIGGER BOT]");
            Console.WriteLine("[AIM]");
            Console.WriteLine("[BHOP]");
            Console.WriteLine("[JUMPHACK]");
            Console.WriteLine("[WHEELJUMP]");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("(???) / WARN - есть высокая вероятность ложного срабатывания");
            Console.WriteLine("(???) / WARN - result can be false positive 50/50");
            Console.ForegroundColor = ConsoleColor.DarkGray;

            Console.WriteLine("Drag & drop .dem file. Or enter path manually:");

            var CurrentDemoFilePath = "";
            var filefound = false;
            if (args.Length > 0)
            {
                CurrentDemoFilePath = args[args.Length - 1].Replace("\"", "");
                if (!File.Exists(CurrentDemoFilePath))
                    Console.WriteLine("WATAL ERROR! File not found! Try again:");
                else
                    filefound = true;
            }

            if (!filefound)
            {
                CurrentDemoFilePath = Console.ReadLine().Replace("\"", "");
                while (!File.Exists(CurrentDemoFilePath))
                {
                    Console.WriteLine("File not found! Try again!");
                    CurrentDemoFilePath = Console.ReadLine().Replace("\"", "");
                }
            }

            CurrentDemoFile = CrossDemoParser.Parse(CurrentDemoFilePath);

            if (File.Exists(CurrentDemoFilePath.Remove(CurrentDemoFilePath.Length - 3) +
                            "log"))
            {
                try
                {
                    File.Delete(CurrentDemoFilePath.Remove(CurrentDemoFilePath.Length - 3) +
                                "log.bak");
                }
                catch
                {
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

            if (File.Exists(CurrentDemoFilePath.Remove(CurrentDemoFilePath.Length - 3) +
                            "cdb"))
            {
                try
                {
                    File.Delete(CurrentDemoFilePath.Remove(CurrentDemoFilePath.Length - 3) +
                                "cdb.bak");
                }
                catch
                {
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
                ViewDemoHelperComments = new BinaryWriter(new MemoryStream());
            }
            catch
            {
                Console.WriteLine(
                    "File " + CurrentDemoFilePath.Remove(CurrentDemoFilePath.Length - 3) +
                    "cdb" + " open error : No access to create!");
                Console.Write("No access to file... Try again!");
                Console.ReadKey();
                return;
            }

            ViewDemoHelperComments.Write(1751611137);
            ViewDemoHelperComments.Write(0);

            //Console.WriteLine("Demo type:" + CurrentDemoFile.Type.ToString());
            Console.WriteLine("DEMO " + Path.GetFileName(CurrentDemoFilePath) + " INFO");
            DemoName = Truncate(Path.GetFileName(CurrentDemoFilePath), 25);
            //PrintDemoDetails(CurrentDemoFile);
            //foreach (var s in CurrentDemoFile.DisplayData)
            //{
            //    Console.WriteLine(s.Item1.Trim().PadRight(30) + s.Item2.Trim());
            //}
            if (CurrentDemoFile.GsDemoInfo.ParsingErrors.Count > 0)
            {
                Console.WriteLine("Parse errors:");
                Console.ForegroundColor = ConsoleColor.DarkRed;
                foreach (var s in CurrentDemoFile.GsDemoInfo.ParsingErrors)
                    Console.WriteLine(s.Trim());
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Start demo analyze.....");

            Console.ForegroundColor = ConsoleColor.Cyan;
            halfLifeDemoParser = new HalfLifeDemoParser(CurrentDemoFile);
            var oldframe = new GoldSource.NetMsgFrame();


            for (var index = 0;
                index < CurrentDemoFile.GsDemoInfo.DirectoryEntries.Count;
                index++)
            {
                var entrynode =
                    new TreeNode("Directory entry [" + (index + 1) + "] - " +
                                 CurrentDemoFile.GsDemoInfo.DirectoryEntries[index]
                                     .FrameCount);
                foreach (var frame in CurrentDemoFile.GsDemoInfo.DirectoryEntries[index]
                    .Frames)
                {
                    CurrentTime2 = frame.Key.Time;
                    CurrentFrameIdWeapon++;
                    CurrentFrameIdAll++;

                    var row = index + "/" + frame.Key.FrameIndex + " " + "[" +
                              frame.Key.Time + "s]: " +
                              frame.Key.Type.ToString().ToUpper();
                    var node = new TreeNode();
                    var subnode = new TreeNode();

                    switch (frame.Key.Type)
                    {
                        case GoldSource.DemoFrameType.NetMsg:
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
                            invalidframes++;
                            break;
                    }

                    switch (frame.Key.Type)
                    {
                        case GoldSource.DemoFrameType.DemoStart:
                            break;
                        case GoldSource.DemoFrameType.ConsoleCommand:
                            {
                                if (needsaveframes)
                                {
                                    subnode.Text = "{\n";
                                    subnode.Text +=
                                        ((GoldSource.ConsoleCommandFrame)frame.Value).Command;
                                }

                                CheckConsoleCommand(
                                    ((GoldSource.ConsoleCommandFrame)frame.Value).Command);
                                if (needsaveframes) subnode.Text += "}\n";
                                break;
                            }
                        case GoldSource.DemoFrameType.ClientData:
                            {
                                NeedSearchID = CurrentFrameIdAll;

                                CurrentFrameId++;
                                if (needsaveframes) subnode.Text = "{\n";
                                var cdframe = (GoldSource.ClientDataFrame)frame.Value;

                                viewanglesforsearch = cdframe.Viewangles;
                                if (needsaveframes)
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

                                if (CurrentTime != 0.0)
                                {
                                    if (LastFpsCheckTime == 0.0)
                                    {
                                        LastFpsCheckTime = CurrentTime;
                                    }
                                    else
                                    {
                                        if ((CurrentTimeDesync =
                                            Math.Abs(CurrentTime - LastFpsCheckTime)) > 1.0)
                                        {
                                            if (needsaveframes)
                                                subnode.Text =
                                                    "CurrentFps:" + CurrentFps + "\n";
                                            if (CurrentFps > RealFpsMax)
                                                RealFpsMax = CurrentFps;

                                            if (CurrentFps < RealFpsMin && CurrentFps2 > 0)
                                                RealFpsMin = CurrentFps;

                                            SecondFound = true;
                                            LastFpsCheckTime = CurrentTime;
                                            averagefps.Add(CurrentFps);
                                            CurrentFps = 0;
                                        }
                                        else
                                        {
                                            CurrentFps++;
                                        }
                                    }
                                }

                                CurrentFrameViewanglesX = cdframe.Viewangles.X;
                                CurrentFrameViewanglesY = cdframe.Viewangles.Y;

                                if (RealAlive)
                                {
                                    var tmpXangle = AngleBetween(PreviewFrameViewanglesX, CurrentFrameViewanglesX);
                                    var tmpYangle = AngleBetween(PreviewFrameViewanglesY, CurrentFrameViewanglesY);

                                    // List<double> tmpSens = new List<double>();

                                    var anglecheat = 0.0022;
                                    if (PlayerSensitivityHistory.Count > 10)
                                    {
                                        anglecheat = 10.0;
                                        var i = PlayerSensitivityHistory.Count - 10;
                                        for (; i < PlayerSensitivityHistory.Count - 1; i++)
                                        {
                                            //tmpSens.Add(PlayerSensitivityHistory[i]);
                                            if (anglecheat > (PlayerSensitivityHistory[i] * 0.022) / 2.1)
                                                anglecheat = (PlayerSensitivityHistory[i] * 0.022) / 2.1;
                                        }
                                    }

                                    var anglecheat2 = 0.0003;

                                    if (PlayerSensitivityHistory.Count > 25)
                                    {
                                        anglecheat2 = 10.0;
                                        var i = PlayerSensitivityHistory.Count - 25;
                                        for (; i < PlayerSensitivityHistory.Count - 1; i++)
                                        {
                                            //tmpSens.Add(PlayerSensitivityHistory[i]);
                                            if (anglecheat2 > (PlayerSensitivityHistory[i] * 0.022) / 2.1)
                                                anglecheat2 = (PlayerSensitivityHistory[i] * 0.022) / 2.1;
                                        }
                                    }

                                    if (anglecheat < 0.0022) anglecheat = 0.0022;
                                    if (anglecheat2 < 0.0003) anglecheat2 = 0.0003;

                                    if (CurrentWeapon == WeaponIdType.WEAPON_AWP ||
                                        CurrentWeapon == WeaponIdType.WEAPON_SCOUT ||
                                        CurrentWeapon == WeaponIdType.WEAPON_G3SG1 ||
                                        CurrentWeapon == WeaponIdType.WEAPON_SG550)
                                    {
                                        anglecheat = anglecheat / 100.0;
                                        anglecheat2 = anglecheat2 / 150.0;
                                    }
                                    else if (
                                        CurrentWeapon == WeaponIdType.WEAPON_SG552 ||
                                        CurrentWeapon == WeaponIdType.WEAPON_AUG)
                                    {
                                        anglecheat = anglecheat / 50.0;
                                        anglecheat2 = anglecheat2 / 100.0;
                                    }
                                    if (Program.LastAim5DetectedReal != 0.0f && CurrentTime - Program.LastAim5DetectedReal >= 0.5f)
                                    {
                                        Program.LastAim5DetectedReal = 0.0f;
                                    }
                                    if (Program.LastAim5Detected != 0.0f && CurrentTime - Program.LastAim5Detected >= 0.5f)
                                    {
                                        Program.LastAim5Detected = 0.0f;
                                    }
                                    if (NewAttack)
                                    {
                                        if (PlayerSensitivityWarning)
                                        {
                                            PlayerSensitivityWarning = false;
                                            if ((Program.LastAim5DetectedReal != 0.0f &&
                                           CurrentTime - Program.LastAim5DetectedReal < 0.5f) || (Program.LastAim5Detected != 0.0f &&
                                            CurrentTime - Program.LastAim5Detected < 0.5f))
                                            {
                                                var tmpcol = Console.ForegroundColor;
                                                Console.ForegroundColor = ConsoleColor.Gray;
                                                TextComments.WriteLine(
                                                    "WARN [AIM TYPE 5] on (" + CurrentTime +
                                                    "):" + Program.CurrentTimeString);
                                                AddViewDemoHelperComment(
                                                    "WARN [AIM TYPE 5]. Weapon:" +
                                                    CurrentWeapon);
                                                Console.WriteLine(
                                                    "WARN [AIM TYPE 5] on (" + CurrentTime +
                                                    "):" + Program.CurrentTimeString + " (???)");
                                                Console.ForegroundColor = tmpcol;
                                                Program.LastAim5DetectedReal = 0.0f;
                                                Program.LastAim5Detected = 0.0f;
                                            }
                                        }
                                        else
                                        if (Program.LastAim5DetectedReal != 0.0f &&
                                            CurrentTime - Program.LastAim5DetectedReal < 0.5f)
                                        {
                                            TextComments.WriteLine(
                                                "Detected [AIM TYPE 5] on (" + LastAim5DetectedReal +
                                                "):" + Program.CurrentTimeString);
                                            AddViewDemoHelperComment(
                                                "Detected [AIM TYPE 5]. Weapon:" +
                                                CurrentWeapon);
                                            Console.WriteLine(
                                                "Detected [AIM TYPE 5] on (" + LastAim5DetectedReal +
                                                "):" + Program.CurrentTimeString);
                                            SilentAimDetected++;
                                            Program.LastAim5DetectedReal = 0.0f;
                                            Program.LastAim5Detected = 0.0f;
                                        }
                                        else if (Program.LastAim5Detected != 0.0f &&
                                            CurrentTime - Program.LastAim5Detected < 0.5f)
                                        {
                                            if (SilentAimDetected > 1 || JumpHackCount > 1)
                                            {
                                                TextComments.WriteLine(
                                                    "Detected [AIM TYPE 5] on (" + CurrentTime +
                                                    "):" + Program.CurrentTimeString);
                                                AddViewDemoHelperComment(
                                                    "Detected [AIM TYPE 5]. Weapon:" +
                                                    CurrentWeapon);
                                                Console.WriteLine(
                                                    "Detected [AIM TYPE 5] on (" + CurrentTime +
                                                    "):" + Program.CurrentTimeString);
                                                SilentAimDetected++;
                                            }
                                            else
                                            {
                                                var tmpcol = Console.ForegroundColor;
                                                Console.ForegroundColor = ConsoleColor.Gray;
                                                TextComments.WriteLine(
                                                    "Detected [AIM TYPE 5] on (" + LastAim5Detected +
                                                    "):" + Program.CurrentTimeString);
                                                AddViewDemoHelperComment(
                                                    "Detected [AIM TYPE 5]. Weapon:" +
                                                    CurrentWeapon);
                                                Console.WriteLine(
                                                    "Detected [AIM TYPE 5] on (" + LastAim5Detected +
                                                    "):" + Program.CurrentTimeString + " (???)");
                                                Console.ForegroundColor = tmpcol;
                                            }
                                            Program.LastAim5DetectedReal = 0.0f;
                                            Program.LastAim5Detected = 0.0f;
                                        }
                                    }


                                    {
                                        if (tmpXangle > 0.0 &&
                                        tmpXangle < anglecheat2)
                                        {
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
                                                //  foreach (var s in tmpSens)
                                                //  {
                                                //      Console.WriteLine(Math
                                                //.Round(s, 8,
                                                //    MidpointRounding.AwayFromZero)
                                                //.ToString("F5"));
                                                //  }
                                                //  Console.WriteLine(Math
                                                //.Round(tmpXangle / 0.022, 8,
                                                //    MidpointRounding.AwayFromZero)
                                                //.ToString("F5"));
                                                if (CurrentFrameAttacked
                                                        || PreviewFrameAttacked)
                                                {
                                                    Program.LastAim5Detected = CurrentTime;
                                                    PlayerSensitivityWarning = true;
                                                }
                                                else
                                                    Program.LastAim5DetectedReal = CurrentTime;
                                            }
                                        }
                                        else if (tmpXangle > 0.0 &&
                                        tmpXangle < anglecheat)
                                        {
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
                                                //  foreach (var s in tmpSens)
                                                //  {
                                                //      Console.WriteLine(Math
                                                //  .Round(s, 8,
                                                //      MidpointRounding.AwayFromZero)
                                                //  .ToString("F5"));
                                                //  }
                                                //  Console.WriteLine(Math
                                                //.Round(tmpXangle / 0.022, 8,
                                                //    MidpointRounding.AwayFromZero)
                                                //.ToString("F5"));
                                                if (CurrentFrameAttacked
                                                        || PreviewFrameAttacked)
                                                {
                                                    PlayerSensitivityWarning = true;
                                                }
                                                Program.LastAim5Detected = CurrentTime;
                                            }
                                        }
                                    }

                                    if (SecondFound)
                                    {
                                        if (CurrentSensitivity != 100.0 &&
                                            CurrentSensitivity > 0.0)
                                        {
                                            PlayerSensitivityHistory.Add(
                                                CurrentSensitivity);
                                            PlayerSensitivityHistoryStr.Add(Math
                                                .Round(CurrentSensitivity, 8,
                                                    MidpointRounding.AwayFromZero)
                                                .ToString("F5"));
                                            PlayerSensitivityHistoryStrTime.Add(
                                                "(" + CurrentTime + "): " + Program.CurrentTimeString);
                                            PlayerSensitivityHistoryStrWeapon.Add(
                                                LastSensWeapon);
                                        }

                                        CurrentSensitivity = 100.0;
                                    }

                                    if (tmpXangle > 0.0 && tmpXangle / 0.022 < CurrentSensitivity && !CurrentFrameAttacked
                                        && !PreviewFrameAttacked)
                                    {
                                        if (CurrentWeapon == WeaponIdType.WEAPON_NONE
                                          || CurrentWeapon == WeaponIdType.WEAPON_BAD
                                          || CurrentWeapon == WeaponIdType.WEAPON_BAD2)
                                        {
                                            //fixyou
                                        }
                                        else
                                        {

                                            LastSensWeapon = CurrentWeapon.ToString();
                                            CurrentSensitivity = tmpXangle / 0.022;
                                        }
                                    }
                                    if (AimType7Event == 2 || AimType7Event == 3
                                        || AimType7Event == 4 || AimType7Event == 5)
                                    {
                                        if (tmpXangle == 0.0 && tmpYangle == 0.0 && AimType7Frames < 20)
                                        {
                                            //Console.WriteLine("5");
                                            AimType7Frames++;
                                        }
                                        else
                                        {
                                            // Console.WriteLine("11");
                                            if (AimType7Frames > 2)
                                            {
                                                if (Aim8CurrentFrameViewanglesY !=
                                                    CurrentFrameViewanglesY &&
                                                    Aim8CurrentFrameViewanglesX !=
                                                    CurrentFrameViewanglesX)
                                                {
                                                    var tmpcol = Console.ForegroundColor;
                                                    Console.ForegroundColor = ConsoleColor.Gray;

                                                    var tmpangle2 =
                                                        AngleBetween(
                                                                Aim8CurrentFrameViewanglesX, CurrentFrameViewanglesX);
                                                    tmpangle2 +=
                                                        AngleBetween(
                                                                Aim8CurrentFrameViewanglesY, CurrentFrameViewanglesY);


                                                    TextComments.WriteLine(
                                                        GetAim7String(OldAimType7Frames,
                                                            AimType7Frames, AimType7Event,
                                                            tmpangle2) + " on (" + CurrentTime +
                                                        "):" + Program.CurrentTimeString + " (???)");
                                                    AddViewDemoHelperComment(
                                                        GetAim7String(OldAimType7Frames,
                                                            AimType7Frames, AimType7Event,
                                                            tmpangle2) + " Weapon:" +
                                                        CurrentWeapon + " (???)");
                                                    Console.WriteLine(
                                                        GetAim7String(OldAimType7Frames,
                                                            AimType7Frames, AimType7Event,
                                                            tmpangle2) + " on (" + CurrentTime +
                                                        "):" + Program.CurrentTimeString + " (???)");
                                                    Console.ForegroundColor = tmpcol;
                                                }
                                            }

                                            if (Aim8CurrentFrameViewanglesY ==
                                                    CurrentFrameViewanglesY/* &&
                                                    Aim8CurrentFrameViewanglesX !=
                                                    CurrentFrameViewanglesX*/)
                                            {
                                                AimType7Event += 10;
                                                var tmpcol = Console.ForegroundColor;
                                                Console.ForegroundColor = ConsoleColor.Gray;

                                                var tmpangle2 =
                                                    AngleBetween(
                                                            Aim8CurrentFrameViewanglesX, CurrentFrameViewanglesX);
                                                tmpangle2 +=
                                                    AngleBetween(
                                                            Aim8CurrentFrameViewanglesY, CurrentFrameViewanglesY);


                                                TextComments.WriteLine(
                                                    GetAim7String(OldAimType7Frames,
                                                        AimType7Frames, AimType7Event,
                                                        tmpangle2) + " on (" + CurrentTime +
                                                    "):" + Program.CurrentTimeString + " (???)");
                                                AddViewDemoHelperComment(
                                                    GetAim7String(OldAimType7Frames,
                                                        AimType7Frames, AimType7Event,
                                                        tmpangle2) + " Weapon:" +
                                                    CurrentWeapon + " (???)");
                                                Console.WriteLine(
                                                    GetAim7String(OldAimType7Frames,
                                                        AimType7Frames, AimType7Event,
                                                        tmpangle2) + " on (" + CurrentTime +
                                                    "):" + Program.CurrentTimeString + " (???)");
                                                Console.ForegroundColor = tmpcol;
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
                                        if (tmpXangle != 0.0 && tmpYangle != 0.0)
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
                                        if (tmpXangle != 0.0 && tmpYangle != 0.0)
                                            AimType7Event = 4;
                                        else
                                            AimType7Event = 5;
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
                                        if (tmpXangle == 0.0 && tmpYangle == 0.0/* &&
                                            !CurrentFrameAttacked*/)
                                        {
                                            // Console.WriteLine("3");
                                            Aim8CurrentFrameViewanglesY =
                                                CurrentFrameViewanglesY;
                                            Aim8CurrentFrameViewanglesX =
                                                CurrentFrameViewanglesX;
                                            AimType7Frames++;
                                        }
                                        // Иначе Если угол изменился и набралось больше 1 таких кадров то вкл поиск аим
                                        else
                                        {
                                            if (AimType7Frames > 1
                                                && tmpXangle != 0.0 && tmpYangle != 0.0)
                                            {
                                                //Console.WriteLine("4");
                                                OldAimType7Frames = AimType7Frames;
                                                AimType7Event = 1;
                                                if (CurrentFrameAttacked)
                                                    AimType7Event = 11;
                                            }
                                            AimType7Frames = 0;
                                        }
                                    }
                                }
                                else
                                {
                                    Program.LastAim5DetectedReal = 0.0f;
                                    Program.LastAim5Detected = 0.0f;
                                    AimType7Event = 0;
                                    AimType7Frames = 0;
                                }

                                //Console.Write(RealAlive);
                                //Console.Write(" ");
                                //Console.Write(CurrentSensitivity);
                                //Console.Write(" ");
                                //Console.Write(tmpXangle);
                                //Console.Write(" ");
                                //Console.Write(SecondFound);
                                //Console.Write("\n");

                                PreviewFrameViewanglesX = CurrentFrameViewanglesX;
                                PreviewFrameViewanglesY = CurrentFrameViewanglesY;
                                break;
                            }
                        case GoldSource.DemoFrameType.NextSection:
                            {
                                if (needsaveframes)
                                    subnode.Text = @"End of the DirectoryEntry!";
                                break;
                            }
                        case GoldSource.DemoFrameType.Event:
                            {
                                if (needsaveframes) subnode.Text = "{\n";
                                var eframe = (GoldSource.EventFrame)frame.Value;
                                if (needsaveframes)
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
                                        @"Origin.Y = " + eframe.EventArguments.Origin.Y + "\n";
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

                                break;
                            }
                        case GoldSource.DemoFrameType.WeaponAnim:
                            {
                                if (needsaveframes) subnode.Text = "{\n";
                                var waframe = (GoldSource.WeaponAnimFrame)frame.Value;
                                if (needsaveframes)
                                {
                                    subnode.Text += @"Anim = " + waframe.Anim + "\n";
                                    subnode.Text += @"Body = " + waframe.Body + "\n";

                                    subnode.Text += "}\n";
                                }

                                break;
                            }
                        case GoldSource.DemoFrameType.Sound:
                            {
                                if (needsaveframes) subnode.Text = "{\n";
                                var sframe = (GoldSource.SoundFrame)frame.Value;
                                if (needsaveframes)
                                {
                                    subnode.Text += @"Channel = " + sframe.Channel + "\n";
                                    subnode.Text += @"Sample = " + sframe.Sample + "\n";
                                    subnode.Text += @"Attenuation = " + sframe.Attenuation + "\n";
                                    subnode.Text += @"Volume = " + sframe.Volume + "\n";
                                    subnode.Text += @"Flags = " + sframe.Flags + "\n";
                                    subnode.Text += @"Pitch = " + sframe.Pitch + "\n";

                                    subnode.Text += "}\n";
                                }

                                break;
                            }
                        case GoldSource.DemoFrameType.DemoBuffer:
                            //var bframe = (GoldSource.DemoBufferFrame)frame.Value;
                             //if (bframe.Buffer.Length > 8)
                            //{
                            //    Console.WriteLine("Demobuf found");
                            //    File.WriteAllBytes("demobuffer/demobuf_" + CurrentFrameId + ".bin", bframe.Buffer);
                            //}
                            break;
                        case GoldSource.DemoFrameType.NetMsg:
                        default:
                            {
                                //if (frame.Key.Type != GoldSource.DemoFrameType.NetMsg)
                                //{
                                //    Console.WriteLine("Invalid");
                                //}

                                NeedCheckAttack = true;
                                //Program.IsAttackSkipTimes--;

                                CurrentFrameIdWeapon += 10;
                                row = index + "/" + frame.Key.FrameIndex + " " + "[" +
                                      frame.Key.Time + "s]: " +
                                      "NETMESSAGE";

                                var nf = (GoldSource.NetMsgFrame)frame.Value;

                                if (CurrentTime != 0.0f && !FoundFirstTime)
                                    FoundFirstTime = true;
                                if (IsUserAlive() && nf.RParms.Health <= 0)
                                {
                                    UserAlive = false;
                                    DeathsCoount2++;
                                    if (needsaveframes)
                                        subnode.Text +=
                                            "LocalPlayer " + UserId + " killed[METHOD 2]!\n";
                                }
                                else if (CurrentTime - LastDeathTime < 0.05)
                                {
                                    if (nf.RParms.Health < 0) DeathsCoount2++;
                                }

                                CurrentFrameAlive = IsUserAlive();

                                RealAlive = CurrentFrameAlive && PreviewFrameAlive;

                                bool FoundDublicader = false;
                                // if (nf.RParms.Time != 0.0f && CurrentTime == nf.RParms.Time)

                                if (nf.UCmd.Buttons == oldframe.UCmd.Buttons)
                                {
                                    if (nf.UCmd.Viewangles.X == oldframe.UCmd.Viewangles.X)
                                    {
                                        if (nf.UCmd.Viewangles.Y ==
                                            oldframe.UCmd.Viewangles.Y)
                                        {
                                            if (nf.RParms.Time == oldframe.RParms.Time ||
                                                nf.RParms.Frametime == oldframe.RParms.Frametime)
                                            {
                                                FoundDublicader = true;
                                                FrameDuplicates++;
                                                break;
                                            }
                                        }
                                    }
                                }

                                if (FoundDublicader)
                                {
                                    if (NeedWriteAim)
                                        AimType1FalseDetect = true;
                                    // AttackCheck = 4;

                                    if (SkipNextAttack == 1)
                                        SkipNextAttack = 3;
                                    else
                                        SkipNextAttack = 2;
                                }
                                else
                                {
                                    if (SkipNextAttack == 2) SkipNextAttack = 0;
                                }



                                CurrentTime3 = frame.Key.Time;


                                CurrentTime = nf.RParms.Time;
                                if ((nf.UCmd.Buttons & 1) > 0)
                                    CurrentFrameAttacked = true;
                                else
                                    CurrentFrameAttacked = false;

                                NewAttack = false;

                                if (!PreviewFrameAttacked && CurrentFrameAttacked)
                                {
                                    FirstAttack = true;
                                    NewAttack = true;
                                    attackscounter3++;
                                }

                                if (NewAttack)
                                {
                                    if (AimType7Event == 53)
                                        AimType7Event = 3;
                                    if (AimType7Event == 52)
                                        AimType7Event = 2;
                                }
                                else
                                {
                                    if (AimType7Event == 53)
                                        AimType7Event = 0;
                                    if (AimType7Event == 52)
                                        AimType7Event = 0;
                                }

                                if ((nf.UCmd.Buttons & 2) > 0)
                                    CurrentFrameJumped = true;
                                else
                                    CurrentFrameJumped = false;

                                if (!PreviewFrameJumped && CurrentFrameJumped) JumpCount2++;

                                //if (FirstAttack && CurrentTime == IsAttackLastTime)
                                //{
                                //    Program.SearchAim6 = true;
                                //}
                                //else
                                //{
                                //    if (Program.SearchAim6 )
                                //    {

                                //    }
                                //    Program.SearchAim6 = false;
                                //}


                                if ((nf.UCmd.Buttons & 4) > 0)
                                    CurrentFrameDuck = true;
                                else
                                    CurrentFrameDuck = false;

                                if (!FirstDuck && CurrentFrameDuck)
                                {
                                    FirstDuck = true;
                                    IsDuck = true;
                                }
                                else if (!FirstDuck && !CurrentFrameDuck)
                                {
                                    FirstDuck = true;
                                    IsDuck = false;
                                }
                                else if (FirstDuck)
                                {
                                    if (CurrentTime - LastUnDuckTime > 0.5 &&
                                        CurrentTime - LastDuckTime > 0.5)
                                    {
                                        if (CurrentFrameDuck && !IsDuck)
                                        {
                                        }
                                        else if (!CurrentFrameDuck && IsDuck)
                                        {
                                            if (CurrentTime - LastJumpHackTime > 10.0)
                                            {
                                                if (CurrentTime - LastJumpHackTime > 5.0)
                                                {
                                                    AddViewDemoHelperComment(
                                                        "Detected [DUCKHACK] duck.", 1.00f);
                                                    TextComments.WriteLine(
                                                        "Detected [DUCKHACK] duck on (" +
                                                        CurrentTime + ") " + CurrentTimeString);
                                                    Console.WriteLine(
                                                        "Detected [DUCKHACK] duck on (" +
                                                        CurrentTime + ") " + CurrentTimeString);
                                                }

                                                LastJumpHackTime = CurrentTime;
                                            }

                                            JumpHackCount++;
                                        }
                                    }
                                }

                                if (nf.RParms.Onground != 0)
                                    CurrentFrameOnGround = true;
                                else
                                    CurrentFrameOnGround = false;

                                if (!PreviewFrameOnGround && CurrentFrameOnGround) JumpCount3++;

                                if (!PreviewFrameOnGround && CurrentFrameOnGround &&
                                    RealAlive
                                    && PreviewFramePunchangleZ == 0.0f &&
                                    CurrentFramePunchangleZ > 2.0f)
                                    NeedDetectBHOPHack = true;



                                if (AimType7Event != 0)
                                {
                                    if (Aim7PunchangleY == 0.0f && nf.RParms.Punchangle.Y != 0.0f)
                                    {
                                        Aim7PunchangleY = nf.RParms.Punchangle.Y;
                                    }
                                }
                                else
                                {
                                    Aim7PunchangleY = 0.0f;
                                }

                                CurrentFrameLerp = nf.UCmd.LerpMsec;

                                if (!FakeLagsValus.Contains(CurrentFrameLerp))
                                    FakeLagsValus.Add(CurrentFrameLerp);
                                CurrentFramePunchangleZ = nf.RParms.Punchangle.Z;

                                Program.CurrentTimeString = "MODIFIED";
                                try
                                {
                                    var t = TimeSpan.FromSeconds(CurrentTime);

                                    Program.CurrentTimeString = string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
                                        t.Hours,
                                        t.Minutes,
                                        t.Seconds,
                                        t.Milliseconds);
                                    lastnormalanswer = Program.CurrentTimeString;

                                    Console.Title =
                                        "[ANTICHEAT/ANTIHACK] Unreal Demo Scanner v1.33b2. Demo:" +
                                        DemoName + ". DEMO TIME: " + Program.CurrentTimeString;
                                }
                                catch
                                {
                                    try
                                    {
                                        Console.Title =
                                            "[ANTICHEAT/ANTIHACK] Unreal Demo Scanner v1.33b2. Demo:" +
                                            DemoName + ". DEMO TIME: " + lastnormalanswer;
                                    }
                                    catch
                                    {
                                        try
                                        {
                                            Console.Title =
                                                "[ANTICHEAT/ANTIHACK] Unreal Demo Scanner v1.33b2. Demo:" +
                                                "BAD NAME" + ". DEMO TIME: " + lastnormalanswer;
                                        }
                                        catch
                                        {

                                        }
                                    }
                                }

                                if (RealAlive && CurrentFrameAttacked)
                                    if (cdframeFov > 90)
                                        if (!FovHackDetected)
                                        {
                                            TextComments.WriteLine(
                                                "Detected [FOV HACK] on (" + CurrentTime +
                                                "):" + Program.CurrentTimeString);
                                            AddViewDemoHelperComment(
                                                "Detected [FOV HACK]. Weapon:" + CurrentWeapon);
                                            Console.WriteLine(
                                                "Detected [FOV HACK] on (" + CurrentTime +
                                                "):" + Program.CurrentTimeString);
                                            if (cdframeFov == ClientFov)
                                                Console.Write("[BY SERVER ???]");
                                            FovHackDetected = true;
                                        }

                                if (RealAlive && CurrentFrameAttacked)
                                    if (GetDistance(new Point(nf.View.X, nf.View.Y),
                                        new Point(nf.RParms.Vieworg.X,
                                            nf.RParms.Vieworg.Y)) > 50)
                                        if (!ThirdHackDetected && CurrentWeapon !=
                                                               WeaponIdType.WEAPON_NONE
                                                               && CurrentWeapon !=
                                                               WeaponIdType.WEAPON_BAD &&
                                                               CurrentWeapon !=
                                                               WeaponIdType.WEAPON_BAD2)
                                        {
                                            TextComments.WriteLine(
                                                "Detected [THIRD PERSON HACK] on (" +
                                                CurrentTime + "):" + Program.CurrentTimeString);
                                            AddViewDemoHelperComment(
                                                "Detected [THIRD PERSON HACK]. Weapon:" +
                                                CurrentWeapon);
                                            Console.WriteLine(
                                                "Detected [THIRD PERSON HACK] on (" +
                                                CurrentTime + "):" + Program.CurrentTimeString);
                                            ThirdHackDetected = true;
                                        }

                                if (CurrentTime2 != 0.0)
                                {
                                    if (LastFpsCheckTime2 == 0.0)
                                    {
                                        LastFpsCheckTime2 = CurrentTime2;
                                    }
                                    else
                                    {
                                        if ((CurrentTimeDesync2 =
                                                Math.Abs(CurrentTime2 - LastFpsCheckTime2)) >
                                            1.0)
                                        {
                                            if (needsaveframes)
                                                subnode.Text =
                                                    "CurrentFps2:" + CurrentFps2 + "\n";

                                            if (CurrentFps2 > RealFpsMax2)
                                                RealFpsMax2 = CurrentFps2;

                                            if (CurrentFps2 < RealFpsMin2 && CurrentFps2 > 0)
                                                RealFpsMin2 = CurrentFps2;

                                            LastFpsCheckTime2 = CurrentTime2;
                                            SecondFound2 = true;
                                            CurrentFps2 = 0;
                                        }
                                        else
                                        {
                                            CurrentFps2++;
                                        }
                                    }
                                }


                                if (RealAlive)
                                {
                                    if (CurrentFrameAttacked || PreviewFrameAttacked)
                                    {
                                        if (CurrentTime - AimType8WarnTime < 0.250f)
                                        {
                                            var tmpcol = Console.ForegroundColor;
                                            Console.ForegroundColor = ConsoleColor.Gray;
                                            TextComments.WriteLine(
                                                "WARN [AIM TYPE 8.1] on (" + AimType8WarnTime +
                                                "):" + Program.CurrentTimeString + " (???)");
                                            AddViewDemoHelperComment(
                                                "WARN [AIM TYPE 8.1] ???. Weapon:" +
                                                CurrentWeapon, 0.75f);
                                            Console.WriteLine(
                                                "WARN [AIM TYPE 8.1] on (" + AimType8WarnTime +
                                                "):" + Program.CurrentTimeString + " (???)");
                                            Console.ForegroundColor = tmpcol;
                                            AimType8WarnTime = 0.0f;
                                        }
                                        else if (CurrentTime - AimType8WarnTime2 < 0.250f)
                                        {
                                            var tmpcol = Console.ForegroundColor;
                                            Console.ForegroundColor = ConsoleColor.Gray;
                                            TextComments.WriteLine(
                                                "WARN [AIM TYPE 8.2] on (" + AimType8WarnTime2 +
                                                "):" + Program.CurrentTimeString + " (???)");
                                            AddViewDemoHelperComment(
                                                "WARN [AIM TYPE 8.2] ???. Weapon:" +
                                                CurrentWeapon, 0.75f);
                                            Console.WriteLine(
                                                "WARN [AIM TYPE 8.2] on (" + AimType8WarnTime2 +
                                                "):" + Program.CurrentTimeString + " (???)");
                                            Console.ForegroundColor = tmpcol;
                                            AimType8WarnTime2 = 0.0f;
                                        }
                                    }

                                    //if (CurrentFrameIdAll == NeedSearchID + 1)
                                    {
                                        if (viewanglesforsearch.X != nf.RParms.Viewangles.X)
                                        {
                                            if (CurrentFrameAttacked && CurrentFrameOnGround)
                                            {
                                                if (Math.Round(Math.Abs(Math.Abs(viewanglesforsearch.X - nf.RParms.Viewangles.X) - Math.Abs(nf.RParms.Punchangle.X)), 8, MidpointRounding.AwayFromZero) > nospreadtest)
                                                {
                                                    nospreadtest = Math.Round(Math.Abs(Math.Abs(viewanglesforsearch.X - nf.RParms.Viewangles.X) - Math.Abs(nf.RParms.Punchangle.X)), 8, MidpointRounding.AwayFromZero);
                                                    //Console.WriteLine(nospreadtest.ToString("F8"));
                                                }
                                            }

                                            //if (CurrentFrameAttacked)
                                            //    ;// Console.WriteLine("ATTACKED");
                                            //else if (CurrentFrameDuck)
                                            //    ;// Console.WriteLine("DUCKED");
                                            //else if (CurrentFrameJumped)
                                            //    ;// Console.WriteLine("JUMPED");
                                            //else if (PreviewFrameAttacked)
                                            //    ;// Console.WriteLine("PREVATT");
                                            //else if (!CurrentFrameOnGround)
                                            //    ;// Console.WriteLine("NOGROUND");
                                            //else if (CurrentTime - IsAttackLastTime < 3.0f || IsAttack)
                                            //    ;// Console.WriteLine("LASTATTACK");
                                            //else
                                            //{
                                            //    Console.WriteLine("NONE(" + nf.UCmd.Buttons + ")");
                                            //}
                                        }
                                        else if (viewanglesforsearch.Y != nf.RParms.Viewangles.Y)
                                        {
                                            if (CurrentFrameAttacked && CurrentFrameOnGround)
                                            {
                                                if (Math.Round(Math.Abs(Math.Abs(viewanglesforsearch.X - nf.RParms.Viewangles.X) - Math.Abs(nf.RParms.Punchangle.X)), 8, MidpointRounding.AwayFromZero) > nospreadtest2)
                                                {
                                                    nospreadtest2 = Math.Round(Math.Abs(Math.Abs(viewanglesforsearch.X - nf.RParms.Viewangles.X) - Math.Abs(nf.RParms.Punchangle.X)), 8, MidpointRounding.AwayFromZero);
                                                    //Console.WriteLine(nospreadtest.ToString("F8"));
                                                }
                                            }

                                            //if (CurrentFrameAttacked)
                                            //    ;// Console.WriteLine("ATTACKED");
                                            //else if (CurrentFrameDuck)
                                            //    ;// Console.WriteLine("DUCKED");
                                            //else if (CurrentFrameJumped)
                                            //    ;// Console.WriteLine("JUMPED");
                                            //else if (PreviewFrameAttacked)
                                            //    ;// Console.WriteLine("PREVATT");
                                            //else if (!CurrentFrameOnGround)
                                            //    ;// Console.WriteLine("NOGROUND");
                                            //else if (CurrentTime - IsAttackLastTime < 3.0f || IsAttack)
                                            //    ;// Console.WriteLine("LASTATTACK");
                                            //else
                                            //{
                                            //    Console.WriteLine("NONE(" + nf.UCmd.Buttons + ")");
                                            //}
                                        }


                                        if (CurrentWeapon == WeaponIdType.WEAPON_C4
                                               || CurrentWeapon == WeaponIdType.WEAPON_HEGRENADE
                                               || CurrentWeapon == WeaponIdType.WEAPON_SMOKEGRENADE
                                               || CurrentWeapon == WeaponIdType.WEAPON_FLASHBANG
                                               || CurrentWeapon == WeaponIdType.WEAPON_NONE
                                               || CurrentWeapon == WeaponIdType.WEAPON_BAD
                                               || CurrentWeapon == WeaponIdType.WEAPON_BAD2 || !CurrentFrameOnGround)
                                        {
                                            AimType8Warn = 0;
                                        }
                                        else if (viewanglesforsearch != nf.RParms.ClViewangles)
                                        {
                                            AimType8Warn++;
                                            if (AimType8Warn > 0)
                                            {
                                                AimType8WarnTime = CurrentTime;
                                                AimType8Warn = 0;
                                            }
                                        }
                                        else if (viewanglesforsearch != nf.UCmd.Viewangles)
                                        {
                                            AimType8Warn++;
                                            if (AimType8Warn > 0)
                                            {
                                                AimType8WarnTime2 = CurrentTime;
                                                AimType8Warn = 0;
                                            }
                                        }
                                        else
                                        {
                                            AimType8Warn = 0;
                                        }
                                    }
                                }
                                else
                                {
                                    AimType8Warn = 0;
                                }


                                if (LastTimeDesync == 0.0f)
                                {
                                    LastTimeDesync = Math.Abs(CurrentTime - CurrentTime2);
                                }
                                else
                                {
                                    if (Math.Abs(LastTimeDesync -
                                                 Math.Abs(CurrentTime - CurrentTime2)) > 0.08)
                                    {
                                        if (RealAlive)
                                        {
                                            //Console.WriteLine("LastTimeDesync1:" + Math.Abs(LastTimeDesync - Math.Abs(CurrentTime - CurrentTime2)).ToString());
                                            if (SecondFound && CurrentTime != 0.0 &&
                                                CurrentTime2 != 0.0)
                                            {
                                                if (LastTimeOut == 1)
                                                    //if (LastTimeOutCount > 1)
                                                    //{
                                                    //    Console.WriteLine("Отладочная инфа: Обнаружена машина времени.");
                                                    //}
                                                    LastTimeOutCount++;
                                                LastTimeOut = 1;
                                                //Console.WriteLine("Second.");
                                            }

                                            if (SecondFound2 && CurrentTime != 0.0 &&
                                                CurrentTime2 != 0.0)
                                            {
                                                if (LastTimeOut == 2)
                                                    //if (LastTimeOutCount > 1)
                                                    //{
                                                    //    Console.WriteLine("Отладочная инфа: Обнаружена машина времени v2.0.");
                                                    //}
                                                    LastTimeOutCount++;
                                                LastTimeOut = 2;
                                                //Console.WriteLine("Second2.");
                                            }
                                        }

                                        LastTimeDesync = Math.Abs(CurrentTime - CurrentTime2);
                                    }
                                    else
                                    {
                                        LastTimeDesync = Math.Abs(CurrentTime - CurrentTime2);
                                    }
                                }

                                //if (NeedDetectBHOPHack)
                                //{
                                //    NeedDetectBHOPHack = false;
                                //    if (!CurrentFrameOnGround && CurrentFramePunchangleZ > 2.0f && PreviewFramePunchangleZ > CurrentFramePunchangleZ)
                                //    {
                                //        Console.WriteLine("BHOP");
                                //    }
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
                                                    TextComments.WriteLine(
                                                        "Detected [AIM TYPE 4] on (" +
                                                        CurrentTime + "):" + Program.CurrentTimeString);
                                                    AddViewDemoHelperComment(
                                                        "Detected [AIM TYPE 4]. Weapon:" +
                                                        CurrentWeapon);
                                                    Console.WriteLine(
                                                        "Detected [AIM TYPE 4] on (" +
                                                        CurrentTime + "):" + Program.CurrentTimeString);
                                                    SilentAimDetected++;
                                                }
                                }



                                if (NeedSearchViewAnglesAfterAttack && IsAttack &&
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
                                        //fixyou
                                    }
                                    else if (CurrentFrameAttacked)
                                    {
                                        NeedSearchViewAnglesAfterAttack = false;
                                        ViewanglesXAfterAttack = CurrentFrameViewanglesX;
                                        ViewanglesYAfterAttack = CurrentFrameViewanglesY;
                                        //Console.WriteLine("ViewanglesXAfterAttack:" + ViewanglesXAfterAttack);
                                        //Console.WriteLine("ViewanglesYAfterAttack:" + ViewanglesYAfterAttack);
                                        NeedSearchViewAnglesAfterAttackNext = true;
                                    }
                                }
                                else
                                {
                                    NeedSearchViewAnglesAfterAttack = false;
                                    if (NeedSearchViewAnglesAfterAttackNext && RealAlive &&
                                        IsAttack)
                                    {
                                        {
                                            NeedSearchViewAnglesAfterAttackNext = false;
                                            ViewanglesXAfterAttackNext =
                                                CurrentFrameViewanglesX;
                                            ViewanglesYAfterAttackNext =
                                                CurrentFrameViewanglesY;
                                            if (ViewanglesXBeforeAttack !=
                                                ViewanglesXAfterAttack &&
                                                ViewanglesYBeforeAttack !=
                                                ViewanglesYAfterAttack &&
                                                ViewanglesXBeforeAttack ==
                                                ViewanglesXAfterAttackNext &&
                                                ViewanglesYBeforeAttack ==
                                                ViewanglesYAfterAttackNext
                                            )
                                            {
                                                if (maxfalsepositiveaim3 > 0 &&
                                                    SilentAimDetected <= 1 &&
                                                    JumpHackCount <= 1)
                                                {
                                                    var tmpcol = Console.ForegroundColor;
                                                    Console.ForegroundColor = ConsoleColor.Gray;
                                                    TextComments.WriteLine(
                                                        "Detected [AIM TYPE 3] on (" +
                                                        CurrentTime + "):" + Program.CurrentTimeString + " (???)");
                                                    AddViewDemoHelperComment(
                                                        "Detected [AIM TYPE 3]. Weapon:" +
                                                        CurrentWeapon + " (???)");
                                                    Console.WriteLine(
                                                        "Detected [AIM TYPE 3] on (" +
                                                        CurrentTime + "):" + Program.CurrentTimeString + " (???)");
                                                    Console.ForegroundColor = tmpcol;
                                                    maxfalsepositiveaim3--;
                                                }
                                                else
                                                {
                                                    TextComments.WriteLine(
                                                        "Detected [AIM TYPE 3] on (" +
                                                        CurrentTime + "):" + Program.CurrentTimeString);
                                                    AddViewDemoHelperComment(
                                                        "Detected [AIM TYPE 3]. Weapon:" +
                                                        CurrentWeapon);
                                                    Console.WriteLine(
                                                        "Detected [AIM TYPE 3] on (" +
                                                        CurrentTime + "):" + Program.CurrentTimeString);
                                                    SilentAimDetected++;
                                                }
                                            }
                                            else if (ViewanglesXBeforeAttack !=
                                                     ViewanglesXAfterAttack &&
                                                     ViewanglesYBeforeAttack !=
                                                     ViewanglesYAfterAttack &&
                                                     // ViewanglesXBeforeAttack == ViewanglesXAfterAttackNext &&
                                                     ViewanglesYBeforeAttack ==
                                                     ViewanglesYAfterAttackNext
                                            )
                                            {
                                                //var tmpcol = Console.ForegroundColor;
                                                //Console.ForegroundColor = ConsoleColor.Gray;
                                                //Program.TextComments.WriteLine("Detected [AIM TYPE 3] on (" + CurrentTime + "):" + Program.CurrentTimeString + " (???)");
                                                //AddViewDemoHelperComment("Detected [AIM TYPE 3]. Weapon:" + CurrentWeapon.ToString() + " (???)", 0.5f);
                                                //Console.WriteLine("Detected [AIM TYPE 3] on (" + CurrentTime + "):" + Program.CurrentTimeString + " (???)");
                                                //Console.ForegroundColor = tmpcol;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        NeedSearchViewAnglesAfterAttackNext = false;
                                    }
                                }


                                if (CurrentTime != 0.0f && RealAlive)
                                {
                                    if (IsJump && FirstJump ||
                                        CurrentTime - LastUnJumpTime < 0.5) JumpHackCount2 = 0;

                                    if (!CurrentFrameJumped && !FirstJump)
                                    {
                                        FirstJump = true;
                                        IsJump = false;
                                    }
                                    else
                                    {
                                        if (FirstJump && !IsJump && JumpHackCount2 > 0)
                                        {
                                            JumpHackCount2 = 0;
                                            if (CurrentTime - LastUnJumpTime > 0.5 &&
                                                FirstJump)
                                            {
                                                if (CurrentTime - LastJumpHackTime > 5.0)
                                                {
                                                    AddViewDemoHelperComment(
                                                        "Detected [JUMPHACK] jump.", 1.00f);
                                                    TextComments.WriteLine(
                                                        "Detected [JUMPHACK] jump on (" +
                                                        CurrentTime + ") " + Program.CurrentTimeString);
                                                    Console.WriteLine(
                                                        "Detected [JUMPHACK] jump on (" +
                                                        CurrentTime + ") " + Program.CurrentTimeString);
                                                }

                                                LastJumpHackTime = CurrentTime;
                                                JumpHackCount++;
                                            }
                                        }

                                        if (FirstJump && !IsJump && CurrentFrameJumped)
                                            if (CurrentTime - LastUnJumpTime > 0.5)
                                                JumpHackCount2++;
                                    }
                                }
                                else
                                {
                                    JumpHackCount2 = 0;
                                }

                                if (needsaveframes)
                                {
                                    subnode.Text = "{\n";
                                    subnode.Text +=
                                        @"RParms.Time  = " + nf.RParms.Time + "(" + Program.CurrentTimeString +
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

                                if (nf.RParms.Frametime > 0.0)
                                    averagefps2.Add(1000.0 / (1000.0 * nf.RParms.Frametime));

                                if (CurrentFrameOnGround)
                                    AddJumpToBHOPScan(true);
                                else
                                    AddJumpToBHOPScan(false);

                                if (needsaveframes)
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
                                    UserId = nf.RParms.Playernum + 1;
                                    UserId2 = nf.RParms.Viewentity;
                                }

                                addresolution(nf.RParms.Viewport.Z, nf.RParms.Viewport.W);

                                if (needsaveframes)
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
                                        @"UCmd.Viewangles.X  = " + nf.UCmd.Viewangles.X + "\n";
                                    subnode.Text +=
                                        @"UCmd.Viewangles.Y  = " + nf.UCmd.Viewangles.Y + "\n";
                                    subnode.Text +=
                                        @"UCmd.Viewangles.Z  = " + nf.UCmd.Viewangles.Z + "\n";
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
;
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
                                            if (ReallyAim2 == 2 && SilentAimDetected < 2 &&
                                                BadAttackCount < 2)
                                            {
                                                var tmpcol = Console.ForegroundColor;
                                                Console.ForegroundColor = ConsoleColor.Gray;
                                                TextComments.WriteLine(
                                                    "Detected [AIM TYPE 2] on (" + CurrentTime +
                                                    "):" + Program.CurrentTimeString + " (???)");
                                                AddViewDemoHelperComment(
                                                    "Detected [AIM TYPE 2] ???. Weapon:" +
                                                    CurrentWeapon, 0.75f);
                                                Console.WriteLine(
                                                    "Detected [AIM TYPE 2] on (" + CurrentTime +
                                                    "):" + Program.CurrentTimeString + " (???)");
                                                Console.ForegroundColor = tmpcol;
                                            }
                                            else
                                            {
                                                TextComments.WriteLine(
                                                    "Detected [AIM TYPE 2] on (" + CurrentTime +
                                                    "):" + Program.CurrentTimeString);
                                                AddViewDemoHelperComment(
                                                    "Detected [AIM TYPE 2]. Weapon:" +
                                                    CurrentWeapon);
                                                Console.WriteLine(
                                                    "Detected [AIM TYPE 2] on (" + CurrentTime +
                                                    "):" + Program.CurrentTimeString);
                                                SilentAimDetected++;
                                            }

                                            //FirstAttack = false;
                                        }
                                    }

                                    ReallyAim2 = 0;
                                }


                                if (DisableAttackFromNextFrame > 0)
                                {
                                    DisableAttackFromNextFrame--;
                                    if (DisableAttackFromNextFrame == 0 &&
                                        !CurrentFrameAttacked)
                                        //Console.WriteLine("-attack1");
                                        IsAttack = false;
                                }


                                if (!IsAttack)
                                {
                                    if (!BadAttackFound && CurrentFrameAttacked && FirstAttack)
                                    {
                                        if (CurrentWeapon == WeaponIdType.WEAPON_KNIFE
                                            || CurrentWeapon == WeaponIdType.WEAPON_C4
                                            || CurrentWeapon == WeaponIdType.WEAPON_HEGRENADE
                                            || CurrentWeapon == WeaponIdType.WEAPON_SMOKEGRENADE
                                            || CurrentWeapon == WeaponIdType.WEAPON_FLASHBANG)
                                        {
                                            //  Console.WriteLine("Invalid weapon(" + Program.CurrentTime + ") " + Program.CurrentTimeString);
                                        }
                                        else if (SelectSlot > 0)
                                        {
                                            //  Console.WriteLine("Select weapon(" + Program.CurrentTime + ") " + Program.CurrentTimeString);
                                        }
                                        else if (CurrentTime2 - IsNoAttackLastTime2 < 0.1)
                                        {
                                            //   Console.WriteLine("No attack in current frame(" + Program.CurrentTime + ") " + Program.CurrentTimeString);
                                        }
                                        else if (IsReload)
                                        {
                                            //  Console.WriteLine("Reload weapon(" + Program.CurrentTime + ") " + Program.CurrentTimeString);
                                        }
                                        else if (!RealAlive)
                                        {
                                            //  Console.WriteLine("Dead (" + Program.CurrentTime + ") " + Program.CurrentTimeString);
                                        }
                                        else
                                        {
                                            TextComments.WriteLine(
                                                "Detected [TRIGGER BOT] on (" + CurrentTime +
                                                ") " + Program.CurrentTimeString);
                                            AddViewDemoHelperComment(
                                                "Detected [TRIGGER BOT]. Weapon:" +
                                                CurrentWeapon);
                                            Console.WriteLine(
                                                "Detected [TRIGGER BOT] on (" + CurrentTime +
                                                ") " + Program.CurrentTimeString);

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
                                    else if (Aim2AttackDetected)
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
                                            if (SilentAimDetected > 0 || JumpHackCount > 0)
                                            {
                                                TextComments.WriteLine(
                                                    "Detected [AIM TYPE 1] on (" + CurrentTime +
                                                    "):" + Program.CurrentTimeString);
                                                AddViewDemoHelperComment(
                                                    "Detected [AIM TYPE 1]. Weapon:" +
                                                    CurrentWeapon, 0.75f);
                                                Console.WriteLine(
                                                    "Detected [AIM TYPE 1] on (" + CurrentTime +
                                                    "):" + Program.CurrentTimeString);
                                                SilentAimDetected++;
                                            }
                                            else
                                            {
                                                var tmpcol = Console.ForegroundColor;
                                                Console.ForegroundColor = ConsoleColor.Gray;
                                                TextComments.WriteLine(
                                                    "Detected [AIM TYPE 1] on (" + CurrentTime +
                                                    "):" + Program.CurrentTimeString + " (???)");
                                                AddViewDemoHelperComment(
                                                    "Detected [AIM TYPE 1] ???. Weapon:" +
                                                    CurrentWeapon, 0.75f);
                                                Console.WriteLine(
                                                    "Detected [AIM TYPE 1] on (" + CurrentTime +
                                                    "):" + Program.CurrentTimeString + " (???)");
                                                Console.ForegroundColor = tmpcol;
                                            }

                                            // FirstAttack = false;
                                        }
                                    }
                                    else
                                    {
                                        if (FirstAttack)
                                        {
                                            TextComments.WriteLine(
                                                "Detected [AIM TYPE 1] on (" + CurrentTime +
                                                "):" + Program.CurrentTimeString);
                                            AddViewDemoHelperComment(
                                                "Detected [AIM TYPE 1]. Weapon:" +
                                                CurrentWeapon);
                                            Console.WriteLine(
                                                "Detected [AIM TYPE 1] on (" + CurrentTime +
                                                "):" + Program.CurrentTimeString);
                                            SilentAimDetected++;
                                            //FirstAttack = false;
                                        }
                                    }

                                    LastSilentAim = CurrentTime;
                                }
                                //File.AppendAllText("bug.log",CurrentFrameAttacked.ToString() + "\n");

                                if (AttackCheck > -1)
                                {
                                    if (!CurrentFrameAttacked && !IsReload && SelectSlot <= 0 &&
                                        IsAttack && RealAlive &&
                                        CurrentWeapon != WeaponIdType.WEAPON_KNIFE
                                        && CurrentWeapon != WeaponIdType.WEAPON_C4 &&
                                        CurrentWeapon != WeaponIdType.WEAPON_HEGRENADE &&
                                        CurrentWeapon != WeaponIdType.WEAPON_SMOKEGRENADE
                                        && CurrentWeapon != WeaponIdType.WEAPON_FLASHBANG)
                                    {
                                        //Console.WriteLine("Check attack:" + AttackCheck + " " + SkipNextAttack + " " + nf.UCmd.Buttons + " " + CurrentTime);
                                        if (AttackCheck > 0)
                                        {
                                            AttackCheck--;
                                        }
                                        else
                                        {
                                            if (SkipNextAttack <= 0)
                                                //File.AppendAllText("bug.log", "NeedWriteAim" + CurrentTime + " " + IsAttackLastTime + "\n");
                                                // Console.WriteLine("Aim detected.......");
                                                NeedWriteAim = true;
                                            SkipNextAttack = 0;
                                            AttackCheck--;
                                        }
                                    }
                                    else
                                    {
                                        AttackCheck = -1;
                                        //    if (AttackCheck != -2)
                                        //    {
                                        //        //if (Program.SelectSlot > 0)
                                        //        //    Console.WriteLine("select slot...");
                                        //        //if (!Program.IsAttack)
                                        //        //    Console.WriteLine("not in attack...");
                                        //        //if (!Program.UserAlive)
                                        //        //    Console.WriteLine("user invalid blya...");
                                        //        //if (IsReload)
                                        //        //    Console.WriteLine("in reload...");
                                        //        //if ((nf.UCmd.Buttons & 1) > 0)
                                        //        //    Console.WriteLine("not attack...");
                                        //        //Console.WriteLine("Weapon:" + CurrentWeapon.ToString());
                                        //        //Console.WriteLine("Don't check attack!");
                                        //        AttackCheck = -2;
                                        //    }
                                    }
                                }

                                if (needsaveframes)
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

                                if (RealAlive && CurrentFrameOnGround)
                                {
                                    onground_and_alive_tests++;

                                    if (onground_and_alive_tests > 5 && CurrentTime != 0.0f)
                                        if (GetDistance(oldoriginpos,
                                            new Point(nf.RParms.Vieworg.X,
                                                nf.RParms.Vieworg.Y)) > 500)
                                        {
                                            // MessageBox.Show("SPEEDHOOK");
                                            speedhackdetects++;
                                            speedhackdetect_time = CurrentTime;
                                            if (needsaveframes)
                                                subnode.Text +=
                                                    @"speedhaaak " + CurrentTime + "\n";
                                        }

                                    oldoriginpos.X = nf.RParms.Vieworg.X;
                                    oldoriginpos.Y = nf.RParms.Vieworg.Y;
                                }
                                else
                                {
                                    onground_and_alive_tests = 0;
                                }

                                var outstr = subnode.Text;

                                ParseGameData(nf.MsgBytes, ref outstr);

                                if (needsaveframes)
                                {
                                    subnode.Text = outstr;

                                    subnode.Text += 1 / nf.RParms.Frametime + @" FPS";


                                    subnode.Text += "}\n";
                                }

                                PreviewFrameAlive = CurrentFrameAlive;
                                PreviewFrameAttacked = CurrentFrameAttacked;
                                PreviewFrameJumped = CurrentFrameJumped;
                                PreviewFrameDuck = CurrentFrameDuck;
                                PreviewFrameOnGround = CurrentFrameOnGround;
                                PreviewFramePunchangleZ = CurrentFramePunchangleZ;

                                SecondFound = false;
                                SecondFound2 = false;

                                oldframe = nf;
                                FrameCrash = 0;
                                break;
                            }
                    }

                    node.Text = row;
                    if (subnode?.Text?.Length > 0) node.Nodes.Add(subnode);

                    entrynode.Nodes.Add(node);
                }

                PrintNodesRecursive(entrynode);
            }

            BHOPANALYZERMUTE();

            try
            {
                if (File.Exists("Frames.log"))
                    File.WriteAllLines("Frames.log", outFrames.ToArray());
            }
            catch
            {
            }

            Console.ForegroundColor = ConsoleColor.DarkGreen;

            Console.WriteLine("Unreal Demo Scanner v1.33b2 scan result:");

            //Console.WriteLine(nospreadtest.ToString("F8"));

            Console.ForegroundColor = ConsoleColor.DarkRed;

            BHOPANALYZER();

            //if (AttackErrors > 0)
            //{
            //    TextComments.WriteLine("Detected [UNKNOWN CONFIG] для атаки. Detect count:" + AttackErrors);
            //    Console.WriteLine("Detected [UNKNOWN CONFIG] для атаки. Detect count:" + AttackErrors);
            //    Console.WriteLine("Last using at " + LastAttackHack + " second game time.");
            //}

            if (BadAttackCount > 0)
            {
                TextComments.WriteLine(
                    "Detected [TRIGGERBOT]. Detect count:" + BadAttackCount);
                Console.WriteLine("Detected [TRIGGERBOT]. Detect count:" + BadAttackCount);
                //Console.WriteLine("Last using at " + LastBadAttackCount + " second game time.");
            }

            if (SilentAimDetected > 0)
            {
                TextComments.WriteLine("Detected [AIM]. Detect count:" + SilentAimDetected);
                Console.WriteLine("Detected [AIM]. Detect count:" + SilentAimDetected);
                //Console.WriteLine("Last using at " + LastSilentAim + " second game time.");
            }

            if (FakeLagAim > 0)
            {
                TextComments.WriteLine("Detected [FAKELAG]. Detect count:" + FakeLagAim);
                Console.WriteLine("Detected [FAKELAG]. Detect count:" + FakeLagAim);
            }

            if (JumpHackCount > 0)
            {
                TextComments.WriteLine(
                    "Detected [JUMPHACK]. Detect count:" +
                    JumpHackCount /*+ ". Found " + JumpCount + " +jump commands"*/);
                Console.WriteLine(
                    "Detected [JUMPHACK]. Detect count:" +
                    JumpHackCount /*+ ". Found " + JumpCount + " +jump commands*/);
            }


            //if (FrameDuplicates > 0)
            //{
            //    Console.WriteLine("Дальше инфа только для отладки. Не обращайте на нее внимания: ");
            //    Console.WriteLine("1. " + FrameDuplicates + " кадров повторяются");
            //    Console.WriteLine("2. " + AttackErrors + " подозрительных выстрелов(бинды?)");
            //    Console.WriteLine("3. Конец");
            //}


            //if (speedhackdetects > 0)
            //{
            //    Console.WriteLine("Возможно игрок использует SPEEDHACK. Detect count:" + speedhackdetects);
            //    Console.WriteLine("Последнее время использования " + speedhackdetect_time + " секунда игрового времени.");
            //}

            if (hackList.Count > 0)
            {
                Console.WriteLine("Bind/alias from blacklist detected:");
                TextComments.WriteLine("Bind/alias from blacklist detected:");
                foreach (var chet in hackList)
                {
                    TextComments.WriteLine(chet);
                    Console.WriteLine(chet);
                }
            }


            var maxfps1 =
                Math.Round(
                    1000.0 / (1000.0f * CurrentDemoFile.GsDemoInfo.AditionalStats
                        .FrametimeMin), 5);
            var maxfps2 =
                Math.Round(1000.0f / CurrentDemoFile.GsDemoInfo.AditionalStats.MsecMin, 5);
            try
            {
                if (maxfps1 == 1000.0f && maxfps2 == 1000.0f &&
                    maxfps1 / averagefps2.Average() > 4.0f)
                {
                    Console.WriteLine("[BHOP/JUMP] Обнаружен неизвестный хак. (???)");
                    Console.WriteLine("[BHOP/JUMP] Unknown hack detected. (???)");
                }
            }
            catch
            {
            }
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

            if (LastTimeOutCount > 2)
                Console.WriteLine(
                    "[DEBUG/TRACE MODE] Обнаружены незначительные искривления пространства и времени. Количество: " +
                    LastTimeOutCount);

            Console.ForegroundColor = ConsoleColor.DarkGreen;

            ViewDemoHelperComments.Seek(4, SeekOrigin.Begin);
            ViewDemoHelperComments.Write(ViewDemoCommentCount);
            ViewDemoHelperComments.Seek(0, SeekOrigin.Begin);


            Console.WriteLine("Scan completed. Choose next actions:");

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                var table = new ConsoleTable("Save CDB", "Save TXT", "Demo info",
                    "Player info", "Wav Player", "Sens History", "About", "EXIT");
                table.AddRow("1", "2", "3", "4", "5", "6", "7", "0/Enter");
                table.Write(Format.Alternative);

                var command = Console.ReadLine();

                if (command.Length == 0 || command == "0") return;
                if (command == "7")
                {
                    Console.WriteLine(" ");
                    Console.WriteLine(" ");
                    Console.WriteLine(" ");
                    Console.WriteLine(
                        " * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * ");
                    Console.WriteLine(" * Developer: Karaulov");
                    Console.WriteLine(
                        " * Unreal Demo Scanner tool for search hack patterns in demo file.");
                    Console.WriteLine(
                        " * Report false positive: (Сообщить о ложном срабатывании):");
                    Console.WriteLine(
                        " * https://c-s.net.ua/forum/topic90973.html , https://goldsrc.ru/threads/4627/");
                    Console.WriteLine(
                        " * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * ");
                    Console.WriteLine(" ");
                    Console.WriteLine(" ");
                    Console.WriteLine(" ");
                }

                if (command == "6")
                {
                    table = new ConsoleTable("Second", "Sensitivity", "Time", "Weapon");
                    for (var i = 0; i < PlayerSensitivityHistoryStr.Count; i++)
                        table.AddRow(i + 1, PlayerSensitivityHistoryStr[i],
                            PlayerSensitivityHistoryStrTime[i]
                            , PlayerSensitivityHistoryStrWeapon[i]);
                    table.Write(Format.Alternative);
                    try
                    {
                        File.WriteAllText("SensHistory.log", table.ToStringAlternative());
                    }
                    catch
                    {
                    }
                }

                if (command == "5")
                {
                    if (!File.Exists(@"wav\decoder.exe"))
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;

                        Console.WriteLine(
                            "No decoder found at path " + Directory.GetCurrentDirectory() +
                            @"wav\decoder.exe");

                        continue;
                    }

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    try
                    {
                        try
                        {
                            if (Directory.Exists("out")) Directory.Delete("out", true);
                        }
                        catch
                        {
                        }

                        if (!Directory.Exists("out")) Directory.CreateDirectory("out");

                        fullPlayerList.AddRange(playerList);

                        foreach (var player in fullPlayerList)
                        {
                            if (player.Name.Length <= 0) continue;

                            if (player.voicedata_stream.Length > 0)
                            {
                                try
                                {
                                    File.Delete(@"wav\input.wav.enc");
                                }
                                catch
                                {
                                }

                                try
                                {
                                    File.Delete(@"wav\output.speex.wav");
                                }
                                catch
                                {
                                }

                                try
                                {
                                    File.Delete(@"wav\output.silk.wav");
                                }
                                catch
                                {
                                }

                                try
                                {
                                    File.Delete(@"wav\output.opus.wav");
                                }
                                catch
                                {
                                }

                                var binaryReader =
                                    new BinaryReader(player.voicedata_stream);
                                player.voicedata_stream.Seek(0, SeekOrigin.Begin);
                                var data2 = binaryReader.ReadBytes(
                                    (int)player.voicedata_stream.Length);
                                File.WriteAllBytes(@"wav\input.wav.enc", data2);
                                var process = new Process();
                                process.StartInfo.FileName = @"wav\decoder.exe";
                                process.Start();
                                process.WaitForExit();

                                var filename = player.Name + "(" + player.Id + ").wav";

                                foreach (var c in Path.GetInvalidFileNameChars())
                                    filename = filename.Replace(c, '_');

                                var fileexistcount = 1;

                                while (File.Exists(@"out\" + filename) ||
                                       File.Exists(@"out\1_" + filename) ||
                                       File.Exists(@"out\2_" + filename))
                                {
                                    filename = player.Name + "(" + player.Id + ") (" +
                                               fileexistcount + ").wav";
                                    foreach (var c in Path.GetInvalidFileNameChars())
                                        filename = filename.Replace(c, '_');

                                    fileexistcount++;
                                }

                                if (File.Exists(@"wav\output.opus.wav"))
                                    File.Move(@"wav\output.opus.wav", @"out\2_" + filename);
                                else if (File.Exists(@"wav\output.silk.wav"))
                                    File.Move(@"wav\output.silk.wav", @"out\1_" + filename);
                                else if (File.Exists(@"wav\output.speex.wav"))
                                    File.Move(@"wav\output.speex.wav", @"out\" + filename);
                            }
                        }

                        Console.WriteLine("Success players voice decode!");
                        Process.Start(Directory.GetCurrentDirectory() + @"\out\");
                    }
                    catch
                    {
                        Console.WriteLine("Player voice decode error!");
                    }
                }

                if (command == "4")
                    try
                    {
                        File.Delete("players.txt");
                        File.AppendAllText("players.txt", "Current players:\n");

                        foreach (var player in playerList)
                            if (player.Name.Length > 0)
                            {
                                table = new ConsoleTable(
                                    "Player:" + player.Name + "(" + player.Id + ")");
                                foreach (var keys in player.InfoKeys)
                                    table.AddRow(keys.Key + " = " + keys.Value);
                                File.AppendAllText("players.txt",
                                    table.ToStringAlternative());
                            }

                        File.AppendAllText("players.txt", "Old players:\n");
                        foreach (var player in fullPlayerList)
                            if (player.Name.Length > 0)
                            {
                                table = new ConsoleTable(
                                    "Player:" + player.Name + "(" + player.Id + ")");
                                foreach (var keys in player.InfoKeys)
                                    table.AddRow(keys.Key + " = " + keys.Value);
                                File.AppendAllText("players.txt",
                                    table.ToStringAlternative());
                            }


                        Console.WriteLine("All players saved to players.txt");
                        Process.Start("players.txt");
                    }
                    catch
                    {
                        Console.WriteLine("Error while saving players to txt!");
                    }


                if (command == "1")
                {
                    if (ViewDemoCommentCount > 0)
                        try
                        {
                            var binaryReader =
                                new BinaryReader(ViewDemoHelperComments.BaseStream);
                            ViewDemoHelperComments.BaseStream.Seek(0, SeekOrigin.Begin);
                            var comments =
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
                    else
                        Console.WriteLine("No View Demo Helper comments found.");
                }

                if (command == "2")
                {
                    if (TextCommentsStream.Length > 0)
                        try
                        {
                            var binaryReader = new BinaryReader(TextCommentsStream);
                            TextCommentsStream.Seek(0, SeekOrigin.Begin);
                            var comments =
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
                        .AddRow(2, JumpCount4, attackscounter2)
                        .AddRow(3, JumpCount2, attackscounter3)
                        .AddRow(4, JumpCount3, attackscounter4)
                        .AddRow("-", " -- ", Reloads).AddRow("-", " -- ", Reloads2);


                    table.Write(Format.Alternative);

                    table = new ConsoleTable("СМЕРТЕЙ/DEATHS", "(2) СМЕРТЕЙ / DEATHS",
                        "УБИЙСТВ /KILLS");

                    table.AddRow(DeathsCoount, DeathsCoount2, KillsCount);

                    table.Write(Format.Alternative);

                    Console.WriteLine("Calculated FPS / Подсчитанный FPS");
                    table = new ConsoleTable("Максимальный FPS / FPS MAX",
                        "Минимальная зареджка/Min Delay", "Средний FPS/Average FPS");
                    table.AddRow(
                        Math.Round(CurrentDemoFile.GsDemoInfo.AditionalStats.FrametimeMin,
                            5) + "(" + Math.Round(
                            1000.0 / (1000.0f * CurrentDemoFile.GsDemoInfo.AditionalStats
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
                            1000.0 / (1000.0f * CurrentDemoFile.GsDemoInfo.AditionalStats
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
                    }

                    if (playerresolution.Count > 0)
                    {
                        table = new ConsoleTable("Screen resolution");
                        foreach (var s in playerresolution) table.AddRow(s.x + "x" + s.y);
                        table.Write(Format.Alternative);
                    }

                    if (invalidframes > 0)
                    {
                        Console.WriteLine("Unreadable frames: " + invalidframes);
                    }

                    if (FrameDuplicates > 0)
                    {
                        Console.WriteLine("Dublicate frames: " + invalidframes);
                    }

                    if (playerList.Count > 0)
                        Console.WriteLine("Players: " + playerList.Count);
                    try
                    {
                        //if ((new FileInfo(Assembly.GetExecutingAssembly().Location).Length <
                        //        1000000 || playerList.Count > 4) &&
                        if (CheatKey > 0)
                            Console.WriteLine(
                                "Press cheat key " + CheatKey + " times. (???)");
                    }
                    catch
                    {
                    }
                }
            }
        }

        private static double normalizeangle(double angle)
        {
            if (angle > 360.0 || angle < -360.0)
                angle = angle % 360.0;
            if (angle < 0.0)
                angle += 360.0;
            if (angle > 360.0)
                angle -= 360.0;
            return angle;
        }

        private static double AngleBetween(double angle1, double angle2)
        {
            double angle = normalizeangle(normalizeangle(angle1) - normalizeangle(angle2));
            if (360.0 - angle > angle)
                return angle;
            else
                return 360.0 - angle;
        }

        private static void ParseGameData(byte[] msgBytes, ref string s)
        {
            halfLifeDemoParser.ParseGameDataMessages(msgBytes, ref s);
        }

        public struct WindowResolution
        {
            public int x, y;
        }
        public static int cipid = 0;
        public static float LastAim5Detected = 0.0f;
        public static float LastAim5DetectedReal = 0.0f;
        public static bool voicefound = false;
        public static float AimType8WarnTime = 0.0f;
        public static float AimType8WarnTime2 = 0.0f;
        public static int AimType8Warn = 0;

        public static float Aim7PunchangleY = 0.0f;
        public static double nospreadtest2 = 0.0;

        public class Player
        {
            public byte Slot;
            public string Name;
            public int Id;
            public Dictionary<string, string> InfoKeys;
            public BinaryWriter voicedata;
            public Stream voicedata_stream;
            public int pid;
            private int VoiceExploit;


            public void WriteVoice(int len, byte[] data)
            {
                if (len == 0)
                {
                    VoiceExploit += 1;
                    if (VoiceExploit > 10 && !voicefound)
                    {
                        voicefound = true;
                        VoiceExploit = 0;
                        //Console.WriteLine("Player:" + Name + " use voice exploit???");
                    }
                }
                else
                {
                    VoiceExploit = 0;
                    voicedata.Write(len);
                    voicedata.Write(data);
                }
            }

            public Player(byte slot, int id)
            {
                this.pid = cipid++;
                VoiceExploit = 0;
                Name = string.Empty;
                Slot = slot;
                Id = id;
                InfoKeys = new Dictionary<string, string>();
                voicedata_stream = new MemoryStream();
                voicedata = new BinaryWriter(voicedata_stream);
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

        public static void XmlFileSerialize(string fileName, object o, Type type)
        {
            XmlFileSerialize(fileName, o, type, null);
        }

        public static void XmlFileSerialize(string fileName, object o, Type type,
            Type[] extraTypes)
        {
            XmlSerializer serializer;

            if (extraTypes == null)
                serializer = new XmlSerializer(type);
            else
                serializer = new XmlSerializer(type, extraTypes);

            using (TextWriter writer = new StreamWriter(fileName))
            {
                serializer.Serialize(writer, o);
            }
        }

        public static object XmlFileDeserialize(string fileName, Type type)
        {
            return XmlFileDeserialize(fileName, type, null);
        }

        public static object XmlFileDeserialize(string fileName, Type type,
            Type[] extraTypes)
        {
            object result = null;

            using (TextReader stream = new StreamReader(fileName))
            {
                result = XmlFileDeserialize((StreamReader)stream, type, extraTypes);
            }

            return result;
        }

        public static object XmlFileDeserialize(StreamReader stream, Type type,
            Type[] extraTypes)
        {
            XmlSerializer serializer;

            if (extraTypes == null)
                serializer = new XmlSerializer(type);
            else
                serializer = new XmlSerializer(type, extraTypes);

            return serializer.Deserialize(stream);
        }

        public static Process FindProcess(string fileNameWithoutExtension)
        {
            return FindProcess(fileNameWithoutExtension, null);
        }

        public static Process FindProcess(string fileNameWithoutExtension,
            string exeFullPath)
        {
            return FindProcess(fileNameWithoutExtension, exeFullPath, -1);
        }

        public static Process FindProcess(string fileNameWithoutExtension,
            string exeFullPath, int ignoreId)
        {
            var processes = Process.GetProcessesByName(fileNameWithoutExtension);

            if (exeFullPath == null)
            {
                if (processes.Length > 0) return processes[0];

                return null;
            }

            foreach (var p in processes)
            {
                var compare = exeFullPath; // just incase the following fucks up...

                // ugly fix for weird error message.
                // possible cause: accessing MainModule too soon after process has executed.
                // FileName seems to be read just find though so...
                try
                {
                    compare = p.MainModule.FileName;
                }
                catch (NullReferenceException)
                {
                }
                catch (Win32Exception)
                {
                }

                if (p.Id != ignoreId && string.Equals(exeFullPath, compare,
                    StringComparison.CurrentCultureIgnoreCase)) return p;
            }

            return null;
        }

        public static string SanitisePath(string s)
        {
            return s.Replace('/', '\\');
        }


        /// <summary>
        ///     Returns a string representation of the specified number in the format H:MM:SS.
        /// </summary>
        /// <param name="d">The duration in seconds.</param>
        /// <returns></returns>
        public static string DurationString(float d)
        {
            var ts = new TimeSpan(0, 0, (int)d);
            return string.Format("{0}{1}:{2}", ts.Hours > 0 ? ts.Hours + ":" : "",
                ts.Minutes.ToString("00"), ts.Seconds.ToString("00"));
        }

        public static string ReadNullTerminatedString(BinaryReader br, int charCount)
        {
            var startingByteIndex = br.BaseStream.Position;
            var s = ReadNullTerminatedString(br);

            var bytesToSkip =
                charCount - (int)(br.BaseStream.Position - startingByteIndex);
            br.BaseStream.Seek(bytesToSkip, SeekOrigin.Current);

            return s;
        }

        // TODO: replace with bitbuffer algorithm? benchmark and see which is faster
        public static string ReadNullTerminatedString(BinaryReader br)
        {
            var chars = new List<char>();

            while (true)
            {
                var c = br.ReadChar();

                if (c == '\0') break;

                chars.Add(c);
            }

            return new string(chars.ToArray());
        }

        public static int LogBase2(int number)
        {
            Debug.Assert(number == 1 || number % 2 == 0);

            var result = 0;

            while ((number >>= 1) != 0) result++;

            return result;
        }

        public static void AbortThread(Thread thread)
        {
            Debug.Assert(thread != null);

            if (!thread.IsAlive) return;

            thread.Abort();
            thread.Join();

            /* do
             {
                 thread.Abort();
             }
             while (!thread.Join(100));*/
        }

        public static TSource FirstOrDefault<TSource>(IEnumerable<TSource> source,
            Function<TSource, bool> predicate)
        {
            foreach (var item in source)
                if (predicate(item))
                    return item;

            return default;
        }

        public static void LogException(Exception e)
        {
            LogException(e, false);
        }

        private static void LogCultureInfo(TextWriter writer, string description,
            CultureInfo cultureInfo)
        {
            writer.WriteLine("{0}: \'{1}\', \'{2}\', \'{3}\'", description,
                cultureInfo.Name, cultureInfo.DisplayName, cultureInfo.EnglishName);
        }

        public static void LogException(Exception e, bool warning)
        {
        }

        /// <summary>
        ///     Calculates a Steam ID from a "*sid" Half-Life infokey value.
        /// </summary>
        /// <param name="sidInfoKeyValue">The "*sid" infokey value.</param>
        /// <returns>A Steam ID in the format "STEAM_0:x:y".</returns>
        public static string CalculateSteamId(string sidInfoKeyValue)
        {
            if (!ulong.TryParse(sidInfoKeyValue, out var sid) || sid == 0)
                // HLTV proxy or LAN dedicated server.
                return null;

            var authId = sid - 76561197960265728;
            var serverId = authId % 2 == 0 ? 0 : 1;
            authId = (authId - (ulong)serverId) / 2;
            return string.Format("STEAM_0:{0}:{1}", serverId, authId);
        }

        /// <summary>
        ///     Copies the column values of the selected rows in the given ListView to the clipboard. Column values are comma
        ///     separated, each row is on a new line.
        /// </summary>
        /// <remarks>A column's value is read from the first TextBlock element in the column.</remarks>
        [ValueConversion(typeof(object), typeof(object))]
        public class NullConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter,
                CultureInfo culture)
            {
                if (value == null) return "-";

                return value;
            }

            public object ConvertBack(object value, Type targetType, object parameter,
                CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        [ValueConversion(typeof(float), typeof(string))]
        public class TimestampConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter,
                CultureInfo culture)
            {
                if (value == null) return null;

                return DurationString((float)value);
            }

            public object ConvertBack(object value, Type targetType, object parameter,
                CultureInfo culture)
            {
                throw new NotImplementedException();
            }
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
            var newHandler = new MessageHandler
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
            var s = (string)messageStringTable[id];

            if (s == null) return "UNKNOWN";

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
            var log = new MessageLog
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

            //svc_damage = 19,
            svc_spawnstatic = 20,
            svc_event_reliable = 21,
            svc_spawnbaseline = 22,
            svc_tempentity = 23,
            svc_setpause = 24,
            svc_signonnum = 25,
            svc_centerprint = 26,

            //svc_killedmonster = 27,
            //svc_foundsecret = 28,
            svc_spawnstaticsound = 29,
            svc_intermission = 30,
            svc_finale = 31,
            svc_cdtrack = 32,

            //svc_restore = 33, // TEST ME!!! something to do with loading/saving
            //svc_cutscene = 34,
            svc_weaponanim = 35,

            //svc_decalname = 36,
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
            svc_sendcvarvalue2 = 58
        }

        public static string outDataStr = string.Empty;
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

            var deltaDescription = new HalfLifeDeltaStructure("delta_description_t");
            AddDeltaStructure(deltaDescription);

            deltaDescription.AddEntry("flags", 32, 1.0f,
                HalfLifeDeltaStructure.EntryFlags.Integer);
            deltaDescription.AddEntry("name", 8, 1.0f,
                HalfLifeDeltaStructure.EntryFlags.String);
            deltaDescription.AddEntry("offset", 16, 1.0f,
                HalfLifeDeltaStructure.EntryFlags.Integer);
            deltaDescription.AddEntry("size", 8, 1.0f,
                HalfLifeDeltaStructure.EntryFlags.Integer);
            deltaDescription.AddEntry("nBits", 8, 1.0f,
                HalfLifeDeltaStructure.EntryFlags.Integer);
            deltaDescription.AddEntry("divisor", 32, 4000.0f,
                HalfLifeDeltaStructure.EntryFlags.Float);
            deltaDescription.AddEntry("preMultiplier", 32, 4000.0f,
                HalfLifeDeltaStructure.EntryFlags.Float);


            // message handlers
            AddMessageHandler((byte)MessageId.svc_nop, 0);
            AddMessageHandler((byte)MessageId.svc_disconnect, MessageDisconnect);
            AddMessageHandler((byte)MessageId.svc_event, MessageEvent);
            AddMessageHandler((byte)MessageId.svc_version, MessageVersion);
            AddMessageHandler((byte)MessageId.svc_setview, MessageView);
            AddMessageHandler((byte)MessageId.svc_sound, MessageSound);
            AddMessageHandler((byte)MessageId.svc_time, 4);
            AddMessageHandler((byte)MessageId.svc_print, MessagePrint);
            AddMessageHandler((byte)MessageId.svc_stufftext, MessageStuffText);
            AddMessageHandler((byte)MessageId.svc_setangle, 6);
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
            AddMessageHandler((byte)MessageId.svc_intermission, 0);
            AddMessageHandler((byte)MessageId.svc_finale, 1);
            AddMessageHandler((byte)MessageId.svc_cdtrack, 2);
            AddMessageHandler((byte)MessageId.svc_weaponanim, 2);
            AddMessageHandler((byte)MessageId.svc_roomtype, 2);
            AddMessageHandler((byte)MessageId.svc_addangle, 2);
            AddMessageHandler((byte)MessageId.svc_newusermsg, MessageNewUserMsg);
            AddMessageHandler((byte)MessageId.svc_packetentities,
                MessagePacketEntities);
            AddMessageHandler((byte)MessageId.svc_deltapacketentities,
                MessageDeltaPacketEntities);
            AddMessageHandler((byte)MessageId.svc_choke, 0);
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

            AddUserMessageHandler("DeathMsg", MessageDeath);
            AddUserMessageHandler("CurWeapon", MessageCurWeapon);
            AddUserMessageHandler("ResetHUD", MessageResetHud);
            AddUserMessageHandler("ScreenFade", MessageScreenFade);
        }

        // public so svc_deltadescription can be parsed elsewhere
        public void AddDeltaStructure(HalfLifeDeltaStructure structure)
        {
            // remove decoder if it already exists (duplicate svc_deltadescription message)
            // e.g. GotFrag Demo 6 (rs vs TSO).zip

            if (deltaDecoderTable[structure.Name] != null)
                deltaDecoderTable.Remove(structure.Name);

            deltaDecoderTable.Add(structure.Name, structure);
        }

        public HalfLifeDeltaStructure GetDeltaStructure(String name)
        {
            HalfLifeDeltaStructure structure = (HalfLifeDeltaStructure)deltaDecoderTable[name];

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
            if (userMessageTable.Contains(name)) userMessageTable.Remove(name);

            var userMessage = new UserMessage
            {
                Id = id,
                Length = length
            };

            userMessageTable.Add(name, userMessage);
            AddMessageIdString(id, name);

            // see if there's a handler waiting to be attached to this message
            var callback = (Procedure)userMessageCallbackTable[name];

            if (callback == null)
                AddMessageHandler(id, length);
            else
                AddMessageHandler(id, callback);
        }

        public void AddUserMessageHandler(string name, Procedure callback)
        {
            // override existing callback
            if (userMessageCallbackTable.Contains(name))
                userMessageCallbackTable.Remove(name);

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
            var userMessage = (UserMessage)userMessageTable[name];

            if (userMessage.Length == -1)
                // if svc_newusermsg length is -1, first byte is length
                return BitBuffer.ReadByte();

            return userMessage.Length;
        }

        public FrameHeader ReadFrameHeader()
        {
            var header = new FrameHeader
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
            var length = 0;

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
            var header = new GameDataFrameHeader();

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

        public void ParseGameDataMessages(byte[] frameData, ref string s)
        {
            if (ErrorCount > 100) return;

            try
            {
                outDataStr = s;
            }
            catch
            {
            }

            if (Program.needsaveframes) outDataStr += "{\n";
            try
            {
                ParseGameDataMessages(frameData, null);
            }
            catch (Exception ex)
            {
                ErrorCount++;
                var tmpcolor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("(E="+ ex.Message +")");
                Console.ForegroundColor = tmpcolor;
                if (ErrorCount > 5)
                {
                    if (Program.needsaveframes)
                        outDataStr += "FATAL ERROR. STOP MESSAGE PARSING.\n}\n";
                    if (!SkipNextErrors && MessageBox.Show(
                            "Error while demo parsing.\nPossible demo is HLTV or too old\nPlease convert demo using coldemoplayer and try again!\nContinue?? (Skip all errors but got bad result)",
                            "Something wrong!", MessageBoxButtons.YesNo) !=
                        DialogResult.Yes) Environment.Exit(-1);
                    SkipNextErrors = true;
                    return;
                }

                if (Program.needsaveframes) outDataStr += "ERROR IN PARSE MESSAGE.";
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
            //        if (Program.needsaveframes)
            //            outDataStr += "FATAL ERROR. STOP MESSAGE PARSING.\n}\n";
            //        if (!SkipNextErrors && MessageBox.Show(
            //                "Error while demo parsing.\nPossible demo is HLTV or too old\nPlease convert demo using coldemoplayer and try again!\nContinue?? (Skip all errors but got bad result)",
            //                "Something wrong!", MessageBoxButtons.YesNo) !=
            //            DialogResult.Yes) Environment.Exit(-1);
            //        SkipNextErrors = true;
            //        return;
            //    }

            //    if (Program.needsaveframes) outDataStr += "ERROR IN PARSE MESSAGE.";
            //}

            if (Program.needsaveframes)
            {
                outDataStr += "}\n";
                s = outDataStr;
            }
        }

        public void ParseGameDataMessages(byte[] frameData,
            Function<byte, byte> userMessageCallback)
        {
            // read game data frame into memory
            BitBuffer = new BitBuffer(frameData);
            readingGameData = true;
            int messageFrameOffsetOld = 0;
            if (frameData.Length == 0 && BitBuffer.CurrentByte == 0)
            {
                return;
            }
            // start parsing messages
            while (true)
            {
                if (Program.needsaveframes)
                {
                    outDataStr +=
                        "{MSGLEN-" + frameData.Length + ".MSGBYTE:" + BitBuffer.CurrentByte;
				}
                
                
                Byte messageId = BitBuffer.ReadByte();
                if (Program.needsaveframes)
                {
                	outDataStr += "MSGID:" + messageId;
                }
                // File.AppendAllText("messages.bin", messageId + "\n");

                String messageName = Enum.GetName(typeof(MessageId), messageId);

                if (messageName == null) // a user message, presumably
                {
                    messageName = FindMessageIdString(messageId);
                }
                if (messageName == null)
                    messageName = "UNKNOWN MESSAGE";
                if (Program.needsaveframes) outDataStr += ".MSGNAME:" + messageName + "}\n";
               
                MessageHandler messageHandler = FindMessageHandler(messageId);

                if (messageName == "svc_intermission")
                {
                    Console.WriteLine("----------Конец игры / Game Over----------");
                }

                // Handle the conversion of user message id's.
                // Used by demo writing to convert to the current network protocol.
                if (messageId > 64 && userMessageCallback != null)
                {
                    Byte newMessageId = userMessageCallback(messageId);

                    if (newMessageId != messageId)
                    {
                        // write the new id to the bitbuffer
                        BitBuffer.SeekBytes(-1);
                        BitBuffer.RemoveBytes(1);
                        BitBuffer.InsertBytes(new Byte[] { newMessageId });
                    }
                }

                // unknown message
                if (messageHandler == null)
                {
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
                    Byte length = BitBuffer.ReadByte();

                    Seek(length);
                }
                else
                {
                    throw new ApplicationException(String.Format("Unknown message id \"{0}\"", messageId));
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
                    throw new ApplicationException(String.Format("Offset error message \"{0}\"", 1234));
                }
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
            {
                //Debug.Assert(offset <= int.MaxValue);
                BitBuffer.SeekBytes((int)offset, origin);
            }
            else
            {
                fileStream.Seek(offset, origin);
            }
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
                if (demo.GsDemoInfo.Header.NetProtocol <= 43)
                    return 532;
                return 436;
            }
        }

        // doesn't need to be a property, but for consistencies sake...
        public int GameDataSequenceInfoLength => 28;

        #endregion

        #region Engine Messages

        private void MessageDisconnect()
        {
            if (Program.needsaveframes)
                outDataStr += "MessageDisconnect:" + BitBuffer.ReadString();
            else
                BitBuffer.ReadString();
        }

        public void MessageEvent()
        {
            if (demo.GsDemoInfo.Header.NetProtocol <= 43)
                BitBuffer.Endian = BitBuffer.EndianType.Big;

            var nEvents = BitBuffer.ReadUnsignedBits(5);
            if (Program.needsaveframes)
                outDataStr += "MessageEvents:(" + nEvents + "){\n";
            for (var i = 0; i < nEvents; i++)
            {
                if (Program.needsaveframes)
                {
                    outDataStr +=
                        "index:" + BitBuffer.ReadUnsignedBits(10); // event index
                    outDataStr += "{\n";
                }
                else
                {
                    BitBuffer.ReadUnsignedBits(10);
                }

                var packetIndexBit = BitBuffer.ReadBoolean();

                if (packetIndexBit)
                {
                    BitBuffer.SeekBits(11); // packet index

                    var deltaBit = BitBuffer.ReadBoolean();

                    if (deltaBit)
                        GetDeltaStructure("event_t").ReadDelta(BitBuffer, null);
                }

                var fireTimeBit = BitBuffer.ReadBoolean();

                if (fireTimeBit)
                {
                    if (Program.needsaveframes)
                        outDataStr += "TIME:" + BitBuffer.ReadUnsignedBits(16);
                    else
                        BitBuffer.ReadUnsignedBits(16);
                    //BitBuffer.SeekBits(16); // fire time
                }

                if (Program.needsaveframes) outDataStr += "}\n";
            }

            if (Program.needsaveframes) outDataStr += "}\n";
            
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
            if (Program.needsaveframes)
                outDataStr += "switch view to " + entityview + " player\n";
        }

        public void MessageSound()
        {
            if (demo.GsDemoInfo.Header.NetProtocol <= 43)
                BitBuffer.Endian = BitBuffer.EndianType.Big;

            var flags = BitBuffer.ReadUnsignedBits(9);

            if ((flags & (1 << 0)) != 0) // volume
                BitBuffer.SeekBits(8);

            if ((flags & (1 << 1)) != 0) // attenuation * 64
                BitBuffer.SeekBits(8);

            BitBuffer.SeekBits(3); // channel
            BitBuffer.SeekBits(11); // edict number

            if ((flags & (1 << 2)) != 0) // sound index (short)
                BitBuffer.SeekBits(16);
            else // sound index (byte)
                BitBuffer.SeekBits(8);

            BitBuffer.ReadVectorCoord(true); // position

            if ((flags & (1 << 3)) != 0) // pitch
                BitBuffer.SeekBits(8);

            BitBuffer.SkipRemainingBits();
            BitBuffer.Endian = BitBuffer.EndianType.Little;
        }

        private void MessagePrint()
        {
            if (Program.needsaveframes)
                outDataStr += "MessagePrint:" + BitBuffer.ReadString();
            else
                BitBuffer.ReadString();
        }

        private void MessageStuffText()
        {
            if (Program.needsaveframes)
                outDataStr += "MessageStuffText:" + BitBuffer.ReadString();
            else
                BitBuffer.ReadString();
        }

        private void MessageServerInfo()
        {
            Seek(28);
            demo.MaxClients = BitBuffer.ReadByte();

            Seek(2); // ???, byte multiplayer

            Program.GameDir = BitBuffer.ReadString(); // game dir

            if (demo.GsDemoInfo.Header.NetProtocol > 43)
                Program.ServerName =
                    BitBuffer.ReadString(); // server name                

            // skip map
            Program.MapName = BitBuffer.ReadString();

            if (demo.GsDemoInfo.Header.NetProtocol == 45)
            {
                var extraInfo = BitBuffer.ReadByte();
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
                    if (BitBuffer.ReadByte() > 0)
                        Seek(21);
            }
        }

        private void MessageLightStyle()
        {
            Seek(1);
            BitBuffer.ReadString();
        }

        public void MessageUpdateUserInfo()
        {
            var slot = BitBuffer.ReadByte();
            var id = BitBuffer.ReadInt32();
            var s = BitBuffer.ReadString();

            if (demo.GsDemoInfo.Header.NetProtocol > 43) Seek(16); // string hash

            /*
             * Если s пустая значит
             * ищем игрока с id и присваиваем ему новый слот
             * игрок со старым slot удаляем и перемещаем в другое место
             * */

            Program.Player player = null;
            var playerfound = false;
            var i = 0;

            // поиск игрока с нужным ID если он существует
            foreach (var p in Program.playerList)
            {
                if (p.Id == id)
                {
                    // игрок существует
                    playerfound = true;
                    player = p;
                    break;
                }

                i++;
            }

            // Если нет создаем нового
            // create player if it doesn't exist
            if (!playerfound || i == 0 || i >= Program.playerList.Count)
            {
                player = new Program.Player(slot, id);
                Program.playerList.Add(player);
                i = Program.playerList.Count - 1;
            }

            if (playerfound)
            {
                i = 0;
                playerfound = false;
                for (var n = 0; n < Program.playerList.Count;)
                    if (Program.playerList[n].Slot == slot)
                    {
                        Program.fullPlayerList.Add(Program.playerList[n]);
                        Program.playerList.RemoveAt(n);
                    }
                    else
                    {
                        n++;
                    }

                foreach (var p in Program.playerList)
                {
                    if (p.Id == id)
                    {
                        playerfound = true;
                        player = p;
                        break;
                    }

                    i++;
                }

                if (!playerfound)
                {
                    player = new Program.Player(slot, id);
                    Program.playerList.Add(player);
                    i = Program.playerList.Count - 1;
                }
            }

            // Если стркоа пустая значит игрок SLOT вышел, и на его место пришел новый с ID
            if (s.Length == 0)
            {
                // 0 length text = a player just left and another player's slot is being changed


                player.Slot = slot;
                Program.playerList[i] = player;
            }
            else
            {
                var bakaka = s;
                try
                {
                    // parse infokey string
                    s = s.Remove(0, 1); // trim leading slash
                    var infoKeyTokens = s.Split('\\');
                    for (var n = 0; n + 1 < infoKeyTokens.Length; n += 2)
                    {
                        if (n + 1 >= infoKeyTokens.Length)
                            // Must be an odd number of strings - a key without a value - ignore it.
                            break;
                        var key = infoKeyTokens[n];

                        if (key == "STEAMID") key = "pid";
                        if (key.ToLower() == "name") player.Name = infoKeyTokens[n + 1];

                        if (key == "*sid")
                        {
                            key = "STEAMID";
                            infoKeyTokens[n + 1] =
                                CalculateSteamId(infoKeyTokens[n + 1]);
                        }

                        // If the key already exists, overwrite it.
                        if (player.InfoKeys.ContainsKey(key))
                            player.InfoKeys.Remove(key);
                        player.InfoKeys.Add(key, infoKeyTokens[n + 1]);
                    }

                    Program.playerList[i] = player;
                }
                catch
                {
                    Console.WriteLine("Error in parsing:" + bakaka);
                }
            }
        }

        public void MessageDeltaDescription()
        {
            var structureName = BitBuffer.ReadString();

            if (demo.GsDemoInfo.Header.NetProtocol == 43)
                BitBuffer.Endian = BitBuffer.EndianType.Big;

            var nEntries = BitBuffer.ReadUnsignedBits(16);

            var newDeltaStructure = new HalfLifeDeltaStructure(structureName);
            AddDeltaStructure(newDeltaStructure);

            var deltaDescription = GetDeltaStructure("delta_description_t");

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
            if (demo.GsDemoInfo.Header.NetProtocol <= 43)
                BitBuffer.Endian = BitBuffer.EndianType.Big;

            var deltaSequence = BitBuffer.ReadBoolean();

            if (deltaSequence) BitBuffer.SeekBits(8); // delta sequence number

            var tmpdelta = GetDeltaStructure("clientdata_t");
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
        }

        public void MessagePings()
        {
            if (demo.GsDemoInfo.Header.NetProtocol <= 43)
                BitBuffer.Endian = BitBuffer.EndianType.Big;

            while (BitBuffer.ReadBoolean())
                BitBuffer.SeekBits(24); // int32 each: slot, ping, loss

            BitBuffer.SkipRemainingBits();
            BitBuffer.Endian = BitBuffer.EndianType.Little;
        }

        private void MessageSpawnStatic()
        {
            BitBuffer.SeekBytes(18);
            var renderMode = BitBuffer.ReadByte();

            if (renderMode != 0) BitBuffer.SeekBytes(5);
        }

        private void MessageEventReliable()
        {
            if (demo.GsDemoInfo.Header.NetProtocol <= 43)
                BitBuffer.Endian = BitBuffer.EndianType.Big;

            BitBuffer.SeekBits(10); // event index

            GetDeltaStructure("event_t").ReadDelta(BitBuffer, null);

            var delayBit = BitBuffer.ReadBoolean();

            if (delayBit) BitBuffer.SeekBits(16); // delay / 100.0f

            BitBuffer.SkipRemainingBits();
            BitBuffer.Endian = BitBuffer.EndianType.Little;
        }

        public void MessageSpawnBaseline()
        {
            if (demo.GsDemoInfo.Header.NetProtocol <= 43)
                BitBuffer.Endian = BitBuffer.EndianType.Big;

            while (true)
            {
                var entityIndex = BitBuffer.ReadUnsignedBits(11);

                if (entityIndex == (1 << 11) - 1) // all 1's
                    break;

                var entityType = BitBuffer.ReadUnsignedBits(2);
                string entityTypeString;

                if ((entityType & 1) != 0) // is bit 1 set?
                {
                    if (entityIndex > 0 && entityIndex <= demo.MaxClients)
                        entityTypeString = "entity_state_player_t";
                    else
                        entityTypeString = "entity_state_t";
                }
                else
                {
                    entityTypeString = "custom_entity_state_t";
                }

                GetDeltaStructure(entityTypeString).ReadDelta(BitBuffer, null);
            }

            var footer = BitBuffer.ReadUnsignedBits(5); // should be all 1's

            if (footer != (1 << 5) - 1)
                throw new ApplicationException("Bad svc_spawnbaseline footer.");

            var nExtraData = BitBuffer.ReadUnsignedBits(6);

            for (var i = 0; i < nExtraData; i++)
                GetDeltaStructure("entity_state_t").ReadDelta(BitBuffer, null);

            BitBuffer.Endian = BitBuffer.EndianType.Little;
            BitBuffer.SkipRemainingBits();
        }

        private void MessageTempEntity()
        {
            var type = BitBuffer.ReadByte();

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
                    Seek(5);
                    var textParmsEffect = BitBuffer.ReadByte();
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
                    Seek(9);
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
        }

        private void MessageSetPause()
        {
            Seek(1);
        }

        private void MessageCenterPrint()
        {
            var msgprint = BitBuffer.ReadString();
            if (Program.needsaveframes) outDataStr += "MessageCenterPrint:" + msgprint;
            if (msgprint == "%s")
            {
                var msgprint2 = BitBuffer.ReadString();
                if (Program.needsaveframes) outDataStr += "->" + msgprint2;
               // Console.Write("..bad msgcenterprint?..");
            }
        }

        public void MessageNewUserMsg()
        {
            var id = BitBuffer.ReadByte();
            var length = BitBuffer.ReadSByte();
            var name = BitBuffer.ReadString(16);
            if (Program.needsaveframes)
                outDataStr +=
                    "user msg:" + name + " len:" + length + " id " + id + "\n";
            AddUserMessage(id, length, name);
        }

        public void MessagePacketEntities()
        {
            BitBuffer.SeekBits(
                16); // num entities (not reliable at all, loop until footer - see below)

            if (demo.GsDemoInfo.Header.NetProtocol <= 43)
                BitBuffer.Endian = BitBuffer.EndianType.Big;

            uint entityNumber = 0;

            // begin entity parsing
            while (true)
            {
                var footer = BitBuffer.ReadUInt16();

                if (footer == 0) break;

                BitBuffer.SeekBits(-16);

                var entityNumberIncrement = BitBuffer.ReadBoolean();

                if (!entityNumberIncrement
                ) // entity number isn't last entity number + 1, need to read it in
                {
                    // is the following entity number absolute, or relative from the last one?
                    var absoluteEntityNumber = BitBuffer.ReadBoolean();

                    if (absoluteEntityNumber)
                        entityNumber = BitBuffer.ReadUnsignedBits(11);
                    else
                        entityNumber += BitBuffer.ReadUnsignedBits(6);
                }
                else
                {
                    entityNumber++;
                }

                if (demo.GsDemoInfo.Header.GameDir == "tfc")
                    BitBuffer.ReadBoolean(); // unknown

                var custom = BitBuffer.ReadBoolean();
                var useBaseline = BitBuffer.ReadBoolean();

                if (useBaseline) BitBuffer.SeekBits(6); // baseline index

                var entityType = "entity_state_t";

                if (entityNumber > 0 && entityNumber <= demo.MaxClients)
                    entityType = "entity_state_player_t";
                else if (custom) 
                	entityType = "custom_entity_state_t";

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

            if (demo.GsDemoInfo.Header.NetProtocol <= 43)
                BitBuffer.Endian = BitBuffer.EndianType.Big;

            uint entityNumber = 0;

            while (true)
            {
                var footer = BitBuffer.ReadUInt16();

                if (footer == 0) break;

                BitBuffer.SeekBits(-16);

                var removeEntity = BitBuffer.ReadBoolean();

                // is the following entity number absolute, or relative from the last one?
                var absoluteEntityNumber = BitBuffer.ReadBoolean();

                if (absoluteEntityNumber)
                    entityNumber = BitBuffer.ReadUnsignedBits(11);
                else
                    entityNumber += BitBuffer.ReadUnsignedBits(6);

                if (!removeEntity)
                {
                    if (demo.GsDemoInfo.Header.GameDir == "tfc")
                        BitBuffer.ReadBoolean(); // unknown

                    var custom = BitBuffer.ReadBoolean();

                    if (demo.GsDemoInfo.Header.NetProtocol <= 43)
                        BitBuffer.SeekBits(1); // unknown

                    var entityType = "entity_state_t";

                    if (entityNumber > 0 && entityNumber <= demo.MaxClients)
                        entityType = "entity_state_player_t";
                    else if (custom) 
                    	entityType = "custom_entity_state_t";

                    GetDeltaStructure(entityType).ReadDelta(BitBuffer, null);
                }
            }

            BitBuffer.SkipRemainingBits();
            BitBuffer.Endian = BitBuffer.EndianType.Little;
        }

        private void MessageResourceList()
        {
            if (demo.GsDemoInfo.Header.NetProtocol <= 43)
                BitBuffer.Endian = BitBuffer.EndianType.Big;

            var nEntries = BitBuffer.ReadUnsignedBits(12);

            for (var i = 0; i < nEntries; i++)
            {
                BitBuffer.SeekBits(4); // entry type
                BitBuffer.ReadString(); // entry name
                BitBuffer.SeekBits(36); // index (12 bits), file size (24 bits) signed?

                var flags = BitBuffer.ReadUnsignedBits(3);


                if ((flags & 4) != 0) // md5 hash
                    BitBuffer.SeekBytes(16);

                if (BitBuffer.ReadBoolean()) BitBuffer.SeekBytes(32); // reserved data
            }

            // consistency list
            // indices of resources to force consistency upon?
            if (BitBuffer.ReadBoolean())
                while (BitBuffer.ReadBoolean())
                {
                    var nBits = BitBuffer.ReadBoolean() ? 5 : 10;
                    BitBuffer.SeekBits(nBits); // consistency index
                }

            BitBuffer.SkipRemainingBits();
            BitBuffer.Endian = BitBuffer.EndianType.Little;
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
            foreach (var c in s)
            {
                if (char.IsLetterOrDigit(c) || c == '.')
                {

                }
                else
                    return true;
            }
            return false;
        }

        private void MessageCustomization()
        {
            // ???
            Seek(2);
            if (IsBadMessage(BitBuffer.ReadString()))
			{
                Console.WriteLine("ERROR: MessageCustomization bad arguments!");
            }
            Seek(23);
        }

        private void MessageFileTransferFailed()
        {
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

            var subCommand = BitBuffer.ReadByte();

            if (subCommand == 2) // HLTV_LISTEN/HLTV_CAMERA
            {
                Seek(8);
            }
            else if (subCommand != 0)
            {
                // TODO: fix this
            }
        }

        public static string ByteArrayToString(byte[] ba)
        {
            return BitConverter.ToString(ba).Replace("-", "");
        }

        private void MessageDirector()
        {
            var len = BitBuffer.ReadByte();
            var bytes = ByteArrayToString(BitBuffer.ReadBytes(len));
            //Console.WriteLine();
            //Console.Write(bytes);
            //Console.ReadLine();
            //bitBuffer.ReadByte();
            //if (Program.needsaveframes)
            //{
            //    outDataStr += "MessageDirector:" + bitBuffer.ReadString();
            //}
            //else
            //{
            //    bitBuffer.ReadString();
            //}
        }

        private static string codecname = "unknown";

        private void MessageVoiceInit()
        {
            // string: codec name (sv_voicecodec, either voice_miles or voice_speex)
            // byte: quality (sv_voicequality, 1 to 5)

            var tmpcodecname = BitBuffer.ReadString();
            if (tmpcodecname.Length > 0) codecname = tmpcodecname;

            //MessageBox.Show(codecname);
            if (Program.needsaveframes)
                outDataStr += "MessageVoiceInit:" + tmpcodecname + "\n";
            if (demo.GsDemoInfo.Header.NetProtocol >= 47) Seek(1);
        }


        private void MessageVoiceData()
        {
            // byte: client id/slot?
            // short: data length
            // length bytes: data

            int playerid = BitBuffer.ReadByte();
            var length = BitBuffer.ReadUInt16();
            var data = BitBuffer.ReadBytes(length);

            if (Program.needsaveframes)
                outDataStr += "MessageVoiceData:" + length + "\n";
            //MessageBox.Show(playerid + "(2):" + length);

            if (playerid >= 0 && playerid <= 33)
            {
                for (var i = 0; i < Program.playerList.Count; i++)
                {
                    if (Program.playerList[i].Slot == playerid)
                    {
                        Program.playerList[i].WriteVoice(length, data);
                    }
                }
            }
        }

        private void MessageSendExtraInfo()
        {
            // string: "com_clientfallback", always seems to be null
            // byte: sv_cheats

            // NOTE: had this backwards before, shouldn't matter
            var extra = BitBuffer.ReadString();
            //if (extra.Length > 0)
            //    Console.WriteLine(extra);
            //Console.ReadLine();
            Seek(1);
        }

        private void MessageResourceLocation()
        {
            // string: location?
            BitBuffer.ReadString();
        }

        private void MessageSendCvarValue()
        {
            BitBuffer.ReadString(); // The cvar.
        }

        private void MessageSendCvarValue2()
        {
            Seek(4); // unsigned int
            BitBuffer.ReadString(); // The cvar.
        }

        private void MessageCurWeapon()
        {
            var cStatus = BitBuffer.ReadByte();

            var weaponid =
                (Program.WeaponIdType)Enum.ToObject(typeof(Program.WeaponIdType),
                    BitBuffer.ReadByte());


            var clip = BitBuffer.ReadSByte();

            if (cStatus < 1) 
            {
            	
            	return;
            }

            //Console.WriteLine(Program.UserAlive.ToString() + " = " + cStatus
            //     + " = " + weaponid.ToString() + " = " + clip);


            if (Program.IsUserAlive() && Program.CurrentWeapon != weaponid &&
                weaponid != Program.WeaponIdType.WEAPON_BAD &&
                weaponid != Program.WeaponIdType.WEAPON_BAD2)
            {
                //Program.UserAlive = true;
                if (Program.needsaveframes)
                {
                    outDataStr +=
                        "\nCHANGE WEAPON 22 (" + Program.CurrentTime + ") " + Program.CurrentTimeString;
                    outDataStr += "\nFrom " + Program.CurrentWeapon + " to " + weaponid;
                    Console.WriteLine(
                        "CHANGE WEAPON 22 (" + Program.CurrentTime + ") " + Program.CurrentTimeString);
                    Console.WriteLine(
                        "From " + Program.CurrentWeapon + " to " + weaponid);
                }

                Program.NeedSearchAim2 = false;
                Program.Aim2AttackDetected = false;
                Program.ShotFound = -1;
                if (!Program.CurrentFrameAttacked && Program.IsAttack)
                {
                    //Console.WriteLine("-attack2");
                    Program.DisableAttackFromNextFrame = 2;
                }
                Program.SelectSlot = 0;
                Program.WeaponChanged = true;
                Program.AmmoCount = 0;
                // Program.IsAttackSkipTimes = 0;
                if (Program.CurrentWeapon != Program.WeaponIdType.WEAPON_NONE)
                    Program.SkipNextAttack = 1;

                Program.IsNoAttackLastTime = Program.CurrentTime + 1.0f;
                Program.NeedCheckAttack = false;
            }

            if (Program.UserAlive && !Program.UsingAnotherMethodWeaponDetection)
                Program.CurrentWeapon = weaponid;
        }

        private void MessageDeath()
        {
            var len = BitBuffer.ReadByte();
            var iKiller = BitBuffer.ReadByte();
            var iVictim = BitBuffer.ReadByte();
            var isHeadShot = BitBuffer.ReadByte();
            var weapon = BitBuffer.ReadString();

            if (iVictim == Program.UserId && iVictim == Program.UserId2)
            {
                Program.LastDeathTime = Program.CurrentTime;
                Program.UserAlive = false;
                Program.DeathsCoount++;
                if (Program.needsaveframes)
                    outDataStr += "LocalPlayer " + iVictim + " killed!\n";
            }
            else if (iKiller == Program.UserId && iKiller == Program.UserId2)
            {
                Program.KillsCount++;
            }

            if (Program.needsaveframes) outDataStr += "User " + iVictim + " killed!\n";
        }

        private void MessageResetHud()
        {
            if (Program.needsaveframes) outDataStr += "ResetHud!\n";
            Program.UserAlive = true;
        }

        private void MessageScreenFade()
        {
            var duration = BitBuffer.ReadUInt16();
            var holdTime = BitBuffer.ReadUInt16();
            var fadeFlags = BitBuffer.ReadUInt16();
            var r = BitBuffer.ReadByte();
            var g = BitBuffer.ReadByte();
            var b = BitBuffer.ReadByte();
            var a = BitBuffer.ReadByte();
            if (Program.needsaveframes)
                outDataStr +=
                    "MessageScreenFade. DUR:" + duration + ". HOLD:" + holdTime +
                    ". RGBA:" + r + " " + g + " " + b + " " + a + "\n";
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
            if (data == null)
                throw new ArgumentNullException("data", "Value cannot be null.");

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
            else if (origin == SeekOrigin.End) 
            	CurrentBit = data.Count * 8 - offset;

            if (CurrentBit < 0 || CurrentBit > data.Count * 8)
                throw new BitBufferOutOfRangeException();
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
            if (CurrentBit + nBits > data.Count * 8)
                throw new BitBufferOutOfRangeException();

            var currentByte = CurrentBit / 8;
            var bitOffset = CurrentBit - currentByte * 8;
            var nBytesToRead = (bitOffset + nBits) / 8;

            if ((bitOffset + nBits) % 8 != 0) nBytesToRead++;

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
                throw new ArgumentException(
                    "Value must be a positive integer between 1 and 32 inclusive.",
                    "nBits");

            // check for overflow
            if (CurrentBit + nBits > data.Count * 8)
                throw new BitBufferOutOfRangeException();

            var currentByte = CurrentBit / 8;
            var bitOffset = CurrentBit - currentByte * 8;
            var nBytesToRead = (bitOffset + nBits) / 8;

            if ((bitOffset + nBits) % 8 != 0) nBytesToRead++;

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
            if (Endian == EndianType.Little)
                return ReadUnsignedBitsLittleEndian(nBits);
            return ReadUnsignedBitsBigEndian(nBits);
        }

        public int ReadBits(int nBits)
        {
            var result = (int)ReadUnsignedBits(nBits - 1);
            var sign = ReadBoolean() ? 1 : 0;

            if (sign == 1) result = -((1 << (nBits - 1)) - result);

            return result;
        }

        public bool ReadBoolean()
        {
            // check for overflow
            if (CurrentBit + 1 > data.Count * 8)
                throw new BitBufferOutOfRangeException();

            var result =
                (data[CurrentBit / 8] & (Endian == EndianType.Little
                    ? 1 << (CurrentBit % 8)
                    : 128 >> (CurrentBit % 8))) == 0
                    ? false
                    : true;
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

            for (var i = 0; i < nBytes; i++) result[i] = ReadByte();

            return result;
        }

        public char[] ReadChars(int nChars)
        {
            var result = new char[nChars];

            for (var i = 0; i < nChars; i++)
                result[i] = (char)ReadByte(); // not unicode

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
            var xFlag = ReadBoolean();
            var yFlag = ReadBoolean();
            var zFlag = ReadBoolean();

            var result = new float[3];

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
            var intFlag = ReadBoolean();
            var fractionFlag = ReadBoolean();

            var value = 0.0f;

            if (!intFlag && !fractionFlag) return value;

            var sign = ReadBoolean();
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
            for (var i = 0; i < nBits; i++)
            {
                var currentByte = CurrentBit / 8;
                var bitOffset = CurrentBit - currentByte * 8;

                var temp = data[currentByte];
                temp -= (byte)(data[currentByte] & (1 << bitOffset));
                data[currentByte] = temp;

                CurrentBit++;
            }
        }

        public void PrintBits(StreamWriter writer, int nBits)
        {
            if (writer == null || nBits == 0) return;

            var sb = new StringBuilder();

            for (var i = 0; i < nBits; i++)
                sb.AppendFormat("{0}", ReadBoolean() ? 1 : 0);

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
            var currentByte = currentBit / 8;
            var bitOffset = currentBit - currentByte * 8;

            // calculate how many bits need to be written to the current byte
            var bitsToWriteToCurrentByte = 8 - bitOffset;
            if (bitsToWriteToCurrentByte > nBits) bitsToWriteToCurrentByte = nBits;

            // calculate how many bytes need to be added to the list
            var bytesToAdd = 0;

            if (nBits > bitsToWriteToCurrentByte)
            {
                var temp = nBits - bitsToWriteToCurrentByte;
                bytesToAdd = temp / 8;

                if (temp % 8 != 0) bytesToAdd++;
            }

            if (bitOffset == 0) bytesToAdd++;

            // add new bytes if needed
            for (var i = 0; i < bytesToAdd; i++) data.Add(new byte());

            var nBitsWritten = 0;

            // write bits to the current byte
            var b = (byte)(value & ((1 << bitsToWriteToCurrentByte) - 1));
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

            var sign = value < 0 ? 1u : 0u;
            WriteUnsignedBits(sign, 1);
        }

        public void WriteBoolean(bool value)
        {
            var currentByte = currentBit / 8;

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
            for (var i = 0; i < values.Length; i++) WriteByte(values[i]);
        }

        public void WriteChars(char[] values)
        {
            for (var i = 0; i < values.Length; i++) WriteByte((byte)values[i]);
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
            for (var i = 0; i < value.Length; i++) WriteByte((byte)value[i]);

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
            for (var i = 0; i < length - (value.Length + 1); i++) WriteByte(0);
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

            var intValue = (uint)value;

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
                var e = new Entry
                {
                    Name = name,
                    Value = null
                };

                entryList.Add(e);
            }

            public object FindEntryValue(string name)
            {
                var e = FindEntry(name);

                if (e == null) return null;

                return e.Value;
            }

            public void SetEntryValue(string name, object value)
            {
                var e = FindEntry(name);

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
                foreach (var e in entryList)
                    if (e.Name == name)
                        return e;

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
            public enum EntryFlags
            {
                Byte = (1 << 0),
                Short = (1 << 1),
                Float = (1 << 2),
                Integer = (1 << 3),
                Angle = (1 << 4),
                TimeWindow8 = (1 << 5),
                TimeWindowBig = (1 << 6),
                String = (1 << 7),
                Signed = (1 << 31)
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
                var name = (string)delta.FindEntryValue("name");
                var nBits = (uint)delta.FindEntryValue("nBits");
                var divisor = (float)delta.FindEntryValue("divisor");
                var flags = (EntryFlags)(uint)delta.FindEntryValue("flags");
                //Single preMultiplier = (Single)delta.FindEntryValue("preMultiplier");

                AddEntry(name, nBits, divisor, flags);
            }

            /// <summary>
            ///     Adds an entry manually. Should only need to be called when creating a delta_description_t structure.
            /// </summary>
            /// <param name="name"></param>
            /// <param name="nBits"></param>
            /// <param name="divisor"></param>
            /// <param name="flags"></param>
            public void AddEntry(string name, uint nBits, float divisor,
                EntryFlags flags)
            {
                var entry = new Entry
                {
                    Name = name,
                    nBits = nBits,
                    Divisor = divisor,
                    Flags = flags,
                    PreMultiplier = 1.0f
                };

                entryList.Add(entry);
            }

            public HalfLifeDelta CreateDelta()
            {
                var delta = new HalfLifeDelta(entryList.Count);

                // create delta structure with the same entries as the delta decoder, but no data
                foreach (var e in entryList) delta.AddEntry(e.Name);

                return delta;
            }

            public void ReadDelta(BitBuffer bitBuffer, HalfLifeDelta delta)
            {
                Byte[] bitmaskBytes;

                ReadDelta(bitBuffer, delta, out bitmaskBytes);
            }

            public void ReadDelta(BitBuffer bitBuffer, HalfLifeDelta delta,
                out byte[] bitmaskBytes)
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
                    bitmaskBytes[i] = bitBuffer.ReadByte();

                for (var i = 0; i < nBitmaskBytes; i++)
                    for (var j = 0; j < 8; j++)
                    {
                        var index = j + i * 8;

                        if (index == entryList.Count) return;

                        if ((bitmaskBytes[i] & (1 << j)) != 0)
                        {
                            if (Program.needsaveframes)
                                HalfLifeDemoParser.outDataStr +=
                                    "ENTRY(" + Name + ")" + entryList[index].Name + " = ";
                            var value = ParseEntry(bitBuffer, entryList[index]);
                            if (Name != null && entryList[index].Name != null)
                            {
                                if (Name == "weapon_data_t")
                                {
                                    if (entryList[index].Name == "m_flNextPrimaryAttack")
                                    {
                                        var reloadstatus =
                                            value != null ? (float)value : 1.0f;
                                        if (reloadstatus < 0.01 ||
                                            Program.CurrentWeapon ==
                                            Program.WeaponIdType.WEAPON_DEAGLE &&
                                            reloadstatus < 0.025)
                                        {
                                            Program.WeaponAvaiabled = true;
                                            Program.WeaponAvaiabledFrameId =
                                                Program.CurrentFrameIdWeapon;
                                            Program.WeaponAvaiabledFrameTime =
                                                Program.CurrentTime2;
                                        }
                                        else
                                        {
                                            if (Program.WeaponAvaiabled)
                                                Program.AutoAttackStrikesID++;
                                            Program.WeaponAvaiabled = false;
                                            Program.WeaponAvaiabledFrameId = 0;
                                            Program.WeaponAvaiabledFrameTime = 0.0f;
                                        }
                                    }

                                    if (entryList[index].Name == "m_fInReload")
                                    {
                                        var reloadstatus = value != null ? (uint)value : 0;
                                        if (reloadstatus > 0)
                                        {
                                            //Console.WriteLine("-attack4");
                                            //if (!Program.CurrentFrameAttacked)
                                            //    Program.IsAttack = false;
                                            Program.IsReload = true;
                                            Program.AttackCheck = -1;
                                            Program.Reloads++;
                                        }
                                        else
                                        {
                                            Program.Reloads2++;
                                            Program.IsReload = false;
                                        }
                                    }
                                }

                                if (Name == "clientdata_t")
                                {
                                    if (entryList[index].Name == "weaponanim")
                                        if (Program.SelectSlot > 0)
                                            Program.SkipNextAttack = 1;

                                    if (entryList[index].Name == "fov")
                                    {
                                        var fov = value != null ? (float)value : 0.0f;
                                        Program.ClientFov = fov;
                                    }
                                }

                                if ( /*this.name == "weapon_data_t" || */
                                    Name == "clientdata_t")
                                    if (entryList[index].Name == "m_iId")
                                    {
                                        var weaponid =
                                            (Program.WeaponIdType)Enum.ToObject(
                                                typeof(Program.WeaponIdType), value);

                                        if (Program.CurrentWeapon != weaponid)
                                        {
                                            Program.UsingAnotherMethodWeaponDetection =
                                                true;

                                            if (Program.needsaveframes)
                                            {
                                                HalfLifeDemoParser.outDataStr +=
                                                    "\nCHANGE WEAPON 2 (" +
                                                    Program.CurrentTime + ") " + Program.CurrentTimeString;
                                                HalfLifeDemoParser.outDataStr +=
                                                    "\nFrom " + Program.CurrentWeapon +
                                                    " to " + weaponid;
                                                Console.WriteLine(
                                                    "CHANGE WEAPON 2 (" +
                                                    Program.CurrentTime + ") " + Program.CurrentTimeString);

                                                Console.WriteLine(
                                                    "From " + Program.CurrentWeapon +
                                                    " to " + weaponid);
                                            }

                                            Program.NeedSearchAim2 = false;
                                            Program.Aim2AttackDetected = false;
                                            Program.ShotFound = -1;
                                            if (!Program.CurrentFrameAttacked &&
                                                Program.IsAttack)
                                            {
                                                //Console.WriteLine("-attack3");
                                                // Program.DisableAttackFromNextFrame = 1;
                                            }

                                            Program.SelectSlot = 0;
                                            Program.WeaponChanged = true;
                                            Program.AmmoCount = 0;
                                            // Program.IsAttackSkipTimes = 0;
                                            if (Program.CurrentWeapon !=
                                                Program.WeaponIdType.WEAPON_NONE)
                                                Program.SkipNextAttack = 1;

                                            Program.IsNoAttackLastTime =
                                                Program.CurrentTime + 1.0f;
                                            Program.NeedCheckAttack = false;
                                            Program.CurrentWeapon = weaponid;
                                        }
                                    }

                                if (Name == "weapon_data_t" && Program.NeedCheckAttack)
                                    if (entryList[index].Name == "m_iClip")
                                    {
                                        Program.NeedCheckAttack = false;
                                        var ammocount = value != null ? (int)value : 0;
                                        Program.attackscounter4++;

                                        //if (Program.LastWatchWeapon == Program.CurrentWeapon && Program.UserAlive && Program.AmmoCount > 0 && ammocount > 0 && Program.AmmoCount - ammocount > 0 && Program.AmmoCount - ammocount < 4)
                                        //{
                                        //    Program.SkipNextAttack2--;
                                        //    if (Program.IsAttack || Program.CurrentTime - Program.IsNoAttackLastTime < 0.1)
                                        //    {
                                        //        if (Program.CurrentWeapon == Program.WeaponIdType.WEAPON_FAMAS)
                                        //            Program.SkipNextAttack2 = 3;
                                        //        // Console.WriteLine("Attack");
                                        //    }
                                        //    else if (!Program.IsAttack && !Program.IsReload && Program.SkipNextAttack2 <= 0)
                                        //    {
                                        //        // Program.ShotFound = true;

                                        //        Program.TextComments.WriteLine("Detected [TRIGGER BOT] on (" + Program.CurrentTime + ") " + Program.CurrentTimeString);
                                        //        Program.AddViewDemoHelperComment("Detected [TRIGGER BOT]. Weapon:" + Program.CurrentWeapon.ToString(), 0.5f);
                                        //        Console.WriteLine("Detected [TRIGGER BOT] on (" + Program.CurrentTime + ") " + Program.CurrentTimeString);
                                        //        Program.BadAttackCount++;
                                        //        Program.LastBadAttackCount = Program.CurrentTime;

                                        //        Program.IsNoAttackLastTime = 0.0f;//fixme
                                        //    }
                                        //}

                                        Program.LastWatchWeapon = Program.CurrentWeapon;
                                        Program.WeaponChanged = false;
                                        Program.AmmoCount = ammocount;
                                    }
                            }

                            if (delta != null) delta.SetEntryValue(index, value);
                            if (Program.needsaveframes)
                                HalfLifeDemoParser.outDataStr += "\n";
                        }
                    }
            }

            public byte[] CreateDeltaBitmask(HalfLifeDelta delta)
            {
                var nBitmaskBytes =
                    (uint)(entryList.Count / 8 + (entryList.Count % 8 > 0 ? 1 : 0));
                var bitmaskBytes = new byte[nBitmaskBytes];

                for (var i = 0; i < bitmaskBytes.Length; i++)
                    for (var j = 0; j < 8; j++)
                    {
                        var index = j + i * 8;

                        if (index >= entryList.Count) break;

                        if (delta.FindEntryValue(entryList[index].Name) != null)
                            bitmaskBytes[i] |= (byte)(1 << j);
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

                for (var i = 0; i < bitmaskBytes.Length; i++)
                    bitWriter.WriteByte(bitmaskBytes[i]);

                for (var i = 0; i < bitmaskBytes.Length; i++)
                    for (var j = 0; j < 8; j++)
                    {
                        var index = j + i * 8;

                        if (index == entryList.Count) return;

                        if ((bitmaskBytes[i] & (1 << j)) != 0)
                            WriteEntry(delta, bitWriter, entryList[index]);
                    }
            }

            private object ParseEntry(BitBuffer bitBuffer, Entry e)
            {
                var signed = (e.Flags & EntryFlags.Signed) != 0;

                if ((e.Flags & EntryFlags.Byte) != 0)
                {
                    if (signed)
                    {
                        var retval = (sbyte)ParseInt(bitBuffer, e);
                        if (Program.needsaveframes)
                            HalfLifeDemoParser.outDataStr += retval + "(SByte)";
                        return retval;
                    }
                    else
                    {
                        var retval = (byte)ParseUnsignedInt(bitBuffer, e);
                        if (Program.needsaveframes)
                            HalfLifeDemoParser.outDataStr += retval + "(Byte)";
                        return retval;
                    }
                }

                if ((e.Flags & EntryFlags.Short) != 0)
                {
                    if (signed)
                    {
                        var retval = (short)ParseInt(bitBuffer, e);
                        if (Program.needsaveframes)
                            HalfLifeDemoParser.outDataStr += retval + "(Int16)";
                        return retval;
                    }
                    else
                    {
                        var retval = (ushort)ParseUnsignedInt(bitBuffer, e);
                        if (Program.needsaveframes)
                            HalfLifeDemoParser.outDataStr += retval + "(UInt16)";
                        return retval;
                    }
                }

                if ((e.Flags & EntryFlags.Integer) != 0)
                {
                    if (signed)
                        try
                        {
                            var retval = ParseInt(bitBuffer, e);
                            if (Program.needsaveframes)
                                HalfLifeDemoParser.outDataStr += retval + "(Int32)";
                            return retval;
                        }
                        catch
                        {
                            return 0;
                        }

                    try
                    {
                        var retval = ParseUnsignedInt(bitBuffer, e);
                        if (Program.needsaveframes)
                            HalfLifeDemoParser.outDataStr += retval + "(UInt32)";
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
                        var negative = false;
                        var bitsToRead = (int)e.nBits;

                        if (signed)
                        {
                            negative = bitBuffer.ReadBoolean();
                            bitsToRead--;
                        }

                        var retval =
                            bitBuffer.ReadUnsignedBits(bitsToRead) / e.Divisor *
                            (negative ? -1.0f : 1.0f);
                        if (Program.needsaveframes)
                            HalfLifeDemoParser.outDataStr += retval + "(FloatBit)";
                        return retval;
                    }


                    if ((e.Flags & EntryFlags.Angle) != 0)
                    {
                        var retval =
                            bitBuffer.ReadUnsignedBits((int)e.nBits) *
                            (360.0f / (1 << (int)e.nBits));
                        if (Program.needsaveframes)
                            HalfLifeDemoParser.outDataStr += retval + "(Float)";
                        return retval;
                    }
                }
                catch
                {
                    return 0.0f;
                }

                if ((e.Flags & EntryFlags.String) != 0)
                {
                    var retval = bitBuffer.ReadString();
                    if (Program.needsaveframes)
                        HalfLifeDemoParser.outDataStr += retval + "(String)";
                    return retval;
                }

                throw new ApplicationException(
                    string.Format("Unknown delta entry type {0}.", e.Flags));
            }

            private int ParseInt(BitBuffer bitBuffer, Entry e)
            {
                var negative = bitBuffer.ReadBoolean();
                return (int)bitBuffer.ReadUnsignedBits((int)e.nBits - 1) /
                    (int)e.Divisor * (negative ? -1 : 1);
            }

            private uint ParseUnsignedInt(BitBuffer bitBuffer, Entry e)
            {
                return bitBuffer.ReadUnsignedBits((int)e.nBits) / (uint)e.Divisor;
            }

            private void WriteEntry(HalfLifeDelta delta, BitWriter bitWriter, Entry e)
            {
                var signed = (e.Flags & EntryFlags.Signed) != 0;
                var value = delta.FindEntryValue(e.Name);

                if ((e.Flags & EntryFlags.Byte) != 0)
                {
                    if (signed)
                    {
                        var writeValue = (sbyte)value;
                        WriteInt(bitWriter, e, writeValue);
                    }
                    else
                    {
                        var writeValue = (byte)value;
                        WriteUnsignedInt(bitWriter, e, writeValue);
                    }
                }
                else if ((e.Flags & EntryFlags.Short) != 0)
                {
                    if (signed)
                    {
                        var writeValue = (short)value;
                        WriteInt(bitWriter, e, writeValue);
                    }
                    else
                    {
                        var writeValue = (ushort)value;
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
                    var writeValue = (float)value;
                    var bitsToWrite = (int)e.nBits;

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
                var writeValue = value * (int)e.Divisor;

                bitWriter.WriteBoolean(writeValue < 0);
                bitWriter.WriteUnsignedBits((uint)Math.Abs(writeValue),
                    (int)e.nBits - 1);
            }

            private void WriteUnsignedInt(BitWriter bitWriter, Entry e, uint value)
            {
                var writeValue = value * (uint)e.Divisor;
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
    }
}