using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace tripping.Patches
{
    public class TrippingPatches
    {
        public static SyncedList characterbans;
        public static Dictionary<string, int> timers = new Dictionary<string, int>();

        //initialize the synced list used for tracking bans
        [HarmonyPatch(typeof(ZNet), "Awake")]
        public static class awakepatch
        {
            private static void Postfix()
            {
                characterbans = new SyncedList(Utils.GetSaveDataPath(FileHelpers.FileSource.Local) + "/permadeath.txt",
                "List of dead players by steamid|charname - ONE per line - serverpermadeath mod");
            }
        }

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
                                //name string
                                string idandname = znetPeer.m_socket.GetHostName() + "|" + znetPeer.m_playerName;

                                //get zdo and see if their dead
                                ZDO zdo = __instance.m_zdoMan.GetZDO(znetPeer.m_characterID);
                                bool dead = false;
                                if (zdo != null && zdo.IsValid())
                                {
                                    ZLog.Log(idandname);
                                    ZLog.Log(znetPeer.m_characterID);

                                    dead = zdo.GetBool("dead".GetStableHashCode(), false);
                                }
                                else
                                {
                                    ZLog.LogError("permadeath - invalid zdo for " + idandname);
                                }

                                bool bannedalready = characterbans.Contains(idandname);

                                //they are banned, gtfo
                                if (bannedalready)
                                {
                                    ZNet.instance.InternalKick(znetPeer);
                                }
                                else
                                {
                                    //check if they're dead
                                    if (dead && !TrippingPatches.timers.ContainsKey(idandname))
                                    {
                                        //tell everyone they are dead
                                        ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "ChatMessage", new object[]
                                        {
                                        znetPeer.m_refPos,
                                        2,
                                        znetPeer.m_playerName,
                                        znetPeer.m_playerName + " has been slain"
                                        });
                                        ZLog.Log("permadeath - " + znetPeer.m_playerName + " DIED " + znetPeer.m_refPos.ToString());

                                        //start a timer to kick them
                                        TrippingPatches.timers.Add(idandname, 5);
                                    }
                                }
                            }
                        }
                    }
                }

                //clean up the timers
                foreach (string key in TrippingPatches.timers.Keys.ToList<string>())
                {
                    TrippingPatches.timers[key] = TrippingPatches.timers[key] - 1;
                    if (TrippingPatches.timers[key] < 0)
                    {
                        //add them to the banned list
                        characterbans.Add(key);

                        //remove the timer
                        TrippingPatches.timers.Remove(key);
                    }
                }

                return true;
            }
        }
    }
}