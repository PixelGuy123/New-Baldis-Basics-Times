using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace BBTimes.Extensions.ComponentCreationExtensions
{
	public static partial class ComponentCreationExtensions
	{
		static readonly FieldInfo _audio_minDistance = AccessTools.Field(typeof(PropagatedAudioManager), "minDistance");
		static readonly FieldInfo _audio_maxDistance = AccessTools.Field(typeof(PropagatedAudioManager), "maxDistance");

		public static PropagatedAudioManager CreateAudioManager(this GameObject target, float minDistance = 25f, float maxDistance = 50f, bool isAPrefab = false)
		{
			var audio = target.AddComponent<PropagatedAudioManager>();
			_audio_minDistance.SetValue(audio, minDistance);
			_audio_maxDistance.SetValue(audio, maxDistance);
			if (!isAPrefab) 
				return audio;

			Object.Destroy(audio.audioDevice.gameObject);
			AudioManager.totalIds--;
			audio.sourceId = 0; // Copypaste from api lmao

			return audio;
		}
	}
}
