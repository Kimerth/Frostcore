﻿using UnityEngine;
using System.Collections;

public class P2DI_DestroyBlock : MonoBehaviour {

    private bool _isMining;
    private Camera _camera;
    private float _m_Resistance;
    private float _speed=1;
    private float _time;
	private GameObject _block;
	private GameObject _previousBlock = null;

	void Start () {
        _isMining = false;
        _camera = Camera.main;
	}

	public void MiningStart()
    {
        _time = 0;
        _isMining = true;
    }

    void Update()
    {
        if (_isMining == true)
        {
            if (isBreakable() == true)
            {
				if (_previousBlock != _block)
                {
					_time = Time.deltaTime * _speed;
                    _previousBlock = _block;
                }
				else
					_time += Time.deltaTime * _speed;
            }
            else
                _time = 0;
        }
        if (_time>=_m_Resistance && _m_Resistance!=0)
        {
            Destroy(_block);
        }
    }
    
    // Check to see if the player clicked an object with the Breakable tag
    private bool isBreakable()
    {
		GameObject objectHit = null;
		Vector3 pos2 = _camera.ScreenToWorldPoint (Input.mousePosition);
		Vector2 pos = new Vector2 (pos2.x, pos2.y);

		try{
			objectHit = Physics2D.OverlapPoint(pos).gameObject;
			if (objectHit.tag == "Breakable")
			{
				_block = objectHit;
				_m_Resistance = objectHit.GetComponent<BlockBreak>().m_Resistance;
				return true;
			}
		}catch{};
        
		return false;
    }

    public void MiningStop()
    {
        _time = 0;
        _isMining = false;
    }
}