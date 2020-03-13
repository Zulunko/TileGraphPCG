using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Average distance as a positive attribute
public class FitAway : AFitnessVariable {
    public FitAway(VarDescriber vd) : base(vd) { }

    public override float Value(IMemberContainer member) {
        List<RenderedTileInfo> renderedTiles = new List<RenderedTileInfo>(((MemberTilePCG)member).renderedTiles.Values);
        float distance = 0;
        foreach (RenderedTileInfo rti in renderedTiles) {
            distance += rti.location.magnitude;
        }
        return distance / renderedTiles.Count;
    }
}
