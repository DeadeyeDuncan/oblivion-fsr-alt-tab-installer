using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
class App:Form {
 TextBox path=new TextBox(); Label status=new Label();
 [STAThread] static void Main(){Application.EnableVisualStyles();Application.Run(new App());}
 App(){Text="FSR Alt-Tab Workaround - Installer 1.2";ClientSize=new Size(650,330);FormBorderStyle=FormBorderStyle.FixedDialog;MaximizeBox=false;StartPosition=FormStartPosition.CenterScreen;Font=new Font("Segoe UI",10);BackColor=Color.FromArgb(31,30,29);ForeColor=Color.Gainsboro;
 var title=new Label{Text="FSR ALT-TAB WORKAROUND",Bounds=new Rectangle(24,20,610,35),Font=new Font("Segoe UI",17,FontStyle.Bold),ForeColor=Color.FromArgb(220,185,113)};Controls.Add(title);
 Controls.Add(new Label{Text="Oblivion Remastered | Close the game before making changes.\nApplies two settings, with a backup. Restart the game afterward.",Bounds=new Rectangle(25,68,610,48)});
 path.Bounds=new Rectangle(25,130,495,28);Controls.Add(path);Button browse=ButtonAt("Browse...",530,128,95);browse.Click+=(s,e)=>{using(var d=new OpenFileDialog{Filter="Engine.ini|Engine.ini",Title="Select Oblivion Remastered Engine.ini"}){if(d.ShowDialog()==DialogResult.OK){path.Text=d.FileName;RefreshStatus();}}};
 string root=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"My Games","Oblivion Remastered","Saved","Config");var found=new[]{"Windows","WinGDK"}.Select(x=>Path.Combine(root,x,"Engine.ini")).Where(File.Exists).ToArray();if(found.Length==1)path.Text=found[0];
 Button apply=ButtonAt("Apply Fix",25,183,150);apply.Click+=(s,e)=>Run(()=>Fix.Apply(path.Text),"Fix applied. Restart the game. For menu Alt+Tab, recovery occurs when you exit the menu.");
 Button remove=ButtonAt("Remove Fix",190,183,150);remove.Click+=(s,e)=>Run(()=>Fix.Remove(path.Text),"Installer changes removed. Restart the game. Backups were retained.");
 status.Bounds=new Rectangle(25,240,600,70);Controls.Add(status);RefreshStatus();}
 Button ButtonAt(string text,int x,int y,int w){var b=new Button{Text=text,Bounds=new Rectangle(x,y,w,38),BackColor=Color.FromArgb(65,60,51),ForeColor=Color.White,FlatStyle=FlatStyle.Flat};Controls.Add(b);return b;}
 void RefreshStatus(){status.Text=File.Exists(path.Text)?(Fix.Installed(path.Text)?(Fix.Healthy(path.Text)?"Installed. Recovery record available.":"Installed settings have changed. Recovery record preserved."):"Ready. Existing values will be backed up before applying."):"Select Engine.ini using Browse. Run the game once if the file does not exist.";}
 void Run(Action a,string success){try{a();RefreshStatus();MessageBox.Show(success,"FSR Alt-Tab Workaround",MessageBoxButtons.OK,MessageBoxIcon.Information);}catch(Exception e){MessageBox.Show(e.Message,"No changes completed",MessageBoxButtons.OK,MessageBoxIcon.Warning);}}
}
