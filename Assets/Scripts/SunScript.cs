using System;
using UnityEngine;

namespace UnityStandardAssets.Utility
{
    public class SunScript : MonoBehaviour{
        public float timeDivider = 100; //bigger it is, slower it moves - 100 is about 30min on Brian's computer
		public float maxIntensity = 1;
		public GameObject Moon; //moon should be attached to sun
		
		private Vector3 originalPosition;

        private void Start(){
			originalPosition = transform.eulerAngles;
        }


        // Update is called once per frame
        private void Update(){
			Vector3 newRotation = new Vector3(Mathf.Repeat(Globals.time/Globals.time_resolution/timeDivider + originalPosition.x, 360), originalPosition.y, originalPosition.z);
			transform.eulerAngles = newRotation;

            // Modulate intensity
            if (transform.eulerAngles.x < 180){
				GetComponent<Light>().intensity = maxIntensity;
				//GetComponent<Light>().intensity = (-Math.Abs(transform.eulerAngles.x - 90) / 90 + 1) * maxIntensity;
				Moon.GetComponent<Light>().intensity = 0.0f;
			}else{
				GetComponent<Light>().intensity = 0.0f;
				Moon.GetComponent<Light>().intensity = 0.5f;
			}
        }

    }
}