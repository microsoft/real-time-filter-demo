/**
 * Copyright (c) 2013 Nokia Corporation.
 */

namespace RealtimeFilterDemo
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;

    /// <summary>
    /// A class to work around a problem with MediaElement.
    /// MediaElement doesn't destruct the current MediaStreamSource when the
    /// source is changed. To avoid memory leaks when source is changed (for
    /// example when navigating away from a page) reserve the memory once and
    /// use it for all the MediaStreamSource instances we create.
    /// </summary>
    public sealed class CameraStreamSourceDataSingleton
    {
        private const int FramePixelSize = 4; // RGBA
        private const int NumberOfBufferedFrames = 1;

        private static CameraStreamSourceDataSingleton classInstance = null;
        public static CameraStreamSourceDataSingleton Instance
        {
            get
            {
                if (classInstance == null)
                {
                    classInstance = new CameraStreamSourceDataSingleton();
                }

                return classInstance;
            }
        }

        public MemoryStream FrameStream { get; private set; }
        public byte[] ImageBuffer { get; private set; }
        public ICameraEffect CameraEffect { get; set; }
        public int FrameHeight { get; private set; }
        public int FrameWidth { get; private set; }
        public int FrameBufferSize { get; private set; }
        public int FrameStreamSize { get; private set; }

        /// <summary>
        /// Create the buffers of the data source.
        /// </summary>
        /// <param name="cameraFrameSize">The dimensions of a camera frame.</param>
        public void Initialize(Windows.Foundation.Size cameraFrameSize)
        {
            if (this.FrameWidth != 0)
            {
                if (((int)cameraFrameSize.Width != this.FrameWidth)
                    || ((int)cameraFrameSize.Height != this.FrameHeight))
                {
                    // Currently, we don't allow changing the frame size
                    // after first initialization
                    throw new NotSupportedException();
                }

                return; // singleton is already initialized.
            }

            this.FrameWidth = (int)cameraFrameSize.Width;
            this.FrameHeight = (int)cameraFrameSize.Height;
            this.FrameBufferSize = FrameWidth * FrameHeight * FramePixelSize;
            this.ImageBuffer = new byte[FrameBufferSize];
            this.FrameStreamSize = FrameBufferSize * NumberOfBufferedFrames;
            this.FrameStream = new MemoryStream(ImageBuffer);
        }
    }
}
