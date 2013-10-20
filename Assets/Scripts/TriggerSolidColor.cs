using UnityEngine;
using System.Collections.Generic;

public class TriggerSolidColor : MonoBehaviour
{
	#region fields
		List<Material> materials = new List<Material>();
		Dictionary<Material, Texture2D> mat2tex = new Dictionary<Material, Texture2D>();
		Dictionary<Material, Color> mat2col = new Dictionary<Material, Color>();
		Texture2D whiteTexture;
		Color grayColor = new Color( 0.5f, 0.5f, 0.5f, 1.0f );
		Color orangeColor = new Color ( 192.0f/255.0f, 111.0f/255.0f, 21.3f/255.0f, 1.0f );
		Color solidColor;
	#endregion
	
	
	
	void Awake()
	{
		foreach ( Renderer rend in GetComponentsInChildren<Renderer>() )
			foreach ( Material mat in rend.materials )
			{
				materials.Add( mat );
				mat2tex[mat] = mat.mainTexture as Texture2D;
				if ( mat.HasProperty("_Color") )
					mat2col[mat] = mat.color;
			}
		
		whiteTexture = Resources.Load("Textures/White_16x16") as Texture2D;
		
		solidColor = gameObject.layer == 8 ? orangeColor : grayColor;
	}
	
	
	
	public void MakeSolid()
	{
		foreach ( Material mat in materials )
		{
			mat.mainTexture = whiteTexture;
			if ( mat.HasProperty("_Color") )
				mat.color = solidColor;
		}
	}
	
	
	
	public void RestoreOriginalTexture()
	{
		foreach ( Material mat in materials )
		{
			mat.mainTexture = mat2tex[mat];
			if ( mat.HasProperty("_Color") )
				mat.color = mat2col[mat];
		}
	}
	
	
}
