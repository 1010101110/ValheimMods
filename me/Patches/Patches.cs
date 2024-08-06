using HarmonyLib;
using UnityEngine;

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

        [HarmonyPatch(typeof(Chat), "OnNewChatMessage")]
        private class addstringpatch
        {
            private static bool Prefix(
                GameObject go,
                long senderID,
                Vector3 pos,
                Talker.Type type,
                UserInfo user,
                string text,
                string senderNetworkUserId,
                ref Chat __instance
            )
            {
                bool flag = text.Contains("{|me|}");
                bool result;
                if (flag)
                {
                    string text2 = text.Replace("{|me|}", "");
                    __instance.AddString(string.Concat(new string[]
                    {
                        "<color=#607D8B>",
                        user.GetDisplayName(senderNetworkUserId),
                        " ",
                        text2,
                        "</color>"
                    }));
                    result = false;
                }
                else
                {
                    result = true;
                }
                return result;
            }
        }
    }
}