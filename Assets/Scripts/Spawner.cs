using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour {

	[Tooltip("Name of prefab in resources to clone.")]
	[SerializeField] string toClone;

	[Tooltip("How long to wait before spawning another object.")]
    [SerializeField] float delayToRespawn;

	[Tooltip("How long to wait before destroying each spawned object.")]
    [SerializeField] float delayToDestroy;

    GameObject cube; 
	Color[] colors;
	void MakeAnother() {
		float x = Random.Range(-4, 4);
  		float y = Random.Range(4, 10);
  		float z = Random.Range(-4, 8);
		GameObject myCube = Instantiate(cube, new Vector3(x,y,z), Quaternion.identity) as GameObject;
		myCube.GetComponent<Renderer>().material.color = colors[Random.Range(0, colors.Length)];
		myCube.transform.localScale = Vector3.one * Random.Range(0.3f, 0.8f);
		Destroy(myCube, delayToDestroy);
	}

	void Start () {
		colors = new Color[6];
		colors[0] = Color.cyan;
		colors[1] = Color.red;
		colors[2] = Color.green;
		colors[3] = new Color(0.5f, 0.2f, 0);
		colors[4] = Color.yellow;
		colors[5] = Color.magenta;
		cube = Resources.Load(toClone, typeof(GameObject)) as GameObject;
		InvokeRepeating("MakeAnother", 0, delayToRespawn);
	}
}