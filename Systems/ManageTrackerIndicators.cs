using Kitchen;
using KitchenTracker.Components;
using Unity.Entities;

namespace KitchenTracker.Systems
{
    public class ManageTrackerIndicators : IndicatorManager
    {
        protected override ViewType ViewType => TrackerIndicator;

        protected override EntityQuery GetCandidateQuery()
        {
            return GetEntityQuery(typeof(CPosition), typeof(CItemTracker), typeof(CItemTrackerID));
        }

        protected override bool ShouldHaveIndicator(Entity candidate) =>
            Require(candidate, out CItemTrackerID id) && id.Display != TrackerPosition.None && !Has<CHeldBy>(candidate);

        protected override Entity CreateIndicator(Entity source)
        {
            var indicator = base.CreateIndicator(source);
            Set<CPosition>(indicator, GetComponent<CPosition>(source).Position);
            Set<CItemTrackerIndicator>(indicator);
            return indicator;
        }
    }
}
