using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SelectGame : MonoBehaviour {

        public Button singleColi;
        public Button multiColi;
        public Button singleLogo;
        public Button cancel;

	// Use this for initialization
	void Start () {
                singleColi.onClick.AddListener(StartSingleColi);
                multiColi.onClick.AddListener(StartMultiColi);
                singleLogo.onClick.AddListener(StartSingleLogo);
                singleLogo.onClick.AddListener(CancelGame);
	}

        void StartSingleLogo () {
          SceneManager.LoadScene("Logo");
        }

        void StartMultiColi() {
          SceneManager.LoadScene("ColiseumMulti");
        }

        void StartSingleColi() {
          SceneManager.LoadScene("Coliseum");
        }

        void CancelGame() {
          SceneManager.LoadScene("Main Menu");
        }

}
