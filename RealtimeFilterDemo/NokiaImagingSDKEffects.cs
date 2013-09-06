/**
 * Copyright (c) 2013 Nokia Corporation.
 */

namespace RealtimeFilterDemo
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Media.Imaging;
    using System.Runtime.InteropServices.WindowsRuntime;

    using Windows.Foundation;
    using Windows.Phone.Media.Capture;
    using Windows.Storage.Streams;

    using Nokia.Graphics;
    using Nokia.Graphics.Imaging;
    using Nokia.InteropServices.WindowsRuntime;

    /// <summary>
    /// A concrete implementation of the ICameraEffect.
    /// </summary>
    public class NokiaImagingSDKEffects : ICameraEffect
    {
        // Constants
        public static readonly int NumberOfEffects = 6;

        // Members
        private Windows.Foundation.Size outputBufferSize;
        private int effectIndex = 0;
        private Nokia.Graphics.Imaging.CameraPreviewImageSource _cameraSource;

        public String EffectName
        {
            get
            {
                switch (effectIndex)
                {
                    case 0: return "Lomo";
                    case 1: return "Magic pen";
                    case 2: return "Grayscale";
                    case 3: return "Antique";
                    case 4: return "Stamp";
                    case 5: return "Cartoon";
                }

                return "Undefined";
            }
        }

        public PhotoCaptureDevice CaptureDevice
        {
            set
            {
                _cameraSource = new CameraPreviewImageSource(value);
            }
        }

        public Windows.Foundation.Size OutputBufferSize { get; set; }

        public async Task GetNewFrameAndApplyEffect(IBuffer processedBuffer)
        {
            Bitmap outputBtm = new Bitmap(
                               OutputBufferSize,
                               ColorMode.Bgra8888,
                               (uint)outputBufferSize.Width * 4, // 4 bytes per pixel in BGRA888 mode
                               processedBuffer); 

            FilterEffect session = new FilterEffect(_cameraSource);
            IList<IFilter> filters = new List<IFilter>();
            
            switch (effectIndex)
            {
                case 0:
                    filters.Add(new LomoFilter(0.5, 0.5, LomoVignetting.High, LomoStyle.Yellow));
                    break;
                case 1:
                    filters.Add(new MagicPenFilter());
                    break;
                case 2:
                    filters.Add(new GrayscaleFilter());
                    break;
                case 3:
                    filters.Add(new AntiqueFilter());
                    break;
                case 4:
                    filters.Add(new StampFilter(5, 100));
                    break;
                case 5:
                    filters.Add(new CartoonFilter(false));
                    break;
            }
            session.Filters = filters;

            BitmapRenderer renderer = new BitmapRenderer(session, outputBtm);
            await renderer.RenderAsync();
        }

        /// <summary>
        /// The effect index.
        /// </summary>
        public int EffectIndex
        {
            get
            {
                return effectIndex;
            }
            set
            {
                if (effectIndex != value && value < NumberOfEffects)
                {
                    effectIndex = value;
                }
            }
        }
    }
}
