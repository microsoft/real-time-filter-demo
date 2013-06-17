/**
 * Copyright (c) 2013 Nokia Corporation.
 */

namespace RealtimeFilterDemo
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices.WindowsRuntime;
    using System.Text;
    using System.Threading.Tasks;
    using Windows.Phone.Media.Capture;
    using Windows.Storage.Streams;

    public interface ICameraEffect
    {
        /// <summary>
        /// The effect name.
        /// </summary>
        String EffectName { get; }

        /// <summary>
        /// The camera device, the effect will poll the preview frames from it.
        /// </summary>
        PhotoCaptureDevice CaptureDevice { set; }

        /// <summary>
        /// The dimensions of the output buffer.
        /// </summary>
        Windows.Foundation.Size OutputBufferSize { set; }

        /// <summary>
        /// Get a frame from the camera and apply an effect on it.
        /// </summary>
        /// <param name="processedBuffer">A buffer with the camera data with the effect applied.</param>
        /// <returns>A task that completes when effect has been applied.</returns>
        Task GetNewFrameAndApplyEffect(IBuffer processedBuffer);
    }
}