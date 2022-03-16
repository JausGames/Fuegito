using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] List<Player> players;
    [SerializeField] List<Player> alives;
    // Start is called before the first frame update
    void Start()
    {
        players.AddRange(FindObjectsOfType<Player>());
        alives.AddRange(players);
    }

    // Update is called once per frame
    void Update()
    {
        alives.Clear();
        foreach(Player pl in players)
        {
            if (pl.GetAlive()) alives.Add(pl);
        }

        var moy = new Vector3();

        for (int i = 0; i < alives.Count; i++)
        {
            moy += alives[i].transform.position;
        }

        if (alives.Count == 1)
        {
            var pos = alives[0].transform.position + new Vector3(0f, 7f , -7f);
            transform.position = Vector3.Slerp(transform.position, pos, 0.15f);
            return;

        }
        if (alives.Count == 0)
        {
            return;
        }
        var dist = new List<float>();
        for (int i = 0; i < alives.Count - 1; i++)
        {
            dist.Add(Vector3.Distance(alives[i].transform.position, alives[i + 1].transform.position) / alives.Count);
        }
        var farest = dist[0];
        for (int i = 0; i < dist.Count - 1; i++)
        {
            if (dist[i + 1] > dist[i]) farest = dist[i+1];
        }

        var x = 18f - 18f / (farest * 0.3f + 1);
        var z = -30f + 30f / (farest * 0.2f + 1);

        var posMoy = moy / alives.Count + new Vector3(0f, Mathf.Clamp(x, 4f , 14f), Mathf.Clamp(z, -18f, -4f));
        transform.position = Vector3.Slerp(transform.position, posMoy, 0.05f);
        
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position - new Vector3(0f, 18f, -25f), 2f);
    }
}
