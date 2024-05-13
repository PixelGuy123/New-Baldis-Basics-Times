using BBTimes.Extensions;
using BBTimes.CustomComponents;
using System.Collections;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_HardHat : Item
	{
		public override bool Use(PlayerManager pm)
		{
			if (used)
			{
				Destroy(gameObject);
				return false;
			}
			this.pm = pm;
			gameObject.SetActive(true);
			used = true;

			canvas.gameObject.SetActive(true);
			canvas.worldCamera = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).canvasCam;

			StartCoroutine(Timer(pm.GetAttribute()));

			return true;
		}

		IEnumerator Timer(PlayerAttributesComponent comp)
		{
			comp.AddAttribute("protectedhead");
			float cooldown = 15f;
			while (cooldown > 0f)
			{
				cooldown -= pm.PlayerTimeScale * Time.deltaTime;
				yield return null;
			}
			comp.RemoveAttribute("protectedhead");
			Destroy(gameObject);
			yield break;
		}

		void OnDestroy() => used = false;

		[SerializeField]
		internal Canvas canvas;

		static bool used = false;
	}
}
