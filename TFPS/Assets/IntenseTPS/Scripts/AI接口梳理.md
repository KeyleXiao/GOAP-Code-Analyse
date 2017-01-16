---
title: AI接口梳理
tags: AI
categories: INTENSE项目源码分析
abbrlink: 6aa8a811
date: 2016-12-6 23:02:53
---

> 关于GOAP目标导向型AI以前没有自己写过 所以本篇更新的会比较慢，现在正慢慢理清思路往下写，本篇是个人代码阅读笔记 更新中...
2016.12.6 更新StateDictionary／State 部分分析

<!-- more -->


# 阅读经验 💡 
阅读每个文件夹的时候，问自己几个问题
- 与AIAction的关系 ?
- 与AIGoals的关系 ?
- 与AIMemory的关系 ?
- 与AISensors的关系 ?
- 与AISystem的关系 ?

> 笔记部分

# Goal-Oriented Action Planning (GOAP)
面向目标的行动规划

# AIBrain
AIBrain.cs AI的大脑，大脑会知道当前AI的一切状态;

## AIBrain <- AIStateSystem Interface
当前 AI Agent 所处的状态。动画的开始结束，更新函数被 AIBrain 高层面统一调用。

## AIBrain <- AISensor Interface
约定AI Sensor模版是可以被序列化的支持在Unity3d中使用ScriptableTool操作。为了方便模版的批量定制

# StateDictionary
封装一个 key-value ,里面无意义代码也比较多。根本上就是处理key-value的对比，合并重写，初始化的快捷操作
```
public Dictionary<string, object> conditions;
```

# State
统一调用所有的 AIStateSystem ，先是State基类 作为统一调用的入口而存在，图二通过方法侵入，设置当前状态。还是忍不住截了张图，这设计太经典了。打出一套combo连招～
![图一 State基类](/uploads/QQ20161206-0@2x.jpg)
![图二 分别表示三种基础状态](/uploads/QQ20161206-1@2x.jpg)
忍不住要说说这种设计的好处，隐藏了具体实现细节，而且只需要重写相应的代码即可修改代码执行之前的判断。

# Checkers
优化基础代码
![图一 Checkers类](/uploads/QQ20161206-2@2x.jpg)


# AIAction Interface
1. 具体行为的执行者 
2. 计算当前节点消耗 [CalculateCost]
3. AIAction可以得到一切计算所需要的数据，有最具体的数据处理逻辑
4. preConditions AI能被执行的前提条件，只有这个条件列表都被满足才能执行这个AI行为
5. postEffects 当前行为结束[IsCompleted(AIBrain)]之后 将要抛出这个变更过的条件列表，给AIBrain
6. 提供函数给Planner校验是否当前行为可以被添加到计划中[CanBeAddedToPlan]
7. [JustBeforePlan] 一个钩子函数，如果有什么需要预处理的可以提前处理。

# Planner 
1. 计划所有目标的可行性
2. 给当前脑中的所有goal排序 
3. [GetRelated] 给goal的每条状态找到N条同质化的行为Action (即抛出的信息相同的行为)
4. 


[GOAP参考文档](http://alumni.media.mit.edu/~jorkin/goap.html)
[GOAP Demo](https://gamedevelopment.tutsplus.com/tutorials/goal-oriented-action-planning-for-a-smarter-ai--cms-20793)
