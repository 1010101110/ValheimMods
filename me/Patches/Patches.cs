using HarmonyLib;
using Splatform;
using UnityEngine;
using System;

namespace tripping.Patches
{
    public class TrippingPatches
    {
        [HarmonyPatch(typeof(Chat), "Awake")]
        private class awakepatch
        {
            private static void Postfix(ref Chat __instance)
            {
                __instance.AddString("/me textttttttt");
                __instance.AddString("");
            }
        }

        [HarmonyPatch(typeof(Chat), "InputText")]
        private class sendtextpatch
        {
            private static bool Prefix(ref Chat __instance)
            {
                bool flag = __instance.m_input.text.StartsWith("/me ");
                if (flag)
                {
                    try
                    {
                        string str = __instance.m_input.text.Substring(4);
                        __instance.m_input.text = str + "{|me|}";
                    }
                    catch
                    {
                        __instance.AddString("failed me... /me blah blah blah");
                        return false;
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Chat), "AddInworldText")]
        private class noinworldtext
        {
            private static bool Prefix(GameObject go, long senderID, Vector3 position, Talker.Type type, UserInfo user, string text)
            {
                bool ret = true;
                if (text.Contains("{|me|}"))
                {
                    ret = false;
                }
                return ret;
            }
        }

        [HarmonyPatch(typeof(Terminal), "AddString", new Type[] { typeof(PlatformUserID), typeof(string), typeof(Talker.Type), typeof(bool) })]
        private class mestring
        {
            private static bool Prefix(PlatformUserID user, string text, Talker.Type type, bool timestamp, ref Terminal __instance)
            {
                bool ret = true;
                if (text.Contains("{|me|}"))
                {
                    ZLog.LogError(user);
                    ZLog.LogError(text);

                    //get username
                    if (ZNet.TryGetPlayerByPlatformUserID(user, out var playerInfo))
                    {
                        string text2 = text.Replace("{|me|}", "");
                        __instance.AddString(string.Concat(new string[]
                        {
                            "<color=#607D8B>",
                            playerInfo.m_name,
                            " ",
                            text2,
                            "</color>"
                        }));

                        ret = false;
                    }
                }
                return ret;
            }
        }
    }
}