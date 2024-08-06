using HarmonyLib;
using UnityEngine;

namespace vrpstable.Patches
{
    public class Patches
    {
        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.UpdateItemDrag))]
        private class trashhotkeypatch
        {
            public static void Postfix()
            {
                //make sure stuff is ok
                if (InventoryGui.instance != null && InventoryGui.instance.m_dragGo != null)
                {
                    //hotkey is pressed
                    if (Input.GetKeyDown(Mod.instance.confighotkey.Value))
                    {
                        //trash the item
                        ZLog.Log("trash hotkey pressed");
                        droplistener();
                    }
                }
            }
        }

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Awake))]
        private class trashpatch
        {
            public static void Postfix()
            {
                //trash bucket creation
                var inventory = InventoryGui.instance;
                if (inventory != null)
                {
                    //ac_text object
                    var actext = inventory.m_armor.gameObject;
                    if (actext != null)
                    {
                        //armor object
                        var armorbutton = actext.transform.parent;
                        if (armorbutton != null)
                        {
                            //check to see if we already have a trash object
                            var mytrash = armorbutton.parent.Find("1010101110trash");
                            if (mytrash == null)
                            {
                                //ok no trash is created so lets create it
                                ZLog.LogWarning("creating 1010101110trash");
                                var newtrash = GameObject.Instantiate(armorbutton);
                                if (newtrash != null)
                                {
                                    //set parent
                                    newtrash.SetParent(armorbutton.parent);
                                    newtrash.SetAsFirstSibling();
                                    //rename it
                                    newtrash.gameObject.name = "1010101110trash";
                                    //set scale
                                    newtrash.localScale = new Vector3(1, 1, 1);
                                    //move it down
                                    newtrash.localPosition = new Vector3(602, -145, 0);
                                    //hide armor icon
                                    var hideme = newtrash.Find("armor_icon");
                                    if (hideme != null)
                                    {
                                        hideme.gameObject.SetActive(false);
                                    }
                                    //change text to trash
                                    var trashtexto = newtrash.Find("ac_text");
                                    if (trashtexto != null)
                                    {
                                        var trashtextt = trashtexto.GetComponent<UnityEngine.UI.Text>();
                                        if (trashtextt != null)
                                        {
                                            trashtextt.text = "Trash";
                                        }
                                    }
                                    //add button to handle drop
                                    var trashbutton = newtrash.gameObject.AddComponent<UnityEngine.UI.Button>();
                                    if (trashbutton != null)
                                    {
                                        //add listener
                                        trashbutton.onClick.AddListener(droplistener);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void droplistener()
        {
            //lets reload the config incase it got updated since last use
            if (Mod.instance != null)
            {
                Mod.instance.Config.Reload();
            }

            //check to make sure shit is alive
            if (InventoryGui.instance == null && Player.m_localPlayer == null)
            {
                return;
            }

            //check to see if we have a dragged item
            if (InventoryGui.instance.m_dragGo != null)
            {
                ZLog.Log("trash item " + InventoryGui.instance.m_dragItem.m_shared.m_name);
                //check for invalid drag item
                if (!InventoryGui.instance.m_dragInventory.ContainsItem(InventoryGui.instance.m_dragItem))
                {
                    InventoryGui.instance.SetupDragItem(null, null, 1);
                    return;
                }
                //Ok fucking trash it
                if (InventoryGui.instance.m_dragAmount > 0)
                {
                    //unequip it
                    if (InventoryGui.instance.m_dragAmount > InventoryGui.instance.m_dragItem.m_stack)
                    {
                        InventoryGui.instance.m_dragAmount = InventoryGui.instance.m_dragItem.m_stack;
                    }
                    Player.m_localPlayer.RemoveEquipAction(InventoryGui.instance.m_dragItem);
                    Player.m_localPlayer.UnequipItem(InventoryGui.instance.m_dragItem, false);
                    if (Player.m_localPlayer.m_hiddenLeftItem == InventoryGui.instance.m_dragItem)
                    {
                        Player.m_localPlayer.m_hiddenLeftItem = null;
                        Player.m_localPlayer.SetupVisEquipment(Player.m_localPlayer.m_visEquipment, false);
                    }
                    if (Player.m_localPlayer.m_hiddenRightItem == InventoryGui.instance.m_dragItem)
                    {
                        Player.m_localPlayer.m_hiddenRightItem = null;
                        Player.m_localPlayer.SetupVisEquipment(Player.m_localPlayer.m_visEquipment, false);
                    }

                    //remove item from inventory
                    if (InventoryGui.instance.m_dragAmount == InventoryGui.instance.m_dragItem.m_stack)
                    {
                        ZLog.Log("trash all " + InventoryGui.instance.m_dragAmount.ToString() + "  " + InventoryGui.instance.m_dragItem.m_stack.ToString());
                        if (!InventoryGui.instance.m_dragInventory.RemoveItem(InventoryGui.instance.m_dragItem))
                        {
                            ZLog.Log("Was not removed");
                            return;
                        }
                    }
                    else
                    {
                        ZLog.Log("trash some " + InventoryGui.instance.m_dragAmount.ToString() + "  " + InventoryGui.instance.m_dragItem.m_stack.ToString());
                        InventoryGui.instance.m_dragInventory.RemoveItem(InventoryGui.instance.m_dragItem, InventoryGui.instance.m_dragAmount);
                    }

                    //show trashing
                    Player.m_localPlayer.m_zanim.SetTrigger("interact");
                    Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "trashed " + InventoryGui.instance.m_dragItem.m_shared.m_name, InventoryGui.instance.m_dragAmount);

                    //remove item from gui
                    InventoryGui.instance.m_moveItemEffects.Create(InventoryGui.instance.transform.position, Quaternion.identity, null, 1f, -1);
                    InventoryGui.instance.SetupDragItem(null, null, 1);
                    InventoryGui.instance.UpdateCraftingPanel(false);
                }
            }
        }
    }
}