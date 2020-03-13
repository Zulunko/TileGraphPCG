using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileLookup : MonoBehaviour {

    private RenderedTileInfo t;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void AddRenderedTileInfo(RenderedTileInfo t) {
        this.t = t;
    }

    public RenderedTileInfo GetRenderedTileInfo() {
        return t;
    }
}
