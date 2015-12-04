using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpacePew.Camera;

namespace SpacePew.ParticleSystem
{
	public class ExplosionParticleSystem : ParticleSystem
	{
		public ExplosionParticleSystem(MainGame game, int howManyEffects, ICamera2D camera) : base(game, howManyEffects, camera) { }

		protected override void InitializeConstants()
		{
			_textureFilename = "ParticleTextures\\explosion";

			_minInitialSpeed = 40;
			_maxInitialSpeed = 500;

			_minAcceleration = 0;
			_maxAcceleration = 0;

			_minLifetime = .5f;
			_maxLifetime = 1.0f;

			_minScale = .3f;
			_maxScale = 1.0f;

			_minNumParticles = 20;
			_maxNumParticles = 25;

			_minRotationSpeed = -MathHelper.PiOver4;
			_maxRotationSpeed = MathHelper.PiOver4;

			_blendState = BlendState.Additive;

			DrawOrder = AdditiveDrawOrder;
		}

		protected override void InitializeParticle(Particle p, Vector2 where)
		{
			base.InitializeParticle(p, where);

			p.Acceleration = -p.Velocity / p.Lifetime;
		}
	}
}