﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;


namespace AsteroidHockey
{
    class Asteroid : SpaceObject
    {
        private float m_scale = 1;

        SoundEffect m_warpSound;
        SoundEffectInstance m_warpInstance;

        public SoundEffectInstance WarpInstance
        {
            get 
            { 
                return m_warpInstance; 
            }
        }

        public float Scale
        {
            get
            {
                return m_scale;
            }

            set
            {
                m_scale = value;
            }
        }

        private Vector2 m_maxVelocity = new Vector2(250, 250);

        public Asteroid(Texture2D txr, Vector2 pos, float rotSpeed, Vector2 startingSpeed, float mass, SoundEffect warpSound)
            : base(txr, pos, rotSpeed, startingSpeed, mass)
        {
            m_warpSound = warpSound;
            m_warpInstance = m_warpSound.CreateInstance();

            m_warpInstance.Volume = 0.25f;
        }

        public void UpdateMe(GameTime gt, Rectangle sBounds)
        {
            Position += Velocity * (float)gt.ElapsedGameTime.TotalSeconds;

            m_rotation = (m_rotation + m_rotationSpeed) % MathHelper.TwoPi;

            if (Position.X + m_txr.Width / 2 >= sBounds.Width || Position.X - m_txr.Width / 2 <= 0)
            {
                Velocity.X = -Velocity.X;
            }
            if (Position.Y + m_txr.Height / 2 >= sBounds.Height || Position.Y - m_txr.Height / 2 <= 75)
            {
                Velocity.Y = -Velocity.Y;
            }

            Velocity = Vector2.Clamp(Velocity, -m_maxVelocity, m_maxVelocity);
        }

        public void ResetAsteroid()
        {
            m_warpInstance.Stop();
            Position = new Vector2(Game1.windowSize.X / 2, Game1.windowSize.Y / 2 + (75 / 2));
            Scale = 1;
            Velocity = Vector2.Zero;
            CollisionSphere = new BoundingSphere(new Vector3(Position, 0), m_txr.Width / 2);
        }

        public void ReduceSize(GameTime gt)
        {
           m_scale -= 0.25f * (float)gt.ElapsedGameTime.TotalSeconds;
           m_collisionSphere = new BoundingSphere(new Vector3(m_position, 0), (m_txr.Width / 2 * m_scale));
        }

        public override void DrawMe(SpriteBatch sBatch)
        {
            sBatch.Draw(m_txr,
                new Vector2(m_position.X, m_position.Y),
                null,
                Color.White,
                m_rotation,
                m_centreOfRotation,
                m_scale,
                SpriteEffects.None,
                0f);
        }
    }

    class BlackHole : SpaceObject
    {
        float alpha = 0.75f;
        float alphaChange = 0.2f;

        private Rectangle srcRect1;
        private Rectangle srcRect2;

        public BlackHole(Texture2D baseTexture, Vector2 center, float rotationSpeed):
            base(baseTexture, center, rotationSpeed, Vector2.Zero, 0)
        {
            srcRect1 = new Rectangle(0, 0, baseTexture.Width / 2, baseTexture.Height);
            srcRect2 = new Rectangle(baseTexture.Width / 2, 0, baseTexture.Width / 2, baseTexture.Height);

            m_collisionSphere = new BoundingSphere(new Vector3(m_position, 0), m_txr.Width / 4);
            m_collisionSphere.Radius /= 2;
            m_centreOfRotation.X /= 2;
        }

        public override void UpdateMe(GameTime gt)
        {
            m_rotation = (m_rotation += m_rotationSpeed) % (MathHelper.TwoPi * 4) ;

            alpha += alphaChange * (float)gt.ElapsedGameTime.TotalSeconds;

            if (alpha > 0.9f)
            {
                alphaChange *= -1;
            }
            if(alpha < 0.0f)
            {
                alphaChange *= -1;
            }
        }

        public override void DrawMe(SpriteBatch sBatch)
        {
            sBatch.Draw(m_txr, m_position, srcRect1, Color.White * alpha,
                m_rotation, m_centreOfRotation, 1, SpriteEffects.None, 0);
            sBatch.Draw(m_txr, m_position, srcRect2, Color.White * alpha,
                m_rotation / 4, m_centreOfRotation, 1, SpriteEffects.None, 0);

#if DEBUG
            sBatch.DrawString(Game1.debugFont,
                this + " at (" + m_position.X.ToString("0.00") + ", " + m_position.Y.ToString("0.00") + ")"
                + " rot: " + m_rotation.ToString("0.00")
                + "\nVelocity is: " + Velocity.ToString()
                + "\nCollision sphere is: " + m_collisionSphere.Center.X.ToString("0.00") + ","
                + m_collisionSphere.Center.Y.ToString("0.00"),
                m_position, Color.White);
#endif
        }
    }

    class PlayerShip : SpaceObject
    {
        int m_score = 0;

        private bool m_shipCollision = false;
        private bool m_asteroidCollision = false;

        private Color m_tint;

        private Vector2 m_direction;
        private Vector2 m_maxVelocity = new Vector2(200, 200);
        private float m_thrust;

        private float m_inertia;

        private Texture2D m_txrDirection;
        private Vector2 m_txrDirection_Centre;

        private Texture2D m_txrShield;
        private int m_shieldRunTime;
        private int m_shieldDuration;

        SoundEffect m_shipThruster;
        SoundEffect m_shieldSFX;
        SoundEffect m_explosionSFX;
        SoundEffectInstance m_thrusterInstance;
        SoundEffectInstance m_shieldInstance;
        SoundEffectInstance m_explosionInstance;

        public float Rotation
        {
            set
            {
                m_rotation = value;
                m_direction.X = (float)Math.Cos(m_rotation);
                m_direction.Y = (float)Math.Sin(m_rotation);
            }
            get
            {
                return m_rotation;
            }
        }

        public int Score
        {
            get
            {
                return m_score;
            }

            set
            {
                m_score = value;
            }
        }

        public bool ShipCollision
        {
            set
            {
                m_shipCollision = value;
            }
        }

        public bool AsteroidCollision
        {
            set
            {
                m_asteroidCollision = value;
            }
        }


        public PlayerShip(Texture2D txrImage, Texture2D txrDirection, Texture2D txrShield, Vector2 position, float mass, Color tint,
            float thrust, float inertia, SoundEffect shipThruster, SoundEffect shieldSFX, SoundEffect explosionSFX)
            : base(txrImage, position, 0.1f, Vector2.Zero, mass)
        {
            m_tint = tint;

            m_thrust = thrust;
            m_direction = new Vector2(1, 0);

            m_inertia = inertia;

            m_shipThruster = shipThruster;
            m_thrusterInstance = m_shipThruster.CreateInstance();

            m_txrDirection = txrDirection;
            m_txrDirection_Centre = new Vector2(txrDirection.Width / 2, txrDirection.Height / 2);

            m_txrShield = txrShield;
            m_shieldDuration = 32;
            m_shieldRunTime = 0;

            m_shieldSFX = shieldSFX;
            m_shieldInstance = m_shieldSFX.CreateInstance();    

            m_explosionSFX = explosionSFX;
            m_explosionInstance = m_explosionSFX.CreateInstance();

            m_thrusterInstance.Volume = 0.5f;
            m_shieldInstance.Volume = 0.25f;
        }

        public void UpdateMe(GameTime gt, GamePadState pad)
        {
            m_thrusterInstance.Play();

            m_rotation += m_rotationSpeed * pad.ThumbSticks.Left.X;

            m_direction.X = (float)Math.Cos(m_rotation); 
            m_direction.Y = (float)Math.Sin(m_rotation);

            if (m_shieldRunTime > 0)
            {
                m_shieldRunTime--;

                if (m_asteroidCollision)
                {
                    m_explosionInstance.Play();
                }
                else if(m_shipCollision)
                {
                    m_shieldInstance.Play();
                }
            }
            else
            {
                m_shipCollision = false;
                m_asteroidCollision = false;

                m_explosionInstance.Stop();
                m_shieldInstance.Stop();

                Velocity += (m_direction * m_thrust) * (pad.Triggers.Right * 20);
                Velocity += (m_direction * -m_thrust) * (pad.Triggers.Left * 20);
                m_shieldInstance.Pause();
            }
            Position += Velocity * (float)gt.ElapsedGameTime.TotalSeconds;

            Velocity *= m_inertia;
            ReduceVelocity();

            if(pad.Triggers.Right == 0 && pad.Triggers.Left == 0)
            {
                m_thrusterInstance.Pause();
            }
            else
            {
                m_thrusterInstance.Play();
            }

            if(pad.Buttons.LeftShoulder == ButtonState.Pressed)
            {
                m_rotation = 0;
                Position = new Vector2(100, 300);
                Velocity = Vector2.Zero;
            }

            if (pad.Buttons.RightShoulder == ButtonState.Pressed)
            {
                ShieldsUp();
            }
        }

        public void ShieldsUp()
        {
            m_thrusterInstance.Pause();
            m_shieldRunTime = m_shieldDuration;
        }

        public void AddGoal(int scoreToAdd)
        {
            m_score += scoreToAdd;
        }

        private void ReduceVelocity()
        {
            if (Velocity.Length() < 2f)
                Velocity = Vector2.Zero;

            Velocity = Vector2.Clamp(Velocity, -m_maxVelocity, m_maxVelocity);
        }

        public void DrawMe(SpriteBatch sBatch, Rectangle screenBounds, GameTime gt)
        {
            sBatch.Draw(m_txr, m_position, null, m_tint,
                m_rotation, m_centreOfRotation,
                1,
                SpriteEffects.None,
                0f);

            sBatch.Draw(m_txrDirection,
                new Vector2(MathHelper.Clamp(Position.X, screenBounds.X + m_txrDirection.Width / 2, screenBounds.Width - m_txrDirection.Width / 2),
                            MathHelper.Clamp(Position.Y, screenBounds.Y + m_txrDirection.Height / 2, screenBounds.Height - m_txrDirection.Height / 2)),
                null,
                m_tint * 0.5f,
                m_rotation,
                m_txrDirection_Centre,
                1,
                SpriteEffects.None,
                1);

            if(m_shieldRunTime > 0)
            {
                float alpha = 0f;

                sBatch.Draw(m_txrShield,
                    Position,
                    null,
                    m_tint * (float)(alpha += (float)(m_shieldRunTime * gt.ElapsedGameTime.TotalSeconds)),
                    0,
                    new Vector2(m_collisionSphere.Radius),
                    1.5f,
                    SpriteEffects.None,
                    1);
            }

#if DEBUG
            sBatch.DrawString(Game1.debugFont,
                this + " at " + m_position
                + "\nRot: " + m_rotation
                + "\nVelocity: " + Velocity
                + "\nCollision Sphere: " + m_collisionSphere
                + "\nDirection: " + m_direction
                + "\nShields remaining: " + m_shieldRunTime,
                m_position, Color.White);
#endif
        }
    }
}
