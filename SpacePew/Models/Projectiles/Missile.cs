using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpacePew.Models.Projectiles
{
	public sealed class Missile : ProjectileBase
	{
		#region Constructors

		public Missile()
		{
			this.Color = Color.Red;
			this.Velocity = new Vector2();
			this.Angle = 0;
			this.Health = 10;
		}

		public Missile(Vector2 position)
			: this()
		{
			this.Position = position;
		}

		#endregion

		#region IEntity Members

		public override void CollideWithLevel(Level level)
		{
			// blow stuff
		}

		public override int Health
		{
			get;
			set;
		}

		public override string TextureName
		{
			get { return "bullet"; }
		}

		#endregion

		#region IProjectile Members

		public override string FireSoundAssetName
		{
			get { return "Audio/Waves/bullet_sound"; }
		}

		public override string HitSoundAssetName
		{
			get { return "Audio/Waves/bullet_hit"; }
		}

		public override int Damage
		{
			get { return 50; }
		}

		public override float Speed
		{
			get { return 600f; }
		}

		public override CollisionType CollisionType
		{
			get { return CollisionType.Explode; }
		}

		#endregion
	}
}
