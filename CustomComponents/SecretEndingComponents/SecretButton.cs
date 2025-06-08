using BBTimes.ModPatches.EnvironmentPatches;
using UnityEngine;

namespace BBTimes.CustomComponents.SecretEndingComponents
{
	internal class SecretButton : MonoBehaviour, IClickable<int>, IItemAcceptor
	{
		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject audPress;

		[SerializeField]
		internal SpriteRenderer renderer;

		[SerializeField]
		internal Sprite sprPressed, sprReadyToPress;

		bool pressed = false, canBePressed = false;
		bool lastItemFitsReturn = false;

		public void Clicked(int player)
		{
			if (pressed || !canBePressed) return;

			renderer.sprite = sprPressed;
			audMan.PlaySingle(audPress);
			pressed = true;
			MainGameManagerPatches.allowEndingToBePlayed = true;
		}

		public bool ClickableHidden() => pressed || !canBePressed;
		public bool ClickableRequiresNormalHeight() => true;
		public void ClickableSighted(int player) { }
		public void ClickableUnsighted(int player) { }
		public bool ItemFits(Items itm)
		{
			lastItemFitsReturn = !canBePressed && itm == MTM101BaldAPI.EnumExtensions.GetFromExtendedName<Items>("SmallTimesKey");
			return lastItemFitsReturn;
		}
		public void InsertItem(PlayerManager pm, EnvironmentController ec)
		{
			if (!lastItemFitsReturn) // Avoiding I.O.U implementation lol
				return;
			canBePressed = true;
			renderer.sprite = sprReadyToPress;
		}
	}
}
