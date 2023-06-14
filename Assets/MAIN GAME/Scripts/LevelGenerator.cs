using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour {

    public static LevelGenerator instance;
    public List<Texture2D> list2DMaps = new List<Texture2D>();
	public Texture2D map;
    public Tile tilePrefab;
    public GameObject parentObject;
    public int numOfStacks;
    Transform currentParent;
    Vector3 originalPos;
    float width;
    public GameObject l, r, u, d;

    void OnEnable()
    {
        instance = this;
        var currentLevel = PlayerPrefs.GetInt("currentLevel");
        map = list2DMaps[currentLevel];
        originalPos = parentObject.transform.position;
        currentParent = parentObject.transform;
        GameController.totalPixel = 0;
        GenerateMap(map);
        parentObject.transform.position = originalPos;
        parentObject.transform.localScale = Vector3.one * (15 / width);
    }

    private void GenerateMap(Texture2D texture)
    {
        width = texture.width;
        float ratioX = texture.width;
        float ratioY = texture.height;
        float ratio;
        if (ratioY > ratioX)
        {
            ratio = ratioX / ratioY;
        }
        else
        {
            ratio = ratioY / ratioX;
        }
        if(ratio < 0.6f && ratio > 0.4f)
        {
            ratio = 1;
        }

        Vector3 positionTileParent = new Vector3(-((texture.width - 1) * ratio / 2), 0, -((texture.height - 1) * ratio / 2));
        currentParent.localPosition = positionTileParent;

        for (int x = 0; x < texture.width - 1; x++)
        {
            for (int y = 0; y < texture.height - 1; y++)
            {
                GenerateTile(texture, x, y, ratio);
            }
        }
        //var lSpawn = Instantiate(l, new Vector3(-1, 1, texture.height / 2), Quaternion.identity);
        //var rSpawn = Instantiate(l, new Vector3(texture.width + 1, 1, texture.height / 2), Quaternion.identity);
        //var uSpawn = Instantiate(l, new Vector3(-1, 1, texture.width / 2), Quaternion.identity);
        //var dSpawn = Instantiate(l, new Vector3(texture.height + 1, 1, texture.width / 2), Quaternion.identity);
        l.transform.position = new Vector3(-(ratio * texture.width) + ratio, -1.5f * ratio, 0);
        r.transform.position = new Vector3(-ratio, -1.5f * ratio, 0);
        u.transform.position = new Vector3(-texture.width / 2, -1.5f * ratio, texture.height/2 * ratio - ratio);
        d.transform.position = new Vector3(-texture.width / 2, -1.5f * ratio, -(texture.height/2) * ratio);
        l.transform.SetParent(currentParent);
        r.transform.SetParent(currentParent);
        u.transform.SetParent(currentParent);
        d.transform.SetParent(currentParent);
        l.transform.localScale *= ratio;
        r.transform.localScale *= ratio;
        u.transform.localScale *= ratio;
        d.transform.localScale *= ratio;
    }

    private void GenerateTile(Texture2D texture, int x, int y, float ratio)
    {
        Tile instance;
        instance = Instantiate(tilePrefab);
        instance.transform.SetParent(currentParent);
        Color pixelColor = texture.GetPixel(x, y);
        Vector3 pos = new Vector3(x - texture.width / 2, 0, y) * ratio;
        Vector3 scale = Vector3.one * ratio;

        if (pixelColor.a == 0 || pixelColor == null)
        {
            pixelColor = new Color32(169, 169, 169, 255);
            Destroy(instance.GetComponent<BoxCollider>());
            Destroy(instance.GetComponent<Rigidbody>());
        }
        else
        {
            GameController.totalPixel++;
        }

        instance.Init();
        instance.SetTransfrom(pos, scale);
        instance.SetColor(pixelColor);
    }

}
