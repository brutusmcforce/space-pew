using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SpacePew.Models
{
	public interface IEntity
	{
		string TextureName { get; }
		Texture2D Texture { get; set; }
		Color[] GetTextureData();
		Color Color { get; set; }

		string Owner { get; set; }

		int Health { get; set; }

		Vector2 Origin { get; }
		Vector2 Position { get; set; }
		Vector2 Velocity { get; set; }
		float Angle { get; set; }

		bool Collide(IEntity entity);
		void CollideWithLevel(Level level);
		void ApplyGravity(GameTime time);
		void Update(GameTime time);
		void Created();
	}
}
