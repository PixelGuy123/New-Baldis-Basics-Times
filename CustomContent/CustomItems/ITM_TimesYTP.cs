using BBTimes.CustomComponents;
using BBTimes.Extensions;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_TimesYTP : Item, IItemPrefab
	{
		public void SetupPrefab() =>
			ItmObj.audPickupOverride = this.GetSoundNoSub("audTimesYtpPickup.wav", SoundType.Effect);

		public void SetupPrefabPost() { }

		public string Name { get; set; } public string Category => "items";
		
		public ItemObject ItmObj { get; set; }
		public override bool Use(PlayerManager pm)
		{
			Singleton<CoreGameManager>.Instance.AddPoints(100 + Mathf.FloorToInt(Mathf.Abs(Singleton<CoreGameManager>.Instance.GetPointsThisLevel(pm.playerNumber)) * multiplier), pm.playerNumber, true);
			Destroy(gameObject);
			return true;
		}

		[SerializeField]
		internal float multiplier = 1.5f;
	}
}
