using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class TileData {
    // This says what the tile is, for easy debugging/display. This should also be the
    //  key for this tile into the TileTypeMap.
    public string type;
    // The actual tile.
    public GameObject prefab;
    // The locations of the doors. In 2d, the Z location should always be zero.
    //  Note: These tiles will be rotated. The contents of doorLocs must be transformed
    //   to the tile rotation prior to use.
    public Vector3[] doorLocs;
    // The bounding boxes for this tile.
    //  Note: These bounding boxes will be rotated based on the rotation of the placed tile.
    public TileBoundingBox[] boundingBoxes;
}
