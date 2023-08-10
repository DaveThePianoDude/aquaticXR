
class TerrainTileWizard extends ScriptableWizard {
	
    var terrainWidth : int;
    var terrainLength : int;
	
	var heightmapResolution : int = 2048;

    
    function OnWizardUpdate() {
        helpString = "Select Source Terrain and Target Terrain to Transfer Attributes To.";
        
    }
    
    
   
    function OnWizardCreate() {
		//var newTerrain : GameObject = new GameObject("terrain");
    	//var tTerrain : Terrain = newTerrain.AddComponent(Terrain);
		//var tCollider : TerrainCollider = newTerrain.AddComponent(TerrainCollider);
		
		var tData : TerrainData = new TerrainData();
		//tData.Init(1024, 1024, 512);
		tData.size = new Vector3(1000, 10, 1000);
		tData.heightmapResolution = 512;
		tData.baseMapResolution = 1024;
		tData.SetDetailResolution(1024,16);
		var height_data : float[,] = new float[513, 513];
		tData.SetHeights(0, 0, height_data);

		var tGo : GameObject = Terrain.CreateTerrainGameObject(tData);
		var tCol : TerrainCollider = tGo.GetComponent(TerrainCollider);
		var tTer : Terrain = tGo.GetComponent(Terrain);
		
    }
    
    
    @MenuItem("Terrain/Terrain Tiles")
    static function TerrainTileWizardDisplay() {
    	    DisplayWizard("Transfer Tiles", TerrainTileWizard, "Create Tiles");
    }
    
    function CreateTerrainTiles(){
    
	
    }
}