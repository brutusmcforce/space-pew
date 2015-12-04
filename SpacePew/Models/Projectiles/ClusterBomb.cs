using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using SpacePew.Models.Weapons;

namespace SpacePew.Models.Projectiles
{
	class ClusterBomb : Bullet, IKillable
	{
		private const int FuseTime = 7000; //time in milliseconds before it explodes
		private double _elapsed = 0;
		readonly DateTime _startTick;

		public override int Damage
		{
			get { return 10; }
		}

		public override float Speed
		{
			get { return 100; }
		}

		public ClusterBomb()
		{
			_startTick = DateTime.Now;
		}

		public override Color Color
		{
			get
			{
				var r = (byte)((_elapsed / FuseTime) * 255f);
				var g = (byte)(128 + Math.Sin(_elapsed / 150f) * 127f);
				var b = (byte)(255 - g);
				return new Color(r, g, b);
			}
		}

		public override void Update(Microsoft.Xna.Framework.GameTime time)
		{
			_elapsed = (DateTime.Now - _startTick).TotalMilliseconds;

			if (_elapsed >= FuseTime)
			{
				this.Health = 0;
			}

			base.Update(time);
		}

		private void CreateCluster(float angle, string owner)
		{
			var entity = EntityFactory.Instance.CreateEntity<ProjectileBase>(
					typeof(Bullet),
					owner,
					Position,
					Vector2.Zero,
					angle);

			Matrix m = Matrix.CreateRotationZ(angle + (float)(WeaponBase.Randomizer.NextDouble() * .2f - .1f));
			Vector2 velocity = Vector2.Transform(-Vector2.UnitY, m);

			entity.Position += velocity * 10;
			entity.Velocity = velocity * entity.Speed * (float)(WeaponBase.Randomizer.NextDouble() + 0.5f);

			//TODO.. fix
			//FiredShots.Enqueue(entity);
		}

		public override void ApplyGravity(GameTime time)
		{
			float timeSeconds = time.ElapsedGameTime.Milliseconds * .001f;
			Velocity = new Vector2(
					Velocity.X,
					Velocity.Y + (timeSeconds * GravityModifier * 0.5f));
		}

		#region IKillable Members

		public void Kill()
		{
			//TODO: make sure the clusters are only created on the client that created the cluster, and then let the server send them to all klients
			for (float i = 0; i < 45; i += .5f)
				CreateCluster(i, this.Owner);
		}

		#endregion
	}
}
