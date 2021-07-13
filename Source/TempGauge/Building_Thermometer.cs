using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TempGauge
{
    // Token: 0x02000006 RID: 6
    [StaticConstructorOnStartup]
    public abstract class Building_Thermometer : Building
    {
        // Token: 0x04000014 RID: 20
        public CompTempControl CompTempControl;

        // Token: 0x04000013 RID: 19
        public bool onHighTemp = true;

        // Token: 0x17000006 RID: 6
        // (get) Token: 0x06000020 RID: 32 RVA: 0x00002B5C File Offset: 0x00000D5C
        public string tempGizmoLabel => onHighTemp ? "OnHighTemp".Translate() : "OnLowTemp".Translate();

        // Token: 0x17000007 RID: 7
        // (get) Token: 0x06000021 RID: 33 RVA: 0x00002B8C File Offset: 0x00000D8C
        public string targetTempString => CompTempControl.targetTemperature.ToStringTemperature("F0");

        // Token: 0x17000008 RID: 8
        // (get) Token: 0x06000022 RID: 34 RVA: 0x00002BB4 File Offset: 0x00000DB4
        public bool tempOutOfRange => onHighTemp
            ? this.GetRoom(RegionType.Set_Passable).Temperature > CompTempControl.targetTemperature
            : this.GetRoom(RegionType.Set_Passable).Temperature < CompTempControl.targetTemperature;

        // Token: 0x06000023 RID: 35 RVA: 0x00002C02 File Offset: 0x00000E02
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref onHighTemp, "onHighTemp", true);
        }

        // Token: 0x06000024 RID: 36 RVA: 0x00002C1F File Offset: 0x00000E1F
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            CompTempControl = GetComp<CompTempControl>();
        }

        // Token: 0x06000025 RID: 37 RVA: 0x00002C37 File Offset: 0x00000E37
        public override IEnumerable<Gizmo> GetGizmos()
        {
            yield return new Command_Action
            {
                icon = ContentFinder<Texture2D>.Get("UI/Commands/" + (onHighTemp ? "TempHigh" : "TempLow")),
                defaultLabel = (onHighTemp ? "OnHighTemp" : "OnLowTemp").Translate(),
                defaultDesc = "TempGizmoDesc".Translate(),
                action = delegate
                {
                    SoundDefOf.Tick_High.PlayOneShotOnCamera();
                    onHighTemp = !onHighTemp;
                }
            };
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }
        }
    }
}