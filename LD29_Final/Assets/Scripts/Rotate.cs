using UnityEngine;
using System.Collections;

public class Rotate : MonoBehaviour {

	public float rotationVel=0.0f;
	public float maxVel = 300.0f;
	public float rotationAccel=3.0f;
	public float rotationBraking=10.0f;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		float inputHorz = Input.GetAxis("Horizontal");
		rotationVel += inputHorz * rotationAccel * Time.deltaTime;

		if (inputHorz == 0.0f &&
		    rotationVel != 0.0f)
		{
			float curBraking = rotationBraking * Time.deltaTime;
			if( (rotationVel > 0 &&
			     rotationVel - curBraking < 0) ||
			   (rotationVel < 0 &&
			 rotationVel + curBraking > 0 ))
			{
				rotationVel = 0.0f;
			}
			else
			{
				rotationVel += ( (rotationVel > 0) ? -curBraking : curBraking );
			}

		}
		if (rotationVel != 0)
		{
			rotationVel = (rotationVel > 0) ? Mathf.Min(rotationVel, maxVel) : Mathf.Max(rotationVel, -maxVel);
		}

		transform.Rotate( Vector3.forward * Time.deltaTime * rotationVel);
	}
}
