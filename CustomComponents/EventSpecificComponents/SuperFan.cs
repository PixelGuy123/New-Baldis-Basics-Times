using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomComponents.EventSpecificComponents
{
	public class SuperFan : TileBasedObject // Basically a Cloudy Copter, but as a Fan
	{
		public void Initialize(EnvironmentController ec, IntVector2 pos, Direction dir, out List<Cell> usedCells)
		{
			transform.position = ec.CellFromPosition(pos).CenterWorldPosition + dir.GetOpposite().ToVector3() * 4.99f;
			transform.rotation = dir.ToRotation();
			direction = dir;
			this.ec = ec;
			position = pos;
			usedCells = [];

			tileA = ec.CellFromPosition(position);
			IntVector2 p = position;
			IntVector2 d = direction.ToIntVector2();
			while (ec.CellFromPosition(p).TileMatches(tileA.room))
			{
				var c = ec.CellFromPosition(p);
				if (c.TileMatches(tileA.room))
					usedCells.Add(c);
				p += d;
			}
			tileB = ec.CellFromPosition(p - d);

			windManager.transform.SetParent(ec.transform);
			windManager.Initialize(ec);
			windManager.SetSpeed(pushForce);

			for (int i = 0; i < windGraphics.Length; i++)
				windGraphics[i].sharedMaterial = windManager.newMaterial;
		}

		public void TurnMe(bool turn)
		{
			if (!turn)
			{
				if (--turns < 0)
					turns = 0;
				else if (turns == 0)
				{
					windManager.gameObject.SetActive(false); // Disable the windManager once
					audMan.FlushQueue(true);
				}
				return;
			}
			turns++;
			if (turns == 1) // Enable the wind manager once
			{
				windManager.SetDirection(direction);
				IntVector2 intVector = tileA.position - tileB.position;

				windManager.transform.position = new Vector3((intVector.x / 2f + tileB.position.x) * 10f + 5f, 5f, (intVector.z / 2f + tileB.position.z) * 10f + 5f);
				windManager.BoxCollider.size = new Vector3(10f, 10f, Mathf.Abs(intVector.x + intVector.z) * 10f + 10f);
				foreach (MeshRenderer meshRenderer in windGraphics)
				{
					meshRenderer.transform.localScale = Vector3.zero + Vector3.forward + Vector3.right * 10f;
					meshRenderer.transform.localScale = meshRenderer.transform.localScale + Vector3.up * (Mathf.Abs(intVector.x + intVector.z) + 1) * 10f;
				}
				windManager.newMaterial.SetVector("_Tiling", Vector2.one + Vector2.up * Mathf.Abs(intVector.x + intVector.z));
				windGraphicsParent.rotation = direction.ToRotation();
				windManager.gameObject.SetActive(true);
				audMan.SetLoop(true);
				audMan.maintainLoop = true;
				audMan.QueueAudio(audBlow);
			}
		}

		void Update()
		{
			frame += ec.EnvironmentTimeScale * Time.deltaTime * animSpeed;
			frame %= sprites.Length;
			renderer.sprite = sprites[Mathf.FloorToInt(frame)];
			if (!IsActive)
				animSpeed += -animSpeed * 0.35f * ec.EnvironmentTimeScale * Time.deltaTime;
			else
				animSpeed += animSpeedFactor * ec.EnvironmentTimeScale * Time.deltaTime * pushForce * 1.6f;
			animSpeed = Mathf.Clamp(animSpeed, 0, maxAnimSpeed);


		}

		float frame = 0f, animSpeed = 0f;
		public bool IsActive => turns > 0;
		int turns = 0;
		Cell tileA, tileB;

		[SerializeField]
		internal Transform windGraphicsParent;

		[SerializeField]
		internal MeshRenderer[] windGraphics;

		[SerializeField]
		internal BeltManager windManager;

		[SerializeField]
		internal AudioManager audMan;

		[SerializeField]
		internal SoundObject audBlow;

		[SerializeField]
		internal SpriteRenderer renderer;

		[SerializeField]
		internal Sprite[] sprites;

		[SerializeField]
		[Range(0.1f, 1f)]
		internal float animSpeedFactor = 0.75f;

		[SerializeField]
		internal float maxAnimSpeed = 55f, pushForce = 13f;
	}
}
