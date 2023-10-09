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

    public class MechancUnbindSystem : ModSystem
    {
        public int timer;
        public override void PostUpdateNPCs()
        {
            // Note to self: check if this is better for performance
            if (++timer  >= 600)
            {
                timer = 0;
            }
            // Note to self: Find a way to not even do this check after the mechanic can spawn, just in case it causes issues
            if ((NPC.downedBoss3 && !NPC.savedMech) || (ModContent.GetInstance<UnboundMechanicConfig>().MechanicAlwaysAvailable && !NPC.savedMech))
            {
                NPC.savedMech = true;
            }

            /* 
            This way might save resources?, since it skips checking the other variables if the mechanic is already saved?
            
            if (!NPC.savedMech) {
                if (NPC.downedBoss3 || ModContent.GetInstance<UnboundMechanicConfig>().MechanicAlwaysAvailable){
                    NPC.savedMech = true;
                }
            }
            */
        }
    }
}
