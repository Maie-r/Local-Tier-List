using Android.App;
using Android.Content.PM;
using Android.OS;

namespace Local_Tier_List
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Window.SetDecorFitsSystemWindows(true);

            // Ensure there's a container view with id "jumpToStart" available for fragment transactions.
            // Some libraries or fragments may try to attach to this id; if the hosting layout doesn't
            // provide it, add a FrameLayout programmatically to avoid "No view found for id" errors.
            try
            {
                var root = FindViewById(Android.Resource.Id.Content) as Android.Views.ViewGroup;
                if (root != null)
                {
                    // Resource.Id.jumpToStart comes from Resources/values/ids.xml (added to project)
                    var existing = root.FindViewById(Resource.Id.jumpToStart);
                    if (existing == null)
                    {
                        var frame = new Android.Widget.FrameLayout(this) { Id = Resource.Id.jumpToStart };
                        var lp = new Android.Views.ViewGroup.LayoutParams(Android.Views.ViewGroup.LayoutParams.MatchParent, Android.Views.ViewGroup.LayoutParams.MatchParent);
                        root.AddView(frame, lp);
                    }
                }
            }
            catch
            {
                // Avoid crashing here; if something goes wrong, leave default behavior.
            }
        }
    }
}
