using SpacePew.Models.Projectiles;

namespace SpacePew.Models.Weapons
{
	public interface IWeapon
	{
		int Delay { get; }
		int Spread { get; }
		float Heat { get; set; }
		float HeatGeneration { get; }
		void Fire(IProjectile projectile, Player player);
	}
}