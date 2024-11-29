using TMPro;
using UnityEngine;

namespace BBTimes.CustomContent.Objects
{
	public class ItemDescriptor : EnvironmentObject, IClickable<int>
	{
		public void Clicked(int player)
		{
			var item = Singleton<CoreGameManager>.Instance.GetPlayer(player)
				.itm.items[Singleton<CoreGameManager>.Instance.GetPlayer(player).itm.selectedItem];

			if (item.itemType == Items.None)
				return;

			string descKey = Singleton<LocalizationManager>.Instance.GetLocalizedText(item.descKey);
			text.text = string.IsNullOrEmpty(descKey) ?
				Singleton<LocalizationManager>.Instance.GetLocalizedText("PST_ItemDescriptor_NoDescription") : descKey;

			audMan.PlaySingle(audTap);
		}
		public void ClickableSighted(int player) { }
		public void ClickableUnsighted(int player) { }
		public bool ClickableHidden() => false;
		public bool ClickableRequiresNormalHeight() => false;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal SoundObject audTap;

		[SerializeField]
		internal TextMeshPro text;
	}
}
