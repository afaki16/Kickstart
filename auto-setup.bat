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
echo    📦 Tam otomatik kurulum başlıyor...
echo.

echo    📁 Dosya/klasör adlarını değiştiriyoruz...
powershell -ExecutionPolicy Bypass -Command "Get-ChildItem backend -Directory | Where-Object {$_.Name -like '*PROJECT_NAME*'} | ForEach-Object { $newName = $_.Name -replace '\{\{PROJECT_NAME\}\}', '%PROJECT_NAME%'; Rename-Item $_.FullName $newName }"

powershell -ExecutionPolicy Bypass -Command "Get-ChildItem backend -File | Where-Object {$_.Name -like '*PROJECT_NAME*'} | ForEach-Object { $newName = $_.Name -replace '\{\{PROJECT_NAME\}\}', '%PROJECT_NAME%'; Rename-Item $_.FullName $newName }"


powershell -ExecutionPolicy Bypass -Command "Get-ChildItem backend -Recurse -File | Where-Object {$_.Name -match '\{\{PROJECT_NAME\}\}'} | ForEach-Object { $newName = $_.Name -replace '\{\{PROJECT_NAME\}\}', '%PROJECT_NAME%'; Rename-Item $_.FullName $newName }"
powershell -ExecutionPolicy Bypass -Command "Get-ChildItem backend -Recurse -Directory | Where-Object {$_.Name -match '\{\{PROJECT_NAME\}\}'} | ForEach-Object { $newName = $_.Name -replace '\{\{PROJECT_NAME\}\}', '%PROJECT_NAME%'; Rename-Item $_.FullName $newName }"

echo    🔧 Backend içeriklerini yapılandırıyor...
powershell -ExecutionPolicy Bypass -Command "(Get-ChildItem -Recurse backend -Include *.cs,*.csproj,*.sln,*.json,*.http | ForEach-Object { (Get-Content $_.FullName) -replace '\{\{PROJECT_NAME\}\}', '%PROJECT_NAME%' | Set-Content $_.FullName })"

echo    🎨 Frontend tamamen yapılandırılıyor...
powershell -ExecutionPolicy Bypass -Command "(Get-Content frontend/package.json) -replace '\{\{PROJECT_NAME\}\}', '%PROJECT_NAME%' | Set-Content frontend/package.json"
powershell -ExecutionPolicy Bypass -Command "(Get-Content frontend/nuxt.config.ts) -replace '\{\{PROJECT_NAME\}\}', '%PROJECT_NAME%' | Set-Content frontend/nuxt.config.ts"
powershell -ExecutionPolicy Bypass -Command "if (Test-Path 'frontend/public/data.json') { (Get-Content frontend/public/data.json) -replace '\{\{PROJECT_NAME\}\}', '%PROJECT_NAME%' | Set-Content frontend/public/data.json }"
powershell -ExecutionPolicy Bypass -Command "Get-ChildItem -Recurse frontend -Include *.vue,*.ts,*.js,*.md | ForEach-Object { (Get-Content $_.FullName) -replace '\{\{PROJECT_NAME\}\}', '%PROJECT_NAME%' | Set-Content $_.FullName }"

echo    📝 Root dosyaları güncelleniniyor...
powershell -ExecutionPolicy Bypass -Command "if (Test-Path 'README.md') { (Get-Content README.md) -replace '\{\{PROJECT_NAME\}\}', '%PROJECT_NAME%' | Set-Content README.md }"
powershell -ExecutionPolicy Bypass -Command "if (Test-Path 'DATA_CONFIGURATION.md') { (Get-Content DATA_CONFIGURATION.md) -replace '\{\{PROJECT_NAME\}\}', '%PROJECT_NAME%' | Set-Content DATA_CONFIGURATION.md }"

echo    🧹 Setup dosyası temizleniyor...
del /q auto-setup.bat

cls
echo.
echo    ✅ TAM OTOMATİK KURULUM TAMAMLANDI! 🎉
echo    📁 Proje: %PROJECT_NAME%
echo.
echo    🔍 Değiştirilen yerler:
echo    ✅ Backend namespace'leri ve dosya adları
echo    ✅ Frontend uygulama adları ve başlıkları  
echo    ✅ Package.json ve config dosyaları
echo    ✅ Tüm sayfa başlıkları ve marka adları
echo    ✅ Email adresleri ve dokümantasyon
echo.
echo    📋 Sonraki Adımlar:
echo    1. Backend: cd backend ^&^& dotnet restore ^&^& dotnet run
echo    2. Frontend: cd frontend ^&^& npm install ^&^& npm run dev
echo    3. İlk Migration: cd backend ^&^& dotnet ef migrations add InitialCreate
echo.
echo    🚀 Artık %PROJECT_NAME% projen hazır!
echo.
pause