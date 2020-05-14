using System;
using System.Reflection;
using TR.BIDSsv;
using System.IO;
using TR;
using System.Threading.Tasks;
using System.Linq;

namespace BIDS_Server
{
  class Program
  {
    static bool IsLooping = true;
    const string VerNumStr = "012";
    static void Main(string[] args)
    {
      Console.WriteLine("BIDS Server Application");
      Console.WriteLine("ver : " + VerNumStr);

      Common.Start(1);

      for (int i = 0; i < args.Length; i++) Console.WriteLine("{0}[{1}] :: {2}", "args", i, args[i]);
      if (args?.Length > 0)
      {
        for(int i = 0; i < args.Length; i++)
        {
          if (args[i].StartsWith("/"))
          {
            //do some process
            continue;
          }
          if (args[i].StartsWith("-"))
          {
            //do some process
            continue;
          }

          ReadLineDO(args[i]);
        }
      }

      do ReadLineDO(); while (IsLooping);
      Common.Dispose();

      Console.WriteLine("Please press any key to exit...");
      Console.ReadKey();
    }

    static void ReadLineDO() => ReadLineDO(Console.ReadLine());
    static async void ReadLineDO(string s)
    {
      if (s.EndsWith(".bsvcmd"))
      {
        Console.WriteLine("{0} => BIDS_Server Command preset file", s);
        try
        {
          using (StreamReader sr = new StreamReader(s))
          {
            string[] sa = sr.ReadToEnd().Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            if (sa?.Length > 0)
              for (int pi = 0; pi < sa.Length; pi++)
              {
                Console.WriteLine("do {0}[{1}] : {2}", s, pi, sa[pi]);
                ReadLineDO(sa[pi]);
              }
            else Console.WriteLine("{0} : There is no command in the file.", s);
          }
        } catch (FileNotFoundException){ Console.WriteLine("Program.ReadLineDO : the file \"{0}\" is not found.", s); }
        catch(DirectoryNotFoundException) { Console.WriteLine("Program.ReadLineDO : the directory is not found. (file path:\"{0}\")", s); }
        catch(Exception e)
        {
          Console.WriteLine("Program.ReadLineDO : an exception has occured (file path:\"{0}\")\n{1}", s, e);
        }

        return;
      }

      if (s.EndsWith(".btsetting"))
      {
        Console.WriteLine("{0} => Button assignment setting file to keep the compatibility with GIPI", s);
        using (StreamReader sr = new StreamReader(s))
        {
          GIPI.LoadFromStream(sr);
        }

        return;
      }

      string[] cmd_orig = s?.Split(' ');
      string[] cmd = cmd_orig?.Select((str) => str.ToLower()).ToArray();
      
      switch (cmd[0])
      {
        case "man":
          if (cmd.Length >= 2)
          {
            switch (cmd[1])
            {
              case "auto":
                Console.WriteLine("auto sending process turn on command : auto send will start when this command is entered.");
                Console.WriteLine("  Option : bve5, common, const, handle, obve, panel, sound");
                break;
              case "autodel":
                Console.WriteLine("auto sending process turn off command : auto send will stop when this command is entered.");
                Console.WriteLine("  Option : bve5, common, const, handle, obve, panel, sound");
                break;
              case "exit":
                Console.WriteLine("exit command : Used when user want to close this program.  This command has no arguments.");
                break;
              case "ls":
                Console.WriteLine("connection listing command : Print the list of the Name of alive connection.  This command has no arguments.");
                break;
              case "lsmods":
                Console.WriteLine("mods listing command : Print the list of the Name of mods you can use.  This command has no arguments.");
                break;
              case "close":
                Console.WriteLine("close connection command : Used when user want to close the connection.  User must set the Connection Name to be closed.");
                break;
              case "delay":
                Console.WriteLine("insert pause command : Insert Pause function.  You can set 0 ~ Int.Max[ms]");
                break;
              default:
                IBIDSsv ibsv = LoadMod(FindMod(cmd[1]));
                if (ibsv != null)
                {
                  ibsv.WriteHelp(string.Empty);
                  ibsv.Dispose();
                }
                else Console.WriteLine("Mod not found.");
                break;
            }
          }
          else
          {
            Console.WriteLine("BIDS Server Application");
            Console.WriteLine("ver : " + VerNumStr + "\n");
            Console.WriteLine("auto : Set the Auto Sending Function");
            Console.WriteLine("close: Close the connection.");
            Console.WriteLine("delay : insert delay function.");
            Console.WriteLine("exit : close this program.");
            Console.WriteLine("ls : Print the list of the Name of alive connection.");
            Console.WriteLine("lsmods : Print the list of the Name of mods you can use.");
            Console.WriteLine("man : Print the mannual of command.");
            Console.WriteLine("  If you want to check the manual of each mod, please check the mod name and type \"man + (mod name)\"");
          }
          break;
        case "ls": Console.Write(Common.PrintList()); break;
        case "lsmods":
          try
          {
            string[] fs = LSMod();
            if (fs == null || fs.Length <= 0)
            {
              Console.WriteLine("There are no modules in the mods folder.");
            }
            else
            {
              for (int i = 0; i < fs.Length; i++)
              {
                string[] cn = fs[i].Split('.');
                Console.WriteLine(" {0} : {1}", cn[cn.Length - 2], fs[i]);
              }
            }
          }
          catch (Exception e) { Console.WriteLine(e); }
          break;
        case "auto":
          foreach(string str in cmd)
          {
            if (str == null || str == string.Empty) continue;
            if (str.StartsWith("/") || str.StartsWith("-"))
            {
              switch(str.Remove(0, 1).Substring(0,3))
              {
                case "com": Common.AutoSendSetting.BasicCommonAS = true; Console.WriteLine("Common Data Autosend Enabled"); break;
                case "bve": Common.AutoSendSetting.BasicBVE5AS = true; Console.WriteLine("BVE5 Data Autosend Enabled"); break;
                case "obv": Common.AutoSendSetting.BasicOBVEAS = true; Console.WriteLine("OBVE Data Autosend Enabled"); break;
                case "pan": Common.AutoSendSetting.BasicPanelAS = true; Console.WriteLine("Panel Data Autosend Enabled"); break;
                case "sou": Common.AutoSendSetting.BasicPanelAS = true; Console.WriteLine("Sound Data Autosend Enabled"); break;
                case "con": Common.AutoSendSetting.BasicConstAS = true; Console.WriteLine("Const Data Autosend Enabled"); break;
                case "han": Common.AutoSendSetting.BasicHandleAS = true; Console.WriteLine("Handle Data Autosend Enabled"); break;
                default: Console.WriteLine("Option Not Found : {0}", str); break;
              }
            }
          }
          break;
        case "autodel":
          foreach (string str in cmd)
          {
            if (str == null || str == string.Empty) continue;
            if (str.StartsWith("/") || str.StartsWith("-"))
            {
              switch (str.Remove(0, 1).Substring(0, 3))
              {
                case "com": Common.AutoSendSetting.BasicCommonAS = false; Console.WriteLine("Common Data Autosend Disabled"); break;
                case "bve": Common.AutoSendSetting.BasicBVE5AS = false; Console.WriteLine("BVE5 Data Autosend Disabled"); break;
                case "obv": Common.AutoSendSetting.BasicOBVEAS = false; Console.WriteLine("OBVE Data Autosend Disabled"); break;
                case "pan": Common.AutoSendSetting.BasicPanelAS = false; Console.WriteLine("Panel Data Autosend Disabled"); break;
                case "sou": Common.AutoSendSetting.BasicPanelAS = false; Console.WriteLine("Sound Data Autosend Disabled"); break;
                case "con": Common.AutoSendSetting.BasicConstAS = false; Console.WriteLine("Const Data Autosend Disabled"); break;
                case "han": Common.AutoSendSetting.BasicHandleAS = false; Console.WriteLine("Handle Data Autosend Disabled"); break;
                default: Console.WriteLine("Option Not Found : {0}", str); break;
              }
            }
          }
          break;

        case "exit":
          Common.Remove();
          IsLooping = false;
          break;
        case "close":
          if (cmd.Length >= 2) Common.Remove(cmd_orig[1]);
          else Common.Remove();
          break;
        case "debug":
          if (cmd.Length >= 2) Common.DebugDo(cmd_orig[1]);
          else Common.DebugDo();
          break;
        case "print":
          if (cmd.Length >= 3)
            for (int i = 2; i < cmd.Length; i++)
              if (Common.Print(cmd_orig[1], cmd_orig[i]) != true) break;
          break;
        case "delay":
          await Task.Delay(int.Parse(cmd[1]));
          break;
        default:
          string modn = FindMod(cmd[0]);
          if (modn != null && modn != string.Empty)
          {
            IBIDSsv ibsv = LoadMod(modn);
            if (ibsv != null)
            {
              try
              {
                if (ibsv.Connect(s)) Common.Add(ref ibsv);
              }
              catch (Exception e)
              {
                Console.WriteLine(e);
                ibsv.Dispose();
                Common.Remove(ibsv);
              }
            }
            else Console.WriteLine("The specified dll file does not implement the IBIDSsv interface.");
          }
          else Console.WriteLine("Command({0}) Not Found", cmd[0]);
          break;
      }
    }

    static string[] LSMod()
    {
      string[] fl = null;
      try
      {
        fl = Directory.GetFiles(@"mods\", "*.dll");
        if (fl.Length > 0)
        {
          for (int i = 0; i < fl.Length; i++) fl[i] = fl[i].Replace(@"mods\", string.Empty);
        }
      }
      catch (DirectoryNotFoundException)
      {
        Directory.CreateDirectory(@"mods");
        Console.WriteLine("Created \"mods\" folder.");
      }
      
      return fl;
    }

    static string FindMod(string keyword)
    {
      string[] ml = LSMod();
      if (ml?.Length > 0)
      {
        for (int i = 0; i < ml.Length; i++)
        {
          string[] sa = ml[i].Split('.');
          if (sa.Length >= 2 && sa[sa.Length - 2] == keyword) return ml[i];
        }
      }
      return string.Empty;
    }

    //https://qiita.com/rita0222/items/609583c31cb7f0132086
    static IBIDSsv LoadMod(string fname)
    {
      IBIDSsv ibs = null;
      Assembly a = null;
      try
      {
        a = Assembly.LoadFrom(@"mods\" + (fname ?? string.Empty));
      }
      catch(FileNotFoundException) { return null; }
      catch(Exception) { throw; }
      try
      {
        foreach (var t in a?.GetTypes())
        {
          if (t.IsInterface) continue;
          ibs = Activator.CreateInstance(t) as IBIDSsv;
          if (ibs != null) return ibs;
        }
      }
      catch (ReflectionTypeLoadException e) { foreach (var ex in e.LoaderExceptions) Console.WriteLine(ex); }
      catch (Exception e) { Console.WriteLine(e); }
      return ibs;
    }
  }
}