namespace SpacePew.Models.Projectiles
{
	public sealed class BouncingBullet : Bullet
	{
		public BouncingBullet()
		{
			Health = 30;
		}

		public override CollisionType CollisionType
		{
			get
			{
				return CollisionType.Bounce;
			}
		}
	}
}
