﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;

namespace PolyNavi
{
	class ScheduleFragmentAdapter : FragmentStatePagerAdapter
	{
		private int numOfTabs;
		private ScheduleWeekFragment tab1;
		private ScheduleWeekFragment tab2;

		public ScheduleFragmentAdapter(FragmentManager fragmentManager, int numOfTabs) : base(fragmentManager)
		{
			this.numOfTabs = numOfTabs;
			
			tab1 = new ScheduleWeekFragment(PolyNaviLib.BL.Weeks.Current, MainApp.SharedPreferences.GetString("groupnumber_preference", null));
			tab2 = new ScheduleWeekFragment(PolyNaviLib.BL.Weeks.Next, MainApp.SharedPreferences.GetString("groupnumber_preference", null));
		}

		public override Fragment GetItem(int position)
		{
			switch (position)
			{
				case 0:
					return tab1;
				case 1:
					return tab2;
				default:
					return null;
			}
		}

		public override int Count
		{
			get
			{
				return numOfTabs;
			}
		}

	}

}