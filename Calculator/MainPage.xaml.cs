﻿namespace Calculator;

public partial class MainPage : ContentPage
{
	double RetainedInput = 0;
    string Operation = string.Empty;
    const string ADD = "+";
    const string SUB = "-";
    const string MUL = "*";
    const string DIV = "/";

    public MainPage()
	{
		InitializeComponent();
	}

	private void btnBack_Clicked(object sender, EventArgs e)
	{
        int length = entryResult.Text.Length;
        string text;

        if (length > 1)
        {
            text = entryResult.Text.Remove(length - 1, 1);
        }
        else
        {
            text = "0";
        }

        entryResult.Text = text;
    }

	private void btnClear_Clicked(object sender, EventArgs e)
	{
		entryCalculation.Text = string.Empty;
		entryResult.Text = "0";
        RetainedInput = 0;
        Operation = string.Empty;
    }

	private void btn1PerX_Clicked(object sender, EventArgs e)
	{
        entryCalculation.Text = $"1/{entryResult.Text}";
        
        if(double.TryParse(entryResult.Text, out double temp))
        {
            if(temp != 0)
            {
                entryResult.Text = $"{1 / temp}";
            }
            else
            {
                // TODO: pop here a message
            }
        }
    }

    private void btn7_Clicked(object sender, EventArgs e) => AddDigit("7");

    private void btn8_Clicked(object sender, EventArgs e) => AddDigit("8");

    private void btn9_Clicked(object sender, EventArgs e) => AddDigit("9");

    private void btn4_Clicked(object sender, EventArgs e) => AddDigit("4");

    private void btn5_Clicked(object sender, EventArgs e) => AddDigit("5");

    private void btn6_Clicked(object sender, EventArgs e) => AddDigit("6");

    private void btn1_Clicked(object sender, EventArgs e) => AddDigit("1");

    private void btn2_Clicked(object sender, EventArgs e) => AddDigit("2");

    private void btn3_Clicked(object sender, EventArgs e) => AddDigit("3");

    private void btn0_Clicked(object sender, EventArgs e) => AddDigit("0");

    private void btnPlus_Clicked(object sender, EventArgs e)
    {
        BuildOperation("+");
    }

    private void btnDivision_Clicked(object sender, EventArgs e) => BuildOperation("/");

    private void btnMult_Clicked(object sender, EventArgs e) => BuildOperation("x");

    private void btnMinus_Clicked(object sender, EventArgs e) => BuildOperation("-");

    private void btnMore_Clicked(object sender, EventArgs e)
	{

	}

    private void btnDot_Clicked(object sender, EventArgs e)
	{
		if(!entryResult.Text.Contains('.'))
		{
			entryResult.Text += '.';
        }
	}

	private void btnEqual_Clicked(object sender, EventArgs e)
	{
		if(double.TryParse(entryResult.Text, out double result))
		{
			switch(Operation)
            {
                case ADD:
                    entryResult.Text = $"{RetainedInput + result}";

                    if(!entryCalculation.Text.Contains('='))
                    {
                        entryCalculation.Text += $" {result} =";
                    }
                    break;

                case SUB:
                    entryResult.Text = $"{RetainedInput - result}";
                    entryCalculation.Text += $" {result} =";
                    break;

                case MUL:
                    entryResult.Text = $"{RetainedInput * result}";
                    entryCalculation.Text += $" {result} =";
                    break;

                case DIV:
                    if(result != 0)
                    {
                        entryResult.Text = $"{RetainedInput / result}";
                        entryCalculation.Text += $" {result} =";
                    }
                    else
                    {
                        // TODO: pop here a message
                    }
                    break;

                case "":
                    break;
            }

            RetainedInput = 0;
            Operation = string.Empty;
		}

        // the else case happens if user changes operator and presses equal
	}

    private void BuildOperation(string in_operator)
    {
        if (double.TryParse(entryResult.Text, out double temp))
        {
            // this means equal was not pressed and another input is added
            if (Operation != string.Empty)
            {
                switch(Operation)
                {
                    case ADD: entryResult.Text = $"{RetainedInput + temp}"; break;
                    case SUB: entryResult.Text = $"{RetainedInput - temp}"; break;
                    case MUL: entryResult.Text = $"{RetainedInput * temp}"; break;
                    case DIV: entryResult.Text = $"{RetainedInput / temp}"; break;
                }
            }
            else
            {
                RetainedInput = temp;
            }

            entryCalculation.Text = $"{entryResult.Text} {in_operator}";
            entryResult.Text = string.Empty;
        }
        else
        {
            // means user changed mind about operator
            if(entryCalculation.Text.Contains(" "))
            {
                int length = entryCalculation.Text.Length;
                entryCalculation.Text = entryCalculation.Text.Remove(length - 1) + in_operator;
            }
        }

        Operation = in_operator;
    }

	private void AddDigit(string digit)
	{
        if (entryResult.Text == "0")
            entryResult.Text = digit;
        else
            entryResult.Text += digit;
    }
}
