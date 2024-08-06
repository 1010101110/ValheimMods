using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace speedyships.Patches
{
    internal class JotunnPatches
    {
        public static AssetBundle bun;

        public static void AddHooks()
        {
            PrefabManager.OnVanillaPrefabsAvailable += AddMaterials;
            PrefabManager.OnPrefabsRegistered += AddShips;
        }

        private static void AddMaterials()
        {
            bun = AssetUtils.LoadAssetBundleFromResources("ship", Assembly.GetExecutingAssembly());
            Jotunn.Logger.LogInfo($"Embedded resources: {string.Join(",", Assembly.GetExecutingAssembly().GetManifestResourceNames())}");
            Jotunn.Logger.LogInfo(bun);

            addMaterial(bun, "Blubber");
            addMaterial(bun, "SharkTooth");

            PrefabManager.OnVanillaPrefabsAvailable -= AddMaterials;
        }

        private static void AddShips()
        {
            try
            {
                var cs_prefab = bun.LoadAsset<GameObject>("ChitinNails");
                if (cs_prefab)
                {
                    CustomItem ci = new CustomItem(cs_prefab, true, new ItemConfig
                    {
                        CraftingStation = "forge",
                        MinStationLevel = 1,
                        Amount = 10,
                        Requirements = new RequirementConfig[]
                        {
                            new RequirementConfig
                            {
                                Item = "Chitin",
                                Amount = 10
                            },
                        }
                    });
                    ItemManager.Instance.AddItem(ci);
                }
            }
            catch (Exception ex)
            {
                Jotunn.Logger.LogError("speedyships failed to add item " + "ChitinNails");
                Jotunn.Logger.LogError(ex);
            }

            try
            {
                var cs_prefab = bun.LoadAsset<GameObject>("SlickWood");
                if (cs_prefab)
                {
                    CustomItem ci = new CustomItem(cs_prefab, true, new ItemConfig
                    {
                        CraftingStation = "piece_workbench",
                        MinStationLevel = 2,
                        Amount = 10,
                        Requirements = new RequirementConfig[]
                        {
                            new RequirementConfig
                            {
                                Item = "Blubber",
                                Amount = 1
                            },
                            new RequirementConfig
                            {
                                Item = "Resin",
                                Amount = 1
                            },
                            new RequirementConfig
                            {
                                Item = "RoundLog",
                                Amount = 1
                            },
                        }
                    });
                    ItemManager.Instance.AddItem(ci);
                }
            }
            catch (Exception ex)
            {
                Jotunn.Logger.LogError("speedyships failed to add item " + "SlickWood");
                Jotunn.Logger.LogError(ex);
            }

            try
            {
                var cs_prefab = bun.LoadAsset<GameObject>("ClubShark");
                if (cs_prefab)
                {
                    CustomItem ci = new CustomItem(cs_prefab, true, new ItemConfig
                    {
                        Requirements = new RequirementConfig[]
                        {
                            new RequirementConfig
                            {
                                Item = "SlickWood",
                                Amount = 2,
                                AmountPerLevel = 2
                            },
                            new RequirementConfig
                            {
                                Item = "SharkTooth",
                                Amount = 10,
                                AmountPerLevel = 5
                            },
                        }
                    });
                    ItemManager.Instance.AddItem(ci);
                }
            }
            catch (Exception ex)
            {
                Jotunn.Logger.LogError("speedyships failed to add item " + "ClubShark");
                Jotunn.Logger.LogError(ex);
            }

            addPiece(bun, "SpeedKarveW", new[]{
                new RequirementConfig()
                {
                    Item = "SlickWood",
                    Amount = 20
                },
                new RequirementConfig()
                {
                    Item = "DeerHide",
                    Amount = 10
                },
                new RequirementConfig()
                {
                    Item = "ChitinNails",
                    Amount = 10
                },
            });

            addPiece(bun, "SpeedKarveB", new[]{
                new RequirementConfig()
                {
                    Item = "SlickWood",
                    Amount = 20
                },
                new RequirementConfig()
                {
                    Item = "DeerHide",
                    Amount = 10
                },
                new RequirementConfig()
                {
                    Item = "ChitinNails",
                    Amount = 10
                },
            });

            addPiece(bun, "SpeedLongboatW", new[]{
                new RequirementConfig()
                {
                    Item = "SlickWood",
                    Amount = 40
                },
                new RequirementConfig()
                {
                    Item = "Chain",
                    Amount = 2
                },
                new RequirementConfig()
                {
                    Item = "DeerHide",
                    Amount = 10
                },
                new RequirementConfig()
                {
                    Item = "ChitinNails",
                    Amount = 20
                },
            });

            addPiece(bun, "SpeedLongboatB", new[]{
                new RequirementConfig()
                {
                    Item = "SlickWood",
                    Amount = 40
                },
                new RequirementConfig()
                {
                    Item = "Chain",
                    Amount = 2
                },
                new RequirementConfig()
                {
                    Item = "DeerHide",
                    Amount = 10
                },
                new RequirementConfig()
                {
                    Item = "ChitinNails",
                    Amount = 20
                },
            });

            PrefabManager.OnPrefabsRegistered -= AddShips;
        }

        private static void addPrefab(AssetBundle bundle, string prefabname)
        {
            try
            {
                var cs_prefab = bundle.LoadAsset<GameObject>(prefabname);
                if (cs_prefab)
                {
                    PrefabManager.Instance.AddPrefab(cs_prefab);
                }
            }
            catch (Exception ex)
            {
                Jotunn.Logger.LogError("speedyships failed to add prefab " + prefabname);
                Jotunn.Logger.LogError(ex);
            }
        }

        private static void addMaterial(AssetBundle bundle, string prefabname)
        {
            try
            {
                var cs_prefab = bundle.LoadAsset<GameObject>(prefabname);
                if (cs_prefab)
                {
                    CustomItem ci = new CustomItem(cs_prefab, true);
                    ItemManager.Instance.AddItem(ci);
                }
            }
            catch (Exception ex)
            {
                Jotunn.Logger.LogError("speedyships failed to add item " + prefabname);
                Jotunn.Logger.LogError(ex);
            }
        }

        private static void addPiece(AssetBundle bundle, string prefabname, RequirementConfig[] reqs)
        {
            try
            {
                // Create and add a custom item
                var cs_prefab = bundle.LoadAsset<GameObject>(prefabname);
                if (cs_prefab)
                {
                    //if its a ship
                    var cs_ship = cs_prefab.GetComponent<Ship>();
                    if (cs_ship)
                    {
                        var allKids = cs_prefab.GetComponentsInChildren<Transform>();
                        var sail = allKids.Where(k => k.gameObject.name == "sail_full").FirstOrDefault();

                        if (sail)
                        {
                            var renderer = sail.gameObject.GetComponent<SkinnedMeshRenderer>();
                            if (renderer)
                            {
                                foreach (Material material in renderer.sharedMaterials)
                                {
                                    string name = material.shader.name;
                                    material.shader = Shader.Find(name);
                                }
                            }
                        }
                    }

                    var cs = new CustomPiece(cs_prefab, true, new PieceConfig
                    {
                        PieceTable = "Hammer",
                        Category = "Misc",
                        Requirements = reqs
                    });
                    if (cs != null)
                    {
                        PieceManager.Instance.AddPiece(cs);
                    }
                }
            }
            catch (Exception ex)
            {
                Jotunn.Logger.LogError("speedyships failed to add piece " + prefabname);
                Jotunn.Logger.LogError(ex);
            }
        }
    }
}