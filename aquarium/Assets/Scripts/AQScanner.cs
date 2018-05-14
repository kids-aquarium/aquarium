////////////////////////////////////////////////////////////////////////
//
// This is a module for scan image by John Lee. 06 Jan 2018
// 
// This module reads a image from camera and do a perpective transform.
// Then find a QR code which is ID of a fish.
// Finally, it'll pass only texture of fish
//
// 5 May 2018, scanner_enhancement_001.
//
// 1. I added 'flip' function of fish image from camera because we don't know the angle of camera would be installed at hospital.
//    so, just call this function would flip fish image 180degree if x axis.
//    you can call this function inside ProcessFinalImage, don't forget you also should flip mask iamge as well.
//
//
// 2. 




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
public class AQScannerEvent : UnityEvent<Texture2D> {}



// This is custom made Texture2D class, the original plan was to use unity Texture2D
// However, I can't use this class in Thread. So, I just made own Testure2D class which is equevalant to Texture2D

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


    // [scanner_enhancement_001]. 5 May 2018. To just call function would flip fish image 180degree of x axis.
    // if you see fish image flipped then just call function

    public void flip()
    {
        int index1, index2;
        Color tempColor;

        for (int y = 0; y < myHeight / 2; y++)
        {
            for (int x = 0; x < myWidth; x++)
            {
                index1 = (y * width) + x;

                tempColor = pixels[index1];

                index2 = (((myHeight - 1) - y) * width) + x;

                pixels[index1] = pixels[index2];
                pixels[index2] = tempColor;

            }
        }
    }

    public void SetPixel(int x , int y, Color recvColor)
    {
        int index;

        index = (y * width) + x;

        pixels[index] = recvColor;
    }
  

    //[001] added this function to copy array by region, John

    public Color[] GetPixels(int x , int y , int width, int height)
    {
		int sum;
		int count;
		int index;
		int yMax, xMax;
        Color[] retColor;

		count = 0;

		yMax = (y + height);
		xMax = (x + width);

        sum = (width * height);
        
        retColor = new Color[sum];

		for (int i = y; i < yMax;i++)
		{
			for (int j = x; j < xMax;j++)
			{
				index = (i * myWidth) + j;

				retColor[count++] = pixels[index];
			}
		}

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

    private char FILE_SEP; // this is file name separator
    private SCANNER_MODE scannerMode;
    private bool isScannerReady;
    private bool isFishFileReady;
    private bool isScannerBusy;
    private bool isDebugMode;
    private int fishID;
    private int fishMaskCount;
    private int lastTime;
    private Rect cameraSize;
    private Rect []codeArea;
    private String maskImageName;
    private String cameraName;
    private string fishName;
    private string fishNameHeader;
    private WebCamTexture webCam;
    private Texture2D orgImage;
    private Texture2D finalFishTexture;
    private Texture2D fishIDTestTexture;
    private AQTexture2D fishIDImage;
    private AQTexture2D interimImage;
    private AQTexture2D finalImage;
    private AQTexture2D[] fishMasks;
    private Texture2D maskImage;
    private BarcodeReader fishCodeReader;
    private Thread scannerThread;


    // public member property area //////////////////////////////////


    public AQScannerEvent fishReadyEvent;


    // private member method area ///////////////////////////////////

	private void setCodeAndCameraArea()
	{
		codeArea = new Rect[4];

		for (int i = 0; i < 4; i++)
        {
            codeArea[i].width = 500; // this is enough width to detect only QR code, if camera resolution is changed then you need to change this as well.
            codeArea[i].height = 400; // this is enough height to detect only QR code, if camera resolution is changed then you need to change this as well.
        }

		codeArea[0].x = 0; // First area to search QRCode
        codeArea[0].y = 0; 

		codeArea[1].x = (cameraSize.width - codeArea[0].width); // Second area to search QRCode
        codeArea[1].y = 0; 
        
		codeArea[2].x = (cameraSize.width - codeArea[0].width); // Third area to search QRCode
		codeArea[2].y = (cameraSize.height - codeArea[0].height); 

		codeArea[3].x = 0; // Forth area to search QRCode
		codeArea[3].y = (cameraSize.height - codeArea[0].height);    
	}

	private void createObjects()
	{
		fishMasks = new AQTexture2D[fishMaskCount];

        scannerThread = new Thread(procImage);

        fishCodeReader = new BarcodeReader();

        orgImage = new Texture2D((int)cameraSize.width, (int)cameraSize.height, TextureFormat.RGBA32, false);
        fishIDTestTexture = new Texture2D((int)codeArea[0].width, (int)codeArea[0].height, TextureFormat.RGBA32, false);

        fishIDImage = new AQTexture2D((int)codeArea[0].width, (int)codeArea[0].height);
        interimImage = new AQTexture2D((int)cameraSize.width, (int)cameraSize.height);
	}

    private void initDefaultData()
    {
        lastTime = 0;

        fishID = 0; // if scanner module couldn't found QR code then just use fish 0 as default.

        fishMaskCount = 6;

		isFishFileReady = false;

        interimImage = null;
        webCam = null;
        scannerThread = null;
        isScannerBusy = false;
        isScannerReady = true;
		isDebugMode = false; // use this flag to turn on/off debug mode

        scannerMode = SCANNER_MODE.SIMULATION;

		FILE_SEP = '_';

        maskImageName = "fishmask"; // for testing

        fishName = "00000";
        fishNameHeader = "fish_scanned" + FILE_SEP;

		cameraSize.Set(0, 0, 1920, 1080);

		setCodeAndCameraArea();
     
		createObjects();
       
        loadMaskImage();

        initCamera();
    }

    private void loadMaskImage()
    {
        String tempImageName;
        Texture2D tempMaskImage;

        for (int i = 0; i < fishMaskCount; i++)
        {
            tempImageName = maskImageName + FILE_SEP+ String.Format("{0:00}", i);

            tempMaskImage = new Texture2D((int)cameraSize.width, (int)cameraSize.height, TextureFormat.RGBA32, false);

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

            webCam = new WebCamTexture(cameraName, (int)cameraSize.width, (int)cameraSize.height, 1);

            webCam.deviceName = devices[camIndex].name; // because webcam is always 0

            webCam.Play();

            isScannerReady = true;
            scannerMode = SCANNER_MODE.ACTIVE;

            Debug.Log("camera resolution : " + webCam.width + " , " + webCam.height);
            Debug.Log("done : " + MethodBase.GetCurrentMethod().Name);
        }
        else
        {
            isScannerReady = false;
            scannerMode = SCANNER_MODE.SIMULATION;

            Debug.Log("the scanner mode is : " + scannerMode + " " + MethodBase.GetCurrentMethod().Name);
            Debug.Log("failed : " + MethodBase.GetCurrentMethod().Name);
        }
    }

    /*// just keep this now for testing
     
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
        tempTexture   = new Texture2D((int)codeArea.width, (int)codeSize.y);
        fishIDTexture = new Texture2D((int)codeArea.width, (int)codeSize.y);


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
            for (int x = scale; x < codeArea.width; x++)
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

    private void SharpenCameraImage2(int areaID)
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

                if (tempGray > 0.5f) //[001] image enhancement
                {
                    tempGray = 1.0f;
                }

                colorGray.r = tempGray;
                colorGray.g = tempGray;
                colorGray.b = tempGray;
                colorGray.a = 1.0f;

                fishIDImage.SetPixel(x, y, colorGray);
            }
        }

		for (int x = scale; x < codeArea[areaID].width - scale; x++)
        {
			for (int y = scale; y < codeArea[areaID].height - scale;y++)
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

		Debug.Log("Done : " + MethodBase.GetCurrentMethod().Name + " Area ID = " + areaID);
    }

	private void optainFishIDImage(int areaID)
	{      
		fishIDImage.SetPixels(interimImage.GetPixels((int)codeArea[areaID].x, (int)codeArea[areaID].y,
                                                               (int)codeArea[areaID].width, (int)codeArea[areaID].height));
	}

    private void ObtainCameraImage()
    {
        orgImage.SetPixels32(webCam.GetPixels32());
        orgImage.Apply();

        interimImage.SetPixels(orgImage.GetPixels(0, 0, orgImage.width, orgImage.height));

		optainFishIDImage(0); // always from area id 0
        
        Debug.Log("done : " + MethodBase.GetCurrentMethod().Name + "camera resolution : " + webCam.width + " , " + webCam.height);
    }

    private void OptainTestImage()
    {
        orgImage = Resources.Load("defaultFishImage") as Texture2D;

		interimImage.SetPixels(orgImage.GetPixels(0, 0, orgImage.width, orgImage.height));

		optainFishIDImage(0); // always from area id 0
       
        Debug.Log("done : " + MethodBase.GetCurrentMethod().Name);
    }

    private bool DetectQRCode()
    {
        bool fishIDFound;
        String[] tempString;
        ZXing.Result result;

        fishID = 0; // just for init

        fishIDFound = false;

		fishCodeReader.AutoRotate = true;
        fishCodeReader.Options.TryHarder = true;
                      
        result = fishCodeReader.Decode(fishIDImage.GetPixels32(), fishIDImage.width, fishIDImage.height);

        if (result != null)
        {
            maskImageName = result.Text;

            tempString = result.Text.Split(FILE_SEP);

            if (tempString.Length == 3)
            {
                fishIDFound = true;

                fishID = Convert.ToInt32(tempString[2]);

                Debug.Log("Fish ID detected " + fishID);
            }
        }
     

        if (fishIDFound == false)
        {
            Debug.Log("Can't detect ID, this function will return default fish id which is 0");
        }

		return fishIDFound;
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

        //interimImage.reverse();

        interimImage.flip(); // [001] flip 180degree by John. 5 May 2018

        fishMasks[fishID].flip(); // [001] don't forget you need flip mask image as well. by John. 5 May 2018

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

        finalWidth = (maxX - minX);
        finalHeight = (maxY - minY);

        finalImage = new AQTexture2D(finalWidth, finalHeight);

        for (int y = minY; y < maxY; y++)
        {
            for (int x = minX; x < maxX; x++)
            {
                finalImage.SetPixel((x - minX), (y - minY), interimImage.GetPixel(x, y));
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
		int index;
        
		index = 0;

		while(true && scannerMode == SCANNER_MODE.ACTIVE)
		{
			Debug.Log("Scanning for area ID = " + index);

			SharpenCameraImage2(index);

			if (DetectQRCode() == true)
			{
				break;
			}

			Debug.Log("Can't find QR code, it will be scanned for area ID = " + index);

			index++;

			if (index <= 3)
			{
				optainFishIDImage(index);
			}
			else
			{
				break;
			}
		} 
        
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

    private void sendFinalImage()
    {
        Byte[] bytes;
        Texture2D tempTexture;

        isFishFileReady = false;
        finalFishTexture = new Texture2D(finalImage.width, finalImage.height, TextureFormat.RGBA32, false);

        finalFishTexture.SetPixels(finalImage.GetPixels());

        finalFishTexture.Apply();

        fishReadyEvent.Invoke(finalFishTexture);

        // [001] we need to save the raw image from camera when we install this system for the first time.
        // because there is only way to make a fish mask with raw camera image.
        // when you click 's' button with isDebugMode = true then it will save raw camera image then use this image to create a mask.
       
        if (isDebugMode == true && scannerMode == SCANNER_MODE.ACTIVE)
        {
            fishIDTestTexture.SetPixels32(fishIDImage.GetPixels32());

            fishIDTestTexture.Apply();

            tempTexture = new Texture2D(webCam.width, webCam.height, TextureFormat.RGBA32, false);

            tempTexture.SetPixels(webCam.GetPixels());

            tempTexture.Apply();

            bytes = tempTexture.EncodeToPNG();

            fishName = fishNameHeader + String.Format("{0:00}", fishID);

            System.IO.File.WriteAllBytes(Application.dataPath + "/resources/" + fishName + ".png", bytes);

            Debug.Log("------ Raw camera image saved.-------------");
        }


        Debug.Log("------ Scanning Done -------------");
    }

    private void OnGUI()
    {

        if (isDebugMode == true && scannerMode == SCANNER_MODE.ACTIVE)
        {
            if (finalFishTexture != null)
            {
				GUI.DrawTexture(new Rect(fishIDTestTexture.width+100, 0, finalFishTexture.width, finalFishTexture.height), finalFishTexture, ScaleMode.ScaleAndCrop, true);
            }
            else
            {
                GUI.DrawTexture(new Rect(0, 0, 1920, 1080), webCam, ScaleMode.ScaleAndCrop, true);
            }

            if (fishIDTestTexture != null)
            {
                GUI.DrawTexture(new Rect(0, 0, fishIDTestTexture.width, fishIDTestTexture.height), fishIDTestTexture, ScaleMode.ScaleAndCrop, true);
            }
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

        if (Input.GetKeyUp("s") == true)
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

        if (Input.GetKeyUp("d") == true) //[001] to press 'd' key it to turn on debug mode. in debug mode you can see the camera and extra image.
        {
            isDebugMode = !isDebugMode;
        }

        if (isFishFileReady == true)
        {
            sendFinalImage();
        }

    }
}
