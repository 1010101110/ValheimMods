using HarmonyLib;
using UnityEngine;

namespace tripping.Patches
{
    //tripping mushroom effects
    //red vanilla - none
    //yellow vanilla - none
    //blue vanilla - trippy camera effect
    //black - kills you
    //blood - red trippy camera effect, high healing
    //green - wishbone
    //pink - pickable object effect
    //purple - random noises
    //rainbow - rainbow camera effect, high health / stam values

    public class TrippingPatches
    {
        private static Color[] rainbowcolors =
        {
            //red
            new Color(5,0,0,1),
            //orange
            new Color(5,2,0,1),
            //yellow
            new Color(5,5,0,1),
            //green
            new Color(0,5,0,1),
            //teal
            new Color(0,3,2,1),
            //blue
            new Color(0,0,5,1),
            //purple
            new Color(3,0,5,1),
            //magenta
            new Color(4,0,2,1),
        };

        private static int rainbowcurrent = 0;
        private static int rainbownext = 1;
        private static float rainbowtimer = 0f;
        private static float rainbowchangeevery = 0.5f;
        private static float blacktimer = 0f;

        [HarmonyPatch(typeof(Player), "FixedUpdate")]
        private class playerposty
        {
            private static void Postfix(ref Player __instance)
            {
                if (__instance.m_nview.IsValid())
                {
                    if (__instance.m_nview.IsOwner())
                    {
                        if (__instance.GetPlayerID() == Player.m_localPlayer.GetPlayerID())
                        {
                            //tasty mead status effect
                            var tipsy = __instance.m_seman.GetStatusEffect("se_alcohol".GetStableHashCode());

                            //mushroom trip effects
                            bool bluetrip = false;
                            bool blacktrip = false;
                            bool bloodtrip = false;
                            bool rainbowtrip = false;
                            bool purpletrip = false;
                            bool greentrip = false;
                            foreach (Player.Food food in Player.m_localPlayer.m_foods)
                            {
                                if (food.m_name == "MushroomBlue")
                                {
                                    bluetrip = true;
                                }
                                if (food.m_name == "MushroomBlack")
                                {
                                    blacktrip = true;
                                    blacktimer += Time.deltaTime;
                                    if (blacktimer > 0.5f)
                                    {
                                        HitData hitData = new HitData();
                                        hitData.m_point = __instance.GetCenterPoint();
                                        hitData.m_damage.m_spirit = 1f;
                                        __instance.ApplyDamage(hitData, true, false, HitData.DamageModifier.Normal);
                                        blacktimer = 0f;
                                    }
                                }
                                if (food.m_name == "MushroomBlood")
                                {
                                    bloodtrip = true;
                                }
                                if (food.m_name == "MushroomRainbow")
                                {
                                    rainbowtrip = true;
                                }
                                if (food.m_name == "MushroomPurple")
                                {
                                    purpletrip = true;
                                }
                                if (food.m_name == "MushroomGreen")
                                {
                                    greentrip = true;
                                }
                            }

                            //wishbone status effect
                            if (greentrip
                                || (Player.m_localPlayer.m_utilityItem != null && Player.m_localPlayer.m_utilityItem.m_shared.m_name.Contains("wishbone"))
                            )
                            {
                                Player.m_localPlayer.m_seman.AddStatusEffect("Wishbone".GetStableHashCode());
                            }
                            else
                            {
                                Player.m_localPlayer.m_seman.RemoveStatusEffect("Wishbone".GetStableHashCode());
                            }

                            //random sound generator
                            if (purpletrip && Time.frameCount % 200 == 0)
                            {
                                var purpleroll = UnityEngine.Random.Range(1, 200);
                                GameObject sfxprefab = null;
                                switch (purpleroll)
                                {
                                    case 100:
                                        sfxprefab = Jotunn.Managers.PrefabManager.Instance.GetPrefab("sfx_ghost_alert");
                                        break;

                                    case 99:
                                        sfxprefab = Jotunn.Managers.PrefabManager.Instance.GetPrefab("sfx_haldor_laugh");
                                        break;

                                    case 98:
                                        sfxprefab = Jotunn.Managers.PrefabManager.Instance.GetPrefab("sfx_land_water");
                                        break;

                                    case 97:
                                        sfxprefab = Jotunn.Managers.PrefabManager.Instance.GetPrefab("sfx_offering");
                                        break;

                                    case 96:
                                        sfxprefab = Jotunn.Managers.PrefabManager.Instance.GetPrefab("sfx_secretfound");
                                        break;

                                    case 95:
                                        sfxprefab = Jotunn.Managers.PrefabManager.Instance.GetPrefab("sfx_tree_fall");
                                        break;

                                    case 94:
                                        sfxprefab = Jotunn.Managers.PrefabManager.Instance.GetPrefab("sfx_wraith_idle");
                                        break;

                                    case 93:
                                        sfxprefab = Jotunn.Managers.PrefabManager.Instance.GetPrefab("sfx_coins_pile_destroyed");
                                        break;

                                    case 92:
                                        sfxprefab = Jotunn.Managers.PrefabManager.Instance.GetPrefab("sfx_eikthyr_idle");
                                        break;

                                    case 91:
                                        sfxprefab = Jotunn.Managers.PrefabManager.Instance.GetPrefab("sfx_gui_craftitem_cauldron");
                                        break;

                                    case 90:
                                        sfxprefab = Jotunn.Managers.PrefabManager.Instance.GetPrefab("sfx_lootspawn");
                                        break;

                                    case 89:
                                        Player.m_localPlayer.UnequipAllItems();
                                        break;

                                    default:
                                        //do nothing
                                        break;
                                }

                                if (sfxprefab)
                                {
                                    //spawn random sound
                                    ZLog.LogError("purple sound " + sfxprefab.name);
                                    UnityEngine.Object.Instantiate(sfxprefab, Player.m_localPlayer.transform.position + Player.m_localPlayer.transform.forward * 2f + Vector3.up, Quaternion.identity);
                                }
                            }

                            var pp = GameCamera.instance.gameObject.GetComponent<UnityEngine.PostProcessing.PostProcessingBehaviour>();

                            if (pp != null)
                            {
                                //vignette
                                if (rainbowtrip)
                                {
                                    pp.m_Vignette.model.enabled = true;
                                    pp.m_Vignette.model.m_Settings.intensity = Mathf.Min(2f, pp.m_Vignette.model.m_Settings.intensity + .01f);

                                    //rainbow switches colors
                                    rainbowtimer += Time.deltaTime;
                                    if (rainbowtimer > rainbowchangeevery)
                                    {
                                        rainbowcurrent = (rainbowcurrent + 1) % rainbowcolors.Length;
                                        rainbownext = (rainbowcurrent + 1) % rainbowcolors.Length;
                                        rainbowtimer = 0f;
                                    }

                                    pp.m_Vignette.model.m_Settings.color = Color.Lerp(rainbowcolors[rainbowcurrent], rainbowcolors[rainbownext], rainbowtimer / rainbowchangeevery);
                                }
                                else if (bloodtrip)
                                {
                                    pp.m_Vignette.model.enabled = true;
                                    pp.m_Vignette.model.m_Settings.color = new Color(5, 0, 0, 1);
                                    pp.m_Vignette.model.m_Settings.intensity = Mathf.Min(1f, pp.m_Vignette.model.m_Settings.intensity + .01f);
                                }
                                else if (blacktrip)
                                {
                                    pp.m_Vignette.model.enabled = true;
                                    pp.m_Vignette.model.m_Settings.color = new Color(0, 0, 0, 1);
                                    pp.m_Vignette.model.m_Settings.intensity = Mathf.Min(.8f, pp.m_Vignette.model.m_Settings.intensity + .01f);
                                }
                                else if (bluetrip)
                                {
                                    pp.m_Vignette.model.enabled = true;
                                    pp.m_Vignette.model.m_Settings.color = new Color(0, 0, 2, 1);
                                    pp.m_Vignette.model.m_Settings.intensity = Mathf.Min(.2f, pp.m_Vignette.model.m_Settings.intensity + .01f);
                                }
                                else
                                {
                                    if (pp.m_Vignette.model.m_Settings.intensity > .05f)
                                    {
                                        pp.m_Vignette.model.m_Settings.intensity = Mathf.Max(.05f, pp.m_Vignette.model.m_Settings.intensity - .001f);
                                    }
                                    else
                                    {
                                        pp.m_Vignette.model.enabled = false;
                                    }
                                }

                                //chromatic aberration
                                if (bluetrip || blacktrip || bloodtrip || rainbowtrip || purpletrip)
                                {
                                    pp.m_ChromaticAberration.model.m_Settings.intensity = Mathf.Min(20f, pp.m_ChromaticAberration.model.m_Settings.intensity + .5f);
                                }
                                else if (tipsy != null)
                                {
                                    pp.m_ChromaticAberration.model.m_Settings.intensity += .003f;
                                }
                                else
                                {
                                    if (pp.m_ChromaticAberration.model.m_Settings.intensity > .14f)
                                    {
                                        pp.m_ChromaticAberration.model.m_Settings.intensity = Mathf.Max(.14f, pp.m_ChromaticAberration.model.m_Settings.intensity - .01f);
                                    }
                                }

                                //color grading
                                if (bluetrip || rainbowtrip)
                                {
                                    if (pp.m_ColorGrading.model.m_Settings.basic.saturation != 2f)
                                    {
                                        pp.m_ColorGrading.model.m_Settings.basic.saturation = Mathf.Min(2f, pp.m_ColorGrading.model.m_Settings.basic.saturation + .05f);
                                        pp.m_ColorGrading.model.isDirty = true;
                                    }
                                }
                                else if (blacktrip || greentrip)
                                {
                                    if (pp.m_ColorGrading.model.m_Settings.basic.saturation != .2f)
                                    {
                                        pp.m_ColorGrading.model.m_Settings.basic.saturation = .2f;
                                        pp.m_ColorGrading.model.isDirty = true;
                                    }
                                }
                                else
                                {
                                    if (pp.m_ColorGrading.model.m_Settings.basic.saturation != 1f)
                                    {
                                        if (pp.m_ColorGrading.model.m_Settings.basic.saturation < 1f)
                                        {
                                            pp.m_ColorGrading.model.m_Settings.basic.saturation = Mathf.Min(1f, pp.m_ColorGrading.model.m_Settings.basic.saturation + .01f);
                                        }
                                        else
                                        {
                                            pp.m_ColorGrading.model.m_Settings.basic.saturation = Mathf.Max(1f, pp.m_ColorGrading.model.m_Settings.basic.saturation - .01f);
                                        }
                                        pp.m_ColorGrading.model.isDirty = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Pickable), "Awake")]
        private class pickableposty
        {
            private static void Postfix(ref Pickable __instance)
            {
                //loop to check for mushroom
                __instance.StartCoroutine(CheckPinkMushroom(__instance));
            }
        }

        public static System.Collections.IEnumerator CheckPinkMushroom(Pickable __instance)
        {
            for (; ; )
            {
                //check pickable object is valid
                if (__instance.m_nview.IsValid())
                {
                    //check player is valid
                    if (Player.m_localPlayer != null && Player.m_localPlayer.m_nview.IsValid())
                    {
                        //check to see if player has eaten a pink shroom
                        bool pinkshroom = false;
                        foreach (Player.Food food in Player.m_localPlayer.m_foods)
                        {
                            if (food.m_name == "MushroomPink")
                            {
                                pinkshroom = true;
                            }
                        }

                        if (__instance.m_picked == false && pinkshroom)
                        {
                            //get game object
                            var parento = __instance.transform.gameObject;
                            if (parento != null)
                            {
                                //check if we need to add our light
                                bool found = false;
                                var floaterstransform = parento.transform.Find("pinkfloaters");
                                if (floaterstransform && floaterstransform.gameObject)
                                {
                                    found = true;
                                }

                                //we didn't find it so add it!
                                if (!found)
                                {
                                    var pinkprefab = Jotunn.Managers.PrefabManager.Instance.GetPrefab("MushroomPink");
                                    if (pinkprefab)
                                    {
                                        var pinkfloaterstransform = pinkprefab.transform.Find("attach/floaters");
                                        if (pinkfloaterstransform && pinkfloaterstransform.gameObject)
                                        {
                                            var pinkfloaters = GameObject.Instantiate(pinkfloaterstransform.gameObject, parento.transform);
                                            pinkfloaters.name = "pinkfloaters";
                                            pinkfloaters.transform.localScale = new Vector3(2, 2, 2);
                                        }
                                        else
                                        {
                                            Jotunn.Logger.LogError("failed to find MushroomPink floaters");
                                        }
                                    }
                                    else
                                    {
                                        Jotunn.Logger.LogError("failed to find MushroomPink");
                                    }
                                }
                            }
                        }
                        else
                        {
                            var parento = __instance.transform.gameObject;
                            if (parento != null)
                            {
                                //remove floaters
                                var floaterstransform = parento.transform.Find("pinkfloaters");
                                if (floaterstransform && floaterstransform.gameObject)
                                {
                                    UnityEngine.Object.Destroy(floaterstransform.gameObject);
                                }
                            }
                        }
                    }
                }

                yield return new WaitForSeconds(UnityEngine.Random.Range(10f, 20f));
            }
        }

        [HarmonyPatch(typeof(Pickable), "SetPicked")]
        private class pickablepostpick
        {
            private static void Postfix(bool picked, ref Pickable __instance)
            {
                if (picked)
                {
                    //remove pink mushroom particles when they get picked
                    var parento = __instance.transform.gameObject;
                    if (parento != null)
                    {
                        //remove floaters
                        var floaterstransform = parento.transform.Find("pinkfloaters");
                        if (floaterstransform && floaterstransform.gameObject)
                        {
                            UnityEngine.Object.Destroy(floaterstransform.gameObject);
                        }
                    }
                }
            }
        }
    }
}