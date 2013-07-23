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
        private PhotoCaptureDevice captureDevice;
        private WriteableBitmap cameraBitmap;
        private Windows.Foundation.Size outputBufferSize;
        private int effectIndex = 0;

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
                captureDevice = value;

                if (captureDevice != null)
                {
                    Windows.Foundation.Size previewSize = captureDevice.PreviewResolution;
                    cameraBitmap = new WriteableBitmap((int)previewSize.Width, (int)previewSize.Height);
                }
            }
        }

        public Windows.Foundation.Size OutputBufferSize
        {
            set
            {
                outputBufferSize = value;
            }
        }

        public async Task GetNewFrameAndApplyEffect(IBuffer processedBuffer)
        {
            if (captureDevice == null)
            {
                return;
            }

            captureDevice.GetPreviewBufferArgb(cameraBitmap.Pixels);

            Bitmap outputBtm = new Bitmap(
                outputBufferSize,
                ColorMode.Bgra8888,
                (uint)outputBufferSize.Width * 4, // 4 bytes per pixel in BGRA888 mode
                processedBuffer);

            using (EditingSession session = new EditingSession(cameraBitmap.AsBitmap()))
            {
                switch (effectIndex)
                {
                    case 0:
                        session.AddFilter(FilterFactory.CreateLomoFilter(0.5, 0.5, LomoVignetting.High, LomoStyle.Yellow));
                        break;
                    case 1:
                        session.AddFilter(FilterFactory.CreateMagicPenFilter());
                        break;
                    case 2:
                        session.AddFilter(FilterFactory.CreateGrayscaleFilter());
                        break;
                    case 3:
                        session.AddFilter(FilterFactory.CreateAntiqueFilter());
                        break;
                    case 4:
                        session.AddFilter(FilterFactory.CreateStampFilter(5, 100));
                        break;
                    case 5:
                        session.AddFilter(FilterFactory.CreateCartoonFilter(false));
                        break;
                }

                await session.RenderToBitmapAsync(outputBtm);
            }
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
