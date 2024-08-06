using HarmonyLib;
using System;
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
                __instance.AddString("/roll min(optional) max(optional) rolls dice, default (1-100)");
                __instance.AddString("");
            }
        }

        [HarmonyPatch(typeof(Chat), "InputText")]
        private class sendtextpatch
        {
            private static bool Prefix(ref Chat __instance)
            {
                if (__instance.m_input.text.StartsWith("/roll"))
                {
                    try
                    {
                        int num = 1;
                        int num2 = 100;
                        string[] array = __instance.m_input.text.Split(new char[]
                        {
                            ' '
                        });
                        bool flag2 = array.Length == 2;
                        if (flag2)
                        {
                            num2 = int.Parse(array[1]);
                        }
                        bool flag3 = array.Length == 3;
                        if (flag3)
                        {
                            num = int.Parse(array[1]);
                            num2 = int.Parse(array[2]);
                        }
                        bool flag4 = num <= num2;
                        if (!flag4)
                        {
                            throw new Exception("invalid roll");
                        }

                        var random = new System.Random();
                        __instance.m_input.text = string.Concat(new string[]
                        {
                            "rolls ",
                            random.Next(num, num2 + 1).ToString(),
                            "  (",
                            num.ToString(),
                            "-",
                            num2.ToString(),
                            "){|roll|}"
                        });
                    }
                    catch
                    {
                        __instance.AddString("failed roll, try /roll or /roll 100 or /roll 1 100");
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
                bool result;
                if (text.Contains("{|roll|}"))
                {
                    string text2 = text.Replace("{|roll|}", "");
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