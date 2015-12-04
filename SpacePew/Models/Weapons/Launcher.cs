namespace SpacePew.Models.Weapons
{
	public class Launcher : WeaponBase
	{
		public override float HeatGeneration
		{
			get { return 100f; }
		}

		public override int Delay
		{
			get { return 200; }
		}

		public override int Spread
		{
			get { return 0; }
		}
	}
}