using Verse;

public class TemperatureGaugeWall_PlaceWorker : PlaceWorker
{
    public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)

    {
        IntVec3 c = loc;

        Building support = c.GetEdifice(map);
        if (support == null)
        {
            return (AcceptanceReport)"MessagePlacementOnSupport".Translate();
        }

        if (
            (support.def == null) ||
            (support.def.graphicData == null)
        )
        {
            return (AcceptanceReport)"MessagePlacementOnSupport".Translate();
        }

        if ((support.def.graphicData.linkFlags & (LinkFlags.Rock | LinkFlags.Wall)) == 0)
            return (AcceptanceReport)"MessagePlacementOnSupport".Translate();

        c = loc + rot.FacingCell;
        if (!c.Walkable(map))
        {
            return (AcceptanceReport)"MessagePlacementTowardsWalkable".Translate();
        }
        return AcceptanceReport.WasAccepted;
    }

}