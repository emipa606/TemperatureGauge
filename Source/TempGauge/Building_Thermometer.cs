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

    public bool OnHighTemp = true;

    public string TempGizmoLabel => OnHighTemp ? "OnHighTemp".Translate() : "OnLowTemp".Translate();

    protected string TargetTempString => CompTempControl.targetTemperature.ToStringTemperature("F0");

    protected bool TempOutOfRange => OnHighTemp
        ? this.GetRoom(RegionType.Set_Passable).Temperature > CompTempControl.targetTemperature
        : this.GetRoom(RegionType.Set_Passable).Temperature < CompTempControl.targetTemperature;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref OnHighTemp, "onHighTemp", true);
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        CompTempControl = GetComp<CompTempControl>();
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        var canUse = ResearchProjectDef.Named("Electricity").IsFinished;
        if (canUse)
        {
            yield return new Command_Action
            {
                icon = ContentFinder<Texture2D>.Get($"UI/Commands/{(OnHighTemp ? "TempHigh" : "TempLow")}"),
                defaultLabel = (OnHighTemp ? "OnHighTemp" : "OnLowTemp").Translate(),
                defaultDesc = "TempGizmoDesc".Translate(),
                action = delegate
                {
                    SoundDefOf.Tick_High.PlayOneShotOnCamera();
                    OnHighTemp = !OnHighTemp;
                }
            };
        }

        foreach (var g in base.GetGizmos())
        {
            if (!canUse && g is Command_Action action && action.icon.name.StartsWith("Temp"))
            {
                continue;
            }

            yield return g;
        }
    }
}