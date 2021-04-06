using UnityEngine;

public class LoadAssetBundle : MonoBehaviour
{
    AssetBundle assetBundle;

    public void LoadAssetBundleAtPath(string path)
    {
        assetBundle = AssetBundle.LoadFromFile(path);
        Debug.Log(assetBundle == null ? "Failed to load Asset Bundle" : "Asset bundle loaded succesfully");

        assetBundle.LoadAllAssets();
    }
}
