using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using ShooterTutorial.GameObjects;
using ShooterTutorial.ShooterContentTypes;


namespace ShooterTutorial
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        // A movement speed for the player.
        private const float PlayerMoveSpeed = 8;

        GraphicsDeviceManager _graphics;
        SpriteBatch _spriteBatch;
        Player _player;

        Texture2D _mainBackground;
        ParallaxingBackground _bgLayer1;
        ParallaxingBackground _bgLayer2;
        Rectangle _rectBackground;
        const float Scale = 1f;


        // Keyboard states
        KeyboardState _currentKeyboardState;
        KeyboardState _prevKeyboardState;

        // Gamepad states
        GamePadState _currentGamePadState;
        GamePadState _prevGamePadState;

        // Mouse states
        MouseState _currentMouseState;
        MouseState _prevMouseState;

        // texture to hold the laser.
        Texture2D laserTexture;
        List<Laser> laserBeams;

        // govern how fast our laser can fire.
        TimeSpan laserSpawnTime;
        TimeSpan previousLaserSpawnTime;

        // The rate at which enemies appear.
        TimeSpan enemySpawnTime;
        TimeSpan previousSpawnTime;

        //Enemies
        Texture2D enemyTexture;
        List<Enemy> enemies;

        // a random number gen
        Random random;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            _player = new Player();

            _bgLayer1 = new ParallaxingBackground();
            _bgLayer2 = new ParallaxingBackground();
            _rectBackground = new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

            TouchPanel.EnabledGestures = GestureType.FreeDrag;

            // init our laser
            laserBeams = new List<Laser>();
            const float SECONDS_IN_MINUTE = 60f;
            const float RATE_OF_FIRE = 200f;
            laserSpawnTime = TimeSpan.FromSeconds(SECONDS_IN_MINUTE / RATE_OF_FIRE);
            previousLaserSpawnTime = TimeSpan.Zero;

            // Initialize the enemies list
            enemies = new List<Enemy>();

            //used to determine how fast the enemies will respawn.
            enemySpawnTime = TimeSpan.FromSeconds(1.0f);

            // init our random number generator
            random = new Random();
            
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load the player resources
            Rectangle titleSafeArea = GraphicsDevice.Viewport.TitleSafeArea;    
            var playerPosition = new Vector2(titleSafeArea.X, titleSafeArea.Y + titleSafeArea.Height/2);
            
            Texture2D playerTexture = Content.Load<Texture2D>("Graphics\\shipAnimation");
            Animation playerAnimation = new Animation();
            playerAnimation.Initialize(playerTexture, playerPosition, 115, 69, 8, 30, Color.White, 1, true);
            
            _player.Initialize(playerAnimation, playerPosition);

            // Load the background.
            _bgLayer1.Initialize(Content, "Graphics/bgLayer1", GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, -1);
            _bgLayer2.Initialize(Content, "Graphics/bgLayer2", GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, -2);
            _mainBackground = Content.Load<Texture2D>("Graphics/mainbackground");

            // load the enemy texture.
            enemyTexture = Content.Load<Texture2D>("Graphics\\mineAnimation");

            // load th texture to serve as the laser.
            laserTexture = Content.Load<Texture2D>("Graphics\\laser");

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // User inputs.
            // Save the previous state of the keyboard and game pad so we can determine single key/button presses
            _prevGamePadState = _currentGamePadState;
            _prevKeyboardState = _currentKeyboardState;
            _prevMouseState = _currentMouseState;
            // Read the current state of the keyboard and gamepad and store it.
            _currentKeyboardState = Keyboard.GetState();
            _currentGamePadState = GamePad.GetState(PlayerIndex.One);
            _currentMouseState = Mouse.GetState();
            
            UpdatePlayer(gameTime);
            _bgLayer1.Update(gameTime);
            _bgLayer2.Update(gameTime);

            // update lasers
            UpdateLasers(gameTime);

            // update the enemies
            UpdateEnemies(gameTime);

            // update collisons
            UpdateCollision();

            base.Update(gameTime);
        }

        void UpdatePlayer(GameTime gameTime)
        {
            _player.Update(gameTime);

            // Touch inputs
            while (TouchPanel.IsGestureAvailable)
            {
                GestureSample gesture = TouchPanel.ReadGesture();
                
                if (gesture.GestureType == GestureType.FreeDrag)
                    _player.Position += gesture.Delta;
            }

            // Get Mouse State
            Vector2 mousePosition = new Vector2(_currentMouseState.X, _currentMouseState.Y);
            if (_currentMouseState.LeftButton == ButtonState.Pressed)
            {
                Vector2 posDelta = mousePosition - _player.Position;
                posDelta.Normalize();
                posDelta = posDelta * PlayerMoveSpeed;
                _player.Position = _player.Position + posDelta;
            }


            // Thumbstick controls
            _player.Position.X += _currentGamePadState.ThumbSticks.Left.X * PlayerMoveSpeed;
            _player.Position.Y += _currentGamePadState.ThumbSticks.Left.Y * PlayerMoveSpeed;

            // Keyboard/DPad
            if (_currentKeyboardState.IsKeyDown(Keys.Left) || _currentGamePadState.DPad.Left == ButtonState.Pressed)
            {
                _player.Position.X -= +PlayerMoveSpeed;
            }
            if (_currentKeyboardState.IsKeyDown(Keys.Right) || _currentGamePadState.DPad.Right == ButtonState.Pressed)
            {
                _player.Position.X += PlayerMoveSpeed;
            }

            if (_currentKeyboardState.IsKeyDown(Keys.Up) || _currentGamePadState.DPad.Up == ButtonState.Pressed)
            {
                _player.Position.Y -= PlayerMoveSpeed;
            }
            if (_currentKeyboardState.IsKeyDown(Keys.Down) || _currentGamePadState.DPad.Down == ButtonState.Pressed)
            {
                _player.Position.Y += PlayerMoveSpeed;
            }

            if (_currentKeyboardState.IsKeyDown(Keys.Space) || _currentGamePadState.Buttons.X == ButtonState.Pressed)
            {
                FireLaser(gameTime);
            }

            // Make sure that the player does not go out of bounds
            _player.Position.X = MathHelper.Clamp(_player.Position.X, 0, GraphicsDevice.Viewport.Width - _player.Width);
            _player.Position.Y = MathHelper.Clamp(_player.Position.Y, 0, GraphicsDevice.Viewport.Height - _player.Height);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            // Start drawing
            _spriteBatch.Begin();

            // Draw background.
            _spriteBatch.Draw(_mainBackground, _rectBackground, Color.White);
            _bgLayer1.Draw(_spriteBatch);
            _bgLayer2.Draw(_spriteBatch);


            // Draw the Player
            _player.Draw(_spriteBatch);

            // Draw the lasers.
            foreach (var l in laserBeams)
            {
                l.Draw(_spriteBatch);
            }


            // draw the enemies
            foreach(var e in enemies)
            {
                e.Draw(_spriteBatch);
            };

            // Stop drawing
            _spriteBatch.End(); 


            base.Draw(gameTime);
        }
        
        protected void UpdateLasers(GameTime gameTime)
        {
           
            // update laserbeams
            for (var i = 0; i < laserBeams.Count;i++ )
                {

                    laserBeams[i].Update(gameTime);

                    // Remove the beam when its deactivated or is at the end of the screen.
                    if (!laserBeams[i].Active || laserBeams[i].Position.X > GraphicsDevice.Viewport.Width)
                    {
                        laserBeams.Remove(laserBeams[i]);
                    }
                }
        }

        protected void FireLaser(GameTime gameTime)
        {
            // govern the rate of fire for our lasers
            if (gameTime.TotalGameTime - previousLaserSpawnTime > laserSpawnTime)
            {
                previousLaserSpawnTime = gameTime.TotalGameTime;

                // Add the laer to our list.
                AddLaser();
            }

        }

        protected void AddLaser()
        {
            Animation laserAnimation = new Animation();

            // initlize the laser animation
            laserAnimation.Initialize(laserTexture,
                _player.Position,
                46,
                16,
                1,
                30,
                Color.White,
                1f,
                true);

            Laser laser = new Laser();

            // Get the starting postion of the laser.
            var laserPostion = _player.Position;
            // Adjust the position slightly to match the muzzle of the cannon.
            laserPostion.Y += 37;
            laserPostion.X += 70;

            // init the laser
            laser.Initialize(laserAnimation, laserPostion);

            laserBeams.Add(laser);

            /* todo: add code to create a laser. */
            //laserSoundInstance.Play();
        }

        protected void UpdateEnemies(GameTime gameTime)
        {
            // spawn a new enemy every 1.5 seconds.
            if (gameTime.TotalGameTime - previousSpawnTime > enemySpawnTime)
            {
                previousSpawnTime = gameTime.TotalGameTime;

                // add an enemy
                AddEnemy();
            }

            // update the enemies
            for (var i = 0; i < enemies.Count; i++)
            {
                enemies[i].Update(gameTime);
                if (!enemies[i].Active)
                {
                    enemies.Remove(enemies[i]);
                }
            }
        }

        protected void AddEnemy()
        {
            // create the animation object
            Animation enemyAnimation = new Animation();

            // Init the animation with the correct 
            // animation information
            enemyAnimation.Initialize(enemyTexture,
                Vector2.Zero,
                47,
                61,
                8,
                30,
                Color.White,
                1f,
                true);

            // randomly generate the postion of the enemy
            Vector2 position = new Vector2(
                GraphicsDevice.Viewport.Width + enemyTexture.Width / 2,
                random.Next(100, GraphicsDevice.Viewport.Height - 100));

            // create an enemy
            Enemy enemy = new Enemy();

            // init the enemy
            enemy.Initialize(enemyAnimation, position);

            // Add the enemy to the active enemies list
            enemies.Add(enemy);

        }

        protected void UpdateCollions()
        {

        }

        protected void UpdateCollision()
        {

            // we are going to use the rectangle's built in intersection
            // methods.

            Rectangle playerRectangle;
            Rectangle enemyRectangle;
            Rectangle laserRectangle;

            // create the rectangle for the player
            playerRectangle = new Rectangle(
                (int)_player.Position.X,
                (int)_player.Position.Y,
                _player.Width,
                _player.Height);

            // detect collisions between the player and all enemies.
            for(var i = 0; i < enemies.Count; i++)
            {
                enemyRectangle = new Rectangle(
                   (int)enemies[i].Position.X,
                   (int)enemies[i].Position.Y,
                   enemies[i].Width,
                   enemies[i].Height);

                // determine if the player and the enemy intersect.
                if (playerRectangle.Intersects(enemyRectangle))
                {
                   

                    // deal damge to the player
                    _player.Health -= enemies[i].Damage;

                    // if the player has no health destroy it.
                    if (_player.Health <= 0)
                    {
                        _player.Active = false;
                    }
                }

                for (var l = 0; l < laserBeams.Count; l++)
                {
                    // create a rectangle for this laserbeam
                    laserRectangle = new Rectangle(
                        (int)laserBeams[l].Position.X,
                        (int)laserBeams[l].Position.Y,
                        laserBeams[l].Width,
                        laserBeams[l].Height);

                    // test the bounds of the laer and enemy
                    if (laserRectangle.Intersects(enemyRectangle))
                    {
                        // kill off the enemy
                        enemies[i].Health = 0;

                        // kill off the laserbeam
                        laserBeams[l].Active = false;
                    }
                }
            }
        }
    }
}
    