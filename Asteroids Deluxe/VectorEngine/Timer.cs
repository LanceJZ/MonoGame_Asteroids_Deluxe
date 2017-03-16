using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Asteroids_Deluxe.VectorEngine
{
    class Timer : GameComponent
    {
        private float m_Seconds = 0;
        private float m_Amount = 0;

        public float Seconds
        {
            get { return m_Seconds; }
        }

        public float Amount
        {
            get
            {
                return m_Amount;
            }

            set
            {
                m_Amount = value;
            }
        }

        public Timer(Game game) : base(game)
        {
            game.Components.Add(this);
        }

        public override void Update(GameTime gameTime)
        {
            m_Seconds += (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void Reset()
        {
            m_Seconds = 0;
        }
    }
}
