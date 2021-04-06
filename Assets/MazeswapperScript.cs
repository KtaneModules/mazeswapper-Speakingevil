using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using System.Text.RegularExpressions;
using Random = UnityEngine.Random;
using KModkit;

public class MazeswapperScript : MonoBehaviour
{

    public KMAudio Audio;
    public KMBombModule module;
    public KMBombInfo info;
    public KMSelectable[] buttons;
    public GameObject[] walls;
    public GameObject[] markers;
    public Renderer[] borders;
    public Renderer[] traverse;
    public Material[] mats;
    public GameObject barycentre;
    public TextMesh[] display;

    private Vector3[] pos = new Vector3[36];
    private bool[][] wallpresent = new bool[36][] { new bool[4], new bool[4], new bool[4], new bool[4], new bool[4], new bool[4], new bool[4], new bool[4], new bool[4], new bool[4], new bool[4], new bool[4], new bool[4], new bool[4], new bool[4], new bool[4], new bool[4], new bool[4], new bool[4], new bool[4], new bool[4], new bool[4], new bool[4], new bool[4], new bool[4], new bool[4], new bool[4], new bool[4], new bool[4], new bool[4], new bool[4], new bool[4], new bool[4], new bool[4], new bool[4], new bool[4]};
    private string[] grids = new string[6] { "7DGPTHO1IFBYM0JX86349QSALEZKC2WVU5RN", "S5B8H6YFI24JAGV70XW1QMERPLZNUKDO39TC", "EYO2H9AIP5FWL1SCZD7VUJBQ48K6NGT03XRM", "NW8OU4TDJ7IQ9E2LP6FZHM50G3YVRACXBKS1", "AHFT1WLD5VRIOSC8MG9QXNP2407J3BUKZEY6", "G0S3H5KXPTIELC9ZA1BM8F2DR7VWJQO4NU6Y" };
    private int source;
    private int[][] disprc = new int[4][] { new int[2], new int[2], new int[2], new int[2]};
    private int[] ordering = new int[36];
    private int[] pair = new int[2] { -1, -1 };
    private bool swapping;

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;
  
    private void Awake()
    {
        module.OnActivate = Activate;
    }

    private void Activate()
    {
        moduleID = ++moduleIDCounter;      
        string[][] maze = new string[18][];
        for (int i = 0; i < 18; i++)
            maze[i] = i % 3 == 1 ? new string[18] { "X", "-", "X", "X", "-", "X", "X", "-", "X", "X", "-", "X", "X", "-", "X", "X", "-", "X" } : new string[18] { "X", "X", "X", "X", "X", "X", "X", "X", "X", "X", "X", "X", "X", "X", "X", "X", "X", "X" };
        maze[(3 * Random.Range(0, 6)) + 1][(3 * Random.Range(0, 6)) + 1] = "+";
        while (maze.Any(r => r.Contains("-")))
        {
            int[] select = new int[2];
            for (int i = 0; i < 2; i++)
                select[i] = Random.Range(0, 6);
            while (maze[(3 * select[0]) + 1][(3 * select[1]) + 1] != "+")
            {
                for (int i = 0; i < 2; i++)
                    select[i] = Random.Range(0, 6);
            }
            int del = Random.Range(0, 4);
            while ((del == 0 && (select[1] == 0 || maze[(3 * select[0]) + 1][3 * select[1]] != "X" || maze[(3 * select[0]) + 1][(3 * select[1]) - 2] != "-")) || (del == 1 && (select[0] == 0 || maze[3 * select[0]][(3 * select[1]) + 1] != "X" || maze[(3 * select[0]) - 2][(3 * select[1]) + 1] != "-")) || (del == 2 && (select[1] == 5 || maze[(3 * select[0]) + 1][(3 * select[1]) + 2] != "X" || maze[(3 * select[0]) + 1][(3 * select[1]) + 4] != "-")) || (del == 3 && (select[0] == 5 || maze[(3 * select[0]) + 2][(3 * select[1]) + 1] != "X" || maze[(3 * select[0]) + 4][(3 * select[1]) + 1] != "-")))
                del = Random.Range(0, 4);
            switch (del)
            {
                case 0:
                    maze[(3 * select[0]) + 1][3 * select[1]] = "/";
                    maze[(3 * select[0]) + 1][(3 * select[1]) - 1] = "/";
                    maze[(3 * select[0]) + 1][(3 * select[1]) - 2] = "+";
                    break;
                case 1:
                    maze[3 * select[0]][(3 * select[1]) + 1] = "/";
                    maze[(3 * select[0]) - 1][(3 * select[1]) + 1] = "/";
                    maze[(3 * select[0]) - 2][(3 * select[1]) + 1] = "+";
                    break;
                case 2:
                    maze[(3 * select[0]) + 1][(3 * select[1]) + 2] = "/";
                    maze[(3 * select[0]) + 1][(3 * select[1]) + 3] = "/";
                    maze[(3 * select[0]) + 1][(3 * select[1]) + 4] = "+";
                    break;
                case 3:
                    maze[(3 * select[0]) + 2][(3 * select[1]) + 1] = "/";
                    maze[(3 * select[0]) + 3][(3 * select[1]) + 1] = "/";
                    maze[(3 * select[0]) + 4][(3 * select[1]) + 1] = "+";
                    break;
            }
            for (int i = 1; i < 18; i += 3)
                for (int j = 1; j < 18; j += 3)
                    if (maze[i][j] == "+" && (j == 1 || maze[i][j - 3] != "-") && (i == 1 || maze[i - 3][j] != "-") && (j == 16 || maze[i][j + 3] != "-") && (i == 16 || maze[i + 3][j] != "-"))
                        maze[i][j] = "o";
        }
        for (int i = 0; i < 36; i++)
        {
            pos[i] = buttons[i].transform.localPosition;
            ordering[i] = i;
            if (maze[3 * (i / 6)][(3 * (i % 6)) + 1] == "X")
                wallpresent[i][0] = true;
            else
                walls[i].SetActive(false);
            if (maze[(3 * (i / 6)) + 1][3 * (i % 6) + 2] == "X")
                wallpresent[i][1] = true;
            else
                walls[i + 36].SetActive(false);
            if (maze[(3 * (i / 6)) + 2][(3 * (i % 6)) + 1] == "X")
                wallpresent[i][2] = true;
            else
                walls[i + 72].SetActive(false);
            if (maze[(3 * (i / 6)) + 1][3 * (i % 6)] == "X")
                wallpresent[i][3] = true;
            else
                walls[i + 108].SetActive(false);
        }
        int[][] arrange = new int[2][] { new int[3] { 0, 1, 2 }.Shuffle(), new int[3] { Random.Range(0, 6), Random.Range(0, 6), Random.Range(0, 6) } };
        display[0].text = string.Join("-", Enumerable.Range(0, 3).Select(x => "ABCDEFabcdef123456"[(arrange[0][x] * 6) + arrange[1][x]].ToString()).ToArray());
        int[] marks = new int[4];
        for(int i = 0; i < 4; i++)
        {
            int[] cells = new int[3];
            for (int j = 0; j < 3; j++)
                cells[j] = grids[arrange[1][j]].IndexOf(info.GetSerialNumber()[i + j].ToString());
            marks[i] = (cells[1] - cells[0] + cells[2] + 36) % 36;
            while(marks.Where((x, k) => k < i).Contains(marks[i]))
            {
                string currentchar = grids[arrange[1][2]][marks[i]].ToString();
                string nextchar = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ"[("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".IndexOf(currentchar) + 1) % 36].ToString();
                marks[i] = grids[arrange[1][2]].IndexOf(nextchar);
            }
            if (i == 0)
            {
                source = marks[0];
                traverse[marks[0]].material = mats[3];
                Debug.LogFormat("[Mazeswapper #{0}] Clues:\n[Mazeswapper #{0}] The display reads: {3}\n[Mazeswapper #{0}] The source tile belongs at {1}{2}", moduleID, "ABCDEF"[marks[0] % 6], (marks[0] / 6) + 1, display[0].text);
            }
            else
            {
                markers[i - 1].GetComponent<Renderer>().material = mats[3];
                markers[arrange[0][i - 1]].transform.localPosition = new Vector3(pos[marks[i]].x, 0.03f, pos[marks[i]].z);
                markers[arrange[0][i - 1]].transform.parent = buttons[marks[i]].transform;
                Debug.LogFormat("[Mazeswapper #{0}] The {1} belongs at {2}{3}", moduleID, new string[] { "icosahedron", "tetrahedron", "sphere"}[arrange[0][i - 1]], "ABCDEF"[marks[i] % 6], (marks[i] / 6) + 1);
            }
            int[] rc = new int[2];
            rc[0] = (maze[(3 * (marks[i] / 6)) + 1].Count(x => x == "X") - 2) / 2;
            rc[1] = (Enumerable.Range(0, 18).Select(x => maze[x][(3 * (marks[i] % 6)) + 1]).Count(x => x == "X") - 2) / 2;
            if(Random.Range(0, 2) == 0)
            {
                disprc[i][0] = rc[0];
                disprc[i][1] = rc[1];
            }
            else
            {
                disprc[i][1] = rc[0];
                disprc[i][0] = rc[1];
            }
        }
        ordering = ordering.Shuffle();
        while (Check())
        {
            for (int i = 0; i < 36; i++)
                if (Array.IndexOf(ordering, i) != source)
                    traverse[i].material = mats[1];
            ordering = ordering.Shuffle();
        }
        Debug.LogFormat("[Mazeswapper #{0}] Solution:\n[Mazeswapper #{0}] {1}", moduleID, string.Join("\n[Mazeswapper #"+ moduleID + "] ", maze.Select(x => x.Select(y => y == "X" ? "\u25a0" : "\u25a1").Join()).ToArray()));
        for (int i = 0; i < 36; i++)
            buttons[i].transform.localPosition = pos[ordering[i]];
        foreach (KMSelectable button in buttons)
        {
            int b = Array.IndexOf(buttons, button);
            button.OnInteract += delegate () {
                if (!moduleSolved && !swapping)
                {
                    if (pair[0] == -1)
                    {
                        pair[0] = b;
                        borders[ordering[b]].material = mats[4];
                        if (marks.Contains(b))
                        {
                            int c = Array.IndexOf(marks, b);
                            display[1].text = disprc[c][0].ToString();
                            display[2].text = disprc[c][1].ToString();
                        }
                        button.transform.localPosition += new Vector3(0, 0.006f, 0);
                        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, button.transform);
                        for (int i = 0; i < 36; i++)
                            if (i != source)
                                traverse[i].material = mats[1];
                    }
                    else if (pair[1] == -1)
                    {
                        pair[1] = b;
                        display[1].text = string.Empty;
                        display[2].text = string.Empty;
                        if (pair[0] == pair[1])
                        {
                            borders[ordering[b]].material = mats[0];
                            button.transform.localPosition -= new Vector3(0, 0.006f, 0);
                            button.AddInteractionPunch(0.75f);
                            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, button.transform);
                            pair[0] = -1;
                            pair[1] = -1;
                            Check();
                        }
                        else
                            StartCoroutine(Swap(pair[0], pair[1]));
                    }
                }
                return false;
            };
        }
    }

    private IEnumerator Swap(int a, int b)
    {
        swapping = true;
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, buttons[b].transform);
        borders[ordering[b]].material = mats[4];
        float time = 0;
        float cutoff = Time.deltaTime;
        barycentre.transform.localPosition = (pos[ordering[a]] + pos[ordering[b]]) / 2;
        while (time < 0.5f - cutoff)
        {
            float del = Time.deltaTime;
            time += del;
            buttons[a].transform.RotateAround(barycentre.transform.position, transform.up, del * 360);
            buttons[b].transform.RotateAround(barycentre.transform.position, transform.up, del * 360);
            buttons[a].transform.localRotation = Quaternion.Euler(-90, 0, 90);
            buttons[b].transform.localRotation = Quaternion.Euler(-90, 0, 90);
            yield return null;
        }
        buttons[a].transform.localPosition = pos[ordering[b]];
        buttons[b].transform.localPosition = pos[ordering[a]];
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        buttons[a].AddInteractionPunch(0.375f);
        buttons[b].AddInteractionPunch(0.375f);
        borders[ordering[a]].material = mats[0];
        borders[ordering[b]].material = mats[0];
        pair[0] = -1;
        pair[1] = -1;
        int swap = ordering[a];
        ordering[a] = ordering[b];
        ordering[b] = swap;
        if (Check())
        {
            moduleSolved = true;
            module.HandlePass();
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
            display[0].text = "!-!-!";
            for (int i = 0; i < 3; i++)
                markers[i].SetActive(false);
            StartCoroutine(SolveAnim());
        }
        swapping = false;
    }

    private bool Check()
    {
        bool[][][] correct = new bool[2][][]{new bool[6][] { new bool[6], new bool[6], new bool[6], new bool[6], new bool[6], new bool[6] }, new bool[6][] { new bool[6], new bool[6], new bool[6], new bool[6], new bool[6], new bool[6] }};
        bool[] c = new bool[36];
        int[] scramble = Enumerable.Range(0, 36).Select(x => Array.IndexOf(ordering, x)).ToArray();
        string[][] mazescramble = new string[18][] { new string[18], new string[18], new string[18], new string[18], new string[18], new string[18], new string[18], new string[18], new string[18], new string[18], new string[18], new string[18], new string[18], new string[18], new string[18], new string[18], new string[18], new string[18]};
        for(int i = 0; i < 6; i++)
            for(int j = 0; j < 6; j++)
            {
                mazescramble[3 * i][3 * j] = "X";
                mazescramble[3 * i][(3 * j) + 2] = "X";
                mazescramble[(3 * i) + 2][3 * j] = "X";
                mazescramble[(3 * i) + 2][(3 * j) + 2] = "X";
                mazescramble[(3 * i) + 1][(3 * j) + 1] = " ";
                mazescramble[3 * i][(3 * j) + 1] = wallpresent[scramble[(i * 6) + j]][0] ? "X" : " ";
                mazescramble[(3 * i) + 1][(3 * j) + 2] = wallpresent[scramble[(i * 6) + j]][1] ? "X" : " ";
                mazescramble[(3 * i) + 2][(3 * j) + 1] = wallpresent[scramble[(i * 6) + j]][2] ? "X" : " ";
                mazescramble[(3 * i) + 1][3 * j] = wallpresent[scramble[(i * 6) + j]][3] ? "X" : " ";
            }      
        correct[1][ordering[source] / 6][ordering[source] % 6] = true;
        while (!Enumerable.Range(0, 36).All(x => correct[0][x / 6][x % 6] == correct[1][x / 6][x % 6]))
        {
            for (int i = 0; i < 36; i++)
                correct[0][i / 6][i % 6] = correct[1][i / 6][i % 6];
            for (int i = 0; i < 36; i++)
            {
                int k = ordering[i];
                if(!correct[1][k / 6][k % 6])
                {
                    if (k % 6 > 0 && correct[1][k / 6][(k % 6) - 1] && mazescramble[(3 * (k / 6)) + 1][3 * (k % 6)] != "X" && mazescramble[(3 * (k / 6)) + 1][(3 * (k % 6)) - 1] != "X")
                        c[k] = true;
                    else if (k / 6 > 0 && correct[1][(k / 6) - 1][k % 6] && mazescramble[3 * (k / 6)][(3 * (k % 6)) + 1] != "X" && mazescramble[(3 * (k / 6)) - 1][(3 * (k % 6)) + 1] != "X")
                        c[k] = true;
                    else if (k % 6 < 5 && correct[1][k / 6][(k % 6) + 1] && mazescramble[(3 * (k / 6)) + 1][(3 * (k % 6)) + 2] != "X" && mazescramble[(3 * (k / 6)) + 1][(3 * (k % 6)) + 3] != "X")
                        c[k] = true;
                    else if (k / 6 < 5 && correct[1][(k / 6) + 1][k % 6] && mazescramble[(3 * (k / 6)) + 2][(3 * (k % 6)) + 1] != "X" && mazescramble[(3 * (k / 6)) + 3][(3 * (k % 6)) + 1] != "X")
                        c[k] = true;
                }
            }
            for(int i = 0; i < 36; i++)
            {
                int k = ordering[i];
                if(!correct[1][k / 6][k % 6] && c[k])
                {
                    correct[1][k / 6][k % 6] = true;
                    traverse[i].material = mats[2];
                }
            }
        }
        return !correct[1].Any(x => x.Contains(false));
    }

    private IEnumerator SolveAnim()
    {
        bool[][][] correct = new bool[2][][] { new bool[6][] { new bool[6], new bool[6], new bool[6], new bool[6], new bool[6], new bool[6] }, new bool[6][] { new bool[6], new bool[6], new bool[6], new bool[6], new bool[6], new bool[6] } };
        bool[] c = new bool[36];
        int[] scramble = Enumerable.Range(0, 36).Select(x => Array.IndexOf(ordering, x)).ToArray();
        string[][] mazescramble = new string[18][] { new string[18], new string[18], new string[18], new string[18], new string[18], new string[18], new string[18], new string[18], new string[18], new string[18], new string[18], new string[18], new string[18], new string[18], new string[18], new string[18], new string[18], new string[18] };
        for (int i = 0; i < 6; i++)
            for (int j = 0; j < 6; j++)
            {
                mazescramble[3 * i][3 * j] = "X";
                mazescramble[3 * i][(3 * j) + 2] = "X";
                mazescramble[(3 * i) + 2][3 * j] = "X";
                mazescramble[(3 * i) + 2][(3 * j) + 2] = "X";
                mazescramble[(3 * i) + 1][(3 * j) + 1] = " ";
                mazescramble[3 * i][(3 * j) + 1] = wallpresent[scramble[(i * 6) + j]][0] ? "X" : " ";
                mazescramble[(3 * i) + 1][(3 * j) + 2] = wallpresent[scramble[(i * 6) + j]][1] ? "X" : " ";
                mazescramble[(3 * i) + 2][(3 * j) + 1] = wallpresent[scramble[(i * 6) + j]][2] ? "X" : " ";
                mazescramble[(3 * i) + 1][3 * j] = wallpresent[scramble[(i * 6) + j]][3] ? "X" : " ";
            }
        correct[1][ordering[source] / 6][ordering[source] % 6] = true;
        while (!Enumerable.Range(0, 36).All(x => correct[0][x / 6][x % 6] == correct[1][x / 6][x % 6]))
        {
            yield return new WaitForSeconds(0.25f);
            for (int i = 0; i < 36; i++)
                correct[0][i / 6][i % 6] = correct[1][i / 6][i % 6];
            for (int i = 0; i < 36; i++)
            {
                int k = ordering[i];
                if (!correct[1][k / 6][k % 6])
                {
                    if (k % 6 > 0 && correct[1][k / 6][(k % 6) - 1] && mazescramble[(3 * (k / 6)) + 1][3 * (k % 6)] != "X" && mazescramble[(3 * (k / 6)) + 1][(3 * (k % 6)) - 1] != "X")
                        c[k] = true;
                    else if (k / 6 > 0 && correct[1][(k / 6) - 1][k % 6] && mazescramble[3 * (k / 6)][(3 * (k % 6)) + 1] != "X" && mazescramble[(3 * (k / 6)) - 1][(3 * (k % 6)) + 1] != "X")
                        c[k] = true;
                    else if (k % 6 < 5 && correct[1][k / 6][(k % 6) + 1] && mazescramble[(3 * (k / 6)) + 1][(3 * (k % 6)) + 2] != "X" && mazescramble[(3 * (k / 6)) + 1][(3 * (k % 6)) + 3] != "X")
                        c[k] = true;
                    else if (k / 6 < 5 && correct[1][(k / 6) + 1][k % 6] && mazescramble[(3 * (k / 6)) + 2][(3 * (k % 6)) + 1] != "X" && mazescramble[(3 * (k / 6)) + 3][(3 * (k % 6)) + 1] != "X")
                        c[k] = true;
                }
            }
            for (int i = 0; i < 36; i++)
            {
                int k = ordering[i];
                if (!correct[1][k / 6][k % 6] && c[k])
                {
                    correct[1][k / 6][k % 6] = true;
                    traverse[i].material = mats[3];
                }
            }
        }
    }

    //twitch plays
#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use !{0} swap [A1-F6] [A1-F6] to swap the tiles on the given coordinates";
#pragma warning restore 414

    string[] CoordinatesL = { "A", "B", "C", "D", "E", "F" };
    string[] CoordinatesN = { "1", "2", "3", "4", "5", "6" };

    IEnumerator ProcessTwitchCommand(string command)
    {
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*swap\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (parameters.Length != 3)
            {
                yield return "sendtochaterror Invalid parameter length. Command ignored.";
                yield break;
            }

            if (parameters[1].Length != 2 || !parameters[1][0].ToString().ToUpper().EqualsAny(CoordinatesL) || !parameters[1][1].ToString().EqualsAny(CoordinatesN) || parameters[2].Length != 2 || !parameters[2][0].ToString().ToUpper().EqualsAny(CoordinatesL) || !parameters[2][1].ToString().EqualsAny(CoordinatesN))
            {
                yield return "sendtochaterror One or more coordinates being sent is not valid. Command ignored.";
                yield break;
            }

            if (parameters[1].ToUpper() == parameters[2].ToUpper())
            {
                yield return "sendtochaterror You can not swap 2 similar coordinates. Command ignored.";
                yield break;
            }

            if (swapping)
            {
                yield return "sendtochaterror The module is performing a swap. Command ignored.";
                yield break;
            }

            buttons[Array.IndexOf(ordering, (Array.IndexOf(CoordinatesN, parameters[1][1].ToString()) * 6) % 36 + Array.IndexOf(CoordinatesL, parameters[1][0].ToString().ToUpper()))].OnInteract();
            yield return new WaitForSecondsRealtime(0.1f);
            buttons[Array.IndexOf(ordering, (Array.IndexOf(CoordinatesN, parameters[2][1].ToString()) * 6) % 36 + Array.IndexOf(CoordinatesL, parameters[2][0].ToString().ToUpper()))].OnInteract();

        }
    }
}