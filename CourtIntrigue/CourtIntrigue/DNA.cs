using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Xml;

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
        class PortraitRule
        {
            public ILogic Requirements { get; private set; }
            public Bitmap Bitmap { get; private set; }
            public int Index { get; private set; }

            public PortraitRule(Bitmap bitmap, ILogic requirements, int index)
            {
                Bitmap = bitmap;
                Requirements = requirements;
                Index = index;
            }
        }

        // Arbitrary colors to show which areas are skin, eyes and hair.
        private static Color SKIN_PIXEL = Color.FromArgb(255, 0, 255); // Using this not Blue since we might want Blue for eyes
        private static Color EYE_PIXEL = Color.FromArgb(255, 0, 0);
        private static Color HAIR_PIXEL = Color.FromArgb(0, 255, 0);
        private static Color SHIRT_PIXEL = Color.FromArgb(0, 255, 255);

        private List<PortraitRule> faceImages = new List<PortraitRule>();
        private List<PortraitRule> mouthImages = new List<PortraitRule>();
        private List<PortraitRule> noseImages = new List<PortraitRule>();
        private List<PortraitRule> eyeImages = new List<PortraitRule>();
        private List<PortraitRule> eyebrowImages = new List<PortraitRule>();
        private List<PortraitRule> earImages = new List<PortraitRule>();
        private List<PortraitRule> hairImages = new List<PortraitRule>();

        // Maybe we should use named colors here?
        private Color[] skinColors = { Color.FromArgb(255, 228, 214), Color.FromArgb(255, 224, 186), Color.FromArgb(255, 213, 163), Color.FromArgb(247, 198, 159), Color.FromArgb(219, 159, 103) };
        private Color[] eyeColors = { Color.FromArgb(0, 165, 255), Color.FromArgb(122, 205, 255), Color.FromArgb(0, 70, 182), Color.FromArgb(39, 153, 92), Color.FromArgb(0, 109, 46), Color.FromArgb(159, 183, 166), Color.FromArgb(153, 92, 39), Color.FromArgb(51, 31, 13), Color.Black };
        private Color[] hairColors = { Color.FromArgb(229, 195, 94), Color.FromArgb(217, 97, 45), Color.FromArgb(84, 53, 37), Color.Black };

        private static Color[] shirtColors = GetShirtColors();

        private Dictionary<DNA, Bitmap> cache = new Dictionary<DNA, Bitmap>();

        public void LoadFromDirectory(string path, Dictionary<string, int> badTags)
        {
            foreach(var file in Directory.EnumerateFiles(path, "*.xml"))
            {
                LoadPortraitRules(file, path, badTags);
            }
        }

        public DNA CreateRandomDNA(Character character, Game game)
        {
            DNA dna = new DNA(SelectRandomAllowed(faceImages, character, game),
                SelectRandomAllowed(mouthImages, character, game), SelectRandomAllowed(noseImages, character, game),
                SelectRandomAllowed(eyeImages, character, game), SelectRandomAllowed(eyebrowImages, character, game),
                SelectRandomAllowed(earImages, character, game), SelectRandomAllowed(hairImages, character, game),
                game.GetRandom(skinColors.Length), game.GetRandom(eyeColors.Length), game.GetRandom(hairColors.Length), game.GetRandom(shirtColors.Length));
            return dna;
        }

        public DNA CreateChildDNA(Character character, DNA father, DNA mother, Game game)
        {
            int face = SelectRandomAllowed(faceImages, character, father.Face, mother.Face, game);
            int mouth = SelectRandomAllowed(mouthImages, character, father.Mouth, mother.Mouth, game);
            int nose = SelectRandomAllowed(noseImages, character, father.Nose, mother.Nose, game);
            int eyes = SelectRandomAllowed(eyeImages, character, father.Eyes, mother.Eyes, game);
            int eyebrows = SelectRandomAllowed(eyebrowImages, character, father.Eyebrows, mother.Eyebrows, game);
            int ears = SelectRandomAllowed(earImages, character, father.Ears, mother.Ears, game);
            int hair = SelectRandomAllowed(hairImages, character, father.Hair, mother.Hair, game);
            int skinColor = SelectRandomAllowedColor(skinColors, character, father.SkinColor, mother.SkinColor, game);
            int eyeColor = SelectRandomAllowedColor(eyeColors, character, father.EyeColor, mother.EyeColor, game);
            int hairColor = SelectRandomAllowedColor(hairColors, character, father.HairColor, mother.HairColor, game);
            int shirtColor = CharacterVisualizationManager.GetRandomShirtColor(game);

            return new DNA(face, mouth, nose, eyes, eyebrows, ears, hair, skinColor, eyebrows, hairColor, shirtColor);
        }

        public Bitmap ComposeFace(DNA dna)
        {
            Bitmap output;
            if(!cache.TryGetValue(dna, out output))
            {
                output = new Bitmap(96, 96);
                using (Graphics G = Graphics.FromImage(output))
                {
                    G.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                    G.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    G.DrawImage(faceImages[dna.Face].Bitmap, 0, 0);
                    G.DrawImage(mouthImages[dna.Mouth].Bitmap, 0, 0);
                    G.DrawImage(noseImages[dna.Nose].Bitmap, 0, 0);
                    G.DrawImage(eyeImages[dna.Eyes].Bitmap, 0, 0);
                    G.DrawImage(eyebrowImages[dna.Eyebrows].Bitmap, 0, 0);
                    G.DrawImage(earImages[dna.Ears].Bitmap, 0, 0);
                    G.DrawImage(hairImages[dna.Hair].Bitmap, 0, 0);
                }
                output = ReplaceColors(output, dna);
                cache.Add(dna, output);
            }
            
            return output;
        }

        private int SelectRandomAllowed(List<PortraitRule> rules, Character character, Game game)
        {
            EventContext context = new EventContext(null, character, null);
            PortraitRule[] allowed = rules.Where(r => r.Requirements.Evaluate(context, game)).ToArray();
            return allowed[game.GetRandom(allowed.Length)].Index;
        }

        private int SelectRandomAllowed(List<PortraitRule> rules, Character character, int father, int mother, Game game)
        {
            EventContext context = new EventContext(null, character, null);

            List<PortraitRule> allowed = new List<PortraitRule>();
            if(rules[father].Requirements.Evaluate(context, game))
            {
                for (int i = 0; i < 45; ++i)
                {
                    allowed.Add(rules[father]);
                }
            }
            if (rules[mother].Requirements.Evaluate(context, game))
            {
                for (int i = 0; i < 45; ++i)
                {
                    allowed.Add(rules[mother]);
                }
            }
            allowed.AddRange(rules.Where(r => r.Requirements.Evaluate(context, game)));
            return allowed[game.GetRandom(allowed.Count)].Index;
        }

        private int SelectRandomAllowedColor(Color[] rules, Character character, int father, int mother, Game game)
        {
            return game.GetRandom(2) == 0 ? father : mother;
        }

        private void LoadPortraitRules(string file, string path, Dictionary<string, int> badTags)
        {
            using (XmlReader reader = XmlReader.Create(file))
            {
                while(reader.Read())
                {
                    if(reader.NodeType == XmlNodeType.Element && reader.Name == "portrait_rules")
                    {
                        ReadPortraitRules(reader, path, badTags);
                    }
                }
            }
        }

        private void ReadPortraitRules(XmlReader reader, string path, Dictionary<string, int> badTags)
        {
            while (reader.Read())
            {
                if(reader.NodeType == XmlNodeType.Element && reader.Name == "face")
                {
                    ReadPortraitRule(reader, faceImages, path, badTags);
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "mouth")
                {
                    ReadPortraitRule(reader, mouthImages, path, badTags);
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "nose")
                {
                    ReadPortraitRule(reader, noseImages, path, badTags);
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "eye")
                {
                    ReadPortraitRule(reader, eyeImages, path, badTags);
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "eyebrow")
                {
                    ReadPortraitRule(reader, eyebrowImages, path, badTags);
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "ear")
                {
                    ReadPortraitRule(reader, earImages, path, badTags);
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "hair")
                {
                    ReadPortraitRule(reader, hairImages, path, badTags);
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "portrait_rules")
                {
                    break;
                }
            }
        }

        private void ReadPortraitRule(XmlReader reader, List<PortraitRule> rules, string path, Dictionary<string, int> badTags)
        {
            string tag = reader.Name;
            Bitmap image = null;
            ILogic requirements = Logic.TRUE;
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "image")
                {
                    image = (Bitmap)Image.FromFile(path + "/" + reader.ReadElementContentAsString());
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "requirements")
                {
                    requirements = XmlHelper.ReadLogic(reader, badTags);
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == tag)
                {
                    break;
                }
            }
            rules.Add(new PortraitRule(image, requirements, rules.Count));
        }

        public static int GetRandomShirtColor(Game g)
        {
            return g.GetRandom(shirtColors.Length);
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
