using FracturedState.Game.Data;
using UnityEngine;
using UnityEngine.UI;

namespace FracturedState.UI
{
    public class RSSItemDisplay : MonoBehaviour
    {
        [SerializeField] private Text title;

        [SerializeField] private Text detail;

        [SerializeField] private Text content;

        public void SetItem(RssItem item)
        {
            title.text = item.Title;
            detail.text = item.PrettyDetails();
            content.text = item.PrettyDescription();

            var btn = GetComponent<UnityEngine.UI.Button>();
            if (btn != null)
            {
                var openUrl = gameObject.AddComponent<OpenURL>();
                openUrl.SetURL(item.Link);
                btn.onClick.AddListener(openUrl.OpenPage);
            }
        }

        private void OnDestroy()
        {
            var btn = GetComponent<UnityEngine.UI.Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
            }
        }
    }
}