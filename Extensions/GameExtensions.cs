using System.Collections;
using System.Collections.Generic;
using System.Text;
using BBTimes.CustomComponents;
using BBTimes.CustomContent.NPCs;
using BBTimes.Manager;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.Registers;
using PixelInternalAPI.Extensions;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace BBTimes.Extensions
{
	public static partial class GameExtensions // A whole storage of extension methods thrown into a single class, how organized (irony intended).
	{
		public static ParticleSystem GetNewParticleSystem()
		{
			var particle = Object.Instantiate(
						(
							ItemMetaStorage.Instance.FindByEnum(Items.ChalkEraser).value.item as ChalkEraser
						).cloud);

			var obj = particle.gameObject;

			Object.DestroyImmediate(particle.particles); // Destroys original Particle instance

			particle.particles = obj.AddComponent<ParticleSystem>(); // Adds a new fresh ParticleSystem to have everything set to default values
			var renderer = obj.GetComponent<ParticleSystemRenderer>();

			renderer.material = new(renderer.material) { name = $"{particle.name}_Mat" };
			return particle.particles;
		}
		public static T SpawnForeignNPC<T>(this EnvironmentController ec, T prefab, Vector3 position) where T : NPC
		{
			T npc2 = Object.Instantiate(prefab, ec.transform);
			npc2.ec = ec;
			npc2.transform.position = position;

			npc2.Initialize();
			npc2.gameObject.SetActive(true);
			return npc2;
		}
		public static StandardMenuButton CreateImageButton(this Canvas canvasRef, string name, Sprite highlightVisual, Sprite unhighlightVisual)
		{
			var but = new GameObject(name)
			{
				tag = "Button",
				layer = LayerMask.NameToLayer("UI")
			}.AddComponent<StandardMenuButton>();

			but.transform.SetParent(canvasRef.transform);
			but.transform.localScale = Vector3.one; // it's set to scale 0 for some reason?

			but.OnRelease = new();
			but.OnHighlight = new();
			but.OffHighlight = new();
			but.OnPress = new();

			but.image = but.gameObject.AddComponent<Image>();
			but.image.sprite = unhighlightVisual;
			but.unhighlightOnEnable = true;

			but.swapOnHigh = true;
			but.highlightedSprite = highlightVisual;
			but.unhighlightedSprite = unhighlightVisual;

			return but;
		}
		public static Texture2D Mask(this Texture2D original, Texture2D texRef) =>
			original.Mask(texRef.GetPixels());
		public static Texture2D Mask(this Texture2D original, Color[] colorRef)
		{
			var pixels = original.GetPixels();
			if (colorRef.Length != pixels.Length)
				throw new System.ArgumentException($"Color reference array has a different length from the original texture given. Ref: ({colorRef.Length}) | Pixels: ({pixels.Length})");
			for (int i = 0; i < pixels.Length; i++)
			{
				if (colorRef[i].a == 0f) // If alpha is 0, it's not inside the mask
					pixels[i] = Color.clear;
			}
			original.SetPixels(pixels);
			original.Apply();
			return original;
		}
		public static Texture2D ActualResize(this Texture2D original, int newWidth, int newHeight) // Apparently you work an average of WxH grid, not linear lol
		{ // yoink from LotsOfItems
			int originalWidth = original.width;
			int originalHeight = original.height;

			if (originalWidth == newWidth && originalHeight == newHeight)
				return original; // No need to apply changes

			// Calculate scaling factors.
			int scaleX = originalWidth / newWidth;
			int scaleY = originalHeight / newHeight;

			Texture2D newTex = new(newWidth, newHeight, original.format, false)
			{
				filterMode = original.filterMode
			};

			// Get the original pixel data.
			Color[] originalColors = original.GetPixels();
			Color[] newColors = new Color[newWidth * newHeight];

			// Loop over every pixel in the new texture.
			for (int newY = 0; newY < newHeight; newY++)
			{
				for (int newX = 0; newX < newWidth; newX++)
				{
					Color colorSum = Color.black;
					bool invalidAlpha = false;

					// For each new pixel, average over the corresponding block in the original texture.
					for (int offsetY = 0; offsetY < scaleY; offsetY++)
					{
						for (int offsetX = 0; offsetX < scaleX; offsetX++)
						{
							int origX = newX * scaleX + offsetX;
							int origY = newY * scaleY + offsetY;
							int index = origY * originalWidth + origX;

							Color current = originalColors[index];
							if (current.a < 1f)
							{
								invalidAlpha = true;
							}
							colorSum += current;
						}
					}

					int pixelCount = scaleX * scaleY;
					Color avgColor = colorSum / pixelCount;
					avgColor.a = invalidAlpha ? 0f : 1f;

					newColors[newY * newWidth + newX] = invalidAlpha ? Color.clear : avgColor;
				}
			}

			// Apply the new colors and update the texture.
			newTex.SetPixels(newColors);
			newTex.Apply();

			return newTex;
		}
		public static Texture2D ConvertToGrayscale(this Texture2D texture)
		{
			Color32[] pixels = texture.GetPixels32();

			for (int i = 0; i < pixels.Length; i++)
			{
				Color32 pixel = pixels[i];
				byte gray = (byte)Mathf.RoundToInt((0.299f * pixel.r) + (0.587f * pixel.g) + (0.114f * pixel.b)); // cool formula I guess

				pixels[i] = new Color32(gray, gray, gray, pixel.a);
			}

			texture.SetPixels32(pixels);
			texture.Apply();

			return texture;
		}
		public static bool IsPlayingClip(this AudioManager audMan, SoundObject audio)
		{
			return audMan.audioDevice.clip == audio.soundClip; // Might change this later once BB+ implements audios for different languages
		}
		public static string CreateSpriteNumbersFromString(this int num)
		{
			if (cachedNumbers.TryGetValue(num, out var str))
				return str;

			string number = num.ToString();

			for (int i = 0; i < number.Length; i++)
				bld.Append($"<sprite={number[i]}>");

			string strFound = bld.ToString();
			cachedNumbers.Add(num, strFound);
			bld.Clear();

			return strFound;
		}
		readonly static Dictionary<int, string> cachedNumbers = new(capacity: 300); // naturally it'd never need to have the whole set of int.MaxValue/int.MinValue
		static readonly StringBuilder bld = new();

		public static bool RaycastNPC(this Looker looker, NPC npc)
		{
			looker.Raycast(npc.transform,
						Mathf.Min((looker.transform.position - npc.transform.position).magnitude + npc.Navigator.Velocity.magnitude,
						looker.distance,
						looker.npc.ec.MaxRaycast), out bool flag);
			return flag;
		}
		public static ItemObject RemoveRandomItemAndReturnIt(this ItemManager itm)
		{
			ItemObject selectedItm = itm.nothing;
			if (itm.HasItem())
			{
				int num = Random.Range(0, itm.maxItem + 1);
				while (itm.items[num] == itm.nothing && !itm.slotLocked[num])
				{
					num = Random.Range(0, itm.maxItem + 1);
				}
				selectedItm = itm.items[num];
				itm.RemoveItem(num);
			}

			return selectedItm;
		}
		public static Sprite[] TakeAPair(this Sprite[] sprs, int index, int count)
		{
			Sprite[] newSprs = new Sprite[count];
			for (int i = 0; i < count; i++)
				newSprs[i] = sprs[index++];
			return newSprs;
		}
		public static RendererContainer AddContainer(this GameObject obj, params Renderer[] renderers)
		{
			var r = obj.AddComponent<RendererContainer>();
			r.renderers = renderers;
			return r;
		}
		public static Vector3 RotateAroundAxis(this Vector3 vector, Vector3 axis, float angle) =>
		 Quaternion.AngleAxis(angle, axis) * vector;

		public static float Magnitude(this IntVector2 vec) =>
			Mathf.Sqrt((vec.x * vec.x) + (vec.z * vec.z));
		public static IntVector2 GetRoomSize(this RoomAsset asset)
		{
			IntVector2 size = new(0, 0);

			for (int i = 0; i < asset.cells.Count; i++)
			{
				if (asset.cells[i].pos.x > size.x)
					size.x = asset.cells[i].pos.x;

				if (asset.cells[i].pos.z > size.z)
					size.z = asset.cells[i].pos.z;
			}

			return size;
		}
		public static string RepeatStr(this string toRepeat, int amount)
		{
			StringBuilder bld = new();
			for (int i = 0; i < amount; i++)
				bld.Append(toRepeat);
			return bld.ToString();
		}
		public static bool IsBitSet(this int flag, int position) // Thanks ChatGPT
		{
			// Check if the bit at the specified position is set (1)
			return (flag & (1 << position)) != 0;
		}
		public static int ToggleBit(this int flag, int position) // Thanks ChatGPT
		{
			// Use XOR to flip the bit at the specified position
			return flag ^ (1 << position);
		}
		public static void TryRunMethod(System.Action actionToRun, bool causeCrashIfFail = true)
		{
			try
			{
				actionToRun();
			}
			catch (System.Exception e)
			{
				Debug.LogWarning("------ Error caught during an action ------");
				Debug.LogException(e);

				if (causeCrashIfFail)
					MTM101BaldiDevAPI.CauseCrash(BBTimesManager.plug.Info, e);
			}
		}
		public static Window ForceBuildWindow(this EnvironmentController ec, Cell tile, Direction dir, WindowObject wObject)
		{
			if (ec.ContainsCoordinates(tile.position + dir.ToIntVector2()))
			{
				var cell = ec.CellFromPosition(tile.position + dir.ToIntVector2());

				if (cell.Null)
					cell.room.wallTex = tile.room.wallTex;

				IntVector2 position = tile.position;
				Window window = Object.Instantiate(wObject.windowPre, tile.room.transform);
				ec.ConnectCells(tile.position, dir);
				Cell cell2 = ec.CellFromPosition(position);
				window.Initialize(ec, tile.position, dir, wObject);
				cell2.HardCoverWall(dir, true);
				cell = ec.CellFromPosition(tile.position + dir.ToIntVector2());
				cell.HardCoverWall(dir.GetOpposite(), true);
				window.transform.position = tile.FloorWorldPosition;
				window.transform.rotation = dir.ToRotation();
				if (window.aTile.Null)
					window.windows[0].enabled = false;

				if (window.bTile.Null)
					window.windows[1].enabled = false;

				return window;
			}
			return null;
		}

		public static List<Cell> AllExistentCells(this EnvironmentController ec)
		{
			List<Cell> list = [];
			for (int i = 0; i < ec.levelSize.x; i++)
			{
				for (int j = 0; j < ec.levelSize.z; j++)
				{
					Cell cell = ec.CellFromPosition(i, j);
					list.Add(cell);
				}
			}
			return list;
		}

		public static IEnumerator LightChanger(this EnvironmentController ec, List<Cell> lights, float delay)
		{
			float maxDelay = delay;
			float time = maxDelay;
			while (lights.Count != 0)
			{
				while (time > 0f)
				{
					time -= Time.deltaTime * ec.EnvironmentTimeScale;
					yield return null;
				}
				maxDelay *= 0.95f;
				time = maxDelay;
				int num = Random.Range(0, lights.Count);
				lights[num].lightColor = Color.red;
				lights[num].SetLight(true);
				lights.RemoveAt(num);
			}
			yield break;
		}

		public static IEnumerator InfiniteAnger(Baldi b, float increaser)
		{
			if (increaser <= 0f)
				yield break;

			while (true)
			{
				b.GetAngry(increaser * b.TimeScale * Time.deltaTime);
				yield return null;
			}
		}

		public static void RemoveFunction(this RoomFunctionContainer container, RoomFunction function) =>
			container.functions.Remove(function);


		public static BoxCollider AddBoxCollider(this GameObject g, Vector3 center, Vector3 size, bool isTrigger)
		{
			var c = g.AddComponent<BoxCollider>();
			c.center = center;
			c.size = size;
			c.isTrigger = isTrigger;
			return c;
		}
		public static NavMeshObstacle AddNavObstacle(this GameObject g, Vector3 size) =>
			g.AddNavObstacle(Vector3.zero, size);
		public static NavMeshObstacle AddNavObstacle(this GameObject g, Vector3 center, Vector3 size)
		{
			var nav = g.AddComponent<NavMeshObstacle>();
			nav.center = center;
			nav.size = size;
			nav.carving = true;
			return nav;
		}

		public static WeightedTexture2D ToWeightedTexture(this WeightedSelection<Texture2D> t) =>
			new() { selection = t.selection, weight = t.weight };

		public static PlayerAttributesComponent GetAttribute(this PlayerManager pm) =>
			pm.GetComponent<PlayerAttributesComponent>();

		public static GameObject SetAsPrefab(this GameObject obj, bool active)
		{
			obj.ConvertToPrefab(active);
			return obj;
		}

		public static T SafeInstantiate<T>(this T obj) where T : Component
		{
			obj.gameObject.SetActive(false);
			var inst = Object.Instantiate(obj); // Instantiate a deactivated object, so Awake() calls aren't *called*
			obj.gameObject.SetActive(true);

			return inst;
		}

		public static T SafeDuplicatePrefab<T>(this T obj, bool setActive) where T : Component
		{
			obj.gameObject.SetActive(false);

			var inst = obj.DuplicatePrefab();
			inst.gameObject.SetActive(setActive);

			obj.gameObject.SetActive(true);

			return inst;
		}

		public static bool RotateSmoothlyToNextPoint(this Transform transform, Vector3 nextPoint, float speed)
		{
			Vector3 rot = (nextPoint - transform.position).normalized;
			Vector3 vector = Vector3.RotateTowards(transform.forward, rot, Time.deltaTime * 2f * Mathf.PI * speed, 0f);
			if (vector != Vector3.zero)
				transform.rotation = Quaternion.LookRotation(vector, Vector3.up);

			return transform.forward == rot;
		}

		public static void SendToDetention(this PlayerManager pm, float time, int detentionNoise = defaultDetentionNoise)
		{
			if (pm.ec.offices.Count > 0)
			{
				int num = Random.Range(0, pm.ec.offices.Count);
				pm.Teleport(pm.ec.RealRoomMid(pm.ec.offices[num]));
				pm.ClearGuilt();
				pm.ec.offices[num].functionObject.GetComponent<DetentionRoomFunction>().Activate(time, pm.ec);

				Baldi baldi = pm.ec.GetBaldi();
				baldi?.ClearSoundLocations();
				pm.ec.MakeNoise(pm.transform.position, detentionNoise);
			}
		}
		public static void BlockAllDirs(this EnvironmentController ec, Vector3 pos, bool block) =>
			ec.BlockAllDirs(IntVector2.GetGridPosition(pos), block);

		public static void BlockAllDirs(this EnvironmentController ec, IntVector2 pos, bool block)
		{
			ec.FreezeNavigationUpdates(true);
			var origin = ec.CellFromPosition(pos);
			for (int i = 0; i < 4; i++)
			{
				var dir = (Direction)i;
				var cell = ec.CellFromPosition(pos + dir.ToIntVector2());
				if (origin.ConstNavigable(dir))
					cell.Block(dir.GetOpposite(), block);
			}
			ec.FreezeNavigationUpdates(false);
		}

		public static Vector3 GetRotationalPosFrom(this SpriteRenderer renderer, Vector2 offset)
		{
			var pro = new MaterialPropertyBlock();
			renderer.GetPropertyBlock(pro);
			float degrees = pro.GetFloat("_SpriteRotation") * Mathf.Deg2Rad; // darn radians
			return new((Mathf.Cos(degrees) * offset.x) + (-Mathf.Sin(degrees) * offset.y), (Mathf.Cos(degrees) * offset.y) + (Mathf.Sin(degrees) * offset.x));

		}

		public static void EndEarlier(this RandomEvent ev) =>
			ev.remainingTime = 0;

		public static List<ItemObject> GetAllShoppingItems()
		{
			List<ItemObject> itmObjs = [];
			foreach (var s in GenericExtensions.FindResourceObjects<SceneObject>())
			{
				s.shopItems.Do(x =>
				{
					var meta = x.selection.GetMeta();
					if (meta != null && !itmObjs.Contains(meta.value))
						itmObjs.Add(meta.value);
				});
			}
			return itmObjs;
		}

		public static void Limit(this ref Vector3 toLimit, float maxX, float maxY, float maxZ)
		{
			toLimit.x = Mathf.Min(maxX, toLimit.x);
			toLimit.x = Mathf.Max(-maxX, toLimit.x);

			toLimit.y = Mathf.Min(maxY, toLimit.y);
			toLimit.y = Mathf.Max(-maxY, toLimit.y);

			toLimit.z = Mathf.Min(maxZ, toLimit.z);
			toLimit.z = Mathf.Max(-maxZ, toLimit.z);
		}

		public static IEnumerator TimerToDestroy(GameObject target, EnvironmentController ec, float timer)
		{
			while (timer > 0f)
			{
				timer -= ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}
			Object.Destroy(target);

			yield break;
		}

		public static Rigidbody AddStaticRigidBody(this GameObject obj)
		{
			var rigid = obj.AddComponent<Rigidbody>();

			rigid.mass = 0f;
			rigid.constraints = RigidbodyConstraints.FreezeAll;
			rigid.angularDrag = 0f;

			return rigid;
		}

		public static Vector3 ToVector3(this IntVector2 vec) =>
			new(vec.x * 10f + 5f, 0f, vec.z * 10f + 5f);
		public static void CallOutPrincipals(this EnvironmentController ec, Vector3 pos, float speedMultiplier = 8f, bool whistleCall = true, bool ignoreNormalPrincipal = false) =>
			ec.CallOutPrincipals(ec.CellFromPosition(pos), speedMultiplier, whistleCall, ignoreNormalPrincipal);
		public static void CallOutPrincipals(this EnvironmentController ec, Cell spot, float speedMultiplier = 8f, bool whistleCall = true, bool ignoreNormalPrincipal = false)
		{
			foreach (var n in ec.Npcs)
			{
				if (n.Navigator.enabled && n.IsAPrincipal())
				{
					if (n is Principal pr)
					{
						if (ignoreNormalPrincipal)
							continue;

						if (whistleCall)
						{
							pr.audMan.FlushQueue(true);
							pr.audMan.PlaySingle(pr.audComing);
						}
					}
					n.navigationStateMachine.ChangeState(new NavigationState_FollowToSpot(n, spot, speedMultiplier));
				}
			}
		}

		public static bool IsAPrincipal(this NPC n)
		{
			var dat = n.GetComponent<INPCPrefab>();
			return n.Character == Character.Principal || (dat != null && dat.ReplacesCharacter(Character.Principal));
		}

		public static IEnumerator FadeOutLightning(this SpriteRenderer renderer, EnvironmentController ec, float delay, float fadeFactor)
		{
			while (delay > 0f)
			{
				delay -= ec.EnvironmentTimeScale * Time.deltaTime;
				yield return null;
			}

			Color color = renderer.color;
			while (true)
			{
				color.a -= ec.EnvironmentTimeScale * Time.deltaTime * fadeFactor;
				if (color.a <= 0f)
				{
					Object.Destroy(renderer.gameObject);
					yield break;
				}
				renderer.color = color;

				yield return null;
			}
		}

		const int defaultDetentionNoise = 95;
	}
}
