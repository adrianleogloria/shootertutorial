using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ShooterTutorial
{
    class Player
    {
        public Texture2D PlayerTexture;

        public Vector2 PlayerPosition;

        // State of the player
        public bool Active;

        // Amount of hit points the player has
        public int Health;

        public int Width
        { get { return PlayerTexture.Width; } }
        
        public int Height
        { get { return PlayerTexture.Height; } }

        public void Initialize(Texture2D playerTexture, Vector2 playerPosition)
        {
            PlayerTexture = playerTexture;
            PlayerPosition = playerPosition;

            Active = true;
            Health = 100;
        }

        public void Update()
        {
            
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(PlayerTexture, PlayerPosition, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None,
                             0f);
        }
    }
}
