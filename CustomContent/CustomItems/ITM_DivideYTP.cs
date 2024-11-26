using BBTimes.CustomComponents;
using BBTimes.Extensions;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_DivideYTP : Item, IItemPrefab
	{
		public void SetupPrefab() =>
			ItmObj.audPickupOverride = this.GetSoundNoSub("audYtpPickup.wav", SoundType.Effect);
		
		public void SetupPrefabPost() { }

		public string Name { get; set; } public string TexturePath => this.GenerateDataPath("items", "Textures");
		public string SoundPath => this.GenerateDataPath("items", "Audios");
		public ItemObject ItmObj { get; set; }

		public override bool Use(PlayerManager pm)
		{
			Singleton<CoreGameManager>.Instance.AddPoints(100 + Mathf.Abs(Singleton<CoreGameManager>.Instance.GetPointsThisLevel(pm.playerNumber)) / Random.Range(minDivider, maxDivider), pm.playerNumber, true);
			Destroy(gameObject);
			return true;
		}

		[SerializeField]
		[Range(1, int.MaxValue)]
		internal int minDivider = 2;

		[SerializeField]
		[Range(1, int.MaxValue)]
		internal int maxDivider = 4;
	}
}
