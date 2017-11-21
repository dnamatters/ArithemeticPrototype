using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ArithemeticObject : MonoBehaviour {
    
    [HideInInspector]
	public List<ArithemeticObject> mChildObjects = new List<ArithemeticObject>();
	
	//public EArithemeticObjectType mType;
	
	protected List<Vector3> mContactPointsInLocalSpace = new List<Vector3>();
	
	public ArithemeticObject parentArithemticObject;

    public bool mIsComplete;

//	public enum EArithemeticObjectType // [TODO] take this out and replace it with inheritance, there should be a different class for each type of object
//	{
//		UNIT,
//		UNITSROD,
//		TEN,
//		TENSPLATE,
//		HUNDRED,
//		HUNDREDSCUBE,
//		THOUSAND,
//		THOUSANDSROD
//	
//	}
	
	protected virtual void Start()
	{
//		if(mType == EArithemeticObjectType.UNIT)
//		{
//			float cubeHalfEdge = 0.5f;
//			mContactPointsInLocalSpace.Add(new Vector3(0,0,cubeHalfEdge));
//			mContactPointsInLocalSpace.Add(new Vector3(0,0,-cubeHalfEdge));
//			mContactPointsInLocalSpace.Add(new Vector3(-cubeHalfEdge,0,0));
//			mContactPointsInLocalSpace.Add(new Vector3(cubeHalfEdge,0,0));
//			mContactPointsInLocalSpace.Add(new Vector3(0,cubeHalfEdge,0));
//			mContactPointsInLocalSpace.Add(new Vector3(0,-cubeHalfEdge,0));
//		}
//		else if(mType == EArithemeticObjectType.TEN || mType == EArithemeticObjectType.HUNDRED)
//		{
//			ArithemeticObject[] childArray = transform.GetComponentsInChildren<ArithemeticObject> ();
//			mChildObjects.AddRange (childArray);
//			UpdateContactPoints ();
//		}
		if(transform.parent != null)
		{
			parentArithemticObject = transform.parent.gameObject.GetComponent<ArithemeticObject>();
		}
    }
    
	
	public virtual List<Vector3> GetContactPoints()
	{
		List<Vector3> contactPointsList = new List<Vector3>();
		
		foreach(Vector3 contactPointLocal in mContactPointsInLocalSpace)
		{
			contactPointsList.Add(transform.TransformPoint(contactPointLocal));
        }
        return contactPointsList;
		
	}
	
	
	public virtual void Attach (ArithemeticObject obj)
	{
		//this should be used only when this is the first child to be added to the parent
		mChildObjects.Add(obj);
		obj.transform.parent = transform;
	}
	
	public virtual void Attach (ArithemeticObject obj, Vector3 contactPoint)
	{
				
		Vector3 direction = (contactPoint - transform.position);
		direction.Normalize();
		Vector3 newPosition = contactPoint + direction * 0.125f;
		obj.transform.position = newPosition;
		obj.transform.parent = transform;
		mChildObjects.Add(obj);
		
		//now offset the parent and children so that the parent is in the middle again.	
		Vector3 newParentPosition = GetCenterPositionForObject();
		
		Vector3 positionOffset = newParentPosition - transform.position;
		
		OffsetAll(-positionOffset);
		
		transform.position = newParentPosition;
			
		if(mChildObjects.Count == 2)
		{
			Vector3 contactPointBehind = contactPoint + -direction * 2 * 0.125f;
			mContactPointsInLocalSpace.Add(transform.InverseTransformPoint(contactPointBehind));
			Vector3 contactPointFront =  contactPoint + direction * 2 * 0.125f;
			mContactPointsInLocalSpace.Add(transform.InverseTransformPoint(contactPointFront));
        }
        else
        {
        	Vector3 contactPointInLocalSpace = transform.InverseTransformPoint(contactPoint);
			int index =	mContactPointsInLocalSpace.FindIndex((point) => point == contactPointInLocalSpace);
			Vector3 contactPointNew =  contactPoint + direction * 2 * 0.125f;
						mContactPointsInLocalSpace[index] = transform.InverseTransformPoint(contactPointNew);	
        }    
	} 

	public Vector3 GetCenterPositionForObject ()
	{
		int objectCounter = 0;
		Vector3 aggregatePosition = Vector3.zero;
		
		foreach(ArithemeticObject obj in mChildObjects)
		{
			aggregatePosition += obj.transform.position;
			objectCounter++;		
		}
		return aggregatePosition / objectCounter;
	}
	
	protected virtual void OffsetAll(Vector3 positionOffset)
	{
		foreach(ArithemeticObject obj in mChildObjects)
		{
			obj.transform.position = obj.transform.position + positionOffset;
		}	
		
		int count = mContactPointsInLocalSpace.Count;
		for(int idx = 0; idx < count ; idx++)
		{
			mContactPointsInLocalSpace[idx] += positionOffset;
		}

	}
	
	public int GetChildCount()
	{
		return mChildObjects.Count;
	}
	
	public void OnDrawGizmos()
	{
		List<Vector3> contactPointsInWorldSpace = GetContactPoints();
		foreach(Vector3 contactPoint in contactPointsInWorldSpace )
		{
			Gizmos.DrawSphere(contactPoint, 0.02f);
		}
	}
	
	public void SetArithemeticObjectParent(ArithemeticObject parent)
	{
		parentArithemticObject = parent;
	}
	
	public ArithemeticObject GetArithemeticObjectParent()
	{
		if(parentArithemticObject != null)
			return parentArithemticObject.GetArithemeticObjectParent();
		else
			return this; //if this does not have a parent then this is the parent.
	}

	public virtual void UpdateContactPoints ()
	{
	}	

}
