/*
 * Copyright (c) 2014 Microsoft Mobile
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
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

            var xElement = XDocument.Load("WMAppManifest.xml").Root;
            if (xElement != null)
            {
                var version = xElement.Element("App").Attribute("Version").Value;

                var versionRun = new Run()
                {
                    Text = String.Format(AppResources.AboutPage_VersionRun_Text, version) + "\n"
                };

                VersionParagraph.Inlines.Add(versionRun);
            }

            // Application about text

            var aboutRun = new Run()
            {
                Text = AppResources.AboutPage_AboutRun_Text + "\n"
            };

            AboutParagraph.Inlines.Add(aboutRun);

            // Link to project homepage

            var projectRunText = AppResources.AboutPage_ProjectRun_Text;
            var projectRunTextSpans = projectRunText.Split(new string[] { "{0}" }, StringSplitOptions.None);

            var projectRunSpan1 = new Run {Text = projectRunTextSpans[0]};

            var projectsLink = new Hyperlink();
            projectsLink.Inlines.Add(AppResources.AboutPage_Hyperlink_Project);
            projectsLink.Click += ProjectsLink_Click;
            projectsLink.Foreground = new SolidColorBrush((Color)App.Current.Resources["PhoneForegroundColor"]);
            projectsLink.MouseOverForeground = new SolidColorBrush((Color)App.Current.Resources["PhoneAccentColor"]);

            var projectRunSpan2 = new Run {Text = projectRunTextSpans[1] + "\n"};

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