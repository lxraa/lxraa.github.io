#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
数据表批量导出工具 (优化版)
直接使用 Lua 一次性导出,避免重复初始化
"""

import os
import sys
import subprocess
import glob
from typing import List, Tuple

class TableExporter:
    def __init__(self):
        self.lua_exe = "tools\\lua.exe"
        self.lua_script = "lua_datatable_loader.lua"
        self.datatable_dir = "re-code\\lua_datatable"
        self.output_dir = "re-code\\datatable"
        
        # 确保输出目录存在
        os.makedirs(self.output_dir, exist_ok=True)
    
    def run_lua_command(self, *args) -> Tuple[bool, str]:
        """运行 Lua 命令"""
        cmd = [self.lua_exe, self.lua_script] + list(args)
        try:
            result = subprocess.run(
                cmd,
                capture_output=True,
                text=True,
                encoding='utf-8',
                errors='replace'
            )
            return result.returncode == 0, result.stdout
        except Exception as e:
            return False, str(e)
    
    def get_all_table_names(self) -> List[str]:
        """获取所有表名"""
        table_files = glob.glob(os.path.join(self.datatable_dir, "*.lua"))
        table_names = []
        
        for file_path in table_files:
            filename = os.path.basename(file_path)
            # 跳过分表文件（包括数字结尾的分表）
            if '_split' in filename or filename[-5].isdigit():
                continue
            # 移除 .lua 扩展名
            table_name = filename[:-4]
            table_names.append(table_name)
        
        return sorted(table_names)
    
    def export_table(self, table_name: str) -> bool:
        """导出单个表为 CSV（直接使用 Lua 一次性导出）"""
        print(f"\n[*] 正在导出表: {table_name}")
        
        # 准备输出路径
        csv_path = os.path.join(self.output_dir, f"{table_name}.csv")
        
        # 直接调用 Lua 的 exportcsv 命令
        print(f"  [...] 正在导出...")
        success, output = self.run_lua_command("exportcsv", table_name, csv_path)
        
        if success and os.path.exists(csv_path):
            # 从输出中提取行数
            lines = output.strip().split('\n')
            for line in lines:
                if '完成:' in line or '行已导出' in line:
                    # 提取并显示结果
                    result = line.strip()
                    if result.startswith('[LOG]'):
                        continue
                    # 移除特殊字符,避免编码问题
                    result = result.replace('✓', '[OK]')
                    print(f"  [+] {result}")
            return True
        else:
            print(f"  [X] 导出失败")
            if output:
                # 只显示错误信息,跳过 LOG 行
                for line in output.split('\n'):
                    if line.strip() and not line.startswith('[LOG]'):
                        try:
                            print(f"      {line.strip()}")
                        except UnicodeEncodeError:
                            # 忽略编码错误
                            pass
            return False
    
    def export_all_tables(self):
        """导出所有表"""
        print("=" * 60)
        print("数据表批量导出工具 (优化版)")
        print("=" * 60)
        
        # 获取所有表名
        table_names = self.get_all_table_names()
        print(f"\n[*] 找到 {len(table_names)} 个数据表")
        
        # 导出每个表
        success_count = 0
        failed_tables = []
        
        for i, table_name in enumerate(table_names, 1):
            print(f"\n进度: [{i}/{len(table_names)}]")
            if self.export_table(table_name):
                success_count += 1
            else:
                failed_tables.append(table_name)
        
        # 显示总结
        print("\n" + "=" * 60)
        print("导出完成!")
        print("=" * 60)
        print(f"成功: {success_count}/{len(table_names)}")
        
        if failed_tables:
            print(f"失败: {len(failed_tables)}")
            print("失败的表:")
            for table_name in failed_tables:
                print(f"  - {table_name}")

def main():
    if len(sys.argv) < 2:
        print("用法:")
        print("  python export_tables_to_csv.py all           # 导出所有表")
        print("  python export_tables_to_csv.py <表名>        # 导出单个表")
        print("  python export_tables_to_csv.py <表1> <表2>   # 导出多个表")
        sys.exit(1)
    
    exporter = TableExporter()
    
    if sys.argv[1] == "all":
        exporter.export_all_tables()
    else:
        # 导出指定的表
        table_names = sys.argv[1:]
        print(f"\n导出 {len(table_names)} 个表")
        
        success_count = 0
        for table_name in table_names:
            if exporter.export_table(table_name):
                success_count += 1
        
        print(f"\n完成: {success_count}/{len(table_names)} 个表导出成功")

if __name__ == "__main__":
    main()
