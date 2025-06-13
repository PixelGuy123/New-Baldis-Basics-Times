using System.Collections;
using System.Collections.Generic;
using BBTimes.CustomComponents;
using BBTimes.Extensions;
using BBTimes.Manager;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_Pogostick : Item, IItemPrefab, IEntityTrigger
	{
		public void SetupPrefab()
		{
			gameObject.layer = LayerStorage.standardEntities;

			audBoing = this.GetSound("boing.wav", "POGST_Boing", SoundType.Effect, Color.white);
			audHit = BBTimesManager.man.Get<SoundObject>("audGenericPunch");
			var falseRenderer = new GameObject("PogoStickRendererBase");
			falseRenderer.transform.SetParent(transform);
			falseRenderer.transform.localPosition = Vector3.zero;


			entity = gameObject.CreateEntity(2f, 2f, rendererBase: falseRenderer.transform);
			entity.SetGrounded(false);
			((CapsuleCollider)entity.collider).height = 10f;

			rendererBase = falseRenderer.transform;
		}
		public void SetupPrefabPost() { }

		public string Name { get; set; }
		public string Category => "items";

		public ItemObject ItmObj { get; set; }


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
			forcesApplied.Add(force);
			pm.plm.Entity.Override(overrider);
			overrider.SetFrozen(true);
			overrider.SetInteractionState(false);


			while (true)
			{
				yVelocity -= pm.PlayerTimeScale * Time.deltaTime * 5.5f;
				height += yVelocity * Time.deltaTime * 0.35f;

				if (height > maxHeight)
				{
					height = maxHeight;
					yVelocity = 0f;
				}

				if ((transform.position - pm.transform.position).magnitude > 5f)
				{
					height = Entity.physicalHeight;
					break;
				}


				pm.Teleport(transform.position);

				if (height < Entity.physicalHeight)
				{
					height = Entity.physicalHeight;
					break;
				}

				entity.SetHeight(height);

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

		public void EntityTriggerEnter(Collider other)
		{
			if (pm.gameObject == other.gameObject) return;

			if (other.isTrigger)
			{
				var e = other.GetComponent<Entity>();
				if (e && !e.Squished)
				{
					Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audHit);
					Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audBoing);
					Vector3 dir = pm.transform.forward;
					Force f = new(dir, 25f / forcesApplied.Count * 0.5f, -3f); // When more forces were applied, more weaker will be the force
					entity.AddForce(f);
					forcesApplied.ForEach(x => x.direction = new(dir.x, dir.z)); // Updates direction

					forcesApplied.Add(f);

					e.Squish(15f);
					if (yVelocity < 0f)
						yVelocity = 0f;
					yVelocity += 10f;

				}
			}
		}

		public void EntityTriggerStay(Collider other) { }

		public void EntityTriggerExit(Collider other) { }

		void OnDestroy() => usingPogo = false;

		static bool usingPogo = false;

		float height = Entity.physicalHeight, yVelocity = 10f;

		const int targetIdx = 15;

		const float maxHeight = 9.5f;

		readonly EntityOverrider overrider = new();

		readonly List<Force> forcesApplied = [];

		[SerializeField]
		internal ItemObject pogoStickReplacement;

		[SerializeField]
		internal SoundObject audBoing, audHit;

		[SerializeField]
		internal Entity entity;

		[SerializeField]
		internal Transform rendererBase;
	}
}
