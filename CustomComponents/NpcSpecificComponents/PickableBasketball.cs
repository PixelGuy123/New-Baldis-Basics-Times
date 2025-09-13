using System.Collections;
using System.Collections.Generic;
using BBTimes.CustomContent.NPCs;
using PixelInternalAPI.Components;
using UnityEngine;

namespace BBTimes.CustomComponents.NpcSpecificComponents
{
	public class PickableBasketball : MonoBehaviour, IClickable<int>, IEntityTrigger // For dribble
	{
		bool _hidden = false;
		Vector3 _direction = default;

		public void Initialize(Dribble dribble, BasketballHoopMarker[] availableHoops)
		{
			dr = dribble;
			ec = dribble.ec;
			this.availableHoops = availableHoops;
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
			clickDelay = 0f;
			canBeClickedIndicatorRenderer.enabled = false;
			entity.SetActive(false);
			entity.UpdateInternalMovement(Vector3.zero);
			_hidden = true;
		}
		public void SuccessThrowInHoop()
		{
			canBeClickedIndicatorRenderer.enabled = false;
			fakeRendererBase = Instantiate(renderer).gameObject.AddComponent<EmptyMonoBehaviour>();
			fakeRendererBase.transform.position = renderer.transform.position;

			Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audThrow);
			fakeRendererBase.StartCoroutine(ThrowItselfOnHoop(availableHoops[Random.Range(0, availableHoops.Length)]));
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
			if (clickDelay > 0f || _hidden || Singleton<CoreGameManager>.Instance.GetPlayer(player) != expectedPlayer) return;
			if (!IsInPickableRange)
			{
				clickDelay = 0.1f;
				return;
			}

			if (availableHoops != null && availableHoops.Length != 0)
				SuccessThrowInHoop();
			else
				IrritateDribble(false); // In an else because throw in hoop will have a timed Dribble happy

			Hide();
		}
		public void ClickableSighted(int player) { }
		public void ClickableUnsighted(int player) { }
		public bool ClickableHidden() => clickDelay > 0f || _hidden || !IsInPickableRange;
		public bool ClickableRequiresNormalHeight() => false; // To make Dribble's minigame fair

		void Update()
		{
			if (!_hidden)
			{
				entity.UpdateInternalMovement(_direction * speed * ec.EnvironmentTimeScale);
				// animation loop
				if (expectedRoom != ec.CellFromPosition(transform.position).room)
				{
					Hide();
					IrritateDribble(true);
				}

				canBeClickedIndicatorRenderer.enabled = IsInPickableRange && clickDelay <= 0f;

				if (clickDelay > 0f)
					clickDelay -= Time.deltaTime * ec.EnvironmentTimeScale;
			}
		}

		void OnDestroy()
		{
			StopAllCoroutines();

			if (fakeRendererBase)
				Destroy(fakeRendererBase.gameObject);

			gauge?.Deactivate();
			while (affectedEntities.Count != 0)
			{
				affectedEntities[0].ExternalActivity.moveMods.Remove(moveMod);
				affectedEntities.RemoveAt(0);
			}
		}

		public void EntityTriggerEnter(Collider other, bool validCollision)
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
					dr.DisappointDribble(expectedPlayer);

					e.StartCoroutine(Timer(e));
				}
			}

		}
		public void EntityTriggerStay(Collider other, bool validCollision) { }
		public void EntityTriggerExit(Collider other, bool validCollision) { }

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

		IEnumerator ThrowItselfOnHoop(BasketballHoopMarker hoop)
		{
			float t = 0f;
			var hoopHolePosition = hoop.transform.position + (hoop.transform.rotation * hoop.localHoopPosition); // multiplies rotation to match the orientation of the hoop when adding the localPos
			Vector3 start = fakeRendererBase.transform.position, end = hoopHolePosition;

			// Calculate arc height (can be tweaked as needed)
			float arcHeight = Mathf.Max(2f, Mathf.Abs(end.y - start.y) + maxThrowHeight);

			while (t < basketballToHoopInSeconds)
			{
				t += Time.deltaTime * ec.EnvironmentTimeScale;
				float progress = Mathf.Clamp01(t / basketballToHoopInSeconds);

				// Parabolic arc calculation
				Vector3 pos = Vector3.Lerp(start, end, progress);
				// Add arc in the Y axis
				float arc = 4 * arcHeight * progress * (1 - progress); // Parabola: 4h * t * (1-t)
				pos.y += arc;
				fakeRendererBase.transform.position = pos;
				yield return null;
			}

			hoop.audMan.PlaySingle(hoop.audGoal);
			IrritateDribble(false);

			t = 0f;
			start = fakeRendererBase.transform.position; end.y = -10f;

			while (t < basketballToHoopInSeconds)
			{
				t += Time.deltaTime * ec.EnvironmentTimeScale;
				fakeRendererBase.transform.position = Vector3.Lerp(start, end, Mathf.Clamp01(t / basketballToHoopInSeconds));
				yield return null;
			}


		}

		internal void Throw(Vector3 rot, object value1, PlayerManager pm, object value2, object value3)
		{
			throw new System.NotImplementedException();
		}

		Dribble dr;
		EnvironmentController ec;
		PlayerManager expectedPlayer;
		RoomController expectedRoom;
		HudGauge gauge;
		readonly List<Entity> affectedEntities = [];
		float speed = 25f, clickDelay = 0f;
		readonly MovementModifier moveMod = new(Vector3.zero, 0.7f);
		BasketballHoopMarker[] availableHoops;
		EmptyMonoBehaviour fakeRendererBase;

		public bool IsInPickableRange
		{
			get
			{
				if (!expectedPlayer)
					return false;
				float distance = Vector3.Distance(expectedPlayer.transform.position, transform.position);
				return distance >= allowedExactDistanceToPick - allowedDistanceRangeToPick && distance <= allowedExactDistanceToPick + allowedDistanceRangeToPick;
			}
		}

		[SerializeField]
		internal Entity entity;

		[SerializeField]
		internal SpriteRenderer renderer, canBeClickedIndicatorRenderer;

		[SerializeField]
		internal Sprite gaugeSprite;

		[SerializeField]
		internal float stunCooldown = 15f, allowedDistanceRangeToPick = 5.7f, allowedExactDistanceToPick = 5.25f, basketballToHoopInSeconds = 0.65f, maxThrowHeight = 0.35f;

		[SerializeField]
		internal SoundObject audHit, audThrow;
	}
}
