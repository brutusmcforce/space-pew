using System;

using Microsoft.Xna.Framework;
using SpacePew.Models.Projectiles;

namespace SpacePew.Models.Weapons
{
	public class TriCannon : WeaponBase
	{
		public override float HeatGeneration
		{
			get { return 0.8f; }
		}

		public override int Delay
		{
			get { return 40; }
		}

		public override int Spread
		{
			get { return 3; }
		}

		public override void Fire(IProjectile projectile, Player player)
		{
			if (Heat + this.HeatGeneration > MaxHeat)
				return;

			Heat += 1f * this.HeatGeneration;

			if (_lastShot <= DateTime.Now.AddMilliseconds(-this.Delay - (Heat >= HeatStartAffectAt ? Heat : 0)))
			{
				float angle = player.Angle + (float)(Randomizer.Next(-Spread * 100, Spread * 100) / 9000.0);

				InternalFire(projectile, player, angle - 0.15f);
				InternalFire(projectile, player, angle);
				InternalFire(projectile, player, angle + 0.15f);

				_lastShot = DateTime.Now;
			}
		}

		private static void InternalFire(IProjectile projectile, IEntity player, float angle)
		{
			var entity = EntityFactory.Instance.CreateEntity<ProjectileBase>(
					projectile.GetType(),
					player.Owner,
					player.Position,
					new Vector2(),
					angle);

			Matrix m = Matrix.CreateRotationZ(angle);
			Vector2 velocity = Vector2.Transform(-Vector2.UnitY, m);

			entity.Position += velocity * (player.Origin.Y + entity.Origin.Y);
			entity.Velocity = velocity * projectile.Speed;

			entity.Velocity = player.Velocity + entity.Velocity;

			FiredShots.Enqueue(entity);
		}
	}
}