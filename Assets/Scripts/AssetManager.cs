using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.Networking;

public class AssetManager : MonoBehaviour
{
    public static AssetManager Instance;

    [SerializeField] private string wearableUri;
    [SerializeField] private List<Wearable> wearables = new List<Wearable>();

    [SerializeField] private string gltfUri;
    [SerializeField] Model model;
    [SerializeField] GameObject objModel;

    private int modelLoadedCount;

    public delegate void LoadAssetDelegate();
    public event LoadAssetDelegate OnLoadFinish;

    void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(GetWearable());
        objModel = CreateObject();
    }
    IEnumerator GetWearable()
    {
        using (UnityWebRequest req = UnityWebRequest.Get(wearableUri))
        {
            yield return req.SendWebRequest();

            string[] pages = wearableUri.Split('/');
            int page = pages.Length - 1;

            switch (req.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + req.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + req.error);
                    break;
                case UnityWebRequest.Result.Success:
                    //get Json data
                    JSONNode jsonData = JSON.Parse(req.downloadHandler.text);
                    //cvt to json obj
                    JSONObject obj = jsonData.AsObject;
                    //cvt nested to json arr
                    JSONArray data = obj["data"].AsArray;
                    //serialize
                    for (int i = 0; i < data.Count; i++)
                    {
                        Wearable m_wearable = new Wearable();

                        m_wearable.id = data[i]["id"].Value;
                        m_wearable.wearableId = data[i]["wearableId"].Value;
                        m_wearable.wearableName = data[i]["wearableName"].Value;
                        m_wearable.tokenId = data[i]["tokenId"].Value;
                        m_wearable.trxHash = data[i]["trxHash"].Value;
                        m_wearable.fileMeta = data[i]["fileMeta"].Value;
                        m_wearable.amount = int.Parse(data[i]["amount"].Value);
                        m_wearable.version = data[i]["version"].Value;
                        m_wearable.createdAt = data[i]["createdAt"].Value;
                        m_wearable.updateAt = data[i]["updatedAt"].Value;

                        wearables.Add(m_wearable);
                    }
                    break;
            }
            req.Dispose();
        }
    }

    private GameObject CreateObject()
    {
        Model m = Instantiate(model, transform.position, Quaternion.identity);
        m.SetData(gltfUri, 0);
        return m.gameObject;
    }

    public void CheckModelLoaded()
    {
        int n = 0;
        for (int x = 0; x < 2; x++)
        {
            for (int z = 0; z < 4; z++)
            {
                GameObject clone = Instantiate(objModel, new Vector3(x * 2, 0f, z * 2), Quaternion.identity);
                clone.GetComponent<Model>().characterNumber = n;
                n++;
            }
        }
        OnLoadFinish?.Invoke();

    }
}