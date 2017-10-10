using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Manager : MonoBehaviour {

    private ArithemeticObject mSelectedObject = null;  
    List<ArithemeticObject> mAttachableObjects = null;
	public float THRESHOLD_FORATTACH = 0.1f;
	private bool mIsDraggingAttachable = false;
	int arithMeticObjectLayerMask;

	void Start () {
	
		ArithemeticObject[] objectArray = FindObjectsOfType<ArithemeticObject>();
		mAttachableObjects = new List<ArithemeticObject>(objectArray);	
		arithMeticObjectLayerMask = 1 << LayerMask.NameToLayer("ArithemticObject");
	}
	
	// Update is called once per frame
	void Update () 
    {
	   UpdateObjectSelected();
       
       UpdateObjectDragged();
       
	   UpdateObjectDropped();
	}

	bool IsTypeAttachable (ArithemeticObject.EArithemeticObjectType type)
	{
		if(type == ArithemeticObject.EArithemeticObjectType.UNIT || type == ArithemeticObject.EArithemeticObjectType.TEN || type == ArithemeticObject.EArithemeticObjectType.HUNDRED || type == ArithemeticObject.EArithemeticObjectType.THOUSAND)
			return true;
		return false;
	}

	void UpdateObjectSelected ()
	{
		if (mSelectedObject == null && Input.GetMouseButtonDown (0)) {
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit hitinfo;
			if (Physics.Raycast (ray, out hitinfo, float.MaxValue, arithMeticObjectLayerMask)) {
				GameObject hitObj = hitinfo.transform.gameObject;
				Debug.LogWarning (" object is hit");
				GameObject parentObject = hitObj;
				if (hitObj.transform.root != null) {
					parentObject = hitObj.transform.root.gameObject;
				}
				ArithemeticObject arithObj = parentObject.GetComponent<ArithemeticObject> ();
				mSelectedObject = arithObj;
				Debug.LogWarning ("Object has been selected");
				if (IsTypeAttachable (arithObj.mType)) {
					mIsDraggingAttachable = true;
					mAttachableObjects.Remove (mSelectedObject);
					Collider collider = parentObject.GetComponent<Collider> ();
					collider.enabled = false;
					//make sure the raycast while dragging does not hit this
				}
				else {
					mIsDraggingAttachable = false;
				}
			}
		}
	}

	void UpdateObjectDragged ()
	{
		if (mSelectedObject != null && Input.GetMouseButton (0)) {
			Debug.LogWarning (" Dragging the selected object ");
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit hitinfo;
			if (Physics.Raycast (ray, out hitinfo, float.MaxValue, ~arithMeticObjectLayerMask)) {
				float offset = 0.25f;
				Vector3 pointInCamSpace = Camera.main.transform.InverseTransformPoint (hitinfo.point);
				Vector3 screenPos = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, pointInCamSpace.z - offset);
				Vector3 worldPos = Camera.main.ScreenToWorldPoint (screenPos);
				mSelectedObject.transform.position = worldPos;
			}
		}
	}

	void UpdateObjectDropped ()
	{
		if (mSelectedObject != null && Input.GetMouseButtonUp (0)) {
			Debug.LogWarning (" Releasing the selected object ");
			if (mIsDraggingAttachable) {
				mIsDraggingAttachable = false;
				mSelectedObject.collider.enabled = true;
				List<Vector3> contactPointsInSelObj = mSelectedObject.GetContactPoints ();
				float leastDistance = float.MaxValue;
				ArithemeticObject nearestObj = null;
				Vector3 nearestContactPoint = Vector3.zero;
				foreach (ArithemeticObject arObj in mAttachableObjects) {
					List<Vector3> contactPoints = arObj.GetContactPoints ();
					foreach (Vector3 contactPoint in contactPoints) {
						foreach (Vector3 contactPointInSelObj in contactPointsInSelObj) {
							float distance = Vector3.Distance (contactPoint, contactPointInSelObj);
							if (distance <= THRESHOLD_FORATTACH && distance < leastDistance) {
								nearestObj = arObj;
								leastDistance = distance;
								nearestContactPoint = contactPoint;
							}
						}
					}
				}
				if (nearestObj != null && nearestObj.mType == ArithemeticObject.EArithemeticObjectType.UNIT) {
					//create a parent, and attach units to it.
					GameObject unitsRod = new GameObject ("UnitsRod", typeof(ArithemeticObject));
					unitsRod.layer = LayerMask.NameToLayer ("ArithemticObject");
					ArithemeticObject unitsObj = unitsRod.GetComponent<ArithemeticObject> ();
					unitsObj.mType = ArithemeticObject.EArithemeticObjectType.UNITSROD;
					unitsRod.transform.position = nearestObj.transform.position;
					unitsObj.Attach (nearestObj);
					unitsObj.Attach (mSelectedObject, nearestContactPoint);
					mAttachableObjects.Add (unitsObj);
					mAttachableObjects.Remove (nearestObj);
				}
				else
					if (nearestObj != null && nearestObj.mType == ArithemeticObject.EArithemeticObjectType.UNITSROD) {
						nearestObj.Attach (mSelectedObject, nearestContactPoint);
					}
					else {
						mAttachableObjects.Add (mSelectedObject);
					}
			}
			mSelectedObject = null;
		}
	}
}
 