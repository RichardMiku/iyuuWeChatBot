import asyncio
import json
import os
import time
import urllib.parse

import bs4
import execjs
import httpx


class ZHSS:

    def __init__(self, username: str, password: str):
        self.client = httpx.Client()
        self.client.headers.update({
            "User-Agent": "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) "
                          "Chrome/120.0.0.0 Safari/537.36 Edg/120.0.0.0"
        })
        self.username = username
        self.password = password
        self.__user_info = None
        with open(os.path.join(os.path.dirname(__file__), 'des.js'), 'r', encoding='utf-8') as f:
            js = f.read()
        self.__ctx = execjs.compile(js)
        self.login_status = self.login()
        self.login_cookie = None

    def __del__(self):
        self.client.close()

    def login(self) -> bool:
        """
        登录智慧山商
        """
        r = self.client.get('https://cas.sdtbu.edu.cn/cas/login', follow_redirects=True)
        if r.url != 'https://zhss.sdtbu.edu.cn/tp_up/view?m=up':
            lt = bs4.BeautifulSoup(r.content, 'lxml').select('#lt')[0]['value']
            self.client.post(f'https://cas.sdtbu.edu.cn/cas/login', params={
                'service': 'https://zhss.sdtbu.edu.cn/tp_up/'
            }, data={
                'rsa': self.__ctx.call('strEnc',
                                       f"{self.username}{self.password}{lt}",
                                       '1',
                                       '2',
                                       '3'
                                       ),
                'ul': len(self.username),
                'pl': len(self.password),
                'lt': lt,
                'execution': 'e1s1',
                '_eventId': 'submit',
            })
            self.client.get('https://zhss.sdtbu.edu.cn/tp_up/', follow_redirects=True)
            if 'CASTGC' not in self.client.cookies:
                return False
            self.login_cookie = self.client.cookies
        self.client.get(f'https://cas.sdtbu.edu.cn/cas/login', params={
            'service': 'http://wmh.sdtbu.edu.cn:7011/tp_wp/wp/score'
        }, follow_redirects=True)
        return True

    def is_login(self):
        """
        返回登录状态
        """
        return self.login_status

    def get_user_info(self):
        """
        获取用户信息
        """
        if self.__user_info:
            return self.__user_info
        result = self.client.post('https://zhss.sdtbu.edu.cn/tp_up/sys/uacm/profile/getUserInfo', json={
            'BE_OPT_ID': self.__ctx.call('strEnc', self.username, 'tp', 'des', 'param')
        }).json()
        self.__user_info = {
            '学号': result['ID_NUMBER'],
            '姓名': result['USER_NAME'],
            '性别': result['USER_SEX'],
            '学院': result['UNIT_NAME']
        }
        return self.__user_info

    def get_card_info(self):
        """
        获取一卡通信息
        """
        result = self.client.post('https://zhss.sdtbu.edu.cn/tp_up/up/subgroup/getOneCardBlance', json={}).json()[0]
        return {
            "上次消费时间": result['TBSJ'],
            "余额": result['YE'],
        }

    def get_exam_info(self):
        """
        获取考试信息
        """
        ...

    def get_exam_score(self):
        """
        获取考试成绩
        """
        score_time = self.client.post('http://wmh.sdtbu.edu.cn:7011/tp_wp/wp/score/getscoretime', json={}).json()
        score = self.client.post('http://wmh.sdtbu.edu.cn:7011/tp_wp/wp/score/getScoreShow', json={
            'nian': score_time['XN'],
            'xueqi': score_time['XQ']
        }).json()
        return score

    def get_spare_classroom(self, _id: int):
        """
        获取空教室
        """
        result = []
        for i in range(1, 12):
            result.extend(self.client.post("http://wmh.sdtbu.edu.cn:7011/tp_wp/wp/kxclassroom/getclassroom", json={
                'build': _id,
                'time': i
            }).json())
        return result

    async def get_spare_classroom_async(self, _id: int):
        """
        获取空教室
        2: 第二教学楼（东校区）
        3: 第三教学楼（东校区）
        4: 第四教学楼（东校区）
        5: 第五教学楼（东校区）
        6: 综合楼（东校区）
        7: 办公楼（东校区）
        8: 第一教学楼（西校区）
        9: 第二教学楼（西校区）
        10: 第三教学楼（西校区）
        11: 商学实验中心（东校区）
        101: 校内基地（东校区）
        103: 校内基地（西校区）
        104: 实验楼（西校区）
        """
        async with httpx.AsyncClient() as client:
            client.headers.update(self.client.headers)
            client.cookies.update(self.client.cookies)
            tasks = []
            for i in range(1, 12):
                tasks.append(client.post("http://wmh.sdtbu.edu.cn:7011/tp_wp/wp/kxclassroom/getclassroom", json={
                    'build': _id,
                    'time': i
                }))
            result = [x for l in await asyncio.gather(*tasks) for x in l.json()]
        return result

    def get_class_info(self):
        """
        获取课程信息
        JSXM: 教师姓名
        JXBMC: 教学班名称
        ZZZ: 未知(应该是总课次)
        XH: 学号
        KCMC: 课程名称
        JXDD: 教学地点
        KKXND: 开课学年度
        JXBH: 教学班号
        KKXQM: 开课学期名
        JSGH: 教师工号
        CXJC: 未知(应该是一节课的时长)
        QSZ: 起始周
        ZCSM: 上课的周次
        SKXQ: 上课星期
        SKJC: 上课节次
        KCH: 课程号
        """
        info = self.client.post('http://wmh.sdtbu.edu.cn:7011/tp_wp/wp/wxH6/wpHome/getLearnweekbyDate', json={}).json()
        return self.client.post('http://wmh.sdtbu.edu.cn:7011/tp_wp/wp/wxH6/wpHome/getWeekClassbyUserId', json={
            'learnWeek': info['learnWeek'],
            'schoolYear': info['schoolYear'],
            'semester': info['semester'],
        }).json()

    def get_student_reviews(self):
        """
        获取学生评教
        """
        self.client.get('http://wfw.sdtbu.edu.cn/sso.jsp')
        self.client.get('https://cas.sdtbu.edu.cn/cas/login', params={
            'service': 'http://wfw.sdtbu.edu.cn/sso.jsp'
        }, cookies=self.login_cookie, follow_redirects=True)
        info = self.client.get('http://wfw.sdtbu.edu.cn/jsxsd/xspj/xspj_find.do').text
        soup = bs4.BeautifulSoup(info, 'lxml')
        result = []
        for x in soup.select('#Form1 tr')[1:]:
            tds = x.select('td')
            result.append({
                '学年学期': tds[1].text,
                '评价分类': tds[2].text,
                '评价批次': tds[3].text,
                '评价课程类别': tds[4].text,
                '开始时间': tds[5].text,
                '结束时间': tds[6].text,
                'url': 'http://wfw.sdtbu.edu.cn' + tds[7].select('a')[0]['href']
            })
        return {
            'code': 200,
            'data': result
        }

    def get_student_reviews_detail(self):
        """
        获取学生评教详情
        """
        data = self.get_student_reviews()
        if not data:
            return {'code': 404}
        result = {'code': 200, 'data': []}
        info = self.client.get(data.get('data')[0].get('url')).text
        soup = bs4.BeautifulSoup(info, 'lxml')
        for x in soup.select('#dataList tr')[1:]:
            tds = x.select('td')
            result['data'].append({
                '教师编号': tds[1].text,
                '教师姓名': tds[2].text,
                '所属院系': tds[3].text,
                '评教类别': tds[4].text,
                '总评分': tds[5].text,
                '已评': tds[6].text.strip(),
                '是否提交': tds[7].text,
                'url': 'http://wfw.sdtbu.edu.cn' + tds[8].select('a')[0]['href']
            })
        return result

    def finish_student_reviews(self):
        """
        完成学生评教
        """
        data = self.get_student_reviews_detail()
        if not data.get('data') and data.get('code') != 200:
            return {
                'code': 404,
                'msg': '没有需要评教的课程'
            }
        l = []
        result = []
        for i in data.get('data'):
            if i.get('是否提交') == '是':
                l.append(i.get('教师编号'))
                continue
            if i.get('教师编号') in l:
                continue
            l.append(i.get('教师编号'))
            result.append(i.get('教师姓名'))
            self.finish_one_student_reviews(i.get('url'))
            time.sleep(0.5)
        return {
            'code': 200,
            'data': result
        }

    def finish_one_student_reviews(self, url):
        data = [('issubmit', '1')]
        soup = bs4.BeautifulSoup(self.client.get(url).text, 'lxml')
        for x in soup.select('#Form1 > input')[1:]:
            data.append((
                x.get('name'),
                x.get('value')
            ))
        sign = False
        for x in soup.select('#Form1 > table > tr')[1:-3]:
            y = x.select('td')
            if not y:
                continue
            j = y[0].select('input')[0]
            k = y[1].find_all('input')
            data.extend((
                (j.get('name'), j.get('value')),
                (k[0].get('name'), k[0].get('value')) if sign else (k[1].get('name'), k[1].get('value')),
                (k[1].get('name'), k[1].get('value')) if sign else (k[2].get('name'), k[2].get('value')),
                (k[3].get('name'), k[3].get('value')),
                (k[5].get('name'), k[5].get('value')),
                (k[7].get('name'), k[7].get('value')),
                (k[9].get('name'), k[9].get('value')),
            ))
            sign = True
        x = soup.select('#Form1 > table > tr')[-3].find('input')
        data.extend((
            (x.get('name'), x.get('value')),
            ('jynr', '老师讲课很好，很认真，很负责，很有耐心，很有爱心，很有责任心，很有教育责任心'),
            ('isxtjg', '1')
        ))
        content = '&'.join([f'{x}={urllib.parse.quote(y)}' for x, y in data])
        self.client.request(
            'POST',
            'http://wfw.sdtbu.edu.cn/jsxsd/xspj/xspj_save.do',
            content=content,
            headers={
                'Content-Type': 'application/x-www-form-urlencoded'
            },
            timeout=15
        )
