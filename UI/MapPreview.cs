using FracturedState.Game;
using UnityEngine;
using UnityEngine.UI;

namespace FracturedState.UI
{
    public class MapPreview : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private AspectRatioFitter fitter;

        private Sprite currentSprite;
        
        private void Awake()
        {
            ShowMapPreview(MapSelect.CurrentMapName, MapSelect.CurrentMapPop);
            MapSelect.OnMapChanged.AddListener(ShowMapPreview);
        }

        private void OnDestroy()
        {
            MapSelect.OnMapChanged.RemoveListener(ShowMapPreview);
        }

        private void ShowMapPreview(string selectedMapName, int selectedMapPop)
        {
            if (currentSprite != null)
            {
                Destroy(currentSprite.texture);
                Destroy(currentSprite);
            }

            var file = $"{DataLocationConstants.GameRootPath}{DataLocationConstants.MapDirectory}/{selectedMapName}/preview.jpg";
            if (!System.IO.File.Exists(file))
            {
                image.enabled = false;
                return;
            }
            
            var www = new WWW(file);
            if (!string.IsNullOrEmpty(www.error))
            {
                image.enabled = false;
                return;
            }

            image.enabled = true;
            var texture = new Texture2D(32, 32);
            www.LoadImageIntoTexture(texture);
            currentSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero, 100);
            image.sprite = currentSprite;
            image.preserveAspect = true;
            fitter.aspectRatio = texture.width / (float)texture.height;
            www.Dispose();
        }
    }
}