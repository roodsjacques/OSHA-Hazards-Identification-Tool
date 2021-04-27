#Custom Object Detection Code for Tensorflow API
#Original Creators: Tensorflow and customized by Youtuber Sentdex
#Edits Author: Samuel Hicks (Edits are marked accordingly throughout the program)

import json
import boto3
import numpy as np
import os
import six.moves.urllib as urllib
import sys
import tarfile
import tensorflow.compat.v1 as tf
#tf.disable_v2_behavior()
import zipfile
import datetime
import pymysql
import logging

from collections import defaultdict
import io
from PIL import Image

import label_map_util
import visualization_utils as vis_util

##################################
#### Begining of my code edit ####
##################################

#rds settings
rds_host  = "RDS_HOSTNAME"
name = "RDS_DB_NAME"
password = "RDS_PASSWORD"
db_name = "RDS_USERNAME"
port = RDS_PORT

logger = logging.getLogger()
logger.setLevel(logging.INFO)

try:
    #Create a connection to the AWS RDS database with the required credentials
    conn = pymysql.connect(host=rds_host, user=name, passwd=password, db=db_name, connect_timeout=5)
except pymysql.MySQLError as e:
    logger.error("ERROR: Unexpected error: Could not connect to MySQL instance.")
    logger.error(e)
    sys.exit()

logger.info("SUCCESS: Connection to RDS MySQL instance succeeded")

#Create a connection to an S3 resource
s3 = boto3.resource('s3')

#Number of classes that were trained with in the model
NUM_CLASSES = 6  

def lambda_handler(event, context):
    #Get the key from the lambda event that triggered the function
    imgkey = event.get("key1")
    
    #Get the s3 bucket name that is storing the trained model
    bucket_name = "S3_BUCKET_NAME"

    #Path in s3 to frozen detection graph. This is the actual model that is used for the object detection.
    PATH_TO_CKPT_obj = s3.Object(bucket_name, "PATH_TO_S3_WITH_MODEL_GRAPH")
    serialized_graph = PATH_TO_CKPT_obj.get()['Body'].read()
    
    #Get the proper lables for objection detection
    categories = [{'id': 1, 'name': 'ladder'}, {'id': 2, 'name': 'rotating_parts'}, {'id': 3, 'name': 'moving_equipment'}, {'id': 4, 'name': 'chemicals'}, {'id': 5, 'name': 'extension_cords'}, {'id': 6, 'name': 'hot_surfaces'}]
    category_index = label_map_util.create_category_index(categories)
    
    #Create a cursor context manager for running SQL queries
    with conn.cursor() as cur:
        #Get the unclassified image data atached to imageid that was sent via the mobile app
        cur.execute("SELECT uimg FROM image_unclass WHERE uimgID = %s", imgkey)
        unclass_img_hexdata = cur.fetchone()
        unclass_img_hexdata = unclass_img_hexdata[0]
        
        #Get the userid attached to the unclassified image
        cur.execute("SELECT user_id FROM user_image_unclass WHERE uimgID = %s", imgkey)
        userid = cur.fetchone()
        userid = userid[0]
        
        conn.commit()
    conn.commit()

    #############################
    #### End of my code edit ####
    #############################

    detection_graph = tf.Graph()
    with detection_graph.as_default():
        od_graph_def = tf.GraphDef()
        od_graph_def.ParseFromString(serialized_graph)
        tf.import_graph_def(od_graph_def, name='')


    with detection_graph.as_default():
        with tf.Session(graph=detection_graph) as sess:
            ##################################
            #### Begining of my code edit ####
            ##################################

            #Convert the binary data retrieved from the database to a readable format and open the file with Pillow
            unclass_img_byte_stream = io.BytesIO(unclass_img_hexdata)
            image = Image.open(unclass_img_byte_stream)
            image.thumbnail((1280, 720))

            #############################
            #### End of my code edit ####
            #############################
            
            # the array based representation of the image will be used later in order to prepare the
            # result image with boxes and labels on it.
            image_np = load_image_into_numpy_array(image)

            # Expand dimensions since the model expects images to have shape: [1, None, None, 3]
            image_np_expanded = np.expand_dims(image_np, axis=0)
            image_tensor = detection_graph.get_tensor_by_name('image_tensor:0')
            # Each box represents a part of the image where a particular object was detected.
            boxes = detection_graph.get_tensor_by_name('detection_boxes:0')
            # Each score represent how level of confidence for each of the objects.
            # Score is shown on the result image, together with the class label.
            scores = detection_graph.get_tensor_by_name('detection_scores:0')
            classes = detection_graph.get_tensor_by_name('detection_classes:0')
            num_detections = detection_graph.get_tensor_by_name('num_detections:0')
            # Actual detection.
            (boxes, scores, classes, num_detections) = sess.run(
                [boxes, scores, classes, num_detections],
                feed_dict={image_tensor: image_np_expanded})
        
            # Visualization of the results of a detection.
            vis_util.visualize_boxes_and_labels_on_image_array(
                image_np,
                np.squeeze(boxes),
                np.squeeze(classes).astype(np.int32),
                np.squeeze(scores),
                category_index,
                use_normalized_coordinates=True,
                line_thickness=5)

            ##################################
            #### Begining of my code edit ####
            ##################################

            #Convert numpy array back into an image
            new_img = Image.fromarray(image_np) 
            new_img.thumbnail((1280, 720))
    
            #Convert the image to the required binary format for saving to the database
            classified_img_bytes = io.BytesIO()
            new_img.save(classified_img_bytes, format='jpeg')
            classified_img_hex_data = classified_img_bytes.getvalue()

            #Get the classes of the detected objects in the image so they can be stored in the database with the image
            classes_list = [category_index.get(value) for index,value in enumerate(classes[0]) if scores[0,index] > 0.5]
            value_of_classes_list = [class_dict['name'] for class_dict in classes_list]

            if value_of_classes_list == []:
                value_of_classes = 'None'
            else:
                value_of_classes_list = list(dict.fromkeys(value_of_classes_list))
                value_of_classes = ', '.join(value_of_classes_list)
    
            #Create a cursor context manager for running SQL queries
            with conn.cursor() as cur:
                #Insert classified image and corresponding attributes into database
                cur.execute("INSERT INTO image (imgID, img, imgDate, hazards, imgTime) VALUES (%s,%s,%s,%s,%s)", (imgkey, classified_img_hex_data, datetime.datetime.now().date(), value_of_classes, datetime.datetime.now().strftime("%H:%M:%S")))
                conn.commit()
                
                #Insert userid and imgid associated with classified image into database
                cur.execute("INSERT INTO user_image (user_id, imgID) VALUES (%s,%s)", (userid, imgkey))
                conn.commit()
            conn.commit()

            #############################
            #### End of my code edit ####
            #############################

def load_image_into_numpy_array(image):
    (im_width, im_height) = image.size
    return np.asarray(image).reshape((im_height, im_width, 3)).astype(np.uint8)