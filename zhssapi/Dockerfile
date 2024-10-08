FROM nikolaik/python-nodejs:python3.11-nodejs21-alpine
LABEL authors="1ZQL1"

COPY . /app
WORKDIR /app
EXPOSE 8090

ENV MYSQL_HOST=''
ENV MYSQL_PORT=''
ENV MYSQL_DATABASE=''
ENV MYSQL_USERNAME=''
ENV MYSQL_PASSWORD=''

RUN cp /usr/share/zoneinfo/Asia/Shanghai /etc/localtime &&  \
    echo 'Asia/Shanghai' >/etc/timezone &&  \
    pip install -r requirements.txt -i https://pypi.tuna.tsinghua.edu.cn/simple

ENTRYPOINT ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8090", "--reload", "--log-level", "info", "--workers", "5"]