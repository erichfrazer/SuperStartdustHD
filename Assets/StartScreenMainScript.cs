using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScreenMainScript : MonoBehaviour
{
    public GameObject ship;
    public GameObject rock;
    public AudioClip backgroundSound;
    public AudioClip buttonSound;

    Quaternion shipRotationOrg;
    Quaternion rockRotationOrg;
    AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        shipRotationOrg = ship.transform.rotation;
        rockRotationOrg = rock.transform.rotation;

        audioSource = GetComponent<AudioSource>();
        audioSource.clip = backgroundSound;
        audioSource.loop = true;
        audioSource.Play();
    }

    float angle = 0;

    // Update is called once per frame
    void Update()
    {
        angle = 15 * Time.time;
        ship.transform.rotation = shipRotationOrg * Quaternion.Euler(angle, 0, 0);
        rock.transform.rotation = rockRotationOrg * Quaternion.Euler(angle, 0, 0);
    }

    private void OnDisable()
    {
        AudioSource source = GetComponent<AudioSource>();
        source.Stop();
    }

    async public void OnBtnStart()
    {
        audioSource.loop = false;
        audioSource.clip = buttonSound;
        float duration = buttonSound.length;
        audioSource.Play();
        await Task.Delay((int)(duration * 1000));
        SceneManager.LoadScene("main");
    }

    void OnBtnHelp()
    {
    }

}
