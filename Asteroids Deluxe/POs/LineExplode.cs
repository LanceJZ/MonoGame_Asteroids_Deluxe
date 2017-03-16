using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Asteroids_Deluxe
{
    using serv = VectorEngine.Services;

    public class LineExplode : GameComponent
    {
        List<Line> m_Lines;
        Game m_Game;
        bool m_Active = false;

        public bool Active
        {
            get
            {
                return m_Active;
            }
        }

        public LineExplode(Game game) : base(game)
        {
            game.Components.Add(this);

            m_Lines = new List<Line>();
            m_Game = game;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            bool done = true;

            foreach (Line line in m_Lines)
            {
                if (line.Active)
                {
                    done = false;
                    break;
                }
            }

            if (done)
                m_Active = false;
        }

        public void Spawn(Vector3 position, float radius)
        {
            m_Active = true;
            int count = (int)serv.RandomMinMax(5, radius);

            if (count > m_Lines.Count)
            {
                int more = count - m_Lines.Count;

                for (int i = 0; i < more; i++)
                {
                    m_Lines.Add(new Line(m_Game));
                }
            }

            for (int i = 0; i < count; i++)
            {
                m_Lines[i].Spawn(position, radius);
            }
        }
    }
}
