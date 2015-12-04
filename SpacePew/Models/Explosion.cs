namespace SpacePew.Models
{
	public class Explosion : EntityBase
	{
		public Explosion()
		{
			_health = 10;
		}

		private int _health;
		public override int Health
		{
			get
			{
				return _health;
			}
			set
			{
				_health = value;
			}
		}

		public override string TextureName
		{
			get { return "explosion_small"; }
		}
	}
}