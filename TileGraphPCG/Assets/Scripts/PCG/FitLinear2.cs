using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FitLinear2 : AFitnessVariable {
    public FitLinear2(VarDescriber vd) : base(vd) { }

    public override float Value(IMemberContainer member) {
        Dictionary<TileNode, RenderedTileInfo> nodes = ((MemberTilePCG)member).renderedTiles;
        Vector3 maxLoc = Vector3.zero;
        foreach (KeyValuePair<TileNode, RenderedTileInfo> node in nodes) {
            if (node.Key.type == TileGraphPCG.tileMap["1Door"]) {
                if (node.Value.location.magnitude > maxLoc.magnitude) {
                    maxLoc = node.Value.location;
                }
            }
        }
        float totalDist = 0;
        foreach (KeyValuePair<TileNode, RenderedTileInfo> node in nodes) {
            float dist = Vector3.Dot(node.Value.location, maxLoc.normalized);
            if (dist < 0) {
                totalDist += node.Value.location.magnitude;
            } else if (dist > maxLoc.magnitude) {
                totalDist += (node.Value.location - maxLoc).magnitude;
            } else {
                totalDist += (node.Value.location - dist * maxLoc.normalized).magnitude;
            }
        }
        return totalDist/10;
    }
}
