using BBTimes.CustomContent.Objects;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomComponents
{
	public class EnvironmentControllerData : MonoBehaviour
	{
		public List<BeltManager> ConveyorBelts = [];
		public List<Duct> Vents = [];
		public List<Trapdoor> Trapdoors = [];
		public List<SecurityCamera> Cameras = [];
		public List<Squisher> Squishers = [];
		public List<LightSwitch> LightSwitches = [];
		public List<Coroutine> OngoingEvents = [];
	}
}
