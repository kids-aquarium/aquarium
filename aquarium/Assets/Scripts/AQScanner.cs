////////////////////////////////////////////////////////////////////////
//
// This is a module for scan image by John Lee. 06 Jan 2018
// 
// This module reads a image from camera and do a perpective transform.
// Then find a QR code which is ID of a fish.
// Finally, it'll save only texture are of fish
//
//
// private member property area /////////////////////////////////

// public member property area //////////////////////////////////

// private member method area ///////////////////////////////////

// public member method area ////////////////////////////////////

// public event handling area //////////////////////////////////
//
////////////////////////////////////////////////////////////////////////


using System;
using System.Reflection;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using ZXing;

enum SCANNER_MODE { SIMULATION, ACTIVE };


[System.Serializable]
public class AQScannerEvent : UnityEvent<string> {}


public class AQScanner : MonoBehaviour
{


    // private member property area /////////////////////////////////

    private SCANNER_MODE scannerMode;
    private bool isScannerReady;
    private int fishID;
    private Vector2 cameraSize;
    private String maskImageName;
    private String cameraName;
    private string fishName;
    private string fishNameHeader;
    private WebCamTexture webCam;
    private Texture2D finalImage;
    private Texture2D orgImage;
    private Texture2D maskImage;
    private Texture2D fishIDTexture;
    private BarcodeReader fishCodeReader;
    private Thread threadScanner;




    // public member property area //////////////////////////////////


    public AQScannerEvent fishReadyEvent;


    // private member method area ///////////////////////////////////

    private void initDefaultData()
    {
        fishID = 0; // for testing

        finalImage = null;
        webCam = null;
        isScannerReady = true;

        scannerMode = SCANNER_MODE.SIMULATION;

        maskImageName = "MaskForTesting"; // for testing

        fishName = "00000";
        fishNameHeader = "fish_";

        cameraSize.Set(1920, 1080);

        fishCodeReader = new BarcodeReader();

        orgImage = new Texture2D((int)cameraSize.x, (int)cameraSize.y, TextureFormat.RGBA32, false);
        maskImage = new Texture2D((int)cameraSize.x, (int)cameraSize.y, TextureFormat.RGBA32, false);

        initCamera();
    }

    private void initCamera()
    {
        WebCamDevice[] devices;

        devices = WebCamTexture.devices;

        for (int i = 0; i < devices.Length; i++)
        {
            Debug.Log(devices[i].name);
        }

        if (devices.Length > 1)
        {
            cameraName = devices[1].name;
        
            webCam = new WebCamTexture(cameraName , (int)cameraSize.x, (int)cameraSize.y, 1);

            webCam.deviceName = devices[1].name; // because webcam is always 0

            webCam.Play();

            isScannerReady = true;
            scannerMode = SCANNER_MODE.ACTIVE;

            Debug.Log("camera resolution : " + webCam.width + " , " + webCam.height);
            Debug.Log("done : " + MethodBase.GetCurrentMethod().Name);
        }else
        {
            isScannerReady = false;
            scannerMode = SCANNER_MODE.SIMULATION;

            Debug.Log("the scanner mode is : "+scannerMode+" "+ MethodBase.GetCurrentMethod().Name);
            Debug.Log("failed : " + MethodBase.GetCurrentMethod().Name);
        }
    }

    private void SharpenCameraImage()
    {
        Color[] fishIDPixels;
        Color colorGray, finalColor, tempColor;
        Vector2 codeSize;
        Texture2D tempTexture;
        int matSize;
        int scale;
        float[,] filter;
        float offSet, factor, tempGray;

        filter = new float[,]
                {
                    {-1, -1, -1, -1, -1},
                    {-1,  2,  2,  2, -1},
                    {-1,  2, 16,  2, -1},
                    {-1,  2,  2,  2, -1},
                    {-1, -1, -1, -1, -1}
                };


        filter = new float[,] {
                    {-1, -1, -1, -1, -1},
                    {-1,  2,  2,  2, -1},
                    {-1,  2, 16,  2, -1},
                    {-1,  2,  2,  2, -1},
                    {-1, -1, -1, -1, -1}
                }; 

        factor = 1;
        offSet = -2;

        matSize = 5;

        scale = (matSize / 2);

        codeSize.x = 300;
        codeSize.y = 250;

        colorGray     = new Color32();
        finalColor    = new Color32();
        tempTexture   = new Texture2D((int)codeSize.x, (int)codeSize.y);
        fishIDTexture = new Texture2D((int)codeSize.x, (int)codeSize.y);

        fishIDPixels = orgImage.GetPixels(300, 100, (int)codeSize.x, (int)codeSize.y);

        tempTexture.SetPixels(fishIDPixels);

        tempTexture.Apply();

        for (int y = 0; y < fishIDTexture.height;y++)
        {
            for (int x = 0; x < fishIDTexture.width; x++)
            {
                tempColor = tempTexture.GetPixel(x, y);

                tempGray = (tempColor.r + tempColor.g + tempColor.b) / 3;
                colorGray.r = tempGray;
                colorGray.g = tempGray;
                colorGray.b = tempGray;
                colorGray.a = 1.0f;

                tempTexture.SetPixel(x,y,colorGray);
            }
        }

        tempTexture.Apply();

        for (int y = scale; y < codeSize.y; y++)
        {
            for (int x = scale; x < codeSize.x; x++)
            {
                finalColor.r = finalColor.g = finalColor.b = 0;

                for (int filterY = 0; filterY < matSize; filterY++)
                {
                    for (int filterX = 0; filterX < matSize; filterX++)
                    {
                        tempColor = tempTexture.GetPixel(x + filterX - scale, y + filterY - scale);

                        finalColor.r += filter[filterX, filterY] * tempColor.r;
                        finalColor.g += filter[filterX, filterY] * tempColor.g;
                        finalColor.b += filter[filterX, filterY] * tempColor.b;
                    }
                }

                finalColor.r = Math.Min(Math.Max((finalColor.r / factor) + offSet, 0), 1);
                finalColor.g = Math.Min(Math.Max((finalColor.g / factor) + offSet, 0), 1);
                finalColor.b = Math.Min(Math.Max((finalColor.b / factor) + offSet, 0), 1);
                finalColor.a = 1.0f;

                fishIDTexture.SetPixel(x, y, finalColor);
            }
        }

        fishIDTexture.Apply();
    }

    private void SharpenCameraImage2()
    {
        Color[] fishIDPixels;
        Color colorGray, finalColor, tempColor;
        Vector2 codeSize;
        Texture2D tempTexture;
        int matSize;
        int scale;
        float[,] filter;
        float offSet, factor, tempGray;
        float strength;

  
        filter = new float[,] {
                    {-1, -1, -1, -1, -1},
                    {-1,  2,  2,  2, -1},
                    {-1,  2, 16,  2, -1},
                    {-1,  2,  2,  2, -1},
                    {-1, -1, -1, -1, -1}
                };

        strength = 2.0f;
        offSet = (1.0f - strength);
        factor = (strength / 16.0f);

        matSize = 5;

        scale = (matSize / 2);

        codeSize.x = 300;
        codeSize.y = 250;

        tempColor = new Color32();
        colorGray = new Color32();
        finalColor = new Color32();
        tempTexture = new Texture2D((int)codeSize.x, (int)codeSize.y);
        fishIDTexture = new Texture2D((int)codeSize.x, (int)codeSize.y);

        fishIDPixels = orgImage.GetPixels(300, 100, (int)codeSize.x, (int)codeSize.y);

        tempTexture.SetPixels(fishIDPixels);

        tempTexture.Apply();

        for (int y = 0; y < fishIDTexture.height; y++)
        {
            for (int x = 0; x < fishIDTexture.width; x++)
            {
                tempColor = tempTexture.GetPixel(x, y);

                tempGray = (tempColor.r + tempColor.g + tempColor.b) / 3;
                colorGray.r = tempGray;
                colorGray.g = tempGray;
                colorGray.b = tempGray;
                colorGray.a = 1.0f;

                tempTexture.SetPixel(x, y, colorGray);
            }
        }

        tempTexture.Apply();

        for (int x = scale; x < codeSize.x - scale; x++)
        {
            for (int y = scale; y < codeSize.y - scale; y++)
            {
                finalColor.r = finalColor.g = finalColor.b = 0;

                for (int filterY = 0; filterY < matSize; filterY++)
                {
                    for (int filterX = 0; filterX < matSize; filterX++)
                    {
                        tempColor = tempTexture.GetPixel(x + filterX - scale, y + filterY - scale);

                        finalColor.r += filter[filterX, filterY] * tempColor.r;
                        finalColor.g += filter[filterX, filterY] * tempColor.g;
                        finalColor.b += filter[filterX, filterY] * tempColor.b;
                    }
                }

                finalColor.r = Math.Min(Math.Max((factor * finalColor.r + (offSet * tempColor.r)), 0), 1);
                finalColor.g = Math.Min(Math.Max((factor * finalColor.g + (offSet * tempColor.g)), 0), 1);
                finalColor.b = Math.Min(Math.Max((factor * finalColor.b + (offSet * tempColor.b)), 0), 1);

                finalColor.a = 1.0f;

                fishIDTexture.SetPixel(x, y, finalColor);
            }
        }

        fishIDTexture.Apply();
    }


    private void ObtainCameraImage()
    {
        webCam.Play();

        orgImage.SetPixels32(webCam.GetPixels32());
        orgImage.Apply();
   
        webCam.Pause(); // for better performance

        Debug.Log("done : " + MethodBase.GetCurrentMethod().Name + "camera resolution : " + webCam.width + " , " + webCam.height);
    }

    private void OptainTestImage()
    {
        orgImage = Resources.Load("FishForTesting02") as Texture2D;
        orgImage = Resources.Load("testHD_ORG") as Texture2D;

        finalImage = new Texture2D(orgImage.width, orgImage.height, TextureFormat.RGBA32, false);

        Debug.Log("done : " + MethodBase.GetCurrentMethod().Name);
    }

    private void DetectQRCode()
    {
        ZXing.Result result;

        result = fishCodeReader.Decode(fishIDTexture.GetPixels32(), fishIDTexture.width, fishIDTexture.height);

        if (result != null)
        {
            maskImageName = result.Text;

            Debug.Log("ID detected " + maskImageName);
        }
        else
        {
            maskImageName = "MaskForTesting";

            Debug.Log(("Can't detect ID, this function will return default file name!"));
        }

        maskImageName = "MaskForTesting"; // for testing

        maskImage = Resources.Load(maskImageName) as Texture2D;
    }

    private void ProcessFinalImage()
    {
        Color colorOrgImage;
        int width, height;
        byte[] bytes;

        width  = orgImage.width;
        height = orgImage.height;

        finalImage = null;

        finalImage = new Texture2D(width , height, TextureFormat.RGBA32, false);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colorOrgImage   = orgImage.GetPixel(x, y);
                colorOrgImage.a = maskImage.GetPixel(x, y).r;

                finalImage.SetPixel(x, y, colorOrgImage);
            }
        }

        finalImage.Apply();

        if (fishReadyEvent != null)
        {
            bytes = finalImage.EncodeToPNG();

            fishName = fishNameHeader + String.Format("{0:00000}", fishID);

            System.IO.File.WriteAllBytes(Application.dataPath + "/resources/"+ fishName + ".png", bytes);

            fishReadyEvent.Invoke(fishName);

            fishID++;
        }

        Debug.Log("The final image is ready!");
        Debug.Log("Done :" + MethodBase.GetCurrentMethod().Name);
    }


    private Texture2D GetTexture()
    {
        return finalImage;
    }

    private Texture GetCamTexture()
    {
        return webCam;
    }

    private bool GoFishing()
    {
        bool success;

        success = true;

        try
        {
            if (scannerMode == SCANNER_MODE.SIMULATION)
            {
                OptainTestImage();
            }
            else
            {
                ObtainCameraImage();
            }

            SharpenCameraImage2();

            DetectQRCode();

            ProcessFinalImage();
        }
        catch (Exception e)
        {
            success = false;

            Debug.LogException(e, this);
        }

        return success;
    }




    // public event handling area //////////////////////////////////




    // public member method area ////////////////////////////////////

    void Start()
    {
        try
        {
            initDefaultData();

            Debug.Log("started : " + MethodBase.GetCurrentMethod().Name);
        }
        catch (Exception e)
        {
            Debug.LogException(e, this);
        }
    }
        
    ~AQScanner()
    {
        Debug.Log("destroyed : " + MethodBase.GetCurrentMethod().Name);
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            GoFishing();
        }


        /*
        if (webCam != null)
        {
            GUI.DrawTexture(new Rect(0, 0, 1920, 1024), webCam, ScaleMode.ScaleAndCrop, true);
        }*/

        /*
        if (finalImage != null)
        {
            GUI.DrawTexture(new Rect(0, 0, 1280, 960), finalImage, ScaleMode.ScaleAndCrop, true);
        }
        else
        {
            GUI.DrawTexture(new Rect(0, 0, 1280, 960), webCam, ScaleMode.ScaleAndCrop, true);
        }*/
    }

}
