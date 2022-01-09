<div  align=center>
    <img src="https://github.com/HIT-ReFreSH/HitGeneralServices/raw/master/images/Full_2048.png" width = 30% height = 30%  />
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
- Jwts.JwtsService：用于Jwts的各种服务

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
|GetTwoPhaseLoginInfoAsync|异步方法|获取进行两段CAS登录的登录信息(第一阶段)|`string username`用户名|`Dictionary<string, string?>`用于两段异步登录的登录信息字典;其中键`password`对应值是加密的salt(Aes.Key);键`captchaResponse`如果存在,则对应值是用于获取验证码的url|-|
|ApplyTwoPhaseLoginInfoAsync|异步方法|应用两段CAS登录的登录信息进行登录(第二阶段)|`Dictionary<string, string?> loginInfo`登录信息，原始产生自`GetTwoPhaseLoginInfoAsync(string)`;但是此处的键`password`对应值已经加密(Aes);键`captchaResponse`如果存在,则对应值已经完成人工填写。|-|`LoginFailedException`: 登陆认证失败，原因见`Message`。|

## Jwts.JwtsService 使用方法

既可以使用依赖注入初始化，也可以直接使用`new`来创建JwtsService的实例。JwtsService提供了以下成员：

|名称|类型|说明|参数|返回值|异常|
|:---:|:---:|:----|:----|:----|:----|
|LoginAsync|异步方法|进行统一认证登录|`string username`用户名<br> `string password`密码<br> `Func<Stream, Task<string>>? captchaGenerator`**可选。** 验证码填写适配器，用于在必须时填写验证码。|-|`CaptchaRequiredException`: 需要填写二维码，但是未提供对应的适配器<br>`LoginFailedException`: 登陆认证失败，原因见`Message`。|
|LoginAsync|异步方法|使用已经登录完毕LoginHttpClient登录Jwts|LoginHttpClient，已经完成登录|-|-|
|GetScheduleAsync|异步方法|获取课表信息|`uint year`学年<br> `JwtsSemester semester`学期|大小为[7,6]的字符串列表矩阵，7为周一至周日，6为节次；数组的每个元素是该位置的课程(课程可能会占用1-3行)|-|
