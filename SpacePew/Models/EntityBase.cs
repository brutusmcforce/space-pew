using System;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace SpacePew.Models
{
	public abstract class EntityBase : IEntity
	{
		public const float OneRound = (float)Math.PI * 2;
		public const float VelocityModifier = .1f;
		public const float GravityModifier = 100f;

		public Rectangle BoundingRectangle
		{
			get
			{
				return new Rectangle((int)this.Position.X - (int)this.Origin.X, (int)this.Position.Y - (int)this.Origin.Y, this.Texture.Width, this.Texture.Height);
			}
		}

		private Color[] _textureData;
		public Color[] GetTextureData()
		{
			if (_textureData == null)
			{
				this._textureData = new Color[this.Texture.Width * this.Texture.Height];
				this.Texture.GetData(this._textureData);
			}

			return _textureData;
		}

		// These should be settings in the start menu GUI later on

		public bool IsCollisionWith(IEntity entity)
		{
			// never collide with oneself
			if (ReferenceEquals(entity, this))
				return false;

			return entity.Collide(this);
		}

		public virtual bool Collide(IEntity entity)
		{
			Matrix transform = Matrix.CreateTranslation(new Vector3(this.Position - this.Origin, 0.0f));

			Matrix entityTransform =
					Matrix.CreateTranslation(new Vector3(-entity.Origin, 0.0f)) *
					Matrix.CreateRotationZ(entity.Angle) *
					Matrix.CreateTranslation(new Vector3(entity.Position, 0.0f));

			Rectangle entityRectangle = CalculateBoundingRectangle(
							 new Rectangle(0, 0, entity.Texture.Width, entity.Texture.Height),
							 entityTransform);

			if (entityRectangle.Intersects(this.BoundingRectangle))
			{
				if (IntersectPixels(transform, this.Texture.Width,
														this.Texture.Height, this.GetTextureData(),
														entityTransform, entity.Texture.Width,
														entity.Texture.Height, entity.GetTextureData()))
				{
					return true;
				}
			}

			return false;
		}

		private static bool IntersectPixels(Matrix transformA, int widthA, int heightA, Color[] dataA,
																			 Matrix transformB, int widthB, int heightB, Color[] dataB)
		{
			Matrix transformAtoB = transformA * Matrix.Invert(transformB);

			Vector2 stepX = Vector2.TransformNormal(Vector2.UnitX, transformAtoB);
			Vector2 stepY = Vector2.TransformNormal(Vector2.UnitY, transformAtoB);

			Vector2 yPosInB = Vector2.Transform(Vector2.Zero, transformAtoB);

			for (int yA = 0; yA < heightA; yA++)
			{
				Vector2 posInB = yPosInB;

				for (int xA = 0; xA < widthA; xA++)
				{
					var xB = (int)Math.Round(posInB.X);
					var yB = (int)Math.Round(posInB.Y);

					if (0 <= xB && xB < widthB &&
							0 <= yB && yB < heightB)
					{
						Color colorA = dataA[xA + yA * widthA];
						Color colorB = dataB[xB + yB * widthB];

						if (colorA.A != 0 && colorB.A != 0)
						{
							return true;
						}
					}

					posInB += stepX;
				}

				yPosInB += stepY;
			}

			return false;
		}

		private static Rectangle CalculateBoundingRectangle(Rectangle rectangle, Matrix transform)
		{
			var leftTop = new Vector2(rectangle.Left, rectangle.Top);
			var rightTop = new Vector2(rectangle.Right, rectangle.Top);
			var leftBottom = new Vector2(rectangle.Left, rectangle.Bottom);
			var rightBottom = new Vector2(rectangle.Right, rectangle.Bottom);

			Vector2.Transform(ref leftTop, ref transform, out leftTop);
			Vector2.Transform(ref rightTop, ref transform, out rightTop);
			Vector2.Transform(ref leftBottom, ref transform, out leftBottom);
			Vector2.Transform(ref rightBottom, ref transform, out rightBottom);

			Vector2 min = Vector2.Min(Vector2.Min(leftTop, rightTop), Vector2.Min(leftBottom, rightBottom));
			Vector2 max = Vector2.Max(Vector2.Max(leftTop, rightTop), Vector2.Max(leftBottom, rightBottom));

			return new Rectangle((int)min.X, (int)min.Y, (int)(max.X - min.X), (int)(max.Y - min.Y));
		}

		#region IEntity Members

		public virtual string Owner
		{
			get;
			set;
		}

		public abstract int Health { get; set; }
		public abstract string TextureName { get; }

		public virtual void CollideWithLevel(Level level) { }

		public Texture2D Texture { get; set; }
		public virtual Color Color { get; set; }
		public Vector2 Position { get; set; }
		public Vector2 Velocity { get; set; }

		public virtual Vector2 Origin
		{
			get
			{
				return new Vector2(this.Texture.Width / 2, this.Texture.Height / 2);
			}
		}

		private float _angle;
		public float Angle
		{
			get
			{
				return _angle;
			}
			set
			{
				_angle = value;

				if (_angle > OneRound)
				{
					_angle -= OneRound;
				}
				else if (_angle < 0)
				{
					_angle += OneRound;
				}
			}
		}

		public virtual void Created() { }

		public virtual void ApplyGravity(GameTime time)
		{
			float timeSeconds = time.ElapsedGameTime.Milliseconds * .001f;
			Velocity = new Vector2(
				Velocity.X,
				Velocity.Y + (timeSeconds * GravityModifier));
		}

		public virtual void Update(GameTime time)
		{
			ApplyGravity(time);

			float timeSeconds = time.ElapsedGameTime.Milliseconds * .001f;
			Position += Velocity * timeSeconds;
		}
		#endregion
	}
}
