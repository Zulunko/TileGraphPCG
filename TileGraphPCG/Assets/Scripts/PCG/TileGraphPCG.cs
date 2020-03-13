using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VAR {
    DistanceToBoss,
    Nonlinearity,
    MaxTileDistance,
    AvgTileDistance,
    LocalVariety,
    TileCount,
}

public class TileGraphPCG : MonoBehaviour {

    public int popSize;
    public int numGens;

    public VarDescriber[] vars;

    public TileData[] tiles;

    public static Dictionary<string, TileData> tileMap;

    Dictionary<IMemberContainer, List<float>> finalTiles;

    GameObject container;

    // Use this for initialization
    void Start () {
        tileMap = new Dictionary<string, TileData>();
        foreach (TileData t in tiles) {
            tileMap.Add(t.type, t);
        }
        AFitnessVariable[] fitVars = new AFitnessVariable[vars.Length];
        for (int i = 0; i < fitVars.Length; i++) {
            switch (vars[i].var) {
                case VAR.DistanceToBoss:
                    fitVars[i] = new FitLinear(vars[i]);
                    break;
                case VAR.Nonlinearity:
                    fitVars[i] = new FitLinear2(vars[i]);
                    break;
                case VAR.MaxTileDistance:
                    fitVars[i] = new FitDistance(vars[i]);
                    break;
                case VAR.AvgTileDistance:
                    fitVars[i] = new FitAway(vars[i]);
                    break;
                case VAR.LocalVariety:
                    fitVars[i] = new FitLocalVariety(vars[i]);
                    break;
                case VAR.TileCount:
                    fitVars[i] = new FitCount(vars[i]);
                    break;
            }
        }
        GA.Initialize<MemberTilePCG>(popSize, fitVars);
        /*GA.Initialize<MemberTilePCG>(16, new IFitnessVariable[]{
            new FitCount(50, -1),
            new FitDistance(50, -1),
            new FitAway(60, 150),
            //new FitLocalVariety(0, -1),
        });*/
        /*GA.Initialize<MemberTilePCG>(16, new IFitnessVariable[] {
            new FitCount(10, -1),
            new FitLinear(-1, -1),
            new FitLinear2(-1, -1),
        });*/
		// Run the GA to gather results.
        finalTiles = GA.Run<MemberTilePCG>(numGens);
        container = new GameObject();
	}
	
	// Update is called once per frame
	void Update () {
		// Display only the minimum and maximum results for each variable. This allows inspection of the constraint edges.
		if (Input.GetKeyDown(KeyCode.A)) {
            GameObject.DestroyImmediate(container);
            container = new GameObject();
            bool init = false;
            List<float> mins = new List<float>();
            List<IMemberContainer> minMbrs = new List<IMemberContainer>();
            List<List<float>> minMbrsVals = new List<List<float>>();
            List<float> maxes = new List<float>();
            List<IMemberContainer> maxMbrs = new List<IMemberContainer>();
            List<List<float>> maxMbrsVals = new List<List<float>>();
            foreach (KeyValuePair<IMemberContainer, List<float>> kvp in finalTiles) {
                if (!init) {
                    for (int i = 0; i < kvp.Value.Count; i++) {
                        mins.Add(-1);
                        minMbrs.Add(null);
                        minMbrsVals.Add(null);
                        maxes.Add(-1);
                        maxMbrs.Add(null);
                        maxMbrsVals.Add(null);
                    }
                    init = true;
                }
                for (int i = 0; i < kvp.Value.Count; i++) {
                    if (kvp.Value[i] < mins[i] || mins[i] == -1) {
                        mins[i] = kvp.Value[i];
                        minMbrs[i] = kvp.Key;
                        minMbrsVals[i] = kvp.Value;
                    }
                    if (kvp.Value[i] > maxes[i] || maxes[i] == -1) {
                        maxes[i] = kvp.Value[i];
                        maxMbrs[i] = kvp.Key;
                        maxMbrsVals[i] = kvp.Value;
                    }
                }
            }
            Vector3 start = new Vector3(0, 10, 0);
            for (int i = 0; i < mins.Count; i++) {
                Vector3 currLoc = start + new Vector3(minMbrsVals[i][0] * 10, (200 - minMbrsVals[i][1]) * 10, minMbrsVals[i][2] * 10);
                minMbrs[i].FinalRender(currLoc, minMbrsVals[i], container);
                currLoc = start + new Vector3(maxMbrsVals[i][0] * 10, (200 - maxMbrsVals[i][1]) * 10, maxMbrsVals[i][2] * 10);
                maxMbrs[i].FinalRender(currLoc, maxMbrsVals[i], container);
            }
        }
		// Display all results. This can get a little messy, as the results are displayed as if in a massive scatterplot but they're actually game levels.
        if (Input.GetKeyDown(KeyCode.S)) {
            GameObject.DestroyImmediate(container);
            container = new GameObject();
            Vector3 start = new Vector3(0, 10, 0);
            foreach (KeyValuePair<IMemberContainer, List<float>> kvp in finalTiles) {
                Vector3 currLoc = start + new Vector3(kvp.Value[0] * 10, (200 - kvp.Value[1]) * 10, kvp.Value[2] * 10);
                kvp.Key.FinalRender(currLoc, kvp.Value, container);
            }
        }
	}
}
