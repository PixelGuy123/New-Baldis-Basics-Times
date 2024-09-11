using BBTimes.CustomComponents;
using BBTimes.Extensions;
using MTM101BaldAPI.Registers;
using System.Collections;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_GoldenQuarter : Item, IItemPrefab
	{
		public void SetupPrefab() =>
			quarter = ItemMetaStorage.Instance.FindByEnum(Items.Quarter).value;
		
		public void SetupPrefabPost() { }
		public string Name { get; set; }
		public string TexturePath => this.GenerateDataPath("items", "Textures");
		public string SoundPath => this.GenerateDataPath("items", "Audios");
		public ItemObject ItmObj { get; set; }

		public override bool Use(PlayerManager pm)
		{
			
			bool nextNull = nextGoldQuart != null;
			if (!nextNull || !pm.itm.InventoryFull())
			{
				StartCoroutine(Delay(pm));
				if (nextNull)
				{
					pm.itm.SetItem(nextGoldQuart, pm.itm.selectedItem);
					return false;
				}
				return true;
			}

			Destroy(gameObject);

			return false;
		}

		IEnumerator Delay(PlayerManager pm)
		{
			yield return null;
			yield return null;
			pm.itm.AddItem(quarter);
			Destroy(gameObject);
			yield break;
		}

		[SerializeField]
		internal ItemObject quarter;

		[SerializeField]
		internal ItemObject nextGoldQuart;
	}
}
