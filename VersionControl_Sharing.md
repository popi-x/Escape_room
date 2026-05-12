# Unity Version Control 共享规则

这个项目用的是 Unity Version Control / Plastic，不是 Git。  
对应的忽略文件是根目录里的 `ignore.conf`，作用类似 `.gitignore`。

## 需要提交的内容
- `Assets/`
- `Packages/`
- `ProjectSettings/`
- `ignore.conf`

## 不要提交的内容
- `Library/`
- `Temp/`
- `Logs/`
- `UserSettings/`
- `.plastic/`
- `.vs/`
- `.vscode/`
- `*.csproj`
- `*.sln`
- `*.slnx`
- `Build/`
- `Builds/`

## Unity 项目最重要的一条
- `Assets` 里的 `.meta` 文件必须一起提交。
- 新建了 scene、prefab、material、script、folder，都要连对应 `.meta` 一起提交。
- 不要手动删别人资源的 `.meta`。

## 推荐协作方式
- `BasicScene` 这种主场景属于共享区域，改之前先说一声。
- 一个正式房间 scene 同时只让一个人主改，减少冲突。
- 如果只是测试 XR 或做实验，尽量在单独 scene 里做，不要直接改正式房间。

## 提交前快速检查
1. `Pending Changes` 里有没有 `Library`、`Temp`、`Logs`、`UserSettings`
2. 有没有把新资源对应的 `.meta` 一起带上
3. 有没有把生成出来的 `.csproj`、`.sln`、`.slnx` 带进去
4. 有没有误动别人正在做的正式房间 scene

## 一句话版本
只传 `Assets + Packages + ProjectSettings + ignore.conf`，不要传 Unity 自动生成缓存；`meta` 一定要跟资源一起走。
