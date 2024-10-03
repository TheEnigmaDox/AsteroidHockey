using Microsoft.Xna.Framework;
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

        public static Point windowSize = new Point(800, 600);

        Rectangle screenBounds;

        StaticGraphic background;
        Asteroid asteroid;
        BlackHole p1Goal;
        BlackHole p2Goal;
        PlayerShip p1ship;

        GamePadState pad1_curr;

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
            p1ship = new PlayerShip(Content.Load<Texture2D>("Textures/falcon"),
                Content.Load<Texture2D>("Textures/direction"),
                Content.Load<Texture2D>("Textures/shield"),
                p1Goal.Position, 10, Color.Red,
                0.2f, 0.97f,
                Content.Load<SoundEffect>("Audio/thrusterFire"),
                Content.Load<SoundEffect>("Audio/forceField"));
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            pad1_curr = GamePad.GetState(PlayerIndex.One);

            asteroid.UpdateMe(gameTime,screenBounds);

            p1Goal.UpdateMe(gameTime);
            p2Goal.UpdateMe(gameTime);

            p1ship.UpdateMe(gameTime, pad1_curr);

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

            p1ship.DrawMe(_spriteBatch, new Rectangle(0, 0, windowSize.X, windowSize.Y), gameTime);

#if DEBUG
    _spriteBatch.DrawString(debugFont, 
        _graphics.PreferredBackBufferWidth + "X" + _graphics.PreferredBackBufferHeight
        + "\nfps" + (int)(1 / gameTime.ElapsedGameTime.TotalSeconds) + "ish"
        + "\npad1 : "
        + (pad1_curr.IsConnected ? " " + pad1_curr.ThumbSticks.Left + pad1_curr.Triggers : "D/C"),
        Vector2.One, Color.White);
#endif


            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
