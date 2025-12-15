using System;
using System.Collections.Generic;
using System.Linq;
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
        public override void OnModLoad()
        {
            IL_NPC.AI_007_TownEntities += HookPermanentSanta;
            IL_NPC.UpdateNPC_Inner += HookPermanentSanta;
            base.OnModLoad();
        }
        // [40048 3 - 40048 54]
        /* AI_OO7_TownEntities
        IL_02cd: ldarg.0      // this
        IL_02ce: ldfld        int32 Terraria.NPC::'type'
        IL_02d3: ldc.i4       142 // 0x0000008e
        IL_02d8: bne.un.s     IL_0325
        IL_02da: ldsfld       int32 Terraria.Main::netMode
        IL_02df: ldc.i4.1
        IL_02e0: beq.s        IL_0325
        IL_02e2: ldsfld       bool Terraria.Main::xMas
        IL_02e7: brtrue.s     IL_0325
        */
        
        // [73662 3 - 73662 111]
        /* NPC_Update_Inner
        IL_024e: ldarg.0      // this
        IL_024f: ldfld        int32 Terraria.NPC::aiStyle
        IL_0254: ldc.i4.7
        IL_0255: bne.un.s     IL_02c2
        IL_0257: ldarg.0      // this
        IL_0258: ldflda       valuetype [FNA]Microsoft.Xna.Framework.Vector2 Terraria.Entity::position
        IL_025d: ldfld        float32 [FNA]Microsoft.Xna.Framework.Vector2::Y
        IL_0262: ldsfld       float32 Terraria.Main::bottomWorld
        IL_0267: ldc.r4       640
        IL_026c: sub
        IL_026d: ldarg.0      // this
        IL_026e: ldfld        int32 Terraria.Entity::height
        IL_0273: conv.r4
        IL_0274: add
        IL_0275: ble.un.s     IL_02c2
        IL_0277: ldsfld       int32 Terraria.Main::netMode
        IL_027c: ldc.i4.1
        IL_027d: beq.s        IL_02c2
        IL_027f: ldsfld       bool Terraria.Main::xMas
        IL_0284: brtrue.s     IL_02c2
        */
        private static void HookPermanentSanta(ILContext il)
        {
            try
            {
                ILCursor c = new ILCursor(il);

                // Should hit the first instance where the xMas bool is checked.
                c.GotoNext(i => i.MatchLdsfld(typeof(bool), nameof(Terraria.Main.xMas)));

                // Right after the xMas bool is added to the stack
                c.Index++;

                // If santa spawning is enabled, return a value of true to stop him being killed
                c.EmitDelegate<Func<bool, bool>>((returnValue) =>
                {
                    return false;
                    if (ModContent.GetInstance<EveryOtherNPCConfig>().AllowRestrictionlessSpawning) //&& TownNPCS.Exists(npc => npc.type == NPCID.SantaClaus))
                    {
                        return true;
                    }
                    return returnValue;
                });
            }
            catch (Exception e)
            {
                MonoModHooks.DumpIL(ModContent.GetInstance<UnboundMechanic>(), il);
            }
        }

        public int timer;

        public static List<NPC> TownNPCS = new();

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
            // This check happens every 20s, purely to save resources
            if (timer >= 1200)
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
