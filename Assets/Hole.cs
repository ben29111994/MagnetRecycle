using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Hole : MonoBehaviour
{
    GameController gameController;

    private void OnEnable()
    {
        gameController = GameObject.FindGameObjectWithTag("Player").GetComponent<GameController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pixel") && other.transform.childCount > 0)
        {
            other.GetComponent<Tile>().isCheck = true;
            other.GetComponent<Tile>().isMagnet = true;
            other.transform.parent = null;
            gameController.pixels.Remove(other.GetComponent<Rigidbody>());
            other.GetComponent<SphereCollider>().isTrigger = true;
            other.GetComponent<Rigidbody>().velocity = Vector3.zero;
            other.transform.DOKill();
            other.transform.DOMove(transform.position, 0.5f);
            other.GetComponent<Rigidbody>().velocity = Vector3.zero;
            Explode(other.gameObject);
        }
        else if (other.CompareTag("Pixel"))
        {
            other.GetComponent<BoxCollider>().isTrigger = true;
            other.transform.DOKill();
            other.transform.DOMove(new Vector3(other.transform.position.x, other.transform.position.y  - 5, other.transform.position.z), 0.5f);
        }
    }

    void Explode(GameObject other)
    {
        StartCoroutine(delayExplode(other));
    }

    IEnumerator delayExplode(GameObject other)
    {
        yield return new WaitForSeconds(0.5f);
        other.GetComponent<SphereCollider>().isTrigger = false;
        var prefab = PoolManager.instance.GetObject(PoolManager.NameObject.pixelExplode);
        if (prefab != null)
        {
            prefab.SetActive(true);
            var getColor = prefab.GetComponent<ParticleSystem>().main;
            getColor.startColor = other.gameObject.GetComponent<Tile>().tileColor;
            prefab.transform.position = other.gameObject.transform.position;
            prefab.GetComponent<ParticleSystem>().Play();
        }
        Destroy(other.gameObject);
    }

    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.CompareTag("Pixel") && other.transform.childCount > 0)
    //    {
    //        if (other.transform.position.y < -2)
    //        {
    //            other.GetComponent<SphereCollider>().isTrigger = false;
    //            var prefab = PoolManager.instance.GetObject(PoolManager.NameObject.pixelExplode);
    //            if (prefab != null)
    //            {
    //                prefab.SetActive(true);
    //                var getColor = prefab.GetComponent<ParticleSystem>().main;
    //                getColor.startColor = other.gameObject.GetComponent<Tile>().tileColor;
    //                prefab.transform.position = other.gameObject.transform.position;
    //                prefab.GetComponent<ParticleSystem>().Play();
    //            }
    //            Destroy(other.gameObject);
    //        }
    //    }
    //    else if (other.CompareTag("Pixel"))
    //    {
    //        other.GetComponent<BoxCollider>().isTrigger = false;
    //    }
    //}
}
