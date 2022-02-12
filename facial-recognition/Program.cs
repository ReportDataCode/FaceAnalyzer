using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

// From your Face subscription in the Azure portal, get your subscription key and endpoint.
const string SUBSCRIPTION_KEY = "<place-your-key-here>";
const string SUBSCRIPTION_ENDPOINT = "<place-your-endpoint->";

const string IMAGE_BASE_URL = "https://csdx.blob.core.windows.net/resources/Face/Images/";
const string RECOGNITION_MODEL_4 = RecognitionModel.Recognition04;

/* AUTHENTICATE
   Uses subscription key and region to create a client.
*/

static IFaceClient authenticateClient(string endpoint, string key)
{
    return new FaceClient(new ApiKeyServiceClientCredentials(key)) {Endpoint = endpoint};
}

static async Task DetectFaceExtractTask(IFaceClient client, string url, string recognitionModel)
{
    Console.WriteLine("===================DETECT FACES=================");
    Console.WriteLine();

    // Create a list of images.
    List<string> imageFileNamesList = new List<string>()
    {
        "detection1.jpg",
        "detection5.jpg",
        "detection6.jpg",
    };

    foreach (string imageFileName in imageFileNamesList)
    {
        // Detect faces with all attributes from image url.
        IList<DetectedFace> detectedFaces = await client.Face.DetectWithUrlAsync($"{url}{imageFileName}",
            returnFaceAttributes: new List<FaceAttributeType>
            {
                FaceAttributeType.Accessories, FaceAttributeType.Age,
                FaceAttributeType.Blur, FaceAttributeType.Emotion, FaceAttributeType.Exposure,
                FaceAttributeType.FacialHair,
                FaceAttributeType.Gender, FaceAttributeType.Glasses, FaceAttributeType.Hair, FaceAttributeType.HeadPose,
                FaceAttributeType.Makeup, FaceAttributeType.Noise, FaceAttributeType.Occlusion, FaceAttributeType.Smile,
                FaceAttributeType.Smile, FaceAttributeType.QualityForRecognition
            },
            // We specify detection model 1 because we are retrieving attributes.
            detectionModel: DetectionModel.Detection01,
            recognitionModel: recognitionModel);

        // Parse and print all attributes of each detected face.
        foreach (var face in detectedFaces)
        {
            Console.WriteLine($"Face attributes for {imageFileName}:");

            // Get bounding box of the faces
            Console.WriteLine(
                $"Rectangle(Left/Top/Width/Height) : {face.FaceRectangle.Left} {face.FaceRectangle.Top} {face.FaceRectangle.Width} {face.FaceRectangle.Height}");

            // Get accessories of the faces
            List<Accessory> accessoriesList = (List<Accessory>) face.FaceAttributes.Accessories;
            int count = face.FaceAttributes.Accessories.Count;
            string accessory;
            string[] accessoryArray = new string[count];
            if (count == 0)
            {
                accessory = "NoAccessories";
            }
            else
            {
                for (int i = 0; i < count; ++i)
                {
                    accessoryArray[i] = accessoriesList[i].Type.ToString();
                }

                accessory = string.Join(",", accessoryArray);
            }

            Console.WriteLine($"Accessories : {accessory}");

            // Get face other attributes
            Console.WriteLine($"Age : {face.FaceAttributes.Age}");
            Console.WriteLine($"Blur : {face.FaceAttributes.Blur.BlurLevel}");

            // Get emotion on the face
            string emotionType = string.Empty;
            double emotionValue = 0.0;
            Emotion emotion = face.FaceAttributes.Emotion;
            if (emotion.Anger > emotionValue)
            {
                emotionValue = emotion.Anger;
                emotionType = "Anger";
            }

            if (emotion.Contempt > emotionValue)
            {
                emotionValue = emotion.Contempt;
                emotionType = "Contempt";
            }

            if (emotion.Disgust > emotionValue)
            {
                emotionValue = emotion.Disgust;
                emotionType = "Disgust";
            }

            if (emotion.Fear > emotionValue)
            {
                emotionValue = emotion.Fear;
                emotionType = "Fear";
            }

            if (emotion.Happiness > emotionValue)
            {
                emotionValue = emotion.Happiness;
                emotionType = "Happiness";
            }

            if (emotion.Neutral > emotionValue)
            {
                emotionValue = emotion.Neutral;
                emotionType = "Neutral";
            }

            if (emotion.Sadness > emotionValue)
            {
                emotionValue = emotion.Sadness;
                emotionType = "Sadness";
            }

            if (emotion.Surprise > emotionValue)
            {
                emotionType = "Surprise";
            }

            Console.WriteLine($"Emotion : {emotionType}");

            // Get more face attributes
            Console.WriteLine($"Exposure : {face.FaceAttributes.Exposure.ExposureLevel}");
            Console.WriteLine(
                $"FacialHair : {(face.FaceAttributes.FacialHair.Moustache + face.FaceAttributes.FacialHair.Beard + face.FaceAttributes.FacialHair.Sideburns > 0 ? "Yes" : "No")}");
            Console.WriteLine($"Gender : {face.FaceAttributes.Gender}");
            Console.WriteLine($"Glasses : {face.FaceAttributes.Glasses}");

            // Get hair color
            Hair hair = face.FaceAttributes.Hair;
            string? color = null;
            if (hair.HairColor.Count == 0)
            {
                if (hair.Invisible)
                {
                    color = "Invisible";
                }
                else
                {
                    color = "Bald";
                }
            }

            HairColorType returnColor = HairColorType.Unknown;
            double maxConfidence = 0.0f;
            foreach (HairColor hairColor in hair.HairColor)
            {
                if (hairColor.Confidence <= maxConfidence)
                {
                    continue;
                }

                maxConfidence = hairColor.Confidence;
                returnColor = hairColor.Color;
                color = returnColor.ToString();
            }

            Console.WriteLine($"Hair : {color}");

            // Get more attributes
            Console.WriteLine(
                $"HeadPose : {string.Format("Pitch: {0}, Roll: {1}, Yaw: {2}", Math.Round(face.FaceAttributes.HeadPose.Pitch, 2), Math.Round(face.FaceAttributes.HeadPose.Roll, 2), Math.Round(face.FaceAttributes.HeadPose.Yaw, 2))}");
            Console.WriteLine(
                $"Makeup : {string.Format("{0}", (face.FaceAttributes.Makeup.EyeMakeup || face.FaceAttributes.Makeup.LipMakeup) ? "Yes" : "No")}");
            Console.WriteLine($"Noise : {face.FaceAttributes.Noise.NoiseLevel}");
            Console.WriteLine(
                $"Occlusion : {string.Format("EyeOccluded: {0}", face.FaceAttributes.Occlusion.EyeOccluded ? "Yes" : "No")} " +
                $" {string.Format("ForeheadOccluded: {0}", face.FaceAttributes.Occlusion.ForeheadOccluded ? "Yes" : "No")}   {string.Format("MouthOccluded: {0}", face.FaceAttributes.Occlusion.MouthOccluded ? "Yes" : "No")}");
            Console.WriteLine($"Smile : {face.FaceAttributes.Smile}");

            // Get quality for recognition attribute
            Console.WriteLine($"QualityForRecognition : {face.FaceAttributes.QualityForRecognition}");
            Console.WriteLine();
        }
    }
}


// Authenticate.
IFaceClient client = authenticateClient(SUBSCRIPTION_ENDPOINT, SUBSCRIPTION_KEY);

// Detect - get features from faces.
DetectFaceExtractTask(client, IMAGE_BASE_URL, RECOGNITION_MODEL_4).Wait();