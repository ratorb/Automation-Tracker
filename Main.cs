global using static KitchenTracker.Main;
using Kitchen;
using KitchenLib;
using KitchenLib.Utils;
using KitchenLib.Views;
using KitchenMods;
using KitchenTracker.GDOs;
using KitchenTracker.Systems;
using KitchenTracker.Utility;
using KitchenTracker.Views;
using PreferenceSystem;
using Shapes;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using static PreferenceSystem.PreferenceSystemManager;

namespace KitchenTracker
{
    public class Main : BaseMod
    {
        public const string NAME = "Automation Tracker";
        public const string GUID = "nova.production-tracker";
        public const string VERSION = "1.1.2";

        public Main() : base(GUID, NAME, "Zoey Davis", VERSION, ">=1.0.0", Assembly.GetExecutingAssembly()) { }

        private static AssetBundle Bundle;
        public static PreferenceSystemManager PrefManager;

        #region References
        public static CustomViewType TrackerDisplay { get; private set; }
        public static CustomViewType TrackerIndicator { get; private set; }

        public static int BinID { get; private set; }
        public static int SpotID { get; private set; }
        #endregion

        protected override void OnPostActivate(Mod mod)
        {
            Bundle = mod.GetPacks<AssetBundleModPack>().SelectMany(e => e.AssetBundles).First();

            SetupMenu();

            SetupTrackerDisplay();
            SetupTrackerIndicator();

            BinID = AddGameDataObject<TrackerBin>().ID;
            SpotID = AddGameDataObject<TrackerSpot>().ID;
        }

        #region Views
        private void SetupTrackerDisplay()
        {
            TrackerDisplay = AddViewType("Basic Tracker", () =>
            {
                var prefab = GetPrefab("Tracker Display");

                var trackerItem = prefab.GetChild(0);
                trackerItem.ApplyMaterialToChild("Panel", "UI Panel - Help");
                var rect = trackerItem.ApplyMaterialToChild("Image", "Flat Image").ApplyMaterialToChild("Backing", "Rect Transparent [CORNER_RADIUS]").TryAddComponent<Rectangle>();
                rect.CornerRadius = 0.05f;
                rect.Height = 0.5f;
                rect.Width = 0.5f;
                rect.Type = Rectangle.RectangleType.RoundedSolid;
                rect.Color = new(0.06f, 0.05f, 0.15f, 0.5f);
                trackerItem.transform.CreateLabel("Average Label", new(0f, 0.25f, -0.01f), Quaternion.identity,
                    MaterialUtils.GetExistingMaterial("Alphakind Atlas Material"), FontUtils.GetExistingTMPFont("Large Text"), 0, 2.25f,
                    "Average");
                trackerItem.transform.CreateLabel("Average", new(0f, -0.25f, -0.01f), Quaternion.identity,
                    MaterialUtils.GetExistingMaterial("Alphakind Atlas Material"), FontUtils.GetExistingTMPFont("Large Text"), 0, 2.5f,
                    "0.000/min");
                trackerItem.transform.CreateLabel("Calculating", new(0f, 0f, -0.01f), Quaternion.identity,
                    MaterialUtils.GetExistingMaterial("Alphakind Atlas Material"), FontUtils.GetExistingTMPFont("Large Text"), 0, 3f,
                    "Calculating\nAverage");
                trackerItem.transform.CreateLabel("ID", new(0f, 0f, -0.01f), Quaternion.identity,
                    MaterialUtils.GetExistingMaterial("Alphakind Atlas Material"), FontUtils.GetExistingTMPFont("Large Text"), 0, 8f,
                    "0");

                var view = prefab.TryAddComponent<TrackerDisplayView>();
                view.BaseItem = trackerItem;
                view.Offset = new(0.85f, 1f);
                view.Spacing = 1f;

                return prefab;
            });
        }

        public void SetupTrackerIndicator()
        {
            TrackerIndicator = AddViewType("Tracker Indicator", () =>
            {
                var prefab = GetPrefab("Tracker Indicator");

                var view = prefab.TryAddComponent<TrackerIndicatorView>();

                var data = prefab.GetChild("Data");
                view.DataParent = data;
                data.ApplyMaterialToChild("Panel", "UI Panel - Help");
                view.AverageLabel = data.transform.CreateLabel("Average Label", new(0, 0.15f, 0), Quaternion.identity,
                    MaterialUtils.GetExistingMaterial("Alphakind Atlas Material"), FontUtils.GetExistingTMPFont("Large Text"), 0, 1.75f,
                    "Average");
                view.Average = data.transform.CreateLabel("Average", new(0, -0.15f, 0), Quaternion.identity,
                    MaterialUtils.GetExistingMaterial("Alphakind Atlas Material"), FontUtils.GetExistingTMPFont("Large Text"), 0, 2.25f,
                    "0.000/min").GetComponent<TextMeshPro>();
                view.Calculating = data.transform.CreateLabel("Calculating", new(0, 0, 0), Quaternion.identity,
                    MaterialUtils.GetExistingMaterial("Alphakind Atlas Material"), FontUtils.GetExistingTMPFont("Large Text"), 0, 2f,
                    "Calculating\nAverage");
                view.Container = data.GetChild("Container").transform;

                var id = prefab.GetChild("ID");
                view.IDParent = id;

                var rect = id.ApplyMaterialToChild("Panel", "Rect Transparent [CORNER_RADIUS]").TryAddComponent<Rectangle>();
                rect.CornerRadius = 0.05f;
                rect.Height = 0.5f;
                rect.Width = 0.5f;
                rect.Type = Rectangle.RectangleType.RoundedSolid;
                rect.Color = new(0.06f, 0.05f, 0.15f, 0.5f);


                view.ID = id.transform.CreateLabel("ID", new(0, -0.04f, 0), Quaternion.identity,
                    MaterialUtils.GetExistingMaterial("Alphakind Atlas Material"), FontUtils.GetExistingTMPFont("Large Text"), 0, 3.5f,
                    "0").GetComponent<TextMeshPro>();
                view.Position = id.transform.CreateLabel("Position", new(0, 0.45f, 0), Quaternion.identity,
                    MaterialUtils.GetExistingMaterial("Alphakind Atlas Material"), FontUtils.GetExistingTMPFont("Large Text"), 0, 2f,
                    "None").GetComponent<TextMeshPro>();

                return prefab;
            });
        }
        #endregion

        #region Menu
        private void SetupMenu()
        {
            PrefManager = new(GUID, "Automation Tracker");

            PrefManager
                .AddConditionalBlocker(() => Session.NetworkedPlayState != Platforms.NetworkedPlayState.Host || !TrackingController.CanModifyTrackers())
                    .AddConditionalBlocker(TrackingController.IsTracking)
                        .AddButton("Enable", TrackingController.EnableTracking)
                    .ConditionalBlockerDone()
                    .AddConditionalBlocker(() => !TrackingController.IsTracking())
                        .AddButton("Disable", TrackingController.DisableTracking)
                    .ConditionalBlockerDone()
                .ConditionalBlockerDone()

                .AddLabel("Time Increment")
                .AddOption("Increment", 60f, new float[] { 1f, 30f, 60f, 120f, 500f },
                    new string[] { "Second", "30 Seconds", "Minute", "2 Minutes", "5 Minutes" })
                .AddInfo("Solely a visual adjustment. Changes nothing internally.");

            PrefManager.RegisterMenu(MenuType.PauseMenu);
        }
        #endregion

        #region Generic Util
        public static GameObject GetPrefab(string name) => Bundle.LoadAsset<GameObject>(name);
        #endregion
    }
}
