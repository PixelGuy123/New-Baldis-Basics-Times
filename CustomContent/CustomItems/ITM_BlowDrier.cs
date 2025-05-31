using System.Collections;
using BBTimes.CustomComponents;
using BBTimes.Extensions;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_BlowDrier : Item, IItemPrefab
	{
		public void SetupPrefab()
		{
			audMan = gameObject.CreateAudioManager(45, 65).MakeAudioManagerNonPositional();
			audBlow = this.GetSoundNoSub("blowDrier.wav", SoundType.Effect);
			gaugeSprite = ItmObj.itemSpriteSmall;
		}
		public void SetupPrefabPost() { }

		public string Name { get; set; }
		public string Category => "items";

		public ItemObject ItmObj { get; set; }


		public override bool Use(PlayerManager pm)
		{
			if (++blowersUsed > 1)
			{
				Destroy(gameObject);
				return false;
			}
			this.pm = pm;
			audMan.maintainLoop = true;
			audMan.QueueAudio(audBlow);
			audMan.SetLoop(true);

			StartCoroutine(Blow());
			return true;
		}

		void OnDestroy() =>
			blowersUsed--;

		IEnumerator Blow()
		{
			float timer = Random.Range(minLifeTime, maxLifeTime), ogTimer = timer;
			gauge = Singleton<CoreGameManager>.Instance.GetHud(pm.playerNumber).gaugeManager.ActivateNewGauge(gaugeSprite, ogTimer);

			float speed = 0f;
			MovementModifier moveMod = new(Vector3.zero, 0.85f) { forceTrigger = true };
			pm.Am.moveMods.Add(moveMod);
			var cam = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber);

			while (timer > 0f)
			{
				timer -= pm.ec.EnvironmentTimeScale * Time.deltaTime;
				gauge.SetValue(ogTimer, timer);
				speed += Time.deltaTime * pm.ec.EnvironmentTimeScale * 1.2f;
				if (speed > maxSpeed)
					speed = maxSpeed;
				moveMod.movementAddend += -cam.transform.forward * speed;
				moveMod.movementAddend.Limit(maxSpeed, maxSpeed, maxSpeed);
				yield return null;
			}
			gauge.Deactivate();

			pm.Am.moveMods.Remove(moveMod);
			Destroy(gameObject);
			yield break;
		}

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject audBlow;

		[SerializeField]
		internal Sprite gaugeSprite;

		[SerializeField]
		internal float minLifeTime = 15f, maxLifeTime = 30f;

		HudGauge gauge;

		const float maxSpeed = 45f;

		internal static int blowersUsed = 0;
	}
}
