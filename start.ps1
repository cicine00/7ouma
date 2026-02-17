# ============================================================
# 7OUMA - Script de démarrage Windows
# ============================================================
# Usage : .\start.ps1
# ============================================================

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "  ___  _____  _   _ __  __    _    " -ForegroundColor Blue
Write-Host " |__ \|  _  || | | |  \/  |  / \   " -ForegroundColor Blue
Write-Host "    ) | | | || | | | |\/| | / _ \  " -ForegroundColor Blue
Write-Host "   / /| |_| || |_| | |  | |/ ___ \ " -ForegroundColor Blue
Write-Host "  /_/  \___/  \___/|_|  |_/_/   \_\" -ForegroundColor Blue
Write-Host ""
Write-Host "  Plateforme de services de proximite - Maroc" -ForegroundColor Cyan
Write-Host ""

# Vérification des prérequis
function Check-Command($cmd) {
    if (-not (Get-Command $cmd -ErrorAction SilentlyContinue)) {
        Write-Host "❌ $cmd n'est pas installé" -ForegroundColor Red
        exit 1
    }
    Write-Host "✅ $cmd OK" -ForegroundColor Green
}

Write-Host "Vérification des prérequis..." -ForegroundColor Yellow
Check-Command "docker"
Check-Command "docker-compose"
Check-Command "dotnet"
Check-Command "node"
Write-Host ""

# Lancer Docker Compose
Write-Host "Démarrage des services Docker..." -ForegroundColor Yellow
docker-compose up -d --build

Write-Host ""
Write-Host "⏳ Attente démarrage des services (15s)..." -ForegroundColor Yellow
Start-Sleep 15

# Status
Write-Host ""
Write-Host "═══════════════════════════════════════" -ForegroundColor Blue
Write-Host " 7OUMA est prêt ! Accès :" -ForegroundColor Green
Write-Host "═══════════════════════════════════════" -ForegroundColor Blue
Write-Host " Frontend PWA   : http://localhost:3000" -ForegroundColor Cyan
Write-Host " API Gateway    : http://localhost:5000" -ForegroundColor Cyan
Write-Host " Identity API   : http://localhost:5001/swagger" -ForegroundColor Cyan
Write-Host " Catalog API    : http://localhost:5002/swagger" -ForegroundColor Cyan
Write-Host " Booking API    : http://localhost:5003/swagger" -ForegroundColor Cyan
Write-Host " Payment API    : http://localhost:5004/swagger" -ForegroundColor Cyan
Write-Host " RabbitMQ UI    : http://localhost:15672" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════" -ForegroundColor Blue
Write-Host ""

# Ouvrir le navigateur
Start-Process "http://localhost:3000"
