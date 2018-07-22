using System;
using STRINGS;
using TUNING;
using UnityEngine;

// Token: 0x02000009 RID: 9
public class PeltierCoolerConfig : IBuildingConfig
{
    // Token: 0x06000019 RID: 25 RVA: 0x00002B8C File Offset: 0x00000D8C
    public override BuildingDef CreateBuildingDef()
    {
        string id = "PeltierCooler";
        int width = 1;
        int height = 1;
        string anim = "farmtilerotating_kanim";
        int hitpoints = 100;
        float construction_time = 120f;
        float[] tier = TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER3;
        string[] all_METALS = MATERIALS.ALL_METALS;
        float melting_point = 1600f;
        BuildLocationRule build_location_rule = BuildLocationRule.Tile;
        EffectorValues noise_none = NOISE_POLLUTION.NONE;
        BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(id, width, height, anim, hitpoints, construction_time, tier, all_METALS, melting_point, build_location_rule, TUNING.BUILDINGS.DECOR.NONE, noise_none, 0.2f);
        BuildingTemplates.CreateElectricalBuildingDef(buildingDef);
        buildingDef.ViewMode = SimViewMode.None;
        buildingDef.EnergyConsumptionWhenActive = 480f;
        // TODO:
        buildingDef.SelfHeatKilowattsWhenActive = 0f;
        buildingDef.ThermalConductivity = 0.01f;
        buildingDef.Floodable = false;
        buildingDef.Entombable = false;
        buildingDef.IsFoundation = true;
        buildingDef.isSolidTile = true;
        buildingDef.PermittedRotations = PermittedRotations.R360;
        buildingDef.TileLayer = ObjectLayer.FoundationTile;
        buildingDef.ReplacementLayer = ObjectLayer.ReplacementTile;
        buildingDef.ForegroundLayer = Grid.SceneLayer.BuildingBack;
        buildingDef.AudioCategory = "HollowMetal";
        buildingDef.AudioSize = "small";
        buildingDef.BaseTimeUntilRepair = -1f;
        buildingDef.SceneLayer = Grid.SceneLayer.TileFront;
        buildingDef.OverheatTemperature = 1273.15f;
        buildingDef.ConstructionOffsetFilter = new CellOffset[]
        {
            new CellOffset(0, -1)
        };
        return buildingDef;
    }

    // Token: 0x0600001A RID: 26 RVA: 0x00002C48 File Offset: 0x00000E48
    public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
    {
        go.AddOrGet<LoopingSounds>();
        PeltierCooler peltierCooler = go.AddOrGet<PeltierCooler>();
        SimCellOccupier simCellOccupier = go.AddOrGet<SimCellOccupier>();
        simCellOccupier.doReplaceElement = true;
        go.AddOrGet<Insulator>();
    }

    // Token: 0x0600001B RID: 27 RVA: 0x00002CA4 File Offset: 0x00000EA4
    public override void DoPostConfigurePreview(BuildingDef def, GameObject go)
    {
        GeneratedBuildings.RegisterLogicPorts(go, PeltierCoolerConfig.INPUT_PORTS);
    }

    // Token: 0x0600001C RID: 28 RVA: 0x00002CB4 File Offset: 0x00000EB4
    public override void DoPostConfigureUnderConstruction(GameObject go)
    {
        GeneratedBuildings.RegisterLogicPorts(go, PeltierCoolerConfig.INPUT_PORTS);
    }

    // Token: 0x0600001D RID: 29 RVA: 0x00002CC4 File Offset: 0x00000EC4
    public override void DoPostConfigureComplete(GameObject go)
    {
        BuildingTemplates.DoPostConfigure(go);
        GeneratedBuildings.RegisterLogicPorts(go, PeltierCoolerConfig.INPUT_PORTS);
        go.AddOrGet<LogicOperationalController>();
        go.GetComponent<KPrefabID>().prefabInitFn += delegate (GameObject game_object)
        {
            PoweredActiveController.Instance instance = new PoweredActiveController.Instance(game_object.GetComponent<KPrefabID>());
            instance.StartSM();
        };
    }

    // Token: 0x0400000F RID: 15
    public const string ID = "PeltierCooler";

    // Token: 0x04000010 RID: 16
    private static readonly LogicPorts.Port[] INPUT_PORTS = new LogicPorts.Port[]
    {
        LogicPorts.Port.InputPort(LogicOperationalController.PORT_ID, new CellOffset(0, 0), UI.LOGIC_PORTS.CONTROL_OPERATIONAL, false)
    };
}
