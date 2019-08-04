﻿using Android.OS;
using Android.Views;

namespace PolyNavi
{
    public class MainBuildingMapFragment : Android.Support.V4.App.Fragment
	{
		public MainBuildingView MapView { get; private set; }
	    private int DrawawbleId { get; }

		public MainBuildingMapFragment(int id)
		{
			DrawawbleId = id;
		}

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			MapView = new MainBuildingView(Activity.BaseContext, DrawawbleId);
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			return MapView;
		}
	}
}