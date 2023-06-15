# CSharp WebSocket 服务端示例

## 项目说明

使用`.NET Framework` 库，无需使用 `IIS`，编译后为 Standalone Console Application

## 核心逻辑

在`MyServer.cs`中，类似于多线程的 TCP 聊天 Demo

- 最外层负责监听端口

- 针对每个请求开启一个线程

- 如果为 WebSocket 连接，则无限循环读取数据

- 维护连接列表，从控制台读取消息，可向客户端广播，实现双工通信
