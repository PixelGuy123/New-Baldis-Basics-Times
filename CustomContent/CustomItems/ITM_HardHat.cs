using PixelInternalAPI.Components;
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

			StartCoroutine(Timer(pm.GetComponent<PlayerAttributesComponent>()));

			return true;
		}

		IEnumerator Timer(PlayerAttributesComponent comp)
		{
			comp.AddAttribute("protectedhead");
			float cooldown = 15f;
			while (cooldown > 0f)
			{
				cooldown -= pm.ec.EnvironmentTimeScale * Time.deltaTime;
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
