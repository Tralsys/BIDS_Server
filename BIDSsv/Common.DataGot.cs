using System;
using System.Runtime.CompilerServices;

namespace TR.BIDSsv
{
	public static partial class Common
	{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
    static private void DataGot(in string GotStr)
    {
      if (string.IsNullOrWhiteSpace(GotStr)) return;
      if (!GotStr.StartsWith("TR")) return;
      string[] GSA = GotStr.Split('X');//接尾辞は除かれたものが使用されていることを期待する.
      if (GSA[0].StartsWith("TRI"))
      {
        int seri = 0;
        try
        {
          seri = Convert.ToInt32(GotStr.Substring(4));
        }
        catch (FormatException)
        {
          throw new Exception("TRE6");//要求情報コード 文字混入
        }
        catch (OverflowException)
        {
          throw new Exception("TRE5");//要求情報コード 変換オーバーフロー
        }
        switch (GotStr.Substring(3, 1))
        {
          case "C":
            switch (seri)
            {
              case -1:
                if (GSA.Length > 5)
                {
                  Spec spec;
                  try
                  {
                    spec.B = int.Parse(GSA[1]);
                    spec.P = int.Parse(GSA[2]);
                    spec.A = int.Parse(GSA[3]);
                    spec.J = int.Parse(GSA[4]);
                    spec.C = int.Parse(GSA[5]);
                    BIDSSharedMemoryData bsmd = BSMD;
                    bsmd.SpecData = spec;
                    BSMD = bsmd;
                  }
                  catch (Exception) { throw; }
                }
                break;
              case 0:
                try
                {
                  BIDSSharedMemoryData bsmd = BSMD;
                  bsmd.SpecData.B = int.Parse(GSA[1]);
                  BSMD = bsmd;
                }
                catch (Exception) { throw; }
                break;
              case 1:
                try
                {
                  BIDSSharedMemoryData bsmd = BSMD;
                  bsmd.SpecData.P = int.Parse(GSA[1]);
                  BSMD = bsmd;
                }
                catch (Exception) { throw; }
                break;
              case 2:
                try
                {
                  BIDSSharedMemoryData bsmd = BSMD;
                  bsmd.SpecData.A = int.Parse(GSA[1]);
                  BSMD = bsmd;
                }
                catch (Exception) { throw; }
                break;
              case 3:
                try
                {
                  BIDSSharedMemoryData bsmd = BSMD;
                  bsmd.SpecData.J = int.Parse(GSA[1]);
                  BSMD = bsmd;
                }
                catch (Exception) { throw; }
                break;
              case 4:
                try
                {
                  BIDSSharedMemoryData bsmd = BSMD;
                  bsmd.SpecData.C = int.Parse(GSA[1]);
                  BSMD = bsmd;
                }
                catch (Exception) { throw; }
                break;
              default: throw new Exception("TRE2");
            }
            break;
          case "E":
            switch (seri)
            {
              case -3://Time
                if (GSA.Length > 4)
                {
                  TimeSpan ts3 = new TimeSpan(0, int.Parse(GSA[1]), int.Parse(GSA[2]), int.Parse(GSA[3]), int.Parse(GSA[4]));
                  BIDSSharedMemoryData bsmd = BSMD;
                  bsmd.StateData.T = (int)ts3.TotalMilliseconds;
                  BSMD = bsmd;
                }
                break;
              case -2://Pressure
                if (GSA.Length > 5)
                {
                  BIDSSharedMemoryData bsmd = BSMD;
                  int i = 1;
                  bsmd.StateData.BC = float.Parse(GSA[i++]);
                  bsmd.StateData.MR = float.Parse(GSA[i++]);
                  bsmd.StateData.ER = float.Parse(GSA[i++]);
                  bsmd.StateData.BP = float.Parse(GSA[i++]);
                  bsmd.StateData.SAP = float.Parse(GSA[i++]);
                  BSMD = bsmd;
                }
                break;
              case -1://All
                if (GSA.Length > 10)
                {
                  BIDSSharedMemoryData bsmd = BSMD;
                  State st = bsmd.StateData;
                  int i = 1;
                  st.Z = double.Parse(GSA[i++]);
                  st.V = float.Parse(GSA[i++]);
                  st.T = int.Parse(GSA[i++]);
                  st.BC = float.Parse(GSA[i++]);
                  st.MR = float.Parse(GSA[i++]);
                  st.ER = float.Parse(GSA[i++]);
                  st.BP = float.Parse(GSA[i++]);
                  st.SAP = float.Parse(GSA[i++]);
                  st.I = float.Parse(GSA[i++]);
                }
                break;
              case 0:
                if (GSA.Length >= 2)
                {
                  BIDSSharedMemoryData bsmd = BSMD;
                  bsmd.StateData.Z = double.Parse(GSA[1]);
                  BSMD = bsmd;
                }
                break;
              case 1:
                if (GSA.Length >= 2)
                {
                  BIDSSharedMemoryData bsmd = BSMD;
                  bsmd.StateData.V = float.Parse(GSA[1]);
                  BSMD = bsmd;
                }
                break;
              case 2:
                if (GSA.Length >= 2)
                {
                  BIDSSharedMemoryData bsmd = BSMD;
                  bsmd.StateData.T = int.Parse(GSA[1]);
                  BSMD = bsmd;
                }
                break;
              case 3:
                if (GSA.Length >= 2)
                {
                  BIDSSharedMemoryData bsmd = BSMD;
                  bsmd.StateData.BC = float.Parse(GSA[1]);
                  BSMD = bsmd;
                }
                break;
              case 4:
                if (GSA.Length >= 2)
                {
                  BIDSSharedMemoryData bsmd = BSMD;
                  bsmd.StateData.MR = float.Parse(GSA[1]);
                  BSMD = bsmd;
                }
                break;
              case 5:
                if (GSA.Length >= 2)
                {
                  BIDSSharedMemoryData bsmd = BSMD;
                  bsmd.StateData.ER = float.Parse(GSA[1]);
                  BSMD = bsmd;
                }
                break;
              case 6:
                if (GSA.Length >= 2)
                {
                  BIDSSharedMemoryData bsmd = BSMD;
                  bsmd.StateData.BP = float.Parse(GSA[1]);
                  BSMD = bsmd;
                }
                break;
              case 7:
                if (GSA.Length >= 2)
                {
                  BIDSSharedMemoryData bsmd = BSMD;
                  bsmd.StateData.SAP = float.Parse(GSA[1]);
                  BSMD = bsmd;
                }
                break;
              case 8:
                if (GSA.Length >= 2)
                {
                  BIDSSharedMemoryData bsmd = BSMD;
                  bsmd.StateData.I = float.Parse(GSA[1]);
                  BSMD = bsmd;
                }
                break;
              //case 9: return ReturnString + BSMD.StateData.Volt;//予約 電圧
              case 10://Hour
                break;
              case 11:
                break;
              case 12:
                break;
              case 13:
                break;
              default: throw new Exception("TRE2");
            }
            break;
          case "H":
            switch (seri)
            {
              case -1:
                if (GSA.Length > 4)
                {
                  BIDSSharedMemoryData bsmd = BSMD;
                  bsmd.HandleData.B = int.Parse(GSA[1]);
                  bsmd.HandleData.P = int.Parse(GSA[2]);
                  bsmd.HandleData.R = int.Parse(GSA[3]);
                  bsmd.HandleData.C = int.Parse(GSA[4]);
                  BSMD = bsmd;
                }
                break;
              case 0:
                if (GSA.Length > 1)
                {
                  BIDSSharedMemoryData bsmd = BSMD;
                  bsmd.HandleData.B = int.Parse(GSA[1]);
                  BSMD = bsmd;
                }
                break;
              case 1:
                if (GSA.Length > 1)
                {
                  BIDSSharedMemoryData bsmd = BSMD;
                  bsmd.HandleData.P = int.Parse(GSA[1]);
                  BSMD = bsmd;
                }
                break;
              case 2:
                if (GSA.Length > 1)
                {
                  BIDSSharedMemoryData bsmd = BSMD;
                  bsmd.HandleData.R = int.Parse(GSA[1]);
                  BSMD = bsmd;
                }
                break;
              case 3:
                if (GSA.Length > 1)
                {
                  BIDSSharedMemoryData bsmd = BSMD;
                  bsmd.HandleData.C = int.Parse(GSA[1]);
                  BSMD = bsmd;
                }
                break;
              case 4:
                if (GSA.Length > 1)
                {
                  OpenD opd = OD;
                  opd.SelfBPosition = int.Parse(GSA[1]);
                  OD = opd;
                }
                break;
              default: break;
            }
            break;
          case "P":
            if (GSA.Length > 1 && seri >= 0 && seri < PD.Length)
              PD.Panels[seri] = int.Parse(GSA[1]);
            break;
          case "S":
            if (GSA.Length > 1 && seri >= 0 && seri < SD.Length)
              SD.Sounds[seri] = int.Parse(GSA[1]);
            break;
          case "D":
            switch (seri)
            {
              case 0:
                if (GSA.Length > 1)
                {
                  BIDSSharedMemoryData bsmd = BSMD;
                  bsmd.IsDoorClosed = GSA[1] == "1";
                  BSMD = bsmd;
                }
                break;
              default: break;
            }
            break;
          case "p":
            if (GSA.Length > 32 && seri >= 0)
            {
              int mx = (seri + 1) * 32;
              int[] pda;
              if (PD.Length >= mx)
              {
                pda = new int[mx];
                Buffer.BlockCopy(PD.Panels, 0, pda, 0, PD.Length * sizeof(int));
              }
              else pda = PD.Panels;

              for (int i = seri * 32; i < mx; i++)
                if (i < PD.Length) pda[i] = int.Parse(GSA[(i % 32) + 1]);
              PanelD pd = new PanelD() { Panels = pda };
              PD = pd;
            }
            break;
          case "s":
            if (GSA.Length > 32 && seri >= 0)
            {
              int mx = (seri + 1) * 32;
              int[] sda;
              if (SD.Length >= mx)
              {
                sda = new int[mx];
                Buffer.BlockCopy(SD.Sounds, 0, sda, 0, SD.Length * sizeof(int));
              }
              else sda = SD.Sounds;

              for (int i = seri * 32; i < mx; i++)
                if (i < SD.Length) sda[i] = int.Parse(GSA[(i % 32) + 1]);
              SoundD sd = new SoundD() { Sounds = sda };
              SD = sd;
            }
            break;
          default: throw new Exception("TRE3");//記号部不正
        }
      }
    }
  }
}
