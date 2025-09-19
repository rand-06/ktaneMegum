using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KModkit;
using System.Linq;

public class script : MonoBehaviour
{

    public GameObject[] fruits = new GameObject[8];
    public SpriteRenderer[] fruitSprites = new SpriteRenderer[8];
    private bool moduleSolved;
    private Vector3[] fruitPos = new Vector3[8];
    public KMSelectable statusLight;
    public KMBombInfo bombInfo;
    public KMBombModule module;
    public Sprite[] SecondStageSprites = new Sprite[8];
    public Sprite[] SecondStageMegum = new Sprite[8];
    public Sprite[] ThirdStageSprites = new Sprite[4];
    public Sprite ThirdStageMegum;
    public Sprite[] SolvedMegum = new Sprite[4];
    public KMAudio audio;
    public AudioClip[] solvedaudio = new AudioClip[4];
    private string[] logItems = { "Bottom Bun", "Cheese", "Pickle", "Ham", "Lettuce", "\"Sauce\"", "Tomatoes", "Top Bun" };
    private string[] ans3log = { "KTaNE", "Dandy's World", "Gartic Phone", "Minecraft" };


    public SpriteRenderer Megum;

    private int[] ans1;
    private bool[] pressed = { false, false, false, false, false, false, false, false };
    private int pressedAmount = 0;
    private int megum2;
    private int stageNumber = 1;
    private int ans3;

    static int ModuleIdCounter = 1;
    int ModuleId;

    int TPForceSelect;
    string stage2Ingredients;

    void Awake()
    {
        ModuleId = ModuleIdCounter++;
    }

    IEnumerator scroll(int ix)
    {
        yield return new WaitForSeconds(.5f);
        if (!moduleSolved) StartCoroutine(scroll((ix + 1) % 8)); else yield return null;
        float delta = 0;
        while (delta < .5f)
        {
            delta += Time.deltaTime / 25;
            fruits[ix].transform.localPosition = Mathf.Lerp(0, 1.5f, delta) * Vector3.left + fruitPos[ix];
            yield return null;
        }
    }
    float abs(float x) { return x < 0 ? -x : x; }
    int abs(int x) { return x < 0 ? -x : x; }
    int findMinItem()
    {
        int min_ind = 0;
        float min = abs(fruits[0].transform.localPosition.x);
        for (int i = 1; i < 8; i++)
        {
            float cur = abs(fruits[i].transform.localPosition.x);
            if (cur < min)
            {
                min = cur;
                min_ind = i;
            }
        }
        return min_ind;
    }
    bool pressStatus()
    {
        if (!moduleSolved)
        {
            int min_ind = TPForceSelect != -1 ? (stageNumber == 2 ? (stage2Ingredients.IndexOf(TPForceSelect.ToString())) : TPForceSelect) : findMinItem();
            if (stageNumber != 3)
            {
                if (!pressed[min_ind])
                {
                    Debug.Log("[Megum #" + ModuleId.ToString() + "] Detected press: " + min_ind.ToString());
                    if (ans1[pressedAmount] == min_ind)
                    {
                        fruitSprites[min_ind].color = new Color(1, 1, 1, 0.2f);
                        pressedAmount++;
                        pressed[min_ind] = true;
                    }
                    else
                    {
                        module.HandleStrike();
                        Debug.Log("[Megum #" + ModuleId.ToString() + "] Strike.");
                    }
                }
            }
            else
            {
                if (ans3 == min_ind % 4) customSolve(ans3);
                else module.HandleStrike();
            }
            if (pressedAmount == 8)
            {
                switch (stageNumber)
                {
                    case 1:
                        {
                            pressedAmount = 0;
                            Debug.Log("[Megum #" + ModuleId.ToString() + "] Stage 1 completed. Stage 2:");
                            pressed = new bool[] { false, false, false, false, false, false, false, false };
                            string seed = generate2ndStage();
                            ans1 = generateAnswer2(seed);
                            Debug.Log("[Megum #" + ModuleId.ToString() + "] Full list: " +
                                logItems[seed[ans1[0]]-'0'] + ", " +
                                logItems[seed[ans1[1]]-'0'] + ", " +
                                logItems[seed[ans1[2]]-'0'] + ", " +
                                logItems[seed[ans1[3]]-'0'] + ", " +
                                logItems[seed[ans1[4]]-'0'] + ", " +
                                logItems[seed[ans1[5]]-'0'] + ", " +
                                logItems[seed[ans1[6]]-'0'] + ", " +
                                logItems[seed[ans1[7]]-'0'] + ".");
                            stageNumber++;
                            break;
                        }
                    case 2:
                        {
                            pressedAmount = 7;
                            pressed = new bool[] { false, false, false, false, false, false, false, false };
                            for (int i = 0; i < 8; i++)
                            {
                                fruitSprites[i].color = new Color(1, 1, 1, 1);
                                fruitSprites[i].sprite = ThirdStageSprites[i % 4];
                            }
                            Megum.sprite = ThirdStageMegum;
                            Debug.Log("[Megum #" + ModuleId.ToString() + "] Third stage. Answer: " + ans3log[ans3]);
                            stageNumber++;
                            break;
                        }
                }
            }
        }
        return false;
    }
    int[] generateAnswer()
    {
        string sn = bombInfo.GetSerialNumber().ToString();
        string base36 = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        int[] mult = { 26496, 26496, 6336, 1296, 36, 1 };
        int num = 0;
        for (int i = 0; i < 6; i++) num += mult[i] * base36.IndexOf(sn[i]);
        num %= 40320;

        Debug.Log("[Megum #" + ModuleId.ToString() + "] First stage. N = " + num.ToString());
        int factorial = 5040;
        int[] ans = { 0, 0, 0, 0, 0, 0, 0, 0 };
        for (int i = 7; num > 0;)
        {
            if (num < factorial)
            {
                factorial /= i;
                i--;
            }
            else
            {
                num -= factorial;
                ans[7 - i]++;
            }
        }
        Debug.Log("[Megum #" + ModuleId.ToString() + "] String of digits: " + 
            ans[0].ToString() + 
            ans[1].ToString() + 
            ans[2].ToString() + 
            ans[3].ToString() + 
            ans[4].ToString() + 
            ans[5].ToString() + 
            ans[6].ToString() + 
            ans[7].ToString());
        int[] ans1 = new int[8];
        string table = "ABGKMOSW";
        string tablecopy = "ABGKMOSW";
        string anstable = "";

        for (int i = 0; i < 8; i++)
        {
            anstable += table[ans[i]];
            ans1[i] = tablecopy.IndexOf(table[ans[i]]);
            table = table.Remove(ans[i], 1);
        }
        Debug.Log("[Megum #" + ModuleId.ToString() + "] String of fruits: " + anstable);
        return ans1;
    }
    string generate2ndStage()
    {
        int num = Random.Range(0, 40320);
        Debug.Log("[Megum #" + ModuleId.ToString() + "] 2nd stage seed: "+num.ToString());
        int factorial = 5040;
        int[] ans = { 0, 0, 0, 0, 0, 0, 0, 0 };
        for (int i = 7; num > 0;)
        {
            if (num < factorial)
            {
                factorial /= i;
                i--;
            }
            else
            {
                num -= factorial;
                ans[7 - i]++;
            }
        }

        string table = "01234567";
        string anstable = "";
        
        for (int i = 0; i < 8; i++)
        {
            anstable += table[ans[i]];
            table = table.Remove(ans[i], 1);
        }

        anstable = anstable.Substring(anstable.IndexOf('7')) + anstable.Substring(0, anstable.IndexOf('7'));
        stage2Ingredients = anstable;

        Debug.Log("[Megum #" + ModuleId.ToString() + "] Ingridient list on the module: " +
            logItems[anstable[0] - '0'] + ", " +
            logItems[anstable[1] - '0'] + ", " +
            logItems[anstable[2] - '0'] + ", " +
            logItems[anstable[3] - '0'] + ", " +
            logItems[anstable[4] - '0'] + ", " +
            logItems[anstable[5] - '0'] + ", " +
            logItems[anstable[6] - '0'] + ", " +
            logItems[anstable[7] - '0'] + ".");

        for (int i = 0; i < 8; i++)
        {
            fruitSprites[i].color = new Color(1, 1, 1, 1);
            fruitSprites[i].sprite = SecondStageSprites[anstable[i] - '0'];
        }

        Megum.sprite = SecondStageMegum[megum2];
        return anstable;
    }
    int[] generateAnswer2(string items0)
    {
        string items = items0;
        int bits1 = abs(items.IndexOf('0') - items.IndexOf('7'));
        if (bits1>3) bits1 = 7 - bits1;
        else bits1 = bits1 - 1;

        Debug.Log("[Megum #" + ModuleId.ToString() + "] Buns position:" + 
            items.IndexOf('0') +", " + items.IndexOf('7') +". Distance: " + bits1);


        items = items.Remove(items.IndexOf('0'), 1);
        items = items.Remove(items.IndexOf('7'), 1);

        Debug.Log("[Megum #" + ModuleId.ToString() + "] Deleting buns. Ingridient list: " +
            logItems[items[0] - '0'] + ", " +
            logItems[items[1] - '0'] + ", " +
            logItems[items[2] - '0'] + ", " +
            logItems[items[3] - '0'] + ", " +
            logItems[items[4] - '0'] + ", " +
            logItems[items[5] - '0'] + ".");

        int[] table = { 0, 1, 2, 3, 4, 5 };
        if (bits1 > 1)
        {
            int temp = table[2];
            table[2] = table[1];
            table[1] = table[0];
            table[0] = temp;
            Debug.Log("[Megum #" + ModuleId.ToString() + "] Config: 1*-***. Ingridient list: " +
            logItems[items[table[0]] - '0'] + ", " +
            logItems[items[table[1]] - '0'] + ", " +
            logItems[items[table[2]] - '0'] + ", " +
            logItems[items[table[3]] - '0'] + ", " +
            logItems[items[table[4]] - '0'] + ", " +
            logItems[items[table[5]] - '0'] + ".");
        }
        else
        {
            int temp = table[0];
            table[0] = table[1];
            table[1] = table[2];
            table[2] = temp;
            Debug.Log("[Megum #" + ModuleId.ToString() + "] Config: 0*-***. Ingridient list: " +
            logItems[items[table[0]] - '0'] + ", " +
            logItems[items[table[1]] - '0'] + ", " +
            logItems[items[table[2]] - '0'] + ", " +
            logItems[items[table[3]] - '0'] + ", " +
            logItems[items[table[4]] - '0'] + ", " +
            logItems[items[table[5]] - '0'] + ".");
        }
        if (bits1 % 2 == 1)
        {
            int temp = table[5];
            table[5] = table[4];
            table[4] = table[3];
            table[3] = temp;
            Debug.Log("[Megum #" + ModuleId.ToString() + "] Config: *1-***. Ingridient list: " +
            logItems[items[table[0]] - '0'] + ", " +
            logItems[items[table[1]] - '0'] + ", " +
            logItems[items[table[2]] - '0'] + ", " +
            logItems[items[table[3]] - '0'] + ", " +
            logItems[items[table[4]] - '0'] + ", " +
            logItems[items[table[5]] - '0'] + ".");
        }
        else
        {
            int temp = table[3];
            table[3] = table[4];
            table[4] = table[5];
            table[5] = temp;
            Debug.Log("[Megum #" + ModuleId.ToString() + "] Config: *0-***. Ingridient list: " +
            logItems[items[table[0]] - '0'] + ", " +
            logItems[items[table[1]] - '0'] + ", " +
            logItems[items[table[2]] - '0'] + ", " +
            logItems[items[table[3]] - '0'] + ", " +
            logItems[items[table[4]] - '0'] + ", " +
            logItems[items[table[5]] - '0'] + ".");
        }

        if (megum2 > 3)
        {
            int temp = table[0];
            table[0] = table[3];
            table[3] = temp;
            Debug.Log("[Megum #" + ModuleId.ToString() + "] Config: **-1**. Ingridient list: " +
            logItems[items[table[0]] - '0'] + ", " +
            logItems[items[table[1]] - '0'] + ", " +
            logItems[items[table[2]] - '0'] + ", " +
            logItems[items[table[3]] - '0'] + ", " +
            logItems[items[table[4]] - '0'] + ", " +
            logItems[items[table[5]] - '0'] + ".");
        }
        if (megum2 % 4 > 1)
        {
            int temp = table[1];
            table[1] = table[4];
            table[4] = temp;
            Debug.Log("[Megum #" + ModuleId.ToString() + "] Config: **-*1*. Ingridient list: " +
            logItems[items[table[0]] - '0'] + ", " +
            logItems[items[table[1]] - '0'] + ", " +
            logItems[items[table[2]] - '0'] + ", " +
            logItems[items[table[3]] - '0'] + ", " +
            logItems[items[table[4]] - '0'] + ", " +
            logItems[items[table[5]] - '0'] + ".");
        }
        if (megum2 % 2 > 0)
        {
            int temp = table[2];
            table[2] = table[5];
            table[5] = temp;
            Debug.Log("[Megum #" + ModuleId.ToString() + "] Config: **-**1. Ingridient list: " +
            logItems[items[table[0]] - '0'] + ", " +
            logItems[items[table[1]] - '0'] + ", " +
            logItems[items[table[2]] - '0'] + ", " +
            logItems[items[table[3]] - '0'] + ", " +
            logItems[items[table[4]] - '0'] + ", " +
            logItems[items[table[5]] - '0'] + ".");
        }

        if (bombInfo.GetBatteryCount() % 2 == 0)
        {
            int temp = table[0];
            table[0] = table[3];
            table[3] = table[4];
            table[4] = table[5];
            table[5] = table[2];
            table[2] = table[1];
            table[1] = temp;
            Debug.Log("[Megum #" + ModuleId.ToString() + "] Even batteries. Ingridient list: " +
            logItems[items[table[0]] - '0'] + ", " +
            logItems[items[table[1]] - '0'] + ", " +
            logItems[items[table[2]] - '0'] + ", " +
            logItems[items[table[3]] - '0'] + ", " +
            logItems[items[table[4]] - '0'] + ", " +
            logItems[items[table[5]] - '0'] + ".");
        }
        else
        {
            int temp = table[0];
            table[0] = table[1];
            table[1] = table[2];
            table[2] = table[5];
            table[5] = table[4];
            table[4] = table[3];
            table[3] = temp;
            Debug.Log("[Megum #" + ModuleId.ToString() + "] Odd batteries. Ingridient list: " +
            logItems[items[table[0]] - '0'] + ", " +
            logItems[items[table[1]] - '0'] + ", " +
            logItems[items[table[2]] - '0'] + ", " +
            logItems[items[table[3]] - '0'] + ", " +
            logItems[items[table[4]] - '0'] + ", " +
            logItems[items[table[5]] - '0'] + ".");
        }

        int[] ans = { 0, items[table[0]] - '0', items[table[1]] - '0', items[table[2]] - '0', items[table[3]] - '0', items[table[4]] - '0', items[table[5]] - '0', 7 };
        int[] ans2 = new int[8];
        for (int i = 0; i < 8; i++)
        {
            ans2[i] = items0.IndexOf((char)('0' + ans[i]));
        }

        return ans2;
    }

    void customSolve(int num)
    {
        audio.PlaySoundAtTransform(solvedaudio[num].name, transform);
        Debug.Log("[Megum #" + ModuleId.ToString() + "] Solve.");
        Megum.sprite = SolvedMegum[num];
        module.HandlePass();
        moduleSolved = true;
    }

    int[] playersPoints()
    {
        List<string> modules = bombInfo.GetModuleIDs();
        int[] ans = { 0, 0, 0, 0, 0, 0 };
        if (modules.Contains("PatternCubeModule"))          ans[0] += 7; 
        if (modules.Contains("SetModule"))                  ans[0] += 6; 
        if (modules.Contains("booleanVennModule"))          ans[0] += 5; 
        if (modules.Contains("deckOfManyThings"))           ans[0] += 4; 
        if (modules.Contains("ChordQualities"))             ans[0] += 3; 
        if (modules.Contains("lasers"))                     ans[0] += 2; 
        if (modules.Contains("PuzzwordModule"))             ans[0] += 1;

        if (modules.Contains("TheOctadecayotton"))          ans[1] += 7;
        if (modules.Contains("StableTimeSignatures"))       ans[1] += 6;
        if (modules.Contains("notkarnaugh"))                ans[1] += 5;
        if (modules.Contains("unfairCipher"))               ans[1] += 4;
        if (modules.Contains("FloorLights"))                ans[1] += 3;
        if (modules.Contains("tripleTermModule"))           ans[1] += 2;
        if (modules.Contains("actual4x4x4SudokuModule"))    ans[1] += 1;

        if (modules.Contains("indigoCipher"))               ans[2] += 7;
        if (modules.Contains("quintuples"))                 ans[2] += 6;
        if (modules.Contains("answerSmashModule"))          ans[2] += 5;
        if (modules.Contains("3dTunnels"))                  ans[2] += 4;
        if (modules.Contains("timingIsEverything"))         ans[2] += 3;
        if (modules.Contains("HexiEvilFMN"))                ans[2] += 2;
        if (modules.Contains("UltraStores"))                ans[2] += 1;

        if (modules.Contains("metapuzzle"))                 ans[3] += 7;
        if (modules.Contains("yellowCipher"))               ans[3] += 6;
        if (modules.Contains("supermarketScramble"))        ans[3] += 5;
        if (modules.Contains("unfairsRevenge"))             ans[3] += 4;
        if (modules.Contains("boomtarTheGreat"))            ans[3] += 3;
        if (modules.Contains("karnaugh"))                   ans[3] += 2;
        if (modules.Contains("Kuro"))                       ans[3] += 1;

        if (modules.Contains("bamboozlingButton"))          ans[4] += 7;
        if (modules.Contains("simonStores"))                ans[4] += 6; 
        if (modules.Contains("dandysFloors"))               ans[4] += 5; 
        if (modules.Contains("BlackHoleModule"))            ans[4] += 4; 
        if (modules.Contains("ForgetAnyColor"))             ans[4] += 3; 
        if (modules.Contains("SimonShrieksModule"))         ans[4] += 2; 
        if (modules.Contains("timezone"))                   ans[4] += 1;

        if (modules.Contains("lightspeed"))                 ans[5] += 7;
        if (modules.Contains("simonStores"))                ans[5] += 6;
        if (modules.Contains("hyperlink"))                  ans[5] += 5;
        if (modules.Contains("forgetMazeNot"))              ans[5] += 4;
        if (modules.Contains("BartendingModule"))           ans[5] += 3;
        if (modules.Contains("RailwayCargoLoading"))        ans[5] += 2;
        if (modules.Contains("widgetry"))                   ans[5] += 1;

        List<string> onInds = bombInfo.GetOnIndicators().ToList();
        List<string> offInds = bombInfo.GetOffIndicators().ToList();

        if (onInds.Contains("BOB")) ans[0] += 3;
        if (onInds.Contains("CAR")) ans[3] += 2;
        if (onInds.Contains("CLR")) ans[5] += 2;
        if (onInds.Contains("FRK")) ans[3] += 3;
        if (onInds.Contains("MSA")) ans[4] += 3;
        if (onInds.Contains("NSA")) ans[4] += 2;
        if (onInds.Contains("SIG")) ans[2] += 1;
        if (onInds.Contains("SND")) ans[2] += 3;
        if (onInds.Contains("TRN")) ans[0] += 2;

        if (offInds.Contains("CAR")) ans[5] += 1;
        if (offInds.Contains("CLR")) ans[2] += 2;
        if (offInds.Contains("FRK")) ans[0] += 1;
        if (offInds.Contains("FRQ")) ans[1] += 2;
        if (offInds.Contains("IND")) ans[1] += 3;
        if (offInds.Contains("NSA")) ans[1] += 1;
        if (offInds.Contains("SIG")) ans[3] += 1;
        if (offInds.Contains("SND")) ans[5] += 3;
        if (offInds.Contains("TRN")) ans[4] += 1;


        if (onInds.Contains("IND"))
        {
            ans[0]++;
            ans[1]++;
            ans[2]++;
            ans[3]++;
            ans[4]++;
            ans[5]++;
        }
        if (offInds.Contains("MSA"))
        {
            ans[0]++;
            ans[1]++;
            ans[2]++;
            ans[3]++;
            ans[4]++;
            ans[5]++;
        }

        if (onInds.Contains("FRQ"))
        {
            ans[0]+=2;
            ans[1]+=2;
            ans[2]+=2;
            ans[3]+=2;
            ans[4]+=2;
            ans[5]+=2;
        }
        if (offInds.Contains("BOB"))
        {
            ans[0]+=2;
            ans[1]+=2;
            ans[2]+=2;
            ans[3]+=2;
            ans[4]+=2;
            ans[5]+=2;
        }

        List<string> ports = bombInfo.GetPorts().ToList();
        if (ports.Contains("DVI"))          ans[0]++;
        if (ports.Contains("Serial"))       ans[1]++;
        if (ports.Contains("StereoRCA"))    ans[2]++;
        if (ports.Contains("PS2"))          ans[3]++;
        if (ports.Contains("RJ45"))         ans[4]++;
        if (ports.Contains("Parallel"))     ans[5]++;


        for (int i = 0; i < 6; i++) ans[i] %= 16;

        return ans;
    }

    int vote(int[] points)
    {
        string[] table = { "MDKKGMDDKGGKDDMK", "KKMGDDKMGKDDMGMD", "MKKKKKDKDKKGGGKK", "KKKDDKGGKDKKDKMD", "KGKDKGDMKKGDKMGD", "DKMGGMKGDMMMGDDM" };

        int[] votes = new int[6];
        for (int i = 0; i < 6; i++) switch (table[i][points[i]])
            {
                case 'K':
                    {
                        votes[i] = 0;
                        break;
                    }
                case 'D':
                    {
                        votes[i] = 1;
                        break;
                    }
                case 'G':
                    {
                        votes[i] = 2;
                        break;
                    }
                case 'M':
                    {
                        votes[i] = 3;
                        break;
                    }
            }
        int[] weights = { 9, 7, 5, 6, 4, 6 };
        int[] res = { 0, 0, 0, 0 };
        for (int i = 0; i < 6; i++)
        {
            res[votes[i]] += weights[i];
        }
        int max_ind = 0;
        int max = res[0];
        for (int i = 1; i < 4; i++)
        {
            if (res[i] > max)
            {
                res[i] = max;
                max_ind = i;
            }
        }
        return max_ind;
    }


    void Start()
    {
        for (int i = 0; i < 8; i++) fruitPos[i] = fruits[i].transform.localPosition;
        StartCoroutine(scroll(0));
        statusLight.OnInteract += pressStatus;
        ans1 = generateAnswer();
        ans3 = vote(playersPoints());
        megum2 = Random.Range(0, 8);
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"In Phase 1, use <!{0} A/B/G/K/M/O/S/W> to select a fruit (Apple/Banana/Grapes/Kiwi/Mango/Orange/Starfruit/Watermelon) - chain with/without spaces. In Phase 2, use <!{0} B/C/P/H/L/S/T> to select a sandwich ingredient ((Bottom Bun/Top Bun, the correct Bun will be selected automatically)/Cheese/Pickle/Ham/Lettuce/Sauce/Tomatoes) - chain with/without spaces. In Phase 3, use <!{0} K/D/G/M> to select an activity (KTANE/Dandy's World/Gartic Phone/Minecraft).";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string Command)
    {
        var commandArgs = Command.ToUpperInvariant().Replace(" ", "");
        switch (stageNumber)
        {
            case 1:
            case 2:
                string stageIngredients = stageNumber == 1 ? "ABGKMOSW" : "BCPHLST";
                List<int> selects = new List<int>();
                foreach (char food in commandArgs)
                {
                    if (stageIngredients.Contains(food)) selects.Add(stageIngredients.IndexOf(food));
                    else yield return "sendtochaterror Invalid command!";
                }
                if (stageNumber == 1 && selects.Count != selects.Distinct().ToList().Count) yield return "sendtochaterror Invalid command!";
                else
                {
                    yield return null;
                    foreach (int ingredient in selects)
                    {
                        TPForceSelect = (stageNumber == 2 && pressed[stage2Ingredients.IndexOf('0')] && ingredient == 0) ? 7 : ingredient;
                        statusLight.OnInteract();
                        yield return new WaitForSeconds(0.2f);
                    }
                }
                break;
            case 3:
                if (commandArgs.Length != 1 || !"KDGM".Contains(commandArgs)) yield return "sendtochaterror Invalid command!";
                else
                {
                    yield return null;
                    TPForceSelect = "KDGM".IndexOf(commandArgs);
                    statusLight.OnInteract();
                    yield return new WaitForSeconds(0.2f);
                }
                break;
            default:
                break;
        }
        yield return null;
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
        while (!moduleSolved)
        {
            switch (stageNumber)
            {
                case 1:
                case 2:
                    foreach (int ingredient in ans1.Skip(pressedAmount))
                    {
                        TPForceSelect = stageNumber == 1 ? ingredient : (stage2Ingredients[ingredient] - '0');
                        statusLight.OnInteract();
                        yield return new WaitForSeconds(0.2f);
                    }
                    break;
                case 3:
                    TPForceSelect = ans3;
                    statusLight.OnInteract();
                    yield return new WaitForSeconds(0.2f);
                    break;
                default:
                    break;
            }
        }
    }
}
