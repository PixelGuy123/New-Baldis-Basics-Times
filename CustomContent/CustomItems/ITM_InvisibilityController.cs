using System.Collections;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_InvisibilityController : Item
	{
		public override bool Use(PlayerManager pm)
		{
			if (use)
			{
				Destroy(gameObject);
				return false;
			}
			use = true;
			gameObject.SetActive(true);
			this.pm = pm;
			Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audUse);
			pm.SetInvisible(true);

			StartCoroutine(Timer());

			return true;
		}

		void OnDestroy() => use = false;

		IEnumerator Timer()
		{
			float cooldown = 10f;
			while (cooldown > 0f)
			{
				cooldown -= pm.ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}
			Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audDeuse);
			pm.SetInvisible(false);
			Destroy(gameObject);
			yield break;
		}

		[SerializeField]
		internal SoundObject audUse, audDeuse;

		static bool use = false;
	}
}
