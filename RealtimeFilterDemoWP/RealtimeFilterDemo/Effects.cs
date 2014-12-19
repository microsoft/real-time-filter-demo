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
using RealtimeFilterDemo.Resources;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Phone.Media.Capture;
using Windows.Storage.Streams;

namespace RealtimeFilterDemo
{
    public class Effects : ICameraEffect
    {
        private PhotoCaptureDevice _photoCaptureDevice = null;
        private CameraPreviewImageSource _cameraPreviewImageSource = null;
        private FilterEffect _filterEffect = null;
        private CustomEffectBase _customEffect = null;
        private int _effectIndex = 0;
        private int _effectCount = 11;
        private Semaphore _semaphore = new Semaphore(1, 1);

        public String EffectName { get; private set; }

        public PhotoCaptureDevice PhotoCaptureDevice
        {
            set
            {
                if (_photoCaptureDevice != value)
                {
                    while (!_semaphore.WaitOne(100));

                    _photoCaptureDevice = value;

                    Initialize();

                    _semaphore.Release();
                }
            }
        }

        ~Effects()
        {
            while (!_semaphore.WaitOne(100));

            Uninitialize();

            _semaphore.Release();
        }

        public async Task GetNewFrameAndApplyEffect(IBuffer frameBuffer, Size frameSize)
        {
            if (_semaphore.WaitOne(500))
            {
                _cameraPreviewImageSource.InvalidateLoad();

                var scanlineByteSize = (uint)frameSize.Width * 4; // 4 bytes per pixel in BGRA888 mode
                var bitmap = new Bitmap(frameSize, ColorMode.Bgra8888, scanlineByteSize, frameBuffer);

                if (_filterEffect != null)
                {
                    var renderer = new BitmapRenderer(_filterEffect, bitmap);
                    await renderer.RenderAsync();
                }
                else if (_customEffect != null)
                {
                    var renderer = new BitmapRenderer(_customEffect, bitmap);
                    await renderer.RenderAsync();
                }
                else
                {
                    var renderer = new BitmapRenderer(_cameraPreviewImageSource, bitmap);
                    await renderer.RenderAsync();
                }

                _semaphore.Release();
            }
        }

        public void NextEffect()
        {
            if (_semaphore.WaitOne(500))
            {
                Uninitialize();

                _effectIndex++;

                if (_effectIndex >= _effectCount)
                {
                    _effectIndex = 0;
                }

                Initialize();

                _semaphore.Release();
            }
        }

        public void PreviousEffect()
        {
            if (_semaphore.WaitOne(500))
            {
                Uninitialize();
                
                _effectIndex--;

                if (_effectIndex < 0)
                {
                    _effectIndex = _effectCount - 1;
                }

                Initialize();

                _semaphore.Release();
            }
        }

        private void Uninitialize()
        {
            if (_cameraPreviewImageSource != null)
            {
                _cameraPreviewImageSource.Dispose();
                _cameraPreviewImageSource = null;
            }

            if (_filterEffect != null)
            {
                _filterEffect.Dispose();
                _filterEffect = null;
            }

            if (_customEffect != null)
            {
                _customEffect.Dispose();
                _customEffect = null;
            }
        }

        private void Initialize()
        {
            var filters = new List<IFilter>();
            var nameFormat = "{0}/" + _effectCount + " - {1}";

            _cameraPreviewImageSource = new CameraPreviewImageSource(_photoCaptureDevice);

            switch (_effectIndex)
            {
                case 0:
                    {
                        EffectName = String.Format(nameFormat, 1, AppResources.Filter_Lomo);
                        filters.Add(new LomoFilter(0.5, 0.5, LomoVignetting.High, LomoStyle.Yellow));
                    }
                    break;

                case 1:
                    {
                        EffectName = String.Format(nameFormat, 2, AppResources.Filter_MagicPen);
                        filters.Add(new MagicPenFilter());
                    }
                    break;

                case 2:
                    {
                        EffectName = String.Format(nameFormat, 3, AppResources.Filter_Grayscale);
                        filters.Add(new GrayscaleFilter());
                    }
                    break;

                case 3:
                    {
                        EffectName = String.Format(nameFormat, 4, AppResources.Filter_Antique);
                        filters.Add(new AntiqueFilter());
                    }
                    break;

                case 4:
                    {
                        EffectName = String.Format(nameFormat, 5, AppResources.Filter_Stamp);
                        filters.Add(new StampFilter(4, 0.3));
                    }
                    break;

                case 5:
                    {
                        EffectName = String.Format(nameFormat, 6, AppResources.Filter_Cartoon);
                        filters.Add(new CartoonFilter(false));
                    }
                    break;

                case 6:
                    {
                        EffectName = String.Format(nameFormat, 7, AppResources.Filter_Sepia);
                        filters.Add(new SepiaFilter());
                    }
                    break;

                case 7:
                    {
                        EffectName = String.Format(nameFormat, 8, AppResources.Filter_Sharpness);
                        filters.Add(new SharpnessFilter(7));
                    }
                    break;

                case 8:
                    {
                        EffectName = String.Format(nameFormat, 9, AppResources.Filter_AutoEnhance);
                        filters.Add(new AutoEnhanceFilter());
                    }
                    break;

                case 9:
                    {
                        EffectName = String.Format(nameFormat, 10, AppResources.Filter_None);
                    }
                    break;

                case 10:
                    {
                        EffectName = String.Format(nameFormat, 11, AppResources.Filter_Custom);
                        _customEffect = new CustomEffect(_cameraPreviewImageSource);
                    }
                    break;
            }

            if (filters.Count > 0)
            {
                _filterEffect = new FilterEffect(_cameraPreviewImageSource)
                {
                    Filters = filters
                };
            }
        }
    }
}