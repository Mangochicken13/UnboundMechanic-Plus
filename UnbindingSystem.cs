using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace UnboundMechanic
{
    public class UnbindMechanicAfterSkele : GlobalNPC
    {
        public override void OnKill(NPC npc)
        {
            if (npc.type == NPCID.SkeletronHead)
            {
                NPC.savedMech = true;
            }
        }
    }

    public class NPCUnlockSystem : ModSystem
    {
        public int timer;

        public List<NPC> TownNPCS = new();

        public override void OnWorldLoad()
        {
            #region Restrictionless Spawning
            // This block also handles for if EveryoneCanSpawnFromStart is true, as AllowRestrictionlessSpawning must necessarily be true too
            if (ModContent.GetInstance<EveryOtherNPCConfig>().AllowRestrictionlessSpawning)
            {
                foreach (NPC npc in ContentSamples.NpcsByNetId.Values)
                {
                    if (npc.townNPC && NPC.TypeToDefaultHeadIndex(npc.type) >= 0)
                    {
                        TownNPCS.Add(npc);
                    }
                }
            }

            // This part is for if EveryoneCanSpawnFromStart is false, and prunes NPCs not present in the config from the list to enable to spawn
            if (!ModContent.GetInstance<EveryOtherNPCConfig>().EveryoneCanSpawnFromStart && ModContent.GetInstance<EveryOtherNPCConfig>().AllowRestrictionlessSpawning)
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
                    // TODO: Double check that this isn't an intensive operation
                    Main.townNPCCanSpawn[npc.type] = true;
                }
            }
            #endregion

            timer++;
            // This check happens every 60s, purely to save resources
            if (timer >= 3600)
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