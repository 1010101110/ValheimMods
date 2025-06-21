using HarmonyLib;
using Splatform;
using System;
using UnityEngine;
using UnityEngine.Android;

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
        
        [HarmonyPatch(typeof(Chat), "AddInworldText")]
        private class noinworldtext
        {
            private static bool Prefix(GameObject go, long senderID, Vector3 position, Talker.Type type, UserInfo user, string text)
            {
                bool ret = true;
                if (text.Contains("{|roll|}"))
                {
                    ret = false;
                }
                return ret;
            }
        }

        [HarmonyPatch(typeof(Terminal), "AddString", new Type[] { typeof(PlatformUserID), typeof(string), typeof(Talker.Type), typeof(bool) })]
        private class rollstring
        {
            private static bool Prefix(PlatformUserID user, string text, Talker.Type type, bool timestamp, ref Terminal __instance)
            {
                bool ret = true;
                if (text.Contains("{|roll|}"))
                {
                    ZLog.LogError(user);
                    ZLog.LogError(text);

                    //get username
                    if (ZNet.TryGetPlayerByPlatformUserID(user, out var playerInfo))
                    {
                        string text2 = text.Replace("{|roll|}", "");
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


        //[HarmonyPatch(typeof(Chat), "OnNewChatMessage")]
        //private class addstringpatch
        //{
        //    private static bool Prefix(
        //        GameObject go, long senderID, Vector3 pos, Talker.Type type, UserInfo sender, ref string text, ref Chat __instance
        //    )
        //    {
        //        //RelationsManager.CheckPermissionAsync(sender.UserId, Permission.CommunicateWithUsingText, isSender: false, delegate (RelationsManagerPermissionResult result)
        //        //{
        //        //    if (result.IsGranted())
        //        //    {
        //        //        if (__instance == null)
        //        //        {
        //        //            Debug.LogError("Chat has already been destroyed!");
        //        //        }
        //        //        else
        //        //        {
        //        //            text = text.Replace('<', ' ');
        //        //            text = text.Replace('>', ' ');
        //        //            if (result == RelationsManagerPermissionResult.GrantedRequiresFiltering)
        //        //            {
        //        //                CensorShittyWords.Filter(text, out text);
        //        //            }
        //        //            if (type != Talker.Type.Ping)
        //        //            {
        //        //                m_hideTimer = 0f;
        //        //                AddString(sender.UserId, text, type);
        //        //            }
        //        //            if (!Minimap.instance || !Player.m_localPlayer || Minimap.instance.m_mode != 0 || !(Vector3.Distance(Player.m_localPlayer.transform.position, pos) > Minimap.instance.m_nomapPingDistance))
        //        //            {
        //        //                AddInworldText(go, senderID, pos, type, sender, text);
        //        //            }
        //        //        }
        //        //    }
        //        //});


        //        if (text.Contains("{|roll|}"))
        //        {
        //            sender.GetDisplayName();
        //            string text2 = text.Replace("{|roll|}", "");
        //            __instance.AddString(string.Concat(new string[]
        //            {
        //                "<color=#607D8B>",
        //                sender.GetDisplayName(),
        //                " ",
        //                text2,
        //                "</color>"
        //            }));
        //            result = false;
        //        }
        //        else
        //        {
        //            result = true;
        //        }
        //        return result;
        //    }
        //}

    }
}