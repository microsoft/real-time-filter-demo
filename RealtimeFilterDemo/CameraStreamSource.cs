/*
 * Copyright © 2013 Nokia Corporation. All rights reserved.
 * Nokia and Nokia Connecting People are registered trademarks of Nokia Corporation. 
 * Other product and company names mentioned herein may be trademarks
 * or trade names of their respective owners. 
 * See LICENSE.TXT for license information.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;
using Windows.Foundation;

namespace RealtimeFilterDemo
{
    /// <summary>
    /// Filtered camera stream source for a media element. Feeds the media element with frames filtered with a ICameraEffect implementation.
    /// </summary>
    public class CameraStreamSource : MediaStreamSource
    {
        private readonly Dictionary<MediaSampleAttributeKeys, string> _emptyAttributes = new Dictionary<MediaSampleAttributeKeys, string>();
        private MediaStreamDescription _videoStreamDescription = null;
        private DispatcherTimer _frameRateTimer = null;
        private MemoryStream _frameStream = null;
        private ICameraEffect _cameraEffect = null;
        private long _currentTime = 0;
        private int _frameStreamOffset = 0;
        private int _frameTime = 0;
        private int _frameCount = 0;
        private Size _frameSize = new Size(0, 0);
        private int _frameBufferSize = 0;
        private byte[] _frameBuffer = null;

        /// <summary>
        /// Occurs when rendering frame rate changes.
        /// </summary>
        public event EventHandler<int> FrameRateChanged;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="_cameraEffect">Camera effect to use.</param>
        /// <param name="size">Size of the media element where the stream is rendered to.</param>
        public CameraStreamSource(ICameraEffect cameraEffect, Size size)
        {
            _cameraEffect = cameraEffect;

            _frameSize = size;
        }

        /// <summary>
        /// Initialises the data structures to pass data to the media pipeline via the MediaStreamSource.
        /// </summary>
        protected override void OpenMediaAsync()
        {
            // General properties

            _frameBufferSize = (int)_frameSize.Width * (int)_frameSize.Height * 4; // RGBA
            _frameBuffer = new byte[_frameBufferSize];
            _frameStream = new MemoryStream(_frameBuffer);

            // Media stream attributes

            var mediaStreamAttributes = new Dictionary<MediaStreamAttributeKeys, string>();

            mediaStreamAttributes[MediaStreamAttributeKeys.VideoFourCC] = "RGBA";
            mediaStreamAttributes[MediaStreamAttributeKeys.Width] = ((int)_frameSize.Width).ToString();
            mediaStreamAttributes[MediaStreamAttributeKeys.Height] = ((int)_frameSize.Height).ToString();

            _videoStreamDescription = new MediaStreamDescription(MediaStreamType.Video, mediaStreamAttributes);

            // Media stream descriptions

            var mediaStreamDescriptions = new List<MediaStreamDescription>();
            mediaStreamDescriptions.Add(_videoStreamDescription);

            // Media source attributes

            var mediaSourceAttributes = new Dictionary<MediaSourceAttributesKeys, string>();
            mediaSourceAttributes[MediaSourceAttributesKeys.Duration] = TimeSpan.FromSeconds(0).Ticks.ToString(CultureInfo.InvariantCulture);
            mediaSourceAttributes[MediaSourceAttributesKeys.CanSeek] = false.ToString();

            _frameTime = (int)TimeSpan.FromSeconds((double)0).Ticks;

            // Start frame rate timer

            _frameRateTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
            _frameRateTimer.Tick += FrameRateTimer_Tick;
            _frameRateTimer.Start();

            // Report that we finished initializing its internal state and can now pass in frame samples

            ReportOpenMediaCompleted(mediaSourceAttributes, mediaStreamDescriptions);
        }

        protected override void CloseMedia()
        {
            if (_frameStream != null)
            {
                _frameStream.Close();
                _frameStream = null;
            }

            if (_frameRateTimer != null)
            {
                _frameRateTimer.Stop();
                _frameRateTimer.Tick -= FrameRateTimer_Tick;
                _frameRateTimer = null;
            }

            _frameStreamOffset = 0;
            _frameTime = 0;
            _frameCount = 0;
            _frameBufferSize = 0;
            _frameBuffer = null;
            _videoStreamDescription = null;
            _currentTime = 0;
        }

        /// <summary>
        /// Processes camera frameBuffer using the set effect and provides media element with a filtered frameBuffer.
        /// </summary>
        protected override void GetSampleAsync(MediaStreamType mediaStreamType)
        {
            var task = _cameraEffect.GetNewFrameAndApplyEffect(_frameBuffer.AsBuffer(), _frameSize);
           
            // When asynchroneous call completes, proceed by reporting about the sample completion

            task.ContinueWith((action) =>
            {
                if (_frameStream != null)
                {
                    _frameStream.Position = 0;
                    _currentTime += _frameTime;
                    _frameCount++;

                    var sample = new MediaStreamSample(_videoStreamDescription, _frameStream, _frameStreamOffset, _frameBufferSize, _currentTime, _emptyAttributes);

                    ReportGetSampleCompleted(sample);
                }
            });
        }

        protected override void GetDiagnosticAsync(MediaStreamSourceDiagnosticKind diagnosticKind)
        {
            throw new NotImplementedException();
        }

        protected override void SeekAsync(long seekToTime)
        {
            _currentTime = seekToTime;

            ReportSeekCompleted(_currentTime);
        }

        protected override void SwitchMediaStreamAsync(MediaStreamDescription mediaStreamDescription)
        {
            throw new NotImplementedException();
        }

        private void FrameRateTimer_Tick(object sender, EventArgs e)
        {
            if (FrameRateChanged != null)
            {
                FrameRateChanged(this, _frameCount);
            }

            _frameCount = 0;
        }
    }
}