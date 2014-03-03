using Nokia.Graphics.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private readonly List<FilterListObject> _filterList = new List<FilterListObject>
        {
            new FilterListObject { Name="LomoFilter", Constructor = new object[]{0.7, 0.4, LomoVignetting.High, LomoStyle.Yellow}},
            new FilterListObject { Name="MagicPenFilter"},
            new FilterListObject { Name="GrayscaleFilter"},
            new FilterListObject { Name="AntiqueFilter"},
            new FilterListObject { Name="StampFilter", Constructor = new object[]{5, 0.7}},
            new FilterListObject { Name="CartoonFilter", Constructor = new object[]{true}},
            new FilterListObject { Name="SepiaFilter"},
            new FilterListObject { Name="SharpnessFilter", Constructor = new object[]{7}},
            new FilterListObject { Name="AutoEnhanceFilter", Constructor = new object[]{true, true}},
            new FilterListObject { Name="No filter"},
        };

        private CameraPreviewImageSource _cameraPreviewImageSource; // Using camera as our image source
        private WriteableBitmap _writeableBitmap; // Target for our renderer
        private FilterEffect _effect; // Filter to be used on the camera image
        private WriteableBitmapRenderer _writeableBitmapRenderer; // renderer for our images
        private bool _isRendering; // used as a lock to prevent multiple renderers running at once
        private bool _changeFilterRequest = true; // to indicate new filter selection by user
        private int _index;
        private bool _loaded;

        private void CreateCamera()
        {
            _cameraPreviewImageSource = new CameraPreviewImageSource();

            _cameraPreviewImageSource.PreviewFrameAvailable += OnPreviewFrameAvailable;

            _effect = new FilterEffect(_cameraPreviewImageSource);

            _writeableBitmap = new WriteableBitmap(640, 480);
            _writeableBitmapRenderer = new WriteableBitmapRenderer(_effect, _writeableBitmap);
        }

        /// <summary>
        /// Initialize and start the camera preview 
        /// </summary>
        private async void Initialize()
        {
            StatusTextBlock.Text = "Starting camera...";

            CaptureImage.Source = _writeableBitmap;

            await _cameraPreviewImageSource.InitializeAsync(string.Empty);

            await _cameraPreviewImageSource.StartPreviewAsync();
            StatusTextBlock.Text = _filterList[_index].Name;
        }

        private void OnPreviewFrameAvailable(IImageSize args)
        {
            RenderPreview();
        }

        /// <summary>
        /// Render a frame with the selected filter
        /// </summary>
        private async void RenderPreview()
        {
            // Lock preventing multiple rendering attempts at once
            if (!_isRendering)
            {
                _isRendering = true;
                bool changed = false;
                // User changed the filter, let's update it before rendering
                if (_changeFilterRequest)
                {
                    SetFilter();
                    changed = true;
                }

                // Render the image with the filter
                await _writeableBitmapRenderer.RenderAsync();

                // Update the screen in the UI thread
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (_writeableBitmap == null)
                        return;

                    _writeableBitmap.Invalidate();

                    if (changed)
                    {
                        StatusTextBlock.Text = _filterList[_index].Name;
                    }
                });

                _isRendering = false;
            }
        }

        /// <summary>
        /// Use reflection to create a new filter class 
        /// </summary>
        private void SetFilter()
        {
            if (string.Compare(_filterList[_index].Name, "No filter", StringComparison.CurrentCultureIgnoreCase) != 0)
            {
                var type = string.Format(
                    "Nokia.Graphics.Imaging.{0}, Nokia.Graphics.Imaging, Version=255.255.255.255, Culture=neutral, PublicKeyToken=null, ContentType=WindowsRuntime",
                    _filterList[_index].Name);

                // Use reflection to create the filter class
                var sampleEffect = Type.GetType(type);
                if (sampleEffect != null)
                {
                    var filter = (IFilter)Activator.CreateInstance(sampleEffect, _filterList[_index].Constructor);
                    _effect.Filters = new[] { filter };
                }
            }
            else
            {
                _effect.Filters = new IFilter[0];
            }
            _changeFilterRequest = false;
        }
    }
}
