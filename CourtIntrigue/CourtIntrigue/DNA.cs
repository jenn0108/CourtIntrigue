using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace CourtIntrigue
{
    class DNA
    {
        public int Face { get; private set; }
        public int Mouth { get; private set; }
        public int Nose { get; private set; }
        public int Eyes { get; private set; }
        public int Eyebrows { get; private set; }
        public int Ears { get; private set; }
        public int Hair { get; private set; }
        public int SkinColor { get; private set; }
        public int EyeColor { get; private set; }
        public int HairColor { get; private set; }
        public int ShirtColor { get; private set; }


        public DNA(int face, int mouth, int nose, int eyes, int eyebrows, int ears, int hair, int skinColor, int eyeColor, int hairColor, int shirtColor)
        {
            Face = face;
            Mouth = mouth;
            Nose = nose;
            Eyes = eyes;
            Eyebrows = eyebrows;
            Ears = ears;
            Hair = hair;
            SkinColor = skinColor;
            EyeColor = eyeColor;
            HairColor = hairColor;
            ShirtColor = shirtColor;
        }
    }

    class CharacterVisualizationManager
    {
        // Arbitrary colors to show which areas are skin, eyes and hair.
        private static Color SKIN_PIXEL = Color.FromArgb(255, 0, 255); // Using this not Blue since we might want Blue for eyes
        private static Color EYE_PIXEL = Color.FromArgb(255, 0, 0);
        private static Color HAIR_PIXEL = Color.FromArgb(0, 255, 0);
        private static Color SHIRT_PIXEL = Color.FromArgb(0, 255, 255);

        private Bitmap[] faceImages;
        private Bitmap[] mouthImages;
        private Bitmap[] noseImages;
        private Bitmap[] eyeImages;
        private Bitmap[] eyebrowImages;
        private Bitmap[] earImages;
        private Bitmap[] hairImages;

        // Maybe we should use named colors here?
        private Color[] skinColors = { Color.FromArgb(255, 228, 214), Color.FromArgb(255, 224, 186), Color.FromArgb(255, 213, 163), Color.FromArgb(247, 198, 159), Color.FromArgb(219, 159, 103) };
        private Color[] eyeColors = { Color.FromArgb(0, 165, 255), Color.FromArgb(122, 205, 255), Color.FromArgb(0, 20, 213), Color.FromArgb(39, 153, 92), Color.FromArgb(0, 109, 46), Color.FromArgb(159, 183, 166), Color.FromArgb(153, 92, 39), Color.FromArgb(51, 31, 13), Color.Black };
        private Color[] hairColors = { Color.FromArgb(229, 195, 94), Color.FromArgb(217, 97, 45), Color.FromArgb(84, 53, 37), Color.Black };

        private Color[] shirtColors = GetShirtColors();

        private Dictionary<DNA, Bitmap> cache = new Dictionary<DNA, Bitmap>();

        public void LoadFromDirectory(string path)
        {
            faceImages = LoadParts(path, "base_*.png");
            mouthImages = LoadParts(path, "mouth_*.png");
            noseImages = LoadParts(path, "nose_*.png");
            eyeImages = LoadParts(path, "eyes_*.png");
            eyebrowImages = LoadParts(path, "eyebrows_*.png");
            earImages = LoadParts(path, "ears_*.png");
            hairImages = LoadParts(path, "hair_*.png");
        }

        public DNA CreateRandomDNA(Game game)
        {
            DNA dna = new DNA(game.GetRandom(faceImages.Length), game.GetRandom(mouthImages.Length), game.GetRandom(noseImages.Length),
                game.GetRandom(eyeImages.Length), game.GetRandom(eyebrowImages.Length), game.GetRandom(earImages.Length), game.GetRandom(hairImages.Length),
                game.GetRandom(skinColors.Length), game.GetRandom(eyeColors.Length), game.GetRandom(hairColors.Length), game.GetRandom(shirtColors.Length));
            return dna;
        }

        public Bitmap ComposeFace(DNA dna)
        {
            Bitmap output;
            if(!cache.TryGetValue(dna, out output))
            {
                output = new Bitmap(96, 96);
                using (Graphics G = Graphics.FromImage(output))
                {
                    G.DrawImage(ReplaceColors(faceImages[dna.Face], dna), 0, 0);
                    G.DrawImage(ReplaceColors(mouthImages[dna.Mouth], dna), 0, 0);
                    G.DrawImage(ReplaceColors(noseImages[dna.Nose], dna), 0, 0);
                    G.DrawImage(ReplaceColors(eyeImages[dna.Eyes], dna), 0, 0);
                    G.DrawImage(ReplaceColors(eyebrowImages[dna.Eyebrows], dna), 0, 0);
                    G.DrawImage(ReplaceColors(earImages[dna.Ears], dna), 0, 0);
                    G.DrawImage(ReplaceColors(hairImages[dna.Hair], dna), 0, 0);
                }
                cache.Add(dna, output);
            }
            
            return output;
        }

        private Bitmap[] LoadParts(string path, string pattern)
        {
            return Directory.EnumerateFiles(path, pattern).OrderBy(file => file).Select(file => (Bitmap)Bitmap.FromFile(file)).ToArray();
        }

        // Good enough for now.
        private static Color[]  GetShirtColors()
        {
            List<Color> colors = new List<Color>();
            // Magic thing from here http://stackoverflow.com/questions/3821174/c-sharp-getting-all-colors-from-color
            // that gets only the Color properties.
            foreach (PropertyInfo property in typeof(Color).GetProperties(BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.Public)) {
                Color color = (Color)property.GetValue(null);
                colors.Add(color);
            }
            return colors.ToArray();
        }

        // Returns a new bitmap created by copying originalImage and replacing all
        // HAIR_PIXEL, SKIN_PIXEL and EYE_PIXEL colors found with the appropriate
        // color from the DNA.
        private Bitmap ReplaceColors(Bitmap originalImage, DNA dna)
        {
            Bitmap result = new Bitmap(originalImage);
            for (int x = 0; x < result.Width; x++)
            {
                for (int y = 0; y < result.Height; y++)
                {
                    // Change the color to new color if it matches,
                    // otherwise just keep going.
                    // I read somewhere that GetPixel was slow so maybe
                    // we want to find a faster way to do this?
                    Color originalPixel = originalImage.GetPixel(x, y);

                    if (originalPixel == SKIN_PIXEL)
                    {
                        result.SetPixel(x, y, skinColors[dna.SkinColor]);
                    }
                    else if (originalPixel == HAIR_PIXEL)
                    {
                        result.SetPixel(x, y, hairColors[dna.HairColor]);
                    }
                    else if (originalPixel == EYE_PIXEL)
                    {
                        result.SetPixel(x, y, eyeColors[dna.EyeColor]);
                    }
                    else if (originalPixel == SHIRT_PIXEL)
                    {
                        result.SetPixel(x, y, shirtColors[dna.ShirtColor]);
                    }
                }
            }
            return result;
        }
    }
}
