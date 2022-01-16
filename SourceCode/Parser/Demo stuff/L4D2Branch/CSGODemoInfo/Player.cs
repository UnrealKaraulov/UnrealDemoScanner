using System.Collections.Generic;
using DemoScanner.DemoStuff.L4D2Branch.CSGODemoInfo.DP;

namespace DemoScanner.DemoStuff.L4D2Branch.CSGODemoInfo
{
    public class Player
    {
        internal int ActiveWeaponID;
        internal int[] AmmoLeft = new int[32];
        internal Entity Entity;
        internal Dictionary<int, Equipment> rawWeapons = new Dictionary<int, Equipment>();
        internal int TeamID;

        public Player()
        {
            Velocity = new Vector();
            LastAlivePosition = new Vector();
        }

        public string Name { get; set; }
        public long SteamID { get; set; }
        public Vector Position { get; set; }
        public int EntityID { get; set; }
        public int HP { get; set; }
        public int Armor { get; set; }
        public Vector LastAlivePosition { get; set; }
        public Vector Velocity { get; set; }
        public float ViewDirectionX { get; set; }
        public float ViewDirectionY { get; set; }
        public int Money { get; set; }
        public int CurrentEquipmentValue { get; set; }
        public int FreezetimeEndEquipmentValue { get; set; }
        public int RoundStartEquipmentValue { get; set; }
        public bool IsDucking { get; set; }
        public bool Disconnected { get; set; }

        public Equipment ActiveWeapon
        {
            get
            {
                if (ActiveWeaponID == DemoParser.INDEX_MASK) return null;

                return rawWeapons[ActiveWeaponID];
            }
        }

        public IEnumerable<Equipment> Weapons => rawWeapons.Values;

        public bool IsAlive => HP > 0;

        public Team Team { get; set; }
        public bool HasDefuseKit { get; set; }
        public bool HasHelmet { get; set; }
        public AdditionalPlayerInformation AdditionaInformations { get; internal set; }

        /// <summary>
        ///     Copy this instance for multi-threading use.
        /// </summary>
        public Player Copy()
        {
            var me = new Player
            {
                EntityID = -1, //this should bot be copied
                Entity = null,

                Name = Name,
                SteamID = SteamID,
                HP = HP,
                Armor = Armor,

                ViewDirectionX = ViewDirectionX,
                ViewDirectionY = ViewDirectionY,
                Disconnected = Disconnected,

                Team = Team,

                HasDefuseKit = HasDefuseKit,
                HasHelmet = HasHelmet
            };

            if (Position != null)
                me.Position = Position.Copy(); //Vector is a class, not a struct - thus we need to make it thread-safe. 

            if (LastAlivePosition != null) me.LastAlivePosition = LastAlivePosition.Copy();

            if (Velocity != null) me.Velocity = Velocity.Copy();

            return me;
        }
    }

    public enum Team
    {
        Spectate = 1,
        Terrorist = 2,
        CounterTerrorist = 3
    }
}