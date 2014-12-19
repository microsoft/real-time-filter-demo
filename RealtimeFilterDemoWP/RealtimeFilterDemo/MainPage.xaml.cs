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
using Microsoft.Phone.Shell;
using RealtimeFilterDemo.Resources;
using System;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using Windows.Phone.Media.Capture;

namespace RealtimeFilterDemo
{
    public partial class MainPage : PhoneApplicationPage
    {
        private MediaElement _mediaElement = null;
        private Effects _cameraEffect = null;
        private CameraStreamSource _cameraStreamSource = null;
        private Semaphore _cameraSemaphore = new Semaphore(1, 1);

        public MainPage()
        {
            InitializeComponent();

            ApplicationBar = new ApplicationBar();

            var previousButton = new ApplicationBarIconButton(new Uri("/Assets/Icons/previous.png", UriKind.Relative))
            {
                Text = AppResources.PreviousEffectButtonText
            };
            previousButton.Click += PreviousButton_Click;

            ApplicationBar.Buttons.Add(previousButton);

            var nextButton = new ApplicationBarIconButton(new Uri("/Assets/Icons/next.png", UriKind.Relative))
            {
                Text = AppResources.NextEffectButtonText
            };
            nextButton.Click += NextButton_Click;

            ApplicationBar.Buttons.Add(nextButton);

            var aboutMenuItem = new ApplicationBarMenuItem {Text = AppResources.AboutPageButtonText};
            aboutMenuItem.Click += AboutMenuItem_Click;

            ApplicationBar.MenuItems.Add(aboutMenuItem);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (App.Camera != null)
            {
                Initialize();
            }
            else
            {
                StatusTextBlock.Text = AppResources.MainPage_Status_InitializingCameraFailed;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            Uninitialize();

        }

        protected override void OnOrientationChanged(OrientationChangedEventArgs e)
        {
            base.OnOrientationChanged(e);

            if (App.Camera != null)
            {
                double canvasAngle;

                if (Orientation.HasFlag(PageOrientation.LandscapeLeft))
                {
                    canvasAngle = App.Camera.SensorRotationInDegrees - 90;
                }
                else if (Orientation.HasFlag(PageOrientation.LandscapeRight))
                {
                    canvasAngle = App.Camera.SensorRotationInDegrees + 90;
                }
                else // PageOrientation.PortraitUp
                {
                    canvasAngle = App.Camera.SensorRotationInDegrees;
                }

                BackgroundVideoBrush.RelativeTransform = new RotateTransform()
                {
                    CenterX = 0.5,
                    CenterY = 0.5,
                    Angle = canvasAngle
                };

                App.Camera.SetProperty(KnownCameraGeneralProperties.EncodeWithOrientation, canvasAngle);
            }
        }

        private async void Initialize()
        {
            StatusTextBlock.Text = AppResources.MainPage_StatusTextBlock_StartingCamera;

            _cameraEffect = new Effects() {PhotoCaptureDevice = App.Camera};
            
            _cameraStreamSource = new CameraStreamSource(_cameraEffect, App.Camera.CaptureResolution);
            _cameraStreamSource.FrameRateChanged += CameraStreamSource_FPSChanged;

            _mediaElement = new MediaElement {Stretch = Stretch.UniformToFill, BufferingTime = new TimeSpan(0)};
            _mediaElement.SetSource(_cameraStreamSource);

            // Using VideoBrush in XAML instead of MediaElement, because otherwise
            // CameraStreamSource.CloseMedia() does not seem to be called by the framework:/

            BackgroundVideoBrush.SetSource(_mediaElement);

            StatusTextBlock.Text = _cameraEffect.EffectName;
        }

        private void Uninitialize()
        {
            StatusTextBlock.Text = "";

            if (_mediaElement != null)
            {
                _mediaElement.Source = null;
                _mediaElement = null;
            }

            if (_cameraStreamSource != null)
            {
                _cameraStreamSource.FrameRateChanged -= CameraStreamSource_FPSChanged;
                _cameraStreamSource = null;
            }

            _cameraEffect = null;
        }

        private void AboutMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            _cameraEffect.NextEffect();

            StatusTextBlock.Text = _cameraEffect.EffectName;
        }

        private void PreviousButton_Click(object sender, EventArgs e)
        {
            _cameraEffect.PreviousEffect();

            StatusTextBlock.Text = _cameraEffect.EffectName;
        }

        private void CameraStreamSource_FPSChanged(object sender, int e)
        {
            FrameRateTextBlock.Text = String.Format(AppResources.MainPage_FrameRateTextBlock_Format, e);
        }

        private async void LayoutRoot_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (_cameraSemaphore.WaitOne(100))
            {
                await App.Camera.FocusAsync();

                _cameraSemaphore.Release();
            }
        }
    }
}