//OSHA Hazard Identification Tool Unity App Code
//Authors: Samuel Hicks and Roods Jacques

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SignOut : MonoBehaviour
{
    public int userid;
    public string username;
    public void SignOutButton()
    {
        //Get logged in user data
        userid = Login.userID;
        username = Login.userUsername;

        //Erase current logged in user data
        userid = 0;
        username = "";

        SceneManager.LoadScene("SignIn");
    }
    public void OpenWebsite()
    {
        Application.OpenURL("URL_TO_OUR_WEBSITE");
    }
}
