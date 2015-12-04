namespace SpacePew.Models.Weapons
{
	public class ClusterLauncher : WeaponBase
	{
		public override float HeatGeneration
		{
			get { return 100f; }
		}

		public override int Delay
		{
			get { return 0; }
		}

		public override int Spread
		{
			get { return 0; }
		}
	}
}
