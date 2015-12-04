using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpacePew.Models.Projectiles
{
	public class Bullet : ProjectileBase
	{
		#region Constructors

		public Bullet()
		{
			this.Color = Color.White;
			this.Velocity = new Vector2();
			this.Angle = 0;
			this.Health = 15;
		}

		public Bullet(Vector2 position)
			: this()
		{
			this.Position = position;
		}

		#endregion

		#region IEntity Members

		public override sealed int Health
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
			get { return 5; }
		}

		public override float Speed
		{
			get { return 400f; }
		}

		public override CollisionType CollisionType
		{
			get { return CollisionType.Explode; }
		}

		#endregion
	}
}