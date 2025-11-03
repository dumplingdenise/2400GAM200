using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // load saved values and set slider handles
        musicSlider.value = PlayerPrefs.GetFloat("MusicVol", 0.8f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVol", 0.8f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
