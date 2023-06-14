using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dissolve : MonoBehaviour {
	public Material[] Mat_Dissolve;
    public Material[] Mat_Fill;

    public Slider sliderDissolve;
    public Slider sliderFill;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		
	}

    public void OnValueChangeDissolve()
    {
        for(int i=0;i<Mat_Dissolve.Length;i++)
        {
            Mat_Dissolve[i].SetFloat("_SliceAmount", sliderDissolve.value);
        }
    }
    public void OnValueChangeFill()
    {
        for (int i = 0; i <Mat_Fill.Length; i++)
        {
            Mat_Fill[i].SetFloat("_FillRate", sliderFill.value); 
        }
    }
}
