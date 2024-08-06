using BepInEx;
using HarmonyLib;
using System.Linq;

namespace ServerCharacters
{
    // Token: 0x02000002 RID: 2
    [BepInPlugin("1010101110.socketparams", "socketparams", "0.0.1")]
    public class Mod : BaseUnityPlugin
    {
        // Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
        private void Awake()
        {
            Mod.harmony.PatchAll();
        }

        // Token: 0x06000002 RID: 2 RVA: 0x0000205E File Offset: 0x0000025E
        private void OnDestroy()
        {
            Mod.harmony.UnpatchSelf();
        }

        // Token: 0x04000001 RID: 1
        public static readonly Harmony harmony = new Harmony(typeof(Mod).GetCustomAttributes(typeof(BepInPlugin), false).Cast<BepInPlugin>().First<BepInPlugin>().GUID);
    }
}