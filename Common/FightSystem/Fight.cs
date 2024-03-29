﻿using System;
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
    /*
     * ActiveFight用于记录当前活跃的战斗
     * 包含参战者的详细信息
     * 玩家: 活跃时间, 帧率, 总伤, 命中数, 损血量, 受击数, 死亡数, 是否触发过无敌帧, 是否用过坐骑, 历史位置的左上右下极点
     * 计划: 主要伤害类型, 伤害模式 (爆发?刮痧?), 战斗风格 (贴脸?风筝?), 是否传送过 (不好弄)
     * 敌怪: 活跃时间, 当前血量
     * 它们都存在字典里, 用whoAmI作为键
     * 计划: 适配分节蠕虫, 用特定的数而不是whoAmI存储
     */
    public class PlayerStat
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

        public PlayerStat()
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
    public class EnemyStat
    {
        public int LifeTime;
        public int HPRemain;

        public EnemyStat()
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
        /// <param name="npc"></param>
        public void UpdateActive(NPC npc)
        {
            if (npc.active)
            {
                LifeTime = npc.GetGlobalNPC<CombatNPC>().LifeTime;
                HPRemain = npc.life;
            }
        }
    }
    public class ActiveFight
    {
        private const string FKey = "Mods.CombatUtil.FightDisplay.";
        private const string CKey = "Mods.CombatUtil.Common.";
        private string yes = Language.GetTextValue(CKey + "Yes");
        private string no = Language.GetTextValue(CKey + "No");

        public Dictionary<int, PlayerStat> PlayerStats;
        public Dictionary<int, EnemyStat> EnemyStats;
        public int Time;
        public bool Active;

        public ActiveFight()
        {
            PlayerStats = new Dictionary<int, PlayerStat>();
            EnemyStats = new Dictionary<int, EnemyStat>();
            Time = 0;
        }
        public void Reset()
        {
            PlayerStats.Clear();
            EnemyStats.Clear();
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
        public void ResetArena(Player player)
        {
            GetPlayerStat(player).TopLeft = player.position;
            GetPlayerStat(player).BottomRight = player.BottomRight;
        }
        public Player GetPlayer(int index)
        {
            if (PlayerStats.ContainsKey(index))
            {
                return Main.player[index];
            }
            else
            {
                return null;
            }
        }
        public PlayerStat GetPlayerStat(Player player)
        {
            if (PlayerStats.TryGetValue(player.whoAmI, out PlayerStat ps))
            {
                return ps;
            }
            else
            {
                return null;
            }
        }
        public NPC GetEnemy(int index)
        {
            if (EnemyStats.ContainsKey(index))
            {
                return Main.npc[index];
            }
            else
            {
                return null;
            }
        }
        public EnemyStat GetEnemyStat(NPC npc)
        {
            if (EnemyStats.TryGetValue(npc.whoAmI, out EnemyStat bs))
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
            if (!PlayerStats.TryAdd(player.whoAmI, new PlayerStat()))
            {
                GetPlayerStat(player).Update(player);
            }
        }
        public void UpdateEnemy(NPC npc)
        {
            if (!EnemyStats.TryAdd(npc.whoAmI, new EnemyStat()))
            {
                GetEnemyStat(npc).UpdateActive(npc);
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
            Utils.PrintText(FKey + "FightWith", new object[] { GetEnemyNames() });
            Main.NewText(GetRemarks(player));
            Utils.PrintText(FKey + "Time", new object[] { time, speed, avgFPS });
            Utils.PrintText(FKey + "Technics", new object[] { immuned, mounted });
            Utils.PrintText(FKey + "ArenaSize", new object[] { size.X, size.Y });
            Utils.PrintText(FKey + "Damage", new object[] { GetPlayerStat(player).DamageDealt, GetPlayerStat(player).HitDealt, GetDPS(player) });
            Utils.PrintText(FKey + "HPLoss", new object[] { GetPlayerStat(player).HPLoss, GetPlayerStat(player).HitTaken });
            Main.NewText(GetenemyStats());
        }
        public string GetEnemyNames()
        {
            string names, last = null;
            string[] rest = new string[] { };
            for (int i = 0; i < EnemyStats.Keys.Count; i++)
            {
                if (i > 0 && i == EnemyStats.Keys.Count - 1)
                {
                    last = $"[c/FF0000:{GetEnemy(i).GivenOrTypeName}]";
                }
                else
                {
                    rest.Append($"[c/FF0000:{GetEnemy(i).GivenOrTypeName}]");
                }
            }
            names = string.Join(", ", rest);
            if (last != null)
            {
                names = string.Join(Utils.SpaceText(Language.GetTextValue(CKey + "And")), names, last);
            }
            return names;
        }
        public string GetenemyStats()
        {
            string stats;
            string[] info = new string[] { };
            for (int i = 0; i < EnemyStats.Keys.Count; i++)
            {
                EnemyStat bs = EnemyStats[i];
                double lifeTime = bs.LifeTime == 0 ? 0 : Math.Round(bs.LifeTime / 60f, 2);
                int remain = Math.Max(bs.HPRemain, 0);
                double remainPct = remain == 0 ? 0 : Math.Round(100f * remain / GetEnemy(i).lifeMax, 1);
                info.Append(Language.GetTextValue(FKey + "EnemyInfo", GetEnemy(i).GivenOrTypeName, lifeTime, remain, remainPct));
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
                avgFPS = GetPlayerStat(player).TotalFPS == 0 ? 0 : (int)(2f * GetPlayerStat(player).TotalFPS / Time); // I donno why but it's half of the reasonable value so I mult 2f
                speed = avgFPS == 0 ? 0 : Math.Min(Math.Round(100f * avgFPS / 60, 0), 100);
                return true;
            }
        }
        public void GetTechicJudgements(Player player, out string immuned, out string mounted)
        {
            immuned = GetPlayerStat(player).Immuned ? yes : no;
            mounted = GetPlayerStat(player).Mounted ? yes : no;
        }
        public Vector2 GetArenaSize(Player player)
        {
            int width = (int)Math.Round((GetPlayerStat(player).BottomRight.X - GetPlayerStat(player).TopLeft.X) / 16, 0);
            int height = (int)Math.Round((GetPlayerStat(player).BottomRight.Y - GetPlayerStat(player).TopLeft.Y) / 16, 0);
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
                return Math.Round(60f * GetPlayerStat(player).DamageDealt / Time, 1);
            }
        }
        public double GetDPH(Player player)
        {
            if (GetPlayerStat(player).HitDealt <= 0)
            {
                return 0;
            }
            else
            {
                return Math.Round(60f * GetPlayerStat(player).DamageDealt / GetPlayerStat(player).HitDealt, 1);
            }
        }
        public string GetRemarks(Player player, bool bracketed = true, bool pink = true)
        {
            string[] remarkList = new string[] { };
            string remarks = "";
            if (GetPlayerStat(player).HitTaken <= 0)
            {
                remarkList.Append(Language.GetTextValue(FKey + "Nothit"));
            }
            if (GetPlayerStat(player).HPLoss <= 0)
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
            if (GetPlayerStat(player).HitTaken <= 0)
            {
                remarkList.Add(Language.GetTextValue(FKey + "NohitRemark"));
            }
            if (GetPlayerStat(player).HPLoss <= 0)
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
