namespace TempGauge;

public static class GaugeSettings_Clipboard
{
    private static bool onHighTemp;

    private static float targetTemperature;

    private static AlertState alertState = AlertState.Normal;

    public static void Copy(bool highTemp, float target)
    {
        onHighTemp = highTemp;
        targetTemperature = target;
    }

    public static void Copy(bool highTemp, float target, AlertState state)
    {
        Copy(highTemp, target);
        alertState = state;
    }

    public static void PasteInto(out bool highTemp, out float target)
    {
        highTemp = onHighTemp;
        target = targetTemperature;
    }

    public static void PasteInto(out bool highTemp, out float target, out AlertState state)
    {
        PasteInto(out highTemp, out target);
        state = alertState;
    }
}