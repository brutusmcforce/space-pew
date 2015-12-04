namespace SpacePew.Models.Projectiles
{
	public interface IProjectile : IEntity
	{
		string HitSoundAssetName { get; }
		string FireSoundAssetName { get; }

		int Damage { get; }
		float Speed { get; }
		CollisionType CollisionType { get; }
	}
}