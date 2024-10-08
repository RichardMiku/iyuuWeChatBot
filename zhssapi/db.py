import json
import os
import random
import re
from abc import ABC
from typing import Any
import datetime

import pymysqlpool

# MySQL连接池
MySQL_POOL = pymysqlpool.ConnectionPool(
    host=os.environ.get('MYSQL_HOST'),
    port=int(os.environ.get('MYSQL_PORT')),
    database=os.environ.get('MYSQL_DATABASE'),
    user=os.environ.get('MYSQL_USERNAME'),
    password=os.environ.get('MYSQL_PASSWORD'),
    autocommit=True
)

"""
字段类型
"""
TINYTEXT = 'tinytext'
TEXT = 'text'
MEDIUMTEXT = 'mediumtext'
LONGTEXT = 'longtext'
INT = 'int'
FLOAT = 'float'
BOOL = 'bool'
TIMESTAMP = 'timestamp'

"""
防止重复初始化
"""
__INITIALIZED__ = {}


class DB(ABC):
    """
    数据库基类
    """

    def __init__(self: Any):
        self.pool = MySQL_POOL
        self.keys = self.__keys()
        self.length = len(self.keys)
        self.__db_keys: list = __INITIALIZED__.get(self.__table_name(), [])
        self.id = None

    def __call__(self):
        return self

    def __keys(self):
        """
        子类中的所有键
        :return: 所有键
        """
        return tuple(self.__annotations__.keys())

    def __keys_type(self):
        """
        子类中的所有键的类型
        :return: 数据类型
        """
        return self.__annotations__.values()

    @property
    def values(self):
        """
        子类中的所有值
        :return: 所有值
        """
        dic = self.to_dict()
        return tuple((v.strftime("%Y-%m-%d %H:%M:%S") if isinstance(v := dic[key], datetime.datetime) else v) for key in
                     self.keys)

    @staticmethod
    def Initialize(cls: Any):
        """
        初始化类装饰器
        :param cls: 需要初始化的类
        :return: cls
        """
        cls().initialize()
        return cls

    def initialize(self):
        """
        初始化函数
        """
        # 检查是否已经初始化
        if self.__class__.__name__ in __INITIALIZED__:
            return
        # 检查表是否存在
        self.__check_table()
        # 检查字段是否完整
        self.__check_field()

    def __check_table(self):
        """
        检查表是否存在
        """
        with self.pool.get_connection() as conn:
            with conn.cursor() as cursor:
                cursor.execute(f'show tables like "{self.__class__.__name__}"')
                if not cursor.fetchone():
                    self.__create_table()

    def __create_table(self):
        """
        创建表
        """
        with self.pool.get_connection() as conn:
            with conn.cursor() as cursor:
                cursor.execute(f'create table {self.__class__.__name__} (id int primary key auto_increment)')
                conn.commit()

    def __check_field(self):
        """
        检查字段是否完整
        """
        with self.pool.get_connection() as conn:
            with conn.cursor() as cursor:
                cursor.execute(f'show columns from {self.__class__.__name__}')
                fields = [field[0] for field in cursor.fetchall()]
                db_keys = fields.copy()
                for key in self.keys:
                    if key not in fields:
                        db_keys.append(key)
                        self.__add_field(key)
                # 记录已经初始化的字段
                __INITIALIZED__[self.__table_name()] = db_keys

    def __add_field(self, key: str):
        """
        添加字段
        :param key: 字段名
        """
        with self.pool.get_connection() as conn:
            with conn.cursor() as cursor:
                if (value := self.__class__.__dict__.get(key)) is not None:
                    cursor.execute(f"""
                    alter table {self.__class__.__name__} 
                    add column {key} {self.__annotations__[key]}
                    default %s
                    """, value)
                else:
                    cursor.execute(f"alter table {self.__class__.__name__} add column {key} {self.__annotations__[key]}")
                conn.commit()

    def __table_name(self):
        """
        表名
        :return: 表名
        """
        return self.__class__.__name__

    @property
    def __next_id(self):
        """
        获取下一个ID
        :return: int
        """
        with self.pool.get_connection() as conn:
            with conn.cursor() as cursor:
                cursor.execute(
                    f'''
                    SHOW CREATE TABLE {self.__table_name()}
                    '''
                )
                return int(re.compile(r'AUTO_INCREMENT=(\d+)').search(cursor.fetchone()[1]).group(1))

    @property
    def __max_id(self):
        """
        获取最大ID
        :return: int
        """
        return self.__next_id - 1

    def new(self, values=None, keys=None, **kwargs):
        """
        新对象
        """
        new = self.__class__()
        if values is not None:
            keys = keys or self.__db_keys
            for key, value in zip(keys, values):
                if key in self.keys:
                    new.__setattr__(key, value)
        for key, value in kwargs.items():
            if key in self.keys:
                new.__setattr__(key, value)
        return new

    def insert(self):
        """
        插入字段
        """
        with self.pool.get_connection() as conn:
            with conn.cursor() as cursor:
                cursor.execute(
                    f"insert into {self.__table_name()} ({', '.join(self.keys)}) "
                    f"values ({', '.join('%s' for _ in range(self.length))})",
                    self.values
                )
                conn.commit()

    def update(self):
        """
        更新字段
        """
        if self.id is None:
            self.insert()
            return
        with self.pool.get_connection() as conn:
            with conn.cursor() as cursor:
                cursor.execute(
                    f'update {self.__table_name()} '
                    f'set {", ".join(f"{x} = %s" for x in self.keys)} '
                    f'where id = {self.id}',
                    self.values
                )
                conn.commit()

    def delete(self):
        """
        删除
        """
        with self.pool.get_connection() as conn:
            with conn.cursor() as cursor:
                cursor.execute(
                    f"delete from {self.__table_name()} "
                    f"where id = {self.id}"
                )
                conn.commit()

    def updates(self, *args, **kwargs):
        """
        更新字段
        :param kwargs: 字段名=字段值
        """
        for dic in args:
            if not isinstance(dic, dict):
                continue
            for key, value in dic.items():
                if key in self.keys:
                    self.__setattr__(key, value)
        for key, value in kwargs.items():
            if key in self.keys:
                self.__setattr__(key, value)
        self.update()

    def find(self, **kwargs):
        """
        查找字段(获取一条)
        :param kwargs: 字段名=字段值
        """
        with self.pool.get_connection() as conn:
            with conn.cursor() as cursor:
                cursor.execute(
                    f'select * from {self.__table_name()} '
                    f'where {" and ".join(f"{k} = %s" for k in kwargs)}',
                    tuple(kwargs.values())
                )
                result = cursor.fetchone()
                if result:
                    for key, value in zip(self.__db_keys, result):
                        self.__setattr__(key, value)
                    return self
                return None

    def find_last(self, **kwargs):
        """
        查找字段(获取最近一条)
        """
        with self.pool.get_connection() as conn:
            with conn.cursor() as cursor:
                if kwargs:
                    cursor.execute(
                        f'select * from {self.__table_name()} '
                        f'where {" and ".join(f"{k} = %s" for k in kwargs)} '
                        f'order by id desc limit 1',
                        tuple(kwargs.values())
                    )
                else:
                    cursor.execute(f'select * from {self.__table_name()} order by id desc limit 1')
                result = cursor.fetchone()
                if result:
                    for key, value in zip(self.__db_keys, result):
                        self.__setattr__(key, value)
                    return self
                return None

    def find_all(self, **kwargs):
        """
        查找字段(获取所有)
        :param kwargs: 字段名=字段值
        """
        with self.pool.get_connection() as conn:
            with conn.cursor() as cursor:
                if kwargs:
                    cursor.execute(
                        f'select * from {self.__table_name()} '
                        f'where {" and ".join(f"{k} = %s" for k in kwargs)}',
                        tuple(kwargs.values())
                    )
                else:
                    cursor.execute(f'select * from {self.__table_name()}')
                result = cursor.fetchall()
                if result:
                    for row in result:
                        data = self.__class__()
                        for key, value in zip(self.__db_keys, row):
                            data.__setattr__(key, value)
                        yield data
                return None

    def random_choice(self, **kwargs):
        """
        随机选择字段
        :param kwargs: 字段名=字段值
        """
        with self.pool.get_connection() as conn:
            with conn.cursor() as cursor:
                if kwargs:
                    # 如果有参数，则采用先查找全部符合要求的ID，再随机选择一个ID
                    cursor.execute(
                        f'''
                        select * from {self.__table_name()}
                        where {" and ".join(f"{k} = %s" for k in kwargs)}
                        order by rand()
                        limit 1
                        ''',
                        tuple(kwargs.values())
                    )
                    return self.new(cursor.fetchone())
                else:
                    # 如果没有参数，则直接找到最大ID，再随机选择一个ID
                    choice_id = random.randint(1, self.__max_id)
                    # 查找ID
                    return self.find(id=choice_id)

    def random_choices(self, num: int = 1, **kwargs):
        """
        随机选择字段
        :param num: 选择数量
        :param kwargs: 字段名=字段值
        """
        with self.pool.get_connection() as conn:
            with conn.cursor() as cursor:
                if kwargs:
                    # 如果有参数，则采用先查找全部符合要求的ID，再随机选择ID
                    cursor.execute(
                        f'''
                        select * from {self.__table_name()}
                        where {" and ".join(f"{k} = %s" for k in kwargs)}
                        order by rand()
                        limit {num}
                        ''',
                        tuple(kwargs.values())
                    )
                    yield from (self.new(info) for info in cursor.fetchall())
                else:
                    # 如果有参数，则采用先查找全部符合要求的ID，再随机选择ID
                    cursor.execute(
                        f'''
                        select * from {self.__table_name()}
                        order by rand()
                        limit {num}
                        '''
                    )
                    yield from (self.new(info) for info in cursor.fetchall())

    def to_dict(self) -> dict:
        """
        将字段转换为字典
        :return: 字段字典
        """
        return {key: self.__dict__.get(key, self.__class__.__dict__.get(key, None)) for key in self.__annotations__}


@DB.Initialize
class ZHSS_LOG(DB):
    """
    智慧山商数据库
    """
    uid: TINYTEXT
    password: TEXT
    name: TINYTEXT
    sex: TINYTEXT
    unit_name: TINYTEXT
    status: INT
    result: TEXT
    timestamp: TIMESTAMP
    ps: TEXT
    api: TEXT
