using KitchenData;
using Unity.Entities;

namespace KitchenTracker.Components
{
    // No longer tracking days through buffers but instead through saved JSON files
    [InternalBufferCapacity(8)]
    public struct CTrackedDay : IBufferElementData // Discarded
    {
        public int Day;
        public int ItemCount;
        public float Average;
    }

    // Old Tracking Methods

    public struct STrackingItems : IComponentData
    {
        public float Ticker;
    }

    public struct CTrackerDisplaySurrogate : IComponentData, TypeHash.ISurrogate<CTrackerDisplay> // Surrogate
    {
        public IComponentData Convert() => new CTrackerDisplay();
    }

    public struct CTrackedItem : IComponentData // Discarded
    {
        public int Item;
        public ItemList Components;
        public float TimeSinceRefresh;
        public int TrackedItems;
    }

    /*public struct CItemTrackerSurrogate : IApplianceProperty, IComponentData, TypeHash.ISurrogate<CItemTracker> // Surrogate
    {
        public bool FullHolder;
        public bool DestroyItem;

        public IComponentData Convert() => new CItemTracker { DestroyItem = DestroyItem, UpdateAfterCount = 1 };
    }*/
}
