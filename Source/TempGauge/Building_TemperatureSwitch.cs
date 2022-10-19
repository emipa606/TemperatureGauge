using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TempGauge;

[StaticConstructorOnStartup]
internal class Building_TemperatureSwitch : Building_Thermometer
{
    private static readonly Material GaugeFillHotMat =
        SolidColorMaterials.SimpleSolidColorMaterial(new Color(1f, 0.5f, 0.2f));

    private static readonly Material GaugeFillColdMat =
        SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.2f, 0.4f, 1f));

    private static readonly Material GaugeUnfilledMat =
        SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.1f, 0.1f, 0.1f));

    private CompFlickable compFlickable;

    private Graphic offGraphic;

    private bool switchOnOld;

    public override Graphic Graphic
    {
        get
        {
            Graphic defaultGraphic;
            try
            {
                var transmitsPowerNow = TransmitsPowerNow;
                if (transmitsPowerNow)
                {
                    defaultGraphic = DefaultGraphic;
                }
                else
                {
                    if (offGraphic == null)
                    {
                        var graphicData = def.graphicData;
                        offGraphic = GraphicDatabase.Get(graphicData.graphicClass, $"{graphicData.texPath}_Off",
                            graphicData.shaderType.Shader, graphicData.drawSize, graphicData.color,
                            graphicData.colorTwo);
                    }

                    defaultGraphic = offGraphic;
                }
            }
            catch (Exception)
            {
                defaultGraphic = DefaultGraphic;
            }

            return defaultGraphic;
        }
    }

    public override bool TransmitsPowerNow => tempOutOfRange && compFlickable.SwitchIsOn;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref switchOnOld, "switchOnOld", true);
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        compFlickable = GetComp<CompFlickable>();
    }

    public override void Draw()
    {
        base.Draw();
        var temperature = this.GetRoom(RegionType.Set_Passable).Temperature;
        var r = default(GenDraw.FillableBarRequest);
        r.center = DrawPos + (Vector3.up * 0.05f);
        r.size = new Vector2(0.7f, 0.1f);
        r.margin = 0.05f;
        r.fillPercent = compFlickable == null || compFlickable.SwitchIsOn
            ? Mathf.Clamp(Mathf.Abs(temperature), 1f, 50f) / 50f
            : 0f;
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
        if (!Spawned || switchOnOld == TransmitsPowerNow)
        {
            return;
        }

        switchOnOld = TransmitsPowerNow;
        Map.powerNetManager.Notfiy_TransmitterTransmitsPowerNowChanged(PowerComp);
    }

    public override string GetInspectString()
    {
        var stringBuilder = new StringBuilder();
        var tempInfoAdd = string.Empty;
        if (GetType() == typeof(MinifiedThing))
        {
            stringBuilder.Append("NotInstalled".Translate());
            stringBuilder.AppendLine();
        }
        else
        {
            tempInfoAdd = "CurrentTempIs".Translate();
            var currentTemp = this.GetRoom(RegionType.Set_Passable).Temperature;
            var niceTemp = (float)Math.Round(currentTemp * 10f) / 10f;
            tempInfoAdd += niceTemp.ToStringTemperature("F0");
        }

        if (PowerComp.PowerNet == null)
        {
            stringBuilder.Append("PowerNotConnected".Translate());
        }
        else
        {
            var value =
                (PowerComp.PowerNet.CurrentEnergyGainRate() / CompPower.WattsToWattDaysPerTick).ToString("F0");
            var value2 = PowerComp.PowerNet.CurrentStoredEnergy().ToString("F0");
            stringBuilder.Append("PowerConnectedRateStored".Translate(value, value2));
        }

        stringBuilder.AppendLine();
        stringBuilder.Append(onHighTemp
            ? "SwitchOnHighTemperatureDesc".Translate(targetTempString)
            : "SwitchOnLowTemperatureDesc".Translate(targetTempString));

        if (compFlickable is { SwitchIsOn: false })
        {
            stringBuilder.AppendLine();
            stringBuilder.Append("SwitchOffDesc".Translate());
        }

        if (string.IsNullOrEmpty(tempInfoAdd))
        {
            return stringBuilder.ToString();
        }

        stringBuilder.AppendLine();
        stringBuilder.Append(tempInfoAdd);

        return stringBuilder.ToString();
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
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
                GaugeSettings_Clipboard.Copy(onHighTemp, CompTempControl.targetTemperature);
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
                GaugeSettings_Clipboard.PasteInto(out onHighTemp, out CompTempControl.targetTemperature);
            },
            hotKey = KeyBindingDefOf.Misc5
        };
    }
}