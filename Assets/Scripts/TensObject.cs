using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TensObject : ArithemeticObject {

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

		Vector3 direction = transform.position - mChildObjects[2].transform.position;
		direction.Normalize ();
		Vector3 localSpaceDirection = transform.InverseTransformDirection (direction);
		if (localSpaceDirection != Vector3.back && localSpaceDirection != Vector3.forward) {
				mContactPointsInLocalSpace.Add (new Vector3 (0, 0, cubeHalfEdge));
				mContactPointsInLocalSpace.Add (new Vector3 (0, 0, -cubeHalfEdge));
		}
		if (localSpaceDirection != Vector3.right && localSpaceDirection != Vector3.left) {
				mContactPointsInLocalSpace.Add (new Vector3 (-cubeHalfEdge, 0, 0));
				mContactPointsInLocalSpace.Add (new Vector3 (cubeHalfEdge, 0, 0));
		}
		if (localSpaceDirection != Vector3.up && localSpaceDirection != Vector3.down) {
				mContactPointsInLocalSpace.Add (new Vector3 (0, cubeHalfEdge, 0));
				mContactPointsInLocalSpace.Add (new Vector3 (0, -cubeHalfEdge, 0));
		}
	}					
}
