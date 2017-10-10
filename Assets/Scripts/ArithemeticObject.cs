using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ArithemeticObject : MonoBehaviour {

	private List<ArithemeticObject> mChildObjects = new List<ArithemeticObject>();
	
	public EArithemeticObjectType mType;
	
	private List<Vector3> mContactPointsInLocalSpace = new List<Vector3>();

	public enum EArithemeticObjectType
	{
		UNIT,
		UNITSROD,
		TEN,
		TENSPLATE,
		HUNDRED,
		HUNDREDSCUBE,
		THOUSAND,
		THOUSANDSROD
	
	}
	
	public void Start()
	{
		if(mType == EArithemeticObjectType.UNIT)
		{
			float cubeHalfEdge = 0.5f;
			mContactPointsInLocalSpace.Add(new Vector3(0,0,cubeHalfEdge));
			mContactPointsInLocalSpace.Add(new Vector3(0,0,-cubeHalfEdge));
			mContactPointsInLocalSpace.Add(new Vector3(-cubeHalfEdge,0,0));
			mContactPointsInLocalSpace.Add(new Vector3(cubeHalfEdge,0,0));
			mContactPointsInLocalSpace.Add(new Vector3(0,cubeHalfEdge,0));
			mContactPointsInLocalSpace.Add(new Vector3(0,-cubeHalfEdge,0));
		}
    }
    
	
	public List<Vector3> GetContactPoints()
	{
		List<Vector3> contactPointsList = new List<Vector3>();
		
		foreach(Vector3 contactPointLocal in mContactPointsInLocalSpace)
		{
			contactPointsList.Add(transform.TransformPoint(contactPointLocal));
        }
        return contactPointsList;
		
	}
	
	
	public void Attach (ArithemeticObject obj)
	{
	
		//this should be used only when this is the first child to be added to the parent
		mChildObjects.Add(obj);
		obj.transform.parent = transform;
	}
	
	public void Attach (ArithemeticObject obj, Vector3 contactPoint)
	{
		switch (mType) {
	
		case EArithemeticObjectType.UNITSROD:
		//calculate the local position of the new unit based on the direction in which it is added
//		GameObject debugObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
//		debugObject.transform.position = contactPoint;
//		debugObject.transform.localScale /= 10;
//		
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
		break;
		case EArithemeticObjectType.TENSPLATE:
			break;
		case EArithemeticObjectType.HUNDREDSCUBE:
			break;
		case EArithemeticObjectType.THOUSANDSROD:
			break;
		
		}
	} 

	Vector3 GetCenterPositionForObject ()
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
	
	void OffsetAll(Vector3 positionOffset)
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
		if(mType != EArithemeticObjectType.UNITSROD)
			return;
		List<Vector3> contactPointsInWorldSpace = GetContactPoints();
		foreach(Vector3 contactPoint in contactPointsInWorldSpace )
		{
			Gizmos.DrawSphere(contactPoint, 0.02f);
		}
	}

}
