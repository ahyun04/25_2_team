// 나중에 많은 맵이 생길경우를 대비해서 구현해놓은거 안쓰일경우는 지울예정
[System.Serializable]
public class MapState
{
    public int id;
    public string mapName;
    public Map lakeMap;
    public Map riverMap;
    public Map oceanMap;

    public MapState(int id, string mapName, Map lakeMap, Map riverMap, Map oceanMap)
    {
        this.id = id;
        this.mapName = mapName;
        this.lakeMap = lakeMap;
        this.riverMap = riverMap;
        this.oceanMap = oceanMap;     
    }
}
