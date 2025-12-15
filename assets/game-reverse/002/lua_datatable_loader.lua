#!/usr/bin/env lua
-- Lua 数据表加载工具
-- 用法: tools\lua.exe lua_datatable_loader.lua [命令] [参数...]

-- 添加当前目录到搜索路径
-- 注意：Lua 的 require 会将模块名中的点号转换为路径分隔符
-- 所以 require("LuaDatatable.reward") 会查找 "LuaDatatable/reward.lua"
package.path = package.path .. ";.\\?.lua;.\\re-code\\lua_datatable\\?.lua"

-- 创建 LuaDatatable 别名，使 require("LuaDatatable.xxx") 能够找到 re-code/lua_datatable/xxx.lua
package.preload["LuaDatatable"] = function()
  return {}
end

-- 设置一个自定义的 loader，将 LuaDatatable.xxx 映射到 re-code/lua_datatable/xxx
local original_require = require
_G.require = function(modname)
  -- 如果模块名以 LuaDatatable. 开头，转换路径
  if type(modname) == "string" and modname:match("^LuaDatatable%.") then
    local tableName = modname:sub(14) -- 移除 "LuaDatatable." 前缀
    -- 尝试从 re-code/lua_datatable 加载
    local path = ".\\re-code\\lua_datatable\\" .. tableName:gsub("%.", "\\") .. ".lua"
    local file = io.open(path, "r")
    if file then
      file:close()
      local chunk, err = loadfile(path)
      if chunk then
        local result = chunk()
        package.loaded[modname] = result
        return result
      else
        error("Failed to load " .. path .. ": " .. tostring(err))
      end
    end
  end
  return original_require(modname)
end

-- 加载 LocalController
local LocalController = require("LocalController")

-- 颜色输出辅助函数（Windows 不支持 ANSI，简化为普通输出）
local function colorPrint(color, text)
  print(text)
end

local function printTitle(text)
  print("\n" .. string.rep("=", 60))
  print(text)
  print(string.rep("=", 60))
end

local function printSection(text)
  print("\n" .. string.rep("-", 60))
  print(text)
  print(string.rep("-", 60))
end

-- 格式化输出值
local function formatValue(value)
  if value == nil then
    return "nil"
  elseif type(value) == "string" then
    return value
  elseif type(value) == "table" then
    return "[Table]"
  else
    return tostring(value)
  end
end

-- 命令：查看表信息
local function cmdTableInfo(tableName)
  printTitle("表信息: " .. tableName)
  
  local lc = LocalController.instance()
  local tbl = lc:getTable(tableName)
  
  if not tbl then
    print("❌ 表不存在或加载失败: " .. tableName)
    return
  end
  
  print("✓ 表加载成功")
  
  -- 统计数据行数
  local count = 0
  if tbl.data then
    for _ in pairs(tbl.data) do
      count = count + 1
    end
  end
  
  print("\n📊 基本信息:")
  print("  - 数据行数: " .. count)
  print("  - 是否有分表: " .. (tbl.link and "是" or "否"))
  print("  - 是否有 vExt: " .. (tbl.vExt and "是 (" .. #tbl.vExt .. " 项)" or "否"))
  
  -- 显示字段信息
  if tbl.index then
    printSection("字段列表")
    local fields = {}
    for name, info in pairs(tbl.index) do
      table.insert(fields, {
        name = name,
        index = info[1],
        type = info[2],
        isLink = info[3] or false
      })
    end
    
    -- 按索引排序
    table.sort(fields, function(a, b) return a.index < b.index end)
    
    print(string.format("%-4s %-30s %-10s %-8s", "序号", "字段名", "类型", "Link"))
    print(string.rep("-", 60))
    for _, field in ipairs(fields) do
      print(string.format("%-4d %-30s %-10s %-8s", 
        field.index, 
        field.name, 
        field.type, 
        field.isLink and "是" or ""))
    end
    print("\n总计: " .. #fields .. " 个字段")
  end
  
  -- 显示分表信息
  if tbl.link then
    printSection("分表范围")
    for k, v in pairs(tbl.link) do
      print(string.format("  分表 %s: ID 范围 [%d - %d]", k, v[1], v[2]))
    end
  end
end

-- 命令：查看单行数据
local function cmdGetRow(tableName, rowId)
  printTitle("查询数据: " .. tableName .. " [ID: " .. rowId .. "]")
  
  local lc = LocalController.instance()
  local line = lc:getLine(tableName, tonumber(rowId) or rowId)
  
  if not line then
    print("❌ 未找到数据: " .. tableName .. " [ID: " .. rowId .. "]")
    return
  end
  
  print("✓ 数据找到")
  
  local tbl = lc:getTable(tableName)
  if not tbl or not tbl.index then
    print("❌ 无法获取表结构")
    return
  end
  
  -- 收集所有字段
  local fields = {}
  for name, info in pairs(tbl.index) do
    table.insert(fields, {
      name = name,
      index = info[1],
      type = info[2],
      isLink = info[3] or false
    })
  end
  
  -- 按索引排序
  table.sort(fields, function(a, b) return a.index < b.index end)
  
  printSection("字段值")
  print(string.format("%-30s %-10s %-8s %s", "字段名", "类型", "Link", "值"))
  print(string.rep("-", 80))
  
  for _, field in ipairs(fields) do
    local value = line:getValue(field.name)
    print(string.format("%-30s %-10s %-8s %s", 
      field.name, 
      field.type,
      field.isLink and "是" or "",
      formatValue(value)))
  end
end

-- 命令：查询字段值
local function cmdGetValue(tableName, rowId, fieldName)
  local lc = LocalController.instance()
  local value = lc:getValue(tableName, tonumber(rowId) or rowId, fieldName)
  
  print(string.format("\n表: %s, ID: %s, 字段: %s", tableName, rowId, fieldName))
  print("值: " .. formatValue(value))
end

-- 命令：列出所有行ID
local function cmdListIds(tableName, limit)
  printTitle("列出 ID: " .. tableName)
  
  local lc = LocalController.instance()
  local tbl = lc:getTable(tableName)
  
  if not tbl then
    print("❌ 表不存在或加载失败: " .. tableName)
    return
  end
  
  local ids = {}
  if tbl.data then
    for id in pairs(tbl.data) do
      table.insert(ids, id)
    end
  end
  
  -- 处理分表
  if tbl.link then
    for i = 1, #tbl.link do
      local splitTableName = tableName .. "_split" .. i
      local subTbl = lc:getTable(splitTableName)
      if subTbl and subTbl.data then
        for id in pairs(subTbl.data) do
          table.insert(ids, id)
        end
      end
    end
  end
  
  -- 排序
  table.sort(ids, function(a, b)
    if type(a) == "number" and type(b) == "number" then
      return a < b
    else
      return tostring(a) < tostring(b)
    end
  end)
  
  print("\n总计: " .. #ids .. " 行数据")
  
  local maxShow = limit or 50
  if #ids > maxShow then
    print("显示前 " .. maxShow .. " 个 ID:")
  end
  
  print("\nID 列表:")
  for i = 1, math.min(#ids, maxShow) do
    if i % 10 == 1 then
      io.write("\n  ")
    end
    io.write(string.format("%-10s", tostring(ids[i])))
  end
  print()
  
  if #ids > maxShow then
    print("\n... 还有 " .. (#ids - maxShow) .. " 个 ID 未显示")
    print("使用 listids " .. tableName .. " [数量] 查看更多")
  end
end

-- 命令：搜索数据
local function cmdSearch(tableName, fieldName, searchValue)
  printTitle("搜索: " .. tableName .. " [" .. fieldName .. " = " .. searchValue .. "]")
  
  local lc = LocalController.instance()
  local results = {}
  
  lc:visitTable(tableName, function(id, line)
    local value = line:getValue(fieldName)
    if value and string.find(tostring(value), searchValue, 1, true) then
      table.insert(results, {id = id, value = value})
    end
  end)
  
  print("\n找到 " .. #results .. " 条匹配结果:")
  
  if #results == 0 then
    print("  (无匹配)")
    return
  end
  
  print(string.format("\n%-15s %s", "ID", "值"))
  print(string.rep("-", 60))
  
  for i, result in ipairs(results) do
    if i <= 20 then
      print(string.format("%-15s %s", tostring(result.id), formatValue(result.value)))
    end
  end
  
  if #results > 20 then
    print("\n... 还有 " .. (#results - 20) .. " 条结果未显示")
  end
end

-- 命令：导出为 JSON
local function cmdExportJson(tableName, outputFile)
  printTitle("导出 JSON: " .. tableName)
  
  local lc = LocalController.instance()
  local tbl = lc:getTable(tableName)
  
  if not tbl then
    print("❌ 表不存在或加载失败: " .. tableName)
    return
  end
  
  -- 简单的 JSON 序列化
  local function toJson(value, indent)
    indent = indent or ""
    if type(value) == "table" then
      local items = {}
      local isArray = true
      local maxIndex = 0
      
      for k in pairs(value) do
        if type(k) ~= "number" then
          isArray = false
          break
        end
        maxIndex = math.max(maxIndex, k)
      end
      
      if isArray then
        for i = 1, maxIndex do
          table.insert(items, toJson(value[i], indent .. "  "))
        end
        return "[\n" .. indent .. "  " .. table.concat(items, ",\n" .. indent .. "  ") .. "\n" .. indent .. "]"
      else
        for k, v in pairs(value) do
          table.insert(items, string.format('"%s": %s', tostring(k), toJson(v, indent .. "  ")))
        end
        return "{\n" .. indent .. "  " .. table.concat(items, ",\n" .. indent .. "  ") .. "\n" .. indent .. "}"
      end
    elseif type(value) == "string" then
      return '"' .. value:gsub('"', '\\"'):gsub("\n", "\\n") .. '"'
    elseif type(value) == "number" or type(value) == "boolean" then
      return tostring(value)
    else
      return "null"
    end
  end
  
  local file = io.open(outputFile or (tableName .. ".json"), "w")
  if not file then
    print("❌ 无法创建文件")
    return
  end
  
  file:write(toJson(tbl))
  file:close()
  
  print("✓ 导出成功: " .. (outputFile or (tableName .. ".json")))
end

-- CSV 转义函数
local function escapeCSV(value)
  if value == nil then
    return ""
  end
  
  local str = tostring(value)
  -- 如果包含逗号、引号或换行符，需要用引号包裹并转义内部引号
  if str:find('[,"\n\r]') then
    str = '"' .. str:gsub('"', '""') .. '"'
  end
  return str
end

-- 将 table 转换为字符串表示
local function tableToString(tbl)
  if type(tbl) ~= "table" then
    return tostring(tbl)
  end
  
  -- 检查是否是连续数组（从1开始的连续整数key）
  local isArray = true
  local maxIndex = 0
  local count = 0
  
  for k, v in pairs(tbl) do
    count = count + 1
    if type(k) == "number" and k > 0 and k == math.floor(k) then
      maxIndex = math.max(maxIndex, k)
    else
      isArray = false
      break
    end
  end
  
  -- 只有当所有key都是从1开始的连续整数时才是数组
  if isArray and maxIndex == count then
    -- 数组类型：转换为 value1|value2|value3 格式
    local parts = {}
    for i = 1, maxIndex do
      local v = tbl[i]
      if type(v) == "table" then
        table.insert(parts, tableToString(v))
      else
        table.insert(parts, tostring(v))
      end
    end
    return table.concat(parts, "|")
  else
    -- 字典类型：转换为 key1;value1|key2;value2 格式
    local parts = {}
    for k, v in pairs(tbl) do
      if type(v) == "table" then
        table.insert(parts, tostring(k) .. ";" .. tableToString(v))
      else
        table.insert(parts, tostring(k) .. ";" .. tostring(v))
      end
    end
    return table.concat(parts, "|")
  end
end

-- 获取单行数据的 CSV 格式
local function getRowCSV(controller, tableName, id, fields)
  local line = controller:getLine(tableName, id)
  
  if not line then
    return nil
  end
  
  -- 获取原始数据和元信息
  local index, lineData, vExt = line:getMetaData()
  
  local values = {}
  for _, fieldName in ipairs(fields) do
    local fieldInfo = index[fieldName]
    local value = nil
    
    if fieldInfo and lineData then
      local key = fieldInfo[1]
      local link = fieldInfo[3]  -- link 标记
      value = lineData[key]
      
      -- 和原始 getValue 逻辑保持一致: 如果有 link 标记,从 vExt 中展开数据
      if link and value and vExt then
        value = vExt[value]
      end
    end
    
    -- 处理 table 类型，转换为字符串表示
    if type(value) == "table" then
      value = tableToString(value)
    end
    table.insert(values, escapeCSV(value))
  end
  
  return table.concat(values, ",")
end

-- 导出整个表为 CSV 文件
local function exportTableToCSV(tableName, outputPath)
  local controller = LocalController.instance()
  
  -- 获取表和字段列表
  local tbl = controller:getTable(tableName)
  if not tbl or not tbl.index then
    print("❌ 表不存在或无索引: " .. tableName)
    return false
  end
  
  -- 从 index 获取字段列表（按索引排序）
  local fields = {}
  for fieldName, fieldInfo in pairs(tbl.index) do
    table.insert(fields, {name = fieldName, index = fieldInfo[1]})
  end
  table.sort(fields, function(a, b) return a.index < b.index end)
  
  local fieldNames = {}
  for _, f in ipairs(fields) do
    table.insert(fieldNames, f.name)
  end
  
  -- 获取所有 ID
  local ids = {}
  controller:visitTable(tableName, function(id, data)
    table.insert(ids, id)
  end)
  table.sort(ids)
  
  -- 打开输出文件
  local file = io.open(outputPath, "w")
  if not file then
    print("❌ 无法创建文件: " .. outputPath)
    return false
  end
  
  -- 写入表头
  file:write(table.concat(fieldNames, ",") .. "\n")
  
  -- 写入数据行
  local count = 0
  for i, id in ipairs(ids) do
    local csvLine = getRowCSV(controller, tableName, id, fieldNames)
    if csvLine then
      file:write(csvLine .. "\n")
      count = count + 1
    else
      -- 数据不存在时写入空行
      local emptyValues = {}
      for j = 1, #fieldNames do
        table.insert(emptyValues, "")
      end
      file:write(table.concat(emptyValues, ",") .. "\n")
    end
    
    -- 显示进度
    if i % 100 == 0 then
      io.write(string.format("\r  进度: %d/%d (%d%%)", i, #ids, math.floor(i * 100 / #ids)))
      io.flush()
    end
  end
  
  file:close()
  print(string.format("\r✓ 完成: %d 行已导出到 %s", count, outputPath))
  return true
end

-- 显示帮助
local function showHelp()
  printTitle("Lua 数据表加载工具 - 使用说明")
  
  print([[
命令列表:

  info <表名>
    显示表的详细信息（字段列表、数据统计等）
    示例: info reward

  get <表名> <ID>
    查询指定 ID 的完整数据
    示例: get reward 100000

  getcsv <表名> <ID>
    以 CSV 格式输出指定 ID 的数据（用于批量导出）
    示例: getcsv reward 100000

  exportcsv <表名> <输出文件>
    导出整个表为 CSV 文件（高性能，一次性完成）
    示例: exportcsv reward re-code/datatable/reward.csv

  value <表名> <ID> <字段名>
    查询指定字段的值
    示例: value reward 100000 item

  listids <表名> [数量]
    列出表中所有 ID（默认显示前 50 个）
    示例: listids reward 100

  search <表名> <字段名> <搜索值>
    搜索包含指定值的数据行
    示例: search reward item 200103

  export <表名> [输出文件]
    导出表为 JSON 格式
    示例: export reward reward_export.json

  help
    显示此帮助信息

使用示例:
  tools\lua.exe lua_datatable_loader.lua info reward
  tools\lua.exe lua_datatable_loader.lua get reward 100000
  tools\lua.exe lua_datatable_loader.lua exportcsv reward output.csv
  tools\lua.exe lua_datatable_loader.lua value reward 100000 item
  tools\lua.exe lua_datatable_loader.lua search lw_hero name "张"
]])
end

-- 主函数
local function main(args)
  if #args == 0 then
    showHelp()
    return
  end
  
  local cmd = args[1]
  
  if cmd == "help" or cmd == "-h" or cmd == "--help" then
    showHelp()
  elseif cmd == "info" then
    if #args < 2 then
      print("❌ 缺少参数: info <表名>")
      return
    end
    cmdTableInfo(args[2])
  elseif cmd == "get" then
    if #args < 3 then
      print("❌ 缺少参数: get <表名> <ID>")
      return
    end
    cmdGetRow(args[2], args[3])
  elseif cmd == "getcsv" then
    if #args < 3 then
      print("❌ 缺少参数: getcsv <表名> <ID>")
      return
    end
    -- 获取表和字段列表
    local controller = LocalController.instance()
    local tbl = controller:getTable(args[2])
    if not tbl or not tbl.index then
      print("❌ 表不存在或无索引: " .. args[2])
      return
    end
    
    -- 从 index 获取字段列表（按索引排序）
    local fields = {}
    for fieldName, fieldInfo in pairs(tbl.index) do
      table.insert(fields, {name = fieldName, index = fieldInfo[1]})
    end
    table.sort(fields, function(a, b) return a.index < b.index end)
    
    local fieldNames = {}
    for _, f in ipairs(fields) do
      table.insert(fieldNames, f.name)
    end
    
    -- 输出 CSV 行
    local csvLine = getRowCSV(controller, args[2], args[3], fieldNames)
    if csvLine then
      print(csvLine)
    else
      -- 数据不存在时输出空行（保持行数一致）
      local emptyValues = {}
      for i = 1, #fieldNames do
        table.insert(emptyValues, "")
      end
      print(table.concat(emptyValues, ","))
    end
  elseif cmd == "exportcsv" then
    if #args < 3 then
      print("❌ 缺少参数: exportcsv <表名> <输出文件>")
      return
    end
    exportTableToCSV(args[2], args[3])
  elseif cmd == "value" then
    if #args < 4 then
      print("❌ 缺少参数: value <表名> <ID> <字段名>")
      return
    end
    cmdGetValue(args[2], args[3], args[4])
  elseif cmd == "listids" then
    if #args < 2 then
      print("❌ 缺少参数: listids <表名> [数量]")
      return
    end
    cmdListIds(args[2], tonumber(args[3]))
  elseif cmd == "search" then
    if #args < 4 then
      print("❌ 缺少参数: search <表名> <字段名> <搜索值>")
      return
    end
    cmdSearch(args[2], args[3], args[4])
  elseif cmd == "export" then
    if #args < 2 then
      print("❌ 缺少参数: export <表名> [输出文件]")
      return
    end
    cmdExportJson(args[2], args[3])
  else
    print("❌ 未知命令: " .. cmd)
    print("使用 'help' 查看帮助信息")
  end
end

-- 运行主函数
local status, err = pcall(main, arg)
if not status then
  print("\n❌ 错误: " .. tostring(err))
  print("\n堆栈跟踪:")
  print(debug.traceback())
end

