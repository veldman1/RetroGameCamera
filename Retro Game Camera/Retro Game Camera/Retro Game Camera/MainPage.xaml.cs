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

namespace RetroGameCamera
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        private SKBitmap DrawBitmap;
        private SKBitmap UserPickedBitmap;
        private SKBitmap ApplyDitherBitmap;

        public MainPage()
        {
            InitializeComponent();

            var canvasView = SkiaView;
            canvasView.PaintSurface += OnCanvasViewPaintSurface;

            SKRect bounds = new SKRect();
            DrawBitmap = new SKBitmap((int)bounds.Right,
                                        (int)bounds.Height);

        }

        private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            float x = (info.Width - DrawBitmap.Width) / 2;
            float y = (info.Height - DrawBitmap.Height) / 2;

            canvas.DrawBitmap(DrawBitmap, x, y);
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                await DisplayAlert("Load Issue", "Could not open photo library.", "OK");
            }

            var pickPhoto = await CrossMedia.Current.PickPhotoAsync();

            SKBitmap loadedBitmap = null;
            using (MemoryStream memStream = new MemoryStream())
            {
                await pickPhoto.GetStreamWithImageRotatedForExternalStorage().CopyToAsync(memStream);
                memStream.Seek(0, SeekOrigin.Begin);

                UserPickedBitmap = SKBitmap.Decode(pickPhoto.GetStreamWithImageRotatedForExternalStorage());
            };

            await RezPickedBitmap();
            await CreateWithPickedBitmap();
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
                DrawBitmap.Encode(wstream, SKEncodedImageFormat.Jpeg, 90);
                byte[] data = memStream.ToArray();

                bool success = await DependencyService.Get<IPhotoLibrary>().SavePhotoAsync(data, path, "Retro16BitCamera" + DateTime.Now.Ticks + ".jpg");

                if (!success)
                {
                    await DisplayAlert("Save Issue", "Could not save to photo library.", "OK");
                } else
                {
                    await DisplayAlert("Saved", "Saved file to photo library.", "OK");
                }
            }

        }

        private async Task RezPickedBitmap()
        {
            if (UserPickedBitmap == null)
            {
                return;
            }
            var rez = 100 + RezSlider.Value * 200;
            ApplyDitherBitmap = new SKBitmap((int)rez, (int)(rez * (UserPickedBitmap.Height / UserPickedBitmap.Width)));
            UserPickedBitmap.ScalePixels(ApplyDitherBitmap, SKFilterQuality.None);
        }

        private async Task CreateWithPickedBitmap()
        {
            if (ApplyDitherBitmap == null)
            {
                return;
            }

            /*var colorList = new List<SKColor>()
            {
                new SKColor(255, 0, 0),
                new SKColor(255, 255, 0),
                new SKColor(255, 255, 255),
                new SKColor(255, 0, 255),
                new SKColor(0, 255, 255),
                new SKColor(0, 0, 255),
            };*/

            /*var colorList = new List<SKColor>()
            {
                new SKColor(155,188,15),
                new SKColor(139,172,15),
                new SKColor(48,98,48),
                new SKColor(15,56,15),
            };*/

            var colorList = new List<SKColor>()
            {
                new SKColor(0, 0, 0),
                new SKColor(255, 255, 255),
                new SKColor(170, 255, 238),
                new SKColor(204, 68, 204),
                new SKColor(0, 204, 85),
                new SKColor(0, 0, 170),
                new SKColor(238, 238, 119),
                new SKColor(221, 136, 85),
                new SKColor(102, 68, 0),
                new SKColor(255, 119, 119),
                new SKColor(51, 51, 51),
                new SKColor(119, 119, 119),
                new SKColor(170, 255, 102),
                new SKColor(0, 136, 255),
                new SKColor(187, 187, 187),
            };



            for (int x = 0; x < ApplyDitherBitmap.Width; x++)
            {
                for (int y = 0; y < ApplyDitherBitmap.Height; y++)
                {
                    var pixel = ApplyDitherBitmap.GetPixel(x, y);

                    var color1 = colorList.OrderBy(colorFromPalette => ColorDistance(pixel, colorFromPalette)).FirstOrDefault();
                    var colorDist1 = ColorDistance(pixel, color1);
                    var color2 = colorList.OrderBy(colorFromPalette => ColorDistance(pixel, colorFromPalette)).ElementAt(1);
                    var colorDist2 = ColorDistance(pixel, color2);

                    var ditherFactor = DitherSlider.Value * 50;

                    if (Math.Abs(colorDist1 - colorDist2) > ditherFactor || (x % 2 == y % 2))
                    {
                        ApplyDitherBitmap.SetPixel(x, y, color1);
                    }
                    else
                    {
                        ApplyDitherBitmap.SetPixel(x, y, color2);
                    }
                }
            }
        }

        private async Task DrawScaledUpImage()
        {
            if (ApplyDitherBitmap == null)
            {
                return;
            }

            int newCanvasWidth = (int)SkiaView.CanvasSize.Width;
            int newCanvasHeight = (int)(SkiaView.CanvasSize.Height * (SkiaView.CanvasSize.Width / SkiaView.CanvasSize.Height));
            if ((int)SkiaView.CanvasSize.Width > (int)SkiaView.CanvasSize.Height)
            {
                newCanvasWidth = (int)(SkiaView.CanvasSize.Width * (SkiaView.CanvasSize.Height / SkiaView.CanvasSize.Width));
                newCanvasHeight = (int)SkiaView.CanvasSize.Height;
            }

            DrawBitmap = new SKBitmap(newCanvasWidth, newCanvasHeight);
            ApplyDitherBitmap.ScalePixels(DrawBitmap, SKFilterQuality.None);

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
            DrawScaledUpImage();
        }
    }
}
