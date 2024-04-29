﻿using BBTimes.CustomContent.Objects;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomComponents
{
	public class EnvironmentControllerData : MonoBehaviour
	{
		public List<BeltManager> ConveyorBelts = [];
		public List<Vent> Vents = [];
		public List<Trapdoor> Trapdoors = [];
	}
}
