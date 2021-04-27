//OSHA Hazard Identification Tool Unity App Code
//Authors: Samuel Hicks and Roods Jacques

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;
using MySql.Data;
using MySql.Data.MySqlClient;
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using System.Collections.Generic;


public class Screenshot : MonoBehaviour
{
    public GameObject Panel;
    public bool takingScreenshot = false;
	private int userid;
	private static int rand_imgid;
	private static int imgIDExists;

    void Start()
    {
        UnityInitializer.AttachToGameObject(this.gameObject);
    }

    public void CaptureScreenshot()
    {
        StartCoroutine(TakeScreenshotAndSave());
    }

    //Function to close the dialog box popup
    public void ClosePanel()
    {
        if (Panel != null)
        {
            bool isActive = Panel.activeSelf;
            Panel.SetActive(!isActive);
        }
    }


    private IEnumerator TakeScreenshotAndSave()
    {
        GameObject.Find("Canvas").GetComponent<Canvas>().enabled = false;//remove icons from screenshot

        //SQL Database Connection
        string connStr = "RDS_HOSTNAME; user=RDS_DB_NAME; database=RDS_USERNAME; port=RDS_PORT;password=RDS_PASSWORD";
        MySqlConnection conn = new MySqlConnection(connStr);

        //Initialize the Amazon Cognito credentials provider
        CognitoAWSCredentials credentials = new CognitoAWSCredentials(
            "AWS_IDENTITY_POOL_ID", // Identity pool ID
            RegionEndpoint.USEast1 // Region
        );

        takingScreenshot = true;
        yield return new WaitForEndOfFrame();

        //Create a new texture with the width and height of the screen
        Texture2D ss = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        //Read the pixels in the Rect starting at 0,0 and ending at the screen's width and height
        ss.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        ss.Apply();

		//Get the bytes of the image taken
        byte[] image_bytes = ss.EncodeToJPG();

        //Start of SQL Queries
        try
        {
            Debug.Log("Connecting to MySQL...");
            conn.Open(); //Connect to AWS RDS database

            //Get global userid variable
            userid = Login.userID;

            imgIDExists = 1;

            //Generate a random image id that doesn't exist in the database already
            while (imgIDExists > 0)
            {
                //Get random number for image id
                rand_imgid = UnityEngine.Random.Range(0, 10000);

                //Check Database for unclassified image imgid availability
                MySqlCommand check_imgid = new MySqlCommand("SELECT count(*) FROM image_unclass WHERE uimgID = @uimgID", conn);
                check_imgid.Parameters.AddWithValue("@uimgID", rand_imgid);
                imgIDExists = Convert.ToInt32(check_imgid.ExecuteScalar());

                if (imgIDExists == 0)
                {
                    break; //Image id is available
                }
                imgIDExists = 1; //Image id is not available
            }

            //Insert unclassified image into database
            MySqlCommand insert_img = new MySqlCommand("INSERT INTO image_unclass (uimgID, uimg) VALUES (@uimgid, @image_bytes)", conn);
            insert_img.Parameters.AddWithValue("@uimgid", rand_imgid);
            insert_img.Parameters.AddWithValue("@image_bytes", image_bytes);
            insert_img.ExecuteNonQuery();

            //Link image to the user who took it
            MySqlCommand insert_img1 = new MySqlCommand("INSERT INTO user_image_unclass(user_id, uimgID) VALUES(@user_id, @imgid)", conn);
            insert_img1.Parameters.AddWithValue("@user_id", userid);
            insert_img1.Parameters.AddWithValue("@imgid", rand_imgid);
            insert_img1.ExecuteNonQuery();
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

        //Connect to AWS Lambda
        var Client = new AmazonLambdaClient(credentials, RegionEndpoint.USEast1);

        //Request to send to AWS Lambda function which holds object detection code
        var request = new InvokeRequest()
        {
            FunctionName = "AWS_LAMBDA_FUNCTION_NAME",
            Payload = "{\"key1\" : " + rand_imgid + "}",
            InvocationType = InvocationType.RequestResponse
        };

        //Invoke the request to AWS Lambda function
        Client.InvokeAsync(request, (result) =>
        {
            if (result.Exception == null)
            {
                Debug.Log(Encoding.ASCII.GetString(result.Response.Payload.ToArray()));
            }
            else
            {
                Debug.LogError(result.Exception);
            }
        });

        yield return new WaitForSeconds(1);

        takingScreenshot = false;
        GameObject.Find("Canvas").GetComponent<Canvas>().enabled = true;//Make the icons reappear from screenshot

        //Popup the dialog box
        yield return new WaitForSeconds(0.5f);
        if (Panel != null)
        {
            Panel.SetActive(true);
        }

    }
}
