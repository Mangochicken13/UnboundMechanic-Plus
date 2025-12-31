using System;
using System.Collections.Generic;
using MonoMod.Cil;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace UnboundMechanic
{
    public class NPCUnlockSystem : ModSystem
    {
        // Patching the NPC class to prevent santa being killed
        // while he is in the restrictionless spawns list
        public override void Load() {
            IL_NPC.AI_007_TownEntities += HookPermanentSanta;
            On_WorldGen.CheckSpecialTownNPCSpawningConditions += On_CheckSpecialTownNPCSpawningConditions;
            //IL_NPC.UpdateNPC_Inner += HookPermanentSanta;
            base.Load();
        }

        private static void HookPermanentSanta(ILContext il)
        {
            try
            {
                ILCursor c = new ILCursor(il);
                
                // NPCID.SantaClaus
                c.GotoNext(i => i.MatchLdcI4(142)); 

                c.Index++;
                //c.Emit(Mono.Cecil.Cil.OpCodes.Pop);
                //c.Emit(Mono.Cecil.Cil.OpCodes.Ldc_I4_0);

                // If santa spawning is enabled, return a value of true to stop him being killed
                c.EmitDelegate<Func<int, int>>((returnValue) =>
                {
                    //ModContent.GetInstance<UnboundMechanic>().Logger.Info("Delegate Reached");
                    //return 1;
                    if (ModContent.GetInstance<EveryOtherNPCConfig>().AllowRestrictionlessSpawning && TownNPCS.Exists(npc => npc.type == NPCID.SantaClaus))
                    {
                        return 1;
                    }
                    return returnValue;
                });
            }
            catch (Exception e)
            {
                MonoModHooks.DumpIL(ModContent.GetInstance<UnboundMechanic>(), il);
                throw new ILPatchFailureException(ModContent.GetInstance<UnboundMechanic>(), il, e);
            }
        }

        private bool On_CheckSpecialTownNPCSpawningConditions(On_WorldGen.orig_CheckSpecialTownNPCSpawningConditions orig, int type) {
            bool canSpawn = orig(type);
            if (type == NPCID.Truffle && TownNPCS.Exists(npc => npc.type == NPCID.Truffle)) return true;
            return canSpawn;
        }
        
        public int timer;

        private static List<NPC> TownNPCS = new();

        public override void OnWorldLoad()
        {
            #region Restrictionless Spawning
            // This block also handles for if EveryoneCanSpawnFromStart is true, as AllowRestrictionlessSpawning must necessarily be true too
            if (ModContent.GetInstance<EveryOtherNPCConfig>().AllowRestrictionlessSpawning)
            {
                foreach (NPC npc in ContentSamples.NpcsByNetId.Values)
                {
                    if (npc.type == NPCID.TravellingMerchant)
                        continue;
                    if (npc.townNPC && NPC.TypeToDefaultHeadIndex(npc.type) >= 0 && !TownNPCS.Contains(npc)) { TownNPCS.Add(npc); }
                }
            }

            // This part is for if EveryoneCanSpawnFromStart is false, and prunes NPCs not present in the config from the list to enable to spawn
            if (!ModContent.GetInstance<EveryOtherNPCConfig>().EveryoneCanSpawnFromStart && 
                ModContent.GetInstance<EveryOtherNPCConfig>().AllowRestrictionlessSpawning)
            {
                List<int> types = new();
                List<NPC> townNPCsTemp = new();

                foreach (NPCDefinition npcDef in ModContent.GetInstance<EveryOtherNPCConfig>().NPCList)
                {
                    if (!npcDef.IsUnloaded)
                    {
                        types.Add(npcDef.Type);
                    }
                }

                foreach (NPC npc in TownNPCS)
                {
                    if (types.Contains(npc.type))
                    {
                        townNPCsTemp.Add(npc);
                    }
                }
                TownNPCS.Clear();
                TownNPCS = townNPCsTemp;
            }
            #endregion
        }

        public override void PostUpdateNPCs()
        {
            // This part of the mod should be done, do test multiplayer before releasing though
            #region Restrictionless Spawning
            if (ModContent.GetInstance<EveryOtherNPCConfig>().AllowRestrictionlessSpawning)
            {
                foreach (NPC npc in TownNPCS)
                {
                    Main.townNPCCanSpawn[npc.type] = true;
                    
                    // Extra logic to unlock specific NPCs
                    switch (npc.type) {
                        case NPCID.Truffle:
                            if (!NPC.unlockedTruffleSpawn) { NPC.unlockedTruffleSpawn = true; }
                            break;
                    }
                        
                }
            }
            #endregion

            timer++;
            if (timer >= 120)
            {
                timer = 0;

                var config = ModContent.GetInstance<UnboundMechanicConfig>();
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
            if (!NPC.savedGoblin && (NPC.downedGoblins || ModContent.GetInstance<UnboundMechanicConfig>().GoblinAlwaysAvailable))
                NPC.savedGoblin = true;
        }

        public static void TavernkeepCheck()
        {
            if (!NPC.savedBartender && (NPC.downedBoss2 || ModContent.GetInstance<UnboundMechanicConfig>().TavernkeepAlwaysAvailable))
                NPC.savedBartender = true;
        }

        public static void MechanicCheck()
        {
            if (!NPC.savedMech && (NPC.downedBoss3 || ModContent.GetInstance<UnboundMechanicConfig>().MechanicAlwaysAvailable))
                NPC.savedMech = true;
        }

        public static void WizardCheck()
        {
            if (!NPC.savedWizard && (Main.hardMode || ModContent.GetInstance<UnboundMechanicConfig>().WizardAlwaysAvailablePreHardmode))
                NPC.savedWizard = true;

        }

        public static void TaxCollectorCheck()
        {
            if (!NPC.savedTaxCollector && (Main.hardMode || ModContent.GetInstance<UnboundMechanicConfig>().TaxCollectorAlwaysAvailablePreHardmode))
                NPC.savedTaxCollector = true;
        }
        #endregion
    }
}
