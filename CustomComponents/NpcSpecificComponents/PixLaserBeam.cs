using System.Collections;
using BBTimes.CustomContent.NPCs;
using UnityEngine;

namespace BBTimes.CustomComponents
{
	public class PixLaserBeam : MonoBehaviour, IEntityTrigger
	{
		public void InitBeam(Pix pixc, PlayerManager pm)
		{
			pix = pixc;
			targetPlayer = pm;
			ec = pm.ec;

			entity.Initialize(ec, pix.transform.position);
			transform.LookAt(targetPlayer.transform);
			direction = ec.CellFromPosition(transform.position).open ? transform.forward : Directions.DirFromVector3(targetPlayer.transform.position - transform.position, 45f).ToVector3(); // If not in an open cell, just shoot a straight line

			entity.OnEntityMoveInitialCollision += (hit) =>
			{
				if (flying && hit.transform.gameObject.layer != 2)
				{
					flying = false;
					pix?.DecrementBeamCount();
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
			bool wasPlayer = other.CompareTag("Player");
			if (other.isTrigger && (other.CompareTag("NPC") || wasPlayer))
			{
				var entity = other.GetComponent<Entity>();
				if (entity)
				{
					flying = false;
					if (other.gameObject == targetPlayer.gameObject)
					{
						pix?.SetAsSuccess();
						renderer.gameObject.SetActive(false);
					}


					audMan.QueueAudio(audShock);
					audMan.SetLoop(true);
					audMan.maintainLoop = true;

					pix?.DecrementBeamCount();
					actMod = entity.ExternalActivity;
					actMod.moveMods.Add(moveMod);
					entity.AddForce(new(other.transform.position - transform.position, 9f, -8.5f));

					if (wasPlayer)
						gauge = Singleton<CoreGameManager>.Instance.GetHud(other.GetComponent<PlayerManager>().playerNumber).gaugeManager.ActivateNewGauge(gaugeSprite, lifeTime);

					StartCoroutine(Timer());
				}

			}
		}

		void Update()
		{
			if (flying)
			{
				frame += 10 * ec.EnvironmentTimeScale * Time.deltaTime;
				frame %= flyingSprites.Length;
				renderer.sprite = flyingSprites[Mathf.FloorToInt(frame)];
				entity.UpdateInternalMovement(direction * speed * ec.EnvironmentTimeScale);
			}
			else
			{
				if (actMod != null)
				{
					entity.UpdateInternalMovement(Vector3.zero);
					transform.position = actMod.transform.position;
				}
				frame += 14 * ec.EnvironmentTimeScale * Time.deltaTime;
				frame %= shockSprites.Length;
				renderer.sprite = shockSprites[Mathf.FloorToInt(frame)];
			}
		}

		IEnumerator Timer()
		{
			float time = lifeTime;
			while (time > 0f)
			{
				time -= ec.EnvironmentTimeScale * Time.deltaTime;
				gauge?.SetValue(lifeTime, time);
				yield return null;
			}
			actMod?.moveMods.Remove(moveMod);
			gauge?.Deactivate();

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

		[SerializeField]
		internal Sprite gaugeSprite;

		[SerializeField]
		internal float lifeTime = 15f;

		HudGauge gauge;
		Pix pix;
		PlayerManager targetPlayer;
		EnvironmentController ec;

		ActivityModifier actMod = null;

		readonly MovementModifier moveMod = new(Vector3.zero, 0.65f);
		Vector3 direction;

		const float speed = 25f;
	}
}
