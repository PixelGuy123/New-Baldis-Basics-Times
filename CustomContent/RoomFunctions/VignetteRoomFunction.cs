using MTM101BaldAPI.Components;
using PixelInternalAPI.Extensions;
using UnityEngine;
using System.Collections.Generic;

namespace BBTimes.CustomContent.RoomFunctions
{
	public class VignetteRoomFunction : RoomFunction
	{

		public override void OnPlayerEnter(PlayerManager player)
		{
			base.OnPlayerEnter(player);
			vignette.gameObject.SetActive(true);
			vignette.worldCamera = Singleton<CoreGameManager>.Instance.GetCamera(player.playerNumber).canvasCam;
			var mod = new ValueModifier();
			mods.Add(player, new(mod, player.GetCustomCam().SlideFOVAnimation(mod, fovModifier, fovEnterSpeed)));
		}

		public override void OnPlayerExit(PlayerManager player)
		{
			base.OnPlayerExit(player);
			if (mods.ContainsKey(player))
			{
				var keyValPair = mods[player];
				player.GetCustomCam().StopCoroutine(keyValPair.Value);
				player.GetCustomCam().ResetSlideFOVAnimation(keyValPair.Key, fovExitSpeed);
				mods.Remove(player);
			}

			if (mods.Count == 0)
				vignette.gameObject.SetActive(false);
		}

		readonly Dictionary<PlayerManager, KeyValuePair<ValueModifier, Coroutine>> mods = [];

		[SerializeField]
		internal Canvas vignette;

		[SerializeField]
		internal float fovModifier = 0f, fovEnterSpeed = 3.5f, fovExitSpeed = 2.5f;
	}
}
