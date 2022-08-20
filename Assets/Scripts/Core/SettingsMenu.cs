using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using GV.Shared.Collections;
using TMPro;

public class SettingsMenu : MonoBehaviour
{

     public AudioMixer audiomixer;

     Resolution[] resolutions;

     [SerializeField]
     TMP_Dropdown resolutionDropdown;
     private void Start()
     {
          resolutions = Screen.resolutions;

          resolutionDropdown.ClearOptions();

          List<string> options = new List<string>();

          int currentResolutionIndex = 0;
          foreach (var (resolution, index) in resolutions.WithIndex())
          {
               string option = $"{resolution.width} x {resolution.height}";
               options.Add(option);

               if(resolution.width == Screen.currentResolution.width && resolution.height == Screen.currentResolution.height)
               {
                    currentResolutionIndex = index;
               }
          }
          resolutionDropdown.AddOptions(options);
          resolutionDropdown.value = currentResolutionIndex;
          resolutionDropdown.RefreshShownValue();
     }

     public void SetVolume(float volume)
     {
          audiomixer.SetFloat("volume", volume);
     }

     public void SetQuality(int qualityIndex)
     {
          QualitySettings.SetQualityLevel(qualityIndex);
     }

     public void SetFullscreen(bool isFullscreen)
     {
          Screen.fullScreen = isFullscreen;
     }

     public void SetResolution(int resolutionIndex)
     {
          Resolution res = resolutions[resolutionIndex];
          Screen.SetResolution(res.width, res.height, Screen.fullScreen);
     }
}
