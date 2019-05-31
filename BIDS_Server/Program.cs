using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using TR.BIDScs;

namespace BIDS_Server
{
  class Program
  {
    /*BIDS Server
     * HTML+JavaScriptとの間でWebSocketでの通信。
     */
     
    static bool IsLooping = true;
    const string VerNumStr = "011a2";
    static void Main(string[] args)
    {
      Console.WriteLine("BIDS Server Application");
      Console.WriteLine("ver : " + VerNumStr);
      Console.WriteLine("In this version(" + VerNumStr + "), only Serial Connection is supported.");
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
              case "serial":
                (new Serial()).WriteHelp(in s);
                break;
              case "tcpsv":
                (new TCPIO()).WriteHelp(in s);
                break;
              case "tcpcl":
                (new TCPcl()).WriteHelp(in s);
                break;
              case "exit":
                Console.WriteLine("exit command : Used when user want to close this program.  This command has no arguments.");
                break;
              case "ls":
                Console.WriteLine("connection listing command : Print the list of the Name of alive connection.  This command has no arguments.");
                break;
              case "close":
                Console.WriteLine("close connection command : Used when user want to close the connection.  User must set the Connection Name to be closed.");
                break;
              default:
                Console.WriteLine("Command not found.");
                break;
            }
          }
          else
          {
            Console.WriteLine("BIDS Server Application");
            Console.WriteLine("ver : " + VerNumStr + "\n");
            Console.WriteLine("close: Close the connection.");
            Console.WriteLine("exit : close this program.");
            Console.WriteLine("ls : Print the list of the Name of alive connection.");
            Console.WriteLine("man : Print the mannual of command.");
            Console.WriteLine("serial : Start the serial connection.  If you want to know more info, please do the command \"man serial\".");
            Console.WriteLine("tcpsv : Turn on the TCP Server.  If you want to know more info, please do the command \"man tcpsv\".");
            Console.WriteLine("tcpcl : Connect to TCP Server.  If you want to know more info, please do the command \"man tcpcl\".");
          }
          break;
        case "serial":
          Serial sl = new Serial();
          try
          {
            sl.Connect(s);
            Common.Add(ref sl);
          }
          catch (Exception e)
          {
            Console.WriteLine(e);
            sl.Dispose();
            Common.Remove(sl.Name);
            
          }
          break;
        case "tcpsv":
          TCPIO tcp = new TCPIO();
          try
          {
            tcp.Connect(s);
            Common.Add(ref tcp);
          }
          catch (Exception e)
          {
            Console.WriteLine(e);
            tcp?.Dispose();
            Common.Remove(tcp.Name);

          }
          break;
        case "tcpcl":
          TCPcl tcl = new TCPcl();
          try
          {
            tcl.Connect(s);
            Common.Add(ref tcl);
          }catch(Exception e)
          {
            Console.WriteLine(e);
            tcl?.Dispose();
            Common.Remove(tcl.Name);
          }
          break;
        case "ls": Console.Write(Common.PrintList()); break;
        case "udp":
          Console.WriteLine("UDP has not supported yet.");
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
          Console.WriteLine("Command Not Found");
          break;
      }
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