using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class lightcaster : MonoBehaviour
{
    [SerializeField] LayerMask ignoreMe;
    [SerializeField] float getRadius;
    [SerializeField] LayerMask wallMask;

    public Collider[] sceneObjects;

    private Mesh mesh;

    public GameObject lightRays;

    public float offset = 0.0001f;

    public bool showRed;
    public bool showGreen;

    public struct angledVerts{
        public Vector3 vert;
        public float angle;
        public Vector2 uv;
    }

    void Start () {
        mesh = lightRays.GetComponent<MeshFilter>().mesh;
    }

    public static int[] AddItemsToArray (int[] original, int itemToAdd1, int itemToAdd2, int itemToAdd3) {
        int[] finalArray = new int[ original.Length + 3 ];
        for(int i = 0; i < original.Length; i ++ ) {
            finalArray[i] = original[i];
        }
        finalArray[original.Length] = itemToAdd1;
        finalArray[original.Length + 1] = itemToAdd2;
        finalArray[original.Length + 2] = itemToAdd3;
        return finalArray;
    }

    public static Vector3[] ConcatArrays(Vector3[] first, Vector3[] second){
        Vector3[] concatted = new Vector3[first.Length + second.Length];
        Array.Copy(first, concatted, first.Length);
        Array.Copy(second, 0, concatted, first.Length, second.Length);
        return concatted;
    }

    void Update()
    {
        GetWalls();
        mesh.Clear();

        Vector3[] objverts = sceneObjects[0].GetComponent<MeshFilter>().mesh.vertices;
        for (int i = 1; i < sceneObjects.Length; i++)
        {
            objverts = ConcatArrays(objverts, sceneObjects[i].GetComponent<MeshFilter>().mesh.vertices);
        }

        angledVerts[] angleds = new angledVerts[(objverts.Length*2)];
        Vector3[] verts = new Vector3[(objverts.Length*2)+1];
        Vector2[] uvs = new Vector2[(objverts.Length*2)+1];

        verts[0] = lightRays.transform.worldToLocalMatrix.MultiplyPoint3x4(this.transform.position);
        uvs[0] = new Vector2(lightRays.transform.worldToLocalMatrix.MultiplyPoint3x4(this.transform.position).x, lightRays.transform.worldToLocalMatrix.MultiplyPoint3x4(this.transform.position).y);

        int h = 0;

        for (int j = 0; j < sceneObjects.Length; j++)
        {
            for (int i = 0; i < sceneObjects[j].GetComponent<MeshFilter>().mesh.vertices.Length; i++)
            {
                Vector3 me = this.transform.position;
                Vector3 other = sceneObjects[j].transform.localToWorldMatrix.MultiplyPoint3x4(objverts[h]);

                float angle1 = Mathf.Atan2(((other.y-me.y)-offset),((other.x-me.x)-offset));
                float angle3 = Mathf.Atan2(((other.y-me.y)+offset),((other.x-me.x)+offset));

                RaycastHit hit;
                Physics.Raycast(transform.position, new Vector2( (other.x-me.x)-offset , (other.y-me.y)-offset ) , out hit, 100, ~ignoreMe);
                RaycastHit hit2;
                Physics.Raycast(transform.position, new Vector2( (other.x-me.x)+offset , (other.y-me.y)+offset ), out hit2, 100, ~ignoreMe);

                angleds[(h*2)].vert = lightRays.transform.worldToLocalMatrix.MultiplyPoint3x4(hit.point);
                angleds[(h*2)].angle = angle1;
                angleds[(h*2)].uv = new Vector2(angleds[(h*2)].vert.x, angleds[(h*2)].vert.y);

                angleds[(h*2)+1].vert = lightRays.transform.worldToLocalMatrix.MultiplyPoint3x4(hit2.point);
                angleds[(h*2)+1].angle = angle3;
                angleds[(h*2)+1].uv = new Vector2(angleds[(h*2)+1].vert.x, angleds[(h*2)+1].vert.y);

                h++;

                if(showRed && hit.collider != null)
                {
                    Debug.DrawLine(transform.position, hit.point, Color.red);      
                }
                if(showGreen)
                {
                    Debug.DrawLine(transform.position, hit2.point, Color.green);      
                }   

            }
        }
        
        Array.Sort(angleds, delegate(angledVerts one, angledVerts two) {
            return one.angle.CompareTo(two.angle);
        });

        for (int i = 0; i < angleds.Length; i++)
        {
            verts[i+1] = angleds[i].vert;
            uvs[i+1] = angleds[i].uv;
        }

        mesh.vertices = verts;

        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2 (uvs[i].x + .5f, uvs[i].y + .5f);
        }

        mesh.uv = uvs;
        
        int[] triangles = {0,1,verts.Length-1};

        for (int i = verts.Length-1; i > 0; i--)
        {
            triangles = AddItemsToArray(triangles, 0, i, i-1);
        }

        mesh.triangles = triangles;
    }

    void GetWalls()
    {
        sceneObjects = Physics.OverlapSphere(transform.position, getRadius, wallMask);
    }
}
