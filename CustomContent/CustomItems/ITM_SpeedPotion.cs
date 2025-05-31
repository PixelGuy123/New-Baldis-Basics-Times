using System.Collections;
using BBTimes.CustomComponents;
using BBTimes.Extensions;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.PlusExtensions;
using PixelInternalAPI.Components;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_SpeedPotion : Item, IItemPrefab
	{
		public void SetupPrefab()
		{
			audPower = this.GetSoundNoSub("potion_speedCoilNoises.wav", SoundType.Effect);
			audMan = gameObject.CreateAudioManager(75f, 75f)
				.MakeAudioManagerNonPositional();
			gaugeSprite = ItmObj.itemSpriteSmall;
		}
		public void SetupPrefabPost() { }

		public string Name { get; set; }
		public string Category => "items";

		public ItemObject ItmObj { get; set; }


		public override bool Use(PlayerManager pm)
		{
			if (usedPotions >= 2)
			{
				Destroy(gameObject);
				return false;
			}

			usedPotions++;

			this.pm = pm;
			pm.RuleBreak("Drinking", 1.5f);
			gauge = Singleton<CoreGameManager>.Instance.GetHud(pm.playerNumber).gaugeManager.ActivateNewGauge(gaugeSprite, lifeTime);
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
			float cooldown = lifeTime;
			while (cooldown > 0f)
			{
				cooldown -= pm.PlayerTimeScale * Time.deltaTime;
				gauge.SetValue(lifeTime, cooldown);
				yield return null;
			}

			gauge.Deactivate();

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
		internal float lifeTime = 12f;

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal Sprite gaugeSprite;

		HudGauge gauge;

		readonly ValueModifier speedMod = new(2f);

		readonly ValueModifier fovMod = new();

		const float smoothness = 3f;

		static int usedPotions = 0;
	}
}
