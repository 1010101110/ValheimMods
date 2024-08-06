using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace tripping.Patches
{
    public class TrippingPatches
    {
        public static Dictionary<string, int> timers = new Dictionary<string, int>();

        //client side way
        //[HarmonyPatch(typeof(Player), "OnDeath")]
        //private static class reworkdeath
        //{
        //    private static bool Prefix(ref Player __instance)
        //    {
        //        if (__instance.IsOwner() && Chat.instance != null)
        //        {
        //            Chat.instance.SendText(Talker.Type.Shout, __instance.GetPlayerName() + " has been slain");
        //        }
        //        return true;
        //    }
        //}

        //server side way
        [HarmonyPatch(typeof(ZNet), "CheckWhiteList")]
        public static class CheckCharWhitelist
        {
            private static bool Prefix(ZNet __instance)
            {
                //runs every 5s
                //check whitelist
                if (__instance.m_peers != null)
                {
                    if (__instance.m_peers.Count > 0)
                    {
                        foreach (ZNetPeer znetPeer in __instance.m_peers.ToList<ZNetPeer>())
                        {
                            if (znetPeer.IsReady() && !znetPeer.m_characterID.IsNone())
                            {
                                string charName = znetPeer.m_playerName;
                                string id = znetPeer.m_socket.GetHostName();
                                string idandname = id + "|" + charName;

                                //check if they're dead
                                ZDO zdo = __instance.m_zdoMan.GetZDO(znetPeer.m_characterID);
                                if (zdo != null)
                                {
                                    bool dead = zdo.GetBool("dead", false);
                                    if (dead && !TrippingPatches.timers.ContainsKey(idandname))
                                    {
                                        ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "ChatMessage", new object[]
                                        {
                                            znetPeer.m_refPos,
                                            2,
                                            znetPeer.m_playerName,
                                            znetPeer.m_playerName + " has been slain"
                                        });
                                        ZLog.Log(znetPeer.m_playerName + " DIED " + znetPeer.m_refPos.ToString());

                                        TrippingPatches.timers.Add(idandname, 5);
                                    }
                                }
                            }
                        }
                    }
                }

                foreach (string key in TrippingPatches.timers.Keys.ToList<string>())
                {
                    TrippingPatches.timers[key] = TrippingPatches.timers[key] - 1;
                    bool flag7 = TrippingPatches.timers[key] < 0;
                    if (flag7)
                    {
                        TrippingPatches.timers.Remove(key);
                    }
                }

                return true;
            }
        }
    }
}