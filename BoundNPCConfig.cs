using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace UnboundMechanic
{
    public class BoundNPCConfig : ModConfig { // Vanilla bound NPCs
        public override ConfigScope Mode => ConfigScope.ServerSide;

        // The "AlwaysAvailable" configs will let that respective NPC spawn in immediately,
        // regardless of other conditions that normally have to be met
        // These should all default to false, and tell the player about sequence breaks with a tooltip

        // TODO: Check if there's a better/cleaner way of writing the presets
        #region Presets
        [Header("Presets")]
        public bool None {
            get { 
                return (
                    !EnableStylist &&
                    !EnableAngler &&
                    !EnableGolfer &&
                    !EnableGoblin &&
                    !GoblinAlwaysAvailable &&
                    !EnableMechanic &&
                    !MechanicAlwaysAvailable &&
                    !EnableTavernkeep &&
                    !TavernkeepAlwaysAvailable &&
                    !EnableWizard &&
                    !WizardAlwaysAvailable &&
                    !EnableTaxCollector &&
                    !TaxCollectorAlwaysAvailable
                );
            }
            set {
                if (value) {
                    // Disable everything
                    EnableStylist = false;
                    EnableAngler = false;
                    EnableGolfer = false;
                    EnableGoblin = false;
                    GoblinAlwaysAvailable = false;
                    EnableMechanic = false;
                    MechanicAlwaysAvailable = false;
                    EnableTavernkeep = false;
                    TavernkeepAlwaysAvailable = false;
                    EnableWizard = false;
                    WizardAlwaysAvailable = false;
                    EnableTaxCollector = false;
                    TaxCollectorAlwaysAvailable = false;
                }
            }
        }

        public bool OnlyUntie
        {
            get => EnableStylist && EnableAngler && EnableGolfer && EnableGoblin && EnableMechanic && EnableTavernkeep && EnableWizard && EnableTaxCollector
                && !GoblinAlwaysAvailable && !TavernkeepAlwaysAvailable && !MechanicAlwaysAvailable && !WizardAlwaysAvailable && !TaxCollectorAlwaysAvailable;
            set
            {
                if (value)
                {
                    // Enable unbinding bound NPCs
                    EnableStylist = true;
                    EnableAngler = true;
                    EnableGolfer = true;
                    EnableGoblin = true;
                    EnableMechanic = true;
                    EnableTavernkeep = true;
                    EnableWizard = true;
                    EnableTaxCollector = true;

                    // Set the instant availabilities to false
                    GoblinAlwaysAvailable = false;
                    TavernkeepAlwaysAvailable = false;
                    MechanicAlwaysAvailable = false;
                    WizardAlwaysAvailable = false;
                    TaxCollectorAlwaysAvailable = false;
                }
            }
        }

        public bool BoundAlwaysAvailable
        {
            get => EnableStylist && EnableAngler && EnableGolfer && EnableGoblin && EnableMechanic && EnableTavernkeep && EnableWizard && EnableTaxCollector
                && GoblinAlwaysAvailable && TavernkeepAlwaysAvailable && MechanicAlwaysAvailable && WizardAlwaysAvailable && TaxCollectorAlwaysAvailable;
            set
            {
                if (value)
                {
                    // Enable unbinding bound NPCs
                    EnableStylist = true;
                    EnableAngler = true;
                    EnableGolfer = true;
                    EnableGoblin = true;
                    EnableMechanic = true;
                    EnableTavernkeep = true;
                    EnableWizard = true;
                    EnableTaxCollector = true;

                    // Set the instant availabilities to true
                    GoblinAlwaysAvailable = true;
                    TavernkeepAlwaysAvailable = true;
                    MechanicAlwaysAvailable = true;
                    WizardAlwaysAvailable = true;
                    TaxCollectorAlwaysAvailable = true;
                }
            }
        }

        #endregion

        #region Bound NPCs

        [Header("Stylist")]
        public bool EnableStylist { get; set; }

        [Header("Angler")]
        public bool EnableAngler { get; set; }

        [Header("Golfer")]
        public bool EnableGolfer { get; set; }

        [Header("Goblin")]
        [DefaultValue(true)]
        public bool EnableGoblin { get; set; }
        public bool GoblinAlwaysAvailable { get; set; }

        [Header("Tavernkeep")]
        public bool EnableTavernkeep { get; set; }
        public bool TavernkeepAlwaysAvailable { get; set; }

        [Header("Mechanic")]
        [DefaultValue(true)]
        public bool EnableMechanic { get; set; }
        public bool MechanicAlwaysAvailable { get; set; }

        [Header("Wizard")]
        public bool EnableWizard { get; set; }
        public bool WizardAlwaysAvailable { get; set; }

        [Header("TaxCollector")]
        public bool EnableTaxCollector { get; set; }
        public bool TaxCollectorAlwaysAvailable { get; set; }

        #endregion
    }

    public class EveryOtherNPCConfig : ModConfig
    {
        // TODO: Revamp this class to be for restrictionless NPC spawning
        public override ConfigScope Mode => ConfigScope.ServerSide;

        // Attempt to update the list when the config changes
        public override void OnChanged() {
            if (!Main.gameMenu)
                ModContent.GetInstance<TownNPCSpawnSystem>().UpdateSpawnableTownNPCs();
            
            base.OnChanged();
        }

        private bool _everyoneCanSpawnFromStart;
        private bool _allowRestrictionlessSpawning;

        [Header("Cheats")]
        public bool EveryoneCanSpawnFromStart {
            get { return _everyoneCanSpawnFromStart && _allowRestrictionlessSpawning; }
            set {
                if (value) {
                    _everyoneCanSpawnFromStart = true;
                    _allowRestrictionlessSpawning = true;
                }
                else {
                    _everyoneCanSpawnFromStart = false;
                }
            }
        }

        public bool AllowRestrictionlessSpawning
        {
            get { return _allowRestrictionlessSpawning; }
            set {
                if (!value) {
                    _everyoneCanSpawnFromStart = false;
                    _allowRestrictionlessSpawning = false;
                }
                else {
                    _allowRestrictionlessSpawning = true;
                }
            }
        }

        #region Specific NPCs
        
        [Header("NPCList")]
        [JsonIgnore]
        [ShowDespiteJsonIgnore]
        public bool SampleText;

        public List<NPCDefinition> NPCList = new() {
            // PreHardmode
            new NPCDefinition(NPCID.Guide),
            new NPCDefinition(NPCID.Merchant),
            new NPCDefinition(NPCID.Nurse),
            new NPCDefinition(NPCID.Demolitionist),
            new NPCDefinition(NPCID.DyeTrader),
            new NPCDefinition(NPCID.Angler),
            new NPCDefinition(NPCID.BestiaryGirl), // Zoologist
            new NPCDefinition(NPCID.Dryad),
            new NPCDefinition(NPCID.Painter),
            new NPCDefinition(NPCID.Golfer),
            new NPCDefinition(NPCID.ArmsDealer),
            new NPCDefinition(NPCID.DD2Bartender), // Tavernkeep
            new NPCDefinition(NPCID.Stylist),
            new NPCDefinition(NPCID.GoblinTinkerer),
            new NPCDefinition(NPCID.WitchDoctor),
            new NPCDefinition(NPCID.Clothier),
            new NPCDefinition(NPCID.Mechanic),
            new NPCDefinition(NPCID.PartyGirl),

            // Hardmode
            new NPCDefinition(NPCID.Wizard),
            new NPCDefinition(NPCID.TaxCollector),
            new NPCDefinition(NPCID.Truffle),
            new NPCDefinition(NPCID.Pirate),
            new NPCDefinition(NPCID.Steampunker),
            new NPCDefinition(NPCID.Cyborg),
            new NPCDefinition(NPCID.SantaClaus),
            new NPCDefinition(NPCID.Princess),

            // Town Slimes + Animals
            new NPCDefinition(NPCID.TownSlimeBlue),
            new NPCDefinition(NPCID.TownSlimeGreen),
            new NPCDefinition(NPCID.TownSlimeOld),
            new NPCDefinition(NPCID.TownSlimePurple),
            new NPCDefinition(NPCID.TownSlimeRainbow),
            new NPCDefinition(NPCID.TownSlimeRed),
            new NPCDefinition(NPCID.TownSlimeYellow),
            new NPCDefinition(NPCID.TownSlimeCopper),
            new NPCDefinition(NPCID.TownBunny),
            new NPCDefinition(NPCID.TownCat),
            new NPCDefinition(NPCID.TownDog),
        };
        #endregion
    }
}
