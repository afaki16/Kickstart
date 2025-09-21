@echo off
chcp 65001 >nul
cls
echo.
echo    🚀 Kickstart Template - Otomatik Kurulum
echo    =======================================
echo.
echo    Proje adını girin (örnek: MyAwesomeProject):
set /p PROJECT_NAME=

if "%PROJECT_NAME%"=="" (
    echo    ❌ Proje adı boş olamaz!
    pause
    exit /b 1
)

cls
echo.
echo    🎯 Proje: %PROJECT_NAME%
echo    📦 Kurulum başlıyor...
echo.

echo    📁 Önce dosya/klasör adlarını değiştiriyoruz...
powershell -ExecutionPolicy Bypass -Command "Get-ChildItem backend -Directory | Where-Object {$_.Name -like '*PROJECT_NAME*'} | ForEach-Object { $newName = $_.Name -replace '\{\{PROJECT_NAME\}\}', '%PROJECT_NAME%'; Rename-Item $_.FullName $newName }"

powershell -ExecutionPolicy Bypass -Command "Get-ChildItem backend -File | Where-Object {$_.Name -like '*PROJECT_NAME*'} | ForEach-Object { $newName = $_.Name -replace '\{\{PROJECT_NAME\}\}', '%PROJECT_NAME%'; Rename-Item $_.FullName $newName }"

powershell -ExecutionPolicy Bypass -Command "Get-ChildItem -Recurse backend -Include *.csproj | Where-Object {$_.Name -like '*PROJECT_NAME*'} | ForEach-Object { $newName = $_.Name -replace '\{\{PROJECT_NAME\}\}', '%PROJECT_NAME%'; Rename-Item $_.FullName $newName }"

echo    🔧 Backend içeriklerini yapılandırıyor...
powershell -ExecutionPolicy Bypass -Command "(Get-ChildItem -Recurse backend -Include *.cs,*.csproj,*.sln,*.json | ForEach-Object { (Get-Content $_.FullName) -replace '\{\{PROJECT_NAME\}\}', '%PROJECT_NAME%' | Set-Content $_.FullName })"

echo    🎨 Frontend yapılandırılıyor...
powershell -ExecutionPolicy Bypass -Command "(Get-Content frontend/package.json) -replace '\{\{PROJECT_NAME\}\}', '%PROJECT_NAME%' | Set-Content frontend/package.json"
powershell -ExecutionPolicy Bypass -Command "(Get-Content frontend/nuxt.config.ts) -replace '\{\{PROJECT_NAME\}\}', '%PROJECT_NAME%' | Set-Content frontend/nuxt.config.ts"
powershell -ExecutionPolicy Bypass -Command "(Get-Content frontend/public/data.json) -replace '\{\{PROJECT_NAME\}\}', '%PROJECT_NAME%' | Set-Content frontend/public/data.json"
powershell -ExecutionPolicy Bypass -Command "Get-ChildItem -Recurse frontend -Include *.vue,*.ts,*.js,*.md | ForEach-Object { (Get-Content $_.FullName) -replace '\{\{PROJECT_NAME\}\}', '%PROJECT_NAME%' | Set-Content $_.FullName }"

echo    🧹 Setup dosyası temizleniyor...
del /q auto-setup.bat

cls
echo.
echo    ✅ Kurulum Tamamlandı! 🎉
echo    📁 Proje: %PROJECT_NAME%
echo.
echo    📋 Sonraki Adımlar:
echo    1. Backend: cd backend ^&^& dotnet restore ^&^& dotnet run
echo    2. Frontend: cd frontend ^&^& npm install ^&^& npm run dev
echo.
echo    🚀 Happy Coding!
echo.
pause