using BBTimes.CustomComponents;
using BBTimes.CustomContent.Objects;
using BBTimes.Extensions;
using BBTimes.Extensions.ObjectCreationExtensions;
using MTM101BaldAPI;
using UnityEngine;

namespace BBTimes.CustomContent.Builders
{
	public class Structure_NotebookMachine : StructureBuilder, IBuilderPrefab
	{

		public StructureWithParameters SetupBuilderPrefabs()
		{
			var ntbMachineRenderer = this.GetModel("NotebookMachine", true, false, out var renderer).transform;
			ntbMachineRenderer.gameObject.ConvertToPrefab(true);

			ntbMachinePre = ntbMachineRenderer.gameObject.AddComponent<NotebookMachine>();
			ntbMachinePre.boxRenderer = renderer.transform.Find("NtbkMachine_Base").GetComponent<MeshRenderer>();

			// Get the "Power On" indicator
			ntbMachinePre.offMat = [ntbMachinePre.boxRenderer.materials[0], ntbMachinePre.boxRenderer.materials[1]];

			ntbMachinePre.onMat = [
			ntbMachinePre.offMat[0],
			new(ntbMachinePre.offMat[1])
			{
				name = "NotebookMachine_OnPower",
				mainTexture = this.GetTexture("NtbMachine_On.png")
			}];

			ntbMachinePre.capsuleRenderer = renderer.transform.Find("NtbkMachine_Capsule").gameObject;

			// Map Icon for Notebook
			ntbMachinePre.spriteMapIcon = this.GetSprite(ObjectCreationExtension.defaultMapIconPixelsPerUnit, "NtbkMachine_MapIcon.png");


			return new() { prefab = this, parameters = null }; // 0 = Amount of cameras, 1 = minMax distance for them
		}
		public void SetupPrefab() { }
		public void SetupPrefabPost() { }

		public string Name { get; set; }
		public string Category => "objects";

		public override void OnGenerationFinished(LevelBuilder lb)
		{
			base.OnGenerationFinished(lb);
			foreach (var room in lb.Ec.rooms)
			{
				if (room.hasActivity && room.activity is NoActivity) // No Activity is from Notebooks
				{
					var machine = Instantiate(ntbMachinePre, room.objectObject.transform);
					machine.transform.position = room.activity.transform.position + Vector3.down * 1.25f;
					var door = room.doors[lb.controlledRNG.Next(room.doors.Count)].transform;

					Vector3 lookAtPos = door.position;
					lookAtPos.y = machine.transform.position.y; // To avoid the machine tilting
					machine.transform.LookAt(lookAtPos, Vector3.up);

					machine.LinkToNotebook(room.activity.notebook, ec);
				}
			}
		}



		[SerializeField]
		internal NotebookMachine ntbMachinePre;
	}
}
