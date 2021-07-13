using RimWorld;
using UnityEngine;
using Verse;

namespace TempGauge
{
    // Token: 0x02000003 RID: 3
    public class Alert_TemperatureGauge : Alert
    {
        // Token: 0x04000008 RID: 8
        private int lastActiveFrame = -1;

        // Token: 0x04000007 RID: 7
        private bool onHighTemp;

        // Token: 0x04000005 RID: 5
        private AlertState state = AlertState.Normal;

        // Token: 0x04000006 RID: 6
        private float targetTemp;

        // Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
        public Alert_TemperatureGauge()
        {
            defaultLabel = "TempGaugeAlert".Translate();
            defaultExplanation = "TempGaugeAlertDesc".Translate();
            defaultPriority = AlertPriority.Critical;
        }

        // Token: 0x17000001 RID: 1
        // (get) Token: 0x06000002 RID: 2 RVA: 0x000020AC File Offset: 0x000002AC
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

        // Token: 0x06000003 RID: 3 RVA: 0x00002100 File Offset: 0x00000300
        public override string GetLabel()
        {
            string result = onHighTemp ? "TempGaugeAlertHot".Translate() : "TempGaugeAlertCold".Translate();

            return result;
        }

        // Token: 0x06000004 RID: 4 RVA: 0x00002134 File Offset: 0x00000334
        public override TaggedString GetExplanation()
        {
            TaggedString result;
            result = onHighTemp
                ? "TempGaugeAlertHotDesc".Translate(targetTemp.ToStringTemperature("F0"))
                : "TempGaugeAlertColdDesc".Translate(targetTemp.ToStringTemperature("F0"));

            return result;
        }

        // Token: 0x06000005 RID: 5 RVA: 0x00002194 File Offset: 0x00000394
        public override AlertReport GetReport()
        {
            var maps = Find.Maps;
            foreach (var map in maps)
            {
                if (!map.IsPlayerHome)
                {
                    continue;
                }

                foreach (var building in map.listerBuildings.allBuildingsColonist)
                {
                    if (building is Building_TemperatureGauge {shouldSendAlert: true} building_TemperatureGauge)
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

        // Token: 0x06000006 RID: 6 RVA: 0x000022A8 File Offset: 0x000004A8
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
}