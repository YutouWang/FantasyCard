using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Serialization;
using UnityEngine;


/// <summary>
/// PlayerPrefs数据管理类 统一管理数据的存储和读取
/// </summary>
public class PlayerPrefsDataManager
{
    private static PlayerPrefsDataManager instance = new PlayerPrefsDataManager();

    //属性 提供给外部获取实例
    public static PlayerPrefsDataManager Instance
    {
        get
        {
            return instance;
        }
    }

    //无参构造 私有防止其实例化
    private PlayerPrefsDataManager()
    {

    }

    /// <summary>
    /// 存储数据
    /// </summary>
    /// <param name="data">要存储的数据对象</param>
    /// <param name="KeyName">存储对象的唯一标识key</param>
    public void SaveData(object data, string KeyName)
    {
        //通过Type得到传入的实例的类型的所有字段 结合PlayerPrefs和自定义的KeyName将字段的数据存储进来
        //1.通过实例得到 Type 信息(不知道类名 知道实例名 用 GetType)
        Type t = data.GetType();
        //2.通过TYpe信息得到所有字段
        FieldInfo[] infos = t.GetFields();
        //3.确定 KeyName 的拼接规则 遍历字段 拿出来添加存储
        // 键值拼接规则 Player1_PlayerInfo_string_name  （KeyName_数据类型_字段类型_字段名）
        // 这里不用加“i”是因为一个Player实例只用存一组数据进去 一个player只有一个name 不像一种ID的道具可以有很多个用i来区分
        // 这里的每一个i都代表不同的字段

        string KeyValue = "";
        for (int i = 0; i < infos.Length; i++)
        {
            KeyValue = $"{KeyName}_{t.Name}_{infos[i].FieldType.Name}_{infos[i].Name}";

            SaveValue(infos[i].GetValue(data), KeyValue);

        }

        PlayerPrefs.Save();
    }


    //按类型存储每个字段
    private void SaveValue(object value, string KeyValue)
    {
        // 传入的 value = infos[i].GetValue(data) 是本字段类型的 object 实例
        // 通过实例获取Type 用 object.GetType();方法
        Type fieldType = value.GetType();
        if (fieldType == typeof(string))
        {
            //infos[i]是字段（FieldInfo类型）装载当前要存的字段信息
            //GetValue(object) 和 SetValue(object) 是 FieldInfo 的成员方法 传进的参数是这个字段（name）所在的类（PlayerInfo）的实例 (data)
            //可以获取和修改这个字段中的值 并返回一个 object 对象

            PlayerPrefs.SetString(KeyValue, value as string);
            //Debug.Log($"string:{KeyValue}:{PlayerPrefs.GetString(KeyValue)}");
        }
        else if (fieldType == typeof(int))
        {
            //值类型的变量不能用as 转换对象 因为其本身就不是对象 只能用括号强转
            PlayerPrefs.SetInt(KeyValue, (int)value);


            //Debug.Log($"int:{KeyValue}:{PlayerPrefs.GetInt(KeyValue)}");
        }
        else if (fieldType == typeof(float))
        {
            PlayerPrefs.SetFloat(KeyValue, (float)value);
            //Debug.Log($"float:{KeyValue}:{PlayerPrefs.GetFloat(KeyValue)}");
        }
        else if (fieldType == typeof(bool))
        {
            //value = infos[i].GetValue(data) 已经获取了值 直接比较即可 但需要强转匹配一下类型
            //存储成 int 还是 string 的规则 是自定义的
            PlayerPrefs.SetInt(KeyValue, (bool)value == true ? 1 : 0);
            //Debug.Log($"bool:{KeyValue}:{PlayerPrefs.GetInt(KeyValue)}");
        }
        //枚举类型string dictionary list 都继承了IEnumerable 所以不能用这个判断
        //一个类中有一个枚举成员 当下时刻的枚举成员的值只能是枚举值中的一个 所以就相当于一个int值
        //只需要存储这一个int值就可以了 不用像dic List 一样需要存储多份数据
        else if (fieldType.IsEnum)
        {
            PlayerPrefs.SetInt(KeyValue, (int)value);
            //Debug.Log($"Enum:{KeyValue}:{Enum.ToObject(fieldType,PlayerPrefs.GetInt(KeyValue))}");
        }
        //利用父类装载子类判断list类型 泛型中的类型在存储数据的时候再判断
        else if (typeof(IList).IsAssignableFrom(fieldType))
        {
            //如果仅分支  则证明本字段是 IList 的子类 先默认就是List类型
            //现在的目的：是要存储一个List<?> 不知道泛型类型
            //(用Playerprefs 存储一个List是一个小型知识点 和前面存储道具List 排行榜List是一样的 有空总结一下)

            //1.先将object类型的字段转换为IList
            IList list = value as IList;

            //2.存储List的长度 即Count
            PlayerPrefs.SetInt(KeyValue, list.Count);
            //Debug.Log($"listCount:{KeyValue}:{PlayerPrefs.GetInt(KeyValue)}");

            //3.存储List中的每一个元素 因为List中有多个元素 所以需要循环来辅助存储
            // IList 中有索引器 所以可以用for 也可以用foreach
            for (int i = 0; i < list.Count; i++)
            {
                //遍历每一个list中的元素 依次存进本地硬盘
                //但不知道泛型类型 也就等于不知道list中的数据类型 这里只处理基础类型数据 自定义类型后面再处理
                //递归调用本函数 SaveValue() 进去后会自动判断是哪一种基础数据类型 只需要做好键值keyName的区分即可
                //Debug.Log($"键名为{KeyValue + i}的值为{list[i]}");
                SaveValue(list[i], KeyValue + "_" + i);
            }

        }

        //先判断是不是字典类型 进去再处理泛型类型
        else if (typeof(IDictionary).IsAssignableFrom(fieldType))
        {
            //进入if分支 则代表该字段是 IDictionary 的子类 默认则是 Dictionary 类型
            //把未知的类型转化为 Dictionary 实例
            IDictionary dict = value as IDictionary;

            //1.先存储 Dictionary 的 count
            PlayerPrefs.SetInt(KeyValue, dict.Count);
            //2.利用for循环存储数据 但是相比于List Dictionary有一组有两个数据
            //这个传进来的字典不确定键和值的类型 所以不要用for遍历 不能得到key 和 value
            int index = 0;
            foreach (object key in dict.Keys)
            {
                SaveValue(key, $"{KeyValue}_key_{index}");
                SaveValue(dict[key], $"{KeyValue}_value_{index}");
                index++;
            }
        }
        
        //基础类型都不是 那就是自定义类型了
        else
        {
            SaveData(value, KeyValue);
        }
    }

    /// <summary>
    /// 读取数据
    /// </summary>
    /// <param name="type">读取数据的类型</param>
    /// <param name="KeyName">读取数据的唯一标识key</param>
    /// <returns></returns>
    /// 

    //如果要传入一个对象 把读取的结果存进这个对象中，则在调用Load 的时候就要创建一个对象 比较麻烦
    //传入类型的type信息 就可以在内部用 Activator 动态创建一个对象并且返回 操作统一 不用在外部创建对象
    //而SaveData必须得传进去一个对象是因为 存储数据必须是实例会携带数据 仅仅是 type 的话 不携带要存储的数据
    public object LoadData(Type type, string KeyName)
    {
        //创建返回结果的实例（存储容器）
        object data = Activator.CreateInstance(type);

        //得到所有的字段
        FieldInfo[] infos = type.GetFields();

        string keyValue;
        //遍历字段 按字段的类型判断用哪一种 PlayerPrefs 读取
        for (int i = 0; i < infos.Length; i++)
        {
            Type valueType = infos[i].FieldType;
            //键值拼接规则 Player1_PlayerInfo_String_name
            keyValue = $"{KeyName}_{type.Name}_{valueType.Name}_{infos[i].Name}";


            //这取出来不是一个一个的字段吗 怎么能凑成一个PlayerPrefs的实例呢？？？
            //反射中有 GetValue SetValue 两个方法 可以获取和赋值字段中的值
            //可以用LoadValue把字段的值通过 PlayerPrefs 取出来 直接赋值给该字段
            infos[i].SetValue(data, LoadValue(valueType, keyValue));


        }
        return data;
    }

    public object LoadValue(Type valueType, string KeyValue)
    {
        //基础数据类型
        if (valueType == typeof(string))
        {
            return PlayerPrefs.GetString(KeyValue);
        }
        else if (valueType == typeof(int))
        {
            return PlayerPrefs.GetInt(KeyValue);
        }
        else if (valueType == typeof(float))
        {
            return PlayerPrefs.GetFloat(KeyValue);
        }
        //bool类型需要转换一下 存进去的值是bool类型的
        else if (valueType == typeof(bool))
        {
            return PlayerPrefs.GetInt(KeyValue) == 1 ? true : false;
        }
        //枚举类型
        else if (valueType.IsEnum)
        {
            //ToObject(type,value)是Enum提供的静态方法 可以把value还原成type类型
            return Enum.ToObject(valueType, PlayerPrefs.GetInt(KeyValue));
        }
        //List类型
        else if (typeof(IList).IsAssignableFrom(valueType))
        {

            //List的数据类型是统一的 都是<>中的类型 可以先获取泛型类型
            Type[] listGenerTypes = valueType.GetGenericArguments();
            int Count = PlayerPrefs.GetInt(KeyValue);
            IList list = Activator.CreateInstance(valueType) as IList;
            for (int i = 0; i < Count; i++)
            {

                object element = (LoadValue(listGenerTypes[0], KeyValue + "_" + i));

                if (element != null)
                {
                    list.Add(element);
                    //Debug.Log($"{KeyValue+i}: {element}");
                }
                else
                {
                    Debug.LogWarning($"{KeyValue + i}读取失败");
                }
            }
            return list;

        }
        else if (typeof(IDictionary).IsAssignableFrom(valueType))
        {
            IDictionary dic = Activator.CreateInstance(valueType) as IDictionary;
            Type[] dicGenerTypes = valueType.GetGenericArguments();
            //字典的元素 Count 有用吗
            int Count = PlayerPrefs.GetInt(KeyValue);
            for (int i = 0; i < Count; i++)
            {
                object key = LoadValue(dicGenerTypes[0], $"{KeyValue}_key_{i}");
                object value = LoadValue(dicGenerTypes[1], $"{KeyValue}_value_{i}");
                dic.Add(key, value);
            }

            return dic;
        }
        //自定义类型
        else
        {
            //再次调用整个大的读取函数 再次判断大自定义类型中的小自定义类型的各个字段是什么类型
            return LoadData(valueType, KeyValue);
        }
    }
}
