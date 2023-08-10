
class TerrainTransferWizard extends ScriptableWizard {
	
    var sourceTerrain : Terrain;
    var targetTerrain : Terrain[];
    var transferTerrainAttributes : boolean = true;
    var transferTextures : boolean = true;
    var transferTreePrototypes : boolean = true;
    var transferDetailPrototypes : boolean = true;
    //var transferHeightmap : boolean = false;
    
    //var sourceTextures: Texture2D[];
    //var targetTextures : Texture2D[];
    
    function OnWizardUpdate() {
        helpString = "Select Source Terrain and Target Terrain to Transfer Attributes To.";
        
    }
    
    
   
    function OnWizardCreate() {
  
    	for(var ter : Terrain in targetTerrain){
		if(transferTextures){
			ter.terrainData.splatPrototypes = sourceTerrain.terrainData.splatPrototypes;
		}
		
		if(transferTreePrototypes){
			ter.terrainData.treePrototypes = sourceTerrain.terrainData.treePrototypes;
		}
		
		if(transferDetailPrototypes){
			ter.terrainData.detailPrototypes = sourceTerrain.terrainData.detailPrototypes;
		}
		
		if(transferTerrainAttributes){
			ter.basemapDistance = sourceTerrain.basemapDistance;
			ter.castShadows = sourceTerrain.castShadows;
			ter.detailObjectDensity = sourceTerrain.detailObjectDensity;
			ter.detailObjectDistance = sourceTerrain.detailObjectDistance;
			//ter.heightmapMaximumLOD = sourceTerrain.heightmapMaximumLOD;
			ter.heightmapPixelError = sourceTerrain.heightmapPixelError;			
			ter.treeBillboardDistance = sourceTerrain.treeBillboardDistance;
			ter.treeCrossFadeLength = sourceTerrain.treeCrossFadeLength;
			ter.treeDistance = sourceTerrain.treeDistance;
			//ter.treeMaximumFullLODCount = sourceTerrain.treeMaximumFullLODCount;
			//ter. = sourceTerrain.;
		}
		
		
		ter.Flush();
		    
		ter.terrainData.RefreshPrototypes();
    	
    	}
    	
    	sourceTerrain.Flush();
    }
    
    
    @MenuItem("Terrain/Terrain Transfer Attributes")
    static function TransferTerrainAttributes() {
    	    DisplayWizard("Transfer Terrain Attributes", TerrainTransferWizard, "Transfer");
    }
    
    function TransferTerrainTexturesToTerrain(){
    
	
    }
}


	/*
        if(sourceTerrain != null){
        	
        	sourceTextures = new Texture2D[sourceTerrain.terrainData.splatPrototypes.Length];
        	
        	for (i = 0; i < sourceTerrain.terrainData.splatPrototypes.Length; i++) {
        		sourceTextures[i] = sourceTerrain.terrainData.splatPrototypes[i].texture;
		}        	
		
	} 
	if(targetTerrain != null){        	
        	targetTextures = new Texture2D[targetTerrain.terrainData.splatPrototypes.Length];
        	
        	for (i = 0; i < targetTerrain.terrainData.splatPrototypes.Length; i++) {
        		targetTextures[i] = targetTerrain.terrainData.splatPrototypes[i].texture;
		}        	
		
	} 
	*/


  	//TransferTerrainTexturesToTerrain();  
    	//var go : GameObject = new GameObject ("TestObject");
    	//print("Ass");
	//var i : int = 0;
	/*
	for (i = 0; i < sourceTerrain.terrainData.splatPrototypes.Length; i++) {
		targetTerrain.terrainData.splatPrototypes[i].texture = sourceTerrain.terrainData.splatPrototypes[i].texture;
		
		
	} 
	
	*/
	/*
	//targetTerrain.terrainData.splatPrototypes.Clear();
	var ssl : int  = sourceTerrain.terrainData.splatPrototypes.Length;
	var tsl : int  = targetTerrain.terrainData.splatPrototypes.Length;
	targetTerrain.terrainData.splatPrototypes = new SplatPrototype[ssl];
	//targetTerrain.terrainData.splatPrototypes[1].texture = sourceTextures[1];
	*/
	/*
	for (i = 0; i < targetTerrain.terrainData.splatPrototypes.Length; i++) {
		targetTerrain.terrainData.splatPrototypes[i].texture = null;
		
		
	} 
	*/
	/*
	for (i = 0; i < sourceTextures.Length; i++) {
		targetTerrain.terrainData.splatPrototypes[i].texture = null;
		targetTerrain.terrainData.splatPrototypes[i].texture = sourceTextures[i];
		targetTerrain.terrainData.RefreshPrototypes();
		
	} 
	*/
	/*
	for (i = 0; i < sourceTerrain.terrainData.detailPrototypes.Length; i++) {
		
		targetTerrain.terrainData.detailPrototypes[i] = sourceTerrain.terrainData.detailPrototypes[i];
		
	}
	
    	*/
    	//targetTerrain.terrainData = sourceTerrain.terrainData;
