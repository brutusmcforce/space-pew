using System;

using Microsoft.Xna.Framework;

namespace SpacePew.ParticleSystem
{
	public class Particle
	{
		private static readonly Random _random = new Random();
		private float RandomBetween(float min, float max)
		{
			return min + (float)_random.NextDouble() * (max - min);
		}

		public Vector2 Position;
		public Vector2 Velocity;
		public Vector2 Acceleration;

		public float Scale { get; set; }

		public float Lifetime { get; private set; }
		public float TimeSinceStart { get; private set; }
		public float Rotation { get; private set; }
		public float RotationSpeed { get; private set; }

		public bool Active
		{
			get { return TimeSinceStart < Lifetime; }
		}

		public void Initialize(Vector2 position, Vector2 velocity, Vector2 acceleration, float lifetime, float scale, float rotationSpeed)
		{
			this.Position = position;
			this.Velocity = velocity;// *this.Scale;
			this.Acceleration = acceleration;
			this.Lifetime = lifetime * this.Scale;
			this.Scale = scale * this.Scale;
			this.RotationSpeed = rotationSpeed;

			this.TimeSinceStart = 0.0f;

			this.Rotation = RandomBetween(0, MathHelper.TwoPi);
		}

		public void Update(float dt)
		{
			Velocity += Acceleration * dt;
			Position += Velocity * dt;

			Rotation += RotationSpeed * dt;

			TimeSinceStart += dt;
		}
	}
}