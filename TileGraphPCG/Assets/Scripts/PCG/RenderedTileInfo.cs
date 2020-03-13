using UnityEngine;
using System.Collections.Generic;

public class RenderedTileInfo {
    public TileData tileType;
    public GameObject placedTile;
    public Vector3 location;
    public float rotation;
    private bool[] doors;
    public List<RenderedTileInfo> connectedTiles;
    int currDoor;
    public TileNode sourceNode;
    private GameObject graphHolder;

    public RenderedTileInfo(TileData tileType, Vector3 location, float rotation, TileNode sourceNode, GameObject graphHolder) {
        this.tileType = tileType;
        placedTile = Object.Instantiate(tileType.prefab, location, Quaternion.Euler(0, rotation, 0), graphHolder.transform);
        TileLookup curr = placedTile.AddComponent<TileLookup>();
        curr.AddRenderedTileInfo(this);
        this.location = location;
        this.rotation = rotation;
        this.sourceNode = sourceNode;
        this.graphHolder = graphHolder;
        doors = new bool[tileType.doorLocs.Length];
        currDoor = sourceNode.seed % tileType.doorLocs.Length;
        connectedTiles = new List<RenderedTileInfo>();
        CheckForDoors();
    }

    // Sees if there are already-rendered tiles nearby that can be connected.
    // If so, connect the rendered tiles and backpropogate the change to the TileNode.
    private void CheckForDoors() {
        for (int i = 0; i < tileType.doorLocs.Length; i++) {
            Vector3 doorLoc = placedTile.transform.TransformPoint(tileType.doorLocs[i]);
            // project bounding box at door loc
            Collider[] overlaps = Physics.OverlapBox(doorLoc, new Vector3(1.5f, 1.5f, 1.5f));
            // check the door locs of colliding tiles for a match, excluding this tile
            foreach (Collider c in overlaps) {
                RenderedTileInfo other = c.transform.parent.GetComponent<TileLookup>().GetRenderedTileInfo();
                if (other != this) {
                    if (other.OccupyNearbyDoor(doorLoc, this)) {
                        ConnectTo(other);
                        doors[i] = true;
                    }
                }
            }
            // if there is a match, occupy the door on this tile, occupy the door on the other tile, add connected rendered tiles, and backpropogate connections to tilenodes
        }
    }

    // Returns true if there was a door to occupy (and occupies that door), else returns false.
    public bool OccupyNearbyDoor(Vector3 incDoorLoc, RenderedTileInfo incoming) {
        // Check if there is a door to occupy
        for (int i = 0; i < tileType.doorLocs.Length; i++) {
            Vector3 doorLoc = placedTile.transform.TransformPoint(tileType.doorLocs[i]);
            if ((incDoorLoc - doorLoc).magnitude < 0.5f) {
                ConnectTo(incoming);
                GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
                door.GetComponent<Collider>().enabled = false;
                door.transform.SetParent(graphHolder.transform);
                door.transform.position = doorLoc;
                doors[i] = true;
                return true;
            }
        }
        return false;
    }
    
    public void ConnectTo(RenderedTileInfo incoming) {
        if (!connectedTiles.Contains(incoming))
            connectedTiles.Add(incoming);
        sourceNode.AddConnectedNode(incoming.sourceNode);
    }
    
    public Vector3 GetDoorLoc() {
        Vector3 r = placedTile.transform.TransformPoint(tileType.doorLocs[currDoor]);
        currDoor = (currDoor + 1) % tileType.doorLocs.Length;
        return r;
    }
}
