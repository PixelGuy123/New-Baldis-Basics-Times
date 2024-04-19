using System.Collections;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class Bell : Item, IEntityTrigger
	{
		public override bool Use(PlayerManager pm)
		{
			owner = pm.gameObject;
			pm.RuleBreak("littering", 5f, 0.8f);

			gameObject.SetActive(true);

			entity.Initialize(pm.ec, pm.transform.position);
			ec = pm.ec;

			return true;
		}

		public void EntityTriggerEnter(Collider other)
		{
			if (owner == other.gameObject || !active)
				return;

			active = false;
			renderer.sprite = deactiveSprite;
			ec.MakeNoise(transform.position, noiseVal);
			audMan.PlaySingle(audBell);

			StartCoroutine(WaitForDespawn());
		}

		public void EntityTriggerStay(Collider other)
		{

		}

		public void EntityTriggerExit(Collider other)
		{
			if (owner == other.gameObject)
				owner = null; // left owner's 
		}

		IEnumerator WaitForDespawn()
		{
			while (audMan.AnyAudioIsPlaying) yield return null;

			Destroy(gameObject);
			yield break;
		}

		bool active = true;
		GameObject owner;
		EnvironmentController ec;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal SoundObject audBell;

		[SerializeField]
		internal SpriteRenderer renderer;

		[SerializeField]
		internal Entity entity;

		[SerializeField]
		internal Sprite deactiveSprite;

		const int noiseVal = 112;
	}
}
