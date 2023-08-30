using System.Collections;
using UnityEngine;
using System.IO;

namespace Monocle.Unity
{
    public class GIFTaker : MonoBehaviour
    {
        [SerializeField]
        float rotateRate;

        [SerializeField]
        GameObject exteriorStructure;

        [SerializeField]
        GameObject interiorStructure;

        void Start()
        {
            string[] oldFiles = Directory.GetFiles(Application.dataPath + "/../../gif", "*.png", SearchOption.TopDirectoryOnly);
            if (oldFiles != null && oldFiles.Length > 0)
            {
                foreach (string file in oldFiles)
                {
                    File.Delete(file);
                }
            }

            if (exteriorStructure != null && interiorStructure != null)
            {
                StartCoroutine(TakeStructureGif());
            }
            else
            {
                StartCoroutine(TakeGif());
            }
        }

        IEnumerator TakeStructureGif()
        {
            int r = 0;
            int count = 0;
            while (r <= 360)
            {
                Save(count);
                transform.RotateAround(Vector3.zero, Vector3.up, rotateRate);
                r += (int)rotateRate;
                count++;
                yield return null;
            }
            r = 0;
            exteriorStructure.SetActive(false);
            interiorStructure.SetActive(true);
            yield return null;
            while (r <= 360)
            {
                Save(count);
                transform.RotateAround(Vector3.zero, Vector3.up, rotateRate);
                r += (int)rotateRate;
                count++;
                yield return null;
            }
        }

        IEnumerator TakeGif()
        {
            int r = 0;
            int count = 0;
            while (r <= 360)
            {
                Save(count);
                transform.RotateAround(Vector3.zero, Vector3.up, rotateRate);
                r += (int)rotateRate;
                count++;
                yield return null;
            }
        }

        void Save(int count)
        {
            ScreenCapture.CaptureScreenshot(Application.dataPath + "/../../gif/" + count.ToString("D3") + ".png");
        }
    }
}