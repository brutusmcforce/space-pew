using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpacePew.Camera;

namespace SpacePew.ParticleSystem
{
	public class ExplosionSmokeParticleSystem : ParticleSystem
	{
		public ExplosionSmokeParticleSystem(MainGame game, int howManyEffects, ICamera2D camera)
			: base(game, howManyEffects, camera)
		{
		}

		protected override void InitializeConstants()
		{
			_textureFilename = "ParticleTextures\\smoke";

			_minInitialSpeed = 20;
			_maxInitialSpeed = 200;

			_minAcceleration = -10;
			_maxAcceleration = -50;

			_minLifetime = 1.0f;
			_maxLifetime = 2.5f;

			_minScale = 1.0f;
			_maxScale = 2.0f;

			_minNumParticles = 10;
			_maxNumParticles = 20;

			_minRotationSpeed = -MathHelper.PiOver4;
			_maxRotationSpeed = MathHelper.PiOver4;

			_blendState = BlendState.AlphaBlend;

			DrawOrder = AlphaBlendDrawOrder;
		}
	}
}
