using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;

namespace CombatUtil.Common
{
    public class CombatNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public int LifeTime = 0;

        public static NPC EoWHead = new NPC();

        public override void Load()
        {
            EoWHead.SetDefaults(NPCID.EaterofWorldsHead);
        }
        public override void ResetEffects(NPC npc)
        {
            LifeTime = Math.Max(++LifeTime, 0);
        }
        public override void PostAI(NPC npc)
        {
            if (npc.active)
            {
                if (npc.type != NPCID.MartianSaucerCore && npc.boss)
                {
                    for (int i = 0; i < Main.player.Length; i++)
                    {
                        Player player = Main.player[i];
                        if (player != null && player.TryGetModPlayer(out CombatPlayer cp))
                        {
                            if (!cp.BossFight.BossInfo.TryAdd(npc, new BossStats()))
                            {
                                cp.BossFight.SetBossInfo(npc, LifeTime, npc.life);
                            }
                        }
                    }
                }
            }

            if (npc.lifeRegen < 0)
            {
                for (int i = 0; i < Main.player.Length; i++)
                {
                    Player player = Main.player[i];
                    if (player != null && player.TryGetModPlayer(out CombatPlayer cp))
                    {
                        cp.TDPSSample[0] += -npc.lifeRegen;
                    }
                }
            }
        }
        public override void OnKill(NPC npc)
        {
            if (Utils.CountAsBoss(npc))
            {
                for (int i = 0; i < Main.player.Length; i++)
                {
                    Player player = Main.player[i];
                    if (player != null && player.TryGetModPlayer(out CombatPlayer cp))
                    {
                        if (!cp.BossFight.BossInfo.TryAdd(npc, new BossStats()))
                        {
                            if (cp.BossFight.BossInfo.TryGetValue(npc, out BossStats status))
                            {
                                status.HPRemain = 0;
                            }
                        }
                    }
                }
            }
        }
    }
}
