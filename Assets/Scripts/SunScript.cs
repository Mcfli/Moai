using System;
using UnityEngine;

namespace UnityStandardAssets.Utility
{
    public class SunScript : MonoBehaviour
    {
        public string buttonName;
        public Vector3andSpace normalRotateDegreesPerSecond;
        public Vector3andSpace pressedRotateDegreesPerSecond;
        public Vector3andSpace moveUnitsPerSecond;
        public bool ignoreTimescale;
        private float m_LastRealTime;
        private bool speed;
        private Light light;

        private void Start()
        {
            m_LastRealTime = Time.realtimeSinceStartup;
            speed = false;
            light = GetComponent<Light>();
        }


        // Update is called once per frame
        private void Update()
        {
            float deltaTime = Time.deltaTime;
            if (ignoreTimescale)
            {
                deltaTime = (Time.realtimeSinceStartup - m_LastRealTime);
                m_LastRealTime = Time.realtimeSinceStartup;
            }
            transform.Translate(moveUnitsPerSecond.value * deltaTime, moveUnitsPerSecond.space);
            if (speed) transform.Rotate(pressedRotateDegreesPerSecond.value * deltaTime, moveUnitsPerSecond.space);
            else transform.Rotate(normalRotateDegreesPerSecond.value * deltaTime, moveUnitsPerSecond.space);

            if (Input.GetButton(buttonName)) speed = true;
            else speed = false;

            //light.intensity = ((transform.eulerAngles.x+90)%180)/180;
            if (transform.eulerAngles.x < 180) light.intensity = -Math.Abs(transform.eulerAngles.x - 90) / 90 + 1;
            else light.intensity = 0;
        }


        [Serializable]
        public class Vector3andSpace
        {
            public Vector3 value;
            public Space space = Space.Self;
        }
    }
}
