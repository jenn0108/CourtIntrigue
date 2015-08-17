using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

namespace CourtIntrigue
{
    class DNA
    {
        public int Face { get; private set; }
        public int Mouth { get; private set; }
        public int Nose { get; private set; }
        public int Eyes { get; private set; }
        public int Eyebrows { get; private set; }
        public int Hair { get; private set; }

        public DNA(int face, int mouth, int nose, int eyes, int eyebrows, int hair)
        {
            Face = face;
            Mouth = mouth;
            Nose = nose;
            Eyes = eyes;
            Eyebrows = eyebrows;
            Hair = hair;
        }
    }

    class CharacterVisualizationManager
    {
        private Bitmap[] faceImages;
        private Bitmap[] mouthImages;
        private Bitmap[] noseImages;
        private Bitmap[] eyeImages;
        private Bitmap[] eyebrowImages;
        private Bitmap[] hairImages;

        public void LoadFromDirectory(string path)
        {
            faceImages = LoadParts(path, "base_*.png");
            mouthImages = LoadParts(path, "mouth_*.png");
            noseImages = LoadParts(path, "nose_*.png");
            eyeImages = LoadParts(path, "eyes_*.png");
            eyebrowImages = LoadParts(path, "eyebrows_*.png");
            hairImages = LoadParts(path, "hair_*.png");
        }

        public DNA CreateRandomDNA(Game game)
        {
            return new DNA(game.GetRandom(faceImages.Length), game.GetRandom(mouthImages.Length), game.GetRandom(noseImages.Length),
                game.GetRandom(eyeImages.Length), game.GetRandom(eyebrowImages.Length), game.GetRandom(hairImages.Length));
        }

        public Bitmap ComposeFace(DNA dna)
        {
            Bitmap output = new Bitmap(96, 96);
            using (Graphics G = Graphics.FromImage(output))
            {
                G.DrawImage(faceImages[dna.Face], 0, 0);
                G.DrawImage(mouthImages[dna.Mouth], 0, 0);
                G.DrawImage(noseImages[dna.Nose], 0, 0);
                G.DrawImage(eyeImages[dna.Eyes], 0, 0);
                G.DrawImage(eyebrowImages[dna.Eyebrows], 0, 0);
                G.DrawImage(hairImages[dna.Hair], 0, 0);
            }
            return output;
        }

        private Bitmap[] LoadParts(string path, string pattern)
        {
            return Directory.EnumerateFiles(path, pattern).OrderBy(file => file).Select(file => (Bitmap)Bitmap.FromFile(file)).ToArray();
        }
    }
}
