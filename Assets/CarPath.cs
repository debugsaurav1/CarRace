using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarPath : MonoBehaviour
{
	
	public List<Transform> nodes;
	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		for (int i = 0; i < nodes.Count; i++)
		{
			//Gizmos.DrawLine(transform.nodes[i], transform.position.[i+1]);
		}
	}
}
