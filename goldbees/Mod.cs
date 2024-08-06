using BepInEx;
using HarmonyLib;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using System.Linq;
using UnityEngine;

namespace goldbees
{
    [BepInPlugin("1010101110.goldbees", "goldbees", "1.0.6")]
    [BepInDependency(Jotunn.Main.ModGuid)]
    public class Mod : BaseUnityPlugin
    {
        private void Awake()
        {
            Mod.harmony.PatchAll();
            PrefabManager.OnVanillaPrefabsAvailable += AddPrefabs;
        }

        private void OnDestroy()
        {
            Mod.harmony.UnpatchSelf();
        }

        private void AddPrefabs()
        {
            //load emebedded asset bundle
            var mybundle = AssetUtils.LoadAssetBundleFromResources("goldbees", typeof(Mod).Assembly);

            //add items
            ItemManager.Instance.AddItem(new CustomItem(
                mybundle.LoadAsset<GameObject>("QueenBeeGold"),
                false,
                new ItemConfig
                {
                    Description = "this sweetie sure is shiny",
                    CraftingStation = "forge",
                    MinStationLevel = 1,
                    Amount = 1,
                    Requirements = new RequirementConfig[]
                    {
                        new RequirementConfig{
                            Item = "QueenBee",
                            Amount = 2
                        },
                        new RequirementConfig{
                            Item = "Coins",
                            Amount = 999
                        },
                    }
                })
            );
            ItemManager.Instance.AddItem(new CustomItem(
                mybundle.LoadAsset<GameObject>("HoneyGold"),
                false,
                new ItemConfig
                {
                    Enabled = false
                })
            );
            ItemManager.Instance.AddItem(new CustomItem(
                mybundle.LoadAsset<GameObject>("Mums"),
                false,
                new ItemConfig
                {
                    Enabled = false
                })
            );
            ItemManager.Instance.AddItem(new CustomItem(
                mybundle.LoadAsset<GameObject>("Edelweiss"),
                false,
                new ItemConfig
                {
                    Enabled = false
                })
            );
            ItemManager.Instance.AddItem(new CustomItem(
                mybundle.LoadAsset<GameObject>("Scabiosa"),
                false,
                new ItemConfig
                {
                    Enabled = false
                })
            );

            //add behive prefab incase people want to spawn it in manually
            PrefabManager.Instance.AddPrefab(mybundle.LoadAsset<GameObject>("BeehiveGold"));

            //flowers vegetation spawners
            ZoneManager.Instance.AddCustomVegetation(
                new CustomVegetation(mybundle.LoadAsset<GameObject>("Pickable_mums"),
                false,
                new VegetationConfig
                {
                    Biome = Heightmap.Biome.Meadows,
                    Max = .75f,
                    BlockCheck = true,
                    InForest = false,
                    GroupSizeMin = 2,
                    GroupSizeMax = 5,
                    GroupRadius = 4f,
                    MinAltitude = 1f,
                    ScaleMin = 1.3f,
                    ScaleMax = 1.8f
                })
            );

            ZoneManager.Instance.AddCustomVegetation(
                new CustomVegetation(mybundle.LoadAsset<GameObject>("Pickable_edelweiss"),
                false,
                new VegetationConfig
                {
                    Biome = Heightmap.Biome.Mountain,
                    Max = 1f,
                    BlockCheck = true,
                    GroupSizeMin = 2,
                    GroupSizeMax = 8,
                    GroupRadius = 4f,
                    MinAltitude = 1f,
                    TerrainDeltaRadius = 2f,
                    MinTerrainDelta = 0f,
                    MaxTerrainDelta = 3f,
                    ScaleMin = .6f,
                    ScaleMax = .8f,
                })
            );

            ZoneManager.Instance.AddCustomVegetation(
                new CustomVegetation(mybundle.LoadAsset<GameObject>("Pickable_scabiosa"),
                false,
                new VegetationConfig
                {
                    Biome = Heightmap.Biome.Plains,
                    Max = .5f,
                    BlockCheck = true,
                    InForest = false,
                    GroupSizeMin = 5,
                    GroupSizeMax = 8,
                    GroupRadius = 4f,
                    MinAltitude = 1f,
                    ScaleMin = .4f,
                    ScaleMax = .6f
                })
            );

            //add buildable beehive
            PieceManager.Instance.AddPiece(new CustomPiece(
                mybundle.LoadAsset<GameObject>("piece_beehive_gold"),
                true,
                new PieceConfig
                {
                    PieceTable = "Hammer",
                    Category = "Crafting",
                    CraftingStation = "piece_workbench",
                    Requirements = new RequirementConfig[]
                    {
                        new RequirementConfig { Item="FineWood", Amount=10},
                        new RequirementConfig { Item="QueenBeeGold", Amount=1, Recover=true },
                    }
                }
            ));

            //soothing status effect
            var soothing = ScriptableObject.CreateInstance<SE_Stats>();
            soothing.name = "Soothin";
            soothing.m_name = "Soothin";
            soothing.m_icon = PrefabManager.Instance.GetPrefab("Honey").GetComponent<ItemDrop>().m_itemData.GetIcon();
            soothing.m_startMessageType = MessageHud.MessageType.TopLeft;
            soothing.m_startMessage = "you feel soothed and comforted.";
            soothing.m_speedModifier = -0.8f;
            soothing.m_healthOverTime = 20f;
            soothing.m_staminaOverTime = 20f;
            soothing.m_healthOverTimeInterval = 2f;
            soothing.m_ttl = 10f;
            var soothingeffect = new CustomStatusEffect(soothing, false);
            ItemManager.Instance.AddStatusEffect(soothingeffect);

            var golden = ScriptableObject.CreateInstance<SE_Stats>();
            golden.name = "Golden";
            golden.m_name = "Golden";
            golden.m_icon = PrefabManager.Instance.GetPrefab("HoneyGold").GetComponent<ItemDrop>().m_itemData.GetIcon();
            golden.m_startMessageType = MessageHud.MessageType.TopLeft;
            golden.m_startMessage = "you feel shiny and comforted.";
            golden.m_speedModifier = -0.8f;
            golden.m_healthOverTime = 30f;
            golden.m_staminaOverTime = 30f;
            golden.m_healthOverTimeInterval = 2f;
            golden.m_ttl = 10f;
            var goldeneffect = new CustomStatusEffect(golden, false);
            ItemManager.Instance.AddStatusEffect(goldeneffect);

            var icey = ScriptableObject.CreateInstance<SE_Stats>();
            icey.name = "Chillin";
            icey.m_name = "Chillin";
            icey.m_icon = PrefabManager.Instance.GetPrefab("Edelweiss").GetComponent<ItemDrop>().m_itemData.GetIcon();
            icey.m_startMessageType = MessageHud.MessageType.TopLeft;
            icey.m_startMessage = "so refreshing! sweet and cool and comforting!";
            icey.m_speedModifier = -0.8f;
            icey.m_healthOverTime = 69f;
            icey.m_healthOverTimeInterval = 1f;
            icey.m_staminaOverTime = 69f;
            icey.m_ttl = 10f;
            var iceyeffect = new CustomStatusEffect(icey, false);
            ItemManager.Instance.AddStatusEffect(iceyeffect);

            var easy = ScriptableObject.CreateInstance<SE_Stats>();
            easy.name = "Easy";
            easy.m_name = "Easy";
            easy.m_icon = PrefabManager.Instance.GetPrefab("Scabiosa").GetComponent<ItemDrop>().m_itemData.GetIcon();
            easy.m_startMessageType = MessageHud.MessageType.TopLeft;
            easy.m_startMessage = "just taking it easy comfortably";
            easy.m_speedModifier = -0.8f;
            easy.m_healthOverTime = 69f;
            easy.m_healthOverTimeInterval = 1f;
            easy.m_staminaOverTime = 420f;
            easy.m_ttl = 10f;
            var easyeffect = new CustomStatusEffect(easy, false);
            ItemManager.Instance.AddStatusEffect(easyeffect);

            /////////////////////////////////////////
            var mushSoothing = new CustomItem("MushSoothing", "MeadBaseTasty", new ItemConfig
            {
                Description = "This mush smells pleasant, throw it in a fermenter?",
                Amount = 1,
                CraftingStation = "piece_cauldron",
                MinStationLevel = 1,
                Requirements = new RequirementConfig[] {
                        new RequirementConfig
                        {
                            Item = "Dandelion",
                            Amount = 5
                        },
                        new RequirementConfig
                        {
                            Item = "Raspberry",
                            Amount = 5
                        },
                        new RequirementConfig
                        {
                            Item = "Honey",
                            Amount = 4
                        }
                    }
            });
            mushSoothing.ItemDrop.m_itemData.m_shared.m_name = "Soothing Mush";
            mushSoothing.ItemDrop.m_itemData.m_shared.m_consumeStatusEffect = soothingeffect.StatusEffect;
            ItemManager.Instance.AddItem(mushSoothing);

            //beer
            var htea = new CustomItem("HoneyTea", "Tankard", new ItemConfig
            {
                Name = "Honey Tea",
                Description = "a soothing tea",
                CraftingStation = "piece_cauldron",
                MinStationLevel = 1,
                Amount = 20,
                Requirements = new RequirementConfig[]
                {
                        new RequirementConfig{
                            Item = "SledgeCheat",
                            Amount = 1
                        },
                }
            });
            htea.ItemDrop.m_itemData.m_shared.m_itemType = ItemDrop.ItemData.ItemType.Consumable;
            htea.ItemDrop.m_itemData.m_shared.m_ammoType = "mead";
            htea.ItemDrop.m_itemData.m_shared.m_attack.m_attackAnimation = "emote_drink";
            htea.ItemDrop.m_itemData.m_shared.m_consumeStatusEffect = soothingeffect.StatusEffect;
            htea.ItemDrop.m_itemData.m_shared.m_maxStackSize = 30;
            ItemManager.Instance.AddItem(htea);

            //ferment recipe
            ItemManager.Instance.AddItemConversion(new CustomItemConversion(new FermenterConversionConfig
            {
                FromItem = "MushSoothing",
                ToItem = "HoneyTea",
                ProducedItems = 10,
            }));

            //////////////////////////////////////////

            /////////////////////////////////////////
            var mushGolden = new CustomItem("MushGolden", "MeadBaseTasty", new ItemConfig
            {
                Description = "This shiny mush, throw it in a fermenter?",
                Amount = 1,
                CraftingStation = "piece_cauldron",
                MinStationLevel = 1,
                Requirements = new RequirementConfig[] {
                        new RequirementConfig
                        {
                            Item = "Mums",
                            Amount = 5
                        },
                        new RequirementConfig
                        {
                            Item = "Blueberries",
                            Amount = 5
                        },
                        new RequirementConfig
                        {
                            Item = "HoneyGold",
                            Amount = 4
                        }
                    }
            });
            mushGolden.ItemDrop.m_itemData.m_shared.m_name = "Golden Mush";
            mushGolden.ItemDrop.m_itemData.m_shared.m_consumeStatusEffect = goldeneffect.StatusEffect;
            ItemManager.Instance.AddItem(mushGolden);

            //beer
            var gtea = new CustomItem("GoldenTea", "Tankard", new ItemConfig
            {
                Name = "Golden Tea",
                Description = "a radiant tea",
                CraftingStation = "piece_cauldron",
                MinStationLevel = 2,
                Amount = 20,
                Requirements = new RequirementConfig[]
                {
                        new RequirementConfig{
                            Item = "SledgeCheat",
                            Amount = 1
                        },
                }
            });
            gtea.ItemDrop.m_itemData.m_shared.m_itemType = ItemDrop.ItemData.ItemType.Consumable;
            gtea.ItemDrop.m_itemData.m_shared.m_ammoType = "mead";
            gtea.ItemDrop.m_itemData.m_shared.m_attack.m_attackAnimation = "emote_drink";
            gtea.ItemDrop.m_itemData.m_shared.m_consumeStatusEffect = goldeneffect.StatusEffect;
            gtea.ItemDrop.m_itemData.m_shared.m_maxStackSize = 30;
            ItemManager.Instance.AddItem(gtea);

            //ferment recipe
            ItemManager.Instance.AddItemConversion(new CustomItemConversion(new FermenterConversionConfig
            {
                FromItem = "MushGolden",
                ToItem = "GoldenTea",
                ProducedItems = 10,
            }));

            //////////////////////////////////////////

            /////////////////////////////////////////
            var mushIced = new CustomItem("MushIced", "MeadBaseTasty", new ItemConfig
            {
                Description = "This mush is cold, throw it in a fermenter?",
                Amount = 1,
                CraftingStation = "piece_cauldron",
                MinStationLevel = 2,
                Requirements = new RequirementConfig[] {
                        new RequirementConfig
                        {
                            Item = "Edelweiss",
                            Amount = 5
                        },
                        new RequirementConfig
                        {
                            Item = "Dandelion",
                            Amount = 5
                        },
                        new RequirementConfig
                        {
                            Item = "Crystal",
                            Amount = 1
                        },
                        new RequirementConfig
                        {
                            Item = "HoneyGold",
                            Amount = 4
                        }
                    }
            });
            mushIced.ItemDrop.m_itemData.m_shared.m_name = "Iced Mush";
            mushIced.ItemDrop.m_itemData.m_shared.m_consumeStatusEffect = iceyeffect.StatusEffect;
            ItemManager.Instance.AddItem(mushIced);

            //beer
            var itea = new CustomItem("IcedTea", "Tankard", new ItemConfig
            {
                Name = "Iced Tea",
                Description = "a chilly tea",
                CraftingStation = "piece_cauldron",
                MinStationLevel = 1,
                Amount = 20,
                Requirements = new RequirementConfig[]
                    {
                        new RequirementConfig{
                            Item = "SledgeCheat",
                            Amount = 1
                        },
                    }
            });
            itea.ItemDrop.m_itemData.m_shared.m_itemType = ItemDrop.ItemData.ItemType.Consumable;
            itea.ItemDrop.m_itemData.m_shared.m_ammoType = "mead";
            itea.ItemDrop.m_itemData.m_shared.m_attack.m_attackAnimation = "emote_drink";
            itea.ItemDrop.m_itemData.m_shared.m_consumeStatusEffect = iceyeffect.StatusEffect;
            itea.ItemDrop.m_itemData.m_shared.m_maxStackSize = 30;
            ItemManager.Instance.AddItem(itea);

            //ferment recipe
            ItemManager.Instance.AddItemConversion(new CustomItemConversion(new FermenterConversionConfig
            {
                FromItem = "MushIced",
                ToItem = "IcedTea",
                ProducedItems = 10,
            }));

            //////////////////////////////////////////

            /////////////////////////////////////////
            var mushEasy = new CustomItem("MushEasy", "MeadBaseTasty", new ItemConfig
            {
                Description = "This mush is flowery, throw it in a fermenter?",
                Amount = 1,
                CraftingStation = "piece_cauldron",
                MinStationLevel = 2,
                Requirements = new RequirementConfig[] {
                        new RequirementConfig
                        {
                            Item = "Mums",
                            Amount = 5
                        },
                        new RequirementConfig
                        {
                            Item = "Scabiosa",
                            Amount = 5
                        },
                        new RequirementConfig
                        {
                            Item = "Cloudberry",
                            Amount = 5
                        },
                        new RequirementConfig
                        {
                            Item = "HoneyGold",
                            Amount = 4
                        }
                    }
            });
            mushEasy.ItemDrop.m_itemData.m_shared.m_name = "Easy Mush";
            mushEasy.ItemDrop.m_itemData.m_shared.m_consumeStatusEffect = easyeffect.StatusEffect;
            ItemManager.Instance.AddItem(mushEasy);

            //beer
            var etea = new CustomItem("EasyTea", "Tankard", new ItemConfig
            {
                Name = "Easy Tea",
                Description = "it's not easy to make but it do make you feel at ease!",
                CraftingStation = "piece_cauldron",
                MinStationLevel = 1,
                Amount = 20,
                Requirements = new RequirementConfig[]
                    {
                        new RequirementConfig{
                            Item = "SledgeCheat",
                            Amount = 1
                        },
                    }
            });
            etea.ItemDrop.m_itemData.m_shared.m_itemType = ItemDrop.ItemData.ItemType.Consumable;
            etea.ItemDrop.m_itemData.m_shared.m_ammoType = "mead";
            etea.ItemDrop.m_itemData.m_shared.m_attack.m_attackAnimation = "emote_drink";
            etea.ItemDrop.m_itemData.m_shared.m_consumeStatusEffect = easyeffect.StatusEffect;
            etea.ItemDrop.m_itemData.m_shared.m_maxStackSize = 30;
            ItemManager.Instance.AddItem(etea);

            //ferment recipe
            ItemManager.Instance.AddItemConversion(new CustomItemConversion(new FermenterConversionConfig
            {
                FromItem = "MushEasy",
                ToItem = "EasyTea",
                ProducedItems = 10,
            }));

            //////////////////////////////////////////
        }

        public static readonly Harmony harmony = new Harmony(typeof(Mod).GetCustomAttributes(typeof(BepInPlugin), false).Cast<BepInPlugin>().First<BepInPlugin>().GUID);
    }
}