﻿using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Support.V4.Content;
using Android;
using Android.Locations;
using Android.Support.V4.App;
using Android.Support.Design.Widget;
using System.Runtime.Remoting.Contexts;
using System.Threading.Tasks;

namespace calculo_frete_correios.Droid
{
    [Activity(Label = "Calculo Entrega", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, ILocationListener
    {
        public static LocationManager locationManager;
        public static Location go;
        public static MainActivity main;
#pragma warning disable CS0649 // Campo "MainActivity.rootLayout" nunca é atribuído e sempre terá seu valor padrão null
        View rootLayout;
#pragma warning restore CS0649 // Campo "MainActivity.rootLayout" nunca é atribuído e sempre terá seu valor padrão null
        public static bool flg_track = false,flg_loc_init=false;
        public static double lat = 0, lng = 0;
        static readonly int RC_LAST_LOCATION_PERMISSION_CHECK = 1000;
        public static string ss;
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);
            main = this;
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) == Permission.Granted)
            {
                try
                {
                    
                    loc_init(this);
                }
                catch(Exception e)
                {
                    ss= e.Message;
                }
                

            }
            else
            {
                
                RequestLocationPermission(RC_LAST_LOCATION_PERMISSION_CHECK);
            }
            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());
        }
        public static void loc_init(MainActivity obj)
        {
            flg_loc_init = true;
            locationManager = obj.GetSystemService(LocationService) as LocationManager;
            locationManager.RequestLocationUpdates(LocationManager.GpsProvider, 0, 0, obj);
        }
        void RequestLocationPermission(int requestCode)
        {
            if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.AccessFineLocation))
            {
                Snackbar.Make(rootLayout, 2131165206, Snackbar.LengthIndefinite)
                        .SetAction(2131165186,
                                   delegate
                                   {
                                       ActivityCompat.RequestPermissions(this, new[] { Manifest.Permission.AccessFineLocation }, requestCode);
                                   })
                        .Show();
            }
            else
            {
                ActivityCompat.RequestPermissions(this, new[] { Manifest.Permission.AccessFineLocation }, requestCode);
            }
        }

        public void OnLocationChanged(Location location)
        {
            lat = location.Latitude;
            lng = location.Longitude;
            flg_track = true;
            go = location;
            //throw new NotImplementedException();
        }

        public void OnProviderDisabled(string provider)
        {
        }

        public void OnProviderEnabled(string provider)
        {
        }

        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {

        }
        
    }
}

