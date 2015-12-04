using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpacePew.Camera;

namespace SpacePew.ParticleSystem
{
	public class SmokePlumeParticleSystem : ParticleSystem
	{
		public SmokePlumeParticleSystem(MainGame game, int howManyEffects, ICamera2D camera)
			: base(game, howManyEffects, camera)
		{
		}

		protected override void InitializeConstants()
		{
			_textureFilename = "ParticleTextures\\smoke";

			_minInitialSpeed = 20;
			_maxInitialSpeed = 100;

			_minAcceleration = 0;
			_maxAcceleration = 0;

			_minLifetime = 5.0f;
			_maxLifetime = 7.0f;

			_minScale = .5f;
			_maxScale = 1.0f;

			_minNumParticles = 7;
			_maxNumParticles = 15;

			_minRotationSpeed = -MathHelper.PiOver4 / 2.0f;
			_maxRotationSpeed = MathHelper.PiOver4 / 2.0f;

			_blendState = BlendState.AlphaBlend;

			DrawOrder = AlphaBlendDrawOrder;
		}

		protected override Vector2 PickRandomDirection()
		{
			float radians = RandomBetween(
					MathHelper.ToRadians(80), MathHelper.ToRadians(100));

			Vector2 direction = Vector2.Zero;

			direction.X = (float)Math.Cos(radians);
			direction.Y = -(float)Math.Sin(radians);
			return direction;
		}

		protected override void InitializeParticle(Particle p, Vector2 where)
		{
			base.InitializeParticle(p, where);

			p.Acceleration.X += RandomBetween(10, 50);
		}
	}
}
