from PIL import Image
import os

# 图片目录
img_dir = r"C:\Users\lilithgames\Desktop\lxraa.github.io\assets\game-reverse\001"

# 最大宽度（根据博客内容区域设定）
MAX_WIDTH = 800

# 需要处理的图片列表
images = ["1.png", "2.png", "3.png", "4.png", "5.png", "6.jpg"]

def resize_image(image_path, max_width=MAX_WIDTH):
    """调整图片尺寸，如果宽度超过最大宽度则按比例缩小"""
    try:
        img = Image.open(image_path)
        width, height = img.size
        
        print(f"处理: {os.path.basename(image_path)}")
        print(f"  原始尺寸: {width}x{height}")
        
        if width > max_width:
            # 计算新的高度，保持宽高比
            ratio = max_width / width
            new_height = int(height * ratio)
            
            # 调整图片尺寸
            resized_img = img.resize((max_width, new_height), Image.LANCZOS)
            
            # 保存图片，覆盖原文件
            resized_img.save(image_path, quality=95, optimize=True)
            
            print(f"  调整后尺寸: {max_width}x{new_height}")
            print(f"  已保存")
        else:
            print(f"  尺寸合适，无需调整")
            
        print()
        
    except Exception as e:
        print(f"处理 {image_path} 时出错: {e}")
        print()

if __name__ == "__main__":
    print("开始处理图片...\n")
    print(f"图片目录: {img_dir}")
    print(f"最大宽度限制: {MAX_WIDTH}px\n")
    print("-" * 50)
    print()
    
    for img_name in images:
        img_path = os.path.join(img_dir, img_name)
        if os.path.exists(img_path):
            resize_image(img_path)
        else:
            print(f"警告: 文件不存在 - {img_path}\n")
    
    print("-" * 50)
    print("处理完成！")

