using BepInEx;
using HarmonyLib;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace multiverse
{
    [BepInPlugin("1010101110.multiverse", "multiverse", "0.0.6")]
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

        private static readonly byte x = 41;
        public static string servertojoin = "";
        public static string worldtojoin = "";
        public static string passtojoin = "";

        [HarmonyPatch(typeof(TeleportWorld), "Interact")]
        public static class InteractPatch
        {
            private static bool Prefix(
              ref Humanoid human,
              ref bool hold,
              ref bool __result,
              TeleportWorld __instance)
            {
                if (hold)
                {
                    __result = false;
                    return false;
                }
                if (!PrivateArea.CheckAccess(__instance.transform.position))
                {
                    human.Message(MessageHud.MessageType.Center, "$piece_noaccess");
                    __result = true;
                    return false;
                }
                TextInput.instance.RequestText((TextReceiver)__instance, "$piece_portal_tag", 50);
                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(TeleportWorld), "RPC_SetTag")]
        public static class settagpatch
        {
            private static bool Prefix(long sender, ref string tag)
            {
                if (tag.Contains("server|"))
                {
                    string[] strArray = tag.Split('|');
                    if (strArray.Length > 2)
                    {
                        byte[] bytes = Encoding.UTF8.GetBytes(strArray[2]);
                        for (int index = 0; index < bytes.Length; ++index)
                            bytes[index] = (byte)((uint)bytes[index] ^ (uint)x);
                        strArray[2] = Convert.ToBase64String(bytes);
                    }
                    tag = string.Join("|", strArray);
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(TeleportWorld), "HaveTarget")]
        public static class HaveTargetPatch
        {
            private static bool Prefix(ref bool __result, TeleportWorld __instance)
            {
                string text = __instance.GetText();
                if (!text.Contains("server|") && !text.Contains("world|"))
                    return true;
                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(TeleportWorld), "TargetFound")]
        public static class TargetFoundPatch
        {
            private static bool Prefix(ref bool __result, TeleportWorld __instance)
            {
                string text = __instance.GetText();
                if (!text.Contains("server|") && !text.Contains("world|"))
                    return true;
                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(TeleportWorld), "Teleport")]
        public static class TeleportPatch
        {
            private static void Postfix(ref Player player, TeleportWorld __instance)
            {
                if (!player.IsTeleportable())
                {
                    player.Message(MessageHud.MessageType.Center, "$msg_noteleport", 0, (Sprite)null);
                }
                else
                {
                    string text = __instance.GetText();
                    if (text.Contains("server|"))
                    {
                        string[] strArray1 = text.Split('|');
                        if (strArray1.Length > 1)
                        {
                            string[] strArray2 = strArray1[1].Split(':');
                            if ((uint)strArray2.Length > 0U)
                            {
                                string str = strArray2[0];
                                int result = 2456;
                                if (strArray2.Length > 1)
                                    int.TryParse(strArray2[1], out result);
                                servertojoin = str + ":" + result.ToString();
                            }
                            if (strArray1.Length > 2)
                            {
                                byte[] bytes = Convert.FromBase64String(strArray1[2]);
                                for (int index = 0; index < bytes.Length; ++index)
                                    bytes[index] = (byte)((uint)bytes[index] ^ (uint)x);
                                passtojoin = Encoding.UTF8.GetString(bytes);
                            }
                        }
                    }
                    if (text.Contains("world|"))
                    {
                        string[] strArray = text.Split('|');
                        if (strArray.Length > 1)
                            worldtojoin = strArray[1];
                    }
                    if (!(worldtojoin != "") && !(servertojoin != ""))
                        return;
                    Vector3 position = player.transform.position;
                    position.x -= 2f;
                    position.z -= 2f;
                    ZoneSystem.instance.FindFloor(position, out position.y);
                    player.transform.position = position;
                    ZLog.Log(("Multiverse - logging out and connecting to: " + servertojoin + worldtojoin));
                    Game.instance.Logout();
                }
            }
        }

        [HarmonyPatch(typeof(FejdStartup), "Start")]
        public static class starterfirst
        {
            private static void Postfix()
            {
                if (servertojoin != "")
                {
                    ZLog.Log("Multiverse - starting game");
                    ZSteamMatchmaking.instance.QueueServerJoin(servertojoin);
                    ZLog.Log(("Multiverse - queued server " + servertojoin));
                }
                if (!(worldtojoin != ""))
                    return;
                FejdStartup.instance.HideAll();
                FejdStartup.instance.ShowCharacterSelection();
            }
        }

        [HarmonyPatch(typeof(FejdStartup), "ShowCharacterSelection")]
        public static class starterselect
        {
            private static async void Postfix()
            {
                if (!(servertojoin != "") && !(worldtojoin != ""))
                    return;
                ZLog.Log("Multiverse - starting character");
                await Task.Delay(500);
                servertojoin = "";
                if (worldtojoin != "")
                {
                    PlayerPrefs.SetString("world", worldtojoin);
                    FejdStartup.instance.m_world = null;
                }
                FejdStartup.instance.OnCharacterStart();
            }
        }

        [HarmonyPatch(typeof(FejdStartup), "OnSelectWorldTab")]
        public static class worldstartpatch
        {
            public static async void Postfix()
            {
                if (!(worldtojoin != ""))
                    return;
                await Task.Delay(1000);
                ZLog.Log("Multiverse - starting world " + worldtojoin);
                if (FejdStartup.instance.m_world != null && FejdStartup.instance.m_world.m_name == worldtojoin)
                {
                    FejdStartup.instance.HideAll();
                    FejdStartup.instance.OnWorldStart();
                }
                else
                {
                    ZLog.LogError("Multiverse - world not set properly");
                    ZLog.LogError(FejdStartup.instance.m_world.m_name);
                }
                worldtojoin = "";
            }
        }

        [HarmonyPatch(typeof(ZNet), "RPC_ClientHandshake")]
        public static class passwordenter
        {
            public static void Postfix()
            {
                ZLog.Log("Multiverse - entering password");
                if (string.IsNullOrEmpty(passtojoin) || ZNet.instance.m_tempPasswordRPC == null)
                    return;
                string pwd = string.Copy(passtojoin);
                passtojoin = "";
                ZNet.instance.OnPasswordEnter(pwd);
            }
        }
    }
}