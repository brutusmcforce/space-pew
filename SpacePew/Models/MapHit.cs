using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SpacePew.Models
{
	public class MapHit
	{
		public Texture2D Texture { get; private set; }
		public Vector2 Position { get; private set; }
		public Vector2 Origin { get; private set; }
		public float Angle { get; private set; }

		public MapHit(IEntity entity)
		{
			Texture = entity.Texture;
			Position = entity.Position;
			Origin = entity.Origin;
			Angle = entity.Angle;
		}
	}
}
