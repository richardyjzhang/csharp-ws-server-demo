using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;

namespace csharp_ws_server_demo
{
  class MyServer
  {
    private readonly object listLock = new object();
    private HttpListener listener = null;
    private Thread thread = null;
    private List<WebSocket> wss = null;

    // 构造函数，初始化监听线程
    public MyServer()
    {
      listener = new HttpListener();
      listener.Prefixes.Add("http://127.0.0.1:8080/");
      wss = new List<WebSocket>();

      thread = new Thread(Run);
      thread.Start();
    }

    // 析构函数，停止监听，清空连接列表
    ~MyServer()
    {
      listener.Stop();
      wss.Clear();
    }

    // 向各个WebSocket端广播消息
    public async void BroadcastMessage(string message)
    {
      int cnt = 0;

      foreach (WebSocket ws in wss)
      {
        try
        {
          byte[] buffer = Encoding.UTF8.GetBytes(message);
          await ws.SendAsync(
            new ArraySegment<byte>(buffer),
            WebSocketMessageType.Text, true, CancellationToken.None);
          ++cnt;
        }
        catch (Exception ex)
        {
          Console.Error.Write("Error Sending Message: " + ex.Message);
        }
      }

      if (cnt > 0)
      {
        Console.WriteLine("↗↗ 向客户端发送消息:  " + message);
      }
    }

    // 监听线程，有新连接时启动处理连接线程
    private void Run()
    {
      listener.Start();

      while (listener.IsListening)
      {
        HttpListenerContext context = listener.GetContext();
        Thread thread = new Thread(
          new ParameterizedThreadStart(RunOneConnection));
        thread.Start(context);
      }
    }

    // 连接处理线程，判断是否为WS，并仅处理WS
    private async void RunOneConnection(object _context)
    {
      HttpListenerContext context = _context as HttpListenerContext;
      if (context == null) return;
      if (!context.Request.IsWebSocketRequest) return;

      HttpListenerWebSocketContext wsContext =
        await context.AcceptWebSocketAsync(null);
      WebSocket webSocket = wsContext.WebSocket;

      RunOneWebSocket(webSocket);
    }

    // 不断监听WebSocket消息
    private async void RunOneWebSocket(WebSocket ws)
    {
      // 首先注册ws
      AddOneWebSocket(ws);
      Console.Error.WriteLine("收到客户端连接");

      // 不断接收消息
      while (ws.State == WebSocketState.Open)
      {
        byte[] buffer = new byte[512];
        WebSocketReceiveResult result = await ws.ReceiveAsync(
          new ArraySegment<byte>(buffer), CancellationToken.None);

        // close事件，移除ws
        if (result.CloseStatus.HasValue)
        {
          Console.WriteLine("Closed; Status: " + result.CloseStatus);
          RemoveOneWebSocket(ws);
        }
        else
        {
          Console.WriteLine("↘↘ 从客户端收到消息:  " +
            Encoding.UTF8.GetString(buffer, 0, result.Count));
        }
      }
    }

    // 向ws列表中添加一个连接
    private void AddOneWebSocket(WebSocket ws)
    {
      lock (listLock)
      {
        wss.Add(ws);
      }
    }

    // 从ws列表中移除一个连接
    private void RemoveOneWebSocket(WebSocket ws)
    {
      lock (listLock)
      {
        wss.Remove(ws);
      }
    }


  }
}
