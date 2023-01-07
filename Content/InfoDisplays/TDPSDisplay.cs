using CombatUtil.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using Terraria.Localization;

namespace CombatUtil.Content.InfoDisplays
{
    public class TDPSDisplay : InfoDisplay
    {
        public override bool Active()
        {
            return Configs.Instance.TDPSActive;
        }
        public override string DisplayValue()
        {
            int TDPS = 0;
            if (Main.LocalPlayer.TryGetModPlayer(out CombatPlayer cp))
            {
                TDPS = cp.TDPS;
            }
            return Language.GetTextValue("Mods.CombatUtil.InfoDisplay.TDPS", TDPS, Configs.Instance.TDPSTime);
        }
    }
}
