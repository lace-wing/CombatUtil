using CombatUtil.Common;
using Terraria.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CombatUtil.Content.InfoDisplays
{
    public class DPHDisplay : InfoDisplay
    {
        public override bool Active()
        {
            return Configs.Instance.DPHActive;
        }
        public override string DisplayValue()
        {
            int dph = 0;
            int hits = 0;
            if (Main.LocalPlayer.TryGetModPlayer(out CombatPlayer cp))
            {
                dph = cp.DPH;
                hits = cp.HitSample.Sum();
            }
            return Language.GetTextValue("Mods.CombatUtil.InfoDisplay.DPH", dph, hits, Configs.Instance.DPHTime);
        }
    }
}
