using System.Collections;
using UnityEngine;

namespace BBTimes.CustomComponents.SecretEndingComponents
{
	internal class FakeLever : EnvironmentObject, IClickable<int>, IItemAcceptor
	{
		public override void LoadingFinished()
		{
			base.LoadingFinished();
			baldi = FindObjectOfType<SecretBaldi>();
		}
		public void Clicked(int player)
		{
			if (!available || baldi.IsTalking || triggeredBaldiAnger) return;

			// Trigger Baldi ending
			Vector3 baldPos = Singleton<CoreGameManager>.Instance.GetPlayer(player).transform.position;
			baldPos.y = baldi.transform.position.y;
			baldi.transform.position = baldPos;
			triggeredBaldiAnger = true;

			baldi.TriggerEndSequence(Singleton<CoreGameManager>.Instance.GetPlayer(player));
			StartCoroutine(AnimatePlayerMovement(Singleton<CoreGameManager>.Instance.GetPlayer(player)));
		}

		IEnumerator AnimatePlayerMovement(PlayerManager pm)
		{
			float t = 0;
			Vector3 pos = pm.transform.position;
			Vector3 tarPos = transform.position - transform.forward * 8f + transform.right * 14f;
			while (true)
			{
				t += Time.deltaTime * 8.5f;
				if (t > 1f)
				{
					pm.transform.position = Vector3.Lerp(pos, tarPos, t);
					yield break;
				}
				pm.transform.position = Vector3.Lerp(pos, tarPos, t);
				yield return null;
			}
		}

		public bool ClickableHidden() => !available || baldi.IsTalking || triggeredBaldiAnger;
		public bool ClickableRequiresNormalHeight() => true;
		public void ClickableSighted(int player) { }
		public void ClickableUnsighted(int player) { }
		public bool ItemFits(Items itm) => !available && itm == MTM101BaldAPI.EnumExtensions.GetFromExtendedName<Items>("Screwdriver");
		public void InsertItem(PlayerManager pm, EnvironmentController ec)
		{
			

			available = true;
			renderer.sprite = sprUnscrewed;

			Vector3 baldPos = transform.position - transform.forward * 40f;
			baldPos.y = baldi.transform.position.y;
			baldi.transform.position = baldPos;

			baldi.TriggerWarningSequence(pm);
		}

		[SerializeField]
		internal Sprite sprUnscrewed;

		[SerializeField]
		internal SpriteRenderer renderer;

		SecretBaldi baldi;

		bool available = false, triggeredBaldiAnger = false;
	}
}
