using BepInEx;
using HarmonyLib;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using System.Linq;

namespace sparring
{
    // Token: 0x02000002 RID: 2
    [BepInPlugin("1010101110.sparring", "sparring", "1.1.0")]
    public class Mod : BaseUnityPlugin
    {
        private void Awake()
        {
            Mod.harmony.PatchAll();
            PrefabManager.OnVanillaPrefabsAvailable += AddWeapons;
        }

        private void OnDestroy()
        {
            Mod.harmony.UnpatchSelf();
        }

        public static readonly Harmony harmony = new Harmony(typeof(Mod).GetCustomAttributes(typeof(BepInPlugin), false).Cast<BepInPlugin>().First<BepInPlugin>().GUID);

        private static void AddWeapons()
        {
            //club
            var ClubSpar = new CustomItem("ClubSpar", "Club", new ItemConfig
            {
                Name = "Sparring Club",
                Description = "a weak club for sparring",
                Amount = 1,
                CraftingStation = "piece_workbench",
                RepairStation = null,
                Requirements = new RequirementConfig[]
    {
                    new RequirementConfig{ Item = "Wood", Amount = 2, AmountPerLevel = 2 }
    }
            });
            ClubSpar.ItemDrop.m_itemData.m_shared.m_damages.m_blunt = 2;
            ClubSpar.ItemDrop.m_itemData.m_shared.m_damages.m_pierce = 0;
            ClubSpar.ItemDrop.m_itemData.m_shared.m_damages.m_slash = 0;
            ClubSpar.ItemDrop.m_itemData.m_shared.m_damagesPerLevel.m_blunt = 1;
            ClubSpar.ItemDrop.m_itemData.m_shared.m_damagesPerLevel.m_pierce = 0;
            ClubSpar.ItemDrop.m_itemData.m_shared.m_damagesPerLevel.m_slash = 0;
            ItemManager.Instance.AddItem(ClubSpar);

            //axe
            var AxeSpar = new CustomItem("AxeSpar", "AxeStone", new ItemConfig
            {
                Name = "Sparring Axe",
                Description = "a weak axe for sparring",
                Amount = 1,
                CraftingStation = "piece_workbench",
                RepairStation = null,
                Requirements = new RequirementConfig[]
    {
                    new RequirementConfig{ Item = "Wood", Amount = 1, AmountPerLevel = 1 },
                    new RequirementConfig{ Item = "Stone", Amount = 1, AmountPerLevel = 1 }
    }
            });
            AxeSpar.ItemDrop.m_itemData.m_shared.m_damages.m_blunt = 0;
            AxeSpar.ItemDrop.m_itemData.m_shared.m_damages.m_pierce = 0;
            AxeSpar.ItemDrop.m_itemData.m_shared.m_damages.m_slash = 2;
            AxeSpar.ItemDrop.m_itemData.m_shared.m_damages.m_chop = 0;
            AxeSpar.ItemDrop.m_itemData.m_shared.m_damagesPerLevel.m_blunt = 0;
            AxeSpar.ItemDrop.m_itemData.m_shared.m_damagesPerLevel.m_pierce = 0;
            AxeSpar.ItemDrop.m_itemData.m_shared.m_damagesPerLevel.m_slash = 1;
            AxeSpar.ItemDrop.m_itemData.m_shared.m_damagesPerLevel.m_chop = 0;
            ItemManager.Instance.AddItem(AxeSpar);

            //sword
            var SwordSpar = new CustomItem("SwordSpar", "SwordBronze", new ItemConfig
            {
                Name = "Sparring Sword",
                Description = "a weak sword for sparring",
                Amount = 1,
                CraftingStation = "piece_workbench",
                RepairStation = null,
                Requirements = new RequirementConfig[]
{
                    new RequirementConfig{ Item = "Wood", Amount = 2, AmountPerLevel = 2 },
}
            });
            SwordSpar.ItemDrop.m_itemData.m_shared.m_damages.m_blunt = 0;
            SwordSpar.ItemDrop.m_itemData.m_shared.m_damages.m_pierce = 0;
            SwordSpar.ItemDrop.m_itemData.m_shared.m_damages.m_slash = 2;
            SwordSpar.ItemDrop.m_itemData.m_shared.m_damagesPerLevel.m_blunt = 0;
            SwordSpar.ItemDrop.m_itemData.m_shared.m_damagesPerLevel.m_pierce = 0;
            SwordSpar.ItemDrop.m_itemData.m_shared.m_damagesPerLevel.m_slash = 1;
            ItemManager.Instance.AddItem(SwordSpar);

            //dagger
            var KnifeSpar = new CustomItem("KnifeSpar", "KnifeFlint", new ItemConfig
            {
                Name = "Sparring Knife",
                Description = "a weak knife for sparring",
                Amount = 1,
                CraftingStation = "piece_workbench",
                RepairStation = null,
                Requirements = new RequirementConfig[]
{
                    new RequirementConfig{ Item = "Wood", Amount = 1, AmountPerLevel = 1 },
                    new RequirementConfig{ Item = "Stone", Amount = 1, AmountPerLevel = 1 }
}
            });
            KnifeSpar.ItemDrop.m_itemData.m_shared.m_damages.m_blunt = 0;
            KnifeSpar.ItemDrop.m_itemData.m_shared.m_damages.m_pierce = 1;
            KnifeSpar.ItemDrop.m_itemData.m_shared.m_damages.m_slash = 1;
            KnifeSpar.ItemDrop.m_itemData.m_shared.m_damagesPerLevel.m_blunt = 0;
            KnifeSpar.ItemDrop.m_itemData.m_shared.m_damagesPerLevel.m_pierce = 1;
            KnifeSpar.ItemDrop.m_itemData.m_shared.m_damagesPerLevel.m_slash = 1;
            ItemManager.Instance.AddItem(KnifeSpar);

            //spear
            var SpearSpar = new CustomItem("SpearSpar", "SpearFlint", new ItemConfig
            {
                Name = "Sparring spear",
                Description = "a weak spear for sparring",
                Amount = 1,
                CraftingStation = "piece_workbench",
                RepairStation = null,
                Requirements = new RequirementConfig[]
{
                    new RequirementConfig{ Item = "Wood", Amount = 1, AmountPerLevel = 1 },
                    new RequirementConfig{ Item = "Stone", Amount = 1, AmountPerLevel = 1 }
}
            });
            SpearSpar.ItemDrop.m_itemData.m_shared.m_damages.m_blunt = 0;
            SpearSpar.ItemDrop.m_itemData.m_shared.m_damages.m_pierce = 2;
            SpearSpar.ItemDrop.m_itemData.m_shared.m_damages.m_slash = 0;
            SpearSpar.ItemDrop.m_itemData.m_shared.m_damagesPerLevel.m_blunt = 0;
            SpearSpar.ItemDrop.m_itemData.m_shared.m_damagesPerLevel.m_pierce = 1;
            SpearSpar.ItemDrop.m_itemData.m_shared.m_damagesPerLevel.m_slash = 0;
            ItemManager.Instance.AddItem(SpearSpar);

            //altgeir
            var AtgeirSpar = new CustomItem("AtgeirSpar", "AtgeirBronze", new ItemConfig
            {
                Name = "Sparring Atgeir",
                Description = "a weak atgeir for sparring",
                Amount = 1,
                CraftingStation = "piece_workbench",
                RepairStation = null,
                Requirements = new RequirementConfig[]
{
                    new RequirementConfig{ Item = "Wood", Amount = 1, AmountPerLevel = 1 },
                    new RequirementConfig{ Item = "Stone", Amount = 1, AmountPerLevel = 1 }
}
            });
            AtgeirSpar.ItemDrop.m_itemData.m_shared.m_damages.m_blunt = 0;
            AtgeirSpar.ItemDrop.m_itemData.m_shared.m_damages.m_pierce = 4;
            AtgeirSpar.ItemDrop.m_itemData.m_shared.m_damages.m_slash = 0;
            AtgeirSpar.ItemDrop.m_itemData.m_shared.m_damagesPerLevel.m_blunt = 0;
            AtgeirSpar.ItemDrop.m_itemData.m_shared.m_damagesPerLevel.m_pierce = 1;
            AtgeirSpar.ItemDrop.m_itemData.m_shared.m_damagesPerLevel.m_slash = 0;
            //var atgeirrender = AtgeirSpar.ItemPrefab.GetComponentInChildren<Renderer>();
            //if (atgeirrender != null)
            //{
            //    var mats = swordrender.materials;
            //    if (mats != null)
            //    {
            //        foreach (var mat in mats)
            //        {
            //            ZLog.LogWarning(mat.name);
            //            mat.SetFloat("_Metallic", 0f);
            //            mat.SetFloat("_Glossiness", 0f);
            //        }
            //    }
            //}
            ItemManager.Instance.AddItem(AtgeirSpar);

            var BaxeSpar = new CustomItem("BaxeSpar", "Battleaxe", new ItemConfig
            {
                Name = "Sparring 2h Axe",
                Description = "a weak 2h axe for sparring",
                Amount = 1,
                CraftingStation = "piece_workbench",
                RepairStation = null,
                Requirements = new RequirementConfig[]
{
                    new RequirementConfig{ Item = "Wood", Amount = 1, AmountPerLevel = 1 },
                    new RequirementConfig{ Item = "Stone", Amount = 1, AmountPerLevel = 1 }
}
            });
            BaxeSpar.ItemDrop.m_itemData.m_shared.m_damages.m_blunt = 0;
            BaxeSpar.ItemDrop.m_itemData.m_shared.m_damages.m_pierce = 0;
            BaxeSpar.ItemDrop.m_itemData.m_shared.m_damages.m_slash = 4;
            BaxeSpar.ItemDrop.m_itemData.m_shared.m_damages.m_chop = 0;
            BaxeSpar.ItemDrop.m_itemData.m_shared.m_damagesPerLevel.m_blunt = 0;
            BaxeSpar.ItemDrop.m_itemData.m_shared.m_damagesPerLevel.m_pierce = 0;
            BaxeSpar.ItemDrop.m_itemData.m_shared.m_damagesPerLevel.m_slash = 4;
            BaxeSpar.ItemDrop.m_itemData.m_shared.m_damagesPerLevel.m_chop = 0;
            ItemManager.Instance.AddItem(BaxeSpar);

            var BowSpar = new CustomItem("BowSpar", "Bow", new ItemConfig
            {
                Name = "Sparring Bow",
                Description = "a weak Bow for sparring",
                Amount = 1,
                CraftingStation = "piece_workbench",
                RepairStation = null,
                Requirements = new RequirementConfig[]
{
                    new RequirementConfig{ Item = "Wood", Amount = 1, AmountPerLevel = 1 },
                    new RequirementConfig{ Item = "Stone", Amount = 1, AmountPerLevel = 1 }
}
            });
            BowSpar.ItemDrop.m_itemData.m_shared.m_damages.m_blunt = 0;
            BowSpar.ItemDrop.m_itemData.m_shared.m_damages.m_pierce = 2;
            BowSpar.ItemDrop.m_itemData.m_shared.m_damages.m_slash = 0;
            BowSpar.ItemDrop.m_itemData.m_shared.m_damages.m_chop = 0;
            BowSpar.ItemDrop.m_itemData.m_shared.m_damagesPerLevel.m_blunt = 0;
            BowSpar.ItemDrop.m_itemData.m_shared.m_damagesPerLevel.m_pierce = 1;
            BowSpar.ItemDrop.m_itemData.m_shared.m_damagesPerLevel.m_slash = 0;
            BowSpar.ItemDrop.m_itemData.m_shared.m_damagesPerLevel.m_chop = 0;
            ItemManager.Instance.AddItem(BowSpar);

            var ShieldSpar = new CustomItem("ShieldSpar", "ShieldWood", new ItemConfig
            {
                Name = "Sparring Shield",
                Description = "a weak shield for sparring",
                Amount = 1,
                CraftingStation = "piece_workbench",
                RepairStation = null,
                Requirements = new RequirementConfig[]
{
                    new RequirementConfig{ Item = "Wood", Amount = 1, AmountPerLevel = 1 },
                    new RequirementConfig{ Item = "Stone", Amount = 1, AmountPerLevel = 1 }
}
            });
            ShieldSpar.ItemDrop.m_itemData.m_shared.m_blockPower = 1;
            ShieldSpar.ItemDrop.m_itemData.m_shared.m_blockPowerPerLevel = 1;
            ItemManager.Instance.AddItem(ShieldSpar);

            var TowerSpar = new CustomItem("TowerSpar", "ShieldWoodTower", new ItemConfig
            {
                Name = "Sparring Tower",
                Description = "a weak tower shield for sparring",
                Amount = 1,
                CraftingStation = "piece_workbench",
                RepairStation = null,
                Requirements = new RequirementConfig[]
{
                    new RequirementConfig{ Item = "Wood", Amount = 1, AmountPerLevel = 1 },
                    new RequirementConfig{ Item = "Stone", Amount = 1, AmountPerLevel = 1 }
}
            });
            ShieldSpar.ItemDrop.m_itemData.m_shared.m_blockPower = 2;
            ShieldSpar.ItemDrop.m_itemData.m_shared.m_blockPowerPerLevel = 1;
            ItemManager.Instance.AddItem(TowerSpar);
        }
    }
}