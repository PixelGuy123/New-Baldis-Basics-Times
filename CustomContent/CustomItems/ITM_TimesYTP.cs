using BBTimes.CustomComponents;
using BBTimes.Extensions;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_TimesYTP : Item, IItemPrefab
	{
		public void SetupPrefab() =>
			ItmObj.audPickupOverride = this.GetSoundNoSub("audYtpPickup.wav", SoundType.Effect);

		public void SetupPrefabPost() { }

		public string Name { get; set; } public string TexturePath => this.GenerateDataPath("items", "Textures");
		public string SoundPath => this.GenerateDataPath("items", "Audios");
		public ItemObject ItmObj { get; set; }
		public override bool Use(PlayerManager pm)
		{
			Singleton<CoreGameManager>.Instance.AddPoints(Mathf.Abs(Singleton<CoreGameManager>.Instance.GetPointsThisLevel(pm.playerNumber)) * multiplier, pm.playerNumber, true);
			Destroy(gameObject);
			return true;
		}

		[SerializeField]
		[Range(1, int.MaxValue)]
		internal int multiplier = 1;
	}
}
