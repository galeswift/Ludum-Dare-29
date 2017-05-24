using UnityEngine;
using System.Collections;

public class Brick : MonoBehaviour {
 
    public GameObject breakParticleSystem;
    public GameObject crashParticleSystemTemplate=null;
    public GameObject crashParticleSystem;
	public EBrickTypes type;
	public Vector3[] newVertices;
    public Vector2[] newUV;
	public Color[] newColors;
    public int[] newTriangles;
    public Material newMaterial;
    public Mesh mesh;
	// Radius in degrees
	public float width=50.0f;
    public float height=1.0f;
    public float centerHeight=0.5f;
	public int segments=30;	
    public bool _isCrash=false;
    public bool isBreaking = false;
    public bool isPendingBreak =false;
    public Vector2 origin;

    public bool isCrash
    {
        get
        {
            return this._isCrash;
        }
        set
        {
            this._isCrash = value;
            if( value &&
               crashParticleSystemTemplate != null)
            {
                crashParticleSystem = (GameObject)Instantiate(crashParticleSystemTemplate);
            }
        }
    }
    [SerializeField]
    private Vector2 _pos;
    // X == row, y == column
    public Vector2 pos
    {
        get
        {
            return _pos;
        }
        set
        {
            this._pos = value;
            BuildMesh();
        }
    }

	private float RAD_TO_DEG=57.2957795f;
	private float DEG_TO_RAD=1/57.2957795f;

    void Update()
    {
        breakParticleSystem.transform.position = new Vector3(origin.x, origin.y, 0);
    }

    public void SetPendingBreak()
    {
        if (!isBreaking)
        {
            isPendingBreak = true;
        }
    }
    public void Break()
    {
        if (isPendingBreak)
        {
            if( crashParticleSystem != null )
            {
                Destroy(crashParticleSystem.gameObject);
            }
            mesh.Clear();
            Instantiate(breakParticleSystem, new Vector3(origin.x, origin.y, 0),transform.rotation);
            isPendingBreak = false;
            isBreaking = true;
        }
    }
	public void BuildMesh(bool force=false)
	{
        segments = (int)width;

        if (mesh == null || force)
        {
            mesh = new Mesh();
            newVertices = new Vector3[segments*2];
            newUV = new Vector2[segments*2];
            newColors = new Color[segments * 2];
        }
		mesh.Clear ();
		GetComponent<MeshFilter>().mesh = mesh;
		
    
		float angleStep = (float)width/(segments-1);
		
        origin.x = Mathf.Sin(DEG_TO_RAD * (width/2.0f + pos.y * width)) * (pos.x*height + height + centerHeight);
        origin.y = Mathf.Cos(DEG_TO_RAD * (width/2.0f + pos.y * width)) * (pos.x*height + height + centerHeight);
		for( int i=0 ; i<segments; i++)
		{			
            float curAngle = DEG_TO_RAD * (i* angleStep + pos.y * width);
            float outerX = Mathf.Sin(curAngle) * (pos.x*height + height + centerHeight);
            float outerY = Mathf.Cos(curAngle) * (pos.x*height + height + centerHeight);
			newVertices[i*2+0] = new Vector3(outerX,outerY,0);
			
            float innerX = Mathf.Sin(curAngle) * (pos.x*height + centerHeight);
            float innerY = Mathf.Cos(curAngle) * (pos.x*height + centerHeight);
            newVertices[i*2+1] = new Vector3(innerX,innerY,0);
			
			newUV[i*2+0]= new Vector2((float)i/(segments-1),1);
			newUV[i*2+1]= new Vector2((float)i/(segments-1),0);
			
            // Outer
			newColors[i*2+0] = new Color(255 * (float)i/segments,255,255,255);

            // Inner
            newColors[i*2+1] = new Color(255 * (float)i/segments,255,255,255);
		}
		
		// Generate triangles indices
		int triangleLength = (segments - 1) * 2 * 3;
		newTriangles = new int[triangleLength];
		for (int i=0;i<triangleLength / 6;i++)
		{
			newTriangles[i * 6 + 0] = i * 2;
			newTriangles[i * 6 + 1] = i * 2 + 1;
			newTriangles[i * 6 + 2] = i * 2 + 2;
			
			newTriangles[i * 6 + 3] = i * 2 + 2;
			newTriangles[i * 6 + 4] = i * 2 + 1;
			newTriangles[i * 6 + 5] = i * 2 + 3;
		}
		
		mesh.vertices = newVertices;
		mesh.uv = newUV;
		mesh.colors = newColors;
		mesh.triangles = newTriangles;

		MeshRenderer mr = GetComponent<MeshRenderer>();
		mr.material = newMaterial;
	}
}