using System.Collections;
using BBTimes.CustomComponents;
using BBTimes.Extensions;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_InvisibilityController : Item, IItemPrefab
	{
		public void SetupPrefab()
		{
			audUse = this.GetSound("longHighBeep.wav", "InvCon_Active", SoundType.Effect, Color.white);
			audDeuse = this.GetSound("longDownBeep.wav", "InvCon_Deactive", SoundType.Effect, Color.white);
		}
		public void SetupPrefabPost() { }

		public string Name { get; set; }
		public string Category => "items";

		public ItemObject ItmObj { get; set; }


		public override bool Use(PlayerManager pm)
		{
			if (use)
			{
				Destroy(gameObject);
				return false;
			}
			use = true;
			this.pm = pm;
			Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audUse);
			pm.plm.Entity.SetVisible(true);

			StartCoroutine(Timer());

			return true;
		}

		void OnDestroy() => use = false;

		IEnumerator Timer()
		{
			float cooldown = 10f;
			while (cooldown > 0f)
			{
				cooldown -= pm.PlayerTimeScale * Time.deltaTime;
				yield return null;
			}
			Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audDeuse);
			pm.plm.Entity.SetVisible(false);
			Destroy(gameObject);
			yield break;
		}

		[SerializeField]
		internal SoundObject audUse, audDeuse;

		static bool use = false;
	}
}
