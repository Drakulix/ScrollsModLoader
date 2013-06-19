// Copyright
// Microsoft Corporation
// All rights reserved

// OpenFileDlg.cs

using System;
using System.Text;
using System.Runtime.InteropServices;

[ StructLayout( LayoutKind.Sequential, CharSet=CharSet.Auto )]  
public class OpenFileName 
{
    public int      structSize = 0;
    public IntPtr   dlgOwner = IntPtr.Zero; 
    public IntPtr   instance = IntPtr.Zero;

    public String   filter = null;
    public String   customFilter = null;
    public int      maxCustFilter = 0;
    public int      filterIndex = 0;

    public String   file = null;
    public int      maxFile = 0;

    public String   fileTitle = null;
    public int      maxFileTitle = 0;

    public String   initialDir = null;

    public String   title = null;   

    public int      flags = 0; 
    public short    fileOffset = 0;
    public short    fileExtension = 0;

    public String   defExt = null; 

    public IntPtr   custData = IntPtr.Zero;  
    public IntPtr   hook = IntPtr.Zero;  

    public String   templateName = null; 

    public IntPtr   reservedPtr = IntPtr.Zero; 
    public int      reservedInt = 0;
    public int      flagsEx = 0;
}

public class LibWrap
{
    [ DllImport( "Comdlg32.dll", CharSet=CharSet.Auto )]                
    public static extern bool GetOpenFileName([ In, Out ] OpenFileName ofn );   
}

public class WindowsDialog
{
    public static string ShowWindowsDialog()
    {
        OpenFileName ofn = new OpenFileName();

        ofn.structSize = Marshal.SizeOf( ofn );

        ofn.filter = "0";

        ofn.file = new String( new char[ 256 ]);
        ofn.maxFile = ofn.file.Length;

        ofn.fileTitle = new String( new char[ 64 ]);
        ofn.maxFileTitle = ofn.fileTitle.Length;    

        ofn.initialDir = "C:\\";
        ofn.title = "Open file";
        ofn.defExt = "";

		LibWrap.GetOpenFileName (ofn);

		Console.WriteLine ("File Selection: "+ofn.file);
		return ofn.file;
    }
}