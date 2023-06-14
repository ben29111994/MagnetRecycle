using UnityEngine;
using GPUInstancer;
using DG.Tweening;

public class Tile : MonoBehaviour
{
    public Color tileColor;
    private Renderer meshRenderer;
    public bool isCheck = false;
    public bool isMagnet = false;
    public bool isHole = false;
    GameController gameController;
    Rigidbody rigid;

    private void OnEnable()
    {
        Init();
        gameController = GameObject.FindGameObjectWithTag("Player").GetComponent<GameController>();
        rigid = GetComponent<Rigidbody>();
    }

    public void Init()
    {
        if (meshRenderer == null)
            meshRenderer = GetComponent<Renderer>();
    }

    public void SetTransfrom(Vector3 pos,Vector3 scale)
    {
        transform.localPosition = pos;
        transform.localScale = new Vector3(scale.x,scale.y,scale.z);
    }

    public void SetColor(Color inputColor)
    {
        tileColor = inputColor;       
        meshRenderer.material.color = tileColor;
        tag = "Pixel";
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Wall"))
        {
            //Debug.Log("Hit");
            if (transform.childCount > 0)
            {
                isCheck = true;
                isMagnet = false;
                transform.parent = null;
                gameController.pixels.Remove(rigid);
                var prefab = PoolManager.instance.GetObject(PoolManager.NameObject.bullet);
                if (prefab != null)
                {
                    prefab.SetActive(true);
                    prefab.transform.position = collision.gameObject.transform.position;
                    prefab.GetComponent<ParticleSystem>().Play();
                }

                prefab = PoolManager.instance.GetObject(PoolManager.NameObject.pixelExplode);
                if (prefab != null)
                {
                    prefab.SetActive(true);
                    prefab.transform.position = transform.position;
                    prefab.GetComponent<ParticleSystem>().Play();
                }
                Destroy(collision.gameObject);
                transform.DOKill();
                Destroy(gameObject, 0.1f);
            }
            else
            {
                isMagnet = false;
                transform.parent = null;
                //transform.DOMoveY(0.5f, 0.5f);
                //gameController.RemovePixel(rigid);
            }
        }
    }

    public void Check()
    {
        isCheck = true;
    }   
}
