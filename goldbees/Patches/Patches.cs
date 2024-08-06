using HarmonyLib;
using System;

namespace goldbees
{
    //tripping mushroom effects
    //red vanilla - none
    //yellow vanilla - none
    //blue vanilla - trippy camera effect
    //black - kills you
    //blood - red trippy camera effect, high healing
    //green - wishbone
    //pink - pickable object effect
    //purple - random noises
    //rainbow - rainbow camera effect, high health / stam values

    public class MyPatches
    {
        [HarmonyPatch(typeof(SE_Rested), nameof(SE_Rested.CalculateComfortLevel), new Type[] { typeof(Player) })]
        private class restedadditions
        {
            private static void Postfix(Player player, ref int __result)
            {
                if (player.m_seman.HaveStatusEffect("Soothin".GetStableHashCode()))
                {
                    __result += 5;
                }
                if (player.m_seman.HaveStatusEffect("Golden".GetStableHashCode()))
                {
                    __result += 5;
                }
                if (player.m_seman.HaveStatusEffect("Chillin".GetStableHashCode()))
                {
                    __result += 5;
                }
                if (player.m_seman.HaveStatusEffect("Easy".GetStableHashCode()))
                {
                    __result += 10;
                }
            }
        }
    }
}