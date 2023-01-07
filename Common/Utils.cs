using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Localization;
using Terraria.ID;

namespace CombatUtil.Common
{
    public class Utils : ModSystem
    {
        public static int UpdateTimer { get; private set; }

        public override void PostUpdateEverything()
        {
            UpdateTimer = Math.Max(++UpdateTimer, 0);
        }

        public static int FindLastDamageCombatText(bool friendly = false)
        {
            int lastCombatText = -1;
            for (int i = Main.maxCombatText - 1; i >= 0; i--)
            {
                CombatText ct = Main.combatText[i];
                if (ct.lifeTime == 60 || ct.lifeTime == 120)
                {
                    if (ct.alpha == 1)
                    {
                        if (friendly ? (ct.color == CombatText.DamagedFriendly || ct.color == CombatText.DamagedFriendlyCrit) : (ct.color == CombatText.DamagedHostile || ct.color == CombatText.DamagedHostileCrit))
                        {
                            lastCombatText = i;
                            break;
                        }
                    }
                }
            }
            return lastCombatText;
        }
        public static void PrintText(string key, object[] args = null, Color? color = null)
        {
            if (args == null)
            {
                Main.NewText(Language.GetTextValue(key), color);
            }
            else
            {
                Main.NewText(Language.GetTextValue(key, args), color);
            }
        }
        /// <summary>
        /// Add a certain amount of whitespace(s) before and/or after the text
        /// </summary>
        /// <param name="text"></param>
        /// <param name="pre">number of whitespace to add before the text</param>
        /// <param name="suf">number of whitespace to add after the text</param>
        /// <returns></returns>
        public static string SpaceText(string text, int pre = 1, int suf = 1)
        {
            string space = " ";
            string[] prefix = new string[] { }, suffix = new string[] { };
            for (int i = 0; i < pre; i++)
            {
                prefix.Append(space);
            }
            for (int i = 0; i < suf; i++)
            {
                suffix.Append(space);
            }
            return string.Format("{0}{1}{2}", string.Join("", prefix), text, string.Join("", suffix));
        }
        public static bool CountAsBoss(NPC npc, bool fuckEoW = false)
        {
            return npc.type != NPCID.MartianSaucerCore && (npc.boss || (!fuckEoW && npc.type == NPCID.EaterofWorldsHead));
        }
    }
}
