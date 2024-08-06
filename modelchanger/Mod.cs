using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace modelchanger
{
    [BepInPlugin("1010101110.modelchanger", "modelchanger", "1.0.4")]
    public class Mod : BaseUnityPlugin
    {
        private void Awake()
        {
            Mod.harmony.PatchAll();
        }

        private void OnDestroy()
        {
            Mod.harmony.UnpatchSelf();
        }

        public static readonly Harmony harmony = new Harmony(typeof(Mod).GetCustomAttributes(typeof(BepInPlugin), false).Cast<BepInPlugin>().First<BepInPlugin>().GUID);
        private static RuntimeAnimatorController CustomRuntime;
        private static Dictionary<string, string> Humanoids;
        private static IOrderedEnumerable<KeyValuePair<string, string>> OrderedHumanoids;
        private static Dictionary<string, string> replacementMap = new Dictionary<string, string>();
        public static Dictionary<string, AnimationClip> ExternalAnimations = new Dictionary<string, AnimationClip>();

        private static void ResetPlayerModel(Player p)
        {
            Transform transform = p.transform.Find("NewVisual");
            bool flag = (transform != null) ? transform.gameObject : null;
            if (flag)
            {
                UnityEngine.Object.Destroy(p.transform.Find("NewVisual").gameObject);
            }
            p.m_visual = p.transform.Find("Visual").gameObject;
            p.m_visual.transform.SetSiblingIndex(0);
            p.m_visual.SetActive(true);
            p.m_animator = p.m_visual.GetComponent<Animator>();
            p.m_zanim.m_animator = p.m_visual.GetComponent<Animator>();
            p.m_visEquipment.m_visual = p.m_visual;
            p.GetComponent<FootStep>().m_feet = new Transform[]
            {
                Utils.FindChild(p.m_visual.transform, "LeftFoot"),
                Utils.FindChild(p.m_visual.transform, "RightFoot")
            };
            p.m_visEquipment.m_rightHand = Utils.FindChild(p.m_visual.transform, "RightHand_Attach");
            p.m_visEquipment.m_leftHand = Utils.FindChild(p.m_visual.transform, "LeftHand_Attach");
            p.m_visEquipment.m_helmet = Utils.FindChild(p.m_visual.transform, "Helmet_attach");
            p.m_collider.enabled = true;
        }

        private static void ApplyModelOnPlayer(Player p, string changedModel)
        {
            ResetPlayerModel(p);
            GameObject gameObject = ZNetScene.instance.GetPrefab(changedModel);
            bool flag = !gameObject || !gameObject.GetComponent<Humanoid>();
            if (!flag)
            {
                gameObject = gameObject.GetComponentInChildren<Animator>().gameObject;
                p.m_visual = UnityEngine.Object.Instantiate<GameObject>(gameObject, p.transform);
                p.m_visual.transform.SetSiblingIndex(0);
                p.m_visual.transform.name = "NewVisual";
                Collider collider = CopyComponent<Collider>(ZNetScene.instance.GetPrefab(changedModel).GetComponent<Collider>(), p.m_visual);
                collider.gameObject.layer = p.m_collider.gameObject.layer;
                Transform transform = Utils.FindChild(p.m_visual.transform, "Armature");
                bool flag2 = transform;
                if (flag2)
                {
                    bool flag3 = Vector3.Scale(transform.localScale, p.m_visual.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh.bounds.size).magnitude >= 12f;
                    if (flag3)
                    {
                        transform.localScale = new Vector3(transform.localScale.x / 3.5f, transform.localScale.y / 3.5f, transform.localScale.z / 3.5f);
                        CapsuleCollider capsuleCollider = collider as CapsuleCollider;
                        bool flag4 = capsuleCollider != null;
                        if (flag4)
                        {
                            capsuleCollider.radius /= 3.5f;
                        }
                    }
                }
                p.m_visual.transform.localPosition = Vector3.zero;
                p.m_visual.GetComponent<Animator>().runtimeAnimatorController = CustomRuntime;
                p.m_animator = p.m_visual.GetComponent<Animator>();
                p.m_zanim.m_animator = p.m_visual.GetComponent<Animator>();
                p.transform.Find("Visual").gameObject.SetActive(false);
                p.m_visEquipment.m_visual = p.m_visual;
                bool flag5 = Utils.FindChild(p.m_visual.transform, "RightHand_Attach") != null;
                if (flag5)
                {
                    p.m_visEquipment.m_rightHand = Utils.FindChild(p.m_visual.transform, "RightHand_Attach");
                    p.m_visEquipment.m_leftHand = Utils.FindChild(p.m_visual.transform, "LeftHand_Attach");
                    p.m_visEquipment.m_helmet = Utils.FindChild(p.m_visual.transform, "Helmet_attach");
                }
                else
                {
                    bool flag6 = Utils.FindChild(p.m_visual.transform, "RightAttach") != null;
                    if (flag6)
                    {
                        p.m_visEquipment.m_rightHand = Utils.FindChild(p.m_visual.transform, "RightAttach");
                        p.m_visEquipment.m_leftHand = Utils.FindChild(p.m_visual.transform, "LeftAttach");
                        p.m_visEquipment.m_helmet = Utils.FindChild(p.m_visual.transform, "HelmetAttach");
                    }
                    else
                    {
                        bool flag7 = Utils.FindChild(p.m_visual.transform, "RightHand") != null;
                        if (flag7)
                        {
                            p.m_visEquipment.m_rightHand = Utils.FindChild(p.m_visual.transform, "RightHand");
                            p.m_visEquipment.m_leftHand = Utils.FindChild(p.m_visual.transform, "LeftHand");
                            p.m_visEquipment.m_helmet = Utils.FindChild(p.m_visual.transform, "Head");
                        }
                        else
                        {
                            bool flag8 = Utils.FindChild(p.m_visual.transform, "l_hand") != null;
                            if (flag8)
                            {
                                p.m_visEquipment.m_rightHand = Utils.FindChild(p.m_visual.transform, "r_hand");
                                p.m_visEquipment.m_leftHand = Utils.FindChild(p.m_visual.transform, "l_hand");
                                p.m_visEquipment.m_helmet = Utils.FindChild(p.m_visual.transform, "head");
                            }
                            else
                            {
                                bool flag9 = Utils.FindChild(p.m_visual.transform, "mixamorig:RightHand") != null;
                                if (flag9)
                                {
                                    p.m_visEquipment.m_rightHand = Utils.FindChild(p.m_visual.transform, "mixamorig:RightHand");
                                    p.m_visEquipment.m_leftHand = Utils.FindChild(p.m_visual.transform, "mixamorig:LeftHand");
                                    p.m_visEquipment.m_helmet = Utils.FindChild(p.m_visual.transform, "mixamorig:HeadTop_End");
                                }
                            }
                        }
                    }
                }
                p.m_collider.enabled = false;
                p.GetComponent<FootStep>().m_feet = new Transform[]
                {
                    Utils.FindChild(p.m_visual.transform, "LeftFoot"),
                    Utils.FindChild(p.m_visual.transform, "RightFoot")
                };
            }
        }

        private static void PlayerChangedModel(long sender, ZPackage pkg)
        {
            ZDOID id = pkg.ReadZDOID();
            string changedModel = pkg.ReadString();
            GameObject gameObject = ZNetScene.instance.FindInstance(id);
            bool flag = gameObject && gameObject.GetComponent<Player>();
            if (flag)
            {
                Player component = gameObject.GetComponent<Player>();
                ApplyModelOnPlayer(component, changedModel);
            }
        }

        private static T CopyComponent<T>(T original, GameObject destination) where T : Component
        {
            Type type = original.GetType();
            Component component = destination.AddComponent(type);
            try
            {
                BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                PropertyInfo[] properties = type.GetProperties(bindingAttr);
                foreach (PropertyInfo propertyInfo in properties)
                {
                    bool canWrite = propertyInfo.CanWrite;
                    if (canWrite)
                    {
                        propertyInfo.SetValue(component, propertyInfo.GetValue(original, null), null);
                    }
                }
                FieldInfo[] fields = type.GetFields(bindingAttr);
                foreach (FieldInfo fieldInfo in fields)
                {
                    fieldInfo.SetValue(component, fieldInfo.GetValue(original));
                }
            }
            catch
            {
            }
            return component as T;
        }

        public static RuntimeAnimatorController MakeAOC(Dictionary<string, string> replacement, RuntimeAnimatorController ORIGINAL)
        {
            AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController(ORIGINAL);
            List<KeyValuePair<AnimationClip, AnimationClip>> list = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            foreach (AnimationClip animationClip in animatorOverrideController.animationClips)
            {
                string name = animationClip.name;
                bool flag = replacement.ContainsKey(name);
                if (flag)
                {
                    AnimationClip value = UnityEngine.Object.Instantiate<AnimationClip>(ExternalAnimations[replacement[name]]);
                    list.Add(new KeyValuePair<AnimationClip, AnimationClip>(animationClip, value));
                }
                else
                {
                    list.Add(new KeyValuePair<AnimationClip, AnimationClip>(animationClip, animationClip));
                }
            }
            animatorOverrideController.ApplyOverrides(list);
            return animatorOverrideController;
        }

        [HarmonyPatch(typeof(Player), "Start")]
        private static class PlayerStartPatch
        {
            private static void Postfix(Player __instance)
            {
                bool flag = CustomRuntime == null;
                if (flag)
                {
                    CustomRuntime = MakeAOC(replacementMap, __instance.m_animator.runtimeAnimatorController);
                }
                bool flag2 = !Player.m_localPlayer;
                if (!flag2)
                {
                    string @string = __instance.m_nview.m_zdo.GetString("KGmodelchanged", "");
                    bool flag3 = !string.IsNullOrWhiteSpace(@string);
                    if (flag3)
                    {
                        ApplyModelOnPlayer(__instance, @string);
                    }
                    bool flag4 = Humanoids == null;
                    if (flag4)
                    {
                        Humanoids = new Dictionary<string, string>();
                        foreach (GameObject gameObject in ZNetScene.instance.m_prefabs)
                        {
                            Humanoid component = gameObject.GetComponent<Humanoid>();
                            bool flag5 = component;
                            if (flag5)
                            {
                                bool flag6 = component.m_name != null;
                                if (flag6)
                                {
                                    Humanoids.Add(gameObject.gameObject.name, gameObject.GetComponent<Humanoid>().m_name);
                                }
                                else
                                {
                                    Humanoids.Add(gameObject.gameObject.name, gameObject.gameObject.name);
                                }
                            }
                            OrderedHumanoids = from name in Humanoids
                                               orderby Localization.instance.Localize(name.Value)
                                               select name;
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(ZNetScene), "Awake")]
        private static class AddingZroutMethods
        {
            private static void Postfix()
            {
                ZRoutedRpc.instance.Register<ZPackage>("KGchangemodel", new Action<long, ZPackage>(PlayerChangedModel));
            }
        }

        [HarmonyPatch(typeof(Terminal), "InitTerminal")]
        public static class addcheats
        {
            private static void Postfix()
            {
                new Terminal.ConsoleCommand("modelchange", "", delegate (Terminal.ConsoleEventArgs args)
                {
                    if (args.Length == 2)
                    {
                        if (Humanoids.ContainsKey(args[1]))
                        {
                            Player.m_localPlayer.m_nview.m_zdo.Set("KGmodelchanged", args[1]);
                            ZPackage zpackage = new ZPackage();
                            zpackage.Write(Player.m_localPlayer.GetZDOID());
                            zpackage.Write(args[1]);
                            ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "KGchangemodel", new object[]
                            {
                                zpackage
                            });
                            args.Context.AddString("model changed! " + args[1]);
                        }
                        else
                        {
                            args.Context.AddString("invalid model");
                        }
                    }
                    else
                    {
                        args.Context.AddString("you must specify a model, use modellist to get the options");
                    }
                }, true, false, false, true, false, null, false);

                new Terminal.ConsoleCommand("modellist", "", delegate (Terminal.ConsoleEventArgs args)
                {
                    string lol = "Models you can change to(hopefully): ";
                    foreach (var h in OrderedHumanoids)
                    {
                        lol += h.Key + ", ";
                    }
                    args.Context.AddString(lol);
                }, true, false, false, true, false, null, false);
            }
        }
    }
}