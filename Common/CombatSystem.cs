using CombatUtil.Common.FightSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.IO;

namespace CombatUtil.Common
{
    public class CombatSystem : ModSystem
    {
        /*
         * 负责记录当前Boss战
         * BossFight自身只在服务端更新
         * 各个玩家将他们的数据发到这里
         * 玩家部分的更新在ModPlayer进行
         * 各个NPC将他们的数据发到这里
         * NPC部分的更新在ModNPC进行
         */
        /// <summary>
        /// The only active boss fight in the world
        /// </summary>
        public static ActiveFight BossFight = new ActiveFight();
        public static bool IsFightingBoss = false;

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
                if (Main.projectile[i].hostile)
                {
                    Main.projectile[i].Kill();
                }
            }
            for (int i = 0; i < Main.maxItems; i++)
            {
                Main.item[i].active = false;
            }
        }

        public override void PreUpdatePlayers()
        {
            base.PreUpdatePlayers();
        }
        public override void PostUpdateEverything()
        {
            BossFight.Update();
        }
        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(Utils.ToJSON(BossFight));
        }
        public override void NetReceive(BinaryReader reader)
        {
            string bf = reader.ReadString();
            BossFight = Utils.FromJSON<ActiveFight>(bf);
        }
    }
}
