//OSHA Hazard Identification Tool Unity App Code
//Authors: Samuel Hicks and Roods Jacques

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MySql.Data;
using MySql.Data.MySqlClient;
using System;
using System.Text;

public class ForgetPassword : MonoBehaviour
{
    public GameObject email;
	public GameObject username;
	public GameObject reset_password;
	private string Email;
	private string Username;
	private string Reset_password;
	private string HashedResetPass;
	private int userID;

	public Text GreenWarningText;
	public Text RedWarningText;

	public void ResetPassword()
    {
		//SQL Database Connection
		string connStr = "RDS_HOSTNAME; user=RDS_DB_NAME; database=RDS_USERNAME; port=RDS_PORT;password=RDS_PASSWORD";
		MySqlConnection conn = new MySqlConnection(connStr);

		//Check for empty fields
		if (Reset_password == "" || Email == "" || Username == "")
		{
			RedWarningText.text = "A field is empty or format is incorrect!!";
			return;
		}

		//Check password length
		if(Reset_password.Length < 8)
        {
			RedWarningText.text = "Password must be at least 8 characters!!";
			return;
		}

		//Start of SQL Queries
		try
		{
			Debug.Log("Connecting to MySQL...");
			conn.Open(); //Connect to AWS RDS database

			//User Check
			MySqlCommand check_user = new MySqlCommand("SELECT count(*) FROM user WHERE email = @email and username = @username", conn);//Check Database for user
			check_user.Parameters.AddWithValue("@email", Email);
			check_user.Parameters.AddWithValue("@username", Username);
			int UserExist = Convert.ToInt32(check_user.ExecuteScalar());

			//Check if user exists in the database
			if (UserExist == 1)
			{
				//Hash new password
				//This section has a hashing algorithm but I've taken it out for security purposes

				//Enter reset password into database under the entered username and email
				MySqlCommand cmd = new MySqlCommand("UPDATE user SET password = @password WHERE email = @email and username = @username", conn);

				cmd.Parameters.AddWithValue("@password", HashedResetPass);
				cmd.Parameters.AddWithValue("@email", Email);
				cmd.Parameters.AddWithValue("@username", Username);
				cmd.ExecuteNonQuery();

				username.GetComponent<InputField>().text = "";
				email.GetComponent<InputField>().text = "";
				reset_password.GetComponent<InputField>().text = "";

				RedWarningText.text = "";
				GreenWarningText.text = "Your password has been reset!!";
			}
			else
			{
				RedWarningText.text = "User doesn't Exist!!";
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
			if (username.GetComponent<InputField>().isFocused)
			{
				email.GetComponent<InputField>().Select();
			}
			if (email.GetComponent<InputField>().isFocused)
			{
				reset_password.GetComponent<InputField>().Select();
			}
		}

		if (Input.GetKeyDown(KeyCode.Return))
		{
			if (Email != "" && Username != "" && Reset_password != "")
			{
				ResetPassword();
			}
			RedWarningText.text = "A field is empty or format is incorrect!!";
		}

		//Get input data from the user
		Email = email.GetComponent<InputField>().text;
		Username = username.GetComponent<InputField>().text;
		Reset_password = reset_password.GetComponent<InputField>().text;
	}
}
