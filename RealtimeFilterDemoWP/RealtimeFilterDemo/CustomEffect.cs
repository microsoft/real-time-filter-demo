/*
 * Copyright © 2013 Nokia Corporation. All rights reserved.
 * Nokia and Nokia Connecting People are registered trademarks of Nokia Corporation. 
 * Other product and company names mentioned herein may be trademarks
 * or trade names of their respective owners. 
 * See LICENSE.TXT for license information.
 */

using Nokia.Graphics.Imaging;

namespace RealtimeFilterDemo
{
    public class CustomEffect : CustomEffectBase
    {
        public CustomEffect(IImageProvider source) : base(source)
        {
        }

        protected override void OnProcess(PixelRegion sourcePixelRegion, PixelRegion targetPixelRegion)
        {
            var sourcePixels = sourcePixelRegion.ImagePixels;
            var targetPixels = targetPixelRegion.ImagePixels;

            sourcePixelRegion.ForEachRow((index, width, position) =>
            {
                for (int x = 0; x < width; ++x, ++index)
                {
                    // the only supported color format is ColorFormat.Bgra8888

                    uint pixel = sourcePixels[index];
                    uint blue = pixel & 0x000000ff; // blue color component
                    uint green = (pixel & 0x0000ff00) >> 8; // green color component
                    uint red = (pixel & 0x00ff0000) >> 16; // red color component
                    uint average = (uint)(0.0722 * blue + 0.7152 * green + 0.2126 * red); // weighted average component
                    uint grayscale = 0xff000000 | average | (average << 8) | (average << 16); // use average for each color component

                    targetPixels[index] = ~grayscale; // use inverse grayscale
                }
            });
        }
    }
}
