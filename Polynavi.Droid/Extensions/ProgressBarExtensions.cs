﻿using Android.Views;
using Android.Widget;

namespace Polynavi.Droid.Extensions
{
    internal static class ProgressBarExtensions
    {
        internal static void Show(this ProgressBar progressBar)
        {
            progressBar.Visibility = ViewStates.Visible;
        }

        internal static void Hide(this ProgressBar progressBar)
        {
            progressBar.Visibility = ViewStates.Invisible;
        }
    }
}
