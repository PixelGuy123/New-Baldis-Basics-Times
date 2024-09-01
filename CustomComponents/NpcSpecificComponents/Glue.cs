using System.Collections;
using UnityEngine;

namespace BBTimes.CustomComponents.NpcSpecificComponents
{
	public class Glue : GlueObject
	{
		protected override void ActivityEnter(ActivityModifier actMod)
		{
			base.ActivityEnter(actMod);
			audMan.PlaySingle(audSteppedOn);
		}

		protected override void Initialize()
		{
			base.Initialize();
			StartCoroutine(Spawn());
		}
		IEnumerator Spawn()
		{
			float sizeMult = 0f;
			Vector3 ogSize = render.localScale;
			while (true)
			{
				sizeMult += ec.EnvironmentTimeScale * Time.deltaTime * 3f;
				if (sizeMult >= 1f)
					break;
				render.localScale = ogSize * sizeMult;
				yield return null;
			}
			render.localScale = ogSize;
			yield break;
		}

		protected override void VirtualUpdate()
		{
			base.VirtualUpdate();
			if (despawning)
				return;

			cooldown -= ec.EnvironmentTimeScale * Time.deltaTime;
			if (cooldown < 0f)
			{
				despawning = true;
				StartCoroutine(Despawn());
			}
		}

		IEnumerator Despawn()
		{
			float sizeMult = 1f;
			Vector3 ogSize = render.localScale;
			while (true)
			{
				sizeMult -= ec.EnvironmentTimeScale * Time.deltaTime * 3f;
				if (sizeMult <= 0.1f)
					break;
				render.localScale = ogSize * sizeMult;
				yield return null;
			}
			render.localScale = ogSize * sizeMult;

			Destroy(gameObject);
			yield break;
		}

		float cooldown = Random.Range(20f, 35f);

		bool despawning = false;

		[SerializeField]
		internal Transform render;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal SoundObject audSteppedOn;
	}
}
