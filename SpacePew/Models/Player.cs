using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using SpacePew.Networking;
using SpacePew.Camera;
using SpacePew.Models.Projectiles;
using SpacePew.Models.Weapons;

namespace SpacePew.Models
{
	public class Player : EntityBase, IFocusable
	{
		#region Constructors

		private SoundEffectInstance _thrustSound;
		public Player()
		{
			_weapon = new Cannon();
			_projectile = new Bullet();

			_secondaryWeapon = new ClusterLauncher();
			_secondaryProjectile = new ClusterBomb();

			Health = 100;

			Fuel = 50000;

			_lastCollide = DateTime.Now;

			_thrustSound = SoundManager.GetSoundEffectInstance("Audio/Waves/engine");
		}

		#endregion

		private readonly Vector2 _up = new Vector2(0, -1);
		private Matrix _rotationMatrix;

		private Vector2 _direction;

		private readonly IProjectile _projectile;
		private readonly IProjectile _secondaryProjectile;

		private IWeapon _secondaryWeapon;
		public IWeapon SecondaryWeapon
		{
			get { return _secondaryWeapon; }
			set { _secondaryWeapon = value; }
		}

		private IWeapon _weapon;
		public IWeapon Weapon
		{
			get { return _weapon; }
			set { _weapon = value; }
		}

		public bool IsRemotePlayer { get; set; }
		public bool Landed { get; set; }
		public double Fuel { get; private set; }

		#region Input handling

		private bool _isThrusting;
		public void HandleKeyboard(KeyboardState state)
		{
			if (!MainGame.IsKeyboardInUse)
			{
				if (state.IsKeyDown(Keys.Right))
				{
					if (Landed)
					{
						Landed = false;
					}

					this.MoveRight();
				}

				if (state.IsKeyDown(Keys.Left))
				{
					if (Landed)
					{
						Landed = false;
					}

					this.MoveLeft();
				}

				if (state.IsKeyDown(Keys.Up))
				{
					if (Landed)
					{
						Landed = false;
					}

					_isThrusting = true;

					this.Thrust();
				}
				else
				{
					_isThrusting = false;
				}

				if (state.IsKeyDown(Keys.Space))
				{
					this.Fire();
				}
				else
				{
					if (this.Weapon.Heat > 0)
					{
						this.Weapon.Heat -= 1;
					}
				}

				if (state.IsKeyDown(Keys.LeftControl))
				{
					this._secondaryWeapon.Fire(this._secondaryProjectile, this);
				}
				else
				{
					if (this._secondaryWeapon.Heat > 0)
					{
						this._secondaryWeapon.Heat -= 1;
					}
				}

				if (state.IsKeyDown(Keys.R))
				{
					this.Health = 0;
				}
			}
		}

		private void Fire()
		{
			this._weapon.Fire(_projectile, this);
		}

		private void MoveRight()
		{
			Angle += 0.1f;
		}

		private void MoveLeft()
		{
			Angle -= 0.1f;
		}

		private void Thrust()
		{
			Fuel -= 1;

			_rotationMatrix = Matrix.CreateRotationZ(Angle);
			_direction = Vector2.Transform(_up, _rotationMatrix);

			Velocity += _direction / VelocityModifier;
		}

		#endregion

		#region IEntity Members

		public override string Owner { get; set; }
		public override sealed int Health { get; set; }

		public override void ApplyGravity(GameTime time)
		{
			if (!Landed)
			{
				base.ApplyGravity(time);
			}
		}

		public override Vector2 Origin
		{
			get { return new Vector2(Texture.Width / 2, Texture.Height / 2); }
		}

		public override string TextureName
		{
			get { return "player"; }
		}

		#endregion

		private DateTime _lastCollide;
		public void CollideWith(IEntity entity)
		{
			var projectile = entity as IProjectile;
			if (projectile != null)
			{
				this.Health -= projectile.Damage;
				return;
			}
			var player = entity as Player;
			if (player != null)
			{
				if (_lastCollide <= DateTime.Now.AddMilliseconds(-80))
				{
					_lastCollide = DateTime.Now;

					if (Math.Abs(this.Velocity.Length()) > Math.Abs(entity.Velocity.Length()))
					{
						entity.Velocity += this.Velocity / 2;
						this.Velocity /= 4;
					}
					else
					{
						this.Velocity += entity.Velocity / 2;
						entity.Velocity /= 4;
					}
				}

				this.Landed = false;
				player.Landed = false;
			}
		}

		public override void CollideWithLevel(Level level)
		{
			Velocity -= (Velocity / 300);
		}

		public static Player CreatePlayer(Vector2 startPosition)
		{
			return CreatePlayer(startPosition, string.Empty);
		}

		public static Player CreatePlayer(Vector2 startPosition, string name)
		{
			return EntityFactory.Instance.CreateEntity<Player>(
					name,
					startPosition,
					new Vector2(),
					0
					);
		}

		public void Land()
		{
			if (!Landed)
			{
				this.Velocity = new Vector2(0, 0);
				this.Position = new Vector2(this.Position.X, this.Position.Y - 1); // Need to adjust because per pixel collision 
				// sometimes can't catch up with frame rate
				this.Angle = 0;

				Landed = true;
			}
		}

		public void Kill()
		{
			SoundManager.Play("Audio/Waves/explosion", this.Position);
			this.Position = new Vector2(25, 25);
			this.Velocity = new Vector2(0, 0);
			this.Angle = 0;
			this.Health = 100;
			this.Landed = false;
		}

		public override void Update(GameTime time)
		{
			if (_isThrusting)
			{
				if (_thrustSound.State != SoundState.Playing)
				{
					_thrustSound.Play();
				}

				this.Texture = TextureManager.LoadTexture("player_thrusting");
			}
			else
			{
				_thrustSound.Pause();
				this.Texture = TextureManager.LoadTexture("player");
			}

			base.Update(time);
		}

		#region IFocusable Members

		Vector2 IFocusable.Position
		{
			get { return this.Position; }
		}

		#endregion
	}
}
