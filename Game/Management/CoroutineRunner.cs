using System.Collections;
using UnityEngine;

namespace Code.Game.Management
{
    public class CoroutineRunner : MonoBehaviour
    {
        private static CoroutineRunner instance;

        private static CoroutineRunner Instance
        {
            get
            {
                instance = instance ? instance : new GameObject("_coroutineRunner").AddComponent<CoroutineRunner>();
                return instance;
            }
        }

        public static Coroutine RunCoroutine(IEnumerator cor)
        {
            return Instance.StartCoroutine(cor);
        }
    }
}