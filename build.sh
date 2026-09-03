#!/usr/bin/env bash
# Vercel build machine không có sẵn .NET SDK (chỉ built-in Node.js), nên
# phải tự tải và cài trước khi build Blazor WASM. Dùng script cài đặt
# chính thức của Microsoft, cài .NET 10 vào thư mục tạm trong quá trình build.
set -e

echo "==> Đang cài .NET SDK..."
curl -sSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 10.0 --install-dir "$HOME/.dotnet"
export PATH="$HOME/.dotnet:$PATH"
export DOTNET_ROOT="$HOME/.dotnet"

echo "==> Cai workload wasm-tools..."
dotnet workload install wasm-tools

echo "==> Publish Blazor Client..."
dotnet publish src/DmsBlazor.Client -c Release -o publish-output

echo "==> Xong."
