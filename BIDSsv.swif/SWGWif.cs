using Newtonsoft.Json;
using RestSharp;
using System;

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace TR.BIDSsv.swif
{
  class SWGWif : IDisposable
  {
    private readonly string SetFName = "swifSetting.json";
    private RestClient rclient;
    
    public string APIKey { get; }
    public string UserID { get; }
    public string PeerID { get; private set; }
    private string Token;
    public SWGWif(string apikey = null, string userID = null, string addr = "localhost")
    {
      bool knrw = string.IsNullOrWhiteSpace(apikey);
      bool unrw = string.IsNullOrWhiteSpace(userID);

      XmlSerializer serializer = new XmlSerializer(typeof(SwifSetting));
      SwifSetting swst = new SwifSetting();

      if (knrw || unrw)
      {
        using (StreamReader sr = new StreamReader(SetFName))
        {
          swst = (SwifSetting)serializer.Deserialize(sr);
        }
      }

      if (knrw) APIKey = swst.APIKey;
      else swst.APIKey = APIKey = apikey;
      if (unrw) UserID = swst.UserID;
      else swst.UserID = UserID = userID;

      if (string.IsNullOrWhiteSpace(swst.APIKey) || string.IsNullOrWhiteSpace(swst.UserID))
        throw new ArgumentException("NULL, Empty or WhiteSpace Only is not allowed in \'apikey\' and \'userID\'");

      using (StreamWriter sw = new StreamWriter(SetFName, false))
      {
        serializer.Serialize(sw, swst);
      }

      rclient = new RestClient(addr);

    }

    public bool Connect()
    {
      var rreq = new RestRequest();
      rreq.Method = Method.POST;

      rreq.AddJsonBody(new
      {
        key = APIKey,
        domain = rclient.BaseUrl.OriginalString,
        turn = true
      });

      var res = rclient.Execute(rreq);
      if ((int)res.StatusCode != 201) return false;

      switch ((int)res.StatusCode)
      {
        case 201://created
          dynamic cobj = JsonConvert.DeserializeObject<dynamic>(res.Content);
          PeerID = cobj.peer_id;
          Token = cobj.token;
          return true;
        case 400://invalid input
          dynamic iiobj = JsonConvert.DeserializeObject<dynamic>(res.Content);

          var iinput = new InvalidInput();
          iinput.command_type = iiobj.command_type;
          iinput.field = iiobj.field;
          iinput.message = iiobj.message;

          throw iinput;
        case 403://forbidden
          throw new Forbidden();
        case 405://method not allowed
          throw new MethodNotAllowed();
        case 406://not acceptable
          throw new NotAcceptable();
        case 408://request timeout
          throw new RequestTimeout();
      }

      return false;
    }

    #region IDisposable Support
    private bool disposedValue = false;

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          // TODO: マネージ状態を破棄します (マネージ オブジェクト)。
          
        }

        // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
        // TODO: 大きなフィールドを null に設定します。

        disposedValue = true;
      }
    }

    // TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
    // ~SWGWif()
    // {
    //   // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
    //   Dispose(false);
    // }

    // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
    void IDisposable.Dispose()
    {
      // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
      Dispose(true);
      // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
      // GC.SuppressFinalize(this);
    }
    #endregion

    public bool PeerDispose()
    {
      throw new NotImplementedException();
    }

    public PeerEventMessage GetEvents()
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>IsDisconnected</returns>
    public bool GetStatus()
    {
      throw new NotImplementedException();
    }

    public DataSockParams DataPortOpen()
    {
      throw new NotImplementedException();
    }

    public bool DataPortClose()
    {
      throw new NotImplementedException();
    }

    public void DataConnectOpen()
    {
      throw new NotImplementedException();
    }
  }

  public class InvalidInput : Exception
  {
    public string command_type;
    public string field;
    public string message;
  }
  public class Forbidden : Exception { }
  public class MethodNotAllowed : Exception { }
  public class NotAcceptable : Exception { }
  public class RequestTimeout : Exception { }
  internal class SwifSetting
  {
    public string APIKey { get; set; }
    public string UserID { get; set; }
  }

  public class PeerEventMessage
  {
    public EventType EvType;

    public string peer_id;
    public string token;

    public string media_connection_id;
    public string data_connection_id;

    public ErrMsg error_message;
  }
  public enum EventType
  {
    OPEN, CONNECTION, CALL, STREAM, CLOSE, ERROR
  }
  public enum ErrMsg
  {
    BROWSER_INCOMPATIBLE, INVALID_ID, INVALID_KEY, UNAVAILABLE_ID,
    SSL_UNAVAILABLE, SERVER_DISCONNECTED, SERVER_ERROR, SOCKET_ERROR, SOCKET_CLOSED
  }

  public class DataSockParams
  {
    public string data_id;
    public ushort port;
    public string ip_v4;
    public string ip_v6;
  }

  public class PeerConnectOpts
  {
    public string peer_id;
    public string token;
    public string metadata;
    public SerializeOpt serialization;
    public class DcInit
    {
      public bool ordered;
      public int maxPacketLifeTine;
      public int maxRetransmits;
      public string protocol;
      public bool negotiated;
      public int id;
      public string priority;
    }


  }

  public enum SerializeOpt
  {
    BINARY, JSON, NONE
  }
}
