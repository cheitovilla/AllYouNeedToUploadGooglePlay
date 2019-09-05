using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class AndroidSplitLoadFirstScene : MonoBehaviour
{
    //Only use in Preloader Scene for android split APK
	private string nextScene = "SceneExperience";
	private bool obbisok = false;
	private bool loading = false;
	private bool replacefiles = false;
	//true if you wish to over copy each time
	
	private string[] paths = {	
		"Vuforia/DB_POC_Carulla.dat", //direccion de la db y nombre de la misma
		"Vuforia/DB_POC_Carulla.xml" // direccion de la db y nombre de la misma
	};

	void Update ()
	{
		if (Application.platform == RuntimePlatform.Android) {
			if (Application.dataPath.Contains (".obb") && !obbisok) {
				StartCoroutine (CheckSetUp ());
				obbisok = true;
			}
		} else {
			if (!loading) {
				StartApp ();
			}
		}
	}

	public void StartApp ()
	{
		loading = true;
		SceneManager.LoadSceneAsync(nextScene);
	}

	public IEnumerator CheckSetUp ()
	{
		//Check and install!
		for (int i = 0; i < paths.Length; ++i) {
			yield return StartCoroutine (PullStreamingAssetFromObb (paths [i]));
		}
		yield return new WaitForSeconds (1f); 
		StartApp ();
	}
	
	//Alternatively with movie files these could be extracted on demand and destroyed or written over
	//saving device storage space, but creating a small wait time.
	public IEnumerator PullStreamingAssetFromObb (string sapath)
	{ 
		if (!File.Exists (Application.persistentDataPath + sapath) || replacefiles) {
			WWW unpackerWWW = new WWW (Application.streamingAssetsPath + "/" + sapath);
			while (!unpackerWWW.isDone) {
				yield return null;
			}
			if (!string.IsNullOrEmpty (unpackerWWW.error)) {
				Debug.Log ("Error unpacking:" + unpackerWWW.error + " path: " + unpackerWWW.url);
				
				yield break; //skip it
			} else {
				Debug.Log ("Extracting " + sapath + " to Persistant Data");
				
				if (!Directory.Exists (Path.GetDirectoryName (Application.persistentDataPath + "/" + sapath))) {
					Directory.CreateDirectory (Path.GetDirectoryName (Application.persistentDataPath + "/" + sapath));
				}
				File.WriteAllBytes (Application.persistentDataPath + "/" + sapath, unpackerWWW.bytes);
			}
		}
		yield return 0;
	}
}