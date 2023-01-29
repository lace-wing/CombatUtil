using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Humanizer;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.Localization;

namespace CombatUtil.Common.FightSystem
{
    public class PlayerStats
    {
        public int ActiveTime;
        public int TotalFPS;
        public int DamageDealt;
        public int HitDealt;
        public int HPLoss;
        public int HitTaken;
        public int DeathCount;
        public bool Immuned;
        public bool Mounted;
        public bool TPed;
        public Vector2 TopLeft;
        public Vector2 BottomRight;

        public PlayerStats()
        {
            ActiveTime = 0;
            TotalFPS = 0;
            DamageDealt = 0;
            HitDealt = 0;
            HPLoss = 0;
            HitTaken = 0;
            DeathCount = 0;
            Immuned = false;
            Mounted = false;
            TPed = false;
            TopLeft = new Vector2();
            BottomRight = new Vector2();
        }
        public void Reset()
        {
            ActiveTime = 0;
            TotalFPS = 0;
            DamageDealt = 0;
            HitDealt = 0;
            HPLoss = 0;
            HitTaken = 0;
            DeathCount = 0;
            Immuned = false;
            Mounted = false;
            TPed = false;
            TopLeft = new Vector2();
            BottomRight = new Vector2();
        }
        public void Update(Player player)
        {
            ActiveTime = Math.Max(++ActiveTime, 0);
            TotalFPS = Math.Max(TotalFPS + Main.fpsCount, 0);
            if (player.position.X < TopLeft.X)
            {
                TopLeft.X = player.position.X;
            }
            if (player.position.Y < TopLeft.Y)
            {
                TopLeft.Y = player.position.Y;
            }
            if (player.BottomRight.X > BottomRight.X)
            {
                BottomRight.X = player.BottomRight.X;
            }
            if (player.BottomRight.Y > BottomRight.Y)
            {
                BottomRight.Y = player.BottomRight.Y;
            }
            if (player.immune && player.immuneTime > 0)
            {
                Immuned = true;
            }
            if (player.mount.Active)
            {
                Mounted = true;
            }
            if (player.TryGetModPlayer(out CombatPlayer cp))
            {
                HPLoss = cp.HPLoss;
                if (cp.RefreshHPLossInFight)
                {
                    cp.HPLoss = 0;
                    cp.RefreshHPLossInFight = false;
                }
            }
        }
    }
    public class BossStats
    {
        public int LifeTime;
        public int HPRemain;

        public BossStats()
        {
            LifeTime = 0;
            HPRemain = 0;
        }
        public void Reset()
        {
            LifeTime = 0;
            HPRemain = 0;
        }
        /// <summary>
        /// Call this when the boss is active
        /// </summary>
        /// <param name="boss"></param>
        public void UpdateActive(NPC boss)
        {
            if (boss.active)
            {
                LifeTime = boss.GetGlobalNPC<CombatNPC>().LifeTime;
                HPRemain = boss.life;
            }
        }
    }
    public class FightInfo
    {
        private const string FKey = "Mods.CombatUtil.FightDisplay.";
        private const string CKey = "Mods.CombatUtil.Common.";
        private string yes = Language.GetTextValue(CKey + "Yes");
        private string no = Language.GetTextValue(CKey + "No");

        public Dictionary<int, PlayerStats> PlayerInfo;
        public Dictionary<int, BossStats> BossInfo;
        public int Time;
        public bool Active;

        public FightInfo()
        {
            PlayerInfo = new Dictionary<int, PlayerStats>();
            BossInfo = new Dictionary<int, BossStats>();
            Time = 0;
        }
        public void Reset()
        {
            PlayerInfo.Clear();
            BossInfo.Clear();
            Time = 0;
            Active = false;
        }
        public void Update()
        {
            if (Active)
            {
                Time = Math.Max(++Time, 0);
            }
            else
            {
                Reset();
            }
        }
        public void ResizeArena(Player player)
        {
            GetPlayerInfo(player).TopLeft = player.position;
            GetPlayerInfo(player).BottomRight = player.BottomRight;
        }
        public Player GetPlayer(int index)
        {
            if (PlayerInfo.ContainsKey(index))
            {
                return Main.player[index];
            }
            else
            {
                return null;
            }
        }
        public PlayerStats GetPlayerInfo(Player player)
        {
            if (PlayerInfo.TryGetValue(player.whoAmI, out PlayerStats ps))
            {
                return ps;
            }
            else
            {
                return null;
            }
        }
        public NPC GetBoss(int index)
        {
            if (BossInfo.ContainsKey(index))
            {
                return Main.npc[index];
            }
            else
            {
                return null;
            }
        }
        public BossStats GetBossInfo(NPC boss)
        {
            if (BossInfo.TryGetValue(boss.whoAmI, out BossStats bs))
            {
                return bs;
            }
            else
            {
                return null;
            }
        }
        public void UpdatePlayer(Player player)
        {
            if (!PlayerInfo.TryAdd(player.whoAmI, new PlayerStats()))
            {
                GetPlayerInfo(player).Update(player);
            }
        }
        public void UpdateBoss(NPC boss)
        {
            if (!BossInfo.TryAdd(boss.whoAmI, new BossStats()))
            {
                GetBossInfo(boss).UpdateActive(boss);
            }
        }
        public void DisplayInfo(Player player)
        {
            // Get fight time
            TryGetFightTime(player, out double time, out int avgFPS, out double speed);
            // Get technics
            GetTechicJudgements(player, out string immuned, out string mounted);
            // Get arena size
            Vector2 size = GetArenaSize(player);

            Utils.PrintText(FKey + "Summary", color: Color.Yellow);
            Utils.PrintText(FKey + "FightWith", new object[] { GetBossNames() });
            Main.NewText(GetRemarks(player));
            Utils.PrintText(FKey + "Time", new object[] { time, speed, avgFPS });
            Utils.PrintText(FKey + "Technics", new object[] { immuned, mounted });
            Utils.PrintText(FKey + "ArenaSize", new object[] { size.X, size.Y });
            Utils.PrintText(FKey + "Damage", new object[] { GetPlayerInfo(player).DamageDealt, GetPlayerInfo(player).HitDealt, GetDPS(player) });
            Utils.PrintText(FKey + "HPLoss", new object[] { GetPlayerInfo(player).HPLoss, GetPlayerInfo(player).HitTaken });
            Main.NewText(GetBossStats());
        }
        public string GetBossNames()
        {
            string names, last = null;
            string[] rest = new string[] { };
            for (int i = 0; i < BossInfo.Keys.Count; i++)
            {
                if (i > 0 && i == BossInfo.Keys.Count - 1)
                {
                    last = $"[c/FF0000:{GetBoss(i).GivenOrTypeName}]";
                }
                else
                {
                    rest.Append($"[c/FF0000:{GetBoss(i).GivenOrTypeName}]");
                }
            }
            names = string.Join(", ", rest);
            if (last != null)
            {
                names = string.Join(Utils.SpaceText(Language.GetTextValue(CKey + "And")), names, last);
            }
            return names;
        }
        public string GetBossStats()
        {
            string stats;
            string[] info = new string[] { };
            for (int i = 0; i < BossInfo.Keys.Count; i++)
            {
                BossStats bs = BossInfo[i];
                double lifeTime = bs.LifeTime == 0 ? 0 : Math.Round(bs.LifeTime / 60f, 2);
                int remain = Math.Max(bs.HPRemain, 0);
                double remainPct = remain == 0 ? 0 : Math.Round(100f * remain / GetBoss(i).lifeMax, 1);
                info.Append(Language.GetTextValue(FKey + "BossInfo", GetBoss(i).GivenOrTypeName, lifeTime, remain, remainPct));
            }
            stats = string.Join("\n", info);
            return stats;
        }
        public bool TryGetFightTime(Player player, out double time, out int avgFPS, out double speed)
        {
            if (Time <= 0)
            {
                time = 0;
                avgFPS = 0;
                speed = 0;
                return false;
            }
            else
            {
                time = Math.Round(Time / 60f, 2);
                avgFPS = GetPlayerInfo(player).TotalFPS == 0 ? 0 : (int)(2f * GetPlayerInfo(player).TotalFPS / Time); // I donno why but it's half of the reasonable value so I mult 2f
                speed = avgFPS == 0 ? 0 : Math.Min(Math.Round(100f * avgFPS / 60, 0), 100);
                return true;
            }
        }
        public void GetTechicJudgements(Player player, out string immuned, out string mounted)
        {
            immuned = GetPlayerInfo(player).Immuned ? yes : no;
            mounted = GetPlayerInfo(player).Mounted ? yes : no;
        }
        public Vector2 GetArenaSize(Player player)
        {
            int width = (int)Math.Round((GetPlayerInfo(player).BottomRight.X - GetPlayerInfo(player).TopLeft.X) / 16, 0);
            int height = (int)Math.Round((GetPlayerInfo(player).BottomRight.Y - GetPlayerInfo(player).TopLeft.Y) / 16, 0);
            return new Vector2(width, height);
        }
        public double GetDPS(Player player)
        {
            if (Time <= 0)
            {
                return 0;
            }
            else
            {
                return Math.Round(60f * GetPlayerInfo(player).DamageDealt / Time, 1);
            }
        }
        public double GetDPH(Player player)
        {
            if (GetPlayerInfo(player).HitDealt <= 0)
            {
                return 0;
            }
            else
            {
                return Math.Round(60f * GetPlayerInfo(player).DamageDealt / GetPlayerInfo(player).HitDealt, 1);
            }
        }
        public string GetRemarks(Player player, bool bracketed = true, bool pink = true)
        {
            string[] remarkList = new string[] { };
            string remarks = "";
            if (GetPlayerInfo(player).HitTaken <= 0)
            {
                remarkList.Append(Language.GetTextValue(FKey + "Nothit"));
            }
            if (GetPlayerInfo(player).HPLoss <= 0)
            {
                remarkList.Append(Language.GetTextValue(FKey + "Nodamage"));
            }
            if (remarkList.Length > 0)
            {
                remarks = string.Join(", ", remarkList);
                if (bracketed)
                {
                    remarks = string.Join("", new object[] { " (", remarks, ") " });
                }
                if (pink)
                {
                    remarks = string.Join("", new object[] { "[c/FFC0CB:", remarks, "]" });
                }
            }
            return remarks;
        }
        public void PopupRemarks(Player player)
        {
            // Remarks
            List<string> remarkList = new List<string>();
            string remarks = "";
            if (GetPlayerInfo(player).HitTaken <= 0)
            {
                remarkList.Add(Language.GetTextValue(FKey + "NohitRemark"));
            }
            if (GetPlayerInfo(player).HPLoss <= 0)
            {
                remarkList.Add(Language.GetTextValue(FKey + "NodamageRemark"));
            }
            for (int i = 0; i < remarkList.Count; i++)
            {
                if (i > 0)
                {
                    remarks += " ";
                }
                remarks += remarkList[i];
            }
            if (remarkList.Count > 0)
            {
                int cti = CombatText.NewText(player.Hitbox, Color.Pink, remarks, true);
                if (cti < Main.maxCombatText)
                {
                    Main.combatText[cti].lifeTime = 360;
                }
            }
        }
    }
}
