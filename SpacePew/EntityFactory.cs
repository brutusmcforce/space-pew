using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpacePew.Models;

namespace SpacePew
{
	public class EntityFactory
	{
		public static EntityFactory Instance { get; private set; }

		private MainGame _game;
		private readonly List<IEntity> _entities = new List<IEntity>();

		public IList<IEntity> Entities
		{
			get { return _entities; }
		}

		public void RemoveEntities(IEnumerable<IEntity> entities)
		{
			Action<IEntity> a = e =>
			{
				e.Collide(null);
				_entities.Remove(e);
			};

			entities.ToList().ForEach(e =>
			{
				if (e is IKillable)
					(e as IKillable).Kill();

				_entities.Remove(e);
			});
		}

		public void RemoveEntity(IEntity entity)
		{
			_entities.Remove(entity);
		}

		public EntityFactory(MainGame game)
		{
			Instance = this;
			this._game = game;
		}

		public T CreateEntity<T>(string owner, Vector2 position, Vector2 velocity, float angle) where T : IEntity, new()
		{
			var entity = new T();

			var tex = TextureManager.LoadTexture(entity.TextureName);
			entity.Texture = tex;
			entity.Position = position;
			entity.Velocity = velocity;
			entity.Angle = angle;

			entity.Owner = owner;

			this._entities.Add(entity);

			entity.Created();

			return entity;
		}

		public T CreateEntity<T>(Type entityType, string owner, Vector2 position, Vector2 velocity, float angle) where T : IEntity
		{
			T entity = (T)entityType.Assembly.CreateInstance(entityType.FullName);

			Texture2D tex = TextureManager.LoadTexture(entity.TextureName);
			entity.Texture = tex;
			entity.Position = position;
			entity.Velocity = velocity;
			entity.Angle = angle;

			entity.Owner = owner;

			this._entities.Add(entity);

			entity.Created();

			return entity;
		}
	}
}
