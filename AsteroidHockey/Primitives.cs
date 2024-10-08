﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using System;

namespace AsteroidHockey
{
    class StaticGraphic
    {
        protected Vector2 m_position;
        protected Texture2D m_txr;

        public StaticGraphic(Texture2D txrImage, Vector2 position)
        {
            m_position = position;
            m_txr = txrImage;
        }

        public virtual void DrawMe(SpriteBatch sBatch)
        {
            sBatch.Draw(m_txr, m_position, Color.White);
        }
    }

    class TextRenderer
    {
        private SpriteFont m_font;
        private Vector2 m_position;

        float alpha = 0.75f;
        float alphaChange = 0.6f;

        public TextRenderer(SpriteFont font, Vector2 position)
        {
            m_font = font;
            m_position = position;
        }

        public void UpdateMe(GameTime gameTime)
        {
            alpha += alphaChange * (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (alpha > 0.9f)
            {
                alphaChange *= -1;
            }
            if (alpha < 0.0f)
            {
                alphaChange *= -1;
            }
        }

        public void DrawString(SpriteBatch sBatch, string textToDraw)
        {
            sBatch.DrawString(m_font, textToDraw, m_position - m_font.MeasureString(textToDraw) / 2, Color.White * alpha);
        }
    }

    abstract class SpaceObject : StaticGraphic
    {
        public Vector2 Velocity;

        protected Vector2 m_centreOfRotation;
        protected float m_rotation;
        protected float m_rotationSpeed;

        private float m_mass;
        protected BoundingSphere m_collisionSphere;

        public Texture2D Texture
        {
            get
            {
                return m_txr;
            }
        }

        public BoundingSphere CollisionSphere
        {
            get
            {
                return m_collisionSphere;
            }

            set
            {
                m_collisionSphere = value;
            }
        }
        
        public Vector2 Position
        {
            get
            {
                return m_position;
            }
            set
            {
                m_position = value;
                m_collisionSphere.Center = new Vector3(m_position, 0);
            }
        }

        public float Mass
        {
            get
            {
                return m_mass;
            }
        }

        public SpaceObject(Texture2D txr, Vector2 pos,
            float rotationSpeed, Vector2 startingSpeed, float mass)
            : base(txr, pos)
        {
            m_rotation = 0;
            m_rotationSpeed = rotationSpeed;
            m_centreOfRotation = new Vector2(txr.Width / 2, txr.Height / 2);

            Velocity = startingSpeed;

            m_collisionSphere = new BoundingSphere(new Vector3(m_position, 0), txr.Width / 2);
            m_mass = mass;
        }

        public virtual void UpdateMe(GameTime gt)
        {

        }

        public override void DrawMe(SpriteBatch sBatch)
        {
            sBatch.Draw(m_txr, m_position, null, Color.White,
                m_rotation, m_centreOfRotation, 1, SpriteEffects.None, 0);

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
}
