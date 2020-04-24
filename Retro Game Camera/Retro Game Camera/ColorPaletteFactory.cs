using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace RetroGameCamera
{
    class ColorPaletteFactory
    {
        public static List<ColorPalette> MakeAllPalettes()
        {
            return new List<ColorPalette> {
                new ColorPalette
                {
                    Title = "80s PC",
                    Colors = new List<SKColor>()
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
                    }
                },
                new ColorPalette
                {
                    Title = "Game Dude",
                    Colors = new List<SKColor>()
                    {
                        new SKColor(155,188,15),
                        new SKColor(139,172,15),
                        new SKColor(48,98,48),
                        new SKColor(15,56,15),
                    }
                },
                new ColorPalette
                {
                    Title = "Primaries",
                    Colors = new List<SKColor>()
                    {
                        new SKColor(255, 0, 0),
                        new SKColor(255, 255, 0),
                        new SKColor(255, 255, 255),
                        new SKColor(255, 0, 255),
                        new SKColor(0, 255, 255),
                        new SKColor(0, 0, 255),    
                    }
                },
                new ColorPalette
                {
                    Title = "CGA Cold",
                    Colors = new List<SKColor>()
                    {
                        SKColor.Parse("#000000"),
                        SKColor.Parse("#ff55ff"),
                        SKColor.Parse("#55ffff"),
                        SKColor.Parse("#ffffff"),
                    }
                },
                new ColorPalette
                {
                    Title = "CGA Warm",
                    Colors = new List<SKColor>()
                    {
                        SKColor.Parse("#000000"),
                        SKColor.Parse("#55ff55"),
                        SKColor.Parse("#ff5555"),
                        SKColor.Parse("#ffff55"),
                    }
                },
            };
        }
    }
}
