//OSHA Hazard Identification Tool Unity App Code
//Authors: Samuel Hicks and Roods Jacques

using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System;
using UnityEngine.SceneManagement;
using System.Text;
using System.Security.Cryptography;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Collections;

public class Register : MonoBehaviour
{
	public GameObject username;
	public GameObject email;
	public GameObject password;
	public GameObject confPassword;

	private string Username;
	private string Email;
	private string Password;
	private string ConfPassword;
	private string HashedPass;
	private int userIDExists;
	private int rand_userid;

	public Text GreenWarningText;
	public Text RedWarningText;

	public void RegisterButton()
	{
		//SQL Database Connection
		string connStr = "RDS_HOSTNAME; user=RDS_DB_NAME; database=RDS_USERNAME; port=RDS_PORT;password=RDS_PASSWORD";
		MySqlConnection conn = new MySqlConnection(connStr);

		bool UN = false;
		bool EM = false;
		bool PW = false;
		bool CPW = false;

		//Check for empty fields
		if (Username == "" || Email == "" || Password == "" || ConfPassword == "")
        {
			RedWarningText.text = "A field is empty or format is incorrect!!";
			return;
        }

		//Start of SQL Queries
		try
		{
			Debug.Log("Connecting to MySQL...");
			conn.Open(); //Connect to AWS RDS database

			//Username Check
			MySqlCommand check_User_Name = new MySqlCommand("SELECT count(*) FROM user WHERE username = @username", conn);//Check Database for username avalability
			check_User_Name.Parameters.AddWithValue("@username", Username);
			int UserExist = Convert.ToInt32(check_User_Name.ExecuteScalar());

			//Check if a user exists in the database or not
			if (UserExist > 0)
			{
				RedWarningText.text = "Username already taken!!";
				return;
			}
			else
			{
				UN = true;
			}

			//Email Check
			MySqlCommand check_Email = new MySqlCommand("SELECT count(*) FROM user WHERE email = @email", conn);//Check Database for email avalability
			check_Email.Parameters.AddWithValue("@email", Email);
			int EmailExist = Convert.ToInt32(check_Email.ExecuteScalar());

			//Check if an email exists in the database or not
			if (EmailExist > 0)
			{
				RedWarningText.text = "Email already Exists!!";
				return;
			}
			else
			{
				//Make sure the entered email has @ and . in it
				if (Email.Contains("@") && Email.Contains("."))
				{
					EM = true;
				}
				else
				{
					RedWarningText.text = "Email is formated incorrectly!!";
					return;
				}
			}

            //Password Check
            if (Password.Length > 8)
            {
                PW = true;
			}
            else
            {
				RedWarningText.text = "Password must be at least 8 characters long!!";
				return;
			}

			//Confirm Password
            if (ConfPassword == Password)
            {
                CPW = true;
            }
            else
            {
				RedWarningText.text = "Passwords Don't Match!!";
				return;
			}

			//Check if everything is correct
			if (UN == true && EM == true && PW == true && CPW == true)
			{
				//Hash the entered password
				//This section has a hashing algorithm but I've taken it out for security purposes

				userIDExists = 1;

				//Generate a random user id that doesn't exist in the database already
				while (userIDExists > 0)
				{
					//Get random number for user id
					rand_userid = UnityEngine.Random.Range(0, 10000);

					//Check Database for user userid availability
					MySqlCommand check_userid = new MySqlCommand("SELECT count(*) FROM user WHERE user_id = @user_id", conn);
					check_userid.Parameters.AddWithValue("@user_id", rand_userid);
					userIDExists = Convert.ToInt32(check_userid.ExecuteScalar());

					if (userIDExists == 0)
					{
						break; //User id is available
					}
					userIDExists = 1; //User id is not available
				}

				//Insert user info into database
                MySqlCommand cmd = new MySqlCommand("INSERT INTO user (user_id, username, password, email) VALUES (@user_id, @username, @password, @email)", conn);

				cmd.Parameters.AddWithValue("@user_id", rand_userid);
				cmd.Parameters.AddWithValue("@username", Username);
                cmd.Parameters.AddWithValue("@password", HashedPass);
                cmd.Parameters.AddWithValue("@email", Email);
                cmd.ExecuteNonQuery();
            }
		}
		catch (Exception e)
		{
			Debug.Log("Error Generated. Details: " + e.ToString());
		}
		finally
		{
			conn.Close(); //Close the connection to the AWS RDS database
			Debug.Log("Closing Connection...");
		}

		username.GetComponent<InputField>().text = "";
		email.GetComponent<InputField>().text = "";
		password.GetComponent<InputField>().text = "";
		confPassword.GetComponent<InputField>().text = "";

		RedWarningText.text = "";
		GreenWarningText.text = "Registration Complete!!";
		StartCoroutine(DelayChangeScene("SignIN", 3f));
	}

	private IEnumerator DelayChangeScene(string sceneToChangeTo, float delay)
	{
		yield return new WaitForSeconds(delay);
		SceneManager.LoadScene(sceneToChangeTo);
	}


	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Tab))
		{
			if (username.GetComponent<InputField>().isFocused)
			{
				email.GetComponent<InputField>().Select();
			}
			if (email.GetComponent<InputField>().isFocused)
			{
				password.GetComponent<InputField>().Select();
			}
			if (password.GetComponent<InputField>().isFocused)
			{
				confPassword.GetComponent<InputField>().Select();
			}
		}

		if (Input.GetKeyDown(KeyCode.Return))
		{
			if (Password != "" && Email != "" && Password != "" && ConfPassword != "")
			{
				RegisterButton();
			}
			RedWarningText.text = "A field is empty or format is incorrect!!";
		}

		//Get input data from the user
		Username = username.GetComponent<InputField>().text;
		Email = email.GetComponent<InputField>().text;
		Password = password.GetComponent<InputField>().text;
		ConfPassword = confPassword.GetComponent<InputField>().text;
	}
}
