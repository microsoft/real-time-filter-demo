/**
 * Copyright (c) 2013 Nokia Corporation.
 */

using System;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace RealtimeFilterDemo
{
    public partial class AboutPage : PhoneApplicationPage
    {
        public AboutPage()
        {
            InitializeComponent();
            string appVersion = System.Xml.Linq.XDocument.Load("WMAppManifest.xml").Root.Element("App").Attribute("Version").Value;
            VersionTextBlock.Text = "Version " + appVersion;
        }

        private void BackButtonClick(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}