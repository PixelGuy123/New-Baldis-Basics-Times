namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_WaterBottle : Item
	{
		public override bool Use(PlayerManager pm)
		{
			Destroy(gameObject);
			pm.plm.AddStamina(pm.plm.staminaMax, true);
			Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audDrink);
			pm.itm.SetItem(emptyBottle, pm.itm.selectedItem);
			return false;
		}

		internal static ItemObject emptyBottle;

		internal static SoundObject audDrink;
	}
}
