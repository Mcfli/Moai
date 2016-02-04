using System;
using UnityEngine;

namespace UnityStandardAssets.Utility
{
    public class PickupObject : MonoBehaviour{
		public string leftHand = "LeftHand";
		public string rightHand = "RightHand";
		GameObject hand1;
		float hand1size;
		GameObject hand2;
		float hand2size;

		// Use this for initialization
		void Start () {
		
		}
		
		GameObject GetMouseHoverObject(float range){
			Vector3 position = gameObject.transform.position;
			RaycastHit raycastHit;
			Vector3 target = position + Camera.main.transform.forward * range;
			if(Physics.Linecast(position,target,out raycastHit)) return raycastHit.collider.gameObject;
			return null;
		}
		
		bool CanGrab(GameObject obj){
			return obj.GetComponent<Rigidbody>() != null;
		}
		
		void TryGrabObject1(GameObject obj){
			if(obj == null || !CanGrab(obj)) return;
			hand1 = obj;
			Physics.IgnoreCollision(hand1.GetComponent<Collider>(), GetComponent<Collider>());
			hand1size = obj.GetComponent<Renderer>().bounds.size.magnitude;
			
		}
		
		void DropObject1(){
			if(hand1 == null) return;
			
			if(hand1.GetComponent<Rigidbody>() != null){
				hand1.GetComponent<Rigidbody>().velocity = new Vector3(0,0,0);
				Physics.IgnoreCollision(hand1.GetComponent<Collider>(), GetComponent<Collider>(), false);
			}
			
			hand1 = null;
		}
		
		
		void TryGrabObject2(GameObject obj){
			if(obj == null || !CanGrab(obj)) return;
			hand2 = obj;
			Physics.IgnoreCollision(hand2.GetComponent<Collider>(), GetComponent<Collider>());
			hand2size = obj.GetComponent<Renderer>().bounds.size.magnitude;
			
		}
		
		void DropObject2(){
			if(hand2 == null) return;
			
			if(hand2.GetComponent<Rigidbody>() != null){
				hand2.GetComponent<Rigidbody>().velocity = new Vector3(0,0,0);
				Physics.IgnoreCollision(hand2.GetComponent<Collider>(), GetComponent<Collider>(), false);
			}
			
			hand2 = null;
		}
		
		// Update is called once per frame
		void Update () {
			//Debug.Log(GetMouseHoverObject(5)); //return what you're looking at
			
			if(Input.GetButtonDown(leftHand)){
				if(hand1 == null) TryGrabObject1(GetMouseHoverObject(5));
				else DropObject1();
			}
			
			if(hand1 != null){
				Vector3 newPosition = (gameObject.transform.position + Vector3.Lerp(Camera.main.transform.forward, Camera.main.transform.right, 0.3f)* hand1size);
				hand1.transform.position = newPosition;
				hand1.transform.forward = Camera.main.transform.forward;
			}
			
			if(Input.GetButtonDown(rightHand)){
				if(hand2 == null) TryGrabObject2(GetMouseHoverObject(5));
				else DropObject2();
			}
			
			if(hand2 != null){
				Vector3 newPosition = (gameObject.transform.position + Vector3.Lerp(Camera.main.transform.forward, Camera.main.transform.right * -1, 0.3f) * hand2size);
				hand2.transform.position = newPosition;
				hand2.transform.forward = Camera.main.transform.forward;
			}
			
		}
	}
}
