using System.Collections.Generic;

using Microsoft.Xna.Framework.Graphics;

namespace SpacePew
{
	public static class TextureManager
	{
		private static MainGame _game;

		static readonly Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();

		public static void Initialize(MainGame game)
		{
			_game = game;
		}

		public static Texture2D LoadTexture(string assetName)
		{
			if (!Textures.ContainsKey(assetName))
			{
				var texture = _game.Content.Load<Texture2D>(assetName);
				Textures.Add(assetName, texture);
			}

			return Textures[assetName];
		}
	}
}
