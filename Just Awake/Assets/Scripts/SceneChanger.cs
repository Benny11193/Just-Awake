using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public string NextSceneName;
    public void LoadToScene()
    {
        SceneManager.LoadScene(NextSceneName);
    }
}
