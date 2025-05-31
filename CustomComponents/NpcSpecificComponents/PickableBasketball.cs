using System.Collections;
using System.Collections.Generic;
using BBTimes.CustomContent.NPCs;
using UnityEngine;

namespace BBTimes.CustomComponents.NpcSpecificComponents
{
	public class PickableBasketball : MonoBehaviour, IClickable<int>, IEntityTrigger // For dribble
	{
		bool _hidden = false;
		Vector3 _direction = default;
		float frame = 0f;

		public void Initialize(Dribble dribble)
		{
			dr = dribble;
			ec = dribble.ec;
			entity.Initialize(ec, transform.position);
			entity.SetActive(false);
			entity.OnEntityMoveInitialCollision += (hit) =>
			{
				if (!_hidden)
				{
					Hide();
					IrritateDribble(true);
				}
			};
			Hide();
		}
		public void Hide()
		{
			entity.SetActive(false);
			entity.UpdateInternalMovement(Vector3.zero);
			_hidden = true;
		}
		public void Throw(Vector3 direction, Vector3 position, PlayerManager targetPlayer, float mult = 0.7f, float speed = 35f)
		{
			entity.SetHeight(5f);
			entity.Teleport(position);
			entity.SetActive(true);
			_direction = direction;
			_hidden = false;
			expectedPlayer = targetPlayer;
			expectedRoom = ec.CellFromPosition(position).room;
			this.speed = speed;
			moveMod.movementMultiplier = mult;
		}
		void IrritateDribble(bool angry)
		{
			dr.MinigameEnd(angry, expectedPlayer);
			expectedPlayer = null;
		}
		public void Clicked(int player)
		{
			if (_hidden || Singleton<CoreGameManager>.Instance.GetPlayer(player) != expectedPlayer) return;

			Hide();
			IrritateDribble(false);
		}
		public void ClickableSighted(int player) { }
		public void ClickableUnsighted(int player) { }
		public bool ClickableHidden() => _hidden;
		public bool ClickableRequiresNormalHeight() => false; // To make Dribble's minigame fair

		void Update()
		{
			if (!_hidden)
			{
				entity.UpdateInternalMovement(_direction * speed * ec.EnvironmentTimeScale);
				// animation loop
				frame += 8f * ec.EnvironmentTimeScale * Time.deltaTime;
				frame %= spriteAnim.Length;
				renderer.sprite = spriteAnim[Mathf.FloorToInt(frame)];
				if (expectedRoom != ec.CellFromPosition(transform.position).room)
				{
					Hide();
					IrritateDribble(true);
				}
			}
		}

		void OnDestroy()
		{
			StopAllCoroutines();
			while (affectedEntities.Count != 0)
			{
				affectedEntities[0].ExternalActivity.moveMods.Remove(moveMod);
				affectedEntities.RemoveAt(0);
			}
		}

		public void EntityTriggerEnter(Collider other)
		{
			if (_hidden || other.gameObject == dr.gameObject) return;
			if (other.isTrigger && other.CompareTag("Player"))
			{
				Entity e = other.GetComponent<Entity>();
				if (e)
				{
					// Already expects to be the Player only
					gauge = Singleton<CoreGameManager>.Instance.GetHud(other.GetComponent<PlayerManager>().playerNumber).gaugeManager.ActivateNewGauge(gaugeSprite, stunCooldown);

					Hide();
					Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audHit);

					e.AddForce(new((other.transform.position - transform.position).normalized, speed * 1.5f, -speed));
					dr.DisappointDribble();

					e.StartCoroutine(Timer(e));
				}
			}

		}
		public void EntityTriggerStay(Collider other) { }
		public void EntityTriggerExit(Collider other) { }

		IEnumerator Timer(Entity e)
		{
			e.ExternalActivity.moveMods.Add(moveMod);
			affectedEntities.Add(e);
			float cooldown = stunCooldown;
			while (cooldown > 0)
			{
				cooldown -= ec.EnvironmentTimeScale * Time.deltaTime;
				gauge.SetValue(stunCooldown, cooldown);
				yield return null;
			}

			gauge.Deactivate();
			affectedEntities.Remove(e);
			e.ExternalActivity.moveMods.Remove(moveMod);

			yield break;
		}


		Dribble dr;
		EnvironmentController ec;
		PlayerManager expectedPlayer;
		RoomController expectedRoom;
		HudGauge gauge;
		readonly List<Entity> affectedEntities = [];
		float speed = 25f;
		readonly MovementModifier moveMod = new(Vector3.zero, 0.7f);

		[SerializeField]
		internal Entity entity;

		[SerializeField]
		internal SpriteRenderer renderer;

		[SerializeField]
		internal Sprite gaugeSprite;

		[SerializeField]
		internal float stunCooldown = 15f;

		[SerializeField]
		internal Sprite[] spriteAnim;

		[SerializeField]
		internal SoundObject audHit;
	}
}
