using System.Collections.Generic;
using Newtonsoft.Json;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace UnboundMechanic
{
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
            get => _everyoneCanSpawnFromStart && _allowRestrictionlessSpawning;
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
            get => _allowRestrictionlessSpawning;
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
