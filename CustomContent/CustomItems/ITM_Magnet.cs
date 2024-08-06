using System.Collections;
using UnityEngine;
using PixelInternalAPI.Extensions;
using System.Collections.Generic;
using BBTimes.Extensions;

namespace BBTimes.CustomContent.CustomItems
{
	public class ITM_Magnet : Item, IEntityTrigger
	{
		public override bool Use(PlayerManager pm)
		{
			Throw(pm.transform.position, Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward, pm.ec, 10f);
			return true;
		}

		public void Throw(Vector3 pos, Vector3 dir, EnvironmentController ec, float cooldown)
		{
			this.ec = ec;
			audMan.PlaySingle(audThrow);
			entity.Initialize(ec, pos);
			entity.AddForce(new(dir, 14f, -12f));
			this.cooldown = cooldown;

			StartCoroutine(ThrowAnimation());
		}

		IEnumerator ThrowAnimation()
		{
			float height = entity.InternalHeight;
			float time = 0f;

			while (true)
			{
				time += ec.EnvironmentTimeScale * Time.deltaTime;
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

			foreach (var e in touchedEntities)
			{
				var vec = transform.position - e.Key.transform.position;
				e.Value.movementAddend += vec * Mathf.Max(1, maxForce - vec.magnitude);
				e.Value.movementAddend.Limit(maxForce, maxForce, maxForce);
			}

			cooldown -= ec.EnvironmentTimeScale * Time.deltaTime;
			if (cooldown < 0f)
				Destroy(gameObject);
		}

		void OnDestroy()
		{
			foreach (var e in touchedEntities)
				e.Key.ExternalActivity.moveMods.Remove(e.Value);
		}

		public void EntityTriggerEnter(Collider other)
		{
			if (other.gameObject == pm.gameObject) return;

			var e = other.GetComponent<Entity>();
			if (e)
			{
				var m = new MovementModifier(Vector3.zero, 0.3f);
				e.ExternalActivity.moveMods.Add(m);
				touchedEntities.Add(e, m);
			}
		}
		public void EntityTriggerStay(Collider other){}
		public void EntityTriggerExit(Collider other)
		{
			if (other.gameObject == pm.gameObject) return;

			var e = other.GetComponent<Entity>();
			if (e)
			{
				e.ExternalActivity.moveMods.Remove(touchedEntities[e]);
				touchedEntities.Remove(e);
			}
		}

		float frame = 0f, cooldown = 10f;

		EnvironmentController ec;

		Dictionary<Entity, MovementModifier> touchedEntities = [];

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
