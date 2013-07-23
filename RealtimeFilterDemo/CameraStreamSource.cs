/**
 * Copyright (c) 2013 Nokia Corporation.
 */

namespace RealtimeFilterDemo
{
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

    /// <summary>
    /// A source for the media element. Feeds the Media Element with frames
    /// coming from the ICameraEffect implementation.
    /// </summary>
    public class CameraStreamSource : MediaStreamSource
    {
        private readonly Dictionary<MediaSampleAttributeKeys, string> emptySampleDict =
            new Dictionary<MediaSampleAttributeKeys, string>();
        
        private MediaStreamDescription videoStreamDescription;
        private long currentTime;
        private int frameStreamOffset;
        private int frameTime;
        private int frameCount;

        /// <summary>
        /// Occurs when the FPS count changes.
        /// </summary>
        public event EventHandler<int> FPSChanged;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="cameraEffect">The camera effect to use.</param>
        /// <param name="targetMediaElementSize">The size of the element where
        /// the stream is rendered to.</param>
        public CameraStreamSource(ICameraEffect cameraEffect, Size targetMediaElementSize)
        {
            CameraStreamSourceDataSingleton dataSource = CameraStreamSourceDataSingleton.Instance;
            dataSource.Initialize(targetMediaElementSize);
            dataSource.CameraEffect = cameraEffect;
            cameraEffect.OutputBufferSize = targetMediaElementSize;
        }

        /// <summary>
        /// Initialises the data structures to pass data to the media pipeline
        /// via the MediaStreamSource.
        /// </summary>
        protected override void OpenMediaAsync()
        {
            Dictionary<MediaSourceAttributesKeys, string> mediaSourceAttributes =
                new Dictionary<MediaSourceAttributesKeys, string>();
            Dictionary<MediaStreamAttributeKeys, string> mediaStreamAttributes =
                new Dictionary<MediaStreamAttributeKeys, string>();
            List<MediaStreamDescription> mediaStreamDescriptions =
                new List<MediaStreamDescription>();

            CameraStreamSourceDataSingleton dataSource = CameraStreamSourceDataSingleton.Instance;

            mediaStreamAttributes[MediaStreamAttributeKeys.VideoFourCC] = "RGBA";
            mediaStreamAttributes[MediaStreamAttributeKeys.Width] = dataSource.FrameWidth.ToString();
            mediaStreamAttributes[MediaStreamAttributeKeys.Height] = dataSource.FrameHeight.ToString();

            videoStreamDescription =
                new MediaStreamDescription(MediaStreamType.Video, mediaStreamAttributes);
            mediaStreamDescriptions.Add(videoStreamDescription);

            // A zero timespan is an infinite video
            mediaSourceAttributes[MediaSourceAttributesKeys.Duration] =
                TimeSpan.FromSeconds(0).Ticks.ToString(CultureInfo.InvariantCulture);

            mediaSourceAttributes[MediaSourceAttributesKeys.CanSeek] = false.ToString();

            frameTime = (int)TimeSpan.FromSeconds((double)0).Ticks;

            // Report that we finished initializing its internal state and can now
            // pass in frame samples.
            ReportOpenMediaCompleted(mediaSourceAttributes, mediaStreamDescriptions);

            DispatcherTimer fpsTimer = new DispatcherTimer();
            fpsTimer.Interval = TimeSpan.FromSeconds(1);
            fpsTimer.Tick += Fps_Tick; 
            fpsTimer.Start();
        }

        /// <summary>
        /// Processes the camera buffer using the set effect and provides the
        /// media element with the buffer.
        /// </summary>
        /// <param name="mediaStreamType">Not used.</param>
        protected override void GetSampleAsync(MediaStreamType mediaStreamType)
        {
            CameraStreamSourceDataSingleton dataSource = CameraStreamSourceDataSingleton.Instance;

            if (frameStreamOffset + dataSource.FrameBufferSize > dataSource.FrameStreamSize)
            {
                dataSource.FrameStream.Seek(0, SeekOrigin.Begin);
                frameStreamOffset = 0;
            }

            Task tsk = dataSource.CameraEffect.GetNewFrameAndApplyEffect(dataSource.ImageBuffer.AsBuffer());
           
            // Wait that the asynchroneous call completes, and proceed by reporting 
            // the MediaElement that new samples are ready.
            tsk.ContinueWith((task) =>
            {
                dataSource.FrameStream.Position = 0;

                MediaStreamSample msSample = new MediaStreamSample(
                    videoStreamDescription, 
                    dataSource.FrameStream, 
                    frameStreamOffset,
                    dataSource.FrameBufferSize,
                    currentTime,
                    emptySampleDict);

                ReportGetSampleCompleted(msSample);
                frameCount++;
                currentTime += frameTime;
                frameStreamOffset += dataSource.FrameBufferSize;
            });
        }

        protected override void CloseMedia()
        {
            // No implementation required
        }

        protected override void GetDiagnosticAsync(MediaStreamSourceDiagnosticKind diagnosticKind)
        {
            throw new NotImplementedException();
        }

        protected override void SeekAsync(long seekToTime)
        {
            currentTime = seekToTime;
            ReportSeekCompleted(seekToTime);
        }

        protected override void SwitchMediaStreamAsync(MediaStreamDescription mediaStreamDescription)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Notifies the handler(s) about the current frame rate.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Fps_Tick(object sender, EventArgs e)
        {
            EventHandler<int> handler = FPSChanged;

            if (handler != null)
            {
                handler(this, frameCount);
            }

            frameCount = 0;
        }
    }
}