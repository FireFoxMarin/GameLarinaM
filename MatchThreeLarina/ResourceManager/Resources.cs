using MatchThreeLarina.GameLogic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MatchThreeLarina.ResourceManager
{
    internal static class Resources
    {
        public static SpriteFont Font { get; private set; }
        public static Texture2D Cell { get; private set; }
        public static Texture2D GameOverScreen { get; private set; }
        public static Texture2D MenuButton { get; private set; }
        public static Texture2D PlayButton { get; private set; }
        public static Texture2D Destroyer { get; private set; }
        public static Texture2D FieldBackground { get; private set; }

        public static SoundEffect BackgroundSound { get; private set; }
        public static SoundEffect BombSound { get; private set; }
        public static SoundEffect MatchSound { get; private set; }
        public static SoundEffect ClickSound { get; private set; }
        public static SoundEffect LineSound { get; private set; }
        public static SoundEffect TickSound { get; private set; }


        public static Texture2D GetTexture(ShapeType shape, Bonus bonus)
        {
            return elementsDict[shape][(int)bonus];
        }

        public static void Init(ContentManager Content)
        {
            content = Content;

            Font = content.Load<SpriteFont>("Fonts/Font");

            Cell = content.Load<Texture2D>("Sprites/CellBackground");
            GameOverScreen = content.Load<Texture2D>("Sprites/Gameover");
            MenuButton = content.Load<Texture2D>("Sprites/MenuButton");
            PlayButton = content.Load<Texture2D>("Sprites/PlayButton");
            Destroyer = content.Load<Texture2D>("Sprites/Destroyer");
            FieldBackground = content.Load<Texture2D>("Sprites/FieldBackground");

            BombSound = content.Load<SoundEffect>("Sound/bomb");
            MatchSound = content.Load<SoundEffect>("Sound/match");
            ClickSound = content.Load<SoundEffect>("Sound/click");
            LineSound = content.Load<SoundEffect>("Sound/bonusline");
            TickSound = content.Load<SoundEffect>("Sound/tick");

            elementsDict.Add(ShapeType.Empty,
                Enumerable.Repeat(content.Load<Texture2D>("Sprites/BlankCell"), 4).ToArray());

            var memberInfos = typeof(ShapeType).GetMembers(BindingFlags.Public | BindingFlags.Static);
            for (var i = 1; i < memberInfos.Length; i++)
            {
                var type = (ShapeType)Enum.Parse(typeof(ShapeType), memberInfos[i].Name);
                elementsDict.Add(type, buildTexturesByType(type));
            }

            BackgroundSound = content.Load<SoundEffect>("Sound/marimba");
            var backSong = BackgroundSound.CreateInstance();
            backSong.IsLooped = true;
            backSong.Play();
        }

        #region Inner-space logic

        private static ContentManager content;

        private static readonly Dictionary<ShapeType, Texture2D[]> elementsDict =
            new Dictionary<ShapeType, Texture2D[]>();

        private const string element = "Sprites/Elements/";
        private const string lineH = "LineH";
        private const string lineV = "LineV";
        private const string bomb = "Bomb";

        private static Texture2D[] buildTexturesByType(ShapeType type)
        {
            return new Texture2D[4]
            {
                content.Load<Texture2D>(element + type),
                content.Load<Texture2D>(element + type + lineV),
                content.Load<Texture2D>(element + type + lineH),
                content.Load<Texture2D>(element + type + bomb)
            };
        }

        #endregion
    }
}
