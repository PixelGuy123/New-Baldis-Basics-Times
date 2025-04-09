using BBTimes.CustomComponents;
using BBTimes.Extensions;
using BBTimes.Manager;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_ThrowableTeleporter : Item, IItemPrefab, IEntityTrigger
	{
		public void SetupPrefab()
		{
			gameObject.layer = LayerStorage.standardEntities;

			throwAnimation = this.GetSpriteSheet(3, 2, 50f, "telepWorld.png");

			renderer = ObjectCreationExtensions.CreateSpriteBillboard(throwAnimation[0]);
			renderer.transform.SetParent(transform);
			renderer.name = "ThrowableTeleporterVisual";

			audMan = gameObject.CreatePropagatedAudioManager(85f, 115f);
			audThrow = this.GetSoundNoSub("throw.wav", SoundType.Effect);
			audTeleport = BBTimesManager.man.Get<SoundObject>("teleportAud");

			entity = gameObject.CreateEntity(2f, 2f, renderer.transform);
		}
		public void SetupPrefabPost() { }

		public string Name { get; set; } public string Category => "items";
		
		public ItemObject ItmObj { get; set; }


		public override bool Use(PlayerManager pm)
		{
			pm.RuleBreak("littering", 2f, 0.8f);
			Throw(pm.transform.position, Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward, pm.ec);
			return true;
		}

		public void Throw(Vector3 pos, Vector3 dir, EnvironmentController ec)
		{
			this.ec = ec;
			audMan.PlaySingle(audThrow);
			entity.Initialize(ec, pos);
			entity.AddForce(new(dir, 45f, -25f));

			StartCoroutine(ThrowAnimation());
		}

		IEnumerator ThrowAnimation()
		{
			float height = 1.2f, time = 0f;

			while (true)
			{
				time += ec.EnvironmentTimeScale * Time.deltaTime * 2.5f;
				entity.SetHeight(height + GenericExtensions.QuadraticEquation(time, -0.5f, 1, 0));
				renderer.sprite = throwAnimation[
					Mathf.FloorToInt(
						Mathf.Lerp(0f, throwAnimation.Length - 1, time * 0.5f)
						)
					];
				if (time >= 2f)
				{
					renderer.sprite = throwAnimation[throwAnimation.Length - 1];
					entity.SetHeight(height);
					break;
				}
				yield return null;
			}

			canTeleport = true;
		}

		public void EntityTriggerEnter(Collider other) { }

		public void EntityTriggerStay(Collider other) 
		{ 
			if (canTeleport && other.isTrigger && (other.CompareTag("Player") || other.CompareTag("NPC")) && other.TryGetComponent<Entity>(out var e))
			{
				TeleportEntity(e);
			}
		}
		public void EntityTriggerExit(Collider other) { }

		void TeleportEntity(Entity e)
		{
			DijkstraMap map = new(ec, PathType.Const, transform);
			map.Calculate();

			List<Cell> spots = ec.AllTilesNoGarbage(false, false);
			spots.ConvertEntityUnsafeCells();

			for (int i = 0; i < spots.Count; i++)
			{
				if (map.Value(spots[i].position) < minDistanceFromTeleporter)
					spots.RemoveAt(i--);
			}

			if (spots.Count != 0)
			{
				e.Teleport(spots[Random.Range(0, spots.Count)].FloorWorldPosition);
				audMan.PlaySingle(audTeleport);
			}

			StartCoroutine(DespawnAnimation());
		}

		IEnumerator DespawnAnimation()
		{
			canTeleport = false;
			float height = entity.BaseHeight;
			while (true)
			{
				height -= ec.EnvironmentTimeScale * Time.deltaTime * despawnSpeed;
				entity.SetHeight(height);
				if (height < -5f)
				{
					break;
				}
				yield return null;
			}

			while (audMan.AnyAudioIsPlaying)
				yield return null;

			Destroy(gameObject);
		}

		EnvironmentController ec;

		[SerializeField]
		internal Entity entity;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal SoundObject audThrow, audTeleport;

		[SerializeField]
		internal SpriteRenderer renderer;

		[SerializeField]
		internal Sprite[] throwAnimation;

		[SerializeField]
		internal float maxForce = 55f, despawnSpeed = 5f;

		[SerializeField]
		internal int minDistanceFromTeleporter = 15;

		bool canTeleport = false;


	}
}
