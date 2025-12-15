#!/usr/bin/env python3
"""
微调背景图片裁剪位置
"""

from PIL import Image

def crop_with_offset(input_path, output_path, target_ratio=21/9, vertical_offset=0):
    """
    裁剪图片，可调整垂直偏移量
    
    Args:
        input_path: 输入图片路径
        output_path: 输出图片路径
        target_ratio: 目标宽高比，默认 21:9
        vertical_offset: 垂直偏移（像素）
                        正数 = 往上偏移（硬盘往下移）
                        负数 = 往下偏移（硬盘往上移）
    """
    img = Image.open(input_path)
    original_width, original_height = img.size
    
    # 计算目标尺寸
    target_height = int(original_width / target_ratio)
    target_width = original_width
    
    # 计算裁剪区域（基于底部）
    base_top = original_height - target_height
    
    # 应用偏移
    top = base_top - vertical_offset
    bottom = top + target_height
    
    # 确保不超出边界
    if top < 0:
        top = 0
        bottom = target_height
    if bottom > original_height:
        bottom = original_height
        top = bottom - target_height
    
    left = 0
    right = target_width
    
    print(f"原始尺寸: {original_width}x{original_height}")
    print(f"目标尺寸: {target_width}x{target_height}")
    print(f"垂直偏移: {vertical_offset}px {'(往上偏移，硬盘下移)' if vertical_offset > 0 else '(往下偏移，硬盘上移)' if vertical_offset < 0 else '(无偏移)'}")
    print(f"裁剪区域: top={top}, bottom={bottom}")
    
    # 裁剪
    cropped_img = img.crop((left, top, right, bottom))
    cropped_img.save(output_path, quality=95, optimize=True)
    
    print(f"✓ 已保存到: {output_path}")
    return cropped_img

def main():
    input_file = "img/about/attic-discovery.png"
    
    print("=" * 60)
    print("微调背景图片 - 21:9 比例")
    print("=" * 60)
    
    # 生成多个偏移版本
    offsets = [
        (0, "原始（硬盘在上沿）"),
        (50, "往下移 50px"),
        (80, "往下移 80px (推荐)"),
        (100, "往下移 100px"),
        (120, "往下移 120px"),
        (150, "往下移 150px"),
    ]
    
    for offset, desc in offsets:
        print(f"\n【{desc}】")
        print("-" * 60)
        output = f"img/about/attic-bg-21x9-offset{offset}.png"
        crop_with_offset(input_file, output, target_ratio=21/9, vertical_offset=offset)
    
    print("\n" + "=" * 60)
    print("✓ 完成！已生成6个版本，硬盘位置逐渐下移:")
    print("")
    for offset, desc in offsets:
        print(f"  - attic-bg-21x9-offset{offset}.png  [{desc}]")
    
    print("\n提示: 预览选择一个合适的版本")
    print("=" * 60)

if __name__ == "__main__":
    main()

