namespace SpacePew.Models.Weapons
{
	/// <summary>
	/// A slower version of the cannon, should be used for powerful projectiles like the homing bullets to even things out
	/// </summary>
	public class SecondaryCannon : Cannon
	{
		public override float HeatGeneration
		{
			get
			{
				return 1f;
			}
		}

		public override int Delay
		{
			get
			{
				return 80;
			}
		}
	}
}