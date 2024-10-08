# 🏫 [爬虫]智慧山商API
## 项目介绍
> 该项目提取了智慧山商中常用功能的API并进行了封装，方便使用者快速开发

## 部署方式
### 1. 本地部署
#### 配置环境变量
```shell
# MYSQL数据库地址
MYSQL_HOST=''
# MYSQL数据库端口
MYSQL_PORT=''
# MYSQL数据库名称
MYSQL_DATABASE=''
# MYSQL数据库用户名
MYSQL_USERNAME=''
# MYSQL数据库密码
MYSQL_PASSWORD=''
```
#### 安装依赖并运行
```shell
# 使用pip安装依赖
pip install -r requirements.txt -i https://pypi.tuna.tsinghua.edu.cn/simple
# 运行
uvicorn main:app --host 127.0.0.1 --port 8080 --reload --log-level info --workers 5
```
### 2. Docker部署
```Dockfile
FROM nikolaik/python-nodejs:python3.11-nodejs21
LABEL authors="1ZQL1"

COPY . /app
WORKDIR /app

ENV MYSQL_HOST=''
ENV MYSQL_PORT=''
ENV MYSQL_DATABASE=''
ENV MYSQL_USERNAME=''
ENV MYSQL_PASSWORD=''

RUN cp /usr/share/zoneinfo/Asia/Shanghai /etc/localtime &&  \
    echo 'Asia/Shanghai' >/etc/timezone &&  \
    pip install -r requirements.txt -i https://pypi.tuna.tsinghua.edu.cn/simple

ENTRYPOINT ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8080", "--reload", "--log-level", "info", "--workers", "5"]
```

#### 登录实现思路
在[智慧山商登录界面](https://zhss.sdtbu.edu.cn/)中进行登录，同时使用`开发者工具(F12)`观察所有网络请求  
<img alt="登录请求1" src="http://ddns.sdtbu.com.cn:30070/d/%E6%95%B0%E6%8D%AE%E4%B8%AD%E5%BF%83/data/Images/gitness/zhss_api/login_r_1.png" width="800"/>

观察到登录成功后的第一个请求为`https://zhss.sdtbu.edu.cn/tp_up/view?m=up `，之前的请求分别为
```http request
POST https://cas.sdtbu.edu.cn/cas/login?service=https://zhss.sdtbu.edu.cn/tp_up  

GET https://zhss.sdtbu.edu.cn/tp_up/?ticket=ST-542063-ztfn4vc5a0JeEZ0sdcbL-cas  

GET https://zhss.sdtbu.edu.cn/tp_up/ 
```

<img alt="登录请求2" src="http://ddns.sdtbu.com.cn:30070/d/%E6%95%B0%E6%8D%AE%E4%B8%AD%E5%BF%83/data/Images/gitness/zhss_api/login_r_2.png" width="800"/>

> 观察第一个登录请求的`Body`，可以看到有`ras` `ul` `pl` `lt` `execution` `_eventId` 等参数，其中`lt`参数为动态参数，每次登录都会变化，因此需要在登录前先获取`lt`参数，再进行登录  

<img alt="登录请求3" src="http://ddns.sdtbu.com.cn:30070/d/%E6%95%B0%E6%8D%AE%E4%B8%AD%E5%BF%83/data/Images/gitness/zhss_api/login_r_3.png" width="800"/>

> 找到登录的Js代码，轻松发现了获取`lt`参数的方法，在102行中明确说明了`lt`参数在id为`lt`的标签中，因此只需要获取该标签的`value`属性即可

> `ul`与`pl`则分别为用户名与密码的长度，`ras`为一组加密数据，`execution`为`e1s1`，`_eventId`为`submit`

至此，登录的所有参数都已经获取到，只需要将其放入`Body`中，再进行登录即可
