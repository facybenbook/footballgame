using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Wall : MonoBehaviour {

	public static Wall share;

	public List<GameObject> _bodiesEditor;
	public List<GameObject> _bodies;

	public Transform _poleLeft;
	public Transform _poleRight;

	private Transform _sphere;
	private Transform _sphere1;

    private bool _isWall;

    public bool IsWall
    {
        get { return _isWall; }
        set
        {
            _isWall = value;
            SetActiveWall(_isWall);
        }
    }

    public void OnToggle_Wall(bool val)
	{
	    SetActiveWall(val);
	}

	public void SetActiveWall(bool active) {
	    foreach (GameObject go in _bodies)
	    {
	        if (go.activeInHierarchy != active)
	        {
                go.SetActive(active);
	        }
	    }
	    foreach (GameObject go in _bodiesEditor)
	    {
	        if (go.activeInHierarchy != active)
	        {
                go.SetActive(active);
	        }
	    }
	}



	void Awake() {
		share = this;
		_bodies = new List<GameObject>();
		IsWall = false;
        //SetActiveWall();
	}

	public Renderer[] _renderers;

	public void setWallAlpha(bool isFront) {
		float a;

		foreach(Renderer renderer in _renderers) { 
			Material mat1 = renderer.materials[0];
			Material mat2 = renderer.materials[1];
			Material mat3 = renderer.materials[2];

			if(isFront) {
				a = 1f;
				mat3.shader = Shader.Find("Diffuse");
			}
			else {
				a = 0.4f;
				mat3.shader = Shader.Find("Transparent/Diffuse");
			}

			Color c = mat1.color;
			c.a = a;
			mat1.color = c;

			c = mat2.color;
			c.a = a;
			mat2.color = c;

			c = mat3.color;
			c.a = a;
			mat3.color = c;
		}
	}

	public void setWallUniform(Country country) {

		foreach(Renderer renderer in _renderers) {
			Material mat1 = renderer.materials[0];
			Material mat2 = renderer.materials[1];
			Material mat3 = renderer.materials[2];
			mat3.mainTexture = (Texture2D) Resources.Load( "Uniform/UniformFootball_" + country.ToString());
			mat1.mainTexture = (Texture2D) Resources.Load( "Numbers/number" + ((int)Random.Range(2, 22)));
			mat2.mainTexture = (Texture2D) Resources.Load( "Numbers/number" + ((int)Random.Range(2, 22)));
		}
	}

	public void setWall(Vector3 ballPosition) {
		if(IsWall == false) {
			SetActiveWall(false);
			return;
		}

		Vector3 pos = -ballPosition;
		pos.Normalize();
		float angleRadian = Mathf.Atan2(pos.z, pos.x);		// tinh' goc' lech
		float angle = 90 - angleRadian * Mathf.Rad2Deg;		// angle in degree
		float angleAbs = Mathf.Abs(angle);

		int count;		// bao nhieu nguoi dung hang rao

		if(angleAbs < 10) {
			count = 6;
		}
		else if(angleAbs < 20) {
			count = 5;
		}
		else if(angleAbs < 30) {
			count = 4;
		}
		else if(angleAbs < 40) {
			count = 3;
		}
		else {
			count = 2;
		}

		foreach(GameObject go in _bodies) 
			_bodiesEditor.Add(go);

		_bodies.Clear();

		foreach(GameObject go in _bodiesEditor)
			go.SetActive(false);

		for(int i = 0; i < count; ++i) {		// count la so nguoi se~ dung' trong hang` rao`, add dzo _bodies
			GameObject go = _bodiesEditor[Random.Range(0, _bodiesEditor.Count)];
			_bodies.Add(go);
			_bodiesEditor.Remove(go);
		}


		Vector3 posPole;
		if(angle > 0) {  // hang rao` se~ lay' cot doc ben trai lam chuan, nhin` tu` banh den' goal
			posPole = _poleLeft.position;
		}
		else {			// hang rao` se~ lay' cot doc ben fai lam chuan,  nhin` tu` banh den' goal
			posPole = _poleRight.position;
		}

		float z =  ballPosition.z + 11f;		// z cua hang rao, hang rao se dat o~ vi tri' z, * 0.55f co' nghia la hang rao se gan trai banh hon la gan khung thanh
		Vector3 lineVector = (posPole - ballPosition).normalized;		//  vector chi~ phuong tu` trai' banh den' cot doc

		Vector3 intersection;		// giao diem cua~ duong thang tu trai banh den' cot doc cat' mat phang co' z = z vua tim duoc o~ tren, cau thu thu' 2 se dat o giao diem nay
		Math3d.LinePlaneIntersection (out intersection, ballPosition, lineVector, Vector3.forward, new Vector3(0, 0, z));

		intersection.y = 0;

		float sign = Mathf.Sign(angle);

		intersection.x -= (sign * 0.5f);		// cau thu dau` tien se~ bi dat lech 0.5f, tuy luc' do' la ben trai' hay fai ma` cau thu se bi lech trai hay lech fai~

		for(int i = 0; i < _bodies.Count; ++i) {
			GameObject go = _bodies[i];
			go.transform.position = intersection;
			intersection.x += (sign * 0.5f);
			go.SetActive(true);
		}
	}
}
