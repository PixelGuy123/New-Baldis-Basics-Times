using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
namespace BBTimes.ModPatches
{
	[HarmonyPatch(typeof(GameCamera), "Awake")]
	internal class GameCameraPatch
	{
		private static void Prefix(GameCamera __instance)
		{
			var visual = Object.Instantiate(playerVisual, __instance.transform);
			visual.transform.localPosition = Vector3.zero;
			visual.Initialize(__instance);
		}

		static internal PlayerVisual playerVisual;
	}
	public class PlayerVisual : MonoBehaviour 
	{
		[SerializeField]
		internal Sprite[] emotions;

		[SerializeField]
		internal SpriteRenderer renderer;
		int id = -1;

		readonly static Dictionary<int, PlayerVisual> visuals = [];
		public static PlayerVisual GetPlayerVisual(int id) => visuals[id];
		public void SetEmotion(int id) =>
			renderer.sprite = emotions[id];

		public void Initialize(GameCamera cam) 
		{
			visuals.Add(cam.camNum, this);
			id = cam.camNum;
		}

		void OnDestroy() =>
			visuals.Remove(id);
	}
}
