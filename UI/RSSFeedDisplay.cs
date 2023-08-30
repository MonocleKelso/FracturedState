using FracturedState.Game.Data;
using UnityEngine;

namespace FracturedState.UI
{
    public class RSSFeedDisplay : MonoBehaviour
    {
        [SerializeField] private string feedUrl;

        [SerializeField] private RSSItemDisplay itemPrefab;

        [SerializeField] private string maskUrl;

        [SerializeField] private GameObject loading;

        [SerializeField] private GameObject fail;

        private void OnEnable()
        {
            loading.SetActive(true);
            RSSFeed.Get(feedUrl, this,
            feed =>
            {
                string baseUrl = feed.Channel.Link;
                foreach (var item in feed.Channel.Items)
                {
                    if (System.Array.IndexOf(item.Categories, "Fractured State") >= 0)
                    {
                        if (!string.IsNullOrEmpty(maskUrl) && !string.IsNullOrEmpty(item.Link))
                        {
                            item.Link = item.Link.Replace(baseUrl, maskUrl);
                        }
                        var itemGo = Instantiate(itemPrefab, transform);
                        itemGo.SetItem(item);
                    }
                }
                loading.SetActive(false);
            },
            () =>
            {
                loading.SetActive(false);
                fail.SetActive(true);
            });
        }
    }
}