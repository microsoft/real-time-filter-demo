﻿/*
 * Copyright (c) 2014 Nokia Corporation. All rights reserved.
 *
 * Nokia and Nokia Connecting People are registered trademarks of Nokia Corporation.
 * Other product and company names mentioned herein may be trademarks
 * or trade names of their respective owners.
 *
 * See the license text file for license information.
 */
using RealTimeFilterDemoWindows.Common;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace RealTimeFilterDemoWindows
{
    /// <summary>
    /// This partial class has the navigation logic, see MainPage.Camera.cs for Imaging SDK functionality
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        /// <summary>
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Constructor, includes the call to camera creation
        /// </summary>
        public MainPage()
        {
            InitializeComponent();

            navigationHelper = new NavigationHelper(this);
            navigationHelper.LoadState += navigationHelper_LoadState;
            navigationHelper.SaveState += navigationHelper_SaveState;

            Window.Current.VisibilityChanged += Current_VisibilityChanged;

            InitializeAsync();
        }

        /// <summary>
        /// Toggles on and off the camera when app loses/gains visibility on the screen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Current_VisibilityChanged(object sender, Windows.UI.Core.VisibilityChangedEventArgs e)
        {
            try
            {
                if (_initialized)
                {
                    if (e.Visible)
                    {
                        await _cameraPreviewImageSource.InitializeAsync(string.Empty);
                        await _cameraPreviewImageSource.StartPreviewAsync();
                    }
                    else
                    {
                        await _cameraPreviewImageSource.StopPreviewAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Next button was pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnNextEffect(object sender, RoutedEventArgs e)
        {
            if (_index < _filterList.Count - 1)
            {
                _index++;
                _changeFilterRequest = true;
            }
        }

        /// <summary>
        /// Previous button was pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPreviousEffect(object sender, RoutedEventArgs e)
        {
            if (_index > 0)
            {
                _index--;
                _changeFilterRequest = true;
            }
        }

        private void OnTapped(object sender, TappedRoutedEventArgs e)
        {
            TopAppBar.IsOpen = !TopAppBar.IsOpen;
            BottomAppBar.IsOpen = TopAppBar.IsOpen;
        }

        #region Saving and loading state

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            if (e.PageState != null && e.PageState.ContainsKey("filter"))
            {
                int.TryParse(e.PageState["filter"].ToString(), out _index);
            }
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            e.PageState["filter"] = _index;
        }

        #endregion

        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
    }
}