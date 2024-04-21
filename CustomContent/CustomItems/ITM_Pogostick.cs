using PixelInternalAPI.Extensions;
using System.Collections;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_Pogostick : Item
	{
		public override bool Use(PlayerManager pm)
		{
			if (usingPogo)
			{
				Destroy(gameObject);
				return false;
			}
			this.pm = pm;
			usingPogo = true;
			gameObject.SetActive(true);

			Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audBoing);
			entity.Initialize(pm.ec, pm.transform.position);
			transform.rotation = pm.cameraBase.rotation;

			Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).UpdateTargets(rendererBase, targetIdx);

			StartCoroutine(AnimatedJump());

			if (pogoStickReplacement != null)
			{
				pm.itm.SetItem(pogoStickReplacement, pm.itm.selectedItem);
				return false;
			}


			return true;
		}

		IEnumerator AnimatedJump()
		{
			Force force = new(pm.transform.forward, 28f, -2.5f);
			entity.AddForce(force);
			pm.Hide(true);

			float time = 0f;

			while (true)
			{
				height = time.QuadraticEquation(-3f, 7f, 0f);
				time += Time.deltaTime;

				if (height > maxHeight)
					height = maxHeight;

				if (height < 0f)
				{
					height = 0f;
					break;
				}

				entity.SetHeight(pm.plm.Entity.Height + height);

				yield return null;
			}

			pm.Hide(false);
			pm.Teleport(transform.position);
			Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).UpdateTargets(null, targetIdx);

			Destroy(gameObject);

			yield break;
		}

		void Update()
		{
			transform.rotation = pm.cameraBase.rotation;
		}

		void OnDestroy() => usingPogo = false;

		static bool usingPogo = false;

		float height = 0f;

		const int targetIdx = 15;

		const float maxHeight = 4.7f;

		[SerializeField]
		internal ItemObject pogoStickReplacement;

		[SerializeField]
		internal SoundObject audBoing;

		[SerializeField]
		internal Entity entity;

		[SerializeField]
		internal Transform rendererBase;
	}
}
