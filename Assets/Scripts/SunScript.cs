using System;
using UnityEngine;

namespace UnityStandardAssets.Utility
{
    public class SunScript : MonoBehaviour{
		public GameObject Player;
		
        public float timeresPerDegree = 10;
		//bigger it is, slower it moves
		//360 degrees in a day, so
		//1 is 1 degree per update or frame
		//360 updates in a day
		
		public float maxIntensity = 1;
		//public GameObject Moon; //moon should be attached to sun
		
		private Vector3 originalSunAngle;

        private void Start(){
			originalSunAngle = transform.eulerAngles;
        }


        // Update is called once per frame
        private void Update(){
			Vector3 newRotation = new Vector3(Mathf.Repeat(Globals.time/Globals.time_resolution/timeresPerDegree + originalSunAngle.x, 360), originalSunAngle.y, originalSunAngle.z);
			transform.eulerAngles = newRotation;
			transform.position = new Vector3(Player.transform.position.x, 0, Player.transform.position.z);

            // Modulate intensity
            if (transform.eulerAngles.x < 180){
				GetComponent<Light>().intensity = maxIntensity;
				//GetComponent<Light>().intensity = (-Math.Abs(transform.eulerAngles.x - 90) / 90 + 1) * maxIntensity;
				//Moon.GetComponent<Light>().intensity = 0.0f;
			}else{
				GetComponent<Light>().intensity = 0.0f;
				//Moon.GetComponent<Light>().intensity = 0.5f;
			}
        }

    }
}