using Android.App;
using DictEt.Droid;

[assembly: Xamarin.Forms.Dependency(typeof(AndroidMethods))]
namespace DictEt.Droid
{
    public class AndroidMethods : IAndroidMethods
    {
        public void CloseApp()
        {
            var activity = (Activity)Android.App.Application.Context;
            activity.FinishAffinity();
            //Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
        }
    }
}