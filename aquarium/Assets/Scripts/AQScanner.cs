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
using System.Collections;
using System.Reflection;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using ZXing;

enum SCANNER_MODE { SIMULATION, ACTIVE };

[System.Serializable]
public class AQScannerEvent : UnityEvent<string> {}

public class AQTexture2D
{

    // private member property area /////////////////////////////////

    private Color[] pixels;
    private int myWidth;
    private int myHeight;
    private int length;

    // public member property area //////////////////////////////////

    // private member method area ///////////////////////////////////

    // public member method area ////////////////////////////////////


    public AQTexture2D(int width, int height)
    {
        myWidth = width;
        myHeight = height;

        length = (myWidth * myHeight);

        pixels = new Color[myWidth * myHeight];
    }

    public int width
    {
        get
        {
            return myWidth;
        }
    }

    public int height
    {
        get
        {
            return myHeight;
        }
    }

    public void SetPixels(Color[] recvPixels)
    {
        Array.Copy(recvPixels,pixels,length);
    }

    public void reverse()
    {
        System.Array.Reverse(pixels);
    }

    public void SetPixel(int x , int y, Color recvColor)
    {
        int index;

        index = (y * width) + x;

        pixels[index] = recvColor;
    }
  
    public Color[] GetPixels(int width, int height)
    {
        Color[] retColor;

        retColor = null;


        return retColor;
    }

    public Color[] GetPixels()
    {
        return pixels;
    }

    public Color32[] GetPixels32()
    {
        int sum;
        Color32[] retColor;

        sum = (width * height);

        retColor = new Color32[sum];

        for (int i = 0; i < sum;i++)
        {
            retColor[i] = pixels[i];
        }

        return retColor;
    }

    public Color GetPixel(int x, int y)
    {
        int index;
        Color retColor;

        index = (y * width) + x;

        retColor = pixels[index];

        return retColor;
    }

    // public event handling area //////////////////////////////////






}


public class AQScanner : MonoBehaviour
{


    // private member property area /////////////////////////////////

    private SCANNER_MODE scannerMode;
    private bool isScannerReady;
    private bool isFishFileReady;
    private bool isScannerBusy;
    private int fishID;
    private int fishMaskCount;
    private int lastTime;
    private Vector2 cameraSize;
    private Vector2 codeSize;
    private String maskImageName;
    private String cameraName;
    private string fishName;
    private string fishNameHeader;
    private WebCamTexture webCam;
    private Texture2D orgImage;
    private Texture2D testQRCodeImage;
    private AQTexture2D fishIDImage;
    private AQTexture2D interimImage;
    private AQTexture2D finalImage;
    private AQTexture2D[] fishMasks;
    private Texture2D maskImage;
    private BarcodeReader fishCodeReader;
    private Thread scannerThread;


    private Color32[] testColor;




    // public member property area //////////////////////////////////


    public AQScannerEvent fishReadyEvent;


    // private member method area ///////////////////////////////////

    private void initDefaultData()
    {
        lastTime = 0;

        fishID = 0; // for testing

        fishMaskCount = 6;

        codeSize.x = 300;
        codeSize.y = 250;

        isFishFileReady = false;

        interimImage = null;
        webCam = null;
        scannerThread = null;
        isScannerBusy = false;
        isScannerReady = true;

        scannerMode = SCANNER_MODE.SIMULATION;

        maskImageName = "MaskForTesting"; // for testing

        fishName = "00000";
        fishNameHeader = "fish_";

        cameraSize.Set(1920, 1080);

        fishMasks = new AQTexture2D[fishMaskCount];

        scannerThread = new Thread(procImage);

        fishCodeReader = new BarcodeReader();

        orgImage = new Texture2D((int)cameraSize.x, (int)cameraSize.y, TextureFormat.RGBA32, false);

        fishIDImage   = new AQTexture2D((int)codeSize.x, (int)codeSize.y);
        interimImage  = new AQTexture2D((int)cameraSize.x, (int)cameraSize.y);

        loadMaskImage();

        initCamera();
    }

    private void loadMaskImage()
    {
        String tempImageName;
        Texture2D tempMaskImage;

        for (int i = 0; i < fishMaskCount; i++)
        {
            tempImageName = "fishMask" + String.Format("{0:00}", i);

            tempMaskImage = new Texture2D((int)cameraSize.x, (int)cameraSize.y, TextureFormat.RGBA32, false);

            tempMaskImage = Resources.Load(tempImageName) as Texture2D;

            fishMasks[i] = new AQTexture2D(tempMaskImage.width, tempMaskImage.height);

            fishMasks[i].SetPixels(tempMaskImage.GetPixels(0, 0, tempMaskImage.width, tempMaskImage.height));
        }
    }

    private void initCamera()
    {
        int camIndex;
        WebCamDevice[] devices;

        camIndex = -1;
        devices = WebCamTexture.devices;

        for (int i = 0; i < devices.Length; i++)
        {
            Debug.Log(devices[i].name);

            if (devices[i].name.Contains("C920") == true)
            {
                camIndex = i;

                break;
            }
        }

        if (camIndex >= 0)
        {
            cameraName = devices[camIndex].name;
        
            webCam = new WebCamTexture(cameraName , (int)cameraSize.x, (int)cameraSize.y, 1);

            webCam.deviceName = devices[camIndex].name; // because webcam is always 0

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

    /*
    private void SharpenCameraImage()
    {
        Color[] fishIDPixels;
        Color colorGray, finalColor, tempColor;
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

        colorGray     = new Color32();
        finalColor    = new Color32();
        tempTexture   = new Texture2D((int)codeSize.x, (int)codeSize.y);
        fishIDTexture = new Texture2D((int)codeSize.x, (int)codeSize.y);


       // tempTexture.SetPixels(fishIDPixels);

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
    }*/

    private void SharpenCameraImage2()
    {
        Color colorGray, finalColor, tempColor;
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

        tempColor = new Color();
        colorGray = new Color();
        finalColor = new Color();
         
        for (int y = 0; y < fishIDImage.height; y++)
        {
            for (int x = 0; x < fishIDImage.width; x++)
            {
                tempColor = fishIDImage.GetPixel(x, y);

                tempGray = (tempColor.r + tempColor.g + tempColor.b) / 3;
                colorGray.r = tempGray;
                colorGray.g = tempGray;
                colorGray.b = tempGray;
                colorGray.a = 1.0f;

                fishIDImage.SetPixel(x, y, colorGray);
            }
        }

        for (int x = scale; x < codeSize.x - scale; x++)
        {
            for (int y = scale; y < codeSize.y - scale; y++)
            {
                finalColor.r = finalColor.g = finalColor.b = 0;

                for (int filterY = 0; filterY < matSize; filterY++)
                {
                    for (int filterX = 0; filterX < matSize; filterX++)
                    {
                        tempColor = fishIDImage.GetPixel(x + filterX - scale, y + filterY - scale);

                        finalColor.r += filter[filterX, filterY] * tempColor.r;
                        finalColor.g += filter[filterX, filterY] * tempColor.g;
                        finalColor.b += filter[filterX, filterY] * tempColor.b;
                    }
                }

                finalColor.r = Math.Min(Math.Max((factor * finalColor.r + (offSet * tempColor.r)), 0), 1);
                finalColor.g = Math.Min(Math.Max((factor * finalColor.g + (offSet * tempColor.g)), 0), 1);
                finalColor.b = Math.Min(Math.Max((factor * finalColor.b + (offSet * tempColor.b)), 0), 1);

                finalColor.a = 1.0f;

                fishIDImage.SetPixel(x, y, finalColor);
            }
        }

        Debug.Log("Done : " + MethodBase.GetCurrentMethod().Name);
    }

    private void ObtainCameraImage()
    {
        orgImage.SetPixels32(webCam.GetPixels32());
        orgImage.Apply();

        interimImage.SetPixels(orgImage.GetPixels(0, 0, orgImage.width, orgImage.height));

        fishIDImage.SetPixels(orgImage.GetPixels(300, 100, (int)codeSize.x, (int)codeSize.y));

        Debug.Log("done : " + MethodBase.GetCurrentMethod().Name + "camera resolution : " + webCam.width + " , " + webCam.height);
    }

    private void OptainTestImage()
    {
        orgImage = Resources.Load("defaultFishImage") as Texture2D;

        interimImage.SetPixels(orgImage.GetPixels(0, 0, orgImage.width, orgImage.height));

        fishIDImage.SetPixels(orgImage.GetPixels(300, 100, (int)codeSize.x, (int)codeSize.y));

        Debug.Log("done : " + MethodBase.GetCurrentMethod().Name);
    }

    private void DetectQRCode()
    {
        ZXing.Result result;

        result = fishCodeReader.Decode(fishIDImage.GetPixels32(), fishIDImage.width, fishIDImage.height);

        if (result != null)
        {
            maskImageName = result.Text;

            Debug.Log("ID detected " + maskImageName);

            fishID = 0; // just for test

            // here I need to find the index of fishID
        }
        else
        {
            fishID = 0;

            Debug.Log(("Can't detect ID, this function will return default file name!"));
        }
    }

    private void ProcessFinalImage()
    {
        Color colorOrgImage;
        int width, height;
        int minX, minY;
        int maxX, maxY;
        int finalWidth, finalHeight;

        minX = 1000;
        minY = 1000;

        maxX = -1000;
        maxY = -1000;

        width = interimImage.width;
        height = interimImage.height;

        interimImage.reverse();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colorOrgImage = interimImage.GetPixel(x, y);
                colorOrgImage.a = fishMasks[fishID].GetPixel(x, y).r;

                interimImage.SetPixel(x, y, colorOrgImage);

                if (colorOrgImage.a.Equals(0.0f) == false)
                {
                    if (x < minX)
                    {
                        minX = x;
                    }

                    if (y < minY)
                    {
                        minY = y;
                    }

                    if (x > maxX)
                    {
                        maxX = x;
                    }

                    if (y > maxY)
                    {
                        maxY = y;
                    }
                }
            }
        }

        finalImage = null;

        finalWidth  = (maxX - minX);
        finalHeight = (maxY - minY);

        finalImage = new AQTexture2D(finalWidth,finalHeight);

        for (int y = minY; y < maxY; y++)
        {
            for (int x = minX; x < maxX; x++)
            {
                finalImage.SetPixel((x-minX), (y-minY), interimImage.GetPixel(x, y));
            }
        }

        Debug.Log("The final image is ready!");
        Debug.Log("Done :" + MethodBase.GetCurrentMethod().Name);

        isFishFileReady = true;
    }

    private Texture GetCamTexture()
    {
        return webCam;
    }

    private void procImage()
    {
        SharpenCameraImage2();

        DetectQRCode();

        ProcessFinalImage();

        isScannerBusy = false;
    }

    private void GoFishing()
    {
        isScannerBusy = true;

        lastTime = Environment.TickCount;

        Debug.Log("------ Scanning Started ------------- ");

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

            scannerThread = null;

            scannerThread = new Thread(procImage);

            scannerThread.Start();
        }
        catch (Exception e)
        {
            Debug.LogException(e, this);
        }
    }

    private void checkSaveFinalImage()
    {
        int curTime;
        Byte[] bytes;
   
        isFishFileReady = false;

        testQRCodeImage = new Texture2D(finalImage.width, finalImage.height, TextureFormat.RGBA32, false);

        testQRCodeImage.SetPixels(finalImage.GetPixels());

        bytes = testQRCodeImage.EncodeToPNG();

        fishName = fishNameHeader + String.Format("{0:00000}", 100);

        System.IO.File.WriteAllBytes(Application.dataPath + "/resources/" + fishName + ".png", bytes);

        curTime = Environment.TickCount;

        Debug.Log("------ Scanning Done -------------");

        fishReadyEvent.Invoke(fishName);
    }

	private void OnGUI()
    {
        if (testQRCodeImage != null)
        {
            ;//GUI.DrawTexture(new Rect(0, 0, testQRCodeImage.width, testQRCodeImage.height) , testQRCodeImage, ScaleMode.ScaleAndCrop, true);
        }
        else
        {
            ;//GUI.DrawTexture(new Rect(0, 0, 1280, 960), webCam, ScaleMode.ScaleAndCrop, true);
        }
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
        
        if (Input.GetMouseButtonUp(0) == true)
        {
            if (isScannerBusy == false)
            {
                GoFishing();
            }
            else
            {
                Debug.Log("scanner is busy, try later!" + MethodBase.GetCurrentMethod().Name);
            }
        }
  
        if (isFishFileReady == true)
        {
            checkSaveFinalImage();
        }

    }
}
