using System.Collections;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class Present : Item
	{
		public override bool Use(PlayerManager pm)
		{
			gameObject.SetActive(true);
			audMan.QueueAudio(aud_unbox);

			StartCoroutine(WaitForAudio());

			pm.itm.SetItem(items[Random.Range(0, items.Length)], pm.itm.selectedItem);

			return false;
		}

		IEnumerator WaitForAudio()
		{
			while (audMan.AnyAudioIsPlaying)
				yield return null;
			Destroy(gameObject);

			yield break;
		}

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject aud_unbox;

		[SerializeField]
		internal ItemObject[] items = [];
	}
}
