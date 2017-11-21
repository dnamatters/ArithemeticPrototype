using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitsObject : ArithemeticObject
{

	// Use this for initialization
	protected override void Start () 
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
