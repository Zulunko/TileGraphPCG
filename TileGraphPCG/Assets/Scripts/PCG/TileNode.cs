using UnityEngine;
using System.Collections.Generic;

public class TileNode {
    public TileData type;
    public List<TileNode> connectedNodes;
    public int seed;

    public TileNode(TileData type) {
        this.type = type;
        this.seed = Random.Range(0, 9999);
        connectedNodes = new List<TileNode>();
    }

    public void AddConnectedNode(TileNode t) {
        if (!connectedNodes.Contains(t))
            connectedNodes.Add(t);
    }
    
    public void RemoveConnectedNode(TileNode t) {
        connectedNodes.Remove(t);
    }

    public static TileNode Copy(TileNode t) {
        TileNode newTile = new TileNode(t.type);
        newTile.seed = t.seed;
        return newTile;
    }
}
