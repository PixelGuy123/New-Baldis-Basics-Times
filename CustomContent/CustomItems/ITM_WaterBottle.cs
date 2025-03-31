using UnityEngine;
using BBTimes.CustomComponents;
using BBTimes.Manager;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_WaterBottle : Item, IItemPrefab
	{
		public void SetupPrefab()
		{
			audDrink = BBTimesManager.man.Get<SoundObject>("audRobloxDrink");
		}
		public void SetupPrefabPost() { }

		public string Name { get; set; }
		public string Category => "items";

		public ItemObject ItmObj { get; set; }

		public override bool Use(PlayerManager pm)
		{
			Destroy(gameObject);
			if (waterBottle) // If water bottle reference, this is an empty bottle
			{
				if (Physics.Raycast(pm.transform.position, Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward, out var hit, pm.pc.reach))
				{
					var water = hit.transform.GetComponent<WaterFountain>();
					if (water)
					{
						this.pm = pm;
						InteractWithFountain(water);
						pm.itm.SetItem(waterBottle, pm.itm.selectedItem);
					}
				}
				return false;
			}
			
			pm.plm.AddStamina(pm.plm.staminaMax, true);
			Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audDrink);
			pm.itm.SetItem(emptyBottle, pm.itm.selectedItem);
			return false;
		}

		void InteractWithFountain(WaterFountain fountain)
		{
			fountain.audMan.PlaySingle(fountain.audSip);
		} // will be used to be patched by BB+ Animations aswell

		[SerializeField]
		internal ItemObject emptyBottle, waterBottle = null;

		[SerializeField]
		internal SoundObject audDrink;
	}
}
