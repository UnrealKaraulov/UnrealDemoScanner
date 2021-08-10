using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VolvoWrench.DemoStuff.L4D2Branch.CSGODemoInfo.DP.FastNetmessages;

namespace VolvoWrench.DemoStuff.L4D2Branch.CSGODemoInfo.DP.Handler
{
    /// <summary>
    ///     This class manages all GameEvents for a demo-parser.
    /// </summary>
    public static class GameEventHandler
    {
        public static void HandleGameEventList(IEnumerable<GameEventList.Descriptor> gel, DemoParser parser)
        {
            parser.GEH_Descriptors = new Dictionary<int, GameEventList.Descriptor>();
            foreach (var d in gel) parser.GEH_Descriptors[d.EventId] = d;
        }

        /// <summary>
        ///     Apply the specified rawEvent to the parser.
        /// </summary>
        /// <param name="rawEvent">The raw event.</param>
        /// <param name="parser">The parser to mutate.</param>
        public static void Apply(GameEvent rawEvent, DemoParser parser)
        {
            var descriptors = parser.GEH_Descriptors;
            var blindPlayers = parser.GEH_BlindPlayers;

            if (descriptors == null) return;

            Dictionary<string, object> data;
            var eventDescriptor = descriptors[rawEvent.EventId];

            if (parser.Players.Count == 0 && eventDescriptor.Name != "player_connect") return;

            if (eventDescriptor.Name == "round_start")
            {
                data = MapData(eventDescriptor, rawEvent);

                var rs = new RoundStartedEventArgs
                {
                    TimeLimit = (int) data["timelimit"],
                    FragLimit = (int) data["fraglimit"],
                    Objective = (string) data["objective"]
                };

                parser.RaiseRoundStart(rs);
            }

            if (eventDescriptor.Name == "cs_win_panel_match") parser.RaiseWinPanelMatch();

            if (eventDescriptor.Name == "round_announce_final") parser.RaiseRoundFinal();

            if (eventDescriptor.Name == "round_announce_last_round_half") parser.RaiseLastRoundHalf();

            if (eventDescriptor.Name == "round_end")
            {
                data = MapData(eventDescriptor, rawEvent);

                var t = Team.Spectate;

                var winner = (int) data["winner"];

                if (winner == parser.tID)
                    t = Team.Terrorist;
                else if (winner == parser.ctID) t = Team.CounterTerrorist;

                var roundEnd = new RoundEndedEventArgs
                {
                    Reason = (RoundEndReason) data["reason"],
                    Winner = t,
                    Message = (string) data["message"]
                };

                parser.RaiseRoundEnd(roundEnd);
            }

            if (eventDescriptor.Name == "round_officially_ended") parser.RaiseRoundOfficiallyEnd();

            if (eventDescriptor.Name == "round_mvp")
            {
                data = MapData(eventDescriptor, rawEvent);

                var roundMVPArgs = new RoundMVPEventArgs
                {
                    Player = parser.Players.ContainsKey((int) data["userid"])
                        ? parser.Players[(int) data["userid"]]
                        : null,
                    Reason = (RoundMVPReason) data["reason"]
                };

                parser.RaiseRoundMVP(roundMVPArgs);
            }

            if (eventDescriptor.Name == "bot_takeover")
            {
                data = MapData(eventDescriptor, rawEvent);

                var botTakeOverArgs = new BotTakeOverEventArgs
                {
                    Taker = parser.Players.ContainsKey((int) data["userid"])
                        ? parser.Players[(int) data["userid"]]
                        : null
                };

                parser.RaiseBotTakeOver(botTakeOverArgs);
            }

            if (eventDescriptor.Name == "begin_new_match") parser.RaiseMatchStarted();

            if (eventDescriptor.Name == "round_freeze_end") parser.RaiseFreezetimeEnded();

            //if (eventDescriptor.Name != "player_footstep" && eventDescriptor.Name != "weapon_fire" && eventDescriptor.Name != "player_jump") {
            //	Console.WriteLine (eventDescriptor.Name);
            //}

            switch (eventDescriptor.Name)
            {
                case "weapon_fire":

                    data = MapData(eventDescriptor, rawEvent);

                    var fire = new WeaponFiredEventArgs
                    {
                        Shooter = parser.Players.ContainsKey((int) data["userid"])
                            ? parser.Players[(int) data["userid"]]
                            : null,
                        Weapon = new Equipment((string) data["weapon"])
                    };

                    if (fire.Shooter != null && fire.Weapon.Class != EquipmentClass.Grenade)
                        fire.Weapon = fire.Shooter.ActiveWeapon;

                    parser.RaiseWeaponFired(fire);
                    break;
                case "player_death":
                    data = MapData(eventDescriptor, rawEvent);

                    var kill = new PlayerKilledEventArgs
                    {
                        Victim = parser.Players.ContainsKey((int) data["userid"])
                            ? parser.Players[(int) data["userid"]]
                            : null,
                        Killer = parser.Players.ContainsKey((int) data["attacker"])
                            ? parser.Players[(int) data["attacker"]]
                            : null,
                        Assister = parser.Players.ContainsKey((int) data["assister"])
                            ? parser.Players[(int) data["assister"]]
                            : null,
                        Headshot = (bool) data["headshot"],
                        Weapon = new Equipment((string) data["weapon"], (string) data["weapon_itemid"])
                    };

                    if (kill.Killer != null && kill.Weapon.Class != EquipmentClass.Grenade && kill.Killer.Weapons.Any())
                    {
#if DEBUG
                        if (kill.Weapon.Weapon != kill.Killer.ActiveWeapon.Weapon)
                            throw new InvalidDataException();
#endif
                        kill.Weapon = kill.Killer.ActiveWeapon;
                    }


                    kill.PenetratedObjects = (int) data["penetrated"];

                    parser.RaisePlayerKilled(kill);
                    break;
                case "player_hurt":
                    data = MapData(eventDescriptor, rawEvent);

                    var hurt = new PlayerHurtEventArgs
                    {
                        Player = parser.Players.ContainsKey((int) data["userid"])
                            ? parser.Players[(int) data["userid"]]
                            : null,
                        Attacker = parser.Players.ContainsKey((int) data["attacker"])
                            ? parser.Players[(int) data["attacker"]]
                            : null,
                        Health = (int) data["health"],
                        Armor = (int) data["armor"],
                        HealthDamage = (int) data["dmg_health"],
                        ArmorDamage = (int) data["dmg_armor"],
                        Hitgroup = (Hitgroup) (int) data["hitgroup"],

                        Weapon = new Equipment((string) data["weapon"], "")
                    };

                    if (hurt.Attacker != null && hurt.Weapon.Class != EquipmentClass.Grenade &&
                        hurt.Attacker.Weapons.Any())
                        hurt.Weapon = hurt.Attacker.ActiveWeapon;

                    parser.RaisePlayerHurt(hurt);
                    break;

                #region Nades

                case "player_blind":
                    data = MapData(eventDescriptor, rawEvent);
                    if (parser.Players.ContainsKey((int) data["userid"]))
                        blindPlayers.Add(parser.Players[(int) data["userid"]]);

                    break;
                case "flashbang_detonate":
                    var args = FillNadeEvent<FlashEventArgs>(MapData(eventDescriptor, rawEvent), parser);
                    args.FlashedPlayers = blindPlayers.ToArray();
                    parser.RaiseFlashExploded(args);
                    blindPlayers.Clear();
                    break;
                case "hegrenade_detonate":
                    parser.RaiseGrenadeExploded(FillNadeEvent<GrenadeEventArgs>(MapData(eventDescriptor, rawEvent),
                        parser));
                    break;
                case "decoy_started":
                    parser.RaiseDecoyStart(FillNadeEvent<DecoyEventArgs>(MapData(eventDescriptor, rawEvent), parser));
                    break;
                case "decoy_detonate":
                    parser.RaiseDecoyEnd(FillNadeEvent<DecoyEventArgs>(MapData(eventDescriptor, rawEvent), parser));
                    break;
                case "smokegrenade_detonate":
                    parser.RaiseSmokeStart(FillNadeEvent<SmokeEventArgs>(MapData(eventDescriptor, rawEvent), parser));
                    break;
                case "smokegrenade_expired":
                    parser.RaiseSmokeEnd(FillNadeEvent<SmokeEventArgs>(MapData(eventDescriptor, rawEvent), parser));
                    break;
                case "inferno_startburn":
                    parser.RaiseFireStart(FillNadeEvent<FireEventArgs>(MapData(eventDescriptor, rawEvent), parser));
                    break;
                case "inferno_expire":
                    parser.RaiseFireEnd(FillNadeEvent<FireEventArgs>(MapData(eventDescriptor, rawEvent), parser));
                    break;

                #endregion

                case "player_connect":
                    data = MapData(eventDescriptor, rawEvent);

                    var player = new PlayerInfo
                    {
                        UserID = (int) data["userid"],
                        Name = (string) data["name"],
                        GUID = (string) data["networkid"]
                    };
                    player.XUID = player.GUID == "BOT" ? 0 : GetCommunityID(player.GUID);


                    //player.IsFakePlayer = (bool)data["bot"];

                    var index = (int) data["index"];

                    parser.RawPlayers[index] = player;


                    break;
                case "player_disconnect":
                    data = MapData(eventDescriptor, rawEvent);

                    var disconnect = new PlayerDisconnectEventArgs
                    {
                        Player = parser.Players.ContainsKey((int) data["userid"])
                            ? parser.Players[(int) data["userid"]]
                            : null
                    };
                    parser.RaisePlayerDisconnect(disconnect);

                    var toDelete = (int) data["userid"];
                    for (var i = 0; i < parser.RawPlayers.Length; i++)
                        if (parser.RawPlayers[i] != null && parser.RawPlayers[i].UserID == toDelete)
                        {
                            parser.RawPlayers[i] = null;
                            break;
                        }

                    if (parser.Players.ContainsKey(toDelete)) parser.Players.Remove(toDelete);

                    break;

                case "player_team":
                    data = MapData(eventDescriptor, rawEvent);
                    var playerTeamEvent = new PlayerTeamEventArgs();

                    var t = Team.Spectate;

                    var team = (int) data["team"];

                    if (team == parser.tID)
                        t = Team.Terrorist;
                    else if (team == parser.ctID) t = Team.CounterTerrorist;

                    playerTeamEvent.NewTeam = t;

                    t = Team.Spectate;
                    team = (int) data["oldteam"];
                    if (team == parser.tID)
                        t = Team.Terrorist;
                    else if (team == parser.ctID) t = Team.CounterTerrorist;

                    playerTeamEvent.OldTeam = t;

                    playerTeamEvent.Swapped = parser.Players.ContainsKey((int) data["userid"])
                        ? parser.Players[(int) data["userid"]]
                        : null;
                    playerTeamEvent.IsBot = (bool) data["isbot"];
                    playerTeamEvent.Silent = (bool) data["silent"];

                    parser.RaisePlayerTeam(playerTeamEvent);
                    break;
                case "bomb_beginplant": //When the bomb is starting to get planted
                case "bomb_abortplant": //When the bomb planter stops planting the bomb
                case "bomb_planted": //When the bomb has been planted
                case "bomb_defused": //When the bomb has been defused
                case "bomb_exploded": //When the bomb has exploded
                    data = MapData(eventDescriptor, rawEvent);

                    var bombEventArgs = new BombEventArgs
                    {
                        Player = parser.Players.ContainsKey((int) data["userid"])
                            ? parser.Players[(int) data["userid"]]
                            : null
                    };

                    var site = (int) data["site"];

                    if (site == parser.bombsiteAIndex)
                    {
                        bombEventArgs.Site = 'A';
                    }
                    else if (site == parser.bombsiteBIndex)
                    {
                        bombEventArgs.Site = 'B';
                    }
                    else
                    {
                        var relevantTrigger = parser.triggers.Single(a => a.Index == site);
                        if (relevantTrigger.Contains(parser.bombsiteACenter))
                        {
                            //planted at A.
                            bombEventArgs.Site = 'A';
                            parser.bombsiteAIndex = site;
                        }
                        else if (relevantTrigger.Contains(parser.bombsiteBCenter))
                        {
                            //planted at B.
                            bombEventArgs.Site = 'B';
                            parser.bombsiteBIndex = site;
                        }
                        else
                        {
                            throw new InvalidDataException(
                                "Was the bomb planted at C? Neither A nor B is inside the bombsite");
                        }
                    }


                    switch (eventDescriptor.Name)
                    {
                        case "bomb_beginplant":
                            parser.RaiseBombBeginPlant(bombEventArgs);
                            break;
                        case "bomb_abortplant":
                            parser.RaiseBombAbortPlant(bombEventArgs);
                            break;
                        case "bomb_planted":
                            parser.RaiseBombPlanted(bombEventArgs);
                            break;
                        case "bomb_defused":
                            parser.RaiseBombDefused(bombEventArgs);
                            break;
                        case "bomb_exploded":
                            parser.RaiseBombExploded(bombEventArgs);
                            break;
                    }

                    break;
                case "bomb_begindefuse":
                    data = MapData(eventDescriptor, rawEvent);
                    var e = new BombDefuseEventArgs
                    {
                        Player = parser.Players.ContainsKey((int) data["userid"])
                            ? parser.Players[(int) data["userid"]]
                            : null,
                        HasKit = (bool) data["haskit"]
                    };
                    parser.RaiseBombBeginDefuse(e);
                    break;
                case "bomb_abortdefuse":
                    data = MapData(eventDescriptor, rawEvent);
                    var e2 = new BombDefuseEventArgs
                    {
                        Player = parser.Players.ContainsKey((int) data["userid"])
                            ? parser.Players[(int) data["userid"]]
                            : null
                    };
                    e2.HasKit = e2.Player.HasDefuseKit;
                    parser.RaiseBombAbortDefuse(e2);
                    break;
            }
        }

        private static T FillNadeEvent<T>(Dictionary<string, object> data, DemoParser parser)
            where T : NadeEventArgs, new()
        {
            var nade = new T();

            if (data.ContainsKey("userid") && parser.Players.ContainsKey((int) data["userid"]))
                nade.ThrownBy = parser.Players[(int) data["userid"]];

            var vec = new Vector
            {
                X = (float) data["x"],
                Y = (float) data["y"],
                Z = (float) data["z"]
            };
            nade.Position = vec;

            return nade;
        }

        private static Dictionary<string, object> MapData(GameEventList.Descriptor eventDescriptor, GameEvent rawEvent)
        {
            var data = new Dictionary<string, object>();

            for (var i = 0; i < eventDescriptor.Keys.Length; i++)
                data.Add(eventDescriptor.Keys[i].Name, rawEvent.Keys[i]);

            return data;
        }

        private static long GetCommunityID(string steamID)
        {
            var authServer = Convert.ToInt64(steamID.Substring(8, 1));
            var authID = Convert.ToInt64(steamID.Substring(10));
            return 76561197960265728 + authID * 2 + authServer;
        }
    }
}