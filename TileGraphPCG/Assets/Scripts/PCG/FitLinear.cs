using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FitLinear : AFitnessVariable {
    public FitLinear(VarDescriber vd) : base(vd) { }

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
        return maxLoc.magnitude;
    }
}
