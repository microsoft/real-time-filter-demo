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

using Lumia.Imaging;
using Lumia.Imaging.Adjustments;
using Lumia.Imaging.Artistic;
using RealtimeFilterDemo.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;

namespace RealtimeFilterDemo
{
    /// <summary>
    /// This partial class has all the camera functionality collected together
    /// </summary>
    public class MainPageViewModel : INotifyPropertyChanged
    {
        // List of filters and their constructors used in this demo
        private readonly List<Filter> _filterList = new List<Filter>
        {
            new Filter { Name = "No Filter", Type = null},
            new Filter { Name = "Lomo", Type = typeof(LomoFilter), Parameters = new object[]{0.7, 0.4, LomoVignetting.High, LomoStyle.Yellow}},
            new Filter { Name = "Magic Pen", Type = typeof(MagicPenFilter)},
            new Filter { Name = "Grayscale", Type = typeof(GrayscaleFilter)},
            new Filter { Name = "Antique", Type = typeof(AntiqueFilter)},
            new Filter { Name = "Stamp", Type = typeof(StampFilter), Parameters = new object[]{5, 0.7}},
            new Filter { Name = "Cartoon", Type = typeof(CartoonFilter), Parameters = new object[]{true}},
            new Filter { Name = "Sepia", Type = typeof(SepiaFilter)},
            new Filter { Name = "Sharpness", Type = typeof(SharpnessFilter), Parameters = new object[]{7}},
            new Filter { Name = "Auto Enhance", Type = typeof(AutoEnhanceFilter), Parameters = new object[]{true, true}}
        };

        private string _status;
        private CameraPreviewImageSource _cameraPreviewImageSource; // Using camera as our image source
        private WriteableBitmap _writeableBitmap; // Target for our renderer
        private FilterEffect _effect; // Filter to be used on the camera image
        private WriteableBitmapRenderer _writeableBitmapRenderer; // renderer for our images
        private bool _isRendering; // Used to prevent multiple renderers running at once
        private bool _changeFilterRequest = true; // To indicate new filter selection by user
        private int _index = 0; // Index of the currently active filter
        private bool _initialized;

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public string Status
        {
            get
            {
                return _status;
            }

            private set
            {
                if (_status != value)
                {
                    _status = value;

                    NotifyPropertyChanged("Status");
                }
            }
        }

        public int FilterIndex
        {
            get
            {
                return _index;
            }

            set
            {
                if (_index != value)
                {
                    _index = value;
                    _changeFilterRequest = true;

                    NotifyPropertyChanged("FilterIndex");
                }
            }
        }

        public bool Initialized
        {
            get
            {
                return _initialized;
            }

            private set
            {
                if (_initialized != value)
                {
                    _initialized = value;

                    NotifyPropertyChanged("Initialized");
                }
            }
        }

        public WriteableBitmap PreviewBitmap
        {
            get
            {
                return _writeableBitmap;
            }

            private set
            {
                if (_writeableBitmap != value)
                {
                    _writeableBitmap = value;

                    NotifyPropertyChanged("PreviewBitmap");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public RelayCommand NextFilterCommand { get; private set; }
        public RelayCommand PreviousFilterCommand { get; private set; }

        public MainPageViewModel()
        {
            NextFilterCommand = new RelayCommand(
                () =>
                    {
                        if (_index < _filterList.Count - 1)
                        {
                            _index++;
                            _changeFilterRequest = true;

                            NextFilterCommand.RaiseCanExecuteChanged();
                            PreviousFilterCommand.RaiseCanExecuteChanged();
                        }
                    },
                () =>
                    {
                        return Initialized && _index < _filterList.Count - 1;
                    });

            PreviousFilterCommand = new RelayCommand(
                () =>
                {
                    if (_index > 0)
                    {
                        _index--;
                        _changeFilterRequest = true;

                        NextFilterCommand.RaiseCanExecuteChanged();
                        PreviousFilterCommand.RaiseCanExecuteChanged();
                    }
                },
                () =>
                {
                    return Initialized && _index > 0;
                });
        }

        /// <summary>
        /// Initialize and start the camera preview
        /// </summary>
        public async Task InitializeAsync()
        {
            Status = "Starting camera...";

            // Create a camera preview image source (from Imaging SDK)
            _cameraPreviewImageSource = new CameraPreviewImageSource();
            await _cameraPreviewImageSource.InitializeAsync(string.Empty);
            var properties = await _cameraPreviewImageSource.StartPreviewAsync();

            // Create a preview bitmap with the correct aspect ratio
            var width = 640.0;
            var height = (width / properties.Width) * properties.Height;
            var bitmap = new WriteableBitmap((int)width, (int)height);

            PreviewBitmap = bitmap;

            // Create a filter effect to be used with the source (no filters yet)
            _effect = new FilterEffect(_cameraPreviewImageSource);
            _writeableBitmapRenderer = new WriteableBitmapRenderer(_effect, _writeableBitmap);

            // Attach preview frame delegate
            _cameraPreviewImageSource.PreviewFrameAvailable += OnPreviewFrameAvailable;

            Status = _filterList[_index].Name;

            Initialized = true;
            NextFilterCommand.RaiseCanExecuteChanged();
            PreviousFilterCommand.RaiseCanExecuteChanged();
        }

        public async Task PausePreviewAsync()
        {
            if (Initialized)
            {
                await _cameraPreviewImageSource.StopPreviewAsync();
            }
        }

        public async Task ResumePreviewAsync()
        {
            if (Initialized)
            {
                await _cameraPreviewImageSource.InitializeAsync(string.Empty);
                await _cameraPreviewImageSource.StartPreviewAsync();
            }
        }

        /// <summary>
        /// Render a frame with the selected filter
        /// </summary>
        private async void OnPreviewFrameAvailable(IImageSize args)
        {
            // Prevent multiple rendering attempts at once
            if (Initialized && !_isRendering)
            {
                _isRendering = true;

                // User changed the filter, let's update it before rendering
                if (_changeFilterRequest)
                {
                    if (_filterList[_index].Type != null)
                    {
                        // Use reflection to create a new filter class
                        var filter = (IFilter)Activator.CreateInstance(_filterList[_index].Type, _filterList[_index].Parameters);

                        _effect.Filters = new[] { filter };
                    }
                    else
                    {
                        _effect.Filters = new IFilter[0];
                    }

                    _changeFilterRequest = false;
                }

                // Render the image with the filter
                await _writeableBitmapRenderer.RenderAsync();

                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                    () =>
                        {
                            Status = _filterList[_index].Name;

                            _writeableBitmap.Invalidate();
                        });

                _isRendering = false;
            }
        }
    }
}
