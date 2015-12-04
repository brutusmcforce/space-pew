namespace SpacePew.Models.Projectiles
{
	public abstract class ProjectileBase : EntityBase, IProjectile
	{
		#region IEntity Members

		public override bool Collide(IEntity entity)
		{
			bool collided = base.Collide(entity);

			if (collided)
			{
				SoundManager.Play(HitSoundAssetName, this.Position);
			}

			return collided;
		}

		public override void CollideWithLevel(Level level)
		{
			if (this.CollisionType == CollisionType.Explode)
			{
				// blow up somehow (particle system is there, but needs to trash the level a little more too)
			}
			else if (this.CollisionType == CollisionType.Bounce)
			{
				this.Velocity -= (this.Velocity * 1.9f);
			}
		}

		public override void Created()
		{
			SoundManager.Play(FireSoundAssetName, this.Position);
		}

		#endregion

		#region IProjectile Members

		public abstract string HitSoundAssetName { get; }
		public abstract string FireSoundAssetName { get; }
		public abstract int Damage { get; }
		public abstract float Speed { get; }
		public abstract CollisionType CollisionType { get; }

		#endregion
	}
}