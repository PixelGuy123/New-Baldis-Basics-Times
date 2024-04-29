using BBTimes.CustomContent.NPCs;
using System.Collections;
using UnityEngine;

namespace BBTimes.CustomComponents
{
	public class PixLaserBeam : MonoBehaviour, IEntityTrigger
	{
		void Awake()
		{
			
			entity.Initialize(pix.ec, pix.transform.position);
			transform.LookAt(targetPlayer.transform);
			rotation = pix.ec.CellFromPosition(transform.position).open ? transform.rotation : Directions.DirFromVector3(targetPlayer.transform.position - transform.position, 45f).ToRotation(); // If not in an open cell, just shoot a straight line

			entity.OnEntityMoveInitialCollision += (hit) =>
			{
				if (flying && hit.transform.gameObject.layer != 2)
				{
					flying = false;
					pix.DecrementBeamCount();
					Destroy(gameObject);
				}
			};
		}

		public void EntityTriggerStay(Collider other)
		{

		}

		public void EntityTriggerExit(Collider other)
		{
			if (other.gameObject == pix.gameObject)
				ignorePix = false;
		}

		public void EntityTriggerEnter(Collider other)
		{
			if (!flying || (other.gameObject == pix.gameObject && ignorePix)) return;

			if (other.isTrigger && (other.CompareTag("NPC") || other.CompareTag("Player")))
			{
				var entity = other.GetComponent<Entity>();
				if (entity) 
				{
					flying = false;
					if (other.gameObject == targetPlayer.gameObject)
					{
						pix.SetAsSuccess();
						renderer.gameObject.SetActive(false);
					}
						

					audMan.QueueAudio(audShock);
					audMan.SetLoop(true);
					audMan.maintainLoop = true;

					pix.DecrementBeamCount();
					actMod = entity.ExternalActivity;
					actMod.moveMods.Add(moveMod);
					entity.AddForce(new(other.transform.position - transform.position, 9f, -8.5f));

					StartCoroutine(Timer());
				}

			}
		}

		void Update()
		{
			if (flying)
			{
				transform.rotation = rotation; // Workaround to get this stupid rotation working
				frame += 10 * pix.ec.EnvironmentTimeScale * Time.deltaTime;
				frame %= flyingSprites.Length;
				renderer.sprite = flyingSprites[Mathf.FloorToInt(frame)];
				entity.UpdateInternalMovement(transform.forward * speed * pix.ec.EnvironmentTimeScale);
			}
			else
			{
				if (actMod != null)
				{
					entity.UpdateInternalMovement(Vector3.zero);
					transform.position = actMod.transform.position;
				}
				frame += 14 * pix.ec.EnvironmentTimeScale * Time.deltaTime;
				frame %= shockSprites.Length;
				renderer.sprite = shockSprites[Mathf.FloorToInt(frame)];
			}
		}

		IEnumerator Timer()
		{
			float time = 15f;
			while (time > 0f)
			{
				time -= pix.ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}
			actMod?.moveMods.Remove(moveMod);

			Destroy(gameObject);

			yield break;
		}

		bool flying = true, ignorePix = true;
		float frame = 0f;

		[SerializeField]
		internal Entity entity;

		[SerializeField]
		internal SpriteRenderer renderer;

		[SerializeField]
		internal Sprite[] flyingSprites;

		[SerializeField]
		internal Sprite[] shockSprites;

		[SerializeField]
		internal SoundObject audShock;

		[SerializeField]
		internal AudioManager audMan;

		internal Pix pix;
		internal PlayerManager targetPlayer;

		ActivityModifier actMod = null;

		readonly MovementModifier moveMod = new(Vector3.zero, 0.65f);

		Quaternion rotation;

		const float speed = 25f;
	}
}
