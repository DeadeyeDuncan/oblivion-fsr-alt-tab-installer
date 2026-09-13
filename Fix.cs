using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Diagnostics;
public static class Fix {
 public static readonly string[] Keys={"r.FidelityFX.FI.OverrideSwapChainDX12","vts.ToggleFSR3OnPauseMenu"};
 static string[] Lines(string s){return s.Replace("\r\n","\n").Split('\n');}
 static bool Section(string s){return s.TrimStart().StartsWith("[");}
 static bool TargetSection(string s){return s.Trim().Equals("[SystemSettings]",StringComparison.OrdinalIgnoreCase);}
 static int Key(string s){int p=s.IndexOf('=');return p<0?-1:Array.FindIndex(Keys,k=>k.Equals(s.Substring(0,p).Trim(),StringComparison.OrdinalIgnoreCase));}
 public static List<string> Capture(string s){bool active=false;var r=new List<string>();foreach(string l in Lines(s)){if(Section(l))active=TargetSection(l);else if(active&&Key(l)>=0)r.Add(l);}return r;}
 public static string Merge(string s,List<string> restore){
 string nl=s.Contains("\r\n")?"\r\n":"\n";var output=new List<string>();bool active=false,inserted=false;
 var add=restore??new List<string>{Keys[0]+"=0",Keys[1]+"=1"};
 foreach(string l in Lines(s)){
 if(Section(l)){active=TargetSection(l);output.Add(l);if(active&&!inserted){output.AddRange(add);inserted=true;}}
 else if(!(active&&Key(l)>=0))output.Add(l);
 }
 if(!inserted&&add.Count>0){if(output.Count>0&&output[output.Count-1]!="")output.Add("");output.Add("[SystemSettings]");output.AddRange(add);output.Add("");}
 return string.Join(nl,output);
 }
 static Encoding EncodingOf(byte[] b){
 if(b.Length>=4&&b[0]==255&&b[1]==254&&b[2]==0&&b[3]==0)throw new InvalidOperationException("UTF-32 configuration is not supported.");
 if(b.Length>=2&&b[0]==255&&b[1]==254)return new UnicodeEncoding(false,true,true);
 if(b.Length>=2&&b[0]==254&&b[1]==255)return new UnicodeEncoding(true,true,true);
 if(b.Length>=3&&b[0]==239&&b[1]==187&&b[2]==191)return new UTF8Encoding(true,true);
 try{new UTF8Encoding(false,true).GetString(b);return new UTF8Encoding(false,true);}catch(DecoderFallbackException){return Encoding.Default;}
 }
 static string Decode(byte[] b,Encoding e){int skip=e.GetPreamble().Length;return e.GetString(b,skip,b.Length-skip);}
 static void Check(string p){if(!File.Exists(p)||!Path.GetFileName(p).Equals("Engine.ini",StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Select an existing Engine.ini. Run the game once first if it is missing.");
 if((File.GetAttributes(p)&FileAttributes.ReadOnly)!=0)throw new InvalidOperationException("Engine.ini is read-only. Remove that attribute before applying or removing the fix.");

#if !TEST
 if(Process.GetProcesses().Any(x=>{try{return x.ProcessName.StartsWith("Oblivion",StringComparison.OrdinalIgnoreCase);}catch{return false;}}))throw new InvalidOperationException("Close Oblivion Remastered first.");
#endif
 }
 static void Write(string p,string s,Encoding e){string tmp=p+".fsr-"+Guid.NewGuid()+".tmp";try{File.WriteAllText(tmp,s,e);File.Replace(tmp,p,null);}finally{if(File.Exists(tmp))File.Delete(tmp);}}
 public static bool Installed(string p){return File.Exists(p+".fsr-alt-tab.state.xml");}
 public static void Apply(string p){Check(p);if(Installed(p)){if(!Healthy(p))throw new InvalidOperationException("Settings changed since installation. Review Engine.ini before reapplying; your original recovery record has been preserved.");return;}
 byte[] bytes=File.ReadAllBytes(p);Encoding enc=EncodingOf(bytes);string before=Decode(bytes,enc);string after=Merge(before,null);
 string backup=p+".fsr-alt-tab.original.bak";if(File.Exists(backup))backup=p+".fsr-alt-tab."+Guid.NewGuid()+".bak";
 File.Copy(p,backup,false);
 var state=new XElement("FSRAltTab",new XAttribute("path",Path.GetFullPath(p)),new XAttribute("backup",backup),Capture(before).Select(l=>new XElement("line",l)));
 state.Save(p+".fsr-alt-tab.state.xml");
 try{Write(p,after,enc);}catch{File.Delete(p+".fsr-alt-tab.state.xml");throw;}
 }
 public static bool Healthy(string p){var actual=Capture(File.ReadAllText(p));return actual.Count==2&&actual.Contains(Keys[0]+"=0")&&actual.Contains(Keys[1]+"=1");}
 public static void Remove(string p){Check(p);if(!Installed(p))throw new InvalidOperationException("No installer recovery record found. Nothing has been changed.");
 var state=XElement.Load(p+".fsr-alt-tab.state.xml");if(!string.Equals((string)state.Attribute("path"),Path.GetFullPath(p),StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("Recovery record belongs to a different file.");
 if(!Healthy(p))throw new InvalidOperationException("One of the two settings has changed since installation. Removal stopped to preserve your changes. The original backup and recovery record remain next to Engine.ini.");
 byte[] bytes=File.ReadAllBytes(p);var enc=EncodingOf(bytes);string restored=Merge(Decode(bytes,enc),state.Elements("line").Select(l=>l.Value).ToList());
 File.Copy(p,p+".fsr-alt-tab.before-removal-"+Guid.NewGuid()+".bak",false);Write(p,restored,enc);File.Delete(p+".fsr-alt-tab.state.xml");}
}
