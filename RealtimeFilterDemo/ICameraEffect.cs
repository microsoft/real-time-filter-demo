/*
 * Copyright © 2013 Nokia Corporation. All rights reserved.
 * Nokia and Nokia Connecting People are registered trademarks of Nokia Corporation. 
 * Other product and company names mentioned herein may be trademarks
 * or trade names of their respective owners. 
 * See LICENSE.TXT for license information.
 */

using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Phone.Media.Capture;
using Windows.Storage.Streams;

namespace RealtimeFilterDemo
{
    public interface ICameraEffect
    {
        /// <summary>
        /// The camera device, the effect will poll the preview frames from it.
        /// </summary>
        PhotoCaptureDevice PhotoCaptureDevice { set; }

        /// <summary>
        /// The effect name.
        /// </summary>
        String EffectName { get; }

        /// <summary>
        /// Get a frame from the camera and apply an effect on it.
        /// </summary>
        /// <param name="frameBuffer">Frame to apply the effect on.</param>
        /// <param name="frameSize">Requested frame size.</param>
        /// <returns>A task that completes when effect has been applied.</returns>
        Task GetNewFrameAndApplyEffect(IBuffer frameBuffer, Size frameSize);
    }
}