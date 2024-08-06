using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using System;
using System.Linq;
using UnityEngine;

namespace vrp.Patches
{
    internal class JotunnPatches
    {
        public static void AddVariants()
        {
            PrefabManager.OnVanillaPrefabsAvailable += AddArmor;
        }

        private static void AddArmor()
        {
            //load a single asset bundle
            var bundle = AssetUtils.LoadAssetBundle("1010101110-assettester/Assets/assets");
            Jotunn.Logger.LogInfo(bundle);

            for (int i = 0; i < 20; i++)
            {
                try
                {
                    // Create and add a custom item
                    var bs_prefab = bundle.LoadAsset<GameObject>("Item" + i);
                    if (bs_prefab)
                    {
                        var bs = new CustomItem(bs_prefab, fixReference: false,
                        new ItemConfig
                        {
                            Amount = 1,
                            Requirements = new[]
                            {
                            new RequirementConfig
                            {
                                Item = "Stone",
                            }
                            }
                        });
                        ItemManager.Instance.AddItem(bs);
                    }
                }
                catch (Exception ex)
                {
                    Jotunn.Logger.LogError(ex);
                }
            }

            for (int i = 0; i < 20; i++)
            {
                try
                {
                    // Create and add a custom item
                    var cs_prefab = bundle.LoadAsset<GameObject>("Piece" + i);
                    if (cs_prefab)
                    {
                        ZLog.LogError(cs_prefab.name);

                        //if its a ship
                        var cs_ship = cs_prefab.GetComponent<Ship>();
                        if (cs_ship)
                        {
                            ZLog.LogError("its a ship");
                            var allKids = cs_prefab.GetComponentsInChildren<Transform>();
                            var sail = allKids.Where(k => k.gameObject.name == "sail_full").FirstOrDefault();

                            if (sail)
                            {
                                ZLog.LogError(sail.name);

                                var renderer = sail.gameObject.GetComponent<SkinnedMeshRenderer>();
                                if (renderer)
                                {
                                    ZLog.LogError(renderer.name);

                                    foreach (Material material in renderer.sharedMaterials)
                                    {
                                        string name = material.shader.name;
                                        material.shader = Shader.Find(name);
                                        ZLog.LogError(sail.name + " replacing " + name);
                                    }
                                }
                            }
                        }

                        var cs = new CustomPiece(cs_prefab, false, new PieceConfig
                        {
                            AllowedInDungeons = false,
                            PieceTable = "Hammer",
                            Category = "testing",
                            Requirements = new[]
                            {
                                new RequirementConfig{ Item = "Stone" }
                            }
                        });
                        PieceManager.Instance.AddPiece(cs);
                    }
                }
                catch (Exception ex)
                {
                    Jotunn.Logger.LogError(ex);
                }
            }
        }
    }
}