using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

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
            if (ModContent.GetInstance<B_EveryOtherNPCConfig>().EveryoneCanSpawnFromStart)
            {
                foreach (NPC npc in ContentSamples.NpcsByNetId.Values)
                {
                    if (npc.townNPC && NPC.TypeToDefaultHeadIndex(npc.type) >= 0)
                    {
                        TownNPCS.Add(npc);
                    }
                }
            }
        }

        public override void PostUpdateNPCs()
        {

            if (ModContent.GetInstance<B_EveryOtherNPCConfig>().EveryoneCanSpawnFromStart)
            {
                foreach (NPC npc in TownNPCS)
                {
                    // TODO: Double check that this isn't an intensive operation
                    Main.townNPCCanSpawn[npc.type] = true;
                }
            }

            timer++;
            if (timer >= 3600)
            {
                timer = 0;

                var config = ModContent.GetInstance<A_UnboundMechanicConfig>();
                if (config == null)
                    return;

                // NPC.saved[name] for trapped NPCs
                // NPC.unlocked[name]spawn for non "trapped" NPCs

                if (config.EnableAngler && !NPC.savedAngler)
                    NPC.savedAngler = true;

                if (config.EnableGoblin && !NPC.savedGoblin)
                    GoblinCheck();

                if (config.EnableGolfer && !NPC.savedGolfer)
                    NPC.savedGolfer = true;

                if (config.EnableMechanic && !NPC.savedMech)
                    MechanicCheck();

                if (config.EnableStylist && !NPC.savedStylist)
                    NPC.savedStylist = true;
            }

        }

        #region Specific NPC Checks
        public void MechanicCheck()
        {
            // Assumes NPC.savedMech is false
            if (NPC.downedBoss3 || ModContent.GetInstance<A_UnboundMechanicConfig>().MechanicAlwaysAvailable)
                NPC.savedMech = true;
        }

        public void GoblinCheck()
        {
            if (NPC.downedGoblins || ModContent.GetInstance<A_UnboundMechanicConfig>().GoblinAlwaysAvailable)
                NPC.savedGoblin = true;
        }

        public void TavernkeepCheck()
        {
            if (NPC.downedBoss2 || ModContent.GetInstance<A_UnboundMechanicConfig>().TavernkeepAlwaysAvailable)
                NPC.savedBartender = true;
        }

        public void WizardCheck()
        {
            if (Main.hardMode || ModContent.GetInstance<A_UnboundMechanicConfig>().WizardAlwaysAvailablePreHardmode)
                NPC.savedWizard = true;

        }

        public void TaxCollectorCheck()
        {
            if (Main.hardMode || ModContent.GetInstance<A_UnboundMechanicConfig>().TaxCollectorAlwaysAvailablePreHardmode)
                NPC.savedTaxCollector = true;
        }

        public void UnlockMerchant()
        {
            NPC.unlockedMerchantSpawn = true;
        }
        #endregion
    }
}