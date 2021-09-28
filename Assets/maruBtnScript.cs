using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Vuforia;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
using System.IO;

using OpenCvSharp;
using OpenCvSharp.Demo;

public class maruBtnScript : MonoBehaviour
{
    public GameObject ansBtnObj;
    public GameObject ansBtnObj2;
    public GameObject maruRen;
    public GameObject batuRen;

    public Text qText;
    public Image qBack;
    public GameObject maruObj;
    public GameObject batuObj;
    public GameObject nextBtn;
    public GameObject rtnBtn;
    public GameObject dispBtn;
    public GameObject testBtn;

    public TextAsset csvFile; // CSVファイル
    public List<string[]> csvDatas = new List<string[]>(); // CSVの中身を入れるリスト;
    public int qCnt = 0;
    private VBatuBtnScript vBatuScr;
    private VMaruBtnScript vMaruScr;

    public GameObject canvas;//UIが貼り付けられたcanvas
    public RawImage preview;//プレビュー領域
    UnityEngine.Rect capRect;//キャプチャ領域を保持
    Texture2D capTexture;//キャプチャ画像を保持
    Mat bgraMat, binMat;//OpenCVで使う画像を保持
    public GameObject original;//スタンプのテンプレオブジェクト(テクスチャ付きのquad)
    List<GameObject> stampList = new List<GameObject>();

    byte[,] colors = { { 255, 255, 255 }, { 18, 0, 230 }, { 0, 152, 243 },
                       { 0, 241, 255 }, { 31, 195, 143 }, { 68, 153, 0 },
                       { 150, 158, 0 }, { 233, 160, 0 }, { 183, 104, 0 },
                       { 136, 32, 29 }, { 131, 7, 146 }, { 127, 0, 228 },
                       { 79, 0, 229 }, { 0, 0, 0 } };
    int colorNo = 0;//何番目の色かを表す変数


    private void Awake()
    {
        
    }
    // Start is called before the first frame up;date
    void Start()
    {
        GetCsv();


        // GameObject("LacieBtn")の名前は適宜変更してください
        ansBtnObj = GameObject.Find("VBtnMaru");
        ansBtnObj.GetComponent<VirtualButtonBehaviour>().RegisterOnButtonPressed(OnButtonPressed);
        ansBtnObj.GetComponent<VirtualButtonBehaviour>().RegisterOnButtonReleased(OnButtonReleased);

        ansBtnObj2 = GameObject.Find("VBtnBatu");
        ansBtnObj2.GetComponent<VirtualButtonBehaviour>().RegisterOnButtonPressed(OnButtonPressed);
        ansBtnObj2.GetComponent<VirtualButtonBehaviour>().RegisterOnButtonReleased(OnButtonReleased);

        vMaruScr = maruRen.GetComponent<VMaruBtnScript>(); //unitychanの中にあるUnityChanScriptを取得して変数に格納する
        vBatuScr = batuRen.GetComponent<VBatuBtnScript>(); //unitychanの中にあるUnityChanScriptを取得して変数に格納する

        testBtn = GameObject.Find("NextBtn2");
        //int w = Screen.width;
        //int h = Screen.height;
        //int w = qBack.rectTransform.rect.width;
        //int h = qBack.rectTransform.rect.height;

        //原点から画面のたてて横の長さまでをキャプチャ領域とする
        //capRect = new UnityEngine.Rect(0, 0, w, h);
        //画面サイズの空画像を作成
        //capTexture = new Texture2D(w, h, TextureFormat.RGBA32, false);
        //capTextureをプレビュー領域に貼り付け
        //preview.material.mainTexture = capTexture;
        //AnchorStage.SetActive(false);

    }
    //スタンプを空間に置くための関数
    //public void PutObject() {//ボタン押下時
    //    Camera cam = Camera.main;//カメラの情報を取得
    //    Vector3 v1 = cam.ViewportToWorldPoint(new Vector3(0, 0, 0.6f));//画面左下のxy座標を3次元に変換(0.6m手前におくとする)
    //    Vector3 v2 = cam.ViewportToWorldPoint(new Vector3(1, 1, 0.6f));//画面右上のxy座標を3次元に変換
    //    Vector3 v3 = cam.ViewportToWorldPoint(new Vector3(0, 1, 0.6f));//画面左上のxy座標を3次元に変換

    //    float w = Vector3.Distance(v2, v3);//キャプチャ領域の実空間でのサイズを計算
    //    float h = Vector3.Distance(v1, v3);

    //    GameObject stamp = GameObject.Instantiate(original);
    //    stamp.transform.parent = cam.transform;
    //    stamp.transform.localPosition = new Vector3(0, 0, 0.6f);
    //    stamp.transform.localRotation = Quaternion.identity;
    //    stamp.transform.localScale = new Vector3(w, h, 1);//オブジェクトの生成とのカメラに対する位置・向き・サイズを指定

    //    Texture2D stampTexture = new Texture2D(capTexture.width, capTexture.height);//上記で作ったオブジェクトに貼るテクスチャを作成
    //    SetColor(stampTexture);//色を塗り、そのあとにテクスチャとして貼り付ける
    //    stamp.GetComponent<Renderer>().material.mainTexture = stampTexture;

    //    stamp.transform.parent = null; //スタンプの原点をカメラではなくワールドに変更
    //    stampList.Add(stamp);
    //    if (stampList.Count == 10) {
    //        DestroyImmediate(stampList[0].GetComponent<Renderer>().material.mainTexture);
    //        DestroyImmediate(stampList[0]);
    //        stampList.RemoveAt(0);
    //    }//生成したスタンプの記録と削除
    //    preview.enabled = false;
    // }

    public void startCV()//ボタン押下時
    {
        Debug.Log("(・∀・)ｲｲ!!");
        StartCoroutine(ImageProcessing());//コルーチンの実行
    }

 
    IEnumerator ImageProcessing() {
        canvas.SetActive(false);//canvas上のUIを一時的に消す
        //qText.enabled = false;
        //testBtn.SetActive(false);
        if (bgraMat != null) {
            bgraMat.Release();
        }
        if (binMat != null){
            binMat.Release();
        }//Mat用に確保したメモリを解放
        yield return new WaitForEndOfFrame();//フレーム終了を待つ
        CreateImages();//画像の生成
        SetColor(capTexture);//テクスチャに色をセット
        judgeCard();
        Debug.Log("(・∀・)");
        canvas.SetActive(true);//canvas上のUIを再表示
        Debug.Log("(・∀・)ｲ");
        preview.enabled = true;//プレビューを表示する
        //qText.enabled = true;
        //testBtn.SetActive(true);
    }

    void CreateImages() {

        capTexture.ReadPixels(capRect, 0, 0);//キャプチャ開始
        capTexture.Apply();//各画素の色をテクスチャに反映

        bgraMat = OpenCvSharp.Unity.TextureToMat(capTexture);//Texture2dをMatに変換
        binMat = bgraMat.CvtColor(ColorConversionCodes.BGRA2GRAY);//カラー画像をグレースケール(濃淡)画像に変換
        binMat = binMat.Threshold(100, 255, ThresholdTypes.Otsu); //大津の方法で二値化結果を白黒反転
        bgraMat = binMat.CvtColor(ColorConversionCodes.GRAY2BGRA);//後で色を変えられるようにカラー(BGR)に変換
     }


    public void ChangeColor() {//ボタン押下時
        colorNo++;
        colorNo %= colors.Length / 3;
        SetColor(capTexture);
    }

    void SetColor(Texture2D texture) {
        //Matが初期化されていない場合は何もしない
        if (bgraMat == null || binMat == null) {
            return;
        }
        unsafe {
            byte* bgraPtr = bgraMat.DataPointer;
            byte* binPtr = binMat.DataPointer;//各Matのピクセル情報の配列(ポインタ)を取得
            int pixelCount = binMat.Width * binMat.Height;//全ピクセル数を算出
            //各ピクセルを参照して黒画素なら色を塗る
            for (int i = 0; i < pixelCount; i++) {
                int bgraPos = i * 4;//白黒画像のi番目に相当するBGRAのデータの位置
                if (binPtr[i] == 255){//白かったら透過
                    bgraPtr[bgraPos + 3] = 0; //非透過
                }
                else {//黒かったら色を塗る
                    bgraPtr[bgraPos] = 255; //B
                    bgraPtr[bgraPos+1] = 0; //G
                    bgraPtr[bgraPos+2] = 0; //R
                    bgraPtr[bgraPos+3] = 255; //非透過
                }
            }
        }
        OpenCvSharp.Unity.MatToTexture(bgraMat, texture);//bgraMatが保持する色をテクスチャにセットする
    }




   


    public void judgeCard() {
        Debug.Log("(・∀・)ｲｲ!!!");
        //Load texture
        Mat image = OpenCvSharp.Unity.TextureToMat(capTexture);

        //Gray scale image
        Mat grayMat = new Mat();
        Cv2.CvtColor(image, grayMat, ColorConversionCodes.BGR2GRAY);

        Mat thresh = new Mat();
        Cv2.Threshold(grayMat, thresh, 127, 255, ThresholdTypes.BinaryInv);


        // Extract Contours
        Point[][] contours;
        HierarchyIndex[] hierarchy;
        Cv2.FindContours(thresh, out contours, out hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxNone, null);

        foreach (Point[] contour in contours)
        {
            double length = Cv2.ArcLength(contour, true);
            Point[] approx = Cv2.ApproxPolyDP(contour, length * 0.01, true);
            string shapeName = null;
            Scalar color = new Scalar();


            if (approx.Length == 2)
            {
                
                shapeName = "Cross";
                color = new Scalar(0, 255, 0);
            }
            if (approx.Length == 3)
            {
               
                shapeName = "Triangle";
                color = new Scalar(0, 255, 0);
            }
            else if (approx.Length == 4)
            {
                Debug.Log("い");
                OpenCvSharp.Rect rect = Cv2.BoundingRect(contour);
                if (rect.Width / rect.Height <= 0.1)
                {
                    Debug.Log("う");
                    shapeName = "Square";
                    color = new Scalar(0, 125, 255);
                }
                else
                {
                    Debug.Log("ああ");
                    shapeName = "Rectangle";
                    color = new Scalar(0, 0, 255);
                }
            }
            else if (approx.Length == 10)
            {
                Debug.Log("ばつ");
                shapeName = "Star";
                color = new Scalar(255, 255, 0);
            }
            else if (approx.Length >= 15)
            {
                Debug.Log("まる");
                shapeName = "Circle";
                color = new Scalar(0, 255, 255);

                if (csvDatas[1][qCnt] == "1")
                {

                   // maruObj.SetActive(true);
                    //Instantiate(maruObj, AnchorStage.transform.position, AnchorStage.transform.rotation);
                    break;
                }
                else
                {
                    //batuObj.SetActive(true);
                  
                    break;
                }
            }

            if (shapeName != null)
            {
                Moments m = Cv2.Moments(contour);
                int cx = (int)(m.M10 / m.M00);
                int cy = (int)(m.M01 / m.M00);

                Cv2.DrawContours(image, new Point[][] { contour }, 0, color, -1);
                Cv2.PutText(image, shapeName, new Point(cx - 50, cy), HersheyFonts.HersheySimplex, 1.0, new Scalar(0, 0, 0));
            }
        }




    }
    // Update is called once per frame
    void Update()
    {
        
    }

    void GetCsv() {
        csvFile = Resources.Load("ar_maru_batu") as TextAsset; // Resouces下のCSV読み込み
        StringReader reader = new StringReader(csvFile.text);

        // , で分割しつつ一行ずつ読み込み
        // リストに追加していく
        while (reader.Peek() != -1) // reader.Peaekが-1になるまで
        {
            string line = reader.ReadLine(); // 一行ずつ読み込み
            csvDatas.Add(line.Split(',')); // , 区切りでリストに追加
        }

        // csvDatas[行][列]を指定して値を自由に取り出せる
        Debug.Log("GetCSV");
        qText.text = csvDatas[0][qCnt];

    }

    

    public void OnButtonPressed(VirtualButtonBehaviour vb)
    {
        Debug.Log("Press!!!!");
      
    
        Debug.Log("aaa" + vMaruScr.IsVisible());
        //Debug.Log("bbb" + batuAppear);

        //Renderer ren_maruBtn = ansBtnObj.GetComponent<Renderer>();
        // Renderer ren_batuBtn = ansBtnObj2.GetComponent<Renderer>();

        if (vMaruScr.IsVisible() == true && vBatuScr.IsVisible() == false) {

        Debug.Log("○おした");
       

        if (csvDatas[qCnt][1] == "1")
            {
                Debug.Log("○で正解" + csvDatas[qCnt][1]);
                Instantiate(maruObj, ansBtnObj.transform.position, Quaternion.Euler(90, 0, 0));
            }
            else
            {
                Debug.Log("×で不正解" + csvDatas[qCnt][1]);
                Instantiate(batuObj, ansBtnObj.transform.position, Quaternion.Euler(90, 0, 0));
            }
        }
        if (vMaruScr.IsVisible() == false && vBatuScr.IsVisible() == true)
        {
            Debug.Log("×おした");
        if (csvDatas[qCnt][2] == "1")
        {
            Debug.Log("×で正解" + csvDatas[qCnt][2]);
            Instantiate(batuObj, ansBtnObj2.transform.position, Quaternion.Euler(90, 0, 0));
        }
        else
        {
            Debug.Log("○で不正解" + csvDatas[qCnt][2]);
            Instantiate(maruObj, ansBtnObj2.transform.position, Quaternion.Euler(90, 0, 0));
        }
        }


    }



    public void OnButtonReleased(VirtualButtonBehaviour vb)
    {
        Debug.Log("Button released");
        throw new System.NotImplementedException();

        // アタッチしているGameObjectをDestroyする
        //Destroy(gameObject);
    }

    public void onClick()
    {
        Debug.Log("(・∀・)ｲｲ");
        qCnt++;
        qText.text = csvDatas[qCnt][0];
    }
}
