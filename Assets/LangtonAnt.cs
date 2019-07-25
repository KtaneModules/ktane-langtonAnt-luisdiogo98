using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;
using rnd = UnityEngine.Random;

public class LangtonAnt : MonoBehaviour 
{
	public KMBombInfo bomb;
	public KMAudio Audio;

	public KMSelectable[] squares;
	public KMSelectable[] colorBtns;
	public KMSelectable goBtn;
	public Material[] colors;
	public Material[] ants;
	public bool[] validColors;
	public bool[] rotations;
	public GameObject ant;
	public GameObject currentColorSquare;

	int[][] grid = new int[][] { 
		new int[] {0, 0, 0, 0, 0},
		new int[] {0, 0, 0, 0, 0},
		new int[] {0, 0, 0, 0, 0},
		new int[] {0, 0, 0, 0, 0},
		new int[] {0, 0, 0, 0, 0}
	};

	int currentColor = 0;
	int[][] solution;
	int row;
	int col;
	int rot;
	String moveOrder;

	float submitTime;

	//Logging
	static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

	void Awake()
	{
		moduleId = moduleIdCounter++;
		GetComponent<KMBombModule>().OnActivate += Activate;

		squares[0].OnInteract += delegate () { PressSquare(0); return false; };
		squares[1].OnInteract += delegate () { PressSquare(1); return false; };
		squares[2].OnInteract += delegate () { PressSquare(2); return false; };
		squares[3].OnInteract += delegate () { PressSquare(3); return false; };
		squares[4].OnInteract += delegate () { PressSquare(4); return false; };
		squares[5].OnInteract += delegate () { PressSquare(5); return false; };
		squares[6].OnInteract += delegate () { PressSquare(6); return false; };
		squares[7].OnInteract += delegate () { PressSquare(7); return false; };
		squares[8].OnInteract += delegate () { PressSquare(8); return false; };
		squares[9].OnInteract += delegate () { PressSquare(9); return false; };
		squares[10].OnInteract += delegate () { PressSquare(10); return false; };
		squares[11].OnInteract += delegate () { PressSquare(11); return false; };
		squares[12].OnInteract += delegate () { PressSquare(12); return false; };
		squares[13].OnInteract += delegate () { PressSquare(13); return false; };
		squares[14].OnInteract += delegate () { PressSquare(14); return false; };
		squares[15].OnInteract += delegate () { PressSquare(15); return false; };
		squares[16].OnInteract += delegate () { PressSquare(16); return false; };
		squares[17].OnInteract += delegate () { PressSquare(17); return false; };
		squares[18].OnInteract += delegate () { PressSquare(18); return false; };
		squares[19].OnInteract += delegate () { PressSquare(19); return false; };
		squares[20].OnInteract += delegate () { PressSquare(20); return false; };
		squares[21].OnInteract += delegate () { PressSquare(21); return false; };
		squares[22].OnInteract += delegate () { PressSquare(22); return false; };
		squares[23].OnInteract += delegate () { PressSquare(23); return false; };
		squares[24].OnInteract += delegate () { PressSquare(24); return false; };

		colorBtns[0].OnInteract += delegate () { ChangeColor(0); return false; };
		colorBtns[1].OnInteract += delegate () { ChangeColor(1); return false; };
		colorBtns[2].OnInteract += delegate () { ChangeColor(2); return false; };
		colorBtns[3].OnInteract += delegate () { ChangeColor(3); return false; };
		colorBtns[4].OnInteract += delegate () { ChangeColor(4); return false; };
		colorBtns[5].OnInteract += delegate () { ChangeColor(5); return false; };
		colorBtns[6].OnInteract += delegate () { ChangeColor(6); return false; };
		colorBtns[7].OnInteract += delegate () { ChangeColor(7); return false; };
		colorBtns[8].OnInteract += delegate () { ChangeColor(8); return false; };
		colorBtns[9].OnInteract += delegate () { ChangeColor(9); return false; };
		colorBtns[10].OnInteract += delegate () { ChangeColor(10); return false; };
		colorBtns[11].OnInteract += delegate () { ChangeColor(11); return false; };
		colorBtns[12].OnInteract += delegate () { ChangeColor(12); return false; };

		goBtn.OnInteract += delegate () { HandleSubmit(); return false; };
	}

	void Activate()
	{
		
	}
	
	void PressSquare(int i)
	{
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		squares[i].AddInteractionPunch(.5f);

		if(moduleSolved)
			return;

		grid[i / 5][i % 5] = currentColor;

		squares[i].gameObject.transform.GetComponentInChildren<Renderer>().material = colors[grid[i / 5][i % 5]];
	}

	void ChangeColor(int i)
	{
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		currentColor = i;
		currentColorSquare.transform.GetComponentInChildren<Renderer>().material = colors[currentColor];
	}

	void HandleSubmit()
	{
		GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
		goBtn.AddInteractionPunch(.5f);

		if(moduleSolved)
			return;
			
		CalcColors();
		CalcPath();
		CheckSolution();
	}

	void CalcColors()
	{
		validColors = new bool[] {false, false, false, false, false, false, false, false, false, false, false, false, false};
		rotations = new bool[] {false, false, false, false, false, false, false, false, false, false, false, false, false};

		for(int i = 0; i < validColors.Length; i++)
		{
			validColors[i] = IsColorValid(i);
			if(validColors[i])
			{
				rotations[i] = SetRotation(i);
       			Debug.LogFormat("[Langton's Ant #{0}] {1} is valid. Associated rotation is {2}.", moduleId, GetColorName(i), rotations[i] ? "CCW" : "CW");
			}
		}
	}

	bool IsColorValid(int n)
	{
		switch(n)
		{
			case 0: return false;
			case 1: return true;
			case 2: return IsPrime(bomb.GetSolvedModuleNames().Count());
			case 3: return bomb.GetPortPlateCount() >= 3;
			case 4: return bomb.GetPortPlates().Any((x) => x.Length == 0);
			case 5: return bomb.IsIndicatorPresent(Indicator.FRK);
			case 6: return bomb.GetSerialNumberNumbers().ToList().FindAll(x => x % 2 == 1).Count() == bomb.GetSerialNumberNumbers().ToList().FindAll(x => x % 2 == 0).Count();
			case 7: return bomb.GetSerialNumberLetters().ToList().FindAll(x => new char[] {'A', 'E', 'I', 'O', 'U'}.Contains(x)).Count() >= 2;
			case 8: return IsPrime(bomb.GetSolvableModuleNames().Count() - bomb.GetSolvedModuleNames().Count());
			case 9: return bomb.GetPortCount() == bomb.GetBatteryCount() || bomb.GetPortCount() == bomb.GetIndicators().Count();
			case 10: return bomb.GetSolvedModuleNames().Count() <= 5;
			case 11: return bomb.IsIndicatorPresent(Indicator.BOB);
			case 12: return true;
		}

		return false;
	}

	bool SetRotation(int n)
	{
		switch(n)
		{
			case 0: return false;
			case 1: return bomb.GetBatteryCount() == 1;
			case 2: return bomb.GetBatteryCount() < 5;
			case 3: return !(bomb.IsPortPresent(Port.AC) || bomb.IsPortPresent(Port.ComponentVideo) || bomb.IsPortPresent(Port.CompositeVideo) || bomb.IsPortPresent(Port.HDMI) || bomb.IsPortPresent(Port.PCMCIA) || bomb.IsPortPresent(Port.USB));
			case 4: return bomb.GetPortPlateCount() % 2 == 1;
			case 5: return bomb.GetIndicators().Count() < 3;
			case 6: return bomb.GetBatteryHolderCount() % 2 == 1;
			case 7: return bomb.GetBatteryCount() % 2 == 1;
			case 8: return bomb.IsIndicatorPresent(Indicator.NLL);
			case 9: return bomb.GetSerialNumberNumbers().Count() >= 3;
			case 10: return bomb.GetIndicators().Count() == 3;
			case 11: return bomb.IsPortPresent(Port.DVI);
			case 12: return !rotations.ToList().Exists(x => x);
		}

		return false;
	}

	bool IsPrime(int n)
	{
		if(n <= 1)
			return false;

		for(int i = n - 1; i > 1; i--)
			if(n % i == 0)
				return false;

		return true;
	}

	void CalcPath()
	{
		row = 2;
		col = 2;
		rot = 2;
		moveOrder = "";

		solution = new int[][] { 
			new int[] {0, 0, 0, 0, 0},
			new int[] {0, 0, 0, 0, 0},
			new int[] {0, 0, 0, 0, 0},
			new int[] {0, 0, 0, 0, 0},
			new int[] {0, 0, 0, 0, 0}
		};

		while(true)
		{
			SetSquareColor();
			SetRotation();
			if(Move()) break;
		}

		Debug.LogFormat("[Langton's Ant #{0}] Ant's path: [ {1}].", moduleId, moveOrder);
		Debug.LogFormat("[Langton's Ant #{0}] Expected solution: [{1}].", moduleId, GetSolutionString());
	}

	void SetSquareColor()
	{
		if(solution[row][col] == 12)
		{
			solution[row][col] = 1;
		}
		else
		{
			for(int i = solution[row][col] + 1; i <= 12; i++)
			{
				if(validColors[i])
				{
					solution[row][col] = i;
					break;
				}
			}
		}

		moveOrder += GetColName(col) + (row + 1) + "(" + GetColorName(solution[row][col]) + ") ";
	}

	void SetRotation()
	{
		if(rotations[solution[row][col]])
			rot--;
		else
			rot++;

		if(rot < 0) rot = 3;
		if(rot > 3) rot = 0;
	}

	bool Move()
	{
		switch(rot)
		{
			case 0: row++; break;
			case 1: col--; break;
			case 2: row--; break;
			case 3: col++; break;
		}

		if(row < 0 || row > 4 || col < 0 || col > 4)
			return true;
		
		return false;
	}

	void CheckSolution()
	{
		for(int i = 0; i < 5; i++)
			for(int j = 0; j < 5; j++)
				if(grid[i][j] != solution[i][j])
				{
					Debug.LogFormat("[Langton's Ant #{0}] Strike! Expected {1}{2} to be {3}. Instead, was {4}.", moduleId, GetColName(j), i + 1, GetColorName(solution[i][j]), GetColorName(grid[i][j]));
					GetComponent<KMBombModule>().HandleStrike();
					StartCoroutine("ResetModule");
					StartCoroutine("FlashStrike");
					return;
				}

		Debug.LogFormat("[Langton's Ant #{0}] Grid matches solution. Module solved.", moduleId);
		moduleSolved = true;
		GetComponent<KMBombModule>().HandlePass();
		StartCoroutine("ShowSolution");
	}

	String GetColorName(int color)
	{
		switch(color)
		{
			case 0: return "unpainted";
			case 1: return "Red";
			case 2: return "Lime";
			case 3: return "White";
			case 4: return "Yellow";
			case 5: return "Blue";
			case 6: return "Brown";
			case 7: return "Forest";
			case 8: return "Orange";
			case 9: return "Black";
			case 10: return "Cyan";
			case 11: return "Magenta";
			case 12: return "Purple";
		}

		return "";
	}

	String GetColName(int c)
	{
		switch(c)
		{
			case 0: return "A";
			case 1: return "B";
			case 2: return "C";
			case 3: return "D";
			case 4: return "E";
		}

		return "";
	}

	String GetSolutionString()
	{
		String ret = "";

		for(int i = 0; i < 5; i++)
		{
			for(int j = 0; j < 5; j++)
				ret += solution[i][j] + "/";
		}

		return ret;
	}

	IEnumerator ResetModule()
	{
		int[] priority = Enumerable.Range(0, 25).ToArray().OrderBy(x => rnd.Range(0, 1000)).ToArray();

		for(int i = 0; i < priority.Length; i++)
		{
			if(grid[i / 5][i % 5] != 0)
			{
				squares[i].transform.GetComponentInChildren<Renderer>().material = colors[0];
				yield return new WaitForSeconds(0.01f);
			}
		}

		grid = new int[][] { 
			new int[] {0, 0, 0, 0, 0},
			new int[] {0, 0, 0, 0, 0},
			new int[] {0, 0, 0, 0, 0},
			new int[] {0, 0, 0, 0, 0},
			new int[] {0, 0, 0, 0, 0}
		};
	}

	IEnumerator FlashStrike()
	{
		ant.transform.GetComponentInChildren<Renderer>().material = ants[2];
		yield return new WaitForSeconds(1f);
		ant.transform.GetComponentInChildren<Renderer>().material = ants[0];
	}

	IEnumerator ShowSolution()
	{
		row = 2;
		col = 2;
		rot = 2;
		moveOrder = "";

		solution = new int[][] { 
			new int[] {0, 0, 0, 0, 0},
			new int[] {0, 0, 0, 0, 0},
			new int[] {0, 0, 0, 0, 0},
			new int[] {0, 0, 0, 0, 0},
			new int[] {0, 0, 0, 0, 0}
		};

		foreach(KMSelectable square in squares)
			square.gameObject.transform.GetComponentInChildren<Renderer>().material = colors[0];

		while(true)
		{
			SetSquareColor();
			squares[row * 5 + col].GetComponentInChildren<Renderer>().material = colors[solution[row][col]];
			
			SetRotation();
			int rotDir = rotations[solution[row][col]] ? -1 : 1;
			for(int i = 0; i < 10; i++)
			{
				ant.transform.localEulerAngles += new Vector3(0, 9f * rotDir, 0);
				yield return new WaitForSeconds(0.005f);
			}
			
			if(Move()) break;

			int xDelta = 0;
			int zDelta = 0;

			switch(rot)
			{
				case 0: zDelta = -1; break;
				case 1: xDelta = -1; break;
				case 2: zDelta = 1; break;
				case 3: xDelta = 1; break;
			}

			Audio.PlaySoundAtTransform("move", transform);

			for(int i = 0; i < 10; i++)
			{
				ant.transform.localPosition += new Vector3(0.003f * xDelta, 0, 0.003f * zDelta);
				yield return new WaitForSeconds(0.005f);
			}
		}

		ant.transform.GetComponentInChildren<Renderer>().material = ants[1];
	}

    //twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} f c1 c3 e5 n col1 k row3 [Presses the specified buttons, with the following key: r=red,l=lime,w=white,y=yellow,b=blue,n=brown,f=forest,o=orange,k=black,c=cyan,m=magenta,p=purple. Entire columns/rows can be pressed as well as singular cells] | !{0} submit [Submits the current color configuration] | !{0} clear [Uncolors all squares]";
    #pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        if (Regex.IsMatch(command, @"^\s*clear\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            colorBtns[0].OnInteract();
            foreach(KMSelectable but in squares)
            {
                yield return new WaitForSeconds(0.1f);
                but.OnInteract();
            }
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*submit\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            goBtn.OnInteract();
            yield break;
        }
        string[] parameters = command.Split(' ');
        var buttonsToPress = new List<KMSelectable>();
        foreach (string param in parameters)
        {
            if (param.EqualsIgnoreCase("r") || param.EqualsIgnoreCase("red"))
                buttonsToPress.Add(colorBtns[1]);
            else if (param.EqualsIgnoreCase("l") || param.EqualsIgnoreCase("lime"))
                buttonsToPress.Add(colorBtns[2]);
            else if (param.EqualsIgnoreCase("w") || param.EqualsIgnoreCase("white"))
                buttonsToPress.Add(colorBtns[3]);
            else if (param.EqualsIgnoreCase("y") || param.EqualsIgnoreCase("yellow"))
                buttonsToPress.Add(colorBtns[4]);
            else if (param.EqualsIgnoreCase("b") || param.EqualsIgnoreCase("blue"))
                buttonsToPress.Add(colorBtns[5]);
            else if (param.EqualsIgnoreCase("n") || param.EqualsIgnoreCase("brown"))
                buttonsToPress.Add(colorBtns[6]);
            else if (param.EqualsIgnoreCase("f") || param.EqualsIgnoreCase("forest"))
                buttonsToPress.Add(colorBtns[7]);
            else if (param.EqualsIgnoreCase("o") || param.EqualsIgnoreCase("orange"))
                buttonsToPress.Add(colorBtns[8]);
            else if (param.EqualsIgnoreCase("k") || param.EqualsIgnoreCase("black"))
                buttonsToPress.Add(colorBtns[9]);
            else if (param.EqualsIgnoreCase("c") || param.EqualsIgnoreCase("cyan"))
                buttonsToPress.Add(colorBtns[10]);
            else if (param.EqualsIgnoreCase("m") || param.EqualsIgnoreCase("magenta"))
                buttonsToPress.Add(colorBtns[11]);
            else if (param.EqualsIgnoreCase("p") || param.EqualsIgnoreCase("purple"))
                buttonsToPress.Add(colorBtns[12]);
            else if (param.EqualsIgnoreCase("a1"))
                buttonsToPress.Add(squares[0]);
            else if (param.EqualsIgnoreCase("a2"))
                buttonsToPress.Add(squares[5]);
            else if (param.EqualsIgnoreCase("a3"))
                buttonsToPress.Add(squares[10]);
            else if (param.EqualsIgnoreCase("a4"))
                buttonsToPress.Add(squares[15]);
            else if (param.EqualsIgnoreCase("a5"))
                buttonsToPress.Add(squares[20]);
            else if (param.EqualsIgnoreCase("b1"))
                buttonsToPress.Add(squares[1]);
            else if (param.EqualsIgnoreCase("b2"))
                buttonsToPress.Add(squares[6]);
            else if (param.EqualsIgnoreCase("b3"))
                buttonsToPress.Add(squares[11]);
            else if (param.EqualsIgnoreCase("b4"))
                buttonsToPress.Add(squares[16]);
            else if (param.EqualsIgnoreCase("b5"))
                buttonsToPress.Add(squares[21]);
            else if (param.EqualsIgnoreCase("c1"))
                buttonsToPress.Add(squares[2]);
            else if (param.EqualsIgnoreCase("c2"))
                buttonsToPress.Add(squares[7]);
            else if (param.EqualsIgnoreCase("c3"))
                buttonsToPress.Add(squares[12]);
            else if (param.EqualsIgnoreCase("c4"))
                buttonsToPress.Add(squares[17]);
            else if (param.EqualsIgnoreCase("c5"))
                buttonsToPress.Add(squares[22]);
            else if (param.EqualsIgnoreCase("d1"))
                buttonsToPress.Add(squares[3]);
            else if (param.EqualsIgnoreCase("d2"))
                buttonsToPress.Add(squares[8]);
            else if (param.EqualsIgnoreCase("d3"))
                buttonsToPress.Add(squares[13]);
            else if (param.EqualsIgnoreCase("d4"))
                buttonsToPress.Add(squares[18]);
            else if (param.EqualsIgnoreCase("d5"))
                buttonsToPress.Add(squares[23]);
            else if (param.EqualsIgnoreCase("e1"))
                buttonsToPress.Add(squares[4]);
            else if (param.EqualsIgnoreCase("e2"))
                buttonsToPress.Add(squares[9]);
            else if (param.EqualsIgnoreCase("e3"))
                buttonsToPress.Add(squares[14]);
            else if (param.EqualsIgnoreCase("e4"))
                buttonsToPress.Add(squares[19]);
            else if (param.EqualsIgnoreCase("e5"))
                buttonsToPress.Add(squares[24]);
            else if (param.EqualsIgnoreCase("row1")) {
                buttonsToPress.Add(squares[0]);
                buttonsToPress.Add(squares[1]);
                buttonsToPress.Add(squares[2]);
                buttonsToPress.Add(squares[3]);
                buttonsToPress.Add(squares[4]);
            }
            else if (param.EqualsIgnoreCase("row2"))
            {
                buttonsToPress.Add(squares[5]);
                buttonsToPress.Add(squares[6]);
                buttonsToPress.Add(squares[7]);
                buttonsToPress.Add(squares[8]);
                buttonsToPress.Add(squares[9]);
            }
            else if (param.EqualsIgnoreCase("row3"))
            {
                buttonsToPress.Add(squares[10]);
                buttonsToPress.Add(squares[11]);
                buttonsToPress.Add(squares[12]);
                buttonsToPress.Add(squares[13]);
                buttonsToPress.Add(squares[14]);
            }
            else if (param.EqualsIgnoreCase("row4"))
            {
                buttonsToPress.Add(squares[15]);
                buttonsToPress.Add(squares[16]);
                buttonsToPress.Add(squares[17]);
                buttonsToPress.Add(squares[18]);
                buttonsToPress.Add(squares[19]);
            }
            else if (param.EqualsIgnoreCase("row5"))
            {
                buttonsToPress.Add(squares[20]);
                buttonsToPress.Add(squares[21]);
                buttonsToPress.Add(squares[22]);
                buttonsToPress.Add(squares[23]);
                buttonsToPress.Add(squares[24]);
            }
            else if (param.EqualsIgnoreCase("col1"))
            {
                buttonsToPress.Add(squares[0]);
                buttonsToPress.Add(squares[5]);
                buttonsToPress.Add(squares[10]);
                buttonsToPress.Add(squares[15]);
                buttonsToPress.Add(squares[20]);
            }
            else if (param.EqualsIgnoreCase("col2"))
            {
                buttonsToPress.Add(squares[1]);
                buttonsToPress.Add(squares[6]);
                buttonsToPress.Add(squares[11]);
                buttonsToPress.Add(squares[16]);
                buttonsToPress.Add(squares[21]);
            }
            else if (param.EqualsIgnoreCase("col3"))
            {
                buttonsToPress.Add(squares[2]);
                buttonsToPress.Add(squares[7]);
                buttonsToPress.Add(squares[12]);
                buttonsToPress.Add(squares[17]);
                buttonsToPress.Add(squares[22]);
            }
            else if (param.EqualsIgnoreCase("col4"))
            {
                buttonsToPress.Add(squares[3]);
                buttonsToPress.Add(squares[8]);
                buttonsToPress.Add(squares[13]);
                buttonsToPress.Add(squares[18]);
                buttonsToPress.Add(squares[23]);
            }
            else if (param.EqualsIgnoreCase("col5"))
            {
                buttonsToPress.Add(squares[4]);
                buttonsToPress.Add(squares[9]);
                buttonsToPress.Add(squares[14]);
                buttonsToPress.Add(squares[19]);
                buttonsToPress.Add(squares[24]);
            }
            else
                yield break;
        }

        yield return null;
        yield return buttonsToPress;
    }
}
