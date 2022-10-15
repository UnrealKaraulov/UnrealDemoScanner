using System.Collections.Generic;
using System.Linq;

namespace DemoScanner.DemoStuff.L4D2Branch
{
    public class Category
    {
        /// <summary>
        ///     The details of CSGO (this is handled by the csgo parser but in case something brakes)
        /// </summary>
        public static readonly Category Csgo = new Category("Counter-Strike: Global Offensive", 66.6);

        /// <summary>
        ///     Anything unidentifiable
        /// </summary>
        public static readonly Category Uncommon = new Category("Uncommon");

        /// <summary>
        ///     The details of Portal 1
        /// </summary>
        public static readonly Category Portal = new Category("Portal", 66.6666666666667,
            new[]
            {
                "testchmb_a_00", "testchmb_a_01", "testchmb_a_02", "testchmb_a_03", "testchmb_a_04", "testchmb_a_05",
                "testchmb_a_06", "testchmb_a_07", "testchmb_a_08", "testchmb_a_09", "testchmb_a_10", "testchmb_a_11",
                "testchmb_a_13", "testchmb_a_14", "testchmb_a_15", "escape_00", "escape_01", "escape_02"
            },
            new[]
            {
                "Ch. 0/1", "Ch. 2/3", "Ch. 4/5", "Ch. 6/7", "Ch. 8", "Ch. 9", "Ch. 10", "Ch. 11/12", "Ch. 13", "Ch. 14",
                "Ch. 15", "Ch. 16", "Ch. 17", "Ch. 18", "Ch. 19", "Escape 0", "Escape 1", "Escape 2"
            });

        /// <summary>
        ///     The details of Portal 2 Single Player
        /// </summary>
        public static readonly Category Portal2Sp = new Category("Portal 2", 60,
            new[]
            {
                "sp_a1_intro1", "sp_a1_intro2", "sp_a1_intro3", "sp_a1_intro4", "sp_a1_intro5", "sp_a1_intro6",
                "sp_a1_intro7", "sp_a1_wakeup", "sp_a2_intro", "sp_a2_laser_intro", "sp_a2_laser_stairs",
                "sp_a2_dual_lasers", "sp_a2_laser_over_goo", "sp_a2_catapult_intro", "sp_a2_trust_fling",
                "sp_a2_pit_flings", "sp_a2_fizzler_intro", "sp_a2_sphere_peek", "sp_a2_ricochet", "sp_a2_bridge_intro",
                "sp_a2_bridge_the_gap", "sp_a2_turret_intro", "sp_a2_laser_relays", "sp_a2_turret_blocker",
                "sp_a2_laser_vs_turret", "sp_a2_pull_the_rug", "sp_a2_column_blocker", "sp_a2_laser_chaining",
                "sp_a2_triple_laser", "sp_a2_bts1", "sp_a2_bts2", "sp_a2_bts3", "sp_a2_bts4", "sp_a2_bts5",
                "sp_a2_bts6",
                "sp_a2_core", "sp_a3_00", "sp_a3_01", "sp_a3_03", "sp_a3_jump_intro", "sp_a3_bomb_flings",
                "sp_a3_crazy_box", "sp_a3_transition01", "sp_a3_speed_ramp", "sp_a3_speed_flings", "sp_a3_portal_intro",
                "sp_a3_end", "sp_a4_intro", "sp_a4_tb_intro", "sp_a4_tb_trust_drop", "sp_a4_tb_wall_button",
                "sp_a4_tb_polarity", "sp_a4_tb_catch", "sp_a4_stop_the_box", "sp_a4_laser_catapult",
                "sp_a4_laser_platform", "sp_a4_speed_tb_catch", "sp_a4_jump_polarity", "sp_a4_finale1", "sp_a4_finale2",
                "sp_a4_finale3", "sp_a4_finale4"
            },
            new[]
            {
                "Container Ride", "Portal Carousel", "Portal Gun", "Smooth Jazz", "Cube Momentum", "Future Starter",
                "Secret Panel", "Wakeup", "Incinerator", "Laser Intro", "Laser Stairs", "Dual Lasers", "Laser over Goo",
                "Catapult Intro", "Trust Fling", "Pit Flings", "Fizzler Intro", "Ceiling Catapult", "Ricochet",
                "Bridge Intro", "Bridge the Gap", "Turret Intro", "Laser Relays", "Turret Blocker", "Laser vs Turret",
                "Pull the Rug", "Column Blocker", "Laser Chaining", "Triple Laser", "Jail Break", "Escape",
                "Turret Factory", "Turret Sabotage", "Neurotoxin Sabotage", "Tube Ride", "Core", "The Fall",
                "Underground", "Cave Johnson", "Repulsion Intro", "Bomb Flings", "Crazy Box", "PotatOS",
                "Propulsion Intro", "Propulsion Flings", "Conversion Intro", "Three Gels", "Test", "Funnel Intro",
                "Ceiling Button", "Wall Button", "Polarity", "Funnel Catch", "Stop the Box", "Laser Catapult",
                "Laser Platform", "Propulsion Catch", "Repulsion Polarity", "Finale 1", "Finale 2", "Finale 3",
                "Finale 4"
            });

        /// <summary>
        ///     The details of Portal 2's co-operative mode
        /// </summary>
        public static readonly Category Portal2Coop = new Category("Portal 2 Coop", 60,
            new[]
            {
                "mp_coop_start", "mp_coop_lobby_3", "mp_coop_doors", "mp_coop_race_2", "mp_coop_laser_2",
                "mp_coop_rat_maze", "mp_coop_laser_crusher", "mp_coop_teambts", "mp_coop_lobby_3", "mp_coop_fling_3",
                "mp_coop_infinifling_train", "mp_coop_come_along", "mp_coop_fling_1", "mp_coop_catapult_1",
                "mp_coop_multifling_1", "mp_coop_fling_crushers", "mp_coop_fan", "mp_coop_lobby_3",
                "mp_coop_wall_intro",
                "mp_coop_wall_2", "mp_coop_catapult_wall_intro", "mp_coop_wall_block", "mp_coop_catapult_2",
                "mp_coop_turret_walls", "mp_coop_turret_ball", "mp_coop_wall_5", "mp_coop_lobby_3",
                "mp_coop_tbeam_redirect", "mp_coop_tbeam_drill", "mp_coop_tbeam_catch_grind_1", "mp_coop_tbeam_laser_1",
                "mp_coop_tbeam_polarity", "mp_coop_tbeam_polarity2", "mp_coop_tbeam_polarity3", "mp_coop_tbeam_maze",
                "mp_coop_tbeam_end", "mp_coop_lobby_3", "mp_coop_paint_come_along", "mp_coop_paint_redirect",
                "mp_coop_paint_bridge", "mp_coop_paint_walljumps", "mp_coop_paint_speed_fling",
                "mp_coop_paint_red_racer", "mp_coop_paint_speed_catch", "mp_coop_paint_longjump_intro"
            },
            new[]
            {
                "Calibration", "Lobby 1", "Doors", "Buttons", "Lasers", "Rat Maze", "Laser Crushers",
                "Behind the Scenes",
                "Lobby 2", "Flings", "Infinifling", "Team Retrieval", "Vertical Flings", "Catapults", "Multifling",
                "Fling Crushers", "Industrial Fan", "Lobby 3", "Cooperative Bridges", "Bridge Swap", "Fling Block",
                "Catapult Block", "Bridge Fling", "Turret Walls", "Turret Assassin", "Bridge Testing", "Lobby 4",
                "Cooperative Funnels", "Funnel Drill", "Funnel Catch", "Funnel Laser", "Cooperative Polarity",
                "Funnel Hop", "Advanced Polarity", "Funnel Maze", "Turret Warehouse", "Lobby 5", "Repulsion Jumps",
                "Double Bounce", "Bridge Repulsion", "Wall Repulsion", "Propulsion Crushers", "Turret Ninja",
                "Propulsion Retrieval", "Vault Entrance"
            });

        /// <summary>
        ///     The details of Course 6 in Portal 2's co-operative mode
        /// </summary>
        public static readonly Category Portal2CoopCourse6 = new Category("Portal 2 Coop - Course 6", 60,
            new[]
            {
                "mp_coop_lobby_3", "mp_coop_separation_1", "mp_coop_tripleaxis", "mp_coop_catapult_catch",
                "mp_coop_2paints_1bridge", "mp_coop_paint_conversion", "mp_coop_bridge_catch", "mp_coop_laser_tbeam",
                "mp_coop_paint_rat_maze", "mp_coop_paint_crazy_box"
            },
            new[]
            {
                "Lobby", "Separation", "Triple Axis", "Catapult Catch", "Bridge Gels", "Maintenance", "Bridge Catch",
                "Double Lift", "Gel Maze", "Crazier Box"
            });

        /// <summary>
        ///     The details of Portal 2's workshop mode
        /// </summary>
        public static readonly Category Portal2Workshop = new Category("Portal 2 Workshop", 60);

        /// <summary>
        ///     The details of Aperture Tag
        /// </summary>
        public static readonly Category ApertureTag = new Category("Aperture Tag", 60,
            new[]
            {
                "gg_intro_wakeup",
                "gg_blue_only",
                "gg_blue_only_2",
                "gg_blue_only_3",
                "gg_blue_only_2_pt2",
                "gg_a1_intro4",
                "gg_blue_upplatform",
                "gg_red_only",
                "gg_red_surf",
                "gg_all_intro",
                "gg_all_rotating_wall",
                "gg_all_fizzler",
                "gg_all_intro_2",
                "gg_a2_column_blocker",
                "gg_all_puzzle2",
                "gg_all2_puzzle1",
                "gg_all_puzzle1",
                "gg_all2_escape",
                "gg_stage_reveal",
                "gg_stage_bridgebounce_2",
                "gg_stage_redfirst",
                "gg_stage_laserrelay",
                "gg_stage_beamscotty",
                "gg_stage_bridgebounce",
                "gg_stage_roofbounce",
                "gg_stage_pickbounce",
                "gg_stage_theend",
                "gg_tag_remix",
                "gg_trailer_map"
            });

        /// <summary>
        ///     The details of Aperture Tag's workshop mode
        /// </summary>
        public static readonly Category ApertureTagWorkshop = new Category("Aperture Tag Workshop", 60);

        /// <summary>
        ///     The details of Portal Stories: Mel
        /// </summary>
        public static readonly Category PortalStoriesMel = new Category("Portal Stories: Mel", 60,
            new[]
            {
                "sp_a1_tramride",
                "sp_a1_mel_intro",
                "sp_a1_lift",
                "sp_a1_garden",
                "sp_a2_garden_de",
                "sp_a2_underbounce",
                "sp_a2_once_upon",
                "sp_a2_past_power",
                "sp_a2_ramp",
                "sp_a2_firestorm",
                "sp_a3_junkyard",
                "sp_a3_concepts",
                "sp_a3_paint_fling",
                "sp_a3_faith_plate",
                "sp_a3_transition",
                "sp_a4_overgrown",
                "sp_a4_tb_over_goo",
                "sp_a4_two_of_a_kind",
                "sp_a4_destroyed",
                "sp_a4_factory",
                "sp_a4_core_access",
                "sp_a4_finale",
                "st_a1_tramride",
                "st_a1_mel_intro",
                "st_a1_lift",
                "st_a1_garden",
                "st_a2_garden_de",
                "st_a2_underbounce",
                "st_a2_once_upon",
                "st_a2_past_power",
                "st_a2_ramp",
                "st_a2_firestorm",
                "st_a3_junkyard",
                "st_a3_concepts",
                "st_a3_paint_fling",
                "st_a3_faith_plate",
                "st_a3_transition",
                "st_a4_overgrown",
                "st_a4_tb_over_goo",
                "st_a4_two_of_a_kind",
                "st_a4_destroyed",
                "st_a4_factory",
                "st_a4_core_access",
                "st_a4_finale"
            });

        /// <summary>
        ///     The details of INFRA
        /// </summary>
        public static readonly Category Infra = new Category("INFRA", 120,
            new[]
            {
                "infra_c1_m1_office",
                "infra_c2_m1_reserve1",
                "infra_c2_m2_reserve2",
                "infra_c2_m3_reserve3",
                "infra_c3_m1_tunnel",
                "infra_c3_m2_tunnel2",
                "infra_c3_m3_tunnel3",
                "infra_c3_m4_tunnel4",
                "infra_c4_m2_furnace",
                "infra_c4_m3_tower",
                "infra_c5_m1_watertreatment",
                "infra_c5_m2_sewer",
                "infra_c5_m2b_sewer2",
                "infra_c6_m1_sewer3",
                "infra_c6_m2_metro",
                "infra_c6_m3_metroride",
                "infra_c6_m4_waterplant",
                "infra_c6_m5_minitrain",
                "infra_c6_m6_central",
                "infra_c7_m1_servicetunnel",
                "infra_c7_m1b_skyscraper",
                "infra_c7_m2_bunker",
                "infra_c7_m3_stormdrain",
                "infra_c7_m4_cistern",
                "infra_c7_m5_powerstation",
                "infra_ee_hallway",
                "main_menu"
            });

        /// <summary>
        ///     The details of INFRA's workshop mode
        /// </summary>
        public static readonly Category InfraWorkshop = new Category("INFRA Workshop", 120);

        private static readonly Category HalfLife2 = new Category("Half Life 2", 66.6666666666667,
            new[]
            {
                "d1_trainstation_01", "d1_trainstation_02", "d1_trainstation_03", "d1_trainstation_04",
                "d1_trainstation_05", "d1_trainstation_06", "d1_canals_01", "d1_canals_01A", "d1_canals_02",
                "d1_canals_03", "d1_canals_05", "d1_canals_06", "d1_canals_07", "d1_canals_08", "d1_canals_09",
                "d1_canals_10", "d1_canals_11", "d1_canals_12", "d1_canals_13", "d1_eli_01", "d1_eli_02", "d1_town_01",
                "d1_town_01A", "d1_town_02", "d1_town_02A", "d1_town_03", "d1_town_04", "d1_town_05", "d2_coast_01",
                "d2_coast_03", "d2_coast_04", "d2_coast_05", "d2_coast_07", "d2_coast_08", "d2_coast_09", "d2_coast_10",
                "d2_coast_11", "d2_coast_12", "d2_prison_01", "d2_prison_02", "d2_prison_03", "d2_prison_04",
                "d2_prison_05", "d2_prison_06", "d2_prison_07", "d2_prison_08", "d3_c17_01", "d3_c17_02", "d3_c17_03",
                "d3_c17_04", "d3_c17_05", "d3_c17_06a", "d3_c17_06b", "d3_c17_07", "d3_c17_08", "d3_c17_09",
                "d3_c17_10a", "d3_c17_10b", "d3_c17_11", "d3_c17_12", "d3_c17_12b", "d3_c17_13", "d3_citadel_01",
                "d3_citadel_02", "d3_citadel_03", "d3_citadel_04", "d3_citadel_05", "d3_breen_01"
            },
            new[]
            {
                "d1_trainstation_01", "d1_trainstation_02", "d1_trainstation_03", "d1_trainstation_04",
                "d1_trainstation_05", "d1_trainstation_06", "d1_canals_01", "d1_canals_01A", "d1_canals_02",
                "d1_canals_03", "d1_canals_05", "d1_canals_06", "d1_canals_07", "d1_canals_08", "d1_canals_09",
                "d1_canals_10", "d1_canals_11", "d1_canals_12", "d1_canals_13", "d1_eli_01", "d1_eli_02", "d1_town_01",
                "d1_town_01A", "d1_town_02", "d1_town_02A", "d1_town_03", "d1_town_04", "d1_town_05", "d2_coast_01",
                "d2_coast_03", "d2_coast_04", "d2_coast_05", "d2_coast_07", "d2_coast_08", "d2_coast_09", "d2_coast_10",
                "d2_coast_11", "d2_coast_12", "d2_prison_01", "d2_prison_02", "d2_prison_03", "d2_prison_04",
                "d2_prison_05", "d2_prison_06", "d2_prison_07", "d2_prison_08", "d3_c17_01", "d3_c17_02", "d3_c17_03",
                "d3_c17_04", "d3_c17_05", "d3_c17_06a", "d3_c17_06b", "d3_c17_07", "d3_c17_08", "d3_c17_09",
                "d3_c17_10a", "d3_c17_10b", "d3_c17_11", "d3_c17_12", "d3_c17_12b", "d3_c17_13", "d3_citadel_01",
                "d3_citadel_02", "d3_citadel_03", "d3_citadel_04", "d3_citadel_05", "d3_breen_01"
            });

        private Category(string name, double tps = 0, string[] maps = null, string[] mapNames = null)
        {
            Name = name;
            TicksPerSecond = tps;
            Maps = maps;
            MapNames = mapNames;
        }

        /// <summary>
        ///     The possible game types
        /// </summary>
        public static IEnumerable<Category> Values
        {
            get
            {
                yield return Csgo;
                yield return Portal;
                yield return Portal2Sp;
                yield return Portal2Coop;
                yield return Portal2CoopCourse6;
                yield return Portal2Workshop;
                yield return ApertureTag;
                yield return ApertureTagWorkshop;
                yield return PortalStoriesMel;
                yield return Infra;
                yield return InfraWorkshop;
                yield return HalfLife2;
                yield return Uncommon;
            }
        }

        /// <summary>
        ///     Name of the game
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Tickrate of the game
        /// </summary>
        public double TicksPerSecond { get; }

        /// <summary>
        ///     Maps of the game
        /// </summary>
        public string[] Maps { get; }

        /// <summary>
        ///     The literal name of maps
        /// </summary>
        public string[] MapNames { get; }

        /// <summary>
        ///     Finds the appropriate category for a game name
        /// </summary>
        /// <param name="name">Name of the game</param>
        /// <returns>A category</returns>
        public static Category FromName(string name)
        {
            return Values.FirstOrDefault(category => name == category.Name);
        }

        /// <summary>
        ///     Checks if the category contains the specified map
        /// </summary>
        /// <param name="mapName">Name of the map</param>
        /// <param name="ignoreCase">Set to true if name cases should be ignored</param>
        /// <returns></returns>
        public bool HasMap(string mapName, bool ignoreCase = false)
        {
            return ignoreCase
                ? (bool)Maps?.Select(map => map.ToLower()).Contains(mapName.ToLower())
                : (bool)Maps?.Contains(mapName);
        }
    }
}