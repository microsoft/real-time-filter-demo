/*
 * Copyright © 2013 Nokia Corporation. All rights reserved.
 * Nokia and Nokia Connecting People are registered trademarks of Nokia Corporation. 
 * Other product and company names mentioned herein may be trademarks
 * or trade names of their respective owners. 
 * See LICENSE.TXT for license information.
 */
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using RealtimeFilterDemo.Resources;
using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Xml.Linq;

namespace RealtimeFilterDemo
{
    public partial class AboutPage : PhoneApplicationPage
    {
        public AboutPage()
        {
            InitializeComponent();

            // Application version number

            var version = XDocument.Load("WMAppManifest.xml").Root.Element("App").Attribute("Version").Value;

            var versionRun = new Run()
            {
                Text = String.Format(AppResources.AboutPage_VersionRun_Text, version) + "\n"
            };

            VersionParagraph.Inlines.Add(versionRun);

            // Application about text

            var aboutRun = new Run()
            {
                Text = AppResources.AboutPage_AboutRun_Text + "\n"
            };

            AboutParagraph.Inlines.Add(aboutRun);

            // Link to project homepage

            var projectRunText = AppResources.AboutPage_ProjectRun_Text;
            var projectRunTextSpans = projectRunText.Split(new string[] { "{0}" }, StringSplitOptions.None);

            var projectRunSpan1 = new Run();
            projectRunSpan1.Text = projectRunTextSpans[0];

            var projectsLink = new Hyperlink();
            projectsLink.Inlines.Add(AppResources.AboutPage_Hyperlink_Project);
            projectsLink.Click += ProjectsLink_Click;
            projectsLink.Foreground = new SolidColorBrush((Color)App.Current.Resources["PhoneForegroundColor"]);
            projectsLink.MouseOverForeground = new SolidColorBrush((Color)App.Current.Resources["PhoneAccentColor"]);

            var projectRunSpan2 = new Run();
            projectRunSpan2.Text = projectRunTextSpans[1] + "\n";

            ProjectParagraph.Inlines.Add(projectRunSpan1);
            ProjectParagraph.Inlines.Add(projectsLink);
            ProjectParagraph.Inlines.Add(projectRunSpan2);
        }

        private void ProjectsLink_Click(object sender, RoutedEventArgs e)
        {
            var webBrowserTask = new WebBrowserTask()
            {
                Uri = new Uri(AppResources.AboutPage_Hyperlink_Project, UriKind.Absolute)
            };

            webBrowserTask.Show();
        }
    }
}