using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Reflection;
using TR.BIDScs;
using TR.BIDSsv;
using System.IO;
using TR;

namespace BIDS_Server
{
  class Program
  {
    static bool IsLooping = true;
    const string VerNumStr = "011b";
    static void Main(string[] args)
    {
      Console.WriteLine("BIDS Server Application");
      Console.WriteLine("ver : " + VerNumStr);

      Common.Start(5);
      do ReadLineDO(); while (IsLooping);
      Common.Dispose();

      Console.WriteLine("Please press any key to exit...");
      Console.ReadKey();
    }

    static void ReadLineDO()
    {
      string s = Console.ReadLine();
      string[] cmd = s.ToLower().Split(' ');
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
          if (cmd.Length >= 2) Common.Remove(cmd[1]);
          else Common.Remove();
          Console.WriteLine("Close Complete.");
          break;
        case "debug":
          if (cmd.Length >= 2) Common.DebugDo(cmd[1]);
          else Common.DebugDo();
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
                ibsv.Connect(s);
                Common.Add(ref ibsv);
              }
              catch (Exception e)
              {
                Console.WriteLine(e);
                ibsv.Dispose();
                Common.Remove(ibsv.Name);
              }
            }
            else Console.WriteLine("The specified dll file does not implement the IBIDSsv interface.");
          }
          else Console.WriteLine("Command Not Found");
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
      for (int i = 0; i < ml.Length; i++)
      {
        string[] sa = ml[i].Split('.');
        if (sa.Length >= 2 && sa[sa.Length - 2] == keyword) return ml[i];
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

    static async void SocketDo()
    {
      HttpListener HL = new HttpListener();
      HL.Prefixes.Add("http://localhost:12835/");
      HL.Start();
      while (true)
      {
        HttpListenerContext HLC = await HL.GetContextAsync();
        if (HLC.Request.IsWebSocketRequest)
        {
          Console.WriteLine("WebSocket接続要求を確認しました。");
          WebSocketContext WSC = null;
          try
          {
            WSC = await HLC.AcceptWebSocketAsync(subProtocol: null);
          }catch(Exception e)
          {
            Console.WriteLine("Socket開始エラー : " + e.Message);
            HLC.Response.StatusCode = 500;
            HLC.Response.Close();
            Console.WriteLine("再度接続試行を行います。");
          }
          Console.WriteLine("WebSocket接続を開始します。");
          WebSocket WS = WSC.WebSocket;
          try
          {
            using (Pipe Pp = new Pipe())
            {
              Pp.Open();
              byte[] SendArray = new byte[64];
              Pipe.StateData ST1 = new Pipe.StateData();
              Pipe.State2Data ST2 = new Pipe.State2Data();
              while (WS.State == WebSocketState.Open)
              {
                if (!Equals(ST1, Pp.NowState) || !Equals(ST2, Pp.NowState2))
                {
                  ST1 = Pp.NowState;
                  ST2 = Pp.NowState2;
                  BitConverter.GetBytes(2).CopyTo(SendArray, 0);
                  BitConverter.GetBytes(ST1.Location).CopyTo(SendArray, 4);
                  BitConverter.GetBytes(ST1.Speed).CopyTo(SendArray, 12);
                  BitConverter.GetBytes(ST2.BC).CopyTo(SendArray, 16);
                  BitConverter.GetBytes(ST2.MR).CopyTo(SendArray, 20);
                  BitConverter.GetBytes(ST2.ER).CopyTo(SendArray, 24);
                  BitConverter.GetBytes(ST2.BP).CopyTo(SendArray, 28);
                  BitConverter.GetBytes(ST2.SAP).CopyTo(SendArray, 32);
                  BitConverter.GetBytes(ST1.Current).CopyTo(SendArray, 36);
                  BitConverter.GetBytes(ST1.PowerNotch).CopyTo(SendArray, 40);
                  BitConverter.GetBytes(ST1.BrakeNotch).CopyTo(SendArray, 44);
                  BitConverter.GetBytes((sbyte)ST1.Reverser).CopyTo(SendArray, 48);
                  BitConverter.GetBytes(ST2.IsDoorClosed).CopyTo(SendArray, 49);
                  BitConverter.GetBytes((byte)ST2.Hour).CopyTo(SendArray, 50);
                  BitConverter.GetBytes((byte)ST2.Minute).CopyTo(SendArray, 51);
                  BitConverter.GetBytes((byte)ST2.Second).CopyTo(SendArray, 52);
                  BitConverter.GetBytes((byte)ST2.Millisecond).CopyTo(SendArray, 53);
                  BitConverter.GetBytes(0xFEFEFEFE).CopyTo(SendArray, 60);
                  await WS.SendAsync(new ArraySegment<byte>(SendArray, 0, SendArray.Length), WebSocketMessageType.Binary, true, CancellationToken.None);
                }
                Thread.Sleep(10);
              }
            }
          }catch(Exception e)
          {
            Console.WriteLine("Socket通信エラー : " + e.Message);
            HLC.Response.StatusCode = 500;
            HLC.Response.Close();
          }
          finally { if (WS != null) WS.Dispose(); }
          Console.WriteLine("WebSocket接続が終了しました。再度接続を試行します。");
        }
        else
        {
          Console.WriteLine("WebSocket接続ではありませんでした。");
          HLC.Response.StatusCode = 400;
          HLC.Response.Close();
        }
      }
    }
  }
}