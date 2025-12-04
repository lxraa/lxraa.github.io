source "https://rubygems.org"

gem "jekyll", "~> 4.3.0"

# Ruby 3.4+ 需要的依赖
gem "csv"
gem "logger"
gem "base64"
gem "webrick", "~> 1.7"

group :jekyll_plugins do
  gem "jekyll-feed", "~> 0.6"
  gem "jekyll-paginate", "~> 1.1.0"
  gem "jekyll-sitemap"
end

# Windows does not include zoneinfo files, so bundle the tzinfo-data gem
platforms :mingw, :mswin, :x64_mingw, :jruby do
  gem "tzinfo", ">= 1", "< 3"
  gem "tzinfo-data"
end

# wdm 在 Ruby 3.4 上有兼容性问题,暂时移除
# gem "wdm", "~> 0.1.0" if Gem.win_platform?
