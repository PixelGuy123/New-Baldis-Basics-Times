using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomContent.Objects
{
	public class SmallDoor : TileBasedObject
	{
		void Start()
		{
			bg[0] = ATile.room.wallTex;
			bg[1] = BTile.room.wallTex;
			for (int i = 0; i < doors.Length; i++)
			{
				MaterialModifier.ChangeHole(doors[i], mask[i], overlayShut[i]);
				MaterialModifier.SetBase(doors[i], bg[i]);
			}
			ATile.Block(direction, true); // Blocks the cell by default, only leading
			BTile.Block(direction.GetOpposite(), true);

			var mapTile = ec.map.AddExtraTile(ATile.position);
			mapTile.SpriteRenderer.sprite = doorIcon;
			mapTile.SpriteRenderer.color = ATile.room.color;
			mapTile.transform.rotation = direction.ToUiRotation();

			mapTile = ec.map.AddExtraTile(BTile.position);
			mapTile.SpriteRenderer.sprite = doorIcon;
			mapTile.SpriteRenderer.color = BTile.room.color;
			mapTile.transform.rotation = direction.GetOpposite().ToUiRotation();
		}
		void OnTriggerEnter(Collider other)
		{
			if (other.GetComponent<Entity>()?.Squished ?? false)
				Open(other);
		}

		void OnTriggerExit(Collider other)
		{
			if (touchedCols.Contains(other))
			{
				touchedCols.Remove(other);
				IgnoreCollision(other, false);
				Close();
			}
		}

		public void Open(Collider toIgnore)
		{
			if (!IsOpen)
			{
				audMan.PlaySingle(audOpen);
				if (toIgnore.CompareTag("Player"))
					ec.MakeNoise(transform.position, 2);
			}

			IgnoreCollision(toIgnore, true);
			for (int i = 0; i < doors.Length; i++)
				MaterialModifier.ChangeOverlay(doors[i], overlayOpen[i]);

			touchedCols.Add(toIgnore);

			opens++;
		}

		public void Close()
		{
			if (opens == 1)
			{
				audMan.PlaySingle(audClose);
				for (int i = 0; i < doors.Length; i++)
					MaterialModifier.ChangeOverlay(doors[i], overlayShut[i]);
			}

			opens--;
			if (opens <= 0)
				opens = 0;
		}

		void IgnoreCollision(Collider toIgnore, bool ignore)
		{
			for (int i = 0; i < colliders.Length; i++)
				Physics.IgnoreCollision(toIgnore, colliders[i], ignore);
		}

		readonly HashSet<Collider> touchedCols = [];

		int opens = 0;

		public bool IsOpen => opens > 0;

		public Cell ATile => ec.CellFromPosition(position);
		public Cell BTile => ec.CellFromPosition(position + direction.ToIntVector2());

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject audOpen, audClose;

		[SerializeField]
		internal Material[] mask, overlayShut, overlayOpen;

		[SerializeField]
		internal MeshRenderer[] doors;

		[SerializeField]
		internal MeshCollider[] colliders;

		[SerializeField]
		internal Texture2D[] bg;

		[SerializeField]
		internal Sprite doorIcon;
	}
}
