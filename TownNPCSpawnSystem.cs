using System;
using System.Collections.Generic;
using MonoMod.Cil;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace UnboundMechanic
{
    public class TownNPCSpawnSystem : ModSystem
    {
        public override void Load() {
            IL_NPC.AI_007_TownEntities += HookPermanentSanta;
            On_WorldGen.CheckSpecialTownNPCSpawningConditions += On_CheckSpecialTownNPCSpawningConditions;
            base.Load();
        }

        private static void HookPermanentSanta(ILContext il) {
            try {
                ILCursor c = new ILCursor(il);
                
                c.GotoNext(i => i.MatchLdcI4(NPCID.SantaClaus));

                c.Index++;

                // If santa spawning is enabled, return a value of true (1) to stop him being killed
                c.EmitDelegate<Func<int, int>>((returnValue) => {
                    if (ModContent.GetInstance<EveryOtherNPCConfig>().AllowRestrictionlessSpawning &&
                        SpawnableTownNPCS.Exists(npc => npc.type == NPCID.SantaClaus))
                        return 1;

                    return returnValue;
                });
            }
            catch (Exception e) {
                MonoModHooks.DumpIL(ModContent.GetInstance<UnboundMechanic>(), il);
                throw new ILPatchFailureException(ModContent.GetInstance<UnboundMechanic>(), il, e);
            }
        }

        private static bool On_CheckSpecialTownNPCSpawningConditions(
            On_WorldGen.orig_CheckSpecialTownNPCSpawningConditions orig, int type) {
            if (type == NPCID.Truffle && SpawnableTownNPCS.Exists(npc => npc.type == NPCID.Truffle)) return true;

            bool canSpawn = orig(type);
            return canSpawn;
        }

        public int timer;

        // TODO: Update this list on config change, not just world load
        private static List<NPC> AllTownNPCS = new();
        public static List<NPC> SpawnableTownNPCS = new();

        public override void OnWorldLoad() {
            var otherNPCConfig = ModContent.GetInstance<EveryOtherNPCConfig>();
            
            // Add all town NPCs to a reference list, skip Travelling Merchant
            // TODO: Check that skeleton merchant dosen't do the same multiple spawning thing on secret seeds
            if (otherNPCConfig.AllowRestrictionlessSpawning) {
                foreach (NPC npc in ContentSamples.NpcsByNetId.Values) {
                    if (npc.type == NPCID.TravellingMerchant)
                        continue;
                    if (npc.townNPC && NPC.TypeToDefaultHeadIndex(npc.type) >= 0)
                        AllTownNPCS.Add(npc);
                }
            }

            if (!otherNPCConfig.EveryoneCanSpawnFromStart && otherNPCConfig.AllowRestrictionlessSpawning)
            {
                UpdateSpawnableTownNPCs();
            }
        }

        // Prune unloaded/disabled NPCs from the reference list
        public void UpdateSpawnableTownNPCs() {
            List<int> types = new();
            List<NPC> townNPCsTemp = new();

            foreach (NPCDefinition npcDef in ModContent.GetInstance<EveryOtherNPCConfig>().NPCList) {
                if (!npcDef.IsUnloaded)
                    types.Add(npcDef.Type);
            }

            foreach (NPC npc in AllTownNPCS) {
                if (types.Contains(npc.type))
                    townNPCsTemp.Add(npc);
            }
            SpawnableTownNPCS = townNPCsTemp;
        }

        public override void PostUpdateNPCs()
        {
            // Hopefully avoid breaking census for multiplayer clients?
            // Maybe check for the mod on the server?
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;
            
            if (ModContent.GetInstance<EveryOtherNPCConfig>().AllowRestrictionlessSpawning) {
                foreach (NPC npc in SpawnableTownNPCS) {
                    Main.townNPCCanSpawn[npc.type] = true;
                }
            }

            timer++;
            if (timer >= 120) {
                timer = 0;

                var config = ModContent.GetInstance<BoundNPCConfig>();
                if (config == null)
                    return;

                // NPC.saved[name] for trapped NPCs
                // NPC.unlocked[name]spawn for non "trapped" NPCs

                if (config.EnableAngler)
                    AnglerCheck();

                if (config.EnableStylist)
                    StylistCheck();

                if (config.EnableGolfer)
                    GolferCheck();

                if (config.EnableGoblin)
                    GoblinCheck();

                if (config.EnableTavernkeep)
                    TavernkeepCheck();

                if (config.EnableMechanic)
                    MechanicCheck();

                if (config.EnableWizard)
                    WizardCheck();

                if (config.EnableTaxCollector)
                    TaxCollectorCheck();
            }
        }

        #region Specific NPC Checks
        public static void AnglerCheck()
        {
            if (!NPC.savedAngler)
                NPC.savedAngler = true;
        }

        public static void StylistCheck()
        {
            if (!NPC.savedStylist)
                NPC.savedStylist = true;
        }

        public static void GolferCheck()
        {
            if (!NPC.savedGolfer)
                NPC.savedGolfer = true;
        }

        public static void GoblinCheck()
        {
            if (!NPC.savedGoblin && (NPC.downedGoblins || ModContent.GetInstance<BoundNPCConfig>().GoblinAlwaysAvailable))
                NPC.savedGoblin = true;
        }

        public static void TavernkeepCheck()
        {
            if (!NPC.savedBartender && (NPC.downedBoss2 || ModContent.GetInstance<BoundNPCConfig>().TavernkeepAlwaysAvailable))
                NPC.savedBartender = true;
        }

        public static void MechanicCheck()
        {
            if (!NPC.savedMech && (NPC.downedBoss3 || ModContent.GetInstance<BoundNPCConfig>().MechanicAlwaysAvailable))
                NPC.savedMech = true;
        }

        public static void WizardCheck()
        {
            if (!NPC.savedWizard && (Main.hardMode || ModContent.GetInstance<BoundNPCConfig>().WizardAlwaysAvailable))
                NPC.savedWizard = true;

        }

        public static void TaxCollectorCheck()
        {
            if (!NPC.savedTaxCollector && (Main.hardMode || ModContent.GetInstance<BoundNPCConfig>().TaxCollectorAlwaysAvailable))
                NPC.savedTaxCollector = true;
        }
        #endregion
    }
}
