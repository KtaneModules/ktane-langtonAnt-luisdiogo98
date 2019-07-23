using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
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
}
