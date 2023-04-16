using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameUtils.CameraUtils {

    // Measures the camera size and bounds in world units
    public class MeasureCameraWorldSpace : MonoBehaviour
    {
        Camera myCamera;

        public bool printMeasurements = true;

        public Vector2 size {get; private set;}
        public Vector2 xBounds {get; private set;}
        public Vector2 yBounds {get; private set;}

        void Awake() {
            // Calculates camera size
            myCamera = gameObject.GetComponent<Camera>();
            float height = myCamera.orthographicSize * 2;
            float width = height * myCamera.aspect;
            size = new Vector2(width, height);
            // Calculates camera bounds depending on camera position and size
            xBounds = new Vector2(transform.position.x - width/2, transform.position.x + width/2);
            yBounds = new Vector2(transform.position.y - height/2, transform.position.y + height/2);
            if (printMeasurements) {
                string s = "Size: " + size + "\n";
                s += "x Bounds: " + xBounds + "\n";
                s += "y Bounds: " + yBounds + "\n";
                Debug.Log(s);
            }
            
        }
    }
}