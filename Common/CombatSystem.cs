using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CombatUtil.Common
{
    public class CombatSystem : ModSystem
    {
        /// <summary>
        /// The only active boss fight in the world
        /// </summary>
        public static FightInfo BossFight = new FightInfo();

        public override void PostUpdateNPCs()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.boss)
                {
                    BossFight.BossInfo
                }
            }
        }
        public override void PostUpdateEverything()
        {
            base.PostUpdateEverything();
        }
    }
}
