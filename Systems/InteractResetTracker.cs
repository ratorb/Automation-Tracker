using Kitchen;
using KitchenTracker.Components;

namespace KitchenTracker.Systems
{
    public class InteractResetTracker : ItemInteractionSystem
    {
        private CItemTracker cTracker;
        protected override bool IsPossible(ref InteractionData data) =>
            Has<STrackerEnabled>() && Require(data.Target, out cTracker);

        protected override void Perform(ref InteractionData data)
        {
            cTracker.HeldItem = false;
            cTracker.CountForUpdate = 0;
            cTracker.TotalCount = 0;
            cTracker.StartTime = 0;
            cTracker.Average = 0;
            Set(data.Target, cTracker);
        }
    }
}
