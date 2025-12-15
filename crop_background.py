#!/usr/bin/env python3
"""
裁剪背景图片脚本
将 1024x1024 的方图裁剪为适合博客背景的宽屏比例
"""

from PIL import Image
import os

def crop_for_background(input_path, output_path, target_ratio=16/9):
    """
    裁剪图片为指定比例，保留中心区域
    
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
    # 对于方图 (1024x1024)，裁剪为 16:9 意味着保留全宽，裁剪高度
    target_height = int(original_width / target_ratio)
    
    # 如果计算出的高度大于原始高度，则保留全高，裁剪宽度
    if target_height > original_height:
        target_height = original_height
        target_width = int(original_height * target_ratio)
    else:
        target_width = original_width
    
    print(f"目标尺寸: {target_width}x{target_height} (比例 {target_ratio:.2f}:1)")
    
    # 计算裁剪区域（居中裁剪）
    left = (original_width - target_width) // 2
    top = (original_height - target_height) // 2
    right = left + target_width
    bottom = top + target_height
    
    print(f"裁剪区域: left={left}, top={top}, right={right}, bottom={bottom}")
    
    # 裁剪
    cropped_img = img.crop((left, top, right, bottom))
    
    # 保存
    cropped_img.save(output_path, quality=95, optimize=True)
    print(f"✓ 已保存到: {output_path}")
    print(f"✓ 新尺寸: {cropped_img.size[0]}x{cropped_img.size[1]}")
    
    return cropped_img

def main():
    # 输入输出路径
    input_file = "img/about/attic-discovery.png"
    output_file = "img/about/attic-discovery-bg.png"
    
    # 检查输入文件是否存在
    if not os.path.exists(input_file):
        print(f"错误: 找不到文件 {input_file}")
        return
    
    print("=" * 60)
    print("裁剪背景图片")
    print("=" * 60)
    
    # 方案1: 16:9 宽屏比例（推荐）
    print("\n【方案1】16:9 宽屏比例 (1024x576)")
    crop_for_background(
        input_file, 
        output_file.replace('.png', '-16x9.png'),
        target_ratio=16/9
    )
    
    # 方案2: 21:9 超宽比例（更电影感）
    print("\n【方案2】21:9 超宽比例 (1024x439)")
    crop_for_background(
        input_file, 
        output_file.replace('.png', '-21x9.png'),
        target_ratio=21/9
    )
    
    # 方案3: 2:1 极宽比例（类似网站横幅）
    print("\n【方案3】2:1 极宽比例 (1024x512)")
    crop_for_background(
        input_file, 
        output_file.replace('.png', '-2x1.png'),
        target_ratio=2/1
    )
    
    print("\n" + "=" * 60)
    print("✓ 完成！已生成3个版本:")
    print("  - attic-discovery-bg-16x9.png (推荐，常规宽屏)")
    print("  - attic-discovery-bg-21x9.png (更有电影感)")
    print("  - attic-discovery-bg-2x1.png (最宽，类似横幅)")
    print("\n提示: 选择一个喜欢的版本重命名为 attic-discovery-bg.png")
    print("=" * 60)

if __name__ == "__main__":
    main()

