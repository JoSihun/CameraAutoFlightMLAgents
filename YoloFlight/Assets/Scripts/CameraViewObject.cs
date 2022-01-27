using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ca : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    private List<GameObject> findList = null;
    private Camera cam;

    void Start()
    {
        cam = UnityEngine.Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < findList.Count; i++)
        {
            Vector3 viewPos = cam.WorldToViewportPoint(findList[i].transform.position);
            if (0 <= viewPos.x && viewPos.x <= 1 && 0 <= viewPos.y && viewPos.y <= 1 && viewPos.z > 0)
            {
                Debug.Log("Object Name in Camera = " + findList[i].name);
            }
        }
    }
}
