using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ID;
using static CombatUtil.Common.CombatSystem;

namespace CombatUtil.Common
{
    public class CombatPlayer : ModPlayer
    {
        public int DPH = 0;
        public int[] DamageSample = new int[10];
        public int[] HitSample = new int[10];
        public int TDPS = 0;
        public int[] TDPSSample = new int[10];

        public int[] HPSample = new int[3];
        public int HPLoss = 0;
        
        public bool InBossFight = false;
        public bool RefreshHPLossInFight = true;

        public override void ResetEffects()
        {
            if (Configs.Refresh)
            {
                DamageSample = new int[Configs.Instance.DPHTime];
                HitSample = new int[Configs.Instance.DPHTime];
                TDPSSample = new int[Configs.Instance.TDPSTime];
                Configs.Refresh = false;
            }
            if (Utils.UpdateTimer % 60 == 0)
            {
                for (int i = DamageSample.Length - 1; i > 0; i--)
                {
                    DamageSample[i] = DamageSample[i - 1];
                }
                DamageSample[0] = 0;
                for (int i = HitSample.Length - 1; i > 0; i--)
                {
                    HitSample[i] = HitSample[i - 1];
                }
                HitSample[0] = 0;
                for (int i = TDPSSample.Length - 1; i > 0; i--)
                {
                    TDPSSample[i] = TDPSSample[i - 1];
                }
                TDPSSample[0] = 0;

                UpdateDPH();
                UpdateTDPS();
            }

            SampleHPAndUpdateLoss();

            InBossFight = IsInBossFight();
        }
        public override void PostUpdate()
        {
            if (InBossFight)
            {
                BossFight.UpdatePlayer(Player);
            }
            else
            {
                if (BossFight.Time > 0)
                {
                    BossFight.GetPlayerInfo(Player).HPLoss = HPLoss;
                    BossFight.DisplayInfo(Player);
                    BossFight.PopupRemarks(Player);
                }
                BossFight.Reset();
                BossFight.ResizeArena(Player);
                RefreshHPLossInFight = true;
            }
        }
        public override void OnHitNPC(Item item, NPC target, int damage, float knockback, bool crit)
        {
            HitSample[0] += 1;
            if (Utils.FindLastDamageCombatText() != -1)
            {
                CombatText ct = Main.combatText[Utils.FindLastDamageCombatText()];
                if (int.TryParse(ct.text, out int dmgDisplayed))
                {
                    DamageSample[0] += dmgDisplayed;
                    if (InBossFight)
                    {
                        BossFight.GetPlayerInfo(Player).DamageDealt += dmgDisplayed;
                    }
                }
            }
            UpdateDPH();
            UpdateTDPS();
            if (InBossFight)
            {
                BossFight.GetPlayerInfo(Player).HitDealt += 1;
                if (Utils.CountAsBoss(target))
                {
                    if (!BossFight.BossInfo.TryAdd(target.whoAmI, new BossStats()))
                    {
                        CombatNPC cn = target.GetGlobalNPC<CombatNPC>();
                        BossFight.GetBossInfo(target).LifeTime = cn.LifeTime;
                        BossFight.GetBossInfo(target).HPRemain = target.life;
                    }
                }
            }
        }
        public override void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockback, bool crit)
        {
            HitSample[0] += 1;
            if (Utils.FindLastDamageCombatText() != -1)
            {
                CombatText ct = Main.combatText[Utils.FindLastDamageCombatText()];
                if (int.TryParse(ct.text, out int dmgDisplayed))
                {
                    DamageSample[0] += dmgDisplayed;
                    if (InBossFight)
                    {
                        BossFight.GetPlayerInfo(Player).DamageDealt += dmgDisplayed;
                    }
                }
            }
            UpdateDPH();
            UpdateTDPS();
            if (InBossFight)
            {
                BossFight.GetPlayerInfo(Player).HitDealt += 1;
                if (Utils.CountAsBoss(target))
                {
                    if (!BossFight.BossInfo.TryAdd(target.whoAmI, new BossStats()))
                    {
                        CombatNPC cn = target.GetGlobalNPC<CombatNPC>();
                        BossFight.GetBossInfo(target).LifeTime = cn.LifeTime;
                        BossFight.GetBossInfo(target).HPRemain = target.life;
                    }
                }
            }
        }
        public override void OnHitPvp(Item item, Player target, int damage, bool crit)
        {
            HitSample[0] += 1;
            if (Utils.FindLastDamageCombatText(true) != -1)
            {
                CombatText ct = Main.combatText[Utils.FindLastDamageCombatText()];
                if (int.TryParse(ct.text, out int dmgDisplayed))
                {
                    DamageSample[0] += dmgDisplayed;
                }
            }
            UpdateDPH();
        }
        public override void OnHitPvpWithProj(Projectile proj, Player target, int damage, bool crit)
        {
            HitSample[0] += 1;
            if (Utils.FindLastDamageCombatText(true) != -1)
            {
                CombatText ct = Main.combatText[Utils.FindLastDamageCombatText()];
                if (int.TryParse(ct.text, out int dmgDisplayed))
                {
                    DamageSample[0] += dmgDisplayed;
                }
            }
            UpdateDPH();
            SampleHPAndUpdateLoss();
        }
        public override void PostHurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit, int cooldownCounter)
        {
            if (InBossFight)
            {
                BossFight.GetPlayerInfo(Player).HitTaken += 1;
            }
            SampleHPAndUpdateLoss();
        }
        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            bool otherPlayer = false;
            foreach (Player player in Main.player)
            {
                if (player.active && Player != player && !player.dead)
                {
                    otherPlayer = true;
                    break;
                }
            }
            if (!otherPlayer)
            {
                ClearHostiles();
            }
        }
        public override void UpdateDead()
        {
            InBossFight = IsInBossFight();
            if (InBossFight)
            {
                BossFight.UpdatePlayer(Player);
            }
            else
            {
                if (BossFight.Time > 0)
                {
                    BossFight.GetPlayerInfo(Player).HPLoss = HPLoss;
                    BossFight.DisplayInfo(Player);
                    BossFight.PopupRemarks(Player);
                }
                BossFight.Reset();
                RefreshHPLossInFight = true;
                BossFight.ResizeArena(Player);
            }
        }

        private void UpdateDPH()
        {
            DPH = DamageSample.Sum() == 0 ? 0 : (int)((float)DamageSample.Sum() / HitSample.Sum());
        }
        private void UpdateTDPS()
        {
            TDPS = TDPSSample.Sum() == 0 ? 0 : (int)((float)TDPSSample.Sum() / 120 / Configs.Instance.TDPSTime);
        }
        private void SampleHPAndUpdateLoss()
        {
            for (int i = HPSample.Length - 1; i > 0; i--)
            {
                HPSample[i] = HPSample[i - 1];
            }
            HPSample[0] = Player.statLife;
            HPLoss += Math.Max(HPSample[1] - HPSample[0], 0);
            HPLoss = Math.Max(HPLoss, 0);
        }
        private bool IsInBossFight()
        {
            foreach (NPC npc in Main.npc)
            {
                if (npc != null && npc.active && (Utils.CountAsBoss(npc) || npc.type == NPCID.EaterofWorldsTail))
                {
                    return true;
                }
            }
            return false;
        }
        private void ClearHostiles()
        {

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (!Main.npc[i].friendly)
                {
                    Main.npc[i].active = false;
                }
            }
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (!Main.projectile[i].hostile)
                {
                    Main.projectile[i].Kill();
                }
            }
            for (int i = 0; i < Main.maxItems; i++)
            {
                Main.item[i].active = false;
            }
        }
    }
}
