using UnityEngine;

namespace WayfarerKit.Runtime.Addressables
{
    [CreateAssetMenu(fileName = "AddressablesConfig", menuName = "WayfarerSDK/Addressables/Addressables Config", order = 2)]
    public class AddressablesConfig : ScriptableObject
    {
        public bool IsProd = true;
        public string ProfileStg = "stg";
        public string ProfileProd = "prod";
        public string[] LabelsToDownload;
    }
}