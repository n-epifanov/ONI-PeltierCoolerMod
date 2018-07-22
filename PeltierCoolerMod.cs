using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using JetBrains.Annotations;
using STRINGS;

namespace Mods
{
    namespace PeltierCoolerMod
    {
        [HarmonyPatch(typeof(KSerialization.Manager))]
        [HarmonyPatch("GetType")]
        [HarmonyPatch(new Type[] { typeof(string) })]
        class AddBuildingType
        {
            static bool Prefix(string type_name, ref Type __result)
            {
                if (type_name == "PeltierCooler")
                {
                    __result = typeof(PeltierCooler);
                    return false;
                }
                if (type_name == "PeltierCoolerConfig")
                {
                    __result = typeof(PeltierCoolerConfig);
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
        class RegisterBuilding
        {
            static bool Prefix()
            {
                Debug.Log(" === Registering building (prefix) for " + PeltierCoolerConfig.ID);
                Strings.Add("STRINGS.BUILDINGS.PREFABS.PELTIERCOOLER.NAME", UI.FormatAsLink("Peltier Cooler", "PELTIERCOOLER"));
                Strings.Add("STRINGS.BUILDINGS.PREFABS.PELTIERCOOLER.DESC", "Moves heat from one side to another.");
                Strings.Add("STRINGS.BUILDINGS.PREFABS.PELTIERCOOLER.EFFECT", "Cools gas or liquid on one side and heats up on another.");

                int plan_info_idx = Array.FindIndex(TUNING.BUILDINGS.PLANORDER, item => item.category == PlanScreen.PlanCategory.Utilities);
                if (plan_info_idx == -1)
                {
                    Debug.Log(" === ERROR Failed to find PlanInfo for " + PeltierCoolerConfig.ID);
                    return true;
                }
                //Debug.Log(" === found " + plan_info_idx);
                List<string> ls = new List<string>((string[])TUNING.BUILDINGS.PLANORDER[plan_info_idx].data);
                ls.Add(PeltierCoolerConfig.ID);
                TUNING.BUILDINGS.PLANORDER[plan_info_idx].data = (string[])ls.ToArray();

                TUNING.BUILDINGS.COMPONENT_DESCRIPTION_ORDER.Add(PeltierCoolerConfig.ID);

                return true;
            }
            static void Postfix()
            {
                Debug.Log(" === Registering building (postfix) for " + PeltierCoolerConfig.ID);
                object obj = Activator.CreateInstance(typeof(PeltierCoolerConfig));
                BuildingConfigManager.Instance.RegisterBuilding(obj as IBuildingConfig);
            }
        }

        [HarmonyPatch(typeof(Db), "Initialize")]
        class RegisterTech
        {
            private static void Prefix(Db __instance)
            {
                Debug.Log(" === Registering Tech for " + PeltierCoolerConfig.ID);
                List<string> ls = new List<string>((string[])Database.Techs.TECH_GROUPING["HVAC"]);
                ls.Add(PeltierCoolerConfig.ID);
                Database.Techs.TECH_GROUPING["HVAC"] = (string[])ls.ToArray();
            }
        }
    }
}