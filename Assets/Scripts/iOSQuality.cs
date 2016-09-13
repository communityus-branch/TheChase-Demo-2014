using UnityEngine;
using System.Collections;

public class iOSQuality : MonoBehaviour {

	void Awake () {
		#if UNITY_IPHONE
			switch (iPhone.generation)
			{
				case iPhoneGeneration.iPhone4:
				case iPhoneGeneration.iPodTouch4Gen:
					Destroy(GameObject.Find("Flare_HoverBikeMaleFrontRight"));
					Destroy(GameObject.Find("Flare_HoverBikeFemaleFrontRight"));
					Destroy(GameObject.Find("Effects"));
				break;
				case iPhoneGeneration.iPhone:
				case iPhoneGeneration.iPhone3G:
				case iPhoneGeneration.iPhone3GS:
				case iPhoneGeneration.iPodTouch1Gen:
				case iPhoneGeneration.iPodTouch2Gen:
				case iPhoneGeneration.iPodTouch3Gen:
				case iPhoneGeneration.iPad1Gen:
					Debug.Log("iPhone 1st, iPhone3G, iPhone3GS, iPod1, iPod2, iPod3, iPad1 are not supported. Quitting application.");
					Application.Quit();
				break;
			}
		#endif
	}
}
