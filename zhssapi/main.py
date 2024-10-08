import datetime
import json

from fastapi import FastAPI
from pydantic import BaseModel

from zhss import ZHSS
from db import ZHSS_LOG

app = FastAPI()


class Item(BaseModel):
    username: str
    password: str


def verify(api, data: Item, cache=True) -> tuple[dict | None, ZHSS | None, ZHSS_LOG | None]:
    """
    验证用户名和密码
    """
    # 检查参数
    if not data.username or not data.password:
        return {
            "code": 400,
            "msg": "参数错误"
        }, None, None
    # 获取最近一次登录记录
    log = ZHSS_LOG().find_last(uid=data.username, password=data.password, api=api)
    # 如果有记录且为今天
    if cache and log and log.status == 1 and log.timestamp.date() == datetime.date.today():
        return None, None, log
    # 创建日志
    log = ZHSS_LOG()
    # 记录时间戳
    log.timestamp = datetime.datetime.now()
    # 记录用户名和密码
    log.uid = data.username
    log.password = data.password
    # 记录API
    log.api = api
    # 创建智慧山商对象
    zhss = ZHSS(data.username, data.password)
    # 登录
    if not zhss.is_login():
        # 记录登录失败
        log.status = 0
        log.insert()
        return {
            "code": 401,
            "msg": "登录失败"
        }, None, None
    # 获取用户信息
    user_info = zhss.get_user_info()
    # 记录用户信息
    log.name = user_info.get("姓名")
    log.sex = user_info.get("性别")
    log.unit_name = user_info.get("学院")
    # 返回结果
    return None, zhss, log


@app.post("/v1/exam_score")
def _(data: Item):
    """
    获取考试分数
    """
    # 验证用户名和密码
    err, zhss, log = verify("/v1/exam_score", data)
    # 返回错误
    if err:
        return err
    # 如果有缓存
    if zhss is None:
        # 返回缓存
        return {
            "code": 200,
            "msg": "获取成功",
            "data": [{
                "课程名称": x.get("COURSENAME"),
                "课程类型": x.get("EXAMPROPERTY"),
                "开课学年": x.get("XN"),
                "课程分数": x.get("SCORE_NUMERIC")
            } for x in json.loads(log.result)]
        }
    # 获取考试分数
    exam_score = zhss.get_exam_score()
    # 记录考试分数
    log.result = json.dumps(exam_score, ensure_ascii=False)
    # 记录成功
    log.status = 1
    log.insert()
    # 返回结果
    return {
        "code": 200,
        "msg": "获取成功",
        "data": [{
            "课程名称": x.get("COURSENAME"),
            "课程类型": x.get("EXAMPROPERTY"),
            "开课学年": x.get("XN"),
            "课程分数": x.get("SCORE_NUMERIC")
        } for x in exam_score]
    }


@app.post("/v1/class_schedule")
async def _(data: Item):
    """
    获取课表
    """
    # 验证用户名和密码
    err, zhss, log = verify("/v1/class_schedule", data)
    # 返回错误
    if err:
        return err
    # 如果有缓存
    if zhss is None:
        # 返回缓存
        return {
            "code": 200,
            "msg": "获取成功",
            "data": [{
                '课程名称': x.get('KCMC'),
                '教师姓名': x.get('JSXM'),
                '起始时间': x.get('SKJC'),
                '结束时间': x.get('SKJC') + x.get('CXJC') - 1,
                '上课地点': x.get('JXDD'),
                '上课星期': x.get('SKXQ')
            } for x in json.loads(log.result)]
        }

    # 获取课表
    class_schedule = zhss.get_class_info()
    # 记录课表
    log.result = json.dumps(class_schedule, ensure_ascii=False)
    # 记录成功
    log.status = 1
    log.insert()
    # 返回结果
    return {
        "code": 200,
        "msg": "获取成功",
        "data": [{
            '课程名称': x.get('KCMC'),
            '教师姓名': x.get('JSXM'),
            '起始时间': x.get('SKJC'),
            '结束时间': x.get('SKJC') + x.get('CXJC') - 1,
            '上课地点': x.get('JXDD'),
            '上课星期': x.get('SKXQ')
        } for x in class_schedule]
    }


@app.get("/v1/user_info")
async def _(uid: str):
    """
    获取用户信息
    """
    # 获取缓存
    log = ZHSS_LOG().find_last(uid=uid, status=1)
    # 如果有缓存
    if log:
        # 返回缓存
        return {
            "code": 200,
            "msg": "获取成功",
            "data": {
                '学号': log.uid,
                '姓名': log.name,
                '性别': log.sex,
                '学院': log.unit_name
            }
        }
    # 返回错误
    return {
        "code": 404,
        "msg": "未找到用户",
        "data": {}
    }


@app.post("/v1/student_reviews")
async def _(data: Item):
    """
    获取学生评教消息
    """
    # 验证用户名和密码
    err, zhss, log = verify("/v1/student_reviews", data, cache=False)
    # 返回错误
    if err:
        return err
    # 获取学生评教信息
    student_reviews = zhss.get_student_reviews_detail()
    # 记录学生评教信息
    log.result = json.dumps(student_reviews, ensure_ascii=False)
    # 记录成功
    log.status = 1
    log.insert()
    # 返回结果
    return student_reviews | {'msg': "获取成功"}


@app.post("/v1/finish_student_reviews")
async def _(data: Item):
    """
    完成学生评教
    """
    # 验证用户名和密码
    err, zhss, log = verify("/v1/finish_student_reviews", data, cache=False)
    # 返回错误
    if err:
        return err
    # 完成学生评教
    result = zhss.finish_student_reviews()
    # 记录结果
    log.result = json.dumps(result, ensure_ascii=False)
    # 记录成功
    log.status = 1
    log.insert()
    # 返回结果
    return result | {'msg': "已完成"}