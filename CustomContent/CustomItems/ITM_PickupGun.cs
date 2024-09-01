using BBTimes.CustomComponents;
using PixelInternalAPI.Extensions;
using UnityEngine;
using BBTimes.Extensions;
using System.Collections.Generic;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_PickupGun : Item, IItemPrefab
	{

		public void SetupPrefab()
		{
			var renderer = ObjectCreationExtensions.CreateSpriteBillboard(this.GetSprite(55f, "grabber.png"));
			var rendererBase = renderer.transform;
			rendererBase.SetParent(transform);
			rendererBase.localPosition = Vector3.zero;
			gameObject.layer = 0; // Default layer, should touch pickups now
			entity = gameObject.CreateEntity(1.5f, 3f, rendererBase);

			audMan = gameObject.CreatePropagatedAudioManager(95, 125);
			var grap = Resources.FindObjectsOfTypeAll<ITM_GrapplingHook>()[0];
			lineRenderer = grap.lineRenderer.SafeInstantiate();
			lineRenderer.transform.SetParent(transform);
			lineRenderer.transform.localPosition = Vector3.zero;
			lineRenderer.gameObject.SetActive(true);

			audShoot = grap.audLaunch;
			audBreak = grap.audSnap;
			entity.collisionLayerMask = grap.entity.collisionLayerMask;
		}

		public override bool Use(PlayerManager pm)
		{
			this.pm = pm;
			ec = pm.ec;
			entity.Initialize(pm.ec, pm.transform.position);
			dir = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward;
			entity.OnEntityMoveInitialCollision += hit => { Destroy(gameObject); audMan.PlaySingle(audBreak); };
			audMan.PlaySingle(audShoot);
			return true;
		}

		public void SetupPrefabPost() { }

		public string Name { get; set; }
		public string TexturePath => this.GenerateDataPath("items", "Textures");
		public string SoundPath => this.GenerateDataPath("items", "Audios");
		public ItemObject ItmObj { get; set; }

		void LateUpdate()
		{
			positions[0] = transform.position;
			positions[1] = pm.transform.position - Vector3.up * 1f;
			lineRenderer.SetPositions(positions);
		}

		void Update()
		{
			if (goBack)
			{
				if (!pm)
				{
					Destroy(gameObject);
					return;
				}

				entity.UpdateInternalMovement(Vector3.zero);
				var dist = pm.transform.position - transform.position;
				entity.Teleport(transform.position + dist * 29f * Time.deltaTime * ec.EnvironmentTimeScale);
				if (dist.magnitude < 5f)
					Destroy(gameObject);
				return;
			}
			entity.UpdateInternalMovement(dir * 95f * ec.EnvironmentTimeScale);
			if (pm)
			{
				ray.origin = transform.position;
				ray.direction = dir;

				if (Physics.Raycast(ray, out var hit, 2f))
				{
					var comp = hit.transform.GetComponent<IClickable<int>>();
					if (comp != null && allowedClickables.Contains(comp.GetType()))
					{
						comp.Clicked(pm.playerNumber);
						goBack = true;
					}
				}
			}
		}



		[SerializeField]
		internal Entity entity;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal SoundObject audShoot, audBreak;

		[SerializeField]
		internal LineRenderer lineRenderer;

		Vector3[] positions = new Vector3[2];
		Vector3 dir;
		Ray ray = new();
		EnvironmentController ec;
		bool goBack = false;

		readonly HashSet<System.Type> allowedClickables = [typeof(Pickup), typeof(Notebook)];
	}
}
