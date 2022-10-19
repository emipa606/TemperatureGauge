using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TempGauge;

[StaticConstructorOnStartup]
public abstract class Building_Thermometer : Building
{
    public CompTempControl CompTempControl;

    public bool onHighTemp = true;

    public string tempGizmoLabel => onHighTemp ? "OnHighTemp".Translate() : "OnLowTemp".Translate();

    public string targetTempString => CompTempControl.targetTemperature.ToStringTemperature("F0");

    public bool tempOutOfRange => onHighTemp
        ? this.GetRoom(RegionType.Set_Passable).Temperature > CompTempControl.targetTemperature
        : this.GetRoom(RegionType.Set_Passable).Temperature < CompTempControl.targetTemperature;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref onHighTemp, "onHighTemp", true);
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        CompTempControl = GetComp<CompTempControl>();
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        yield return new Command_Action
        {
            icon = ContentFinder<Texture2D>.Get($"UI/Commands/{(onHighTemp ? "TempHigh" : "TempLow")}"),
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