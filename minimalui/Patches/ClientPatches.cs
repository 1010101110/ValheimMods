using HarmonyLib;
using UnityEngine;

namespace vrp.Patches
{
    public class ClientPatches
    {
        public static void BottomCenter(ref RectTransform rect)
        {
            bool flag = rect != null;
            if (flag)
            {
                rect.anchorMin = new Vector2(0.5f, 0f);
                rect.anchorMax = new Vector2(0.5f, 0f);
                rect.pivot = new Vector2(0.5f, 0f);
            }
        }

        [HarmonyPatch(typeof(Hud), "SetVisible")]
        public static class MoveHealthPatch
        {
            private static void Postfix(bool visible, ref Hud __instance)
            {
                if (visible)
                {
                    //guardian power
                    BottomCenter(ref __instance.m_gpRoot);
                    __instance.m_gpRoot.anchoredPosition = new Vector2(315f, 15f);

                    //health and food
                    BottomCenter(ref __instance.m_healthPanel);
                    __instance.m_healthPanel.rotation = Quaternion.Euler(0f, 0f, -90f);
                    __instance.m_healthPanel.anchoredPosition = new Vector2(-110f, 250f);

                    //hide these
                    Component[] componentsInChildren = __instance.m_healthPanel.parent.GetComponentsInChildren<Component>();
                    foreach (Component component in componentsInChildren)
                    {
                        if (component.name.Contains("healthicon") || component.name.Contains("foodicon "))
                        {
                            (component.transform as RectTransform).anchoredPosition = new Vector2(10000f, 0f);
                        }

                        if (component.name.Contains("HealthText") || component.name.Contains("food0") || component.name.Contains("food1") || component.name.Contains("food2"))
                        {
                            (component.transform as RectTransform).rotation = Quaternion.Euler(0f, 0f, 0f);
                        }

                        if (component.name == "Health")
                        {
                            (component.transform as RectTransform).anchoredPosition = new Vector2(-30f, 37.8f);
                        }
                    }

                    //effects
                    BottomCenter(ref __instance.m_statusEffectListRoot);
                    __instance.m_statusEffectListRoot.anchoredPosition = new Vector2(500f, 190f);

                    //stamina
                    BottomCenter(ref __instance.m_staminaBar2Root);
                    __instance.m_staminaBar2Root.anchoredPosition = new Vector2(0f, 120f);

                    //minimap
                    RectTransform rectTransform = Minimap.instance.m_smallRoot.transform as RectTransform;
                    rectTransform.anchorMin = new Vector2(1f, 0f);
                    rectTransform.anchorMax = new Vector2(1f, 0f);
                    rectTransform.pivot = Vector2.zero;
                    rectTransform.localScale = new Vector3(1.2f, 1.2f, 0f);
                    rectTransform.anchoredPosition = new Vector2(-260f, 10f);

                    //chat
                    RectTransform rectTransform2 = Chat.instance.m_input.transform.parent.transform as RectTransform;
                    rectTransform2.anchorMin = new Vector2(0f, 0f);
                    rectTransform2.anchorMax = new Vector2(0f, 0f);
                    rectTransform2.pivot = Vector2.zero;
                    rectTransform2.anchoredPosition = new Vector2(10f, 10f);

                    //hotkeys
                    Component[] componentsInChildren2 = __instance.GetComponentsInChildren<Component>();
                    foreach (Component component2 in componentsInChildren2)
                    {
                        RectTransform component3 = component2.GetComponent<RectTransform>();
                        bool flag2 = component3 != null;
                        if (flag2)
                        {
                            bool flag3 = component2.name == "HotKeyBar";
                            if (flag3)
                            {
                                component3.offsetMin = Vector2.zero;
                                component3.offsetMax = Vector2.zero;
                                BottomCenter(ref component3);
                                component3.anchoredPosition = new Vector2(-270f, 80f);
                            }
                            bool flag4 = component2.name == "SelectedInfo";
                            if (flag4)
                            {
                                BottomCenter(ref component3);
                                component3.anchoredPosition = new Vector2(-410f, 100f);
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Hud), "UpdateStamina")]
        public static class staminapositionfix
        {
            public static bool Prefix(ref Player player, ref float dt)
            {
                float stamina = player.GetStamina();
                float maxStamina = player.GetMaxStamina();
                if (stamina < maxStamina)
                {
                    Hud.instance.m_staminaHideTimer = 0f;
                }
                else
                {
                    Hud.instance.m_staminaHideTimer += dt;
                }
                Hud.instance.m_staminaAnimator.SetBool("Visible", true);
                Hud.instance.m_staminaText.text = Mathf.CeilToInt(stamina).ToString();
                Hud.instance.SetStaminaBarSize(maxStamina / 25f * 32f);
                RectTransform rectTransform = Hud.instance.m_staminaBar2Root.transform as RectTransform;
                rectTransform.anchoredPosition = new Vector2(0f, 190f);
                Hud.instance.m_staminaBar2Slow.SetValue(stamina / maxStamina);
                Hud.instance.m_staminaBar2Fast.SetValue(stamina / maxStamina);

                return false;
            }
        }

        [HarmonyPatch(typeof(MessageHud), "Start")]
        public static class messagefix
        {
            public static void Postfix(ref MessageHud __instance)
            {
                RectTransform rectTransform = __instance.m_unlockMsgPrefab.transform as RectTransform;
                BottomCenter(ref rectTransform);
                rectTransform.anchoredPosition = new Vector2(-850f, 600f);
                RectTransform rectTransform2 = MessageHud.instance.m_messageText.transform.parent.transform as RectTransform;
                rectTransform2.offsetMin = Vector2.zero;
                rectTransform2.offsetMax = Vector2.zero;
                BottomCenter(ref rectTransform2);
                rectTransform2.anchoredPosition = new Vector2(-800f, 300f);
            }
        }
    }
}