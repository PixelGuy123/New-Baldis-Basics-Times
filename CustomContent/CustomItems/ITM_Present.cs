using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_Present : Item
	{
		public override bool Use(PlayerManager pm)
		{
			gameObject.SetActive(true);
			Singleton<CoreGameManager>.Instance.audMan.PlaySingle(aud_unbox);
			pm.itm.SetItem(items[Random.Range(0, items.Length)], pm.itm.selectedItem);

			return false;
		}

		[SerializeField]
		internal SoundObject aud_unbox;

		[SerializeField]
		internal ItemObject[] items = [];
	}
}
