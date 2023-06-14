using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using NaughtyAttributes;

public class MapFlowEffect : MonoBehaviour
{
    public static MapFlowEffect instance;
    public GameObject baseObject;
    public List<GameObject> listMap = new List<GameObject>();
    public float lengthX;
    public float lengthZ;
    public Color targetColor;
    public Color currentColor;
    float offSetX;
    float offSetZ;
    float sizeX;
    float sizeZ;
    public GameObject pivot;
    public GameObject start;
    public GameObject end;
    public bool isStop = false;

    void Start()
    {
        instance = this;
        sizeX = baseObject.transform.localScale.x;
        sizeZ = baseObject.transform.localScale.z;
        offSetX = lengthX / 2;
        offSetZ = lengthZ / 2;
        //GenerateMap();
    }

    public void GenerateMap()
    {
        for (float z = -offSetZ; z < offSetZ; z += 1)
        {
            for (float x = -offSetX; x < offSetX; x += 1)
            {
                GameObject parent = Instantiate(baseObject, Vector3.zero, Quaternion.identity);
                GameObject temp = Instantiate(baseObject, new Vector3(x * sizeX, baseObject.transform.position.y, z * sizeZ), Quaternion.identity);
                temp.transform.parent = parent.transform;
                parent.transform.parent = transform;
                listMap.Add(parent);
            }
        }
    }

    [Button("Run Effect")]
    public void RunMapFlowEffect()
    {
        for (int i = 0; i < listMap.Count; i++)
        {
            targetColor = Color.red;
            currentColor = Color.white;
            listMap[i].GetComponentInChildren<WaveAnimation>().targetColor = targetColor;
            listMap[i].GetComponentInChildren<WaveAnimation>()._currentColor = currentColor;
            listMap[i].transform.position = Vector3.zero;
        }
        pivot.transform.position = start.transform.position;
        pivot.transform.DOMove(end.transform.position, 1.75f);
        StartCoroutine(delayRepeat());
    }

    IEnumerator delayRepeat()
    {
        yield return new WaitForSeconds(2);
        if (!isStop)
        {
            RunMapFlowEffect();
        }
        else
        {
            foreach(var item in listMap)
            {
                item.transform.position = Vector3.zero;
            }
            gameObject.SetActive(false);
        }
    }
}
