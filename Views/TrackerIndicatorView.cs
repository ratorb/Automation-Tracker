using Kitchen;
using KitchenData;
using KitchenTracker.Components;
using MessagePack;
using System.Runtime.Remoting.Contexts;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace KitchenTracker.Views
{
    public class TrackerIndicatorView : UpdatableObjectView<TrackerIndicatorView.ViewData>
    {
        public GameObject DataParent;
        public GameObject AverageLabel;
        public TextMeshPro Average;
        public GameObject Calculating;
        public Transform Container;

        public GameObject IDParent;
        public TextMeshPro ID;
        public TextMeshPro Position;

        private ViewData Data = default;

        public override void Initialise()
        {
            base.Initialise();

            // never will understand why it doesn't persist
            Position.gameObject.transform.localPosition = new(0, 0.35f, 0);
        }

        protected override void UpdateData(ViewData data)
        {
            if (!data.IsChangedFrom(Data))
                return;

            if (data.ShowID)
            {
                DataParent.SetActive(false);
                IDParent.SetActive(true);

                ID.text = data.ID.ToString();
                Position.text = data.Side == TrackerPosition.Personal ? "This" : data.Side == TrackerPosition.Left ? "Left" : "Right";
            }
            else
            {
                IDParent.SetActive(false);

                if (data.Side != TrackerPosition.Personal)
                {
                    DataParent.SetActive(false);
                    return;
                }

                DataParent.SetActive(true);

                if (data.Average != 0)
                {
                    var increment = PrefManager.Get<float>("Increment");
                    var incText = increment == 1 ? "s" :
                        increment < 60f ? $"{increment}s" :
                        increment == 60f ? "m" :
                        $"{increment / 60f}m";

                    AverageLabel.SetActive(true);
                    Average.gameObject.SetActive(true);
                    Calculating.SetActive(false);

                    Average.text = (increment * data.Average).ToString("0.00") + '/' + incText;
                } else
                {
                    AverageLabel.SetActive(false);
                    Average.gameObject.SetActive(false);
                    Calculating.SetActive(true);
                }
            }
            Data = data;
        }

        [MessagePackObject]
        public struct ViewData : IViewData, IViewData.ICheckForChanges<ViewData>
        {
            [Key(1)] public TrackerPosition Side;
            [Key(2)] public bool ShowID;
            [Key(3)] public int ID;
            [Key(4)] public float Average;
            [Key(5)] public int Item;
            [Key(6)] public ItemList Components;

            public bool IsChangedFrom(ViewData check) =>
                ShowID != check.ShowID || ID != check.ID || Average != check.Average ||
                Item != check.Item || !Components.IsEquivalent(check.Components) || Side != check.Side;
        }

        private class UpdateView : IncrementalViewSystemBase<ViewData>
        {
            private EntityQuery Indicators;
            protected override void Initialise()
            {
                base.Initialise();
                Indicators = GetEntityQuery(typeof(CLinkedView), typeof(CItemTrackerIndicator), typeof(CIndicator));
                RequireSingletonForUpdate<STrackerEnabled>();
            }

            protected override void OnUpdate()
            {
                using (var views = Indicators.ToComponentDataArray<CLinkedView>(Allocator.Temp))
                {
                    using var indicators = Indicators.ToComponentDataArray<CIndicator>(Allocator.Temp);
                    for (int i = 0; i < views.Length; i++)
                    {
                        var source = indicators[i].Source;
                        if (!Require(source, out CItemTracker cTracker) || !Require(source, out CItemTrackerID cID))
                            continue;

                        SendUpdate(views[i], new()
                        {
                            Side = cID.Display,
                            ShowID = HasSingleton<SIsNightTime>(),
                            ID = cID.ID,
                            Average = cTracker.Average,
                            Item = cTracker.Item,
                            Components = cTracker.Components
                        });
                    }
                }
            }
        }
    }
}
