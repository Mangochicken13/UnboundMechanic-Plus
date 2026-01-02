using System;
using System.Collections.Generic;
using System.Reflection;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.RuntimeDetour.HookGen;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace UnboundMechanic
{
    public class TownNPCSpawnSystem : ModSystem
    {
        private Hook my_Hook;
        private delegate bool orig_CanTownNPCSpawn(ModNPC self, int numTownNPCs);
        public override void Load() {
            IL_NPC.AI_007_TownEntities += HookPermanentSanta;
            On_WorldGen.CheckSpecialTownNPCSpawningConditions += On_CheckSpecialTownNPCSpawningConditions;
            //On_WorldGen.TownNPC

            MethodInfo detourMethod = typeof(ModNPC).GetMethod("CanTownNPCSpawn", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            MonoModHooks.Add(detourMethod, On_CanTownNPCSpawn);
            
            base.Load();
        }

        private static void HookPermanentSanta(ILContext il) {
            try {
                ILCursor c = new ILCursor(il);
                
                c.GotoNext(i => i.MatchLdcI4(NPCID.SantaClaus));

                c.Index++;

                // If santa spawning is enabled, return a true value (1) to stop him being killed
                c.EmitDelegate<Func<int, int>>((returnValue) => {
                    if (ModContent.GetInstance<RestrictionlessSpawningConfig>().AllowRestrictionlessSpawning &&
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

        private bool On_CanTownNPCSpawn(orig_CanTownNPCSpawn orig, ModNPC self, int numTownNPCs) {
            if (SpawnableTownNPCS.Exists(npc => npc.type == self.Type))
                return true;
            return orig(self, numTownNPCs);
        }

        public int timer;

        // TODO: Update this list on config change, not just world load
        private static List<NPC> AllTownNPCS = new();
        public static List<NPC> SpawnableTownNPCS = new();

        public override void OnWorldLoad() {
            var restrictionlessConfig = ModContent.GetInstance<RestrictionlessSpawningConfig>();
            
            // Add all town NPCs to a reference list, skip Travelling Merchant
            // TODO: Check that skeleton merchant dosen't do the same multiple spawning thing on secret seeds
            if (restrictionlessConfig.AllowRestrictionlessSpawning) {
                foreach (NPC npc in ContentSamples.NpcsByNetId.Values) {
                    if (npc.townNPC && NPC.TypeToDefaultHeadIndex(npc.type) >= 0)
                        AllTownNPCS.Add(npc);
                }
            }

            UpdateSpawnableTownNPCs();
        }

        // Prune unloaded/disabled NPCs from the reference list
        public void UpdateSpawnableTownNPCs(bool allEnabled = false) {
            if (allEnabled) {
                SpawnableTownNPCS = AllTownNPCS;
                return;
            }
            
            List<int> validNPCTypes = new();
            List<NPC> spawnableTownNPCs = new();

            var restrictionlessConfig = ModContent.GetInstance<RestrictionlessSpawningConfig>();
            
            foreach (NPCDefinition npcDef in restrictionlessConfig.NPCList) {
                if (!npcDef.IsUnloaded)
                    validNPCTypes.Add(npcDef.Type);
            }
            foreach (NPC npc in AllTownNPCS) {
                if (npc.type == NPCID.TravellingMerchant &&
                    !restrictionlessConfig.AllowMultipleTravellingMerchantSpawns) {
                    continue;
                }

                if (validNPCTypes.Contains(npc.type))
                    spawnableTownNPCs.Add(npc);
            }
            SpawnableTownNPCS = spawnableTownNPCs;
        }

        public override void PostUpdateNPCs()
        {
            //return;
            
            // Hopefully avoid breaking census for multiplayer clients?
            // Maybe check for the mod on the server?
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;
            
            if (ModContent.GetInstance<RestrictionlessSpawningConfig>().AllowRestrictionlessSpawning) {
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
                    TryEnableAngler();

                if (config.EnableStylist)
                    TryEnableStylist();

                if (config.EnableGolfer)
                    TryEnableGolfer();

                if (config.EnableGoblin)
                    TryEnableGoblin();

                if (config.EnableTavernkeep)
                    TryEnableTavernkeep();

                if (config.EnableMechanic)
                    TryEnableMechanic();

                if (config.EnableWizard)
                    TryEnableWizard();

                if (config.EnableTaxCollector)
                    TryEnableTaxCollector();
            }
        }

        #region Specific NPC Checks
        public static void TryEnableAngler() {
            if (!NPC.savedAngler)
                NPC.savedAngler = true;
        }

        public static void TryEnableStylist() {
            if (!NPC.savedStylist)
                NPC.savedStylist = true;
        }

        public static void TryEnableGolfer() {
            if (!NPC.savedGolfer)
                NPC.savedGolfer = true;
        }

        public static void TryEnableGoblin() {
            if (!NPC.savedGoblin &&
                (NPC.downedGoblins || ModContent.GetInstance<BoundNPCConfig>().GoblinAlwaysAvailable))
                NPC.savedGoblin = true;
        }

        public static void TryEnableTavernkeep() {
            if (!NPC.savedBartender &&
                (NPC.downedBoss2 || ModContent.GetInstance<BoundNPCConfig>().TavernkeepAlwaysAvailable))
                NPC.savedBartender = true;
        }

        public static void TryEnableMechanic() {
            if (!NPC.savedMech &&
                (NPC.downedBoss3 || ModContent.GetInstance<BoundNPCConfig>().MechanicAlwaysAvailable))
                NPC.savedMech = true;
        }

        public static void TryEnableWizard() {
            if (!NPC.savedWizard &&
                (Main.hardMode || ModContent.GetInstance<BoundNPCConfig>().WizardAlwaysAvailable))
                NPC.savedWizard = true;

        }

        public static void TryEnableTaxCollector() {
            if (!NPC.savedTaxCollector &&
                (Main.hardMode || ModContent.GetInstance<BoundNPCConfig>().TaxCollectorAlwaysAvailable))
                NPC.savedTaxCollector = true;
        }
        #endregion
    }
}
