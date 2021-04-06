using UnityEngine;
using System.IO;

public class LoadAssetBundle : MonoBehaviour
{
    AssetBundle assetBundle;

    public void LoadAssetBundleByName(string name)
    {
        assetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, name));
        Debug.Log(assetBundle == null ? "Failed to load Asset Bundle" : "Asset bundle loaded succesfully");
        assetBundle.LoadAllAssets();
    }
}
