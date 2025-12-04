// Service Worker placeholder
// 这是一个占位文件，用于消除404错误
// 如果不需要PWA功能，可以保持为空

self.addEventListener('install', function(event) {
  // 安装事件
});

self.addEventListener('fetch', function(event) {
  // 不做任何处理，直接返回网络请求
  return;
});

