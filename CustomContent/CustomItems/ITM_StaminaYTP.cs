using BBTimes.CustomComponents;
using BBTimes.Extensions;
using BBTimes.Manager;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_StaminaYTP : Item, IItemPrefab
	{
		public void SetupPrefab() =>
			ItmObj.audPickupOverride = BBTimesManager.man.Get<SoundObject>("audGenericStaminaYTPGrab");

		public void SetupPrefabPost() { }

		public string Name { get; set; } public string Category => "items";
		
		public ItemObject ItmObj { get; set; }
		public override bool Use(PlayerManager pm)
		{
			Singleton<CoreGameManager>.Instance.AddPoints(points, pm.playerNumber, true);
			pm.plm.stamina = staminaGain;
			Destroy(gameObject);
			return true;
		}

		[SerializeField]
		internal int points = 45;

		[SerializeField]
		internal float staminaGain = 100f;
	}
}
