﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;

namespace AsteroidHockey
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        #if DEBUG
            public static SpriteFont debugFont;
        #endif

        public static Point windowSize = new Point(800, 675);
        public static Texture2D debugPixel;
        public static Random rng = new Random();

        Rectangle screenBounds;

        TextRenderer titleFont;
        TextRenderer pressEnter;
        TextRenderer p1Score;
        TextRenderer p2Score;
        TextRenderer gameOver;

        StaticGraphic titleBar;
        StaticGraphic background;
        StaticGraphic scoreBar;

        Asteroid asteroid;
        BlackHole p1Goal;
        BlackHole p2Goal;
        PlayerShip p1ship;
        PlayerShip p2ship;

        GamePadState pad1_curr;
        GamePadState pad2_curr;
        KeyboardState keyboard_Curr;

        GameState gameState = GameState.Title;

         public enum GameState
        {
            Title,
            Game,
            GameOver
        }

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = windowSize.X;
            _graphics.PreferredBackBufferHeight = windowSize.Y;
            _graphics.ApplyChanges();   
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

        #if DEBUG
            Trace.WriteLine("** GAME TRACE STARTING **");
            Trace.WriteLine("Timestamp is: " + DateTime.Now);   
        #endif

            screenBounds = GraphicsDevice.Viewport.Bounds;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here

#if DEBUG
    debugFont = Content.Load<SpriteFont>(@"Fonts/Arial07");
#endif

            debugPixel = Content.Load<Texture2D>("Textures/pixel");

            titleFont = new TextRenderer(Content.Load<SpriteFont>("Fonts/TitleFont"), 
                new Vector2(windowSize.X / 2, 75 / 2));

            pressEnter = new TextRenderer(Content.Load<SpriteFont>("Fonts/ToolTip"),
                new Vector2(windowSize.X / 2, windowSize.Y - 20));

            titleBar = new StaticGraphic(Content.Load<Texture2D>("Textures/TitleBar"), Vector2.Zero);
            scoreBar = new StaticGraphic(Content.Load<Texture2D>("Textures/ScoreBar"), Vector2.Zero);
            
            background = new StaticGraphic(Content.Load<Texture2D>("Textures/stars800"), new Vector2(0, 75));
            asteroid = new Asteroid(Content.Load<Texture2D>("Textures/asteroid"),
                new Vector2(windowSize.X / 2, windowSize.Y / 2 + (75 / 2)),
                -0.002f, Vector2.Zero, 20);

            p1Goal = new BlackHole(Content.Load<Texture2D>("Textures/warp"), new Vector2(100, 375), 0.02f);
            p2Goal = new BlackHole(Content.Load<Texture2D>("Textures/warp"), new Vector2(700, 375), 0.02f);

            p1ship = new PlayerShip(Content.Load<Texture2D>("Textures/falcon"),
                Content.Load<Texture2D>("Textures/direction"),
                Content.Load<Texture2D>("Textures/shield"),
                p1Goal.Position, 10, Color.Red,
                0.2f, 0.97f,
                Content.Load<SoundEffect>("Audio/thrusterFire"),
                Content.Load<SoundEffect>("Audio/forceField"));

            p2ship = new PlayerShip(Content.Load<Texture2D>("Textures/falcon"),
                Content.Load<Texture2D>("Textures/direction"),
                Content.Load<Texture2D>("Textures/shield"),
                p2Goal.Position,
                10, Color.Yellow,
                0.2f, 0.97f,
                Content.Load<SoundEffect>("Audio/thrusterFire"),
                Content.Load<SoundEffect>("Audio/forceField"));

            p2ship.Rotation = (float)Math.PI;

            p1Score = new TextRenderer(Content.Load<SpriteFont>("Fonts/ScoreText"),
                new Vector2(200, 75 / 2));

            p2Score = new TextRenderer(Content.Load<SpriteFont>("Fonts/ScoreText"),
                new Vector2(600 , 75 / 2)); 

            gameOver = new TextRenderer(Content.Load<SpriteFont>("Fonts/TitleFont"),
                new Vector2(windowSize.X / 2, 75 / 2));
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            pad1_curr = GamePad.GetState(PlayerIndex.One);
            pad2_curr = GamePad.GetState(PlayerIndex.Two);
            keyboard_Curr = Keyboard.GetState();

            switch (gameState)
            {
                case GameState.Title:
                    UpdateTitle(gameTime, pad1_curr, pad2_curr, keyboard_Curr);
                    break;
                case GameState.Game:
                    UpdateGame(gameTime);
                    break;
                case GameState.GameOver:
                    UpdateGameOver();
                    break;
            }

            base.Update(gameTime);
        }

        void UpdateTitle(GameTime gameTime, GamePadState pad1, GamePadState pad2, KeyboardState keyboardState)
        {
            pressEnter.UpdateMe(gameTime);

            p1Goal.UpdateMe(gameTime);
            p2Goal.UpdateMe(gameTime);

            asteroid.UpdateMe(gameTime, new Rectangle(0, 75, windowSize.X, windowSize.Y));

            if(pad1.Buttons.A == ButtonState.Pressed ||
                pad2.Buttons.A == ButtonState.Pressed ||
                keyboardState.IsKeyDown(Keys.Enter))
            {
                gameState = GameState.Game;

                asteroid.Position = new Vector2(windowSize.X / 2, windowSize.Y / 2 + (75 / 2));
            }
        }

        void UpdateGame(GameTime gameTime)
        {
            asteroid.UpdateMe(gameTime, screenBounds);

            p1Goal.UpdateMe(gameTime);
            p2Goal.UpdateMe(gameTime);

            p1ship.UpdateMe(gameTime, pad1_curr);
            p2ship.UpdateMe(gameTime, pad2_curr);


            if (asteroid.CollisionSphere.Intersects(p1ship.CollisionSphere))
            {
#if DEBUG
                Trace.WriteLine("Asteriod/P1 collision at : " + gameTime.TotalGameTime + "!");
#endif
                UtilityFunctions.cresponse(asteroid.Position, p1ship.Position,
                    ref asteroid.Velocity, ref p1ship.Velocity,
                    asteroid.Mass, p1ship.Mass);

                p1ship.ShieldsUp();
            }

            if (asteroid.CollisionSphere.Intersects(p2ship.CollisionSphere))
            {
#if DEBUG
                Trace.WriteLine("Asteroid/P2 collision at : " + gameTime.TotalGameTime + "!");
#endif 
                UtilityFunctions.cresponse(asteroid.Position, p2ship.Position,
                    ref asteroid.Velocity, ref p2ship.Velocity,
                    asteroid.Mass, p2ship.Mass);

                p2ship.ShieldsUp();
            }

            if (p1ship.CollisionSphere.Intersects(p2ship.CollisionSphere))
            {
#if DEBUG
                Trace.WriteLine("P1/P2 collision at " + gameTime.TotalGameTime);
#endif 
                UtilityFunctions.cresponse(p1ship.Position, p2ship.Position,
                    ref p1ship.Velocity, ref p2ship.Velocity,
                    p1ship.Mass, p2ship.Mass);

                p1ship.ShieldsUp();
                p2ship.ShieldsUp();
            }

            if (asteroid.CollisionSphere.Intersects(p1Goal.CollisionSphere))
            {
                asteroid.ReduceSize(gameTime);

                if(asteroid.Scale < 0.5f)
                {
                    asteroid.ResetAsteroid();
                    
                    p1ship.Position = p1Goal.Position;
                    p2ship.Position = p2Goal.Position;
                    p2ship.AddGoal(1);
                }
                
            }
            if (asteroid.CollisionSphere.Intersects(p2Goal.CollisionSphere))
            {
                asteroid.ReduceSize(gameTime);

                if (asteroid.Scale < 0.5f)
                {
                    asteroid.ResetAsteroid();
                    
                    p1ship.Position = p1Goal.Position;
                    p2ship.Position = p2Goal.Position;
                    p1ship.AddGoal(1);
                }
            }

            if(pad1_curr.Buttons.Start == ButtonState.Pressed)
            {
                gameState = GameState.GameOver;
            }

            if (p1ship.Score == 5)
            {
                gameState = GameState.GameOver;
            }
            else if (p2ship.Score == 5)
            {
                gameState = GameState.GameOver;
            }
        }

        void UpdateGameOver()
        {

        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            _spriteBatch.Begin();

            switch (gameState)
            {
                case GameState.Title:
                    DrawTitle();
                    break;
                case GameState.Game:
                    DrawGame(gameTime);
                    break;
                case GameState.GameOver:
                    DrawGameOver();
                    break;
            }

#if DEBUG
            _spriteBatch.DrawString(debugFont,
                _graphics.PreferredBackBufferWidth + "X" + _graphics.PreferredBackBufferHeight
                + "\nfps" + (int)(1 / gameTime.ElapsedGameTime.TotalSeconds) + "ish"
                + "\npad1 : "
                + (pad1_curr.IsConnected ? " " + pad1_curr.ThumbSticks.Left + pad1_curr.Triggers : "D/C")
                +(pad2_curr.IsConnected ? " " + pad2_curr.ThumbSticks.Left + pad2_curr.Triggers : "D/C"),
                new Vector2(0, 100), Color.White);
#endif


            _spriteBatch.End();
            base.Draw(gameTime);
        }

        void DrawTitle()
        {
            #region Game Hud
            titleBar.DrawMe(_spriteBatch);
            background.DrawMe(_spriteBatch);

            titleFont.DrawString(_spriteBatch, "Asteroid Hockey");
            
            #endregion

            p1Goal.DrawMe(_spriteBatch);
            p2Goal.DrawMe(_spriteBatch);

            asteroid.DrawMe(_spriteBatch);

            pressEnter.DrawString(_spriteBatch, "Press Enter/A to start!");
        }

        void DrawGame(GameTime gameTime)
        {
            scoreBar.DrawMe(_spriteBatch);
            background.DrawMe(_spriteBatch);

            p1Score.DrawString(_spriteBatch, "Goals : " + p1ship.Score);
            p2Score.DrawString(_spriteBatch, "Goals : " + p2ship.Score);

            p1Goal.DrawMe(_spriteBatch);
            p2Goal.DrawMe(_spriteBatch);
            asteroid.DrawMe(_spriteBatch);

            p1ship.DrawMe(_spriteBatch, new Rectangle(0, 0, windowSize.X, windowSize.Y), gameTime);
            p2ship.DrawMe(_spriteBatch, new Rectangle(0, 0, windowSize.X, windowSize.Y), gameTime);
        }

        void DrawGameOver()
        {
            titleBar.DrawMe(_spriteBatch);
            background.DrawMe(_spriteBatch);

            gameOver.DrawString(_spriteBatch, "GAME OVER!");
        }
    }
}
