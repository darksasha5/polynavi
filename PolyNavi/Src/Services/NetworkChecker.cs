﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using PolyNaviLib.BL;

namespace PolyNavi
{

	public class NetworkChecker : INetworkChecker
	{
		Context context;

		public NetworkChecker(Context context)
		{
			this.context = context;
		}

		public bool Check()
		{
			ConnectivityManager cm = (ConnectivityManager)context.GetSystemService(Context.ConnectivityService);
			NetworkInfo info = cm.ActiveNetworkInfo;
			if (info != null && info.IsConnected)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}