using UnityEngine;

namespace BBTimes.CustomComponents.NpcSpecificComponents.EverettTreewood
{
	using EverettTreewood = CustomContent.NPCs.EverettTreewood;

	public class ChristmasDecoration : MonoBehaviour
	{
		public void LinkToEverettTreewood(EverettTreewood wood)
		{
			this.wood = wood;
			initialized = true;
		}

		void OnTriggerEnter(Collider other)
		{
			if (!initialized || IsBroken || wood.gameObject == other.gameObject)
				return;

			if (other.isTrigger && (other.CompareTag("NPC") || other.CompareTag("Player")))
			{
				renderer.sprite = sprBroken;
				audMan.PlaySingle(audBreak);
				collider.enabled = false;
				IsBroken = true;
				wood.QueueDecor(this);
			}
		}

		[SerializeField]
		internal SpriteRenderer renderer;

		[SerializeField]
		internal Sprite sprBroken;

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject audBreak;

		[SerializeField]
		internal Collider collider;

		public bool IsBroken { get; private set; }
		bool initialized = false;

		EverettTreewood wood;
	}
}
