# Alarm v3

第三代闹钟, 尚未完成，但是已经可以使用

## Alarm.Core [1.0.1]

闹钟的核心组件

## Alarm.Loader [1.0.0]

闹钟的 Mod 加载器，用来实现可扩展功能

（还未开工）Mod 开发指南

## Alarm.App [1.1.2]

命令行版本的闹钟应用程序，适合与任务计划程序共同使用

## Alarm.Log [1.0.0]

闹钟的日志功能

## Alarm.StreamController [1.2.0]

闹钟的控制器，可以使用各种输入流控制闹钟

目前支持以下命令：
|命令|功能|
|---|---|
|`exit;`|使闹钟停止|
|`exit later;`|当闹钟播放完当前文件后停止闹钟|
|`next;`|跳过当前音频文件|

## Alarm.Net [1.1.2]

闹钟的网络控制功能，提供 TCP/HTTP/WebSocket 控制服务

推荐使用 curl (HTTP) 或者 浏览器 (WebSocket) 进行控制

[纯网页 WebSocket 客户端](http://pages.hplzh.cn/websocket.html)

## Alarm.Providers [1.0.1]

Provider 是向播放器控制器提供音频文件序列的组件

相关信息见 Mod 开发指南（还未开工）
