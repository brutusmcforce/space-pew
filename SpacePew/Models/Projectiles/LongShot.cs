using System;

using Microsoft.Xna.Framework;

namespace SpacePew.Models.Projectiles
{
	public class LongShot : Bullet
	{
		public override Vector2 Origin
		{
			get { return new Vector2(5, 12); }
		}

		public override string TextureName
		{
			get { return "longshot"; }
		}

		public override void ApplyGravity(GameTime time)
		{
			base.ApplyGravity(time);

			//Sätt rotationen till lika som riktningen som pilen åker    
			Vector2 vNormal = Velocity;
			vNormal.Normalize();
			Angle = (vNormal.X > 0 ? 1f : -1f) * (float)(Math.Acos(Vector2.Dot(-Vector2.UnitY, vNormal)));
		}
	}
}
