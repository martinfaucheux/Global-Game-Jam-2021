using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
public class ItemListUI : MonoBehaviour
{
    // Start is called before the first frame update

    private static ItemListUI _instance;
    public static ItemListUI instance { get { return _instance; } }

    private Camera camera;
    void Awake()
    {

        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    public void showList(List<GameObject> list)

    {

        List<Transform> transforms;
        transforms = GetComponentsInChildren<Transform>().ToList();
        destroyOldList(transforms); 
        var i = 0;
        if (list.Count == 0)
            {
                return;
            }
        transforms = GetComponentsInChildren<Transform>().ToList();
        
        foreach (var tr in transforms)
  {
            if (tr.gameObject.tag == "SecondCam" && tr.gameObject.layer != 5)
                {
                if (i < list.Count)
                {
                    Debug.Log(i);
                    GameObject listObject = Instantiate(list[i], tr.position, transform.rotation);
                    listObject.transform.parent = tr;
                    listObject.GetComponent<FollowMouse>().enabled = false;
                    listObject.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
                    i++;
                }
            }
        }
    }

    void destroyOldList(List<Transform> transforms)
    {
        foreach (var tr in transforms)
        {
            if (tr.gameObject.tag != "SecondCam")
            {
                Destroy(tr.gameObject);
            }

        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
