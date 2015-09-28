using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Map : ScriptableObject{
	public string title;
	public int width;
	public int height;
	public Tile[,] map;
	public List<Tile> mapList;

	public bool CanMoveInDirection(TileObject to, Vector2 dir){
		int x = to.x + (int)dir.x;
		int y = to.y + (int)dir.y;


		float dumbtimer = 0;
		int moves = 0;
		while (true && dumbtimer < 4){
			dumbtimer += Time.deltaTime;
			//			Debug.Log("dumbtimer: " + dumbtimer + ", x: " + x + " ,y: "+ y);
			if (!IsPositionValid(x, y)) return false;
			
			switch (map[x,y].type) {
			case TileType.BOX:
				x += (int)dir.x;
				y += (int)dir.y;
				break;
			case TileType.WALL:
				return false;
			case TileType.EMPTY:
				return true;
			case TileType.EXIT:
				if (moves == 0 && to.type == TileType.PLAYER) return true;
				return false;
			case TileType.DOOR:
				Debug.Log("map[x,y].keyType: " + map[x,y].keyType + ", to.keyType: " + to.keyType);
				if (moves == 0 && to.type == TileType.KEY && map[x,y].keyType == to.keyType) return true;
				return false; 
			case TileType.KEY:
				x += (int)dir.x;
				y += (int)dir.y;
				break;
			case TileType.LASER:
				x += (int)dir.x;
				y += (int)dir.y;
				break;
			case TileType.WATER:
				if (moves == 0 && to.type == TileType.BOX) return true;  //TODO handle box in water
				return false;
			case TileType.PLAYER:
				x += (int)dir.x;
				y += (int)dir.y;
				break;
			default:
				break;
			}
			moves++;
		}
		return false;
	}
	
	public bool IsPositionValid(int x, int y){
		if (x < 0 || x >= width || y < 0 || y >= height) return false;
		return true;
	}
	
	public bool IsPositionEmpty(int x, int y){
		if (!IsPositionValid(x, y)) return false;
		if (map[x,y].type == TileType.EMPTY) return true;
		return false;
	}

	public void MakeTileList(){
		Debug.Log("MakeTileList - w , h: " + width + ", " + height);
		mapList = new List<Tile>(width * height);
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				Debug.Log("x, y: " + x + ", " + y +", map[x,y]: " + map[x,y]);
				mapList.Add(map[x,y]);
			}
		}
	}

	public void ReadTileList(){
		map = new Tile[width,height];
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				map[x,y] = mapList[y + x * height];
			}
		}
	}
}