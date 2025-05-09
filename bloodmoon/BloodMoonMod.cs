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
    public partial class BloodMoonMod : BaseUnityPlugin
    {
        // mod info
        public const string pluginAuthor = "1010101110";
        public const string pluginName = "bloodmoon";
        public const string pluginID = pluginAuthor + "." + pluginName;
        public const string pluginVersion = "0.0.1";
        public const string pluginBuildDate = "2025-05-09";
        public static BloodMoonMod mod;

        // mod config
        internal static ConfigEntry<bool> configLocked;

        internal static ConfigEntry<int> configBloodyInterval;
        internal static ConfigEntry<long> configDayLength;


        internal static ConfigEntry<float> configDarkness;
        internal static ConfigEntry<float> configMoonlight;
        internal static ConfigEntry<float> configMoonRedness;
        internal static ConfigEntry<float> configWarnRedness;

        internal static ConfigEntry<bool> configShowDays;
        internal static ConfigEntry<bool> configShowTime;

        internal static ConfigEntry<string> configBloodySpawnMethod;

        internal static ConfigEntry<bool> configDisableRaids;
        internal static ConfigEntry<bool> configHideBranches;

        //variables
        public bool isBloodWarning = false;
        public bool isBloodMoon = false;
        public float warningLerp = 0f;
        public float moonLerp = 0f;
        List<EnvSetup> vanillaEnvs = null;
        Color vanillaSkyTint = Color.clear;
        Color vanillaMoonTint = Color.clear;

        // init harmony
        public static readonly Harmony harmony = new Harmony(typeof(BloodMoonMod).GetCustomAttributes(typeof(BepInPlugin), false).Cast<BepInPlugin>().First<BepInPlugin>().GUID);

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

            // core config
            configDayLength = config("1 - Core", "Day Length", 1200L, "how long a day is in seconds, server side", true);
            configBloodyInterval = config("1 - Core", "Blood Moon Interval", 7, "every x days a blood moon is triggered, default is 7 days", true);
            configDisableRaids = config("1 - Core", "Disable raids", true, "disables all other raids, leaving only the 7d2d raid", true);
            configBloodySpawnMethod = config(
                "1 - Core", 
                "Blood Moon Spawn", 
                "Biome", 
                new ConfigDescription("How mobs are spawned on the blood moon, see mod docs for explanation", new AcceptableValueList<string>("Biome","Boss")), 
                true
            );

            // ui config
            configShowDays = config("2 - UI", "Show Days", true, "shows day number in the ui, client side", false);
            configShowTime = config("2 - UI", "Show Time", true, "shows time in the ui, client side", false);

            // blood moon config
            configDarkness = config("3 - Graphics", "Night Darkness", 0.55f, "this makes it globally darker", true);
            configMoonlight = config("3 - Graphics", "Moonlight", 1f, "this makes the moonlight brighter to make up for global darkness", true);
            configMoonRedness = config("3 - Graphics", "Moon Redness", 0.33f, "how red you want the lighting on blood moon", true);
            configWarnRedness = config("3 - Graphics", "Warn Redness", 0.33f, "how red you want the lighting before the bloom moon to be", true);
            configHideBranches = config("3 - Graphics", "Hide Yggdrasil", true, "Hides the Branches in the skybox for a clearer night sky.", true);

            // ui panels
            _displayUnderMiniMap = config("4 - UI Panel", "Display under minimap", false, "Display under minimap", false);
            _displayBackground = config("4 - UI Panel", "Display background", true, "Display background", false);
            _twentyFourHourClock = config("4 - UI Panel", "24-hour clock", true, "24-hour clock", false);
            _fontSize = config("4 - UI Panel", "Font size", 16, "Font size", false);
            _fontName = config("4 - UI Panel", "Font name", "AveriaSansLibre-Bold", "Font name", false);
            _fontColor = config("4 - UI Panel", "Font color", new Color(1f, 1f, 1f, 0.791f), "Font color", false);
            _textOutlineEnabled = config("4 - UI Panel", "Text outline enabled", true, "Text outline enabled", false);
            _textOutlineColor = config("4 - UI Panel", "Text outline color", Color.black, "Text outline color", false);
            _backgroundColor = config("4 - UI Panel", "Background color", new Color(0f, 0f, 0f, 0.3921569f), "Background color", false);
            _marginBetweenMiniMap = config("4 - UI Panel", "Margin between minimap and this panel", 0f, "Margin between minimap and this panel", false);
            _padding = config("4 - UI Panel", "Padding left and right", 10f, "Padding left and right from text.", false);
            _reverseTextPositions = config("4 - UI Panel", "Reverse text positions", false, "If set to 'true', time will display to the left and day will display to the right.", false);
            _panelWidth = config("4 - UI Panel", "Panel width", 200f, "Panel width", false);
            _panelHeight = config("4 - UI Panel", "Panel height", 40f, "Panel height", false);

            // locks config
            _ = configSync.AddLockingConfigEntry(configLocked);

            BloodMoonMod.harmony.PatchAll();

            Logger.LogInfo("blood moon initialized, feelin spooky?");
        }

        private void OnDestroy()
        {
            Config.Save();
            BloodMoonMod.harmony.UnpatchSelf();
        }

        private bool isHeadless()
        {
            return SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null;
        }

        // disables sleeping on blood moon
        [HarmonyPatch(typeof(Game), nameof(Game.EverybodyIsTryingToSleep))]
        private static class disablesleep
        {
            private static void Postfix(ref bool __result)
            {
                if (mod.isBloodMoon || mod.isBloodWarning)
                {
                    __result = false;
                }
            }
        }

        // triggers for blood moon
        [HarmonyPatch(typeof(EnvMan), nameof(EnvMan.UpdateTriggers))]
        private static class triggerfix
        {
            private static void Postfix()
            {
                // dont do anything if game isn't ready
                if (Player.m_localPlayer == null || _panel == null)
                {
                    return;
                }

                // setters
                bool warn = false;
                bool moon = false;

                var day = EnvMan.instance.GetCurrentDay();

                // it is blood moon day
                if (day > 2 && day % configBloodyInterval.Value == 0)
                {
                    if (EnvMan.instance.m_smoothDayFraction >= 0.5f && EnvMan.instance.m_smoothDayFraction < 0.75f)
                    {
                        warn = true;
                    }

                    if (EnvMan.instance.m_smoothDayFraction >= 0.75f)
                    {
                        moon = true;
                    }
                }

                //it is the day after but still night time
                if (day > 2 && day % configBloodyInterval.Value == 1)
                {
                    if (EnvMan.instance.m_smoothDayFraction < 0.25f)
                    {
                        moon = true;
                    }
                }

                //set setters
                if (mod.isBloodWarning != warn)
                {
                    mod.Logger.LogWarning($"change blood warn status : ${warn.ToString()} ${day} ${EnvMan.instance.m_smoothDayFraction}");
                    mod.isBloodWarning = warn;
                    if (warn)
                    {
                        mod.warningLerp = 0f;
                    }
                }
                if (mod.isBloodMoon != moon)
                {
                    mod.Logger.LogWarning($"change blood moon status : ${moon.ToString()} ${day} ${EnvMan.instance.m_smoothDayFraction}");
                    mod.isBloodMoon = moon;
                    if (moon)
                    {
                        mod.moonLerp = 0f;
                    }
                }
            }
        }

        public void RefreshEnvironments()
        {
            mod.Logger.LogWarning("refreshing environments");

            // reset environment definitions from config
            foreach (var env in EnvMan.instance.m_environments)
            {
                //reset to vanilla values
                var venv = mod.vanillaEnvs.FirstOrDefault(v => v.m_name == env.m_name);
                if (venv != null)
                {
                    //instance.Logger.LogWarning($"refreshing environment ${env.m_name} - m_sunColorNight ${env.m_sunColorNight} > ${venv.m_sunColorNight}");

                    env.m_fogColorSunEvening = venv.m_fogColorSunEvening;
                    env.m_fogColorSunDay = venv.m_fogColorSunDay;
                    env.m_sunColorNight = venv.m_sunColorNight;
                    env.m_ambColorNight = venv.m_ambColorNight;
                    env.m_fogColorNight = venv.m_fogColorNight;
                    env.m_fogColorSunNight = venv.m_fogColorSunNight;
                    env.m_lightIntensityNight = venv.m_lightIntensityNight;
                }
                // set darkness and lumincance from config
                env.m_ambColorNight = Color.Lerp(env.m_ambColorNight, Color.black, configDarkness.Value);
                env.m_fogColorNight = Color.Lerp(env.m_ambColorNight, Color.black, configDarkness.Value);
                env.m_fogColorSunNight = Color.Lerp(env.m_ambColorNight, Color.black, configDarkness.Value);
                env.m_lightIntensityNight = configMoonlight.Value;
            }
        }

        // refreshes environments every morning
        [HarmonyPatch(typeof(EnvMan), nameof(EnvMan.OnMorning))]
        private static class morningfix
        {
            private static void Postfix(Heightmap.Biome biome, EnvSetup currentEnv)
            {
                mod.RefreshEnvironments();
                //reset blood moon tints
                RenderSettings.skybox.SetColor("_SkyTint", mod.vanillaSkyTint);
                RenderSettings.skybox.SetColor("_MoonColor", mod.vanillaMoonTint);
            }
        }

        // store vanilla environments before we change them
        [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.SetupLocations))]
        private static class zoneenvfix
        {
            private static void Postfix()
            {
                // copy vanilla environments
                mod.vanillaEnvs = new List<EnvSetup>();
                EnvMan.instance.m_environments.ForEach(venv =>
                {
                    mod.vanillaEnvs.Add(venv.Clone());
                });

                // process environments
                mod.RefreshEnvironments();
            }
        }

        // patching when game loads
        [HarmonyPatch(typeof(EnvMan), nameof(EnvMan.Awake))]
        private static class envawake
        {
            private static void Postfix()
            {
                //sets day lenth
                if(configDayLength.Value != EnvMan.instance.m_dayLengthSec)
                {
                    EnvMan.instance.m_dayLengthSec = configDayLength.Value;
                }

                //gets vanilla tints
                mod.vanillaSkyTint = RenderSettings.skybox.GetColor("_SkyTint");
                mod.vanillaMoonTint = RenderSettings.skybox.GetColor("_MoonColor");

                // removes yggdrasil branches from the sky for a clearer skybox
                if (configHideBranches.Value)
                {
                    GameObject[] array = System.Array.FindAll<GameObject>(UnityEngine.Object.FindObjectsOfType<GameObject>(), (GameObject obj) => obj.name.StartsWith("YggdrasilBranch"));
                    if (array.Length != 0)
                    {
                        foreach (GameObject gameObject in array)
                        {
                            //remove from sky
                            GameObject.Destroy(gameObject);
                        }
                    }
                }                
            }
        }

        // patches for environment redness
        [HarmonyPatch(typeof(EnvMan), nameof(EnvMan.SetEnv))]
        private static class envfix
        {
            private static void Prefix(ref EnvSetup env, float dayInt, float nightInt, float morningInt, float eveningInt, float dt)
            {
                //turns stuff red
                if (mod.isBloodWarning)
                {
                    mod.warningLerp = Mathf.Min(1f, mod.warningLerp + 0.001f);
                    if (mod.warningLerp < 1f)
                    {
                        foreach (var v in EnvMan.instance.m_environments)
                        {
                            RenderSettings.skybox.SetColor("_SkyTint", Color.Lerp(mod.vanillaSkyTint, Color.red, mod.warningLerp));
                        }
                    }
                }

                if (mod.isBloodMoon)
                {
                    mod.moonLerp = Mathf.Min(1f, mod.moonLerp + 0.001f);
                    if (mod.moonLerp < 1f)
                    {
                        foreach (var v in EnvMan.instance.m_environments)
                        {
                            v.m_sunColorNight = Color.Lerp(v.m_sunColorNight, Color.red, configWarnRedness.Value * mod.moonLerp);
                            RenderSettings.skybox.SetColor("_MoonColor", Color.Lerp(mod.vanillaMoonTint, Color.red, mod.moonLerp));
                            RenderSettings.skybox.SetColor("_SkyTint", Color.Lerp(mod.vanillaSkyTint, Color.red, mod.warningLerp));
                        }
                    }
                }
            }
        }

        // makes random events impossible (forced events still possible)
        [HarmonyPatch(typeof(RandEventSystem), nameof(RandEventSystem.GetPossibleRandomEvents))]
        private static class novalidevents
        {
            private static void Postfix(RandEventSystem __instance, List<KeyValuePair<RandomEvent, Vector3>> __result)
            {
                if (configDisableRaids.Value)
                {
                    __instance.m_lastPossibleEvents.Clear();
                    __result = __instance.m_lastPossibleEvents;
                }
            }
        }
        [HarmonyPatch(typeof(RandEventSystem), nameof(RandEventSystem.GetValidEventPoints))]
        private static class novalideventpoints
        {
            private static void Postfix(RandEventSystem __instance, List<Vector3> __result)
            {
                if (configDisableRaids.Value)
                {
                    __instance.points.Clear();
                    __result = __instance.points;
                }
            }
        }

        // blood moon raid/event
        [HarmonyPatch(typeof(Player), nameof(Player.FixedUpdate))]
        private static class raidfix
        {
            private static void Postfix(Player __instance)
            {
                if (Player.m_localPlayer && !mod.isHeadless())
                {
                    if (__instance.GetPlayerID() == Player.m_localPlayer.GetPlayerID())
                    {
                        if (mod.isBloodMoon)
                        {
                            //throttle the spawning
                            if (UnityEngine.Random.value >= 0.95f)
                            {
                                //check what mobs and players are in range
                                List<Character> characters = new List<Character>();
                                Character.GetCharactersInRange(__instance.transform.position, 100f, characters);
                                int numofmobs = 0, numofplayers = 0;
                                foreach (var c in characters)
                                {
                                    ZNetView nv = c.gameObject.GetComponent<ZNetView>();

                                    if (nv != null && nv.GetZDO().GetBool("bloodmoon"))
                                    {
                                        numofmobs++;
                                    }
                                    if (c.IsPlayer())
                                    {
                                        numofplayers++;
                                    }
                                }

                                //if there aren't too many mobs already, spawn them
                                if (numofmobs < Math.Min(10, numofplayers * 5))
                                {
                                    // pick which mob prefab to spawn
                                    var mobname = "Skeleton";
                                    var mobrandomizer = UnityEngine.Random.value;
                                    var spawnme = Heightmap.Biome.Meadows;

                                    switch (configBloodySpawnMethod.Value)
                                    {
                                        case "Boss":
                                            spawnme = ZoneSystem.instance.GetGlobalKey("defeated_eikthyr") ? Biome.BlackForest : spawnme;
                                            spawnme = ZoneSystem.instance.GetGlobalKey("defeated_gdking") ? Biome.Swamp : spawnme;
                                            spawnme = ZoneSystem.instance.GetGlobalKey("defeated_bonemass") ? Biome.Mountain : spawnme;
                                            spawnme = ZoneSystem.instance.GetGlobalKey("defeated_dragon") ? Biome.Plains : spawnme;
                                            spawnme = ZoneSystem.instance.GetGlobalKey("defeated_goblinking") ? Biome.Mistlands : spawnme;
                                            spawnme = ZoneSystem.instance.GetGlobalKey("defeated_queen") ? Biome.AshLands : spawnme;
                                            //spawnme = ZoneSystem.instance.GetGlobalKey("defeated_fader") ? Biome.AshLands : spawnme;
                                            break;
                                        default:
                                            //default is biome
                                            spawnme = __instance.GetCurrentBiome();
                                            break;
                                    }

                                    switch (spawnme)
                                    {
                                        case Heightmap.Biome.Meadows:
                                            mobname = mobrandomizer > 0.4f ? "Neck" : mobname;
                                            mobname = mobrandomizer > 0.7f ? "Boar" : mobname;
                                            break;
                                        case Heightmap.Biome.BlackForest:
                                            mobname = mobrandomizer > 0.2f ? "Greydwarf" : mobname;
                                            mobname = mobrandomizer > 0.6f ? "Greydwarf_Shaman" : mobname;
                                            mobname = mobrandomizer > 0.8f ? "Greydwarf_Elite" : mobname;
                                            mobname = mobrandomizer > 0.96f ? "Troll" : mobname;
                                            break;
                                        case Heightmap.Biome.Swamp:
                                            mobname = mobrandomizer > 0.2f ? "Draugr" : mobname;
                                            mobname = mobrandomizer > 0.6f ? "Draugr_Elite" : mobname;
                                            mobname = mobrandomizer > 0.8f ? "Wraith" : mobname;
                                            mobname = mobrandomizer > 0.9f ? "Blob" : mobname;
                                            break;
                                        case Heightmap.Biome.Mountain:
                                            mobname = mobrandomizer > 0.2f ? "Wolf" : mobname;
                                            mobname = mobrandomizer > 0.7f ? "Hatchling" : mobname;
                                            mobname = mobrandomizer > 0.98f ? "Fenring" : mobname;
                                            break;
                                        case Heightmap.Biome.Plains:
                                            mobname = mobrandomizer > 0.2f ? "Goblin" : mobname;
                                            mobname = mobrandomizer > 0.8f ? "GoblinShaman" : mobname;
                                            mobname = mobrandomizer > 0.9f ? "GoblinBrute" : mobname;
                                            break;
                                        case Heightmap.Biome.Mistlands:
                                            mobname = mobrandomizer > 0.2f ? "Seeker" : mobname;
                                            mobname = mobrandomizer > 0.8f ? "SeekerBrute" : mobname;
                                            mobname = mobrandomizer > 0.9f ? "Gjall" : mobname;
                                            break;
                                        case Heightmap.Biome.AshLands:
                                            mobname = mobrandomizer > 0.2f ? "Charred_Melee" : mobname;
                                            mobname = mobrandomizer > 0.6f ? "Charred_Archer" : mobname;
                                            mobname = mobrandomizer > 0.7f ? "Charred_Mage" : mobname;
                                            mobname = mobrandomizer > 0.8f ? "Volture" : mobname;
                                            mobname = mobrandomizer > 0.96f ? "Morgen" : mobname;
                                            break;
                                        case Heightmap.Biome.DeepNorth:
                                            mobname = mobrandomizer > 0.2f ? "Wolf" : mobname;
                                            break;
                                        case Heightmap.Biome.Ocean:
                                            mobname = "Serpent";
                                            mobname = mobrandomizer > 0.3f ? "Bat" : mobname;
                                            break;
                                    }

                                    //spawn at random position                                    
                                    var randomPosition = new Vector3(
                                        __instance.transform.position.x + (UnityEngine.Random.Range(25f, 60f) * (UnityEngine.Random.value > 0.5f ? -1 : 1)),
                                        __instance.transform.position.y,
                                        __instance.transform.position.z + (UnityEngine.Random.Range(25f, 60f) * (UnityEngine.Random.value > 0.5f ? -1 : 1))
                                        );
                                    if (ZoneSystem.instance.FindFloor(randomPosition, out var height))
                                    {
                                        randomPosition.y = height;
                                    }

                                    if (!Physics.Raycast(randomPosition, Vector3.up, 100f))
                                    {
                                        var mobPrefab = ZNetScene.instance.GetPrefab(mobname);
                                        mod.Logger.LogInfo($"spawning bloodmoon mob {mobname} at {randomPosition}");
                                        var spawnedCreature = Instantiate(mobPrefab, randomPosition, Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f));
                                        var spawnedai = spawnedCreature.GetComponent<BaseAI>();
                                        ZNetView nv = spawnedCreature.GetComponent<ZNetView>();
                                        if (spawnedai != null)
                                        {
                                            spawnedai.SetHuntPlayer(true);
                                        }
                                        if (nv != null)
                                        {
                                            nv.GetZDO().Set("bloodmoon", true);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        /////////////////////////////////////////////////////////////////////////
        private static GameObject _panel;
        private static Text _dayText;
        private static Text _timeText;

        private static ConfigEntry<bool> _displayUnderMiniMap;

        private static ConfigEntry<bool> _displayBackground;

        private static ConfigEntry<bool> _twentyFourHourClock;

        private static ConfigEntry<int> _fontSize;

        private static ConfigEntry<string> _fontName;

        private static ConfigEntry<Color> _fontColor;

        private static ConfigEntry<bool> _textOutlineEnabled;

        private static ConfigEntry<Color> _textOutlineColor;

        private static ConfigEntry<Color> _backgroundColor;

        private static ConfigEntry<float> _marginBetweenMiniMap;

        private static ConfigEntry<float> _padding;

        private static ConfigEntry<bool> _reverseTextPositions;

        private static ConfigEntry<float> _panelWidth;

        private static ConfigEntry<float> _panelHeight;

        private void FixedUpdate()
        {
            if ((bool)Player.m_localPlayer && !mod.isHeadless() && (bool)Hud.instance && Traverse.Create((object)Hud.instance).Method("IsVisible", Array.Empty<object>()).GetValue<bool>())
            {
                //create it if not yet
                if (_panel == null)
                {
                    CreatePanel(Hud.instance);
                }

                //only active if we have one of our things enabled
                if (configShowDays.Value || configShowTime.Value)
                {
                    _panel.SetActive(true);
                }
                else
                {
                    _panel.SetActive(false);
                    return;
                }

                RectTransform component = _panel.GetComponent<RectTransform>();
                component.anchoredPosition = new Vector2(-140f, _displayUnderMiniMap.Value ? (-255f - _marginBetweenMiniMap.Value) : (-25f + _marginBetweenMiniMap.Value));
                component.sizeDelta = new Vector2(_panelWidth.Value, _panelHeight.Value);
                Image component2 = _panel.GetComponent<Image>();
                component2.enabled = _displayBackground.Value;
                component2.color = _backgroundColor.Value;
                _dayText.enabled = configShowDays.Value;
                _timeText.enabled = configShowTime.Value;
                if (configShowDays.Value)
                {
                    UpdateDay();
                }
                if (configShowTime.Value)
                {
                    UpdateTime();
                }
            }
        }

        public static void CreatePanel(Hud hudInstance)
        {
            mod.Logger.LogInfo("Creating panel...");
            _panel = new GameObject("DayTimePanel")
            {
                layer = 5
            };
            _panel.transform.SetParent(hudInstance.m_rootObject.transform);
            RectTransform rectTransform = _panel.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(1f, 1f);
            rectTransform.anchorMax = new Vector2(1f, 1f);
            rectTransform.anchoredPosition = new Vector2(-140f, _displayUnderMiniMap.Value ? (-255f - _marginBetweenMiniMap.Value) : (-25f + _marginBetweenMiniMap.Value));
            rectTransform.sizeDelta = new Vector2(_panelWidth.Value, _panelHeight.Value);
            Sprite sprite = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault((Sprite s) => s.name == "InputFieldBackground");
            Image image = _panel.AddComponent<Image>();
            image.enabled = _displayBackground.Value;
            image.color = _backgroundColor.Value;
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            mod.Logger.LogInfo("Panel created!");
            CreateDay();
            CreateTime();
        }

        private static void CreateDay()
        {
            mod.Logger.LogInfo("Creating day text...");
            GameObject gameObject = new GameObject("Day");
            gameObject.layer = 5;
            gameObject.transform.SetParent(_panel.transform);
            RectTransform rectTransform = gameObject.AddComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(_reverseTextPositions.Value ? (_panelWidth.Value / 4f - _padding.Value) : (0f - _panelWidth.Value / 4f + _padding.Value), 0f);
            rectTransform.sizeDelta = new Vector2(_panelWidth.Value / 2f, _panelHeight.Value);
            _dayText = gameObject.AddComponent<Text>();
            _dayText.color = _fontColor.Value;
            _dayText.font = GetFont();
            _dayText.fontSize = _fontSize.Value;
            _dayText.enabled = configShowDays.Value;
            _dayText.alignment = (TextAnchor)(_reverseTextPositions.Value ? 5 : 3);
            Outline outline = gameObject.AddComponent<Outline>();
            outline.effectColor = _textOutlineColor.Value;
            outline.effectDistance = new Vector2(1f, -1f);
            outline.useGraphicAlpha = true;
            outline.useGUILayout = true;
            outline.enabled = _textOutlineEnabled.Value;
            mod.Logger.LogInfo("Day created!");
        }

        private static void CreateTime()
        {
            mod.Logger.LogInfo("Creating time text...");
            GameObject gameObject = new GameObject("Time");
            gameObject.layer = 5;
            gameObject.transform.SetParent(_panel.transform);
            RectTransform rectTransform = gameObject.AddComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(_reverseTextPositions.Value ? (0f - _panelWidth.Value / 4f + _padding.Value) : (_panelWidth.Value / 4f - _padding.Value), 0f);
            rectTransform.sizeDelta = new Vector2(_panelWidth.Value / 2f, _panelHeight.Value);
            _timeText = gameObject.AddComponent<Text>();
            _timeText.color = _fontColor.Value;
            _timeText.font = GetFont();
            _timeText.fontSize = _fontSize.Value;
            _timeText.enabled = configShowTime.Value;
            _timeText.alignment = (TextAnchor)(_reverseTextPositions.Value ? 3 : 5);
            Outline outline = gameObject.AddComponent<Outline>();
            outline.effectColor = _textOutlineColor.Value;
            outline.effectDistance = new Vector2(1f, -1f);
            outline.useGraphicAlpha = true;
            outline.useGUILayout = true;
            outline.enabled = _textOutlineEnabled.Value;
            mod.Logger.LogInfo("Time created!");
        }

        private void UpdateDay()
        {
            if (((UnityEngine.Object)(object)_dayText.font).name != _fontName.Value)
            {
                _dayText.font = GetFont();
            }
            RectTransform component = _dayText.GetComponent<RectTransform>();
            component.anchoredPosition = new Vector2(_reverseTextPositions.Value ? (_panelWidth.Value / 4f - _padding.Value) : (0f - _panelWidth.Value / 4f + _padding.Value), 0f);
            component.sizeDelta = new Vector2(_panelWidth.Value / 2f, _panelHeight.Value / 2f);
            _dayText.alignment = (TextAnchor)(_reverseTextPositions.Value ? 5 : 3);
            _dayText.color = mod.isBloodMoon || mod.isBloodWarning ? Color.red : _fontColor.Value;
            _dayText.fontSize = _fontSize.Value;
            _dayText.text = GetCurrentDayText();
            Outline component2 = _dayText.GetComponent<Outline>();
            component2.enabled = _textOutlineEnabled.Value;
            component2.effectColor = _textOutlineColor.Value;
        }

        private void UpdateTime()
        {
            if (((UnityEngine.Object)(object)_timeText.font).name != _fontName.Value)
            {
                _timeText.font = GetFont();
            }
            RectTransform component = _timeText.GetComponent<RectTransform>();
            component.anchoredPosition = new Vector2(_reverseTextPositions.Value ? (0f - _panelWidth.Value / 4f + _padding.Value) : (_panelWidth.Value / 4f - _padding.Value), 0f);
            component.sizeDelta = new Vector2(_panelWidth.Value / 2f, _panelHeight.Value / 2f);
            _timeText.alignment = (TextAnchor)(_reverseTextPositions.Value ? 3 : 5);
            _timeText.color = _fontColor.Value;
            _timeText.fontSize = _fontSize.Value;
            _timeText.text = GetCurrentTimeText();
            Outline component2 = _timeText.GetComponent<Outline>();
            component2.enabled = _textOutlineEnabled.Value;
            component2.effectColor = _textOutlineColor.Value;
        }

        private string GetCurrentDayText()
        {
            if (!EnvMan.instance || Localization.instance == null)
            {
                return null;
            }
            int num = (int)typeof(EnvMan).GetMethod("GetCurrentDay", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(EnvMan.instance, null);
            return Localization.instance.Localize("$msg_newday", num.ToString());
        }

        private string GetCurrentTimeText()
        {
            if (!EnvMan.instance)
            {
                return null;
            }
            float num = (float)typeof(EnvMan).GetField("m_smoothDayFraction", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(EnvMan.instance);
            int num2 = (int)(num * 24f);
            int num3 = (int)((num * 24f - (float)num2) * 60f);
            int second = (int)(((num * 24f - (float)num2) * 60f - (float)num3) * 60f);
            DateTime dateTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, num2, num3, second);
            return dateTime.ToString(_twentyFourHourClock.Value ? "HH:mm" : "hh:mm tt");
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