using System.Collections;
using UnityEngine;
using PixelInternalAPI.Extensions;
using System.Collections.Generic;
using BBTimes.Extensions;
using BBTimes.CustomComponents;
using PixelInternalAPI.Classes;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_Magnet : Item, IEntityTrigger, IItemPrefab
	{
		public void SetupPrefab()
		{
			gameObject.layer = LayerStorage.standardEntities;

			var storedSprites = this.GetSpriteSheet(2, 2, 35f, "magnet.png");
			renderer = ObjectCreationExtensions.CreateSpriteBillboard(storedSprites[0]);
			renderer.transform.SetParent(transform);
			renderer.name = "MagnetVisual";
			sprs = [.. storedSprites];

			audMan = gameObject.CreatePropagatedAudioManager(75f, 100f);
			audThrow = this.GetSoundNoSub("throw.wav", SoundType.Effect);

			entity = gameObject.CreateEntity(2f, 65f, renderer.transform);
		}
		public void SetupPrefabPost() { }

		public string Name { get; set; } public string TexturePath => this.GenerateDataPath("items", "Textures");
		public string SoundPath => this.GenerateDataPath("items", "Audios");
		public ItemObject ItmObj { get; set; }


		public override bool Use(PlayerManager pm)
		{
			if (++usedMagnets > 2)
			{
				Destroy(gameObject);
				return false;
			}
			pm.RuleBreak("littering", 2f, 0.8f);
			owner = pm.gameObject;
			Throw(pm.transform.position, Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward, pm.ec, 10f);
			return true;
		}

		public void Throw(Vector3 pos, Vector3 dir, EnvironmentController ec, float cooldown)
		{
			this.ec = ec;
			audMan.PlaySingle(audThrow);
			entity.Initialize(ec, pos);
			entity.AddForce(new(dir, 36f, -15f));
			this.cooldown = cooldown;

			StartCoroutine(ThrowAnimation());
		}

		IEnumerator ThrowAnimation()
		{
			float height = 5.5f;
			float time = 0f;

			while (true)
			{
				time += ec.EnvironmentTimeScale * Time.deltaTime * 2f;
				entity.SetHeight(height + GenericExtensions.QuadraticEquation(time, -0.5f, 1, 0));
				if (time >= 2f)
				{
					entity.SetHeight(height);
					yield break;
				}
				yield return null;
			}
		}

		void Update()
		{
			frame += ec.EnvironmentTimeScale * Time.deltaTime * 7f;
			frame %= sprs.Length;
			renderer.sprite = sprs[Mathf.FloorToInt(frame)];

			for (int i = 0; i < touchedEntities.Count; i++)
			{
				if (touchedEntities[i].Key)
				{
					var vec = transform.position - touchedEntities[i].Key.transform.position;
					touchedEntities[i].Value.movementAddend += vec.normalized * Mathf.Max(0.05f, maxForce - vec.magnitude);
					touchedEntities[i].Value.movementAddend.Limit(maxForce, maxForce, maxForce);
				}
				else
					touchedEntities.RemoveAt(i--);
			}

			cooldown -= ec.EnvironmentTimeScale * Time.deltaTime;
			if (cooldown < 0f)
				Destroy(gameObject);
		}

		void OnDestroy()
		{
			foreach (var e in touchedEntities)
				e.Key?.ExternalActivity.moveMods.Remove(e.Value);
			usedMagnets--;
		}

		public void EntityTriggerEnter(Collider other)
		{
			if (other.gameObject == owner) return;

			var e = other.GetComponent<Entity>();
			if (e)
			{
				var m = new MovementModifier(Vector3.zero, 0.3f);
				e.ExternalActivity.moveMods.Add(m);
				touchedEntities.Add(new(e, m));
			}
		}
		public void EntityTriggerStay(Collider other){}
		public void EntityTriggerExit(Collider other)
		{
			if (other.gameObject == owner) return;

			var e = other.GetComponent<Entity>();
			if (e)
			{
				var entity = touchedEntities.Find(x => x.Key == e);
				e.ExternalActivity.moveMods.Remove(entity.Value);
				touchedEntities.Remove(entity);
			}
		}

		float frame = 0f, cooldown = 10f;

		EnvironmentController ec;
		GameObject owner = null;
		static int usedMagnets = 0;

		readonly List<KeyValuePair<Entity, MovementModifier>> touchedEntities = [];

		[SerializeField]
		internal Entity entity;

		[SerializeField]
		internal Sprite[] sprs;

		[SerializeField]
		internal PropagatedAudioManager audMan;

		[SerializeField]
		internal SoundObject audThrow;

		[SerializeField]
		internal SpriteRenderer renderer;

		[SerializeField]
		internal float maxForce = 55f;

		
	}
}
