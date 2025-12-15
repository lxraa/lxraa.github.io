#!/usr/bin/env python3
"""
裁剪背景图片脚本 - 保留底部（前景硬盘）
"""

from PIL import Image
import os

def crop_for_background_keep_bottom(input_path, output_path, target_ratio=16/9):
    """
    裁剪图片为指定比例，保留底部区域（前景硬盘）
    
    Args:
        input_path: 输入图片路径
        output_path: 输出图片路径
        target_ratio: 目标宽高比，默认 16:9
    """
    # 打开图片
    img = Image.open(input_path)
    original_width, original_height = img.size
    
    print(f"原始尺寸: {original_width}x{original_height}")
    
    # 计算目标尺寸
    target_height = int(original_width / target_ratio)
    target_width = original_width
    
    print(f"目标尺寸: {target_width}x{target_height} (比例 {target_ratio:.2f}:1)")
    
    # 计算裁剪区域（保留底部）
    left = 0
    top = original_height - target_height  # 从底部往上裁
    right = target_width
    bottom = original_height  # 保留到最底部
    
    print(f"裁剪区域: left={left}, top={top}, right={right}, bottom={bottom}")
    print(f"保留区域: 底部 {target_height}px (硬盘所在位置)")
    
    # 裁剪
    cropped_img = img.crop((left, top, right, bottom))
    
    # 保存
    cropped_img.save(output_path, quality=95, optimize=True)
    print(f"✓ 已保存到: {output_path}")
    print(f"✓ 新尺寸: {cropped_img.size[0]}x{cropped_img.size[1]}")
    
    return cropped_img

def crop_for_background_keep_top(input_path, output_path, target_ratio=16/9):
    """
    裁剪图片为指定比例，保留顶部区域（光束）
    """
    img = Image.open(input_path)
    original_width, original_height = img.size
    
    print(f"原始尺寸: {original_width}x{original_height}")
    
    target_height = int(original_width / target_ratio)
    target_width = original_width
    
    print(f"目标尺寸: {target_width}x{target_height} (比例 {target_ratio:.2f}:1)")
    
    # 计算裁剪区域（保留顶部）
    left = 0
    top = 0  # 从顶部开始
    right = target_width
    bottom = target_height
    
    print(f"裁剪区域: left={left}, top={top}, right={right}, bottom={bottom}")
    print(f"保留区域: 顶部 {target_height}px (光束所在位置)")
    
    cropped_img = img.crop((left, top, right, bottom))
    cropped_img.save(output_path, quality=95, optimize=True)
    print(f"✓ 已保存到: {output_path}")
    print(f"✓ 新尺寸: {cropped_img.size[0]}x{cropped_img.size[1]}")
    
    return cropped_img

def main():
    input_file = "img/about/attic-discovery.png"
    
    if not os.path.exists(input_file):
        print(f"错误: 找不到文件 {input_file}")
        return
    
    print("=" * 60)
    print("裁剪背景图片 - 保留关键元素")
    print("=" * 60)
    
    # 方案A: 保留底部（硬盘在前景）- 推荐
    print("\n【方案A】保留底部 - 硬盘在前景 (推荐)")
    print("-" * 60)
    
    print("\n16:9 宽屏 (1024x576)")
    crop_for_background_keep_bottom(
        input_file, 
        "img/about/attic-discovery-bg-bottom-16x9.png",
        target_ratio=16/9
    )
    
    print("\n21:9 超宽 (1024x438)")
    crop_for_background_keep_bottom(
        input_file, 
        "img/about/attic-discovery-bg-bottom-21x9.png",
        target_ratio=21/9
    )
    
    print("\n2:1 极宽 (1024x512)")
    crop_for_background_keep_bottom(
        input_file, 
        "img/about/attic-discovery-bg-bottom-2x1.png",
        target_ratio=2/1
    )
    
    # 方案B: 保留顶部（光束在上方）
    print("\n" + "=" * 60)
    print("\n【方案B】保留顶部 - 光束在上方")
    print("-" * 60)
    
    print("\n16:9 宽屏 (1024x576)")
    crop_for_background_keep_top(
        input_file, 
        "img/about/attic-discovery-bg-top-16x9.png",
        target_ratio=16/9
    )
    
    print("\n21:9 超宽 (1024x438)")
    crop_for_background_keep_top(
        input_file, 
        "img/about/attic-discovery-bg-top-21x9.png",
        target_ratio=21/9
    )
    
    print("\n2:1 极宽 (1024x512)")
    crop_for_background_keep_top(
        input_file, 
        "img/about/attic-discovery-bg-top-2x1.png",
        target_ratio=2/1
    )
    
    print("\n" + "=" * 60)
    print("✓ 完成！已生成6个版本:")
    print("\n方案A（保留底部 - 硬盘）:")
    print("  - attic-discovery-bg-bottom-16x9.png (推荐)")
    print("  - attic-discovery-bg-bottom-21x9.png")
    print("  - attic-discovery-bg-bottom-2x1.png")
    print("\n方案B（保留顶部 - 光束）:")
    print("  - attic-discovery-bg-top-16x9.png")
    print("  - attic-discovery-bg-top-21x9.png")
    print("  - attic-discovery-bg-top-2x1.png")
    print("\n提示: 如果硬盘在前景，选方案A；如果光束更重要，选方案B")
    print("=" * 60)

if __name__ == "__main__":
    main()

