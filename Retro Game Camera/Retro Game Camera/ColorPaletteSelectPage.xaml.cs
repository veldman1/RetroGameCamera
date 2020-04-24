using SkiaSharp;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace RetroGameCamera
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ColorPaletteSelectPage : ContentPage
    {
        private ObservableCollection<ColorPalette> _items;
        private Action<ColorPaletteSelectPage> _paletteSelectCallback;

        public ColorPalette Selected { get; private set; }

        public ColorPaletteSelectPage(Action<ColorPaletteSelectPage> paletteSelectCallback)
        {
            InitializeComponent();

            _paletteSelectCallback = paletteSelectCallback;

            _items = new ObservableCollection<ColorPalette>(ColorPaletteFactory.MakeAllPalettes());

            foreach (var item in _items)
            {
                SKImageInfo info = new SKImageInfo(100, 20);
                SKBitmap bitmap = new SKBitmap(info);
                SKCanvas canvas = new SKCanvas(bitmap);

                var cellWidth = info.Width / item.Colors.Count;
                for (int i = 0; i < item.Colors.Count; i++)
                {
                    var paint = new SKPaint();
                    paint.Color = item.Colors[i];
                    paint.Style = SKPaintStyle.Fill;
                    canvas.DrawRect(new SKRect(i * cellWidth, 0, (i * cellWidth) + cellWidth, info.Height), paint);
                }

                var img = SKImage.FromBitmap(bitmap);
                var encoded = img.Encode();
                item.Image = ImageSource.FromStream(encoded.AsStream);
            }

            PaletteCollection.SetBinding(CollectionView.ItemsSourceProperty, new Binding { Source = _items });
        }

        private async void MyListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            await Navigation.PopModalAsync();
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            var palette = (sender as Button).Parent.BindingContext as ColorPalette;
            Selected = palette;
            _paletteSelectCallback(this);
            await Navigation.PopModalAsync();
        }
    }
}
