using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
namespace Skewworks.Labs
{


    public class SBASIC
    {

        #region Enumerations

        public enum Buttons
        {
            Up = 0,
            Right = 1,
            Down = 2,
            Left = 3,
            A = 4,
            B = 5,
            C = 6,
            None = 99,
        }

        public enum EOS
        {
            Clear = 0,
            Scroll = 1,
        }

        private enum VariableTypes
        {
            INT = 0,
            SINGLE = 1,
            DOUBLE = 2,
            LONG = 3,
            STRING = 4,
        }

        #endregion

        #region Structures

        private struct ForLoop
        {
            public int FirstCmdLine;
            public string VarName;
            public int Step;
            public int Limit;
            public bool DelVarAfter;
            public ForLoop(int FirstCmdLine, string VarName, int Step, int Limit, bool DelVarAfter)
            {
                this.FirstCmdLine = FirstCmdLine;
                this.VarName = VarName;
                this.Step = Step;
                this.Limit = Limit;
                this.DelVarAfter = DelVarAfter;
            }
        }

        private struct Variable
        {
            public VariableTypes VarType;
            public object Value;
            public int ArraySize;
            public Variable(VariableTypes VarType, object Value)
            {
                this.VarType = VarType;
                this.Value = Value;
                this.ArraySize = -1;
            }
            public Variable(VariableTypes VarType, object Value, int ArraySize)
            {
                this.VarType = VarType;
                this.Value = Value;
                this.ArraySize = ArraySize;
            }
        }

        #endregion

        #region Classes

        private sealed class Stack
        {

            #region Variables

            private Hashtable _vars;
            private ArrayList _forloops;
            public int IfCounter;
            public int ReturnToLine;
            public string ReturnValue;
            public Hashtable FileList;

            #endregion

            #region Properties

            public ArrayList ForLoops
            {
                get { return _forloops; }
            }

            public Hashtable Variables
            {
                get { return _vars; }
            }

            #endregion

            #region Constructor

            public Stack()
            {
                _vars = new Hashtable();
                _forloops = new ArrayList();
            }

            #endregion

            #region Public Methods

            public void AddVariable(string Name, Variable v)
            {
                _vars.Add(Name, v);
            }

            public void AddVariable(string Name, VariableTypes Type, string Value)
            {
                Variable v = new Variable();
                v.VarType = Type;

                if (Value == string.Empty && Type != VariableTypes.STRING)
                    Value = "0";

                switch (Type)
                {
                    case VariableTypes.DOUBLE:
                        v.Value = double.Parse(Value);
                        break;
                    case VariableTypes.INT:
                        v.Value = int.Parse(Value);
                        break;
                    case VariableTypes.LONG:
                        v.Value = long.Parse(Value);
                        break;
                    case VariableTypes.SINGLE:
                        v.Value = (Single)double.Parse(Value);
                        break;
                    default:
                        v.Value = Value;
                        break;
                }

                _vars.Add(Name, v);
            }

            #endregion

        }

        #endregion

        #region Events

        public event OnClearScreen ClearScreen;
        protected virtual void OnClearScreen(SBASIC sender)
        {
            if (ClearScreen != null)
                ClearScreen(sender);
        }

        public event OnBackColor BackColor;
        protected virtual void OnBackColor(SBASIC sender, Color color)
        {
            if (BackColor != null)
                BackColor(sender, color);
        }

        public event OnForeColor ForeColor;
        protected virtual void OnForeColor(SBASIC sender, Color color)
        {
            if (ForeColor != null)
                ForeColor(sender, color);
        }

        public event OnInKey InKey;
        protected virtual void OnInKey(SBASIC sender, ref int KeyCode)
        {
            if (InKey != null)
                InKey(sender, ref KeyCode);
        }

        public event OnInput Input;
        protected virtual void OnInput(SBASIC sender, ref string Text)
        {
            if (Input != null)
                Input(sender, ref Text);
        }

        public event OnPrint Print;
        protected virtual void OnPrint(SBASIC sender, string value)
        {
            if (Print != null)
                Print(sender, value);
        }

        #endregion

        #region Variables

        private string[] _lines;
        private int _exitCode;
        private bool _running;

        private Hashtable _lineNumbers, _subs, _funcs;
        private Stack[] _stacks;
        private Stack _localStack;

        #endregion

        #region Public Methods

        public int Run(string Script)
        {
            int l;

            // Check if we're already running
            if (_running)
                throw new Exception("Already running");

            // Update flag & exit code
            _running = true;
            _exitCode = 0;

            // Split lines
            _lines = StrReplace(StrReplace(Script, "\r\n", "\n"), "\r", "\n").Split('\n');

            // Remove Comments
            RemoveComments();

            // Clean blank lines
            RemoveBlanks();

            // Get line numbers
            LoadLineNumbers();

            // Get subroutine & function locations
            LoadSubsAndFuncs();

            // Create Stack
            _localStack = new Stack();

            // Run script
            for (l = 0; l < _lines.Length; l++)
                l = RunLine(_lines[l], l);

            // Update flag
            _running = false;

            // Return exit code
            return _exitCode;
        }

        #endregion

        #region Private Methods

        private void AddVariable(int CurLine, string Expression, VariableTypes VarType)
        {
            int i;
            int arrSize = -1;
            string varName, varVal, tmp;

            // Get name & value
            i = Expression.IndexOf('=');
            if (i < 0)
            {
                varName = Expression.Trim();
                varVal = string.Empty;
            }
            else
            {
                varName = Expression.Substring(0, i).Trim();
                varVal = EvalExpression(CurLine, Expression.Substring(i + 1).Trim());
            }

            // Check for array
            i = varName.IndexOf('[');
            if (i > -1)
            {
                tmp = varName.Substring(i + 1);
                varName = varName.Substring(0, i);
                i = tmp.IndexOf(']');
                arrSize = int.Parse(tmp.Substring(0, i).Trim());

                if (arrSize < 0)
                    throw new Exception("Invalid array size");

                if (varVal != string.Empty)
                    throw new Exception("Cannot assign values to array inside declaration");

                switch (VarType)
                {
                    case VariableTypes.DOUBLE:
                        _localStack.AddVariable(varName, new Variable(VarType, new double[arrSize], arrSize));
                        break;
                    case VariableTypes.INT:
                        _localStack.AddVariable(varName, new Variable(VarType, new int[arrSize], arrSize));
                        break;
                    case VariableTypes.LONG:
                        _localStack.AddVariable(varName, new Variable(VarType, new long[arrSize], arrSize));
                        break;
                    case VariableTypes.SINGLE:
                        _localStack.AddVariable(varName, new Variable(VarType, new Single[arrSize], arrSize));
                        break;
                    case VariableTypes.STRING:
                        _localStack.AddVariable(varName, new Variable(VarType, new string[arrSize], arrSize));
                        break;
                }
            }
            else
                _localStack.AddVariable(varName, VarType, varVal);
        }

        private void AddVariable(int CurLine, string Expression, Stack TargetStack)
        {
            int i;
            int arrSize = -1;
            string varName, varVal, tmp;
            VariableTypes VarType;

            // Get variable type
            i = Expression.IndexOf(' ');
            switch (Expression.Substring(0, i).Trim().ToUpper())
            {
                case "DOUBLE":
                    VarType = VariableTypes.DOUBLE;
                    break;
                case "INT":
                    VarType = VariableTypes.INT;
                    break;
                case "LONG":
                    VarType = VariableTypes.LONG;
                    break;
                case "SINGLE":
                    VarType = VariableTypes.SINGLE;
                    break;
                case "STRING":
                    VarType = VariableTypes.STRING;
                    break;
                default:
                    throw new Exception("Invalid variable declaration");
            }
            Expression = Expression.Substring(i + 1);

            // Get name & value
            i = Expression.IndexOf('=');
            if (i < 0)
            {
                varName = Expression.Trim();
                varVal = string.Empty;
            }
            else
            {
                varName = Expression.Substring(0, i).Trim();
                varVal = EvalExpression(CurLine, Expression.Substring(i + 1).Trim());
            }

            // Check for array
            i = varName.IndexOf('[');
            if (i > -1)
            {
                tmp = varName.Substring(i + 1);
                varName = varName.Substring(0, i);
                i = tmp.IndexOf(']');
                arrSize = int.Parse(tmp.Substring(0, i).Trim());

                if (arrSize < 0)
                    throw new Exception("Invalid array size");

                if (varVal != string.Empty)
                    throw new Exception("Cannot assign values to array inside declaration");

                switch (VarType)
                {
                    case VariableTypes.DOUBLE:
                        TargetStack.AddVariable(varName, new Variable(VarType, new double[arrSize], arrSize));
                        break;
                    case VariableTypes.INT:
                        TargetStack.AddVariable(varName, new Variable(VarType, new int[arrSize], arrSize));
                        break;
                    case VariableTypes.LONG:
                        TargetStack.AddVariable(varName, new Variable(VarType, new long[arrSize], arrSize));
                        break;
                    case VariableTypes.SINGLE:
                        TargetStack.AddVariable(varName, new Variable(VarType, new Single[arrSize], arrSize));
                        break;
                    case VariableTypes.STRING:
                        TargetStack.AddVariable(varName, new Variable(VarType, new string[arrSize], arrSize));
                        break;
                }
            }
            else
                TargetStack.AddVariable(varName, VarType, varVal);
        }

        private void AssignVar(int CurLine, string VarName, string Expression)
        {
            Variable var = (Variable)_localStack.Variables[VarName];
            object o;
            int i;
            int index = -1;

            i = Expression.IndexOf('=');
            if (i < 0)
                throw new Exception("Invalid variable assignment " + VarName + " " + Expression);

            if (Expression.Substring(0, 1) == "[")
                index = int.Parse(EvalExpression(CurLine, Expression.Substring(1, Expression.IndexOf(']') - 1)));

            Expression = Expression.Substring(i + 1).Trim();

            // Check for variable name first
            o = _localStack.Variables[Expression];
            if (o == null)
            {
                // Literal assignment
                switch (var.VarType)
                {
                    case VariableTypes.DOUBLE:
                        try
                        {
                            if (index == -1)
                                var.Value = double.Parse(EvalExpression(CurLine, Expression));
                            else
                                ((double[])var.Value)[index] = double.Parse(EvalExpression(CurLine, Expression));
                        }
                        catch (Exception) { throw new Exception("Invalid variable assignment " + VarName + " " + Expression); }
                        break;
                    case VariableTypes.INT:
                        try
                        {
                            if (index == -1)
                                var.Value = int.Parse(EvalExpression(CurLine, Expression));
                            else
                                ((int[])var.Value)[index] = int.Parse(EvalExpression(CurLine, Expression));
                        }
                        catch (Exception) { throw new Exception("Invalid variable assignment " + VarName + " " + Expression); }
                        break;
                    case VariableTypes.LONG:
                        try
                        {
                            if (index == -1)
                                var.Value = long.Parse(EvalExpression(CurLine, Expression));
                            else
                                ((long[])var.Value)[index] = long.Parse(EvalExpression(CurLine, Expression));
                        }
                        catch (Exception) { throw new Exception("Invalid variable assignment " + VarName + " " + Expression); }
                        break;
                    case VariableTypes.SINGLE:
                        try
                        {
                            if (index == -1)
                                var.Value = (Single)double.Parse(EvalExpression(CurLine, Expression));
                            else
                                ((Single[])var.Value)[index] = (Single)double.Parse(EvalExpression(CurLine, Expression));
                        }
                        catch (Exception) { throw new Exception("Invalid variable assignment " + VarName + " " + Expression); }
                        break;
                    case VariableTypes.STRING:
                        var.Value = "\"" + FixString(CurLine, Expression) + "\"";
                        break;
                }
            }
            else
            {
            }

            _localStack.Variables[VarName] = var;
        }

        private string EvalExpression(int CurLine, string Expression)
        {
            string ex = Expression;
            string s;
            int i, e;
            int idx;
            object o;
            Variable v;

            // Check for Len
            while (true)
            {
                i = ex.ToUpper().IndexOf("LEN(");
                if (i == -1)
                    break;

                e = ex.IndexOf(')', i);

                ex = ex.Substring(0, i) + "LEN{" + ex.Substring(i + 4, e - i - 4) + "}" + ex.Substring(e + 1);
            }

            // Evaluate parenthises first (left to right)
            while (true)
            {
                // Get the first close paren
                // it's easier to find embedded this way
                i = ex.IndexOf(')');
                if (i == -1)
                    break;

                // Separate part
                s = ex.Substring(0, i);

                // Cull expression
                e = ex.LastIndexOf('(');
                if (e == -1)
                    throw new Exception("Invalid expression: " + Expression);

                s = EvalExpression(CurLine, s.Substring(e + 1));

                if (e == 0 || ex.Substring(e - 1, 1) == "(" || ex.Substring(e - 1, 1) == ")" || ex.Substring(e - 1, 1) == " ")
                    ex = ex.Substring(0, e) + s + ex.Substring(i + 1);
                else
                    ex = ex.Substring(0, e) + "{" + s + "}" + ex.Substring(i + 1);
            }

            // Split
            string[] parts = SplitOperators(ex);

            // Replace Variables
            for (i = 0; i < parts.Length; i++)
            {
                idx = -1;

                // Check for function
                e = parts[i].IndexOf("{");
                if (e > 0)
                    parts[i] = EvalFunction(CurLine, parts[i]);
                else
                {
                    // check for array
                    e = parts[i].IndexOf('[');
                    if (e > 0)
                    {
                        s = parts[i].Substring(e + 1, parts[i].IndexOf(']') - e - 1);
                        idx = int.Parse(EvalExpression(CurLine, s));
                        parts[i] = parts[i].Substring(0, e);
                    }

                    o = _localStack.Variables[parts[i]];
                    if (o != null)
                    {
                        v = (Variable)o;
                        if (idx == -1)
                            parts[i] = v.Value.ToString();
                        else
                        {
                            switch (v.VarType)
                            {
                                case VariableTypes.DOUBLE:
                                    parts[i] = ((double[])v.Value)[idx].ToString();
                                    break;
                                case VariableTypes.INT:
                                    parts[i] = ((int[])v.Value)[idx].ToString();
                                    break;
                                case VariableTypes.LONG:
                                    parts[i] = ((long[])v.Value)[idx].ToString();
                                    break;
                                case VariableTypes.SINGLE:
                                    parts[i] = ((Single[])v.Value)[idx].ToString();
                                    break;
                                case VariableTypes.STRING:
                                    parts[i] = ((string[])v.Value)[idx];
                                    break;
                            }
                        }
                    }
                }
            }

            if (parts.Length > 1)
            {
                parts = PerformExp(parts);
                parts = PerformMulDiv(parts);
                parts = PerformAddSub(parts);
            }

            return parts[0];
        }

        private string EvalFunction(int CurLine, string Expression)
        {
            string sRet = null;

            int i, e;
            string[] fncParams, usrParams;
            string funcName;
            object o;
            int fncStart;

            // Get function name
            i = Expression.IndexOf("{");
            funcName = Expression.Substring(0, i).Trim();
            o = _funcs[funcName];
            if (o == null)
            {
                switch (funcName.ToUpper())
                {
                    case "ASC":
                        return ((int)FixString(CurLine, Expression.Substring(i + 1, Expression.Length - i - 2)).ToCharArray()[1]).ToString();
                    case "CHR":
                        return new string(new char[] { (char)int.Parse(FixString(CurLine, Expression.Substring(i + 1, Expression.Length - i - 2))) });
                    case "INKEY":
                        int key = -1;
                        OnInKey(this, ref key);
                        return key.ToString();
                    case "INPUT":
                        funcName = string.Empty;
                        OnInput(this, ref funcName);
                        return funcName;
                    case "INSTR":
                        return GetInstr(CurLine, Expression.Substring(i + 1, Expression.Length - i - 2)).ToString();
                    case "LCASE":
                        return FixString(CurLine, Expression.Substring(i + 1, Expression.Length - i - 2)).ToLower();
                    case "LEN":
                        return GetLen(CurLine, Expression.Substring(i + 1, Expression.Length - i - 2)).ToString();
                    case "LEFT":
                        return GetLeft(CurLine, FixString(CurLine, Expression.Substring(i + 1, Expression.Length - i - 2)));
                    case "MID":
                        return GetMid(CurLine, FixString(CurLine, Expression.Substring(i + 1, Expression.Length - i - 2)));
                    case "RIGHT":
                        return GetRight(CurLine, FixString(CurLine,Expression.Substring(i + 1, Expression.Length - i - 2)));
                    case "RND":
                        return GenRandom(CurLine, Expression.Substring(i + 1, Expression.Length - i - 2)).ToString();
                    case "SPACE":
                        i = int.Parse(Expression.Substring(i + 1, Expression.Length - i - 2));
                        funcName = string.Empty;
                        for (e = 0; e < i; e++)
                            funcName += " ";
                        return "\"" + funcName + "\"";
                    case "STR":
                        return EvalExpression(CurLine, Expression.Substring(i + 1, Expression.Length - i - 2));
                    case "UCASE":
                        return FixString(CurLine,Expression.Substring(i + 1, Expression.Length - i - 2)).ToUpper();
                    case "VAL":
                        return int.Parse(FixString(CurLine, Expression.Substring(i + 1, Expression.Length - i - 2))).ToString();
                    default:
                        throw new Exception("Invalid function " + funcName);
                }
            }
            fncStart = (int)o;

            // Get all the parameters for the sub
            i = _lines[fncStart].IndexOf('(');
            e = _lines[fncStart].IndexOf(')');
            fncParams = _lines[fncStart].Substring(i + 1, e - i - 1).Split(',');

            // Get all the parameters for the sub
            i = Expression.IndexOf('{');
            e = Expression.IndexOf('}');
            usrParams = Expression.Substring(i + 1, e - i - 1).Split(',');

            if (fncParams.Length != usrParams.Length)
                throw new Exception("Invalid arguments on line " + CurLine);

            // Copy over variables
            Stack ns = new Stack();
            ns.ReturnToLine = CurLine + 1;

            for (i = 0; i < fncParams.Length; i++)
                AddVariable(CurLine, fncParams[i].Trim() + " = " + usrParams[i].Trim(), ns);

            // Backup current stack
            if (_stacks == null)
                _stacks = new Stack[] { _localStack };
            else
            {
                Stack[] tmp = new Stack[_stacks.Length + 1];
                Array.Copy(_stacks, tmp, _stacks.Length);
                tmp[tmp.Length - 1] = _localStack;
                _stacks = tmp;
            }

            // Copy stack
            _localStack = ns;

            // Run Function
            int l;
            for (l = fncStart + 1; l < _lines.Length; l++)
            {
                l = RunLine(_lines[l], l);
                if (l == -1)
                    break;
            }

            // Copy out return value
            sRet = _localStack.ReturnValue;

            // Pop Stack
            PopStack();

            return sRet;
        }

        private bool EvalIF(int CurLine, string value)
        {
            string[] sides = SplitSides(value);
            if (sides.Length != 3)
                throw new Exception("Invalid expression: " + value);

            int v1 = int.Parse(EvalExpression(CurLine, sides[0]));
            int v2 = int.Parse(EvalExpression(CurLine, sides[2]));

            switch (sides[1])
            {
                case ">":
                    if (v1 > v2)
                        return true;
                    else
                        return false;
                case "<":
                    if (v1 < v2)
                        return true;
                    else
                        return false;
                case "==":
                    if (v1 == v2)
                        return true;
                    else
                        return false;
                case ">=":
                    if (v1 >= v2)
                        return true;
                    else
                        return false;
                case "<=":
                    if (v1 <= v2)
                        return true;
                    else
                        return false;
            }

            return false;
        }

        private int ExitSub()
        {
            int retLine = _localStack.ReturnToLine;
            PopStack();
            return retLine;
        }

        private int FindElseEndIf(int start)
        {
            int iIF = _localStack.IfCounter;

            for (int l = start; l < _lines.Length; l++)
            {
                if (_lines[l].Length >= 2 && _lines[l].Substring(0, 2).ToUpper() == "IF")
                    _localStack.IfCounter++;
                else if (_lines[l].Length >= 4 && _lines[l].Substring(0, 4).ToUpper() == "ELSE")
                {
                    if (_localStack.IfCounter == iIF)
                        return l - 1;
                }
                else if (_lines[l].Length >= 6 && _lines[l].Substring(0, 6).ToUpper() == "END IF")
                {
                    if (_localStack.IfCounter == iIF)
                    {
                        _localStack.IfCounter--;
                        return l;
                    }
                    _localStack.IfCounter--;
                }
            }

            throw new Exception("IF without END IF");
        }

        private int FindEndFunc(int start)
        {
            for (int l = start; l < _lines.Length; l++)
                if (_lines[l].ToUpper() == "END FUNCTION")
                    return l;

            throw new Exception("FUNCTION without END FUNCTION");
        }

        private int FindEndSelect(int start)
        {
            for (int l = start; l < _lines.Length; l++)
                if (_lines[l].ToUpper() == "END SELECT")
                    return l;

            throw new Exception("SELECT without END SELECT");
        }

        private int FindEndSub(int start)
        {
            for (int l = start; l < _lines.Length; l++)
                if (_lines[l].ToUpper() == "END SUB")
                    return l;

            throw new Exception("SUB without END SUB");
        }

        private int FindFirstNonCase(int start)
        {
            for (int l = start; l < _lines.Length; l++)
                if (_lines[l].Length < 5 || _lines[l].Substring(0, 5).ToUpper() != "CASE ")
                    return l;

            return _lines.Length;
        }

        private int FindNextLoop(int start)
        {
            int l;
            for (l = start; l < _lines.Length; l++)
            {
                if (_lines[l] == "NEXT" || _lines[l] == "LOOP" || _lines[l] == "END WHILE")
                {
                    _localStack.ForLoops.RemoveAt(_localStack.ForLoops.Count - 1);
                    return l;
                }
            }

            throw new Exception("Could not break from current location; line " + start);
        }

        private string FixString(int CurLine, string value)
        {
            string[] parts = SplitComponents(value, ';');
            string ret = string.Empty;

            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i] != string.Empty)
                {
                    if (IsLiteral(parts[i]))
                        ret += parts[i].Substring(1, parts[i].Length - 2);
                    else
                    {
                        parts[i] = EvalExpression(CurLine, parts[i]);
                        if (IsLiteral(parts[i]))
                            ret += parts[i].Substring(1, parts[i].Length - 2);
                        else
                            ret += parts[i];
                    }
                }
            }

            return ret;
        }

        private string GetLeft(int CurLine, string Expression)
        {
            string[] v = SplitComponents(Expression, ',');
            if (v.Length != 2)
                throw new Exception("Invalid LEFT statement");
            return FixString(CurLine, v[0]).Substring(0, int.Parse(EvalExpression(CurLine, v[1])));
        }

        private int GetLen(int CurLine, string Expression)
        {
            object o = _localStack.Variables[Expression.Trim()];
            if (o == null)
                throw new Exception("Invalid LEN call");

            Variable v = (Variable)o;
            if (v.VarType == VariableTypes.STRING)
                return ((string)v.Value).Length - 2;
            else if (v.ArraySize > -1)
                return v.ArraySize;

            throw new Exception("Invalid LEN call");
        }

        private int GetInstr(int CurLine, string Expression)
        {
            string[] v = SplitComponents(Expression, ',');

            v[0] = FixString(CurLine, v[0]);

            if (v.Length == 2)
                return v[0].IndexOf(FixString(CurLine, v[1]));
            else if (v.Length == 3)
                return v[0].IndexOf(FixString(CurLine, v[1]), int.Parse(EvalExpression(CurLine, v[2])));

            throw new Exception("Ivalid INSTR statement");
        }

        private string GetMid(int CurLine, string Expression)
        {
            string[] v = SplitComponents(Expression, ',');
            if (v.Length != 3)
                throw new Exception("Invalid MID statement");
            string s = FixString(CurLine, v[0]);
            int i = int.Parse(EvalExpression(CurLine, v[1]));
            int e = int.Parse(EvalExpression(CurLine, v[2]));
            return s.Substring(i, e);
        }

        private string GetRight(int CurLine, string Expression)
        {
            string[] v = SplitComponents(Expression, ',');
            if (v.Length != 2)
                throw new Exception("Invalid RIGHT statement");
            string s = FixString(CurLine, v[0]);
            int l = int.Parse(EvalExpression(CurLine, v[1]));
            return s.Substring(s.Length - l);
        }

        private int GenRandom(int CurLine, string Expression)
        {
            Random rnd = new Random();

            if (Expression == string.Empty)
                return rnd.Next();
            else
                return rnd.Next(int.Parse(EvalExpression(CurLine, Expression)));
        }

        private void GoToSub(int CurLine, int SubStart, string Params)
        {
            int i, e;
            string[] subParams, usrParams;

            // Get all the parameters for the sub
            i = _lines[SubStart].IndexOf('(');
            e = _lines[SubStart].IndexOf(')');
            subParams = _lines[SubStart].Substring(i + 1, e - i - 1).Split(',');

            // Get all the parameters for the sub
            i = Params.IndexOf('(');
            e = Params.IndexOf(')');
            usrParams = Params.Substring(i + 1, e - i - 1).Split(',');

            if (subParams.Length != usrParams.Length)
                throw new Exception("Invalid arguments on line " + CurLine);

            // Copy over variables
            Stack ns = new Stack();
            ns.ReturnToLine = CurLine;

            for (i = 0; i < subParams.Length; i++)
                AddVariable(CurLine, subParams[i].Trim() + " = " + usrParams[i].Trim(), ns);

            // Backup current stack
            if (_stacks == null)
                _stacks = new Stack[] { _localStack };
            else
            {
                Stack[] tmp = new Stack[_stacks.Length + 1];
                Array.Copy(_stacks, tmp, _stacks.Length);
                tmp[tmp.Length - 1] = _localStack;
                _stacks = tmp;
            }

            // Copy stack
            _localStack = ns;
        }

        private bool InQuotes(string value, int position)
        {
            int qcount = 0;
            int i;
            int iStart = 0;

            while (true)
            {
                // Find next instance of a quote
                i = value.IndexOf('"', iStart);

                // If not return our value
                if (i < 0 || i >= position)
                    return qcount % 2 != 0;

                // Check if it's a qualified quote
                if (i > 0 && value.Substring(i, 1) != "\\" || i == 0)
                    qcount++;

                iStart = i + 1;
            }
        }

        private bool IsLiteral(string value)
        {
            if (value == null || value.Length < 2)
                return false;
            if (value == string.Empty || value.Substring(0, 1) == "\"" && value.Substring(value.Length - 1) == "\"")
                return true;
            return false;
        }

        private bool IsNumeric(string value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] < '0' || value[i] > '9')
                    return false;
            }

            return true;
        }

        private void LoadLineNumbers()
        {
            int i;
            string s;

            _lineNumbers = new Hashtable();
            
            for (int l = 0; l < _lines.Length; l++)
            {
                i = _lines[l].IndexOf(' ');


                if (i < 0)
                {
                    // No space
                    if (IsNumeric(_lines[l]))
                    {
                        _lineNumbers.Add(_lines[l], l);
                        _lines[l] = string.Empty;
                    }
                }
                else
                {
                    s = _lines[l].Substring(0, i).Trim();
                    if (IsNumeric(s))
                    {
                        _lineNumbers.Add(s, l);
                        _lines[l] = _lines[l].Substring(s.Length).Trim();
                    }

                }

            }
        }

        private void LoadSubsAndFuncs()
        {
            int i;
            string s;

            _subs = new Hashtable();
            _funcs = new Hashtable();

            for (int l = 0; l < _lines.Length; l++)
            {
                if (_lines[l].Length >= 4 && _lines[l].Substring(0, 4).ToUpper() == "SUB ")
                {
                    s = _lines[l].Substring(4);
                    i = s.IndexOf('(');
                    if (i < 0)
                        throw new Exception("Invalid SUB format");
                    s = s.Substring(0, i).Trim();
                    _subs.Add(s, l);
                }
                else if (_lines[l].Length >= 9 && _lines[l].Substring(0, 9).ToUpper() == "FUNCTION ")
                {
                    s = _lines[l].Substring(9);
                    i = s.IndexOf('(');
                    if (i < 0)
                        throw new Exception("Invalid FUNCTION format");
                    s = s.Substring(0, i).Trim();
                    _funcs.Add(s, l);
                }
            }
        }

        private void PopStack()
        {
            _localStack = _stacks[_stacks.Length - 1];

            if (_stacks.Length == 1)
                _stacks = null;
            else
            {
                Stack[] tmp = new Stack[_stacks.Length - 1];
                Array.Copy(_stacks, tmp, _stacks.Length - 1);
                _stacks = tmp;
            }

        }

        private void PrepDoLoop(int CurLine, string value)
        {
            if (value != string.Empty)
                throw new Exception("Invalid DO");
            ForLoop fl = new ForLoop();
            fl.FirstCmdLine = CurLine + 1;
            _localStack.ForLoops.Add(fl);
        }

        private void PrepForLoop(int CurLine, string value)
        {
            ForLoop fl = new ForLoop();
            int i;
            string sStart;

            // Check for STEP
            i = value.ToUpper().LastIndexOf("STEP");
            if (i == -1)
                fl.Step = 1;
            else
            {
                try
                {
                    fl.Step = int.Parse(value.Substring(i + 4).Trim());
                }
                catch (Exception) { throw new Exception("Invalid STEP: " + value); }

                value = value.Substring(0, i).Trim();
            }

            // Separate Start from To
            i = value.ToUpper().IndexOf("TO");
            if (i == -1)
                throw new Exception("Invalid syntax: " + value);
            sStart = value.Substring(0, i).Trim();
            value = value.Substring(i + 2).Trim();

            // Evalute TO
            fl.Limit = int.Parse(EvalExpression(CurLine, value));

            // Separate Variable from Value
            i = sStart.IndexOf('=');
            if (i == -1)
                throw new Exception("Invalid syntax: " + sStart);
            value = sStart.Substring(i + 1).Trim();
            sStart = sStart.Substring(0, i).Trim();

            // Assign variable & value
            fl.VarName = sStart;
            if (_localStack.Variables[sStart] == null)
            {
                _localStack.Variables.Add(sStart, new Variable(VariableTypes.INT, int.Parse(EvalExpression(CurLine, value))));
                fl.DelVarAfter = true;
            }
            else
                _localStack.Variables[sStart] = new Variable(VariableTypes.INT, int.Parse(EvalExpression(CurLine, value)));

            // Set line
            fl.FirstCmdLine = CurLine + 1;

            _localStack.ForLoops.Add(fl);
        }

        private void PrepWhileLoop(int CurLine, string value)
        {
            ForLoop fl = new ForLoop();
            fl.FirstCmdLine = CurLine + 1;
            fl.VarName = value;
            _localStack.ForLoops.Add(fl);
        }

        private int ProcIF(int CurLine, string value)
        {
            // Increment IF counter
            _localStack.IfCounter++;

            if (value.Substring(value.Length - 4).ToUpper() != "THEN")
                throw new Exception("Invalid expression: " + value);
            value = value.Substring(0, value.Length - 4).Trim();

            string[] sides = SplitSides(value);
            if (sides.Length != 3)
                throw new Exception("Invalid expression: " + value);

            int v1 = int.Parse(EvalExpression(CurLine, sides[0]));
            int v2 = int.Parse(EvalExpression(CurLine, sides[2]));

            switch (sides[1])
            {
                case ">":
                    if (v1 > v2)
                        return CurLine;
                    else
                        return FindElseEndIf(CurLine + 1);
                case "<":
                    if (v1 < v2)
                        return CurLine;
                    else
                        return FindElseEndIf(CurLine + 1);
                case "==":
                    if (v1 == v2)
                        return CurLine;
                    else
                        return FindElseEndIf(CurLine + 1);
                case ">=":
                    if (v1 >= v2)
                        return CurLine;
                    else
                        return FindElseEndIf(CurLine + 1);
                case "<=":
                    if (v1 <= v2)
                        return CurLine;
                    else
                        return FindElseEndIf(CurLine + 1);
            }

            return CurLine;
        }

        private int ProcLoopDo(int lineNumber)
        {
            if (_localStack.ForLoops.Count == 0)
                throw new Exception("LOOP without DO");

            ForLoop fl = (ForLoop)_localStack.ForLoops[_localStack.ForLoops.Count - 1];
            return fl.FirstCmdLine - 1;
        }
        
        private int ProcLoopNext(int lineNumber)
        {
            if (_localStack.ForLoops.Count == 0)
                throw new Exception("NEXT without FOR");

            ForLoop fl = (ForLoop)_localStack.ForLoops[_localStack.ForLoops.Count - 1];
            Variable v = (Variable)_localStack.Variables[fl.VarName];
            int val = (int)v.Value + fl.Step;
            bool complete = false;

            v.Value = val;

            // Have we met or exceeded the limit
            if (fl.Step > 0)
            {
                if (val >= fl.Limit)
                    complete = true;
            }
            else
            {
                if (val <= fl.Limit)
                    complete = true;
            }

            if (complete)
            {
                if (fl.DelVarAfter)
                    _localStack.Variables.Remove(fl.VarName);
                else
                    _localStack.Variables[fl.VarName] = v;

                _localStack.ForLoops.Remove(fl);

                return lineNumber;
            }

            _localStack.Variables[fl.VarName] = v;
            return fl.FirstCmdLine - 1;
        }

        private int ProcLoopWhile(int lineNumber)
        {
            if (_localStack.ForLoops.Count == 0)
                throw new Exception("END WHILE without WHILE");

            ForLoop fl = (ForLoop)_localStack.ForLoops[_localStack.ForLoops.Count - 1];
            if (!EvalIF(fl.FirstCmdLine - 1, fl.VarName))
            {
                _localStack.ForLoops.Remove(fl);
                return lineNumber;
            }
            else
                return fl.FirstCmdLine - 1;
        }

        private void ProcScreen(string value)
        {
            string[] pArr = SplitComponents(value, ',');
            try
            {
                OnBackColor(this, Color.FromArgb(byte.Parse(pArr[0].Trim()), byte.Parse(pArr[1].Trim()), byte.Parse(pArr[2].Trim())));
            }
            catch (Exception) { throw new Exception("Invalid arguments for SCREEN: " + value); }
        }

        private int ProcSelectCase(int CurLine, string value)
        {
            int caseEnds = FindEndSelect(CurLine);
            int l;
            string val = EvalExpression(CurLine, value);

            // Check for case matches
            for (l = CurLine + 1; l < caseEnds; l++)
            {
                if (_lines[l].Length > 5 && _lines[l].Substring(0, 5).ToUpper() == "CASE ")
                {
                    if (EvalExpression(CurLine, _lines[l].Substring(5)) == val)
                        return FindFirstNonCase(l + 1) - 1;
                }
            }

            // Check for default catch
            for (l = CurLine + 1; l < caseEnds; l++)
            {
                if (_lines[l].ToUpper() == "DEFAULT")
                    return FindFirstNonCase(l + 1) - 1;
            }

            return caseEnds;
        }

        private void ProcText(string value)
        {
            string[] pArr = SplitComponents(value, ',');
            try
            {
                OnForeColor(this, Color.FromArgb(byte.Parse(pArr[0].Trim()), byte.Parse(pArr[1].Trim()), byte.Parse(pArr[2].Trim())));
            }
            catch (Exception) { throw new Exception("Invalid arguments for TEXT: " + value); }
        }

        private void RemoveBlanks()
        {
            int l;
            string[] tmp1 = null;
            string[] tmp2;
            for (l = 0; l < _lines.Length; l++)
            {
                if (_lines[l] != string.Empty)
                {
                    if (tmp1 == null)
                        tmp1 = new string[] { _lines[l].Trim() };
                    else
                    {
                        tmp2 = new string[tmp1.Length + 1];
                        Array.Copy(tmp1, tmp2, tmp1.Length);
                        tmp2[tmp2.Length - 1] = _lines[l].Trim();
                        tmp1 = tmp2;
                        tmp2 = null;
                    }
                }
            }
            _lines = tmp1;
            tmp1 = null;
            
            //Debug.GC(true);
            GC.Collect();
        }

        private void RemoveComments()
        {
            int i, iStart;

            for (int l =0; l < _lines.Length; l++)
            {
                iStart = 0;
                while (true)
                {
                    i = _lines[l].IndexOf('\'', iStart);
                    if (i < 0)
                        break;
                    else if (!InQuotes(_lines[l], i))
                    {
                        if (i == 0)
                             _lines[l]  = string.Empty;
                        else
                            _lines[l] = _lines[l].Substring(0, i);

                        break;
                    }

                    iStart = i + 1;
                }
            }
        }

        private int RunLine(string value, int lineNumber)
        {
            string sCMD = string.Empty;
            string sParam = string.Empty;
            int i;
            object o;

            if (value == string.Empty)
                return lineNumber;

            // Get Command and Params
            i = value.IndexOf(' ');
            if (i < 0)
                sCMD = value;
            else
            {
                sCMD = value.Substring(0, i).Trim();
                sParam = value.Substring(i).Trim();
            }

            // Check for paren
            i = sCMD.IndexOf('(');
            if (i > -1)
            {
                sParam = sCMD.Substring(i) + sParam;
                sCMD = sCMD.Substring(0, i);
            }

            // Process command
            switch (sCMD.ToUpper())
            {
                case "ASC":         // ASCII code for character
                    break;
                case "BREAK":       // Break
                    return FindNextLoop(lineNumber);
                case "CASE":
                    return FindEndSelect(lineNumber);
                case "CHR":         // Character from code
                    break;
                case "CLS":         // Clear screen
                    OnClearScreen(this);
                    break;
                case "DO":          // Do
                    PrepDoLoop(lineNumber, sParam);
                    break;
                case "DOUBLE":      // Declare double
                    AddVariable(lineNumber, sParam, VariableTypes.DOUBLE);
                    break;
                case "END":         // End program
                    switch (sParam.Trim().ToUpper())
                    {
                        case "IF":
                            _localStack.IfCounter--;
                            break;
                        case "FUNCTION":
                            _localStack.ReturnValue = string.Empty;
                            return -1;
                        case "SUB":
                            return ExitSub();
                        case "WHILE":
                            return ProcLoopWhile(lineNumber);                            
                    }
                    break;
                case "FOR":         // For/Loop
                    PrepForLoop(lineNumber, sParam);
                    break;
                case "FUNCTION":
                    return FindEndFunc(lineNumber + 1);
                case "GOTO":        // The most dangerous code
                    o = _lineNumbers[sParam];
                    if (o == null)
                        throw new Exception("Invalid line number in GOTO: " + sParam);
                    else
                        return (int)o - 1;
                case "GOSUB":       // Go to subroutine
                    sCMD = sParam;
                    sParam = string.Empty;

                    // Check for paren
                    i = sCMD.IndexOf('(');
                    if (i > -1)
                    {
                        sParam = sCMD.Substring(i) + sParam;
                        sCMD = sCMD.Substring(0, i);
                    }

                    o = _subs[sCMD];
                    if (o == null)
                        throw new Exception("Sub " + sCMD + " not found!");

                    GoToSub(lineNumber, (int)o, sParam);
                    return (int)o;
                case "IF":          // If statement
                    return ProcIF(lineNumber, sParam);
                case "INKEY":       // Get key state
                    i = -1;
                    OnInKey(this, ref i);
                    break;
                case "INPUT":       // Get line
                    sParam = string.Empty;
                    OnInput(this, ref sParam);
                    break;
                case "INSTR":       // IndexOf
                    break;
                case "INT":         // Declare integer
                    AddVariable(lineNumber, sParam, VariableTypes.INT);
                    break;
                case "LCASE":       // ToLower
                case "LEFT":        // Left
                case "LEN":         // Length
                    break;
                case "LOOP":        // Loop
                    return ProcLoopDo(lineNumber);
                case "LONG":        // Declare long
                    AddVariable(lineNumber, sParam, VariableTypes.LONG);
                    break;
                case "MID":         // Substring
                    break;
                case "NEXT":        // Next
                    return ProcLoopNext(lineNumber);
                case "PRINT":       // Print to screen
                    OnPrint(this, FixString(lineNumber, sParam));
                    break;
                case "RETURN":      // Return
                    _localStack.ReturnValue = EvalExpression(lineNumber, sParam);
                    return -1;
                case "RIGHT":       // Right
                    break;
                case "RND":         // Random
                    Debug.WriteLine("RND out of place; ignore");
                    break;
                case "SCREEN":      // Change background color
                    ProcScreen(sParam);
                    break;
                case "SELECT":      // Select
                    return ProcSelectCase(lineNumber, sParam);
                case "SINGLE":      // Declare single
                    AddVariable(lineNumber, sParam, VariableTypes.SINGLE);
                    break;
                case "SPACE":       // Spaced string
                case "STR":         // To string
                    break;
                case "STRING":      // Declare string
                    AddVariable(lineNumber, sParam, VariableTypes.STRING);
                    break;
                case "SUB":         // Subroutine start
                    return FindEndSub(lineNumber + 1);
                case "TEXT":        // Change foreground color
                    ProcText(sParam);
                    break;
                case "UCASE":       // ToUpper
                case "VAL":         // To int
                    break;
                case "WHILE":       // While
                    PrepWhileLoop(lineNumber, sParam);
                    break;
                default:            // Variable or Unimplemented command
                    // Check for array
                    i = sCMD.IndexOf('[');
                    if (i > -1)
                    {
                        sParam = sCMD.Substring(i) + sParam;
                        sCMD = sCMD.Substring(0, i);
                    }

                    if (_localStack.Variables[sCMD] == null)
                        Debug.WriteLine("UNKNOWN COMMAND: " + sCMD.ToUpper());
                    else
                        AssignVar(lineNumber, sCMD, sParam.Trim());
                    break;
            }

            return lineNumber;
        }

        private string[] SplitComponents(string value, char deliminator)
        {
            int iStart = 0;
            string[] ret = null;
            string[] tmp;
            int i;
            string s;

            while (true)
            {
                // Find deliminator
                i = value.IndexOf(deliminator, iStart);

                if (InQuotes(value, i))
                    iStart = i + 1;
                else
                {
                    // Separate value
                    if (i < 0)
                        s = value;
                    else
                    {
                        s = value.Substring(0, i).Trim();
                        value = value.Substring(i + 1);
                    }

                    // Add value
                    if (ret == null)
                        ret = new string[] { s };
                    else
                    {
                        tmp = new string[ret.Length + 1];
                        Array.Copy(ret, tmp, ret.Length);
                        tmp[tmp.Length - 1] = s;
                        ret = tmp;
                    }

                    iStart = 0;
                }

                // Break on last value
                if (i < 0)
                    break;
            }

            return ret;
        }

        private string[] SplitOperators(string value)
        {
            // Remove all spaces
            value = StrRemSpaces(value);

            value = StrReplace(value, "^", " ^ ");
            value = StrReplace(value, "+", " + ");
            value = StrReplace(value, "-", " - ");
            value = StrReplace(value, "*", " * ");
            value = StrReplace(value, "%", " % ");
            value = StrReplace(value, "/", " / ");

            return SplitComponents(value, ' ');
        }

        private string[] SplitSides(string value)
        {
            // Remove all spaces
            value = StrRemSpaces(value);

            value = StrReplace(value, "<", " < ");
            value = StrReplace(value, ">", " > ");
            value = StrReplace(value, "==", " == ");
            value = StrReplace(value, ">=", " >= ");
            value = StrReplace(value, "<=", " <= ");

            return SplitComponents(value, ' ');
        }

        private string StrRemSpaces(string Source)
        {
            int i;
            int iStart = 0;

            if (Source == string.Empty || Source == null)
                return Source;

            while (true)
            {
                i = Source.IndexOf(" ", iStart);
                if (i < 0) break;


                if (InQuotes(Source, i))
                    iStart = i + 1;
                else
                {
                    if (i > 0)
                        Source = Source.Substring(0, i) + Source.Substring(i + 1);
                    else
                        Source = Source.Substring(i + 1);

                    iStart = i;
                }
            }

            return Source;
        }

        private string StrReplace(string Source, string ToFind, string ReplaceWith)
        {
            int i;
            int iStart = 0;

            if (Source == string.Empty || Source == null || ToFind == string.Empty || ToFind == null)
                return Source;

            while (true)
            {
                i = Source.IndexOf(ToFind, iStart);
                if (i < 0) break;

                if (i > 0)
                    Source = Source.Substring(0, i) + ReplaceWith + Source.Substring(i + ToFind.Length);
                else
                    Source = ReplaceWith + Source.Substring(i + ToFind.Length);

                iStart = i + ReplaceWith.Length;
            }

            return Source;
        }

        #endregion

        #region Math Methods

        private string[] PerformAddSub(string[] parts)
        {
            int i = 0;
            string[] tmp;

            for (i = 0; i < parts.Length; i++)
            {
                if (parts[i] == "+")
                {
                    if (IsLiteral(parts[i - 1]))
                    {
                        // Update previous item
                        parts[i - 1] = "\"" + FixString(0, parts[i - 1]) + FixString(0, parts[i + 1]) + "\"";
                    }
                    else
                    {
                        // Update previous item
                        parts[i - 1] = (Int32.Parse(parts[i - 1]) + Int32.Parse(parts[i + 1])).ToString();
                    }

                    // Create new array (2 smaller)
                    tmp = new string[parts.Length - 2];

                    // Copy left hand
                    Array.Copy(parts, tmp, i);

                    // Copy right hand
                    if (i + 2 < parts.Length - 1)
                        Array.Copy(parts, i + 2, tmp, i, tmp.Length - i);

                    parts = tmp;

                    // Dec i
                    i--;
                }
                else if (parts[i] == "-")
                {
                    // Update previous item
                    parts[i - 1] = (Int32.Parse(parts[i - 1]) - Int32.Parse(parts[i + 1])).ToString();

                    // Create new array (2 smaller)
                    tmp = new string[parts.Length - 2];

                    // Copy left hand
                    Array.Copy(parts, tmp, i);

                    // Copy right hand
                    if (i + 2 < parts.Length - 1)
                        Array.Copy(parts, i + 2, tmp, i, tmp.Length - i);

                    parts = tmp;

                    // Dec i
                    i--;
                }
            }


            tmp = null;
            return parts;
        }

        private string[] PerformExp(string[] parts)
        {
            int i = 0;
            string[] tmp;

            for (i = 0; i < parts.Length; i++)
            {
                if (parts[i] == "^")
                {
                    // Update previous item
                    parts[i - 1] = (Int32.Parse(parts[i - 1]) ^ Int32.Parse(parts[i + 1])).ToString();

                    // Create new array (2 smaller)
                    tmp = new string[parts.Length - 2];

                    // Copy left hand
                    Array.Copy(parts, tmp, i);

                    // Copy right hand
                    if (i + 2 < parts.Length - 1)
                        Array.Copy(parts, i + 2, tmp, i, tmp.Length - i);

                    parts = tmp;

                    // Dec i
                    i--;
                }
            }

            tmp = null;
            return parts;
        }

        private string[] PerformMulDiv(string[] parts)
        {
            int i = 0;
            string[] tmp;

            for (i = 0; i < parts.Length; i++)
            {
                if (parts[i] == "*")
                {
                    // Update previous item
                    parts[i - 1] = (Int32.Parse(parts[i - 1]) * Int32.Parse(parts[i + 1])).ToString();

                    // Create new array (2 smaller)
                    tmp = new string[parts.Length - 2];

                    // Copy left hand
                    Array.Copy(parts, tmp, i);

                    // Copy right hand
                    if (i + 2 < parts.Length - 1)
                        Array.Copy(parts, i + 2, tmp, i, tmp.Length - i);

                    parts = tmp;

                    // Dec i
                    i--;
                }
                else if (parts[i] == "/")
                {
                    // Update previous item
                    parts[i - 1] = (Int32.Parse(parts[i - 1]) / Int32.Parse(parts[i + 1])).ToString();

                    // Create new array (2 smaller)
                    tmp = new string[parts.Length - 2];

                    // Copy left hand
                    Array.Copy(parts, tmp, i);

                    // Copy right hand
                    if (i + 2 < parts.Length - 1)
                        Array.Copy(parts, i + 2, tmp, i, tmp.Length - i);

                    parts = tmp;

                    // Dec i
                    i--;
                }
            }


            tmp = null;
            return parts;
        }

        #endregion

    }


}
