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

using Lumia.Imaging;

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
