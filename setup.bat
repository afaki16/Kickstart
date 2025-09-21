@echo off
setlocal enabledelayedexpansion
chcp 65001 >nul
echo 🚀 Kickstart Template - Proje Kurulumu
echo =====================================
echo.
echo Yeni proje adını girin (örnek: MyAwesomeProject):
set /p PROJECT_NAME=

if "%PROJECT_NAME%"=="" (
    echo ❌ Proje adı boş olamaz!
    pause
    exit /b 1
)

echo.
echo 📝 Proje adı '%PROJECT_NAME%' olarak belirlendi
echo.

echo 🔧 Backend yapılandırılıyor...
powershell -Command "(Get-ChildItem -Recurse backend -Include *.cs,*.csproj,*.sln,*.json | ForEach-Object { (Get-Content $_.FullName) -replace '\{\{PROJECT_NAME\}\}', '%PROJECT_NAME%' | Set-Content $_.FullName })"

echo 🎨 Frontend yapılandırılıyor...
powershell -Command "(Get-Content frontend/package.json) -replace '\{\{PROJECT_NAME\}\}', '%PROJECT_NAME%' | Set-Content frontend/package.json"

echo 📁 Dosya adları güncelleniyor...
powershell -Command "Get-ChildItem backend -Directory | Where-Object {$_.Name -like '*PROJECT_NAME*'} | ForEach-Object { $newName = $_.Name -replace '\{\{PROJECT_NAME\}\}', '%PROJECT_NAME%'; Rename-Item $_.FullName $newName }"

powershell -Command "Get-ChildItem backend -File | Where-Object {$_.Name -like '*PROJECT_NAME*'} | ForEach-Object { $newName = $_.Name -replace '\{\{PROJECT_NAME\}\}', '%PROJECT_NAME%'; Rename-Item $_.FullName $newName }"

echo 🧹 Kurulum dosyaları temizleniyor...
del /q setup.bat

echo.
echo ✅ Kurulum tamamlandı!
echo 🎉 Proje adı: %PROJECT_NAME%
echo.
echo 📋 Sonraki adımlar:
echo 1. Backend: cd backend ^&^& dotnet restore ^&^& dotnet run
echo 2. Frontend: cd frontend ^&^& npm install ^&^& npm run dev
echo.
pause