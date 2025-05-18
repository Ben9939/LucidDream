using UnityEngine;

/// <summary>
/// ImageToMaze クラスは、Texture2D 型の画像を迷路用の２次元配列に変換する機能を提供します。
/// GenerateMaze メソッドで画像から迷路配列を生成します。
/// </summary>
public class ImageToMaze : MonoBehaviour
{
    public static ImageToMaze Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    /// <summary>
    /// 指定した画像を基に迷路配列を生成します。
    /// </summary>
    /// <param name="inputImage">入力画像</param>
    /// <param name="scale">拡張比率</param>
    /// <returns>生成された迷路配列</returns>
    public int[,] GenerateMaze(Texture2D inputImage, int scale)
    {
        int[,] mazeArray = ConvertImageToArray(inputImage);
        mazeArray = ExpandArray(mazeArray, scale);
        return mazeArray;
    }

    // === 色判定用メソッド ===

    bool IsBlack(Color color)
    {
        return color.r < 0.1f && color.g < 0.1f && color.b < 0.1f;
    }

    bool IsWhite(Color color)
    {
        return color.r > 0.9f && color.g > 0.9f && color.b > 0.9f;
    }

    bool IsBlue(Color color)
    {
        return color.b > 0.9f && color.r < 0.1f && color.g < 0.1f;
    }

    bool IsRed(Color color)
    {
        return color.r > 0.9f && color.g < 0.1f && color.b < 0.1f;
    }

    bool IsGreen(Color color)
    {
        return color.g > 0.9f && color.r < 0.1f && color.b < 0.1f;
    }

    bool IsYellow(Color color)
    {
        return color.r > 0.9f && color.g > 0.9f && color.b < 0.1f; // 黄色：赤+緑、高輝度、青低
    }

    bool IsMagenta(Color color)
    {
        return color.r > 0.9f && color.b > 0.9f && color.g < 0.1f; // 品紅：赤+青、高輝度、緑低
    }

    // === 画像を配列に変換する ===

    int[,] ConvertImageToArray(Texture2D image)
    {
        int width = image.width;
        int height = image.height;
        int[,] array = new int[height, width];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color pixelColor = image.GetPixel(x, y);

                // 特定の色に応じた値を設定
                if (IsBlack(pixelColor))
                {
                    array[y, x] = 1; // 黒は 1
                    continue;
                }
                if (IsBlue(pixelColor))
                {
                    array[y, x] = 2; // 青は 2
                    continue;
                }
                if (IsRed(pixelColor))
                {
                    array[y, x] = 3; // 赤は 3
                    continue;
                }
                if (IsGreen(pixelColor))
                {
                    array[y, x] = 4; // 緑は 4
                    continue;
                }
                if (IsMagenta(pixelColor))
                {
                    array[y, x] = 5; // 品紅は 5
                    continue;
                }
                if (IsYellow(pixelColor))
                {
                    array[y, x] = 6; // 黄色は 6
                    continue;
                }
                if (IsWhite(pixelColor))
                {
                    array[y, x] = 0; // 白その他は 0
                    continue;
                }
            }
        }

        return array;
    }

    int[,] ExpandArray(int[,] originalArray, int scale)
    {
        int originalRows = originalArray.GetLength(0);
        int originalCols = originalArray.GetLength(1);

        // 拡張後の配列サイズ
        int newRows = originalRows * scale;
        int newCols = originalCols * scale;

        int[,] expandedArray = new int[newRows, newCols];

        // 元配列を拡張
        for (int row = 0; row < originalRows; row++)
        {
            for (int col = 0; col < originalCols; col++)
            {
                int value = originalArray[row, col];

                if (value == 2 || value == 3)
                {
                    // 値が 2 または 3 の場合、中央のみ設定
                    int centerRow = row * scale + scale / 2;
                    int centerCol = col * scale + scale / 2;
                    expandedArray[centerRow, centerCol] = value;
                }
                else
                {
                    // 該当セルを scale x scale の領域に展開
                    for (int i = 0; i < scale; i++)
                    {
                        for (int j = 0; j < scale; j++)
                        {
                            expandedArray[row * scale + i, col * scale + j] = value;
                        }
                    }
                }
            }
        }

        return expandedArray;
    }

    /// <summary>
    /// ２次元配列の内容をデバッグ出力します。
    /// </summary>
    /// <param name="array">出力対象の２次元配列</param>
    void PrintArray(int[,] array)
    {
        string arrayString = "";
        for (int y = array.GetLength(0) - 1; y >= 0; y--)
        {
            for (int x = 0; x < array.GetLength(1); x++)
            {
                arrayString += array[y, x] + " ";
            }
            arrayString += "\n";
        }

        Debug.Log(arrayString);
    }
}