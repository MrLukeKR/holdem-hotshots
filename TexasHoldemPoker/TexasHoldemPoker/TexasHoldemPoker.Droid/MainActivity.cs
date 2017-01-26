﻿using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using Urho.Droid;

namespace TexasHoldemPoker.Droid
{
	[Activity (Label = "Hold 'em Hotshots", Icon = "@drawable/icon", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
	public class MainActivity : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
            base.Window.RequestFeature(WindowFeatures.ActionBar);
            base.SetTheme(global::Android.Resource.Style.ThemeNoTitleBarFullScreen);
            base.OnCreate (bundle);

#pragma warning disable
            var mLayout = new AbsoluteLayout(this); //Although it's obselete - Urho3D requires absolute layout to not crash :(
            var surface = UrhoSurface.CreateSurface<Poker>(this, new Urho.ApplicationOptions("Data"));
            mLayout.AddView(surface);
            SetContentView(mLayout);
        }
    
        protected override void OnResume()
        {
            UrhoSurface.OnResume();
            base.OnResume();
        }

        protected override void OnPause()
        {
            UrhoSurface.OnPause();
            base.OnPause();
        }

        public override void OnLowMemory()
        {
            UrhoSurface.OnLowMemory();
            base.OnLowMemory();
        }

        protected override void OnDestroy()
        {
            UrhoSurface.OnDestroy();
            base.OnDestroy();
        }

        public override bool DispatchKeyEvent(KeyEvent e)
        {
            if (!UrhoSurface.DispatchKeyEvent(e))
                return false;
            return base.DispatchKeyEvent(e);
        }

        public override void OnWindowFocusChanged(bool hasFocus)
        {
            UrhoSurface.OnWindowFocusChanged(hasFocus);
            base.OnWindowFocusChanged(hasFocus);
        }

        public void rotateToLandscape()
        {
            base.RequestedOrientation = ScreenOrientation.Landscape;
        }

        public void rotateToPortrait()
        {
            base.RequestedOrientation = ScreenOrientation.Portrait;
        }
    }
}

