#if !UNITY_ANDROID

//var meshFilters : MeshFilter[];
//var material : Material;

function MergePathMeshes( go : GameObject[] ) : GameObject {
	var ngo : GameObject = new GameObject("MergedPath");
    // if not specified, go find meshes
	//***
	//var meshFilters : MeshFilter[];
	//var material : Material;
	
	var meshFilters = new Array(MeshFilter);
	var material = new Array(Material);
	
	//Make a list of all the meshes and materials in the game objects
	if(go.length > 1){
		var mfis = new Array();
		var mats = new Array();
		//Get all the meshes and materials that we've generated
		var i : int = 0;
		for(var gos : GameObject in go){
			var mf : MeshFilter = gos.GetComponent(MeshFilter);
			mfis.Add(mf);
			var mat : Material = gos.GetComponent(Material);
			mats.Add(mat);
		}
		meshFilters = mfis;
		material = mats;
	}	
	//meshFilters = mfis.ToBuiltin(MeshFilter);
	
	/*
    if(meshFilters.Length == 0){
		// find all the mesh filters
		var comps : Component[];
		comps = GetComponentsInChildren(typeof(MeshFilter));
		meshFilters = new MeshFilter[comps.Length];
	
		var mfi : int = 0;
		for(var comp : Component in comps){
			//***This is weird syntax
			meshFilters[mfi++] = comp;
		}
	}
	*/
	
	var vertCount : int = 0;
	var normCount : int = 0;
	var triCount : int = 0;
	var uvCount : int = 0;

	for(var mfff : MeshFilter in meshFilters){    
		vertCount += mfff.mesh.vertices.Length;
		normCount += mfff.mesh.normals.Length;
		triCount += mfff.mesh.triangles.Length;
		uvCount += mfff.mesh.uv.Length;
		//material = mf.gameObject.renderer.material;       
    }
   
	var verts : Vector3[] = new Vector3[vertCount];
	var norms : Vector3[] = new Vector3[normCount];
	//var aBones : Transform[] = new Transform[meshFilters.Length];
	//var bindPoses : Matrix4x4[] = new Matrix4x4[meshFilters.Length];
	//var weights : BoneWeight[] = new BoneWeight[vertCount];
	var tris : int[] = new int[triCount];
	var uvs : Vector2[] = new Vector2[uvCount];
   
	var vertOffset : int = 0;
	var normOffset : int = 0;
	var triOffset : int = 0;
	var uvOffset : int = 0;
	var meshOffset : int = 0;
   
	for(var mffff : MeshFilter in meshFilters){
	
		for(var g : int in mffff.mesh.triangles){
			tris[triOffset++] = g + vertOffset;
		}
		
		//aBones[meshOffset] = mf.transform;
		//bindPoses[meshOffset] = Matrix4x4.identity;
		 
		for(var v : Vector3 in mffff.mesh.vertices){  
			//weights[vertOffset].weight0 = 1.0;
			//weights[vertOffset].boneIndex0 = meshOffset;
			verts[vertOffset++] = v;
		}
		
		for(var n : Vector3  in mffff.mesh.normals){
			norms[normOffset++] = n;
		}
		
		for(var uv : Vector2 in mffff.mesh.uv){
			uvs[uvOffset++] = uv;
		}
	 
		meshOffset++;
		 
		var mr : MeshRenderer;
		mr = mffff.gameObject.GetComponent(typeof(MeshRenderer))as MeshRenderer;
		
		if(mr){
			mr.enabled = false;
		}
		
    }

	var me : Mesh = new Mesh();    
    //me.name = gameObject.name;
	me.name = "MergedPath";
    me.vertices = verts;
    me.normals = norms;
    //me.boneWeights = weights;
    me.uv = uvs;
    me.triangles = tris;
    //me.bindposes = bindPoses;

    // hook up the mesh renderer    
	//var smr : SkinnedMeshRenderer;
    //smr = gameObject.AddComponent(typeof(SkinnedMeshRenderer))as SkinnedMeshRenderer;
 
    //smr.sharedMesh = me;
    //smr.bones = aBones;
    //***me.renderer.material = material;
	//ngo = me;
	//ngo.Mesh = me;
	var nmf = ngo.AddComponent(MeshFilter);
	nmf.mesh = me;
	return ngo;
  }

#endif