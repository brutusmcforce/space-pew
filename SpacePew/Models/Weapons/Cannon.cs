namespace SpacePew.Models.Weapons
{
	public class Cannon : WeaponBase
	{
		public override float HeatGeneration
		{
			get { return 0.4f; }
		}

		public override int Delay
		{
			get { return 35; }
		}

		public override int Spread
		{
			get { return 0; }
		}
	}
}
