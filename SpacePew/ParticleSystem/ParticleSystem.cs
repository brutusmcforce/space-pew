using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using SpacePew.Camera;

namespace SpacePew.ParticleSystem
{
	public abstract class ParticleSystem : DrawableGameComponent
	{
		private static readonly Random _random = new Random();
		protected Random Random
		{
			get { return _random; }
		}

		protected float RandomBetween(float min, float max)
		{
			return min + (float)_random.NextDouble() * (max - min);
		}

		public const int AlphaBlendDrawOrder = 100;
		public const int AdditiveDrawOrder = 200;

		private readonly MainGame _game;
		private readonly int _howManyEffects;

		private Texture2D _texture;
		private Vector2 _origin;

		private Particle[] _particles;

		private Queue<Particle> _freeParticles;

		public int FreeParticleCount
		{
			get { return _freeParticles.Count; }
		}

		protected int _minNumParticles;
		protected int _maxNumParticles;

		protected string _textureFilename;

		protected float _minInitialSpeed;
		protected float _maxInitialSpeed;

		protected float _minAcceleration;
		protected float _maxAcceleration;

		protected float _minRotationSpeed;
		protected float _maxRotationSpeed;

		protected float _minLifetime;
		protected float _maxLifetime;

		protected float _minScale;
		protected float _maxScale;

		protected BlendState _blendState;

		protected ICamera2D _camera;

		protected ParticleSystem(MainGame game, int howManyEffects, ICamera2D camera)
			: base(game)
		{
			this._game = game;
			this._howManyEffects = howManyEffects;
			this._camera = camera;
		}

		public override void Initialize()
		{
			InitializeConstants();

			_particles = new Particle[_howManyEffects * _maxNumParticles];
			_freeParticles = new Queue<Particle>(_howManyEffects * _maxNumParticles);

			for (int i = 0; i < _particles.Length; i++)
			{
				_particles[i] = new Particle();
				_freeParticles.Enqueue(_particles[i]);
			}

			base.Initialize();
		}

		protected abstract void InitializeConstants();

		protected override void LoadContent()
		{
			if (string.IsNullOrEmpty(_textureFilename))
			{
				throw new ArgumentNullException("textureFilename");
			}

			_texture = _game.Content.Load<Texture2D>(_textureFilename);

			_origin.X = _texture.Width / 2;
			_origin.Y = _texture.Height / 2;

			base.LoadContent();
		}

		public void AddParticles(Vector2 where, float scale)
		{
			int numParticles = Random.Next(_minNumParticles, _maxNumParticles);

			for (int i = 0; i < numParticles && _freeParticles.Count > 0; i++)
			{
				Particle p = _freeParticles.Dequeue();
				p.Scale = scale;
				InitializeParticle(p, where);
			}
		}

		protected virtual void InitializeParticle(Particle p, Vector2 where)
		{
			Vector2 direction = PickRandomDirection();

			float velocity =
					RandomBetween(_minInitialSpeed, _maxInitialSpeed);
			float acceleration =
					RandomBetween(_minAcceleration, _maxAcceleration);
			float lifetime =
					RandomBetween(_minLifetime, _maxLifetime);
			float scale =
					RandomBetween(_minScale, _maxScale);
			float rotationSpeed =
					RandomBetween(_minRotationSpeed, _maxRotationSpeed);

			p.Initialize(
					where, velocity * direction, acceleration * direction,
					lifetime, scale, rotationSpeed);
		}

		protected virtual Vector2 PickRandomDirection()
		{
			float angle = RandomBetween(0, MathHelper.TwoPi);
			return new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
		}

		public override void Update(GameTime gameTime)
		{
			var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

			foreach (var p in _particles.Where(p => p.Active))
			{
				p.Update(dt);

				if (!p.Active)
				{
					_freeParticles.Enqueue(p);
				}
			}

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			_game.SpriteBatch.Begin(SpriteSortMode.Deferred, _blendState, null, null, null, null, _camera.Transform);

			foreach (Particle p in _particles)
			{
				if (!p.Active)
					continue;

				float normalizedLifetime = p.TimeSinceStart / p.Lifetime;

				float alpha = 4 * normalizedLifetime * (1 - normalizedLifetime);
				var color = new Color(new Vector4(1, 1, 1, alpha));

				float scale = p.Scale * (.75f + .25f * normalizedLifetime);

				_game.SpriteBatch.Draw(_texture,
						p.Position,
						null,
						color,
						p.Rotation,
						_origin,
						scale,
						SpriteEffects.None,
						0.0f);
			}

			_game.SpriteBatch.End();

			base.Draw(gameTime);
		}
	}
}
