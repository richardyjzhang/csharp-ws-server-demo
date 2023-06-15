using System;

namespace csharp_ws_server_demo
{
  class Program
  {
    static void Main(string[] args)
    {
      MyServer server = new MyServer();

      while (true)
      {
        string message = Console.ReadLine();
        server.BroadcastMessage(message);
      }
    }
  }
}
