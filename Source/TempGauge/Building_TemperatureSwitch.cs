using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TempGauge
{
	// Token: 0x02000005 RID: 5
	[StaticConstructorOnStartup]
	internal class Building_TemperatureSwitch : Building_Thermometer
	{
		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000014 RID: 20 RVA: 0x000026D4 File Offset: 0x000008D4
		public override Graphic Graphic
		{
			get
			{
				Graphic defaultGraphic;
				try
				{
					bool transmitsPowerNow = this.TransmitsPowerNow;
					if (transmitsPowerNow)
					{
						defaultGraphic = base.DefaultGraphic;
					}
					else
					{
						bool flag = this.offGraphic == null;
						if (flag)
						{
							GraphicData graphicData = this.def.graphicData;
							this.offGraphic = GraphicDatabase.Get(graphicData.graphicClass, graphicData.texPath + "_Off", graphicData.shaderType.Shader, graphicData.drawSize, graphicData.color, graphicData.colorTwo);
						}
						defaultGraphic = this.offGraphic;
					}
				}
				catch (Exception)
				{
					defaultGraphic = base.DefaultGraphic;
				}
				return defaultGraphic;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<bool>(ref this.switchOnOld, "switchOnOld", true, false);
		}

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x06000015 RID: 21 RVA: 0x00002778 File Offset: 0x00000978
		public override bool TransmitsPowerNow
		{
			get
			{
				return base.tempOutOfRange && this.compFlickable.SwitchIsOn;
			}
		}

		// Token: 0x06000016 RID: 22 RVA: 0x000027A0 File Offset: 0x000009A0
		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			this.compFlickable = base.GetComp<CompFlickable>();
		}

		// Token: 0x06000017 RID: 23 RVA: 0x000027B8 File Offset: 0x000009B8
		public override void Draw()
		{
			base.Draw();
			float temperature = this.GetRoom(RegionType.Set_Passable).Temperature;
			GenDraw.FillableBarRequest r = default(GenDraw.FillableBarRequest);
			r.center = this.DrawPos + Vector3.up * 0.05f;
			r.size = new Vector2(0.7f, 0.1f);
			r.margin = 0.05f;
			r.fillPercent = ((this.compFlickable == null || this.compFlickable.SwitchIsOn) ? (Mathf.Clamp(Mathf.Abs(temperature), 1f, 50f) / 50f) : 0f);
			r.unfilledMat = Building_TemperatureSwitch.GaugeUnfilledMat;
			bool flag = temperature > 0f;
			if (flag)
			{
				r.filledMat = Building_TemperatureSwitch.GaugeFillHotMat;
			}
			else
			{
				r.filledMat = Building_TemperatureSwitch.GaugeFillColdMat;
			}
			Rot4 rotation = base.Rotation;
			rotation.Rotate(RotationDirection.Clockwise);
			r.rotation = rotation;
			GenDraw.DrawFillableBar(r);
		}

		// Token: 0x06000018 RID: 24 RVA: 0x000028C0 File Offset: 0x00000AC0
		public override void TickRare()
		{
			base.TickRare();
			bool flag = base.Spawned && this.switchOnOld != this.TransmitsPowerNow;
			if (flag)
			{
				this.switchOnOld = this.TransmitsPowerNow;
				base.Map.powerNetManager.Notfiy_TransmitterTransmitsPowerNowChanged(base.PowerComp);
			}
		}

		// Token: 0x06000019 RID: 25 RVA: 0x0000291C File Offset: 0x00000B1C
		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = base.GetType() == typeof(MinifiedThing);
			var tempInfoAdd = string.Empty;
			if (flag)
			{
				stringBuilder.Append(Translator.Translate("NotInstalled"));
				stringBuilder.AppendLine();
			} else
            {
				tempInfoAdd = Translator.Translate("CurrentTempIs");
				var currentTemp = this.GetRoom(RegionType.Set_Passable).Temperature;
				var niceTemp = (float)Math.Round(currentTemp * 10f) / 10f;
				tempInfoAdd += niceTemp.ToStringTemperature("F0");
			}
			bool flag2 = base.PowerComp.PowerNet == null;
			if (flag2)
			{
				stringBuilder.Append(Translator.Translate("PowerNotConnected"));
			}
			else
			{
				string value = (base.PowerComp.PowerNet.CurrentEnergyGainRate() / CompPower.WattsToWattDaysPerTick).ToString("F0");
				string value2 = base.PowerComp.PowerNet.CurrentStoredEnergy().ToString("F0");
				stringBuilder.Append(TranslatorFormattedStringExtensions.Translate("PowerConnectedRateStored", value, value2));
			}
			stringBuilder.AppendLine();
			bool onHighTemp = this.onHighTemp;
			if (onHighTemp)
			{
				stringBuilder.Append(TranslatorFormattedStringExtensions.Translate("SwitchOnHighTemperatureDesc", base.targetTempString));
			}
			else
			{
				stringBuilder.Append(TranslatorFormattedStringExtensions.Translate("SwitchOnLowTemperatureDesc", base.targetTempString));
			}
			bool flag3 = this.compFlickable != null && !this.compFlickable.SwitchIsOn;
			if (flag3)
			{
				stringBuilder.AppendLine();
				stringBuilder.Append(Translator.Translate("SwitchOffDesc"));
			}
			if (!string.IsNullOrEmpty(tempInfoAdd))
			{
				stringBuilder.AppendLine();
				stringBuilder.Append(tempInfoAdd);
			}
			return stringBuilder.ToString();
		}

		// Token: 0x0600001A RID: 26 RVA: 0x00002A82 File Offset: 0x00000C82
		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo g in base.GetGizmos())
			{
				yield return g;
			}
			yield return new Command_Action
			{
				icon = ContentFinder<Texture2D>.Get("UI/Commands/CopySettings", true),
				defaultLabel = Translator.Translate("CommandCopyZoneSettingsLabel"),
				defaultDesc = Translator.Translate("CommandCopyZoneSettingsDesc"),
				action = delegate()
				{
					SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
					GaugeSettings_Clipboard.Copy(this.onHighTemp, this.CompTempControl.targetTemperature);
				},
				hotKey = KeyBindingDefOf.Misc4
			};
			yield return new Command_Action
			{
				icon = ContentFinder<Texture2D>.Get("UI/Commands/PasteSettings", true),
				defaultLabel = Translator.Translate("CommandPasteZoneSettingsLabel"),
				defaultDesc = Translator.Translate("CommandPasteZoneSettingsDesc"),
				action = delegate()
				{
					SoundDefOf.Tick_High.PlayOneShotOnCamera(null);
					GaugeSettings_Clipboard.PasteInto(out this.onHighTemp, out this.CompTempControl.targetTemperature);
				},
				hotKey = KeyBindingDefOf.Misc5
			};
			yield break;
		}

		// Token: 0x0400000D RID: 13
		private static readonly Material GaugeFillHotMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(1f, 0.5f, 0.2f), false);

		// Token: 0x0400000E RID: 14
		private static readonly Material GaugeFillColdMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.2f, 0.4f, 1f), false);

		// Token: 0x0400000F RID: 15
		private static readonly Material GaugeUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.1f, 0.1f, 0.1f), false);

		// Token: 0x04000010 RID: 16
		private Graphic offGraphic;

		// Token: 0x04000011 RID: 17
		private CompFlickable compFlickable;

		// Token: 0x04000012 RID: 18
		private bool switchOnOld = false;
	}
}
