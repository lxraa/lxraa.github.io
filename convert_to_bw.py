from PIL import Image
import sys
import os

def convert_to_bw(input_path, output_path):
    img = Image.open(input_path)
    bw_img = img.convert('L')
    
    os.makedirs(os.path.dirname(output_path), exist_ok=True)
    bw_img.save(output_path)
    print(f"Converted: {os.path.basename(input_path)} -> {os.path.basename(output_path)}")

if __name__ == "__main__":
    if len(sys.argv) != 3:
        print("Usage: python convert_to_bw.py <input> <output>")
        sys.exit(1)
    
    input_file = sys.argv[1]
    output_file = sys.argv[2]
    
    if not os.path.exists(input_file):
        print(f"Error: Input file not found: {input_file}")
        sys.exit(1)
    
    convert_to_bw(input_file, output_file)

