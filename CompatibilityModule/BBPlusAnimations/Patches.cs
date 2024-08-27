using BBPlusAnimations.Components;
using BBPlusAnimations.Patches;
using BBTimes.CustomComponents;
using BBTimes.CustomContent.CustomItems;
using BBTimes.Plugin;
using BepInEx.Bootstrap;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;
using PixelInternalAPI.Components;
using PixelInternalAPI.Extensions;
using System.Collections;
using System.IO;
using UnityEngine;

namespace BBTimes.CompatibilityModule.BBPlusAnimations
{
	[HarmonyPatch]
	[ConditionalPatchMod("pixelguy.pixelmodding.baldiplus.newanimations")]
	internal static class PostTimesPatchForAnimations
	{
		[HarmonyPatch(typeof(BasePlugin), "SetupPostAssets")]
		[HarmonyPostfix]
		static void Preload() // Actually load the stuff
		{
			var animations = (global::BBPlusAnimations.BasePlugin)Chainloader.PluginInfos["pixelguy.pixelmodding.baldiplus.newanimations"].Instance;
			var aud = ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(AssetLoader.GetModPath(animations), "GS_Sweeping.wav")), "Vfx_GottaSweep", SoundType.Voice, new(0, 0.6226f, 0.0614f));
			var sweepSprs = TextureExtensions.LoadSpriteSheet(7, 1, 26f,
					BasePlugin.ModPath, "npcs", "ClassicGottaSweep", "Textures", "anims", "oldsweep.png");

			NPCMetaStorage.Instance.Get(Character.Sweep).prefabs.DoIf(x => x.Value.GetComponent<INPCPrefab>() != null, (x) =>
			{
				var comp = x.Value.gameObject.AddComponent<GenericAnimationExtraComponent>();
				comp.sprites = [x.Value.spriteRenderer[0].sprite, .. sweepSprs];

				var c = x.Value.gameObject.AddComponent<GottaSweepComponent>();
				c.aud_sweep = aud;

			});
			((ITM_Gum)ItemMetaStorage.Instance.FindByEnum(EnumExtensions.GetFromExtendedName<Items>("Gum")).value.item).aud_splash = GumSplash.splash;
		}

		[HarmonyPatch(typeof(ITM_Gum), "HitSomething")] // Override gum hit animation
		[HarmonyPrefix]
		private static void OverrideGumBehaviour(ITM_Gum __instance, ref RaycastHit hit, EnvironmentController ___ec, Transform ___rendererBase, Transform ___flyingSprite)
		{
			var gum = Object.Instantiate(GumSplash.gumSplash);
			Vector3 pos = hit.transform.position - (__instance.transform.forward * 0.03f);
			pos.y = ___flyingSprite.position.y;
			gum.transform.position = pos;
			gum.transform.rotation = Quaternion.Euler(0f, (__instance.transform.rotation.eulerAngles.y + 180f) % 360f, 0f); // Quaternion.Inverse doesn't reverse y with 180 and 0 angles. Wth
			gum.transform.localScale = Vector3.zero;
			gum.gameObject.SetActive(true);
			gum.GetComponent<EmptyMonoBehaviour>().StartCoroutine(Timer(gum, 10f, ___ec));

			___rendererBase.gameObject.SetActive(false);

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
				Object.Destroy(target.gameObject);

				yield break;
			}
		}

		[HarmonyPatch(typeof(ITM_EmptyWaterBottle), "InteractWithFountain")] // Fountain animation
		[HarmonyPostfix]
		private static void AnimatedWaterFountain(WaterFountain fountain, PlayerManager ___pm) =>
				GenericAnimation.AnimateWaterFountain(fountain, ___pm);
	}
}
