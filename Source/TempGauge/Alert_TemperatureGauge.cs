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
                if (building is Building_TemperatureGauge { shouldSendAlert: true } building_TemperatureGauge)
                {
                    state = building_TemperatureGauge.alertState;
                    targetTemp = building_TemperatureGauge.CompTempControl.targetTemperature;
                    onHighTemp = building_TemperatureGauge.onHighTemp;
                    return building_TemperatureGauge;
                }

                if (building is not Building_TemperatureGaugeWall building_TemperatureGaugeWall ||
                    !building_TemperatureGaugeWall.shouldSendAlert)
                {
                    continue;
                }

                state = building_TemperatureGaugeWall.alertState;
                targetTemp = building_TemperatureGaugeWall.CompTempControl.targetTemperature;
                onHighTemp = building_TemperatureGaugeWall.onHighTemp;
                return building_TemperatureGaugeWall;
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