using System.Collections.ObjectModel;
using System;
using System.Management.Automation;
using System.Collections.ObjectModel;
using System.Diagnostics;



namespace WinFormsAppRunPowerShell
{
  public partial class Form1 : Form
  {
    public Form1()
    {
      InitializeComponent();
    }

    private void btnTest_Click(object sender, EventArgs e)
    {


      // Define your PowerShell script
      string script = @"Write-Output 'Hello from PowerShell!'; Start-Sleep -Seconds 5";

      // Escape any special characters in the script
      string escapedScript = script.Replace("\"", "`\"");

      // Create the PowerShell command to run
      string command = $"-NoProfile -Command \"{escapedScript}\"";

      // Start a new PowerShell process
      Process.Start(new ProcessStartInfo
      {
        FileName = "powershell.exe",
        Arguments = command,
        UseShellExecute = true,
        CreateNoWindow = false
      });



    }



  }
}
