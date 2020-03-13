using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphRenderer {
    private static GameObject renderedGraph;

    // finalLoc teleports this render somewhere once it's complete.
    // Note that it will still render around the origin before teleporting away, so DON'T SET FINALLOC TO ZERO
    //   UNLESS YOU'RE DELETING IMMEDIATELY / ONLY RENDERING ONE.

    public static Dictionary<TileNode, RenderedTileInfo> FinalRender(TileGraph g, Vector3 loc, List<float> fitVals, GameObject parent) {
        Dictionary<TileNode, RenderedTileInfo> ret = RenderNodes(g, parent);
        renderedGraph.transform.position = loc;
        DisplayFitness df = renderedGraph.AddComponent<DisplayFitness>();
        df.fitnessValues = fitVals;
        return ret;
    }

    public static Dictionary<TileNode, RenderedTileInfo> RenderNodes(TileGraph g, GameObject parent) {
        renderedGraph = new GameObject();
        renderedGraph.transform.position = Vector3.zero;
        if (parent != null) {
            renderedGraph.transform.parent = parent.transform;
        }
        GameObject origin = new GameObject();
        origin.name = "Origin";
        origin.transform.parent = renderedGraph.transform;
        Dictionary<TileNode, RenderedTileInfo> renderedTiles = new Dictionary<TileNode, RenderedTileInfo>();
        //List<TileNode> nodesToRemove = new List<TileNode>();
        renderedTiles.Add(g.nodes[0], new RenderedTileInfo(g.nodes[0].type, Vector3.zero, 0, g.nodes[0], renderedGraph));

        // Unexplored nodes HAVE BEEN RENDERED but have not been explored for connections.
        Queue<TileNode> unexploredNodes = new Queue<TileNode>();
        // Explored nodes HAVE BEEN RENDERED and have been explored.
        List<TileNode> exploredNodes = new List<TileNode>();
        unexploredNodes.Enqueue(g.nodes[0]);
        while (unexploredNodes.Count > 0) {
            TileNode curr = unexploredNodes.Dequeue();
            exploredNodes.Add(curr);
            // For each node connected to this one...
            foreach (TileNode newTile in curr.connectedNodes) {
                if (renderedTiles.ContainsKey(newTile)) continue;
                Vector3 doorLoc = renderedTiles[curr].GetDoorLoc();
                Vector3 firstDoor = doorLoc;
                Vector3 tileLoc = Vector3.zero;
                float rotation = 0;
                bool placeable = true;
                for (int d = 0; d < newTile.type.doorLocs.Length; d++) {
                    int rotationCount = 0;
                    for (rotation = (newTile.seed%4)*90; rotationCount < 4; rotation += 90, rotationCount++) {
                        do {
                            tileLoc = doorLoc - Quaternion.Euler(0, rotation, 0) * newTile.type.doorLocs[d];
                            placeable = true;
                            for (int k = 0; k < newTile.type.boundingBoxes.Length; k++) {
                                if (!newTile.type.boundingBoxes[k].IsPlaceable(tileLoc, rotation)) {
                                    placeable = false;
                                    break;
                                }
                            }
                            if (placeable) {
                                break;
                            }
                            doorLoc = renderedTiles[curr].GetDoorLoc();
                        } while (doorLoc != firstDoor);
                        if (placeable)
                            break;
                    }
                    if (placeable)
                        break;
                }
                if (!placeable) {
                    continue;
                }
                RenderedTileInfo newRenderedTile = new RenderedTileInfo(newTile.type, tileLoc, rotation, newTile, renderedGraph);
                // door detection should do this.
                //renderedTiles[connections[j]].OccupyLastDoor(newRenderedTile);
                renderedTiles.Add(newTile, newRenderedTile);
                unexploredNodes.Enqueue(newTile);
            }
        }

        List<TileNode> nodesToRemove = new List<TileNode>();
        foreach (TileNode t in g.nodes) {
            if (!renderedTiles.ContainsKey(t)) {
                nodesToRemove.Add(t);
            }
        }
        // Backpropogate removed nodes.
        foreach (TileNode t in nodesToRemove) {
            g.nodes.Remove(t);
        }
        // Backpropogate removed node connections.
        foreach (TileNode t in g.nodes) {
            foreach (TileNode r in nodesToRemove) {
                t.connectedNodes.Remove(r);
            }
        }
        // Check for other connections to remove.
        foreach (TileNode t in g.nodes) {
            List<TileNode> connsToRemove = new List<TileNode>();
            foreach (TileNode c in t.connectedNodes) {
                bool found = false;
                foreach (RenderedTileInfo r in renderedTiles[t].connectedTiles) {
                    if (c == r.sourceNode)
                        found = true;
                }
                if (!found)
                    connsToRemove.Add(c);
            }
            foreach (TileNode c in connsToRemove) {
                t.RemoveConnectedNode(c);
            }
        }
        return renderedTiles;
    }

    public static void DeleteRender() {
        GameObject.DestroyImmediate(renderedGraph);
    }
}
