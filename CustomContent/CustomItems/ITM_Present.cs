using BBTimes.CustomComponents;
using BBTimes.Extensions;
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

		public string Name { get; set; } public string TexturePath => this.GenerateDataPath("items", "Textures");
		public string SoundPath => this.GenerateDataPath("items", "Audios");
		public ItemObject ItmObj { get; set; }


		public override bool Use(PlayerManager pm)
		{
			Singleton<CoreGameManager>.Instance.audMan.PlaySingle(aud_unbox);
			pm.itm.SetItem(items[Random.Range(0, items.Length)], pm.itm.selectedItem);

			Destroy(gameObject);

			return false;
		}

		[SerializeField]
		internal SoundObject aud_unbox;

		[SerializeField]
		internal ItemObject[] items = [];
	}
}
