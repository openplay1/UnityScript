using UnityEngine;
using System.Collections;

public class FogWar : MonoBehaviour {

	Texture2D tex;
	Texture2D texorigin;
	Vector3 pos1;
	Vector2 pos2;
	float ppu;
	float time;

	void Awake() {
		texorigin=new Texture2D (256, 256);
		texorigin = Resources.Load ("fog5") as Texture2D;
		tex = new Texture2D (256, 256);

		tex.SetPixels (texorigin.GetPixels());
		GetComponent<Renderer> ().material.mainTexture = tex;
		tex.Apply();

		GetComponent<MeshRenderer>().enabled = true;
	}

	void CutoutCircle(int x, int y, int radius,Transform entity) {
		int startX = Mathf.Clamp(x - radius, 0, tex.width);
		int startY = Mathf.Clamp(y - radius, 0, tex.height);
		int endX = Mathf.Clamp(x + radius, 0, tex.width);
		int endY = Mathf.Clamp(y + radius, 0, tex.height);
		var cutout = new Color(0, 0, 0, 0);
		var pos = new Vector2(x, y);

		var center2D = new Vector2(tex.width, tex.height) /2	;


		if(entity.GetComponent<main>() is Play||entity.GetComponent<main>() is Tower)
			for (int j = startY; j < endY; ++j) {
				for (int i = startX; i < endX; ++i) {


					var pos2D = new Vector2(i, j) - center2D;
					var pos3D = transform.position - new Vector3(pos2D.x,0.5f,pos2D.y) / ppu;


					RaycastHit hit;

					if (!Physics.Raycast (new Vector3(entity.position.x,0.5f,entity.position.z),pos3D-new Vector3(entity.position.x,0.5f,entity.position.z),out hit,10f	,1<<20)) {
						if (Vector2.Distance (new Vector2 (i, j), pos) <= radius)
							tex.SetPixel (i, j, cutout);
					}
				}	
			}
		else
			for (int j = startY; j < endY; ++j) 
				for (int i = startX; i < endX; ++i) 
					if (Vector2.Distance (new Vector2 (i, j), pos) <= radius)
						tex.SetPixel (i, j, cutout);

	}

	void HandleEntity(Transform entity) {

		var pos3D = entity.position - transform.position;


		var size = GetComponent<Renderer>().bounds.size;

		ppu = tex.width / size.x;

	
		var center2D = new Vector2(tex.width, tex.height) /2;

		var pos2D = center2D - new Vector2(pos3D.x, pos3D.z) * ppu;

		var visRange = 10;
		CutoutCircle(Mathf.RoundToInt(pos2D.x),
			Mathf.RoundToInt(pos2D.y),
			Mathf.RoundToInt(visRange * ppu),
			entity
		);
	}





	void Update () {
		time += Time.deltaTime;
		if (time > 0.1f) {
			tex.SetPixels (texorigin.GetPixels ());

			foreach (var e in main.teams[Team.Good])
				HandleEntity (e.transform);

			tex.Apply ();
			time = 0;
		}

	}
}
