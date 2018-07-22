using System;
using UnityEngine;

using System.Collections.Generic;
using KSerialization;
using STRINGS;


[SerializationConfig(MemberSerialization.OptIn)]
public class PeltierCooler : StateMachineComponent<PeltierCooler.StatesInstance>
{
    public float MinInputTemperature
    {
        get
        {
            return this.minInputTemperature;
        }
    }

    // Token: 0x17000158 RID: 344
    // (get) Token: 0x06001D23 RID: 7459 RVA: 0x000952B4 File Offset: 0x000934B4
    public float MaxOutputTemperature
    {
        get
        {
            return this.maxOutputTemperature;
        }
    }

    // Token: 0x06001D24 RID: 7460 RVA: 0x000952BC File Offset: 0x000934BC
    protected override void OnPrefabInit()
    {
        base.OnPrefabInit();
    }

    // Token: 0x06001D25 RID: 7461 RVA: 0x000952F4 File Offset: 0x000934F4
    protected override void OnSpawn()
    {
        base.OnSpawn();
        Building component = base.GetComponent<Building>();
        int base_cell = Grid.PosToCell(base.transform.GetPosition());
        this.inputCell = Grid.OffsetCell(base_cell, component.GetRotatedOffset(new CellOffset(0, -1)));
        this.outputCell = Grid.OffsetCell(base_cell, component.GetRotatedOffset(new CellOffset(0, 1)));
        base.smi.StartSM();
    }

    // Token: 0x06001D2B RID: 7467 RVA: 0x00095814 File Offset: 0x00093A14
    public List<Descriptor> GetDescriptors(BuildingDef def)
    {
        List<Descriptor> list = new List<Descriptor>();
        // TODO: ?
        /*string formattedTemperature = GameUtil.GetFormattedTemperature(this.temperatureDelta, GameUtil.TimeSlice.None, GameUtil.TemperatureInterpretation.Relative, true);
        Element element = (!this.isLiquidConditioner) ? ElementLoader.GetElement("Oxygen") : ElementLoader.GetElement("Water");
        float num = Mathf.Abs(this.temperatureDelta * element.specificHeatCapacity);
        Descriptor item = default(Descriptor);
        string txt = string.Format((!this.isLiquidConditioner) ? UI.BUILDINGEFFECTS.HEATGENERATED_AIRCONDITIONER : UI.BUILDINGEFFECTS.HEATGENERATED_LIQUIDCONDITIONER, GameUtil.GetFormattedWattage(num, GameUtil.WattageFormatterUnit.Automatic));
        string tooltip = string.Format((!this.isLiquidConditioner) ? UI.BUILDINGEFFECTS.TOOLTIPS.HEATGENERATED_AIRCONDITIONER : UI.BUILDINGEFFECTS.TOOLTIPS.HEATGENERATED_LIQUIDCONDITIONER, GameUtil.GetFormattedJoules(num, string.Empty, GameUtil.TimeSlice.None));
        item.SetupDescriptor(txt, tooltip, Descriptor.DescriptorType.Effect);
        list.Add(item);
        Descriptor item2 = default(Descriptor);
        item2.SetupDescriptor(string.Format((!this.isLiquidConditioner) ? UI.BUILDINGEFFECTS.GASCOOLING : UI.BUILDINGEFFECTS.LIQUIDCOOLING, formattedTemperature), string.Format((!this.isLiquidConditioner) ? UI.BUILDINGEFFECTS.TOOLTIPS.GASCOOLING : UI.BUILDINGEFFECTS.TOOLTIPS.LIQUIDCOOLING, formattedTemperature), Descriptor.DescriptorType.Effect);
        list.Add(item2);*/
        return list;
    }


    // Token: 0x04001926 RID: 6438
    [MyCmpReq]
    protected Operational operational;

    private const float K = 273.15f;

    private readonly float minInputTemperature = -120f + K;
    private readonly float maxOutputTemperature = 1000f + K;

    private int inputCell = -1;
    private int outputCell = -1;

    private readonly float minCellMass = 0f;

    private readonly float kjTransferRate = 2500f;

    // Token: 0x06002338 RID: 9016 RVA: 0x000AE5F8 File Offset: 0x000AC7F8
    private PeltierCooler.MonitorState MonitorHeating(float dt)
    {
        //Debug.Log(" !!! Monitoring heat ");
        
        if (Grid.Mass[this.inputCell] <= this.minCellMass ||
            Grid.Mass[this.outputCell] <= this.minCellMass)
        {
            return PeltierCooler.MonitorState.NotEnoughMass;
        }

        if (Grid.Temperature[this.inputCell] <= this.minInputTemperature)
        {
            return PeltierCooler.MonitorState.TooCold;
        }

        // XXX: SimMessages.ModifyEnergy() gets close to max_temperature but it's never reached.
        if (Grid.Temperature[this.outputCell] >= this.maxOutputTemperature-10)
        {
            return PeltierCooler.MonitorState.TooHot;
        }

        float kjTransferred = this.kjTransferRate * dt;
        SimMessages.ModifyEnergy(this.inputCell, -kjTransferred, 5000f, SimMessages.EnergySourceID.StructureTemperature);
        SimMessages.ModifyEnergy(this.outputCell, kjTransferred, this.maxOutputTemperature, SimMessages.EnergySourceID.StructureTemperature);

        return PeltierCooler.MonitorState.ReadyToHeat;
    }

    public class StatesInstance : GameStateMachine<PeltierCooler.States, PeltierCooler.StatesInstance, PeltierCooler, object>.GameInstance
    {
        // Token: 0x06002339 RID: 9017 RVA: 0x000AE718 File Offset: 0x000AC918
        public StatesInstance(PeltierCooler master) : base(master)
        {
        }
    }

    public class States : GameStateMachine<PeltierCooler.States, PeltierCooler.StatesInstance, PeltierCooler>
    {
        // Token: 0x0600233B RID: 9019 RVA: 0x000AE72C File Offset: 0x000AC92C
        public override void InitializeStates(out StateMachine.BaseState default_state)
        {
            default_state = this.offline;
            base.serializable = false;
            
            this.statusItemUnderMass = new StatusItem("statusItemUnderMass", "No material in contact with operating surface(s)", "Both operating surfaces has to be in contact with gas, liquid or tile.", string.Empty, StatusItem.IconType.Info, NotificationType.BadMinor, false, SimViewMode.None, 63486);

            this.statusItemOverTemp = new StatusItem("statusItemOverTemp", BUILDING.STATUSITEMS.HEATINGSTALLEDHOTENV.NAME, BUILDING.STATUSITEMS.HEATINGSTALLEDHOTENV.TOOLTIP, string.Empty, StatusItem.IconType.Info, NotificationType.BadMinor, false, SimViewMode.None, 63486);
            this.statusItemOverTemp.resolveStringCallback = delegate (string str, object obj)
            {
                PeltierCooler.StatesInstance statesInstance = (PeltierCooler.StatesInstance)obj;
                return string.Format(str, GameUtil.GetFormattedTemperature(statesInstance.master.MaxOutputTemperature, GameUtil.TimeSlice.None, GameUtil.TemperatureInterpretation.Absolute, true));
            };

            this.statusItemUnderTemp = new StatusItem("statusItemUnderTemp", BUILDING.STATUSITEMS.CANNOTCOOLFURTHER.NAME, BUILDING.STATUSITEMS.CANNOTCOOLFURTHER.TOOLTIP, string.Empty, StatusItem.IconType.Info, NotificationType.BadMinor, false, SimViewMode.None, 63486);
            this.statusItemUnderTemp.resolveStringCallback = delegate (string str, object obj)
            {
                PeltierCooler.StatesInstance statesInstance = (PeltierCooler.StatesInstance)obj;
                return string.Format(str, GameUtil.GetFormattedTemperature(statesInstance.master.MinInputTemperature, GameUtil.TimeSlice.None, GameUtil.TemperatureInterpretation.Absolute, true));
            };

            this.offline.EventTransition(GameHashes.OperationalChanged, this.online, (PeltierCooler.StatesInstance smi) => smi.master.operational.IsOperational);
            this.online.EventTransition(GameHashes.OperationalChanged, this.offline, (PeltierCooler.StatesInstance smi) => !smi.master.operational.IsOperational).DefaultState(this.online.heating).Update("peltiercooler_online", delegate (PeltierCooler.StatesInstance smi, float dt)
            {
                switch (smi.master.MonitorHeating(dt))
                {
                    case PeltierCooler.MonitorState.ReadyToHeat:
                        smi.GoTo(this.online.heating);
                        break;
                    case PeltierCooler.MonitorState.TooHot:
                        smi.GoTo(this.online.overtemp);
                        break;
                    case PeltierCooler.MonitorState.TooCold:
                        smi.GoTo(this.online.undertemp);
                        break;
                    case PeltierCooler.MonitorState.NotEnoughMass:
                        smi.GoTo(this.online.undermass);
                        break;
                }
            }, UpdateRate.SIM_200ms, false);
            this.online.heating.Enter(delegate (PeltierCooler.StatesInstance smi)
            {
                smi.master.operational.SetActive(true, false);
            }).Exit(delegate (PeltierCooler.StatesInstance smi)
            {
                smi.master.operational.SetActive(false, false);
            });
            this.online.undermass.ToggleCategoryStatusItem(Db.Get().StatusItemCategories.Heat, this.statusItemUnderMass, null);
            this.online.overtemp.ToggleCategoryStatusItem(Db.Get().StatusItemCategories.Heat, this.statusItemOverTemp, null);
            this.online.undertemp.ToggleCategoryStatusItem(Db.Get().StatusItemCategories.Heat, this.statusItemUnderTemp, null);
        }

        // Token: 0x04001D8D RID: 7565
        public GameStateMachine<PeltierCooler.States, PeltierCooler.StatesInstance, PeltierCooler, object>.State offline;

        // Token: 0x04001D8E RID: 7566
        public PeltierCooler.States.OnlineStates online;

        // Token: 0x04001D90 RID: 7568
        private StatusItem statusItemUnderMass;

        // Token: 0x04001D91 RID: 7569
        private StatusItem statusItemOverTemp;
        private StatusItem statusItemUnderTemp;

        // Token: 0x0200071B RID: 1819
        public class OnlineStates : GameStateMachine<PeltierCooler.States, PeltierCooler.StatesInstance, PeltierCooler, object>.State
        {
            // Token: 0x04001D97 RID: 7575
            public GameStateMachine<PeltierCooler.States, PeltierCooler.StatesInstance, PeltierCooler, object>.State heating;

            // Token: 0x04001D98 RID: 7576
            public GameStateMachine<PeltierCooler.States, PeltierCooler.StatesInstance, PeltierCooler, object>.State overtemp;
            public GameStateMachine<PeltierCooler.States, PeltierCooler.StatesInstance, PeltierCooler, object>.State undertemp;

            // Token: 0x04001D9A RID: 7578
            public GameStateMachine<PeltierCooler.States, PeltierCooler.StatesInstance, PeltierCooler, object>.State undermass;
        }
    }

    // Token: 0x0200071C RID: 1820
    private enum MonitorState
    {
        // Token: 0x04001D9C RID: 7580
        ReadyToHeat,
        TooCold,
        // Token: 0x04001D9D RID: 7581
        TooHot,
        // Token: 0x04001D9F RID: 7583
        NotEnoughMass
    }
}
