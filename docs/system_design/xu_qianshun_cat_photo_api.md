# 徐千顺负责模块 API 说明

## 模块范围

徐千顺负责猫咪档案、猫咪照片与识别特征记录。本文件记录猫咪照片模块的公开接口契约；照片特征生成、相似度匹配和分页不在本阶段范围内。

猫咪照片基础路径：`/api/cats/{catId}/photos`

| 方法 | 路径 | 用途 |
|---|---|---|
| GET | `/api/cats/{catId}/photos` | 查询指定猫咪的照片列表 |
| GET | `/api/cats/{catId}/photos/{photoId}` | 查询单张照片元数据 |
| POST | `/api/cats/{catId}/photos` | 上传 JPEG 或 PNG 照片 |
| PUT | `/api/cats/{catId}/photos/{photoId}/primary` | 将指定照片设为唯一主图 |
| GET | `/api/cats/{catId}/photos/{photoId}/feature` | 读取照片识别特征 |
| DELETE | `/api/cats/{catId}/photos/{photoId}` | 删除未被匹配记录引用的照片 |

## 数据类型

照片元数据响应字段如下：

| 字段 | 类型 | 可为空 | 说明 |
|---|---|---|---|
| `photoID` | string | 否 | 服务端生成的 36 字符串 UUID |
| `catID` | string | 是 | 关联猫咪 ID；本模块上传的照片必定存在 |
| `photoUrl` | string | 否 | 服务器上的相对静态文件 URL |
| `uploadUserID` | string | 是 | 上传用户 ID；本模块上传时必填 |
| `uploadTime` | string | 是 | ISO 8601 UTC 时间；本模块上传时由服务端生成 |
| `isPrimary` | integer | 否 | `0` 表示普通照片，`1` 表示主图 |

列表响应不包含 `featureVector`。特征接口将 Oracle CLOB 中的 JSON 数值数组解析为 `number[]`；尚未生成特征时返回 `null`。

## 查询照片列表

- Method：`GET`
- URL：`/api/cats/{catId}/photos`
- 路径参数：`catId`，必填，目标猫咪 ID。
- 排序：主图优先，其余按上传时间倒序；时间相同时按 `photoID` 排序。

成功响应：`200 OK`

```json
[
  {
    "photoID": "d37cf42a-2306-4502-97f1-532c52aec408",
    "catID": "demo-cat-campus-001",
    "photoUrl": "/uploads/cats/demo-cat-campus-001/d37cf42a-2306-4502-97f1-532c52aec408.jpg",
    "uploadUserID": "demo-user-zhaoqing",
    "uploadTime": "2026-08-03T02:30:00Z",
    "isPrimary": 1
  }
]
```

猫咪存在但没有照片时返回空数组；猫咪不存在时返回 `404 Not Found`。

## 查询单张照片

- Method：`GET`
- URL：`/api/cats/{catId}/photos/{photoId}`
- 路径参数：`catId` 和 `photoId`，均必填。

成功响应：`200 OK`，响应体为照片元数据。照片不存在或不属于 URL 中的猫咪时返回 `404 Not Found`。

## 上传照片

- Method：`POST`
- URL：`/api/cats/{catId}/photos`
- Content-Type：`multipart/form-data`

| 表单字段 | 类型 | 必填 | 说明 |
|---|---|---|---|
| `file` | file | 是 | JPEG 或 PNG；非空且最大 10 MiB |
| `uploadUserID` | string | 是 | 已存在的上传用户 ID，最多 36 字节 |
| `isPrimary` | integer | 否 | 只能为 `0` 或 `1`，默认 `0` |

后端同时校验扩展名、MIME 类型和文件头，不使用原始文件名作为存储文件名。猫咪的首张照片会自动成为主图；后续上传只有显式提供 `isPrimary=1` 时才切换主图。

成功响应：`201 Created`，`Location` 指向单张照片查询接口，响应体为照片元数据。

以下情况返回 `400 Bad Request`：表单字段缺失或非法、文件为空或超过 10 MiB、图片格式不匹配、上传用户不存在。猫咪不存在返回 `404 Not Found`；猫咪已归档返回 `409 Conflict`。

## 设置唯一主图

- Method：`PUT`
- URL：`/api/cats/{catId}/photos/{photoId}/primary`
- 请求体：无。

成功响应：`204 No Content`。重复设置同一主图仍返回成功。照片不存在或不属于该猫咪时返回 `404 Not Found`；猫咪已归档返回 `409 Conflict`。

取消旧主图和设置新主图在同一 Oracle 事务内完成。数据库同时通过条件唯一索引保证每只猫最多一张主图。

## 读取识别特征

- Method：`GET`
- URL：`/api/cats/{catId}/photos/{photoId}/feature`
- 路径参数：`catId` 和 `photoId`，均必填。

已有特征时返回：`200 OK`

```json
{
  "photoID": "d37cf42a-2306-4502-97f1-532c52aec408",
  "catID": "demo-cat-campus-001",
  "featureVector": [0.12, -0.34, 0.56]
}
```

尚未生成特征时仍返回 `200 OK`，其中 `featureVector` 为 `null`。照片不存在或不属于该猫咪时返回 `404 Not Found`。数据库中的非空特征不是合法 JSON 数值数组时返回 `500 Internal Server Error`。

## 删除照片

- Method：`DELETE`
- URL：`/api/cats/{catId}/photos/{photoId}`
- 请求体：无。

成功响应：`204 No Content`。删除当前主图且仍有其他照片时，上传时间最新的剩余照片自动成为主图；删除数据库行和主图接替在同一事务内完成。

照片不存在或不属于该猫咪时返回 `404 Not Found`。照片被 `CAT_MATCHRECORDS` 引用时返回 `409 Conflict`，不会删除数据库记录或本地文件。归档猫咪仍允许删除照片。

## 通用错误响应

业务错误使用以下 JSON 结构：

```json
{
  "message": "错误原因。"
}
```

路径 ID 为空、超过 36 字节或包含不安全字符时返回 `400 Bad Request`。文件系统或数据库操作失败且无法完成业务动作时返回 `500 Internal Server Error`，后端会执行相应的文件补偿清理。

## 本地联调

1. 配置本地 Oracle 连接字符串并执行 `database/queries/cat_photos_oracle_programming.sql`。
2. 执行演示数据和 `database/queries/cat_photos_acceptance.sql`。
3. 运行 `dotnet run --project src/CampusStrayCatSystem.Core/CampusStrayCatSystem.Core.csproj --launch-profile http`。
4. 打开 `http://localhost:5047/swagger`，或导入 `assets/demo/XuQianshunCatPhotos.postman_collection.json`。
5. 使用仓库根目录的 `logo.png` 作为 JPEG/PNG 上传校验之外的 PNG 联调样例。
