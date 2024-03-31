using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace MFPS.Addon.Clan
{
    public class bl_TagColorSelector : MonoBehaviour
    {
        [SerializeField] private Button colorButtonTemplate = null;
        [SerializeField] private RectTransform listPanel = null;

        public int SelectedColor { get; private set; } = 0;
        private List<Button> cacheButtons = new List<Button>();

        /// <summary>
        /// 
        /// </summary>
        private void Start()
        {
            colorButtonTemplate.gameObject.SetActive(false);
            InstanceButtons();
            OnClickColorButton(0);
        }

        /// <summary>
        /// 
        /// </summary>
        public void InstanceButtons()
        {
            var all = bl_ClanSettings.Instance.GetAllTagColors();
            for (int i = 0; i < all.Length; i++)
            {
                var go = Instantiate(colorButtonTemplate.gameObject);
                go.transform.SetParent(listPanel, false);
                go.SetActive(true);
                var button = go.GetComponent<Button>();
                button.GetComponent<Image>().color = all[i];
                int bid = i;
                button.onClick.AddListener(() => OnClickColorButton(bid));
                cacheButtons.Add(button);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        public void OnClickColorButton(int id)
        {
            foreach (var item in cacheButtons)
            {
                item.interactable = true;
            }
            SelectedColor = id;
            cacheButtons[id].interactable = false;
        }
    }
}