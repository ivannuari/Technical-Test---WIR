using GLTFast;
using System.Collections.Generic;
using UnityEngine;

public class Model: MonoBehaviour
{
    public int characterNumber;
    [SerializeField] private List<string> animStates = new List<string>();

    private void OnEnable()
    {
        AssetManager.Instance.OnLoadFinish += Instance_OnLoadFinish;
    }

    private void OnDisable()
    {
        AssetManager.Instance.OnLoadFinish -= Instance_OnLoadFinish;
    }

    private void Instance_OnLoadFinish()
    {
        TryGetComponent<Animation>(out Animation anim);
        if(!anim)
        {
            return;
        }
        foreach (AnimationState state in anim)
        {
            animStates.Add(state.name);
        }
        anim.clip = anim[animStates[characterNumber]].clip;
        anim.Play();
    }

    public async void SetData(string uri , int n)
    {
        characterNumber = n;

        GltfAsset gltfAsset = GetComponent<GltfAsset>();
        gltfAsset.Url = uri;

        var gltf = new GLTFast.GltfImport();
        var success = await gltf.Load(uri);

        if (success)
        {
            // Instantiate the glTF's main scene
            await gltf.InstantiateMainSceneAsync(this.transform);
            Debug.Log("finish load model");
            AssetManager.Instance.CheckModelLoaded();
            Destroy(gameObject, 1f);
        }
        else
        {
            Debug.Log("Loading glTF failed!");
        }
    }
}