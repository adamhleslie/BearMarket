using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour {

	public Button startGame;
        public Button options;
        public Button quit;
        public Text title;

	public Button singleColi;
        public Button multiColi;
        public Button singleLogo;
        public Button cancel;

	// Use this for initialization
	void Start () {
          startGame.onClick.AddListener(TaskOnClick);

          singleLogo.gameObject.SetActive(false);
          singleColi.gameObject.SetActive(false);
          multiColi.gameObject.SetActive(false);
          cancel.gameObject.SetActive(false);

          singleLogo.onClick.AddListener(StartSingleLogo);
          singleColi.onClick.AddListener(StartSingleColi);
          multiColi.onClick.AddListener(StartMultiColi);
          cancel.onClick.AddListener(CancelGame);
        }

        void StartSingleLogo () {
          CloseUI();
          SceneManager.LoadScene("Logo");
        }

        void StartMultiColi() {
          CloseUI();
          SceneManager.LoadScene("ColiseumMulti");
        }

        void StartSingleColi() {
          CloseUI();
          SceneManager.LoadScene("Coliseum");
        }

        void CancelGame() {
          startGame.gameObject.SetActive(true);
          options.gameObject.SetActive(true);
          quit.gameObject.SetActive(true);

          singleLogo.gameObject.SetActive(false);
          singleColi.gameObject.SetActive(false);
          multiColi.gameObject.SetActive(false);
          cancel.gameObject.SetActive(false);
        }

	void TaskOnClick () {
          startGame.gameObject.SetActive(false);
          options.gameObject.SetActive(false);
          quit.gameObject.SetActive(false);

          singleLogo.gameObject.SetActive(true);
          singleColi.gameObject.SetActive(true);
          multiColi.gameObject.SetActive(true);
          cancel.gameObject.SetActive(true);
	}

        void CloseUI() {
          title.gameObject.SetActive(false);
          singleLogo.gameObject.SetActive(false);
          singleColi.gameObject.SetActive(false);
          multiColi.gameObject.SetActive(false);
          cancel.gameObject.SetActive(false);

          startGame.gameObject.SetActive(false);
          options.gameObject.SetActive(false);
          quit.gameObject.SetActive(false);
        }
}
