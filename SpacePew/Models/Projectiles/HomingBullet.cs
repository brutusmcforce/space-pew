using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;

namespace SpacePew.Models.Projectiles
{
	public class HomingBullet : Bullet
	{
		public HomingBullet()
		{
			this.Color = Color.PowderBlue;
		}

		public override float Speed
		{
			get
			{
				return 200f;
			}
		}

		public override int Damage
		{
			get
			{
				return 3;
			}
		}

		private Player _target;

		const float TimeIncrementSpeed = 5;

		public override void Update(GameTime time)
		{
			base.Update(time);

			if (_target == null)
			{
				GetTarget();
			}

			if (_target != null)
			{
				var delta = new Vector2(_target.Position.X - this.Position.X, _target.Position.Y - this.Position.Y);

				if (delta.Length() > TimeIncrementSpeed)
				{
					delta.Normalize();
					delta.X *= TimeIncrementSpeed;
					delta.Y *= TimeIncrementSpeed;
				}

				this.Position = new Vector2(Position.X + (int)delta.X, Position.Y + (int)delta.Y);
			}
		}

		private void GetTarget()
		{
			var players = EntityFactory.Instance.Entities.OfType<Player>().Where(player => player.Owner != this.Owner).ToList();

			if (players.Count > 0)
			{
				_target = FindClosestPlayer(players, this.Position);
			}
		}

		private static Player FindClosestPlayer(IEnumerable<Player> list, Vector2 pointToCompare)
		{
			Player closestPlayer = list.ToList()[0];

			float distance = float.PositiveInfinity;
			foreach (var player in list)
			{
				float dx = pointToCompare.X - player.Position.X;
				float dy = pointToCompare.Y - player.Position.Y;

				float d = dx * dx - dy * dy;

				if (d < distance)
				{
					distance = d;
					closestPlayer = player;
				}
			}

			return closestPlayer;
		}
	}
}