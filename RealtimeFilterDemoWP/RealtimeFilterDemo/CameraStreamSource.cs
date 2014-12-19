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
    /// Filtered camera stream source for a media element. Feeds the media
    /// element with frames filtered with a ICameraEffect implementation.
    /// </summary>
    public class CameraStreamSource : MediaStreamSource
    {
        private readonly Dictionary<MediaSampleAttributeKeys, string> _emptyAttributes = new Dictionary<MediaSampleAttributeKeys, string>();
        private MediaStreamDescription _videoStreamDescription;
        private DispatcherTimer _frameRateTimer;
        private MemoryStream _frameStream;
        private ICameraEffect _cameraEffect;
        private long _currentTime;
        private int _frameStreamOffset;
        private int _frameTime;
        private int _frameCount;
        private Size _frameSize = new Size(0, 0);
        private int _frameBufferSize;
        private byte[] _frameBuffer;

        /// <summary>
        /// Occurs when rendering frame rate changes.
        /// </summary>
        public event EventHandler<int> FrameRateChanged;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="cameraEffect">Camera effect to use.</param>
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

            var mediaStreamDescriptions = new List<MediaStreamDescription> { _videoStreamDescription };

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
        /// Processes camera frame buffer using the set effect and provides
        /// media element with a filtered frame buffer.
        /// </summary>
        protected override void GetSampleAsync(MediaStreamType mediaStreamType)
        {
            var task = _cameraEffect.GetNewFrameAndApplyEffect(_frameBuffer.AsBuffer(), _frameSize);

            // When asynchronous call completes, proceed by reporting about the sample completion

            task.ContinueWith((action) =>
            {
                if (_frameStream != null)
                {
                    _frameStream.Position = 0;
                    _currentTime += _frameTime;
                    _frameCount++;

                    var sample = new MediaStreamSample(
                        _videoStreamDescription, _frameStream, _frameStreamOffset,
                        _frameBufferSize, _currentTime, _emptyAttributes);

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