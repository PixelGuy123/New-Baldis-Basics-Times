using BBTimes.Extensions;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BBTimes.CustomContent.Objects
{
	public class SnowShovel : EnvironmentObject, IClickable<int>
	{
		[SerializeField]
		internal TextMeshPro timer;

		[SerializeField]
		internal Vector3 pickupAxisOffset = Vector3.up;

		[SerializeField]
		internal BoxCollider clickableCollision;

		[SerializeField]
		internal GameObject holdRender, normalRender;

		[SerializeField]
		internal float forwardOffset = 6f, offsetAngle = 25f, yOffset = -1.5f, defaultPickupCooldown = 30f, layOnFloorYOffset = 0.1f;

		[SerializeField]
		internal LayerMask overlayMask = LayerMask.NameToLayer("Overlay");

		Vector3 lastPosition;
		RoomController home;


		public bool IsOnCooldown => pickCooldown > 0f;
		float pickCooldown = 0f;
		GameCamera camToFollow;
		PlayerManager playerToFollow;

		public void RestInPlace(bool resetTimer)
		{
			SetLayerOfRenderers(false);

			activeShovels.Remove(playerToFollow);

			lastPosition.y = layOnFloorYOffset;
			transform.position = lastPosition;

			camToFollow = null;
			playerToFollow = null;

			transform.rotation = default;
			clickableCollision.enabled = true;
			if (resetTimer)
			{
				pickCooldown = defaultPickupCooldown;
				timer.gameObject.SetActive(true);
			}
		}
		public override void LoadingFinished()
		{
			base.LoadingFinished();
			home = ec.CellFromPosition(transform.position).room;
			lastPosition = transform.position;
		}

		public void Clicked(int player)
		{
			var pm = Singleton<CoreGameManager>.Instance.GetPlayer(player);

			if (PlayerHasShovel(pm, out _) || !ec.CellFromPosition(pm.transform.position).TileMatches(home) || IsOnCooldown) return;

			clickableCollision.enabled = false;

			camToFollow = Singleton<CoreGameManager>.Instance.GetCamera(player);
			playerToFollow = pm;
			SetLayerOfRenderers(true);

			activeShovels.Add(pm, this);
		}
		public bool ClickableHidden() => IsOnCooldown;
		public bool ClickableRequiresNormalHeight() => false;
		public void ClickableSighted(int player) { }
		public void ClickableUnsighted(int player) { }
		void SetLayerOfRenderers(bool overlay)
		{
			holdRender.layer = overlay ? overlayMask : 0;
			holdRender.SetActive(overlay);
			normalRender.SetActive(!overlay);
		}
		void Update()
		{
			if (!camToFollow)
			{
				if (pickCooldown > 0f)
				{
					pickCooldown -= ec.EnvironmentTimeScale * Time.deltaTime;
					timer.text = Mathf.FloorToInt(pickCooldown).ToString();
				}
				else timer.gameObject.SetActive(false);

				return;
			}
			var cell = ec.CellFromPosition(camToFollow.transform.position);
			if (!cell.TileMatches(home))
				RestInPlace(false);
			else
				lastPosition = cell.FloorWorldPosition;
		}

		void LateUpdate()
		{
			if (!camToFollow)
				return;

			holdRender.transform.position = camToFollow.transform.position +
				(forwardOffset * camToFollow.transform.forward.RotateAroundAxis(pickupAxisOffset, offsetAngle)) +
				(yOffset * Vector3.up);
		}


		readonly static Dictionary<PlayerManager, SnowShovel> activeShovels = [];

		public static bool PlayerHasShovel(PlayerManager pm, out SnowShovel shovel) =>
			activeShovels.TryGetValue(pm, out shovel);
	}
}
