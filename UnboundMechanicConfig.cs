using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace UnboundMechanic
{
    public class UnboundMechanicConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        [Header("UnboundMechanic.Configs.Headers.Mechanic")]
        [DefaultValue(true)]
        public bool EnableMechanic;

        // The "AlwaysAvailable" configs will let that respective NPC spawn in immediately,
        // regardless of other conditions that normally have to be met
        // These should all default to false, and tell the player with a tooltip
        [DefaultValue(false)]
        public bool MechanicAlwaysAvailable;

        [Header("UnboundMechanic.Configs.Headers.Goblin")]
        [DefaultValue(true)]
        public bool EnableGoblin;

        [DefaultValue(false)]
        public bool GoblinAlwaysAvailable;

        [Header("UnboundMechanic.Configs.Headers.Stylist")]
        public bool EnableStylist;

        [Header("UnboundMechanic.Configs.Headers.Wizard")]
        public bool EnableWizard;

        public bool WizardAlwaysAvailablePreHardmode;

        [Header("UnboundMechanic.Configs.Headers.Tavernkeep")]
        public bool EnableTavernkeep;

        public bool TavernkeepAlwaysAvailable;

        [Header("UnboundMechanic.Configs.Headers.Angler")]
        public bool EnableAngler;

        [Header("UnboundMechanic.Configs.Headers.Golfer")]
        public bool EnableGolfer;

        // Town slimes. These options are a little more cheaty, make sure to mention in localization tooltips
        [Header("UnboundMechanic.Configs.Headers.TownSlimes")]
        [DefaultValue(false)]
        public bool EnableTownSlimeOld;

        [DefaultValue(true)]
        public bool ConsumeGoldKeyToFree;

        [DefaultValue(false)]
        public bool OldSlimeAlwaysAvailable;

        public bool EnableTownSlimeYellow;

        public bool EnableTownSlimePurple;
    }
}
