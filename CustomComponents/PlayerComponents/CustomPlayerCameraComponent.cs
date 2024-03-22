using BBTimes.Misc.Modifiers;
using System.Collections.Generic;
using UnityEngine;

namespace BBTimes.CustomComponents.PlayerComponents
{
    public class CustomPlayerCameraComponent : MonoBehaviour
	{
		private void Awake()
		{
			cam = GetComponent<GameCamera>();

			if (cam == null)
			{
				Debug.LogError("BB TIMES: Failed to apply CustomPlayerCameraComponent into the GameCamera, the component doesn't exist");
				DestroyImmediate(gameObject);
				return;
			}

			defaultFov = cam.billboardCam.fieldOfView;
		}

		private void Update()
		{
			float fov = defaultFov;

			if (fovModifiers.Count != 0) // Can't be -1 lol
				for (int i = 0; i < fovModifiers.Count; i++)
					fov += fovModifiers[i].Mod;
				

			fov = Mathf.Clamp(fov, 0f, 125f);

			cam.billboardCam.fieldOfView = fov;
			cam.camCom.fieldOfView = fov;
		}

		private GameCamera cam;

		private float defaultFov;

		public List<BaseModifier> fovModifiers = [];
	}
}
