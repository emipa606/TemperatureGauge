using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace TempGauge
{
	// Token: 0x02000003 RID: 3
	public class Alert_TemperatureGauge : Alert
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		public Alert_TemperatureGauge()
		{
			this.defaultLabel = Translator.Translate("TempGaugeAlert");
			this.defaultExplanation = Translator.Translate("TempGaugeAlertDesc");
			this.defaultPriority = AlertPriority.Critical;
		}

		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000002 RID: 2 RVA: 0x000020AC File Offset: 0x000002AC
		protected override Color BGColor
		{
			get
			{
				bool flag = this.state == AlertState.Critical;
				Color result;
				if (flag)
				{
					float num = Pulser.PulseBrightness(0.5f, Pulser.PulseBrightness(0.5f, 0.6f));
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
			bool flag = this.onHighTemp;
			string result;
			if (flag)
			{
				result = Translator.Translate("TempGaugeAlertHot");
			}
			else
			{
				result = Translator.Translate("TempGaugeAlertCold");
			}
			return result;
		}

		// Token: 0x06000004 RID: 4 RVA: 0x00002134 File Offset: 0x00000334
		public override TaggedString GetExplanation()
		{
			bool flag = this.onHighTemp;
			TaggedString result;
			if (flag)
			{
				result = TranslatorFormattedStringExtensions.Translate("TempGaugeAlertHotDesc", this.targetTemp.ToStringTemperature("F0"));
			}
			else
			{
				result = TranslatorFormattedStringExtensions.Translate("TempGaugeAlertColdDesc", this.targetTemp.ToStringTemperature("F0"));
			}
			return result;
		}

		// Token: 0x06000005 RID: 5 RVA: 0x00002194 File Offset: 0x00000394
		public override AlertReport GetReport()
		{
			List<Map> maps = Find.Maps;
			foreach (Map map in maps)
			{
				bool flag = !map.IsPlayerHome;
				if (!flag)
				{
					foreach (Building building in map.listerBuildings.allBuildingsColonist)
					{
						Building_TemperatureGauge building_TemperatureGauge = building as Building_TemperatureGauge;
						bool flag2 = building_TemperatureGauge != null && building_TemperatureGauge.shouldSendAlert;
						if (flag2)
						{
							this.state = building_TemperatureGauge.alertState;
							this.targetTemp = building_TemperatureGauge.CompTempControl.targetTemperature;
							this.onHighTemp = building_TemperatureGauge.onHighTemp;
                            return building_TemperatureGauge;
                        } 
                        Building_TemperatureGaugeWall building_TemperatureGaugeWall = building as Building_TemperatureGaugeWall;
						bool flag3 = building_TemperatureGaugeWall != null && building_TemperatureGaugeWall.shouldSendAlert;
                        if (flag3)
                        {
                            this.state = building_TemperatureGaugeWall.alertState;
                            this.targetTemp = building_TemperatureGaugeWall.CompTempControl.targetTemperature;
                            this.onHighTemp = building_TemperatureGaugeWall.onHighTemp;
                            return building_TemperatureGaugeWall;
                        }
                    }
				}
			}
			return false;
		}

		// Token: 0x06000006 RID: 6 RVA: 0x000022A8 File Offset: 0x000004A8
		public override void AlertActiveUpdate()
		{
			bool flag = this.state != AlertState.Critical;
			if (!flag)
			{
				bool flag2 = this.lastActiveFrame < Time.frameCount - 1;
				if (flag2)
				{
					Messages.Message(TranslatorFormattedStringExtensions.Translate("MessageCriticalAlert", this.GetLabel()), new LookTargets(this.GetReport().AllCulprits), MessageTypeDefOf.ThreatBig, false);
				}
				this.lastActiveFrame = Time.frameCount;
			}
		}

		// Token: 0x04000005 RID: 5
		private AlertState state = AlertState.Normal;

		// Token: 0x04000006 RID: 6
		private float targetTemp = 0f;

		// Token: 0x04000007 RID: 7
		private bool onHighTemp = false;

		// Token: 0x04000008 RID: 8
		private int lastActiveFrame = -1;
	}
}
