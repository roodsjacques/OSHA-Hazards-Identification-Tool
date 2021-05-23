# OSHA-Hazards-Identification-Tool
This tool was crearted by a team of Computer Science and Computer Engineering students at FAU for a senior design project. The purpose of this project was to create a tool to collect and annotate real-time images of OHSA hazards for identification, correction, and training purposes. Our project was created to identify six types of OSHA hazards using Tensorflow's object detection. The classes that we trainined our neural network on were ladders, moving equipment (forklifts), rotating parts (gears and saw blades not covered), extension cords, chemical labels, and hot surfaces (steam flowing from a machine or pipe). The basis of this project consists of a mobile application, a AI engine, a database and a mobile application.

My contributions to this project included creating a custom dataset for our hazards, training about 10 different models for our neural network, creating our object detection code and figured out how to host it on AWS Lambda, and connected our mobile app to our AWS RDS database.

## Contibuters:

Samuel Hicks - Team Leader/Ai Engine Subsystem & Mobile App Subsystem

Roods Jacques - Mobile App Subsystem

Roshni Merugu - Database Subsystem

Laussanne Margaux Lenihan - Web App Subsystem

Oscar Aquino

## Mobile Application
We developed our mobile application with the Unity game engine for IOS and Android devices. It has a user registration page, sign in page, forget password page, mainmenu page and camera page. On the registration page, it asks a user to enter a username, email, password and confirm their password. Once the user submits their account information, their credentials are automaticaly saved in our database and they can login. After they sign in with the email and password they just created, they can either start taking images or view our website. Each image is automaticaly uploaded to our database and sent to our object detection code to be categorized.

## Web Application
Our web application serves as the main interacting point for users to see the images that they’ve taken. We developed our website using html, css and PhP and are hosting it on an Amazon Web Services service called AWS Elastic Compute Cloud(AWS EC2). The web application has a main login page and a home page that displays all of the dates that a user has taken images on. They can then click on a date and will be presetned with all of the images that were taken on that date along with the hazards, time taken and a delete button.

## AI Engine
For our AI Engine, we developed a system for detecting hazardous objects in the workplace. We used the Tensorflow API for our object detection and neural network training utilizing a Faster Region-Based Convolutional Neural Network. There are six classes that our system can detect as mentioned above in the description. For hosting our object detection program, we chose to use Amazon Web Services Lambda (AWS Lambda) and Amazon Web Services Simple Storage Service (AWS S3). The AWS lambda holds our object detection code and will trigger whenever an image is uploaded to the database table unclassified images. When our program is triggered, it will interact with AWS S3 which stores our trained neural network model used for object detection. Once the input image has been processed by the program, it is fitted with appropriate bounding boxes and labels that illustrate all of the hazards that were categorized in the image. This image is then sent to the database under the classified image table along with the unique image id, hazards that were found and the date and time that it was taken.
