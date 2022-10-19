using Verse;

public class TemperatureGaugeWall_PlaceWorker : PlaceWorker
{
    public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map,
        Thing thingToIgnore = null, Thing thing = null)

    {
        var c = loc;

        var support = c.GetEdifice(map);
        if (support == null)
        {
            return "MessagePlacementOnSupport".Translate();
        }

        if (support.def?.graphicData == null)
        {
            return "MessagePlacementOnSupport".Translate();
        }

        if ((support.def.graphicData.linkFlags & (LinkFlags.Rock | LinkFlags.Wall)) == 0)
        {
            return "MessagePlacementOnSupport".Translate();
        }

        c = loc + rot.FacingCell;
        return !c.Walkable(map) ? "MessagePlacementTowardsWalkable".Translate() : AcceptanceReport.WasAccepted;
    }
}