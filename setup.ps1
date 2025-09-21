# Kickstart Template Setup Script
Write-Host "🚀 Kickstart Template - Proje Kurulumu" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host ""

$PROJECT_NAME = Read-Host "Yeni proje adını girin (örnek: MyAwesomeProject)"

if ([string]::IsNullOrWhiteSpace($PROJECT_NAME)) {
    Write-Host "❌ Proje adı boş olamaz!" -ForegroundColor Red
    Read-Host "Devam etmek için Enter'a basın"
    exit 1
}

Write-Host ""
Write-Host "📝 Proje adı '$PROJECT_NAME' olarak belirlendi" -ForegroundColor Yellow
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
        Write-Host "  Değiştiriliyor: $($_.Name) -> $newName" -ForegroundColor Gray
        Rename-Item $_.FullName $newName
    }

    # Backend dosya adlarını değiştir
    Get-ChildItem backend -File | Where-Object {$_.Name -like '*PROJECT_NAME*'} | ForEach-Object { 
        $newName = $_.Name -replace '\{\{PROJECT_NAME\}\}', $PROJECT_NAME
        Write-Host "  Değiştiriliyor: $($_.Name) -> $newName" -ForegroundColor Gray
        Rename-Item $_.FullName $newName
    }

    # Setup dosyalarını temizle
    Write-Host "🧹 Kurulum dosyaları temizleniyor..." -ForegroundColor Cyan
    Remove-Item "setup.ps1" -Force

    Write-Host ""
    Write-Host "✅ Kurulum tamamlandı!" -ForegroundColor Green
    Write-Host "🎉 Proje adı: $PROJECT_NAME" -ForegroundColor Green
    Write-Host ""
    Write-Host "📋 Sonraki adımlar:" -ForegroundColor Yellow
    Write-Host "1. Backend: cd backend && dotnet restore && dotnet run" -ForegroundColor White
    Write-Host "2. Frontend: cd frontend && npm install && npm run dev" -ForegroundColor White
    Write-Host ""
    
} catch {
    Write-Host "❌ Hata oluştu: $($_.Exception.Message)" -ForegroundColor Red
}

Read-Host "Devam etmek için Enter'a basın"