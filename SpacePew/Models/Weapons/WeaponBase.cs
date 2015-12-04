using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using SpacePew.Models.Projectiles;

namespace SpacePew.Models.Weapons
{
	public abstract class WeaponBase : IWeapon
	{
		protected const int MaxHeat = 100;
		protected const int HeatStartAffectAt = MaxHeat / 2;

		static WeaponBase()
		{
			FiredShots = new Queue<IProjectile>();
		}

		public static Queue<IProjectile> FiredShots;

		protected DateTime _lastShot;

		private static Random _randomizer;

		public static Random Randomizer
		{
			get { return _randomizer ?? (_randomizer = new Random()); }
		}

		private float _heat;

		protected WeaponBase()
		{
			_lastShot = DateTime.Now;
		}

		public virtual float Heat
		{
			get { return _heat; }
			set { _heat = value; }
		}

		#region IWeapon Members

		public abstract float HeatGeneration { get; }
		public abstract int Delay { get; }
		public abstract int Spread { get; }

		public virtual void Fire(IProjectile projectile, Player player)
		{
			if (_heat + this.HeatGeneration > MaxHeat)
				return;

			_heat += 1f * this.HeatGeneration;

			if (_lastShot <= DateTime.Now.AddMilliseconds(-this.Delay - (_heat >= HeatStartAffectAt ? _heat : 0)))
			{
				float angle = player.Angle + (float)(Randomizer.Next(-Spread * 100, Spread * 100) / 9000.0);

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

				_lastShot = DateTime.Now;
			}
		}

		#endregion
	}
}