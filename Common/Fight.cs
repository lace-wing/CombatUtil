using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Humanizer;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.Localization;

namespace CombatUtil.Common
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
        public void UpdateLive(NPC boss)
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
        public int Damage;

        public FightInfo()
        {
            PlayerInfo = new Dictionary<int, PlayerStats>();
            BossInfo = new Dictionary<int, BossStats>();
            Time = 0;
            Damage = 0;
        }
        public void Reset()
        {
            PlayerInfo.Clear();
            BossInfo.Clear();
            Time = 0;
            Damage = 0;
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
        public void Update(Player player)
        {
            Time = Math.Max(++Time, 0);
            PlayerStats ps = GetPlayerInfo(player);
            ps.TotalFPS = Math.Max(ps.TotalFPS + Main.fpsCount, 0);
            if (player.position.X < ps.TopLeft.X)
            {
                ps.TopLeft.X = player.position.X;
            }
            if (player.position.Y < ps.TopLeft.Y)
            {
                ps.TopLeft.Y = player.position.Y;
            }
            if (player.BottomRight.X > ps.BottomRight.X)
            {
                ps.BottomRight.X = player.BottomRight.X;
            }
            if (player.BottomRight.Y > ps.BottomRight.Y)
            {
                ps.BottomRight.Y = player.BottomRight.Y;
            }
            if (player.mount.Active)
            {
                ps.Mounted = true;
            }
            if (player.immune && player.immuneTime > 0)
            {
                ps.Immuned = true;
            }
            if (player.TryGetModPlayer(out CombatPlayer cp))
            {
                if (cp.RefreshHPLossInFight)
                {
                    cp.HPLoss = 0;
                    cp.RefreshHPLossInFight = false;
                }
            }
        }
        public void Update(NPC boss)
        {
            GetBossInfo(boss).HPRemain = boss.life;
            if (boss.TryGetGlobalNPC(out CombatNPC cn))
            {
                GetBossInfo(boss).LifeTime = cn.LifeTime;
            }
        }
        public void DisplayInfo(Player player)
        {
            // Fight time
            double time = Time == 0 ? 0 : Math.Round(Time / 60f, 2);
            int avgFPS = GetPlayerInfo(player).TotalFPS == 0 ? 0 : (int)(2f * GetPlayerInfo(player).TotalFPS / Time); // I donno why but it's half of the reasonable value so I mult 2f
            double speedPct = avgFPS == 0 ? 0 : Math.Min(Math.Round(100f * avgFPS / 60, 0), 100);
            // Technics
            string immuned = GetPlayerInfo(player).Immuned ? yes : no;
            string mounted = GetPlayerInfo(player).Mounted ? yes : no;
            // Arena size
            int width = (int)Math.Round((GetPlayerInfo(player).BottomRight.X - GetPlayerInfo(player).TopLeft.X) / 16, 0);
            int height = (int)Math.Round((GetPlayerInfo(player).BottomRight.Y - GetPlayerInfo(player).TopLeft.Y) / 16, 0);
            double dps = Math.Round(60f * Damage / Time, 1);
            // HP loss and remarks
            List<string> remarkList = new List<string>();
            string hpRemarks = "";
            if (GetPlayerInfo(player).HitTaken <= 0)
            {
                remarkList.Add(Language.GetTextValue(FKey + "Nothit"));
            }
            if (GetPlayerInfo(player).HPLoss <= 0)
            {
                remarkList.Add(Language.GetTextValue(FKey + "Nodamage"));
            }
            if (remarkList.Count > 0)
            {
                hpRemarks += "[c/FFC0CB: (";
                for (int i = 0; i < remarkList.Count; i++)
                {
                    if (i > 0)
                    {
                        hpRemarks += ", ";
                    }
                    hpRemarks += remarkList[i];
                }
                hpRemarks += ")]";
            }

            Utils.PrintText(FKey + "Summary", color: Color.Yellow);
            Utils.PrintText(FKey + "FightWith", new object[] { BossNames() });
            Utils.PrintText(FKey + "Time", new object[] { time, speedPct, avgFPS });
            Utils.PrintText(FKey + "Technics", new object[] { immuned, mounted });
            Utils.PrintText(FKey + "ArenaSize", new object[] { width, height });
            Utils.PrintText(FKey + "Damage", new object[] { Damage, GetPlayerInfo(player).HitDealt, dps });
            Utils.PrintText(FKey + "HPLoss", new object[] { GetPlayerInfo(player).HPLoss, GetPlayerInfo(player).HitTaken, hpRemarks });
            Main.NewText(BossStatList());
        }
        public string BossNames()
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
        public string BossStatList()
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
                    Main.combatText[cti].scale = 6;
                }
            }
        }
    }
}
