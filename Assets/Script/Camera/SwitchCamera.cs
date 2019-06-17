using UnityEngine;
using System.Collections;

public class SwitchCamera : MonoBehaviour {

	public static SwitchCamera share;

	public GameObject _cameraFront;
	public GameObject _cameraBack;
    public GameObject _cameraCorner;
    //Henry update value true ->false
	private bool _isFront = true;

    //Henry add more fucntion to set Active Camera directly
    public void setActiveCamera(string _cameraName)
    {
        switch (_cameraName)
        {
            case "CameraMain":
                _cameraFront.SetActive(true); //Disable Frond Camera
                _cameraBack.SetActive(false); //Disable Back camera
                _cameraCorner.SetActive(false); //Enable Corner Camera
                break;
            case "CameraBack":
                _cameraFront.SetActive(false); //Disable Frond Camera
                _cameraBack.SetActive(true); //Disable Back camera
                _cameraCorner.SetActive(false); //Enable Corner Camera

                break;
            case "CameraCorner":
                _cameraFront.SetActive(false); //Disable Frond Camera
                _cameraBack.SetActive(false); //Disable Back camera
                _cameraCorner.SetActive(true); //Enable Corner Camera
                break;

        }

    }

	public bool IsFront {
		get {
			return _isFront;
		}
		set {
			_isFront = value;

			_cameraFront.SetActive(_isFront);
			_cameraBack.SetActive(!_isFront);
			
			
			float a;
			if(_isFront) {
				a = 1f;
				_matGK3.shader = Shader.Find("Diffuse");
			}
			else {
				a = 0.4f;
				_matGK3.shader = Shader.Find("Transparent/Diffuse");
			}
			
			Color c = _matGK1.color;
			c.a = a;
			_matGK1.color = c;

			c = _matGK2.color;
			c.a = a;
			_matGK2.color = c;

			c = _matGK3.color;
			c.a = a;
			_matGK3.color = c;

			c = _matNetGoal.color;
			c.a = a;
			_matNetGoal.color = c;

			//HENRY CLOSE
            //Wall.share.setWallAlpha(_isFront);
		}
	}

	public Material _matGK1;
	public Material _matGK2;
	public Material _matGK3;
	public Material _matNetGoal;

	void Awake() {
		share = this;
	}
    //HENRY CLOSE
    /*
    public void OnToggle_BeGoalKeeper(bool val)
    {
        IsFront = !val;
    }

	public void onValueChange_FrontCamera(bool val) {
		IsFront = val;
	}*/
}
