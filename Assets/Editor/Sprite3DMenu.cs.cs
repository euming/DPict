using UnityEngine;
using UnityEditor;
using CustomEditorExtensions;

public class Sprite3DMenu : ScriptableObject
{
	public static Object CreateSpriteFromObject(Object obj)	
	{
		GameObject	go = obj as GameObject;
		Sprite3D	asSprite = null;
//		ScaleableSpritePrefab scDialog = null; // removed due to warning (slc)
		Object		newSprite = null;
		
		//TextureAtlas texAtlas = null;
		if (go) {
			//texAtlas = go.GetComponent<TextureAtlas>();
			asSprite = go.GetComponent<Sprite3D>();
//			scDialog = go.GetComponent<ScaleableSpritePrefab>(); // removed due to warning (slc)
		}
		Texture tex2D = obj as Texture;
		/*
		if (texAtlas) {
			GameObject[] listGO = Sprite3DMenu_CreateSprite3D(texAtlas);
			if (listGO != null) {
				newSprite = listGO[0];	//	hack: Just pick the first one only.
			}
		}
		else 
		*/
		if (tex2D) {
			newSprite = Sprite3DMenu_CreateSprite3D(tex2D);
		}
		else if (asSprite) {
			newSprite = Sprite3DMenu_CreateSprite3D(asSprite);
		}
		return newSprite;
    }
	
	[MenuItem ("GameObject/Create Sprite3D &S")]
    static void Sprite3DMenu_CreateSprite3Ds()
    {
		ScriptableObjectExtension.UnityObjectFcn createObjectFcn = CreateSpriteFromObject;//new ScriptableObjectExtension.UnityObjectFcn(CreateAnimFromObject);
		ScriptableObjectExtension.DoCreateObjectLoop(createObjectFcn);
		
		/*
		Object[]		objects = Selection.objects;

        foreach(Object obj in objects)
        {
			GameObject	go = obj as GameObject;
			TextureAtlas texAtlas = null;
			Sprite3D	asSprite = null;
			ScaleableSpritePrefab scDialog = null;
			
			if (go) {
				texAtlas = go.GetComponent<TextureAtlas>();
				asSprite = go.GetComponent<Sprite3D>();
				scDialog = go.GetComponent<ScaleableSpritePrefab>();
			}
			Texture tex2D = obj as Texture;
			
			if (texAtlas) {
				Sprite3DMenu_CreateSprite3D(texAtlas);
			}
			else if (tex2D) {
				Sprite3DMenu_CreateSprite3D(tex2D);
			}
			else if (asSprite) {
				Sprite3DMenu_CreateSprite3D(asSprite);
			}
        }
        */
	}
	
	/*
	[System.Obsolete("Use gameObject.SetDefaultComponent(null, typeof(Sprite3D)); method instead of this one")]
	static private void SetDefaultSprite(GameObject newSprite3D)
	{
		Sprite3D newSprite3D_component = newSprite3D.GetComponent<Sprite3D>();
		MonoBehaviour mb = newSprite3D_component as MonoBehaviour;
		if (mb) {
			GameObject defaultSprite = mb.LoadDefault() as GameObject;
			if (defaultSprite) {
				newSprite3D.SetDefaultLike(defaultSprite);					//	set the gameObject defaults
				newSprite3D_component.SetDefaultLike(defaultSprite);
			}
		}
	}
	*/
	
	static public GameObject Sprite3DMenu_CreateSprite3D(Texture tex2D)
	{
		
		GameObject newSprite3D = Sprite3D.CreateSprite3D(tex2D);
		//	this stuff is now placed in Sprite3D::Reset()
		/*
		newSprite3D.SetDefaultLike(typeof(Sprite3D));	//	GameObject defaults
		newSprite3D.SetDefaultLikeComponent(null, typeof(Sprite3D));	//	Sprite3D defaults
		newSprite3D.SetDefaultLikeComponent(null, typeof(Frame));		//	Frame defaults
		*/
		return newSprite3D;
	}

	static public GameObject Sprite3DMenu_CreateSprite3D(Sprite3D sprite3Dgo)
	{
		GameObject newSprite3D = null;
		Texture			tex2D = sprite3Dgo.GetTexture();
		if (tex2D) {
			newSprite3D = Sprite3DMenu_CreateSprite3D(tex2D);
		}
		return newSprite3D;
	}
	
	/*
	static public GameObject[] Sprite3DMenu_CreateSprite3D(TextureAtlas texAtlas)
	{
		Texture			tex2D = texAtlas.GetTexture();
		int				nSprites = texAtlas.GetNumRects();
		GameObject[]	newSprites = new GameObject[nSprites];
		
		//	first create all the sprites as copies of the original texture
		for(int ii=0; ii<nSprites; ii++) {
			newSprites[ii] = Sprite3DMenu_CreateSprite3D(tex2D);
			newSprites[ii].name = texAtlas.name + ii.ToString();
		}
		
		//	then make the texture atlas apply the differences to each sprite
		texAtlas.ApplyToGameObjects(newSprites);
		
		return newSprites;
	}
	*/
	
	/*
	static public GameObject[] Sprite3DMenu_CreateSprite3D(ScaleableSpritePrefab scDialog)
	{
		Texture			tex2D = scDialog.GetTexture();
		int				nSprites = scDialog.GetNumRects();
		GameObject[]	newSprites = new GameObject[nSprites];
		
		//	first create all the sprites as copies of the original texture
		for(int ii=0; ii<nSprites; ii++) {
			newSprites[ii] = Sprite3DMenu_CreateSprite3D(tex2D);
			newSprites[ii].name = scDialog.name + ii.ToString();
		}
		
		//	then make the texture atlas apply the differences to each sprite
		scDialog.ApplyToGameObjects(newSprites);
		
		return newSprites;
	}
	*/
}
