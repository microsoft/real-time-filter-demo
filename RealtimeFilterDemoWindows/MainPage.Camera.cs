/*
 * Copyright (c) 2014 Nokia Corporation. All rights reserved.
 *
 * Nokia and Nokia Connecting People are registered trademarks of Nokia Corporation.
 * Other product and company names mentioned herein may be trademarks
 * or trade names of their respective owners.
 *
 * See the license text file for license information.
 */

using Nokia.Graphics.Imaging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;

namespace RealTimeFilterDemoWindows
{
    /// <summary>
    /// This partial class has all the camera functionality collected together
    /// </summary>
    partial class MainPage
    {
        // List of filters and their constructors used in this demo
        private readonly List<Filter> _filterList = new List<Filter>
        {
            new Filter { Name = "Lomo", Type = typeof(LomoFilter), Parameters = new object[]{0.7, 0.4, LomoVignetting.High, LomoStyle.Yellow}},
            new Filter { Name = "Magic Pen", Type = typeof(MagicPenFilter)},
            new Filter { Name = "Grayscale", Type = typeof(GrayscaleFilter)},
            new Filter { Name = "Antique", Type = typeof(AntiqueFilter)},
            new Filter { Name = "Stamp", Type = typeof(StampFilter), Parameters = new object[]{5, 0.7}},
            new Filter { Name = "Cartoon", Type = typeof(CartoonFilter), Parameters = new object[]{true}},
            new Filter { Name = "Sepia", Type = typeof(SepiaFilter)},
            new Filter { Name = "Sharpness", Type = typeof(SharpnessFilter), Parameters = new object[]{7}},
            new Filter { Name = "AutoEnhance", Type = typeof(AutoEnhanceFilter), Parameters = new object[]{true, true}},
            new Filter { Name = "No filter", Type = null},
        };

        private CameraPreviewImageSource _cameraPreviewImageSource; // Using camera as our image source
        private WriteableBitmap _writeableBitmap; // Target for our renderer
        private FilterEffect _effect; // Filter to be used on the camera image
        private WriteableBitmapRenderer _writeableBitmapRenderer; // renderer for our images
        private bool _isRendering; // Used to prevent multiple renderers running at once
        private bool _changeFilterRequest = true; // To indicate new filter selection by user
        private int _index; // Index of the currently active filter
        private bool _initialized;

        /// <summary>
        /// Initialize and start the camera preview
        /// </summary>
        private async void InitializeAsync()
        {
            StatusTextBlock.Text = "Starting camera...";

            _cameraPreviewImageSource = new CameraPreviewImageSource();
            _cameraPreviewImageSource.PreviewFrameAvailable += OnPreviewFrameAvailable;

            _effect = new FilterEffect(_cameraPreviewImageSource);

            _writeableBitmap = new WriteableBitmap(640, 480);
            _writeableBitmapRenderer = new WriteableBitmapRenderer(_effect, _writeableBitmap);

            CaptureImage.Source = _writeableBitmap;

            await _cameraPreviewImageSource.InitializeAsync(string.Empty);
            await _cameraPreviewImageSource.StartPreviewAsync();

            StatusTextBlock.Text = _filterList[_index].Name;

            _initialized = true;
        }

        /// <summary>
        /// Render a frame with the selected filter
        /// </summary>
        private async void OnPreviewFrameAvailable(IImageSize args)
        {
            // Prevent multiple rendering attempts at once
            if (!_isRendering)
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

                // Update the screen in the UI thread
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (_writeableBitmap != null)
                    {
                        StatusTextBlock.Text = _filterList[_index].Name;

                        _writeableBitmap.Invalidate();
                    }

                });

                _isRendering = false;
            }
        }
    }
}
