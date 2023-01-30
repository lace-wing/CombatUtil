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

        public static int EoWHead = -1;

        public override void ResetEffects(NPC npc)
        {
            LifeTime = Math.Max(++LifeTime, 0);
        }
        public override void PostAI(NPC npc)
        {
            if (npc.type != NPCID.MartianSaucerCore && npc.boss)
            {
                CombatSystem.BossFight.UpdateEnemy(npc);
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
        }
    }
}
