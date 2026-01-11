# Sprite 子图导出工具（Sprite Exporter）

一个 Unity Editor 扩展工具，用于从 **已切割的 Sprite Texture（Multiple Sprite）**
中，**可视化选择子图并批量导出为 PNG 文件**。

适合 UI 图集拆分、策划资源导出、美术调试等场景。

---

## ✨ 功能特性

- 支持拖入 **Sprite 类型纹理（Multiple）**
- 自动读取所有子 Sprite
- 网格化预览子图
- 支持名称搜索过滤
- 支持全选 / 全不选
- 只导出勾选的子图
- 自动处理重名文件
- 导出路径默认与原图同目录

---

## 📦 安装方式（推荐）

### Add package from git URL（推荐）

1. 打开 Unity
2. 打开 **Window → Package Manager**
3. 点击左上角 **「+」**
4. 选择 **Add package from git URL**
5. 输入仓库地址: https://github.com/nanfeng979/sprite-exporter.git
6. 点击 Add，等待 Unity 编译完成

---

## 🧭 使用方法

1. 打开工具：
   - 菜单栏选择 **Tools → Sprite 子图导出工具**

2. 在窗口中：
   - 拖入一张 **Texture Type = Sprite**
   - Sprite Mode 必须是 **Multiple**
   - 确保已经切割（Sprite Editor）

3. 工具会自动加载所有子 Sprite

4. 勾选需要导出的子图（支持搜索）

5. 点击 **「导出选中项」**

---

## ⚠ 注意事项

- 若纹理未开启 **Read/Write Enabled**
- 工具会弹窗提示并自动修改
- 若用户拒绝修改 Readable，导出会终止
- 尺寸为 0 的 Sprite 会被自动跳过

---

## 📜 License

MIT License  
你可以自由修改、集成到自己的项目中使用。
