using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ShooterTutorial;


namespace ShooterTutorial.GameObjects
{
    public class Enemy
    {
        // animation represneting the enemy.
        public Animation EnemyAnimation;

        // The postion of the enemy ship relative to the 
        // top of left corner of the screen
        public Vector2 Position;

        // state of the enemy ship
        public bool Active;

        // Hit points of the enemy, if this goes
        // to zero the enemy dies.      
        public int Health;

        // the amount of damage that the enemy
        // ship inflicts on the player.
        public int Damage;

        // the amount of the score enemy is worth.
        public int Value;

        // Get the width of the enemy ship
        public int Width
        {
            get { return EnemyAnimation.FrameWidth; }
        }

        // Get the height of the enemy ship.
        public int Height
        {
            get { return EnemyAnimation.FrameHeight; }
        }

        public float enemyMoveSpeed;

        public void Initialize(Animation animation,
            Vector2 position)
        {
            // load the enemy ship texture;
            EnemyAnimation = animation;

            // set the postion of th enemy ship
            Position = position;

            // set the enemy to be active
            Active = true;

            // set the health of the enemy
            Health = 10;

            // Set the amount of damage the enemy does
            Damage = 10;

            // Set how fast the enemy moves.
            enemyMoveSpeed = 10;

            // set the value of the enemy
            Value = 100;
        }

        public void Update(GameTime gameTime)
        {
            // the enemy always moves to the left.
            Position.X -= enemyMoveSpeed;

            // Update the postion of the anaimation
            EnemyAnimation.Position = Position;

            // Update the animation;
            EnemyAnimation.Update(gameTime);

            /* If the enenmy is past the screen or its
             * health reaches 0 then deactivate it. */
            if (Position.X < -Width || Health <= 0)
            {
                //deactivate the enemy
                Active = false;

            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // draw the animation
            EnemyAnimation.Draw(spriteBatch);
        }
    }
}
