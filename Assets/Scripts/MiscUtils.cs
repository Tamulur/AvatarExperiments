using UnityEngine;
using System.Collections.Generic;


public class MiscUtils : MonoBehaviour
{
	#region fields
		static Dictionary<string, GameObject> dummyObjects = new Dictionary<string, GameObject>();
	#endregion
	
	
    public static GameObject FindInChildren(GameObject go, string name)
    {
        if (go.name == name)
            return go;

        foreach (Transform t in go.transform)
        {
            GameObject foundGO = FindInChildren(t.gameObject, name);
            if (foundGO != null)
                return foundGO;
        }

        return null;
    }

	
	
	public static T GetComponentSafely<T> ( string objectName ) where T: Component
	{
		GameObject go = GameObject.Find( objectName );
		return ( go != null )	?	go.GetComponent<T>()
								:	null;
	}
	
	
	
	public static float NormalizedDegAngle ( float degrees )
	{
		int factor = (int) (degrees/360);
		degrees -= factor * 360;
		if ( degrees > 180 )
			return degrees - 360;
		
		if ( degrees < -180 )
			return degrees + 360;
		
		return degrees;
	}
	
	
	
	public static void PlaceDummyObject( string name, Vector3 pos )
	{
		GameObject dummyObject = null;
		
		if ( dummyObjects.ContainsKey( name ) )
			dummyObject = dummyObjects[ name ];
		else
		{
			dummyObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
			dummyObject.name = name;
			dummyObject.transform.localScale = 0.1f * Vector3.one;
			dummyObjects[ name ] = dummyObject;
		}
		
		dummyObject.transform.position = pos;
	}

}
