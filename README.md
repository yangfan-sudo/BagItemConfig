想要实现的配置方式：
物品的基类为BagItemConfigData，包括了物品的ID，描述，类型，资源路径等基本配置，ReverHOItemConfigData，ArmorItemConfigData等物品继承BagItemConfigData。如图1所示


图1
配置方式如下图所示，红框内是物品基类的属性，蓝框是物品类型特有的属性，黄色框是所有物品配置都有的id越界检测，和排序功能按钮。

图2

难点1：prefabPath 想要通过拖拽prefab，来实现记录prefab路径，这样可以避免手动填写路径错误的问题，同时如果prefab路径更换也不需重新修改。

实现方法：
重写PathPair 的inspector表现。
关键字：[CustomPropertyDrawer(typeof(PathPair), true)]
PrefabPathInspector : PropertyDrawer

图3
需要注意的是这里的ObjectField使用的Editor GUI的接口而不是EditorGUILayerout的接口，原因是如果使用EditorGUILayerout，在该类型InspectorGUI外层没有Layerout时会位置不正确，所以需要使用OnGUI的Rect。
难点2：想要使用Asset文件来配置每一个类型的物品列表，需要实现一个继承ScriptableObject的类，并且该类中包含List<BagItemConfigData>，该基类命名为BagItemConfigs。
希望能够重写BagItemConfigs的Editor Inspector来实现图2根据id排序和检查id合法性功能。所有继承自BagItemConfigs的类如RevertHPItemsConfig,能够显示ReverHPItemConfigData 自有属性：RevertHP ，Duration，和id排序按钮，检查id按钮，并且不用再重写Editor Inspector。而如果像图4 一样实现就会导致RevertHPItemsConfig 对象 的list 和BagItemConfigs 对象的list维护的不是同一个list。

图4
最终实现：接口IBagItemListFunc 定义void CheckIdLegal(ref int crossborder, ref int incontinuity);
    void SortList();接口。定义模板类BagItemConfigs<T>:ScriptableObject,IBagItemListFunc where T: BagItemConfigData，继承接口IBagItemListFunc 和ScriptableObject，且模板T需要继承自BagItemConfigData，然后[CustomEditor(typeof(BagItemConfigs<>), true)]
public class BagItemsConfigInspector : Editor  customEditor参数需要添加true参数，表示继承自BagItemConfigs<> 的类都可以使用该重写的BagItemsConfigInspector 的GUI。

