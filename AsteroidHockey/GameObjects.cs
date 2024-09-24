using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;


namespace AsteroidHockey
{
    class Asteroid : SpaceObject
    {
        public Asteroid(Texture2D txr, Vector2 pos, float rotSpeed, Vector2 startingSpeed, float mass)
            : base(txr, pos, rotSpeed, startingSpeed, mass)
        { 
            
        }

        public void UpdateMe(GameTime gt, Rectangle sBounds)
        {
            Position += m_velocity * (float)gt.ElapsedGameTime.TotalSeconds;

            m_rotation = (m_rotation + m_rotationSpeed) % MathHelper.TwoPi;


            if (Position.X + m_txr.Width / 2 >= sBounds.Width || Position.X - m_txr.Width / 2 <= 0)
            {
                m_velocity.X = -m_velocity.X;
            }
            if (Position.Y + m_txr.Height / 2 >= sBounds.Height || Position.Y - m_txr.Height / 2 <= 0)
            {
                m_velocity.Y = -m_velocity.Y;
            }
        }
    }

    class BlackHole : SpaceObject
    {
        float innerRotation;
        float alpha = 0.75f;
        float alphaChange = 0.2f;

        private Rectangle srcRect1;
        private Rectangle srcRect2;

        public BlackHole(Texture2D baseTexture, Vector2 center, float rotationSpeed):
            base(baseTexture, center, rotationSpeed, Vector2.Zero, 0)
        {
            srcRect1 = new Rectangle(0, 0, baseTexture.Width / 2, baseTexture.Height);
            srcRect2 = new Rectangle(baseTexture.Width / 2, 0, baseTexture.Width / 2, baseTexture.Height);

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

            Debug.WriteLine(alpha);
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
                + "\nVelocity is: " + m_velocity.ToString()
                + "\nCollision sphere is: " + m_collisionSphere.Center.X.ToString("0.00") + ","
                + m_collisionSphere.Center.Y.ToString("0.00"),
                m_position, Color.White);
#endif
        }
    }
}
