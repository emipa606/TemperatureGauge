using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TempGauge;

[StaticConstructorOnStartup]
public class Building_TemperatureGauge : Building_Thermometer
{
    private static readonly Material GaugeFillHotMat =
        SolidColorMaterials.SimpleSolidColorMaterial(new Color(1f, 0.5f, 0.2f));

    private static readonly Material GaugeFillColdMat =
        SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.2f, 0.4f, 1f));

    private static readonly Material GaugeUnfilledMat =
        SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.1f, 0.1f, 0.1f));

    public AlertState alertState = AlertState.Normal;

    public bool shouldSendAlert => tempOutOfRange && alertState > AlertState.Off;

    private string alertGizmoLabel
    {
        get
        {
            string result;
            switch (alertState)
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

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref alertState, "alertState", AlertState.Normal);
    }

    protected override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        base.DrawAt(drawLoc, flip);
        var temperature = this.GetRoom(RegionType.Set_Passable).Temperature;
        var r = default(GenDraw.FillableBarRequest);
        r.center = drawLoc + (Vector3.up * 0.05f);
        r.size = new Vector2(0.55f, 0.2f);
        r.margin = 0.05f;
        r.fillPercent = Mathf.Clamp(Mathf.Abs(temperature), 1f, 50f) / 50f;
        r.unfilledMat = GaugeUnfilledMat;
        r.filledMat = temperature > 0f ? GaugeFillHotMat : GaugeFillColdMat;

        var rotation = Rotation;
        rotation.Rotate(RotationDirection.Clockwise);
        r.rotation = rotation;
        GenDraw.DrawFillableBar(r);
    }

    public override void TickRare()
    {
        base.TickRare();
        if (shouldSendAlert)
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
            var currentTemp = this.GetRoom(RegionType.Set_Passable).Temperature;
            var niceTemp = (float)Math.Round(currentTemp * 10f) / 10f;
            tempInfoAdd += niceTemp.ToStringTemperature("F0");
        }

        if (ResearchProjectDef.Named("Electricity").IsFinished)
        {
            if (alertState == AlertState.Off)
            {
                stringBuilder.Append("AlertOffDesc".Translate());
            }
            else
            {
                stringBuilder.Append(onHighTemp
                    ? "AlertOnHighTemperatureDesc".Translate(targetTempString)
                    : "AlertOnLowTemperatureDesc".Translate(targetTempString));
            }
        }
        else
        {
            alertState = AlertState.Off;
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
                icon = ContentFinder<Texture2D>.Get($"UI/Commands/Alert_{alertState}"),
                defaultLabel = alertGizmoLabel,
                defaultDesc = "AlertGizmoDesc".Translate(),
                action = delegate
                {
                    SoundDefOf.Tick_High.PlayOneShotOnCamera();
                    if (alertState >= AlertState.Critical)
                    {
                        alertState = AlertState.Off;
                    }
                    else
                    {
                        alertState++;
                    }
                }
            };
        }
        else
        {
            yield return new Command_Action
            {
                icon = ContentFinder<Texture2D>.Get($"UI/Commands/Alert_{alertState}"),
                defaultLabel = alertGizmoLabel,
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
                GaugeSettings_Clipboard.Copy(onHighTemp, CompTempControl.targetTemperature, alertState);
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
                GaugeSettings_Clipboard.PasteInto(out onHighTemp, out CompTempControl.targetTemperature,
                    out alertState);
            },
            hotKey = KeyBindingDefOf.Misc5
        };
    }
}