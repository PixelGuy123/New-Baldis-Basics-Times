using HarmonyLib;
using System.Collections;
using UnityEngine;
using static UnityEngine.Object;
using BBTimes.CustomComponents;

namespace BBTimes.ModPatches
{
	[HarmonyPatch(typeof(Gum))]
	internal class GumSplash
	{
		[HarmonyPatch("OnEntityMoveCollision")]
		[HarmonyPrefix]
		private static void AnimationPre(out bool __state, bool ___flying) => // Basically trigger .Hide() after the gum is properly disabled
			__state = ___flying;

		[HarmonyPatch("OnEntityMoveCollision")]
		[HarmonyPostfix]
		private static void TriggerAnimation(bool __state, Gum __instance, ref RaycastHit hit)
		{
			if (__state && hit.transform.gameObject.layer != 2)
			{
				__instance.Hide();
				var gum = Instantiate(gumSplash);
				gum.transform.position = hit.transform.position - __instance.transform.forward * 0.03f;
				gum.transform.rotation = Quaternion.Euler(0f, (__instance.transform.rotation.eulerAngles.y + 180f) % 360f, 0f); // Quaternion.Inverse doesn't reverse y with 180 and 0 angles. Wth
				gum.transform.localScale = Vector3.zero;
				gum.gameObject.SetActive(true);
				gum.GetComponent<EmptyMonoBehaviour>().StartCoroutine(Timer(gum, 10f, __instance.ec));
			}
		}


		[HarmonyPatch("Initialize")]
		[HarmonyPostfix]
		private static void SetAudio(ref SoundObject ___audSplat) => ___audSplat = splash;

		static IEnumerator Timer(Transform target, float cooldown, EnvironmentController ec)
		{
			float sizeSpeed = 0f;
			float size = 0;
			while (true)
			{
				sizeSpeed += 0.6f * Time.deltaTime * ec.EnvironmentTimeScale;
				size += sizeSpeed;
				if (size >= 1.01f)
					break;
				target.localScale = Vector3.one * size;
				yield return null;
			}
			target.localScale = Vector3.one;
			size = 1;

			float c = cooldown;
			while (c > 0f)
			{
				c -= Time.deltaTime * ec.EnvironmentTimeScale;
				yield return null;
			}

			sizeSpeed = 0f;
			while (true)
			{
				sizeSpeed += 0.5f * Time.deltaTime * ec.EnvironmentTimeScale;
				size -= sizeSpeed;
				if (size <= 0f)
					break;
				target.localScale = Vector3.one * size;
				yield return null;
			}
			Destroy(target.gameObject);

			yield break;
		}

		public static Transform gumSplash;

		public static SoundObject splash;
	}
}
