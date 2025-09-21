# Kickstart Template - PowerShell Setup Script
param(
    [Parameter(Mandatory=$false)]
    [string]$PROJECT_NAME
)

if ([string]::IsNullOrWhiteSpace($PROJECT_NAME)) {
    Write-Host "❌ PROJECT_NAME parametresi gerekli!" -ForegroundColor Red
    Write-Host "Kullanım: PowerShell -ExecutionPolicy Bypass -Command `"& { `$PROJECT_NAME='YourProject'; . .\run-setup.ps1 }`"" -ForegroundColor Yellow
    exit 1
}

Write-Host "🚀 Kickstart Template Kurulumu Başlıyor..." -ForegroundColor Green
Write-Host "📝 Proje Adı: $PROJECT_NAME" -ForegroundColor Yellow
Write-Host ""

try {
    # Backend dosya içeriklerini değiştir
    Write-Host "🔧 Backend yapılandırılıyor..." -ForegroundColor Cyan
    Get-ChildItem -Recurse backend -Include *.cs,*.csproj,*.sln,*.json | ForEach-Object { 
        (Get-Content $_.FullName) -replace '\{\{PROJECT_NAME\}\}', $PROJECT_NAME | Set-Content $_.FullName 
    }

    # Frontend dosya içeriklerini değiştir
    Write-Host "🎨 Frontend yapılandırılıyor..." -ForegroundColor Cyan
    if (Test-Path "frontend/package.json") {
        (Get-Content "frontend/package.json") -replace '\{\{PROJECT_NAME\}\}', $PROJECT_NAME | Set-Content "frontend/package.json"
    }

    # Backend klasör adlarını değiştir
    Write-Host "📁 Klasör adları güncelleniyor..." -ForegroundColor Cyan
    Get-ChildItem backend -Directory | Where-Object {$_.Name -like '*PROJECT_NAME*'} | ForEach-Object { 
        $newName = $_.Name -replace '\{\{PROJECT_NAME\}\}', $PROJECT_NAME
        Write-Host "  ✓ $($_.Name) -> $newName" -ForegroundColor Gray
        Rename-Item $_.FullName $newName
    }

    # Backend dosya adlarını değiştir
    Get-ChildItem backend -File | Where-Object {$_.Name -like '*PROJECT_NAME*'} | ForEach-Object { 
        $newName = $_.Name -replace '\{\{PROJECT_NAME\}\}', $PROJECT_NAME
        Write-Host "  ✓ $($_.Name) -> $newName" -ForegroundColor Gray
        Rename-Item $_.FullName $newName
    }

    # Setup dosyalarını temizle
    Write-Host "🧹 Setup dosyaları temizleniyor..." -ForegroundColor Cyan
    Remove-Item "setup.html" -Force -ErrorAction SilentlyContinue
    Remove-Item "run-setup.ps1" -Force -ErrorAction SilentlyContinue

    Write-Host ""
    Write-Host "✅ Kurulum Tamamlandı! 🎉" -ForegroundColor Green
    Write-Host "📁 Proje: $PROJECT_NAME" -ForegroundColor Green
    Write-Host ""
    Write-Host "📋 Sonraki Adımlar:" -ForegroundColor Yellow
    Write-Host "1. Backend: cd backend && dotnet restore && dotnet run" -ForegroundColor White
    Write-Host "2. Frontend: cd frontend && npm install && npm run dev" -ForegroundColor White
    Write-Host ""
    
} catch {
    Write-Host "❌ Hata: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "🚀 Happy Coding!" -ForegroundColor Magenta