using System;
using System.Collections.Generic;

internal class JsonMessageSplitter
{
    private string _current;
    private int _goalDepth;
    private LinkedList<string> _queued;
    private LinkedList<string> _queuedWithDepth;

    public JsonMessageSplitter() : this(0)
    {
    }

    public JsonMessageSplitter(int getAtDepth)
    {
        this._queued = new LinkedList<string>();
        this._queuedWithDepth = new LinkedList<string>();
        this._goalDepth = getAtDepth;
    }

    private void _parseData()
    {
        int num4 = 0;
        bool flag = false;
        int num5 = 0;
        int startIndex = 0;
        char[] chArray = this._current.ToCharArray();
        for (int i = 0; i < chArray.Length; i++)
        {
            char c = chArray[i];
            switch (num4)
            {
                case 0:
                    if (c != '{')
                    {
                        if (!char.IsWhiteSpace(c))
                        {
                            return;
                        }
                    }
                    else
                    {
                        num4 = 1;
                        num5++;
                    }
                    break;

                case 1:
                    switch (c)
                    {
                        case '"':
                            num4 = 2;
                            goto Label_0130;

                        case '{':
                            num5++;
                            if (num5 == this._goalDepth)
                            {
                                startIndex = i;
                            }
                            goto Label_0130;
                    }
                    if (c != '}')
                    {
                        break;
                    }
                    num5--;
                    if ((this._goalDepth > 0) && (num5 == this._goalDepth))
                    {
                        this._queuedWithDepth.AddLast(this._current.Substring(startIndex, (i + 1) - startIndex));
                    }
                    if (num5 != 0)
                    {
                        break;
                    }
                    this._queued.AddLast(this._current.Substring(0, i + 1));
                    this._current = this._current.Substring(i + 1);
                    return;

                default:
                    if (((num4 == 2) && !flag) && (c == '"'))
                    {
                        num4 = 1;
                    }
                    break;
            }
        Label_0130:
            flag = !flag && (c == '\\');
        }
    }

    public void clear()
    {
        this._current = null;
        this._queued.Clear();
        this._queuedWithDepth.Clear();
    }

    public void feed(string s)
    {
        this._current = this._current + s;
    }

    public string getNextMessage()
    {
        if (this._current == null)
        {
            return null;
        }
        LinkedList<string> list = (this._goalDepth != 0) ? this._queuedWithDepth : this._queued;
        if (list.Count == 0)
        {
            return null;
        }
        string str = list.First.Value;
        list.RemoveFirst();
        return str;
    }

    public void runParsing()
    {
        if ((this._current != null) && (this._current.Length > 0))
        {
            this._parseData();
        }
    }
}

