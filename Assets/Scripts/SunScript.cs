using System;
using UnityEngine;

namespace UnityStandardAssets.Utility
{
    public class SunScript : MonoBehaviour
    {

        public float rot_speed;

        public float maxIntensity;
        //private float m_LastRealTime;
        private Light sunLight;
        private Vector3 rot_vector;

        private void Start()
        {
            rot_vector = new Vector3(rot_speed, 0, 0);
            //m_LastRealTime = Time.realtimeSinceStartup;
            sunLight = GetComponent<Light>();
        }


        // Update is called once per frame
        private void Update()
        {
            rot_vector = new Vector3(rot_speed*Globals.time_scale, 0, 0);
            transform.Rotate(rot_vector * Time.deltaTime,Space.Self);

            // Modulate intensity
            if (transform.eulerAngles.x < 180) sunLight.intensity = (-Math.Abs(transform.eulerAngles.x - 90) / 90 + 1) * maxIntensity;
            else sunLight.intensity = 0;
        }

    }
}
