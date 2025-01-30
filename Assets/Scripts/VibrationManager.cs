using UnityEngine;

namespace BoomBox
{
    public static class VibrationManager
    {
#if UNITY_ANDROID
        public static AndroidJavaClass unityPlayer;
        public static AndroidJavaObject currentActivity;
        public static AndroidJavaObject vibration;
        public static AndroidJavaObject context;
        public static AndroidJavaClass vibrationEffect;
        public static int AndroidVersion
        {
            get
            {
                int iVersionNumber = 0;
                if (Application.platform == RuntimePlatform.Android)
                {
                    string androidVersion = SystemInfo.operatingSystem;
                    int sdkPos = androidVersion.IndexOf("API-");
                    iVersionNumber = int.Parse(androidVersion.Substring(sdkPos + 4, 2).ToString());
                }
                return iVersionNumber;
            }
        }
#endif

        private static bool bIsInitialized = false;
        public static void Init()
        {
            if (bIsInitialized) return;

#if UNITY_ANDROID
            if (Application.isMobilePlatform)
            {
                unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                vibration = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                context = currentActivity.Call<AndroidJavaObject>("getApplicationContext");

                if (AndroidVersion >= 26)
                {
                    vibrationEffect = new AndroidJavaClass("android.os.VibrationEffect");
                }
            }
#endif

            bIsInitialized = true;

            Application.targetFrameRate = 60;
        }

        public static void VibratePop()
        {
            if (Application.isMobilePlatform)
            {
#if UNITY_ANDROID
                VibrateAndroid(5);
#endif
            }
        }

        public static void VibratePeek()
        {
            if (Application.isMobilePlatform)
            {
#if UNITY_ANDROID
                VibrateAndroid(15);
#endif
            }
        }

        public static void VibrateNope()
        {
            if (Application.isMobilePlatform)
            {
#if UNITY_ANDROID
                long[] pattern = { 0, 50, 50, 50 };
                VibrateAndroid(pattern, -1);
#endif
            }
        }

        public static void VibrateStarCollect()
        {
            if (Application.isMobilePlatform)
            {
#if UNITY_ANDROID
                long[] pattern = { 0, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50, 50 };
                VibrateAndroid(pattern, -1);
#endif
            }
        }

#if UNITY_ANDROID
        public static void VibrateAndroid(long milliseconds)
        {
            if (Application.isMobilePlatform)
            {
                if (AndroidVersion >= 26)
                {
                    AndroidJavaObject createOneShot = vibrationEffect.CallStatic<AndroidJavaObject>("createOneShot", milliseconds, -1);
                    vibration.Call("vibrate", createOneShot);

                }
                else
                {
                    vibration.Call("vibrate", milliseconds);
                }
            }
        }

        public static void VibrateAndroid(long[] pattern, int repeat)
        {
            if (Application.isMobilePlatform)
            {
                if (AndroidVersion >= 26)
                {
                    AndroidJavaObject createWaveform = vibrationEffect.CallStatic<AndroidJavaObject>("createWaveform", pattern, repeat);
                    vibration.Call("vibrate", createWaveform);
                }
                else
                {
                    vibration.Call("vibrate", pattern, repeat);
                }
            }
        }
#endif

        public static void CancelAndroid()
        {
            if (Application.isMobilePlatform)
            {
#if UNITY_ANDROID
                vibration.Call("cancel");
#endif
            }
        }
    }
}
