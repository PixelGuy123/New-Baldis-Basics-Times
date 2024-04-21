using PixelInternalAPI.Classes;
using PixelInternalAPI.Components;
using System.Collections;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_SpeedPotion : Item
	{
		public override bool Use(PlayerManager pm)
		{
			if (usedPotions >= 2)
			{
				Destroy(gameObject);
				return false;
			}

			usedPotions++;

			gameObject.SetActive(true);
			this.pm = pm;
			pm.RuleBreak("Drinking", 1.5f);
			StartCoroutine(Timer(pm.GetComponent<PlayerAttributesComponent>(), Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).GetComponent<CustomPlayerCameraComponent>()));

			return true;
		}

		IEnumerator Timer(PlayerAttributesComponent comp, CustomPlayerCameraComponent comp2)
		{
			audMan.PlaySingle(audDrink);
			var cor = comp2.SlideFOVAnimation(fovMod, -35f, smoothness);

			while (audMan.AnyAudioIsPlaying)
				yield return null;

			if (cor != null)
				comp2.StopCoroutine(cor);

			audMan.PlaySingle(audPower);

			cor = comp2.SlideFOVAnimation(fovMod, -fovMod.Mod + 45f, smoothness);

			comp.SpeedMods.Add(speedMod);
			float cooldown = 12f;
			while (cooldown > 0f)
			{
				cooldown -= pm.ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}

			comp.SpeedMods.Remove(speedMod);
			if (cor != null)
				comp2.StopCoroutine(cor);

			comp2.ResetSlideFOVAnimation(fovMod, smoothness);

			Destroy(gameObject);

			yield break;
		}

		void OnDestroy() =>
			usedPotions--;
		

		static internal SoundObject audDrink;

		[SerializeField]
		internal SoundObject audPower;

		[SerializeField]
		internal AudioManager audMan;

		readonly SpeedModifier speedMod = new(2f, 2f);

		readonly BaseModifier fovMod = new();

		const float smoothness = 3f;

		static int usedPotions = 0;
	}
}
