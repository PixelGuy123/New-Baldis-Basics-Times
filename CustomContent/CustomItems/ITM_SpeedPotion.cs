using BBTimes.Extensions;
using PixelInternalAPI.Components;
using BBTimes.CustomComponents;
using System.Collections;
using UnityEngine;
using MTM101BaldAPI.PlusExtensions;
using MTM101BaldAPI.Components;
using PixelInternalAPI.Extensions;

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
			StartCoroutine(Timer(pm.GetMovementStatModifier(), pm.GetCustomCam()));

			return true;
		}

		IEnumerator Timer(PlayerMovementStatModifier comp, CustomPlayerCameraComponent comp2)
		{
			audMan.PlaySingle(audDrink);
			var cor = comp2.SlideFOVAnimation(fovMod, -35f, smoothness);

			while (audMan.AnyAudioIsPlaying)
				yield return null;

			if (cor != null)
				comp2.StopCoroutine(cor);

			audMan.PlaySingle(audPower);

			cor = comp2.SlideFOVAnimation(fovMod, -fovMod.addend + 45f, smoothness);

			comp.AddModifier("walkSpeed", speedMod);
			comp.AddModifier("runSpeed", speedMod);
			float cooldown = 12f;
			while (cooldown > 0f)
			{
				cooldown -= pm.PlayerTimeScale * Time.deltaTime;
				yield return null;
			}

			comp.RemoveModifier(speedMod);

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

		readonly ValueModifier speedMod = new(2f);

		readonly ValueModifier fovMod = new();

		const float smoothness = 3f;

		static int usedPotions = 0;
	}
}
