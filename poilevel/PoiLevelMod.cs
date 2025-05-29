using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using ServerSync;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Rendering;
using UnityEngine.UI;
//using UnityEngine.UIElements;
using UnityStandardAssets.ImageEffects;
using Valheim.UI;
using static DungeonGenerator;
using static Heightmap;
using static MeleeWeaponTrail;

//using UnityEngine.UIElements;
using static UnityEngine.Networking.UnityWebRequest;

namespace sevendays
{
    [BepInPlugin(pluginID, pluginName, pluginVersion)]
    public partial class PoiLevelMod : BaseUnityPlugin
    {
        // mod info
        public const string pluginAuthor = "1010101110";
        public const string pluginName = "poilevel";
        public const string pluginID = pluginAuthor + "." + pluginName;
        public const string pluginVersion = "0.0.2";
        public const string pluginBuildDate = "2025-05-29";
        public static PoiLevelMod mod;


        // mod config
        internal static ConfigEntry<bool> configLocked;

        // init harmony
        public static readonly Harmony harmony = new Harmony(typeof(PoiLevelMod).GetCustomAttributes(typeof(BepInPlugin), false).Cast<BepInPlugin>().First<BepInPlugin>().GUID);

        // config sync and helpers
        internal static readonly ConfigSync configSync = new ConfigSync(pluginID) { DisplayName = pluginName, CurrentVersion = pluginVersion, MinimumRequiredVersion = pluginVersion };
        ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description, bool synchronizedSetting = true)
        {
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = configSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }
        ConfigEntry<T> config<T>(string group, string name, T value, string description, bool synchronizedSetting = true) => config(group, name, value, new ConfigDescription(description), synchronizedSetting);

        private void Awake()
        {
            //init
            Game.isModded = true;
            mod = this;

            // lock config
            configLocked = config("0 - Lock", "Lock Configuration", true, "Configuration is locked and can be changed by server admins only.");
            
            //
            _display = config("2 - UI Panel", "Show poi text", true, "shows poi text in the ui, client side", false);
            _displayBackground = config("2 - UI Panel", "Display background", false, "Display background", false);
            _fontSize = config("2 - UI Panel", "Font size", 16, "Font size", false);
            _fontName = config("2 - UI Panel", "Font name", "AveriaSansLibre-Bold", "Font name", false);
            _fontColor = config("2 - UI Panel", "Font color", new Color(1f, 1f, 1f, 0.791f), "Font color", false);
            _textOutlineEnabled = config("2 - UI Panel", "Text outline enabled", true, "Text outline enabled", false);
            _textOutlineColor = config("2 - UI Panel", "Text outline color", Color.black, "Text outline color", false);
            _backgroundColor = config("2 - UI Panel", "Background color", new Color(0f, 0f, 0f, 0.3921569f), "Background color", false);
            _marginBetweenMiniMap = config("2 - UI Panel", "Margin between minimap and this panel", 0f, "Margin between minimap and this panel", false);
            _padding = config("2 - UI Panel", "Padding left and right", 10f, "Padding left and right from text.", false);
            _panelWidth = config("2 - UI Panel", "Panel width", 260f, "Panel width", false);
            _panelHeight = config("2 - UI Panel", "Panel height", 80f, "Panel height", false);

            // locks config
            _ = configSync.AddLockingConfigEntry(configLocked);

            PoiLevelMod.harmony.PatchAll();

            Logger.LogInfo("blood moon initialized, feelin spooky?");
        }

        private void OnDestroy()
        {
            Config.Save();
            PoiLevelMod.harmony.UnpatchSelf();
        }

        private bool isHeadless()
        {
            return SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null;
        }


        private static readonly string[] names = {
            "Astrid","Bodil","Frida","Gertrud","Gro","Estrid","Hilda","Gudrun","Gunhild",
            "Helga","Inga","Liv","Randi","Signe","Sigrid","Revna","Sif","Tora","Tove","Thyra","Thurid","Yrsa","Ulfhild","Åse",
            "Arne","Birger","Bjørn","Bo","Erik","Frode","Gorm","Halfdan","Harald","Knud",
            "Kåre","Leif","Njal","Roar","Rune","Sten","Skarde","Sune","Svend","Troels","Toke","Torsten","Trygve","Ulf","Ødger","Åge",
        };

        private static readonly string[] adjectives = {
            "Jagged","Rugged","Massive","Weathered","Craggy","Towering","Eroded",
            "Ancient","Layered","Hidden","Steep","Precarious","Smooth","Sharp",
            "Brittle","Heavy","Looming","Crumbling","Enormous","Uneven","Mysterious",
            "Hollowed","Colossal","Moist","Glistening","Dank","Stinky","Haunting"
        };

        private static string GenderateName()
        {
            string name = string.Empty;
            string adjective = string.Empty;

            name = names[UnityEngine.Random.Range(0, names.Length)];
            adjective = adjectives[UnityEngine.Random.Range(0, adjectives.Length)];

            return $"{name}'s {adjective} ";
        }

        private class Location7d2d
        {
            public Vector2i zone { get; set; }
            public string name7d2d { get; set; }
            public int level7d2d { get; set; }
        }

        private Dictionary<Vector2i, Location7d2d> locations7d2d = new Dictionary<Vector2i, Location7d2d>();

        // patching when game loads
        [HarmonyPatch(typeof(EnvMan), nameof(EnvMan.Awake))]
        private static class envawake
        {
            private static void Postfix()
            {
                //reset locations on new load
                mod.locations7d2d = new Dictionary<Vector2i, Location7d2d>();
            }
        }

        // fix for more vertical sections = bigger dungeons
        [HarmonyPatch(typeof(DungeonGenerator), nameof(DungeonGenerator.SetupAvailableRooms))]
        private static class dgroomfix
        {
            private static void Postfix(DungeonGenerator __instance)
            {
                //sunken crypt only
                if ((__instance.m_themes & Room.Theme.SunkenCrypt) != 0)
                {
                    foreach (DungeonDB.RoomData room in DungeonDB.GetRooms())
                    {
                        //adds this room with vertical stair to sunken crypt
                        //this makes it able to spawn more rooms for a bigger dungeon
                        if (room.Hash == "forestcrypt_Chasm01".GetStableHashCode() || room.Hash == "forestcrypt_Stairs1".GetStableHashCode())
                        {
                            DungeonGenerator.m_availableRooms.Add(room);
                        }
                    }
                }

                foreach (var r in DungeonGenerator.m_availableRooms)
                {
                    // lets there be more stairs generated quicker in sequence, allows for more verticality 
                    if (r.Hash == "forestcrypt_Chasm01".GetStableHashCode() || r.Hash == "forestcrypt_Stairs1".GetStableHashCode())
                    {
                        r.m_prefab.Load();
                        r.RoomInPrefab.m_minPlaceOrder = 2;
                    }

                    //bug in valheim code
                    if (r.Hash == "sunkencrypt_new_Room4".GetStableHashCode())
                    {
                        r.m_prefab.Load();
                        if (r.RoomInPrefab != null)
                        {
                            var connects = r.RoomInPrefab.GetConnections();
                            if (connects != null && connects.Length > 0)
                            {
                                foreach (var conn in connects)
                                {
                                    if (conn.transform.parent.gameObject != r.RoomInPrefab.gameObject)
                                    {
                                        conn.transform.parent = r.RoomInPrefab.gameObject.transform;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // loop generation, if dungeon is too small retry generation with new seed
        [HarmonyPatch(typeof(DungeonGenerator), nameof(DungeonGenerator.GenerateRooms))]
        private static class dungeon_retry
        {
            private static void Postfix(ZoneSystem.SpawnMode mode, DungeonGenerator __instance)
            {
                // Check dungeon size
                int retries = 0;
                int retryLimit = 5;
                // cache some values for the maximum we generated
                int seed = 0;
                int bestSeed = 0;
                int bestRoomCount = 0;

                //only regenerate dungeons, camps never generate big enough
                if (__instance.m_algorithm != Algorithm.Dungeon)
                {
                    return;
                }

                while (retries <= retryLimit && DungeonGenerator.m_placedRooms.Count < __instance.m_minRooms)
                {
                    //dungeon is too small
                    mod.Logger.LogError($"dg.GenerateRooms placed {DungeonGenerator.m_placedRooms.Count} < {__instance.m_minRooms}, dungeon is too small, regenerating {retries}");

                    //cache best seed
                    bestSeed = bestRoomCount < DungeonGenerator.m_placedRooms.Count ? seed : bestSeed;
                    bestRoomCount = bestRoomCount < DungeonGenerator.m_placedRooms.Count ? DungeonGenerator.m_placedRooms.Count : bestRoomCount;

                    //clean up objects
                    var obj = GetDungeonObjects(ref __instance);
                    if (obj != null && obj.Count > 0)
                    {
                        mod.Logger.LogInfo($"destroying {obj.Count} dungeon objects");
                        for (int i = 0; i < obj.Count; i++)
                        {
                            var o = obj[i];
                            //make sure we don't delete the dungeon generator itself lol
                            if (o.gameObject.GetInstanceID() != __instance.gameObject.GetInstanceID())
                            {
                                ZNetScene.instance.Destroy(o.gameObject);
                            }
                        }
                    }

                    //double check to ensure rooms are cleaned up
                    foreach (var r in DungeonGenerator.m_placedRooms)
                    {
                        if (r.gameObject)
                        {
                            ZNetScene.instance.Destroy(r.gameObject);
                        }
                    }

                    //clear up lists
                    DungeonGenerator.m_placedRooms.Clear();
                    DungeonGenerator.m_openConnections.Clear();
                    DungeonGenerator.m_doorConnections.Clear();


                    //this is our last retry, lets just use the cached best seed
                    if (retries == retryLimit)
                    {
                        //we failed to generate a good dungeon so we'll just use the best we have
                        seed = bestSeed;
                    }
                    else
                    {
                        //try a new seed
                        seed = UnityEngine.Random.Range(0, int.MaxValue);
                    }
                    UnityEngine.Random.InitState(seed);

                    mod.Logger.LogInfo($"dg.GenerateRooms seed {seed}");

                    //call the generation again
                    switch (__instance.m_algorithm)
                    {
                        case Algorithm.Dungeon:
                            __instance.GenerateDungeon(mode);
                            break;
                        case Algorithm.CampGrid:
                            __instance.GenerateCampGrid(mode);
                            break;
                        case Algorithm.CampRadial:
                            __instance.GenerateCampRadial(mode);
                            break;
                    }

                    retries++;
                }

                mod.Logger.LogWarning($"dg.GenerateRooms postfix final room count: {DungeonGenerator.m_placedRooms.Count}");
            }
        }

        // set generation parameters
        [HarmonyPatch(typeof(DungeonGenerator), nameof(DungeonGenerator.Generate), new Type[] { typeof(int), typeof(ZoneSystem.SpawnMode) })]
        private static class dungeongenerate
        {
            private static void Prefix(DungeonGenerator __instance)
            {
                if (__instance.gameObject != null && __instance.m_nview != null)
                {
                    var zone = ZoneSystem.GetZone(__instance.gameObject.transform.position);

                    if (zone == null)
                    {
                        mod.Logger.LogError($"dg.g prefix invalid zone? {__instance.name}");
                        return;
                    }

                    if (__instance.m_nview == null)
                    {
                        mod.Logger.LogError($"dg.g prefix invalid netview? {__instance.name}");
                        return;
                    }

                    var zdo = __instance.m_nview.GetZDO();
                    if (zdo == null)
                    {
                        mod.Logger.LogError($"dg.g prefix invalid zdo? {__instance.name}");
                        return;
                    }

                    //dungeon generator setup
                    if (zdo.GetString("7d2d_name").IsNullOrWhiteSpace())
                    {
                        //generate name and level
                        int randomLevel = UnityEngine.Random.Range(1, 5);
                        string randomname = GenderateName() + __instance.gameObject.name.Replace("(Clone)", "").Replace("DG_", "");
                        // dungeon type
                        string gentype = Enum.GetName(typeof(Algorithm), __instance.m_algorithm);

                        //store values
                        var storeme = new Location7d2d { zone = zone, level7d2d = randomLevel, name7d2d = randomname };
                        mod.locations7d2d.Add(zone, storeme);

                        zdo.Set("7d2d_name", randomname);
                        zdo.Set("7d2d_difficulty", randomLevel);

                        // change dungeon room placement minmax
                        __instance.m_minRooms = __instance.m_minRooms + (int)(__instance.m_minRooms * (randomLevel / 2.0f));
                        __instance.m_maxRooms = __instance.m_minRooms * 4;

                        //change camp size
                        //also need to change location size since camps aren't instanced
                        if (__instance.m_algorithm == Algorithm.CampRadial)
                        {
                            __instance.m_campRadiusMin += (randomLevel * 5);
                            __instance.m_campRadiusMax += (randomLevel * 5);
                            var camploc = __instance.transform.GetComponentInParent<Location>();
                            if (camploc)
                            {
                                camploc.m_exteriorRadius = __instance.m_campRadiusMax;
                            }
                        }

                        mod.Logger.LogWarning($"dg.g prefix new generator {__instance.gameObject.name} {randomname} {randomLevel} type:{gentype} rooms:{__instance.m_minRooms} radius:{__instance.m_campRadiusMax}");
                    }
                    else
                    {
                        mod.Logger.LogInfo($"dg.g prefix location already generated {__instance.gameObject.name}");
                    }
                }
                else
                {
                    mod.Logger.LogError($"dg.g prefix invalid object? {__instance.name}");
                }
            }
        }

        // helper gets objects inside dungeon
        private static List<ZNetView> GetDungeonObjects(ref DungeonGenerator dg)
        {
            List<ZNetView> ret = new List<ZNetView>();

            //center of zone
            var zone = ZoneSystem.GetZone(dg.transform.position);
            var zonecenter = ZoneSystem.GetZonePos(zone);

            UnityEngine.Object[] views = FindObjectsOfType(typeof(ZNetView));
            foreach (var v in views)
            {
                var view = v as ZNetView;
                var pos = view.gameObject.transform.position;

                if (pos.x >= zonecenter.x - ZoneSystem.c_ZoneHalfSize
                    && pos.x <= zonecenter.x + ZoneSystem.c_ZoneHalfSize
                    && pos.z >= zonecenter.z - ZoneSystem.c_ZoneHalfSize
                    && pos.z <= zonecenter.z + ZoneSystem.c_ZoneHalfSize)
                {
                    if (dg.m_algorithm == DungeonGenerator.Algorithm.Dungeon)
                    {
                        if (pos.y > 5000f && pos.y < 6000f)
                        {
                            ret.Add(view);
                        }
                    }

                    if (dg.m_algorithm == DungeonGenerator.Algorithm.CampGrid)
                    {
                        var radius = dg.m_gridSize / 2 * dg.m_tileWidth;
                        if (pos.x >= dg.transform.position.x - radius
                        && pos.x <= dg.transform.position.x + radius
                        && pos.z >= dg.transform.position.z - radius
                        && pos.z <= dg.transform.position.z + radius)
                        {
                            ret.Add(view);
                        }
                    }

                    if (dg.m_algorithm == DungeonGenerator.Algorithm.CampRadial)
                    {
                        if (pos.x >= dg.transform.position.x - dg.m_campRadiusMax
                        && pos.x <= dg.transform.position.x + dg.m_campRadiusMax
                        && pos.z >= dg.transform.position.z - dg.m_campRadiusMax
                        && pos.z <= dg.transform.position.z + dg.m_campRadiusMax)
                        {
                            ret.Add(view);
                        }
                    }
                }
            }

            mod.Logger.LogInfo($"found {ret.Count} dungeon objects");

            return ret;
        }

        // locationproxy awake gets run on the server and client
        // SERVER - it only runs right after a location is generated! 
        // this allows the server to initialize the location 7d2d params from the dungeon generator
        // they are never spawned on the server, this is why spawnlocation postfix dont work
        [HarmonyPatch(typeof(LocationProxy), nameof(LocationProxy.Awake))]
        private static class locationawake
        {
            private static void Postfix(LocationProxy __instance)
            {
                var go = __instance.gameObject;
                if (go == null)
                {
                    return;
                }

                var zone = ZoneSystem.GetZone(__instance.gameObject.transform.position);
                if (zone == null || zone == Vector2i.zero)
                {
                    return;
                }

                var nview = __instance.m_nview;
                if (nview == null || !nview.isActiveAndEnabled)
                {
                    return;
                }

                var zdo = __instance.m_nview.GetZDO();
                if (zdo == null || !zdo.IsValid())
                {
                    return;
                }

                if (!ZNet.instance.IsServer())
                {
                    return;
                }


                var name = zdo.GetString("7d2d_name");
                if (String.IsNullOrWhiteSpace(name))
                {
                    //initialize the 7d2d location from our internal object store
                    mod.locations7d2d.TryGetValue(zone, out Location7d2d interalloc);
                    if (interalloc != null)
                    {
                        //set them so they show up in UI
                        zdo.Set("7d2d_name", interalloc.name7d2d);
                        zdo.Set("7d2d_difficulty", interalloc.level7d2d);
                        mod.Logger.LogWarning($"7d2d location initialized {interalloc.name7d2d} {interalloc.level7d2d}");
                    }
                }

                name = zdo.GetString("7d2d_name");
                var diff = zdo.GetInt("7d2d_difficulty");
                mod.Logger.LogInfo($"lp.a postfix {zone} {name} {diff}");
            }
        }

        [HarmonyPatch(typeof(LocationProxy), nameof(LocationProxy.SpawnLocation))]
        private static class locationspawned
        {
            private static void Postfix(LocationProxy __instance, bool __result)
            {
                //only run this if the location gets actually spawned
                if (!__result)
                {
                    return;
                }

                //ok lets do it
                var go = __instance.gameObject;
                if (go == null)
                {
                    return;
                }

                var zone = ZoneSystem.GetZone(__instance.gameObject.transform.position);
                if (zone == null || zone == Vector2i.zero)
                {
                    return;
                }

                var nview = __instance.m_nview;
                if (nview == null || !nview.isActiveAndEnabled)
                {
                    return;
                }

                var zdo = __instance.m_nview.GetZDO();
                if (zdo == null || !zdo.IsValid())
                {
                    return;
                }

                var loc = __instance.m_instance;
                if (loc == null)
                {
                    return;
                }


                var name = zdo.GetString("7d2d_name");
                if (String.IsNullOrWhiteSpace(name))
                {
                    //initialize the 7d2d location from our internal object store
                    mod.locations7d2d.TryGetValue(zone, out Location7d2d interalloc);
                    if (interalloc != null)
                    {
                        //set them so they show up in UI
                        zdo.Set("7d2d_name", interalloc.name7d2d);
                        zdo.Set("7d2d_difficulty", interalloc.level7d2d);
                        mod.Logger.LogWarning($"7d2d location initialized {loc?.name} {interalloc.name7d2d} {interalloc.level7d2d}");
                    }
                }

                name = zdo.GetString("7d2d_name");
                var diff = zdo.GetInt("7d2d_difficulty");
                mod.Logger.LogInfo($"lp.sl postfix {zone} {loc?.name} {name} {diff}");
            }
        }

        [HarmonyPatch(typeof(SpawnArea), nameof(SpawnArea.UpdateSpawn))]
        private static class areaawake
        {
            private static void Postfix(SpawnArea __instance)
            {
                // is it valid? 
                if (__instance.m_nview && __instance.m_nview.IsValid() && __instance.m_nview.IsOwner())
                {
                    // is it in a location?
                    var loc = Location.GetLocation(__instance.gameObject.transform.position);
                    if (loc != null)
                    {
                        var proxyobject = loc.transform.parent;
                        if (proxyobject != null)
                        {
                            var proxy = proxyobject.GetComponent<LocationProxy>();
                            if (proxy != null)
                            {
                                var zdol = proxy.m_nview.GetZDO();
                                var zdos = __instance.m_nview.GetZDO();
                                if (zdol != null)
                                {
                                    var diff = zdol.GetInt("7d2d_difficulty");
                                    var name = zdol.GetString("7d2d_name");
                                    //if its in a 7d2d location
                                    if (diff > 0)
                                    {
                                        //change spawn difficulty
                                        if (__instance.m_prefabs != null && __instance.m_prefabs.Count > 0)
                                        {
                                            foreach (var prefab in __instance.m_prefabs)
                                            {
                                                prefab.m_minLevel = Math.Max(prefab.m_minLevel, diff - 2);
                                                prefab.m_maxLevel = diff;
                                            }
                                        }

                                        //if its never been 7d2d initialized
                                        if (diff != zdos.GetInt("7d2d_difficulty"))
                                        {
                                            mod.Logger.LogInfo($"sa.a postfix new 7d2d {__instance.name} {name} {diff}");
                                            //init
                                            zdos.Set("7d2d_difficulty", diff);
                                            zdos.Set("7d2d_name", name);


                                            //detele any existing mobs
                                            List<GameObject> deleteThese = new List<GameObject>();
                                            for (int i = 0; i < BaseAI.BaseAIInstances.Count; i++)
                                            {
                                                var baseAIInstance = BaseAI.BaseAIInstances[i];
                                                if (__instance.IsSpawnPrefab(baseAIInstance.gameObject))
                                                {
                                                    //check distance
                                                    float dist = Utils.DistanceXZ(baseAIInstance.transform.position, __instance.transform.position);
                                                    if (dist <= __instance.m_farRadius)
                                                    {
                                                        deleteThese.Add(baseAIInstance.gameObject);
                                                    }
                                                }
                                            }

                                            //delete em?
                                            foreach (GameObject obj in deleteThese)
                                            {
                                                if (obj)
                                                {
                                                    ZNetScene.instance.Destroy(obj);
                                                }
                                            }

                                            //respawn one lol
                                            __instance.SpawnOne();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(CreatureSpawner), nameof(CreatureSpawner.UpdateSpawner))]
        private static class creaturespawnerawake
        {
            private static void Postfix(CreatureSpawner __instance)
            {
                // is it valid? 
                if (__instance.m_nview && __instance.m_nview.IsValid() && __instance.m_nview.IsOwner() && __instance.gameObject)
                {
                    // is it in a location?
                    var loc = Location.GetLocation(__instance.gameObject.transform.position);
                    if (loc != null)
                    {
                        var proxyobject = loc.transform.parent;
                        if (proxyobject != null)
                        {
                            var proxy = proxyobject.GetComponent<LocationProxy>();
                            if (proxy != null)
                            {
                                var zdol = proxy.m_nview.GetZDO();
                                var zdos = __instance.m_nview.GetZDO();
                                if (zdol != null && zdos != null)
                                {
                                    var diff = zdol.GetInt("7d2d_difficulty");
                                    var name = zdol.GetString("7d2d_name");

                                    //if its for a 7d2d location
                                    if (diff > 0)
                                    {
                                        //set the spawner parameters
                                        __instance.m_minLevel = Math.Max(__instance.m_minLevel, diff - 2);
                                        __instance.m_maxLevel = diff;
                                        __instance.m_levelupChance = diff * 10;

                                        //if its never been initialized by 7d2d
                                        if (diff != zdos.GetInt("7d2d_difficulty"))
                                        {
                                            mod.Logger.LogInfo($"cs.a postfix new {__instance.name} {name} {diff}");
                                            //init
                                            zdos.Set("7d2d_difficulty", diff);
                                            zdos.Set("7d2d_name", name);

                                            //delete any existing mob
                                            ZDOID connectionZDOID = zdos.GetConnectionZDOID(ZDOExtraData.ConnectionType.Spawned);
                                            __instance.SpawnedCreatureStillExists(connectionZDOID);
                                            var connzdo = ZDOMan.instance.GetZDO(connectionZDOID);
                                            if (connzdo != null)
                                            {
                                                ZDOMan.instance.DestroyZDO(connzdo);
                                                zdos.SetConnection(ZDOExtraData.ConnectionType.Spawned, ZDOID.None);
                                                //force respawn
                                                __instance.Spawn();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Container), nameof(Container.CheckForChanges))]
        private static class containerawake
        {
            private static void Postfix(Container __instance)
            {
                // is it valid? 
                if (__instance.m_nview && __instance.m_nview.IsValid() && __instance.m_nview.IsOwner())
                {
                    // is it in a location?
                    var loc = Location.GetLocation(__instance.gameObject.transform.position);
                    if (loc != null)
                    {
                        var proxyobject = loc.transform.parent;
                        if (proxyobject != null)
                        {
                            var proxy = proxyobject.GetComponent<LocationProxy>();
                            if (proxy != null)
                            {
                                var zdol = proxy.m_nview.GetZDO();
                                if (zdol != null)
                                {
                                    var diff = zdol.GetInt("7d2d_difficulty");
                                    var name = zdol.GetString("7d2d_name");
                                    if (diff > 0)
                                    {
                                        //do not do tombstones
                                        var tomb = __instance.gameObject.GetComponent<TombStone>();
                                        if (tomb)
                                        {
                                            return;
                                        }

                                        //check to see if its been set before
                                        var zdoc = __instance.m_nview.GetZDO();
                                        if (zdoc != null)
                                        {
                                            if (diff != zdoc.GetInt("7d2d_difficulty"))
                                            {
                                                // log it
                                                mod.Logger.LogInfo($"ct.a postfix {__instance.name} new container for 7d2d {name} {diff}");
                                                // this container has never been setup
                                                zdoc.Set("7d2d_difficulty", diff);
                                                // regenerate the container
                                                __instance.m_inventory.RemoveAll();
                                                //set based on dificulty
                                                for (int i = 0; i < __instance.m_defaultItems.m_drops.Count; i++)
                                                {
                                                    var drop = __instance.m_defaultItems.m_drops[i];
                                                    drop.m_stackMax += diff;
                                                }
                                                //regenerate
                                                __instance.AddDefaultItems();
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        private static Text _poiText;
        private static GameObject _panel;


        private static ConfigEntry<bool> _display;
        private static ConfigEntry<int> _fontSize;
        private static ConfigEntry<string> _fontName;
        private static ConfigEntry<Color> _fontColor;
        private static ConfigEntry<bool> _textOutlineEnabled;
        private static ConfigEntry<Color> _textOutlineColor;
        private static ConfigEntry<Color> _backgroundColor;
        private static ConfigEntry<float> _marginBetweenMiniMap;
        private static ConfigEntry<float> _padding;
        private static ConfigEntry<float> _panelWidth;
        private static ConfigEntry<float> _panelHeight;
        private static ConfigEntry<bool> _displayBackground;




        private void FixedUpdate()
        {
            if ((bool)Player.m_localPlayer && (bool)Hud.instance && Traverse.Create((object)Hud.instance).Method("IsVisible", Array.Empty<object>()).GetValue<bool>())
            {
                if (_panel == null)
                {
                    CreatePanel(Hud.instance);
                }
                RectTransform component = _panel.GetComponent<RectTransform>();
                component.anchoredPosition = new Vector2(-140f, -255f - _marginBetweenMiniMap.Value);
                component.sizeDelta = new Vector2(_panelWidth.Value, _panelHeight.Value);
                Image component2 = _panel.GetComponent<Image>();
                component2.enabled = _displayBackground.Value;
                component2.color = _backgroundColor.Value;
                UpdatePoi();
            }
        }

        public static void CreatePanel(Hud hudInstance)
        {
            mod.Logger.LogInfo("Creating panel...");
            _panel = new GameObject("PoiPanel")
            {
                layer = 5
            };
            _panel.transform.SetParent(hudInstance.m_rootObject.transform);
            RectTransform rectTransform = _panel.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(1f, 1f);
            rectTransform.anchorMax = new Vector2(1f, 1f);
            rectTransform.anchoredPosition = new Vector2(-140f, (-255f - _marginBetweenMiniMap.Value));
            rectTransform.sizeDelta = new Vector2(_panelWidth.Value, _panelHeight.Value);
            Sprite sprite = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault((Sprite s) => s.name == "InputFieldBackground");
            Image image = _panel.AddComponent<Image>();
            image.enabled = _displayBackground.Value;
            image.color = _backgroundColor.Value;
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            mod.Logger.LogInfo("Panel created!");
            CreatePoi();
        }
        private static void CreatePoi()
        {
            mod.Logger.LogInfo("Creating biomepoi text...");
            GameObject gameObject = new GameObject("BiomePoi");
            gameObject.layer = 5;
            gameObject.transform.SetParent(_panel.transform);
            RectTransform rectTransform = gameObject.AddComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(0f, 0f);
            rectTransform.sizeDelta = new Vector2(_panelWidth.Value, _panelHeight.Value);
            _poiText = gameObject.AddComponent<Text>();
            _poiText.color = _fontColor.Value;
            _poiText.font = GetFont();
            _poiText.fontSize = _fontSize.Value;
            _poiText.enabled = _display.Value;
            //_biomepoiText.alignment = (TextAnchor)(_reverseTextPositions.Value ? 3 : 5);
            Outline outline = gameObject.AddComponent<Outline>();
            outline.effectColor = _textOutlineColor.Value;
            outline.effectDistance = new Vector2(1f, -1f);
            outline.useGraphicAlpha = true;
            outline.useGUILayout = true;
            outline.enabled = _textOutlineEnabled.Value;
            mod.Logger.LogInfo("BiomePoi created!");
        }

        private void UpdatePoi()
        {
            //var comp = _biomepoiText.GetComponent<RectTransform>();
            if (Player.m_localPlayer != null)
            {
                var currentBiome = Player.m_localPlayer.GetCurrentBiome();
                var biometext = Localization.instance.Localize("$biome_" + currentBiome.ToString().ToLower());

                var loc = Location.GetLocation(Player.m_localPlayer.transform.position);
                var loctext = "";

                if (loc != null)
                {
                    var proxyobject = loc.transform.parent;
                    if (proxyobject != null)
                    {
                        var proxy = proxyobject.GetComponent<LocationProxy>();
                        if (proxy != null)
                        {
                            var zdo = proxy.m_nview.GetZDO();
                            if (zdo != null)
                            {
                                loctext = $"{new string('\u2620', zdo.GetInt("7d2d_difficulty"))} {zdo.GetString("7d2d_name")}";
                            }
                        }
                    }
                }

                //text
                _poiText.text = $"{loctext}";

                //config
                RectTransform component = _poiText.GetComponent<RectTransform>();
                component.sizeDelta = new Vector2(_panelWidth.Value, _panelHeight.Value / 2f);

                _poiText.color = _fontColor.Value;
                _poiText.font = GetFont();
                _poiText.fontSize = _fontSize.Value;
                _poiText.enabled = _display.Value;

                Outline outline = _poiText.GetComponent<Outline>();
                outline.effectColor = _textOutlineColor.Value;
                outline.enabled = _textOutlineEnabled.Value;
            }
        }

        public static Font GetFont()
        {
            Font[] source = Resources.FindObjectsOfTypeAll<Font>();
            Font val = source.FirstOrDefault((Font f) => ((UnityEngine.Object)(object)f).name == _fontName.Value);
            if ((UnityEngine.Object)(object)val == null)
            {
                return source.FirstOrDefault((Font f) => ((UnityEngine.Object)(object)f).name == "AveriaSansLibre-Bold");
            }
            return val;
        }




    }
}