using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Hello : MonoBehaviour {
	public Text TextBoxText;
	public bool hasExtra = false;
	public AndroidJavaObject extras;

	public string arguments = "";

	public AndroidJavaObject intent;

	// Use this for initialization
	void Start () {
		TextBoxText = gameObject.GetComponent<Text>(); 
		AndroidJavaClass UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"); 
		AndroidJavaObject currentActivity = UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
		intent = currentActivity.Call<AndroidJavaObject>("getIntent");
		hasExtra = intent.Call<bool> ("hasExtra", "arguments");
		Debug.Log("start");
	
	}
	
	// Update is called once per frame
	void Update () {
		if (hasExtra) {
			Debug.Log("has extra");
			extras = intent.Call<AndroidJavaObject> ("getExtras");
			arguments = extras.Call<string> ("getString", "arguments");
			TextBoxText.text =arguments;
			Debug.Log(arguments);
		} else {
			TextBoxText.text = "No Extra from Android";
			Debug.Log("no extra");

		}
		
	}

	void FixedUpdate(){

		if (Application.platform == RuntimePlatform.Android)
		{
			if (Input.GetKey(KeyCode.Escape))
			{
				Application.Quit();
			}
		}
	}
}
