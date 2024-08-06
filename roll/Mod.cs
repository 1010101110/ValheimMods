using BepInEx;
using HarmonyLib;
using System.Linq;

namespace tripping
{
    [BepInPlugin("1010101110.roll", "roll", "1.0.0")]
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