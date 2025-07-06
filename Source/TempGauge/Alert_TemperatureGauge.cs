using RimWorld;
using UnityEngine;
using Verse;

namespace TempGauge;

public class Alert_TemperatureGauge : Alert
{
    private int lastActiveFrame = -1;

    private bool onHighTemp;

    private AlertState state = AlertState.Normal;

    private float targetTemp;

    public Alert_TemperatureGauge()
    {
        defaultLabel = "TempGaugeAlert".Translate();
        defaultExplanation = "TempGaugeAlertDesc".Translate();
        defaultPriority = AlertPriority.Critical;
    }

    protected override Color BGColor
    {
        get
        {
            Color result;
            if (state == AlertState.Critical)
            {
                var num = Pulser.PulseBrightness(0.5f, Pulser.PulseBrightness(0.5f, 0.6f));
                result = new Color(num, num, num) * Color.red;
            }
            else
            {
                result = Color.clear;
            }

            return result;
        }
    }

    public override string GetLabel()
    {
        string result = onHighTemp ? "TempGaugeAlertHot".Translate() : "TempGaugeAlertCold".Translate();

        return result;
    }

    public override TaggedString GetExplanation()
    {
        TaggedString result;
        result = onHighTemp
            ? "TempGaugeAlertHotDesc".Translate(targetTemp.ToStringTemperature("F0"))
            : "TempGaugeAlertColdDesc".Translate(targetTemp.ToStringTemperature("F0"));

        return result;
    }

    public override AlertReport GetReport()
    {
        if (!ResearchProjectDef.Named("Electricity").IsFinished)
        {
            return false;
        }

        var maps = Find.Maps;
        foreach (var map in maps)
        {
            if (!map.IsPlayerHome)
            {
                continue;
            }

            foreach (var building in map.listerBuildings.allBuildingsColonist)
            {
                if (building is Building_TemperatureGauge { ShouldSendAlert: true } buildingTemperatureGauge)
                {
                    state = buildingTemperatureGauge.AlertState;
                    targetTemp = buildingTemperatureGauge.CompTempControl.targetTemperature;
                    onHighTemp = buildingTemperatureGauge.OnHighTemp;
                    return buildingTemperatureGauge;
                }

                if (building is not Building_TemperatureGaugeWall
                    {
                        ShouldSendAlert: true
                    } buildingTemperatureGaugeWall)
                {
                    continue;
                }

                state = buildingTemperatureGaugeWall.AlertState;
                targetTemp = buildingTemperatureGaugeWall.CompTempControl.targetTemperature;
                onHighTemp = buildingTemperatureGaugeWall.OnHighTemp;
                return buildingTemperatureGaugeWall;
            }
        }

        return false;
    }

    public override void AlertActiveUpdate()
    {
        if (state != AlertState.Critical)
        {
            return;
        }

        if (lastActiveFrame < Time.frameCount - 1)
        {
            Messages.Message("MessageCriticalAlert".Translate(GetLabel()),
                new LookTargets(GetReport().AllCulprits), MessageTypeDefOf.ThreatBig, false);
        }

        lastActiveFrame = Time.frameCount;
    }
}