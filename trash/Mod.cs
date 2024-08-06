using BepInEx;
using HarmonyLib;
using System.Linq;

namespace vrpstable
{
    // Token: 0x02000002 RID: 2
    [BepInPlugin("1010101110.trash", "trash", "0.2.0")]
    public class Mod : BaseUnityPlugin
    {
        public static Mod instance;
        public BepInEx.Configuration.ConfigEntry<string> confighotkey;

        private void Awake()
        {
            instance = this;
            confighotkey = Config.Bind("General",
                                    "hotkey",
                                    "delete",
                                    @"use this hotkey to trash stuff instead of having to drag it to the trash gui. possible values @ https://docs.unity3d.com/Manual/class-InputManager.html");

            Mod.harmony.PatchAll();
        }

        private void OnDestroy()
        {
            instance = null;
            Mod.harmony.UnpatchSelf();
        }

        public static readonly Harmony harmony = new Harmony(typeof(Mod).GetCustomAttributes(typeof(BepInPlugin), false).Cast<BepInPlugin>().First<BepInPlugin>().GUID);
    }
}