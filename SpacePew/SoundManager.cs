using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework;

namespace SpacePew
{
	public static class SoundManager
	{
		private static MainGame _game;

		public static void Initialize(MainGame game)
		{
			_game = game;
		}

		public static SoundEffectInstance GetSoundEffectInstance(string assetName)
		{
			var soundEffect = _game.Content.Load<SoundEffect>(assetName);
			var soundEffectInstance = soundEffect.CreateInstance();

			return soundEffectInstance;
		}

		private static int _playCalled = 0;
		public static void Play(string assetName, Vector2 position)
		{
			Debug.Print("Play called: " + ++_playCalled);

			var soundEffect = _game.Content.Load<SoundEffect>(assetName);
			var soundEffectInstance = soundEffect.CreateInstance();
			var emitter = new AudioEmitter();
			var listener = new AudioListener();
			emitter.Position = new Vector3(position, 0);
			listener.Position = new Vector3(_game.NetworkClient.LocalPlayer.Position, 0);

			soundEffectInstance.Apply3D(listener, emitter);
			soundEffectInstance.Play();
		}
	}
}
