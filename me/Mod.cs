using BepInEx;
using HarmonyLib;
using System.Linq;

namespace tripping
{
    // Token: 0x02000002 RID: 2
    [BepInPlugin("1010101110.me", "me", "1.0.0")]
    public class Mod : BaseUnityPlugin
    {
        private void Awake()
        {
            Mod.harmony.PatchAll();
        }

        private void OnDestroy()
        {
            Mod.harmony.UnpatchSelf();
        }

        public static readonly Harmony harmony = new Harmony(typeof(Mod).GetCustomAttributes(typeof(BepInPlugin), false).Cast<BepInPlugin>().First<BepInPlugin>().GUID);
    }
}