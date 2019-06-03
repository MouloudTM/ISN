using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Loader
{

    public enum Scene {
        GameScene,
        Loading,
        MainMenu,
    }

private static Action loaderCallbackAction;
    public static void Load(Scene scene) {
        // Met en place l'action qui va être activée après que la Loading scene soit chargée
        loaderCallbackAction = () => {
            // Charge la scène choisie quand la Loading scene a chargée
            SceneManager.LoadScene(scene.ToString());
        };

        // Charge la Loading scene
        SceneManager.LoadScene(Scene.Loading.ToString());
    }

    public static void LoaderCallback() {
        if (loaderCallbackAction != null) {
            loaderCallbackAction();
            loaderCallbackAction = null;
        }
    }

}
