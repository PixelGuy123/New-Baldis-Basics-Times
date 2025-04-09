using BBTimes.CustomComponents;
using BBTimes.Extensions;
using System.Collections;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_Present : Item, IItemPrefab
	{
		public void SetupPrefab()
		{
			aud_unbox = this.GetSound("prs_unbox.wav", "Vfx_PRS_Unbox", SoundType.Effect, Color.white);
		}

		public void SetupPrefabPost()
		{
			var shops = GameExtensions.GetAllShoppingItems();
			shops.RemoveAll(x => x.itemType == ItmObj.itemType);

			items = [.. shops];
		}

		public string Name { get; set; } public string Category => "items";
		
		public ItemObject ItmObj { get; set; }


		public override bool Use(PlayerManager pm)
		{
			this.pm = pm;
			Singleton<CoreGameManager>.Instance.audMan.PlaySingle(aud_unbox);

			StartCoroutine(GiveItem());
			return true;
		}

		IEnumerator GiveItem()
		{
			yield return null;
			pm.itm.AddItem(items[Random.Range(0, items.Length)]);
			Destroy(gameObject);
		}

		[SerializeField]
		internal SoundObject aud_unbox;

		[SerializeField]
		internal ItemObject[] items = [];
	}
}
