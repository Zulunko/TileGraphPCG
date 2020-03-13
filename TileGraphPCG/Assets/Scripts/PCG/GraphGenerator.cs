using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphGenerator {
    private static int numNodes = 10000;

    public static string PickRandomTile() {
        List<string> keys = new List<string>(TileGraphPCG.tileMap.Keys);
        return keys[Random.Range(0, TileGraphPCG.tileMap.Count)];
    }

	// This generates a graph from scratch. Used to start the GA.
    public static TileGraph GenerateGraph() {
        List<TileNode> graph = new List<TileNode>();
        TileData newTileType = TileGraphPCG.tileMap[PickRandomTile()];
        graph.Add(new TileNode(newTileType));
        for (int i = 1; i < numNodes; i++) {
            TryAddNodeToGraph(graph);
        }
        return new TileGraph(graph);
    }

	// This attempts to add a new node to an existing graph. Both used by the initial generation and also used for mutation.
    public static void TryAddNodeToGraph(List<TileNode> graph) {
        TileData newTileType = TileGraphPCG.tileMap[PickRandomTile()];
        TileNode connectionCandidate = graph[Random.Range(0, graph.Count)];
        int b = 0;
        while (connectionCandidate.connectedNodes.Count >= connectionCandidate.type.doorLocs.Length && b <= graph.Count) {
            connectionCandidate = graph[Random.Range(0, graph.Count)];
            b++;
        }
        // Failed to add.
        if (b == graph.Count) { Debug.LogWarning("Add node to graph failed"); return; }
        TileNode curr = new TileNode(newTileType);
        connectionCandidate.AddConnectedNode(curr);
        curr.AddConnectedNode(connectionCandidate);
        graph.Add(curr);
    }
}
