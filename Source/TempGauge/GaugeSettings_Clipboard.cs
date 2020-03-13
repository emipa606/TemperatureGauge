using System;

namespace TempGauge
{
	// Token: 0x02000007 RID: 7
	public static class GaugeSettings_Clipboard
	{
		// Token: 0x06000029 RID: 41 RVA: 0x00002C7D File Offset: 0x00000E7D
		public static void Copy(bool highTemp, float target)
		{
			GaugeSettings_Clipboard.onHighTemp = highTemp;
			GaugeSettings_Clipboard.targetTemperature = target;
		}

		// Token: 0x0600002A RID: 42 RVA: 0x00002C8C File Offset: 0x00000E8C
		public static void Copy(bool highTemp, float target, AlertState state)
		{
			GaugeSettings_Clipboard.Copy(highTemp, target);
			GaugeSettings_Clipboard.alertState = state;
		}

		// Token: 0x0600002B RID: 43 RVA: 0x00002C9D File Offset: 0x00000E9D
		public static void PasteInto(out bool highTemp, out float target)
		{
			highTemp = GaugeSettings_Clipboard.onHighTemp;
			target = GaugeSettings_Clipboard.targetTemperature;
		}

		// Token: 0x0600002C RID: 44 RVA: 0x00002CAE File Offset: 0x00000EAE
		public static void PasteInto(out bool highTemp, out float target, out AlertState state)
		{
			GaugeSettings_Clipboard.PasteInto(out highTemp, out target);
			state = GaugeSettings_Clipboard.alertState;
		}

		// Token: 0x04000015 RID: 21
		private static bool onHighTemp = false;

		// Token: 0x04000016 RID: 22
		private static float targetTemperature = 0f;

		// Token: 0x04000017 RID: 23
		private static AlertState alertState = AlertState.Normal;
	}
}
