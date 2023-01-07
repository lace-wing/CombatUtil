using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.Config;

namespace CombatUtil.Common
{
    [Label("$Mods.CombatUtil.Config.Label")]
    public class Configs : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;
        public static Configs Instance = ModContent.GetInstance<Configs>();
        public static bool Refresh = false;

        private const string Key = "$Mods.CombatUtil.Config.";

        [Header(Key + "Header")]

        [Label(Key + "DPH.Label")]
        [Tooltip(Key + "DPH.Tooltip")]
        [DefaultValue(true)]
        public bool DPHActive;

        [Label(Key + "DPHTime.Label")]
        [Tooltip(Key + "DPHTime.Tooltip")]
        [Range(1, 180)]
        [DefaultValue(10)]
        public int DPHTime;

        [Label(Key + "TDPS.Label")]
        [Tooltip(Key + "TDPS.Tooltip")]
        [DefaultValue(true)]
        public bool TDPSActive;

        [Label(Key + "TDPSTime.Label")]
        [Tooltip(Key + "TDPSTime.Tooltip")]
        [Range(1, 180)]
        [DefaultValue(10)]
        public int TDPSTime;

        public override void OnChanged()
        {
            Refresh = true;
        }
    }
}
