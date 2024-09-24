using Microsoft.Xna.Framework;
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

        public static Point windowSize = new Point(800, 600);

        Rectangle screenBounds;

        StaticGraphic background;
        Asteroid asteroid;
        BlackHole p1Goal;
        BlackHole p2Goal;

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

            background = new StaticGraphic(Content.Load<Texture2D>("Textures/stars800"), Vector2.Zero);
            asteroid = new Asteroid(Content.Load<Texture2D>("Textures/asteroid"),
                new Vector2(windowSize.X / 2, windowSize.Y / 2),
                -0.002f, new Vector2(75, 75), 0);

            p1Goal = new BlackHole(Content.Load<Texture2D>("Textures/warp"), new Vector2(100, 300), 0.02f);
            p2Goal = new BlackHole(Content.Load<Texture2D>("Textures/warp"), new Vector2(700, 300), 0.02f);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            asteroid.UpdateMe(gameTime,screenBounds);

            p1Goal.UpdateMe(gameTime);
            p2Goal.UpdateMe(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            _spriteBatch.Begin();

            background.DrawMe(_spriteBatch);
            p1Goal.DrawMe(_spriteBatch);
            p2Goal.DrawMe(_spriteBatch);
            asteroid.DrawMe(_spriteBatch);

#if DEBUG
    _spriteBatch.DrawString(debugFont, _graphics.PreferredBackBufferWidth + "X" +
        _graphics.PreferredBackBufferHeight + "\nfps" + (int)(1 / gameTime.ElapsedGameTime.TotalSeconds) + "ish",
        Vector2.One, Color.White);
#endif


            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
