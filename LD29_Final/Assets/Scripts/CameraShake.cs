using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour {

    public Vector3 originPosition;
    public Quaternion originRotation;
    
    public float shake_duration;
    public float shake_intensity;
    
    public void Update(){
        if(shake_intensity > 0){
            transform.position = originPosition + Random.insideUnitSphere * shake_intensity;
            transform.rotation =  new Quaternion(
                originRotation.x + Random.Range(-shake_intensity,shake_intensity)*.2f,
                originRotation.y + Random.Range(-shake_intensity,shake_intensity)*.2f,
                originRotation.z + Random.Range(-shake_intensity,shake_intensity)*.2f,
                originRotation.w + Random.Range(-shake_intensity,shake_intensity)*.2f);
            shake_intensity -= shake_duration * Time.deltaTime;
        }
    }
    
    public void Shake(float decay, float intensity){
        shake_duration = decay;
        shake_intensity = intensity;
        originPosition = transform.position;
        originRotation = transform.rotation;
    }
}