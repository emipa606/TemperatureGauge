using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TempGauge;

[StaticConstructorOnStartup]
public class Building_TemperatureGaugeWall : Building_Thermometer
{
    private static readonly Material gaugeFillHotMat =
        SolidColorMaterials.SimpleSolidColorMaterial(new Color(1f, 0.5f, 0.2f));

    private static readonly Material gaugeFillColdMat =
        SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.2f, 0.4f, 1f));

    private static readonly Material gaugeUnfilledMat =
        SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.1f, 0.1f, 0.1f));

    public AlertState AlertState = AlertState.Normal;

    public override string Label
    {
        get
        {
            var returnValue = base.Label;
            var currentRotation = Rotation.AsInt;
            switch (currentRotation)
            {
                case 0:
                    returnValue += " facing North";
                    break;
                case 1:
                    returnValue += " facing East";
                    break;
                case 2:
                    returnValue += " facing South";
                    break;
                case 3:
                    returnValue += " facing West";
                    break;
            }

            return returnValue;
        }
    }

    public bool ShouldSendAlert
    {
        get
        {
            var alertStatus = false;
            try
            {
                if (AlertState > AlertState.Off)
                {
                    var temperature = getRoomCell().GetRoom(Map).Temperature;
                    var targetTemperature = CompTempControl.targetTemperature;
                    //if (Prefs.DevMode) Log.Message($"Temperature Guage: temp {temperature}, target {targetTemperature}");
                    alertStatus = OnHighTemp ? temperature > targetTemperature : temperature < targetTemperature;
                    //if (Prefs.DevMode) Log.Message($"Temperature Guage: alertstatus {alertStatus}");
                }
            }
            catch
            {
                if (Prefs.DevMode)
                {
                    Log.Message("Temperature Guage: failed to raise alert");
                }
            }

            return alertStatus;
        }
    }

    private string AlertGizmoLabel
    {
        get
        {
            string result;
            switch (AlertState)
            {
                case AlertState.Off:
                    result = "AlertOffLabel".Translate();
                    break;
                case AlertState.Normal:
                    result = "AlertNormalLabel".Translate();
                    break;
                case AlertState.Critical:
                    result = "AlertCriticalLabel".Translate();
                    break;
                default:
                    result = "AlertOffLabel".Translate();
                    break;
            }

            return result;
        }
    }

    private IntVec3 getRoomCell()
    {
        var rotation = Rotation.AsInt;
        var returnCell = Position;
        switch (rotation)
        {
            case 0: //North
                returnCell = Position + new IntVec3(0, 0, 1);
                break;
            case 1: //East
                returnCell = Position + new IntVec3(1, 0, 0);
                break;
            case 2: //South
                returnCell = Position + new IntVec3(0, 0, -1);
                break;
            case 3: //West
                returnCell = Position + new IntVec3(-1, 0, 0);
                break;
        }

        return returnCell;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref AlertState, "alertState", AlertState.Normal);
    }

    protected override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        base.DrawAt(drawLoc, flip);
        var currentRotation = Rotation.AsInt;

        var room = (Position + Rotation.FacingCell).GetRoom(Map);
        //Log.Message($Temperature Guage: temp {room.Temperature.ToString()});

        var temperature = room.Temperature;
        var r = default(GenDraw.FillableBarRequest);
        r.center = drawLoc;

        const float offsetFromCenter = 0.4f;
        switch (currentRotation)
        {
            // North
            case 0:
                r.center.z += offsetFromCenter;
                break;
            // South
            case 2:
                r.center.z -= offsetFromCenter;
                break;
            // East
            case 1:
                r.center.x += offsetFromCenter;
                break;
            // West
            default:
                r.center.x -= offsetFromCenter;
                break;
        }

        r.rotation = Rotation;
        r.size = new Vector2(0.45f, 0.1f);
        r.margin = 0.01f;
        r.fillPercent = Mathf.Clamp(Mathf.Abs(temperature), 1f, 50f) / 50f;
        r.unfilledMat = gaugeUnfilledMat;
        r.filledMat = temperature > 0f ? gaugeFillHotMat : gaugeFillColdMat;

        GenDraw.DrawFillableBar(r);
    }

    public override void TickRare()
    {
        base.TickRare();
        if (ShouldSendAlert)
        {
            FleckMaker.ThrowMetaIcon(this.TrueCenter().ToIntVec3(), Map, FleckDefOf.IncapIcon);
        }
    }

    public override string GetInspectString()
    {
        var stringBuilder = new StringBuilder();
        var tempInfoAdd = string.Empty;
        if (GetType() == typeof(MinifiedThing))
        {
            stringBuilder.Append("NotInstalled".Translate());
            stringBuilder.AppendLineIfNotEmpty();
        }
        else
        {
            tempInfoAdd = "CurrentTempIs".Translate();
            var temperature = getRoomCell().GetRoom(Map).Temperature;
            var niceTemp = (float)Math.Round(temperature * 10f) / 10f;
            tempInfoAdd += niceTemp.ToStringTemperature("F0");
        }

        if (ResearchProjectDef.Named("Electricity").IsFinished)
        {
            if (AlertState == AlertState.Off)
            {
                stringBuilder.Append("AlertOffDesc".Translate());
            }
            else
            {
                stringBuilder.Append(OnHighTemp
                    ? "AlertOnHighTemperatureDesc".Translate(TargetTempString)
                    : "AlertOnLowTemperatureDesc".Translate(TargetTempString));
            }
        }
        else
        {
            AlertState = AlertState.Off;
        }

        if (string.IsNullOrEmpty(tempInfoAdd))
        {
            return stringBuilder.ToString();
        }

        stringBuilder.AppendLineIfNotEmpty();
        stringBuilder.Append(tempInfoAdd);

        return stringBuilder.ToString().TrimEndNewlines();
    }


    public override IEnumerable<Gizmo> GetGizmos()
    {
        if (ResearchProjectDef.Named("Electricity").IsFinished)
        {
            yield return new Command_Action
            {
                icon = ContentFinder<Texture2D>.Get($"UI/Commands/Alert_{AlertState}"),
                defaultLabel = AlertGizmoLabel,
                defaultDesc = "AlertGizmoDesc".Translate(),
                action = delegate
                {
                    SoundDefOf.Tick_High.PlayOneShotOnCamera();
                    if (AlertState >= AlertState.Critical)
                    {
                        AlertState = AlertState.Off;
                    }
                    else
                    {
                        AlertState++;
                    }
                }
            };
        }
        else
        {
            yield return new Command_Action
            {
                icon = ContentFinder<Texture2D>.Get($"UI/Commands/Alert_{AlertState}"),
                defaultLabel = AlertGizmoLabel,
                defaultDesc = "AlertGizmoDesc".Translate(),
                Disabled = true,
                disabledReason = "TempGaugeMissingResearch".Translate()
            };
        }

        foreach (var g in base.GetGizmos())
        {
            yield return g;
        }

        yield return new Command_Action
        {
            icon = ContentFinder<Texture2D>.Get("UI/Commands/CopySettings"),
            defaultLabel = "CommandCopyZoneSettingsLabel".Translate(),
            defaultDesc = "CommandCopyZoneSettingsDesc".Translate(),
            action = delegate
            {
                SoundDefOf.Tick_High.PlayOneShotOnCamera();
                GaugeSettings_Clipboard.Copy(OnHighTemp, CompTempControl.targetTemperature, AlertState);
            },
            hotKey = KeyBindingDefOf.Misc4
        };
        yield return new Command_Action
        {
            icon = ContentFinder<Texture2D>.Get("UI/Commands/PasteSettings"),
            defaultLabel = "CommandPasteZoneSettingsLabel".Translate(),
            defaultDesc = "CommandPasteZoneSettingsDesc".Translate(),
            action = delegate
            {
                SoundDefOf.Tick_High.PlayOneShotOnCamera();
                GaugeSettings_Clipboard.PasteInto(out OnHighTemp, out CompTempControl.targetTemperature,
                    out AlertState);
            },
            hotKey = KeyBindingDefOf.Misc5
        };
    }
}