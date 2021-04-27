//OSHA Hazard Identification Tool Unity App Code
//Authors: Samuel Hicks and Roods Jacques

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using UnityEngine.SceneManagement;
using System.Text;
using System.Security.Cryptography;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

public class Login : MonoBehaviour
{
	public GameObject email;
	public GameObject password;
	private string Email;
	private string Password;
	private string HashedPass;

	public static int userID;
	public static string userUsername;

	//define warning text
	public Text RedWarningText;

	public void LoginButton()
	{
		//SQL Database Connection
		string connStr = "RDS_HOSTNAME; user=RDS_DB_NAME; database=RDS_USERNAME; port=RDS_PORT;password=RDS_PASSWORD";
		MySqlConnection conn = new MySqlConnection(connStr);

		//Check for empty fields
		if (Password == "" || Email == "" || !(Email.Contains("@") && Email.Contains(".")))
		{
			RedWarningText.text = "A field is empty or format is incorrect!!";
			return;
		}

		//Start of SQL Queries
		try
		{
			Debug.Log("Connecting to MySQL...");
			conn.Open(); //Connect to AWS RDS database

			//Email Check
			MySqlCommand check_Email = new MySqlCommand("SELECT count(*) FROM user WHERE email = @email", conn);//Check Database for email
			check_Email.Parameters.AddWithValue("@email", Email);
			int EmailExist = Convert.ToInt32(check_Email.ExecuteScalar());

			//Check if email exists in the database and if the entered password is 8 characters or more
			if (EmailExist == 1 && Password.Length > 8 )
			{
				//Hash password check
				//This section has a hashing algorithm but I've taken it out for security purposes

				//Check Database for user
				MySqlCommand login_user = new MySqlCommand("SELECT count(*) FROM user WHERE email = @email AND password = @password", conn);
				login_user.Parameters.AddWithValue("@email", Email);
				login_user.Parameters.AddWithValue("@password", HashedPass);
				int LoginUser = Convert.ToInt32(login_user.ExecuteScalar());

				//Login the user. Set user data
				if (LoginUser == 1)
				{
					email.GetComponent<InputField>().text = "";
					password.GetComponent<InputField>().text = "";

					//Get user_id for logged in user
					MySqlCommand user_id_check = new MySqlCommand("SELECT user_id FROM user WHERE email = @email", conn);
					user_id_check.Parameters.AddWithValue("@email", Email);
					userID = Convert.ToInt32(user_id_check.ExecuteScalar());

					//Get username for logged in user
					MySqlCommand user_name = new MySqlCommand("SELECT username FROM user WHERE user_id = @user_id", conn);
					user_name.Parameters.AddWithValue("@user_id", userID);
					object getUN = user_name.ExecuteScalar();

					if(getUN != null)
                    {
						userUsername = getUN.ToString();
                    }

					Mysettings.usernamestr = userUsername;

					SceneManager.LoadScene("LoginPage");
				}
				else
				{
					RedWarningText.text = "User doesn't Exist!!";
					return;
				}
			}
			else
			{
				RedWarningText.text = "Email or Password is incorrect format!!";
				return;
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
	}

	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Tab))
		{
			if (email.GetComponent<InputField>().isFocused)
			{
				password.GetComponent<InputField>().Select();
			}
		}

		if (Input.GetKeyDown(KeyCode.Return))
		{
			if (Email != "" && Password != "")
			{
				LoginButton();
			}
			RedWarningText.text = "A field is empty or format is incorrect!!";
		}

		//Get input data from the user
		Email = email.GetComponent<InputField>().text;
		Password = password.GetComponent<InputField>().text;
	}
}
