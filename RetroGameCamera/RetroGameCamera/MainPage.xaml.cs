using Plugin.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.Drawing;
using System.IO;
using SkiaSharp.Views.Forms;
using SkiaSharp;
using Color = Xamarin.Forms.Color;
using Plugin.Media.Abstractions;
using System.Collections.ObjectModel;

namespace RetroGameCamera
{
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        private SKBitmap _drawBitmap;
        private SKBitmap _userPickedBitmap;
        private SKBitmap _applyDitherBitmap;
        private ColorPalette _selectedColorPalette;

        public MainPage()
        {
            InitializeComponent();

            SkiaView.PaintSurface += OnCanvasViewPaintSurface;

            SKRect bounds = new SKRect();
            _drawBitmap = new SKBitmap((int)bounds.Right,
                                        (int)bounds.Height);

            PaletteSelection.PaintSurface += OnPaletteViewPaintSurface;

            _selectedColorPalette = ColorPaletteFactory.MakeAllPalettes()[0];
        }

        private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            float x = (info.Width - _drawBitmap.Width) / 2;
            float y = (info.Height - _drawBitmap.Height) / 2;

            canvas.DrawBitmap(_drawBitmap, x, y);
        }

        private void OnPaletteViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear(new SKColor(100, 100, 200));

            var cellWidth = info.Width / _selectedColorPalette.Colors.Count;
            for (int i = 0; i < _selectedColorPalette.Colors.Count; i++)
            {
                var paint = new SKPaint();
                paint.Color = _selectedColorPalette.Colors[i];
                paint.Style = SKPaintStyle.Fill;
                canvas.DrawRoundRect(new SKRoundRect(new SKRect(i * cellWidth, 0, (i * cellWidth) + cellWidth, info.Height), 5, 5), paint);
            }
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                await DisplayAlert("Load Issue", "Could not open photo library.", "OK");
            }

            var pickPhoto = await CrossMedia.Current.PickPhotoAsync();

            if (pickPhoto == null)
            {
                return;
            }

            using (MemoryStream memStream = new MemoryStream())
            {
                await pickPhoto.GetStreamWithImageRotatedForExternalStorage().CopyToAsync(memStream);
                memStream.Seek(0, SeekOrigin.Begin);

                _userPickedBitmap = SKBitmap.Decode(pickPhoto.GetStreamWithImageRotatedForExternalStorage());
            };

            await RezPickedBitmap();
            await CreateWithPickedBitmap();
            await DrawScaledUpImage();
        }

        private async void Save_Clicked(object sender, EventArgs e)
        {
            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                await DisplayAlert("Save Issue", "Could not save to photo library.", "OK");
            }

            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            IPhotoLibrary photoLibrary = DependencyService.Get<IPhotoLibrary>();

            using (MemoryStream memStream = new MemoryStream())
            using (SKManagedWStream wstream = new SKManagedWStream(memStream))
            {
                _drawBitmap.Encode(wstream, SKEncodedImageFormat.Jpeg, 90);
                byte[] data = memStream.ToArray();

                bool success = await DependencyService.Get<IPhotoLibrary>().SavePhotoAsync(data, path, "Retro16BitCamera" + DateTime.Now.Ticks + ".jpg");

                if (!success)
                {
                    await DisplayAlert("Save Issue", "Could not save to photo library.", "OK");
                } else
                {
                    await DisplayAlert("Saved", "Saved file to " + path, "OK");
                }
            }

        }

        private async Task RezPickedBitmap()
        {
            if (_userPickedBitmap == null)
            {
                return;
            }
            var rez = 100 + RezSlider.Value * 200;
            _applyDitherBitmap = new SKBitmap((int)rez, (int)(rez * (_userPickedBitmap.Height / _userPickedBitmap.Width)));
            _userPickedBitmap.ScalePixels(_applyDitherBitmap, SKFilterQuality.None);
        }

        private async Task CreateWithPickedBitmap()
        {
            if (_applyDitherBitmap == null)
            {
                return;
            }

            for (int x = 0; x < _applyDitherBitmap.Width; x++)
            {
                for (int y = 0; y < _applyDitherBitmap.Height; y++)
                {
                    var pixel = _applyDitherBitmap.GetPixel(x, y);

                    var color1 = _selectedColorPalette.Colors.OrderBy(colorFromPalette => ColorDistance(pixel, colorFromPalette)).FirstOrDefault();
                    var colorDist1 = ColorDistance(pixel, color1);
                    var color2 = _selectedColorPalette.Colors.OrderBy(colorFromPalette => ColorDistance(pixel, colorFromPalette)).ElementAt(1);
                    var colorDist2 = ColorDistance(pixel, color2);

                    var ditherFactor = DitherSlider.Value * 50;

                    if (Math.Abs(colorDist1 - colorDist2) > ditherFactor || (x % 2 == y % 2))
                    {
                        _applyDitherBitmap.SetPixel(x, y, color1);
                    }
                    else
                    {
                        _applyDitherBitmap.SetPixel(x, y, color2);
                    }
                }
            }
        }

        private async Task DrawScaledUpImage()
        {
            if (_applyDitherBitmap == null)
            {
                return;
            }

            if (SkiaView.CanvasSize.Width > 0) {            
                int newCanvasWidth = (int)SkiaView.CanvasSize.Width;
                int newCanvasHeight = (int)(SkiaView.CanvasSize.Height * (SkiaView.CanvasSize.Width / SkiaView.CanvasSize.Height));
                if ((int)SkiaView.CanvasSize.Width > (int)SkiaView.CanvasSize.Height)
                {
                    newCanvasWidth = (int)(SkiaView.CanvasSize.Width * (SkiaView.CanvasSize.Height / SkiaView.CanvasSize.Width));
                    newCanvasHeight = (int)SkiaView.CanvasSize.Height;
                }

                _drawBitmap = new SKBitmap(newCanvasWidth, newCanvasHeight);
            }

            _applyDitherBitmap.ScalePixels(_drawBitmap, SKFilterQuality.None);

            SkiaView.InvalidateSurface();
        }

        private double ColorDistance(SKColor col1, SKColor col2)
        {
            var redDiff = Math.Abs(col1.Red - col2.Red);
            var greenDiff = Math.Abs(col1.Green - col2.Green);
            var blueDiff = Math.Abs(col1.Blue - col2.Blue);

            return Math.Sqrt(Math.Pow(redDiff, 2.0) + Math.Pow(greenDiff, 2.0) + Math.Pow(blueDiff, 2.0));
        }

        private async void DitherSlider_DragCompleted(object sender, EventArgs e)
        {
            await RezPickedBitmap();
            await CreateWithPickedBitmap();
            await DrawScaledUpImage();
        }

        private async void RezSlider_DragCompleted(object sender, EventArgs e)
        {
            await RezPickedBitmap();
            await CreateWithPickedBitmap();
            await DrawScaledUpImage();
        }

        private async void ContentPage_SizeChanged(object sender, EventArgs e)
        {
            await DrawScaledUpImage();
        }

        private async void PaletteSelection_Touch(object sender, SKTouchEventArgs e)
        {
            if (e.MouseButton == SKMouseButton.Left && e.ActionType == SKTouchAction.Pressed)
            {
                var paletteSelectPage = new ColorPaletteSelectPage(PaletteSelectCallback);
                await Navigation.PushModalAsync(paletteSelectPage, true);
            }
        }

        private async void PaletteSelectCallback(ColorPaletteSelectPage page)
        {
            _selectedColorPalette = page.Selected;
            await RezPickedBitmap();
            await CreateWithPickedBitmap();
            await DrawScaledUpImage();
            PaletteSelection.InvalidateSurface();

        }
    }
}
