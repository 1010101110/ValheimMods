using BepInEx;
using HarmonyLib;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using System.Linq;
using UnityEngine;

namespace tripping
{
    [BepInPlugin("1010101110.shrooms", "shrooms", "1.3.0")]
    [BepInDependency(Jotunn.Main.ModGuid)]
    public class Mod : BaseUnityPlugin
    {
        private void Awake()
        {
            //config
            //ConfigurationManagerAttributes isAdminOnly = new ConfigurationManagerAttributes { IsAdminOnly = true };
            //Config.Bind("ServerSyncedConfig", "MushroomBlack", true, new BepInEx.Configuration.ConfigDescription("black mushrooms enabled?", null, isAdminOnly));
            //Config.Bind("ServerSyncedConfig", "MushroomPink", true, new BepInEx.Configuration.ConfigDescription("pink mushrooms enabled?", null, isAdminOnly));
            //Config.Bind("ServerSyncedConfig", "MushroomBlood", true, new BepInEx.Configuration.ConfigDescription("blood mushrooms enabled?", null, isAdminOnly));
            //Config.Bind("ServerSyncedConfig", "MushroomGreen", true, new BepInEx.Configuration.ConfigDescription("green mushrooms enabled?", null, isAdminOnly));
            //Config.Bind("ServerSyncedConfig", "MushroomPurple", true, new BepInEx.Configuration.ConfigDescription("purple mushrooms enabled?", null, isAdminOnly));
            //Config.Bind("ServerSyncedConfig", "MushroomRainbow", true, new BepInEx.Configuration.ConfigDescription("rainbow mushrooms enabled?", null, isAdminOnly));
            //Config.Bind("ServerSyncedConfig", "MushroomRainbow", false, new BepInEx.Configuration.ConfigDescription("use vanilla camera post processing?", null, isAdminOnly));

            Mod.harmony.PatchAll();
            PrefabManager.OnVanillaPrefabsAvailable += AddShrooms;
        }

        private void OnDestroy()
        {
            Mod.harmony.UnpatchSelf();
        }

        private void AddShrooms()
        {
            //load emebedded asset bundle
            var mushbundle = AssetUtils.LoadAssetBundleFromResources("mushroom", typeof(Mod).Assembly);

            //forest
            ItemManager.Instance.AddItem(new CustomItem(
                mushbundle.LoadAsset<GameObject>("MushroomBlack"),
                false,
                new ItemConfig
                {
                    Enabled = false
                })
            );
            PrefabManager.Instance.AddPrefab(new CustomPrefab(
                mushbundle.LoadAsset<GameObject>("Pickable_Mushroom_black"),
                false
            ));

            ZoneManager.Instance.AddCustomVegetation(
                new CustomVegetation(mushbundle.LoadAsset<GameObject>("Pickable_Mushroom_black"),
                false,
                new VegetationConfig
                {
                    Biome = Heightmap.Biome.BlackForest,
                    Max = .1f,
                    BlockCheck = true,
                    GroupSizeMin = 1,
                    GroupSizeMax = 1,
                    GroupRadius = 4f,
                    MinAltitude = 1f,
                }
                ));

            //forest
            ItemManager.Instance.AddItem(new CustomItem(
                mushbundle.LoadAsset<GameObject>("MushroomPink"),
                false,
                new ItemConfig
                {
                    Amount = 2,
                    CraftingStation = "piece_cauldron",
                    Requirements = new RequirementConfig[]
                    {
                        new RequirementConfig{ Item = "MushroomYellow", Amount = 2},
                        new RequirementConfig{ Item = "Raspberry", Amount = 4},
                        new RequirementConfig{ Item = "GreydwarfEye", Amount = 4},
                    }
                })
            );
            PrefabManager.Instance.AddPrefab(new CustomPrefab(
                mushbundle.LoadAsset<GameObject>("Pickable_Mushroom_pink"),
                false
            ));

            //ZoneManager.Instance.AddCustomVegetation(
            //    new CustomVegetation(mushbundle.LoadAsset<GameObject>("Pickable_Mushroom_pink"),
            //    false,
            //    new VegetationConfig
            //    {
            //        Biome = Heightmap.Biome.BlackForest,
            //        Max = .1f,
            //        BlockCheck = true,
            //        GroupSizeMin = 1,
            //        GroupSizeMax = 2,
            //        GroupRadius = 4f,
            //        MinAltitude = 1f,
            //    })
            //);

            //swamp
            ItemManager.Instance.AddItem(new CustomItem(
                mushbundle.LoadAsset<GameObject>("MushroomBlood"),
                false,
                new ItemConfig
                {
                    Amount = 2,
                    CraftingStation = "piece_cauldron",
                    Requirements = new RequirementConfig[]
                    {
                        new RequirementConfig{ Item = "MushroomBlack", Amount = 2},
                        new RequirementConfig{ Item = "Bloodbag", Amount = 4},
                    }
                })
            );
            PrefabManager.Instance.AddPrefab(new CustomPrefab(
                mushbundle.LoadAsset<GameObject>("Pickable_Mushroom_blood"),
                false
            ));

            ZoneManager.Instance.AddCustomVegetation(
                new CustomVegetation(mushbundle.LoadAsset<GameObject>("Pickable_Mushroom_blood"),
                false,
                new VegetationConfig
                {
                    Biome = Heightmap.Biome.Swamp,
                    Max = .1f,
                    BlockCheck = true,
                    GroupSizeMin = 1,
                    GroupSizeMax = 1,
                    GroupRadius = 4f,
                    MinAltitude = 0f,
                    MaxAltitude = .5f,
                })
            );

            //swamp
            ItemManager.Instance.AddItem(new CustomItem(
                mushbundle.LoadAsset<GameObject>("MushroomGreen"),
                false,
                new ItemConfig
                {
                    Enabled = false
                })
            );
            PrefabManager.Instance.AddPrefab(new CustomPrefab(
                mushbundle.LoadAsset<GameObject>("Pickable_Mushroom_green"),
                false
            ));

            ZoneManager.Instance.AddCustomVegetation(
                new CustomVegetation(mushbundle.LoadAsset<GameObject>("Pickable_Mushroom_green"),
                false,
                new VegetationConfig
                {
                    Biome = Heightmap.Biome.Swamp,
                    Max = .1f,
                    BlockCheck = true,
                    GroupSizeMin = 1,
                    GroupSizeMax = 1,
                    GroupRadius = 4f,
                    MinAltitude = .2f,
                    MaxAltitude = 2f,
                })
            );

            //mountain
            ZoneManager.Instance.AddCustomVegetation(
                new CustomVegetation(PrefabManager.Instance.GetPrefab("Pickable_Mushroom_blue"),
                false,
                new VegetationConfig
                {
                    Biome = Heightmap.Biome.Mountain,
                    Max = .1f,
                    BlockCheck = true,
                    GroupSizeMin = 1,
                    GroupSizeMax = 1,
                    GroupRadius = 4f,
                    MinAltitude = 20f,
                })
            );

            //plains
            ItemManager.Instance.AddItem(new CustomItem(
                mushbundle.LoadAsset<GameObject>("MushroomPurple"),
                false,
                new ItemConfig
                {
                    Enabled = false
                })
            );
            PrefabManager.Instance.AddPrefab(new CustomPrefab(
                mushbundle.LoadAsset<GameObject>("Pickable_Mushroom_purple"),
                false
            ));

            //ZoneManager.Instance.AddCustomVegetation(
            //    new CustomVegetation(mushbundle.LoadAsset<GameObject>("Pickable_Mushroom_purple"),
            //    false,
            //    new VegetationConfig
            //    {
            //        Biome = Heightmap.Biome.Plains,
            //        Max = .5f,
            //        BlockCheck = true,
            //        GroupSizeMin = 2,
            //        GroupSizeMax = 4,
            //        GroupRadius = 4f,
            //        MinAltitude = 1f,
            //    })
            //);

            //end game
            ItemManager.Instance.AddItem(new CustomItem(
                mushbundle.LoadAsset<GameObject>("MushroomRainbow"),
                false,
                new ItemConfig
                {
                    Amount = 2,
                    CraftingStation = "piece_cauldron",
                    Requirements = new RequirementConfig[]
                    {
                        new RequirementConfig{ Item = "MushroomBlack", Amount = 1},
                        new RequirementConfig{ Item = "MushroomGreen", Amount = 1},
                        new RequirementConfig{ Item = "MushroomBlue", Amount = 1},
                        new RequirementConfig{ Item = "MushroomPurple", Amount = 1},
                    }
                })
            );
            //this doesn't naturally spawn, but let people do it manually
            PrefabManager.Instance.AddPrefab(mushbundle.LoadAsset<GameObject>("Pickable_Mushroom_rainbow"));
        }

        public static readonly Harmony harmony = new Harmony(typeof(Mod).GetCustomAttributes(typeof(BepInPlugin), false).Cast<BepInPlugin>().First<BepInPlugin>().GUID);
    }
}