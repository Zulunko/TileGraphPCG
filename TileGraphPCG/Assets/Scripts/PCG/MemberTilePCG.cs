using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemberTilePCG : IMemberContainer {
    public TileGraph graph;
    public Dictionary<TileNode, RenderedTileInfo> renderedTiles;
    float fitness;

    public void Generate() {
        graph = GraphGenerator.GenerateGraph();
    }

    public IMemberContainer[] Crossover(IMemberContainer otherParent) {
        // This is difficult.
        // First (and bad) attempt:
        // 1. draw the same line through the origin in both graphs, splitting them.
        // 2. for each child, pick two nodes in the two halves as close to the origin as possible that have doors that used to be connected.
        // 3. connect the nodes on each half via the single door.
        // NOTE: THIS IS PRONE TO FAIL CATASTROPHICALLY. If the one new connection cannot be made, the entire crossover will fail.
        float angle = Random.Range(0, 180);
        // turn angle into vector
        Vector3 lineCheck = Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward;
        List<TileNode>[] mySplit = CreateHalvesAndCandidates(lineCheck, this);
        List<TileNode>[] otherSplit = CreateHalvesAndCandidates(lineCheck, (MemberTilePCG)otherParent);

        // Child 1
        List<TileNode> firstChild = new List<TileNode>();
        Dictionary<TileNode, TileNode> firstOldToNewMapping = new Dictionary<TileNode, TileNode>();
        // First Half Copy
        foreach (TileNode t in mySplit[0]) {
            TileNode newTile = TileNode.Copy(t);
            firstOldToNewMapping[t] = newTile;
            firstChild.Add(newTile);
            foreach (TileNode c in t.connectedNodes) {
                if (firstOldToNewMapping.ContainsKey(c)) {
                    newTile.AddConnectedNode(firstOldToNewMapping[c]);
                    firstOldToNewMapping[c].AddConnectedNode(newTile);
                }
            }
        }
        // Second Half Copy
        foreach (TileNode t in otherSplit[1]) {
            TileNode newTile = TileNode.Copy(t);
            firstOldToNewMapping[t] = newTile;
            firstChild.Add(newTile);
            foreach (TileNode c in t.connectedNodes) {
                if (firstOldToNewMapping.ContainsKey(c)) {
                    newTile.AddConnectedNode(firstOldToNewMapping[c]);
                    firstOldToNewMapping[c].AddConnectedNode(newTile);
                }
            }
        }

        // Child 2
        List<TileNode> secondChild = new List<TileNode>();
        Dictionary<TileNode, TileNode> secondOldToNewMapping = new Dictionary<TileNode, TileNode>();
        // First Half Copy
        foreach (TileNode t in mySplit[1]) {
            TileNode newTile = TileNode.Copy(t);
            secondOldToNewMapping[t] = newTile;
            secondChild.Add(newTile);
            foreach (TileNode c in t.connectedNodes) {
                if (secondOldToNewMapping.ContainsKey(c)) {
                    newTile.AddConnectedNode(secondOldToNewMapping[c]);
                    secondOldToNewMapping[c].AddConnectedNode(newTile);
                }
            }
        }
        // Second Half Copy
        foreach (TileNode t in otherSplit[0]) {
            TileNode newTile = TileNode.Copy(t);
            secondOldToNewMapping[t] = newTile;
            secondChild.Add(newTile);
            foreach (TileNode c in t.connectedNodes) {
                if (secondOldToNewMapping.ContainsKey(c)) {
                    newTile.AddConnectedNode(secondOldToNewMapping[c]);
                    secondOldToNewMapping[c].AddConnectedNode(newTile);
                }
            }
        }
        ConnectCandidates(this, mySplit[2], (MemberTilePCG)otherParent, otherSplit[3], firstOldToNewMapping);
        ConnectCandidates(this, mySplit[3], (MemberTilePCG)otherParent, otherSplit[2], secondOldToNewMapping);
        MemberTilePCG[] containers = new MemberTilePCG[2];
        containers[0] = new MemberTilePCG();
        containers[0].graph = new TileGraph(firstChild);
        containers[1] = new MemberTilePCG();
        containers[1].graph = new TileGraph(secondChild);
        return containers;
    }

    private static List<TileNode>[] CreateHalvesAndCandidates(Vector3 lineCheck, MemberTilePCG m) {
        List<TileNode> firstHalf = new List<TileNode>();
        List<TileNode> secondHalf = new List<TileNode>();
        foreach (KeyValuePair<TileNode, RenderedTileInfo> renderedTile in m.renderedTiles) {
            if (Vector3.Cross(lineCheck, renderedTile.Value.location).y < 0) {
                firstHalf.Add(renderedTile.Key);
            } else {
                secondHalf.Add(renderedTile.Key);
            }
        }
        // find candidate doors
        List<TileNode> firstHalfCandidates = new List<TileNode>();
        List<TileNode> secondHalfCandidates = new List<TileNode>();
        foreach (TileNode t in firstHalf) {
            foreach (TileNode c in t.connectedNodes) {
                if (secondHalf.Contains(c)) {
                    firstHalfCandidates.Add(t);
                    if (!secondHalfCandidates.Contains(c))
                        secondHalfCandidates.Add(c);
                }
            }
        }
        return new List<TileNode>[]{ firstHalf, secondHalf, firstHalfCandidates, secondHalfCandidates };
    }

    private static void ConnectCandidates(MemberTilePCG firstParent, List<TileNode> firstCandidates, MemberTilePCG secondParent, List<TileNode> secondCandidates, Dictionary<TileNode, TileNode> oldToNewMapping) {
        foreach (TileNode fc in firstCandidates) {
            float minDistance = -1;
            TileNode candidate = null;
            foreach (TileNode sc in secondCandidates) {
                if (oldToNewMapping[sc].connectedNodes.Count < oldToNewMapping[sc].type.doorLocs.Length) {
                    float dist = (secondParent.renderedTiles[sc].location - firstParent.renderedTiles[fc].location).magnitude;
                    if (minDistance == -1 || dist < minDistance) {
                        minDistance = dist;
                        candidate = oldToNewMapping[sc];
                    }
                }
            }
            if (candidate != null)
                oldToNewMapping[fc].AddConnectedNode(candidate);
        }
    }
    
    public void Mutate() {
        // Possibilities:
        // Add new node(s)
        // Remove node(s)
        // Change the tile type of a node
        // Add a new connection?
        // Remove an existing connection?
        float randVal = Random.value;
        // 40% chance to do nothing
        if (randVal < 0.4f) return;
        // 15% chance to try adding 1-3 new nodes
        else if (randVal < 0.55f) {
            int numNodes = Random.Range(1, 4);
            for (int i = 0; i < numNodes; i++) {
                GraphGenerator.TryAddNodeToGraph(graph.nodes);
            }
            return;
        }
        // 15% chance to remove 1-3 connections
        else if (randVal < 0.7f) {
            int numNodes = Random.Range(1, 4);
            if (graph.nodes.Count <= numNodes) return;
            for (int i = 0; i < numNodes; i++) {
                TileNode n = graph.nodes[Random.Range(0, graph.nodes.Count)];
                // Removing a random node
                //foreach (TileNode t in graph.nodes) {
                //    t.RemoveConnectedNode(nodeToRemove);
                //}
                //graph.nodes.Remove(nodeToRemove);

                // Removing a random connection from a random node
                if (n.connectedNodes.Count != 0)
                    n.connectedNodes.RemoveAt(Random.Range(0, n.connectedNodes.Count));
            }
            return;
        }
        // 15% chance to change the type of 1-3 nodes without changing connections
        else if (randVal < 0.85f) {
            int numNodes = Random.Range(1, 4);
            for (int i = 0; i < numNodes; i++) {
                TileNode nodeToModify = graph.nodes[Random.Range(0, graph.nodes.Count)];
                // Changing node randomly
                //nodeToModify.type = TileGraphPCG.tileMap[GraphGenerator.PickRandomTile()];
                //while (nodeToModify.type.doorLocs.Length < nodeToModify.connectedNodes.Count) {
                //    nodeToModify.connectedNodes.RemoveAt(Random.Range(0, nodeToModify.connectedNodes.Count));
                //}

                // Changing node while preserving connections.
                // This is very likely to pick the same room type if the room is highly-connected.
                TileData type = TileGraphPCG.tileMap[GraphGenerator.PickRandomTile()];
                while (type.doorLocs.Length < nodeToModify.connectedNodes.Count) {
                    type = TileGraphPCG.tileMap[GraphGenerator.PickRandomTile()];
                }
                nodeToModify.type = type;
            }
            return;
        }
        // 15% chance to change the seed of 1-3 nodes
        else {
            int numNodes = Random.Range(1, 4);
            for (int i = 0; i < numNodes; i++) {
                TileNode nodeToModify = graph.nodes[Random.Range(0, graph.nodes.Count)];
                nodeToModify.seed = Random.Range(0, 9999);
            }
        }
    }

    // Must be called before fitness calculations.
    public void GeneratePhenotype() {
        Render();
        DeleteRender();
    }

    public void Render() {
        renderedTiles = GraphRenderer.RenderNodes(graph, null);
    }

    public void FinalRender(Vector3 loc, List<float> fitVals, GameObject parent) {
        renderedTiles = GraphRenderer.FinalRender(graph, loc, fitVals, parent);
    }

    public void DeleteRender() {
        GraphRenderer.DeleteRender();
    }

    public IMemberContainer CopyGenotype() {
        List<TileNode> newGraph = new List<TileNode>();
        Dictionary<TileNode, TileNode> oldToNewMapping = new Dictionary<TileNode, TileNode>();
        foreach (TileNode o in graph.nodes) {
            TileNode n = TileNode.Copy(o);
            oldToNewMapping.Add(o, n);
            newGraph.Add(n);
        }
        foreach (TileNode o in graph.nodes) {
            foreach (TileNode c in o.connectedNodes) {
                oldToNewMapping[o].AddConnectedNode(oldToNewMapping[c]);
            }
        }
        MemberTilePCG ret = new MemberTilePCG();
        ret.graph = new TileGraph(newGraph);
        return ret;
    }
}
