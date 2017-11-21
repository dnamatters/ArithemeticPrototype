using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HundrededObject : ArithemeticObject {

	// Use this for initialization
	protected override void Start () 
	{
		ArithemeticObject[] childArray = transform.GetComponentsInChildren<ArithemeticObject> ();
		mChildObjects.AddRange (childArray);
		UpdateContactPoints ();
	}
	
	public override void UpdateContactPoints () 
	{
		float cubeHalfEdge = 0.125f;
		mContactPointsInLocalSpace.Clear();

		Vector3 direction1 = transform.position - mChildObjects[2].transform.position;
		direction1.Normalize ();
		Vector3 localspaceDirection1 = transform.InverseTransformDirection (direction1);

		ArithemeticObject childObject = mChildObjects[0];

		Vector3 direction2 = childObject.mChildObjects[0].transform.position - childObject.mChildObjects[2].transform.position;
		direction2.Normalize ();
		Vector3 localspaceDirection2 = transform.InverseTransformDirection (direction2);

		if (Vector3.Dot(localspaceDirection1, Vector3.forward) == 0 && Vector3.Dot(localspaceDirection2, Vector3.forward) == 0) {
				mContactPointsInLocalSpace.Add (new Vector3 (0, 0, cubeHalfEdge));
				mContactPointsInLocalSpace.Add (new Vector3 (0, 0, -cubeHalfEdge));
		}
		if (Vector3.Dot(localspaceDirection1, Vector3.right) == 0 && Vector3.Dot(localspaceDirection2, Vector3.right) == 0){
				mContactPointsInLocalSpace.Add (new Vector3 (-cubeHalfEdge, 0, 0));
				mContactPointsInLocalSpace.Add (new Vector3 (cubeHalfEdge, 0, 0));
		}
		if (Vector3.Dot(localspaceDirection1, Vector3.up) == 0 && Vector3.Dot(localspaceDirection2, Vector3.up) == 0) {
				mContactPointsInLocalSpace.Add (new Vector3 (0, cubeHalfEdge, 0));
				mContactPointsInLocalSpace.Add (new Vector3 (0, -cubeHalfEdge, 0));
		}
	}
}
