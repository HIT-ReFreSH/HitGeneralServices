<div  align=center>
    <img src="images/Full_2048.png" width = 30% height = 30%  />
</div>

# HitGeneralServices-HIT通用服务集

![nuget](https://img.shields.io/nuget/v/HitGeneralServices?style=flat-square)
![Nuget](https://img.shields.io/nuget/dt/HitGeneralServices?style=flat-square)
![GitHub](https://img.shields.io/github/license/HIT-ReFreSH/HitGeneralServices?style=flat-square)
![GitHub last commit](https://img.shields.io/github/last-commit/HIT-ReFreSH/HitGeneralServices?style=flat-square)
![GitHub Workflow Status](https://img.shields.io/github/workflow/status/HIT-ReFreSH/HitGeneralServices/publish_to_nuget?style=flat-square)
![GitHub repo size](https://img.shields.io/github/repo-size/HIT-ReFreSH/HitGeneralServices?style=flat-square)
![GitHub code size](https://img.shields.io/github/languages/code-size/HIT-ReFreSH/HitGeneralServices?style=flat-square)

[View at Nuget.org](https://www.nuget.org/packages/HitGeneralServices/)

HIT通用服务集包含了以下常用的组件：

- HitInfoProviderFactory: 用于获取乐学网认证登录后，用户信息的工厂类
- CasLogin.LoginHttpClient：用于使用统一身份认证登录的HttpClient

## HitInfoProviderFactory 使用方法

将需要使用的服务，加入依赖注入的服务集合即可

## CasLogin.LoginHttpClient 使用方法

既可以使用依赖注入初始化，也可以直接使用`new`来创建LoginHttpClient的实例。LoginHttpClient直接继承自HttpClient，可以像HttpClient一样使用。除此外，还提供了以下成员：

|名称|类型|说明|参数|返回值|异常|
|:---:|:---:|:----|:----|:----|:----|
|LoginAsync|异步方法|进行统一认证登录|`string username`用户名<br> `string password`密码<br> `Func<Stream, Task<string>>? captchaGenerator`**可选。** 验证码填写适配器，用于在必须时填写验证码。|-|`CaptchaRequiredException`: 需要填写二维码，但是未提供对应的适配器<br>`LoginFailedException`: 登陆认证失败，原因见`Message`。|
|TryLoginFor|异步方法|持续尝试进行统一认证登录，直至满足最大尝试次数，或者抛出不能处理的异常|`string username`用户名<br> `string password`密码<br> `uint maxTrail`最大尝试次数<br> `Func<Stream, Task<string>>? captchaGenerator`**可选。** 验证码填写适配器，用于在必须时填写验证码。|-|`CaptchaRequiredException`: 需要填写二维码，但是未提供对应的适配器<br>`LoginFailedException`: 登陆认证失败，原因见`Message`。|
|IsAuthorized|异步方法|检查是否已经完成统一认证登录|-|`bool`，是/否|-|
|Win32CaptchaInput|静态函数(验证码填写适配器)|**适用于控制台程序。** 使用Windows默认图片查看器查看验证码图片|-|-|-|
|CaptchaInputFactory|静态函数(验证码填写适配器工厂)|**适用于控制台程序。** 使用指定查看器生成验证码填写适配器|`string pathToJpegViewer`Jpeg查看器的路径|验证码填写适配器|-|