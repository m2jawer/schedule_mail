# 简易计划自动邮件工具
* 提供简单的邮件计划任务部署的工具，配合excel编辑邮件任务和内容，每分钟进行一次邮件任务检测，符合条件的内容执行发送任务.

## 工具使用的基本技术说明
* UI使用ant design pro
* 后端使用的aspnet.core 8.0
* 数据存储使用sqlite
* 数据交互使用ef core

## 配置说明
* UI预设代码使用的是/mail的虚拟路径，如果使用nginx配置则需要注意加入
* tryfiles $url $url/ /mail/index.html

## ui项目打包命令使用
```
npm run deploy 或者 npm run build
```
运行后拷贝dist文件夹下的内容进行部署

API数据部分路径使用的是/api路径，如果在nginx配置则需要注意转义路径如
```
location /mail/api
{
	#proxy_pass	http://api服务器ip:api运行端口/api;
	proxy_pass	http://192.168.0.2:16000/api;
}
```

netcore预设运行的启动并指定端口命令参考.
```
mail.api.exe --urls=http://0.0.0.0:16000
```
