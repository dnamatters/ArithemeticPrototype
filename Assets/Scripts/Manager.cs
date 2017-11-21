using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Manager : MonoBehaviour {

    private ArithemeticObject mSelectedObject = null;  
	List<ArithemeticObject> mAttachableObjects  = new List<ArithemeticObject>();
	List<ArithemeticObject> mAllObjects = new List<ArithemeticObject>();
	public float THRESHOLD_FORATTACH = 0.1f;
	private bool mIsDraggingAttachable = false;
	public Transform arithemeticObjectParent;
	int arithMeticObjectLayerMask;
	int ignoreRaycastLayerMask;
	int arithMeticObjectLayer;
	int ignoreRaycastLayer;

	void Start () 
	{
		SetAllArithemeticObjects();
		arithMeticObjectLayer = LayerMask.NameToLayer("ArithemticObject");
		ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
		arithMeticObjectLayerMask = 1 << arithMeticObjectLayer;
		ignoreRaycastLayerMask = 1 << ignoreRaycastLayer;
	}
	
	void Update () 
    {
	   UpdateObjectSelected();
       UpdateObjectDragged();  
	   UpdateObjectDropped();
	}
				
	void UpdateObjectSelected ()
	{
		if (mSelectedObject == null && Input.GetMouseButtonDown (0)) {
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit hitinfo;
			if (Physics.Raycast (ray, out hitinfo, float.MaxValue, arithMeticObjectLayerMask)) {
				GameObject hitObj = hitinfo.transform.gameObject;
				Debug.LogWarning (" object is hit");
				ArithemeticObject arObj = hitObj.GetComponent<ArithemeticObject>();
				mSelectedObject = arObj.GetArithemeticObjectParent();
				GameObject parentObject = mSelectedObject.gameObject;
				parentObject.layer = ignoreRaycastLayer;
				SetLayerRecursively(parentObject, ignoreRaycastLayer);
				
				Debug.LogWarning ("Object has been selected");
				PopulateAttachableObjectsForType(mSelectedObject);
				//populate attachable objects here as per the type selected.
				
                if (mSelectedObject.mIsComplete)
				{
					mIsDraggingAttachable = true;	
				}
				else 
				{
					mIsDraggingAttachable = false;
				}
			}
		}
	}

	void PopulateAttachableObjectsForType (ArithemeticObject arObj)
	{
		
		mAttachableObjects.Clear();
		if (arObj.GetType() == typeof(UnitsObject)) 
		{
			mAttachableObjects.AddRange(mAllObjects.FindAll((obj) => obj.GetType() == typeof(UnitsObject)));
            mAttachableObjects.AddRange(mAllObjects.FindAll((obj) => obj.GetType() == typeof(TensObject) && obj.mIsComplete == false));
		}
		else if (arObj.GetType() == typeof(TensObject)) 
		{
			mAttachableObjects.AddRange(mAllObjects.FindAll((obj) => obj.GetType() == typeof(TensObject)));
            mAttachableObjects.AddRange(mAllObjects.FindAll((obj) => obj.GetType() == typeof(HundrededObject) && obj.mIsComplete == false));
		}
        else if (arObj.GetType() == typeof(HundrededObject)) 
        {
            mAttachableObjects.AddRange(mAllObjects.FindAll((obj) => obj.GetType() == typeof(HundrededObject)));
            mAttachableObjects.AddRange(mAllObjects.FindAll((obj) => obj.GetType() == typeof(ThousandsObject) && obj.mIsComplete == false));
        }
		mAttachableObjects.Remove(arObj);//make sure you don't do any proximity checks against the selected object itself.
	}

	void UpdateObjectDragged ()
	{
		if (mSelectedObject != null && Input.GetMouseButton (0)) {
			Debug.LogWarning (" Dragging the selected object ");
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit hitinfo;
			if (Physics.Raycast (ray, out hitinfo, float.MaxValue, ~ignoreRaycastLayerMask)) {
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
				//mSelectedObject.collider.enabled = true;
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
                if (nearestObj != null && nearestObj.GetType() == typeof(UnitsObject)) {
					//create a parent, and attach units to it.
                    GameObject unitsRod = new GameObject ("TensObj", typeof(TensObject));
					unitsRod.layer = LayerMask.NameToLayer ("ArithemticObject");
					ArithemeticObject unitsObj = unitsRod.GetComponent<ArithemeticObject> ();
                    unitsRod.transform.parent = arithemeticObjectParent;
					unitsRod.transform.position = nearestObj.transform.position;
					unitsObj.Attach (nearestObj);
					unitsObj.Attach (mSelectedObject, nearestContactPoint);
					nearestObj.SetArithemeticObjectParent(unitsObj);
					mSelectedObject.SetArithemeticObjectParent(unitsObj);
					
					mAllObjects.Remove (nearestObj);
					mAllObjects.Remove (mSelectedObject);
					mAllObjects.Add (unitsObj);
				}
                else if (nearestObj != null && (nearestObj.GetType() == typeof(TensObject) ||  nearestObj.GetType() == typeof(HundrededObject)) && nearestObj.mIsComplete == false) 
				{
					nearestObj.Attach (mSelectedObject, nearestContactPoint);
					mSelectedObject.SetArithemeticObjectParent(nearestObj);
					mAllObjects.Remove (mSelectedObject);
					if(nearestObj.GetChildCount() == 10)//this one is complete , make it into a next denomimation object
					{
                        nearestObj.UpdateContactPoints();
                        nearestObj.mIsComplete = true;
					}
					
				}
                else if (nearestObj != null && nearestObj.GetType() == typeof(TensObject)) 
				{	
                    GameObject tensPlate = new GameObject ("HundredsObj", typeof(HundrededObject));
					tensPlate.layer = LayerMask.NameToLayer ("ArithemticObject");
					ArithemeticObject tensObj = tensPlate.GetComponent<ArithemeticObject> ();
					tensPlate.transform.parent = arithemeticObjectParent;
					mAllObjects.Add(tensObj);
					tensPlate.transform.position = nearestObj.transform.position;
					tensObj.Attach(nearestObj);
					tensObj.Attach(mSelectedObject, nearestContactPoint);
					
					nearestObj.SetArithemeticObjectParent(tensObj);
					mSelectedObject.SetArithemeticObjectParent(tensObj);
					
					mAllObjects.Remove (nearestObj);
					mAllObjects.Remove (mSelectedObject);
				}
                else if (nearestObj != null && nearestObj.GetType() == typeof(HundrededObject)) 
				{	
                    GameObject tensPlate = new GameObject ("ThousandsObj", typeof(ThousandsObject));
					tensPlate.layer = LayerMask.NameToLayer ("ArithemticObject");
					ArithemeticObject tensObj = tensPlate.GetComponent<ArithemeticObject> ();
					tensPlate.transform.parent = arithemeticObjectParent;
					mAllObjects.Add(tensObj);
					tensPlate.transform.position = nearestObj.transform.position;
					tensObj.Attach(nearestObj);
					
					tensObj.Attach(mSelectedObject, nearestContactPoint);
					
					nearestObj.SetArithemeticObjectParent(tensObj);
					mSelectedObject.SetArithemeticObjectParent(tensObj);
					
					mAllObjects.Remove (nearestObj);
					mAllObjects.Remove (mSelectedObject);
				}
				
			}
			
			mSelectedObject.gameObject.layer =  arithMeticObjectLayer;
			SetLayerRecursively(mSelectedObject.gameObject, arithMeticObjectLayer);
			mSelectedObject = null;
		}
	}
       
	
	void SetLayerRecursively(GameObject parentObj, int layer)
	{
		Transform parentTransform = parentObj.transform;
		int childCount = parentTransform.childCount;
		
		for(int idx = 0 ; idx < childCount; idx++)
		{
			Transform child = parentTransform.GetChild(idx);
			child.gameObject.layer = layer;
			SetLayerRecursively(child.gameObject, layer);
		}
	}

	void SetAllArithemeticObjects ()
	{
		if(arithemeticObjectParent != null)
		{
			int childCount = arithemeticObjectParent.childCount;
			
			for(int idx = 0 ; idx < childCount ; idx++)
			{
				 Transform child = arithemeticObjectParent.GetChild(idx);
				 mAllObjects.Add (child.GetComponent<ArithemeticObject>());
			}
		}
	}
}
 