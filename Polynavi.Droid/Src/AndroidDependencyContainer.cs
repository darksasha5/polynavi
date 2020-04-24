﻿using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Android.Content;
using AndroidX.Preference;
using Polynavi.Bll;
using Polynavi.Common.Repositories;
using Polynavi.Common.Services;
using Polynavi.Dal;
using Polynavi.Droid.Services;
using Xamarin.Android.Net;

namespace Polynavi.Droid.Src
{
    internal sealed class AndroidDependencyContainer : BllDependencyContainer
    {
        private static Context context;

        public static AndroidDependencyContainer Instance { get; private set; }

        public static void EnsureInitialized(Context c)
        {
            if (Instance != null)
                return;

            Instance = new AndroidDependencyContainer();
            context = c;
        }

        protected override HttpClient CreateHttpClient()
        {
            var httpHandler = new AndroidClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            return new HttpClient(httpHandler, true);
        }

        protected override INetworkChecker CreateNetworkChecker() =>
            new NetworkChecker(context);

        protected override async Task<IScheduleRepository> CreateScheduleRepository() =>
            await Polynavi.Dal.ScheduleRepository.CreateAsync(SettingsProvider, 
                new SQLiteDatabase(MainApp.GetFileFullPath(MainApp.DatabaseFilename))); //TODO

        protected override ISettingsProvider CreateSettingsProvider() => 
            new SettingsProvider(PreferenceManager.GetDefaultSharedPreferences(context));
    }
}