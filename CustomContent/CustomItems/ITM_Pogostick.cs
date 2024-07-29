using BBTimes.Manager;
using PixelInternalAPI.Extensions;
using System.Collections;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_Pogostick : Item
	{
		public override bool Use(PlayerManager pm)
		{
			if (usingPogo || pm.plm.Entity.Frozen)
			{
				Destroy(gameObject);
				return false;
			}
			this.pm = pm;
			usingPogo = true;

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
			Force force = new(pm.transform.forward, 45f, -2.5f);
			entity.AddForce(force);
			pm.plm.Entity.Override(overrider);
			overrider.SetFrozen(true);
			overrider.SetInteractionState(false);

			float time = 0f;

			while (true)
			{
				height = time.QuadraticEquation(-3f, 7f, 0f);
				time += pm.PlayerTimeScale * Time.deltaTime;

				if (height > maxHeight)
					height = maxHeight;

				if ((transform.position - pm.transform.position).magnitude > 5f)
				{
					height = 0f;
					break;
				}
					

				pm.Teleport(transform.position);

				if (height < 0f)
				{
					height = 0f;
					break;
				}

				entity.SetHeight(pm.plm.Entity.InternalHeight + height);

				yield return null;
			}

			overrider.SetInteractionState(true);
			overrider.SetFrozen(false);
			overrider.Release();

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

		readonly EntityOverrider overrider = new();

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
