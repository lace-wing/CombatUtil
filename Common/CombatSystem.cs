using CombatUtil.Common.FightSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.IO;

namespace CombatUtil.Common
{
    public class CombatSystem : ModSystem
    {
        /// <summary>
        /// The only active boss fight in the world
        /// </summary>
        public static FightInfo BossFight = new FightInfo();

        public override void PostUpdateEverything()
        {
            BossFight.Update();
        }
        public override void PostUpdateNPCs()
        {
        }
    }
}
