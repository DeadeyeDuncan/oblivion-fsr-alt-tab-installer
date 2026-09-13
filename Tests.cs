using System;
using System.IO;
class Tests {
 static void Assert(bool b,string s){if(!b)throw new Exception(s);}
 static void Main(){try{Run();}catch(Exception e){Console.Error.WriteLine(e);Environment.ExitCode=1;}} static void Run(){
 string k="r.FidelityFX.FI.OverrideSwapChainDX12";
 string original="; keep\r\n[Other]\r\n"+k+"=9\r\n[SystemSettings]\r\nOther=42\r\n"+k+" = 1 ; previous\r\n"+k+"=1\r\n";
 var old=Fix.Capture(original); var changed=Fix.Merge(original,null);
 Assert(changed.Contains("Other=42"),"preserve other settings");
 Assert(changed.Contains("[Other]\r\n"+k+"=9"),"preserve other sections");
 Assert(Fix.Merge(changed,null)==changed,"idempotence");
 var restored=Fix.Merge(changed.Replace("Other=42","Other=77"),old);
 Assert(restored.Contains("Other=77"),"retain later user edit");
 Assert(restored.Contains(k+" = 1 ; previous"),"restore prior formatting");
 Assert(!restored.Contains("vts.ToggleFSR3OnPauseMenu=1"),"remove added key");
 Assert(Fix.Merge("[Other]\nA=1\n",null).Contains("[SystemSettings]\n"),"missing section");
 string dir=Path.Combine(Path.GetTempPath(),"FSR-installer-test-"+Guid.NewGuid());Directory.CreateDirectory(dir);
 string path=Path.Combine(dir,"Engine.ini");File.WriteAllText(path,original,new System.Text.UnicodeEncoding(false,true));
 Fix.Apply(path); byte[] applied=File.ReadAllBytes(path);Assert(applied[0]==255&&applied[1]==254,"preserve UTF16");
 Assert(File.Exists(path+".fsr-alt-tab.original.bak"),"backup exists");Fix.Apply(path);
 File.WriteAllText(path,File.ReadAllText(path).Replace("Other=42","Other=77"),new System.Text.UnicodeEncoding(false,true));
 Fix.Remove(path);Assert(File.ReadAllText(path).Contains("Other=77"),"disk remove retains later edit");
 Assert(!File.Exists(path+".fsr-alt-tab.state.xml"),"remove clears active state");
 Fix.Apply(path);File.WriteAllText(path,File.ReadAllText(path).Replace("vts.ToggleFSR3OnPauseMenu=1","vts.ToggleFSR3OnPauseMenu=0"));
 bool refused=false;try{Fix.Remove(path);}catch(InvalidOperationException){refused=true;}Assert(refused,"refuse overwriting changed target");
 Console.WriteLine("PASS: merge, duplicate keys, other sections, idempotence, later edits, rollback, encoding, backup and conflict refusal. Fixtures: "+dir);
 }
}
