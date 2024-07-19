using BBTimes.CustomContent.RoomFunctions;
using System.Collections;
using UnityEngine;

namespace BBTimes.CustomContent.Objects
{
	public class LightSwitch : EnvironmentObject, IClickable<int>
	{
		public void Initialize(LightSwitchSpawner sp)
		{
			functional = true;
			mySpawner = sp;
		}

		public void Clicked(int player)
		{
			if (functional && cooldownToUseAgain <= 0f && disables == 0)
			{
				Switch(false);
				StartCoroutine(SwitchCooldown());
			}
		}
		public void ClickableSighted(int player) { }
		public void ClickableUnsighted(int player) { }
		public bool ClickableHidden() => !functional || cooldownToUseAgain > 0f || disables > 0;
		public bool ClickableRequiresNormalHeight() => true;

		IEnumerator SwitchCooldown()
		{
			cooldownToUseAgain = 60f;
			float cooldown = Random.Range(15f, 20f);
			while (cooldown > 0f)
			{
				cooldown -= ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}
			Switch(true);
			while (cooldownToUseAgain > 0f)
			{
				cooldownToUseAgain -= ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}
			yield break;
		}

		void Switch(bool on)
		{
			if (mySpawner.IsRoomOn == on) return;

			mySpawner.TurnRoom(on);
			audMan.PlaySingle(audSwitch);
			renderer.sprite = on ? sprOn : sprOff;
		}

		public void DisableMe(bool disable)
		{
			if (disable)
			{
				disables++;
				Switch(true);
			}
			else
				disables--;
			disables = Mathf.Max(0, disables);
		}

		float cooldownToUseAgain = 0f;

		int disables = 0;

		bool functional = false;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal SoundObject audSwitch;

		[SerializeField]
		internal Sprite sprOn, sprOff;

		[SerializeField]
		internal SpriteRenderer renderer;

		LightSwitchSpawner mySpawner;
	}
}
