using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TempGauge
{
    // Token: 0x02000004 RID: 4
    [StaticConstructorOnStartup]
    public class Building_TemperatureGauge : Building_Thermometer
    {
        // Token: 0x04000009 RID: 9
        private static readonly Material GaugeFillHotMat =
            SolidColorMaterials.SimpleSolidColorMaterial(new Color(1f, 0.5f, 0.2f));

        // Token: 0x0400000A RID: 10
        private static readonly Material GaugeFillColdMat =
            SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.2f, 0.4f, 1f));

        // Token: 0x0400000B RID: 11
        private static readonly Material GaugeUnfilledMat =
            SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.1f, 0.1f, 0.1f));

        // Token: 0x0400000C RID: 12
        public AlertState alertState = AlertState.Normal;

        // Token: 0x17000002 RID: 2
        // (get) Token: 0x06000007 RID: 7 RVA: 0x0000231C File Offset: 0x0000051C
        public bool shouldSendAlert => tempOutOfRange && alertState > AlertState.Off;

        // Token: 0x17000003 RID: 3
        // (get) Token: 0x06000008 RID: 8 RVA: 0x00002344 File Offset: 0x00000544
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

        // Token: 0x06000009 RID: 9 RVA: 0x000023A2 File Offset: 0x000005A2
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref alertState, "alertState", AlertState.Normal);
        }

        // Token: 0x0600000A RID: 10 RVA: 0x000023C0 File Offset: 0x000005C0
        public override void Draw()
        {
            base.Draw();
            var temperature = this.GetRoom(RegionType.Set_Passable).Temperature;
            var r = default(GenDraw.FillableBarRequest);
            r.center = DrawPos + (Vector3.up * 0.05f);
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

        // Token: 0x0600000B RID: 11 RVA: 0x000024A4 File Offset: 0x000006A4
        public override void TickRare()
        {
            base.TickRare();
            if (shouldSendAlert)
            {
                FleckMaker.ThrowMetaIcon(this.TrueCenter().ToIntVec3(), Map, FleckDefOf.IncapIcon);
            }
        }

        // Token: 0x0600000C RID: 12 RVA: 0x000024E4 File Offset: 0x000006E4
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
                var niceTemp = (float) Math.Round(currentTemp * 10f) / 10f;
                tempInfoAdd += niceTemp.ToStringTemperature("F0");
            }

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

            if (string.IsNullOrEmpty(tempInfoAdd))
            {
                return stringBuilder.ToString();
            }

            stringBuilder.AppendLine();
            stringBuilder.Append(tempInfoAdd);

            return stringBuilder.ToString();
        }

        // Token: 0x0600000D RID: 13 RVA: 0x000025A0 File Offset: 0x000007A0
        public override IEnumerable<Gizmo> GetGizmos()
        {
            yield return new Command_Action
            {
                icon = ContentFinder<Texture2D>.Get("UI/Commands/Alert_" + alertState),
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
}